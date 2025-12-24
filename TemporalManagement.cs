// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - Temporal Management System
// Stage 5: News Cycle Management, Time Scaling, and Media Fatigue
// ═══════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionEmpire.News.Temporal
{
    #region Enums & Configuration
    
    /// <summary>
    /// News cycle stages following real-world news lifecycle.
    /// </summary>
    public enum NewsCycleStage
    {
        Breaking,       // 0-2 hours: Maximum urgency, interrupt gameplay
        Developing,     // 2-24 hours: High urgency, prominent display
        Ongoing,        // 1-7 days: Medium urgency, regular rotation
        Fading,         // 7-14 days: Low urgency, background presence
        Archived        // 14+ days: No longer active, historical record
    }
    
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
    /// Manages news cycle state machine for all active events.
    /// </summary>
    public class NewsCycleManager
    {
        private readonly TemporalConfig _config;
        private readonly Dictionary<string, NewsTemporalState> _activeStates;
        private readonly List<NewsTemporalState> _archivedStates;
        private readonly IGameTimeProvider _timeProvider;
        
        private DateTime _lastUpdateTime;
        
        public event Action<string, NewsCycleStage, NewsCycleStage> OnStageTransition;
        public event Action<string> OnEventArchived;
        public event Action<string> OnBreakingNewsInterrupt;
        
        public NewsCycleManager(IGameTimeProvider timeProvider, TemporalConfig config = null)
        {
            _timeProvider = timeProvider;
            _config = config ?? new TemporalConfig();
            _activeStates = new Dictionary<string, NewsTemporalState>();
            _archivedStates = new List<NewsTemporalState>();
            _lastUpdateTime = DateTime.UtcNow;
        }
        
        #region Event Registration
        
        /// <summary>
        /// Register a new event for temporal tracking.
        /// </summary>
        public void RegisterEvent(string eventId, string sourceNewsId, DateTime publishTime)
        {
            if (_activeStates.ContainsKey(eventId))
            {
                Debug.LogWarning($"[NewsCycleManager] Event {eventId} already registered");
                return;
            }
            
            var state = new NewsTemporalState
            {
                EventId = eventId,
                SourceNewsId = sourceNewsId,
                OriginalPublishTime = publishTime,
                CurrentStage = DetermineInitialStage(publishTime),
                StageEnteredAt = DateTime.UtcNow,
                PublicInterest = 1.0f,
                MediaFatigue = 0f
            };
            
            // Check for similar stories
            FindAndLinkSimilarStories(state);
            
            _activeStates[eventId] = state;
            
            // Check if this should interrupt
            if (state.CurrentStage == NewsCycleStage.Breaking)
            {
                state.IsInterrupting = ShouldInterrupt(state);
                if (state.IsInterrupting)
                {
                    OnBreakingNewsInterrupt?.Invoke(eventId);
                }
            }
            
            Debug.Log($"[NewsCycleManager] Registered event {eventId} at stage {state.CurrentStage}");
        }
        
        /// <summary>
        /// Unregister and archive an event.
        /// </summary>
        public void ArchiveEvent(string eventId)
        {
            if (_activeStates.TryGetValue(eventId, out var state))
            {
                state.MarkedForArchive = true;
                state.CurrentStage = NewsCycleStage.Archived;
                _archivedStates.Add(state);
                _activeStates.Remove(eventId);
                OnEventArchived?.Invoke(eventId);
            }
        }
        
        #endregion
        
        #region Stage Management
        
        /// <summary>
        /// Determine initial stage based on publish time.
        /// </summary>
        private NewsCycleStage DetermineInitialStage(DateTime publishTime)
        {
            var age = DateTime.UtcNow - publishTime;
            
            if (age.TotalHours < _config.BreakingDurationHours)
                return NewsCycleStage.Breaking;
            
            if (age.TotalHours < _config.DevelopingDurationHours)
                return NewsCycleStage.Developing;
            
            if (age.TotalDays < _config.OngoingDurationDays)
                return NewsCycleStage.Ongoing;
            
            if (age.TotalDays < _config.FadingDurationDays)
                return NewsCycleStage.Fading;
            
            return NewsCycleStage.Archived;
        }
        
        /// <summary>
        /// Check if event should advance to next stage.
        /// </summary>
        private bool ShouldAdvanceStage(NewsTemporalState state)
        {
            var timeInStage = DateTime.UtcNow - state.StageEnteredAt;
            
            return state.CurrentStage switch
            {
                NewsCycleStage.Breaking => timeInStage.TotalHours >= _config.BreakingDurationHours,
                NewsCycleStage.Developing => timeInStage.TotalHours >= _config.DevelopingDurationHours,
                NewsCycleStage.Ongoing => timeInStage.TotalDays >= _config.OngoingDurationDays,
                NewsCycleStage.Fading => timeInStage.TotalDays >= _config.FadingDurationDays,
                _ => false
            };
        }
        
        /// <summary>
        /// Get the next stage in the cycle.
        /// </summary>
        private NewsCycleStage GetNextStage(NewsCycleStage current)
        {
            return current switch
            {
                NewsCycleStage.Breaking => NewsCycleStage.Developing,
                NewsCycleStage.Developing => NewsCycleStage.Ongoing,
                NewsCycleStage.Ongoing => NewsCycleStage.Fading,
                NewsCycleStage.Fading => NewsCycleStage.Archived,
                _ => NewsCycleStage.Archived
            };
        }
        
        /// <summary>
        /// Advance an event to the next stage.
        /// </summary>
        private void AdvanceStage(NewsTemporalState state)
        {
            var oldStage = state.CurrentStage;
            state.CurrentStage = GetNextStage(oldStage);
            state.StageEnteredAt = DateTime.UtcNow;
            state.TurnsInCurrentStage = 0;
            state.IsInterrupting = false;
            
            OnStageTransition?.Invoke(state.EventId, oldStage, state.CurrentStage);
            
            Debug.Log($"[NewsCycleManager] Event {state.EventId} transitioned: {oldStage} -> {state.CurrentStage}");
        }
        
        #endregion
        
        #region Update Cycle
        
        /// <summary>
        /// Perform a full temporal update cycle.
        /// </summary>
        public TemporalUpdateResult Update()
        {
            var result = new TemporalUpdateResult();
            var now = DateTime.UtcNow;
            var eventsToArchive = new List<string>();
            
            foreach (var kvp in _activeStates)
            {
                var state = kvp.Value;
                
                // Check for stage advancement
                if (ShouldAdvanceStage(state))
                {
                    var oldStage = state.CurrentStage;
                    AdvanceStage(state);
                    result.StageTransitions.Add($"{state.EventId}: {oldStage} -> {state.CurrentStage}");
                    
                    if (state.CurrentStage == NewsCycleStage.Archived)
                    {
                        eventsToArchive.Add(state.EventId);
                    }
                }
                
                // Update fatigue
                UpdateFatigue(state);
                result.UpdatedFatigue[state.EventId] = state.MediaFatigue;
                
                if (state.MediaFatigue > 0.8f)
                {
                    result.FatigueAlerts.Add(state.EventId);
                }
                
                // Check for interrupting events
                if (state.IsInterrupting)
                {
                    result.InterruptingEvents.Add(state.EventId);
                }
            }
            
            // Archive events
            foreach (var eventId in eventsToArchive)
            {
                ArchiveEvent(eventId);
                result.ArchivedEvents.Add(eventId);
            }
            
            // Enforce limits
            EnforceLimits();
            
            // Clean old archives
            CleanOldArchives();
            
            _lastUpdateTime = now;
            return result;
        }
        
        /// <summary>
        /// Update called when a game turn advances.
        /// </summary>
        public TemporalUpdateResult OnTurnAdvance()
        {
            foreach (var state in _activeStates.Values)
            {
                state.TurnsInCurrentStage++;
            }
            
            return Update();
        }
        
        /// <summary>
        /// Enforce maximum event limits.
        /// </summary>
        private void EnforceLimits()
        {
            // Count by stage
            var breakingEvents = _activeStates.Values.Where(s => s.CurrentStage == NewsCycleStage.Breaking).ToList();
            var developingEvents = _activeStates.Values.Where(s => s.CurrentStage == NewsCycleStage.Developing).ToList();
            var ongoingEvents = _activeStates.Values.Where(s => s.CurrentStage == NewsCycleStage.Ongoing).ToList();
            
            // Force advance excess breaking news
            if (breakingEvents.Count > _config.MaxActiveBreakingNews)
            {
                var toAdvance = breakingEvents
                    .OrderBy(s => s.PublicInterest)
                    .Take(breakingEvents.Count - _config.MaxActiveBreakingNews);
                
                foreach (var state in toAdvance)
                {
                    AdvanceStage(state);
                }
            }
            
            // Force advance excess developing news
            if (developingEvents.Count > _config.MaxActiveDevelopingNews)
            {
                var toAdvance = developingEvents
                    .OrderBy(s => s.PublicInterest)
                    .Take(developingEvents.Count - _config.MaxActiveDevelopingNews);
                
                foreach (var state in toAdvance)
                {
                    AdvanceStage(state);
                }
            }
            
            // Archive excess ongoing news
            if (_activeStates.Count > _config.MaxTotalActiveEvents)
            {
                var toArchive = ongoingEvents
                    .OrderByDescending(s => s.MediaFatigue)
                    .Take(_activeStates.Count - _config.MaxTotalActiveEvents)
                    .Select(s => s.EventId)
                    .ToList();
                
                foreach (var eventId in toArchive)
                {
                    ArchiveEvent(eventId);
                }
            }
        }
        
        /// <summary>
        /// Clean old archived events.
        /// </summary>
        private void CleanOldArchives()
        {
            var cutoff = DateTime.UtcNow.AddDays(-_config.ArchiveRetentionDays);
            _archivedStates.RemoveAll(s => s.StageEnteredAt < cutoff);
        }
        
        #endregion
        
        #region Similar Story Detection
        
        /// <summary>
        /// Find and link similar stories for fatigue calculation.
        /// </summary>
        private void FindAndLinkSimilarStories(NewsTemporalState newState)
        {
            // Simple category-based similarity for now
            // Could be enhanced with NLP embedding similarity
            foreach (var existing in _activeStates.Values)
            {
                if (AreSimilarStories(newState, existing))
                {
                    newState.RelatedEventIds.Add(existing.EventId);
                    existing.RelatedEventIds.Add(newState.EventId);
                    existing.SimilarStoriesThisCycle++;
                    newState.SimilarStoriesThisCycle++;
                }
            }
        }
        
        /// <summary>
        /// Check if two events are similar enough to cause fatigue.
        /// </summary>
        private bool AreSimilarStories(NewsTemporalState a, NewsTemporalState b)
        {
            // For now, just check if source news IDs share characteristics
            // In production, would use embedding similarity or category matching
            return false; // Placeholder
        }
        
        #endregion
        
        #region Query Methods
        
        /// <summary>
        /// Get the current stage of an event.
        /// </summary>
        public NewsCycleStage GetEventStage(string eventId)
        {
            return _activeStates.TryGetValue(eventId, out var state) 
                ? state.CurrentStage 
                : NewsCycleStage.Archived;
        }
        
        /// <summary>
        /// Get the temporal state of an event.
        /// </summary>
        public NewsTemporalState GetTemporalState(string eventId)
        {
            return _activeStates.TryGetValue(eventId, out var state) ? state : null;
        }
        
        /// <summary>
        /// Get all events in a specific stage.
        /// </summary>
        public List<string> GetEventsByStage(NewsCycleStage stage)
        {
            return _activeStates.Values
                .Where(s => s.CurrentStage == stage)
                .OrderByDescending(s => s.PublicInterest)
                .Select(s => s.EventId)
                .ToList();
        }
        
        /// <summary>
        /// Get all active event IDs.
        /// </summary>
        public List<string> GetAllActiveEvents()
        {
            return _activeStates.Keys.ToList();
        }
        
        /// <summary>
        /// Check if there are any breaking news events.
        /// </summary>
        public bool HasBreakingNews()
        {
            return _activeStates.Values.Any(s => s.CurrentStage == NewsCycleStage.Breaking);
        }
        
        /// <summary>
        /// Get interrupting events that need immediate attention.
        /// </summary>
        public List<string> GetInterruptingEvents()
        {
            return _activeStates.Values
                .Where(s => s.IsInterrupting)
                .Select(s => s.EventId)
                .ToList();
        }
        
        #endregion
        
        #region Event Interactions
        
        /// <summary>
        /// Record player interaction with an event.
        /// </summary>
        public void RecordPlayerInteraction(string eventId)
        {
            if (_activeStates.TryGetValue(eventId, out var state))
            {
                state.PlayerInteractionCount++;
                state.LastPlayerInteraction = DateTime.UtcNow;
                state.HasPlayerResponded = true;
                state.IsInterrupting = false;
                
                // Reduce fatigue on interaction
                state.MediaFatigue = Mathf.Max(0f, state.MediaFatigue - _config.PlayerActionRecovery);
            }
        }
        
        /// <summary>
        /// Record a new development on an event.
        /// </summary>
        public void RecordDevelopment(string eventId, bool isEscalation = false)
        {
            if (_activeStates.TryGetValue(eventId, out var state))
            {
                state.LastDevelopment = DateTime.UtcNow;
                state.MediaFatigue = Mathf.Max(0f, state.MediaFatigue - _config.NewDevelopmentRecovery);
                state.PublicInterest = Mathf.Min(1f, state.PublicInterest + 0.2f);
                
                if (isEscalation)
                {
                    state.HasEscalated = true;
                    
                    // Escalation can bump back to earlier stage
                    if (state.CurrentStage == NewsCycleStage.Ongoing || 
                        state.CurrentStage == NewsCycleStage.Fading)
                    {
                        var oldStage = state.CurrentStage;
                        state.CurrentStage = NewsCycleStage.Developing;
                        state.StageEnteredAt = DateTime.UtcNow;
                        OnStageTransition?.Invoke(eventId, oldStage, state.CurrentStage);
                    }
                }
            }
        }
        
        /// <summary>
        /// Check if an event should interrupt gameplay.
        /// </summary>
        private bool ShouldInterrupt(NewsTemporalState state)
        {
            // Only breaking news with high public interest interrupts
            if (state.CurrentStage != NewsCycleStage.Breaking)
                return false;
            
            if (state.PublicInterest < 0.7f)
                return false;
            
            // Don't interrupt if we already have max interruptions
            var currentInterrupts = _activeStates.Values.Count(s => s.IsInterrupting);
            return currentInterrupts < _config.MaxActiveBreakingNews;
        }
        
        #endregion
        
        #region Fatigue Calculation
        
        /// <summary>
        /// Update media fatigue for an event.
        /// </summary>
        private void UpdateFatigue(NewsTemporalState state)
        {
            // Base fatigue increase per update
            float fatigueDelta = _config.BaseFatiguePerTurn;
            
            // Penalty for similar stories
            fatigueDelta += state.SimilarStoriesThisCycle * _config.SimilarStoryFatiguePenalty;
            
            // Stage-based fatigue acceleration
            fatigueDelta *= state.CurrentStage switch
            {
                NewsCycleStage.Breaking => 0.5f,    // Breaking news stays fresh longer
                NewsCycleStage.Developing => 0.8f,
                NewsCycleStage.Ongoing => 1.0f,
                NewsCycleStage.Fading => 1.5f,      // Fading news fatigues faster
                _ => 1.0f
            };
            
            // Recent development recovery
            if ((DateTime.UtcNow - state.LastDevelopment).TotalHours < 6)
            {
                fatigueDelta *= 0.5f;
            }
            
            // Apply fatigue
            state.MediaFatigue = Mathf.Clamp01(state.MediaFatigue + fatigueDelta);
            
            // Update public interest inversely to fatigue
            state.PublicInterest = 1f - (state.MediaFatigue * 0.7f);
        }
        
        #endregion
    }
    
    /// <summary>
    /// Handles time scaling between real-world and game time.
    /// </summary>
    public class TimeScaler
    {
        private readonly TemporalConfig _config;
        private readonly IGameTimeProvider _timeProvider;
        
        public TimeScaler(IGameTimeProvider timeProvider, TemporalConfig config = null)
        {
            _timeProvider = timeProvider;
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
                Entities = entities,
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
            foreach (var entity in entities)
            {
                if (!_entityFatigue.ContainsKey(entity))
                {
                    _entityFatigue[entity] = 0f;
                }
                _entityFatigue[entity] = Mathf.Min(1f, _entityFatigue[entity] + 0.15f);
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
    
    #region Interfaces
    
    /// <summary>
    /// Interface for accessing game time information.
    /// </summary>
    public interface IGameTimeProvider
    {
        int GetCurrentTurn();
        DateTime GetGameDateTime();
        float GetTimeScale();
    }
    
    #endregion
}
