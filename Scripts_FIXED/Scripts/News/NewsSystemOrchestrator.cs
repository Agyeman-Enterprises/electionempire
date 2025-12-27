// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - News System Orchestrator
// Main integration layer connecting all Sprint 7 components
// ═══════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.News.Templates;
using ElectionEmpire.News.Translation;
using ElectionEmpire.News.Temporal;
using ElectionEmpire.News.Consequences;
using ElectionEmpire.News.Fallback;
using ElectionEmpire.News;

namespace ElectionEmpire.News
{
    #region Configuration
    
    /// <summary>
    /// Master configuration for the entire news system.
    /// </summary>
    [Serializable]
    public class NewsSystemConfig
    {
        [Header("General Settings")]
        public bool EnableRealWorldNews = true;
        public bool EnableProceduralFallback = true;
        public float RealityBlendFactor = 0.7f;  // 0 = all procedural, 1 = all real
        
        [Header("Update Intervals")]
        public float NewsCheckIntervalSeconds = 300f;  // 5 minutes
        public float TemporalUpdateIntervalSeconds = 60f;
        public int MaxEventsPerTurn = 5;
        
        [Header("Player Settings")]
        public float NewsFrequencyMultiplier = 1.0f;
        public bool FilterSensitiveContent = true;
        public List<string> DisabledCategories = new List<string>();
        
        [Header("Debug")]
        public bool EnableDebugLogging = false;
        public bool SimulateOfflineMode = false;
    }
    
    #endregion
    
    #region Events & Callbacks
    
    /// <summary>
    /// Event arguments for news system events.
    /// </summary>
    public class NewsEventArgs : EventArgs
    {
        public string EventId { get; set; }
        public NewsGameEvent GameEvent { get; set; }
        public NewsCycleStage Stage { get; set; }
        public bool RequiresPlayerAction { get; set; }
    }
    
    public class ResponseEventArgs : EventArgs
    {
        public string EventId { get; set; }
        public ResponseResult Result { get; set; }
    }
    
    #endregion
    
    /// <summary>
    /// Main orchestrator for the Election Empire news system.
    /// Connects template matching, temporal management, consequence calculation,
    /// and fallback systems into a unified pipeline.
    /// </summary>
    public class NewsSystemOrchestrator : IDisposable
    {
        #region Components
        
        private readonly NewsSystemConfig _config;
        private readonly Translation.IGameStateProvider _gameStateProvider;
        private readonly Consequences.IGameStateModifier _gameStateModifier;
        
        // Core pipeline components
        private readonly AdvancedTemplateMatcher _templateMatcher;
        private readonly VariableInjector _variableInjector;
        private readonly NewsEventFactory _eventFactory;
        
        // Temporal management
        private readonly NewsCycleManager _cycleManager;
        private readonly Temporal.TimeScaler _timeScaler;
        private readonly Temporal.MediaFatigueTracker _fatigueTracker;
        
        // Consequence system
        private readonly Consequences.ConsequenceCalculator _consequenceCalculator;
        private readonly Consequences.EffectApplicator _effectApplicator;
        private readonly Consequences.StanceHistoryTracker _stanceTracker;
        
        // Fallback system
        private readonly Fallback.NewsCacheManager _cacheManager;
        private readonly Fallback.ProceduralNewsGenerator _proceduralGenerator;
        private readonly Fallback.FallbackOrchestrator _fallbackOrchestrator;
        
        // State tracking
        private readonly Dictionary<string, NewsGameEvent> _activeEvents;
        private readonly Dictionary<string, ProcessedNewsItem> _pendingNews;
        private readonly Queue<string> _eventQueue;
        
        private bool _isInitialized;
        private bool _isProcessing;
        private DateTime _lastNewsCheck;
        private DateTime _lastTemporalUpdate;
        
        #endregion
        
        #region Events
        
        public event EventHandler<NewsEventArgs> OnBreakingNews;
        public event EventHandler<NewsEventArgs> OnNewEventAvailable;
        public event EventHandler<NewsEventArgs> OnEventStageChanged;
        public event EventHandler<NewsEventArgs> OnEventExpired;
        public event EventHandler<ResponseEventArgs> OnResponseProcessed;
        public event EventHandler<string> OnSystemWarning;
        
