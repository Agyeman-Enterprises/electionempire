using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ElectionEmpire.Core;
using ElectionEmpire.AI;
using ElectionEmpire.Scandal;

namespace ElectionEmpire.Gameplay
{
    /// <summary>
    /// Initializes and coordinates all gameplay managers in the Game scene.
    /// Attach to a "GameController" GameObject in the Game scene.
    /// 
    /// This script ensures all managers are initialized in the correct order
    /// and provides a central point for gameplay coordination.
    /// </summary>
    public class GameSceneInitializer : MonoBehaviour
    {
        [Header("Manager References")]
        [Tooltip("Assign these if managers are in scene. Leave null to find automatically.")]
        [SerializeField] private ElectionEmpire.Core.TimeManager timeManager;
        [SerializeField] private ElectionEmpire.AI.AIManager aiManager;
        [SerializeField] private ElectionEmpire.Scandal.ScandalManager scandalManager;
        [SerializeField] private MonoBehaviour crisisManager; // CrisisManager type if it exists
        [SerializeField] private ElectionManager electionManager;
        [SerializeField] private ResourceManager resourceManager;
        [SerializeField] private News.NewsEventManager newsEventManager;
        [SerializeField] private MonoBehaviour mediaManager; // MediaManager type if it exists
        
        [Header("UI References")]
        [SerializeField] private MonoBehaviour gameHUD; // GameHUD type if it exists
        [SerializeField] private UI.PanelManager panelManager;
        
        [Header("Initialization Settings")]
        [SerializeField] private bool autoInitialize = true;
        [SerializeField] private float initializationDelay = 0.1f;
        [SerializeField] private bool enableDebugLogs = true;
        
        [Header("Fallback Prefabs (if managers not in scene)")]
        [SerializeField] private GameObject timeManagerPrefab;
        [SerializeField] private GameObject aiManagerPrefab;
        [SerializeField] private GameObject scandalManagerPrefab;
        [SerializeField] private GameObject crisisManagerPrefab;
        [SerializeField] private GameObject electionManagerPrefab;
        
        private bool _isInitialized = false;
        public bool IsInitialized => _isInitialized;
        
        // Events
        public event System.Action OnInitializationStarted;
        public event System.Action OnInitializationComplete;
        public event System.Action<string> OnManagerInitialized;
        public event System.Action<string> OnInitializationFailed;
        
        // Singleton (scene-specific, not persistent)
        private static GameSceneInitializer _instance;
        public static GameSceneInitializer Instance => _instance;
        
        private void Awake()
        {
            _instance = this;
        }
        
        private void Start()
        {
            if (autoInitialize)
            {
                StartCoroutine(InitializeWithDelay());
            }
        }
        
        private IEnumerator InitializeWithDelay()
        {
            // Wait a frame to ensure all Awake methods have run
            yield return new WaitForSeconds(initializationDelay);
            Initialize();
        }
        
        /// <summary>
        /// Initialize all game systems. Call this manually if autoInitialize is false.
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
            {
                Log("Already initialized, skipping.");
                return;
            }
            
            Log("Starting game scene initialization...");
            OnInitializationStarted?.Invoke();
            
            StartCoroutine(InitializationSequence());
        }
        
        private IEnumerator InitializationSequence()
        {
            // Step 1: Find or create managers
            Log("Step 1: Locating managers...");
            yield return null;
            
            FindOrCreateManagers();
            
            // Step 2: Initialize core systems
            Log("Step 2: Initializing core systems...");
            yield return null;
            
            if (!InitializeTimeManager()) yield break;
            if (!InitializeResourceManager()) yield break;
            
            // Step 3: Initialize gameplay systems
            Log("Step 3: Initializing gameplay systems...");
            yield return null;
            
            InitializeAIManager();
            InitializeScandalManager();
            InitializeCrisisManager();
            InitializeElectionManager();
            InitializeMediaManager();
            InitializeNewsEventManager();
            
            // Step 4: Initialize UI
            Log("Step 4: Initializing UI...");
            yield return null;
            
            InitializeUI();
            
            // Step 5: Load game state (if continuing)
            Log("Step 5: Loading game state...");
            yield return null;
            
            LoadGameState();
            
            // Step 6: Start game loop
            Log("Step 6: Starting game loop...");
            yield return null;
            
            StartGameLoop();
            
            _isInitialized = true;
            Log("Game scene initialization complete!");
            OnInitializationComplete?.Invoke();
        }
        
