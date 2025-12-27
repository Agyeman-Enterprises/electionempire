using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.AI;
using ElectionEmpire.Gameplay.Presidency;
using ElectionEmpire.Multiplayer.PersistentWorld;
using ElectionEmpire.InfiniteUniverse;

// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - Asynchronous Ghost Manager
// Your ghost plays when you're away - log in during critical moments!
// ═══════════════════════════════════════════════════════════════════════════════

namespace ElectionEmpire.Multiplayer.PersistentWorld
{
    /// <summary>
    /// Manages ghosts that play asynchronously when players are offline
    /// </summary>
    public class AsynchronousGhostManager : MonoBehaviour
    {
        public static AsynchronousGhostManager Instance { get; private set; }
        
        private Dictionary<string, ActiveGhostSession> _activeGhostSessions;
        private PersistentWorldManager _worldManager;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            _activeGhostSessions = new Dictionary<string, ActiveGhostSession>();
            _worldManager = PersistentWorldManager.Instance;
        }
        
        /// <summary>
        /// Activate a ghost to play while player is offline
        /// </summary>
        public void ActivateGhostForPlayer(string playerId, string ghostId, GhostPlayMode mode)
        {
            var ghost = GetGhost(ghostId);
            if (ghost == null) return;
            
            var session = new ActiveGhostSession
            {
                PlayerId = playerId,
                GhostId = ghostId,
                Ghost = ghost,
                Mode = mode,
                ActivatedAt = DateTime.Now,
                LastActionAt = DateTime.Now,
                ActionsTaken = 0,
                CriticalEvents = new List<CriticalEvent>()
            };
            
            _activeGhostSessions[playerId] = session;
            
            Debug.Log($"[AsyncGhost] Activated ghost {ghost.PlayerName} for player {playerId}");
        }
        
        /// <summary>
        /// Process ghost actions while player is offline
        /// </summary>
        public void ProcessGhostTurn(string playerId)
        {
            if (!_activeGhostSessions.TryGetValue(playerId, out var session))
                return;
            
            // Ghost makes decisions based on behavior profile
            var decision = session.Ghost.MakeDecision("ongoing_gameplay", 50f);
            
            // Execute ghost action
            ExecuteGhostAction(session, decision);
            
            session.LastActionAt = DateTime.Now;
            session.ActionsTaken++;
            
            // Check for critical events that need player attention
            CheckForCriticalEvents(session);
        }
        
        /// <summary>
        /// Get critical events that happened while player was away
        /// </summary>
        public List<CriticalEvent> GetCriticalEvents(string playerId)
        {
            if (!_activeGhostSessions.TryGetValue(playerId, out var session))
                return new List<CriticalEvent>();
            
            return session.CriticalEvents;
        }
        
        /// <summary>
        /// Player returns - see what their ghost did
        /// </summary>
        public GhostActivityReport GetActivityReport(string playerId)
        {
            if (!_activeGhostSessions.TryGetValue(playerId, out var session))
                return null;
            
            var report = new GhostActivityReport
            {
                PlayerId = playerId,
                GhostName = session.Ghost.PlayerName,
                TimeAway = DateTime.Now - session.ActivatedAt,
                ActionsTaken = session.ActionsTaken,
                CriticalEvents = new List<CriticalEvent>(session.CriticalEvents),
                DecisionsMade = session.DecisionsMade,
                Outcomes = session.Outcomes
            };
            
            // Clear critical events (player has seen them)
            session.CriticalEvents.Clear();
            
            return report;
        }
        
        /// <summary>
        /// Player takes control back from ghost
        /// </summary>
        public void PlayerReturns(string playerId)
        {
            if (!_activeGhostSessions.TryGetValue(playerId, out var session))
                return;
            
            // Generate activity report
            var report = GetActivityReport(playerId);
            
            // Deactivate ghost
            _activeGhostSessions.Remove(playerId);
            
            Debug.Log($"[AsyncGhost] Player {playerId} returned. Ghost took {session.ActionsTaken} actions.");
        }
        
