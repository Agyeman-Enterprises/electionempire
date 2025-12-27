// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - Game Manager & System Integration
// Sprint 11: Central game manager, system bootstrapping, state machine
// ═══════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.Balance;
using ElectionEmpire.Monetization;
using ElectionEmpire.Core;
using ElectionEmpire.UI.Screens;
using ElectionEmpire.Economy;

namespace ElectionEmpire.Core
{
    #region Game State
    
    /// <summary>
    /// High-level game states.
    /// </summary>
    public enum GameStateType
    {
        Initializing,
        MainMenu,
        Loading,
        CharacterCreation,
        Playing,
        Paused,
        Election,
        GameOver,
        Credits
    }
    
    /// <summary>
    /// Playing sub-states.
    /// </summary>
    public enum PlayingState
    {
        Campaign,
        Governance,
        Crisis,
        Scandal,
        Debate,
        Event,
        TurnTransition
    }
    
    /// <summary>
    /// Game phase within a political term.
    /// Note: This conflicts with ElectionEmpire.Core.GamePhase - consider removing or renaming.
    /// </summary>
    public enum PoliticalTermPhase
    {
        Exploratory,        // Testing the waters
        Primary,            // Party nomination
        GeneralCampaign,    // Full campaign mode
        ElectionDay,        // Voting
        Transition,         // Post-election
        Governance,         // In office
        ReElection          // Running for another term
    }
    
    #endregion
    
    #region Game Configuration
    
    /// <summary>
    /// Game configuration settings.
    /// </summary>
    [Serializable]
    public class GameConfiguration
    {
        // Game settings
        public DifficultyLevel Difficulty = DifficultyLevel.Normal;
        public bool ChaosMode = false;
        public float GameSpeed = 1f;
        public bool AutosaveEnabled = true;
        public int AutosaveIntervalTurns = 5;
        
        // Display settings
        public int TargetFrameRate = 60;
        public bool VSync = true;
        public int ResolutionWidth = 1920;
        public int ResolutionHeight = 1080;
        public bool Fullscreen = true;
        
        // Audio settings
        public float MasterVolume = 1f;
        public float MusicVolume = 0.8f;
        public float SFXVolume = 1f;
        public float UIVolume = 1f;
        
        // Accessibility
        public bool ScreenShakeEnabled = true;
        public bool FlashEffectsEnabled = true;
        public float TextSize = 1f;
        public bool HighContrastMode = false;
        public bool ColorBlindMode = false;
        
        // Privacy
        public bool AnalyticsEnabled = true;
        public bool CrashReportingEnabled = true;
        
        // Tutorials
        public bool ShowTutorials = true;
        public bool ShowHints = true;
    }
    
    /// <summary>
    /// Current game session data.
    /// </summary>
    [Serializable]
    public class GameSession
    {
        public string SessionId;
        public DateTime StartTime;
        public int TurnNumber;
        public GamePhase CurrentPhase;
        public int CurrentTier;
        public string CurrentOfficeId;
        public int ElectionYear;
        public bool IsChaosMode;
        
        // Quick stats
        public int ElectionsWon;
        public int ElectionsLost;
        public int ScandalsHandled;
        public int CrisesResolved;
        public float TotalPlaytimeMinutes;
        
        public GameSession()
        {
            SessionId = Guid.NewGuid().ToString();
            StartTime = DateTime.UtcNow;
        }
    }
    
    #endregion
    
    #region Game Manager Integration
    
    /// <summary>
    /// Central game manager - singleton that orchestrates all systems.
    /// This extends the existing GameManager with Sprint 11 systems.
    /// </summary>
    public class GameManagerIntegration : MonoBehaviour
    {
        public static GameManagerIntegration Instance { get; private set; }
        
        [Header("Configuration")]
        [SerializeField] private GameConfiguration config;
        
