using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using ElectionEmpire.Character;

namespace ElectionEmpire.UI
{
    /// <summary>
    /// 8-step manual character builder wizard
    /// </summary>
    public class CharacterBuilderUI : MonoBehaviour
    {
        [Header("Wizard Steps")]
        public GameObject[] StepPanels; // 8 panels for 8 steps
        public Button NextButton;
        public Button BackButton;
        public TextMeshProUGUI ProgressText; // "Step 1/8"
        
        [Header("Step 1: Background")]
        public Dropdown BackgroundDropdown;
        public TextMeshProUGUI BackgroundDescription;
        public Toggle[] TierFilters; // Respectable, Questionable, Absurd, Criminal, Celebrity
        
        [Header("Step 2: Personal History")]
        public Transform HistoryContainer;
        public Toggle[] HistoryToggles;
        
        [Header("Step 3: Public Image")]
        public Transform ImageContainer;
        public ToggleGroup ImageToggleGroup;
        
        [Header("Step 4: Skills")]
        public Transform SkillsContainer;
        public Toggle[] SkillToggles;
        public TextMeshProUGUI SkillsSelectedText; // "3/3 selected"
        
        [Header("Step 5: Quirks")]
        public Transform PositiveQuirksContainer;
        public Transform NegativeQuirksContainer;
        public Toggle[] PositiveQuirkToggles;
        public Toggle[] NegativeQuirkToggles;
        public TextMeshProUGUI PositiveQuirksText; // "2/2 selected"
        public TextMeshProUGUI NegativeQuirksText; // "2/2 selected"
        
        [Header("Step 6: Handicaps")]
        public Transform HandicapsContainer;
        public Toggle[] HandicapToggles;
        
        [Header("Step 7: Weapon")]
        public Transform WeaponContainer;
        public ToggleGroup WeaponToggleGroup;
        
        [Header("Step 8: Preview")]
        public CharacterDisplay PreviewDisplay;
        public TMP_InputField CustomNameInput;
        public Button SaveToLibraryButton;
        public Button StartButton;
        
        private int _currentStep = 0;
        private Character _buildingCharacter;
        private CharacterDataLoader _dataLoader;
        
        public System.Action<Character> OnCharacterBuilt;
        
        private void Start()
        {
            _dataLoader = CharacterDataLoader.Instance;
            _buildingCharacter = new Character();
            
            if (NextButton != null)
                NextButton.onClick.AddListener(OnNext);
            
            if (BackButton != null)
                BackButton.onClick.AddListener(OnBack);
            
            if (SaveToLibraryButton != null)
                SaveToLibraryButton.onClick.AddListener(OnSaveToLibrary);
            
            if (StartButton != null)
                StartButton.onClick.AddListener(OnStart);
            
            SetupStep1();
            ShowStep(0);
        }
        
        public void StartBuilder()
        {
            _currentStep = 0;
            _buildingCharacter = new Character();
            ShowStep(0);
            SetupStep1();
        }
        
        /// <summary>
        /// Load a character for editing
        /// </summary>
        public void LoadCharacterForEditing(Character character)
        {
            if (character == null) return;
            
            _buildingCharacter = character;
            _currentStep = 0;
            
            // Pre-fill all steps with character data
            ShowStep(0);
            SetupStep1();
            
            // Select the character's background
            if (BackgroundDropdown != null && character.Background != null)
            {
                var backgrounds = _dataLoader.GetBackgrounds();
                int index = backgrounds.FindIndex(b => b.ID == character.Background.ID);
                if (index >= 0)
                {
                    BackgroundDropdown.value = index;
                    OnBackgroundSelected(index);
                }
            }
            
            // Note: Other steps would need similar pre-filling logic
            // For now, user can navigate through steps to see/edit selections
        }
        
        private void ShowStep(int step)
        {
            _currentStep = step;
            
            for (int i = 0; i < StepPanels.Length; i++)
            {
                if (StepPanels[i] != null)
                    StepPanels[i].SetActive(i == step);
            }
            
            if (ProgressText != null)
                ProgressText.text = $"Step {step + 1}/8";
            
            if (BackButton != null)
                BackButton.interactable = step > 0;
            
            if (NextButton != null)
            {
                NextButton.interactable = CanProceedFromStep(step);
                NextButton.GetComponentInChildren<TextMeshProUGUI>().text = step == 7 ? "Preview" : "Next";
            }
        }
        
