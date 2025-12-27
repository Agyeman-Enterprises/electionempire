using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.World;
using ElectionEmpire.Gameplay;
using ElectionEmpire.News.Translation;
using ElectionEmpire.Core;
using ElectionEmpire.News;

namespace ElectionEmpire.News
{
    /// <summary>
    /// Manages real-world news integration into the game
    /// </summary>
    public class NewsEventManager : MonoBehaviour
    {
        private NewsAPIConnector apiConnector;
        private NewsProcessor processor;
        private NewsTranslationPipeline translationPipeline;
        private NewsCycleManager cycleManager;
        private PlayerResponseSystem responseSystem;
        private FallbackNewsSystem fallbackSystem;
        private NewsSettings settings;
        private GameStateProvider gameStateProvider;
        
        private ElectionEmpire.Gameplay.GameState gameState;
        private PlayerState player;
        private ResourceManager resourceManager;
        
        private List<ProcessedNews> currentNews;
        private List<NewsEvent> activeEvents;
        private List<NewsGameEvent> gameEvents;
        private float lastFetchTime;
        private float fetchInterval = 3600f; // 1 hour in seconds
        
        public void Initialize(ElectionEmpire.Gameplay.GameState gameState, PlayerState player, ResourceManager resourceManager)
        {
            this.gameState = gameState;
            this.player = player;
            this.resourceManager = resourceManager;
            
            // Initialize game state provider
            gameStateProvider = gameObject.AddComponent<GameStateProvider>();
            gameStateProvider.Initialize(gameState, player);
            
            apiConnector = gameObject.AddComponent<NewsAPIConnector>();
            apiConnector.Initialize();
            
            processor = new NewsProcessor();
            processor.Initialize();
            
            // Initialize advanced translation pipeline
            var config = new TemplateMatcherConfig
            {
                EnableDetailedLogging = false
            };
            translationPipeline = new NewsTranslationPipeline(gameStateProvider, config);
            
            cycleManager = new NewsCycleManager();
            
            responseSystem = new PlayerResponseSystem();
            responseSystem.Initialize(player, resourceManager);
            
            fallbackSystem = new FallbackNewsSystem();
            fallbackSystem.Initialize(gameStateProvider);
            
            settings = new NewsSettings();
            settings.Load();
            
            currentNews = new List<ProcessedNews>();
            activeEvents = new List<NewsEvent>();
            gameEvents = new List<NewsGameEvent>();
            
            // Load cached news on start
            LoadCachedNews();
        }
        
        void Update()
        {
            // Fetch new news periodically (adjusted by frequency setting)
            float adjustedInterval = fetchInterval / settings.NewsFrequency;
            if (Time.time - lastFetchTime > adjustedInterval)
            {
                FetchAndProcessNews();
                lastFetchTime = Time.time;
            }
            
            // Update news cycles
            if (cycleManager != null)
            {
                cycleManager.UpdateCycles(Time.deltaTime);
            }
            
            // Update active events
            UpdateActiveEvents();
            
            // Remove expired game events
            RemoveExpiredEvents();
        }
        
        private async void FetchAndProcessNews()
        {
            try
            {
                List<NewsArticle> articles;
                
                // Try to fetch real news
                try
                {
                    articles = await apiConnector.FetchLatestNews(settings.MaxNewsPerDay);
                    fallbackSystem.RecordAPISuccess();
                }
                catch
                {
                    // Fallback to cached or procedural
                    fallbackSystem.RecordAPIFailure();
                    articles = fallbackSystem.FillWithCachedNews(settings.MaxNewsPerDay);
                }
                
                // Blend with procedural if needed
                if (settings.RealityBlend < 1.0f)
                {
                    articles = fallbackSystem.BlendNews(articles, settings.RealityBlend);
                }
                
                // Process each article
                foreach (var article in articles)
                {
                    var processed = processor.ProcessArticle(article);
                    
                    // Filter by category preferences
                    if (settings.IgnoredCategories.Contains(processed.EventType))
                        continue;
                    
                    if (settings.PreferredCategories.Count > 0 && 
                        !settings.PreferredCategories.Contains(processed.EventType))
                        continue;
                    
                    // Only keep politically relevant news
                    if (processed.PoliticalRelevance > 30f)
                    {
                        currentNews.Add(processed);
                        
                        // Cache for offline use
                        fallbackSystem.CacheNewsItem(processed);
                        
                        // Create news cycle
                        cycleManager.CreateCycle(processed);
                        
                        // Create game event if relevant enough
                        if (processed.PoliticalRelevance > 60f && processed.ControversyScore > 40f)
                        {
                            CreateGameEvent(processed);
                        }
                    }
                }
                
                // Keep only recent news (last 7 days)
                currentNews = currentNews
                    .Where(n => (DateTime.Now - n.ProcessedDate).TotalDays < 7)
                    .OrderByDescending(n => n.ProcessedDate)
                    .Take(50)
                    .ToList();
                
                Debug.Log($"Fetched and processed {articles.Count} news articles");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to fetch news: {e.Message}");
                
                // Use fallback
                var fallbackArticles = fallbackSystem.GenerateProceduralNews(settings.MaxNewsPerDay, DateTime.Now);
                foreach (var article in fallbackArticles)
                {
                    var processed = processor.ProcessArticle(article);
                    currentNews.Add(processed);
                }
            }
        }
        
