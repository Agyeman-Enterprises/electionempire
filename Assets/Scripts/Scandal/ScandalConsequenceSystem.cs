using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.World;
using ElectionEmpire.Gameplay;
using ElectionEmpire.Staff;
using ElectionEmpire.Core;
using GameState = ElectionEmpire.Gameplay.GameState;

namespace ElectionEmpire.Scandal
{
    /// <summary>
    /// Applies ongoing consequences from scandals
    /// </summary>
    public class ScandalConsequenceSystem
    {
        private GameState gameState;
        private PlayerState player;
        private ResourceManager resourceManager;
        
        public void Initialize(PlayerState player, GameState gameState, ResourceManager resourceManager)
        {
            this.player = player;
            this.gameState = gameState;
            this.resourceManager = resourceManager;
        }
        
        /// <summary>
        /// Apply ongoing scandal impacts each turn
        /// </summary>
        public void ApplyScandalConsequences(Scandal scandal, float deltaTime)
        {
            var templateLibrary = new ScandalTemplateLibrary();
            templateLibrary.LoadTemplates();
            var template = templateLibrary.GetTemplate(scandal.TemplateID);
            
            if (template == null) return;
            
            // 1. Calculate resource impacts
            CalculateResourceImpacts(scandal, template, deltaTime);
            
            // 2. Calculate voter bloc impacts
            CalculateVoterBlocImpacts(scandal, template, deltaTime);
            
            // 3. Calculate relationship impacts
            CalculateRelationshipImpacts(scandal, deltaTime);
            
            // 4. Apply long-term effects
            ApplyLongTermEffects(scandal, deltaTime);
            
            // 5. Check for strategic limitations
            ApplyStrategicLimitations(scandal);
        }
        
        private void CalculateResourceImpacts(Scandal scandal, ScandalTemplate template, float deltaTime)
        {
            // Trust impact
            float trustImpact = template.ImpactFormula.TrustImpactBase + 
                               (scandal.CurrentSeverity * template.ImpactFormula.TrustImpactPerSeverity);
            
            // Scale by media coverage
            trustImpact *= (scandal.MediaCoverage / 100f);
            
            // Scale by scandal stage
            trustImpact *= scandal.CurrentStage switch
            {
                ScandalStage.Emergence => 0.3f,
                ScandalStage.Development => 0.7f,
                ScandalStage.Crisis => 1.5f,
                ScandalStage.Resolution => 0.5f,
                _ => 1.0f
            };
            
            // Apply any response reductions
            float totalReduction = 0f;
            if (scandal.ResponseHistory != null)
            {
                totalReduction = scandal.ResponseHistory.Sum(r => r.ImpactReduction) / 100f;
            }
            trustImpact *= (1f - totalReduction);
            
            // Apply impact (per day)
            float dailyImpact = trustImpact * (deltaTime / 86400f);
            resourceManager.LoseTrust(dailyImpact, $"Scandal: {scandal.Title}");
            
            if (scandal.ResourceImpacts == null)
                scandal.ResourceImpacts = new Dictionary<string, float>();
            scandal.ResourceImpacts["PublicTrust"] = dailyImpact;
            
            // Political capital impact
            float capitalImpact = template.ImpactFormula.CapitalImpactBase + 
                                 (scandal.CurrentSeverity * template.ImpactFormula.CapitalImpactPerSeverity);
            capitalImpact *= (scandal.MediaCoverage / 100f);
            capitalImpact *= (1f - totalReduction);
            capitalImpact *= (deltaTime / 86400f);
            
            player.Resources["PoliticalCapital"] -= (int)capitalImpact;
            scandal.ResourceImpacts["PoliticalCapital"] = capitalImpact;
            
            // Media influence impact
            float mediaImpact = scandal.CurrentSeverity * 2f;
            mediaImpact *= (scandal.PublicInterest / 100f);
            mediaImpact *= (deltaTime / 86400f);
            
            player.Resources["MediaInfluence"] -= (int)mediaImpact;
            scandal.ResourceImpacts["MediaInfluence"] = mediaImpact;
            
            // Campaign funds impact (legal fees, crisis management)
            if (scandal.Category == ScandalCategory.Financial || 
                (scandal.ResponseHistory != null && scandal.ResponseHistory.Any(r => r.Type == ResponseType.LegalDefense)))
            {
                float fundImpact = scandal.CurrentSeverity * 10000f * (deltaTime / 86400f);
                player.Resources["CampaignFunds"] -= fundImpact;
                scandal.ResourceImpacts["CampaignFunds"] = fundImpact;
            }
            
            // Party loyalty impact
            if (scandal.Category == ScandalCategory.Electoral || 
                scandal.MediaCoverage > 70f)
            {
                float partyImpact = scandal.CurrentSeverity * 1.5f * (deltaTime / 86400f);
                player.Resources["PartyLoyalty"] -= partyImpact;
                scandal.ResourceImpacts["PartyLoyalty"] = partyImpact;
            }
        }
        
