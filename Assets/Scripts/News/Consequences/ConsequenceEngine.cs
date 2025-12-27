// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - Consequence Engine
// Stage 7: Effect Calculation, Application, and Long-term Impact Tracking
// ═══════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.News.Translation;
using ElectionEmpire.News.Templates;
using ElectionEmpire.News;
using ElectionEmpire.Core;

namespace ElectionEmpire.News.Consequences
{
    #region Enums & Configuration
    
    /// <summary>
    /// Types of effects that can be applied.
    /// </summary>
    public enum EffectType
    {
        Immediate,      // Applied instantly
        Delayed,        // Applied after N turns
        Gradual,        // Applied over multiple turns
        Conditional,    // Applied only if condition met
        Permanent       // Never decays
    }
    
    /// <summary>
    /// Resources that can be affected.
    /// </summary>
/// <summary>
    /// Configuration for the consequence system.
    /// </summary>
    [Serializable]
    public class ConsequenceConfig
    {
        [Header("Scaling Factors")]
        public float CrisisSuccessBonus = 1.5f;
        public float CrisisFailurePenalty = 2.0f;
        public float ChaosModeMultiplier = 1.5f;
        public float HighApprovalDiminishingReturns = 0.8f;
        public float LowApprovalAmplification = 1.2f;
        
        [Header("Decay Rates")]
        public float TrustDecayPerTurn = 0.02f;
        public float MediaDecayPerTurn = 0.1f;
        public float VoterBlocDecayPerTurn = 0.03f;
        
        [Header("Thresholds")]
        public float MinimumEffectThreshold = 0.01f;
        public float CriticalTrustThreshold = 0.3f;
        public float MaxSingleEffectMagnitude = 0.3f;
        
        [Header("Polling Integration")]
        public float RealPollWeight = 0.3f;
        public float GameStateWeight = 0.7f;
    }
    
    #endregion
    
    #region Data Structures
    
    /// <summary>
    /// A single effect to be applied.
    /// </summary>
    [Serializable]
    public class ConsequenceEffect
    {
        public string EffectId;
        public string SourceEventId;
        public ResourceType Resource;
        public EffectType Type;
        
        public float BaseMagnitude;
        public float ScaledMagnitude;
        public float ActualApplied;
        
        public int DelayTurns;
        public int DurationTurns;
        public int TurnsRemaining;
        
        public string TargetVoterBloc;  // For voter bloc effects
        public string Condition;         // For conditional effects
        
        public DateTime CreatedAt;
        public DateTime AppliedAt;
        public bool IsApplied;
        public bool IsExpired;
        
        public Dictionary<string, float> Modifiers;
        
        public ConsequenceEffect()
        {
            Modifiers = new Dictionary<string, float>();
            CreatedAt = DateTime.UtcNow;
        }
    }
    /// <summary>
    /// A reputation tag that affects future interactions.
    /// </summary>
    [Serializable]
    public class ReputationTag
    {
        public string TagId;
        public string DisplayName;
        public string Description;
        public float Strength;          // -1.0 to 1.0
        public int DurationTurns;       // -1 for permanent
        public string Category;         // e.g., "crisis_management", "scandal_handling"
        public DateTime AcquiredAt;
    }
    /// <summary>
    /// Context for calculating consequences.
    /// </summary>
    public class ConsequenceContext
    {
        public int PlayerOfficeTier;
        public float PlayerApproval;
        public float PlayerTrustRating;
        public int CurrentTurn;
        public int TurnsUntilElection;
        public bool IsChaosModeEnabled;
        
        public Dictionary<string, float> CurrentResources;
        public Dictionary<string, float> VoterBlocSupport;
        public List<ReputationTag> ActiveTags;
        public List<StanceRecord> StanceHistory;
        
        public float RealWorldPollData;  // Optional integration with real polling
        
        public ConsequenceContext()
        {
            CurrentResources = new Dictionary<string, float>();
            VoterBlocSupport = new Dictionary<string, float>();
            ActiveTags = new List<ReputationTag>();
            StanceHistory = new List<StanceRecord>();
        }
    }
    
    #endregion
    
    /// <summary>
    /// Calculates consequences from player responses to news events.
    /// </summary>
    public class ConsequenceCalculator
    {
        private readonly ConsequenceConfig _config;
        private readonly IConsequenceEngineGameStateProvider _gameState;
        private readonly System.Random _random;
        
        public ConsequenceCalculator(IConsequenceEngineGameStateProvider gameState, ConsequenceConfig config = null)
        {
            _gameState = gameState;
            _config = config ?? new ConsequenceConfig();
            _random = new System.Random();
        }
        
