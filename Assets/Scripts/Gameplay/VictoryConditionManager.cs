using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.World;

namespace ElectionEmpire.Gameplay
{
    /// <summary>
    /// Manages victory conditions and tracks progress
    /// </summary>
    public class VictoryConditionManager
    {
        private GameState gameState;
        
        public enum VictoryType
        {
            ReachPresident,           // Classic: Become President
            ApprovalThreshold,        // Maintain 70%+ approval for 30 days
            TotalDomination,          // Control all regions
            ScandalSurvival,          // Survive 10+ major scandals
            DynastyEstablished,       // Groom successor
            TimeLimit,                // Highest office in X days
            Custom                    // Player-defined condition
        }
        
        public VictoryType SelectedCondition;
        public Dictionary<string, float> ConditionProgress;
        
        public VictoryConditionManager(GameState gameState)
        {
            this.gameState = gameState;
            ConditionProgress = new Dictionary<string, float>();
        }
        
        public void Initialize(VictoryType condition)
        {
            SelectedCondition = condition;
            ConditionProgress.Clear();
            
            switch (condition)
            {
                case VictoryType.ApprovalThreshold:
                    ConditionProgress["DaysAbove70"] = 0f;
                    ConditionProgress["Required"] = 30f;
                    break;
                
                case VictoryType.TotalDomination:
                    ConditionProgress["RegionsControlled"] = 0f;
                    ConditionProgress["Required"] = 10f; // All regions
                    break;
                
                case VictoryType.ScandalSurvival:
                    ConditionProgress["ScandalsSurvived"] = 0f;
                    ConditionProgress["Required"] = 10f;
                    break;
                
                case VictoryType.TimeLimit:
                    ConditionProgress["DaysElapsed"] = 0f;
                    ConditionProgress["TimeLimit"] = 365f; // 1 year
                    break;
            }
        }
        
        public void CheckVictoryConditions(PlayerState player, float deltaTime)
        {
            bool victoryAchieved = false;
            
            switch (SelectedCondition)
            {
                case VictoryType.ReachPresident:
                    victoryAchieved = CheckPresidentVictory(player);
                    break;
                
                case VictoryType.ApprovalThreshold:
                    victoryAchieved = CheckApprovalVictory(player, deltaTime);
                    break;
                
                case VictoryType.TotalDomination:
                    victoryAchieved = CheckDominationVictory(player);
                    break;
                
                case VictoryType.ScandalSurvival:
                    victoryAchieved = CheckScandalVictory(player);
                    break;
                
                case VictoryType.TimeLimit:
                    victoryAchieved = CheckTimeLimitVictory(player, deltaTime);
                    break;
            }
            
            if (victoryAchieved)
            {
                TriggerVictory(player);
            }
        }
        
        private bool CheckPresidentVictory(PlayerState player)
        {
            return player.CurrentOffice != null && 
                   player.CurrentOffice.Type == OfficeType.President;
        }
        
        private bool CheckApprovalVictory(PlayerState player, float deltaTime)
        {
            if (player.ApprovalRating >= 70f)
            {
                ConditionProgress["DaysAbove70"] += deltaTime / 86400f;
            }
            else
            {
                ConditionProgress["DaysAbove70"] = 0f; // Reset if drops below
            }
            
            return ConditionProgress["DaysAbove70"] >= ConditionProgress["Required"];
        }
        
        private bool CheckDominationVictory(PlayerState player)
        {
            if (gameState == null || gameState.World == null || gameState.VoterSim == null)
                return false;
            
            // Count regions where player has majority support
            int controlled = 0;
            
            foreach (var region in gameState.World.Nation.Regions)
            {
                float avgSupport = 0f;
                int districtCount = 0;
                
                foreach (var state in region.States)
                {
                    foreach (var district in state.Districts)
                    {
                        float polling = gameState.VoterSim.CalculateDistrictPolling(player, district);
                        avgSupport += polling;
                        districtCount++;
                    }
                }
                
                if (districtCount > 0)
                {
                    avgSupport /= districtCount;
                    
                    if (avgSupport > 50f)
                        controlled++;
                }
            }
            
            ConditionProgress["RegionsControlled"] = controlled;
            return controlled >= 10;
        }
        
        private bool CheckScandalVictory(PlayerState player)
        {
            // Count major scandals survived (severity >= 5)
            int majorSurvived = 0;
            
            if (player.ScandalHistory != null)
            {
                majorSurvived = player.ScandalHistory
                    .Where(s => s.Severity >= 50f) // 50+ severity = major
                    .Count();
            }
            
            ConditionProgress["ScandalsSurvived"] = majorSurvived;
            return majorSurvived >= 10;
        }
        
        private bool CheckTimeLimitVictory(PlayerState player, float deltaTime)
        {
            ConditionProgress["DaysElapsed"] += deltaTime / 86400f;
            
            // Time limit reached
            if (ConditionProgress["DaysElapsed"] >= ConditionProgress["TimeLimit"])
            {
                // Winner is whoever has highest office
                return player.HighestTierHeld >= 4; // At least Tier 4
            }
            
            return false;
        }
        
        private void TriggerVictory(PlayerState player)
        {
            // Calculate final score
            int score = CalculateFinalScore(player);
            
            // Record victory
            player.VictoriesAchieved++;
            
            // Calculate legacy points earned
            int legacyPoints = CalculateLegacyPoints(player, SelectedCondition);
            
            // Award legacy points
            player.TotalLegacyPoints += legacyPoints;
            
            Debug.Log($"VICTORY! {player.Character.Name} achieved {SelectedCondition}!");
            Debug.Log($"Final Score: {score}");
            Debug.Log($"Legacy Points Earned: {legacyPoints}");
            
            // UI would show victory screen here
        }
        
        private int CalculateFinalScore(PlayerState player)
        {
            int score = 0;
            
            // Base score from office tier
            score += player.HighestTierHeld * 1000;
            
            // Bonus for approval rating
            score += (int)(player.ApprovalRating * 10);
            
            // Bonus for elections won
            score += player.ElectionsWon * 500;
            
            // Bonus for scandals survived
            if (player.ScandalHistory != null)
                score += player.ScandalHistory.Count * 100;
            
            // Bonus for difficulty
            score += player.CampaignDifficulty * 500;
            
            // Penalty for active scandals
            score -= player.ActiveScandals.Count * 200;
            
            return Mathf.Max(0, score);
        }
        
        private int CalculateLegacyPoints(PlayerState player, VictoryType condition)
        {
            int basePoints = 100;
            
            // Multiply by highest tier
            basePoints *= player.HighestTierHeld;
            
            // Bonus for hard mode
            if (player.Character != null)
                basePoints += player.Character.ChaosRating * 50;
            
            // Bonus for specific victory types
            switch (condition)
            {
                case VictoryType.ReachPresident:
                    basePoints += 200;
                    break;
                case VictoryType.ScandalSurvival:
                    basePoints += 500; // Harder achievement
                    break;
                case VictoryType.TotalDomination:
                    basePoints += 300;
                    break;
            }
            
            return basePoints;
        }
    }
}

