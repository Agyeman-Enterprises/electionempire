using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.World;
using ElectionEmpire.Gameplay;
using ElectionEmpire.Character;

namespace ElectionEmpire.Scandal
{
    /// <summary>
    /// Handles player responses to scandals
    /// </summary>
    public class ScandalResponseEngine
    {
        private ScandalTemplateLibrary templateLibrary;
        private GameState gameState;
        private PlayerState player;
        private ResourceManager resourceManager;
        
        public void Initialize(PlayerState player, GameState gameState, ResourceManager resourceManager)
        {
            this.player = player;
            this.gameState = gameState;
            this.resourceManager = resourceManager;
            templateLibrary = new ScandalTemplateLibrary();
            templateLibrary.LoadTemplates();
        }
        
        /// <summary>
        /// Get available response options for a scandal
        /// </summary>
        public List<ResponseOption> GetAvailableResponses(Scandal scandal)
        {
            var options = new List<ResponseOption>();
            var template = templateLibrary.GetTemplate(scandal.TemplateID);
            
            if (template == null) return options;
            
            // DENY
            options.Add(new ResponseOption
            {
                Type = ResponseType.Deny,
                Name = "Deny Allegations",
                Description = "Flatly deny the scandal. High risk, high reward.",
                SuccessChance = CalculateResponseSuccess(scandal, ResponseType.Deny, template),
                Cost = new Dictionary<string, float>
                {
                    { "MediaInfluence", 10f },
                    { "PoliticalCapital", 5f }
                },
                Requirements = new List<string>(),
                PotentialImpact = "If successful: Major trust recovery. If failed: Massive credibility loss."
            });
            
            // APOLOGIZE
            options.Add(new ResponseOption
            {
                Type = ResponseType.Apologize,
                Name = "Public Apology",
                Description = "Admit wrongdoing and apologize. Measured impact.",
                SuccessChance = CalculateResponseSuccess(scandal, ResponseType.Apologize, template),
                Cost = new Dictionary<string, float>
                {
                    { "PublicTrust", 5f },
                    { "MediaInfluence", 5f }
                },
                Requirements = new List<string>(),
                PotentialImpact = "Moderate trust loss, but stops scandal from growing."
            });
            
            // COUNTER-ATTACK
            options.Add(new ResponseOption
            {
                Type = ResponseType.CounterAttack,
                Name = "Counter-Attack",
                Description = "Go on offensive against accusers. Transfer damage.",
                SuccessChance = CalculateResponseSuccess(scandal, ResponseType.CounterAttack, template),
                Cost = new Dictionary<string, float>
                {
                    { "PoliticalCapital", 10f },
                    { "CampaignFunds", 50000f }
                },
                Requirements = new List<string> { "Aggression > 40" },
                PotentialImpact = "Can deflect scandal but makes permanent enemies."
            });
            
            // DISTRACT
            options.Add(new ResponseOption
            {
                Type = ResponseType.Distract,
                Name = "Create Distraction",
                Description = "Announce major policy or create other news. Temporary relief.",
                SuccessChance = CalculateResponseSuccess(scandal, ResponseType.Distract, template),
                Cost = new Dictionary<string, float>
                {
                    { "MediaInfluence", 20f },
                    { "CampaignFunds", 100000f }
                },
                Requirements = new List<string> { "MediaInfluence > 30" },
                PotentialImpact = "Buys time but scandal returns stronger."
            });
            
            // SACRIFICE SUBORDINATE
            options.Add(new ResponseOption
            {
                Type = ResponseType.SacrificeStaff,
                Name = "Blame Staff Member",
                Description = "Scapegoat a subordinate. Contains damage but costs loyalty.",
                SuccessChance = CalculateResponseSuccess(scandal, ResponseType.SacrificeStaff, template),
                Cost = new Dictionary<string, float>
                {
                    { "PartyLoyalty", 10f }
                },
                Requirements = new List<string> { "EthicalFlexibility > 50" },
                PotentialImpact = "Scandal contained but staff may betray you later."
            });
            
            // LEGAL DEFENSE
            options.Add(new ResponseOption
            {
                Type = ResponseType.LegalDefense,
                Name = "Legal Defense",
                Description = "Hire lawyers and fight legally. Expensive but effective.",
                SuccessChance = CalculateResponseSuccess(scandal, ResponseType.LegalDefense, template),
                Cost = new Dictionary<string, float>
                {
                    { "CampaignFunds", 250000f },
                    { "PoliticalCapital", 5f }
                },
                Requirements = new List<string> { "CampaignFunds > 250000" },
                PotentialImpact = "Best for legal scandals. Long process but thorough."
            });
            
            // FULL TRANSPARENCY
            options.Add(new ResponseOption
            {
                Type = ResponseType.FullTransparency,
                Name = "Full Transparency",
                Description = "Release all information voluntarily. Shows honesty.",
                SuccessChance = CalculateResponseSuccess(scandal, ResponseType.FullTransparency, template),
                Cost = new Dictionary<string, float>
                {
                    { "PublicTrust", 10f },
                    { "MediaInfluence", 15f }
                },
                Requirements = new List<string>(),
                PotentialImpact = "Short term pain, long term gain. Builds credibility."
            });
            
            // SPIN CAMPAIGN
            options.Add(new ResponseOption
            {
                Type = ResponseType.SpinCampaign,
                Name = "Spin Campaign",
                Description = "Reframe the narrative. Change the story.",
                SuccessChance = CalculateResponseSuccess(scandal, ResponseType.SpinCampaign, template),
                Cost = new Dictionary<string, float>
                {
                    { "CampaignFunds", 150000f },
                    { "MediaInfluence", 25f }
                },
                Requirements = new List<string> { "MediaInfluence > 40" },
                PotentialImpact = "Can completely change public perception if successful."
            });
            
            // Filter by requirements
            return options.Where(o => MeetsRequirements(o.Requirements)).ToList();
        }
        
