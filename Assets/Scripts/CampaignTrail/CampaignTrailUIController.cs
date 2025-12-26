// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - CAMPAIGN TRAIL UI CONTROLLER
// Unity MonoBehaviour for displaying and interacting with campaign trail encounters
// ═══════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ElectionEmpire.CampaignTrail
{
    /// <summary>
    /// Unity UI controller for campaign trail encounters
    /// </summary>
    public class CampaignTrailUIController : MonoBehaviour
    {
        #region UI References
        
        [Header("Event Display")]
        [SerializeField] private GameObject eventPanel;
        [SerializeField] private TextMeshProUGUI locationNameText;
        [SerializeField] private TextMeshProUGUI locationDescriptionText;
        [SerializeField] private TextMeshProUGUI attendanceText;
        [SerializeField] private TextMeshProUGUI pressStatusText;
        [SerializeField] private Image hostilityIndicator;
        
        [Header("Encounter Display")]
        [SerializeField] private GameObject encounterPanel;
        [SerializeField] private Image citizenPortrait;
        [SerializeField] private TextMeshProUGUI citizenNameText;
        [SerializeField] private TextMeshProUGUI citizenDescriptionText;
        [SerializeField] private TextMeshProUGUI encounterContextText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private GameObject dialogueBubble;
        
        [Header("Choice Buttons")]
        [SerializeField] private GameObject choiceButtonContainer;
        [SerializeField] private GameObject choiceButtonPrefab;
        [SerializeField] private int maxChoiceButtons = 6;
        
        [Header("Outcome Display")]
        [SerializeField] private GameObject outcomePanel;
        [SerializeField] private TextMeshProUGUI outcomeNarrativeText;
        [SerializeField] private TextMeshProUGUI headlineText;
        [SerializeField] private GameObject headlinePanel;
        [SerializeField] private TextMeshProUGUI resourceChangeText;
        [SerializeField] private Image outcomeIcon;
        [SerializeField] private Sprite[] outcomeSprites; // Success, Failure, Critical, Viral
        
        [Header("Reporter Ambush")]
        [SerializeField] private GameObject ambushPanel;
        [SerializeField] private Image reporterPortrait;
        [SerializeField] private TextMeshProUGUI reporterNameText;
        [SerializeField] private TextMeshProUGUI reporterOutletText;
        [SerializeField] private TextMeshProUGUI ambushContextText;
        [SerializeField] private TextMeshProUGUI questionText;
        [SerializeField] private GameObject cameraIndicator;
        [SerializeField] private GameObject liveIndicator;
        
        [Header("Event Summary")]
        [SerializeField] private GameObject summaryPanel;
        [SerializeField] private TextMeshProUGUI summaryHeadlineText;
        [SerializeField] private TextMeshProUGUI encounterCountText;
        [SerializeField] private TextMeshProUGUI memorableMomentText;
        [SerializeField] private Transform resourceChangeSummaryContainer;
        
        [Header("Effects")]
        [SerializeField] private GameObject viralEffectPrefab;
        [SerializeField] private GameObject flashEffectPrefab;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip crowdCheerClip;
        [SerializeField] private AudioClip crowdBooClip;
        [SerializeField] private AudioClip cameraClickClip;
        [SerializeField] private AudioClip projectileHitClip;
        
        [Header("Colors")]
        [SerializeField] private Color friendlyColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color neutralColor = new Color(0.8f, 0.8f, 0.2f);
        [SerializeField] private Color hostileColor = new Color(0.8f, 0.2f, 0.2f);
        [SerializeField] private Color successColor = new Color(0.2f, 0.9f, 0.3f);
        [SerializeField] private Color failureColor = new Color(0.9f, 0.2f, 0.2f);
        
        #endregion
        
        #region State
        
        private TrailEventManager _eventManager;
        private TownsfolkEncounter _currentEncounter;
        private ReporterAmbush _currentAmbush;
        private List<GameObject> _activeChoiceButtons = new List<GameObject>();
        private bool _waitingForInput;
        private Coroutine _typewriterCoroutine;
        
        #endregion
        
        #region Events
        
        public event Action<EncounterChoice> OnChoiceSelected;
        public event Action OnContinuePressed;
        public event Action OnEventComplete;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Initialize button pool
            if (choiceButtonPrefab != null && choiceButtonContainer != null)
            {
                for (int i = 0; i < maxChoiceButtons; i++)
                {
                    var button = Instantiate(choiceButtonPrefab, choiceButtonContainer.transform);
                    button.SetActive(false);
                    _activeChoiceButtons.Add(button);
                }
            }
        }
        
        private void Start()
        {
            HideAllPanels();
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize with the trail event manager
        /// </summary>
        public void Initialize(TrailEventManager eventManager)
        {
            _eventManager = eventManager;
            
            // Subscribe to events
            _eventManager.OnEncounterStarted += HandleEncounterStarted;
            _eventManager.OnEncounterResolved += HandleEncounterResolved;
            _eventManager.OnTrailEventCompleted += HandleEventCompleted;
            _eventManager.OnReporterAmbush += HandleReporterAmbush;
            _eventManager.OnHeadlineGenerated += HandleHeadlineGenerated;
        }
        
        private void OnDestroy()
        {
            if (_eventManager != null)
            {
                _eventManager.OnEncounterStarted -= HandleEncounterStarted;
                _eventManager.OnEncounterResolved -= HandleEncounterResolved;
                _eventManager.OnTrailEventCompleted -= HandleEventCompleted;
                _eventManager.OnReporterAmbush -= HandleReporterAmbush;
                _eventManager.OnHeadlineGenerated -= HandleHeadlineGenerated;
            }
        }
        
        #endregion
        
        #region Event Display
        
        /// <summary>
        /// Display a new trail event
        /// </summary>
        public void DisplayEvent(TrailEvent trailEvent)
        {
            HideAllPanels();
            if (eventPanel != null) eventPanel.SetActive(true);
            
            if (locationNameText != null)
                locationNameText.text = trailEvent.LocationName;
            if (locationDescriptionText != null)
                locationDescriptionText.text = trailEvent.LocationDescription;
            if (attendanceText != null)
                attendanceText.text = $"Attendance: {trailEvent.ActualAttendance:N0}";
            
            // Press status
            if (pressStatusText != null)
            {
                if (trailEvent.PressPresent)
                {
                    pressStatusText.text = trailEvent.LiveCoverage 
                        ? $"<color=red>● LIVE</color> {trailEvent.ReportersPresent} reporters present"
                        : $"📷 {trailEvent.ReportersPresent} reporters present";
                }
                else
                {
                    pressStatusText.text = "No press coverage";
                }
            }
            
            // Hostility indicator
            if (hostilityIndicator != null)
            {
                hostilityIndicator.color = GetHostilityColor(trailEvent.HostilityLevel);
            }
            
            // Start encounter flow after brief delay
            StartCoroutine(StartEncountersAfterDelay(1.5f));
        }
        
        private IEnumerator StartEncountersAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            RequestNextEncounter();
        }
        
        private void RequestNextEncounter()
        {
            var encounter = _eventManager.GetNextEncounter();
            if (encounter == null && _currentAmbush == null)
            {
                // No more encounters - complete event
                var result = _eventManager.CompleteEvent();
                DisplayEventSummary(result);
            }
        }
        
        #endregion
        
        #region Encounter Display
        
        private void HandleEncounterStarted(EncounterStartedEvent e)
        {
            _currentEncounter = e.Encounter;
            DisplayEncounter(e.Encounter);
        }
        
        /// <summary>
        /// Display an encounter
        /// </summary>
        public void DisplayEncounter(TownsfolkEncounter encounter)
        {
            HideAllPanels();
            if (encounterPanel != null) encounterPanel.SetActive(true);
            
            // Citizen info
            if (citizenNameText != null)
                citizenNameText.text = encounter.Name;
            if (citizenDescriptionText != null)
                citizenDescriptionText.text = $"{encounter.Age} years old, {encounter.Occupation}";
            if (encounterContextText != null)
                encounterContextText.text = encounter.Context;
            
            // Portrait would be loaded from assets
            // if (citizenPortrait != null)
            //     citizenPortrait.sprite = LoadPortrait(encounter);
            
            // Opening action and dialogue with typewriter effect
            StartCoroutine(TypewriterSequence(encounter));
        }
        
        private IEnumerator TypewriterSequence(TownsfolkEncounter encounter)
        {
            // First show the action
            if (encounterContextText != null)
            {
                encounterContextText.text = "";
                yield return TypewriterEffect(encounterContextText, encounter.OpeningAction ?? "", 0.03f);
            }
            
            yield return new WaitForSeconds(0.5f);
            
            // Then show the dialogue bubble
            if (dialogueBubble != null) dialogueBubble.SetActive(true);
            if (dialogueText != null)
            {
                dialogueText.text = "";
                yield return TypewriterEffect(dialogueText, encounter.OpeningDialogue ?? "", 0.02f);
            }
            
            yield return new WaitForSeconds(0.3f);
            
            // Show choices
            DisplayChoices(encounter.Choices);
        }
        
        private IEnumerator TypewriterEffect(TextMeshProUGUI textComponent, string fullText, float charDelay)
        {
            if (textComponent == null || string.IsNullOrEmpty(fullText))
                yield break;
                
            textComponent.text = "";
            foreach (char c in fullText)
            {
                textComponent.text += c;
                yield return new WaitForSeconds(charDelay);
            }
        }
        
        /// <summary>
        /// Display choice buttons
        /// </summary>
        private void DisplayChoices(List<EncounterChoice> choices)
        {
            if (choices == null || choices.Count == 0) return;
            
            // Hide all buttons first
            foreach (var button in _activeChoiceButtons)
            {
                if (button != null) button.SetActive(false);
            }
            
            // Show and configure buttons for each choice
            for (int i = 0; i < choices.Count && i < _activeChoiceButtons.Count; i++)
            {
                var button = _activeChoiceButtons[i];
                if (button == null) continue;
                
                var choice = choices[i];
                
                button.SetActive(true);
                
                // Set button text
                var buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    string riskIndicator = GetRiskIndicator(choice.RiskLevel);
                    buttonText.text = $"{choice.Text} {riskIndicator}";
                }
                
                // Set button color based on risk
                var buttonImage = button.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = GetRiskColor(choice.RiskLevel);
                }
                
                // Configure click handler
                var btn = button.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    int index = i; // Capture for closure
                    btn.onClick.AddListener(() => OnChoiceButtonClicked(choices[index]));
                }
            }
            
            _waitingForInput = true;
        }
        
        private void OnChoiceButtonClicked(EncounterChoice choice)
        {
            if (!_waitingForInput) return;
            _waitingForInput = false;
            
            // Hide choices
            foreach (var button in _activeChoiceButtons)
            {
                if (button != null) button.SetActive(false);
            }
            
            // Fire event
            OnChoiceSelected?.Invoke(choice);
            
            // Resolve encounter
            if (_currentAmbush != null)
            {
                var result = _eventManager.ResolveReporterAmbush(choice);
                _currentAmbush = null;
            }
            else if (_currentEncounter != null)
            {
                var result = _eventManager.ResolveCurrentEncounter(choice);
            }
        }
        
        #endregion
        
        #region Outcome Display
        
        private void HandleEncounterResolved(EncounterResolvedEvent e)
        {
            DisplayOutcome(e.Result);
        }
        
        /// <summary>
        /// Display the outcome of an encounter
        /// </summary>
        public void DisplayOutcome(EncounterResolutionResult result)
        {
            HideAllPanels();
            if (outcomePanel != null) outcomePanel.SetActive(true);
            
            // Narrative
            if (outcomeNarrativeText != null)
            {
                outcomeNarrativeText.text = result.OutcomeDescription ?? GetOutcomeDescription(result.Outcome);
                outcomeNarrativeText.color = IsSuccessOutcome(result.Outcome) ? successColor : failureColor;
            }
            
            // Headline
            if (headlinePanel != null)
            {
                if (!string.IsNullOrEmpty(result.HeadlineGenerated))
                {
                    headlinePanel.SetActive(true);
                    if (headlineText != null)
                        headlineText.text = result.HeadlineGenerated;
                }
                else
                {
                    headlinePanel.SetActive(false);
                }
            }
            
            // Resource changes
            if (resourceChangeText != null)
            {
                var changes = new List<string>();
                if (Mathf.Abs(result.TrustChange) > 0.1f)
                {
                    string sign = result.TrustChange > 0 ? "+" : "";
                    string color = result.TrustChange > 0 ? "green" : "red";
                    changes.Add($"<color={color}>Trust: {sign}{result.TrustChange:F1}%</color>");
                }
                if (Mathf.Abs(result.MediaImpactChange) > 0.1f)
                {
                    string sign = result.MediaImpactChange > 0 ? "+" : "";
                    string color = result.MediaImpactChange > 0 ? "green" : "red";
                    changes.Add($"<color={color}>Media: {sign}{result.MediaImpactChange:F1}</color>");
                }
                if (Mathf.Abs(result.PartyLoyaltyChange) > 0.1f)
                {
                    string sign = result.PartyLoyaltyChange > 0 ? "+" : "";
                    string color = result.PartyLoyaltyChange > 0 ? "green" : "red";
                    changes.Add($"<color={color}>Party: {sign}{result.PartyLoyaltyChange:F1}%</color>");
                }
                resourceChangeText.text = string.Join("  |  ", changes);
            }
            
            // Outcome icon
            if (outcomeIcon != null && outcomeSprites != null && outcomeSprites.Length >= 4)
            {
                if (result.ViralMoment)
                    outcomeIcon.sprite = outcomeSprites[3];
                else if (result.Outcome == EncounterOutcome.Disaster || result.Outcome == EncounterOutcome.Triumph)
                    outcomeIcon.sprite = outcomeSprites[2];
                else if (IsSuccessOutcome(result.Outcome))
                    outcomeIcon.sprite = outcomeSprites[0];
                else
                    outcomeIcon.sprite = outcomeSprites[1];
            }
            
            // Visual effects
            if (result.ViralMoment)
            {
                PlayViralEffect();
            }
            
            // Audio
            if (IsSuccessOutcome(result.Outcome))
            {
                PlaySound(crowdCheerClip);
            }
            else if (result.Outcome == EncounterOutcome.Disaster)
            {
                PlaySound(crowdBooClip);
            }
            
            // Continue button - wait then proceed to next encounter
            StartCoroutine(ContinueAfterDelay(result.ViralMoment ? 4f : 2.5f));
        }
        
        private bool IsSuccessOutcome(EncounterOutcome outcome)
        {
            return outcome == EncounterOutcome.Triumph || outcome == EncounterOutcome.Positive;
        }
        
        private string GetOutcomeDescription(EncounterOutcome outcome)
        {
            switch (outcome)
            {
                case EncounterOutcome.Triumph:
                    return "A perfect response! The crowd loves it.";
                case EncounterOutcome.Positive:
                    return "A good response. The encounter went well.";
                case EncounterOutcome.Neutral:
                    return "A neutral response. No major impact.";
                case EncounterOutcome.Negative:
                    return "A poor response. Some damage was done.";
                case EncounterOutcome.Disaster:
                    return "A disastrous response! This will hurt your campaign.";
                case EncounterOutcome.ViralMoment:
                    return "This moment is going viral!";
                case EncounterOutcome.SecretRevealed:
                    return "A damaging secret has been exposed!";
                case EncounterOutcome.Escaped:
                    return "You managed to escape the situation.";
                default:
                    return "The encounter concluded.";
            }
        }
        
        private IEnumerator ContinueAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            // Check for more encounters
            if (_eventManager.HasMoreEncounters())
            {
                RequestNextEncounter();
            }
            else
            {
                var result = _eventManager.CompleteEvent();
                DisplayEventSummary(result);
            }
        }
        
        #endregion
        
        #region Reporter Ambush
        
        private void HandleReporterAmbush(ReporterAmbush ambush)
        {
            _currentAmbush = ambush;
            DisplayReporterAmbush(ambush);
        }
        
        /// <summary>
        /// Display a reporter ambush
        /// </summary>
        public void DisplayReporterAmbush(ReporterAmbush ambush)
        {
            HideAllPanels();
            if (ambushPanel != null) ambushPanel.SetActive(true);
            
            // Reporter info
            if (reporterNameText != null)
                reporterNameText.text = ambush.Reporter.Name;
            if (reporterOutletText != null)
                reporterOutletText.text = ambush.Reporter.Outlet;
            if (ambushContextText != null)
                ambushContextText.text = $"\"{ambush.Reporter.Personality}\"\n\n{ambush.Context}";
            
            // Camera indicators
            if (cameraIndicator != null)
                cameraIndicator.SetActive(ambush.CameraRolling);
            if (liveIndicator != null)
                liveIndicator.SetActive(ambush.LiveBroadcast);
            
            // Play camera sound
            if (ambush.CameraRolling)
            {
                PlaySound(cameraClickClip);
            }
            
            // Questions with typewriter
            StartCoroutine(DisplayAmbushQuestions(ambush));
        }
        
        private IEnumerator DisplayAmbushQuestions(ReporterAmbush ambush)
        {
            // Opening line
            if (questionText != null)
            {
                questionText.text = "";
                yield return TypewriterEffect(questionText, $"\"{ambush.OpeningLine}\"", 0.02f);
            }
            
            yield return new WaitForSeconds(0.5f);
            
            // First question
            if (ambush.Questions != null && ambush.Questions.Count > 0 && questionText != null)
            {
                questionText.text = "";
                yield return TypewriterEffect(questionText, ambush.Questions[0], 0.02f);
            }
            
            yield return new WaitForSeconds(0.3f);
            
            // Show response options
            if (ambush.ResponseOptions != null)
            {
                DisplayChoices(ambush.ResponseOptions);
            }
        }
        
        #endregion
        
        #region Event Summary
        
        private void HandleEventCompleted(TrailEventCompletedEvent e)
        {
            DisplayEventSummary(e.Result);
        }
        
        /// <summary>
        /// Display the summary of a completed event
        /// </summary>
        public void DisplayEventSummary(TrailEventResult result)
        {
            HideAllPanels();
            if (summaryPanel != null) summaryPanel.SetActive(true);
            
            // Headline of the day
            if (summaryHeadlineText != null)
            {
                summaryHeadlineText.text = result.HeadlineOfTheDay;
                
                // Overall result coloring
                switch (result.OverallOutcome)
                {
                    case EncounterOutcome.Positive:
                    case EncounterOutcome.Triumph:
                        summaryHeadlineText.color = successColor;
                        break;
                    case EncounterOutcome.Negative:
                    case EncounterOutcome.Disaster:
                        summaryHeadlineText.color = failureColor;
                        break;
                    default:
                        summaryHeadlineText.color = Color.white;
                        break;
                }
            }
            
            // Encounter counts
            if (encounterCountText != null)
            {
                encounterCountText.text = $"Encounters: {result.TotalEncounters}\n" +
                    $"<color=green>Positive: {result.PositiveEncounters}</color>  " +
                    $"<color=yellow>Neutral: {result.NeutralEncounters}</color>  " +
                    $"<color=red>Negative: {result.NegativeEncounters}</color>";
                
                if (result.DisasterEncounters > 0)
                {
                    encounterCountText.text += $"\n<color=red><b>DISASTERS: {result.DisasterEncounters}</b></color>";
                }
                
                if (result.ViralMoments > 0)
                {
                    encounterCountText.text += $"\n<color=cyan>Viral Moments: {result.ViralMoments}</color>";
                }
            }
            
            // Most memorable moment
            if (memorableMomentText != null)
            {
                if (!string.IsNullOrEmpty(result.MostMemorableMoment))
                {
                    memorableMomentText.text = $"<i>\"{result.MostMemorableMoment}\"</i>";
                }
                else
                {
                    memorableMomentText.text = "";
                }
            }
            
            // Fire completion event after delay
            StartCoroutine(FireEventCompleteAfterDelay(3f));
        }
        
        private IEnumerator FireEventCompleteAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            OnEventComplete?.Invoke();
        }
        
        #endregion
        
        #region Headlines
        
        private void HandleHeadlineGenerated(string headline)
        {
            // Could show a breaking news ticker effect
            Debug.Log($"HEADLINE: {headline}");
        }
        
        #endregion
        
        #region Helper Methods
        
        private void HideAllPanels()
        {
            if (eventPanel != null) eventPanel.SetActive(false);
            if (encounterPanel != null) encounterPanel.SetActive(false);
            if (outcomePanel != null) outcomePanel.SetActive(false);
            if (ambushPanel != null) ambushPanel.SetActive(false);
            if (summaryPanel != null) summaryPanel.SetActive(false);
            if (dialogueBubble != null) dialogueBubble.SetActive(false);
            if (headlinePanel != null) headlinePanel.SetActive(false);
        }
        
        private Color GetHostilityColor(CrowdHostility hostility)
        {
            switch (hostility)
            {
                case CrowdHostility.Friendly:
                    return friendlyColor;
                case CrowdHostility.Mixed:
                case CrowdHostility.Neutral:
                    return neutralColor;
                case CrowdHostility.Skeptical:
                    return Color.Lerp(neutralColor, hostileColor, 0.5f);
                case CrowdHostility.Hostile:
                case CrowdHostility.Dangerous:
                    return hostileColor;
                default:
                    return neutralColor;
            }
        }
        
        private string GetRiskIndicator(int riskLevel)
        {
            if (riskLevel >= 80) return "<color=red>⚠⚠⚠</color>";
            if (riskLevel >= 60) return "<color=orange>⚠⚠</color>";
            if (riskLevel >= 40) return "<color=yellow>⚠</color>";
            return "";
        }
        
        private Color GetRiskColor(int riskLevel)
        {
            if (riskLevel >= 80) return new Color(0.4f, 0.1f, 0.1f);
            if (riskLevel >= 60) return new Color(0.4f, 0.25f, 0.1f);
            if (riskLevel >= 40) return new Color(0.4f, 0.4f, 0.1f);
            return new Color(0.2f, 0.3f, 0.2f);
        }
        
        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
        
        private void PlayViralEffect()
        {
            if (viralEffectPrefab != null)
            {
                Instantiate(viralEffectPrefab, transform);
            }
        }
        
        #endregion
    }
}

