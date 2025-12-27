// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - News Translation Core Classes
// Stage 3-4: Template Matching & Event Factory
// ═══════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.News.Templates;
using ElectionEmpire.Core;
using ElectionEmpire.World;
using ElectionEmpire.Gameplay;
using ElectionEmpire.News;

namespace ElectionEmpire.News.Translation
{
    #region Data Transfer Objects
    
    /// <summary>
    /// Processed news item from Stage 2 (Content Processing)
    /// </summary>
    [Serializable]
    public class ProcessedNewsItem
    {
        // Original data
        public string SourceId;
        public string Headline;
        public string Summary;
        public string FullText;
        public DateTime PublishedAt;
        public DateTime FetchedAt;
        public string SourceUrl;
        public float SourceBias;          // -1.0 (left) to +1.0 (right)
        public float SourceCredibility;   // 0.0 to 1.0
        
        // Classification results
        public PoliticalCategory PrimaryCategory;
        public List<PoliticalCategory> SecondaryCategories;
        public float CategoryConfidence;
        
        // Extracted entities
        public ExtractedEntities Entities;
        
        // Sentiment analysis
        public float OverallSentiment;    // -100 to +100
        public Dictionary<string, float> EntitySentiments;
        public float ControversyScore;    // 0.0 to 1.0
        public PartisanPredictions PartisanReactions;
        
        // Computed scores
        public float PoliticalRelevance;  // 0.0 to 1.0
        public float ImpactScore;         // 1 to 10
        public TemporalClass TemporalClassification;
        
        public ProcessedNewsItem()
        {
            SecondaryCategories = new List<PoliticalCategory>();
            Entities = new ExtractedEntities();
            EntitySentiments = new Dictionary<string, float>();
            PartisanReactions = new PartisanPredictions();
        }
    }
    
    [Serializable]
    public class ExtractedEntities
    {
        public List<ExtractedEntity> People = new List<ExtractedEntity>();
        public List<ExtractedEntity> Organizations = new List<ExtractedEntity>();
        public List<ExtractedEntity> Locations = new List<ExtractedEntity>();
        public List<ExtractedEntity> Legislation = new List<ExtractedEntity>();
        public List<ExtractedEntity> Events = new List<ExtractedEntity>();
        
        public List<ExtractedEntity> GetByType(TemplateEntityType type)
        {
            return type switch
            {
                TemplateEntityType.Person => People,
                TemplateEntityType.Organization => Organizations,
                TemplateEntityType.Location => Locations,
                TemplateEntityType.Legislation => Legislation,
                TemplateEntityType.Event => Events,
                _ => new List<ExtractedEntity>()
            };
        }
    }
    
    [Serializable]
    public class ExtractedEntity
    {
        public string Name;
        public TemplateEntityType Type;
        public string SubType;           // e.g., "govt", "company", "party"
        public float Relevance;          // 0.0 to 1.0
        public string Role;              // e.g., "bill_sponsor", "accused"
        public Dictionary<string, string> Attributes;
        
        public ExtractedEntity()
        {
            Attributes = new Dictionary<string, string>();
        }
    }
    
    [Serializable]
    public class PartisanPredictions
    {
        public float Left;
        public float Center;
        public float Right;
    }
    
    public enum TemporalClass
    {
        Breaking,
        Developing,
        Ongoing,
        Historical
    }
    
    /// <summary>
    /// Result of template matching - template + resolved variables
    /// </summary>
    [Serializable]
    public class MatchedTemplate
    {
        public EventTemplate Template;
        public float MatchScore;
        public Dictionary<string, string> ResolvedVariables;
        public string GeneratedHeadline;
        public string GeneratedDescription;
        public string GeneratedContext;
        public ProcessedNewsItem SourceNews;
        
        public MatchedTemplate()
        {
            ResolvedVariables = new Dictionary<string, string>();
        }
    }
    [Serializable]
    public class AlignmentRange
    {
        public int MinLawChaos;
        public int MaxLawChaos;
        public int MinGoodEvil;
        public int MaxGoodEvil;
        
        public bool IsInRange(int lawChaos, int goodEvil)
        {
            return lawChaos >= MinLawChaos && lawChaos <= MaxLawChaos &&
                   goodEvil >= MinGoodEvil && goodEvil <= MaxGoodEvil;
        }
    }
    
    [Serializable]
    public class AlignmentShift
    {
        public int LawChaos;
        public int GoodEvil;
    }
    
    [Serializable]
    public class ResourceRequirement
    {
        public string ResourceType;
        public float MinAmount;
    }
    
    [Serializable]
    public class ResponseOutcome
    {
        public string Description;
        public Dictionary<string, float> ResourceModifiers;
        public Dictionary<string, float> VoterBlocModifiers;
        public List<string> TriggeredEvents;
        public List<string> ReputationTags;
        
        public ResponseOutcome()
        {
            ResourceModifiers = new Dictionary<string, float>();
            VoterBlocModifiers = new Dictionary<string, float>();
            TriggeredEvents = new List<string>();
            ReputationTags = new List<string>();
        }
    }
    
    [Serializable]
    public class EscalationStage
    {
        public int StageNumber;
        public string StageName;
        public string Description;
        public int TurnsUntilNext;
        public float SeverityMultiplier;
        public List<ResponseOption> StageSpecificOptions;
        
        public EscalationStage()
        {
            StageSpecificOptions = new List<ResponseOption>();
        }
    }
    
    [Serializable]
    public class ScaledEffects
    {
        public float TrustDelta;
        public float CapitalDelta;
        public float FundsDelta;
        public float MediaDelta;
        public float PartyLoyaltyDelta;
        public Dictionary<string, float> VoterBlocDeltas;
        
        public ScaledEffects()
        {
            VoterBlocDeltas = new Dictionary<string, float>();
        }
    }
    
    [Serializable]
    public class PlayerResponse
    {
        public string OptionId;
        public int Turn;
        public bool Success;
        public string GeneratedStatement;
    }

    [Serializable]
    public class ResponseOption
    {
        public string OptionId;
        public string Label;
        public string Description;
        public string StatementTemplate;

        // Alignment effects
        public AlignmentShift AlignmentEffect;
        public AlignmentRange RequiredAlignment;
        public bool IsAlignmentLocked;