        private bool CanProceedFromStep(int step)
        {
            switch (step)
            {
                case 0: return _buildingCharacter.Background != null;
                case 1: return true; // History is optional
                case 2: return _buildingCharacter.PublicImage != null;
                case 3: return _buildingCharacter.Skills != null && _buildingCharacter.Skills.Count == 3;
                case 4: return _buildingCharacter.PositiveQuirks != null && _buildingCharacter.PositiveQuirks.Count == 2 &&
                          _buildingCharacter.NegativeQuirks != null && _buildingCharacter.NegativeQuirks.Count == 2;
                case 5: return true; // Handicaps are optional
                case 6: return _buildingCharacter.Weapon != null;
                case 7: return true; // Preview
                default: return false;
            }
        }
        
        private void OnNext()
        {
            if (_currentStep < 7)
            {
                ShowStep(_currentStep + 1);
                
                // Setup next step
                switch (_currentStep)
                {
                    case 1: SetupStep2(); break;
                    case 2: SetupStep3(); break;
                    case 3: SetupStep4(); break;
                    case 4: SetupStep5(); break;
                    case 5: SetupStep6(); break;
                    case 6: SetupStep7(); break;
                    case 7: SetupStep8(); break;
                }
            }
        }
        
        private void OnBack()
        {
            if (_currentStep > 0)
            {
                ShowStep(_currentStep - 1);
            }
        }
        
        // STEP 1: Background
        private void SetupStep1()
        {
            if (BackgroundDropdown != null)
            {
                BackgroundDropdown.ClearOptions();
                var backgrounds = _dataLoader.GetBackgrounds();
                var options = new List<string>();
                
                foreach (var bg in backgrounds)
                {
                    options.Add($"{bg.Name} ({bg.Tier})");
                }
                
                BackgroundDropdown.AddOptions(options);
                BackgroundDropdown.onValueChanged.AddListener(OnBackgroundSelected);
                
                if (backgrounds.Count > 0)
                    OnBackgroundSelected(0);
            }
        }
        
        private void OnBackgroundSelected(int index)
        {
            var backgrounds = _dataLoader.GetBackgrounds();
            if (index >= 0 && index < backgrounds.Count)
            {
                _buildingCharacter.Background = backgrounds[index];
                if (BackgroundDescription != null)
                    BackgroundDescription.text = backgrounds[index].Description;
            }
        }
        
        // STEP 2: Personal History
        private void SetupStep2()
        {
            if (HistoryContainer != null)
            {
                ClearContainer(HistoryContainer);
                var history = _dataLoader.GetPersonalHistory();
                
                foreach (var item in history)
                {
                    GameObject toggleObj = new GameObject($"Toggle_{item}");
                    toggleObj.transform.SetParent(HistoryContainer);
                    
                    Toggle toggle = toggleObj.AddComponent<Toggle>();
                    TextMeshProUGUI label = toggleObj.AddComponent<TextMeshProUGUI>();
                    label.text = item;
                    label.fontSize = 14;
                    
                    toggle.onValueChanged.AddListener((value) => {
                        if (value)
                        {
                            if (!_buildingCharacter.PersonalHistory.Contains(item))
                                _buildingCharacter.PersonalHistory.Add(item);
                        }
                        else
                        {
                            _buildingCharacter.PersonalHistory.Remove(item);
                        }
                    });
                }
            }
        }
        