        #region Main Calculation Pipeline
        
        /// <summary>
        /// Calculate full consequences for a player response.
        /// Five-step calculation: Base → Context Scaling → Character Mods → Polling → Success Roll
        /// </summary>
        public ResponseResult CalculateConsequences(
            NewsGameEvent gameEvent,
            ResponseOption chosenResponse,
            ConsequenceContext context)
        {
            var result = new ResponseResult
            {
                EventId = gameEvent.EventId,
                ResponseOptionId = chosenResponse.OptionId,
                ImmediateEffects = new List<ConsequenceEffect>(),
                DelayedEffects = new List<ConsequenceEffect>(),
                ReputationChanges = new List<ReputationTag>(),
                UnlockedEvents = new List<string>(),
                BlockedEvents = new List<string>()
            };
            
            // Step 1: Base effects from response option
            var baseEffects = ExtractBaseEffects(chosenResponse);
            
            // Step 2: Context scaling (office tier, event urgency, media attention)
            var scaledEffects = ApplyContextScaling(baseEffects, gameEvent, context);
            
            // Step 3: Character modifiers (traits, background, alignment)
            var modifiedEffects = ApplyCharacterModifiers(scaledEffects, context);
            
            // Step 4: Polling integration (optional real-world data)
            var pollingAdjusted = ApplyPollingIntegration(modifiedEffects, context);
            
            // Step 5: Success/failure roll
            result.IsSuccess = CalculateSuccessRoll(chosenResponse, context, out float roll, out float threshold);
            result.SuccessRoll = roll;
            result.SuccessThreshold = threshold;
            
            // Apply success/failure outcomes
            var finalEffects = ApplySuccessFailure(pollingAdjusted, chosenResponse, result.IsSuccess);
            
            // Categorize effects
            foreach (var effect in finalEffects)
            {
                if (effect.Type == EffectType.Immediate)
                {
                    result.ImmediateEffects.Add(effect);
                }
                else
                {
                    result.DelayedEffects.Add(effect);
                }
            }
            
            // Calculate reputation changes
            result.ReputationChanges = CalculateReputationChanges(gameEvent, chosenResponse, result.IsSuccess, context);
            
            // Generate narrative
            result.NarrativeOutcome = GenerateNarrativeOutcome(gameEvent, chosenResponse, result.IsSuccess);
            result.MediaHeadline = GenerateMediaHeadline(gameEvent, chosenResponse, result.IsSuccess);
            
            // Determine event chains
            DetermineEventChains(gameEvent, chosenResponse, result);
            
            return result;
        }
        
        #endregion
        
        #region Step 1: Base Effects
        
        /// <summary>
        /// Extract base effects from the response option.
        /// </summary>
        private List<ConsequenceEffect> ExtractBaseEffects(ResponseOption response)
        {
            var effects = new List<ConsequenceEffect>();
            
            // Resource effects
            if (response.ResourceEffects != null)
            {
                foreach (var kvp in response.ResourceEffects)
                {
                    effects.Add(new ConsequenceEffect
                    {
                        EffectId = Guid.NewGuid().ToString(),
                        Resource = ParseResourceType(kvp.Key),
                        Type = EffectType.Immediate,
                        BaseMagnitude = kvp.Value
                    });
                }
            }
            
            // Voter bloc effects
            if (response.VoterBlocEffects != null)
            {
                foreach (var kvp in response.VoterBlocEffects)
                {
                    effects.Add(new ConsequenceEffect
                    {
                        EffectId = Guid.NewGuid().ToString(),
                        Resource = ResourceType.VoterBlocSupport,
                        Type = EffectType.Gradual,
                        BaseMagnitude = kvp.Value,
                        TargetVoterBloc = kvp.Key,
                        DurationTurns = 3
                    });
                }
            }
            
            // Alignment effects
            if (response.AlignmentEffect != null)
            {
                effects.Add(new ConsequenceEffect
                {
                    EffectId = Guid.NewGuid().ToString(),
                    Resource = ResourceType.AlignmentLawChaos,
                    Type = EffectType.Permanent,
                    BaseMagnitude = response.AlignmentEffect.LawChaos
                });
                
                effects.Add(new ConsequenceEffect
                {
                    EffectId = Guid.NewGuid().ToString(),
                    Resource = ResourceType.AlignmentGoodEvil,
                    Type = EffectType.Permanent,
                    BaseMagnitude = response.AlignmentEffect.GoodEvil
                });
            }
            
            return effects;
        }
        
