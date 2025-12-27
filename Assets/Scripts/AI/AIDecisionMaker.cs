using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.World;
using ElectionEmpire.Gameplay;
using ElectionEmpire.Core;
using GameState = ElectionEmpire.Gameplay.GameState;

namespace ElectionEmpire.AI
{
    /// <summary>
    /// Makes decisions for AI opponents based on personality and situation
    /// </summary>
    public class AIDecisionMaker
    {
        private AIOpponent opponent;
        private ElectionEmpire.World.World world;
        private VoterSimulation voterSim;
        private List<AIOpponent> allOpponents;
        
        public AIDecisionMaker(AIOpponent opponent, ElectionEmpire.World.World world, VoterSimulation voterSim, List<AIOpponent> allOpponents)
        {
            this.opponent = opponent;
            this.world = world;
            this.voterSim = voterSim;
            this.allOpponents = allOpponents;
        }
        
        /// <summary>
        /// Make AI take their turn
        /// </summary>
        public void TakeTurn(GameState gameState)
        {
            // 1. Analyze current situation
            var situation = AnalyzeSituation(gameState);
            
            // 2. Determine available actions
            var availableActions = GetAvailableActions(gameState);
            
            if (availableActions.Count == 0)
                return;
            
            // 3. Score each action based on personality and goals
            var scoredActions = ScoreActions(availableActions, situation);
            
            // 4. Select best action (with some randomness)
            var selectedAction = SelectAction(scoredActions);
            
            // 5. Execute action
            ExecuteAction(selectedAction, gameState);
            
            // 6. Update strategy if needed
            AdaptStrategy(gameState);
        }
        
        private SituationAnalysis AnalyzeSituation(GameState gameState)
        {
            var analysis = new SituationAnalysis();
            
            // Where am I in the race?
            var playerState = CreatePlayerStateFromOpponent(opponent);
            analysis.CurrentPolling = voterSim.CalculateNationalApproval(playerState);
            analysis.PollingRank = GetPollingRank(gameState);
            
            // How are resources?
            analysis.ResourcesLow = IsResourcesLow();
            analysis.ResourcesHigh = IsResourcesHigh();
            
            // What threats exist?
            analysis.UnderAttack = IsUnderAttack(gameState);
            analysis.ScandalActive = opponent.ActiveScandals.Count > 0;
            analysis.CrisisActive = HasActiveCrisis(gameState);
            
            // What opportunities exist?
            analysis.OpponentVulnerable = FindVulnerableOpponents(gameState);
            analysis.AllianceOpportunity = FindAllianceOpportunities(gameState);
            
            // Time pressure
            analysis.DaysUntilElection = gameState.DaysUntilElection;
            analysis.TimePressure = analysis.DaysUntilElection < 30;
            
            return analysis;
        }
        
