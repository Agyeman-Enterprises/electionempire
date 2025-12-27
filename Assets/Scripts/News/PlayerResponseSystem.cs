using System;
using ElectionEmpire.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.World;
using ElectionEmpire.Gameplay;
using ElectionEmpire.News.Translation;
using ElectionEmpire.News.Templates;
using ElectionEmpire.News;

namespace ElectionEmpire.News
{
    /// <summary>
    /// Handles player responses to news events
    /// </summary>
    public class PlayerResponseSystem
    {
        private PlayerState player;
        private ResourceManager resourceManager;
        private List<StanceRecord> stanceHistory;
        
        public void Initialize(PlayerState player, ResourceManager resourceManager)
        {
            this.player = player;
            this.resourceManager = resourceManager;
            stanceHistory = new List<StanceRecord>();
        }
        
        /// <summary>
        /// Player responds to a news event (new format)
        /// </summary>
        public ResponseResult RespondToEvent(NewsGameEvent newsEvent, string optionId)
        {
            var response = newsEvent.ResponseOptions?.Find(r => r.OptionId == optionId);
            
            if (response == null)
            {
                return new ResponseResult
                {
                    Success = false,
                    Message = "Invalid response option."
                };
            }
            
            // Check alignment requirements
            if (response.RequiredAlignment != null)
            {
                var alignment = GetPlayerAlignment();
                if (!response.RequiredAlignment.IsInRange(alignment.LawChaos, alignment.GoodEvil))
                {
                    return new ResponseResult
                    {
                        Success = false,
                        Message = "Your alignment doesn't allow this response."
                    };
                }
            }
            
            // Pay resource requirements
            if (response.RequiredResources != null)
            {
                foreach (var req in response.RequiredResources)
                {
                    if (!PayCost(req.ResourceType, req.MinAmount))
                    {
                        return new ResponseResult
                        {
                            Success = false,
                            Message = $"Insufficient {req.ResourceType} for this response."
                        };
                    }
                }
            }
            
            // Calculate success
            bool success = UnityEngine.Random.value < response.SuccessProbability;
            
            // Calculate effects based on success/failure
            var effects = success ? 
                CalculateSuccessEffects(newsEvent, response) : 
                CalculateFailureEffects(newsEvent, response);
            
            // Apply effects
            ApplyEffects(effects);
            
            // Apply alignment shift
            if (response.AlignmentEffect != null)
            {
                ApplyAlignmentShift(response.AlignmentEffect);
            }
            
            // Record response
            newsEvent.PlayerResponded = true;
            newsEvent.ResponseHistory.Add(new PlayerResponse
            {
                OptionId = optionId,
                Turn = GetCurrentTurn(),
                Success = success,
                GeneratedStatement = response.StatementTemplate ?? response.Description
            });
            
            // Record stance if applicable
            if (newsEvent.Category != null)
            {
                RecordStanceFromCategory(newsEvent.Category, response.Label);
            }
            
            return new ResponseResult
            {
                Success = true,
                Message = success ? 
                    (response.SuccessOutcome?.Description ?? "Your response was well-received.") :
                    (response.FailureOutcome?.Description ?? "Your response didn't go as planned."),
                Effects = effects
            };
        }
        
        /// <summary>
        /// Player responds to a news event (legacy format for compatibility)
        /// </summary>
        public ResponseResult RespondToLegacyEvent(NewsGameEvent newsEvent, string responseType)
        {
            // Legacy support - convert to new format if needed
            return new ResponseResult
            {
                Success = false,
                Message = "Please use the new event response system."
            };
        }
        
        /// <summary>
        /// Player takes a stance on a policy challenge (legacy support)
        /// </summary>
        public StanceResult TakeStance(PolicyChallenge challenge, string stance)
        {
            var stanceOption = challenge.StanceOptions?.Find(s => s.Stance == stance);
            
            if (stanceOption == null)
            {
                return new StanceResult
                {
                    Success = false,
                    Message = "Invalid stance."
                };
            }
            
            // Record stance
            RecordStance(challenge.Issue, stance);
            
            // Check for flip-flop
            bool isFlipFlop = CheckFlipFlop(challenge.Issue, stance);
            
            // Calculate effects
            var effects = CalculateStanceEffects(challenge, stanceOption, isFlipFlop);
            
            // Apply effects
            ApplyEffects(effects);
            
            challenge.SelectedStance = stance;
            
            return new StanceResult
            {
                Success = true,
                Message = isFlipFlop ? 
                    "You've changed your position. Some voters notice the flip-flop." :
                    "Stance recorded. Voters will remember this.",
                Effects = effects,
                IsFlipFlop = isFlipFlop
            };
        }
        
