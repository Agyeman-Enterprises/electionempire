// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - Template Matcher Algorithm
// Advanced scoring and selection system for news-to-event translation
// ═══════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.News.Templates;

namespace ElectionEmpire.News.Translation
{
    /// <summary>
    /// Configuration for template matching behavior.
    /// </summary>
    [Serializable]
    public class TemplateMatcherConfig
    {
        [Header("Scoring Weights (must sum to 1.0)")]
        [Range(0f, 1f)] public float EntityMatchWeight = 0.30f;
        [Range(0f, 1f)] public float SentimentAlignmentWeight = 0.20f;
        [Range(0f, 1f)] public float OfficeRelevanceWeight = 0.25f;
        [Range(0f, 1f)] public float ControversyFitWeight = 0.15f;
        [Range(0f, 1f)] public float RecencyBonusWeight = 0.10f;
        
        [Header("Thresholds")]
        [Range(0f, 1f)] public float MinMatchThreshold = 0.4f;
        [Range(0f, 1f)] public float KeywordBonusMax = 0.5f;
        
        [Header("Recency Settings (hours)")]
        public float BreakingNewsThreshold = 6f;
        public float RecentNewsThreshold = 24f;
        public float DevelopingNewsThreshold = 72f;
        public float OngoingNewsThreshold = 168f;
        
        [Header("Debug")]
        public bool EnableDetailedLogging = false;
    }
    
    /// <summary>
    /// Detailed scoring breakdown for debugging and analysis.
    /// </summary>
    [Serializable]
    public class TemplateScoreBreakdown
    {
        public string TemplateId;
        public float EntityMatchScore;
        public float SentimentAlignmentScore;
        public float OfficeRelevanceScore;
        public float ControversyFitScore;
        public float RecencyScore;
        public float KeywordBonus;
        public float ImpactPenalty;
        public float FinalScore;
        
        public override string ToString()
        {
            return $"[{TemplateId}] Final: {FinalScore:F3} | " +
                   $"Entity: {EntityMatchScore:F2}, Sentiment: {SentimentAlignmentScore:F2}, " +
                   $"Office: {OfficeRelevanceScore:F2}, Controversy: {ControversyFitScore:F2}, " +
                   $"Recency: {RecencyScore:F2}, Keyword: {KeywordBonus:F2}, Impact: {ImpactPenalty:F2}";
        }
    }
    
    /// <summary>
    /// Result of a template matching operation.
    /// </summary>
    [Serializable]
    public class TemplateMatchResult
    {
        public bool Success;
        public MatchedTemplate BestMatch;
        public List<TemplateScoreBreakdown> AllScores;
        public string FailureReason;
        public float ProcessingTimeMs;
        
        public TemplateMatchResult()
        {
            AllScores = new List<TemplateScoreBreakdown>();
        }
    }
    
    /// <summary>
    /// Advanced template matching system with detailed scoring and analysis.
    /// </summary>
    public class AdvancedTemplateMatcher
    {
        private readonly IGameStateProvider _gameState;
        private readonly TemplateMatcherConfig _config;
        private readonly VariableResolver _variableResolver;
        
        // Cache for performance
        private Dictionary<string, float> _keywordCache;
        private DateTime _lastCacheClear;
        private const int CACHE_LIFETIME_MINUTES = 30;
        
        public AdvancedTemplateMatcher(IGameStateProvider gameState, TemplateMatcherConfig config = null)
        {
            _gameState = gameState;
            _config = config ?? new TemplateMatcherConfig();
            _variableResolver = new VariableResolver(gameState);
            _keywordCache = new Dictionary<string, float>();
            _lastCacheClear = DateTime.UtcNow;
            
            EventTemplateLibrary.Initialize();
        }
        
        /// <summary>
        /// Find the best matching template with full scoring breakdown.
        /// </summary>
        public TemplateMatchResult Match(ProcessedNewsItem news)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var result = new TemplateMatchResult();
            
