using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.AI;
using ElectionEmpire.Multiplayer.PersistentWorld;

// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - Persistent World Manager
// Main system that manages the shared persistent world and ghost integration
// ═══════════════════════════════════════════════════════════════════════════════

namespace ElectionEmpire.Multiplayer.PersistentWorld
{
    /// <summary>
    /// Main manager for the persistent world system.
    /// Handles world state, ghost generation, and player behavior tracking.
    /// </summary>
    public class PersistentWorldManager : MonoBehaviour
    {
        public static PersistentWorldManager Instance { get; private set; }
        
        [Header("Configuration")]
        public bool EnablePersistentWorld = true;
        public string WorldSavePath = "PersistentWorld.json";
        
        // Core Systems
        private PersistentWorldState _worldState;
        private GhostManager _ghostManager;
        private BehaviorTracker _behaviorTracker;

        // Public accessors
        public PersistentWorldState WorldState => _worldState;
        
        // Events
        public event Action<PersistentWorldState> OnWorldStateUpdated;
        public event Action<GhostPolitician> OnGhostCreated;
        public event Action<string> OnPlayerTermEnded;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            Initialize();
        }
        
        private void Initialize()
        {
            if (!EnablePersistentWorld)
            {
                Debug.Log("[PersistentWorld] Persistent world disabled");
                return;
            }
            
            // Load or create world state
            _worldState = LoadWorldState() ?? new PersistentWorldState();
            
            // Initialize managers
            _ghostManager = new GhostManager(_worldState);
            _behaviorTracker = BehaviorTracker.Instance ?? gameObject.AddComponent<BehaviorTracker>();
            
            Debug.Log($"[PersistentWorld] Initialized. Year: {_worldState.CurrentYear}, " +
                     $"Presidents: {_worldState.FormerPresidents.Count}, " +
                     $"Corruption: {_worldState.WorldCorruption:F1}%");
        }
        
        /// <summary>
        /// Get opponents for a new player, including ghosts.
        /// </summary>
        public List<AIOpponent> GetOpponentsForPlayer(string playerId, int officeTier)
        {
            var opponents = new List<AIOpponent>();
            
            // Get ghosts as opponents
            if (_ghostManager != null)
            {
                var ghostOpponents = _ghostManager.GetOpponentsForNewPlayer(
                    _worldState.CurrentYear, officeTier);
                opponents.AddRange(ghostOpponents);
            }
            
            // Also generate some regular AI opponents
            // (This would call your existing AI generation system)
            
            return opponents;
        }
        
        /// <summary>
        /// Record that a player's term has ended.
        /// </summary>
        public void RecordPlayerTermEnd(string playerId, string playerName, PlayerTermRecord record)
        {
            if (!EnablePersistentWorld) return;
            
            // Finalize behavior profile
            var profile = _behaviorTracker?.FinalizeProfile(playerId, record);
            if (profile != null)
            {
                record.BehaviorProfile = profile;
            }
            
            // Apply effects to world
            _worldState.ApplyPlayerTermEffects(record);
            
            // Advance year
            _worldState.CurrentYear += (record.TermEndYear - record.TermStartYear);
            
            // Save world state
            SaveWorldState();
            
            OnPlayerTermEnded?.Invoke(playerId);
            OnWorldStateUpdated?.Invoke(_worldState);
            
            Debug.Log($"[PersistentWorld] Recorded term for {playerName}. " +
                     $"World corruption now: {_worldState.WorldCorruption:F1}%");
        }
        
        /// <summary>
        /// Get the current world state description.
        /// </summary>
        public string GetWorldStateDescription()
        {
            return _worldState?.GetWorldStateDescription() ?? "The world is new.";
        }
        
        /// <summary>
        /// Study a ghost to learn their weaknesses.
        /// </summary>
        public GhostStudyResult StudyGhost(string ghostId)
        {
            var ghost = _worldState.FormerPresidents
                .FirstOrDefault(g => g.GhostId == ghostId) as GhostPolitician
                ?? _worldState.FormerPoliticians
                .FirstOrDefault(g => g.GhostId == ghostId);
            
            if (ghost == null) return null;
            
            return _ghostManager?.StudyGhost(ghost);
        }
        
        /// <summary>
        /// Get all available ghosts for study.
        /// </summary>
        public List<GhostPolitician> GetAvailableGhosts()
        {
            var ghosts = new List<GhostPolitician>();
            ghosts.AddRange(_worldState.FormerPresidents);
            ghosts.AddRange(_worldState.FormerPoliticians);
            return ghosts;
        }
        
        /// <summary>
        /// Load world state from disk.
        /// </summary>
        private PersistentWorldState LoadWorldState()
        {
            try
            {
                string path = System.IO.Path.Combine(Application.persistentDataPath, WorldSavePath);
                if (System.IO.File.Exists(path))
                {
                    string json = System.IO.File.ReadAllText(path);
                    return JsonUtility.FromJson<PersistentWorldState>(json);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PersistentWorld] Failed to load world state: {ex.Message}");
            }
            
            return null;
        }
        
        /// <summary>
        /// Save world state to disk.
        /// </summary>
        private void SaveWorldState()
        {
            try
            {
                string path = System.IO.Path.Combine(Application.persistentDataPath, WorldSavePath);
                string json = JsonUtility.ToJson(_worldState, true);
                System.IO.File.WriteAllText(path, json);
                Debug.Log($"[PersistentWorld] World state saved to {path}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PersistentWorld] Failed to save world state: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Get current world metrics.
        /// </summary>
        public WorldMetrics GetWorldMetrics()
        {
            return new WorldMetrics
            {
                Year = _worldState.CurrentYear,
                Corruption = _worldState.WorldCorruption,
                VoterCynicism = _worldState.VoterCynicism,
                MediaCredibility = _worldState.MediaCredibility,
                EconomicStability = _worldState.EconomicStability,
                SocialCohesion = _worldState.SocialCohesion,
                PoliticalPolarization = _worldState.PoliticalPolarization,
                TotalPresidents = _worldState.FormerPresidents.Count,
                TotalPoliticians = _worldState.FormerPoliticians.Count,
                ActivePolicies = _worldState.ActivePolicies.Count
            };
        }
    }
    
    [Serializable]
    public class WorldMetrics
    {
        public int Year;
        public float Corruption;
        public float VoterCynicism;
        public float MediaCredibility;
        public float EconomicStability;
        public float SocialCohesion;
        public float PoliticalPolarization;
        public int TotalPresidents;
        public int TotalPoliticians;
        public int ActivePolicies;
    }
}

