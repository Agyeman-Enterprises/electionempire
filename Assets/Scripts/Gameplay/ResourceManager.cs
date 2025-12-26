using System;
using System.Collections.Generic;
using UnityEngine;
using ElectionEmpire.World;

namespace ElectionEmpire.Gameplay
{
    /// <summary>
    /// Manages all player resources
    /// </summary>
    public class ResourceManager
    {
        private PlayerState player;
        
        // Core resources (stored in player.Resources)
        public float PublicTrust => player.Resources.GetValueOrDefault("PublicTrust", 50f);
        public int PoliticalCapital => (int)player.Resources.GetValueOrDefault("PoliticalCapital", 0f);
        public float CampaignFunds => player.Resources.GetValueOrDefault("CampaignFunds", 0f);
        public int MediaInfluence => (int)player.Resources.GetValueOrDefault("MediaInfluence", 0f);
        public float PartyLoyalty => player.Resources.GetValueOrDefault("PartyLoyalty", 50f);
        
        // Dirt/blackmail tracking
        public Dictionary<string, List<BlackmailItem>> Blackmail;
        
        public ResourceManager(PlayerState player)
        {
            this.player = player;
            Blackmail = new Dictionary<string, List<BlackmailItem>>();
        }
        
        /// <summary>
        /// Update resources every turn
        /// </summary>
        public void UpdateResources(float deltaTime)
        {
            // 1. Natural decay
            ApplyDecay(deltaTime);
            
            // 2. Office bonuses
            ApplyOfficeBonuses(deltaTime);
            
            // 3. Campaign burn rate
            ApplyCampaignCosts(deltaTime);
            
            // 4. Cap resources
            EnforceResourceCaps();
        }
        
        private void ApplyDecay(float deltaTime)
        {
            float days = deltaTime / 86400f; // Convert to days
            
            // Public trust decays slowly
            float trustDecay = 0.25f * days; // 0.25% per day
            player.Resources["PublicTrust"] = Mathf.Clamp(
                player.Resources["PublicTrust"] - trustDecay, 0f, 100f);
            
            // Political capital decays
            float capitalDecay = PoliticalCapital * 0.05f * days;
            player.Resources["PoliticalCapital"] = Mathf.Max(0, 
                (int)(PoliticalCapital - capitalDecay));
            
            // Media influence decays without refreshing
            float mediaDecay = 10f * days;
            player.Resources["MediaInfluence"] = Mathf.Max(0, 
                MediaInfluence - (int)mediaDecay);
            
            // Party loyalty decays if not active
            if (!IsActiveInParty())
            {
                float loyaltyDecay = 1f * days;
                player.Resources["PartyLoyalty"] = Mathf.Clamp(
                    player.Resources["PartyLoyalty"] - loyaltyDecay, 0f, 100f);
            }
        }
        
        private void ApplyOfficeBonuses(float deltaTime)
        {
            if (player.CurrentOffice != null)
            {
                float days = deltaTime / 86400f;
                
                // Monthly salary (convert to daily)
                float monthlyBonus = player.CurrentOffice.Salary;
                float dailyBonus = monthlyBonus / 30f;
                player.Resources["CampaignFunds"] += dailyBonus * days;
                
                // Resource bonuses from office
                if (player.CurrentOffice.ResourceBonuses != null)
                {
                    foreach (var bonus in player.CurrentOffice.ResourceBonuses)
                    {
                        switch (bonus.Key)
                        {
                            case "MediaInfluence":
                                player.Resources["MediaInfluence"] += (bonus.Value / 30f) * days;
                                break;
                            case "PoliticalCapital":
                                player.Resources["PoliticalCapital"] += (bonus.Value / 30f) * days;
                                break;
                        }
                    }
                }
            }
        }
        
        private void ApplyCampaignCosts(float deltaTime)
        {
            // Campaign has ongoing costs
            if (player.IsInCampaign)
            {
                float days = deltaTime / 86400f;
                float dailyBurnRate = CalculateBurnRate();
                player.Resources["CampaignFunds"] -= dailyBurnRate * days;
                
                // Can't go negative (triggers debt crisis)
                if (player.Resources["CampaignFunds"] < 0)
                {
                    TriggerDebtCrisis();
                    player.Resources["CampaignFunds"] = 0;
                }
            }
        }
        
        private float CalculateBurnRate()
        {
            float baseRate = 1000f; // $1000/day baseline
            
            // Increases with tier
            if (player.CurrentOffice != null)
                baseRate *= player.CurrentOffice.Tier;
            else
                baseRate *= 1f; // No office = base rate
            
            // Increases with campaign intensity
            baseRate *= player.CampaignIntensity; // 0.5-2.0
            
            return baseRate;
        }
        
