using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ElectionEmpire.UI
{
    /// <summary>
    /// Simple main menu implementation using PanelManager.
    /// Attach to a GameObject in MainMenu scene.
    /// 
    /// SETUP INSTRUCTIONS:
    /// 1. Create Canvas with the following panels as children:
    ///    - MainMenuPanel (buttons: New Campaign, Continue, Load, Settings, Quit)
    ///    - CharacterCreationPanel (character creation UI)
    ///    - LoadGamePanel (save file list)
    ///    - SettingsPanel (audio, graphics, etc.)
    ///    - CreditsPanel (credits text)
    /// 2. Assign button references in inspector
    /// 3. Assign PanelManager reference or add PanelManager to same Canvas
    /// </summary>
    public class SimpleMainMenu : MonoBehaviour
    {
        [Header("Panel Manager")]
        [SerializeField] private PanelManager panelManager;
        
        [Header("Main Menu Buttons")]
        [SerializeField] private Button newCampaignButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button loadGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button quitButton;
        
        [Header("Continue Game Info")]
        [SerializeField] private GameObject continueButtonContainer;
        [SerializeField] private TMP_Text continueInfoText;
        
        [Header("Character Creation Panel")]
        [SerializeField] private Button randomCharacterButton;
        [SerializeField] private Button buildCharacterButton;
        [SerializeField] private Button libraryButton;
        [SerializeField] private Button backFromCreationButton;
        [SerializeField] private Button startCampaignButton;
        
        [Header("Load Game Panel")]
        [SerializeField] private Transform saveFileContainer;
        [SerializeField] private GameObject saveFileEntryPrefab;
        [SerializeField] private Button backFromLoadButton;
        
        [Header("Settings Panel")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Button applySettingsButton;
        [SerializeField] private Button backFromSettingsButton;
        
        [Header("Credits Panel")]
        [SerializeField] private Button backFromCreditsButton;
        
        [Header("Panel IDs (match PanelManager config)")]
        [SerializeField] private string mainMenuPanelId = "MainMenu";
        [SerializeField] private string characterCreationPanelId = "CharacterCreation";
        [SerializeField] private string loadGamePanelId = "LoadGame";
        [SerializeField] private string settingsPanelId = "Settings";
        [SerializeField] private string creditsPanelId = "Credits";
        
        private void Start()
        {
            // Auto-find PanelManager if not assigned
            if (panelManager == null)
            {
                panelManager = FindFirstObjectByType<PanelManager>();
            }
            
            SetupButtonListeners();
            CheckForContinueSave();
            LoadSettings();
        }
        
        private void SetupButtonListeners()
        {
            // Main Menu
            if (newCampaignButton) newCampaignButton.onClick.AddListener(OnNewCampaign);
            if (continueButton) continueButton.onClick.AddListener(OnContinue);
            if (loadGameButton) loadGameButton.onClick.AddListener(OnLoadGame);
            if (settingsButton) settingsButton.onClick.AddListener(OnSettings);
            if (creditsButton) creditsButton.onClick.AddListener(OnCredits);
            if (quitButton) quitButton.onClick.AddListener(OnQuit);
            
            // Character Creation
            if (randomCharacterButton) randomCharacterButton.onClick.AddListener(OnRandomCharacter);
            if (buildCharacterButton) buildCharacterButton.onClick.AddListener(OnBuildCharacter);
            if (libraryButton) libraryButton.onClick.AddListener(OnLibrary);
            if (backFromCreationButton) backFromCreationButton.onClick.AddListener(GoBack);
            if (startCampaignButton) startCampaignButton.onClick.AddListener(OnStartCampaign);
            
            // Load Game
            if (backFromLoadButton) backFromLoadButton.onClick.AddListener(GoBack);
            
            // Settings
            if (applySettingsButton) applySettingsButton.onClick.AddListener(ApplySettings);
            if (backFromSettingsButton) backFromSettingsButton.onClick.AddListener(GoBack);
            
            // Credits
            if (backFromCreditsButton) backFromCreditsButton.onClick.AddListener(GoBack);
        }
        
        #region Main Menu Actions
        
        private void OnNewCampaign()
        {
            Debug.Log("[MainMenu] New Campaign clicked");
            panelManager?.ShowPanel(characterCreationPanelId);
        }
        
        private void OnContinue()
        {
            Debug.Log("[MainMenu] Continue clicked");
            // Load the most recent save and start game
            // SaveManager.Instance?.LoadMostRecent();
            StartGame();
        }
        
        private void OnLoadGame()
        {
            Debug.Log("[MainMenu] Load Game clicked");
            panelManager?.ShowPanel(loadGamePanelId);
            PopulateSaveFiles();
        }
        
        private void OnSettings()
        {
            Debug.Log("[MainMenu] Settings clicked");
            panelManager?.ShowPanel(settingsPanelId);
        }
        
        private void OnCredits()
        {
            Debug.Log("[MainMenu] Credits clicked");
            panelManager?.ShowPanel(creditsPanelId);
        }
        
        private void OnQuit()
        {
            Debug.Log("[MainMenu] Quit clicked");
            
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
        
        #endregion
        
        #region Character Creation Actions
        
        private void OnRandomCharacter()
        {
            Debug.Log("[MainMenu] Random Character clicked");
            // CharacterGenerator.Instance?.GenerateRandom();
            // Show preview panel with generated character
        }
        
        private void OnBuildCharacter()
        {
            Debug.Log("[MainMenu] Build Character clicked");
            // Switch to step-by-step character builder
            // Could be a sub-panel or separate panel
        }
        
        private void OnLibrary()
        {
            Debug.Log("[MainMenu] Library clicked");
            // Show saved characters from library
        }
        
        private void OnStartCampaign()
        {
            Debug.Log("[MainMenu] Start Campaign clicked");
            // Validate character is created
            // Start the game
            StartGame();
        }
        
        #endregion
        
        #region Load Game
        
        private void PopulateSaveFiles()
        {
            if (saveFileContainer == null || saveFileEntryPrefab == null)
            {
                Debug.LogWarning("[MainMenu] Save file container or prefab not assigned");
                return;
            }
            
            // Clear existing entries
            foreach (Transform child in saveFileContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Get save files from SaveManager
            // var saveFiles = SaveManager.Instance?.GetSaveFiles();
            
            // For now, create some placeholder entries
            for (int i = 0; i < 3; i++)
            {
                // var entry = Instantiate(saveFileEntryPrefab, saveFileContainer);
                // entry.GetComponentInChildren<TMP_Text>().text = $"Save Slot {i + 1}";
            }
        }
        
        public void LoadSaveFile(string saveFileName)
        {
            Debug.Log($"[MainMenu] Loading save: {saveFileName}");
            // SaveManager.Instance?.LoadGame(saveFileName);
            StartGame();
        }
        
        #endregion
        
        #region Settings
        
        private void LoadSettings()
        {
            // Load settings from PlayerPrefs or settings manager
            if (masterVolumeSlider) masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
            if (musicVolumeSlider) musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
            if (sfxVolumeSlider) sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
            if (fullscreenToggle) fullscreenToggle.isOn = Screen.fullScreen;
            
            // Populate resolution dropdown
            if (resolutionDropdown)
            {
                resolutionDropdown.ClearOptions();
                var options = new System.Collections.Generic.List<string>();
                int currentIndex = 0;
                
                for (int i = 0; i < Screen.resolutions.Length; i++)
                {
                    var res = Screen.resolutions[i];
                    options.Add($"{res.width}x{res.height} @{res.refreshRateRatio.value:0}Hz");
                    
                    if (res.width == Screen.currentResolution.width && 
                        res.height == Screen.currentResolution.height)
                    {
                        currentIndex = i;
                    }
                }
                
                resolutionDropdown.AddOptions(options);
                resolutionDropdown.value = currentIndex;
            }
        }
        
        private void ApplySettings()
        {
            Debug.Log("[MainMenu] Applying settings");
            
            // Save audio settings
            if (masterVolumeSlider) PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);
            if (musicVolumeSlider) PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
            if (sfxVolumeSlider) PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);
            
            // Apply resolution
            if (resolutionDropdown && resolutionDropdown.value < Screen.resolutions.Length)
            {
                var res = Screen.resolutions[resolutionDropdown.value];
                Screen.SetResolution(res.width, res.height, fullscreenToggle?.isOn ?? Screen.fullScreen);
            }
            
            PlayerPrefs.Save();
            
            // Notify audio manager
            // AudioManager.Instance?.UpdateVolumes();
        }
        
        #endregion
        
        #region Utility
        
        private void GoBack()
        {
            panelManager?.GoBack();
        }
        
        private void CheckForContinueSave()
        {
            // Check if there's a save to continue from
            // bool hasSave = SaveManager.Instance?.HasMostRecentSave() ?? false;
            bool hasSave = PlayerPrefs.HasKey("LastSave");
            
            if (continueButtonContainer)
            {
                continueButtonContainer.SetActive(hasSave);
            }
            
            if (continueButton)
            {
                continueButton.interactable = hasSave;
            }
            
            if (hasSave && continueInfoText)
            {
                // Display save info
                string lastSave = PlayerPrefs.GetString("LastSave", "Unknown");
                continueInfoText.text = $"Continue: {lastSave}";
            }
        }
        
        private void StartGame()
        {
            Debug.Log("[MainMenu] Starting game...");
            
            // Use SceneBootstrapper if available
            var bootstrapper = Core.SceneBootstrapper.Instance;
            if (bootstrapper != null)
            {
                bootstrapper.LoadGame();
            }
            else
            {
                // Fallback to direct scene load
                UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
            }
        }
        
        #endregion
    }
}