        #endregion
        
        #region Constructor & Initialization
        
        public NewsSystemOrchestrator(
            Translation.IGameStateProvider gameStateProvider,
            Consequences.IGameStateModifier gameStateModifier,
            NewsSystemConfig config = null)
        {
            _config = config ?? new NewsSystemConfig();
            _gameStateProvider = gameStateProvider;
            _gameStateModifier = gameStateModifier;
            
            _activeEvents = new Dictionary<string, NewsGameEvent>();
            _pendingNews = new Dictionary<string, ProcessedNewsItem>();
            _eventQueue = new Queue<string>();
            
            // Initialize components (must be in constructor for readonly fields)
            // Template system
            Templates.EventTemplateLibrary.Initialize(); // Initialize static library
            
            var matcherConfig = new TemplateMatcherConfig();
            _templateMatcher = new AdvancedTemplateMatcher(_gameStateProvider, matcherConfig);
            _variableInjector = new VariableInjector(_gameStateProvider);
            _eventFactory = new NewsEventFactory(_gameStateProvider);
            
            // Temporal system
            var temporalConfig = new Temporal.TemporalConfig();
            var timeProvider = new GameTimeProviderAdapter(_gameStateProvider);
            _cycleManager = new NewsCycleManager(timeProvider, temporalConfig);
            _timeScaler = new Temporal.TimeScaler(timeProvider, temporalConfig);
            _fatigueTracker = new Temporal.MediaFatigueTracker(temporalConfig);
            
            // Consequence system
            var consequenceConfig = new Consequences.ConsequenceConfig();
            _consequenceCalculator = new Consequences.ConsequenceCalculator(_gameStateProvider, consequenceConfig);
            _effectApplicator = new Consequences.EffectApplicator(_gameStateModifier, consequenceConfig);
            _stanceTracker = new Consequences.StanceHistoryTracker();
            
            // Fallback system
            var fallbackConfig = new Fallback.FallbackConfig();
            _cacheManager = new Fallback.NewsCacheManager(fallbackConfig);
            
            var fallbackGameState = new FallbackGameStateAdapter(_gameStateProvider);
            _proceduralGenerator = new Fallback.ProceduralNewsGenerator(fallbackGameState, fallbackConfig);
            _fallbackOrchestrator = new Fallback.FallbackOrchestrator(_cacheManager, _proceduralGenerator, fallbackConfig);
            
            // Wire up internal events
            WireInternalEvents();
            
            _isInitialized = true;
            _lastNewsCheck = DateTime.UtcNow;
            _lastTemporalUpdate = DateTime.UtcNow;
            
            Log("NewsSystemOrchestrator initialized");
        }
        
        private void WireInternalEvents()
        {
            // Cycle manager events
            _cycleManager.OnStageTransition += HandleStageTransition;
            _cycleManager.OnEventArchived += HandleEventArchived;
            _cycleManager.OnBreakingNewsInterrupt += HandleBreakingNews;
            
            // Effect applicator events
            _effectApplicator.OnEffectApplied += HandleEffectApplied;
            
            // Fallback events
            _fallbackOrchestrator.OnSourceChanged += HandleSourceChanged;
            _fallbackOrchestrator.OnFallbackWarning += HandleFallbackWarning;
        }
        
        #endregion
        
        #region Main Pipeline
        