            try
            {
                // Get templates for primary category
                var templates = GetCandidateTemplates(news);
                
                if (templates.Count == 0)
                {
                    result.Success = false;
                    result.FailureReason = $"No templates found for category: {news.PrimaryCategory}";
                    return result;
                }
                
                // Score all templates
                EventTemplate bestTemplate = null;
                float bestScore = 0f;
                
                foreach (var template in templates)
                {
                    var breakdown = CalculateDetailedScore(template, news);
                    result.AllScores.Add(breakdown);
                    
                    if (breakdown.FinalScore > bestScore)
                    {
                        bestScore = breakdown.FinalScore;
                        bestTemplate = template;
                    }
                }
                
                // Sort scores for analysis
                result.AllScores = result.AllScores.OrderByDescending(s => s.FinalScore).ToList();
                
                // Check threshold
                if (bestScore < _config.MinMatchThreshold)
                {
                    if (_config.EnableDetailedLogging)
                    {
                        Debug.Log($"[TemplateMatcher] Best score {bestScore:F3} below threshold {_config.MinMatchThreshold}");
                        Debug.Log($"[TemplateMatcher] Top 3 candidates:\n" + 
                            string.Join("\n", result.AllScores.Take(3).Select(s => s.ToString())));
                    }
                    
                    // Use fallback
                    bestTemplate = GetFallbackTemplate(news.PrimaryCategory);
                    if (bestTemplate == null)
                    {
                        result.Success = false;
                        result.FailureReason = "No suitable template found and no fallback available";
                        return result;
                    }
                }
                
                // Build matched template
                result.BestMatch = BuildMatchedTemplate(bestTemplate, news, bestScore);
                result.Success = true;
                
                if (_config.EnableDetailedLogging)
                {
                    Debug.Log($"[TemplateMatcher] Selected template: {bestTemplate.TemplateId} " +
                              $"with score {bestScore:F3} for news: {news.Headline.Substring(0, Math.Min(50, news.Headline.Length))}...");
                }
            }
            catch (Exception e)
            {
                result.Success = false;
                result.FailureReason = $"Exception during matching: {e.Message}";
                Debug.LogError($"[TemplateMatcher] Error: {e}");
            }
            finally
            {
                stopwatch.Stop();
                result.ProcessingTimeMs = stopwatch.ElapsedMilliseconds;
            }
            
            return result;
        }
        
        /// <summary>
        /// Get candidate templates including secondary categories.
        /// </summary>
        private List<EventTemplate> GetCandidateTemplates(ProcessedNewsItem news)
        {
            var templates = new List<EventTemplate>();
            
            // Primary category
            templates.AddRange(EventTemplateLibrary.GetTemplatesForCategory(news.PrimaryCategory));
            
            // Add secondary category templates with lower weight consideration
            if (news.SecondaryCategories != null)
            {
                foreach (var secondary in news.SecondaryCategories.Take(2))
                {
                    var secondaryTemplates = EventTemplateLibrary.GetTemplatesForCategory(secondary);
                    templates.AddRange(secondaryTemplates);
                }
            }
            
            // Remove duplicates
            return templates.GroupBy(t => t.TemplateId).Select(g => g.First()).ToList();
        }
        
        /// <summary>
        /// Calculate detailed scoring breakdown for a template.
        /// </summary>
        private TemplateScoreBreakdown CalculateDetailedScore(EventTemplate template, ProcessedNewsItem news)
        {
            var breakdown = new TemplateScoreBreakdown
            {
                TemplateId = template.TemplateId
            };
            
            // Calculate individual components
            breakdown.EntityMatchScore = CalculateEntityMatchScore(template, news);
            breakdown.SentimentAlignmentScore = CalculateSentimentAlignmentScore(template, news);
            breakdown.OfficeRelevanceScore = CalculateOfficeRelevanceScore(template);
            breakdown.ControversyFitScore = CalculateControversyFitScore(template, news);
            breakdown.RecencyScore = CalculateRecencyScore(news);
            breakdown.KeywordBonus = CalculateKeywordBonus(template, news);
            breakdown.ImpactPenalty = CalculateImpactPenalty(template, news);
            
            // Calculate weighted total
            float baseScore = 
                breakdown.EntityMatchScore * _config.EntityMatchWeight +
                breakdown.SentimentAlignmentScore * _config.SentimentAlignmentWeight +
                breakdown.OfficeRelevanceScore * _config.OfficeRelevanceWeight +
                breakdown.ControversyFitScore * _config.ControversyFitWeight +
                breakdown.RecencyScore * _config.RecencyBonusWeight;
            
            // Apply bonuses and penalties
            breakdown.FinalScore = baseScore * breakdown.KeywordBonus * breakdown.ImpactPenalty;
            breakdown.FinalScore = Mathf.Clamp01(breakdown.FinalScore);
            
            return breakdown;
        }
        
