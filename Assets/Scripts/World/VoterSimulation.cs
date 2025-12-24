using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.Character;

namespace ElectionEmpire.World
{
    /// <summary>
    /// Simulates voter behavior and calculates polling
    /// </summary>
    public class VoterSimulation
    {
        private World world;
        
        public VoterSimulation(World world)
        {
            this.world = world;
        }
        
        /// <summary>
        /// Calculate polling for a player in a district
        /// </summary>
        public float CalculateDistrictPolling(PlayerState player, District district)
        {
            float baseSupport = 50f; // Start neutral
            
            // 1. Policy alignment
            baseSupport += CalculatePolicyAlignment(player, district);
            
            // 2. Demographic appeal
            baseSupport += CalculateDemographicAppeal(player, district);
            
            // 3. Scandal impact
            baseSupport -= CalculateScandalDamage(player, district);
            
            // 4. Media coverage effect
            baseSupport += CalculateMediaEffect(player, district);
            
            // 5. Campaign spending
            baseSupport += CalculateCampaignEffect(player, district);
            
            // 6. Character background resonance
            baseSupport += CalculateBackgroundResonance(player, district);
            
            // 7. Voter bloc specific modifiers
            baseSupport += CalculateVoterBlocEffects(player, district);
            
            // 8. Add volatility (randomness)
            float volatility = district.Type == DistrictType.Urban ? 5f : 10f;
            baseSupport += UnityEngine.Random.Range(-volatility, volatility);
            
            return Mathf.Clamp(baseSupport, 0f, 100f);
        }
        
        private float CalculatePolicyAlignment(PlayerState player, District district)
        {
            float alignment = 0f;
            
            // Check if player's policies match district's priority issues
            foreach (var issue in district.PriorityIssues)
            {
                if (player.PolicyStances.ContainsKey(issue))
                {
                    // Positive if player's stance matches district lean
                    float districtPreference = GetDistrictIssuePreference(district, issue);
                    float playerStance = player.PolicyStances[issue];
                    
                    // Similarity bonus (closer = better)
                    float similarity = 1f - Mathf.Abs(districtPreference - playerStance) / 100f;
                    alignment += similarity * 5f; // Up to +5 per issue
                }
            }
            
            return alignment;
        }
        
        private float CalculateDemographicAppeal(PlayerState player, District district)
        {
            float appeal = 0f;
            
            // Match player's character to district demographics
            var character = player.Character;
            var background = character.Background;
            
            if (background == null) return appeal;
            
            // Age appeal (simplified - can be expanded with background properties)
            // For now, use background tier as proxy
            if (background.Tier == "absurd" && district.Demographics.Youth18to29 > 30f)
                appeal += 5f;
            
            if (background.Tier == "respectable" && district.Demographics.Seniors65Plus > 25f)
                appeal += 5f;
            
            // Income appeal
            if (background.Tier == "respectable" && district.Demographics.LowIncome > 40f)
                appeal += 10f;
            
            if (background.Tier == "celebrity" && district.Demographics.HighIncome > 25f)
                appeal += 10f;
            
            // Education appeal
            if (background.Tier == "respectable" && district.Demographics.CollegeEducated > 50f)
                appeal += 5f;
            
            return appeal;
        }
        
        private float CalculateScandalDamage(PlayerState player, District district)
        {
            float damage = 0f;
            
            foreach (var scandal in player.ActiveScandals)
            {
                // Different districts care about different scandals
                float districtSensitivity = GetDistrictScandalSensitivity(district, scandal.Category);
                damage += scandal.Severity * districtSensitivity;
            }
            
            return damage;
        }
        
        private float CalculateMediaEffect(PlayerState player, District district)
        {
            // Media influence affects urban districts more
            float mediaEffect = player.MediaInfluence;
            
            if (district.Type == DistrictType.Urban)
                return mediaEffect * 0.2f; // Up to +20% in urban
            else if (district.Type == DistrictType.Suburban)
                return mediaEffect * 0.1f; // Up to +10% in suburban
            else
                return mediaEffect * 0.05f; // Up to +5% in rural
        }
        
        private float CalculateCampaignEffect(PlayerState player, District district)
        {
            if (!player.DistrictCampaignSpending.ContainsKey(district.ID))
                return 0f;
            
            float spending = player.DistrictCampaignSpending[district.ID];
            
            // Diminishing returns on spending
            float effect = Mathf.Sqrt(spending / 1000f) * 2f; // $1000 = +2%, $10k = +6.3%
            
            return Mathf.Clamp(effect, 0f, 15f); // Cap at +15%
        }
        