        // STEP 3: Public Image
        private void SetupStep3()
        {
            if (ImageContainer != null)
            {
                ClearContainer(ImageContainer);
                var images = _dataLoader.GetPublicImages();
                
                foreach (var image in images)
                {
                    GameObject toggleObj = new GameObject($"Toggle_{image.ID}");
                    toggleObj.transform.SetParent(ImageContainer);
                    
                    Toggle toggle = toggleObj.AddComponent<Toggle>();
                    toggle.group = ImageToggleGroup;
                    
                    TextMeshProUGUI label = toggleObj.AddComponent<TextMeshProUGUI>();
                    label.text = $"{image.Name}\n{image.Description}";
                    label.fontSize = 14;
                    
                    toggle.onValueChanged.AddListener((value) => {
                        if (value)
                            _buildingCharacter.PublicImage = image;
                    });
                }
            }
        }
        
        // STEP 4: Skills
        private void SetupStep4()
        {
            if (SkillsContainer != null)
            {
                ClearContainer(SkillsContainer);
                var skills = _dataLoader.GetSkills();
                
                foreach (var skill in skills)
                {
                    GameObject toggleObj = new GameObject($"Toggle_{skill.ID}");
                    toggleObj.transform.SetParent(SkillsContainer);
                    
                    Toggle toggle = toggleObj.AddComponent<Toggle>();
                    TextMeshProUGUI label = toggleObj.AddComponent<TextMeshProUGUI>();
                    label.text = $"{skill.Name} ({skill.Category})\n{skill.Description}";
                    label.fontSize = 12;
                    
                    toggle.onValueChanged.AddListener((value) => {
                        if (value)
                        {
                            if (_buildingCharacter.Skills.Count < 3)
                                _buildingCharacter.Skills.Add(skill);
                            else
                                toggle.isOn = false; // Can't select more than 3
                        }
                        else
                        {
                            _buildingCharacter.Skills.Remove(skill);
                        }
                        
                        UpdateSkillsCount();
                    });
                }
                
                UpdateSkillsCount();
            }
        }
        
        private void UpdateSkillsCount()
        {
            if (SkillsSelectedText != null)
                SkillsSelectedText.text = $"{_buildingCharacter.Skills.Count}/3 selected";
        }
        
        // STEP 5: Quirks
        private void SetupStep5()
        {
            if (PositiveQuirksContainer != null)
            {
                ClearContainer(PositiveQuirksContainer);
                var quirks = _dataLoader.GetPositiveQuirks();
                
                foreach (var quirk in quirks)
                {
                    GameObject toggleObj = new GameObject($"Toggle_{quirk.ID}");
                    toggleObj.transform.SetParent(PositiveQuirksContainer);
                    
                    Toggle toggle = toggleObj.AddComponent<Toggle>();
                    TextMeshProUGUI label = toggleObj.AddComponent<TextMeshProUGUI>();
                    label.text = $"{quirk.Name}\n{quirk.Description}";
                    label.fontSize = 12;
                    
                    toggle.onValueChanged.AddListener((value) => {
                        if (value)
                        {
                            if (_buildingCharacter.PositiveQuirks.Count < 2)
                                _buildingCharacter.PositiveQuirks.Add(quirk);
                            else
                                toggle.isOn = false;
                        }
                        else
                        {
                            _buildingCharacter.PositiveQuirks.Remove(quirk);
                        }
                        
                        UpdateQuirksCount();
                    });
                }
            }
            
            if (NegativeQuirksContainer != null)
            {
                ClearContainer(NegativeQuirksContainer);
                var quirks = _dataLoader.GetNegativeQuirks();
                
                foreach (var quirk in quirks)
                {
                    GameObject toggleObj = new GameObject($"Toggle_{quirk.ID}");
                    toggleObj.transform.SetParent(NegativeQuirksContainer);
                    
                    Toggle toggle = toggleObj.AddComponent<Toggle>();
                    TextMeshProUGUI label = toggleObj.AddComponent<TextMeshProUGUI>();
                    label.text = $"{quirk.Name}\n{quirk.Description}";
                    label.fontSize = 12;
                    
                    toggle.onValueChanged.AddListener((value) => {
                        if (value)
                        {
                            if (_buildingCharacter.NegativeQuirks.Count < 2)
                                _buildingCharacter.NegativeQuirks.Add(quirk);
                            else
                                toggle.isOn = false;
                        }
                        else
                        {
                            _buildingCharacter.NegativeQuirks.Remove(quirk);
                        }
                        
                        UpdateQuirksCount();
                    });
                }
            }
            
            UpdateQuirksCount();
        }
        