        private List<AIAction> GetAvailableActions(GameState gameState)
        {
            var actions = new List<AIAction>();
            
            // Always can campaign
            actions.Add(new AIAction { Type = DecisionType.Campaign, Subtype = "Rally" });
            actions.Add(new AIAction { Type = DecisionType.Campaign, Subtype = "Ads" });
            actions.Add(new AIAction { Type = DecisionType.Campaign, Subtype = "DoorToDoor" });
            
            // Media actions if have influence
            if (opponent.Resources.ContainsKey("MediaInfluence") && opponent.Resources["MediaInfluence"] > 20)
            {
                actions.Add(new AIAction { Type = DecisionType.Media, Subtype = "PressConference" });
                actions.Add(new AIAction { Type = DecisionType.Media, Subtype = "Interview" });
            }
            
            // Policy actions if in office
            if (opponent.CurrentOffice != null)
            {
                actions.Add(new AIAction { Type = DecisionType.Policy, Subtype = "Propose" });
                actions.Add(new AIAction { Type = DecisionType.Policy, Subtype = "Implement" });
            }
            
            // Attack actions if aggressive enough
            if (opponent.Personality.Aggression > 40)
            {
                foreach (var target in allOpponents)
                {
                    if (target.ID != opponent.ID)
                    {
                        actions.Add(new AIAction { 
                            Type = DecisionType.Attack, 
                            Subtype = "DebateAttack",
                            Target = target.ID
                        });
                        
                        actions.Add(new AIAction { 
                            Type = DecisionType.Attack, 
                            Subtype = "AdAttack",
                            Target = target.ID
                        });
                    }
                }
            }
            
            // Dirty tricks if ethically flexible
            if (opponent.Personality.EthicalFlexibility > 60)
            {
                actions.Add(new AIAction { Type = DecisionType.Scandal, Subtype = "OppositionResearch" });
                actions.Add(new AIAction { Type = DecisionType.Scandal, Subtype = "PlantStory" });
            }
            
            // Alliance actions
            if (opponent.Personality.Loyalty > 40)
            {
                foreach (var potential in allOpponents)
                {
                    if (potential.ID != opponent.ID && IsAllianceViable(potential))
                    {
                        actions.Add(new AIAction { 
                            Type = DecisionType.Alliance, 
                            Subtype = "Propose",
                            Target = potential.ID
                        });
                    }
                }
            }
            
            return actions;
        }
        
        private Dictionary<AIAction, float> ScoreActions(List<AIAction> actions, SituationAnalysis situation)
        {
            var scores = new Dictionary<AIAction, float>();
            
            foreach (var action in actions)
            {
                float score = 0f;
                
                switch (action.Type)
                {
                    case DecisionType.Campaign:
                        score = ScoreCampaignAction(action, situation);
                        break;
                    
                    case DecisionType.Attack:
                        score = ScoreAttackAction(action, situation);
                        break;
                    
                    case DecisionType.Alliance:
                        score = ScoreAllianceAction(action, situation);
                        break;
                    
                    case DecisionType.Policy:
                        score = ScorePolicyAction(action, situation);
                        break;
                    
                    case DecisionType.Scandal:
                        score = ScoreScandalAction(action, situation);
                        break;
                    
                    case DecisionType.Media:
                        score = ScoreMediaAction(action, situation);
                        break;
                }
                
                // Modify score by personality
                score = ModifyScoreByPersonality(score, action);
                
                // Modify score by current strategy
                score = ModifyScoreByStrategy(score, action);
                
                scores[action] = score;
            }
            
            return scores;
        }
        
        private float ScoreCampaignAction(AIAction action, SituationAnalysis situation)
        {
            float score = 50f; // Base score
            
            // Desperate if losing
            if (situation.PollingRank > 3)
                score += 30f;
            
            // Time pressure
            if (situation.TimePressure)
                score += 20f;
            
            // Different campaign actions for different situations
            switch (action.Subtype)
            {
                case "Rally":
                    // Better if have charisma
                    score += opponent.Personality.Charisma * 0.3f;
                    break;
                
                case "Ads":
                    // Better if have funds
                    if (opponent.Resources.ContainsKey("CampaignFunds") && 
                        opponent.Resources["CampaignFunds"] > 50000)
                        score += 20f;
                    break;
                
                case "DoorToDoor":
                    // Better if grassroots style
                    if (opponent.Archetype == Archetype.Populist || 
                        opponent.Archetype == Archetype.Idealist)
                        score += 30f;
                    break;
            }
            
            return score;
        }
        
        private float ScoreAttackAction(AIAction action, SituationAnalysis situation)
        {
            float score = 30f; // Base score (attacks are risky)
            
            // More aggressive when behind
            if (situation.PollingRank > 2)
                score += 40f;
            
            // More aggressive if being attacked
            if (situation.UnderAttack)
                score += 30f;
            
            // Personality factor
            score += opponent.Personality.Aggression * 0.5f;
            
            // Target vulnerability
            var target = GetOpponent(action.Target);
            if (target != null && target.ActiveScandals.Count > 0)
                score += 25f; // Attack wounded opponents
            
            // Risk tolerance
            score *= (opponent.Personality.RiskTolerance / 100f);
            
            return score;
        }
        
