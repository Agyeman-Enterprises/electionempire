// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - GAME STATE MANAGER
// Central hub for all game state, turn management, and system coordination
// ═══════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.Gameplay;

namespace ElectionEmpire.Core
{
    /// <summary>
    /// Global world state
    /// </summary>
    [Serializable]
    public class WorldState
    {
        // Economy
        public float EconomyHealth; // 0-100
        public float UnemploymentRate;
        public float InflationRate;
        public float StockMarketIndex;
        
        // Society
        public float NationalMorale; // 0-100
        public float PoliticalPolarization; // 0-100
        public float SocialUnrestLevel; // 0-100
        public float CrimeRate;
        
        // Environment
        public int DisasterRiskLevel; // 0-10
        public List<string> ActiveDisasters;
        
        // International
        public float InternationalStanding; // 0-100
        public List<string> ActiveConflicts;
        public List<string> TradeDeals;
        
        // Political landscape
        public Dictionary<PartyAffiliation, float> PartyPopularity;
        public string CurrentPresident;
        public string RulingParty;
        
        public WorldState()
        {
            EconomyHealth = 60f;
            UnemploymentRate = 5f;
            InflationRate = 2.5f;
            StockMarketIndex = 100f;
            NationalMorale = 55f;
            PoliticalPolarization = 65f;
            SocialUnrestLevel = 30f;
            CrimeRate = 4f;
            DisasterRiskLevel = 3;
            InternationalStanding = 70f;
            ActiveDisasters = new List<string>();
            ActiveConflicts = new List<string>();
            TradeDeals = new List<string>();
            PartyPopularity = new Dictionary<PartyAffiliation, float>
            {
                { PartyAffiliation.Democratic, 45f },
                { PartyAffiliation.Republican, 43f },
                { PartyAffiliation.Independent, 8f },
                { PartyAffiliation.Libertarian, 2f },
                { PartyAffiliation.Green, 2f }
            };
        }
    }
    
    /// <summary>
    /// NPC politician in the world
    /// </summary>
    [Serializable]
    public class NPCPolitician
    {
        public string Id;
        public string Name;
        public PartyAffiliation Party;
        public PoliticalOffice Office;
        public float Approval;
        public float Influence;
        public AlignmentCategory Alignment;
        public List<string> PolicyPositions;
        public bool IsRival;
        public bool IsAlly;
        public float RelationshipWithPlayer;
        
        public NPCPolitician()
        {
            Id = Guid.NewGuid().ToString();
            PolicyPositions = new List<string>();
        }
    }
    
    /// <summary>
    /// Media outlet in the world
    /// </summary>
    [Serializable]
    public class MediaOutlet
    {
        public string Id;
        public string Name;
        public string Type; // "mainstream", "partisan", "social", "independent"
        public float Reach; // Audience size multiplier
        public float Bias; // -100 (left) to +100 (right)
        public float CredibilityScore;
        public float RelationshipWithPlayer;
        public List<string> FocusAreas;
        
        public MediaOutlet()
        {
            Id = Guid.NewGuid().ToString();
            FocusAreas = new List<string>();
        }
    }
    
    /// <summary>
    /// Pending game event
    /// </summary>
    [Serializable]
    public class PendingEvent
    {
        public string Id;
        public string EventType;
        public string Title;
        public int TriggerTurn;
        public int Priority;
        public Dictionary<string, object> EventData;
        public bool IsTriggered;
        
        public PendingEvent()
        {
            Id = Guid.NewGuid().ToString();
            EventData = new Dictionary<string, object>();
        }
    }
    
    /// <summary>
    /// Active campaign state
    /// </summary>
    [Serializable]
    public class CampaignState
    {
        public PoliticalOffice TargetOffice;
        public string OpponentId;
        public string OpponentName;
        public float PlayerPolling;
        public float OpponentPolling;
        public float UndecidedVoters;
        public int TurnsUntilElection;
        public List<string> ActivePromises;
        public List<string> DebatesCompleted;
        public Dictionary<string, float> DemographicPolling;
        public long CampaignSpent;
        public int RalliesHeld;
        public int AdsRun;
        
        public CampaignState()
        {
            ActivePromises = new List<string>();
            DebatesCompleted = new List<string>();
            DemographicPolling = new Dictionary<string, float>();
        }
    }
    
    /// <summary>
    /// Central game state manager
    /// </summary>
    public class GameStateManager
    {
        #region Singleton
        
        private static GameStateManager _instance;
        public static GameStateManager Instance => _instance ??= new GameStateManager();
        
        #endregion
        
        #region State
        
        private GameState _currentState;
        private List<GameState> _saveSlots;
        private bool _isInitialized;
        private float _lastAutoSave;
        
        #endregion
        
        #region Events
        