        private ResourceType ParseResourceType(string key)
        {
            return key.ToLower() switch
            {
                "trust" or "public_trust" => ResourceType.PublicTrust,
                "capital" or "political_capital" => ResourceType.PoliticalCapital,
                "funds" or "campaign_funds" => ResourceType.CampaignFunds,
                "media" or "media_influence" => ResourceType.MediaInfluence,
                "party" or "party_loyalty" => ResourceType.PartyLoyalty,
                "staff" or "staff_morale" => ResourceType.StaffMorale,
                _ => ResourceType.PublicTrust
            };
        }
        
        #endregion
        
        #region Step 2: Context Scaling
        
        /// <summary>
        /// Apply context-based scaling to effects.
        /// </summary>
        private List<ConsequenceEffect> ApplyContextScaling(
            List<ConsequenceEffect> effects,
            NewsGameEvent gameEvent,
            ConsequenceContext context)
        {
            foreach (var effect in effects)
            {
                float scaling = 1.0f;
                
                // Office tier scaling
                float tierMultiplier = context.PlayerOfficeTier switch
                {
                    1 => 0.5f,
                    2 => 0.75f,
                    3 => 1.0f,
                    4 => 1.25f,
                    5 => 1.5f,
                    _ => 1.0f
                };
                scaling *= tierMultiplier;
                effect.Modifiers["office_tier"] = tierMultiplier;
                
                // Urgency scaling
                float urgencyMultiplier = gameEvent.Urgency switch
                {
                    UrgencyLevel.Breaking => 1.5f,
                    UrgencyLevel.Urgent => 1.25f,
                    UrgencyLevel.Developing => 1.0f,
                    UrgencyLevel.Informational => 0.75f,
                    _ => 1.0f
                };
                scaling *= urgencyMultiplier;
                effect.Modifiers["urgency"] = urgencyMultiplier;
                
                // Election proximity scaling
                if (context.TurnsUntilElection <= 5)
                {
                    float electionMultiplier = 1.0f + (5 - context.TurnsUntilElection) * 0.1f;
                    scaling *= electionMultiplier;
                    effect.Modifiers["election_proximity"] = electionMultiplier;
                }
                
                // Chaos mode
                if (context.IsChaosModeEnabled)
                {
                    scaling *= _config.ChaosModeMultiplier;
                    effect.Modifiers["chaos_mode"] = _config.ChaosModeMultiplier;
                }
                
                effect.ScaledMagnitude = effect.BaseMagnitude * scaling;
            }
            
            return effects;
        }
        
        #endregion
        
        #region Step 3: Character Modifiers
        
        /// <summary>
        /// Apply character-specific modifiers (traits, background, reputation).
        /// </summary>
        private List<ConsequenceEffect> ApplyCharacterModifiers(
            List<ConsequenceEffect> effects,
            ConsequenceContext context)
        {
            foreach (var effect in effects)
            {
                float modifier = 1.0f;
                
                // Approval-based diminishing returns / amplification
                if (effect.BaseMagnitude > 0)  // Positive effect
                {
                    if (context.PlayerApproval > 0.7f)
                    {
                        modifier *= _config.HighApprovalDiminishingReturns;
                        effect.Modifiers["high_approval_diminishing"] = _config.HighApprovalDiminishingReturns;
                    }
                }
                else  // Negative effect
                {
                    if (context.PlayerApproval < _config.CriticalTrustThreshold)
                    {
                        modifier *= _config.LowApprovalAmplification;
                        effect.Modifiers["low_approval_amplification"] = _config.LowApprovalAmplification;
                    }
                }
                
                // Reputation tag modifiers
                foreach (var tag in context.ActiveTags)
                {
                    float tagMod = GetReputationModifier(tag, effect);
                    if (tagMod != 1.0f)
                    {
                        modifier *= tagMod;
                        effect.Modifiers[$"reputation_{tag.TagId}"] = tagMod;
                    }
                }
                
                // Consistency bonus/penalty from stance history
                float consistencyMod = CalculateConsistencyModifier(effect, context);
                if (consistencyMod != 1.0f)
                {
                    modifier *= consistencyMod;
                    effect.Modifiers["consistency"] = consistencyMod;
                }
                
                effect.ScaledMagnitude *= modifier;
            }
            
            return effects;
        }
        