        private float ScoreAllianceAction(AIAction action, SituationAnalysis situation)
        {
            float score = 40f;
            
            // Better if weak
            if (situation.PollingRank > 3)
                score += 30f;
            
            // Personality factor
            score += opponent.Personality.Loyalty * 0.4f;
            
            // Strategic benefit
            var target = GetOpponent(action.Target);
            if (target != null)
            {
                // Ally with similar ideologies
                if (IsIdeologicallyCompatible(opponent, target))
                    score += 20f;
                
                // Ally with strong opponents
                if (target.ApprovalRating > opponent.ApprovalRating)
                    score += 15f;
            }
            
            return score;
        }
        
        private float ScorePolicyAction(AIAction action, SituationAnalysis situation)
        {
            float score = 40f;
            
            // Better if technocrat or idealist
            if (opponent.Archetype == Archetype.Technocrat || 
                opponent.Archetype == Archetype.Idealist)
                score += 30f;
            
            return score;
        }
        
        private float ScoreScandalAction(AIAction action, SituationAnalysis situation)
        {
            float score = 20f; // Base low (dirty tricks risky)
            
            // Desperate times
            if (situation.PollingRank > 4)
                score += 50f;
            
            // Personality factors
            score += opponent.Personality.EthicalFlexibility * 0.6f;
            score += opponent.Personality.Cunning * 0.3f;
            
            // Risk tolerance
            score *= (opponent.Personality.RiskTolerance / 100f);
            
            // Archetype factor
            if (opponent.Archetype == Archetype.MachineBoss ||
                opponent.Archetype == Archetype.Maverick)
                score += 30f;
            
            return score;
        }
        
        private float ScoreMediaAction(AIAction action, SituationAnalysis situation)
        {
            float score = 35f;
            
            // Better if showman
            if (opponent.Archetype == Archetype.Showman)
                score += 30f;
            
            // Better if have charisma
            score += opponent.Personality.Charisma * 0.3f;
            
            return score;
        }
        
        private float ModifyScoreByPersonality(float score, AIAction action)
        {
            // Impulsive AI sometimes ignore scores
            if (opponent.Personality.Impulsiveness > 70 && UnityEngine.Random.value < 0.2f)
            {
                return UnityEngine.Random.Range(0f, 100f); // Random score
            }
            
            return score;
        }
        
        private float ModifyScoreByStrategy(float score, AIAction action)
        {
            switch (opponent.CurrentStrategy)
            {
                case AIStrategy.Aggressive:
                    if (action.Type == DecisionType.Attack)
                        score *= 1.5f;
                    break;
                
                case AIStrategy.Defensive:
                    if (action.Type == DecisionType.Campaign)
                        score *= 1.3f;
                    if (action.Type == DecisionType.Attack)
                        score *= 0.5f;
                    break;
                
                case AIStrategy.Cooperative:
                    if (action.Type == DecisionType.Alliance)
                        score *= 1.5f;
                    break;
                
                case AIStrategy.Opportunistic:
                    // Boost actions that exploit opportunities
                    score *= 1.2f;
                    break;
            }
            
            return score;
        }
        
