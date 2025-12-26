using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.Multiplayer.PersistentWorld;

// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - WORLD SERVER & GHOST AI SYSTEM
// Persistent worlds, player matching, AI that plays like humans did
// ═══════════════════════════════════════════════════════════════════════════════

namespace ElectionEmpire.InfiniteUniverse
{
    #region World Server System
    
    /// <summary>
    /// Manages persistent political worlds and player connections
    /// </summary>
    public class WorldServerManager : MonoBehaviour
    {
        public static WorldServerManager Instance { get; private set; }
        
        // Active worlds
        public List<ExtendedWorldState> ActiveWorlds { get; private set; }
        public ExtendedWorldState CurrentWorld { get; private set; }
        
        // Player's position in the world
        public string LocalPlayerId { get; private set; }
        public ExtendedGhostPolitician LocalPlayerGhost { get; private set; }
        public bool IsCurrentPresident => CurrentWorld?.CurrentPresidentPlayerId == LocalPlayerId;
        
        // Events
        public event Action<ExtendedWorldState> OnWorldJoined;
        public event Action<ExtendedGhostPolitician> OnGhostEncountered;
        public event Action<string> OnWorldEvent;
        public event Action<ActiveChallenger> OnChallengerApproaching;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ActiveWorlds = new List<ExtendedWorldState>();
        }
        
        /// <summary>
        /// Find or create a world for the player to join
        /// </summary>
        public ExtendedWorldState FindWorld(WorldMatchCriteria criteria)
        {
            // Try to find an existing world matching criteria
            var suitableWorld = ActiveWorlds
                .Where(w => WorldMatchesCriteria(w, criteria))
                .OrderByDescending(w => CalculateWorldInterestScore(w, criteria))
                .FirstOrDefault();
            
            if (suitableWorld != null)
            {
                return suitableWorld;
            }
            
            // Create new world if none found
            return CreateNewWorld(criteria);
        }
        
        /// <summary>
        /// Join a specific world
        /// </summary>
        public void JoinWorld(ExtendedWorldState world, string playerId)
        {
            CurrentWorld = world;
            LocalPlayerId = playerId;
            
            // Check if we have a ghost in this world
            LocalPlayerGhost = world.ActivePoliticians
                .FirstOrDefault(g => g.OriginalPlayerId == playerId);
            
            if (LocalPlayerGhost != null)
            {
                // Resuming as existing character
                LocalPlayerGhost.IsActivelyControlled = true;
                LocalPlayerGhost.LastPlayerControl = DateTime.UtcNow;
            }
            
            world.TotalPlayersContributed++;
            OnWorldJoined?.Invoke(world);
            
            Debug.Log($"Joined world {world.WorldId} - Year {world.YearInWorld}");
            Debug.Log($"Current President: {GetPresidentName(world)}");
            Debug.Log($"Your Position: {(LocalPlayerGhost != null ? LocalPlayerGhost.SpecificOffice : "New Challenger")}");
        }
        
        /// <summary>
        /// Leave current world (ghost takes over)
        /// </summary>
        public void LeaveWorld()
        {
            if (LocalPlayerGhost != null)
            {
                LocalPlayerGhost.IsActivelyControlled = false;
                LocalPlayerGhost.LastPlayerControl = DateTime.UtcNow;
                Debug.Log($"Ghost AI taking over for {LocalPlayerGhost.CharacterName}");
            }
            
            CurrentWorld = null;
            LocalPlayerId = null;
        }
        
        /// <summary>
        /// Get list of ghost politicians in current world
        /// </summary>
        public List<ExtendedGhostPolitician> GetActiveGhosts()
        {
            return CurrentWorld?.ActivePoliticians ?? new List<ExtendedGhostPolitician>();
        }
        
        /// <summary>
        /// Get the current world's president (may be ghost or active player)
        /// </summary>
        public (string Name, bool IsPlayer, bool IsGhost) GetCurrentPresident()
        {
            if (CurrentWorld == null) return ("None", false, false);
            
            if (CurrentWorld.PresidentIsPlayer)
            {
                var ghost = CurrentWorld.ActivePoliticians
                    .FirstOrDefault(g => g.Id == CurrentWorld.CurrentPresidentId);
                return (ghost?.CharacterName ?? "Unknown", 
                       ghost?.IsActivelyControlled ?? false,
                       true);
            }
            
            // AI president from history
            var historical = CurrentWorld.PresidentialHistory
                .FirstOrDefault(h => h.Id == CurrentWorld.CurrentPresidentId);
            return (historical?.Name ?? "Unknown", false, false);
        }
        
        /// <summary>
        /// Sync local state with server
        /// </summary>
        public void SyncWithServer()
        {
            // Would send/receive updates from actual server
            // For now, process local simulation
            ProcessWorldTick();
        }
        
        private void ProcessWorldTick()
        {
            if (CurrentWorld == null) return;
            
            // Advance world time
            CurrentWorld.YearInWorld++;
            
            // Process ghost actions
            foreach (var ghost in CurrentWorld.ActivePoliticians.Where(g => !g.IsActivelyControlled))
            {
                ProcessGhostTurn(ghost);
            }
        }
        