        private void UpdateQuirksCount()
        {
            if (PositiveQuirksText != null)
                PositiveQuirksText.text = $"{_buildingCharacter.PositiveQuirks.Count}/2 selected";
            
            if (NegativeQuirksText != null)
                NegativeQuirksText.text = $"{_buildingCharacter.NegativeQuirks.Count}/2 selected";
        }
        
        // STEP 6: Handicaps
        private void SetupStep6()
        {
            if (HandicapsContainer != null)
            {
                ClearContainer(HandicapsContainer);
                var handicaps = _dataLoader.GetHandicaps();
                
                foreach (var handicap in handicaps)
                {
                    GameObject toggleObj = new GameObject($"Toggle_{handicap.ID}");
                    toggleObj.transform.SetParent(HandicapsContainer);
                    
                    Toggle toggle = toggleObj.AddComponent<Toggle>();
                    TextMeshProUGUI label = toggleObj.AddComponent<TextMeshProUGUI>();
                    label.text = $"{handicap.Name} ({handicap.Category})\n{handicap.Description}\n+{handicap.LegacyPointBonus} Legacy Points";
                    label.fontSize = 12;
                    
                    toggle.onValueChanged.AddListener((value) => {
                        if (value)
                        {
                            if (_buildingCharacter.Handicaps.Count < 3)
                                _buildingCharacter.Handicaps.Add(handicap);
                            else
                                toggle.isOn = false;
                        }
                        else
                        {
                            _buildingCharacter.Handicaps.Remove(handicap);
                        }
                    });
                }
            }
        }
        
        // STEP 7: Weapon
        private void SetupStep7()
        {
            if (WeaponContainer != null)
            {
                ClearContainer(WeaponContainer);
                var weapons = _dataLoader.GetWeapons();
                
                foreach (var weapon in weapons)
                {
                    GameObject toggleObj = new GameObject($"Toggle_{weapon.ID}");
                    toggleObj.transform.SetParent(WeaponContainer);
                    
                    Toggle toggle = toggleObj.AddComponent<Toggle>();
                    toggle.group = WeaponToggleGroup;
                    
                    TextMeshProUGUI label = toggleObj.AddComponent<TextMeshProUGUI>();
                    label.text = $"{weapon.Name} ({weapon.Category})\n{weapon.Description}";
                    label.fontSize = 12;
                    
                    toggle.onValueChanged.AddListener((value) => {
                        if (value)
                            _buildingCharacter.Weapon = weapon;
                    });
                }
            }
        }
        
        // STEP 8: Preview
        private void SetupStep8()
        {
            // Calculate final stats
            var generator = new CharacterGenerator();
            _buildingCharacter.Name = CustomNameInput != null && !string.IsNullOrEmpty(CustomNameInput.text) 
                ? CustomNameInput.text 
                : "Custom Character";
            _buildingCharacter.GeneratedNickname = generator.GenerateNickname(_buildingCharacter);
            _buildingCharacter.ChaosRating = generator.CalculateChaosRating(_buildingCharacter);
            _buildingCharacter.DifficultyMultiplier = generator.CalculateDifficultyMultiplier(_buildingCharacter);
            _buildingCharacter.LegacyPointBonus = generator.CalculateLegacyBonus(_buildingCharacter);
            
            if (PreviewDisplay != null)
                PreviewDisplay.DisplayCharacter(_buildingCharacter);
        }
        
        private void OnSaveToLibrary()
        {
            string name = CustomNameInput != null && !string.IsNullOrEmpty(CustomNameInput.text)
                ? CustomNameInput.text
                : _buildingCharacter.GeneratedNickname ?? _buildingCharacter.Name;
            
            CharacterLibrary.Instance.SaveCharacter(_buildingCharacter, name);
            Debug.Log($"Character saved to library: {name}");
        }
        
        private void OnStart()
        {
            OnCharacterBuilt?.Invoke(_buildingCharacter);
        }
        
        private void ClearContainer(Transform container)
        {
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }
        }
    }
}