        private float GetReputationModifier(ReputationTag tag, ConsequenceEffect effect)
        {
            // Example: "crisis_expert" tag improves trust gains during crises
            return tag.TagId switch
            {
                "crisis_expert" when effect.Resource == ResourceType.PublicTrust => 1.0f + tag.Strength * 0.2f,
                "flip_flopper" when effect.Resource == ResourceType.PublicTrust => 1.0f - tag.Strength * 0.15f,
                "media_darling" when effect.Resource == ResourceType.MediaInfluence => 1.0f + tag.Strength * 0.25f,
                "party_loyalist" when effect.Resource == ResourceType.PartyLoyalty => 1.0f + tag.Strength * 0.2f,
                _ => 1.0f
            };
        }
        
        private float CalculateConsistencyModifier(ConsequenceEffect effect, ConsequenceContext context)
        {
            // Check if player has been consistent on related issues
            // Consistency provides bonus, flip-flopping provides penalty
            if (context.StanceHistory == null || context.StanceHistory.Count == 0)
            {
                return 1.0f; // No history = neutral
            }
            
            // Determine issue category from effect
            string category = DetermineIssueCategory(effect);
            if (string.IsNullOrEmpty(category))
            {
                return 1.0f; // Can't determine category
            }
            
            // Get historical position on this category
            var relevantStances = context.StanceHistory
                .Where(s => s.GetCategoryString() == category && s.WasPublic)
                .ToList();

            if (relevantStances.Count == 0)
            {
                return 1.0f; // No history on this category
            }
            
            // Calculate average historical stance
            float avgStance = relevantStances.Average(s => s.StanceStrength);
            
            // Determine expected stance from effect direction
            float expectedStance = effect.BaseMagnitude > 0 ? 0.5f : -0.5f;
            
            // Calculate consistency (how close is current action to historical average)
            float deviation = Mathf.Abs(avgStance - expectedStance);
            
            // Consistency modifier: 1.0 = consistent, <1.0 = flip-flop penalty
            if (deviation < 0.2f)
                return 1.1f; // Bonus for consistency
            if (deviation < 0.5f)
                return 1.0f; // Neutral
            if (deviation < 1.0f)
                return 0.85f; // Minor flip-flop penalty
            return 0.7f; // Major flip-flop penalty
        }
        
        private string DetermineIssueCategory(ConsequenceEffect effect)
        {
            // Map resource types to issue categories
            return effect.Resource switch
            {
                ResourceType.PublicTrust => "trust",
                ResourceType.PoliticalCapital => "political",
                ResourceType.CampaignFunds => "economic",
                ResourceType.MediaInfluence => "media",
                ResourceType.PartyLoyalty => "party",
                ResourceType.VoterBlocSupport => "voter",
                ResourceType.AlignmentLawChaos => "alignment",
                ResourceType.AlignmentGoodEvil => "alignment",
                _ => "general"
            };
        }
        
        #endregion
        
        #region Step 4: Polling Integration
        
        /// <summary>
        /// Integrate real-world polling data if available.
        /// </summary>
        private List<ConsequenceEffect> ApplyPollingIntegration(
            List<ConsequenceEffect> effects,
            ConsequenceContext context)
        {
            // Only apply if real polling data is available
            if (context.RealWorldPollData <= 0)
                return effects;
            
            foreach (var effect in effects)
            {
                if (effect.Resource == ResourceType.PublicTrust ||
                    effect.Resource == ResourceType.VoterBlocSupport)
                {
                    // Blend game state with real polling sentiment
                    float blendedMagnitude = 
                        effect.ScaledMagnitude * _config.GameStateWeight +
                        effect.ScaledMagnitude * context.RealWorldPollData * _config.RealPollWeight;
                    
                    effect.ScaledMagnitude = blendedMagnitude;
                    effect.Modifiers["polling_integration"] = context.RealWorldPollData;
                }
            }
            
            return effects;
        }
        
        #endregion
        
        #region Step 5: Success Roll
        
        /// <summary>
        /// Calculate whether the response succeeds or fails.
        /// </summary>
        private bool CalculateSuccessRoll(
            ResponseOption response,
            ConsequenceContext context,
            out float roll,
            out float threshold)
        {
            // Base success chance from response
            threshold = response.SuccessProbability;
            
            // Modify by staff quality
            if (context.CurrentResources.TryGetValue("staff_quality", out float staffQuality))
            {
                threshold += (staffQuality - 0.5f) * 0.2f;
            }
            
            // Modify by relevant character stats
            // (Would integrate with character system)
            
            // Cap threshold
            threshold = Mathf.Clamp(threshold, 0.1f, 0.95f);
            
            // Roll
            roll = (float)_random.NextDouble();
            
            return roll <= threshold;
        }
        