        private void CalculateVoterBlocImpacts(Scandal scandal, ScandalTemplate template, float deltaTime)
        {
            if (scandal.BlocImpacts == null)
                scandal.BlocImpacts = new Dictionary<VoterBloc, float>();
            
            foreach (VoterBloc bloc in System.Enum.GetValues(typeof(VoterBloc)))
            {
                float sensitivity = template.ImpactFormula.BlocSensitivity.GetValueOrDefault(bloc, 1.0f);
                
                // Base impact
                float impact = scandal.CurrentSeverity * sensitivity * 2f;
                
                // Scale by media coverage
                impact *= (scandal.MediaCoverage / 100f);
                
                // Scale by scandal stage
                impact *= scandal.CurrentStage switch
                {
                    ScandalStage.Emergence => 0.5f,
                    ScandalStage.Development => 0.8f,
                    ScandalStage.Crisis => 1.2f,
                    ScandalStage.Resolution => 0.6f,
                    _ => 1.0f
                };
                
                // Per day
                impact *= (deltaTime / 86400f);
                
                // Apply to player's voter bloc support
                if (!player.VoterBlocSupport.ContainsKey(bloc))
                    player.VoterBlocSupport[bloc] = 50f;
                
                player.VoterBlocSupport[bloc] -= impact;
                player.VoterBlocSupport[bloc] = Mathf.Clamp(player.VoterBlocSupport[bloc], 0f, 100f);
                
                scandal.BlocImpacts[bloc] = impact;
            }
        }
        
        private void CalculateRelationshipImpacts(Scandal scandal, float deltaTime)
        {
            // Impact on allies
            if (player.PoliticalAllies != null)
            {
                foreach (var allyID in player.PoliticalAllies.ToList())
                {
                    // Allies may distance themselves from scandal
                    float distanceChance = (scandal.CurrentSeverity / 10f) * (scandal.MediaCoverage / 100f) * 0.1f; // 10% max per day
                    
                    if (UnityEngine.Random.value < distanceChance)
                    {
                        // Ally distances themselves
                        player.PoliticalAllies.Remove(allyID);
                        
                        Debug.Log($"Ally {allyID} distances themselves due to scandal: {scandal.Title}");
                    }
                }
            }
            
            // Impact on staff loyalty (simplified - would need StaffMember class)
            if (player.Staff != null)
            {
                float loyaltyImpact = scandal.CurrentSeverity * 1.5f * (deltaTime / 86400f);
                
                foreach (var staff in player.Staff.ToList())
                {
                    if (staff.Loyalty != null)
                        staff.Loyalty -= (int)loyaltyImpact;

                    // Staff may resign if loyalty too low
                    if (staff.Loyalty != null && staff.Loyalty < 20f && UnityEngine.Random.value < 0.3f)
                    {
                        player.Staff.Remove(staff);
                        Debug.Log($"Staff member {staff.Name} resigns due to scandal: {scandal.Title}");
                    }
                }
            }
        }
        