        // Resource effects
        public Dictionary<string, float> ResourceEffects;
        public Dictionary<string, float> VoterBlocEffects;
        public List<ResourceRequirement> RequiredResources;

        // Success/failure mechanics
        public float SuccessProbability;
        public bool IsRisky;
        public ResponseOutcome SuccessOutcome;
        public ResponseOutcome FailureOutcome;

        // Chaos mode
        public bool ChaosModeOnly;

        public ResponseOption()
        {
            ResourceEffects = new Dictionary<string, float>();
            VoterBlocEffects = new Dictionary<string, float>();
            RequiredResources = new List<ResourceRequirement>();
            SuccessProbability = 1.0f;
        }
    }

    #endregion
    
    #region Template Matcher
    
    /// <summary>
    /// Scores and selects the best template for a processed news item.
    /// </summary>
    public class TemplateMatcher
    {
        private readonly INewsTranslationCoreGameStateProvider _gameState;
        
        // Scoring weights
        private const float ENTITY_MATCH_WEIGHT = 0.30f;
        private const float SENTIMENT_ALIGNMENT_WEIGHT = 0.20f;
        private const float OFFICE_RELEVANCE_WEIGHT = 0.25f;
        private const float CONTROVERSY_FIT_WEIGHT = 0.15f;
        private const float RECENCY_BONUS_WEIGHT = 0.10f;
        
        private const float MIN_MATCH_THRESHOLD = 0.4f;
        
        public TemplateMatcher(INewsTranslationCoreGameStateProvider gameState)
        {
            _gameState = gameState;
            EventTemplateLibrary.Initialize();
        }
        
        /// <summary>
        /// Find the best matching template for a news item.
        /// </summary>
        public MatchedTemplate FindBestMatch(ProcessedNewsItem news)
        {
            var categoryTemplates = EventTemplateLibrary.GetTemplatesForCategory(news.PrimaryCategory);
            
            if (categoryTemplates == null || categoryTemplates.Count == 0)
            {
                Debug.LogWarning($"[TemplateMatcher] No templates found for category: {news.PrimaryCategory}");
                return null;
            }
            
            EventTemplate bestTemplate = null;
            float bestScore = 0f;
            
            foreach (var template in categoryTemplates)
            {
                float score = CalculateTemplateScore(template, news);
                
                if (score > bestScore)
                {
                    bestScore = score;
                    bestTemplate = template;
                }
            }
            
            // Check minimum threshold
            if (bestScore < MIN_MATCH_THRESHOLD)
            {
                Debug.Log($"[TemplateMatcher] Best score {bestScore} below threshold for: {news.Headline}");
                // Use generic fallback template
                bestTemplate = GetFallbackTemplate(news.PrimaryCategory);
                bestScore = MIN_MATCH_THRESHOLD;
            }
            
            if (bestTemplate == null) return null;
            
            // Build matched template with resolved variables
            return BuildMatchedTemplate(bestTemplate, news, bestScore);
        }
        
        /// <summary>
        /// Calculate match score for a template against a news item.
        /// </summary>
        public float CalculateTemplateScore(EventTemplate template, ProcessedNewsItem news)
        {
            float entityScore = CalculateEntityMatchScore(template, news);
            float sentimentScore = CalculateSentimentAlignmentScore(template, news);
            float officeScore = CalculateOfficeRelevanceScore(template);
            float controversyScore = CalculateControversyFitScore(template, news);
            float recencyScore = CalculateRecencyScore(news);
            
            float totalScore = 
                entityScore * ENTITY_MATCH_WEIGHT +
                sentimentScore * SENTIMENT_ALIGNMENT_WEIGHT +
                officeScore * OFFICE_RELEVANCE_WEIGHT +
                controversyScore * CONTROVERSY_FIT_WEIGHT +
                recencyScore * RECENCY_BONUS_WEIGHT;
            
            // Apply keyword bonus
            totalScore *= CalculateKeywordBonus(template, news);
            
            // Apply impact threshold check
            if (news.ImpactScore < template.MinImpactScore)
            {
                totalScore *= 0.5f; // Penalty for low-impact news matching high-impact template
            }
            
            return Mathf.Clamp01(totalScore);
        }
        
        private float CalculateEntityMatchScore(EventTemplate template, ProcessedNewsItem news)
        {
            if (template.RequiredEntities == null || template.RequiredEntities.Count == 0)
                return 0.8f; // No requirements = good base score
            
            int matchedCount = 0;
            foreach (var requiredType in template.RequiredEntities)
            {
                var entities = news.Entities.GetByType(requiredType);
                if (entities != null && entities.Count > 0)
                {
                    matchedCount++;
                }
            }
            
            return (float)matchedCount / template.RequiredEntities.Count;
        }
        
        private float CalculateSentimentAlignmentScore(EventTemplate template, ProcessedNewsItem news)
        {
            // Match template event type with news sentiment
            float sentimentNormalized = (news.OverallSentiment + 100f) / 200f; // 0 to 1
            
            switch (template.DefaultEventType)
            {
                case TemplateEventType.Crisis:
                    // Crises match better with negative sentiment
                    return 1f - sentimentNormalized;
                
                case TemplateEventType.Opportunity:
                    // Opportunities match better with positive sentiment
                    return sentimentNormalized;
                
                case TemplateEventType.ScandalTrigger:
                    // Scandals match negative sentiment + high controversy
                    return (1f - sentimentNormalized) * 0.5f + news.ControversyScore * 0.5f;
                
                default:
                    // Policy pressure is neutral
                    return 0.5f + (news.ControversyScore * 0.3f);
            }
        }
        
        private float CalculateOfficeRelevanceScore(EventTemplate template)
        {
            int playerTier = _gameState.GetPlayerOfficeTier();
            float scaling = template.Scaling.GetMultiplier(playerTier);
            
            // Normalize scaling to 0-1 range (assuming max scaling is ~2.0)
            return Mathf.Clamp01(scaling / 2f);
        }
        
        private float CalculateControversyFitScore(EventTemplate template, ProcessedNewsItem news)
        {
            // Templates with high min controversy should match high controversy news
            float controversyDiff = Mathf.Abs(template.MinControversy - news.ControversyScore);
            
            // Closer match = higher score
            return 1f - controversyDiff;
        }
        