        /// <summary>
        /// Process incoming news from external API.
        /// Entry point for real-world news integration.
        /// </summary>
        public void ProcessIncomingNews(List<ProcessedNewsItem> newsItems)
        {
            if (!_isInitialized || _isProcessing) return;
            
            _isProcessing = true;
            Log($"Processing {newsItems.Count} incoming news items");
            
            try
            {
                _fallbackOrchestrator.RecordAPISuccess();
                
                foreach (var news in newsItems)
                {
                    // Check category filter
                    if (_config.DisabledCategories.Contains(news.PrimaryCategory.ToString()))
                    {
                        Log($"Skipping news {news.SourceId} - category disabled");
                        continue;
                    }
                    
                    // Check fatigue
                    var entities = news.Entities?.People?.Select(p => p.Name).ToList() ?? new List<string>();
                    float fatigueMod = _fatigueTracker.GetFatigueModifier(news.PrimaryCategory.ToString(), entities);
                    
                    if (fatigueMod < 0.3f)
                    {
                        Log($"Skipping news {news.SourceId} - high fatigue ({fatigueMod:F2})");
                        continue;
                    }
                    
                    // Cache for later use
                    CacheNewsItem(news);
                    
                    // Queue for processing
                    _pendingNews[news.SourceId] = news;
                }
                
                // Process pending news into game events
                ProcessPendingNews();
            }
            finally
            {
                _isProcessing = false;
                _lastNewsCheck = DateTime.UtcNow;
            }
        }
        
        /// <summary>
        /// Process pending news items into game events.
        /// </summary>
        private void ProcessPendingNews()
        {
            int processed = 0;
            var toRemove = new List<string>();
            
            foreach (var kvp in _pendingNews)
            {
                if (processed >= _config.MaxEventsPerTurn) break;
                
                var newsItem = kvp.Value;
                var gameEvent = TranslateToGameEvent(newsItem);
                
                if (gameEvent != null)
                {
                    RegisterGameEvent(gameEvent, newsItem);
                    processed++;
                }
                
                toRemove.Add(kvp.Key);
            }
            
            foreach (var key in toRemove)
            {
                _pendingNews.Remove(key);
            }
            
            Log($"Processed {processed} news items into game events");
        }
        
