using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ElectionEmpire
{
    /// <summary>
    /// Bootstrap script that initializes the game and provides visual feedback.
    /// Attach this to your [GameController] GameObject.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject gameHUDPanel;
        [SerializeField] private GameObject mainMenuPanel;
        
        [Header("Debug Settings")]
        [SerializeField] private bool showDebugInfo = true;
        
        // Manager references (assign in inspector or find at runtime)
        private MonoBehaviour timeManager;
        private MonoBehaviour aiManager;
        private MonoBehaviour scandalManager;
        private MonoBehaviour crisisManager;
        private MonoBehaviour electionManager;
        private MonoBehaviour resourceManager;
        private MonoBehaviour newsEventManager;
        private MonoBehaviour mediaManager;
        private MonoBehaviour characterManager;
        private MonoBehaviour staffManager;
        private MonoBehaviour audioManager;
        
        private void Awake()
        {
            Debug.Log("===========================================");
            Debug.Log("[GameBootstrap] Election Empire Initializing...");
            Debug.Log("===========================================");
            
            FindAllManagers();
            ValidateManagers();
        }
        
        private void Start()
        {
            Debug.Log("[GameBootstrap] Start() - Setting up initial game state");
            
            // Show the HUD panel if assigned
            if (gameHUDPanel != null)
            {
                gameHUDPanel.SetActive(true);
                Debug.Log("[GameBootstrap] GameHUDPanel activated");
            }
            else
            {
                Debug.LogWarning("[GameBootstrap] No GameHUDPanel assigned - looking for it...");
                gameHUDPanel = GameObject.Find("GameHUDPanel");
                if (gameHUDPanel != null)
                {
                    gameHUDPanel.SetActive(true);
                    Debug.Log("[GameBootstrap] Found and activated GameHUDPanel");
                }
            }
            
            // Create debug overlay if enabled
            if (showDebugInfo)
            {
                CreateDebugOverlay();
            }
            
            Debug.Log("===========================================");
            Debug.Log("[GameBootstrap] Initialization Complete!");
            Debug.Log("===========================================");
        }
        
        private void FindAllManagers()
        {
            Debug.Log("[GameBootstrap] Searching for managers...");
            
            GameObject managersParent = GameObject.Find("[Managers]");
            
            if (managersParent != null)
            {
                Debug.Log($"[GameBootstrap] Found [Managers] parent with {managersParent.transform.childCount} children");
                
                foreach (Transform child in managersParent.transform)
                {
                    Debug.Log($"  - Found: {child.name}");
                    
                    // Store references based on name
                    switch (child.name)
                    {
                        case "TimeManager":
                            timeManager = child.GetComponent<MonoBehaviour>();
                            break;
                        case "AIManager":
                            aiManager = child.GetComponent<MonoBehaviour>();
                            break;
                        case "ScandalManager":
                            scandalManager = child.GetComponent<MonoBehaviour>();
                            break;
                        case "CrisisManager":
                            crisisManager = child.GetComponent<MonoBehaviour>();
                            break;
                        case "ElectionManager":
                            electionManager = child.GetComponent<MonoBehaviour>();
                            break;
                        case "ResourceManager":
                            resourceManager = child.GetComponent<MonoBehaviour>();
                            break;
                        case "NewsEventManager":
                            newsEventManager = child.GetComponent<MonoBehaviour>();
                            break;
                        case "MediaManager":
                            mediaManager = child.GetComponent<MonoBehaviour>();
                            break;
                        case "CharacterManager":
                            characterManager = child.GetComponent<MonoBehaviour>();
                            break;
                        case "StaffManager":
                            staffManager = child.GetComponent<MonoBehaviour>();
                            break;
                        case "AudioManager":
                            audioManager = child.GetComponent<MonoBehaviour>();
                            break;
                    }
                }
            }
            else
            {
                Debug.LogError("[GameBootstrap] [Managers] parent not found!");
            }
        }
        
        private void ValidateManagers()
        {
            Debug.Log("[GameBootstrap] Validating manager scripts...");
            
            int foundCount = 0;
            int missingCount = 0;
            
            CheckManager("TimeManager", timeManager, ref foundCount, ref missingCount);
            CheckManager("AIManager", aiManager, ref foundCount, ref missingCount);
            CheckManager("ScandalManager", scandalManager, ref foundCount, ref missingCount);
            CheckManager("CrisisManager", crisisManager, ref foundCount, ref missingCount);
            CheckManager("ElectionManager", electionManager, ref foundCount, ref missingCount);
            CheckManager("ResourceManager", resourceManager, ref foundCount, ref missingCount);
            CheckManager("NewsEventManager", newsEventManager, ref foundCount, ref missingCount);
            CheckManager("MediaManager", mediaManager, ref foundCount, ref missingCount);
            CheckManager("CharacterManager", characterManager, ref foundCount, ref missingCount);
            CheckManager("StaffManager", staffManager, ref foundCount, ref missingCount);
            CheckManager("AudioManager", audioManager, ref foundCount, ref missingCount);
            
            Debug.Log($"[GameBootstrap] Manager Status: {foundCount} found, {missingCount} missing scripts");
        }
        
        private void CheckManager(string name, MonoBehaviour manager, ref int found, ref int missing)
        {
            if (manager != null)
            {
                Debug.Log($"  ✓ {name}: {manager.GetType().Name}");
                found++;
            }
            else
            {
                Debug.LogWarning($"  ✗ {name}: No script attached or GameObject missing");
                missing++;
            }
        }
        
        private void CreateDebugOverlay()
        {
            // Find or create canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("[GameBootstrap] No Canvas found for debug overlay");
                return;
            }
            
            // Create debug panel
            GameObject debugPanel = new GameObject("DebugOverlay");
            debugPanel.transform.SetParent(canvas.transform, false);
            
            // Add background
            Image bg = debugPanel.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.8f);
            
            RectTransform rt = debugPanel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.7f);
            rt.anchorMax = new Vector2(0.4f, 1f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.anchoredPosition = new Vector2(10, -10);
            rt.sizeDelta = new Vector2(-20, -20);
            
            // Add text
            GameObject textObj = new GameObject("DebugText");
            textObj.transform.SetParent(debugPanel.transform, false);
            
            // Try TextMeshPro first, fall back to legacy Text
            Text debugText = textObj.AddComponent<Text>();
            debugText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            debugText.fontSize = 14;
            debugText.color = Color.green;
            debugText.text = "=== ELECTION EMPIRE ===\n" +
                           "Status: RUNNING\n" +
                           "Scene: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "\n" +
                           "---\n" +
                           "Press ESC for menu\n" +
                           "Press F1 to hide debug\n" +
                           "---\n" +
                           "Check Console for\n" +
                           "manager status";
            
            RectTransform textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(10, 10);
            textRt.offsetMax = new Vector2(-10, -10);
            
            Debug.Log("[GameBootstrap] Debug overlay created");
        }
        
        private void Update()
        {
            // Toggle debug overlay with F1
            if (Input.GetKeyDown(KeyCode.F1))
            {
                GameObject overlay = GameObject.Find("DebugOverlay");
                if (overlay != null)
                {
                    overlay.SetActive(!overlay.activeSelf);
                }
            }
            
            // Placeholder ESC menu
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("[GameBootstrap] ESC pressed - Menu system not yet implemented");
            }
        }
    }
}