        private bool PayCost(string resource, float amount)
        {
            if (resource == "CampaignFunds")
            {
                return resourceManager.SpendFunds(amount, "News Event Response");
            }
            
            if (player.Resources.ContainsKey(resource))
            {
                if (player.Resources[resource] >= amount)
                {
                    player.Resources[resource] -= amount;
                    return true;
                }
            }
            
            return false;
        }
        
        private Dictionary<string, float> CalculateSuccessEffects(NewsGameEvent newsEvent, ResponseOption response)
        {
            var effects = new Dictionary<string, float>();
            
            // Base impact from event effects
            if (newsEvent.Effects != null)
            {
                effects["PublicTrust"] = newsEvent.Effects.TrustDelta;
                effects["PoliticalCapital"] = newsEvent.Effects.CapitalDelta;
                effects["CampaignFunds"] = newsEvent.Effects.FundsDelta;
                effects["MediaInfluence"] = newsEvent.Effects.MediaDelta;
                effects["PartyLoyalty"] = newsEvent.Effects.PartyLoyaltyDelta;
                
                // Voter bloc impacts
                if (newsEvent.Effects.VoterBlocDeltas != null)
                {
                    foreach (var impact in newsEvent.Effects.VoterBlocDeltas)
                    {
                        effects[$"VoterBloc_{impact.Key}"] = impact.Value;
                    }
                }
            }
            
            // Response-specific effects
            if (response.ResourceEffects != null)
            {
                foreach (var effect in response.ResourceEffects)
                {
                    effects[effect.Key] = effect.Value;
                }
            }
            
            // Success outcome modifiers
            if (response.SuccessOutcome != null && response.SuccessOutcome.ResourceModifiers != null)
            {
                foreach (var modifier in response.SuccessOutcome.ResourceModifiers)
                {
                    if (effects.ContainsKey(modifier.Key))
                        effects[modifier.Key] += modifier.Value;
                    else
                        effects[modifier.Key] = modifier.Value;
                }
            }
            
            return effects;
        }
        
        private Dictionary<string, float> CalculateFailureEffects(NewsGameEvent newsEvent, ResponseOption response)
        {
            var effects = new Dictionary<string, float>();
            
            // Base impact (reduced)
            if (newsEvent.Effects != null)
            {
                effects["PublicTrust"] = newsEvent.Effects.TrustDelta * 0.5f;
            }
            
            // Response-specific effects (reduced)
            if (response.ResourceEffects != null)
            {
                foreach (var effect in response.ResourceEffects)
                {
                    effects[effect.Key] = effect.Value * 0.5f;
                }
            }
            
            // Failure outcome modifiers
            if (response.FailureOutcome != null && response.FailureOutcome.ResourceModifiers != null)
            {
                foreach (var modifier in response.FailureOutcome.ResourceModifiers)
                {
                    if (effects.ContainsKey(modifier.Key))
                        effects[modifier.Key] += modifier.Value;
                    else
                        effects[modifier.Key] = modifier.Value;
                }
            }
            
            return effects;
        }
        
        private Dictionary<string, float> CalculateStanceEffects(PolicyChallenge challenge, StanceOption stance, bool isFlipFlop)
        {
            var effects = new Dictionary<string, float>();
            
            // Alignment effects
            float alignmentBonus = (stance.Alignment - 50f) * 0.2f;
            effects["PublicTrust"] = alignmentBonus;
            
            // Issue-specific voter bloc impact
            var bloc = GetVoterBlocForIssue(challenge.Issue);
            effects[$"VoterBloc_{bloc}"] = (stance.Alignment - 50f) * 0.3f;
            
            // Flip-flop penalty
            if (isFlipFlop)
            {
                effects["PublicTrust"] -= 5f;
                if (!player.Resources.ContainsKey("Credibility"))
                    player.Resources["Credibility"] = 50f;
                player.Resources["Credibility"] -= 10f;
            }
            
            // Stance-specific effects
            if (stance.Effects != null)
            {
                foreach (var effect in stance.Effects)
                {
                    effects[effect.Key] = effect.Value;
                }
            }
            
            return effects;
        }
        