        private float CalculateResponseSuccess(Scandal scandal, ResponseType responseType, ScandalTemplate template)
        {
            // Base effectiveness from template
            float baseEffectiveness = template.ResponseEffectiveness.GetValueOrDefault(responseType, 0.5f);
            
            // Modify by scandal stage
            float stageModifier = scandal.CurrentStage switch
            {
                ScandalStage.Emergence => 1.2f,
                ScandalStage.Development => 1.0f,
                ScandalStage.Crisis => 0.7f,
                ScandalStage.Resolution => 0.9f,
                _ => 1.0f
            };
            
            // Modify by evidence strength
            float evidenceModifier = 1.0f - (scandal.EvidenceStrength / 200f);
            
            // Modify by character traits
            float traitModifier = 1.0f;
            if (player.Character != null)
            {
                switch (responseType)
                {
                    case ResponseType.Deny:
                        if (player.Character.PositiveQuirks != null && 
                            player.Character.PositiveQuirks.Any(q => q.Name == "Silver Tongue"))
                            traitModifier += 0.3f;
                        if (player.Character.NegativeQuirks != null && 
                            player.Character.NegativeQuirks.Any(q => q.Name == "Pathological Liar"))
                            traitModifier -= 0.2f;
                        break;
                }
            }
            
            // Modify by media influence
            float mediaModifier = 1.0f + (player.Resources.GetValueOrDefault("MediaInfluence", 0f) / 200f);
            
            // Modify by previous response history
            float historyModifier = 1.0f;
            if (scandal.ResponseHistory != null)
            {
                int sameResponseCount = scandal.ResponseHistory.Count(r => r.Type == responseType);
                historyModifier -= sameResponseCount * 0.15f;
            }
            
            // Combine all modifiers
            float finalChance = baseEffectiveness * stageModifier * evidenceModifier * 
                               traitModifier * mediaModifier * historyModifier;
            
            return Mathf.Clamp01(finalChance);
        }
        
        private bool MeetsRequirements(List<string> requirements)
        {
            foreach (var req in requirements)
            {
                if (req.Contains(">"))
                {
                    var parts = req.Split('>');
                    if (parts.Length < 2) continue;
                    string attribute = parts[0].Trim();
                    if (!float.TryParse(parts[1].Trim(), out float threshold))
                        continue;
                    
                    if (!CheckAttributeThreshold(attribute, threshold))
                        return false;
                }
                else if (req.Contains("="))
                {
                    var parts = req.Split('=');
                    if (parts.Length < 2) continue;
                    string attribute = parts[0].Trim();
                    string value = parts[1].Trim();
                    
                    if (!CheckAttributeEquals(attribute, value))
                        return false;
                }
            }
            
            return true;
        }
        