        private void CreateGameEvent(ProcessedNews news)
        {
            // Convert to ProcessedNewsItem for new translation system
            var newsItem = NewsAdapter.ConvertToProcessedNewsItem(news);
            
            // Use advanced translation pipeline
            var gameEvent = translationPipeline.Process(newsItem);
            
            if (gameEvent != null)
            {
                gameEvents.Add(gameEvent);
                
                // Notify UI if available
                var newsUI = FindFirstObjectByType<UI.NewsUI>();
                if (newsUI != null)
                {
                    // UI will display the event
                }
                
                Debug.Log($"[NewsEventManager] Created game event: {gameEvent.Headline}");
            }
        }
        
        private void RemoveExpiredEvents()
        {
            int currentTurn = gameStateProvider?.GetCurrentTurn() ?? 0;
            gameEvents.RemoveAll(e => 
                e.ExpirationTurn <= currentTurn || e.PlayerResponded);
        }
        
        private void LoadCachedNews()
        {
            var cachedArticles = apiConnector.LoadCachedNews();
            
            foreach (var article in cachedArticles)
            {
                var processed = processor.ProcessArticle(article);
                if (processed.PoliticalRelevance > 30f)
                {
                    currentNews.Add(processed);
                }
            }
            
            Debug.Log($"Loaded {currentNews.Count} cached news articles");
        }
        
        private void CreateNewsEvent(ProcessedNews news)
        {
            var newsEvent = new NewsEvent
            {
                ID = System.Guid.NewGuid().ToString(),
                ProcessedNews = news,
                CreatedDate = DateTime.Now,
                ImpactApplied = false
            };
            
            // Determine game impact
            CalculateGameImpact(newsEvent);
            
            activeEvents.Add(newsEvent);
            
            // Notify UI
            Debug.Log($"News event created: {news.OriginalArticle.Title}");
        }
        
        private void CalculateGameImpact(NewsEvent newsEvent)
        {
            var news = newsEvent.ProcessedNews;
            
            // Impact based on sentiment and controversy
            float baseImpact = news.ControversyScore / 10f;
            
            // Negative news affects trust
            if (news.Sentiment.Classification == SentimentType.Negative || 
                news.Sentiment.Classification == SentimentType.VeryNegative)
            {
                newsEvent.TrustImpact = -baseImpact;
            }
            
            // Positive news can boost trust
            if (news.Sentiment.Classification == SentimentType.Positive || 
                news.Sentiment.Classification == SentimentType.VeryPositive)
            {
                newsEvent.TrustImpact = baseImpact * 0.5f; // Less impact from positive
            }
            
            // Controversial news affects voter blocs
            if (news.ControversyScore > 50f)
            {
                foreach (var issue in news.IssueCategories)
                {
                    newsEvent.VoterBlocImpacts[GetVoterBlocForIssue(issue)] = -baseImpact * 0.5f;
                }
            }
            
            // Create policy opportunity
            if (news.IssueCategories.Count > 0)
            {
                newsEvent.PolicyOpportunity = news.IssueCategories[0];
            }
        }
        