        private void ApplyEffects(Dictionary<string, float> effects)
        {
            foreach (var effect in effects)
            {
                if (effect.Key == "PublicTrust")
                {
                    if (effect.Value > 0)
                        resourceManager.GainTrust(effect.Value, "News Event Response");
                    else
                        resourceManager.LoseTrust(-effect.Value, "News Event Response");
                }
                else if (effect.Key.StartsWith("VoterBloc_"))
                {
                    string blocName = effect.Key.Replace("VoterBloc_", "");
                    if (System.Enum.TryParse<VoterBloc>(blocName, out VoterBloc bloc))
                    {
                        if (player.VoterBlocSupport.ContainsKey(bloc))
                        {
                            player.VoterBlocSupport[bloc] += effect.Value;
                            player.VoterBlocSupport[bloc] = Mathf.Clamp(player.VoterBlocSupport[bloc], 0f, 100f);
                        }
                    }
                }
                else if (player.Resources.ContainsKey(effect.Key))
                {
                    player.Resources[effect.Key] += effect.Value;
                }
            }
        }
        
        private void RecordStance(IssueCategory issue, string stance)
        {
            var record = new StanceRecord
            {
                ID = System.Guid.NewGuid().ToString(),
                Issue = issue,
                Stance = stance,
                Date = DateTime.Now
            };
            
            stanceHistory.Add(record);
        }
        