        /// <summary>
        /// Translate a processed news item into a game event.
        /// Core translation pipeline.
        /// </summary>
        private NewsGameEvent TranslateToGameEvent(ProcessedNewsItem newsItem)
        {
            try
            {
                // Step 1: Match to template
                var matchResult = _templateMatcher.FindBestMatch(newsItem);
                
                if (matchResult == null || matchResult.FinalScore < 0.3f)
                {
                    Log($"No suitable template for news {newsItem.SourceId}");
                    return null;
                }
                
                // Step 2: Inject variables
                _variableInjector.InjectVariables(matchResult);
                
                // Step 3: Create game event
                var gameEvent = _eventFactory.CreateEvent(matchResult);
                
                // Step 4: Apply fatigue modifier to effects
                var entities = newsItem.Entities?.People?.Select(p => p.Name).ToList() ?? new List<string>();
                float fatigueMod = _fatigueTracker.GetFatigueModifier(newsItem.PrimaryCategory.ToString(), entities);
                if (gameEvent.ScaledEffects != null)
                {
                    gameEvent.ScaledEffects = ApplyFatigueToEffects(gameEvent.ScaledEffects, fatigueMod);
                }
                
                // Step 5: Calculate deadline and expiration
                var stage = DetermineInitialStage(newsItem);
                gameEvent.DeadlineTurn = _timeScaler.CalculateDeadlineTurn(newsItem.PublishedAt, stage);
                gameEvent.ExpirationTurn = _timeScaler.CalculateExpirationTurn(newsItem.PublishedAt, stage);
                
                return gameEvent;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NewsSystem] Failed to translate news {newsItem.SourceId}: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Register a game event for tracking.
        /// </summary>
        private void RegisterGameEvent(NewsGameEvent gameEvent, ProcessedNewsItem sourceNews)
        {
            _activeEvents[gameEvent.EventId] = gameEvent;
            _cycleManager.RegisterEvent(gameEvent.EventId, sourceNews.SourceId, sourceNews.PublishedAt);
            _fatigueTracker.RecordEvent(
                gameEvent.EventId,
                gameEvent.Category,
                sourceNews.Entities?.People?.Select(p => p.Name).ToList() ?? new List<string>());
            
            _eventQueue.Enqueue(gameEvent.EventId);
            
            var stage = _cycleManager.GetEventStage(gameEvent.EventId);
            OnNewEventAvailable?.Invoke(this, new NewsEventArgs
            {
                EventId = gameEvent.EventId,
                GameEvent = gameEvent,
                Stage = stage,
                RequiresPlayerAction = gameEvent.Urgency == UrgencyLevel.Breaking || gameEvent.Urgency == UrgencyLevel.Urgent
            });
        }
        
        #endregion
        
        #region Player Response Handling
        
        /// <summary>
        /// Process a player's response to a news event.
        /// </summary>
        public ResponseResult ProcessPlayerResponse(string eventId, string responseOptionId)
        {
            if (!_activeEvents.TryGetValue(eventId, out var gameEvent))
            {
                Debug.LogError($"[NewsSystem] Event {eventId} not found");
                return null;
            }
            
            var responseOption = gameEvent.ResponseOptions?.FirstOrDefault(r => r.OptionId == responseOptionId);
            if (responseOption == null)
            {
                Debug.LogError($"[NewsSystem] Response option {responseOptionId} not found for event {eventId}");
                return null;
            }
            
            Log($"Processing response '{responseOption.Label}' for event {eventId}");
            
            // Build consequence context
            var context = BuildConsequenceContext();
            
            // Calculate consequences
            var result = _consequenceCalculator.CalculateConsequences(gameEvent, responseOption, context);
            
            // Apply effects
            _effectApplicator.ApplyResponseResult(result);
            
            // Record stance
            RecordPlayerStance(gameEvent, responseOption);
            
            // Update cycle manager
            _cycleManager.RecordPlayerInteraction(eventId);
            
            // Fire event
            OnResponseProcessed?.Invoke(this, new ResponseEventArgs
            {
                EventId = eventId,
                Result = result
            });
            
            return result;
        }
        
        private Consequences.ConsequenceContext BuildConsequenceContext()
        {
            var alignment = _gameStateProvider.GetPlayerAlignment();
            
            return new Consequences.ConsequenceContext
            {
                PlayerOfficeTier = _gameStateProvider.GetPlayerOfficeTier(),
                PlayerApproval = _gameStateProvider.GetPlayerApproval(),
                CurrentTurn = _gameStateProvider.GetCurrentTurn(),
                TurnsUntilElection = _gameStateProvider.GetTurnsUntilElection(),
                IsChaosModeEnabled = _gameStateProvider.IsChaosModeEnabled(),
                ActiveTags = new List<Consequences.ReputationTag>(),  // Would come from player state
                StanceHistory = new List<Consequences.StanceRecord>()  // Would come from stance tracker
            };
        }
        
        private void RecordPlayerStance(NewsGameEvent gameEvent, ResponseOption response)
        {
            // Determine stance strength from response
            float stanceStrength = 0f;
            if (response.AlignmentEffect != null)
            {
                stanceStrength = (response.AlignmentEffect.LawChaos + response.AlignmentEffect.GoodEvil) / 2f;
            }
            
            _stanceTracker.RecordStance(
                gameEvent.Category,
                response.Label,
                stanceStrength / 10f,  // Normalize to -1 to 1
                _gameStateProvider.GetCurrentTurn(),
                gameEvent.EventId,
                wasPublic: true);
        }
        
        #endregion
        
        #region Update Loop
        
        /// <summary>
        /// Main update method. Call from game's update loop.
        /// </summary>
        public void Update()
        {
            if (!_isInitialized) return;
            
            var now = DateTime.UtcNow;
            
            // Temporal update
            if ((now - _lastTemporalUpdate).TotalSeconds >= _config.TemporalUpdateIntervalSeconds)
            {
                UpdateTemporalState();
                _lastTemporalUpdate = now;
            }
            
            // News check (if not in offline mode)
            if (_config.EnableRealWorldNews && !_config.SimulateOfflineMode)
            {
                if ((now - _lastNewsCheck).TotalSeconds >= _config.NewsCheckIntervalSeconds)
                {
                    CheckForNewNews();
                    _lastNewsCheck = now;
                }
            }
        }
        
        /// <summary>
        /// Called when a game turn advances.
        /// </summary>
        public void OnTurnAdvance()
        {
            Log("Processing turn advance");
            
            // Update temporal state
            var temporalResult = _cycleManager.OnTurnAdvance();
            
            // Process delayed effects
            _effectApplicator.ProcessTurn();
            
            // Decay fatigue
            _fatigueTracker.DecayFatigue();
            
            // Generate procedural news if needed
            if (_config.EnableProceduralFallback && ShouldGenerateProceduralNews())
            {
                GenerateProceduralNews();
            }
            
            // Clean cache
            _cacheManager.CleanExpiredItems();
            
            Log($"Turn advance complete. Stage transitions: {temporalResult.StageTransitions.Count}, " +
                $"Archived: {temporalResult.ArchivedEvents.Count}");
        }
        
        private void UpdateTemporalState()
        {
            var result = _cycleManager.Update();
            
            foreach (var transition in result.StageTransitions)
            {
                Log($"Stage transition: {transition}");
            }
            
            if (result.InterruptingEvents.Count > 0)
            {
                foreach (var eventId in result.InterruptingEvents)
                {
                    if (_activeEvents.TryGetValue(eventId, out var gameEvent))
                    {
                        OnBreakingNews?.Invoke(this, new NewsEventArgs
                        {
                            EventId = eventId,
                            GameEvent = gameEvent,
                            Stage = NewsCycleStage.Breaking,
                            RequiresPlayerAction = true
                        });
                    }
                }
            }
        }
        
        private void CheckForNewNews()
        {
            // This would typically call an external news service
            // For now, we use the fallback system
            var content = _fallbackOrchestrator.GetNewsContent(count: 3);
            
            if (content.Count > 0)
            {
                var processedItems = content.Select(ConvertCachedToProcessed).ToList();
                ProcessIncomingNews(processedItems);
            }
        }
        
        private bool ShouldGenerateProceduralNews()
        {
            // Generate if we have too few active events
            return _activeEvents.Count < 3;
        }
        
        private void GenerateProceduralNews()
        {
            var generated = _proceduralGenerator.GenerateContextualNews(2);
            
            foreach (var item in generated)
            {
                var processed = ConvertCachedToProcessed(item);
                _pendingNews[processed.SourceId] = processed;
            }
            
            ProcessPendingNews();
        }
        
        #endregion
        
        #region Query Methods
        
        /// <summary>
        /// Get all active game events.
        /// </summary>
        public List<NewsGameEvent> GetActiveEvents()
        {
            return _activeEvents.Values.ToList();
        }
        
        /// <summary>
        /// Get events by stage.
        /// </summary>
        public List<NewsGameEvent> GetEventsByStage(NewsCycleStage stage)
        {
            var eventIds = _cycleManager.GetEventsByStage(stage);
            return eventIds
                .Where(id => _activeEvents.ContainsKey(id))
                .Select(id => _activeEvents[id])
                .ToList();
        }
        
        /// <summary>
        /// Get a specific event.
        /// </summary>
        public NewsGameEvent GetEvent(string eventId)
        {
            return _activeEvents.TryGetValue(eventId, out var evt) ? evt : null;
        }
        
        /// <summary>
        /// Get events requiring player action.
        /// </summary>
        public List<NewsGameEvent> GetPendingPlayerActions()
        {
            var interrupting = _cycleManager.GetInterruptingEvents();
            return interrupting
                .Where(id => _activeEvents.ContainsKey(id))
                .Select(id => _activeEvents[id])
                .ToList();
        }
        
        /// <summary>
        /// Get next event from queue.
        /// </summary>
        public NewsGameEvent DequeueNextEvent()
        {
            while (_eventQueue.Count > 0)
            {
                var eventId = _eventQueue.Dequeue();
                if (_activeEvents.TryGetValue(eventId, out var evt))
                {
                    return evt;
                }
            }
            return null;
        }
        
        /// <summary>
        /// Check if player would flip-flop on an issue.
        /// </summary>
        public bool WouldBeFlipFlop(string category, float proposedStrength, out string previousStance)
        {
            return _stanceTracker.WouldBeFlipFlop(category, proposedStrength, out previousStance);
        }
        
        /// <summary>
        /// Get system status.
        /// </summary>
        public NewsSystemStatus GetStatus()
        {
            return new NewsSystemStatus
            {
                IsInitialized = _isInitialized,
                ActiveEventCount = _activeEvents.Count,
                PendingNewsCount = _pendingNews.Count,
                QueuedEventCount = _eventQueue.Count,
                CacheStatus = _fallbackOrchestrator.GetStatus(),
                LastNewsCheck = _lastNewsCheck,
                LastTemporalUpdate = _lastTemporalUpdate
            };
        }
        
        #endregion
        
        #region Internal Event Handlers
        
        private void HandleStageTransition(string eventId, NewsCycleStage oldStage, NewsCycleStage newStage)
        {
            if (_activeEvents.TryGetValue(eventId, out var gameEvent))
            {
                OnEventStageChanged?.Invoke(this, new NewsEventArgs
                {
                    EventId = eventId,
                    GameEvent = gameEvent,
                    Stage = newStage,
                    RequiresPlayerAction = newStage == NewsCycleStage.Breaking
                });
            }
        }
        
        private void HandleEventArchived(string eventId)
        {
            if (_activeEvents.TryGetValue(eventId, out var gameEvent))
            {
                OnEventExpired?.Invoke(this, new NewsEventArgs
                {
                    EventId = eventId,
                    GameEvent = gameEvent,
                    Stage = NewsCycleStage.Archived
                });
                
                _activeEvents.Remove(eventId);
            }
        }
        
        private void HandleBreakingNews(string eventId)
        {
            if (_activeEvents.TryGetValue(eventId, out var gameEvent))
            {
                OnBreakingNews?.Invoke(this, new NewsEventArgs
                {
                    EventId = eventId,
                    GameEvent = gameEvent,
                    Stage = NewsCycleStage.Breaking,
                    RequiresPlayerAction = true
                });
            }
        }
        
        private void HandleEffectApplied(Consequences.ConsequenceEffect effect)
        {
            Log($"Effect applied: {effect.Resource} {effect.ActualApplied:+0.00;-0.00}");
        }
        
        private void HandleSourceChanged(Fallback.NewsSource newSource)
        {
            Log($"News source changed to: {newSource}");
            OnSystemWarning?.Invoke(this, $"News source: {newSource}");
        }
        
        private void HandleFallbackWarning(string warning)
        {
            OnSystemWarning?.Invoke(this, warning);
        }
        
        #endregion
        
        #region Helper Methods
        
        private void CacheNewsItem(ProcessedNewsItem news)
        {
            var cached = new Fallback.CachedNewsItem
            {
                OriginalNewsId = news.SourceId,
                Source = Fallback.NewsSource.RealTimeAPI,
                Headline = news.Headline,
                Summary = news.Summary,
                Category = news.PrimaryCategory.ToString(),
                Keywords = ExtractKeywords(news),
                RelevanceScore = news.ImpactScore / 10f,  // Normalize to 0-1
                QualityScore = 0.8f,
                ControversyScore = news.ControversyScore
            };
            
            _cacheManager.CacheItem(cached);
        }
        
        private ProcessedNewsItem ConvertCachedToProcessed(Fallback.CachedNewsItem cached)
        {
            // Convert cached item back to ProcessedNewsItem
            var processed = new ProcessedNewsItem
            {
                SourceId = cached.OriginalNewsId ?? Guid.NewGuid().ToString(),
                Headline = cached.Headline,
                Summary = cached.Summary,
                PrimaryCategory = ParseCategory(cached.Category),
                Keywords = cached.Keywords,
                ImpactScore = cached.RelevanceScore * 10f,  // Denormalize
                ControversyScore = cached.ControversyScore,
                PublishedAt = cached.CachedAt,
                Entities = new ExtractedEntities()
            };
            
            // Try to extract entities from keywords
            if (cached.Entities != null && cached.Entities.Count > 0)
            {
                foreach (var entityName in cached.Entities)
                {
                    processed.Entities.People.Add(new ExtractedEntity
                    {
                        Name = entityName,
                        Type = TemplateEntityType.Person
                    });
                }
            }
            
            return processed;
        }
        
        private PoliticalCategory ParseCategory(string categoryName)
        {
            if (Enum.TryParse<PoliticalCategory>(categoryName, out var category))
            {
                return category;
            }
            return PoliticalCategory.General;
        }
        
        private NewsCycleStage DetermineInitialStage(ProcessedNewsItem news)
        {
            var age = DateTime.UtcNow - news.PublishedAt;
            
            if (age.TotalHours < 2) return NewsCycleStage.Breaking;
            if (age.TotalHours < 24) return NewsCycleStage.Developing;
            if (age.TotalDays < 7) return NewsCycleStage.Ongoing;
            return NewsCycleStage.Fading;
        }
        
        private ScaledEffects ApplyFatigueToEffects(ScaledEffects effects, float fatigueMod)
        {
            if (effects == null) return null;
            
            effects.TrustDelta *= fatigueMod;
            effects.CapitalDelta *= fatigueMod;
            effects.MediaDelta *= fatigueMod;
            effects.PartyLoyaltyDelta *= fatigueMod;
            
            return effects;
        }
        
        private List<string> ExtractKeywords(ProcessedNewsItem news)
        {
            var keywords = new List<string>();
            
            // Extract from entities
            if (news.Entities != null)
            {
                keywords.AddRange(news.Entities.People.Select(p => p.Name));
                keywords.AddRange(news.Entities.Organizations.Select(o => o.Name));
            }
            
            // Extract from category
            keywords.Add(news.PrimaryCategory.ToString());
            
            return keywords;
        }
        
        private void Log(string message)
        {
            if (_config.EnableDebugLogging)
            {
                Debug.Log($"[NewsSystem] {message}");
            }
        }
        
        #endregion
        
        #region Disposal
        
        public void Dispose()
        {
            _cycleManager.OnStageTransition -= HandleStageTransition;
            _cycleManager.OnEventArchived -= HandleEventArchived;
            _cycleManager.OnBreakingNewsInterrupt -= HandleBreakingNews;
            _effectApplicator.OnEffectApplied -= HandleEffectApplied;
            _fallbackOrchestrator.OnSourceChanged -= HandleSourceChanged;
            _fallbackOrchestrator.OnFallbackWarning -= HandleFallbackWarning;
            
            _isInitialized = false;
        }
        
        #endregion
    }
    
