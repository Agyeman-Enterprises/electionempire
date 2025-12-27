using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.News.Fallback;
using ElectionEmpire.News.Translation;

namespace ElectionEmpire.News
{
    /// <summary>
    /// Fallback system for when news APIs are unavailable
    /// Now uses advanced Fallback system with caching and procedural generation
    /// </summary>
    public class FallbackNewsSystem
    {
        private List<NewsArticle> cachedNews;
        private NewsProcessor processor;
        
        // Advanced fallback system components
        private NewsCacheManager cacheManager;
        private ProceduralNewsGenerator proceduralGenerator;
        private FallbackOrchestrator orchestrator;
        private IFallbackGameStateProvider gameStateProvider;
        
        public void Initialize()
        {
            processor = new NewsProcessor();
            processor.Initialize();
            
            // Initialize advanced fallback system if game state available
            var gameManager = UnityEngine.Object.FindFirstObjectByType<Core.GameManager>();
            if (gameManager != null && gameManager.CurrentPlayer != null)
            {
                // Create game state provider adapter
                gameStateProvider = new FallbackGameStateProvider(gameManager);
                
                var config = new FallbackConfig();
                cacheManager = new NewsCacheManager(config);
                proceduralGenerator = new ProceduralNewsGenerator(gameStateProvider, config);
                orchestrator = new FallbackOrchestrator(cacheManager, proceduralGenerator, config);
            }
        }
        
        /// <summary>
        /// Initialize with game state provider (called from NewsEventManager)
        /// </summary>
        public void Initialize(IGameStateProvider gameState)
        {
            processor = new NewsProcessor();
            processor.Initialize();

            // Adapt IGameStateProvider to IFallbackGameStateProvider
            gameStateProvider = new GameStateProviderAdapter(gameState);
            var config = new FallbackConfig();
            cacheManager = new NewsCacheManager(config);
            proceduralGenerator = new ProceduralNewsGenerator(gameStateProvider, config);
            orchestrator = new FallbackOrchestrator(cacheManager, proceduralGenerator, config);
        }

        /// <summary>
        /// Adapter to convert IGameStateProvider to IFallbackGameStateProvider
        /// </summary>
        private class GameStateProviderAdapter : IFallbackGameStateProvider
        {
            private readonly IGameStateProvider _source;

            public GameStateProviderAdapter(IGameStateProvider source)
            {
                _source = source;
            }

            public int GetPlayerOfficeTier() => _source.GetPlayerOfficeTier();
            public float GetPlayerApproval() => _source.GetPlayerApproval();
            public int GetTurnsUntilElection() => _source.GetTurnsUntilElection();
        }
        
        /// <summary>
        /// Generate procedural news when APIs unavailable
        /// Uses advanced procedural generator if available, otherwise falls back to simple generation
        /// </summary>
        public List<NewsArticle> GenerateProceduralNews(int count, DateTime currentDate)
        {
            // Use advanced procedural generator if available
            if (proceduralGenerator != null)
            {
                var cachedItems = proceduralGenerator.GenerateContextualNews(count);
                return ConvertCachedItemsToArticles(cachedItems, currentDate);
            }
            
            // Fallback to simple generation
            var articles = new List<NewsArticle>();
            for (int i = 0; i < count; i++)
            {
                var article = GenerateSingleArticle(currentDate);
                articles.Add(article);
            }
            return articles;
        }
        
        private List<NewsArticle> ConvertCachedItemsToArticles(List<CachedNewsItem> items, DateTime date)
        {
            var articles = new List<NewsArticle>();
            
            foreach (var item in items)
            {
                articles.Add(new NewsArticle
                {
                    Title = item.Headline,
                    Description = item.Summary,
                    Content = item.Summary,
                    URL = "",
                    PublishedDate = date,
                    Source = item.Source.ToString(),
                    SourceName = item.Source == NewsSource.ProceduralGenerated ? "Game Generated" : "Cached News"
                });
            }
            
            return articles;
        }
        
        private NewsArticle GenerateSingleArticle(DateTime date)
        {
            // Select random category
            EventType[] categories = {
                EventType.Election, EventType.Legislation, EventType.PolicyAnnouncement,
                EventType.Economic, EventType.Crisis, EventType.SocialUnrest
            };
            
            EventType category = categories[UnityEngine.Random.Range(0, categories.Length)];
            
            // Generate title and description
            string title = GenerateTitle(category);
            string description = GenerateDescription(category);
            
            return new NewsArticle
            {
                Title = title,
                Description = description,
                Content = description,
                URL = "",
                PublishedDate = date,
                Source = "Procedural News",
                SourceName = "Game Generated"
            };
        }
        
        private string GenerateTitle(EventType category)
        {
            switch (category)
            {
                case EventType.Election:
                    return new[] {
                        "New Polling Data Released",
                        "Election Campaign Intensifies",
                        "Voter Turnout Concerns Rise",
                        "Candidate Makes Major Announcement"
                    }[UnityEngine.Random.Range(0, 4)];
                
                case EventType.Legislation:
                    return new[] {
                        "Major Bill Advances in Congress",
                        "Legislation Stalled in Committee",
                        "New Policy Proposal Announced",
                        "Lawmakers Debate Key Issue"
                    }[UnityEngine.Random.Range(0, 4)];
                
                case EventType.Crisis:
                    return new[] {
                        "Crisis Situation Develops",
                        "Emergency Response Required",
                        "National Issue Demands Attention",
                        "Urgent Situation Unfolds"
                    }[UnityEngine.Random.Range(0, 4)];
                
                case EventType.Economic:
                    return new[] {
                        "Economic Indicators Shift",
                        "Market Changes Affect Policy",
                        "Economic News Impacts Campaign",
                        "Financial Developments Emerge"
                    }[UnityEngine.Random.Range(0, 4)];
                
                default:
                    return "Political Development Occurs";
            }
        }
        
