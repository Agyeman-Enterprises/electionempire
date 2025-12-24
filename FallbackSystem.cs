// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - Fallback System
// Stage 8: Procedural News Generation, Caching, and Offline Support
// ═══════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionEmpire.News.Fallback
{
    #region Enums & Configuration
    
    /// <summary>
    /// Source type for news content.
    /// </summary>
    public enum NewsSource
    {
        RealTimeAPI,        // Live news from external APIs
        CachedNews,         // Recently fetched but stored
        ProceduralGenerated,// Algorithmically generated fictional news
        HistoricalArchive,  // Past real events with contemporary framing
        CommunityContent    // User-submitted content (moderated)
    }
    
    /// <summary>
    /// Configuration for the fallback system.
    /// </summary>
    [Serializable]
    public class FallbackConfig
    {
        [Header("Cache Settings")]
        public int CacheRetentionHours = 72;
        public int MaxCachedItems = 500;
        public int MinCachedItemsForOffline = 50;
        
        [Header("Procedural Generation")]
        public float ProceduralVariety = 0.7f;      // 0-1, higher = more variety
        public int MinProceduralPerCategory = 3;
        public int MaxProceduralPerTurn = 5;
        public float RealityBlendDefault = 0.5f;    // 0 = all fictional, 1 = all real
        
        [Header("Retry Logic")]
        public int MaxRetryAttempts = 3;
        public float RetryDelaySeconds = 5f;
        public float APITimeoutSeconds = 10f;
        
        [Header("Offline Mode")]
        public bool AllowOfflinePlay = true;
        public float OfflineNewsFrequencyMultiplier = 0.8f;
        
        [Header("Content Filtering")]
        public bool FilterControversialContent = true;
        public float MinContentQualityScore = 0.5f;
    }
    
    #endregion
    
    #region Data Structures
    
    /// <summary>
    /// A cached news item ready for use.
    /// </summary>
    [Serializable]
    public class CachedNewsItem
    {
        public string CacheId;
        public string OriginalNewsId;
        public NewsSource Source;
        
        public string Headline;
        public string Summary;
        public string Category;
        public List<string> Keywords;
        public List<string> Entities;
        
        public float RelevanceScore;
        public float QualityScore;
        public float ControversyScore;
        
        public DateTime CachedAt;
        public DateTime ExpiresAt;
        public int TimesUsed;
        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
        
        public CachedNewsItem()
        {
            Keywords = new List<string>();
            Entities = new List<string>();
        }
    }
    
    /// <summary>
    /// Template for procedural news generation.
    /// </summary>
    [Serializable]
    public class ProceduralNewsTemplate
    {
        public string TemplateId;
        public string Category;
        public string HeadlinePattern;      // "Senator {name} Proposes {policy} Amid {context}"
        public string SummaryPattern;
        public List<string> RequiredVariables;
        public List<string> OptionalVariables;
        public float BaseControversy;
        public float BaseImpact;
        public List<string> TriggerConditions;  // Game state conditions that enable this
        
        public ProceduralNewsTemplate()
        {
            RequiredVariables = new List<string>();
            OptionalVariables = new List<string>();
            TriggerConditions = new List<string>();
        }
    }
    
    /// <summary>
    /// Variable pool for procedural generation.
    /// </summary>
    [Serializable]
    public class VariablePool
    {
        public Dictionary<string, List<string>> PoliticianNames;
        public Dictionary<string, List<string>> PolicyTopics;
        public Dictionary<string, List<string>> LocationNames;
        public Dictionary<string, List<string>> OrganizationNames;
        public Dictionary<string, List<string>> ContextPhrases;
        public Dictionary<string, List<string>> ActionVerbs;
        public Dictionary<string, List<string>> ImpactDescriptions;
        
        public VariablePool()
        {
            PoliticianNames = new Dictionary<string, List<string>>();
            PolicyTopics = new Dictionary<string, List<string>>();
            LocationNames = new Dictionary<string, List<string>>();
            OrganizationNames = new Dictionary<string, List<string>>();
            ContextPhrases = new Dictionary<string, List<string>>();
            ActionVerbs = new Dictionary<string, List<string>>();
            ImpactDescriptions = new Dictionary<string, List<string>>();
        }
    }
    
    /// <summary>
    /// Fallback system status report.
    /// </summary>
    public class FallbackStatus
    {
        public bool IsAPIAvailable;
        public NewsSource CurrentPrimarySource;
        public int CachedItemCount;
        public int ProceduralTemplateCount;
        public DateTime LastAPISuccess;
        public DateTime LastCacheUpdate;
        public int ConsecutiveAPIFailures;
        public List<string> ActiveWarnings;
        
        public FallbackStatus()
        {
            ActiveWarnings = new List<string>();
        }
    }
    
    #endregion
    
    /// <summary>
    /// Manages news caching for offline play and API fallback.
    /// </summary>
    public class NewsCacheManager
    {
        private readonly FallbackConfig _config;
        private readonly Dictionary<string, CachedNewsItem> _cache;
        private readonly Dictionary<string, List<string>> _cacheByCategory;
        private readonly Queue<string> _lruQueue;  // Least recently used tracking
        
        public event Action<int> OnCacheUpdated;
        public event Action<string> OnItemExpired;
        
        public NewsCacheManager(FallbackConfig config = null)
        {
            _config = config ?? new FallbackConfig();
            _cache = new Dictionary<string, CachedNewsItem>();
            _cacheByCategory = new Dictionary<string, List<string>>();
            _lruQueue = new Queue<string>();
        }
        
        #region Cache Operations
        
        /// <summary>
        /// Add an item to the cache.
        /// </summary>
        public void CacheItem(CachedNewsItem item)
        {
            if (string.IsNullOrEmpty(item.CacheId))
            {
                item.CacheId = Guid.NewGuid().ToString();
            }
            
            item.CachedAt = DateTime.UtcNow;
            item.ExpiresAt = DateTime.UtcNow.AddHours(_config.CacheRetentionHours);
            
            // Check quality threshold
            if (item.QualityScore < _config.MinContentQualityScore)
            {
                Debug.Log($"[NewsCacheManager] Item {item.CacheId} below quality threshold, skipping");
                return;
            }
            
            // Enforce cache limits
            while (_cache.Count >= _config.MaxCachedItems)
            {
                EvictOldestItem();
            }
            
            _cache[item.CacheId] = item;
            
            // Update category index
            if (!_cacheByCategory.ContainsKey(item.Category))
            {
                _cacheByCategory[item.Category] = new List<string>();
            }
            _cacheByCategory[item.Category].Add(item.CacheId);
            
            _lruQueue.Enqueue(item.CacheId);
            
            OnCacheUpdated?.Invoke(_cache.Count);
        }
        
        /// <summary>
        /// Retrieve an item from cache.
        /// </summary>
        public CachedNewsItem GetItem(string cacheId)
        {
            if (_cache.TryGetValue(cacheId, out var item))
            {
                if (item.IsExpired)
                {
                    RemoveItem(cacheId);
                    return null;
                }
                
                item.TimesUsed++;
                return item;
            }
            
            return null;
        }
        
        /// <summary>
        /// Get items by category.
        /// </summary>
        public List<CachedNewsItem> GetItemsByCategory(string category, int maxItems = 10)
        {
            if (!_cacheByCategory.TryGetValue(category, out var cacheIds))
            {
                return new List<CachedNewsItem>();
            }
            
            return cacheIds
                .Select(id => _cache.TryGetValue(id, out var item) ? item : null)
                .Where(item => item != null && !item.IsExpired)
                .OrderByDescending(item => item.RelevanceScore)
                .Take(maxItems)
                .ToList();
        }
        
        /// <summary>
        /// Get least-used items for potential event generation.
        /// </summary>
        public List<CachedNewsItem> GetFreshItems(int count = 5)
        {
            return _cache.Values
                .Where(item => !item.IsExpired && item.TimesUsed == 0)
                .OrderByDescending(item => item.RelevanceScore)
                .Take(count)
                .ToList();
        }
        
        /// <summary>
        /// Remove an item from cache.
        /// </summary>
        public void RemoveItem(string cacheId)
        {
            if (_cache.TryGetValue(cacheId, out var item))
            {
                _cache.Remove(cacheId);
                
                if (_cacheByCategory.TryGetValue(item.Category, out var categoryList))
                {
                    categoryList.Remove(cacheId);
                }
                
                OnItemExpired?.Invoke(cacheId);
            }
        }
        
        /// <summary>
        /// Evict the oldest/least-used item.
        /// </summary>
        private void EvictOldestItem()
        {
            while (_lruQueue.Count > 0)
            {
                var candidateId = _lruQueue.Dequeue();
                if (_cache.ContainsKey(candidateId))
                {
                    RemoveItem(candidateId);
                    return;
                }
            }
            
            // Fallback: remove first item
            if (_cache.Count > 0)
            {
                RemoveItem(_cache.Keys.First());
            }
        }
        
        /// <summary>
        /// Clean expired items from cache.
        /// </summary>
        public int CleanExpiredItems()
        {
            var expiredIds = _cache.Where(kvp => kvp.Value.IsExpired)
                                   .Select(kvp => kvp.Key)
                                   .ToList();
            
            foreach (var id in expiredIds)
            {
                RemoveItem(id);
            }
            
            return expiredIds.Count;
        }
        
        #endregion
        
        #region Query Methods
        
        public int GetCacheCount() => _cache.Count;
        
        public bool HasMinimumForOffline() => _cache.Count >= _config.MinCachedItemsForOffline;
        
        public Dictionary<string, int> GetCategoryDistribution()
        {
            return _cacheByCategory.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count);
        }
        
        #endregion
        
        #region Persistence
        
        /// <summary>
        /// Export cache to JSON for persistence.
        /// </summary>
        public string ExportToJson()
        {
            var exportData = new CacheExportData
            {
                ExportedAt = DateTime.UtcNow,
                Items = _cache.Values.ToList()
            };
            
            return JsonUtility.ToJson(exportData);
        }
        
        /// <summary>
        /// Import cache from JSON.
        /// </summary>
        public void ImportFromJson(string json)
        {
            var importData = JsonUtility.FromJson<CacheExportData>(json);
            
            foreach (var item in importData.Items)
            {
                if (!item.IsExpired)
                {
                    CacheItem(item);
                }
            }
        }
        
        [Serializable]
        private class CacheExportData
        {
            public DateTime ExportedAt;
            public List<CachedNewsItem> Items;
        }
        
        #endregion
    }
    
    /// <summary>
    /// Generates procedural fictional news when real news is unavailable.
    /// </summary>
    public class ProceduralNewsGenerator
    {
        private readonly FallbackConfig _config;
        private readonly IGameStateProvider _gameState;
        private readonly Dictionary<string, List<ProceduralNewsTemplate>> _templatesByCategory;
        private readonly VariablePool _variablePool;
        private readonly System.Random _random;
        private readonly HashSet<string> _recentlyUsedTemplates;
        
        public ProceduralNewsGenerator(IGameStateProvider gameState, FallbackConfig config = null)
        {
            _gameState = gameState;
            _config = config ?? new FallbackConfig();
            _templatesByCategory = new Dictionary<string, List<ProceduralNewsTemplate>>();
            _variablePool = new VariablePool();
            _random = new System.Random();
            _recentlyUsedTemplates = new HashSet<string>();
            
            InitializeTemplates();
            InitializeVariablePools();
        }
        
        #region Initialization
        
        private void InitializeTemplates()
        {
            // Domestic Legislation Templates
            AddTemplate("DomesticLegislation", new ProceduralNewsTemplate
            {
                TemplateId = "PROC_LEG_001",
                Category = "DomesticLegislation",
                HeadlinePattern = "{legislative_body} Debates {policy_topic} Amid {political_context}",
                SummaryPattern = "Lawmakers in {location} are considering new {policy_type} legislation that would {policy_effect}. The measure has drawn {reaction_type} from {reaction_source}.",
                RequiredVariables = new List<string> { "legislative_body", "policy_topic", "political_context" },
                OptionalVariables = new List<string> { "location", "policy_type", "policy_effect", "reaction_type", "reaction_source" },
                BaseControversy = 0.5f,
                BaseImpact = 0.6f
            });
            
            AddTemplate("DomesticLegislation", new ProceduralNewsTemplate
            {
                TemplateId = "PROC_LEG_002",
                Category = "DomesticLegislation",
                HeadlinePattern = "Bipartisan {policy_topic} Bill Gains Momentum in {legislative_body}",
                SummaryPattern = "A rare bipartisan effort on {policy_topic} is advancing through {legislative_body}, with support from {supporting_groups}.",
                RequiredVariables = new List<string> { "policy_topic", "legislative_body" },
                BaseControversy = 0.3f,
                BaseImpact = 0.7f
            });
            
            // Economic Policy Templates
            AddTemplate("EconomicPolicy", new ProceduralNewsTemplate
            {
                TemplateId = "PROC_ECON_001",
                Category = "EconomicPolicy",
                HeadlinePattern = "{economic_indicator} {direction} as {economic_cause} Continues",
                SummaryPattern = "Economic data shows {economic_indicator} {direction_verb} by {percentage}%, {impact_description}. Analysts point to {economic_cause} as the primary factor.",
                RequiredVariables = new List<string> { "economic_indicator", "direction" },
                BaseControversy = 0.4f,
                BaseImpact = 0.7f
            });
            
            AddTemplate("EconomicPolicy", new ProceduralNewsTemplate
            {
                TemplateId = "PROC_ECON_002",
                Category = "EconomicPolicy",
                HeadlinePattern = "{company} Announces {economic_action} Affecting {affected_count} Workers",
                SummaryPattern = "Major employer {company} has announced plans to {economic_action}, impacting approximately {affected_count} employees in {location}.",
                RequiredVariables = new List<string> { "company", "economic_action", "affected_count" },
                BaseControversy = 0.6f,
                BaseImpact = 0.65f
            });
            
            // Political Scandal Templates
            AddTemplate("PoliticalScandal", new ProceduralNewsTemplate
            {
                TemplateId = "PROC_SCAN_001",
                Category = "PoliticalScandal",
                HeadlinePattern = "{politician_title} {politician_name} Faces Questions Over {scandal_topic}",
                SummaryPattern = "Allegations have emerged concerning {politician_title} {politician_name}'s involvement in {scandal_topic}. {reaction_source} is calling for {demanded_action}.",
                RequiredVariables = new List<string> { "politician_title", "politician_name", "scandal_topic" },
                BaseControversy = 0.8f,
                BaseImpact = 0.75f
            });
            
            // Crisis Templates
            AddTemplate("Crisis", new ProceduralNewsTemplate
            {
                TemplateId = "PROC_CRISIS_001",
                Category = "Crisis",
                HeadlinePattern = "{crisis_type} in {location} Prompts Emergency Response",
                SummaryPattern = "Officials in {location} are responding to a {crisis_type} that has {crisis_impact}. {response_agency} has deployed resources to the area.",
                RequiredVariables = new List<string> { "crisis_type", "location" },
                BaseControversy = 0.3f,
                BaseImpact = 0.9f
            });
            
            // Election Campaign Templates
            AddTemplate("ElectionCampaign", new ProceduralNewsTemplate
            {
                TemplateId = "PROC_ELEC_001",
                Category = "ElectionCampaign",
                HeadlinePattern = "New Poll Shows {poll_result} in {race_description}",
                SummaryPattern = "A recent survey indicates {poll_result} in the {race_description}. The poll, conducted by {pollster}, shows {poll_details}.",
                RequiredVariables = new List<string> { "poll_result", "race_description" },
                BaseControversy = 0.5f,
                BaseImpact = 0.6f
            });
            
            // Healthcare Templates
            AddTemplate("HealthcarePolicy", new ProceduralNewsTemplate
            {
                TemplateId = "PROC_HEALTH_001",
                Category = "HealthcarePolicy",
                HeadlinePattern = "Health Officials {health_action} Regarding {health_topic}",
                SummaryPattern = "Public health authorities have {health_action} concerning {health_topic}. The announcement comes as {health_context}.",
                RequiredVariables = new List<string> { "health_action", "health_topic" },
                BaseControversy = 0.4f,
                BaseImpact = 0.7f
            });
            
            // Add more templates for other categories...
            Debug.Log($"[ProceduralNewsGenerator] Initialized {GetTotalTemplateCount()} procedural templates");
        }
        
        private void InitializeVariablePools()
        {
            // Politician names (fictional)
            _variablePool.PoliticianNames["generic"] = new List<string>
            {
                "Senator Williams", "Representative Chen", "Governor Martinez",
                "Mayor Thompson", "Commissioner Davis", "Secretary Johnson",
                "Chairman Roberts", "Director Patel", "Attorney General Kim"
            };
            
            // Policy topics
            _variablePool.PolicyTopics["domestic"] = new List<string>
            {
                "infrastructure spending", "tax reform", "education funding",
                "healthcare access", "environmental regulations", "housing affordability",
                "criminal justice reform", "immigration policy", "social security"
            };
            
            _variablePool.PolicyTopics["economic"] = new List<string>
            {
                "interest rates", "trade policy", "job creation",
                "deficit reduction", "wage increases", "business regulations",
                "investment incentives", "unemployment benefits", "inflation control"
            };
            
            // Locations (fictional/generic)
            _variablePool.LocationNames["state"] = new List<string>
            {
                "the state capital", "the metropolitan area", "rural districts",
                "the eastern region", "the industrial corridor", "suburban communities"
            };
            
            // Context phrases
            _variablePool.ContextPhrases["political"] = new List<string>
            {
                "growing partisan tensions", "an election year",
                "shifting public opinion", "economic uncertainty",
                "calls for reform", "mounting pressure from activists"
            };
            
            // Economic indicators
            _variablePool.ContextPhrases["economic_indicator"] = new List<string>
            {
                "unemployment", "consumer confidence", "manufacturing output",
                "retail sales", "housing starts", "GDP growth"
            };
            
            // Action verbs
            _variablePool.ActionVerbs["legislative"] = new List<string>
            {
                "proposes", "advances", "blocks", "amends", "debates", "passes"
            };
            
            // Impact descriptions
            _variablePool.ImpactDescriptions["economic"] = new List<string>
            {
                "raising concerns about the economy",
                "providing relief to markets",
                "signaling potential challenges ahead",
                "offering mixed signals for investors"
            };
        }
        
        private void AddTemplate(string category, ProceduralNewsTemplate template)
        {
            if (!_templatesByCategory.ContainsKey(category))
            {
                _templatesByCategory[category] = new List<ProceduralNewsTemplate>();
            }
            _templatesByCategory[category].Add(template);
        }
        
        private int GetTotalTemplateCount()
        {
            return _templatesByCategory.Values.Sum(list => list.Count);
        }
        
        #endregion
        
        #region Generation
        
        /// <summary>
        /// Generate procedural news items for a category.
        /// </summary>
        public List<CachedNewsItem> GenerateForCategory(string category, int count = 1)
        {
            var results = new List<CachedNewsItem>();
            
            if (!_templatesByCategory.TryGetValue(category, out var templates) || templates.Count == 0)
            {
                Debug.LogWarning($"[ProceduralNewsGenerator] No templates for category: {category}");
                return results;
            }
            
            // Filter out recently used templates
            var availableTemplates = templates
                .Where(t => !_recentlyUsedTemplates.Contains(t.TemplateId))
                .ToList();
            
            if (availableTemplates.Count == 0)
            {
                // Reset if all templates used
                _recentlyUsedTemplates.Clear();
                availableTemplates = templates;
            }
            
            for (int i = 0; i < count && availableTemplates.Count > 0; i++)
            {
                var template = SelectTemplate(availableTemplates);
                var generated = GenerateFromTemplate(template);
                
                if (generated != null)
                {
                    results.Add(generated);
                    _recentlyUsedTemplates.Add(template.TemplateId);
                    availableTemplates.Remove(template);
                }
            }
            
            return results;
        }
        
        /// <summary>
        /// Generate news based on current game state.
        /// </summary>
        public List<CachedNewsItem> GenerateContextualNews(int maxItems = 3)
        {
            var results = new List<CachedNewsItem>();
            int officeTier = _gameState.GetPlayerOfficeTier();
            float approval = _gameState.GetPlayerApproval();
            int turnsUntilElection = _gameState.GetTurnsUntilElection();
            
            // Determine relevant categories based on game state
            var relevantCategories = DetermineRelevantCategories(officeTier, approval, turnsUntilElection);
            
            foreach (var category in relevantCategories.Take(maxItems))
            {
                var generated = GenerateForCategory(category, 1);
                results.AddRange(generated);
            }
            
            return results;
        }
        
        private List<string> DetermineRelevantCategories(int officeTier, float approval, int turnsUntilElection)
        {
            var categories = new List<(string Category, float Weight)>();
            
            // Base weights
            categories.Add(("DomesticLegislation", 1.0f));
            categories.Add(("EconomicPolicy", 1.0f));
            
            // Tier-based adjustments
            if (officeTier >= 3)
            {
                categories.Add(("ForeignPolicy", 0.8f));
            }
            
            // Election proximity
            if (turnsUntilElection <= 10)
            {
                categories.Add(("ElectionCampaign", 1.5f));
            }
            
            // Low approval increases scandal/crisis likelihood
            if (approval < 0.4f)
            {
                categories.Add(("PoliticalScandal", 1.2f));
                categories.Add(("Crisis", 1.3f));
            }
            
            // Always include some variety
            categories.Add(("HealthcarePolicy", 0.6f));
            categories.Add(("LocalGovernment", 0.5f));
            
            // Sort by weight and return categories
            return categories
                .OrderByDescending(c => c.Weight * (0.8f + (float)_random.NextDouble() * 0.4f))
                .Select(c => c.Category)
                .ToList();
        }
        
        private ProceduralNewsTemplate SelectTemplate(List<ProceduralNewsTemplate> templates)
        {
            // Weighted random selection based on game state relevance
            float totalWeight = templates.Sum(t => CalculateTemplateWeight(t));
            float roll = (float)_random.NextDouble() * totalWeight;
            
            float cumulative = 0;
            foreach (var template in templates)
            {
                cumulative += CalculateTemplateWeight(template);
                if (roll <= cumulative)
                {
                    return template;
                }
            }
            
            return templates[_random.Next(templates.Count)];
        }
        
        private float CalculateTemplateWeight(ProceduralNewsTemplate template)
        {
            // Could factor in game state, recent events, player preferences
            return 1.0f;
        }
        
        private CachedNewsItem GenerateFromTemplate(ProceduralNewsTemplate template)
        {
            try
            {
                var variables = ResolveVariables(template);
                
                string headline = FillPattern(template.HeadlinePattern, variables);
                string summary = FillPattern(template.SummaryPattern, variables);
                
                return new CachedNewsItem
                {
                    CacheId = Guid.NewGuid().ToString(),
                    OriginalNewsId = $"PROC_{template.TemplateId}_{DateTime.UtcNow.Ticks}",
                    Source = NewsSource.ProceduralGenerated,
                    Headline = headline,
                    Summary = summary,
                    Category = template.Category,
                    Keywords = variables.Values.Take(5).ToList(),
                    Entities = ExtractEntities(variables),
                    RelevanceScore = CalculateRelevanceScore(template),
                    QualityScore = 0.7f,  // Procedural content has fixed quality
                    ControversyScore = template.BaseControversy + (float)(_random.NextDouble() - 0.5) * 0.2f
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ProceduralNewsGenerator] Failed to generate from template {template.TemplateId}: {ex.Message}");
                return null;
            }
        }
        
        private Dictionary<string, string> ResolveVariables(ProceduralNewsTemplate template)
        {
            var variables = new Dictionary<string, string>();
            
            foreach (var varName in template.RequiredVariables)
            {
                variables[varName] = GetVariableValue(varName);
            }
            
            foreach (var varName in template.OptionalVariables)
            {
                if (_random.NextDouble() > 0.3)  // 70% chance to include optional
                {
                    variables[varName] = GetVariableValue(varName);
                }
            }
            
            return variables;
        }
        
        private string GetVariableValue(string varName)
        {
            // Map variable names to pools
            return varName switch
            {
                "legislative_body" => SelectRandom(new[] { "Congress", "the Senate", "the House", "the State Legislature" }),
                "policy_topic" => SelectFromPool(_variablePool.PolicyTopics, "domestic"),
                "political_context" => SelectFromPool(_variablePool.ContextPhrases, "political"),
                "location" => SelectFromPool(_variablePool.LocationNames, "state"),
                "politician_name" => SelectFromPool(_variablePool.PoliticianNames, "generic"),
                "politician_title" => SelectRandom(new[] { "Senator", "Representative", "Governor", "Mayor" }),
                "economic_indicator" => SelectFromPool(_variablePool.ContextPhrases, "economic_indicator"),
                "direction" => SelectRandom(new[] { "Rises", "Falls", "Holds Steady", "Fluctuates" }),
                "percentage" => _random.Next(1, 10).ToString(),
                "company" => SelectRandom(new[] { "TechCorp Industries", "United Manufacturing", "National Services Inc.", "Regional Holdings" }),
                "economic_action" => SelectRandom(new[] { "layoffs", "expansion plans", "restructuring", "wage increases" }),
                "affected_count" => (_random.Next(50, 500) * 10).ToString(),
                "scandal_topic" => SelectRandom(new[] { "financial irregularities", "ethical concerns", "campaign violations", "conflicts of interest" }),
                "crisis_type" => SelectRandom(new[] { "severe weather event", "public safety incident", "infrastructure failure", "public health concern" }),
                "poll_result" => SelectRandom(new[] { "tight race", "widening lead", "shifting momentum", "unexpected changes" }),
                "race_description" => SelectRandom(new[] { "upcoming election", "key battleground", "competitive district", "statewide race" }),
                "health_action" => SelectRandom(new[] { "issued new guidelines", "raised concerns", "announced initiatives", "updated recommendations" }),
                "health_topic" => SelectRandom(new[] { "public health preparedness", "healthcare access", "prescription costs", "mental health services" }),
                _ => $"[{varName}]"
            };
        }
        
        private string SelectRandom(string[] options)
        {
            return options[_random.Next(options.Length)];
        }
        
        private string SelectFromPool(Dictionary<string, List<string>> pool, string key)
        {
            if (pool.TryGetValue(key, out var options) && options.Count > 0)
            {
                return options[_random.Next(options.Count)];
            }
            return $"[{key}]";
        }
        
        private string FillPattern(string pattern, Dictionary<string, string> variables)
        {
            string result = pattern;
            
            foreach (var kvp in variables)
            {
                result = result.Replace($"{{{kvp.Key}}}", kvp.Value);
            }
            
            // Remove any unfilled variables
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\{[^}]+\}", "");
            
            return result.Trim();
        }
        
        private List<string> ExtractEntities(Dictionary<string, string> variables)
        {
            var entities = new List<string>();
            
            if (variables.TryGetValue("politician_name", out var name))
                entities.Add(name);
            if (variables.TryGetValue("company", out var company))
                entities.Add(company);
            if (variables.TryGetValue("location", out var location))
                entities.Add(location);
            
            return entities;
        }
        
        private float CalculateRelevanceScore(ProceduralNewsTemplate template)
        {
            float baseScore = 0.6f;
            
            // Boost based on impact
            baseScore += template.BaseImpact * 0.3f;
            
            // Add some randomness
            baseScore += (float)(_random.NextDouble() - 0.5) * 0.2f;
            
            return Mathf.Clamp01(baseScore);
        }
        
        #endregion
    }
    
    /// <summary>
    /// Orchestrates fallback behavior when real news is unavailable.
    /// </summary>
    public class FallbackOrchestrator
    {
        private readonly FallbackConfig _config;
        private readonly NewsCacheManager _cacheManager;
        private readonly ProceduralNewsGenerator _proceduralGenerator;
        
        private bool _isAPIAvailable;
        private int _consecutiveFailures;
        private DateTime _lastAPISuccess;
        private NewsSource _currentPrimarySource;
        
        public event Action<NewsSource> OnSourceChanged;
        public event Action<string> OnFallbackWarning;
        
        public FallbackOrchestrator(
            NewsCacheManager cacheManager,
            ProceduralNewsGenerator proceduralGenerator,
            FallbackConfig config = null)
        {
            _cacheManager = cacheManager;
            _proceduralGenerator = proceduralGenerator;
            _config = config ?? new FallbackConfig();
            
            _isAPIAvailable = true;
            _consecutiveFailures = 0;
            _lastAPISuccess = DateTime.UtcNow;
            _currentPrimarySource = NewsSource.RealTimeAPI;
        }
        
        #region API Status Management
        
        /// <summary>
        /// Record a successful API call.
        /// </summary>
        public void RecordAPISuccess()
        {
            _isAPIAvailable = true;
            _consecutiveFailures = 0;
            _lastAPISuccess = DateTime.UtcNow;
            
            if (_currentPrimarySource != NewsSource.RealTimeAPI)
            {
                _currentPrimarySource = NewsSource.RealTimeAPI;
                OnSourceChanged?.Invoke(_currentPrimarySource);
            }
        }
        
        /// <summary>
        /// Record an API failure.
        /// </summary>
        public void RecordAPIFailure()
        {
            _consecutiveFailures++;
            
            if (_consecutiveFailures >= _config.MaxRetryAttempts)
            {
                _isAPIAvailable = false;
                SwitchToFallbackSource();
            }
        }
        
        private void SwitchToFallbackSource()
        {
            // Priority: Cached > Procedural
            if (_cacheManager.HasMinimumForOffline())
            {
                _currentPrimarySource = NewsSource.CachedNews;
                OnFallbackWarning?.Invoke("Switched to cached news due to API unavailability");
            }
            else
            {
                _currentPrimarySource = NewsSource.ProceduralGenerated;
                OnFallbackWarning?.Invoke("Switched to procedural news - limited real-world content available");
            }
            
            OnSourceChanged?.Invoke(_currentPrimarySource);
        }
        
        #endregion
        
        #region Content Retrieval
        
        /// <summary>
        /// Get news content using the best available source.
        /// </summary>
        public List<CachedNewsItem> GetNewsContent(string category = null, int count = 5)
        {
            return _currentPrimarySource switch
            {
                NewsSource.RealTimeAPI => GetFromCacheOrGenerate(category, count),
                NewsSource.CachedNews => GetFromCache(category, count),
                NewsSource.ProceduralGenerated => GenerateProcedural(category, count),
                _ => GenerateProcedural(category, count)
            };
        }
        
        private List<CachedNewsItem> GetFromCacheOrGenerate(string category, int count)
        {
            var results = new List<CachedNewsItem>();
            
            // Try cache first
            if (category != null)
            {
                results.AddRange(_cacheManager.GetItemsByCategory(category, count));
            }
            else
            {
                results.AddRange(_cacheManager.GetFreshItems(count));
            }
            
            // Fill remaining with procedural
            if (results.Count < count)
            {
                var procedural = category != null
                    ? _proceduralGenerator.GenerateForCategory(category, count - results.Count)
                    : _proceduralGenerator.GenerateContextualNews(count - results.Count);
                
                results.AddRange(procedural);
            }
            
            return results;
        }
        
        private List<CachedNewsItem> GetFromCache(string category, int count)
        {
            if (category != null)
            {
                return _cacheManager.GetItemsByCategory(category, count);
            }
            
            return _cacheManager.GetFreshItems(count);
        }
        
        private List<CachedNewsItem> GenerateProcedural(string category, int count)
        {
            if (category != null)
            {
                return _proceduralGenerator.GenerateForCategory(category, count);
            }
            
            return _proceduralGenerator.GenerateContextualNews(count);
        }
        
        #endregion
        
        #region Status
        
        /// <summary>
        /// Get current fallback system status.
        /// </summary>
        public FallbackStatus GetStatus()
        {
            var status = new FallbackStatus
            {
                IsAPIAvailable = _isAPIAvailable,
                CurrentPrimarySource = _currentPrimarySource,
                CachedItemCount = _cacheManager.GetCacheCount(),
                LastAPISuccess = _lastAPISuccess,
                ConsecutiveAPIFailures = _consecutiveFailures
            };
            
            if (!_isAPIAvailable)
            {
                status.ActiveWarnings.Add("Real-time news unavailable");
            }
            
            if (!_cacheManager.HasMinimumForOffline())
            {
                status.ActiveWarnings.Add("Insufficient cached content for offline play");
            }
            
            return status;
        }
        
        #endregion
    }
    
    #region Interfaces
    
    /// <summary>
    /// Interface for reading game state.
    /// </summary>
    public interface IGameStateProvider
    {
        int GetPlayerOfficeTier();
        float GetPlayerApproval();
        int GetTurnsUntilElection();
    }
    
    #endregion
}