        private AIAction SelectAction(Dictionary<AIAction, float> scoredActions)
        {
            // Store situation for use in helper methods
            this.situation = AnalyzeSituation(new GameState { Opponents = allOpponents });
            
            // HUMAN FOIBLES: Sometimes make irrational decisions like real people
            
            // 1. Check for "human moment" - completely irrational decision
            float foibleScore = opponent.Personality.GetFoibleScore();
            if (UnityEngine.Random.value < foibleScore * 0.15f) // 15% of foible score chance
            {
                return MakeIrrationalDecision(scoredActions);
            }
            
            // 2. Ego-driven bad decision (Napoleon invading Russia, Caesar ignoring warnings)
            if (opponent.Personality.Ego > 80f && UnityEngine.Random.value < 0.2f)
            {
                // Pick the most "impressive" action, even if not optimal
                var egoActions = scoredActions.Where(kvp => 
                    kvp.Key.Type == DecisionType.Attack || 
                    kvp.Key.Type == DecisionType.Media ||
                    kvp.Key.Type == DecisionType.Campaign
                ).ToList();
                
                if (egoActions.Count > 0)
                {
                    var egoAction = egoActions.OrderByDescending(kvp => kvp.Value).First();
                    Debug.Log($"{opponent.Name} makes ego-driven decision: {egoAction.Key.Type} (score: {egoAction.Value:F1})");
                    return egoAction.Key;
                }
            }
            
            // 3. Paranoia-driven decision (attack perceived threat even if unwise)
            if (opponent.Personality.Paranoia > 70f && UnityEngine.Random.value < 0.25f)
            {
                var paranoidActions = scoredActions.Where(kvp => 
                    kvp.Key.Type == DecisionType.Attack ||
                    kvp.Key.Type == DecisionType.Scandal
                ).ToList();
                
                if (paranoidActions.Count > 0)
                {
                    var paranoidAction = paranoidActions[UnityEngine.Random.Range(0, paranoidActions.Count)];
                    Debug.Log($"{opponent.Name} acts on paranoia: {paranoidAction.Key.Type}");
                    return paranoidAction.Key;
                }
            }
            
            // 4. Hubris-driven overreach (overconfident, takes unnecessary risks)
            if (opponent.Personality.Hubris > 75f && opponent.ApprovalRating > 60f && UnityEngine.Random.value < 0.3f)
            {
                // When doing well, hubris makes them take bigger risks
                var riskyActions = scoredActions.Where(kvp => 
                    kvp.Key.Type == DecisionType.Attack ||
                    kvp.Key.Type == DecisionType.Scandal ||
                    (kvp.Key.Type == DecisionType.Policy && UnityEngine.Random.value < 0.5f)
                ).ToList();
                
                if (riskyActions.Count > 0)
                {
                    var riskyAction = riskyActions.OrderByDescending(kvp => kvp.Value).First();
                    Debug.Log($"{opponent.Name} acts on hubris - overconfident move: {riskyAction.Key.Type}");
                    return riskyAction.Key;
                }
            }
            
            // 5. Pride-driven stubbornness (won't back down even when losing)
            if (opponent.Personality.Pride > 80f && situation.PollingRank > 3 && UnityEngine.Random.value < 0.4f)
            {
                // When losing, pride makes them double down instead of changing strategy
                var currentStrategyActions = scoredActions.Where(kvp => 
                    MatchesCurrentStrategy(kvp.Key)
                ).ToList();
                
                if (currentStrategyActions.Count > 0)
                {
                    var prideAction = currentStrategyActions[UnityEngine.Random.Range(0, currentStrategyActions.Count)];
                    Debug.Log($"{opponent.Name} too proud to change strategy: {prideAction.Key.Type}");
                    return prideAction.Key;
                }
            }
            
            // 6. Emotional volatility - random mood swing
            if (opponent.Personality.EmotionalVolatility > 70f && UnityEngine.Random.value < 0.3f)
            {
                // Completely random action based on "mood"
                var randomAction = scoredActions.Keys.ElementAt(UnityEngine.Random.Range(0, scoredActions.Count));
                Debug.Log($"{opponent.Name} has emotional mood swing: {randomAction.Type}");
                return randomAction;
            }
            
            // 7. Obsession - single-minded focus on one thing
            if (opponent.Personality.Obsession > 75f && UnityEngine.Random.value < 0.4f)
            {
                // Focus on their obsession even if not optimal
                var obsessionActions = GetObsessionActions(scoredActions);
                if (obsessionActions.Count > 0)
                {
                    var obsessionAction = obsessionActions[UnityEngine.Random.Range(0, obsessionActions.Count)];
                    Debug.Log($"{opponent.Name} acts on obsession: {obsessionAction.Type}");
                    return obsessionAction;
                }
            }
            
            // 8. Standard decision-making (with impulsiveness)
            float impulsiveness = opponent.Personality.Impulsiveness / 100f;
            
            if (UnityEngine.Random.value < impulsiveness * 0.5f) // Reduced from full impulsiveness
            {
                // Impulsive: pick random action
                return scoredActions.Keys.ElementAt(UnityEngine.Random.Range(0, scoredActions.Count));
            }
            else
            {
                // Calculated: pick from top 3
                var topActions = scoredActions.OrderByDescending(kvp => kvp.Value).Take(3).ToList();
                
                if (topActions.Count == 0)
                    return scoredActions.Keys.First();
                
                // Weighted selection from top 3
                float totalScore = topActions.Sum(kvp => kvp.Value);
                if (totalScore == 0) totalScore = 1f;
                
                float roll = UnityEngine.Random.value * totalScore;
                float cumulative = 0f;
                
                foreach (var kvp in topActions)
                {
                    cumulative += kvp.Value;
                    if (roll <= cumulative)
                        return kvp.Key;
                }
                
                return topActions[0].Key;
            }
        }
        
