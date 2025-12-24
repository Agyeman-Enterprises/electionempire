using UnityEngine;
using UnityEngine.UI;
using ElectionEmpire.Character;

namespace ElectionEmpire.UI
{
    /// <summary>
    /// Manages the character creation flow: Random, Build, or Library
    /// </summary>
    public class CharacterCreationFlow : MonoBehaviour
    {
        [Header("Mode Selection")]
        public GameObject ModeSelectionPanel;
        public Button RandomButton;
        public Button BuildButton;
        public Button LibraryButton;
        public Button BackButton;
        
        [Header("Random Generation")]
        public GameObject RandomGenerationPanel;
        public CharacterDisplay RandomCharacterDisplay;
        public Button AcceptRandomButton;
        public Button RerollButton;
        public Button BuildInsteadButton;
        public Dropdown ModeDropdown; // Balanced, Chaos, Hard
        
        [Header("Manual Builder")]
        public GameObject ManualBuilderPanel;
        public CharacterBuilderUI CharacterBuilder;
        
        [Header("Library")]
        public GameObject LibraryPanel;
        public CharacterLibraryUI LibraryUI;
        
        private Character _currentCharacter;
        private CharacterGenerator _generator;
        private RerollSystem _rerollSystem;
        
        private void Start()
        {
            _generator = new CharacterGenerator();
            _rerollSystem = RerollSystem.Instance;
            _rerollSystem.ResetLocks();
            
            SetupButtons();
            ShowModeSelection();
        }
        
        private void SetupButtons()
        {
            if (RandomButton != null)
                RandomButton.onClick.AddListener(OnRandomSelected);
            
            if (BuildButton != null)
                BuildButton.onClick.AddListener(OnBuildSelected);
            
            if (LibraryButton != null)
                LibraryButton.onClick.AddListener(OnLibrarySelected);
            
            if (BackButton != null)
                BackButton.onClick.AddListener(OnBack);
            
            if (AcceptRandomButton != null)
                AcceptRandomButton.onClick.AddListener(OnAcceptRandom);
            
            if (RerollButton != null)
                RerollButton.onClick.AddListener(OnReroll);
            
            if (BuildInsteadButton != null)
                BuildInsteadButton.onClick.AddListener(OnBuildSelected);
        }
        
        private void ShowModeSelection()
        {
            ModeSelectionPanel?.SetActive(true);
            RandomGenerationPanel?.SetActive(false);
            ManualBuilderPanel?.SetActive(false);
            LibraryPanel?.SetActive(false);
        }
        
        private void OnRandomSelected()
        {
            ModeSelectionPanel?.SetActive(false);
            RandomGenerationPanel?.SetActive(true);
            
            GenerateRandomCharacter();
        }
        
        private void OnBuildSelected()
        {
            ModeSelectionPanel?.SetActive(false);
            RandomGenerationPanel?.SetActive(false);
            ManualBuilderPanel?.SetActive(true);
            
            if (CharacterBuilder != null)
            {
                CharacterBuilder.OnCharacterBuilt += OnCharacterBuilt;
                CharacterBuilder.StartBuilder();
            }
        }
        
        private void OnLibrarySelected()
        {
            ModeSelectionPanel?.SetActive(false);
            LibraryPanel?.SetActive(true);
            
            if (LibraryUI != null)
            {
                LibraryUI.OnCharacterSelected += OnLibraryCharacterSelected;
                LibraryUI.RefreshLibrary();
            }
        }
        
        private void OnBack()
        {
            ShowModeSelection();
        }
        
        private void GenerateRandomCharacter()
        {
            RandomMode mode = RandomMode.Balanced;
            
            if (ModeDropdown != null)
            {
                switch (ModeDropdown.value)
                {
                    case 0: mode = RandomMode.Balanced; break;
                    case 1: mode = RandomMode.Chaos; break;
                    case 2: mode = RandomMode.Hard; break;
                }
            }
            
            _currentCharacter = _generator.GenerateRandom(mode);
            
            if (RandomCharacterDisplay != null)
            {
                RandomCharacterDisplay.DisplayCharacter(_currentCharacter);
            }
            
            UpdateRerollButton();
        }
        
        private void OnReroll()
        {
            RandomMode mode = RandomMode.Balanced;
            if (ModeDropdown != null)
            {
                switch (ModeDropdown.value)
                {
                    case 0: mode = RandomMode.Balanced; break;
                    case 1: mode = RandomMode.Chaos; break;
                    case 2: mode = RandomMode.Hard; break;
                }
            }
            
            var rerolled = _rerollSystem.Reroll(mode, _currentCharacter);
            if (rerolled != null)
            {
                _currentCharacter = rerolled;
                if (RandomCharacterDisplay != null)
                {
                    RandomCharacterDisplay.DisplayCharacter(_currentCharacter);
                }
                UpdateRerollButton();
            }
        }
        
        private void UpdateRerollButton()
        {
            if (RerollButton != null)
            {
                int freeRerolls = _rerollSystem.FreeRerollsRemaining;
                if (freeRerolls > 0)
                {
                    var text = RerollButton.GetComponentInChildren<UnityEngine.UI.Text>();
                    if (text != null)
                        text.text = $"Reroll ({freeRerolls} free left)";
                    RerollButton.interactable = true;
                }
                else
                {
                    var text = RerollButton.GetComponentInChildren<UnityEngine.UI.Text>();
                    if (text != null)
                        text.text = $"Reroll ({_rerollSystem.PurrkoinCostPerReroll} PK)";
                    // Check if player has enough Purrkoin
                    int currentPurrkoin = PlayerPrefs.GetInt("Purrkoin", 0);
                    bool hasEnough = currentPurrkoin >= cost;
                    RerollButton.interactable = true;
                }
            }
        }
        
        private void OnAcceptRandom()
        {
            if (_currentCharacter != null)
            {
                StartCampaign(_currentCharacter);
            }
        }
        
        private void OnCharacterBuilt(Character character)
        {
            _currentCharacter = character;
            StartCampaign(character);
        }
        
        private void OnLibraryCharacterSelected(Character character)
        {
            _currentCharacter = character;
            StartCampaign(character);
        }
        
        private void StartCampaign(Character character)
        {
            // Set character in GameManager
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.StartNewCampaign(character);
            }
            
            // Load game scene
            UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
        }
    }
}