        public event Action<GameState> OnGameStarted;
        public event Action<GameState> OnGameLoaded;
        public event Action<GameState> OnGameSaved;
        public event Action<GamePhase, GamePhase> OnPhaseChanged;
        public event Action<int> OnTurnAdvanced;
        public event Action<int, int> OnDateChanged; // year, month
        public event Action OnGameOver;
        
        #endregion
        
        #region Properties
        
        public GameState CurrentState => _currentState;
        public bool IsInitialized => _isInitialized;
        public GamePhase CurrentPhase => _currentState?.CurrentPhase ?? GamePhase.CharacterCreation;
        public int CurrentTurn => _currentState?.CurrentTurn ?? 0;
        
        #endregion
        
        #region Initialization
        
        public GameStateManager()
        {
            _saveSlots = new List<GameState>();
        }
        
        /// <summary>
        /// Start a new game
        /// </summary>
        public GameState StartNewGame(string saveName, GameSettings settings = null)
        {
            _currentState = new GameState
            {
                SaveName = saveName,
                Settings = settings ?? new GameSettings()
            };
            
            _currentState.Settings.ApplyDifficultyPreset();
            
            // Initialize world
            InitializeWorld();
            
            // Initialize media
            InitializeMediaOutlets();
            
            // Initialize NPCs
            InitializeNPCPoliticians();
            
            _isInitialized = true;
            _lastAutoSave = Time.time;
            
            OnGameStarted?.Invoke(_currentState);
            
            return _currentState;
        }
        
        private void InitializeWorld()
        {
            // World state is already initialized with defaults
            // Add some randomization
            var world = _currentState.World;
            world.EconomyHealth = UnityEngine.Random.Range(45f, 75f);
            world.PoliticalPolarization = UnityEngine.Random.Range(50f, 80f);
            world.NationalMorale = UnityEngine.Random.Range(40f, 70f);
        }
        
        private void InitializeMediaOutlets()
        {
            _currentState.MediaOutlets = new List<MediaOutlet>
            {
                new MediaOutlet
                {
                    Name = "National News Network",
                    Type = "mainstream",
                    Reach = 1.0f,
                    Bias = 0f,
                    CredibilityScore = 80f,
                    FocusAreas = new List<string> { "politics", "economy", "international" }
                },
                new MediaOutlet
                {
                    Name = "Progressive Post",
                    Type = "partisan",
                    Reach = 0.4f,
                    Bias = -60f,
                    CredibilityScore = 65f,
                    FocusAreas = new List<string> { "social_issues", "environment", "labor" }
                },
                new MediaOutlet
                {
                    Name = "Conservative Chronicle",
                    Type = "partisan",
                    Reach = 0.4f,
                    Bias = 60f,
                    CredibilityScore = 65f,
                    FocusAreas = new List<string> { "economy", "security", "tradition" }
                },
                new MediaOutlet
                {
                    Name = "SocialBuzz",
                    Type = "social",
                    Reach = 0.8f,
                    Bias = -20f,
                    CredibilityScore = 40f,
                    FocusAreas = new List<string> { "viral", "scandal", "personality" }
                },
                new MediaOutlet
                {
                    Name = "Independent Observer",
                    Type = "independent",
                    Reach = 0.2f,
                    Bias = 0f,
                    CredibilityScore = 90f,
                    FocusAreas = new List<string> { "investigation", "policy", "analysis" }
                }
            };
        }
        
        private void InitializeNPCPoliticians()
        {
            // Generate some initial NPCs
            var names = new[] { "John Smith", "Sarah Johnson", "Michael Brown", "Emily Davis", "Robert Wilson" };
            var parties = new[] { PartyAffiliation.Democratic, PartyAffiliation.Republican };
            var offices = new[] { PoliticalOffice.Mayor, PoliticalOffice.StateSenator, PoliticalOffice.Governor };
            
            for (int i = 0; i < 10; i++)
            {
                _currentState.NPCPoliticians.Add(new NPCPolitician
                {
                    Name = names[i % names.Length] + " " + (i + 1),
                    Party = parties[i % parties.Length],
                    Office = offices[i % offices.Length],
                    Approval = UnityEngine.Random.Range(35f, 65f),
                    Influence = UnityEngine.Random.Range(20f, 80f),
                    RelationshipWithPlayer = 50f
                });
            }
        }
        
        #endregion
        
        #region Phase Management
        
        /// <summary>
        /// Transition to a new game phase
        /// </summary>
        public void SetPhase(GamePhase newPhase)
        {
            if (_currentState == null) return;
            
            var oldPhase = _currentState.CurrentPhase;
            _currentState.CurrentPhase = newPhase;
            
            // Phase-specific initialization
            switch (newPhase)
            {
                case GamePhase.Campaign:
                    InitializeCampaignPhase();
                    break;
                case GamePhase.Governance:
                    InitializeGovernancePhase();
                    break;
                case GamePhase.Election:
                    InitializeElectionPhase();
                    break;
                case GamePhase.GameOver:
                    HandleGameOver();
                    break;
            }
            
            OnPhaseChanged?.Invoke(oldPhase, newPhase);
        }
        