        private AIAction MakeIrrationalDecision(Dictionary<AIAction, float> scoredActions)
        {
            // Completely irrational - pick worst action or random weird one
            // Like historical figures making terrible decisions
            
            string[] irrationalReasons = new[] {
                "gut feeling", "dream", "sign from above", "advice from a friend",
                "read it in a book", "saw it in a movie", "felt lucky", "wanted to prove a point"
            };
            
            string reason = irrationalReasons[UnityEngine.Random.Range(0, irrationalReasons.Length)];
            
            // Sometimes pick worst action
            if (UnityEngine.Random.value < 0.4f)
            {
                var worstAction = scoredActions.OrderBy(kvp => kvp.Value).First();
                Debug.Log($"{opponent.Name} makes irrational decision ({reason}): {worstAction.Key.Type} (WORST SCORE: {worstAction.Value:F1})");
                return worstAction.Key;
            }
            else
            {
                // Or just random
                var randomAction = scoredActions.Keys.ElementAt(UnityEngine.Random.Range(0, scoredActions.Count));
                Debug.Log($"{opponent.Name} makes irrational decision ({reason}): {randomAction.Type}");
                return randomAction;
            }
        }
        
        private bool MatchesCurrentStrategy(AIAction action)
        {
            switch (opponent.CurrentStrategy)
            {
                case AIStrategy.Aggressive:
                    return action.Type == DecisionType.Attack;
                case AIStrategy.Defensive:
                    return action.Type == DecisionType.Campaign;
                case AIStrategy.Cooperative:
                    return action.Type == DecisionType.Alliance;
                default:
                    return true;
            }
        }
        
        private List<AIAction> GetObsessionActions(Dictionary<AIAction, float> scoredActions)
        {
            // Return actions that match their obsession/goals
            var obsessionActions = new List<AIAction>();
            
            foreach (var goal in opponent.Goals.OrderByDescending(kvp => kvp.Value).Take(1))
            {
                switch (goal.Key)
                {
                    case "ImplementPolicies":
                        obsessionActions.AddRange(scoredActions.Keys.Where(a => a.Type == DecisionType.Policy));
                        break;
                    case "AttackElites":
                    case "DisruptSystem":
                        obsessionActions.AddRange(scoredActions.Keys.Where(a => a.Type == DecisionType.Attack));
                        break;
                    case "BuildAlliances":
                        obsessionActions.AddRange(scoredActions.Keys.Where(a => a.Type == DecisionType.Alliance));
                        break;
                }
            }
            
            return obsessionActions;
        }
        
