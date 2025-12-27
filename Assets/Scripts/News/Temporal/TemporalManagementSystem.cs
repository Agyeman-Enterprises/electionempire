// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - Temporal Management System
// Stage 5: News Cycle Management, Time Scaling, and Media Fatigue
// ═══════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.News.Translation;
using ElectionEmpire.News;

namespace ElectionEmpire.News.Temporal
{
    #region Enums & Configuration
    /// <summary>
    /// Configuration for temporal management behavior.
    /// </summary>
    [Serializable]
    public class TemporalConfig
    {
        [Header("News Cycle Durations (real-world hours)")]
        public float BreakingDurationHours = 2f;
        public float DevelopingDurationHours = 24f;
        public float OngoingDurationDays = 7f;
        public float FadingDurationDays = 14f;
        
        [Header("Time Scaling")]
        public float RealMinutesToGameMinutes = 1f;  // 1:1 default
        public int TurnDurationGameHours = 24;       // 1 turn = 1 day
        
        [Header("Media Fatigue")]
        public float BaseFatiguePerTurn = 0.1f;
        public float SimilarStoryFatiguePenalty = 0.15f;
        public float NewDevelopmentRecovery = 0.3f;
        public float PlayerActionRecovery = 0.2f;
        public float MinFatigueThreshold = 0.2f;     // Below this, story is stale
        
        [Header("Event Limits")]
        public int MaxActiveBreakingNews = 2;
        public int MaxActiveDevelopingNews = 5;
        public int MaxActiveOngoingNews = 10;
        public int MaxTotalActiveEvents = 20;
        
        [Header("Update Intervals")]
        public float CycleUpdateIntervalSeconds = 60f;
        public int ArchiveRetentionDays = 30;
    }
    
    #endregion
    
    #region Data Structures
    
    /// <summary>
    /// Tracks the temporal state of a news-based game event.
    /// </summary>
    [Serializable]
    public class NewsTemporalState
    {
        public string EventId;
        public string SourceNewsId;
        
        // Cycle tracking
        public NewsCycleStage CurrentStage;
        public DateTime StageEnteredAt;
        public DateTime OriginalPublishTime;
        public int TurnsInCurrentStage;
        
        // Fatigue tracking
        public float MediaFatigue;           // 0.0 (fresh) to 1.0 (exhausted)
        public float PublicInterest;         // 0.0 to 1.0
        public int PlayerInteractionCount;
        public DateTime LastPlayerInteraction;
        public DateTime LastDevelopment;
        
        // Similar story tracking
        public List<string> RelatedEventIds;
        public int SimilarStoriesThisCycle;
        
        // Flags
        public bool HasPlayerResponded;
        public bool HasEscalated;
        public bool IsInterrupting;
        public bool MarkedForArchive;
        
        public NewsTemporalState()
        {
            RelatedEventIds = new List<string>();
            MediaFatigue = 0f;
            PublicInterest = 1f;
        }
    }
    
    /// <summary>
    /// Result of a temporal update cycle.
    /// </summary>
    [Serializable]
    public class TemporalUpdateResult
    {
        public List<string> StageTransitions;      // Events that changed stage
        public List<string> ArchivedEvents;        // Events moved to archive
        public List<string> FatigueAlerts;         // Events with high fatigue
        public List<string> InterruptingEvents;    // Breaking news interrupts
        public Dictionary<string, float> UpdatedFatigue;
        
        public TemporalUpdateResult()
        {
            StageTransitions = new List<string>();
            ArchivedEvents = new List<string>();
            FatigueAlerts = new List<string>();
            InterruptingEvents = new List<string>();
            UpdatedFatigue = new Dictionary<string, float>();
        }
    }
    
    #endregion
    /// <summary>
    /// Handles time scaling between real-world and game time.
    /// </summary>
    public class TimeScaler
    {
        private readonly TemporalConfig _config;
        private readonly IGameTimeProvider _timeProvider;
        
        public TimeScaler(IGameTimeProvider timeProvider = null, TemporalConfig config = null)
        {
            _timeProvider = timeProvider ?? new DefaultGameTimeProvider();
            _config = config ?? new TemporalConfig();
        }
        
        /// <summary>
        /// Convert real-world time difference to game turns.
        /// </summary>
        public int RealTimeToGameTurns(TimeSpan realTime)
        {
            float realHours = (float)realTime.TotalHours;
            float gameHours = realHours * _config.RealMinutesToGameMinutes;
            return Mathf.FloorToInt(gameHours / _config.TurnDurationGameHours);
        }
        
        /// <summary>
        /// Calculate deadline turn for a news event.
        /// </summary>
        public int CalculateDeadlineTurn(DateTime newsPublishTime, NewsCycleStage stage)
        {
            int currentTurn = _timeProvider.GetCurrentTurn();
            var age = DateTime.UtcNow - newsPublishTime;
            int ageTurns = RealTimeToGameTurns(age);
            
            int deadlineOffset = stage switch
            {
                NewsCycleStage.Breaking => 2,
                NewsCycleStage.Developing => 5,
                NewsCycleStage.Ongoing => 10,
                _ => 15
            };
            
            return currentTurn + deadlineOffset - ageTurns;
        }
        
        /// <summary>
        /// Calculate expiration turn for a news event.
        /// </summary>
        public int CalculateExpirationTurn(DateTime newsPublishTime, NewsCycleStage stage)
        {
            int currentTurn = _timeProvider.GetCurrentTurn();
            
            int expirationOffset = stage switch
            {
                NewsCycleStage.Breaking => 5,
                NewsCycleStage.Developing => 15,
                NewsCycleStage.Ongoing => 30,
                NewsCycleStage.Fading => 45,
                _ => 60
            };
            
            return currentTurn + expirationOffset;
        }
        