        private void InitializeCampaignPhase()
        {
            var character = GameManager.Instance != null ? GameManager.Instance.CurrentCharacter : null;
            if (character == null) return;
            
            _currentState.Campaign = new CampaignState
            {
                TargetOffice = character.CurrentOffice,
                PlayerPolling = 45f,
                OpponentPolling = 45f,
                UndecidedVoters = 10f,
                TurnsUntilElection = 12 // 12 months
            };
            
            // Find or create opponent
            var opponent = _currentState.NPCPoliticians
                .FirstOrDefault(n => n.Office == character.CurrentOffice && n.Party != character.Party);
            
            if (opponent != null)
            {
                _currentState.Campaign.OpponentId = opponent.Id;
                _currentState.Campaign.OpponentName = opponent.Name;
            }
        }
        
        private void InitializeGovernancePhase()
        {
            // Reset campaign state
            _currentState.Campaign = null;
        }
        
        private void InitializeElectionPhase()
        {
            // Final stretch before election
        }
        
        private void HandleGameOver()
        {
            OnGameOver?.Invoke();
        }
        
        #endregion
        
        #region Turn Management
        
        /// <summary>
        /// Advance to next turn
        /// </summary>
        public void AdvanceTurn()
        {
            if (_currentState == null) return;
            
            _currentState.CurrentTurn++;
            
            // Advance date (1 turn = 1 month)
            _currentState.CurrentMonth++;
            if (_currentState.CurrentMonth > 12)
            {
                _currentState.CurrentMonth = 1;
                _currentState.CurrentYear++;
            }
            
            // Process turn
            ProcessTurnEffects();
            
            // Check for pending events
            ProcessPendingEvents();
            
            // Update world
            UpdateWorldState();
            
            // Auto-save check
            CheckAutoSave();
            
            OnTurnAdvanced?.Invoke(_currentState.CurrentTurn);
            OnDateChanged?.Invoke(_currentState.CurrentYear, _currentState.CurrentMonth);
        }
        
        private void ProcessTurnEffects()
        {
            var character = GameManager.Instance != null ? GameManager.Instance.CurrentCharacter : null;
            if (character == null) return;
            
            // Resource decay
            character.Resources.ApplyChange(ResourceType.PoliticalCapital, -2); // -2 per turn
            character.Resources.ApplyChange(ResourceType.MediaInfluence, -5); // -5 per turn
            
            // Campaign burn rate (if in campaign)
            if (_currentState.CurrentPhase == GamePhase.Campaign)
            {
                float burnRate = 0.1f; // 10% base
                long burnAmount = (long)(character.Resources.CampaignFunds * burnRate);
                character.Resources.CampaignFunds -= burnAmount;
                
                if (_currentState.Campaign != null)
                {
                    _currentState.Campaign.CampaignSpent += burnAmount;
                    _currentState.Campaign.TurnsUntilElection--;
                }
            }
            
            // Process active scandals
            foreach (var scandal in character.ActiveScandals.ToList())
            {
                scandal.TurnsActive++;
                
                // Scandals can fade
                if (scandal.PublicAwareness < 10 && scandal.TurnsActive > 6)
                {
                    character.ActiveScandals.Remove(scandal);
                }
            }
            
            // Process active crises
            foreach (var crisis in character.ActiveCrises.ToList())
            {
                crisis.TurnsRemaining--;
                crisis.TurnsToRespond--;
                
                // Escalate if not addressed
                if (crisis.TurnsToRespond <= 0 && crisis.IsEscalating)
                {
                    crisis.Severity = Mathf.Min(crisis.Severity + 1, 10);
                    crisis.TurnsToRespond = 2;
                }
                
                // Crisis ends
                if (crisis.TurnsRemaining <= 0)
                {
                    character.ActiveCrises.Remove(crisis);
                }
            }
            
            character.TotalTurnsPlayed++;
        }
        
        private void ProcessPendingEvents()
        {
            foreach (var pending in _currentState.PendingEvents.ToList())
            {
                if (pending.TriggerTurn <= _currentState.CurrentTurn && !pending.IsTriggered)
                {
                    pending.IsTriggered = true;
                    TriggerEvent(pending);
                }
            }
            
            // Remove triggered events
            _currentState.PendingEvents.RemoveAll(e => e.IsTriggered);
        }
        
        private void TriggerEvent(PendingEvent pending)
        {
            // Event system would handle this
            Debug.Log($"[GameState] Triggering event: {pending.Title}");
        }
        