        private SituationAnalysis situation; // Store for use in SelectAction
        
        private void ExecuteAction(AIAction action, GameState gameState)
        {
            // Apply the action's effects (simplified for now)
            
            switch (action.Type)
            {
                case DecisionType.Campaign:
                    ExecuteCampaignAction(action, gameState);
                    break;
                
                case DecisionType.Attack:
                    ExecuteAttackAction(action, gameState);
                    break;
                
                case DecisionType.Alliance:
                    ExecuteAllianceAction(action, gameState);
                    break;
                
                case DecisionType.Media:
                    ExecuteMediaAction(action, gameState);
                    break;
            }
            
            // Record action in history
            opponent.ActionHistory.Add(new AIAction
            {
                ActionType = action.Type.ToString(),
                Target = action.Target,
                Timestamp = DateTime.Now,
                Successful = true
            });
            
            // Generate storyline if significant
            if (action.Type == DecisionType.Alliance || action.Type == DecisionType.Attack)
            {
                GenerateStoryline(action, gameState);
            }
        }
        
        private void ExecuteCampaignAction(AIAction action, GameState gameState)
        {
            // Spend resources
            if (opponent.Resources.ContainsKey("CampaignFunds"))
                opponent.Resources["CampaignFunds"] -= 5000f;
            
            // Boost approval slightly
            opponent.ApprovalRating += UnityEngine.Random.Range(1f, 3f);
        }
        
        private void ExecuteAttackAction(AIAction action, GameState gameState)
        {
            var target = GetOpponent(action.Target);
            if (target != null)
            {
                // Damage target's approval
                target.ApprovalRating -= UnityEngine.Random.Range(2f, 5f);
                
                // May trigger counter-attack
                if (target.Personality.Aggression > 60 && UnityEngine.Random.value < 0.5f)
                {
                    // Target will counter-attack next turn
                }
            }
        }
        
        private void ExecuteAllianceAction(AIAction action, GameState gameState)
        {
            var target = GetOpponent(action.Target);
            if (target != null && target.Personality.Loyalty > 50)
            {
                // Form alliance
                if (!opponent.Relationships.Contains(target.ID))
                    opponent.Relationships.Add(target.ID);
                
                if (!target.Relationships.Contains(opponent.ID))
                    target.Relationships.Add(opponent.ID);
            }
        }
        
        private void ExecuteMediaAction(AIAction action, GameState gameState)
        {
            // Boost media influence
            if (opponent.Resources.ContainsKey("MediaInfluence"))
                opponent.Resources["MediaInfluence"] += 5f;
            
            // Boost approval
            opponent.ApprovalRating += UnityEngine.Random.Range(1f, 2f);
        }
        
        private void AdaptStrategy(GameState gameState)
        {
            // Change strategy based on performance
            
            if (opponent.Difficulty == AIDifficulty.Adaptive)
            {
                // Check if current strategy is working
                float recentApprovalChange = CalculateRecentApprovalChange();
                
                if (recentApprovalChange < -5f)
                {
                    // Strategy isn't working, switch
                    opponent.CurrentStrategy = SelectNewStrategy();
                }
            }
        }
        
        private void GenerateStoryline(AIAction action, GameState gameState)
        {
            // Create narrative around significant actions
            
            string storyline = "";
            
            switch (action.Type)
            {
                case DecisionType.Alliance:
                    var ally = GetOpponent(action.Target);
                    if (ally != null)
                        storyline = $"{opponent.Name} and {ally.Name} form unexpected alliance!";
                    break;
                
                case DecisionType.Attack:
                    var target = GetOpponent(action.Target);
                    if (target != null)
                        storyline = $"{opponent.Name} launches brutal attack on {target.Name}!";
                    break;
            }
            
            if (!string.IsNullOrEmpty(storyline))
            {
                opponent.Storylines.Add(storyline);
            }
        }
        
