using System.Linq;
using UnityEngine;
using ElectionEmpire.World;

namespace ElectionEmpire.Gameplay
{
    /// <summary>
    /// Manages defeat conditions
    /// </summary>
    public class DefeatConditionManager
    {
        private GameState gameState;
        
        public enum DefeatType
        {
            Impeachment,          // Removed from office
            ElectionLoss,         // Lost too many elections
            ScandalOverwhelm,     // Too many unresolved scandals
            Bankruptcy,           // Ran out of money
            PartyExpulsion,       // Kicked out of party
            Assassination,        // Rare extreme event
            TermLimitReached      // Can't run again, nowhere to go
        }
        
        public DefeatConditionManager(GameState gameState)
        {
            this.gameState = gameState;
        }
        
        public void CheckDefeatConditions(PlayerState player)
        {
            // Check various defeat conditions
            
            // Impeachment (approval too low for too long)
            if (player.ApprovalRating < 20f && player.DaysBelowThreshold > 60)
            {
                TriggerDefeat(player, DefeatType.Impeachment);
                return;
            }
            
            // Scandal overwhelm (5+ active major scandals)
            if (player.ActiveScandals != null && 
                player.ActiveScandals.Count(s => s.Severity >= 50f) >= 5)
            {
                TriggerDefeat(player, DefeatType.ScandalOverwhelm);
                return;
            }
            
            // Bankruptcy (negative funds for 30 days)
            if (player.Resources.ContainsKey("CampaignFunds") && 
                player.Resources["CampaignFunds"] < 0 && 
                player.DaysInDebt > 30)
            {
                TriggerDefeat(player, DefeatType.Bankruptcy);
                return;
            }
            
            // Lost 3 elections in a row
            if (player.ConsecutiveElectionLosses >= 3)
            {
                TriggerDefeat(player, DefeatType.ElectionLoss);
                return;
            }
            
            // Party expulsion (loyalty too low)
            if (player.Resources.ContainsKey("PartyLoyalty") && 
                player.Resources["PartyLoyalty"] < 10f && 
                !string.IsNullOrEmpty(player.Party))
            {
                TriggerDefeat(player, DefeatType.PartyExpulsion);
                return;
            }
        }
        
        private void TriggerDefeat(PlayerState player, DefeatType type)
        {
            Debug.LogWarning($"DEFEAT! {player.Character.Name} defeated by: {type}");
            
            // Still award some legacy points
            int legacyPoints = player.HighestTierHeld * 20;
            player.TotalLegacyPoints += legacyPoints;
            
            Debug.Log($"Legacy Points Awarded: {legacyPoints}");
            
            // UI would show defeat screen here
        }
    }
}