        /// <summary>
        /// Get urgency level based on time remaining.
        /// </summary>
        public float GetUrgencyFactor(int deadlineTurn)
        {
            int currentTurn = _timeProvider.GetCurrentTurn();
            int turnsRemaining = deadlineTurn - currentTurn;
            
            if (turnsRemaining <= 1) return 1.0f;      // Critical
            if (turnsRemaining <= 3) return 0.8f;      // Urgent
            if (turnsRemaining <= 5) return 0.6f;      // High
            if (turnsRemaining <= 10) return 0.4f;     // Medium
            return 0.2f;                               // Low
        }
    }
    
    /// <summary>
    /// Tracks media fatigue across the news ecosystem.
    /// </summary>
    public class MediaFatigueTracker
    {
        private readonly TemporalConfig _config;
        private readonly Dictionary<string, float> _categoryFatigue;
        private readonly Dictionary<string, float> _entityFatigue;
        private readonly Queue<FatigueEvent> _recentEvents;
        
        private const int MAX_RECENT_EVENTS = 100;
        
        public MediaFatigueTracker(TemporalConfig config = null)
        {
            _config = config ?? new TemporalConfig();
            _categoryFatigue = new Dictionary<string, float>();
            _entityFatigue = new Dictionary<string, float>();
            _recentEvents = new Queue<FatigueEvent>();
        }
        
        /// <summary>
        /// Record a news event for fatigue tracking.
        /// </summary>
        public void RecordEvent(string eventId, string category, List<string> entities)
        {
            var fatigueEvent = new FatigueEvent
            {
                EventId = eventId,
                Category = category,
                Entities = entities ?? new List<string>(),
                Timestamp = DateTime.UtcNow
            };
            
            _recentEvents.Enqueue(fatigueEvent);
            
            // Trim old events
            while (_recentEvents.Count > MAX_RECENT_EVENTS)
            {
                _recentEvents.Dequeue();
            }
            
            // Update category fatigue
            if (!_categoryFatigue.ContainsKey(category))
            {
                _categoryFatigue[category] = 0f;
            }
            _categoryFatigue[category] = Mathf.Min(1f, _categoryFatigue[category] + 0.1f);
            
            // Update entity fatigue
            if (entities != null)
            {
                foreach (var entity in entities)
                {
                    if (!_entityFatigue.ContainsKey(entity))
                    {
                        _entityFatigue[entity] = 0f;
                    }
                    _entityFatigue[entity] = Mathf.Min(1f, _entityFatigue[entity] + 0.15f);
                }
            }
        }
        
        /// <summary>
        /// Get current fatigue modifier for a potential event.
        /// </summary>
        public float GetFatigueModifier(string category, List<string> entities)
        {
            float modifier = 1.0f;
            
            // Category fatigue
            if (_categoryFatigue.TryGetValue(category, out float catFatigue))
            {
                modifier *= (1f - catFatigue * 0.3f);
            }
            
            // Entity fatigue
            if (entities != null)
            {
                foreach (var entity in entities)
                {
                    if (_entityFatigue.TryGetValue(entity, out float entFatigue))
                    {
                        modifier *= (1f - entFatigue * 0.2f);
                    }
                }
            }
            
            // Count similar recent events
            int similarCount = _recentEvents.Count(e => e.Category == category);
            modifier *= Mathf.Max(0.5f, 1f - similarCount * 0.05f);
            
            return modifier;
        }
        
        /// <summary>
        /// Decay fatigue over time (call each turn).
        /// </summary>
        public void DecayFatigue()
        {
            var categories = _categoryFatigue.Keys.ToList();
            foreach (var cat in categories)
            {
                _categoryFatigue[cat] = Mathf.Max(0f, _categoryFatigue[cat] - 0.05f);
            }
            
            var entities = _entityFatigue.Keys.ToList();
            foreach (var ent in entities)
            {
                _entityFatigue[ent] = Mathf.Max(0f, _entityFatigue[ent] - 0.05f);
            }
            
            // Clean up zero-fatigue entries
            foreach (var cat in categories.Where(c => _categoryFatigue[c] == 0f))
            {
                _categoryFatigue.Remove(cat);
            }
            
            foreach (var ent in entities.Where(e => _entityFatigue[e] == 0f))
            {
                _entityFatigue.Remove(ent);
            }
        }
        
        /// <summary>
        /// Reset all fatigue (e.g., for new game).
        /// </summary>
        public void Reset()
        {
            _categoryFatigue.Clear();
            _entityFatigue.Clear();
            _recentEvents.Clear();
        }
        
        private struct FatigueEvent
        {
            public string EventId;
            public string Category;
            public List<string> Entities;
            public DateTime Timestamp;
        }
    }
    
    #region Interfaces & Default Implementation
    
    /// <summary>
    /// Interface for accessing game time information.
    /// </summary>
    public interface IGameTimeProvider
    {
        int GetCurrentTurn();
        DateTime GetGameDateTime();
        float GetTimeScale();
    }
    
    /// <summary>
    /// Default implementation of IGameTimeProvider.
    /// </summary>
    public class DefaultGameTimeProvider : IGameTimeProvider
    {
        public int GetCurrentTurn()
        {
            // Get from GameManager or GameLoop if available
            var gameLoop = UnityEngine.Object.FindFirstObjectByType<Gameplay.GameLoop>();
            if (gameLoop != null)
            {
                var gameState = gameLoop.GetGameState();
                return gameState?.TotalDaysElapsed ?? 0;
            }
            return 0;
        }
        
        public DateTime GetGameDateTime()
        {
            return DateTime.UtcNow;
        }
        
        public float GetTimeScale()
        {
            return 1.0f;
        }
    }
    
    #endregion
}