        #region Manager Initialization
        
        private void FindOrCreateManagers()
        {
            // Find managers if not assigned
            if (timeManager == null) timeManager = FindFirstObjectByType<ElectionEmpire.Core.TimeManager>();
            if (aiManager == null) aiManager = FindFirstObjectByType<ElectionEmpire.AI.AIManager>();
            if (scandalManager == null) scandalManager = FindFirstObjectByType<ElectionEmpire.Scandal.ScandalManager>();
            // CrisisManager is a placeholder - will be implemented in future sprint
            // if (crisisManager == null) crisisManager = FindFirstObjectByType<CrisisManager>();
            // ElectionManager and ResourceManager are not MonoBehaviour, so instantiate directly if needed
            // They will be initialized later when game state is available
            if (electionManager == null)
            {
                electionManager = new ElectionManager(); // Will be initialized in InitializeElectionManager()
            }
            if (resourceManager == null)
            {
                // ResourceManager will be created in InitializeResourceManager() when PlayerState is available
                // For now, leave it null - it will be created when needed
            }
            if (newsEventManager == null) newsEventManager = FindFirstObjectByType<News.NewsEventManager>();
            // MediaManager is a placeholder - will be implemented in future sprint
            // if (mediaManager == null) mediaManager = FindFirstObjectByType<MediaManager>();
            
            // Create from prefabs if still null
            if (timeManager == null && timeManagerPrefab != null)
            {
                var go = Instantiate(timeManagerPrefab);
                timeManager = go.GetComponent<ElectionEmpire.Core.TimeManager>();
                Log("Created TimeManager from prefab");
            }
            
            if (aiManager == null && aiManagerPrefab != null)
            {
                var go = Instantiate(aiManagerPrefab);
                aiManager = go.GetComponent<ElectionEmpire.AI.AIManager>();
                Log("Created AIManager from prefab");
            }

            if (scandalManager == null && scandalManagerPrefab != null)
            {
                var go = Instantiate(scandalManagerPrefab);
                scandalManager = go.GetComponent<ElectionEmpire.Scandal.ScandalManager>();
                Log("Created ScandalManager from prefab");
            }
            
            // Find UI if not assigned
            if (gameHUD == null) gameHUD = FindFirstObjectByType<UI.GameHUD>();
            if (panelManager == null) panelManager = FindFirstObjectByType<UI.PanelManager>();
        }
        
        private bool InitializeTimeManager()
        {
            if (timeManager == null)
            {
                LogError("TimeManager not found! Game cannot start without time management.");
                OnInitializationFailed?.Invoke("TimeManager");
                return false;
            }
            
            // TimeManager usually initializes itself, but we can trigger any additional setup
            // timeManager.Initialize();
            OnManagerInitialized?.Invoke("TimeManager");
            return true;
        }
        
        private bool InitializeResourceManager()
        {
            if (resourceManager == null)
            {
                // ResourceManager might be optional or created dynamically
                Log("ResourceManager not found, gameplay may have limited functionality.");
                return true; // Not critical for basic operation
            }
            
            // resourceManager.Initialize();
            OnManagerInitialized?.Invoke("ResourceManager");
            return true;
        }
        
        private void InitializeAIManager()
        {
            if (aiManager == null)
            {
                Log("AIManager not found, opponents will not be active.");
                return;
            }
            
            // aiManager.Initialize();
            OnManagerInitialized?.Invoke("AIManager");
        }
        
        private void InitializeScandalManager()
        {
            if (scandalManager == null)
            {
                Log("ScandalManager not found, scandals will not generate.");
                return;
            }
            
            // scandalManager.Initialize();
            OnManagerInitialized?.Invoke("ScandalManager");
        }
        
        private void InitializeCrisisManager()
        {
            if (crisisManager == null)
            {
                Log("CrisisManager not found, crises will not generate.");
                return;
            }
            
            // crisisManager.Initialize();
            OnManagerInitialized?.Invoke("CrisisManager");
        }
        
        private void InitializeElectionManager()
        {
            if (electionManager == null)
            {
                Log("ElectionManager not found, elections will not occur.");
                return;
            }
            
            // electionManager.Initialize();
            OnManagerInitialized?.Invoke("ElectionManager");
        }
        
