// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - Main Menu Screens
// Sprint 9: Main menu, new game, settings, and character creation
// ═══════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

namespace ElectionEmpire.UI.Screens
{
    #region Menu Data Classes
    
    /// <summary>
    /// Character background data for selection.
    /// </summary>
    [Serializable]
    public class BackgroundOption
    {
        public string BackgroundId;
        public string Name;
        public string Description;
        public Sprite Portrait;
        public Sprite Icon;
        
        // Starting bonuses
        public List<string> Advantages;
        public List<string> Disadvantages;
        public string SpecialAbility;
        public string SpecialAbilityDescription;
        
        // Starting stats
        public int StartingFunds;
        public int StartingTrust;
        public List<string> StartingConnections;
        
        public bool IsUnlocked;
        public string UnlockRequirement;
    }
    
    /// <summary>
    /// Character trait data.
    /// </summary>
    [Serializable]
    public class TraitOption
    {
        public string TraitId;
        public string Name;
        public string Description;
        public Sprite Icon;
        public bool IsPositive;
        public List<string> Effects;
        public List<string> Incompatible; // Traits this conflicts with
        public bool IsUnlocked;
    }
    
    /// <summary>
    /// Save game data for display.
    /// </summary>
    [Serializable]
    public class SaveGameData
    {
        public string SaveId;
        public string CharacterName;
        public string CurrentOffice;
        public int Year;
        public int Turn;
        public float ApprovalRating;
        public DateTime SaveDate;
        public string PlayTime;
        public Sprite Screenshot;
        public bool IsAutoSave;
    }
    
    /// <summary>
    /// Game settings data.
    /// </summary>
    [Serializable]
    public class GameSettings
    {
        // Audio
        public float MasterVolume = 1f;
        public float MusicVolume = 0.8f;
        public float SFXVolume = 1f;
        public float VoiceVolume = 1f;
        public bool MuteWhenUnfocused = true;
        
        // Graphics
        public int ResolutionIndex = 0;
        public bool Fullscreen = true;
        public int QualityLevel = 2;
        public bool VSync = true;
        public int TargetFramerate = 60;
        
        // Gameplay
        public float TextSpeed = 1f;
        public bool ShowTutorials = true;
        public bool AutoSave = true;
        public int AutoSaveInterval = 5; // turns
        public bool ConfirmActions = true;
        public bool ShowProbabilities = true;
        
        // Accessibility
        public float UIScale = 1f;
        public bool HighContrast = false;
        public bool ScreenReaderSupport = false;
        public bool ReduceAnimations = false;
        public int FontSize = 1; // 0=small, 1=medium, 2=large
    }
    
    /// <summary>
    /// New game configuration.
    /// </summary>
    [Serializable]
    public class NewGameConfig
    {
        public string CharacterName;
        public string BackgroundId;
        public Dictionary<string, int> StatAllocation;
        public List<string> SelectedTraits;
        public int Difficulty;
        public string StartingRegion;
        public bool IronmanMode;
        public int StartingYear;
    }
    
    #endregion
    
    /// <summary>
    /// Main Menu screen.
    /// </summary>
    public class MainMenu : BaseScreen
    {
        #region Serialized Fields
        
        [Header("Menu Buttons")]
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _loadGameButton;
        [SerializeField] private Button _multiplayerButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _creditsButton;
        [SerializeField] private Button _quitButton;
        
        [Header("Version Info")]
        [SerializeField] private Text _versionText;
        
        [Header("Continue Game Info")]
        [SerializeField] private GameObject _continueInfo;
        [SerializeField] private Text _continueCharacterText;
        [SerializeField] private Text _continueOfficeText;
        [SerializeField] private Text _continueProgressText;
        
        [Header("Background")]
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private List<Sprite> _backgroundOptions;
        
        [Header("Audio")]
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioClip _menuMusic;
        
        #endregion
        
        #region Private Fields
        
        private SaveGameData _lastSaveGame;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            SetupButtons();
        }
        
        private void Start()
        {
            if (_musicSource != null && _menuMusic != null)
            {
                _musicSource.clip = _menuMusic;
                _musicSource.loop = true;
                _musicSource.Play();
            }
            
            if (_versionText != null)
            {
                _versionText.text = $"v{Application.version}";
            }
            
            // Random background
            if (_backgroundImage != null && _backgroundOptions != null && _backgroundOptions.Count > 0)
            {
                _backgroundImage.sprite = _backgroundOptions[UnityEngine.Random.Range(0, _backgroundOptions.Count)];
            }
        }
        
        #endregion
        
        #region Screen Lifecycle
        
        public override void OnScreenEnter(object data)
        {
            base.OnScreenEnter(data);
            
            // Check for continue save
            _lastSaveGame = GetLastSave();
            UpdateContinueButton();
        }
        
        #endregion
        
        #region Setup
        