        private void UpdateWorldState()
        {
            var world = _currentState.World;
            
            // Economy fluctuation
            world.EconomyHealth += UnityEngine.Random.Range(-2f, 2f);
            world.EconomyHealth = Mathf.Clamp(world.EconomyHealth, 20f, 100f);
            
            // Social dynamics
            world.PoliticalPolarization += UnityEngine.Random.Range(-1f, 1.5f);
            world.PoliticalPolarization = Mathf.Clamp(world.PoliticalPolarization, 0f, 100f);
            
            // Random disaster chance
            if (UnityEngine.Random.value < 0.02f * world.DisasterRiskLevel)
            {
                // Trigger disaster event
                QueueEvent("natural_disaster", "Natural Disaster", _currentState.CurrentTurn, 10);
            }
        }
        
        #endregion
        
        #region Event Queue
        
        /// <summary>
        /// Queue an event for future triggering
        /// </summary>
        public void QueueEvent(string eventType, string title, int triggerTurn, int priority = 5)
        {
            _currentState.PendingEvents.Add(new PendingEvent
            {
                EventType = eventType,
                Title = title,
                TriggerTurn = triggerTurn,
                Priority = priority
            });
        }
        
        #endregion
        
        #region Save/Load
        
        /// <summary>
        /// Save current game state
        /// </summary>
        public void SaveGame(int slotIndex = -1)
        {
            if (_currentState == null) return;
            
            _currentState.LastSavedAt = DateTime.UtcNow;
            
            // Save character
            var character = GameManager.Instance != null ? GameManager.Instance.CurrentCharacter : null;
            if (character != null)
            {
                _currentState.ActiveCharacterId = character.Name; // Use Name as ID if Id property doesn't exist
            }
            
            // Store in slot
            if (slotIndex >= 0)
            {
                while (_saveSlots.Count <= slotIndex)
                {
                    _saveSlots.Add(null);
                }
                _saveSlots[slotIndex] = CloneState(_currentState);
            }
            
            // Serialize to PlayerPrefs or file
            string json = JsonUtility.ToJson(_currentState, true);
            PlayerPrefs.SetString($"SaveSlot_{slotIndex}", json);
            PlayerPrefs.Save();
            
            OnGameSaved?.Invoke(_currentState);
            Debug.Log($"[GameState] Game saved to slot {slotIndex}");
        }
        
        /// <summary>
        /// Load game from slot
        /// </summary>
        public GameState LoadGame(int slotIndex)
        {
            string key = $"SaveSlot_{slotIndex}";
            if (!PlayerPrefs.HasKey(key))
            {
                Debug.LogWarning($"[GameState] No save found in slot {slotIndex}");
                return null;
            }
            
            string json = PlayerPrefs.GetString(key);
            _currentState = JsonUtility.FromJson<GameState>(json);
            
            if (_currentState != null)
            {
                _isInitialized = true;
                OnGameLoaded?.Invoke(_currentState);
                Debug.Log($"[GameState] Game loaded from slot {slotIndex}");
            }
            
            return _currentState;
        }
        
        /// <summary>
        /// Check if save exists
        /// </summary>
        public bool SaveExists(int slotIndex)
        {
            return PlayerPrefs.HasKey($"SaveSlot_{slotIndex}");
        }
        
        /// <summary>
        /// Delete save
        /// </summary>
        public void DeleteSave(int slotIndex)
        {
            PlayerPrefs.DeleteKey($"SaveSlot_{slotIndex}");
            if (slotIndex < _saveSlots.Count)
            {
                _saveSlots[slotIndex] = null;
            }
        }
        
        private void CheckAutoSave()
        {
            if (!_currentState.Settings.AutoSaveEnabled) return;
            
            if (_currentState.CurrentTurn % _currentState.Settings.AutoSaveIntervalTurns == 0)
            {
                SaveGame(0); // Auto-save to slot 0
            }
        }
        
        private GameState CloneState(GameState state)
        {
            string json = JsonUtility.ToJson(state);
            return JsonUtility.FromJson<GameState>(json);
        }
        
        #endregion
        
        #region Utility
        
        /// <summary>
        /// Get formatted date string
        /// </summary>
        public string GetDateString()
        {
            if (_currentState == null) return "";
            
            var monthNames = new[] { "", "January", "February", "March", "April", "May", "June",
                "July", "August", "September", "October", "November", "December" };
            
            return $"{monthNames[_currentState.CurrentMonth]} {_currentState.CurrentYear}";
        }
        
        /// <summary>
        /// Get world state summary
        /// </summary>
        public string GetWorldSummary()
        {
            if (_currentState?.World == null) return "";
            
            var w = _currentState.World;
            return $"Economy: {w.EconomyHealth:F0}% | Morale: {w.NationalMorale:F0}% | Polarization: {w.PoliticalPolarization:F0}%";
        }
        
        #endregion
    }
}