        private string GenerateDescription(EventType category)
        {
            switch (category)
            {
                case EventType.Election:
                    return "Recent developments in the election campaign have shifted the political landscape. " +
                           "Candidates are adjusting their strategies in response.";
                
                case EventType.Legislation:
                    return "A significant piece of legislation is making its way through the political process. " +
                           "Lawmakers are taking positions that will affect voters.";
                
                case EventType.Crisis:
                    return "A crisis situation has emerged that requires political leadership. " +
                           "How officials respond will be closely watched.";
                
                case EventType.Economic:
                    return "Economic developments are affecting the political climate. " +
                           "Voters are paying attention to how candidates address these issues.";
                
                default:
                    return "Political events continue to unfold, creating opportunities and challenges for candidates.";
            }
        }
        
        /// <summary>
        /// Fill content gaps with cached news
        /// Uses advanced cache manager if available
        /// </summary>
        public List<NewsArticle> FillWithCachedNews(int neededCount)
        {
            // Use advanced cache manager if available
            if (cacheManager != null)
            {
                var cachedItems = cacheManager.GetFreshItems(neededCount);
                if (cachedItems.Count > 0)
                {
                    return ConvertCachedItemsToArticles(cachedItems, DateTime.Now);
                }
            }
            
            // Fallback to legacy cache
            if (cachedNews == null || cachedNews.Count == 0)
            {
                // Load from PlayerPrefs
                LoadCachedNews();
            }
            
            if (cachedNews == null || cachedNews.Count == 0)
            {
                // No cache - generate procedural
                return GenerateProceduralNews(neededCount, DateTime.Now);
            }
            
            // Return cached news (oldest first)
            return cachedNews
                .OrderBy(n => n.PublishedDate)
                .Take(neededCount)
                .ToList();
        }
        
        /// <summary>
        /// Cache a processed news item for later use
        /// </summary>
        public void CacheNewsItem(ProcessedNews processedNews)
        {
            if (cacheManager == null) return;
            
            var cachedItem = new CachedNewsItem
            {
                OriginalNewsId = processedNews.OriginalArticle?.Title?.GetHashCode().ToString() ?? Guid.NewGuid().ToString(),
                Source = NewsSource.CachedNews,
                Headline = processedNews.OriginalArticle?.Title ?? "Unknown",
                Summary = processedNews.OriginalArticle?.Description ?? "",
                Category = processedNews.EventType.ToString(),
                Keywords = processedNews.Topics ?? new List<string>(),
                Entities = processedNews.Entities?.Select(e => e.Name).ToList() ?? new List<string>(),
                RelevanceScore = processedNews.PoliticalRelevance / 100f,
                QualityScore = 0.8f, // Assume processed news is good quality
                ControversyScore = processedNews.ControversyScore / 100f
            };
            
            cacheManager.CacheItem(cachedItem);
        }
        
        /// <summary>
        /// Record API success/failure for fallback orchestrator
        /// </summary>
        public void RecordAPISuccess()
        {
            orchestrator?.RecordAPISuccess();
        }
        
        public void RecordAPIFailure()
        {
            orchestrator?.RecordAPIFailure();
        }
        
        /// <summary>
        /// Get fallback status
        /// </summary>
        public FallbackStatus GetStatus()
        {
            return orchestrator?.GetStatus() ?? new FallbackStatus
            {
                IsAPIAvailable = true,
                CurrentPrimarySource = NewsSource.RealTimeAPI,
                CachedItemCount = cachedNews?.Count ?? 0
            };
        }
        
        private void LoadCachedNews()
        {
            string json = PlayerPrefs.GetString("NewsCache", "");
            
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var cache = JsonUtility.FromJson<NewsCache>(json);
                    cachedNews = cache.Articles ?? new List<NewsArticle>();
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to load cached news: {e.Message}");
                    cachedNews = new List<NewsArticle>();
                }
            }
            else
            {
                cachedNews = new List<NewsArticle>();
            }
        }
        
        /// <summary>
        /// Blend real and procedural news
        /// </summary>
        public List<NewsArticle> BlendNews(List<NewsArticle> realNews, float blendRatio)
        {
            var blended = new List<NewsArticle>();
            
            int realCount = (int)(realNews.Count * (1f - blendRatio));
            int proceduralCount = realNews.Count - realCount;
            
            // Add real news
            blended.AddRange(realNews.Take(realCount));
            
            // Add procedural news
            var procedural = GenerateProceduralNews(proceduralCount, DateTime.Now);
            blended.AddRange(procedural);
            
            // Shuffle
            blended = blended.OrderBy(x => UnityEngine.Random.value).ToList();
            
            return blended;
        }
    }
}