        private void SetupButtons()
        {
            if (_newGameButton != null)
                _newGameButton.onClick.AddListener(() => NavigateTo(ScreenType.NewGame));
            
            if (_continueButton != null)
                _continueButton.onClick.AddListener(ContinueGame);
            
            if (_loadGameButton != null)
                _loadGameButton.onClick.AddListener(() => NavigateTo(ScreenType.LoadGame));
            
            if (_multiplayerButton != null)
                _multiplayerButton.onClick.AddListener(() => NavigateTo(ScreenType.MultiplayerLobby));
            
            if (_settingsButton != null)
                _settingsButton.onClick.AddListener(() => NavigateTo(ScreenType.Settings));
            
            if (_creditsButton != null)
                _creditsButton.onClick.AddListener(() => NavigateTo(ScreenType.Credits));
            
            if (_quitButton != null)
                _quitButton.onClick.AddListener(QuitGame);
        }
        
        #endregion
        
        #region Button Actions
        
        private void ContinueGame()
        {
            if (_lastSaveGame != null)
            {
                // Load the save and start game
                // Would trigger actual game loading
                NavigateTo(ScreenType.GameHUD, _lastSaveGame);
            }
        }
        
        private void QuitGame()
        {
            UIManager.ShowConfirmation(
                "Quit Game",
                "Are you sure you want to quit?",
                () =>
                {
                    #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                    #else
                    Application.Quit();
                    #endif
                }
            );
        }
        
        private void UpdateContinueButton()
        {
            if (_continueButton != null)
            {
                _continueButton.interactable = _lastSaveGame != null;
            }
            
            if (_continueInfo != null)
            {
                _continueInfo.SetActive(_lastSaveGame != null);
                
                if (_lastSaveGame != null)
                {
                    if (_continueCharacterText != null)
                        _continueCharacterText.text = _lastSaveGame.CharacterName;
                    if (_continueOfficeText != null)
                        _continueOfficeText.text = _lastSaveGame.CurrentOffice;
                    if (_continueProgressText != null)
                        _continueProgressText.text = $"Year {_lastSaveGame.Year} • {_lastSaveGame.ApprovalRating:F0}% Approval";
                }
            }
        }
        
        private SaveGameData GetLastSave()
        {
            // Would load from save system
            // Placeholder return
            return null;
        }
        