        private void ExecuteGhostAction(ActiveGhostSession session, string decision)
        {
            // Ghost executes decision based on behavior profile
            session.DecisionsMade.Add(new GhostDecision
            {
                Timestamp = DateTime.Now,
                Decision = decision,
                Context = "Ghost autonomous action"
            });
            
            // Track outcomes
            var outcome = SimulateGhostActionOutcome(session, decision);
            session.Outcomes.Add(outcome);
        }
        
        private GhostActionOutcome SimulateGhostActionOutcome(ActiveGhostSession session, string decision)
        {
            // Simulate what happened
            return new GhostActionOutcome
            {
                Action = decision,
                Success = UnityEngine.Random.value > 0.3f, // 70% success rate
                Impact = UnityEngine.Random.Range(-10f, 20f),
                Description = $"Ghost {session.Ghost.PlayerName} {decision}"
            };
        }
        
        private void CheckForCriticalEvents(ActiveGhostSession session)
        {
            // Check if something critical happened
            if (UnityEngine.Random.value < 0.1f) // 10% chance per turn
            {
                var criticalEvent = new CriticalEvent
                {
                    Type = CriticalEventType.ThreatDetected,
                    Severity = UnityEngine.Random.Range(50f, 100f),
                    Description = "A major threat has emerged!",
                    RequiresPlayerAttention = true,
                    Timestamp = DateTime.Now
                };
                
                session.CriticalEvents.Add(criticalEvent);
                
                // Would send notification to player
                Debug.Log($"[AsyncGhost] CRITICAL EVENT for {session.Ghost.PlayerName}: {criticalEvent.Description}");
            }
        }
        
        private GhostPolitician GetGhost(string ghostId)
        {
            if (_worldManager == null) return null;
            
            var ghosts = _worldManager.GetAvailableGhosts();
            return ghosts.FirstOrDefault(g => g.GhostId == ghostId);
        }
    }
    
    #region Data Structures
    
    public enum GhostPlayMode
    {
        Conservative,    // Ghost plays it safe
        Aggressive,      // Ghost takes risks
        MimicPlayer,     // Ghost mimics original player's style
        Adaptive         // Ghost adapts to situation
    }
    
    [Serializable]
    public class ActiveGhostSession
    {
        public string PlayerId;
        public string GhostId;
        public GhostPolitician Ghost;
        public GhostPlayMode Mode;
        public DateTime ActivatedAt;
        public DateTime LastActionAt;
        public int ActionsTaken;
        public List<CriticalEvent> CriticalEvents;
        public List<GhostDecision> DecisionsMade;
        public List<GhostActionOutcome> Outcomes;
        
        public ActiveGhostSession()
        {
            CriticalEvents = new List<CriticalEvent>();
            DecisionsMade = new List<GhostDecision>();
            Outcomes = new List<GhostActionOutcome>();
        }
    }
    
    [Serializable]
    public class CriticalEvent
    {
        public CriticalEventType Type;
        public float Severity;
        public string Description;
        public bool RequiresPlayerAttention;
        public DateTime Timestamp;
    }
    
    public enum CriticalEventType
    {
        ThreatDetected,
        ScandalBreaking,
        ChallengerAnnounced,
        CrisisEscalated,
        OpportunityArises,
        ImpeachmentRisk
    }
    
    [Serializable]
    public class GhostActivityReport
    {
        public string PlayerId;
        public string GhostName;
        public TimeSpan TimeAway;
        public int ActionsTaken;
        public List<CriticalEvent> CriticalEvents;
        public List<GhostDecision> DecisionsMade;
        public List<GhostActionOutcome> Outcomes;
    }
    [Serializable]
    public class GhostActionOutcome
    {
        public string Action;
        public bool Success;
        public float Impact;
        public string Description;
    }
    
    #endregion
}