        /// <summary>
        /// Apply success or failure outcomes to effects.
        /// </summary>
        private List<ConsequenceEffect> ApplySuccessFailure(
            List<ConsequenceEffect> effects,
            ResponseOption response,
            bool isSuccess)
        {
            float multiplier = isSuccess 
                ? _config.CrisisSuccessBonus 
                : _config.CrisisFailurePenalty;
            
            foreach (var effect in effects)
            {
                if (isSuccess)
                {
                    // Success: positive effects enhanced, negative reduced
                    if (effect.BaseMagnitude > 0)
                    {
                        effect.ScaledMagnitude *= multiplier;
                    }
                    else
                    {
                        effect.ScaledMagnitude *= 0.5f;
                    }
                }
                else
                {
                    // Failure: negative effects enhanced, positive reduced
                    if (effect.BaseMagnitude < 0)
                    {
                        effect.ScaledMagnitude *= multiplier;
                    }
                    else
                    {
                        effect.ScaledMagnitude *= 0.3f;
                    }
                }
                
                // Apply magnitude cap
                effect.ScaledMagnitude = Mathf.Clamp(
                    effect.ScaledMagnitude,
                    -_config.MaxSingleEffectMagnitude,
                    _config.MaxSingleEffectMagnitude);
                
                effect.ActualApplied = effect.ScaledMagnitude;
                effect.Modifiers["success_failure"] = isSuccess ? multiplier : -multiplier;
            }
            
            return effects;
        }
        
        #endregion
        
        #region Reputation & Narrative
        
        /// <summary>
        /// Calculate reputation tag changes from this response.
        /// </summary>
        private List<ReputationTag> CalculateReputationChanges(
            NewsGameEvent gameEvent,
            ResponseOption response,
            bool isSuccess,
            ConsequenceContext context)
        {
            var changes = new List<ReputationTag>();
            
            // Crisis handling reputation
            if (gameEvent.Type == ElectionEmpire.News.EventType.Crisis)
            {
                if (isSuccess)
                {
                    changes.Add(new ReputationTag
                    {
                        TagId = "crisis_handler",
                        DisplayName = "Crisis Handler",
                        Description = "Successfully managed a major crisis",
                        Strength = 0.3f,
                        DurationTurns = 20,
                        Category = "crisis_management",
                        AcquiredAt = DateTime.UtcNow
                    });
                }
                else
                {
                    changes.Add(new ReputationTag
                    {
                        TagId = "crisis_fumbler",
                        DisplayName = "Crisis Fumbler",
                        Description = "Mishandled a major crisis",
                        Strength = -0.3f,
                        DurationTurns = 15,
                        Category = "crisis_management",
                        AcquiredAt = DateTime.UtcNow
                    });
                }
            }
            
            // Response-type specific tags
            if (response.Label.Contains("Deny") && !isSuccess)
            {
                changes.Add(new ReputationTag
                {
                    TagId = "credibility_issue",
                    DisplayName = "Credibility Issues",
                    Description = "Denied something later proven true",
                    Strength = -0.4f,
                    DurationTurns = 25,
                    Category = "honesty",
                    AcquiredAt = DateTime.UtcNow
                });
            }
            
            return changes;
        }
        
        private string GenerateNarrativeOutcome(NewsGameEvent gameEvent, ResponseOption response, bool isSuccess)
        {
            if (isSuccess)
            {
                return $"Your decision to {response.Label.ToLower()} on {gameEvent.Headline} paid off. " +
                       "Public reaction has been largely favorable.";
            }
            else
            {
                return $"Your decision to {response.Label.ToLower()} on {gameEvent.Headline} backfired. " +
                       "Critics are seizing on the misstep.";
            }
        }
        
        private string GenerateMediaHeadline(NewsGameEvent gameEvent, ResponseOption response, bool isSuccess)
        {
            if (isSuccess)
            {
                return $"DECISIVE ACTION: {_gameState.GetPlayerName()} Handles {gameEvent.Category} Crisis";
            }
            else
            {
                return $"MISSTEP: {_gameState.GetPlayerName()}'s Response to {gameEvent.Category} Draws Fire";
            }
        }
        