        private bool CheckFlipFlop(IssueCategory issue, string newStance)
        {
            // Check previous stances on this issue
            var previousStances = stanceHistory
                .Where(s => s.Issue == issue)
                .OrderByDescending(s => s.Date)
                .ToList();
            
            if (previousStances.Count > 0)
            {
                string lastStance = previousStances[0].Stance;
                
                // Check if stance changed significantly
                if (IsOppositeStance(lastStance, newStance))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        private bool IsOppositeStance(string stance1, string stance2)
        {
            // Define opposite pairs
            var opposites = new Dictionary<string, string>
            {
                { "Support", "Oppose" },
                { "Oppose", "Support" },
                { "For", "Against" },
                { "Against", "For" }
            };
            
            return opposites.ContainsKey(stance1) && opposites[stance1] == stance2;
        }
        
        private bool CheckCrisisEscalation(CrisisEvent crisis, CrisisResponse response)
        {
            // Denial or poor response can escalate
            if (response.Type == "Deny" || response.Type == "Delegate")
            {
                return UnityEngine.Random.value < 0.6f; // 60% chance
            }
            
            return UnityEngine.Random.value < 0.2f; // 20% chance otherwise
        }
        
        private VoterBloc GetVoterBlocForIssue(IssueCategory issue)
        {
            return issue switch
            {
                IssueCategory.Healthcare => VoterBloc.Seniors,
                IssueCategory.Education => VoterBloc.Educators,
                IssueCategory.Economy => VoterBloc.WorkingClass,
                IssueCategory.Environment => VoterBloc.Activists,
                IssueCategory.Immigration => VoterBloc.Rural,
                _ => VoterBloc.General
            };
        }
        
        private string GenerateResponseMessage(NewsGameEvent newsEvent, ResponseOption response)
        {
            return response.StatementTemplate ?? 
                   $"You chose to {response.Label.ToLower()}. " +
                   $"The public's reaction will depend on how this develops.";
        }
        
        private Translation.PlayerAlignment GetPlayerAlignment()
        {
            // Calculate from character traits
            int lawChaos = 0;
            int goodEvil = 0;
            
            if (player?.Character != null)
            {
                // Simplified calculation
                if (player.Character.NegativeQuirks != null)
                {
                    foreach (var quirk in player.Character.NegativeQuirks)
                    {
                        if (quirk.Name.Contains("Chaos") || quirk.Name.Contains("Unpredictable"))
                            lawChaos += 20;
                        if (quirk.Name.Contains("Evil") || quirk.Name.Contains("Villain"))
                            goodEvil += 20;
                    }
                }
            }
            
            return new Translation.PlayerAlignment
            {
                LawChaos = Mathf.Clamp(lawChaos, -100, 100),
                GoodEvil = Mathf.Clamp(goodEvil, -100, 100)
            };
        }
        
        private void ApplyAlignmentShift(Translation.AlignmentShift shift)
        {
            // Store alignment in player state if needed
            // For now, just track via reputation tags
            if (shift.LawChaos > 0)
                AddReputationTag("Chaotic");
            if (shift.GoodEvil > 0)
                AddReputationTag("Evil");
            if (shift.GoodEvil < 0)
                AddReputationTag("Good");
        }
        
        private void AddReputationTag(string tag)
        {
            if (player.ReputationTags == null)
                player.ReputationTags = new List<string>();
            
            if (!player.ReputationTags.Contains(tag))
                player.ReputationTags.Add(tag);
        }
        
        private void RecordStanceFromCategory(Templates.PoliticalCategory category, string stance)
        {
            // Map category to issue
            IssueCategory? issue = MapCategoryToIssue(category);
            if (issue.HasValue)
            {
                RecordStance(issue.Value, stance);
            }
        }
        
        private IssueCategory? MapCategoryToIssue(Templates.PoliticalCategory category)
        {
            return category switch
            {
                Templates.PoliticalCategory.HealthcarePolicy => IssueCategory.Healthcare,
                Templates.PoliticalCategory.EconomicPolicy => IssueCategory.Economy,
                Templates.PoliticalCategory.Education => IssueCategory.Education,
                Templates.PoliticalCategory.Immigration => IssueCategory.Immigration,
                Templates.PoliticalCategory.ClimateEnvironment => IssueCategory.Environment,
                Templates.PoliticalCategory.CrimeJustice => IssueCategory.Crime,
                _ => null
            };
        }
        
        private int GetCurrentTurn()
        {
            // Get from game state if available
            var gameLoop = UnityEngine.Object.FindFirstObjectByType<GameLoop>();
            if (gameLoop != null)
            {
                var gameState = gameLoop.GetGameState();
                return gameState?.TotalDaysElapsed ?? 0;
            }
            return 0;
        }
        
        /// <summary>
        /// Get stance history for an issue
        /// </summary>
        public List<StanceRecord> GetStanceHistory(IssueCategory issue)
        {
            return stanceHistory
                .Where(s => s.Issue == issue)
                .OrderByDescending(s => s.Date)
                .ToList();
        }
        
        /// <summary>
        /// Check consistency score (higher = more consistent)
        /// </summary>
        public float GetConsistencyScore()
        {
            if (stanceHistory.Count < 2)
                return 100f;
            
            int flipFlops = 0;
            var issues = stanceHistory.Select(s => s.Issue).Distinct().ToList();
            
            foreach (var issue in issues)
            {
                var stances = stanceHistory
                    .Where(s => s.Issue == issue)
                    .OrderBy(s => s.Date)
                    .Select(s => s.Stance)
                    .ToList();
                
                for (int i = 1; i < stances.Count; i++)
                {
                    if (IsOppositeStance(stances[i - 1], stances[i]))
                    {
                        flipFlops++;
                    }
                }
            }
            
            float consistency = 100f - (flipFlops * 10f);
            return Mathf.Clamp(consistency, 0f, 100f);
        }
    }
    
    [Serializable]
    public class StanceRecord
    {
        public string ID;
        public IssueCategory Issue;
        public IssueCategory IssueCategory; // Alias for compatibility
        public string Stance;
        public string StanceTaken; // Alias for compatibility
        public DateTime Date;
        public float StanceStrength;
        public int TurnTaken;
        public string SourceEventId;
        public bool WasPublic;

        public StanceRecord()
        {
            StanceStrength = 1.0f;
            WasPublic = true;
        }
    }
    
    public class ResponseResult
    {
        public bool Success;
        public string Message;
        public Dictionary<string, float> Effects;
        public string EventId;
        public string ResponseOptionId;
        public bool PlayerResponded;

        public ResponseResult()
        {
            Effects = new Dictionary<string, float>();
        }
    }
    
    public class StanceResult
    {
        public bool Success;
        public string Message;
        public Dictionary<string, float> Effects;
        public bool IsFlipFlop;
    }
    
    public class CrisisResponseResult
    {
        public bool Success;
        public string Message;
        public Dictionary<string, float> Effects;
        public bool Escalated;
    }
    
    public class OpportunityResult
    {
        public bool Success;
        public string Message;
        public Dictionary<string, float> Benefits;
    }
}

