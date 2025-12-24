// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - Event & Crisis Screens
// Sprint 9: Event popups, crisis management, scandal response, election night
// ═══════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionEmpire.UI.Screens
{
    #region Event Data Classes
    
    /// <summary>
    /// Event popup data.
    /// </summary>
    [Serializable]
    public class GameEventData
    {
        public string EventId;
        public string Title;
        public string Description;
        public string Category;
        public Sprite EventImage;
        public List<EventChoice> Choices;
        public float TimeLimit; // 0 = no limit
        public bool IsUrgent;
        public string SourceInfo; // "Breaking News", "Staff Report", etc.
    }
    
    [Serializable]
    public class EventChoice
    {
        public string ChoiceId;
        public string Text;
        public string Tooltip;
        public List<string> Requirements;
        public List<string> Consequences;
        public bool IsAvailable;
        public float SuccessChance;
        public int ResourceCost;
        public string ResourceType;
    }
    
    /// <summary>
    /// Crisis data for management screen.
    /// </summary>
    [Serializable]
    public class CrisisData
    {
        public string CrisisId;
        public string Title;
        public string Description;
        public CrisisType Type;
        public CrisisSeverity Severity;
        public int TurnsActive;
        public int TurnsRemaining;
        public float PublicAwareness;
        public float EscalationProgress;
        public List<CrisisResponse> AvailableResponses;
        public List<string> ActiveEffects;
        public Sprite CrisisImage;
    }
    
    public enum CrisisType
    {
        NaturalDisaster,
        EconomicDownturn,
        SecurityThreat,
        SocialUnrest,
        HealthEmergency,
        PoliticalCrisis
    }
    
    public enum CrisisSeverity
    {
        Minor,
        Moderate,
        Major,
        Critical,
        Catastrophic
    }
    
    [Serializable]
    public class CrisisResponse
    {
        public string ResponseId;
        public string Name;
        public string Description;
        public int PoliticalCapitalCost;
        public int FundsCost;
        public float SuccessChance;
        public float TimeToEffect; // turns
        public List<string> PotentialOutcomes;
        public bool RequiresStaff;
        public string RequiredStaffRole;
    }
    
    /// <summary>
    /// Scandal data for response screen.
    /// </summary>
    [Serializable]
    public class ScandalData
    {
        public string ScandalId;
        public string Title;
        public string Description;
        public ScandalCategory Category;
        public int Severity; // 1-10
        public int DaysActive;
        public float MediaCoverage;
        public float PublicOutrage;
        public float EvidenceStrength;
        public List<ScandalResponse> AvailableResponses;
        public List<string> InvolvedParties;
        public bool IsEvolving;
        public string EvolutionWarning;
    }
    
    public enum ScandalCategory
    {
        Financial,
        Personal,
        Policy,
        Administrative,
        Electoral
    }
    
    [Serializable]
    public class ScandalResponse
    {
        public string ResponseId;
        public string Name;
        public string Description;
        public float TrustImpactMin;
        public float TrustImpactMax;
        public float MediaEffect;
        public float EvolutionEffect;
        public int ResourceCost;
        public List<string> Risks;
        public bool RequiresEvidence;
    }
    
    /// <summary>
    /// Election results data.
    /// </summary>
    [Serializable]
    public class ElectionResultsData
    {
        public string ElectionId;
        public string OfficeName;
        public int Year;
        public List<CandidateResult> Results;
        public Dictionary<string, float> RegionalResults;
        public Dictionary<string, float> DemographicBreakdown;
        public float TotalVotes;
        public float TurnoutPercentage;
        public bool IsPlayerVictory;
        public string MarginDescription;
    }
    
    [Serializable]
    public class CandidateResult
    {
        public string CandidateId;
        public string CandidateName;
        public string Party;
        public float VotePercentage;
        public float VoteCount;
        public float PreviousPercentage;
        public bool IsPlayer;
        public bool IsWinner;
        public Color PartyColor;
    }
    
    #endregion
    
    /// <summary>
    /// Event Popup screen for handling random and story events.
    /// </summary>
    public class EventPopup : BaseScreen
    {
        #region Serialized Fields
        
        [Header("Event Display")]
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _descriptionText;
        [SerializeField] private Text _categoryText;
        [SerializeField] private Text _sourceText;
        [SerializeField] private Image _eventImage;
        [SerializeField] private GameObject _urgentBanner;
        
        [Header("Timer")]
        [SerializeField] private GameObject _timerContainer;
        [SerializeField] private Text _timerText;
        [SerializeField] private Image _timerFill;
        
        [Header("Choices")]
        [SerializeField] private Transform _choicesContainer;
        [SerializeField] private GameObject _choicePrefab;
        
        [Header("Choice Details")]
        [SerializeField] private GameObject _choiceDetailPanel;
        [SerializeField] private Text _choiceDescriptionText;
        [SerializeField] private Text _successChanceText;
        [SerializeField] private Text _costText;
        [SerializeField] private Transform _consequencesContainer;
        [SerializeField] private Transform _requirementsContainer;
        
        [Header("Audio")]
        [SerializeField] private AudioClip _eventSound;
        [SerializeField] private AudioClip _urgentSound;
        [SerializeField] private AudioClip _choiceSound;
        
        #endregion
        
        #region Private Fields
        
        private GameEventData _currentEvent;
        private EventChoice _hoveredChoice;
        private float _timeRemaining;
        private bool _timerActive;
        
        #endregion
        
        #region Events
        
        public event Action<string, string> OnChoiceMade; // eventId, choiceId
        public event Action<string> OnEventTimeout; // eventId
        
        #endregion
        
        #region Screen Lifecycle
        
        public override void OnScreenEnter(object data)
        {
            base.OnScreenEnter(data);
            
            if (data is GameEventData eventData)
            {
                _currentEvent = eventData;
                DisplayEvent(eventData);
            }
        }
        
        public override void OnScreenExit()
        {
            base.OnScreenExit();
            _timerActive = false;
        }
        
        public override bool CanNavigateBack()
        {
            // Can't back out of events
            return false;
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Update()
        {
            if (_timerActive && _timeRemaining > 0)
            {
                _timeRemaining -= Time.deltaTime;
                UpdateTimerDisplay();
                
                if (_timeRemaining <= 0)
                {
                    HandleTimeout();
                }
            }
        }
        
        #endregion
        
        #region Display
        
        private void DisplayEvent(GameEventData eventData)
        {
            if (_titleText != null) _titleText.text = eventData.Title;
            if (_descriptionText != null) _descriptionText.text = eventData.Description;
            if (_categoryText != null) _categoryText.text = eventData.Category;
            if (_sourceText != null) _sourceText.text = eventData.SourceInfo ?? "News Report";
            if (_eventImage != null && eventData.EventImage != null) _eventImage.sprite = eventData.EventImage;
            if (_urgentBanner != null) _urgentBanner.SetActive(eventData.IsUrgent);
            
            // Timer
            if (eventData.TimeLimit > 0)
            {
                _timeRemaining = eventData.TimeLimit;
                _timerActive = true;
                if (_timerContainer != null) _timerContainer.SetActive(true);
            }
            else
            {
                _timerActive = false;
                if (_timerContainer != null) _timerContainer.SetActive(false);
            }
            
            // Create choice buttons
            CreateChoiceButtons(eventData.Choices);
            
            // Hide detail panel initially
            if (_choiceDetailPanel != null) _choiceDetailPanel.SetActive(false);
            
            // Play sound
            PlayEventSound(eventData.IsUrgent);
        }
        
        private void CreateChoiceButtons(List<EventChoice> choices)
        {
            if (_choicesContainer == null || _choicePrefab == null) return;
            
            foreach (Transform child in _choicesContainer)
            {
                Destroy(child.gameObject);
            }
            
            foreach (var choice in choices)
            {
                var go = Instantiate(_choicePrefab, _choicesContainer);
                var button = go.GetComponent<EventChoiceButton>();
                
                if (button != null)
                {
                    button.Setup(choice, 
                        () => SelectChoice(choice),
                        () => ShowChoiceDetails(choice),
                        () => HideChoiceDetails());
                }
            }
        }
        
        private void UpdateTimerDisplay()
        {
            if (_timerText != null)
            {
                int seconds = Mathf.CeilToInt(_timeRemaining);
                _timerText.text = $"{seconds}s";
                _timerText.color = _timeRemaining < 10 ? Color.red : Color.white;
            }
            
            if (_timerFill != null && _currentEvent != null)
            {
                _timerFill.fillAmount = _timeRemaining / _currentEvent.TimeLimit;
            }
        }
        
        private void ShowChoiceDetails(EventChoice choice)
        {
            _hoveredChoice = choice;
            
            if (_choiceDetailPanel == null) return;
            
            _choiceDetailPanel.SetActive(true);
            
            if (_choiceDescriptionText != null)
                _choiceDescriptionText.text = choice.Tooltip ?? choice.Text;
            
            if (_successChanceText != null && choice.SuccessChance < 1f)
            {
                _successChanceText.text = $"Success Chance: {choice.SuccessChance * 100:F0}%";
                _successChanceText.gameObject.SetActive(true);
            }
            else if (_successChanceText != null)
            {
                _successChanceText.gameObject.SetActive(false);
            }
            
            if (_costText != null && choice.ResourceCost > 0)
            {
                _costText.text = $"Cost: {choice.ResourceCost} {choice.ResourceType}";
                _costText.gameObject.SetActive(true);
            }
            else if (_costText != null)
            {
                _costText.gameObject.SetActive(false);
            }
            
            // Consequences
            if (_consequencesContainer != null)
            {
                foreach (Transform child in _consequencesContainer)
                    Destroy(child.gameObject);
                
                foreach (var consequence in choice.Consequences ?? new List<string>())
                {
                    CreateListItem(_consequencesContainer, consequence);
                }
            }
            
            // Requirements
            if (_requirementsContainer != null)
            {
                foreach (Transform child in _requirementsContainer)
                    Destroy(child.gameObject);
                
                foreach (var req in choice.Requirements ?? new List<string>())
                {
                    CreateListItem(_requirementsContainer, req, choice.IsAvailable ? Color.green : Color.red);
                }
            }
        }
        
        private void HideChoiceDetails()
        {
            _hoveredChoice = null;
            if (_choiceDetailPanel != null)
                _choiceDetailPanel.SetActive(false);
        }
        
        private void CreateListItem(Transform container, string text, Color? color = null)
        {
            var go = new GameObject("Item");
            go.transform.SetParent(container, false);
            var t = go.AddComponent<Text>();
            t.text = "• " + text;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = 12;
            t.color = color ?? Color.white;
        }
        
        #endregion
        
        #region Choice Handling
        
        private void SelectChoice(EventChoice choice)
        {
            if (!choice.IsAvailable)
            {
                UIManager.ShowAlert("Unavailable", "You don't meet the requirements for this choice.");
                return;
            }
            
            _timerActive = false;
            PlayChoiceSound();
            
            OnChoiceMade?.Invoke(_currentEvent.EventId, choice.ChoiceId);
            NavigateBack();
        }
        
        private void HandleTimeout()
        {
            _timerActive = false;
            
            // Select default/worst option or trigger timeout event
            OnEventTimeout?.Invoke(_currentEvent.EventId);
            
            UIManager.ShowAlert("Time's Up!", "You failed to respond in time. Default action taken.");
            NavigateBack();
        }
        
        #endregion
        
        #region Audio
        
        private void PlayEventSound(bool isUrgent)
        {
            var clip = isUrgent ? _urgentSound : _eventSound;
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
            }
        }
        
        private void PlayChoiceSound()
        {
            if (_choiceSound != null)
            {
                AudioSource.PlayClipAtPoint(_choiceSound, Camera.main.transform.position);
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// Crisis Management screen for handling ongoing crises.
    /// </summary>
    public class CrisisManagement : BaseScreen
    {
        #region Serialized Fields
        
        [Header("Crisis Info")]
        [SerializeField] private Text _crisisTitleText;
        [SerializeField] private Text _crisisDescriptionText;
        [SerializeField] private Text _crisisTypeText;
        [SerializeField] private Image _crisisImage;
        [SerializeField] private Image _severityIndicator;
        
        [Header("Status Meters")]
        [SerializeField] private Slider _publicAwarenessSlider;
        [SerializeField] private Text _publicAwarenessText;
        [SerializeField] private Slider _escalationSlider;
        [SerializeField] private Text _escalationText;
        [SerializeField] private Text _turnsActiveText;
        [SerializeField] private Text _turnsRemainingText;
        
        [Header("Active Effects")]
        [SerializeField] private Transform _effectsContainer;
        [SerializeField] private GameObject _effectPrefab;
        
        [Header("Responses")]
        [SerializeField] private Transform _responsesContainer;
        [SerializeField] private GameObject _responsePrefab;
        
        [Header("Response Details")]
        [SerializeField] private GameObject _responseDetailPanel;
        [SerializeField] private Text _responseNameText;
        [SerializeField] private Text _responseDescriptionText;
        [SerializeField] private Text _responseCostText;
        [SerializeField] private Text _responseSuccessText;
        [SerializeField] private Transform _outcomesContainer;
        [SerializeField] private Button _executeResponseButton;
        
        [Header("Navigation")]
        [SerializeField] private Button _backButton;
        
        [Header("Colors")]
        [SerializeField] private Color _minorColor = Color.yellow;
        [SerializeField] private Color _moderateColor = new Color(1f, 0.6f, 0f);
        [SerializeField] private Color _majorColor = new Color(1f, 0.3f, 0f);
        [SerializeField] private Color _criticalColor = Color.red;
        [SerializeField] private Color _catastrophicColor = new Color(0.5f, 0f, 0f);
        
        #endregion
        
        #region Private Fields
        
        private CrisisData _currentCrisis;
        private CrisisResponse _selectedResponse;
        
        #endregion
        
        #region Events
        
        public event Action<string, string> OnResponseExecuted; // crisisId, responseId
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            if (_backButton != null)
                _backButton.onClick.AddListener(NavigateBack);
            
            if (_executeResponseButton != null)
                _executeResponseButton.onClick.AddListener(ExecuteSelectedResponse);
        }
        
        #endregion
        
        #region Screen Lifecycle
        
        public override void OnScreenEnter(object data)
        {
            base.OnScreenEnter(data);
            
            if (data is CrisisData crisisData)
            {
                _currentCrisis = crisisData;
                DisplayCrisis(crisisData);
            }
            
            if (_responseDetailPanel != null)
                _responseDetailPanel.SetActive(false);
        }
        
        #endregion
        
        #region Display
        
        private void DisplayCrisis(CrisisData crisis)
        {
            if (_crisisTitleText != null) _crisisTitleText.text = crisis.Title;
            if (_crisisDescriptionText != null) _crisisDescriptionText.text = crisis.Description;
            if (_crisisTypeText != null) _crisisTypeText.text = crisis.Type.ToString();
            if (_crisisImage != null && crisis.CrisisImage != null) _crisisImage.sprite = crisis.CrisisImage;
            
            // Severity
            if (_severityIndicator != null)
            {
                _severityIndicator.color = GetSeverityColor(crisis.Severity);
            }
            
            // Status meters
            if (_publicAwarenessSlider != null)
            {
                _publicAwarenessSlider.value = crisis.PublicAwareness;
            }
            if (_publicAwarenessText != null)
            {
                _publicAwarenessText.text = $"{crisis.PublicAwareness * 100:F0}%";
            }
            
            if (_escalationSlider != null)
            {
                _escalationSlider.value = crisis.EscalationProgress;
            }
            if (_escalationText != null)
            {
                _escalationText.text = $"{crisis.EscalationProgress * 100:F0}%";
            }
            
            if (_turnsActiveText != null)
            {
                _turnsActiveText.text = $"Active: {crisis.TurnsActive} turns";
            }
            if (_turnsRemainingText != null)
            {
                _turnsRemainingText.text = crisis.TurnsRemaining > 0 
                    ? $"Estimated: {crisis.TurnsRemaining} turns remaining" 
                    : "Duration unknown";
            }
            
            // Active effects
            DisplayActiveEffects(crisis.ActiveEffects);
            
            // Responses
            DisplayResponses(crisis.AvailableResponses);
        }
        
        private void DisplayActiveEffects(List<string> effects)
        {
            if (_effectsContainer == null) return;
            
            foreach (Transform child in _effectsContainer)
            {
                Destroy(child.gameObject);
            }
            
            foreach (var effect in effects ?? new List<string>())
            {
                if (_effectPrefab != null)
                {
                    var go = Instantiate(_effectPrefab, _effectsContainer);
                    var text = go.GetComponentInChildren<Text>();
                    if (text != null) text.text = effect;
                }
            }
        }
        
        private void DisplayResponses(List<CrisisResponse> responses)
        {
            if (_responsesContainer == null || _responsePrefab == null) return;
            
            foreach (Transform child in _responsesContainer)
            {
                Destroy(child.gameObject);
            }
            
            foreach (var response in responses ?? new List<CrisisResponse>())
            {
                var go = Instantiate(_responsePrefab, _responsesContainer);
                var button = go.GetComponent<CrisisResponseButton>();
                
                if (button != null)
                {
                    button.Setup(response, () => SelectResponse(response));
                }
            }
        }
        
        private Color GetSeverityColor(CrisisSeverity severity)
        {
            return severity switch
            {
                CrisisSeverity.Minor => _minorColor,
                CrisisSeverity.Moderate => _moderateColor,
                CrisisSeverity.Major => _majorColor,
                CrisisSeverity.Critical => _criticalColor,
                CrisisSeverity.Catastrophic => _catastrophicColor,
                _ => Color.white
            };
        }
        
        #endregion
        
        #region Response Handling
        
        private void SelectResponse(CrisisResponse response)
        {
            _selectedResponse = response;
            ShowResponseDetails(response);
        }
        
        private void ShowResponseDetails(CrisisResponse response)
        {
            if (_responseDetailPanel == null) return;
            
            _responseDetailPanel.SetActive(true);
            
            if (_responseNameText != null) _responseNameText.text = response.Name;
            if (_responseDescriptionText != null) _responseDescriptionText.text = response.Description;
            
            if (_responseCostText != null)
            {
                string cost = "";
                if (response.PoliticalCapitalCost > 0)
                    cost += $"{response.PoliticalCapitalCost} Political Capital";
                if (response.FundsCost > 0)
                {
                    if (cost.Length > 0) cost += " + ";
                    cost += $"${response.FundsCost:N0}";
                }
                _responseCostText.text = cost.Length > 0 ? $"Cost: {cost}" : "No cost";
            }
            
            if (_responseSuccessText != null)
            {
                _responseSuccessText.text = $"Success Chance: {response.SuccessChance * 100:F0}%";
            }
            
            // Outcomes
            if (_outcomesContainer != null)
            {
                foreach (Transform child in _outcomesContainer)
                    Destroy(child.gameObject);
                
                foreach (var outcome in response.PotentialOutcomes ?? new List<string>())
                {
                    var go = new GameObject("Outcome");
                    go.transform.SetParent(_outcomesContainer, false);
                    var text = go.AddComponent<Text>();
                    text.text = "• " + outcome;
                    text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    text.fontSize = 12;
                }
            }
        }
        
        private void ExecuteSelectedResponse()
        {
            if (_selectedResponse == null) return;
            
            UIManager.ShowConfirmation(
                "Execute Response",
                $"Execute '{_selectedResponse.Name}'? This action cannot be undone.",
                () =>
                {
                    OnResponseExecuted?.Invoke(_currentCrisis.CrisisId, _selectedResponse.ResponseId);
                    NavigateBack();
                }
            );
        }
        
        #endregion
    }
    
    /// <summary>
    /// Scandal Response screen for managing scandals.
    /// </summary>
    public class ScandalResponse : BaseScreen
    {
        #region Serialized Fields
        
        [Header("Scandal Info")]
        [SerializeField] private Text _scandalTitleText;
        [SerializeField] private Text _scandalDescriptionText;
        [SerializeField] private Text _scandalCategoryText;
        [SerializeField] private Image _severityFill;
        [SerializeField] private Text _severityText;
        
        [Header("Metrics")]
        [SerializeField] private Slider _mediaCoverageSlider;
        [SerializeField] private Text _mediaCoverageText;
        [SerializeField] private Slider _publicOutrageSlider;
        [SerializeField] private Text _publicOutrageText;
        [SerializeField] private Slider _evidenceSlider;
        [SerializeField] private Text _evidenceText;
        [SerializeField] private Text _daysActiveText;
        
        [Header("Evolution Warning")]
        [SerializeField] private GameObject _evolutionWarningPanel;
        [SerializeField] private Text _evolutionWarningText;
        
        [Header("Involved Parties")]
        [SerializeField] private Transform _involvedContainer;
        [SerializeField] private GameObject _involvedItemPrefab;
        
        [Header("Responses")]
        [SerializeField] private Transform _responsesContainer;
        [SerializeField] private GameObject _responsePrefab;
        
        [Header("Response Preview")]
        [SerializeField] private GameObject _previewPanel;
        [SerializeField] private Text _previewNameText;
        [SerializeField] private Text _previewDescriptionText;
        [SerializeField] private Text _trustImpactText;
        [SerializeField] private Text _mediaEffectText;
        [SerializeField] private Transform _risksContainer;
        [SerializeField] private Button _executeButton;
        
        [Header("Navigation")]
        [SerializeField] private Button _backButton;
        
        #endregion
        
        #region Private Fields
        
        private ScandalData _currentScandal;
        private ScandalResponse _selectedResponse;
        
        #endregion
        
        #region Events
        
        public event Action<string, string> OnResponseChosen; // scandalId, responseId
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            if (_backButton != null)
                _backButton.onClick.AddListener(NavigateBack);
            
            if (_executeButton != null)
                _executeButton.onClick.AddListener(ExecuteResponse);
        }
        
        #endregion
        
        #region Screen Lifecycle
        
        public override void OnScreenEnter(object data)
        {
            base.OnScreenEnter(data);
            
            if (data is ScandalData scandalData)
            {
                _currentScandal = scandalData;
                DisplayScandal(scandalData);
            }
            
            if (_previewPanel != null)
                _previewPanel.SetActive(false);
        }
        
        #endregion
        
        #region Display
        
        private void DisplayScandal(ScandalData scandal)
        {
            if (_scandalTitleText != null) _scandalTitleText.text = scandal.Title;
            if (_scandalDescriptionText != null) _scandalDescriptionText.text = scandal.Description;
            if (_scandalCategoryText != null) _scandalCategoryText.text = scandal.Category.ToString();
            
            // Severity
            if (_severityFill != null)
            {
                _severityFill.fillAmount = scandal.Severity / 10f;
                _severityFill.color = Color.Lerp(Color.yellow, Color.red, scandal.Severity / 10f);
            }
            if (_severityText != null)
            {
                _severityText.text = $"Severity: {scandal.Severity}/10";
            }
            
            // Metrics
            if (_mediaCoverageSlider != null) _mediaCoverageSlider.value = scandal.MediaCoverage;
            if (_mediaCoverageText != null) _mediaCoverageText.text = $"{scandal.MediaCoverage * 100:F0}%";
            
            if (_publicOutrageSlider != null) _publicOutrageSlider.value = scandal.PublicOutrage;
            if (_publicOutrageText != null) _publicOutrageText.text = $"{scandal.PublicOutrage * 100:F0}%";
            
            if (_evidenceSlider != null) _evidenceSlider.value = scandal.EvidenceStrength;
            if (_evidenceText != null) _evidenceText.text = $"{scandal.EvidenceStrength * 100:F0}%";
            
            if (_daysActiveText != null) _daysActiveText.text = $"Active for {scandal.DaysActive} days";
            
            // Evolution warning
            if (_evolutionWarningPanel != null)
            {
                _evolutionWarningPanel.SetActive(scandal.IsEvolving);
                if (_evolutionWarningText != null)
                    _evolutionWarningText.text = scandal.EvolutionWarning ?? "This scandal may escalate!";
            }
            
            // Involved parties
            DisplayInvolvedParties(scandal.InvolvedParties);
            
            // Responses
            DisplayResponses(scandal.AvailableResponses);
        }
        
        private void DisplayInvolvedParties(List<string> parties)
        {
            if (_involvedContainer == null) return;
            
            foreach (Transform child in _involvedContainer)
            {
                Destroy(child.gameObject);
            }
            
            foreach (var party in parties ?? new List<string>())
            {
                if (_involvedItemPrefab != null)
                {
                    var go = Instantiate(_involvedItemPrefab, _involvedContainer);
                    var text = go.GetComponentInChildren<Text>();
                    if (text != null) text.text = party;
                }
            }
        }
        
        private void DisplayResponses(List<Screens.ScandalResponse> responses)
        {
            if (_responsesContainer == null || _responsePrefab == null) return;
            
            foreach (Transform child in _responsesContainer)
            {
                Destroy(child.gameObject);
            }
            
            foreach (var response in responses ?? new List<Screens.ScandalResponse>())
            {
                var go = Instantiate(_responsePrefab, _responsesContainer);
                var button = go.GetComponent<ScandalResponseButton>();
                
                if (button != null)
                {
                    button.Setup(response, () => SelectResponse(response));
                }
            }
        }
        
        private void SelectResponse(Screens.ScandalResponse response)
        {
            _selectedResponse = response;
            ShowResponsePreview(response);
        }
        
        private void ShowResponsePreview(Screens.ScandalResponse response)
        {
            if (_previewPanel == null) return;
            
            _previewPanel.SetActive(true);
            
            if (_previewNameText != null) _previewNameText.text = response.Name;
            if (_previewDescriptionText != null) _previewDescriptionText.text = response.Description;
            
            if (_trustImpactText != null)
            {
                string min = response.TrustImpactMin >= 0 ? $"+{response.TrustImpactMin:F0}" : $"{response.TrustImpactMin:F0}";
                string max = response.TrustImpactMax >= 0 ? $"+{response.TrustImpactMax:F0}" : $"{response.TrustImpactMax:F0}";
                _trustImpactText.text = $"Trust Impact: {min}% to {max}%";
                _trustImpactText.color = response.TrustImpactMax >= 0 ? Color.green : Color.red;
            }
            
            if (_mediaEffectText != null)
            {
                string effect = response.MediaEffect > 0 ? "Increases" : response.MediaEffect < 0 ? "Decreases" : "No change to";
                _mediaEffectText.text = $"{effect} media coverage";
            }
            
            // Risks
            if (_risksContainer != null)
            {
                foreach (Transform child in _risksContainer)
                    Destroy(child.gameObject);
                
                foreach (var risk in response.Risks ?? new List<string>())
                {
                    var go = new GameObject("Risk");
                    go.transform.SetParent(_risksContainer, false);
                    var text = go.AddComponent<Text>();
                    text.text = "⚠ " + risk;
                    text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    text.fontSize = 12;
                    text.color = Color.yellow;
                }
            }
        }
        
        private void ExecuteResponse()
        {
            if (_selectedResponse == null) return;
            
            OnResponseChosen?.Invoke(_currentScandal.ScandalId, _selectedResponse.ResponseId);
            NavigateBack();
        }
        
        #endregion
    }
    
    /// <summary>
    /// Election Night screen showing live results.
    /// </summary>
    public class ElectionNight : BaseScreen
    {
        #region Serialized Fields
        
        [Header("Header")]
        [SerializeField] private Text _electionTitleText;
        [SerializeField] private Text _yearText;
        [SerializeField] private Text _reportingText;
        
        [Header("Results Display")]
        [SerializeField] private Transform _candidatesContainer;
        [SerializeField] private GameObject _candidateResultPrefab;
        [SerializeField] private Text _totalVotesText;
        [SerializeField] private Text _turnoutText;
        
        [Header("Map/Regional")]
        [SerializeField] private Transform _regionalContainer;
        [SerializeField] private GameObject _regionalResultPrefab;
        
        [Header("Demographics")]
        [SerializeField] private Transform _demographicsContainer;
        [SerializeField] private GameObject _demographicBarPrefab;
        
        [Header("Call Banner")]
        [SerializeField] private GameObject _callBanner;
        [SerializeField] private Text _callText;
        [SerializeField] private Image _callBackground;
        
        [Header("Animation")]
        [SerializeField] private float _countingDuration = 5f;
        [SerializeField] private AnimationCurve _countingCurve;
        
        [Header("Audio")]
        [SerializeField] private AudioClip _countingSound;
        [SerializeField] private AudioClip _victorySound;
        [SerializeField] private AudioClip _defeatSound;
        
        [Header("Navigation")]
        [SerializeField] private Button _continueButton;
        
        #endregion
        
        #region Private Fields
        
        private ElectionResultsData _results;
        private bool _resultsRevealed;
        private float _revealProgress;
        private Coroutine _revealCoroutine;
        
        #endregion
        
        #region Events
        
        public event Action<bool> OnElectionComplete; // isVictory
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            if (_continueButton != null)
            {
                _continueButton.onClick.AddListener(Continue);
                _continueButton.gameObject.SetActive(false);
            }
            
            if (_callBanner != null)
                _callBanner.SetActive(false);
        }
        
        #endregion
        
        #region Screen Lifecycle
        
        public override void OnScreenEnter(object data)
        {
            base.OnScreenEnter(data);
            
            if (data is ElectionResultsData results)
            {
                _results = results;
                SetupDisplay();
                StartCoroutine(RevealResults());
            }
        }
        
        public override bool CanNavigateBack()
        {
            return _resultsRevealed;
        }
        
        #endregion
        
        #region Display Setup
        
        private void SetupDisplay()
        {
            if (_electionTitleText != null)
                _electionTitleText.text = _results.OfficeName;
            
            if (_yearText != null)
                _yearText.text = _results.Year.ToString();
            
            if (_reportingText != null)
                _reportingText.text = "0% Reporting";
            
            // Create candidate rows
            CreateCandidateRows();
            
            // Create regional display
            CreateRegionalDisplay();
            
            // Create demographic display
            CreateDemographicDisplay();
        }
        
        private void CreateCandidateRows()
        {
            if (_candidatesContainer == null || _candidateResultPrefab == null) return;
            
            foreach (Transform child in _candidatesContainer)
            {
                Destroy(child.gameObject);
            }
            
            foreach (var candidate in _results.Results.OrderByDescending(r => r.VotePercentage))
            {
                var go = Instantiate(_candidateResultPrefab, _candidatesContainer);
                var row = go.GetComponent<CandidateResultRow>();
                
                if (row != null)
                {
                    row.Setup(candidate);
                }
            }
        }
        
        private void CreateRegionalDisplay()
        {
            if (_regionalContainer == null || _regionalResultPrefab == null) return;
            if (_results.RegionalResults == null) return;
            
            foreach (Transform child in _regionalContainer)
            {
                Destroy(child.gameObject);
            }
            
            foreach (var region in _results.RegionalResults)
            {
                var go = Instantiate(_regionalResultPrefab, _regionalContainer);
                var item = go.GetComponent<RegionalResultItem>();
                
                if (item != null)
                {
                    item.Setup(region.Key, region.Value);
                }
            }
        }
        
        private void CreateDemographicDisplay()
        {
            if (_demographicsContainer == null || _demographicBarPrefab == null) return;
            if (_results.DemographicBreakdown == null) return;
            
            foreach (Transform child in _demographicsContainer)
            {
                Destroy(child.gameObject);
            }
            
            foreach (var demo in _results.DemographicBreakdown)
            {
                var go = Instantiate(_demographicBarPrefab, _demographicsContainer);
                var bar = go.GetComponent<DemographicBar>();
                
                if (bar != null)
                {
                    bar.Setup(demo.Key, demo.Value);
                }
            }
        }
        
        #endregion
        
        #region Results Reveal
        
        private IEnumerator RevealResults()
        {
            float elapsed = 0;
            
            // Play counting sound
            if (_countingSound != null)
            {
                AudioSource.PlayClipAtPoint(_countingSound, Camera.main.transform.position);
            }
            
            while (elapsed < _countingDuration)
            {
                elapsed += Time.deltaTime;
                _revealProgress = _countingCurve != null 
                    ? _countingCurve.Evaluate(elapsed / _countingDuration)
                    : elapsed / _countingDuration;
                
                UpdateReveal(_revealProgress);
                yield return null;
            }
            
            // Final reveal
            UpdateReveal(1f);
            
            // Show call banner
            yield return new WaitForSeconds(0.5f);
            ShowElectionCall();
            
            _resultsRevealed = true;
            
            if (_continueButton != null)
                _continueButton.gameObject.SetActive(true);
        }
        
        private void UpdateReveal(float progress)
        {
            // Update reporting percentage
            if (_reportingText != null)
            {
                _reportingText.text = $"{progress * 100:F0}% Reporting";
            }
            
            // Update vote counts
            if (_totalVotesText != null)
            {
                float currentVotes = _results.TotalVotes * progress;
                _totalVotesText.text = $"{currentVotes:N0} votes counted";
            }
            
            // Update candidate bars
            foreach (Transform child in _candidatesContainer)
            {
                var row = child.GetComponent<CandidateResultRow>();
                if (row != null)
                {
                    row.UpdateProgress(progress);
                }
            }
        }
        
        private void ShowElectionCall()
        {
            if (_callBanner == null) return;
            
            _callBanner.SetActive(true);
            
            var winner = _results.Results.FirstOrDefault(r => r.IsWinner);
            
            if (_callText != null)
            {
                if (_results.IsPlayerVictory)
                {
                    _callText.text = $"VICTORY!\n{_results.MarginDescription}";
                }
                else
                {
                    _callText.text = $"DEFEAT\n{winner?.CandidateName ?? "Opponent"} wins {_results.MarginDescription}";
                }
            }
            
            if (_callBackground != null)
            {
                _callBackground.color = _results.IsPlayerVictory 
                    ? new Color(0.2f, 0.6f, 0.3f, 0.9f) 
                    : new Color(0.6f, 0.2f, 0.2f, 0.9f);
            }
            
            // Play sound
            var clip = _results.IsPlayerVictory ? _victorySound : _defeatSound;
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
            }
        }
        
        private void Continue()
        {
            OnElectionComplete?.Invoke(_results.IsPlayerVictory);
            
            if (_results.IsPlayerVictory)
            {
                NavigateTo(ScreenType.VictoryScreen, _results);
            }
            else
            {
                NavigateTo(ScreenType.DefeatScreen, _results);
            }
        }
        
        #endregion
    }
    
    #region UI Component Classes
    
    public class EventChoiceButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Text _choiceText;
        [SerializeField] private Image _availabilityIcon;
        [SerializeField] private Text _costText;
        [SerializeField] private CanvasGroup _canvasGroup;
        
        private EventChoice _choice;
        
        public void Setup(EventChoice choice, Action onClick, Action onHover, Action onExit)
        {
            _choice = choice;
            
            if (_choiceText != null) _choiceText.text = choice.Text;
            if (_costText != null && choice.ResourceCost > 0)
            {
                _costText.text = $"-{choice.ResourceCost}";
                _costText.gameObject.SetActive(true);
            }
            else if (_costText != null)
            {
                _costText.gameObject.SetActive(false);
            }
            
            if (_availabilityIcon != null)
                _availabilityIcon.color = choice.IsAvailable ? Color.green : Color.red;
            
            if (_canvasGroup != null)
                _canvasGroup.alpha = choice.IsAvailable ? 1f : 0.5f;
            
            if (_button != null)
            {
                _button.onClick.AddListener(() => onClick());
                
                var trigger = _button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                
                var enterEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
                enterEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
                enterEntry.callback.AddListener(_ => onHover());
                trigger.triggers.Add(enterEntry);
                
                var exitEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
                exitEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
                exitEntry.callback.AddListener(_ => onExit());
                trigger.triggers.Add(exitEntry);
            }
        }
    }
    
    public class CrisisResponseButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _costText;
        [SerializeField] private Slider _successSlider;
        
        public void Setup(CrisisResponse response, Action onClick)
        {
            if (_nameText != null) _nameText.text = response.Name;
            if (_costText != null) _costText.text = $"{response.PoliticalCapitalCost} PC";
            if (_successSlider != null) _successSlider.value = response.SuccessChance;
            if (_button != null) _button.onClick.AddListener(() => onClick());
        }
    }
    
    public class ScandalResponseButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _riskText;
        
        public void Setup(Screens.ScandalResponse response, Action onClick)
        {
            if (_nameText != null) _nameText.text = response.Name;
            if (_riskText != null) _riskText.text = $"{response.Risks?.Count ?? 0} risks";
            if (_button != null) _button.onClick.AddListener(() => onClick());
        }
    }
    
    public class CandidateResultRow : MonoBehaviour
    {
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _partyText;
        [SerializeField] private Text _percentageText;
        [SerializeField] private Text _voteCountText;
        [SerializeField] private Image _barFill;
        [SerializeField] private Image _playerIndicator;
        [SerializeField] private Image _winnerIcon;
        
        private CandidateResult _data;
        
        public void Setup(CandidateResult data)
        {
            _data = data;
            
            if (_nameText != null) _nameText.text = data.CandidateName;
            if (_partyText != null) _partyText.text = data.Party;
            if (_barFill != null) _barFill.color = data.PartyColor;
            if (_playerIndicator != null) _playerIndicator.gameObject.SetActive(data.IsPlayer);
            if (_winnerIcon != null) _winnerIcon.gameObject.SetActive(false);
        }
        
        public void UpdateProgress(float progress)
        {
            float currentPercent = _data.VotePercentage * progress;
            float currentVotes = _data.VoteCount * progress;
            
            if (_percentageText != null) _percentageText.text = $"{currentPercent:F1}%";
            if (_voteCountText != null) _voteCountText.text = $"{currentVotes:N0}";
            if (_barFill != null) _barFill.fillAmount = currentPercent / 100f;
            
            if (progress >= 1f && _winnerIcon != null)
            {
                _winnerIcon.gameObject.SetActive(_data.IsWinner);
            }
        }
    }
    
    public class RegionalResultItem : MonoBehaviour
    {
        [SerializeField] private Text _regionText;
        [SerializeField] private Text _percentageText;
        [SerializeField] private Image _colorIndicator;
        
        public void Setup(string region, float percentage)
        {
            if (_regionText != null) _regionText.text = region;
            if (_percentageText != null) _percentageText.text = $"{percentage:F1}%";
            if (_colorIndicator != null)
            {
                _colorIndicator.color = percentage >= 50 ? Color.green : Color.red;
            }
        }
    }
    
    public class DemographicBar : MonoBehaviour
    {
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _percentageText;
        [SerializeField] private Image _barFill;
        
        public void Setup(string demographic, float percentage)
        {
            if (_nameText != null) _nameText.text = demographic;
            if (_percentageText != null) _percentageText.text = $"{percentage:F0}%";
            if (_barFill != null)
            {
                _barFill.fillAmount = percentage / 100f;
                _barFill.color = percentage >= 50 ? Color.green : Color.red;
            }
        }
    }
    
    #endregion
}