        private bool IsActiveInParty()
        {
            // Check if player is actively participating in party activities
            // Simplified: if party loyalty > 50, consider active
            return PartyLoyalty > 50f;
        }
        
        // Resource transactions
        public bool SpendFunds(float amount, string reason)
        {
            if (CampaignFunds >= amount)
            {
                player.Resources["CampaignFunds"] -= amount;
                Debug.Log($"Spent ${amount:F2}: {reason}");
                return true;
            }
            return false;
        }
        
        public bool SpendPoliticalCapital(int amount, string reason)
        {
            if (PoliticalCapital >= amount)
            {
                player.Resources["PoliticalCapital"] -= amount;
                Debug.Log($"Used {amount} Political Capital: {reason}");
                return true;
            }
            return false;
        }
        
        public void GainTrust(float amount, string reason)
        {
            player.Resources["PublicTrust"] += amount;
            player.Resources["PublicTrust"] = Mathf.Clamp(player.Resources["PublicTrust"], 0f, 100f);
            Debug.Log($"Gained {amount:F1}% Public Trust: {reason}");
        }
        
        public void LoseTrust(float amount, string reason)
        {
            player.Resources["PublicTrust"] -= amount;
            player.Resources["PublicTrust"] = Mathf.Clamp(player.Resources["PublicTrust"], 0f, 100f);
            Debug.Log($"Lost {amount:F1}% Public Trust: {reason}");
        }
        
        // Blackmail management
        public void AcquireBlackmail(string targetID, BlackmailItem item)
        {
            if (!Blackmail.ContainsKey(targetID))
                Blackmail[targetID] = new List<BlackmailItem>();
            
            Blackmail[targetID].Add(item);
            
            // Max 5 per target
            if (Blackmail[targetID].Count > 5)
                Blackmail[targetID].RemoveAt(0);
        }
        
        public bool UseBlackmail(string targetID, string itemID)
        {
            if (Blackmail.ContainsKey(targetID))
            {
                var item = Blackmail[targetID].Find(b => b.ID == itemID);
                if (item != null)
                {
                    // 30% chance of backfire
                    if (UnityEngine.Random.value < 0.3f)
                    {
                        TriggerBlackmailBackfire(item);
                        return false;
                    }
                    
                    // Apply blackmail effects
                    ApplyBlackmailEffects(targetID, item);
                    
                    // Remove from inventory
                    Blackmail[targetID].Remove(item);
                    
                    return true;
                }
            }
            return false;
        }
        
        private void TriggerDebtCrisis()
        {
            player.DaysInDebt++;
            
            // Debt crisis effects
            LoseTrust(5f, "Debt Crisis");
            
            // May trigger event
            if (player.DaysInDebt > 30)
            {
                // Trigger bankruptcy event
                Debug.LogWarning("Bankruptcy crisis!");
            }
        }
        
        private void TriggerBlackmailBackfire(BlackmailItem item)
        {
            Debug.LogWarning($"Blackmail backfired! {item.Description}");
            LoseTrust(item.Severity * 2f, "Blackmail Backfire");
        }
        
        private void ApplyBlackmailEffects(string targetID, BlackmailItem item)
        {
            // Apply effects based on blackmail severity
            // This would affect the target's approval, etc.
            Debug.Log($"Used blackmail on {targetID}: {item.Description}");
        }
        
        private void EnforceResourceCaps()
        {
            player.Resources["PublicTrust"] = Mathf.Clamp(
                player.Resources["PublicTrust"], 0f, 100f);
            player.Resources["MediaInfluence"] = Mathf.Clamp(
                (int)player.Resources["MediaInfluence"], 0, 100);
            player.Resources["PartyLoyalty"] = Mathf.Clamp(
                player.Resources["PartyLoyalty"], 0f, 100f);
            
            // Political capital varies by tier
            int maxCapital = player.CurrentOffice != null ? 
                player.CurrentOffice.Tier * 30 : 30;
            player.Resources["PoliticalCapital"] = Mathf.Clamp(
                (int)player.Resources["PoliticalCapital"], 0, maxCapital);
        }
    }
    
    /// <summary>
    /// Blackmail item
    /// </summary>
    [Serializable]
    public class BlackmailItem
    {
        public string ID;
        public string TargetID;
        public string Description;     // "Tax evasion evidence"
        public int Severity;           // 1-10
        public ElectionEmpire.Scandal.ScandalCategory Category;
        public DateTime AcquiredDate;
        public float ExpirationChance; // 10% per turn
        
        public bool HasExpired()
        {
            return UnityEngine.Random.value < ExpirationChance;
        }
    }
}