        private float CalculateRecencyScore(ProcessedNewsItem news)
        {
            // More recent news gets higher scores
            var age = DateTime.UtcNow - news.PublishedAt;
            
            if (age.TotalHours < 6) return 1.0f;
            if (age.TotalHours < 24) return 0.9f;
            if (age.TotalDays < 3) return 0.7f;
            if (age.TotalDays < 7) return 0.5f;
            return 0.3f;
        }
        
        private float CalculateKeywordBonus(EventTemplate template, ProcessedNewsItem news)
        {
            if (template.TriggerKeywords == null || template.TriggerKeywords.Count == 0)
                return 1.0f;
            
            string combinedText = $"{news.Headline} {news.Summary}".ToLower();
            int matchCount = 0;
            
            foreach (var keyword in template.TriggerKeywords)
            {
                if (combinedText.Contains(keyword.ToLower()))
                {
                    matchCount++;
                }
            }
            
            float matchRatio = (float)matchCount / template.TriggerKeywords.Count;
            
            // Bonus multiplier: 1.0 to 1.5 based on keyword matches
            return 1.0f + (matchRatio * 0.5f);
        }
        
        private EventTemplate GetFallbackTemplate(PoliticalCategory category)
        {
            // Return a generic template for the category
            var templates = EventTemplateLibrary.GetTemplatesForCategory(category);
            return templates?.FirstOrDefault();
        }
        
        private MatchedTemplate BuildMatchedTemplate(EventTemplate template, ProcessedNewsItem news, float score)
        {
            var matched = new MatchedTemplate
            {
                Template = template,
                MatchScore = score,
                SourceNews = news,
                ResolvedVariables = new Dictionary<string, string>()
            };
            
            // Resolve all variables
            if (template.Variables != null)
            {
                foreach (var mapping in template.Variables)
                {
                    string value = ResolveVariable(mapping, news);
                    matched.ResolvedVariables[mapping.VariableName] = value;
                }
            }
            
            // Add player context variables
            matched.ResolvedVariables["player_office"] = _gameState.GetPlayerOfficeTitle();
            matched.ResolvedVariables["player_name"] = _gameState.GetPlayerName();
            
            // Generate final text
            matched.GeneratedHeadline = ApplyVariables(template.HeadlineTemplate, matched.ResolvedVariables);
            matched.GeneratedDescription = ApplyVariables(template.DescriptionTemplate, matched.ResolvedVariables);
            matched.GeneratedContext = ApplyVariables(template.ContextTemplate, matched.ResolvedVariables);
            
            return matched;
        }
        
        private string ResolveVariable(VariableMapping mapping, ProcessedNewsItem news)
        {
            try
            {
                string value = ResolveVariablePath(mapping.SourcePath, news);
                
                if (string.IsNullOrEmpty(value))
                {
                    return mapping.FallbackValue ?? "";
                }
                
                return value;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[TemplateMatcher] Failed to resolve variable {mapping.VariableName}: {e.Message}");
                return mapping.FallbackValue ?? "";
            }
        }
        
        private string ResolveVariablePath(string path, ProcessedNewsItem news)
        {
            // Parse paths like "entities.people[0].name" or "content.summary.effect"
            var parts = path.Split('.');
            
            if (parts.Length == 0) return null;
            
            switch (parts[0])
            {
                case "entities":
                    return ResolveEntityPath(parts.Skip(1).ToArray(), news.Entities);
                
                case "content":
                    return ResolveContentPath(parts.Skip(1).ToArray(), news);
                
                case "context":
                    return ResolveContextPath(parts.Skip(1).ToArray(), news);
                
                default:
                    return null;
            }
        }
        
        private string ResolveEntityPath(string[] pathParts, ExtractedEntities entities)
        {
            if (pathParts.Length == 0) return null;
            
            // Parse entity type and optional filter: "people[type=govt][0]"
            string entityPart = pathParts[0];
            string entityType = entityPart.Split('[')[0];
            
            List<ExtractedEntity> entityList = entityType switch
            {
                "people" => entities.People,
                "organizations" => entities.Organizations,
                "locations" => entities.Locations,
                "legislation" => entities.Legislation,
                "events" => entities.Events,
                _ => null
            };
            
            if (entityList == null || entityList.Count == 0) return null;
            
            // Apply filters if present
            var filtered = ApplyEntityFilters(entityList, entityPart);
            if (filtered.Count == 0) return null;
            
            // Get index if specified
            int index = ExtractIndex(entityPart);
            if (index >= filtered.Count) index = 0;
            
            var entity = filtered[index];
            
            // Get property if more path parts
            if (pathParts.Length > 1)
            {
                return pathParts[1] switch
                {
                    "name" => entity.Name,
                    "type" => entity.Type.ToString(),
                    "role" => entity.Role,
                    "subtype" => entity.SubType,
                    _ => entity.Name
                };
            }
            
            return entity.Name;
        }
        
        private List<ExtractedEntity> ApplyEntityFilters(List<ExtractedEntity> entities, string filterString)
        {
            // Parse filters like "[type=govt]"
            var filtered = entities.ToList();
            
            var filterMatches = System.Text.RegularExpressions.Regex.Matches(filterString, @"\[(\w+)=(\w+)\]");
            foreach (System.Text.RegularExpressions.Match match in filterMatches)
            {
                string filterKey = match.Groups[1].Value;
                string filterValue = match.Groups[2].Value;
                
                filtered = filtered.Where(e => 
                    (filterKey == "type" && e.SubType?.ToLower() == filterValue.ToLower()) ||
                    (filterKey == "role" && e.Role?.ToLower() == filterValue.ToLower())
                ).ToList();
            }
            
            return filtered;
        }
        