        private void ProcessGhostTurn(ExtendedGhostPolitician ghost)
        {
            // Ghost AI makes decisions based on learned behavior
            var ai = GhostAISystem.Instance;
            if (ai != null)
            {
                ai.ProcessGhostDecision(ghost, CurrentWorld);
            }
        }
        
        private bool WorldMatchesCriteria(ExtendedWorldState world, WorldMatchCriteria criteria)
        {
            // Check if world is suitable for this player
            if (criteria.PreferNewWorld && world.TotalPlayersContributed > 0) return false;
            if (criteria.WantCompetition && world.ActivePoliticians.Count < 5) return false;
            if (criteria.WantEasyStart && world.CorruptionLevel > 50) return false;
            
            return true;
        }
        
        private float CalculateWorldInterestScore(ExtendedWorldState world, WorldMatchCriteria criteria)
        {
            float score = 50f;
            
            // More players = more interesting
            score += world.ActivePoliticians.Count * 5f;
            
            // Rich history = more interesting
            score += world.PresidentialHistory.Count * 3f;
            
            // Current drama = more interesting
            score += world.OverallPolarization * 0.3f;
            
            return score;
        }
        
        private ExtendedWorldState CreateNewWorld(WorldMatchCriteria criteria)
        {
            var world = new ExtendedWorldState();
            
            // Initialize default values
            world.IdeologicalBalance = new Dictionary<string, float>
            {
                { "economic", 50f },
                { "social", 50f },
                { "foreign", 50f }
            };
            
            world.InstitutionalHealth = new Dictionary<string, float>
            {
                { "congress", 60f },
                { "courts", 70f },
                { "media", 50f },
                { "military", 80f }
            };
            
            world.OverallPolarization = 40f;
            world.CorruptionLevel = 30f;
            world.MediaTrust = 45f;
            world.DemocraticNorms = 75f;
            
            // Generate initial AI president (or empty seat)
            GenerateInitialPresident(world);
            
            // Seed some ghost politicians from global pool
            SeedInitialGhosts(world, criteria.DesiredGhostCount);
            
            ActiveWorlds.Add(world);
            return world;
        }
        
        private void GenerateInitialPresident(ExtendedWorldState world)
        {
            // Create an AI president to start
            var president = new HistoricalPresident
            {
                Id = Guid.NewGuid().ToString("N").Substring(0, 8),
                Name = GeneratePresidentName(),
                YearElected = world.YearInWorld - 2,
                AverageApproval = UnityEngine.Random.Range(35f, 55f),
                LegacyReputation = "Current incumbent",
                StillAlive = true,
                OngoingInfluence = 100f
            };
            
            world.PresidentialHistory.Add(president);
            world.CurrentPresidentId = president.Id;
            world.PresidentIsPlayer = false;
        }
        
        private void SeedInitialGhosts(ExtendedWorldState world, int count)
        {
            // Would pull from global ghost pool
            // For now, generate some AI politicians
            for (int i = 0; i < count; i++)
            {
                var tier = UnityEngine.Random.Range(1, 5);
                var ghost = GenerateAIGhost(tier);
                world.ActivePoliticians.Add(ghost);
            }
        }
        
        private ExtendedGhostPolitician GenerateAIGhost(int tier)
        {
            return new ExtendedGhostPolitician
            {
                Id = Guid.NewGuid().ToString("N").Substring(0, 8),
                CharacterName = GeneratePoliticianName(),
                CurrentOffice = tier,
                SpecificOffice = GetOfficeForTier(tier),
                IsActivelyControlled = false,
                CurrentApproval = UnityEngine.Random.Range(40f, 60f),
                BehaviorProfile = GenerateRandomBehaviorProfile(),
                AIBehavior = GhostAIBehavior.FromBehaviorProfile(GenerateRandomBehaviorProfile())
            };
        }
        
        private PlayerBehaviorProfile GenerateRandomBehaviorProfile()
        {
            return new PlayerBehaviorProfile
            {
                DirtyTricksWillingness = UnityEngine.Random.Range(0.1f, 0.8f),
                ScandalResponseStyle = UnityEngine.Random.Range(0.2f, 0.8f),
                LoyaltyToAllies = UnityEngine.Random.Range(0.3f, 0.9f),
                PolicyConsistency = UnityEngine.Random.Range(0.4f, 0.9f),
                RiskTolerance = UnityEngine.Random.Range(0.2f, 0.7f),
                AggressionInDebates = UnityEngine.Random.Range(0.3f, 0.8f)
            };
        }
        
        private string GetPresidentName(ExtendedWorldState world)
        {
            if (world.PresidentIsPlayer)
            {
                return world.ActivePoliticians
                    .FirstOrDefault(g => g.Id == world.CurrentPresidentId)?.CharacterName ?? "Unknown";
            }
            return world.PresidentialHistory
                .FirstOrDefault(h => h.Id == world.CurrentPresidentId)?.Name ?? "Unknown";
        }
        