        // Helper methods
        private int GetPollingRank(GameState gameState)
        {
            // Calculate rank among all opponents
            var allRatings = allOpponents.Select(o => o.ApprovalRating).ToList();
            allRatings.Add(gameState.PlayerApproval);
            allRatings = allRatings.OrderByDescending(r => r).ToList();
            
            return allRatings.IndexOf(opponent.ApprovalRating) + 1;
        }
        
        private bool IsResourcesLow()
        {
            if (opponent.Resources.ContainsKey("CampaignFunds"))
                return opponent.Resources["CampaignFunds"] < 10000f;
            return false;
        }
        
        private bool IsResourcesHigh()
        {
            if (opponent.Resources.ContainsKey("CampaignFunds"))
                return opponent.Resources["CampaignFunds"] > 100000f;
            return false;
        }
        
        private bool IsUnderAttack(GameState gameState)
        {
            // Check if any opponent recently attacked this AI
            return opponent.ActionHistory.Any(a => 
                a.ActionType == "Attack" && 
                (DateTime.Now - a.Timestamp).TotalDays < 7);
        }
        
        private bool HasActiveCrisis(GameState gameState)
        {
            // Check if there's an active crisis
            return gameState.ActiveCrises != null && gameState.ActiveCrises.Count > 0;
        }
        
        private List<string> FindVulnerableOpponents(GameState gameState)
        {
            return allOpponents
                .Where(o => o.ActiveScandals.Count > 0 || o.ApprovalRating < 40f)
                .Select(o => o.ID)
                .ToList();
        }
        
        private List<string> FindAllianceOpportunities(GameState gameState)
        {
            return allOpponents
                .Where(o => o.Personality.Loyalty > 50 && 
                           !opponent.Relationships.Contains(o.ID))
                .Select(o => o.ID)
                .ToList();
        }
        
        private bool IsAllianceViable(AIOpponent potential)
        {
            return potential.Personality.Loyalty > 40 && 
                   !opponent.Relationships.Contains(potential.ID);
        }
        
        private bool IsIdeologicallyCompatible(AIOpponent a, AIOpponent b)
        {
            // Check if policy stances are similar
            float totalDifference = 0f;
            int count = 0;
            
            foreach (var stance in a.PolicyStances)
            {
                if (b.PolicyStances.ContainsKey(stance.Key))
                {
                    totalDifference += Mathf.Abs(stance.Value - b.PolicyStances[stance.Key]);
                    count++;
                }
            }
            
            if (count == 0) return false;
            
            float avgDifference = totalDifference / count;
            return avgDifference < 20f; // Similar if within 20 points
        }
        
        private AIOpponent GetOpponent(string id)
        {
            return allOpponents.FirstOrDefault(o => o.ID == id);
        }
        
        private PlayerState CreatePlayerStateFromOpponent(AIOpponent ai)
        {
            var playerState = new PlayerState(ai.Character);
            playerState.PolicyStances = new Dictionary<Issue, float>(ai.PolicyStances);
            playerState.VoterBlocSupport = new Dictionary<VoterBloc, float>(ai.VoterBlocSupport);
            playerState.MediaInfluence = ai.Resources.GetValueOrDefault("MediaInfluence", 0f);
            playerState.CampaignFunds = ai.Resources.GetValueOrDefault("CampaignFunds", 0f);
            return playerState;
        }
        
        private float CalculateRecentApprovalChange()
        {
            // Calculate approval change over last few turns
            // Simplified: return random for now
            return UnityEngine.Random.Range(-10f, 10f);
        }
        
        private AIStrategy SelectNewStrategy()
        {
            // Select new strategy based on situation
            var strategies = System.Enum.GetValues(typeof(AIStrategy)).Cast<AIStrategy>().ToList();
            return strategies[UnityEngine.Random.Range(0, strategies.Count)];
        }
    }
}