    #region Status & Adapters
    
    /// <summary>
    /// Status report for the news system.
    /// </summary>
    public class NewsSystemStatus
    {
        public bool IsInitialized;
        public int ActiveEventCount;
        public int PendingNewsCount;
        public int QueuedEventCount;
        public Fallback.FallbackStatus CacheStatus;
        public DateTime LastNewsCheck;
        public DateTime LastTemporalUpdate;
    }
    
    /// <summary>
    /// Adapter to provide IGameTimeProvider from IGameStateProvider.
    /// </summary>
    internal class GameTimeProviderAdapter : Temporal.IGameTimeProvider
    {
        private readonly Translation.IGameStateProvider _gameState;
        
        public GameTimeProviderAdapter(Translation.IGameStateProvider gameState)
        {
            _gameState = gameState;
        }
        
        public int GetCurrentTurn() => _gameState.GetCurrentTurn();
        public DateTime GetGameDateTime() => DateTime.UtcNow;  // Would map to game time
        public float GetTimeScale() => 1.0f;
    }
    
    /// <summary>
    /// Adapter for fallback system's game state provider.
    /// </summary>
    internal class FallbackGameStateAdapter : Fallback.IGameStateProvider
    {
        private readonly Translation.IGameStateProvider _gameState;
        
        public FallbackGameStateAdapter(Translation.IGameStateProvider gameState)
        {
            _gameState = gameState;
        }
        
        public int GetPlayerOfficeTier() => _gameState.GetPlayerOfficeTier();
        public float GetPlayerApproval() => _gameState.GetPlayerApproval();
        public int GetTurnsUntilElection() => _gameState.GetTurnsUntilElection();
    }
    
    #endregion
}