        private VoterBloc GetVoterBlocForIssue(IssueCategory issue)
        {
            // Map issues to voter blocs
            return issue switch
            {
                IssueCategory.Healthcare => VoterBloc.Seniors,
                IssueCategory.Economy => VoterBloc.WorkingClass,
                IssueCategory.Education => VoterBloc.Educators,
                IssueCategory.Environment => VoterBloc.Activists,
                IssueCategory.Immigration => VoterBloc.Rural,
                _ => VoterBloc.General
            };
        }
        
        private void UpdateActiveEvents()
        {
            foreach (var newsEvent in activeEvents.ToList())
            {
                // Apply impact if not already applied
                if (!newsEvent.ImpactApplied && 
                    (DateTime.Now - newsEvent.CreatedDate).TotalHours > 1)
                {
                    ApplyNewsImpact(newsEvent);
                    newsEvent.ImpactApplied = true;
                }
                
                // Remove old events (older than 3 days)
                if ((DateTime.Now - newsEvent.CreatedDate).TotalDays > 3)
                {
                    activeEvents.Remove(newsEvent);
                }
            }
        }
        
        private void ApplyNewsImpact(NewsEvent newsEvent)
        {
            if (player == null) return;
            
            // Apply trust impact
            if (newsEvent.TrustImpact != 0f)
            {
                // Would need ResourceManager reference
                // For now, directly modify
                if (player.Resources.ContainsKey("PublicTrust"))
                {
                    player.Resources["PublicTrust"] += newsEvent.TrustImpact;
                    player.Resources["PublicTrust"] = Mathf.Clamp(player.Resources["PublicTrust"], 0f, 100f);
                }
            }
            
            // Apply voter bloc impacts
            foreach (var impact in newsEvent.VoterBlocImpacts)
            {
                if (player.VoterBlocSupport.ContainsKey(impact.Key))
                {
                    player.VoterBlocSupport[impact.Key] += impact.Value;
                    player.VoterBlocSupport[impact.Key] = Mathf.Clamp(player.VoterBlocSupport[impact.Key], 0f, 100f);
                }
            }
        }
        
        /// <summary>
        /// Get current trending news
        /// </summary>
        public List<ProcessedNews> GetTrendingNews(int count = 10)
        {
            return currentNews
                .OrderByDescending(n => n.ControversyScore)
                .ThenByDescending(n => n.PoliticalRelevance)
                .Take(count)
                .ToList();
        }
        
        /// <summary>
        /// Get news by issue category
        /// </summary>
        public List<ProcessedNews> GetNewsByIssue(IssueCategory issue, int count = 5)
        {
            return currentNews
                .Where(n => n.IssueCategories.Contains(issue))
                .OrderByDescending(n => n.ProcessedDate)
                .Take(count)
                .ToList();
        }
        
        /// <summary>
        /// Get active news events affecting the game
        /// </summary>
        public List<NewsEvent> GetActiveEvents()
        {
            return new List<NewsEvent>(activeEvents);
        }
        
        /// <summary>
        /// Get active game events (new format)
        /// </summary>
        public List<NewsGameEvent> GetActiveGameEvents()
        {
            int currentTurn = gameStateProvider?.GetCurrentTurn() ?? 0;
            return gameEvents
                .Where(e => e.ExpirationTurn > currentTurn && !e.PlayerResponded)
                .ToList();
        }
        
        /// <summary>
        /// Player responds to a game event
        /// </summary>
        public void HandlePlayerResponse(NewsGameEvent gameEvent, string optionId)
        {
            var result = responseSystem.RespondToEvent(gameEvent, optionId);
            
            if (result.Success)
            {
                Debug.Log($"[NewsEventManager] Player response: {result.Message}");
            }
            else
            {
                Debug.LogWarning($"[NewsEventManager] Response failed: {result.Message}");
            }
        }
    }
    
    [Serializable]
    public class NewsEvent
    {
        public string ID;
        public ProcessedNews ProcessedNews;
        public DateTime CreatedDate;
        public bool ImpactApplied;
        
        // Game impacts
        public float TrustImpact;
        public Dictionary<VoterBloc, float> VoterBlocImpacts;
        public IssueCategory? PolicyOpportunity;
        
        public NewsEvent()
        {
            VoterBlocImpacts = new Dictionary<VoterBloc, float>();
        }
    }
}

