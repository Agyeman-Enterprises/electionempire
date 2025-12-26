using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace ElectionEmpire.Core
{
    /// <summary>
    /// Handles initialization of persistent managers and scene setup.
    /// Place this on a GameObject in your MainMenu scene.
    /// It will persist across all scene loads and ensure managers are properly initialized.
    /// </summary>
    public class SceneBootstrapper : MonoBehaviour
    {
        private static SceneBootstrapper _instance;
        public static SceneBootstrapper Instance => _instance;
        
        [Header("Manager Prefabs (Optional - can be in scene)")]
        [Tooltip("If assigned, these prefabs will be instantiated. If null, assumes managers are already in scene.")]
        [SerializeField] private GameObject gameManagerPrefab;
        [SerializeField] private GameObject uiManagerPrefab;
        [SerializeField] private GameObject audioManagerPrefab;
        [SerializeField] private GameObject analyticsManagerPrefab;
        
        [Header("Scene Names")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private string characterCreationSceneName = "CharacterCreation";
        [SerializeField] private string gameSceneName = "Game";
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
        
        // References to instantiated managers
        private GameObject _persistentManagersRoot;
        
        // Initialization state
        private bool _isInitialized = false;
        public bool IsInitialized => _isInitialized;
        
        // Events
        public event System.Action OnBootstrapComplete;
        public event System.Action<string> OnSceneLoadStarted;
        public event System.Action<string> OnSceneLoadComplete;
        
        private void Awake()
        {
            // Singleton pattern with DontDestroyOnLoad
            if (_instance != null && _instance != this)
            {
                Log("SceneBootstrapper already exists, destroying duplicate.");
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Create persistent managers root
            SetupPersistentManagers();
            
            // Subscribe to scene events
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            
            _isInitialized = true;
            Log("SceneBootstrapper initialized successfully.");
            OnBootstrapComplete?.Invoke();
        }
        
        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }
        
        private void SetupPersistentManagers()
        {
            // Create a root object for all persistent managers
            _persistentManagersRoot = new GameObject("[Persistent Managers]");
            DontDestroyOnLoad(_persistentManagersRoot);
            
            // Move this bootstrapper under the root
            transform.SetParent(_persistentManagersRoot.transform);
            
            // Instantiate manager prefabs if assigned
            if (gameManagerPrefab != null)
            {
                var go = Instantiate(gameManagerPrefab, _persistentManagersRoot.transform);
                go.name = "GameManager";
                Log("GameManager instantiated from prefab.");
            }
            
            if (uiManagerPrefab != null)
            {
                var go = Instantiate(uiManagerPrefab, _persistentManagersRoot.transform);
                go.name = "UIManager";
                Log("UIManager instantiated from prefab.");
            }
            
            if (audioManagerPrefab != null)
            {
                var go = Instantiate(audioManagerPrefab, _persistentManagersRoot.transform);
                go.name = "AudioManager";
                Log("AudioManager instantiated from prefab.");
            }
            
            if (analyticsManagerPrefab != null)
            {
                var go = Instantiate(analyticsManagerPrefab, _persistentManagersRoot.transform);
                go.name = "AnalyticsManager";
                Log("AnalyticsManager instantiated from prefab.");
            }
        }
        
        #region Scene Loading
        
        /// <summary>
        /// Load a scene by name with optional loading screen
        /// </summary>
        public void LoadScene(string sceneName, bool showLoadingScreen = false)
        {
            StartCoroutine(LoadSceneAsync(sceneName, showLoadingScreen));
        }
        
        /// <summary>
        /// Load the main menu scene
        /// </summary>
        public void LoadMainMenu()
        {
            LoadScene(mainMenuSceneName);
        }
        
        /// <summary>
        /// Load the character creation scene
        /// </summary>
        public void LoadCharacterCreation()
        {
            LoadScene(characterCreationSceneName);
        }
        
        /// <summary>
        /// Load the main game scene
        /// </summary>
        public void LoadGame()
        {
            LoadScene(gameSceneName, true); // Show loading for game scene
        }
        
        private IEnumerator LoadSceneAsync(string sceneName, bool showLoadingScreen)
        {
            Log($"Loading scene: {sceneName}");
            OnSceneLoadStarted?.Invoke(sceneName);
            
            // Optional: Show loading screen
            if (showLoadingScreen)
            {
                // TODO: Trigger loading screen UI
                // UIManager.Instance?.ShowLoadingScreen();
            }
            
            // Load the scene asynchronously
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;
            
            // Wait until the scene is almost loaded
            while (asyncLoad.progress < 0.9f)
            {
                // Optional: Update loading progress
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                // UIManager.Instance?.UpdateLoadingProgress(progress);
                yield return null;
            }
            
            // Activate the scene
            asyncLoad.allowSceneActivation = true;
            
            // Wait one frame for scene to fully activate
            yield return null;
            
            Log($"Scene loaded: {sceneName}");
            OnSceneLoadComplete?.Invoke(sceneName);
            
            // Optional: Hide loading screen
            if (showLoadingScreen)
            {
                // UIManager.Instance?.HideLoadingScreen();
            }
        }
        
        #endregion
        
        #region Scene Callbacks
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Log($"Scene loaded callback: {scene.name}");
            
            // Perform scene-specific initialization
            switch (scene.name)
            {
                case "MainMenu":
                    InitializeMainMenuScene();
                    break;
                case "CharacterCreation":
                    InitializeCharacterCreationScene();
                    break;
                case "Game":
                    InitializeGameScene();
                    break;
            }
        }
        
        private void OnSceneUnloaded(Scene scene)
        {
            Log($"Scene unloaded: {scene.name}");
            
            // Cleanup scene-specific resources if needed
            if (scene.name == "Game")
            {
                CleanupGameScene();
            }
        }
        
        private void InitializeMainMenuScene()
        {
            // Any main menu specific initialization
            Log("Initializing MainMenu scene...");
        }
        
        private void InitializeCharacterCreationScene()
        {
            // Any character creation specific initialization
            Log("Initializing CharacterCreation scene...");
        }
        
        private void InitializeGameScene()
        {
            // Initialize game-specific managers
            Log("Initializing Game scene...");
            
            // Example: Start the game loop
            // var gameLoop = FindObjectOfType<GameLoop>();
            // gameLoop?.Initialize();
        }
        
        private void CleanupGameScene()
        {
            // Cleanup when leaving game scene
            Log("Cleaning up Game scene...");
            
            // Reset any game state if returning to menu
        }
        
        #endregion
        
        #region Utility
        
        private void Log(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[SceneBootstrapper] {message}");
            }
        }
        
        /// <summary>
        /// Quit the application
        /// </summary>
        public void QuitGame()
        {
            Log("Quitting game...");
            
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
        
        #endregion
    }
}