        // Core state
        private GameStateType currentState = GameStateType.Initializing;
        private PlayingState playingState = PlayingState.Campaign;
        private GameSession currentSession;
        
        // System references
        public DifficultyManager DifficultyManager { get; private set; }
        public BalanceDataManager BalanceManager { get; private set; }
        public AnalyticsManager Analytics { get; private set; }
        public Balance.EnhancedSaveManager EnhancedSaveManager { get; private set; }
        public TutorialManager TutorialManager { get; private set; }
        public CurrencyManager CurrencyManager { get; private set; }
        public IAPManager IAPManager { get; private set; }
        public AchievementManager AchievementManager { get; private set; }
        public AudioManager AudioManager { get; private set; }
        public PolishEffectsManager EffectsManager { get; private set; }
        
        // Player data
        public PlayerInventory PlayerInventory { get; private set; }
        public CosmeticsShop CosmeticsShop { get; private set; }
        
        // Events
        public event Action<GameStateType> OnStateChanged;
        public event Action<PlayingState> OnPlayingStateChanged;
        public event Action<GamePhase> OnPhaseChanged;
        public event Action<int> OnTurnAdvanced;
        public event Action OnGameStarted;
        public event Action<bool> OnGameEnded;
        
        // Properties
        public GameStateType CurrentState => currentState;
        public PlayingState CurrentPlayingState => playingState;
        public GameSession CurrentSession => currentSession;
        public GameConfiguration Config => config;
        public bool IsPlaying => currentState == GameStateType.Playing;
        public bool IsPaused => currentState == GameStateType.Paused;
        
        #region Initialization
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeSystems();
        }
        
        private void InitializeSystems()
        {
            Debug.Log("[GameManagerIntegration] Initializing systems...");
            
            // Load configuration
            LoadConfiguration();
            
            // Initialize core systems
            DifficultyManager = new DifficultyManager();
            BalanceManager = new BalanceDataManager();
            Analytics = new AnalyticsManager();
            EnhancedSaveManager = new Balance.EnhancedSaveManager();
            TutorialManager = new TutorialManager();
            
            // Initialize monetization
            CurrencyManager = new CurrencyManager();
            PlayerInventory = new PlayerInventory { PlayerId = GetPlayerId() };
            IAPManager = new IAPManager(CurrencyManager, PlayerInventory);
            AchievementManager = new AchievementManager(CurrencyManager, PlayerInventory);
            CosmeticsShop = new CosmeticsShop(PlayerInventory, CurrencyManager);
            
            // Find component-based managers
            AudioManager = FindFirstObjectByType<AudioManager>();
            EffectsManager = FindFirstObjectByType<PolishEffectsManager>();
            
            // Apply configuration
            ApplyConfiguration();
            
            // Connect events
            ConnectSystemEvents();
            
            // Start analytics session
            Analytics.SetEnabled(config.AnalyticsEnabled);
            Analytics.StartSession(Application.version, config.Difficulty);
            
            Debug.Log("[GameManagerIntegration] Systems initialized successfully");
            
            ChangeState(GameStateType.MainMenu);
        }
        