        private int ExtractIndex(string pathPart)
        {
            var match = System.Text.RegularExpressions.Regex.Match(pathPart, @"\[(\d+)\]");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int index))
            {
                return index;
            }
            return 0;
        }
        
        private string ResolveContentPath(string[] pathParts, ProcessedNewsItem news)
        {
            // Content paths reference the news item's textual content
            // In a full implementation, this would parse the news content
            // For now, return relevant parts based on path
            
            if (pathParts.Length == 0) return null;
            
            return pathParts[0] switch
            {
                "headline" => news.Headline,
                "summary" => news.Summary,
                "sentiment" => news.OverallSentiment > 0 ? "positive" : "negative",
                "controversy" => news.ControversyScore > 0.5f ? "highly controversial" : "developing",
                _ => null
            };
        }
        
        private string ResolveContextPath(string[] pathParts, ProcessedNewsItem news)
        {
            // Context paths reference game-state-aware content
            if (pathParts.Length == 0) return null;
            
            return pathParts[0] switch
            {
                "party_position" => _gameState.GetPlayerPartyPosition(news.PrimaryCategory),
                "expected_action" => DetermineExpectedAction(news),
                _ => null
            };
        }
        
        private string DetermineExpectedAction(ProcessedNewsItem news)
        {
            // Determine what action is expected based on player alignment and issue
            var alignment = _gameState.GetPlayerAlignment();
            
            if (alignment.GoodEvil < -30)
                return "champion this cause";
            else if (alignment.GoodEvil > 30)
                return "carefully consider the implications";
            else
                return "take a position on this issue";
        }
        
        private string ApplyVariables(string template, Dictionary<string, string> variables)
        {
            if (string.IsNullOrEmpty(template)) return template;
            
            string result = template;
            foreach (var kvp in variables)
            {
                result = result.Replace($"{{{kvp.Key}}}", kvp.Value);
            }
            
            // Clean up any unresolved variables
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\{[^}]+\}", "");
            
            return result.Trim();
        }
    }
    
    #endregion
    
    #region Variable Injector
    
    /// <summary>
    /// Handles variable resolution and text generation for templates.
    /// </summary>
    public class VariableInjector
    {
        private readonly INewsTranslationCoreGameStateProvider _gameState;
        
        public VariableInjector(INewsTranslationCoreGameStateProvider gameState)
        {
            _gameState = gameState;
        }
        
        /// <summary>
        /// Inject all variables into a matched template.
        /// </summary>
        public void InjectVariables(MatchedTemplate matched)
        {
            // Add additional context variables
            EnrichWithGameContext(matched);
            
            // Regenerate text with full context
            matched.GeneratedHeadline = ProcessText(matched.Template.HeadlineTemplate, matched);
            matched.GeneratedDescription = ProcessText(matched.Template.DescriptionTemplate, matched);
            matched.GeneratedContext = ProcessText(matched.Template.ContextTemplate, matched);
        }
        
        private void EnrichWithGameContext(MatchedTemplate matched)
        {
            var vars = matched.ResolvedVariables;
            
            // Player context
            vars["player_office"] = _gameState.GetPlayerOfficeTitle();
            vars["player_name"] = _gameState.GetPlayerName();
            vars["player_party"] = _gameState.GetPlayerParty();
            vars["player_state"] = _gameState.GetPlayerState();
            
            // Time context
            vars["current_turn"] = _gameState.GetCurrentTurn().ToString();
            vars["election_distance"] = _gameState.GetTurnsUntilElection().ToString();
            
            // Political context
            vars["approval_rating"] = $"{_gameState.GetPlayerApproval():F0}%";
        }
        
        private string ProcessText(string template, MatchedTemplate matched)
        {
            if (string.IsNullOrEmpty(template)) return "";
            
            string result = template;
            
            // Apply variables
            foreach (var kvp in matched.ResolvedVariables)
            {
                result = result.Replace($"{{{kvp.Key}}}", kvp.Value);
            }
            
            // Apply conditional blocks
            result = ProcessConditionals(result, matched);
            
            // Clean up
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\{[^}]+\}", "");
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\s+", " ");
            
            return result.Trim();
        }
        
        private string ProcessConditionals(string text, MatchedTemplate matched)
        {
            // Process {{if condition}}...{{endif}} blocks
            // Simplified implementation
            return text;
        }
    }
    
    #endregion
    
    #region Event Factory
    
    /// <summary>
    /// Creates game events from matched templates.
    /// </summary>
    public class NewsEventFactory
    {
        private readonly INewsTranslationCoreGameStateProvider _gameState;
        private readonly ResponseOptionBuilder _responseBuilder;
        private int _eventCounter = 0;
        
        public NewsEventFactory(INewsTranslationCoreGameStateProvider gameState)
        {
            _gameState = gameState;
            _responseBuilder = new ResponseOptionBuilder(gameState);
        }
        
        /// <summary>
        /// Create a game event from a matched template.
        /// </summary>
        public NewsGameEvent CreateEvent(MatchedTemplate matched)
        {
            var eventType = DetermineEventType(matched);
            
            return eventType switch
            {
                TemplateEventType.Crisis => CreateCrisisEvent(matched),
                TemplateEventType.Opportunity => CreateOpportunityEvent(matched),
                TemplateEventType.ScandalTrigger => CreateScandalTriggerEvent(matched),
                _ => CreatePolicyPressureEvent(matched)
            };
        }
        
        private TemplateEventType DetermineEventType(MatchedTemplate matched)
        {
            var news = matched.SourceNews;
            var template = matched.Template;
            
            // Override template default based on news characteristics
            if (news.ImpactScore >= 7.0f && news.ControversyScore >= 0.6f)
                return TemplateEventType.Crisis;
            
            if (news.OverallSentiment > 30f && news.PoliticalRelevance >= 0.5f)
                return TemplateEventType.Opportunity;
            
            if (HasScandalKeywords(news))
                return TemplateEventType.ScandalTrigger;
            
            return template.DefaultEventType;
        }
        
        private bool HasScandalKeywords(ProcessedNewsItem news)
        {
            var scandalKeywords = new[] { "scandal", "indictment", "investigation", "misconduct", "corruption", "fraud" };
            string text = $"{news.Headline} {news.Summary}".ToLower();
            return scandalKeywords.Any(k => text.Contains(k));
        }
        
        private NewsGameEvent CreateBaseEvent(MatchedTemplate matched, TemplateEventType type)
        {
            int currentTurn = _gameState.GetCurrentTurn();
            
            return new NewsGameEvent
            {
                EventId = GenerateEventId(),
                Type = type,
                Urgency = matched.Template.DefaultUrgency,
                SourceNewsId = matched.SourceNews.SourceId,
                
                Headline = matched.GeneratedHeadline,
                Description = matched.GeneratedDescription,
                ContextText = matched.GeneratedContext,
                RealWorldNote = $"Based on real news from {matched.SourceNews.PublishedAt:MMM d, yyyy}",
                
                CreatedTurn = currentTurn,
                ExpirationTurn = currentTurn + GetExpirationOffset(matched.Template.DefaultUrgency),
                
                ResponseOptions = new List<ResponseOption>(),
                ResponseHistory = new List<PlayerResponse>(),
                
                Category = matched.Template.Category,
                Tags = matched.Template.Tags,
                IsChaosModeContent = matched.Template.ChaosModeOnly
            };
        }
        
        private NewsGameEvent CreateCrisisEvent(MatchedTemplate matched)
        {
            var evt = CreateBaseEvent(matched, TemplateEventType.Crisis);
            
            // Set deadline based on urgency
            evt.DeadlineTurn = evt.CreatedTurn + GetDeadlineOffset(evt.Urgency);
            
            // Generate escalation stages
            evt.EscalationStages = GenerateEscalationStages(matched);
            evt.CurrentStage = 0;
            
            // Generate response options
            evt.ResponseOptions = _responseBuilder.BuildCrisisResponses(matched);
            
            // Calculate scaled effects
            evt.Effects = CalculateScaledEffects(matched);
            
            return evt;
        }
        
        private NewsGameEvent CreatePolicyPressureEvent(MatchedTemplate matched)
        {
            var evt = CreateBaseEvent(matched, TemplateEventType.PolicyPressure);
            
            // Policy pressure has optional deadline
            if (matched.SourceNews.TemporalClassification == TemporalClass.Breaking)
            {
                evt.DeadlineTurn = evt.CreatedTurn + 3;
            }
            
            // Generate response options
            evt.ResponseOptions = _responseBuilder.BuildPolicyPressureResponses(matched);
            
            // Calculate scaled effects
            evt.Effects = CalculateScaledEffects(matched);
            
            return evt;
        }
        
        private NewsGameEvent CreateOpportunityEvent(MatchedTemplate matched)
        {
            var evt = CreateBaseEvent(matched, TemplateEventType.Opportunity);
            
            // Opportunities have expiration windows
            evt.DeadlineTurn = evt.CreatedTurn + 5;
            
            // Generate response options
            evt.ResponseOptions = _responseBuilder.BuildOpportunityResponses(matched);
            
            // Calculate scaled effects (opportunities have positive base effects)
            evt.Effects = CalculateScaledEffects(matched, positiveOnly: true);
            
            return evt;
        }
        
        private NewsGameEvent CreateScandalTriggerEvent(MatchedTemplate matched)
        {
            var evt = CreateBaseEvent(matched, TemplateEventType.ScandalTrigger);
            evt.Urgency = UrgencyLevel.Breaking;
            evt.DeadlineTurn = evt.CreatedTurn + 2;
            
            // Generate response options
            evt.ResponseOptions = _responseBuilder.BuildScandalResponses(matched);
            
            // Calculate scaled effects (scandals have higher negative potential)
            evt.Effects = CalculateScaledEffects(matched, highStakes: true);
            
            return evt;
        }
        
        private string GenerateEventId()
        {
            _eventCounter++;
            return $"NEWS_EVT_{DateTime.UtcNow:yyyyMMdd}_{_eventCounter:D4}";
        }
        
        private int GetExpirationOffset(UrgencyLevel urgency)
        {
            return urgency switch
            {
                UrgencyLevel.Breaking => 5,
                UrgencyLevel.Urgent => 10,
                UrgencyLevel.Developing => 20,
                UrgencyLevel.Informational => 30,
                _ => 15
            };
        }
        
        private int GetDeadlineOffset(UrgencyLevel urgency)
        {
            return urgency switch
            {
                UrgencyLevel.Breaking => 2,
                UrgencyLevel.Urgent => 5,
                UrgencyLevel.Developing => 10,
                _ => 15
            };
        }
        
        private List<EscalationStage> GenerateEscalationStages(MatchedTemplate matched)
        {
            return new List<EscalationStage>
            {
                new EscalationStage
                {
                    StageNumber = 0,
                    StageName = "Initial Response",
                    Description = "The situation is developing. Early action could shape outcomes.",
                    TurnsUntilNext = 3,
                    SeverityMultiplier = 1.0f
                },
                new EscalationStage
                {
                    StageNumber = 1,
                    StageName = "Escalation",
                    Description = "The crisis is intensifying. Delayed response carries greater risk.",
                    TurnsUntilNext = 3,
                    SeverityMultiplier = 1.5f
                },
                new EscalationStage
                {
                    StageNumber = 2,
                    StageName = "Critical",
                    Description = "The crisis has reached a critical point. Immediate action required.",
                    TurnsUntilNext = 2,
                    SeverityMultiplier = 2.0f
                },
                new EscalationStage
                {
                    StageNumber = 3,
                    StageName = "Resolution",
                    Description = "The window for effective response is closing.",
                    TurnsUntilNext = 0,
                    SeverityMultiplier = 2.5f
                }
            };
        }
        
        private ScaledEffects CalculateScaledEffects(MatchedTemplate matched, bool positiveOnly = false, bool highStakes = false)
        {
            var template = matched.Template;
            var baseEffects = template.Effects;
            int playerTier = _gameState.GetPlayerOfficeTier();
            float tierMultiplier = template.Scaling.GetMultiplier(playerTier);
            float impactMultiplier = matched.SourceNews.ImpactScore / 5f; // Normalize to ~1.0
            float combinedMultiplier = tierMultiplier * impactMultiplier;
            
            if (highStakes) combinedMultiplier *= 1.5f;
            
            var scaled = new ScaledEffects();
            
            if (baseEffects.TrustDelta != null)
            {
                float value = baseEffects.TrustDelta.Roll() * combinedMultiplier;
                scaled.TrustDelta = positiveOnly ? Mathf.Max(0, value) : value;
            }
            
            if (baseEffects.CapitalDelta != null)
            {
                float value = baseEffects.CapitalDelta.Roll() * combinedMultiplier;
                scaled.CapitalDelta = positiveOnly ? Mathf.Max(0, value) : value;
            }
            
            if (baseEffects.FundsDelta != null)
            {
                float value = baseEffects.FundsDelta.Roll() * combinedMultiplier;
                scaled.FundsDelta = positiveOnly ? Mathf.Max(0, value) : value;
            }
            
            if (baseEffects.MediaDelta != null)
            {
                scaled.MediaDelta = baseEffects.MediaDelta.Roll() * combinedMultiplier;
            }
            
            if (baseEffects.PartyLoyaltyDelta != null)
            {
                float value = baseEffects.PartyLoyaltyDelta.Roll() * combinedMultiplier;
                scaled.PartyLoyaltyDelta = positiveOnly ? Mathf.Max(0, value) : value;
            }
            
            if (baseEffects.VoterBlocEffects != null)
            {
                foreach (var kvp in baseEffects.VoterBlocEffects)
                {
                    float value = kvp.Value.Roll() * combinedMultiplier;
                    scaled.VoterBlocDeltas[kvp.Key] = positiveOnly ? Mathf.Max(0, value) : value;
                }
            }
            
            return scaled;
        }
    }
    
    #endregion
    
    #region Response Option Builder
    
    /// <summary>
    /// Builds context-appropriate response options for events.
    /// </summary>
    public class ResponseOptionBuilder
    {
        private readonly INewsTranslationCoreGameStateProvider _gameState;
        private int _optionCounter = 0;
        
        public ResponseOptionBuilder(INewsTranslationCoreGameStateProvider gameState)
        {
            _gameState = gameState;
        }
        
        public List<ResponseOption> BuildCrisisResponses(MatchedTemplate matched)
        {
            var options = new List<ResponseOption>();
            
            // Standard responses for any crisis
            options.Add(CreateAddressImmediatelyOption(matched));
            options.Add(CreateDelegateToStaffOption(matched));
            options.Add(CreateDivertAttentionOption(matched));
            options.Add(CreateIgnoreOption(matched));
            
            // Alignment-specific options
            if (_gameState.GetPlayerAlignment().GoodEvil < 0)
            {
                options.Add(CreateTransparentResponseOption(matched));
            }
            
            if (_gameState.GetPlayerAlignment().GoodEvil > 0)
            {
                options.Add(CreateDeflectBlameOption(matched));
            }
            
            // Chaos mode option
            if (_gameState.IsChaosModeEnabled())
            {
                options.Add(CreateChaosOption(matched, "Embrace the Chaos", 
                    "Turn this crisis into a spectacle that dominates the news cycle."));
            }
            
            return options;
        }
        
        public List<ResponseOption> BuildPolicyPressureResponses(MatchedTemplate matched)
        {
            var options = new List<ResponseOption>();
            
            // Support/Oppose/Hedge/Ignore
            options.Add(CreateSupportOption(matched));
            options.Add(CreateOpposeOption(matched));
            options.Add(CreateHedgeOption(matched));
            options.Add(CreateNoCommentOption(matched));
            
            // Chaos mode
            if (_gameState.IsChaosModeEnabled())
            {
                options.Add(CreateChaosOption(matched, "Blame Your Opponent",
                    "Deflect the entire issue by making wild accusations."));
            }
            
            return options;
        }
        
        public List<ResponseOption> BuildOpportunityResponses(MatchedTemplate matched)
        {
            var options = new List<ResponseOption>();
            
            options.Add(CreateSeizeOpportunityOption(matched));
            options.Add(CreateCautiousApproachOption(matched));
            options.Add(CreatePassOption(matched));
            
            if (_gameState.IsChaosModeEnabled())
            {
                options.Add(CreateChaosOption(matched, "Go All In",
                    "Commit everything to capitalize on this moment."));
            }
            
            return options;
        }
        
        public List<ResponseOption> BuildScandalResponses(MatchedTemplate matched)
        {
            var options = new List<ResponseOption>();
            
            options.Add(CreateDenyOption(matched));
            options.Add(CreateApologizeOption(matched));
            options.Add(CreateCounterAttackOption(matched));
            options.Add(CreateDistractOption(matched));
            options.Add(CreateSacrificeSubordinateOption(matched));
            
            if (_gameState.IsChaosModeEnabled())
            {
                options.Add(CreateChaosOption(matched, "Double Down",
                    "Make the scandal your brand. Turn infamy into fame."));
            }
            
            return options;
        }
        
        #region Response Option Creators
        
        private ResponseOption CreateSupportOption(MatchedTemplate matched)
        {
            return new ResponseOption
            {
                OptionId = GenerateOptionId(),
                Label = "Support",
                Description = "Publicly support this position.",
                StatementTemplate = "I fully support this initiative and believe it will benefit our community.",
                AlignmentEffect = new AlignmentShift { LawChaos = -2, GoodEvil = -5 },
                ResourceEffects = new Dictionary<string, float>
                {
                    { "political_capital", -3f },
                    { "media_influence", 5f }
                },
                VoterBlocEffects = GetCategoryVoterEffects(matched.Template.Category, true),
                SuccessProbability = 0.8f,
                IsRisky = false
            };
        }
        
        private ResponseOption CreateOpposeOption(MatchedTemplate matched)
        {
            return new ResponseOption
            {
                OptionId = GenerateOptionId(),
                Label = "Oppose",
                Description = "Publicly oppose this position.",
                StatementTemplate = "I cannot support this initiative as it will harm our constituents.",
                AlignmentEffect = new AlignmentShift { LawChaos = 2, GoodEvil = 5 },
                ResourceEffects = new Dictionary<string, float>
                {
                    { "political_capital", -3f },
                    { "media_influence", 5f }
                },
                VoterBlocEffects = GetCategoryVoterEffects(matched.Template.Category, false),
                SuccessProbability = 0.8f,
                IsRisky = false
            };
        }
        
        private ResponseOption CreateHedgeOption(MatchedTemplate matched)
        {
            return new ResponseOption
            {
                OptionId = GenerateOptionId(),
                Label = "Hedge",
                Description = "Take a nuanced position that avoids commitment.",
                StatementTemplate = "This is a complex issue that requires careful consideration of all perspectives.",
                AlignmentEffect = new AlignmentShift { LawChaos = 0, GoodEvil = 0 },
                ResourceEffects = new Dictionary<string, float>
                {
                    { "trust", -3f },
                    { "media_influence", -5f }
                },
                VoterBlocEffects = new Dictionary<string, float>
                {
                    { "Moderates", 3f },
                    { "Base", -5f }
                },
                SuccessProbability = 0.9f,
                IsRisky = false
            };
        }
        
        private ResponseOption CreateNoCommentOption(MatchedTemplate matched)
        {
            return new ResponseOption
            {
                OptionId = GenerateOptionId(),
                Label = "No Comment",
                Description = "Decline to comment on this issue.",
                StatementTemplate = "",
                AlignmentEffect = new AlignmentShift { LawChaos = 3, GoodEvil = 0 },
                ResourceEffects = new Dictionary<string, float>
                {
                    { "media_influence", -10f }
                },
                SuccessProbability = 0.7f,
                IsRisky = true
            };
        }
        
        private ResponseOption CreateAddressImmediatelyOption(MatchedTemplate matched)
        {
            return new ResponseOption
            {
                OptionId = GenerateOptionId(),
                Label = "Address Immediately",
                Description = "Take direct action to address the crisis head-on.",
                StatementTemplate = "I am taking immediate action to address this critical situation.",
                AlignmentEffect = new AlignmentShift { LawChaos = -5, GoodEvil = -5 },
                ResourceEffects = new Dictionary<string, float>
                {
                    { "political_capital", -5f },
                    { "trust", 10f },
                    { "media_influence", 10f }
                },
                SuccessProbability = 0.6f,
                SuccessOutcome = new ResponseOutcome
                {
                    Description = "Your decisive action is praised.",
                    ResourceModifiers = new Dictionary<string, float> { { "trust", 15f } }
                },
                FailureOutcome = new ResponseOutcome
                {
                    Description = "Despite your efforts, the situation worsens.",
                    ResourceModifiers = new Dictionary<string, float> { { "trust", -10f } }
                },
                IsRisky = true
            };
        }
        
        private ResponseOption CreateDelegateToStaffOption(MatchedTemplate matched)
        {
            return new ResponseOption
            {
                OptionId = GenerateOptionId(),
                Label = "Delegate to Staff",
                Description = "Let your staff handle the response while you monitor.",
                StatementTemplate = "My team is working around the clock to address this situation.",
                ResourceEffects = new Dictionary<string, float>
                {
                    { "political_capital", -2f }
                },
                SuccessProbability = 0.75f,
                IsRisky = false
            };
        }
        
        private ResponseOption CreateDivertAttentionOption(MatchedTemplate matched)
        {
            return new ResponseOption
            {
                OptionId = GenerateOptionId(),
                Label = "Divert Attention",
                Description = "Create a distraction to shift focus from the crisis.",
                AlignmentEffect = new AlignmentShift { LawChaos = 5, GoodEvil = 5 },
                ResourceEffects = new Dictionary<string, float>
                {
                    { "political_capital", -8f },
                    { "media_influence", -5f }
                },
                SuccessProbability = 0.5f,
                IsRisky = true
            };
        }
        
        private ResponseOption CreateIgnoreOption(MatchedTemplate matched)
        {
            return new ResponseOption
            {
                OptionId = GenerateOptionId(),
                Label = "Ignore",
                Description = "Hope the crisis resolves itself without your involvement.",
                AlignmentEffect = new AlignmentShift { LawChaos = 3, GoodEvil = 5 },
                ResourceEffects = new Dictionary<string, float>
                {
                    { "trust", -5f }
                },
                SuccessProbability = 0.3f,
                IsRisky = true,
                FailureOutcome = new ResponseOutcome
                {
                    Description = "Your inaction is criticized as the crisis escalates.",
                    ResourceModifiers = new Dictionary<string, float> { { "trust", -20f } }
                }
            };
        }
        
        private ResponseOption CreateTransparentResponseOption(MatchedTemplate matched)
        {
            return new ResponseOption
            {
                OptionId = GenerateOptionId(),
                Label = "Full Transparency",
                Description = "Be completely open about the situation and your response.",
                RequiredAlignment = new AlignmentRange { MinGoodEvil = -100, MaxGoodEvil = 0 },
                AlignmentEffect = new AlignmentShift { LawChaos = -5, GoodEvil = -10 },
                ResourceEffects = new Dictionary<string, float>
                {
                    { "trust", 15f },
                    { "media_influence", 10f }
                },
                SuccessProbability = 0.7f,
                IsAlignmentLocked = true
            };
        }
        
        private ResponseOption CreateDeflectBlameOption(MatchedTemplate matched)
        {
            return new ResponseOption
            {
                OptionId = GenerateOptionId(),
                Label = "Deflect Blame",
                Description = "Shift responsibility to others.",
                RequiredAlignment = new AlignmentRange { MinGoodEvil = 0, MaxGoodEvil = 100 },
                AlignmentEffect = new AlignmentShift { LawChaos = 5, GoodEvil = 10 },
                ResourceEffects = new Dictionary<string, float>
                {
                    { "party_loyalty", -10f }
                },
                SuccessProbability = 0.6f,
                IsAlignmentLocked = true,
                IsRisky = true
            };
        }
        
        private ResponseOption CreateSeizeOpportunityOption(MatchedTemplate matched)
        {
            return new ResponseOption
            {
                OptionId = GenerateOptionId(),
                Label = "Seize the Opportunity",
                Description = "Commit resources to capitalize on this moment.",
                ResourceEffects = new Dictionary<string, float>
                {
                    { "political_capital", -5f },
                    { "campaign_funds", -5000f },
                    { "trust", 10f },
                    { "media_influence", 15f }
                },
                SuccessProbability = 0.7f,
                IsRisky = false
            };
        }
        
        private ResponseOption CreateCautiousApproachOption(MatchedTemplate matched)
        {
            return new ResponseOption
            {
                OptionId = GenerateOptionId(),
                Label = "Cautious Approach",
                Description = "Test the waters before fully committing.",
                ResourceEffects = new Dictionary<string, float>
                {
                    { "political_capital", -2f },
                    { "trust", 5f }
                },
                SuccessProbability = 0.85f,
                IsRisky = false
            };
        }
        
        private ResponseOption CreatePassOption(MatchedTemplate matched)
        {
            return new ResponseOption
            {
                OptionId = GenerateOptionId(),
                Label = "Pass",
                Description = "Let this opportunity go.",
                SuccessProbability = 1.0f,
                IsRisky = false
            };
        }
        
        private ResponseOption CreateDenyOption(MatchedTemplate matched)
        {
            return new ResponseOption
            {
                OptionId = GenerateOptionId(),
                Label = "Deny",
                Description = "Categorically deny the allegations.",
                StatementTemplate = "These allegations are completely false and without merit.",
                AlignmentEffect = new AlignmentShift { LawChaos = 5, GoodEvil = 5 },
                ResourceEffects = new Dictionary<string, float>
                {
                    { "media_influence", 10f }
                },
                SuccessProbability = 0.4f,
                IsRisky = true,
                SuccessOutcome = new ResponseOutcome
                {
                    Description = "Your denial is accepted and the story fades.",
                    ResourceModifiers = new Dictionary<string, float> { { "trust", 5f } }
                },
                FailureOutcome = new ResponseOutcome
                {
                    Description = "Evidence contradicts your denial, making things worse.",
                    ResourceModifiers = new Dictionary<string, float> { { "trust", -25f } }
                }
            };
        }
        
        private ResponseOption CreateApologizeOption(MatchedTemplate matched)
        {
            return new ResponseOption
            {
                OptionId = GenerateOptionId(),
                Label = "Apologize",
                Description = "Take responsibility and apologize.",
                StatementTemplate = "I take full responsibility and sincerely apologize.",
                AlignmentEffect = new AlignmentShift { LawChaos = -5, GoodEvil = -5 },
                ResourceEffects = new Dictionary<string, float>
                {
                    { "trust", -5f },
                    { "media_influence", -5f }
                },
                SuccessProbability = 0.75f,
                IsRisky = false
            };
        }
        
        private ResponseOption CreateCounterAttackOption(MatchedTemplate matched)
        {
            return new ResponseOption
            {
                OptionId = GenerateOptionId(),
                Label = "Counter-Attack",
                Description = "Attack those making the allegations.",
                AlignmentEffect = new AlignmentShift { LawChaos = 10, GoodEvil = 10 },
                ResourceEffects = new Dictionary<string, float>
                {
                    { "political_capital", -8f },
                    { "media_influence", 15f }
                },
                SuccessProbability = 0.45f,
                IsRisky = true
            };
        }
        
        private ResponseOption CreateDistractOption(MatchedTemplate matched)
        {
            return new ResponseOption
            {
                OptionId = GenerateOptionId(),
                Label = "Distract",
                Description = "Create a different story to dominate headlines.",
                AlignmentEffect = new AlignmentShift { LawChaos = 8, GoodEvil = 5 },
                ResourceEffects = new Dictionary<string, float>
                {
                    { "campaign_funds", -10000f },
                    { "media_influence", -10f }
                },
                SuccessProbability = 0.55f,
                IsRisky = true
            };
        }
        
        private ResponseOption CreateSacrificeSubordinateOption(MatchedTemplate matched)
        {
            return new ResponseOption
            {
                OptionId = GenerateOptionId(),
                Label = "Sacrifice Subordinate",
                Description = "Let a staff member take the fall.",
                AlignmentEffect = new AlignmentShift { LawChaos = 5, GoodEvil = 15 },
                ResourceEffects = new Dictionary<string, float>
                {
                    { "trust", 5f },
                    { "staff_loyalty", -30f }
                },
                SuccessProbability = 0.7f,
                IsRisky = false
            };
        }
        
        private ResponseOption CreateChaosOption(MatchedTemplate matched, string label, string description)
        {
            return new ResponseOption
            {
                OptionId = GenerateOptionId(),
                Label = $"💀 {label}",
                Description = description,
                AlignmentEffect = new AlignmentShift { LawChaos = 15, GoodEvil = 10 },
                ResourceEffects = new Dictionary<string, float>
                {
                    { "media_influence", 25f },
                    { "trust", -10f }
                },
                SuccessProbability = 0.35f,
                IsRisky = true,
                ChaosModeOnly = true
            };
        }
        
        #endregion
        
        private string GenerateOptionId()
        {
            _optionCounter++;
            return $"OPT_{_optionCounter:D4}";
        }
        
        private Dictionary<string, float> GetCategoryVoterEffects(PoliticalCategory category, bool supportive)
        {
            float modifier = supportive ? 1f : -1f;
            
            return category switch
            {
                PoliticalCategory.ClimateEnvironment => new Dictionary<string, float>
                {
                    { "Youth", 15f * modifier },
                    { "Environmentalists", 20f * modifier },
                    { "Business", -10f * modifier }
                },
                PoliticalCategory.EconomicPolicy => new Dictionary<string, float>
                {
                    { "Business", 15f * modifier },
                    { "WorkingClass", 10f * modifier }
                },
                PoliticalCategory.HealthcarePolicy => new Dictionary<string, float>
                {
                    { "Seniors", 15f * modifier },
                    { "WorkingClass", 10f * modifier }
                },
                PoliticalCategory.CrimeJustice => new Dictionary<string, float>
                {
                    { "Security", 15f * modifier },
                    { "Minorities", -10f * modifier }
                },
                PoliticalCategory.Education => new Dictionary<string, float>
                {
                    { "Parents", 15f * modifier },
                    { "Youth", 10f * modifier }
                },
                _ => new Dictionary<string, float>()
            };
        }
    }
    
    #endregion
    
    #region Interfaces
    
    /// <summary>
    /// Interface for accessing game state information.
    /// Implement this based on your actual game state management.
    /// </summary>
    public interface INewsTranslationCoreGameStateProvider
    {
        int GetPlayerOfficeTier();
        string GetPlayerOfficeTitle();
        string GetPlayerName();
        string GetPlayerParty();
        string GetPlayerState();
        int GetCurrentTurn();
        int GetTurnsUntilElection();
        float GetPlayerApproval();
        PlayerAlignment GetPlayerAlignment();
        string GetPlayerPartyPosition(PoliticalCategory category);
        bool IsChaosModeEnabled();
    }
    
    [Serializable]
    public class PlayerAlignment
    {
        public int LawChaos;    // -100 (Lawful) to +100 (Chaotic)
        public int GoodEvil;   // -100 (Good) to +100 (Evil)
    }
    
    #endregion
}

