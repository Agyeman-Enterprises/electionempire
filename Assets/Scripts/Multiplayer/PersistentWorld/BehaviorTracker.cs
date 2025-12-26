using System;
using System.Collections.Generic;
using UnityEngine;
using ElectionEmpire.Multiplayer.PersistentWorld;

// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - Behavior Tracker
// Tracks player actions in real-time to build behavior profiles
// ═══════════════════════════════════════════════════════════════════════════════

namespace ElectionEmpire.Multiplayer.PersistentWorld
{
    /// <summary>
    /// Tracks player behavior during gameplay to build behavior profiles.
    /// </summary>
    public class BehaviorTracker : MonoBehaviour
    {
        public static BehaviorTracker Instance { get; private set; }
        
        private Dictionary<string, PlayerBehaviorProfile> _activeProfiles;
        private Dictionary<string, List<BehaviorEvent>> _eventHistory;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            _activeProfiles = new Dictionary<string, PlayerBehaviorProfile>();
            _eventHistory = new Dictionary<string, List<BehaviorEvent>>();
        }
        
        /// <summary>
        /// Get or create a behavior profile for a player.
        /// </summary>
        public PlayerBehaviorProfile GetProfile(string playerId)
        {
            if (!_activeProfiles.TryGetValue(playerId, out var profile))
            {
                profile = new PlayerBehaviorProfile();
                _activeProfiles[playerId] = profile;
            }
            
            return profile;
        }
        
        /// <summary>
        /// Track a player decision/action.
        /// </summary>
        public void TrackDecision(string playerId, string actionType, string situation, 
                                 float stressLevel, Dictionary<string, object> context = null)
        {
            var profile = GetProfile(playerId);
            profile.RecordDecision(actionType, situation, stressLevel, context ?? new Dictionary<string, object>());
            
            // Also track in event history
            if (!_eventHistory.ContainsKey(playerId))
                _eventHistory[playerId] = new List<BehaviorEvent>();
            
            _eventHistory[playerId].Add(new BehaviorEvent
            {
                Timestamp = DateTime.Now,
                EventType = "Decision",
                ActionType = actionType,
                Situation = situation,
                StressLevel = stressLevel
            });
        }
        
        /// <summary>
        /// Track a catchphrase.
        /// </summary>
        public void TrackCatchphrase(string playerId, string phrase)
        {
            var profile = GetProfile(playerId);
            profile.RecordCatchphrase(phrase);
        }
        
        /// <summary>
        /// Track a mistake or exploitable pattern.
        /// </summary>
        public void TrackMistake(string playerId, string pattern, float severity)
        {
            var profile = GetProfile(playerId);
            profile.RecordMistake(pattern, severity);
        }
        
        /// <summary>
        /// Track a scandal response.
        /// </summary>
        public void TrackScandalResponse(string playerId, string responseType)
        {
            var profile = GetProfile(playerId);
            profile.RecordScandalResponse(responseType);
        }
        
        /// <summary>
        /// Track an alliance action.
        /// </summary>
        public void TrackAllianceAction(string playerId, bool formed, bool betrayed)
        {
            var profile = GetProfile(playerId);
            profile.RecordAllianceAction(formed, betrayed);
        }
        
        /// <summary>
        /// Finalize a profile when player's term ends.
        /// </summary>
        public PlayerBehaviorProfile FinalizeProfile(string playerId, PlayerTermRecord termRecord)
        {
            if (!_activeProfiles.TryGetValue(playerId, out var profile))
                return null;
            
            // Update final stats
            profile.TotalElectionsWon = termRecord.TermEndYear > termRecord.TermStartYear ? 1 : 0;
            profile.LastActive = DateTime.Now;
            
            // Attach to term record
            termRecord.BehaviorProfile = profile;
            
            return profile;
        }
        
        /// <summary>
        /// Get event history for a player.
        /// </summary>
        public List<BehaviorEvent> GetEventHistory(string playerId, int maxEvents = 100)
        {
            if (!_eventHistory.TryGetValue(playerId, out var events))
                return new List<BehaviorEvent>();
            
            return events.Count > maxEvents 
                ? events.GetRange(events.Count - maxEvents, maxEvents)
                : events;
        }
    }
    
    [Serializable]
    public class BehaviorEvent
    {
        public DateTime Timestamp;
        public string EventType;
        public string ActionType;
        public string Situation;
        public float StressLevel;
        public Dictionary<string, object> Context;
    }
}