        private string GeneratePresidentName()
        {
            var firstNames = new[] { "James", "William", "John", "Robert", "Michael", 
                                     "Elizabeth", "Margaret", "Sarah", "Jennifer", "Patricia" };
            var lastNames = new[] { "Harrison", "Mitchell", "Anderson", "Thompson", "Wilson",
                                    "Martinez", "Garcia", "Rodriguez", "Chen", "Patel" };
            return $"{firstNames[UnityEngine.Random.Range(0, firstNames.Length)]} " +
                   $"{lastNames[UnityEngine.Random.Range(0, lastNames.Length)]}";
        }
        
        private string GeneratePoliticianName()
        {
            return GeneratePresidentName(); // Same generator for now
        }
        
        private string GetOfficeForTier(int tier)
        {
            return tier switch
            {
                1 => new[] { "City Council", "School Board", "County Commissioner" }
                    [UnityEngine.Random.Range(0, 3)],
                2 => new[] { "Mayor", "District Attorney", "State Representative" }
                    [UnityEngine.Random.Range(0, 3)],
                3 => new[] { "State Senator", "Attorney General", "Lieutenant Governor" }
                    [UnityEngine.Random.Range(0, 3)],
                4 => new[] { "Governor", "U.S. Representative", "U.S. Senator" }
                    [UnityEngine.Random.Range(0, 3)],
                5 => "President",
                _ => "Unknown Office"
            };
        }
    }
    
    /// <summary>
    /// Criteria for finding/creating a world
    /// </summary>
    [Serializable]
    public class WorldMatchCriteria
    {
        public bool PreferNewWorld;
        public bool WantCompetition;
        public bool WantEasyStart;
        public int DesiredGhostCount = 10;
        public string PreferredRegion;
        public float MaxPolarization = 100;
        public float MaxCorruption = 100;
    }
    
    #endregion
    
    #region Extended Data Structures
    
    /// <summary>
    /// Extended world state with server-specific properties
    /// </summary>
    [Serializable]
    public class ExtendedWorldState
    {
        public string WorldId;
        public int YearInWorld;
        public int TotalPlayersContributed;
        
        // Current State
        public string CurrentPresidentId;
        public bool PresidentIsPlayer;
        public string CurrentPresidentPlayerId;
        
        // Active Politicians (ghosts and players)
        public List<ExtendedGhostPolitician> ActivePoliticians;
        
        // Historical Presidents
        public List<HistoricalPresident> PresidentialHistory;
        
        // World Metrics
        public float CorruptionLevel;
        public float OverallPolarization;
        public float MediaTrust;
        public float DemocraticNorms;
        
        // Ideological Balance
        public Dictionary<string, float> IdeologicalBalance;
        
        // Institutional Health
        public Dictionary<string, float> InstitutionalHealth;
        
        public ExtendedWorldState()
        {
            WorldId = Guid.NewGuid().ToString("N").Substring(0, 8);
            YearInWorld = 2025;
            ActivePoliticians = new List<ExtendedGhostPolitician>();
            PresidentialHistory = new List<HistoricalPresident>();
            IdeologicalBalance = new Dictionary<string, float>();
            InstitutionalHealth = new Dictionary<string, float>();
        }
    }
    
    /// <summary>
    /// Extended ghost politician with server-specific properties
    /// </summary>
    [Serializable]
    public class ExtendedGhostPolitician : GhostPolitician
    {
        // Server-specific properties
        public int CurrentOffice; // Tier 1-5
        public string SpecificOffice;
        public bool IsActivelyControlled;
        public DateTime LastPlayerControl;
        public float CurrentApproval;
        public GhostAIBehavior AIBehavior;
        public string OriginalPlayerId; // Additional tracking
        public List<string> ActiveScandals;
        
        // Property accessors to map to base class
        public string Id
        {
            get => GhostId;
            set => GhostId = value;
        }
        
        public string CharacterName
        {
            get => PlayerName;
            set => PlayerName = value;
        }
        
        public ExtendedGhostPolitician()
        {
            GhostId = Guid.NewGuid().ToString("N").Substring(0, 8);
            ActiveScandals = new List<string>();
            OriginalPlayerId = PlayerId; // Sync with base
        }
    }
    
    /// <summary>
    /// Historical president record
    /// </summary>
    [Serializable]
    public class HistoricalPresident
    {
        public string Id;
        public string Name;
        public int YearElected;
        public float AverageApproval;
        public string LegacyReputation;
        public bool StillAlive;
        public float OngoingInfluence;
    }
    
    /// <summary>
    /// Active challenger approaching the player
    /// </summary>
    [Serializable]
    public class ActiveChallenger
    {
        public string ChallengerId;
        public string ChallengerName;
        public int OfficeTier;
        public float ThreatLevel;
        public string ThreatType;
        public DateTime ThreatStartTime;
    }
    
    #endregion
}