        private float CalculateBackgroundResonance(PlayerState player, District district)
        {
            float resonance = 0f;
            
            var background = player.Character.Background;
            if (background == null) return resonance;
            
            // Working class backgrounds resonate in working class districts
            if (background.Tier == "respectable" && 
                district.BlocStrength.ContainsKey(VoterBloc.WorkingClass) &&
                district.BlocStrength[VoterBloc.WorkingClass] > 40f)
                resonance += 5f;
            
            // Absurd backgrounds resonate in urban youth districts
            if (background.Tier == "absurd" && 
                district.Type == DistrictType.Urban && 
                district.Demographics.Youth18to29 > 30f)
                resonance += 10f; // They love the chaos
            
            // Criminal backgrounds hurt in law&order districts
            if (background.Tier == "criminal" && 
                district.BlocStrength.ContainsKey(VoterBloc.SecurityPersonnel) &&
                district.BlocStrength[VoterBloc.SecurityPersonnel] > 20f)
                resonance -= 15f;
            
            return resonance;
        }
        
        private float CalculateVoterBlocEffects(PlayerState player, District district)
        {
            float effect = 0f;
            
            foreach (var bloc in district.BlocStrength)
            {
                // Player's standing with each bloc × bloc strength in district
                if (player.VoterBlocSupport.ContainsKey(bloc.Key))
                {
                    float playerBlocSupport = player.VoterBlocSupport[bloc.Key];
                    float blocStrength = bloc.Value / 100f; // Normalize
                    
                    effect += (playerBlocSupport - 50f) * blocStrength * 0.1f;
                }
            }
            
            return effect;
        }
        
        /// <summary>
        /// Calculate overall approval rating (national average)
        /// </summary>
        public float CalculateNationalApproval(PlayerState player)
        {
            float totalSupport = 0f;
            int totalVoters = 0;
            
            foreach (var region in world.Nation.Regions)
            {
                foreach (var state in region.States)
                {
                    foreach (var district in state.Districts)
                    {
                        float districtPolling = CalculateDistrictPolling(player, district);
                        totalSupport += districtPolling * district.Population;
                        totalVoters += district.Population;
                    }
                }
            }
            
            if (totalVoters == 0) return 50f;
            
            return totalSupport / totalVoters;
        }
        
        /// <summary>
        /// Determine election winner in a district
        /// </summary>
        public string DetermineDistrictWinner(List<PlayerState> candidates, District district)
        {
            float highestPolling = 0f;
            string winner = null;
            
            foreach (var candidate in candidates)
            {
                float polling = CalculateDistrictPolling(candidate, district);
                
                if (polling > highestPolling)
                {
                    highestPolling = polling;
                    winner = candidate.Character.Name;
                }
            }
            
            return winner;
        }
        
        // Helper methods
        private float GetDistrictIssuePreference(District district, Issue issue)
        {
            // Return district's position on this issue (0-100 scale)
            // Based on demographics and political lean
            
            float preference = 50f + district.PoliticalLean; // Base on political lean
            
            // Adjust for specific issues
            switch (issue)
            {
                case Issue.Taxes:
                    preference -= district.Demographics.HighIncome * 0.3f; // Rich want lower
                    break;
                case Issue.Healthcare:
                    preference += district.Demographics.Seniors65Plus * 0.5f; // Seniors prioritize
                    break;
                case Issue.Education:
                    preference += district.Demographics.Youth18to29 * 0.4f;
                    break;
                case Issue.Agriculture:
                    if (district.Type == DistrictType.Rural)
                        preference += 20f;
                    break;
                case Issue.Housing:
                    if (district.Type == DistrictType.Urban)
                        preference += 15f;
                    break;
                // Add more issue-specific logic
            }
            
            return Mathf.Clamp(preference, 0f, 100f);
        }
        
        private float GetDistrictScandalSensitivity(District district, ScandalCategory category)
        {
            // How much does this district care about this scandal type?
            
            switch (category)
            {
                case ScandalCategory.Financial:
                    return district.Demographics.HighIncome > 40f ? 0.5f : 1.0f; // Rich care less
                
                case ScandalCategory.Personal:
                    return district.BlocStrength.ContainsKey(VoterBloc.Religious) &&
                           district.BlocStrength[VoterBloc.Religious] > 30f ? 1.5f : 0.7f;
                
                case ScandalCategory.Policy:
                    return district.Demographics.CollegeEducated > 50f ? 1.2f : 0.8f;
                
                default:
                    return 1.0f;
            }
        }
    }
}