        #region Scoring Components
        
        /// <summary>
        /// Score based on how well news entities match template requirements.
        /// </summary>
        private float CalculateEntityMatchScore(EventTemplate template, ProcessedNewsItem news)
        {
            if (template.RequiredEntities == null || template.RequiredEntities.Count == 0)
            {
                // No requirements = neutral score
                return 0.7f;
            }
            
            int totalRequired = template.RequiredEntities.Count;
            int matched = 0;
            float qualitySum = 0f;
            
            foreach (var requiredType in template.RequiredEntities)
            {
                var entities = news.Entities.GetByType(requiredType);
                
                if (entities != null && entities.Count > 0)
                {
                    matched++;
                    
                    // Quality bonus based on entity relevance
                    var bestEntity = entities.OrderByDescending(e => e.Relevance).First();
                    qualitySum += bestEntity.Relevance;
                }
            }
            
            if (matched == 0) return 0.2f;
            
            float matchRatio = (float)matched / totalRequired;
            float avgQuality = qualitySum / matched;
            
            // Combined score: 70% match ratio, 30% quality
            return matchRatio * 0.7f + avgQuality * 0.3f;
        }
        
        /// <summary>
        /// Score based on how well news sentiment matches template event type.
        /// </summary>
        private float CalculateSentimentAlignmentScore(EventTemplate template, ProcessedNewsItem news)
        {
            // Normalize sentiment to 0-1 range
            float sentimentNormalized = (news.OverallSentiment + 100f) / 200f;
            
            switch (template.DefaultEventType)
            {
                case TemplateEventType.Crisis:
                    // Crises match better with negative sentiment and high controversy
                    float crisisScore = (1f - sentimentNormalized) * 0.6f + news.ControversyScore * 0.4f;
                    return crisisScore;
                
                case TemplateEventType.Opportunity:
                    // Opportunities match positive sentiment
                    return sentimentNormalized * 0.8f + (1f - news.ControversyScore) * 0.2f;
                
                case TemplateEventType.ScandalTrigger:
                    // Scandals need negative sentiment + controversy
                    float scandalScore = (1f - sentimentNormalized) * 0.5f + news.ControversyScore * 0.5f;
                    return scandalScore;
                
                case TemplateEventType.PolicyPressure:
                    // Policy pressure is flexible but favors some controversy
                    return 0.5f + news.ControversyScore * 0.3f;
                
                default:
                    return 0.5f;
            }
        }
        
        /// <summary>
        /// Score based on how relevant the template is to player's current office.
        /// </summary>
        private float CalculateOfficeRelevanceScore(EventTemplate template)
        {
            int playerTier = _gameState.GetPlayerOfficeTier();
            float scaling = template.Scaling.GetMultiplier(playerTier);
            
            // Optimal range is 0.8 to 1.2
            // Too low (< 0.3) or too high (> 2.0) is penalized
            
            if (scaling < 0.2f)
            {
                // Very low relevance to this office tier
                return 0.2f;
            }
            else if (scaling < 0.5f)
            {
                // Low relevance
                return 0.4f + (scaling - 0.2f);
            }
            else if (scaling <= 1.5f)
            {
                // Good relevance range
                return 0.7f + (1f - Mathf.Abs(1f - scaling)) * 0.3f;
            }
            else
            {
                // High tier content for lower office - still usable but less ideal
                return 0.8f - (scaling - 1.5f) * 0.2f;
            }
        }
        