        private void LoadConfiguration()
        {
            string json = PlayerPrefs.GetString("GameConfiguration", "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    config = JsonUtility.FromJson<GameConfiguration>(json);
                }
                catch
                {
                    config = new GameConfiguration();
                }
            }
            else
            {
                config = new GameConfiguration();
            }
        }
        
        public void SaveConfiguration()
        {
            string json = JsonUtility.ToJson(config);
            PlayerPrefs.SetString("GameConfiguration", json);
            PlayerPrefs.Save();
        }
        
        private void ApplyConfiguration()
        {
            // Frame rate
            Application.targetFrameRate = config.TargetFrameRate;
            QualitySettings.vSyncCount = config.VSync ? 1 : 0;
            
            // Resolution
            Screen.SetResolution(config.ResolutionWidth, config.ResolutionHeight, config.Fullscreen);
            
            // Audio
            if (AudioManager != null)
            {
                AudioManager.SetMasterVolume(config.MasterVolume);
                AudioManager.SetMusicVolume(config.MusicVolume);
                AudioManager.SetSFXVolume(config.SFXVolume);
            }
            
            // Difficulty
            DifficultyManager.SetDifficulty(config.Difficulty);
            
            // Effects
            if (EffectsManager != null)
            {
                // Effects manager would check config for enabled states
            }
        }
        
        private void ConnectSystemEvents()
        {
            // Achievement events
            AchievementManager.OnAchievementUnlocked += OnAchievementUnlocked;
            
            // Currency events
            CurrencyManager.OnCurrencyChanged += OnCurrencyChanged;
            
            // Save events
            EnhancedSaveManager.OnSaveCompleted += OnSaveCompleted;
            EnhancedSaveManager.OnLoadCompleted += OnLoadCompleted;
            
            // IAP events
            IAPManager.OnPurchaseCompleted += (purchase) => OnPurchaseCompleted(purchase);
        }
        
        private string GetPlayerId()
        {
            string id = PlayerPrefs.GetString("PlayerId", "");
            if (string.IsNullOrEmpty(id))
            {
                id = Guid.NewGuid().ToString();
                PlayerPrefs.SetString("PlayerId", id);
                PlayerPrefs.Save();
            }
            return id;
        }
        
        #endregion
        
        #region State Management
        
        public void ChangeState(GameStateType newState)
        {
            if (newState == currentState) return;
            
            GameStateType previousState = currentState;
            currentState = newState;
            
            Debug.Log($"[GameManagerIntegration] State changed: {previousState} -> {newState}");
            
            // Handle state transitions
            OnExitState(previousState);
            OnEnterState(newState);
            
            OnStateChanged?.Invoke(newState);
            
            // Analytics
            Analytics.LogEvent(AnalyticsEventType.FeatureUsed, new Dictionary<string, object>
            {
                { "feature", "state_change" },
                { "from", previousState.ToString() },
                { "to", newState.ToString() }
            });
        }
        
        private void OnExitState(GameStateType state)
        {
            switch (state)
            {
                case GameStateType.Playing:
                    // Pause game time if needed
                    break;
                    
                case GameStateType.MainMenu:
                    // Clean up menu resources
                    break;
            }
        }
        
        private void OnEnterState(GameStateType state)
        {
            switch (state)
            {
                case GameStateType.MainMenu:
                    AudioManager?.PlayMusic(MusicTrack.MainMenu);
                    break;
                    
                case GameStateType.CharacterCreation:
                    if (config.ShowTutorials && !TutorialManager.IsSequenceCompleted("basic_gameplay"))
                    {
                        TutorialManager.StartSequence("basic_gameplay");
                    }
                    break;
                    
                case GameStateType.Playing:
                    AudioManager?.PlayMusic(MusicTrack.Campaign_Upbeat);
                    break;
                    
                case GameStateType.Paused:
                    Time.timeScale = 0f;
                    break;
                    
                case GameStateType.GameOver:
                    HandleGameOver();
                    break;
            }
        }
        
        public void ChangePlayingState(PlayingState newState)
        {
            if (currentState != GameStateType.Playing) return;
            if (newState == playingState) return;
            
            playingState = newState;
            OnPlayingStateChanged?.Invoke(newState);
            
            // Update music based on state
            var track = newState switch
            {
                PlayingState.Campaign => MusicTrack.Campaign_Upbeat,
                PlayingState.Governance => MusicTrack.Governance,
                PlayingState.Crisis => MusicTrack.Crisis,
                PlayingState.Scandal => MusicTrack.Scandal,
                _ => MusicTrack.Campaign_Upbeat
            };
            
            AudioManager?.PlayMusic(track);
        }
        
        #endregion
        
        #region Game Flow
        
        public void StartNewGame(string backgroundId, DifficultyLevel difficulty, bool chaosMode)
        {
            Debug.Log($"[GameManagerIntegration] Starting new game: {backgroundId}, {difficulty}, Chaos: {chaosMode}");
            
            // Create new session
            currentSession = new GameSession
            {
                IsChaosMode = chaosMode,
                CurrentPhase = GamePhase.Campaign, // PoliticalTermPhase.Exploratory maps to Campaign
                CurrentTier = 1,
                ElectionYear = 2024
            };
            
            // Apply difficulty
            config.Difficulty = difficulty;
            config.ChaosMode = chaosMode;
            DifficultyManager.SetDifficulty(difficulty);
            
            // Analytics
            Analytics.LogGameStart(backgroundId, difficulty, chaosMode);
            
            // Change state
            ChangeState(GameStateType.Playing);
            OnGameStarted?.Invoke();
        }
        
        public void ContinueGame(int saveSlot)
        {
            var saveData = EnhancedSaveManager.LoadGame(saveSlot);
            if (saveData == null)
            {
                Debug.LogError($"Failed to load save slot {saveSlot}");
                return;
            }
            
            // Restore session
            currentSession = new GameSession
            {
                TurnNumber = saveData.World?.TurnNumber ?? 0,
                CurrentTier = saveData.Character?.CurrentTier ?? 1,
                CurrentOfficeId = saveData.Character?.CurrentOfficeId,
                IsChaosMode = saveData.Settings?.IsChaosMode ?? false
            };
            
            // Apply saved difficulty
            if (saveData.Settings?.CustomDifficulty != null)
            {
                DifficultyManager.SetCustomDifficulty(saveData.Settings.CustomDifficulty);
            }
            else
            {
                DifficultyManager.SetDifficulty(saveData.Settings?.Difficulty ?? DifficultyLevel.Normal);
            }
            
            // Restore currencies
            if (saveData.Resources != null)
            {
                CurrencyManager.SetBalance(CurrencyType.CloutBux, saveData.Resources.CloutBux);
                CurrencyManager.SetBalance(CurrencyType.Purrkoin, saveData.Resources.Purrkoin);
            }
            
            ChangeState(GameStateType.Playing);
        }
        
        public void EndGame(bool won, string endReason)
        {
            if (currentSession == null) return;
            
            currentSession.TotalPlaytimeMinutes = (float)(DateTime.UtcNow - currentSession.StartTime).TotalMinutes;
            
            // Record for balance
            BalanceManager.RecordGameResult(
                currentSession.CurrentOfficeId ?? "unknown",
                won,
                currentSession.CurrentTier,
                currentSession.TotalPlaytimeMinutes
            );
            
            // Analytics
            Analytics.LogGameEnd(won, currentSession.CurrentTier, 
                currentSession.TotalPlaytimeMinutes, endReason);
            
            // Award currencies
            int cloutBuxReward = CalculateEndGameReward(won);
            CurrencyManager.Credit(CurrencyType.CloutBux, cloutBuxReward, 
                won ? "Victory Bonus" : "Participation", "game_end");
            
            ChangeState(GameStateType.GameOver);
            OnGameEnded?.Invoke(won);
        }
        
        private int CalculateEndGameReward(bool won)
        {
            int baseReward = won ? 500 : 100;
            int tierBonus = (currentSession?.CurrentTier ?? 1) * (won ? 200 : 50);
            
            float difficultyMult = DifficultyManager.CurrentSettings.CloutBuxMultiplier;
            
            return (int)((baseReward + tierBonus) * difficultyMult);
        }
        
        private void HandleGameOver()
        {
            AudioManager?.PlayMusic(currentSession?.ElectionsWon > currentSession?.ElectionsLost 
                ? MusicTrack.Victory : MusicTrack.Defeat);
        }
        
        public void ReturnToMainMenu()
        {
            currentSession = null;
            Time.timeScale = 1f;
            ChangeState(GameStateType.MainMenu);
        }
        
        #endregion
        
        #region Turn Management
        
        public void AdvanceTurn()
        {
            if (currentSession == null) return;
            
            currentSession.TurnNumber++;
            
            // Autosave check
            if (config.AutosaveEnabled && 
                currentSession.TurnNumber % config.AutosaveIntervalTurns == 0)
            {
                QuickSave();
            }
            
            OnTurnAdvanced?.Invoke(currentSession.TurnNumber);
        }
        
        public void AdvancePhase(GamePhase newPhase)
        {
            if (currentSession == null) return;
            
            currentSession.CurrentPhase = newPhase;
            OnPhaseChanged?.Invoke(newPhase);
        }
        
        #endregion
        
        #region Save/Load
        
        public void QuickSave()
        {
            var saveData = CreateSaveData();
            EnhancedSaveManager.Autosave(saveData);
        }

        public void SaveGame(int slot, string saveName = null)
        {
            var saveData = CreateSaveData();
            EnhancedSaveManager.SaveGame(slot, saveData);
        }
        
        private GameSaveData CreateSaveData()
        {
            return new GameSaveData
            {
                Character = new Balance.CharacterSaveData
                {
                    CurrentTier = currentSession?.CurrentTier ?? 1,
                    CurrentOfficeId = currentSession?.CurrentOfficeId
                },
                World = new Balance.WorldSaveData
                {
                    TurnNumber = currentSession?.TurnNumber ?? 0,
                    CurrentPhase = currentSession?.CurrentPhase.ToString(),
                    GameDate = DateTime.UtcNow
                },
                Resources = new Balance.ResourceSaveData
                {
                    CloutBux = CurrencyManager.GetBalance(CurrencyType.CloutBux),
                    Purrkoin = CurrencyManager.GetBalance(CurrencyType.Purrkoin)
                },
                Settings = new Balance.GameSettingsSaveData
                {
                    Difficulty = config.Difficulty,
                    IsChaosMode = config.ChaosMode,
                    GameSpeed = config.GameSpeed,
                    AutosaveEnabled = config.AutosaveEnabled
                }
            };
        }
        
        #endregion
        
        #region Pause/Resume
        
        public void PauseGame()
        {
            if (currentState == GameStateType.Playing)
            {
                ChangeState(GameStateType.Paused);
            }
        }
        
        public void ResumeGame()
        {
            if (currentState == GameStateType.Paused)
            {
                Time.timeScale = config.GameSpeed;
                ChangeState(GameStateType.Playing);
            }
        }
        
        public void TogglePause()
        {
            if (currentState == GameStateType.Playing)
            {
                PauseGame();
            }
            else if (currentState == GameStateType.Paused)
            {
                ResumeGame();
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnAchievementUnlocked(Achievement achievement)
        {
            Debug.Log($"[GameManagerIntegration] Achievement unlocked: {achievement.DisplayName}");
            
            AudioManager?.PlaySFX(achievement.Rarity >= ItemRarity.Legendary 
                ? SFXType.Achievement_Legendary 
                : achievement.Rarity >= ItemRarity.Rare 
                    ? SFXType.Achievement_Rare 
                    : SFXType.Achievement_Unlock);
            
            // Show notification through UI system
        }
        
        private void OnCurrencyChanged(CurrencyType type, long oldValue, long newValue)
        {
            if (newValue > oldValue)
            {
                AudioManager?.PlaySFX(SFXType.Currency_Earn);
            }
            else
            {
                AudioManager?.PlaySFX(SFXType.Currency_Spend);
            }
        }
        
        private void OnSaveCompleted(int slot, bool success)
        {
            if (success)
            {
                AudioManager?.PlayUI(SFXType.UI_Success);
            }
            else
            {
                AudioManager?.PlayUI(SFXType.UI_Error);
            }
        }
        
        private void OnLoadCompleted(int slot, bool success)
        {
            if (!success)
            {
                AudioManager?.PlayUI(SFXType.UI_Error);
            }
        }
        
        private void OnPurchaseCompleted(IAPPurchase purchase)
        {
            if (purchase != null && !purchase.IsRefunded)
            {
                Analytics.LogPurchase(purchase.PackageId, purchase.Currency, (long)purchase.AmountPaid, purchase.IsVerified);
            }
        }
        
        #endregion
        
        #region Lifecycle
        
        private void Update()
        {
            // Handle escape key for pause
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (currentState == GameStateType.Playing || currentState == GameStateType.Paused)
                {
                    TogglePause();
                }
            }
            
            // Update session playtime
            if (currentSession != null && currentState == GameStateType.Playing)
            {
                currentSession.TotalPlaytimeMinutes = 
                    (float)(DateTime.UtcNow - currentSession.StartTime).TotalMinutes;
            }
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && currentState == GameStateType.Playing)
            {
                QuickSave();
            }
        }
        
        private void OnApplicationQuit()
        {
            // Final save
            if (currentState == GameStateType.Playing)
            {
                QuickSave();
            }
            
            // End analytics session
            Analytics.EndSession();
            
            // Save config
            SaveConfiguration();
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        public void SetGameSpeed(float speed)
        {
            config.GameSpeed = Mathf.Clamp(speed, 0.5f, 3f);
            if (currentState == GameStateType.Playing)
            {
                Time.timeScale = config.GameSpeed;
            }
        }
        
        public void RecordElectionResult(bool won, int tier, float votePercent, string officeId)
        {
            if (currentSession == null) return;
            
            if (won)
            {
                currentSession.ElectionsWon++;
                currentSession.CurrentTier = tier;
                currentSession.CurrentOfficeId = officeId;
                
                EffectsManager?.OnElectionWon();
                AchievementManager.TriggerEvent("election_won", new Dictionary<string, object>
                {
                    { "tier", tier },
                    { "votePercent", votePercent }
                });
            }
            else
            {
                currentSession.ElectionsLost++;
                EffectsManager?.OnElectionLost();
            }
            
            Analytics.LogElection(won, tier, votePercent, officeId);
        }
        
        public void RecordScandalHandled(string scandalType, int severity, bool survived)
        {
            if (currentSession == null) return;
            
            currentSession.ScandalsHandled++;
            
            EffectsManager?.OnScandalRevealed(severity);
            Analytics.LogScandal(scandalType, severity, survived ? "survived" : "career_ended");
            
            AchievementManager.UpdateStat("scandals_survived", survived ? 1 : 0);
        }
        
        public void RecordCrisisResolved(string crisisType, bool positive)
        {
            if (currentSession == null) return;
            
            if (positive)
            {
                currentSession.CrisesResolved++;
                AchievementManager.UpdateStat("crises_positive", 1);
            }
        }
        
        #endregion
    }
    
    #endregion
    
    #region Service Locator
    
    /// <summary>
    /// Service locator for accessing game systems.
    /// </summary>
    public static class Services
    {
        public static GameManagerIntegration Game => GameManagerIntegration.Instance;
        public static DifficultyManager Difficulty => Game?.DifficultyManager;
        public static AnalyticsManager Analytics => Game?.Analytics;
        public static Balance.EnhancedSaveManager Save => Game?.EnhancedSaveManager;
        public static TutorialManager Tutorial => Game?.TutorialManager;
        public static CurrencyManager Currency => Game?.CurrencyManager;
        public static IAPManager IAP => Game?.IAPManager;
        public static AchievementManager Achievements => Game?.AchievementManager;
        public static AudioManager Audio => Game?.AudioManager;
        public static PolishEffectsManager Effects => Game?.EffectsManager;
        public static CosmeticsShop Cosmetics => Game?.CosmeticsShop;
        public static PlayerInventory Inventory => Game?.PlayerInventory;
    }
    
    #endregion
    
    #region Game Events
    
    /// <summary>
    /// Global game event types.
    /// </summary>
    public enum GameEventType
    {
        // Core game events
        TurnStarted,
        TurnEnded,
        PhaseChanged,
        
        // Election events
        CampaignStarted,
        ElectionDay,
        ElectionWon,
        ElectionLost,
        
        // Scandal events
        ScandalTriggered,
        ScandalEscalated,
        ScandalResolved,
        
        // Crisis events
        CrisisStarted,
        CrisisEscalated,
        CrisisResolved,
        
        // Resource events
        FundsLow,
        TrustCritical,
        CapitalDepleted,
        
        // Achievement events
        AchievementUnlocked,
        MilestoneReached,
        
        // Chaos events
        ChaosEventTriggered,
        ViralMoment,
        
        // UI events
        NotificationPosted,
        TooltipRequested
    }

    /// <summary>
    /// Core game event data for the event bus system.
    /// Note: This is separate from ElectionEmpire.UI.Screens.GameEventData which is for UI popups.
    /// </summary>
    [Serializable]
    public class GameEventData
    {
        public GameEventType Type;
        public Dictionary<string, object> Data;
        public int Priority;
        public DateTime Timestamp;
        public bool Handled;
        public string EventId;

        public GameEventData()
        {
            Data = new Dictionary<string, object>();
            Timestamp = DateTime.UtcNow;
            EventId = Guid.NewGuid().ToString();
        }

        public GameEventData(GameEventType type, Dictionary<string, object> data = null, int priority = 0) : this()
        {
            Type = type;
            Data = data ?? new Dictionary<string, object>();
            Priority = priority;
        }

        public T GetData<T>(string key, T defaultValue = default)
        {
            if (Data != null && Data.TryGetValue(key, out var value))
            {
                if (value is T typedValue)
                    return typedValue;
            }
            return defaultValue;
        }
    }

    /// <summary>
    /// Central event bus for game events.
    /// </summary>
    public class GameEventBus
    {
        private static GameEventBus instance;
        public static GameEventBus Instance => instance ??= new GameEventBus();
        
        private Dictionary<GameEventType, List<Action<GameEventData>>> listeners;
        private Queue<GameEventData> eventQueue;
        private bool isProcessing;
        
        private GameEventBus()
        {
            listeners = new Dictionary<GameEventType, List<Action<GameEventData>>>();
            eventQueue = new Queue<GameEventData>();
        }
        
        public void Subscribe(GameEventType type, Action<GameEventData> handler)
        {
            if (!listeners.ContainsKey(type))
            {
                listeners[type] = new List<Action<GameEventData>>();
            }
            listeners[type].Add(handler);
        }
        
        public void Unsubscribe(GameEventType type, Action<GameEventData> handler)
        {
            if (listeners.TryGetValue(type, out var handlers))
            {
                handlers.Remove(handler);
            }
        }
        
        public void Publish(GameEventData evt)
        {
            eventQueue.Enqueue(evt);
            
            if (!isProcessing)
            {
                ProcessQueue();
            }
        }
        
        public void Publish(GameEventType type, Dictionary<string, object> data = null, int priority = 0)
        {
            Publish(new GameEventData(type, data, priority));
        }
        
        private void ProcessQueue()
        {
            isProcessing = true;
            
            while (eventQueue.Count > 0)
            {
                var evt = eventQueue.Dequeue();
                
                if (listeners.TryGetValue(evt.Type, out var handlers))
                {
                    foreach (var handler in handlers.ToList())
                    {
                        try
                        {
                            handler(evt);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Error handling event {evt.Type}: {e.Message}");
                        }
                    }
                }
                
                evt.Handled = true;
            }
            
            isProcessing = false;
        }
        
        public void Clear()
        {
            eventQueue.Clear();
            listeners.Clear();
        }
    }
    
    #endregion
}