        private void DetermineEventChains(NewsGameEvent gameEvent, ResponseOption response, ResponseResult result)
        {
            // Certain responses unlock follow-up events
            // E.g., aggressive counter-attack might trigger opponent retaliation event
            
            // Check if response has chaining potential
            if (response.Label.Contains("Attack") || response.Label.Contains("Counter"))
            {
                // Aggressive responses may trigger opponent retaliation
                if (UnityEngine.Random.value < 0.3f) // 30% chance
                {
                    result.UnlockedEvents.Add($"retaliation_{gameEvent.EventId}");
                }
            }
            
            // Failed responses may trigger follow-up crisis
            if (!result.IsSuccess && gameEvent.Type == ElectionEmpire.News.EventType.Crisis)
            {
                if (UnityEngine.Random.value < 0.4f) // 40% chance
                {
                    result.UnlockedEvents.Add($"escalation_{gameEvent.EventId}");
                }
            }

            // Successful crisis handling may unlock opportunities
            if (result.IsSuccess && gameEvent.Type == ElectionEmpire.News.EventType.Crisis)
            {
                if (UnityEngine.Random.value < 0.25f) // 25% chance
                {
                    result.UnlockedEvents.Add($"opportunity_{gameEvent.Category}");
                }
            }

            // Scandal responses may block or trigger related events
            if (gameEvent.Type == ElectionEmpire.News.EventType.Scandal)
            {
                if (response.Label.Contains("Deny") && !result.IsSuccess)
                {
                    // Failed denial may trigger deeper investigation
                    result.UnlockedEvents.Add($"investigation_{gameEvent.EventId}");
                }
                else if (response.Label.Contains("Admit") && result.IsSuccess)
                {
                    // Successful admission may block further scandal events
                    result.BlockedEvents.Add($"scandal_{gameEvent.Category}");
                }
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// Applies calculated effects to the game state.
    /// </summary>
    public class EffectApplicator
    {
        private readonly IGameStateModifier _gameState;
        private readonly ConsequenceConfig _config;
        private readonly Queue<ConsequenceEffect> _delayedEffectQueue;
        private readonly List<ConsequenceEffect> _activeGradualEffects;
        
        public event Action<ConsequenceEffect> OnEffectApplied;
        public event Action<ConsequenceEffect> OnEffectExpired;
        
        public EffectApplicator(IGameStateModifier gameState, ConsequenceConfig config = null)
        {
            _gameState = gameState;
            _config = config ?? new ConsequenceConfig();
            _delayedEffectQueue = new Queue<ConsequenceEffect>();
            _activeGradualEffects = new List<ConsequenceEffect>();
        }
        
        #region Effect Application
        
        /// <summary>
        /// Apply a response result to the game state.
        /// </summary>
        public void ApplyResponseResult(ResponseResult result)
        {
            // Apply immediate effects
            foreach (var effect in result.ImmediateEffects)
            {
                ApplyEffect(effect);
            }
            
            // Queue delayed effects
            foreach (var effect in result.DelayedEffects)
            {
                if (effect.Type == EffectType.Delayed)
                {
                    _delayedEffectQueue.Enqueue(effect);
                }
                else if (effect.Type == EffectType.Gradual)
                {
                    effect.TurnsRemaining = effect.DurationTurns;
                    _activeGradualEffects.Add(effect);
                }
            }
            
            // Apply reputation changes
            foreach (var tag in result.ReputationChanges)
            {
                _gameState.AddReputationTag(tag);
            }
        }
        
        /// <summary>
        /// Apply a single effect to the game state.
        /// </summary>
        public void ApplyEffect(ConsequenceEffect effect)
        {
            if (Mathf.Abs(effect.ActualApplied) < _config.MinimumEffectThreshold)
            {
                return;  // Effect too small to matter
            }
            
            switch (effect.Resource)
            {
                case ResourceType.PublicTrust:
                    _gameState.ModifyTrust(effect.ActualApplied);
                    break;
                    
                case ResourceType.PoliticalCapital:
                    _gameState.ModifyPoliticalCapital(effect.ActualApplied);
                    break;
                    
                case ResourceType.CampaignFunds:
                    _gameState.ModifyCampaignFunds(effect.ActualApplied);
                    break;
                    
                case ResourceType.MediaInfluence:
                    _gameState.ModifyMediaInfluence(effect.ActualApplied);
                    break;
                    
                case ResourceType.PartyLoyalty:
                    _gameState.ModifyPartyLoyalty(effect.ActualApplied);
                    break;
                    
                case ResourceType.StaffMorale:
                    _gameState.ModifyStaffMorale(effect.ActualApplied);
                    break;
                    
                case ResourceType.VoterBlocSupport:
                    if (!string.IsNullOrEmpty(effect.TargetVoterBloc))
                    {
                        _gameState.ModifyVoterBlocSupport(effect.TargetVoterBloc, effect.ActualApplied);
                    }
                    break;
                    
                case ResourceType.AlignmentLawChaos:
                    _gameState.ShiftAlignment(effect.ActualApplied, 0);
                    break;
                    
                case ResourceType.AlignmentGoodEvil:
                    _gameState.ShiftAlignment(0, effect.ActualApplied);
                    break;
            }
            
            effect.IsApplied = true;
            effect.AppliedAt = DateTime.UtcNow;
            OnEffectApplied?.Invoke(effect);
            
            Debug.Log($"[EffectApplicator] Applied {effect.Resource}: {effect.ActualApplied:+0.00;-0.00}");
        }
        
        #endregion
        
        #region Turn Processing
        
        /// <summary>
        /// Process effects on turn advance.
        /// </summary>
        public void ProcessTurn()
        {
            // Process delayed effects
            ProcessDelayedEffects();
            
            // Process gradual effects
            ProcessGradualEffects();
            
            // Apply natural decay
            ApplyNaturalDecay();
        }
        
        private void ProcessDelayedEffects()
        {
            var effectsToApply = new List<ConsequenceEffect>();
            var remainingEffects = new Queue<ConsequenceEffect>();
            
            while (_delayedEffectQueue.Count > 0)
            {
                var effect = _delayedEffectQueue.Dequeue();
                effect.DelayTurns--;
                
                if (effect.DelayTurns <= 0)
                {
                    effectsToApply.Add(effect);
                }
                else
                {
                    remainingEffects.Enqueue(effect);
                }
            }
            
            // Re-queue remaining
            while (remainingEffects.Count > 0)
            {
                _delayedEffectQueue.Enqueue(remainingEffects.Dequeue());
            }
            
            // Apply ready effects
            foreach (var effect in effectsToApply)
            {
                ApplyEffect(effect);
            }
        }
        
        private void ProcessGradualEffects()
        {
            var expiredEffects = new List<ConsequenceEffect>();
            
            foreach (var effect in _activeGradualEffects)
            {
                // Apply portion of effect
                float portionMagnitude = effect.ActualApplied / effect.DurationTurns;
                var portionEffect = new ConsequenceEffect
                {
                    Resource = effect.Resource,
                    TargetVoterBloc = effect.TargetVoterBloc,
                    ActualApplied = portionMagnitude
                };
                ApplyEffect(portionEffect);
                
                effect.TurnsRemaining--;
                
                if (effect.TurnsRemaining <= 0)
                {
                    effect.IsExpired = true;
                    expiredEffects.Add(effect);
                }
            }
            
            // Remove expired
            foreach (var effect in expiredEffects)
            {
                _activeGradualEffects.Remove(effect);
                OnEffectExpired?.Invoke(effect);
            }
        }
        
        private void ApplyNaturalDecay()
        {
            // Trust decay
            _gameState.ModifyTrust(-_config.TrustDecayPerTurn);
            
            // Media influence decay
            _gameState.ModifyMediaInfluence(-_config.MediaDecayPerTurn);
            
            // Voter bloc support decay
            foreach (var bloc in _gameState.GetAllVoterBlocs())
            {
                _gameState.ModifyVoterBlocSupport(bloc, -_config.VoterBlocDecayPerTurn);
            }
        }
        
        #endregion
        
        #region Query Methods
        
        /// <summary>
        /// Get count of pending delayed effects.
        /// </summary>
        public int GetPendingEffectCount()
        {
            return _delayedEffectQueue.Count + _activeGradualEffects.Count;
        }
        
        /// <summary>
        /// Get summary of active gradual effects.
        /// </summary>
        public List<(string Resource, float RemainingMagnitude, int TurnsLeft)> GetActiveGradualEffects()
        {
            return _activeGradualEffects.Select(e => (
                e.Resource.ToString(),
                e.ActualApplied * e.TurnsRemaining / e.DurationTurns,
                e.TurnsRemaining
            )).ToList();
        }
        
        #endregion
    }
    
    /// <summary>
    /// Tracks player stance history for consistency scoring.
    /// </summary>
    public class StanceHistoryTracker
    {
        private readonly Dictionary<string, List<StanceRecord>> _stancesByCategory;
        private readonly int _maxHistoryPerCategory = 50;
        
        public StanceHistoryTracker()
        {
            _stancesByCategory = new Dictionary<string, List<StanceRecord>>();
        }
        
        /// <summary>
        /// Record a stance taken by the player.
        /// </summary>
        public void RecordStance(string category, string stance, float strength, int turn, string eventId, bool wasPublic = true)
        {
            if (!_stancesByCategory.ContainsKey(category))
            {
                _stancesByCategory[category] = new List<StanceRecord>();
            }
            
            _stancesByCategory[category].Add(new StanceRecord
            {
                CategoryString = category,
                StanceTaken = stance,
                StanceStrength = strength,
                TurnTaken = turn,
                SourceEventId = eventId,
                WasPublic = wasPublic
            });
            
            // Trim old entries
            while (_stancesByCategory[category].Count > _maxHistoryPerCategory)
            {
                _stancesByCategory[category].RemoveAt(0);
            }
        }
        
        /// <summary>
        /// Calculate consistency score for a potential stance.
        /// </summary>
        public float CalculateConsistencyScore(string category, float proposedStrength)
        {
            if (!_stancesByCategory.TryGetValue(category, out var history) || history.Count == 0)
            {
                return 1.0f;  // No history = neutral
            }
            
            // Get average historical stance
            float avgStance = history.Where(s => s.WasPublic).Average(s => s.StanceStrength);
            
            // Calculate deviation
            float deviation = Mathf.Abs(proposedStrength - avgStance);
            
            // Consistency score: 1.0 = consistent, <1.0 = flip-flop penalty
            if (deviation < 0.2f) return 1.1f;   // Bonus for consistency
            if (deviation < 0.5f) return 1.0f;   // Neutral
            if (deviation < 1.0f) return 0.85f;  // Minor flip-flop
            return 0.7f;                          // Major flip-flop
        }
        
        /// <summary>
        /// Check if taking a stance would be considered a flip-flop.
        /// </summary>
        public bool WouldBeFlipFlop(string category, float proposedStrength, out string previousStance)
        {
            previousStance = null;
            
            if (!_stancesByCategory.TryGetValue(category, out var history) || history.Count == 0)
            {
                return false;
            }
            
            var recent = history.Where(s => s.WasPublic).LastOrDefault();
            if (recent == null) return false;
            
            // Flip-flop if signs are different and both are strong stances
            if (Mathf.Sign(proposedStrength) != Mathf.Sign(recent.StanceStrength) &&
                Mathf.Abs(proposedStrength) > 0.3f && Mathf.Abs(recent.StanceStrength) > 0.3f)
            {
                previousStance = recent.StanceTaken;
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Get player's historical position on a category.
        /// </summary>
        public float GetHistoricalPosition(string category)
        {
            if (!_stancesByCategory.TryGetValue(category, out var history) || history.Count == 0)
            {
                return 0f;
            }
            
            return history.Where(s => s.WasPublic).Average(s => s.StanceStrength);
        }
    }
    
    #region Interfaces
    
    /// <summary>
    /// Interface for reading game state (used by ConsequenceCalculator).
    /// </summary>
    public interface IConsequenceEngineGameStateProvider
    {
        int GetPlayerOfficeTier();
        string GetPlayerOfficeTitle();
        string GetPlayerName();
        string GetPlayerParty();
        int GetCurrentTurn();
        int GetTurnsUntilElection();
        float GetPlayerApproval();
        (float LawChaos, float GoodEvil) GetPlayerAlignment();
        bool IsChaosModeEnabled();
    }
    
    /// <summary>
    /// Interface for modifying game state (used by EffectApplicator).
    /// </summary>
    public interface IGameStateModifier
    {
        void ModifyTrust(float delta);
        void ModifyPoliticalCapital(float delta);
        void ModifyCampaignFunds(float delta);
        void ModifyMediaInfluence(float delta);
        void ModifyPartyLoyalty(float delta);
        void ModifyStaffMorale(float delta);
        void ModifyVoterBlocSupport(string bloc, float delta);
        void ShiftAlignment(float lawChaosDelta, float goodEvilDelta);
        void AddReputationTag(ReputationTag tag);
        List<string> GetAllVoterBlocs();
    }
    
    #endregion

    #region Response Result

    /// <summary>
    /// Result from a player response to a news event with full consequence tracking.
    /// </summary>
    public class ResponseResult
    {
        public string EventId;
        public string ResponseOptionId;
        public bool IsSuccess;
        public float SuccessRoll;
        public float SuccessThreshold;
        public List<ConsequenceEffect> ImmediateEffects;
        public List<ConsequenceEffect> DelayedEffects;
        public List<ReputationTag> ReputationChanges;
        public string NarrativeOutcome;
        public string MediaHeadline;
        public List<string> UnlockedEvents;
        public List<string> BlockedEvents;

        public ResponseResult()
        {
            ImmediateEffects = new List<ConsequenceEffect>();
            DelayedEffects = new List<ConsequenceEffect>();
            ReputationChanges = new List<ReputationTag>();
            UnlockedEvents = new List<string>();
            BlockedEvents = new List<string>();
        }
    }

    #endregion
}