        /// <summary>
        /// Score based on how well controversy levels match.
        /// </summary>
        private float CalculateControversyFitScore(EventTemplate template, ProcessedNewsItem news)
        {
            float templateMin = template.MinControversy;
            float newsControversy = news.ControversyScore;
            
            if (newsControversy < templateMin * 0.5f)
            {
                // News not controversial enough for template
                return 0.3f + (newsControversy / templateMin) * 0.4f;
            }
            else if (newsControversy >= templateMin)
            {
                // Good fit
                return 0.8f + (1f - Mathf.Abs(newsControversy - templateMin)) * 0.2f;
            }
            else
            {
                // Close but not quite
                return 0.6f + ((newsControversy - templateMin * 0.5f) / (templateMin * 0.5f)) * 0.2f;
            }
        }
        
        /// <summary>
        /// Score based on how recent the news is.
        /// </summary>
        private float CalculateRecencyScore(ProcessedNewsItem news)
        {
            var age = DateTime.UtcNow - news.PublishedAt;
            float hours = (float)age.TotalHours;
            
            if (hours < _config.BreakingNewsThreshold)
            {
                // Breaking news - maximum score
                return 1.0f;
            }
            else if (hours < _config.RecentNewsThreshold)
            {
                // Recent - high score
                float t = (hours - _config.BreakingNewsThreshold) / 
                         (_config.RecentNewsThreshold - _config.BreakingNewsThreshold);
                return Mathf.Lerp(1.0f, 0.85f, t);
            }
            else if (hours < _config.DevelopingNewsThreshold)
            {
                // Developing - medium score
                float t = (hours - _config.RecentNewsThreshold) / 
                         (_config.DevelopingNewsThreshold - _config.RecentNewsThreshold);
                return Mathf.Lerp(0.85f, 0.6f, t);
            }
            else if (hours < _config.OngoingNewsThreshold)
            {
                // Ongoing - lower score
                float t = (hours - _config.DevelopingNewsThreshold) / 
                         (_config.OngoingNewsThreshold - _config.DevelopingNewsThreshold);
                return Mathf.Lerp(0.6f, 0.35f, t);
            }
            else
            {
                // Old news
                return 0.25f;
            }
        }
        
        /// <summary>
        /// Calculate keyword match bonus multiplier.
        /// </summary>
        private float CalculateKeywordBonus(EventTemplate template, ProcessedNewsItem news)
        {
            if (template.TriggerKeywords == null || template.TriggerKeywords.Count == 0)
            {
                return 1.0f; // No bonus or penalty
            }
            
            // Check cache
            string cacheKey = $"{template.TemplateId}_{news.SourceId}";
            ClearCacheIfNeeded();
            
            if (_keywordCache.TryGetValue(cacheKey, out float cached))
            {
                return cached;
            }
            
            // Calculate match
            string combinedText = $"{news.Headline} {news.Summary}".ToLower();
            int matchCount = 0;
            int strongMatchCount = 0;
            
            foreach (var keyword in template.TriggerKeywords)
            {
                string lowerKeyword = keyword.ToLower();
                
                if (combinedText.Contains(lowerKeyword))
                {
                    matchCount++;
                    
                    // Check if it's in the headline (stronger signal)
                    if (news.Headline.ToLower().Contains(lowerKeyword))
                    {
                        strongMatchCount++;
                    }
                }
            }
            
            float matchRatio = (float)matchCount / template.TriggerKeywords.Count;
            float strongBonus = (float)strongMatchCount / template.TriggerKeywords.Count * 0.5f;
            
            // Bonus multiplier: 1.0 to (1.0 + KeywordBonusMax)
            float bonus = 1.0f + (matchRatio * _config.KeywordBonusMax) + strongBonus * 0.2f;
            
            _keywordCache[cacheKey] = bonus;
            return bonus;
        }
        