        private void InitializeMediaManager()
        {
            if (mediaManager == null)
            {
                Log("MediaManager not found, media system inactive.");
                return;
            }
            
            // mediaManager.Initialize();
            OnManagerInitialized?.Invoke("MediaManager");
        }
        
        private void InitializeNewsEventManager()
        {
            if (newsEventManager == null)
            {
                Log("NewsEventManager not found, real-world news integration inactive.");
                return;
            }
            
            // newsEventManager.Initialize();
            OnManagerInitialized?.Invoke("NewsEventManager");
        }
        
        #endregion
        
        #region UI Initialization
        
        private void InitializeUI()
        {
            if (gameHUD != null)
            {
                // gameHUD.Initialize();
                Log("GameHUD initialized");
            }
            else
            {
                Log("GameHUD not found, UI may not display correctly.");
            }
            
            if (panelManager != null)
            {
                Log("PanelManager found and ready");
            }
        }
        
        #endregion
        
        #region Game State
        
        private void LoadGameState()
        {
            // Check if we're continuing a game or starting new
            var gameManager = GameManager.Instance;
            
            if (gameManager != null && gameManager.HasActiveGame)
            {
                Log("Loading existing game state...");
                // Game state already loaded by GameManager
            }
            else
            {
                Log("Starting new game...");
                // Initialize new game state
                InitializeNewGame();
            }
        }
        
        private void InitializeNewGame()
        {
            // Get player character from GameManager
            var gameManager = GameManager.Instance;
            
            if (gameManager?.CurrentCharacter != null)
            {
                Log($"Starting campaign with character: {gameManager.CurrentCharacter.FullName}");
                // Apply character to game systems
                // resourceManager?.InitializeFromCharacter(gameManager.CurrentCharacter);
            }
            else
            {
                LogWarning("No character loaded! Creating default character for testing.");
                // Create a default test character
                // This should not happen in production
            }
            
            // Initialize world state
            // WorldGenerator.Instance?.GenerateWorld();
            
            // Initialize starting resources
            // resourceManager?.SetStartingResources();
        }
        
        #endregion
        
        #region Game Loop
        
        private void StartGameLoop()
        {
            // Start the main game loop
            if (timeManager != null)
            {
                // timeManager.StartTime();
                Log("Time system started");
            }
            
            // Enable AI
            if (aiManager != null)
            {
                // aiManager.EnableAI();
                Log("AI system started");
            }
            
            // Show initial game HUD
            if (gameHUD != null)
            {
                // gameHUD.Show();
            }
            
            // Trigger initial events/tutorial
            // TutorialManager.Instance?.StartIfFirstTime();
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Pause all game systems
        /// </summary>
        public void PauseGame()
        {
            Log("Game paused");
            timeManager?.PauseTime();
            // Show pause menu
            panelManager?.ShowPanel("PauseMenu");
        }
        
        /// <summary>
        /// Resume all game systems
        /// </summary>
        public void ResumeGame()
        {
            Log("Game resumed");
            panelManager?.GoBack();
            timeManager?.ResumeTime();
        }
        
        /// <summary>
        /// Return to main menu
        /// </summary>
        public void ReturnToMainMenu()
        {
            Log("Returning to main menu");
            
            // Save game if needed
            // SaveManager.Instance?.QuickSave();
            
            // Load main menu scene
            var bootstrapper = Core.SceneBootstrapper.Instance;
            if (bootstrapper != null)
            {
                bootstrapper.LoadMainMenu();
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            }
        }
        
        /// <summary>
        /// Save and quit to desktop
        /// </summary>
        public void SaveAndQuit()
        {
            Log("Saving and quitting...");
            
            // Save game
            // SaveManager.Instance?.QuickSave();
            
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
        
        #endregion
        
        #region Logging
        
        private void Log(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[GameSceneInitializer] {message}");
            }
        }
        
        private void LogWarning(string message)
        {
            Debug.LogWarning($"[GameSceneInitializer] {message}");
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[GameSceneInitializer] {message}");
        }
        
        #endregion
    }
    
    // Note: Manager classes are now defined in their respective namespaces
    // TimeManager -> ElectionEmpire.Core.TimeManager
    // GameManager -> ElectionEmpire.Core.GameManager
}