        private void ApplyLongTermEffects(Scandal scandal, float deltaTime)
        {
            // Create reputation tags that persist
            if (scandal.CurrentSeverity >= 7 && !scandal.IsResolved)
            {
                string tag = GenerateReputationTag(scandal);
                
                if (player.ReputationTags == null)
                    player.ReputationTags = new List<string>();
                
                if (!player.ReputationTags.Contains(tag))
                {
                    player.ReputationTags.Add(tag);
                }
            }
            
            // Increase vulnerability to similar future scandals
            string vulnerabilityKey = $"{scandal.Category}_Vulnerability";
            if (player.ScandalVulnerabilities == null)
                player.ScandalVulnerabilities = new Dictionary<string, float>();
            
            if (!player.ScandalVulnerabilities.ContainsKey(vulnerabilityKey))
                player.ScandalVulnerabilities[vulnerabilityKey] = 0f;
            
            player.ScandalVulnerabilities[vulnerabilityKey] += scandal.CurrentSeverity * 0.05f;
            
            // Media scrutiny increase
            if (scandal.CurrentSeverity >= 5)
            {
                player.MediaScrutinyLevel += scandal.CurrentSeverity * 0.1f;
                player.MediaScrutinyLevel = Mathf.Min(100f, player.MediaScrutinyLevel);
            }
        }
        
        private void ApplyStrategicLimitations(Scandal scandal)
        {
            // Certain policy areas become toxic
            if (scandal.Category == ScandalCategory.Policy)
            {
                string policyArea = ExtractPolicyArea(scandal);
                
                if (!string.IsNullOrEmpty(policyArea))
                {
                    if (player.ToxicPolicyAreas == null)
                        player.ToxicPolicyAreas = new List<string>();
                    
                    if (!player.ToxicPolicyAreas.Contains(policyArea))
                        player.ToxicPolicyAreas.Add(policyArea);
                }
            }
            
            // Demographic penalties
            if (scandal.Category == ScandalCategory.Personal)
            {
                if (scandal.BlocImpacts != null)
                {
                    if (player.DemographicPenalties == null)
                        player.DemographicPenalties = new Dictionary<VoterBloc, float>();
                    
                    foreach (var bloc in scandal.BlocImpacts.Where(kvp => kvp.Value > 10f))
                    {
                        player.DemographicPenalties[bloc.Key] = 
                            player.DemographicPenalties.GetValueOrDefault(bloc.Key, 0f) + 5f;
                    }
                }
            }
            
            // Endorsement lockout
            if (scandal.CurrentSeverity >= 6)
            {
                player.EndorsementLockoutTurns = Mathf.Max(player.EndorsementLockoutTurns, 
                    scandal.CurrentSeverity * 5);
            }
        }
        
        private string GenerateReputationTag(Scandal scandal)
        {
            return scandal.Category switch
            {
                ScandalCategory.Financial => "Financially Questionable",
                ScandalCategory.Personal => "Scandal-Plagued",
                ScandalCategory.Policy => "Policy Failures",
                ScandalCategory.Administrative => "Poor Manager",
                ScandalCategory.Electoral => "Ethically Compromised",
                _ => "Controversial"
            };
        }
        
        private string ExtractPolicyArea(Scandal scandal)
        {
            // Simple keyword extraction from scandal description
            if (scandal.Description != null)
            {
                if (scandal.Description.Contains("healthcare") || scandal.Description.Contains("health"))
                    return "Healthcare";
                if (scandal.Description.Contains("tax") || scandal.Description.Contains("economy"))
                    return "Economy";
                if (scandal.Description.Contains("education") || scandal.Description.Contains("school"))
                    return "Education";
            }
            
            return null;
        }
    }
}