        /// <summary>
        /// Calculate penalty for impact score mismatch.
        /// </summary>
        private float CalculateImpactPenalty(EventTemplate template, ProcessedNewsItem news)
        {
            if (news.ImpactScore >= template.MinImpactScore)
            {
                // No penalty
                return 1.0f;
            }
            
            // Penalty proportional to how far below minimum
            float deficit = template.MinImpactScore - news.ImpactScore;
            float penaltyFactor = deficit / template.MinImpactScore;
            
            // Penalty ranges from 1.0 (no penalty) to 0.5 (50% penalty)
            return 1.0f - (penaltyFactor * 0.5f);
        }
        
        #endregion
        
        #region Template Building
        
        /// <summary>
        /// Build a matched template with all variables resolved.
        /// </summary>
        private MatchedTemplate BuildMatchedTemplate(EventTemplate template, ProcessedNewsItem news, float score)
        {
            var matched = new MatchedTemplate
            {
                Template = template,
                MatchScore = score,
                SourceNews = news,
                ResolvedVariables = new Dictionary<string, string>()
            };
            
            // Resolve template variables
            _variableResolver.ResolveAllVariables(matched);
            
            // Add game context variables
            AddGameContextVariables(matched);
            
            // Generate final text
            matched.GeneratedHeadline = ApplyVariables(template.HeadlineTemplate, matched.ResolvedVariables);
            matched.GeneratedDescription = ApplyVariables(template.DescriptionTemplate, matched.ResolvedVariables);
            matched.GeneratedContext = ApplyVariables(template.ContextTemplate, matched.ResolvedVariables);
            
            return matched;
        }
        
        /// <summary>
        /// Add game state context to variables.
        /// </summary>
        private void AddGameContextVariables(MatchedTemplate matched)
        {
            var vars = matched.ResolvedVariables;
            
            vars["player_office"] = _gameState.GetPlayerOfficeTitle();
            vars["player_name"] = _gameState.GetPlayerName();
            vars["player_party"] = _gameState.GetPlayerParty();
            vars["player_state"] = _gameState.GetPlayerState();
            vars["current_turn"] = _gameState.GetCurrentTurn().ToString();
            vars["approval"] = $"{_gameState.GetPlayerApproval():F0}%";
        }
        
        /// <summary>
        /// Apply variable substitution to template text.
        /// </summary>
        private string ApplyVariables(string template, Dictionary<string, string> variables)
        {
            if (string.IsNullOrEmpty(template)) return "";
            
            string result = template;
            
            foreach (var kvp in variables)
            {
                if (!string.IsNullOrEmpty(kvp.Value))
                {
                    result = result.Replace($"{{{kvp.Key}}}", kvp.Value);
                }
            }
            
            // Clean up unresolved variables
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\{[^}]+\}", "");
            
            // Clean up extra whitespace
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\s+", " ");
            