        private bool CheckAttributeThreshold(string attribute, float threshold)
        {
            switch (attribute)
            {
                case "Aggression":
                    return player.Personality?.Aggression > threshold;
                case "MediaInfluence":
                    return player.Resources.GetValueOrDefault("MediaInfluence", 0f) > threshold;
                case "CampaignFunds":
                    return player.Resources.GetValueOrDefault("CampaignFunds", 0f) > threshold;
                case "EthicalFlexibility":
                    return player.Personality?.EthicalFlexibility > threshold;
            }
            return true;
        }
        
        private bool CheckAttributeEquals(string attribute, string value)
        {
            if (attribute == "Alignment")
            {
                return player.Alignment == value;
            }
            return true;
        }
        
        /// <summary>
        /// Execute a response
        /// </summary>
        public ScandalResponseResult ExecuteResponse(Scandal scandal, ResponseType responseType)
        {
            var template = templateLibrary.GetTemplate(scandal.TemplateID);
            var options = GetAvailableResponses(scandal);
            var selectedOption = options.Find(o => o.Type == responseType);
            
            if (selectedOption == null)
            {
                return new ScandalResponseResult
                {
                    Success = false,
                    Message = "This response is not available."
                };
            }
            
            // Pay costs
            foreach (var cost in selectedOption.Cost)
            {
                if (cost.Key == "CampaignFunds")
                {
                    if (!resourceManager.SpendFunds(cost.Value, $"Scandal Response: {responseType}"))
                    {
                        return new ScandalResponseResult
                        {
                            Success = false,
                            Message = "Insufficient funds for this response."
                        };
                    }
                }
                else if (player.Resources.ContainsKey(cost.Key))
                {
                    player.Resources[cost.Key] -= cost.Value;
                }
            }
            
            // Determine success
            bool successful = UnityEngine.Random.value < selectedOption.SuccessChance;
            
            // Create response record
            var response = new ScandalResponse
            {
                ID = System.Guid.NewGuid().ToString(),
                Type = responseType,
                ResponseDate = gameState != null ? gameState.CurrentGameTime : System.DateTime.Now,
                Statement = GenerateResponseStatement(scandal, responseType, player.Character.Name),
                Successful = successful,
                Costs = selectedOption.Cost
            };
            
            // Calculate impact
            float impactReduction = 0f;
            
            if (successful)
            {
                impactReduction = CalculateImpactReduction(scandal, responseType, template);
                ApplySuccessfulResponseEffects(scandal, responseType, impactReduction);
            }
            else
            {
                ApplyFailedResponseEffects(scandal, responseType);
            }
            
            response.ImpactReduction = impactReduction;
            if (scandal.ResponseHistory == null)
                scandal.ResponseHistory = new List<ScandalResponse>();
            scandal.ResponseHistory.Add(response);
            
            // Generate media reaction
            string mediaReaction = GenerateMediaReaction(scandal, response);
            if (scandal.Headlines == null)
                scandal.Headlines = new List<string>();
            scandal.Headlines.Add(mediaReaction);
            
            return new ScandalResponseResult
            {
                Success = successful,
                Response = response,
                Message = successful ? 
                    $"Response successful! Scandal impact reduced by {impactReduction:F0}%." :
                    $"Response failed! Scandal intensifies.",
                MediaReaction = mediaReaction
            };
        }
        
        private float CalculateImpactReduction(Scandal scandal, ResponseType responseType, ScandalTemplate template)
        {
            float baseReduction = template.ResponseEffectiveness.GetValueOrDefault(responseType, 0.5f) * 50f;
            
            // Better reduction if responded early
            if (scandal.CurrentStage == ScandalStage.Emergence)
                baseReduction *= 1.5f;
            
            // Modify by character traits
            if (responseType == ResponseType.Apologize && 
                player.Character.PositiveQuirks != null &&
                player.Character.PositiveQuirks.Any(q => q.Name == "Humble"))
                baseReduction *= 1.3f;
            
            return baseReduction;
        }
        