        #endregion
    }
    
    /// <summary>
    /// New Game screen with character creation flow.
    /// </summary>
    public class NewGameScreen : BaseScreen
    {
        #region Serialized Fields
        
        [Header("Character Name")]
        [SerializeField] private InputField _characterNameInput;
        [SerializeField] private Button _randomNameButton;
        [SerializeField] private Text _nameFeedbackText;
        
        [Header("Background Selection")]
        [SerializeField] private Transform _backgroundContainer;
        [SerializeField] private GameObject _backgroundCardPrefab;
        [SerializeField] private GameObject _backgroundDetailPanel;
        [SerializeField] private Text _backgroundNameText;
        [SerializeField] private Text _backgroundDescriptionText;
        [SerializeField] private Image _backgroundPortrait;
        [SerializeField] private Transform _advantagesContainer;
        [SerializeField] private Transform _disadvantagesContainer;
        [SerializeField] private Text _specialAbilityText;
        
        [Header("Stat Allocation")]
        [SerializeField] private Transform _statSliderContainer;
        [SerializeField] private GameObject _statSliderPrefab;
        [SerializeField] private Text _pointsRemainingText;
        [SerializeField] private int _totalPoints = 10;
        [SerializeField] private int _maxPerStat = 5;
        
        [Header("Trait Selection")]
        [SerializeField] private Transform _positiveTraitsContainer;
        [SerializeField] private Transform _negativeTraitsContainer;
        [SerializeField] private GameObject _traitCardPrefab;
        [SerializeField] private Text _positiveTraitsCountText;
        [SerializeField] private Text _negativeTraitsCountText;
        [SerializeField] private int _maxPositiveTraits = 2;
        [SerializeField] private int _requiredNegativeTraits = 1;
        
        [Header("Game Options")]
        [SerializeField] private Dropdown _difficultyDropdown;
        [SerializeField] private Dropdown _startingRegionDropdown;
        [SerializeField] private Toggle _ironmanToggle;
        [SerializeField] private Slider _startingYearSlider;
        [SerializeField] private Text _startingYearText;
        
        [Header("Summary")]
        [SerializeField] private GameObject _summaryPanel;
        [SerializeField] private Text _summaryText;
        
        [Header("Navigation")]
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _startButton;
        [SerializeField] private Text _stepIndicatorText;
        
        #endregion
        
        #region Private Fields
        
        private List<BackgroundOption> _backgrounds = new List<BackgroundOption>();
        private List<TraitOption> _traits = new List<TraitOption>();
        
        private BackgroundOption _selectedBackground;
        private Dictionary<string, int> _statAllocation = new Dictionary<string, int>();
        private List<string> _selectedPositiveTraits = new List<string>();
        private List<string> _selectedNegativeTraits = new List<string>();
        
        private int _currentStep = 0;
        private readonly string[] _stepNames = { "Background", "Stats", "Traits", "Options", "Summary" };
        
        private int _pointsRemaining;
        
        #endregion
        
        #region Events
        
        public event Action<NewGameConfig> OnGameStarted;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            SetupButtons();
            InitializeStats();
        }
        
        #endregion
        
        #region Screen Lifecycle
        
        public override void OnScreenEnter(object data)
        {
            base.OnScreenEnter(data);
            
            LoadBackgrounds();
            LoadTraits();
            
            _currentStep = 0;
            ShowStep(0);
        }
        
        #endregion
        
        #region Setup
        
        private void SetupButtons()
        {
            if (_backButton != null)
                _backButton.onClick.AddListener(PreviousStep);
            
            if (_nextButton != null)
                _nextButton.onClick.AddListener(NextStep);
            
            if (_startButton != null)
                _startButton.onClick.AddListener(StartGame);
            
            if (_randomNameButton != null)
                _randomNameButton.onClick.AddListener(GenerateRandomName);
            
            if (_startingYearSlider != null)
            {
                _startingYearSlider.onValueChanged.AddListener(OnStartingYearChanged);
            }
        }
        
        private void InitializeStats()
        {
            _statAllocation["Charisma"] = 2;
            _statAllocation["Intelligence"] = 2;
            _statAllocation["Cunning"] = 2;
            _statAllocation["Resilience"] = 2;
            _statAllocation["Networking"] = 2;
            _pointsRemaining = _totalPoints - 10;
        }
        
        private void LoadBackgrounds()
        {
            // Would load from data files
            _backgrounds = new List<BackgroundOption>
            {
                new BackgroundOption
                {
                    BackgroundId = "businessman",
                    Name = "Businessman",
                    Description = "You've built your success in the private sector, managing companies and making deals.",
                    Advantages = new List<string> { "+30% Campaign Funds", "+2 Corporate Connections" },
                    Disadvantages = new List<string> { "-10% Working Class Trust", "-5% Media Favorability" },
                    SpecialAbility = "Golden Rolodex",
                    SpecialAbilityDescription = "Can call in one major financial favor per election cycle",
                    StartingFunds = 50000,
                    IsUnlocked = true
                },
                new BackgroundOption
                {
                    BackgroundId = "teacher",
                    Name = "Teacher",
                    Description = "Your background in education has given you a strong connection with families and youth.",
                    Advantages = new List<string> { "+20% Education Bloc Support", "+10% Youth Support" },
                    Disadvantages = new List<string> { "-10% Campaign Funds", "-5% Business Support" },
                    SpecialAbility = "Inspirational Speaker",
                    SpecialAbilityDescription = "Convert 10% more undecided voters during debates",
                    StartingFunds = 25000,
                    IsUnlocked = true
                },
                new BackgroundOption
                {
                    BackgroundId = "activist",
                    Name = "Activist",
                    Description = "Your background organizing grassroots movements gives you passionate supporter networks.",
                    Advantages = new List<string> { "+30% Grassroots Support", "+20% Volunteer Recruitment" },
                    Disadvantages = new List<string> { "-25% Corporate Support", "-15% Moderate Bloc Appeal" },
                    SpecialAbility = "Rabble Rouser",
                    SpecialAbilityDescription = "Can organize one free major rally per election",
                    StartingFunds = 15000,
                    IsUnlocked = true
                }
            };
            
            RefreshBackgroundDisplay();
        }
        
        private void LoadTraits()
        {
            _traits = new List<TraitOption>
            {
                // Positive traits
                new TraitOption { TraitId = "silver_tongue", Name = "Silver Tongue", IsPositive = true, 
                    Effects = new List<string> { "+15% debate effectiveness" }, IsUnlocked = true },
                new TraitOption { TraitId = "born_leader", Name = "Born Leader", IsPositive = true,
                    Effects = new List<string> { "+10% staff loyalty" }, IsUnlocked = true },
                new TraitOption { TraitId = "media_darling", Name = "Media Darling", IsPositive = true,
                    Effects = new List<string> { "+20% positive media coverage" }, IsUnlocked = true },
                new TraitOption { TraitId = "policy_wonk", Name = "Policy Wonk", IsPositive = true,
                    Effects = new List<string> { "+15% policy effectiveness" }, IsUnlocked = true },
                new TraitOption { TraitId = "fundraising_genius", Name = "Fundraising Genius", IsPositive = true,
                    Effects = new List<string> { "+25% campaign fund generation" }, IsUnlocked = true },
                
                // Negative traits
                new TraitOption { TraitId = "skeletons", Name = "Skeletons in Closet", IsPositive = false,
                    Effects = new List<string> { "Start with a minor hidden scandal" }, IsUnlocked = true },
                new TraitOption { TraitId = "foot_in_mouth", Name = "Foot in Mouth", IsPositive = false,
                    Effects = new List<string> { "10% chance to cause a gaffe during speeches" }, IsUnlocked = true },
                new TraitOption { TraitId = "thin_skinned", Name = "Thin Skinned", IsPositive = false,
                    Effects = new List<string> { "-15% effectiveness when attacked" }, IsUnlocked = true },
                new TraitOption { TraitId = "technophobe", Name = "Technophobe", IsPositive = false,
                    Effects = new List<string> { "-20% social media effectiveness" }, IsUnlocked = true }
            };
        }
        
        #endregion
        
        #region Step Navigation
        
        private void ShowStep(int step)
        {
            _currentStep = step;
            
            // Update step indicator
            if (_stepIndicatorText != null)
            {
                _stepIndicatorText.text = $"Step {step + 1} of {_stepNames.Length}: {_stepNames[step]}";
            }
            
            // Update button states
            if (_backButton != null)
                _backButton.interactable = step > 0;
            
            if (_nextButton != null)
                _nextButton.gameObject.SetActive(step < _stepNames.Length - 1);
            
            if (_startButton != null)
                _startButton.gameObject.SetActive(step == _stepNames.Length - 1);
            
            // Show appropriate panels
            // Would hide/show panels based on step
        }
        
        private void NextStep()
        {
            if (!ValidateCurrentStep()) return;
            
            if (_currentStep < _stepNames.Length - 1)
            {
                ShowStep(_currentStep + 1);
            }
        }
        
        private void PreviousStep()
        {
            if (_currentStep > 0)
            {
                ShowStep(_currentStep - 1);
            }
            else
            {
                NavigateBack();
            }
        }
        
        private bool ValidateCurrentStep()
        {
            switch (_currentStep)
            {
                case 0: // Background
                    if (_selectedBackground == null)
                    {
                        UIManager.ShowAlert("Select Background", "Please select a background for your character.");
                        return false;
                    }
                    return true;
                    
                case 1: // Stats
                    if (_pointsRemaining > 0)
                    {
                        UIManager.ShowAlert("Allocate Points", $"You have {_pointsRemaining} unallocated stat points.");
                        return false;
                    }
                    return true;
                    
                case 2: // Traits
                    if (_selectedPositiveTraits.Count < _maxPositiveTraits)
                    {
                        UIManager.ShowAlert("Select Traits", $"Please select {_maxPositiveTraits} positive traits.");
                        return false;
                    }
                    if (_selectedNegativeTraits.Count < _requiredNegativeTraits)
                    {
                        UIManager.ShowAlert("Select Traits", $"Please select at least {_requiredNegativeTraits} negative trait.");
                        return false;
                    }
                    return true;
                    
                case 3: // Options
                    var name = _characterNameInput?.text;
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        UIManager.ShowAlert("Enter Name", "Please enter a name for your character.");
                        return false;
                    }
                    return true;
                    
                default:
                    return true;
            }
        }
        
        #endregion
        
        #region Background Selection
        
        private void RefreshBackgroundDisplay()
        {
            if (_backgroundContainer == null || _backgroundCardPrefab == null) return;
            
            foreach (Transform child in _backgroundContainer)
            {
                Destroy(child.gameObject);
            }
            
            foreach (var bg in _backgrounds)
            {
                var go = Instantiate(_backgroundCardPrefab, _backgroundContainer);
                var card = go.GetComponent<BackgroundCardUI>();
                if (card != null)
                {
                    card.Setup(bg, bg.IsUnlocked, () => SelectBackground(bg));
                }
            }
        }
        
        private void SelectBackground(BackgroundOption background)
        {
            if (!background.IsUnlocked) return;
            
            _selectedBackground = background;
            ShowBackgroundDetails(background);
        }
        
        private void ShowBackgroundDetails(BackgroundOption background)
        {
            if (_backgroundDetailPanel != null)
                _backgroundDetailPanel.SetActive(true);
            
            if (_backgroundNameText != null)
                _backgroundNameText.text = background.Name;
            
            if (_backgroundDescriptionText != null)
                _backgroundDescriptionText.text = background.Description;
            
            if (_backgroundPortrait != null && background.Portrait != null)
                _backgroundPortrait.sprite = background.Portrait;
            
            // Advantages
            if (_advantagesContainer != null)
            {
                foreach (Transform child in _advantagesContainer)
                    Destroy(child.gameObject);
                
                foreach (var adv in background.Advantages)
                {
                    CreateTextItem(_advantagesContainer, adv, Color.green);
                }
            }
            
            // Disadvantages
            if (_disadvantagesContainer != null)
            {
                foreach (Transform child in _disadvantagesContainer)
                    Destroy(child.gameObject);
                
                foreach (var dis in background.Disadvantages)
                {
                    CreateTextItem(_disadvantagesContainer, dis, Color.red);
                }
            }
            
            if (_specialAbilityText != null)
                _specialAbilityText.text = $"{background.SpecialAbility}: {background.SpecialAbilityDescription}";
        }
        
        private void CreateTextItem(Transform container, string text, Color color)
        {
            var go = new GameObject("Item");
            go.transform.SetParent(container, false);
            var t = go.AddComponent<Text>();
            t.text = text;
            t.color = color;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = 14;
        }
        
        #endregion
        
        #region Stat Allocation
        
        private void UpdateStatDisplay()
        {
            if (_pointsRemainingText != null)
            {
                _pointsRemainingText.text = $"Points Remaining: {_pointsRemaining}";
                _pointsRemainingText.color = _pointsRemaining > 0 ? Color.yellow : Color.white;
            }
        }
        
        public void OnStatChanged(string statName, int newValue)
        {
            int oldValue = _statAllocation.GetValueOrDefault(statName, 2);
            int delta = newValue - oldValue;
            
            if (_pointsRemaining - delta < 0 || newValue > _maxPerStat || newValue < 0)
            {
                // Reject change
                return;
            }
            
            _statAllocation[statName] = newValue;
            _pointsRemaining -= delta;
            UpdateStatDisplay();
        }
        
        #endregion
        
        #region Trait Selection
        
        private void ToggleTrait(TraitOption trait)
        {
            if (trait.IsPositive)
            {
                if (_selectedPositiveTraits.Contains(trait.TraitId))
                {
                    _selectedPositiveTraits.Remove(trait.TraitId);
                }
                else if (_selectedPositiveTraits.Count < _maxPositiveTraits)
                {
                    _selectedPositiveTraits.Add(trait.TraitId);
                }
            }
            else
            {
                if (_selectedNegativeTraits.Contains(trait.TraitId))
                {
                    _selectedNegativeTraits.Remove(trait.TraitId);
                }
                else
                {
                    _selectedNegativeTraits.Add(trait.TraitId);
                }
            }
            
            UpdateTraitCountDisplay();
        }
        
        private void UpdateTraitCountDisplay()
        {
            if (_positiveTraitsCountText != null)
                _positiveTraitsCountText.text = $"{_selectedPositiveTraits.Count}/{_maxPositiveTraits}";
            
            if (_negativeTraitsCountText != null)
                _negativeTraitsCountText.text = $"{_selectedNegativeTraits.Count}/{_requiredNegativeTraits}+";
        }
        
        #endregion
        
        #region Game Options
        
        private void GenerateRandomName()
        {
            string[] firstNames = { "John", "Jane", "Robert", "Maria", "James", "Elizabeth", "William", "Sarah" };
            string[] lastNames = { "Smith", "Johnson", "Williams", "Brown", "Jones", "Davis", "Miller", "Wilson" };
            
            string name = firstNames[UnityEngine.Random.Range(0, firstNames.Length)] + " " +
                         lastNames[UnityEngine.Random.Range(0, lastNames.Length)];
            
            if (_characterNameInput != null)
                _characterNameInput.text = name;
        }
        
        private void OnStartingYearChanged(float value)
        {
            int year = 2020 + (int)value * 4;
            if (_startingYearText != null)
                _startingYearText.text = year.ToString();
        }
        
        #endregion
        
        #region Start Game
        
        private void StartGame()
        {
            if (!ValidateCurrentStep()) return;
            
            var config = new NewGameConfig
            {
                CharacterName = _characterNameInput?.text ?? "Candidate",
                BackgroundId = _selectedBackground?.BackgroundId,
                StatAllocation = new Dictionary<string, int>(_statAllocation),
                SelectedTraits = _selectedPositiveTraits.Concat(_selectedNegativeTraits).ToList(),
                Difficulty = _difficultyDropdown?.value ?? 1,
                StartingRegion = "US-East",
                IronmanMode = _ironmanToggle?.isOn ?? false,
                StartingYear = 2024
            };
            
            OnGameStarted?.Invoke(config);
            NavigateTo(ScreenType.GameHUD, config);
        }
        
        #endregion
    }
    
    /// <summary>
    /// Settings screen with audio, graphics, and gameplay options.
    /// </summary>
    public class SettingsScreen : BaseScreen
    {
        #region Serialized Fields
        
        [Header("Tabs")]
        [SerializeField] private Button _audioTabButton;
        [SerializeField] private Button _graphicsTabButton;
        [SerializeField] private Button _gameplayTabButton;
        [SerializeField] private Button _accessibilityTabButton;
        
        [Header("Tab Panels")]
        [SerializeField] private GameObject _audioPanel;
        [SerializeField] private GameObject _graphicsPanel;
        [SerializeField] private GameObject _gameplayPanel;
        [SerializeField] private GameObject _accessibilityPanel;
        
        [Header("Audio Settings")]
        [SerializeField] private Slider _masterVolumeSlider;
        [SerializeField] private Slider _musicVolumeSlider;
        [SerializeField] private Slider _sfxVolumeSlider;
        [SerializeField] private Slider _voiceVolumeSlider;
        [SerializeField] private Toggle _muteUnfocusedToggle;
        [SerializeField] private AudioMixer _audioMixer;
        
        [Header("Graphics Settings")]
        [SerializeField] private Dropdown _resolutionDropdown;
        [SerializeField] private Toggle _fullscreenToggle;
        [SerializeField] private Dropdown _qualityDropdown;
        [SerializeField] private Toggle _vsyncToggle;
        [SerializeField] private Slider _framerateSlider;
        [SerializeField] private Text _framerateText;
        
        [Header("Gameplay Settings")]
        [SerializeField] private Slider _textSpeedSlider;
        [SerializeField] private Toggle _tutorialsToggle;
        [SerializeField] private Toggle _autosaveToggle;
        [SerializeField] private Slider _autosaveIntervalSlider;
        [SerializeField] private Text _autosaveIntervalText;
        [SerializeField] private Toggle _confirmActionsToggle;
        [SerializeField] private Toggle _showProbabilitiesToggle;
        
        [Header("Accessibility Settings")]
        [SerializeField] private Slider _uiScaleSlider;
        [SerializeField] private Toggle _highContrastToggle;
        [SerializeField] private Toggle _screenReaderToggle;
        [SerializeField] private Toggle _reduceAnimationsToggle;
        [SerializeField] private Dropdown _fontSizeDropdown;
        
        [Header("Buttons")]
        [SerializeField] private Button _applyButton;
        [SerializeField] private Button _resetButton;
        [SerializeField] private Button _backButton;
        
        #endregion
        
        #region Private Fields
        
        private GameSettings _settings;
        private GameSettings _originalSettings;
        private Resolution[] _resolutions;
        private int _currentTab;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            SetupButtons();
            SetupSliders();
        }
        
        #endregion
        
        #region Screen Lifecycle
        
        public override void OnScreenEnter(object data)
        {
            base.OnScreenEnter(data);
            
            LoadSettings();
            PopulateResolutions();
            ShowTab(0);
        }
        
        public override bool CanNavigateBack()
        {
            // Check for unsaved changes
            return true;
        }
        
        #endregion
        
        #region Setup
        
        private void SetupButtons()
        {
            if (_audioTabButton != null)
                _audioTabButton.onClick.AddListener(() => ShowTab(0));
            if (_graphicsTabButton != null)
                _graphicsTabButton.onClick.AddListener(() => ShowTab(1));
            if (_gameplayTabButton != null)
                _gameplayTabButton.onClick.AddListener(() => ShowTab(2));
            if (_accessibilityTabButton != null)
                _accessibilityTabButton.onClick.AddListener(() => ShowTab(3));
            
            if (_applyButton != null)
                _applyButton.onClick.AddListener(ApplySettings);
            if (_resetButton != null)
                _resetButton.onClick.AddListener(ResetToDefaults);
            if (_backButton != null)
                _backButton.onClick.AddListener(NavigateBack);
        }
        
        private void SetupSliders()
        {
            // Audio
            if (_masterVolumeSlider != null)
                _masterVolumeSlider.onValueChanged.AddListener(v => OnMasterVolumeChanged(v));
            if (_musicVolumeSlider != null)
                _musicVolumeSlider.onValueChanged.AddListener(v => _settings.MusicVolume = v);
            if (_sfxVolumeSlider != null)
                _sfxVolumeSlider.onValueChanged.AddListener(v => _settings.SFXVolume = v);
            
            // Gameplay
            if (_autosaveIntervalSlider != null)
            {
                _autosaveIntervalSlider.onValueChanged.AddListener(v =>
                {
                    _settings.AutoSaveInterval = (int)v;
                    if (_autosaveIntervalText != null)
                        _autosaveIntervalText.text = $"{(int)v} turns";
                });
            }
            
            if (_framerateSlider != null)
            {
                _framerateSlider.onValueChanged.AddListener(v =>
                {
                    _settings.TargetFramerate = (int)v;
                    if (_framerateText != null)
                        _framerateText.text = v >= 120 ? "Unlimited" : $"{(int)v} FPS";
                });
            }
        }
        
        #endregion
        
        #region Tab Navigation
        
        private void ShowTab(int index)
        {
            _currentTab = index;
            
            if (_audioPanel != null) _audioPanel.SetActive(index == 0);
            if (_graphicsPanel != null) _graphicsPanel.SetActive(index == 1);
            if (_gameplayPanel != null) _gameplayPanel.SetActive(index == 2);
            if (_accessibilityPanel != null) _accessibilityPanel.SetActive(index == 3);
        }
        
        #endregion
        
        #region Settings Management
        
        private void LoadSettings()
        {
            // Would load from PlayerPrefs or save file
            _settings = new GameSettings();
            _originalSettings = new GameSettings();
            
            ApplyToUI();
        }
        
        private void ApplyToUI()
        {
            // Audio
            if (_masterVolumeSlider != null) _masterVolumeSlider.value = _settings.MasterVolume;
            if (_musicVolumeSlider != null) _musicVolumeSlider.value = _settings.MusicVolume;
            if (_sfxVolumeSlider != null) _sfxVolumeSlider.value = _settings.SFXVolume;
            if (_voiceVolumeSlider != null) _voiceVolumeSlider.value = _settings.VoiceVolume;
            if (_muteUnfocusedToggle != null) _muteUnfocusedToggle.isOn = _settings.MuteWhenUnfocused;
            
            // Graphics
            if (_fullscreenToggle != null) _fullscreenToggle.isOn = _settings.Fullscreen;
            if (_qualityDropdown != null) _qualityDropdown.value = _settings.QualityLevel;
            if (_vsyncToggle != null) _vsyncToggle.isOn = _settings.VSync;
            
            // Gameplay
            if (_textSpeedSlider != null) _textSpeedSlider.value = _settings.TextSpeed;
            if (_tutorialsToggle != null) _tutorialsToggle.isOn = _settings.ShowTutorials;
            if (_autosaveToggle != null) _autosaveToggle.isOn = _settings.AutoSave;
            if (_confirmActionsToggle != null) _confirmActionsToggle.isOn = _settings.ConfirmActions;
            if (_showProbabilitiesToggle != null) _showProbabilitiesToggle.isOn = _settings.ShowProbabilities;
            
            // Accessibility
            if (_uiScaleSlider != null) _uiScaleSlider.value = _settings.UIScale;
            if (_highContrastToggle != null) _highContrastToggle.isOn = _settings.HighContrast;
            if (_reduceAnimationsToggle != null) _reduceAnimationsToggle.isOn = _settings.ReduceAnimations;
        }
        
        private void PopulateResolutions()
        {
            _resolutions = Screen.resolutions;
            
            if (_resolutionDropdown != null)
            {
                _resolutionDropdown.ClearOptions();
                
                List<string> options = new List<string>();
                int currentIndex = 0;
                
                for (int i = 0; i < _resolutions.Length; i++)
                {
                    string option = $"{_resolutions[i].width} x {_resolutions[i].height} @ {_resolutions[i].refreshRate}Hz";
                    options.Add(option);
                    
                    if (_resolutions[i].width == Screen.currentResolution.width &&
                        _resolutions[i].height == Screen.currentResolution.height)
                    {
                        currentIndex = i;
                    }
                }
                
                _resolutionDropdown.AddOptions(options);
                _resolutionDropdown.value = currentIndex;
            }
        }
        
        private void OnMasterVolumeChanged(float value)
        {
            _settings.MasterVolume = value;
            
            if (_audioMixer != null)
            {
                float db = value > 0 ? Mathf.Log10(value) * 20 : -80f;
                _audioMixer.SetFloat("MasterVolume", db);
            }
        }
        
        private void ApplySettings()
        {
            // Apply graphics settings
            if (_resolutionDropdown != null && _resolutions != null)
            {
                Resolution res = _resolutions[_resolutionDropdown.value];
                Screen.SetResolution(res.width, res.height, _settings.Fullscreen);
            }
            
            QualitySettings.SetQualityLevel(_settings.QualityLevel);
            QualitySettings.vSyncCount = _settings.VSync ? 1 : 0;
            Application.targetFrameRate = _settings.TargetFramerate;
            
            // Save settings
            SaveSettings();
            
            UIManager.ShowToast("Settings Applied", "Your settings have been saved.", NotificationType.Success);
        }
        
        private void SaveSettings()
        {
            // Would save to PlayerPrefs or file
            PlayerPrefs.SetFloat("MasterVolume", _settings.MasterVolume);
            PlayerPrefs.SetFloat("MusicVolume", _settings.MusicVolume);
            PlayerPrefs.SetFloat("SFXVolume", _settings.SFXVolume);
            PlayerPrefs.SetInt("Fullscreen", _settings.Fullscreen ? 1 : 0);
            PlayerPrefs.SetInt("QualityLevel", _settings.QualityLevel);
            PlayerPrefs.Save();
        }
        
        private void ResetToDefaults()
        {
            UIManager.ShowConfirmation(
                "Reset Settings",
                "Are you sure you want to reset all settings to defaults?",
                () =>
                {
                    _settings = new GameSettings();
                    ApplyToUI();
                    ApplySettings();
                }
            );
        }
        
        #endregion
    }
    
    /// <summary>
    /// Load Game screen with save file browser.
    /// </summary>
    public class LoadGameScreen : BaseScreen
    {
        #region Serialized Fields
        
        [Header("Save List")]
        [SerializeField] private Transform _saveListContainer;
        [SerializeField] private GameObject _saveCardPrefab;
        [SerializeField] private Toggle _showAutosavesToggle;
        
        [Header("Save Details")]
        [SerializeField] private GameObject _detailsPanel;
        [SerializeField] private Image _screenshotImage;
        [SerializeField] private Text _characterNameText;
        [SerializeField] private Text _officeText;
        [SerializeField] private Text _yearText;
        [SerializeField] private Text _approvalText;
        [SerializeField] private Text _saveDateText;
        [SerializeField] private Text _playTimeText;
        
        [Header("Actions")]
        [SerializeField] private Button _loadButton;
        [SerializeField] private Button _deleteButton;
        [SerializeField] private Button _backButton;
        
        #endregion
        
        #region Private Fields
        
        private List<SaveGameData> _saveGames = new List<SaveGameData>();
        private SaveGameData _selectedSave;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            SetupButtons();
        }
        
        #endregion
        
        #region Screen Lifecycle
        
        public override void OnScreenEnter(object data)
        {
            base.OnScreenEnter(data);
            
            LoadSaveList();
            RefreshSaveList();
            
            if (_detailsPanel != null)
                _detailsPanel.SetActive(false);
        }
        
        #endregion
        
        #region Setup
        
        private void SetupButtons()
        {
            if (_loadButton != null)
                _loadButton.onClick.AddListener(LoadSelectedSave);
            
            if (_deleteButton != null)
                _deleteButton.onClick.AddListener(DeleteSelectedSave);
            
            if (_backButton != null)
                _backButton.onClick.AddListener(NavigateBack);
            
            if (_showAutosavesToggle != null)
                _showAutosavesToggle.onValueChanged.AddListener(_ => RefreshSaveList());
        }
        
        #endregion
        
        #region Save Management
        
        private void LoadSaveList()
        {
            // Would load from save system
            _saveGames = new List<SaveGameData>
            {
                new SaveGameData
                {
                    SaveId = "save1",
                    CharacterName = "John Smith",
                    CurrentOffice = "Governor",
                    Year = 2028,
                    Turn = 48,
                    ApprovalRating = 62.5f,
                    SaveDate = DateTime.Now.AddHours(-2),
                    PlayTime = "12:34:56",
                    IsAutoSave = false
                }
            };
        }
        
        private void RefreshSaveList()
        {
            if (_saveListContainer == null) return;
            
            foreach (Transform child in _saveListContainer)
            {
                Destroy(child.gameObject);
            }
            
            bool showAutosaves = _showAutosavesToggle?.isOn ?? true;
            var filtered = _saveGames.Where(s => showAutosaves || !s.IsAutoSave);
            
            foreach (var save in filtered.OrderByDescending(s => s.SaveDate))
            {
                if (_saveCardPrefab != null)
                {
                    var go = Instantiate(_saveCardPrefab, _saveListContainer);
                    var card = go.GetComponent<SaveCardUI>();
                    if (card != null)
                    {
                        card.Setup(save, () => SelectSave(save));
                    }
                }
            }
        }
        
        private void SelectSave(SaveGameData save)
        {
            _selectedSave = save;
            ShowSaveDetails(save);
        }
        
        private void ShowSaveDetails(SaveGameData save)
        {
            if (_detailsPanel != null)
                _detailsPanel.SetActive(true);
            
            if (_screenshotImage != null && save.Screenshot != null)
                _screenshotImage.sprite = save.Screenshot;
            
            if (_characterNameText != null) _characterNameText.text = save.CharacterName;
            if (_officeText != null) _officeText.text = save.CurrentOffice;
            if (_yearText != null) _yearText.text = $"Year {save.Year}";
            if (_approvalText != null) _approvalText.text = $"{save.ApprovalRating:F1}% Approval";
            if (_saveDateText != null) _saveDateText.text = save.SaveDate.ToString("g");
            if (_playTimeText != null) _playTimeText.text = $"Playtime: {save.PlayTime}";
        }
        
        private void LoadSelectedSave()
        {
            if (_selectedSave == null) return;
            
            // Would load the save and transition to game
            NavigateTo(ScreenType.GameHUD, _selectedSave);
        }
        
        private void DeleteSelectedSave()
        {
            if (_selectedSave == null) return;
            
            UIManager.ShowConfirmation(
                "Delete Save",
                $"Are you sure you want to delete save '{_selectedSave.CharacterName}'? This cannot be undone.",
                () =>
                {
                    _saveGames.Remove(_selectedSave);
                    _selectedSave = null;
                    RefreshSaveList();
                    
                    if (_detailsPanel != null)
                        _detailsPanel.SetActive(false);
                }
            );
        }
        
        #endregion
    }
    
    #region UI Component Classes
    
    public class BackgroundCardUI : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [SerializeField] private Text _nameText;
        [SerializeField] private GameObject _lockedOverlay;
        [SerializeField] private Text _lockReasonText;
        [SerializeField] private Image _selectedBorder;
        
        public void Setup(BackgroundOption background, bool isUnlocked, Action onClick)
        {
            if (_icon != null && background.Icon != null) _icon.sprite = background.Icon;
            if (_nameText != null) _nameText.text = background.Name;
            if (_lockedOverlay != null) _lockedOverlay.SetActive(!isUnlocked);
            if (_lockReasonText != null && !isUnlocked) _lockReasonText.text = background.UnlockRequirement;
            
            if (_button != null)
            {
                _button.interactable = isUnlocked;
                _button.onClick.AddListener(() => onClick());
            }
        }
        
        public void SetSelected(bool selected)
        {
            if (_selectedBorder != null)
                _selectedBorder.gameObject.SetActive(selected);
        }
    }
    
    public class SaveCardUI : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Text _characterNameText;
        [SerializeField] private Text _officeText;
        [SerializeField] private Text _dateText;
        [SerializeField] private Image _autoSaveIcon;
        
        public void Setup(SaveGameData save, Action onClick)
        {
            if (_characterNameText != null) _characterNameText.text = save.CharacterName;
            if (_officeText != null) _officeText.text = $"{save.CurrentOffice} - Year {save.Year}";
            if (_dateText != null) _dateText.text = save.SaveDate.ToString("g");
            if (_autoSaveIcon != null) _autoSaveIcon.gameObject.SetActive(save.IsAutoSave);
            if (_button != null) _button.onClick.AddListener(() => onClick());
        }
    }
    
    #endregion
}