            return result.Trim();
        }
        
        /// <summary>
        /// Get fallback template for a category.
        /// </summary>
        private EventTemplate GetFallbackTemplate(PoliticalCategory category)
        {
            var templates = EventTemplateLibrary.GetTemplatesForCategory(category);
            
            // Find the most generic template (lowest min requirements)
            return templates?
                .OrderBy(t => t.MinImpactScore)
                .ThenBy(t => t.MinControversy)
                .FirstOrDefault();
        }
        
        private void ClearCacheIfNeeded()
        {
            if ((DateTime.UtcNow - _lastCacheClear).TotalMinutes > CACHE_LIFETIME_MINUTES)
            {
                _keywordCache.Clear();
                _lastCacheClear = DateTime.UtcNow;
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// Handles variable resolution from news items to template variables.
    /// </summary>
    public class VariableResolver
    {
        private readonly IGameStateProvider _gameState;
        
        public VariableResolver(IGameStateProvider gameState)
        {
            _gameState = gameState;
        }
        
        /// <summary>
        /// Resolve all variables in a matched template.
        /// </summary>
        public void ResolveAllVariables(MatchedTemplate matched)
        {
            if (matched.Template.Variables == null) return;
            
            foreach (var mapping in matched.Template.Variables)
            {
                string value = ResolveVariable(mapping, matched.SourceNews);
                matched.ResolvedVariables[mapping.VariableName] = value;
            }
        }
        
        /// <summary>
        /// Resolve a single variable mapping.
        /// </summary>
        public string ResolveVariable(VariableMapping mapping, ProcessedNewsItem news)
        {
            try
            {
                string value = ResolvePath(mapping.SourcePath, news);
                
                if (string.IsNullOrEmpty(value))
                {
                    if (mapping.Required)
                    {
                        Debug.LogWarning($"[VariableResolver] Required variable '{mapping.VariableName}' not found");
                    }
                    return mapping.FallbackValue ?? "";
                }
                
                return value;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[VariableResolver] Error resolving '{mapping.VariableName}': {e.Message}");
                return mapping.FallbackValue ?? "";
            }
        }
        
        /// <summary>
        /// Resolve a variable path against news data.
        /// </summary>
        private string ResolvePath(string path, ProcessedNewsItem news)
        {
            if (string.IsNullOrEmpty(path)) return null;
            
            var segments = path.Split('.');
            if (segments.Length == 0) return null;
            
            return segments[0] switch
            {
                "entities" => ResolveEntityPath(segments.Skip(1).ToArray(), news.Entities),
                "content" => ResolveContentPath(segments.Skip(1).ToArray(), news),
                "sentiment" => ResolveSentimentPath(segments.Skip(1).ToArray(), news),
                "context" => ResolveContextPath(segments.Skip(1).ToArray(), news),
                _ => null
            };
        }
        
        /// <summary>
        /// Resolve entity paths like "entities.people[type=politician][0].name"
        /// </summary>
        private string ResolveEntityPath(string[] segments, ExtractedEntities entities)
        {
            if (segments.Length == 0) return null;
            
            // Parse entity type and filters
            string entitySpec = segments[0];
            var parseResult = ParseEntitySpec(entitySpec);
            
            List<ExtractedEntity> entityList = parseResult.EntityType switch
            {
                "people" => entities.People,
                "organizations" => entities.Organizations,
                "locations" => entities.Locations,
                "legislation" => entities.Legislation,
                "events" => entities.Events,
                "party" => entities.Organizations?.Where(e => e.SubType == "party").ToList(),
                _ => null
            };
            
            if (entityList == null || entityList.Count == 0) return null;
            
            // Apply filters
            var filtered = ApplyFilters(entityList, parseResult.Filters);
            if (filtered.Count == 0) return null;
            
            // Get by index
            int index = Mathf.Clamp(parseResult.Index, 0, filtered.Count - 1);
            var entity = filtered[index];
            
            // Get property
            if (segments.Length > 1)
            {
                return GetEntityProperty(entity, segments[1]);
            }
            
            return entity.Name;
        }
        
        private (string EntityType, Dictionary<string, string> Filters, int Index) ParseEntitySpec(string spec)
        {
            string entityType = spec;
            var filters = new Dictionary<string, string>();
            int index = 0;
            
            // Extract type name before first bracket
            int bracketIndex = spec.IndexOf('[');
            if (bracketIndex > 0)
            {
                entityType = spec.Substring(0, bracketIndex);
            }
            
            // Extract filters [key=value]
            var filterMatches = System.Text.RegularExpressions.Regex.Matches(spec, @"\[(\w+)=(\w+)\]");
            foreach (System.Text.RegularExpressions.Match match in filterMatches)
            {
                filters[match.Groups[1].Value] = match.Groups[2].Value;
            }
            
            // Extract index [0], [1], etc.
            var indexMatch = System.Text.RegularExpressions.Regex.Match(spec, @"\[(\d+)\]");
            if (indexMatch.Success)
            {
                int.TryParse(indexMatch.Groups[1].Value, out index);
            }
            
            return (entityType, filters, index);
        }
        
        private List<ExtractedEntity> ApplyFilters(List<ExtractedEntity> entities, Dictionary<string, string> filters)
        {
            var result = entities.ToList();
            
            foreach (var filter in filters)
            {
                result = result.Where(e =>
                {
                    return filter.Key.ToLower() switch
                    {
                        "type" => e.SubType?.ToLower() == filter.Value.ToLower(),
                        "role" => e.Role?.ToLower() == filter.Value.ToLower(),
                        _ => true
                    };
                }).ToList();
            }
            
            return result;
        }
        
        private string GetEntityProperty(ExtractedEntity entity, string property)
        {
            return property.ToLower() switch
            {
                "name" => entity.Name,
                "type" => entity.Type.ToString(),
                "subtype" => entity.SubType,
                "role" => entity.Role,
                _ => entity.Attributes?.GetValueOrDefault(property) ?? entity.Name
            };
        }
        
        /// <summary>
        /// Resolve content paths from news text.
        /// </summary>
        private string ResolveContentPath(string[] segments, ProcessedNewsItem news)
        {
            if (segments.Length == 0) return null;
            
            // Basic content properties
            return segments[0].ToLower() switch
            {
                "headline" => news.Headline,
                "summary" => news.Summary,
                "source" => news.SourceId,
                "url" => news.SourceUrl,
                _ => ExtractContentFromText(segments, news)
            };
        }
        
        private string ExtractContentFromText(string[] segments, ProcessedNewsItem news)
        {
            // This would use NLP in a full implementation
            // For now, return summary-based content
            string path = string.Join(".", segments);
            
            if (path.Contains("effect") || path.Contains("impact"))
            {
                return SummarizeEffect(news);
            }
            
            if (path.Contains("cause") || path.Contains("reason"))
            {
                return "recent developments";
            }
            
            return null;
        }
        
        private string SummarizeEffect(ProcessedNewsItem news)
        {
            // Simple heuristic - extract action verbs from summary
            var actionWords = new[] { "will", "would", "could", "may", "affect", "impact", "change", "reform" };
            
            var sentences = news.Summary.Split(new[] { '.', '!' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var sentence in sentences)
            {
                if (actionWords.Any(w => sentence.ToLower().Contains(w)))
                {
                    return sentence.Trim();
                }
            }
            
            return "have significant implications";
        }
        
        /// <summary>
        /// Resolve sentiment-related paths.
        /// </summary>
        private string ResolveSentimentPath(string[] segments, ProcessedNewsItem news)
        {
            if (segments.Length == 0) return null;
            
            return segments[0].ToLower() switch
            {
                "overall" => news.OverallSentiment > 20 ? "positive" : 
                             news.OverallSentiment < -20 ? "negative" : "mixed",
                "controversy" => news.ControversyScore > 0.7f ? "highly controversial" :
                                news.ControversyScore > 0.4f ? "controversial" : "developing",
                "partisan" => GetPartisanDescription(news),
                _ => null
            };
        }
        
        private string GetPartisanDescription(ProcessedNewsItem news)
        {
            var reactions = news.PartisanReactions;
            if (reactions == null) return "mixed reactions";
            
            if (Mathf.Abs(reactions.Left - reactions.Right) > 40)
            {
                return "sharply divided along partisan lines";
            }
            else if (reactions.Center > reactions.Left && reactions.Center > reactions.Right)
            {
                return "moderate support across parties";
            }
            
            return "varied reactions";
        }
        
        /// <summary>
        /// Resolve context paths using game state.
        /// </summary>
        private string ResolveContextPath(string[] segments, ProcessedNewsItem news)
        {
            if (segments.Length == 0) return null;
            
            return segments[0].ToLower() switch
            {
                "party_position" => _gameState.GetPlayerPartyPosition(news.PrimaryCategory),
                "party_stance" => GetPartyStanceText(news),
                "expected_action" => GetExpectedAction(news),
                "player_relevance" => GetPlayerRelevanceText(news),
                _ => null
            };
        }
        
        private string GetPartyStanceText(ProcessedNewsItem news)
        {
            string party = _gameState.GetPlayerParty();
            string position = _gameState.GetPlayerPartyPosition(news.PrimaryCategory);
            
            if (!string.IsNullOrEmpty(position))
            {
                return $"The {party} party {position}";
            }
            
            return "";
        }
        
        private string GetExpectedAction(ProcessedNewsItem news)
        {
            var alignment = _gameState.GetPlayerAlignment();
            
            if (alignment.GoodEvil < -30)
            {
                return "take a principled stance";
            }
            else if (alignment.GoodEvil > 30)
            {
                return "consider the political implications";
            }
            else if (alignment.LawChaos < -30)
            {
                return "follow established procedures";
            }
            else if (alignment.LawChaos > 30)
            {
                return "shake up the status quo";
            }
            
            return "respond to this development";
        }
        
        private string GetPlayerRelevanceText(ProcessedNewsItem news)
        {
            int tier = _gameState.GetPlayerOfficeTier();
            string office = _gameState.GetPlayerOfficeTitle();
            
            return tier switch
            {
                1 => $"As {office}, this may seem distant but could affect local policy",
                2 => $"As {office}, you'll likely face questions about this",
                3 => $"As {office}, your position on this matters to voters",
                4 => $"As {office}, you're expected to lead on this issue",
                5 => $"As {office}, the nation looks to you for direction",
                _ => $"As {office}, constituents expect your response"
            };
        }
    }
    
    /// <summary>
    /// Pipeline manager that orchestrates the full news-to-event flow.
    /// </summary>
    public class NewsTranslationPipeline
    {
        private readonly AdvancedTemplateMatcher _matcher;
        private readonly VariableInjector _injector;
        private readonly NewsEventFactory _factory;
        private readonly IGameStateProvider _gameState;
        
        // Statistics
        private int _processedCount;
        private int _successCount;
        private float _avgProcessingTimeMs;
        
        public NewsTranslationPipeline(IGameStateProvider gameState, TemplateMatcherConfig config = null)
        {
            _gameState = gameState;
            _matcher = new AdvancedTemplateMatcher(gameState, config);
            _injector = new VariableInjector(gameState);
            _factory = new NewsEventFactory(gameState);
        }
        
        /// <summary>
        /// Process a news item through the full translation pipeline.
        /// </summary>
        public NewsGameEvent Process(ProcessedNewsItem news)
        {
            _processedCount++;
            
            // Step 1: Match to template
            var matchResult = _matcher.Match(news);
            
            if (!matchResult.Success)
            {
                Debug.LogWarning($"[Pipeline] Failed to match template: {matchResult.FailureReason}");
                return null;
            }
            
            // Step 2: Inject additional variables
            _injector.InjectVariables(matchResult.BestMatch);
            
            // Step 3: Create game event
            var gameEvent = _factory.CreateEvent(matchResult.BestMatch);
            
            // Update stats
            _successCount++;
            _avgProcessingTimeMs = (_avgProcessingTimeMs * (_successCount - 1) + matchResult.ProcessingTimeMs) / _successCount;
            
            Debug.Log($"[Pipeline] Created event {gameEvent.EventId} ({gameEvent.Type}) " +
                     $"from template {matchResult.BestMatch.Template.TemplateId} " +
                     $"in {matchResult.ProcessingTimeMs:F1}ms");
            
            return gameEvent;
        }
        
        /// <summary>
        /// Process multiple news items.
        /// </summary>
        public List<NewsGameEvent> ProcessBatch(IEnumerable<ProcessedNewsItem> newsItems)
        {
            var events = new List<NewsGameEvent>();
            
            foreach (var news in newsItems)
            {
                var evt = Process(news);
                if (evt != null)
                {
                    events.Add(evt);
                }
            }
            
            return events;
        }
        
        /// <summary>
        /// Get pipeline statistics.
        /// </summary>
        public (int processed, int success, float avgTimeMs) GetStats()
        {
            return (_processedCount, _successCount, _avgProcessingTimeMs);
        }
        
        /// <summary>
        /// Reset statistics.
        /// </summary>
        public void ResetStats()
        {
            _processedCount = 0;
            _successCount = 0;
            _avgProcessingTimeMs = 0;
        }
    }
}