        private void ApplySuccessfulResponseEffects(Scandal scandal, ResponseType responseType, float impactReduction)
        {
            // Reduce scandal severity and coverage
            scandal.MediaCoverage -= impactReduction;
            scandal.PublicInterest -= impactReduction * 0.8f;
            scandal.EvidenceStrength -= impactReduction * 0.5f;
            
            // Clamp values
            scandal.MediaCoverage = Mathf.Max(0f, scandal.MediaCoverage);
            scandal.PublicInterest = Mathf.Max(0f, scandal.PublicInterest);
            scandal.EvidenceStrength = Mathf.Max(0f, scandal.EvidenceStrength);
            
            // Special effects
            switch (responseType)
            {
                case ResponseType.Deny:
                    resourceManager.GainTrust(10f, "Successfully denied scandal");
                    break;
                
                case ResponseType.FullTransparency:
                    // Build credibility for future
                    if (!player.Resources.ContainsKey("CredibilityBonus"))
                        player.Resources["CredibilityBonus"] = 0f;
                    player.Resources["CredibilityBonus"] += 5f;
                    break;
            }
        }
        
        private void ApplyFailedResponseEffects(Scandal scandal, ResponseType responseType)
        {
            // Failed response makes scandal worse
            scandal.MediaCoverage += 20f;
            scandal.PublicInterest += 15f;
            scandal.CurrentSeverity = Mathf.Min(10, scandal.CurrentSeverity + 1);
            
            // Specific failures
            switch (responseType)
            {
                case ResponseType.Deny:
                    resourceManager.LoseTrust(15f, "Failed denial damages credibility");
                    scandal.MediaCoverage += 20f;
                    break;
                
                case ResponseType.CounterAttack:
                    resourceManager.LoseTrust(10f, "Counter-attack backfires");
                    scandal.EvidenceStrength += 20f;
                    break;
                
                case ResponseType.LegalDefense:
                    scandal.TurnsInStage += 5;
                    break;
            }
        }
        
        private string GenerateResponseStatement(Scandal scandal, ResponseType responseType, string playerName)
        {
            switch (responseType)
            {
                case ResponseType.Deny:
                    return $"These allegations are completely false and without merit. " +
                           $"I categorically deny any wrongdoing.";
                
                case ResponseType.Apologize:
                    return $"I take full responsibility for this situation and apologize to everyone affected. " +
                           $"I will work to make this right.";
                
                case ResponseType.CounterAttack:
                    return $"These accusations are a coordinated smear campaign by my political opponents. " +
                           $"Let's talk about their record instead.";
                
                case ResponseType.Distract:
                    return $"While some are focused on yesterday's news, I'm focused on tomorrow's solutions. " +
                           $"Today I'm announcing a major new initiative...";
                
                case ResponseType.SacrificeStaff:
                    return $"This was the unauthorized action of a staff member who has been terminated. " +
                           $"I was not aware of these activities.";
                
                case ResponseType.LegalDefense:
                    return $"I have retained counsel and will vigorously defend myself through the legal system. " +
                           $"I'm confident I will be vindicated.";
                
                case ResponseType.FullTransparency:
                    return $"I'm releasing all relevant documents and communications. " +
                           $"The public deserves complete transparency.";
                
                case ResponseType.SpinCampaign:
                    return $"What some call a scandal, I call a learning opportunity. " +
                           $"This experience has shown me...";
                
                default:
                    return $"No comment at this time.";
            }
        }
        
        private string GenerateMediaReaction(Scandal scandal, ScandalResponse response)
        {
            string playerName = player.Character.Name;
            
            if (response.Successful)
            {
                return response.Type switch
                {
                    ResponseType.Deny => $"{playerName}'s Forceful Denial Gains Traction",
                    ResponseType.Apologize => $"{playerName} Apology Well-Received",
                    ResponseType.CounterAttack => $"{playerName} Turns Tables on Accusers",
                    ResponseType.FullTransparency => $"{playerName}'s Transparency Praised",
                    _ => $"{playerName} Response Seems to Work"
                };
            }
            else
            {
                return response.Type switch
                {
                    ResponseType.Deny => $"{playerName} Denial Rings Hollow",
                    ResponseType.Apologize => $"Too Little, Too Late? {playerName} Apology Falls Flat",
                    ResponseType.CounterAttack => $"{playerName} Attack Backfires Spectacularly",
                    ResponseType.LegalDefense => $"Legal Maneuvering Won't Save {playerName}",
                    _ => $"{playerName} Response Fails to Convince"
                };
            }
        }
    }
    
    public class ResponseOption
    {
        public ResponseType Type;
        public string Name;
        public string Description;
        public float SuccessChance;
        public Dictionary<string, float> Cost;
        public List<string> Requirements;
        public string PotentialImpact;
    }
    
    public class ScandalResponseResult
    {
        public bool Success;
        public ScandalResponse Response;
        public string Message;
        public string MediaReaction;
    }
}

