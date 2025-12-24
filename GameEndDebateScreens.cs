// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - Game End & Debate Screens
// Sprint 9: Victory, Defeat, Legacy, and Debate Arena screens
// ═══════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionEmpire.UI.Screens
{
    #region Data Classes
    
    /// <summary>
    /// Victory screen data.
    /// </summary>
    [Serializable]
    public class VictoryData
    {
        public string OfficeName;
        public string CharacterName;
        public int Year;
        public float FinalApproval;
        public float VotePercentage;
        public string MarginDescription;
        public List<AchievementUnlock> UnlockedAchievements;
        public List<LegacyPerk> EarnedPerks;
        public int LegacyPointsEarned;
        public int TotalLegacyPoints;
        public Dictionary<string, float> FinalStats;
        public string VictorySpeechQuote;
        public bool IsPresidentialVictory;
        public bool IsFirstVictory;
    }
    
    /// <summary>
    /// Defeat screen data.
    /// </summary>
    [Serializable]
    public class DefeatData
    {
        public string OfficeName;
        public string CharacterName;
        public int Year;
        public float FinalApproval;
        public float VotePercentage;
        public string DefeatReason;
        public string WinnerName;
        public float WinnerPercentage;
        public List<string> LessonsLearned;
        public List<AchievementUnlock> UnlockedAchievements;
        public int LegacyPointsEarned;
        public bool WasImpeached;
        public bool WasScandal;
        public Dictionary<string, float> FinalStats;
    }
    
    [Serializable]
    public class AchievementUnlock
    {
        public string AchievementId;
        public string Name;
        public string Description;
        public Sprite Icon;
        public int PointsAwarded;
        public bool IsRare;
    }
    
    [Serializable]
    public class LegacyPerk
    {
        public string PerkId;
        public string Name;
        public string Description;
        public string Effect;
        public Sprite Icon;
    }
    
    /// <summary>
    /// Debate data for debate arena.
    /// </summary>
    [Serializable]
    public class DebateData
    {
        public string DebateId;
        public string DebateName;
        public string Venue;
        public List<DebateParticipant> Participants;
        public List<DebateTopic> Topics;
        public int TotalRounds;
        public int CurrentRound;
        public float TimePerResponse;
        public bool IsLive;
        public int ViewerCount;
    }
    
    [Serializable]
    public class DebateParticipant
    {
        public string ParticipantId;
        public string Name;
        public string Party;
        public Sprite Portrait;
        public float CurrentScore;
        public float DebateSkill;
        public bool IsPlayer;
        public List<string> Strengths;
        public List<string> Weaknesses;
    }
    
    [Serializable]
    public class DebateTopic
    {
        public string TopicId;
        public string TopicName;
        public string Question;
        public string Category;
        public float PlayerAdvantage; // -1 to 1
        public bool IsCompleted;
        public float PlayerScore;
        public float OpponentScore;
    }
    
    [Serializable]
    public class DebateResponse
    {
        public string ResponseId;
        public string Text;
        public string Strategy; // "Attack", "Defend", "Pivot", "Appeal"
        public float BaseEffectiveness;
        public float RiskFactor;
        public List<string> TargetDemographics;
        public string CounterTo; // Counters a specific opponent strategy
    }
    
    #endregion
    
    /// <summary>
    /// Victory screen shown after winning an election.
    /// </summary>
    public class VictoryScreen : BaseScreen
    {
        #region Serialized Fields
        
        [Header("Header")]
        [SerializeField] private Text _victoryTitleText;
        [SerializeField] private Text _officeNameText;
        [SerializeField] private Text _characterNameText;
        [SerializeField] private Text _yearText;
        
        [Header("Results")]
        [SerializeField] private Text _votePercentageText;
        [SerializeField] private Text _marginText;
        [SerializeField] private Text _approvalText;
        
        [Header("Victory Speech")]
        [SerializeField] private GameObject _speechPanel;
        [SerializeField] private Text _speechQuoteText;
        
        [Header("Achievements")]
        [SerializeField] private Transform _achievementsContainer;
        [SerializeField] private GameObject _achievementPrefab;
        
        [Header("Legacy")]
        [SerializeField] private Text _legacyPointsText;
        [SerializeField] private Text _totalLegacyText;
        [SerializeField] private Transform _perksContainer;
        [SerializeField] private GameObject _perkPrefab;
        
        [Header("Stats Summary")]
        [SerializeField] private Transform _statsContainer;
        [SerializeField] private GameObject _statRowPrefab;
        
        [Header("Celebration Effects")]
        [SerializeField] private ParticleSystem _confettiEffect;
        [SerializeField] private GameObject _presidentialSeal;
        
        [Header("Audio")]
        [SerializeField] private AudioClip _victoryFanfare;
        [SerializeField] private AudioClip _achievementSound;
        
        [Header("Navigation")]
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _mainMenuButton;
        
        #endregion
        
        #region Private Fields
        
        private VictoryData _data;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            if (_continueButton != null)
                _continueButton.onClick.AddListener(ContinuePlaying);
            
            if (_newGameButton != null)
                _newGameButton.onClick.AddListener(StartNewGame);
            
            if (_mainMenuButton != null)
                _mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }
        
        #endregion
        
        #region Screen Lifecycle
        
        public override void OnScreenEnter(object data)
        {
            base.OnScreenEnter(data);
            
            if (data is VictoryData victoryData)
            {
                _data = victoryData;
                StartCoroutine(DisplayVictorySequence());
            }
        }
        
        public override bool CanNavigateBack()
        {
            return false; // No going back from victory
        }
        
        #endregion
        
        #region Display
        
        private IEnumerator DisplayVictorySequence()
        {
            // Play fanfare
            if (_victoryFanfare != null)
            {
                AudioSource.PlayClipAtPoint(_victoryFanfare, Camera.main.transform.position);
            }
            
            // Start confetti
            if (_confettiEffect != null)
            {
                _confettiEffect.Play();
            }
            
            // Presidential seal for presidential victory
            if (_presidentialSeal != null)
            {
                _presidentialSeal.SetActive(_data.IsPresidentialVictory);
            }
            
            // Header
            if (_victoryTitleText != null)
            {
                _victoryTitleText.text = _data.IsPresidentialVictory 
                    ? "PRESIDENT ELECT!" 
                    : "VICTORY!";
            }
            
            if (_officeNameText != null) _officeNameText.text = _data.OfficeName;
            if (_characterNameText != null) _characterNameText.text = _data.CharacterName;
            if (_yearText != null) _yearText.text = _data.Year.ToString();
            
            yield return new WaitForSeconds(1f);
            
            // Results with animation
            yield return AnimateResults();
            
            // Victory speech
            if (_speechPanel != null && !string.IsNullOrEmpty(_data.VictorySpeechQuote))
            {
                _speechPanel.SetActive(true);
                if (_speechQuoteText != null)
                {
                    yield return TypeText(_speechQuoteText, $"\"{_data.VictorySpeechQuote}\"");
                }
            }
            
            yield return new WaitForSeconds(0.5f);
            
            // Achievements
            yield return DisplayAchievements();
            
            // Legacy
            DisplayLegacy();
            
            // Stats
            DisplayStats();
        }
        
        private IEnumerator AnimateResults()
        {
            if (_votePercentageText != null)
            {
                yield return AnimateNumber(_votePercentageText, 0, _data.VotePercentage, "{0:F1}%", 1f);
            }
            
            if (_marginText != null)
            {
                _marginText.text = _data.MarginDescription;
            }
            
            if (_approvalText != null)
            {
                yield return AnimateNumber(_approvalText, 0, _data.FinalApproval, "Final Approval: {0:F0}%", 0.5f);
            }
        }
        
        private IEnumerator AnimateNumber(Text textComponent, float from, float to, string format, float duration)
        {
            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float current = Mathf.Lerp(from, to, elapsed / duration);
                textComponent.text = string.Format(format, current);
                yield return null;
            }
            textComponent.text = string.Format(format, to);
        }
        
        private IEnumerator TypeText(Text textComponent, string fullText)
        {
            textComponent.text = "";
            foreach (char c in fullText)
            {
                textComponent.text += c;
                yield return new WaitForSeconds(0.02f);
            }
        }
        
        private IEnumerator DisplayAchievements()
        {
            if (_achievementsContainer == null || _achievementPrefab == null) yield break;
            if (_data.UnlockedAchievements == null || _data.UnlockedAchievements.Count == 0) yield break;
            
            foreach (var achievement in _data.UnlockedAchievements)
            {
                var go = Instantiate(_achievementPrefab, _achievementsContainer);
                var display = go.GetComponent<AchievementDisplay>();
                
                if (display != null)
                {
                    display.Setup(achievement);
                }
                
                if (_achievementSound != null)
                {
                    AudioSource.PlayClipAtPoint(_achievementSound, Camera.main.transform.position);
                }
                
                yield return new WaitForSeconds(0.3f);
            }
        }
        
        private void DisplayLegacy()
        {
            if (_legacyPointsText != null)
            {
                _legacyPointsText.text = $"+{_data.LegacyPointsEarned} Legacy Points";
            }
            
            if (_totalLegacyText != null)
            {
                _totalLegacyText.text = $"Total: {_data.TotalLegacyPoints}";
            }
            
            if (_perksContainer != null && _perkPrefab != null && _data.EarnedPerks != null)
            {
                foreach (var perk in _data.EarnedPerks)
                {
                    var go = Instantiate(_perkPrefab, _perksContainer);
                    var display = go.GetComponent<PerkDisplay>();
                    
                    if (display != null)
                    {
                        display.Setup(perk);
                    }
                }
            }
        }
        
        private void DisplayStats()
        {
            if (_statsContainer == null || _statRowPrefab == null) return;
            if (_data.FinalStats == null) return;
            
            foreach (var stat in _data.FinalStats)
            {
                var go = Instantiate(_statRowPrefab, _statsContainer);
                var row = go.GetComponent<StatRow>();
                
                if (row != null)
                {
                    row.Setup(stat.Key, stat.Value);
                }
            }
        }
        
        #endregion
        
        #region Navigation
        
        private void ContinuePlaying()
        {
            // Continue to next term or higher office
            NavigateTo(ScreenType.GameHUD);
        }
        
        private void StartNewGame()
        {
            NavigateTo(ScreenType.NewGame);
        }
        
        private void ReturnToMainMenu()
        {
            NavigateTo(ScreenType.MainMenu);
        }
        
        #endregion
    }
    
    /// <summary>
    /// Defeat screen shown after losing an election or being removed from office.
    /// </summary>
    public class DefeatScreen : BaseScreen
    {
        #region Serialized Fields
        
        [Header("Header")]
        [SerializeField] private Text _defeatTitleText;
        [SerializeField] private Text _officeNameText;
        [SerializeField] private Text _characterNameText;
        [SerializeField] private Text _yearText;
        
        [Header("Results")]
        [SerializeField] private Text _playerPercentageText;
        [SerializeField] private Text _winnerInfoText;
        [SerializeField] private Text _defeatReasonText;
        
        [Header("Lessons Learned")]
        [SerializeField] private Transform _lessonsContainer;
        [SerializeField] private GameObject _lessonPrefab;
        
        [Header("Achievements")]
        [SerializeField] private Transform _achievementsContainer;
        [SerializeField] private GameObject _achievementPrefab;
        [SerializeField] private Text _legacyPointsText;
        
        [Header("Stats Summary")]
        [SerializeField] private Transform _statsContainer;
        [SerializeField] private GameObject _statRowPrefab;
        
        [Header("Visuals")]
        [SerializeField] private Image _backgroundOverlay;
        [SerializeField] private Color _impeachedColor = new Color(0.3f, 0f, 0f, 0.8f);
        [SerializeField] private Color _scandalColor = new Color(0.4f, 0.2f, 0f, 0.8f);
        [SerializeField] private Color _electionLossColor = new Color(0.2f, 0.2f, 0.3f, 0.8f);
        
        [Header("Audio")]
        [SerializeField] private AudioClip _defeatSound;
        
        [Header("Navigation")]
        [SerializeField] private Button _tryAgainButton;
        [SerializeField] private Button _newCharacterButton;
        [SerializeField] private Button _mainMenuButton;
        
        #endregion
        
        #region Private Fields
        
        private DefeatData _data;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            if (_tryAgainButton != null)
                _tryAgainButton.onClick.AddListener(TryAgain);
            
            if (_newCharacterButton != null)
                _newCharacterButton.onClick.AddListener(NewCharacter);
            
            if (_mainMenuButton != null)
                _mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }
        
        #endregion
        
        #region Screen Lifecycle
        
        public override void OnScreenEnter(object data)
        {
            base.OnScreenEnter(data);
            
            if (data is DefeatData defeatData)
            {
                _data = defeatData;
                DisplayDefeat();
            }
        }
        
        public override bool CanNavigateBack()
        {
            return false;
        }
        
        #endregion
        
        #region Display
        
        private void DisplayDefeat()
        {
            // Play sound
            if (_defeatSound != null)
            {
                AudioSource.PlayClipAtPoint(_defeatSound, Camera.main.transform.position);
            }
            
            // Set background based on defeat type
            if (_backgroundOverlay != null)
            {
                if (_data.WasImpeached)
                    _backgroundOverlay.color = _impeachedColor;
                else if (_data.WasScandal)
                    _backgroundOverlay.color = _scandalColor;
                else
                    _backgroundOverlay.color = _electionLossColor;
            }
            
            // Header
            if (_defeatTitleText != null)
            {
                if (_data.WasImpeached)
                    _defeatTitleText.text = "IMPEACHED";
                else if (_data.WasScandal)
                    _defeatTitleText.text = "FORCED TO RESIGN";
                else
                    _defeatTitleText.text = "DEFEATED";
            }
            
            if (_officeNameText != null) _officeNameText.text = _data.OfficeName;
            if (_characterNameText != null) _characterNameText.text = _data.CharacterName;
            if (_yearText != null) _yearText.text = _data.Year.ToString();
            
            // Results
            if (_playerPercentageText != null)
            {
                _playerPercentageText.text = $"Your Vote: {_data.VotePercentage:F1}%";
            }
            
            if (_winnerInfoText != null && !string.IsNullOrEmpty(_data.WinnerName))
            {
                _winnerInfoText.text = $"{_data.WinnerName} wins with {_data.WinnerPercentage:F1}%";
            }
            
            if (_defeatReasonText != null)
            {
                _defeatReasonText.text = _data.DefeatReason;
            }
            
            // Lessons
            DisplayLessons();
            
            // Achievements (even in defeat)
            DisplayAchievements();
            
            // Stats
            DisplayStats();
        }
        
        private void DisplayLessons()
        {
            if (_lessonsContainer == null || _lessonPrefab == null) return;
            if (_data.LessonsLearned == null) return;
            
            foreach (var lesson in _data.LessonsLearned)
            {
                var go = Instantiate(_lessonPrefab, _lessonsContainer);
                var text = go.GetComponentInChildren<Text>();
                if (text != null) text.text = "• " + lesson;
            }
        }
        
        private void DisplayAchievements()
        {
            if (_achievementsContainer == null || _achievementPrefab == null) return;
            if (_data.UnlockedAchievements == null || _data.UnlockedAchievements.Count == 0) return;
            
            foreach (var achievement in _data.UnlockedAchievements)
            {
                var go = Instantiate(_achievementPrefab, _achievementsContainer);
                var display = go.GetComponent<AchievementDisplay>();
                
                if (display != null)
                {
                    display.Setup(achievement);
                }
            }
            
            if (_legacyPointsText != null)
            {
                _legacyPointsText.text = _data.LegacyPointsEarned > 0 
                    ? $"+{_data.LegacyPointsEarned} Legacy Points" 
                    : "No Legacy Points Earned";
            }
        }
        
        private void DisplayStats()
        {
            if (_statsContainer == null || _statRowPrefab == null) return;
            if (_data.FinalStats == null) return;
            
            foreach (var stat in _data.FinalStats)
            {
                var go = Instantiate(_statRowPrefab, _statsContainer);
                var row = go.GetComponent<StatRow>();
                
                if (row != null)
                {
                    row.Setup(stat.Key, stat.Value);
                }
            }
        }
        
        #endregion
        
        #region Navigation
        
        private void TryAgain()
        {
            // Restart with same character setup
            NavigateTo(ScreenType.NewGame);
        }
        
        private void NewCharacter()
        {
            NavigateTo(ScreenType.NewGame);
        }
        
        private void ReturnToMainMenu()
        {
            NavigateTo(ScreenType.MainMenu);
        }
        
        #endregion
    }
    
    /// <summary>
    /// Debate Arena screen for campaign debates.
    /// </summary>
    public class DebateArena : BaseScreen
    {
        #region Serialized Fields
        
        [Header("Debate Info")]
        [SerializeField] private Text _debateNameText;
        [SerializeField] private Text _venueText;
        [SerializeField] private Text _roundText;
        [SerializeField] private Text _viewerCountText;
        
        [Header("Participants")]
        [SerializeField] private Transform _participantsPanel;
        [SerializeField] private GameObject _participantPrefab;
        
        [Header("Current Topic")]
        [SerializeField] private Text _topicNameText;
        [SerializeField] private Text _questionText;
        [SerializeField] private Text _categoryText;
        [SerializeField] private Image _advantageIndicator;
        
        [Header("Timer")]
        [SerializeField] private Text _timerText;
        [SerializeField] private Image _timerFill;
        [SerializeField] private float _warningThreshold = 10f;
        
        [Header("Response Options")]
        [SerializeField] private Transform _responsesContainer;
        [SerializeField] private GameObject _responsePrefab;
        
        [Header("Score Display")]
        [SerializeField] private Text _playerScoreText;
        [SerializeField] private Text _opponentScoreText;
        [SerializeField] private Slider _momentumSlider;
        
        [Header("Feedback")]
        [SerializeField] private GameObject _feedbackPanel;
        [SerializeField] private Text _feedbackText;
        [SerializeField] private Text _crowdReactionText;
        [SerializeField] private Text _mediaReactionText;
        
        [Header("Opponent Speaking")]
        [SerializeField] private GameObject _opponentSpeakingPanel;
        [SerializeField] private Text _opponentStatementText;
        [SerializeField] private Button _interruptButton;
        
        [Header("Topics List")]
        [SerializeField] private Transform _topicsContainer;
        [SerializeField] private GameObject _topicItemPrefab;
        
        [Header("Audio")]
        [SerializeField] private AudioClip _tickSound;
        [SerializeField] private AudioClip _buzzerSound;
        [SerializeField] private AudioClip _applauseSound;
        [SerializeField] private AudioClip _booSound;
        
        [Header("Navigation")]
        [SerializeField] private Button _exitDebateButton;
        
        #endregion
        
        #region Private Fields
        
        private DebateData _debate;
        private DebateTopic _currentTopic;
        private float _timeRemaining;
        private bool _timerActive;
        private bool _playerTurn;
        private int _currentTopicIndex;
        
        #endregion
        
        #region Events
        
        public event Action<string, string> OnResponseSelected; // topicId, responseId
        public event Action OnDebateComplete;
        public event Action<string> OnInterrupt; // topicId
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            if (_interruptButton != null)
                _interruptButton.onClick.AddListener(AttemptInterrupt);
            
            if (_exitDebateButton != null)
                _exitDebateButton.onClick.AddListener(ExitDebate);
        }
        
        private void Update()
        {
            if (_timerActive && _timeRemaining > 0)
            {
                _timeRemaining -= Time.deltaTime;
                UpdateTimer();
                
                if (_timeRemaining <= 0)
                {
                    HandleTimeUp();
                }
            }
        }
        
        #endregion
        
        #region Screen Lifecycle
        
        public override void OnScreenEnter(object data)
        {
            base.OnScreenEnter(data);
            
            if (data is DebateData debateData)
            {
                _debate = debateData;
                SetupDebate();
                StartDebate();
            }
        }
        
        public override bool CanNavigateBack()
        {
            return false; // Can't leave mid-debate
        }
        
        #endregion
        
        #region Setup
        
        private void SetupDebate()
        {
            if (_debateNameText != null) _debateNameText.text = _debate.DebateName;
            if (_venueText != null) _venueText.text = _debate.Venue;
            if (_roundText != null) _roundText.text = $"Round 1 of {_debate.TotalRounds}";
            
            UpdateViewerCount();
            
            // Setup participants
            SetupParticipants();
            
            // Setup topics list
            SetupTopicsList();
            
            // Hide panels initially
            if (_feedbackPanel != null) _feedbackPanel.SetActive(false);
            if (_opponentSpeakingPanel != null) _opponentSpeakingPanel.SetActive(false);
        }
        
        private void SetupParticipants()
        {
            if (_participantsPanel == null || _participantPrefab == null) return;
            
            foreach (Transform child in _participantsPanel)
            {
                Destroy(child.gameObject);
            }
            
            foreach (var participant in _debate.Participants)
            {
                var go = Instantiate(_participantPrefab, _participantsPanel);
                var display = go.GetComponent<DebateParticipantDisplay>();
                
                if (display != null)
                {
                    display.Setup(participant);
                }
            }
        }
        
        private void SetupTopicsList()
        {
            if (_topicsContainer == null || _topicItemPrefab == null) return;
            
            foreach (Transform child in _topicsContainer)
            {
                Destroy(child.gameObject);
            }
            
            foreach (var topic in _debate.Topics)
            {
                var go = Instantiate(_topicItemPrefab, _topicsContainer);
                var item = go.GetComponent<DebateTopicItem>();
                
                if (item != null)
                {
                    item.Setup(topic);
                }
            }
        }
        
        #endregion
        
        #region Debate Flow
        
        private void StartDebate()
        {
            _currentTopicIndex = 0;
            StartNextTopic();
        }
        
        private void StartNextTopic()
        {
            if (_currentTopicIndex >= _debate.Topics.Count)
            {
                EndDebate();
                return;
            }
            
            _currentTopic = _debate.Topics[_currentTopicIndex];
            DisplayTopic(_currentTopic);
            
            // Alternate who goes first
            _playerTurn = _currentTopicIndex % 2 == 0;
            
            if (_playerTurn)
            {
                StartPlayerTurn();
            }
            else
            {
                StartOpponentTurn();
            }
        }
        
        private void DisplayTopic(DebateTopic topic)
        {
            if (_topicNameText != null) _topicNameText.text = topic.TopicName;
            if (_questionText != null) _questionText.text = topic.Question;
            if (_categoryText != null) _categoryText.text = topic.Category;
            
            if (_advantageIndicator != null)
            {
                if (topic.PlayerAdvantage > 0.2f)
                    _advantageIndicator.color = Color.green;
                else if (topic.PlayerAdvantage < -0.2f)
                    _advantageIndicator.color = Color.red;
                else
                    _advantageIndicator.color = Color.yellow;
            }
        }
        
        private void StartPlayerTurn()
        {
            _playerTurn = true;
            if (_opponentSpeakingPanel != null) _opponentSpeakingPanel.SetActive(false);
            
            // Show response options
            DisplayResponseOptions();
            
            // Start timer
            _timeRemaining = _debate.TimePerResponse;
            _timerActive = true;
        }
        
        private void StartOpponentTurn()
        {
            _playerTurn = false;
            if (_responsesContainer != null)
            {
                foreach (Transform child in _responsesContainer)
                    Destroy(child.gameObject);
            }
            
            if (_opponentSpeakingPanel != null)
            {
                _opponentSpeakingPanel.SetActive(true);
            }
            
            // Simulate opponent speaking
            StartCoroutine(SimulateOpponentResponse());
        }
        
        private void DisplayResponseOptions()
        {
            if (_responsesContainer == null || _responsePrefab == null) return;
            
            foreach (Transform child in _responsesContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Generate response options based on topic
            var responses = GenerateResponses(_currentTopic);
            
            foreach (var response in responses)
            {
                var go = Instantiate(_responsePrefab, _responsesContainer);
                var button = go.GetComponent<DebateResponseButton>();
                
                if (button != null)
                {
                    button.Setup(response, () => SelectResponse(response));
                }
            }
        }
        
        private List<DebateResponse> GenerateResponses(DebateTopic topic)
        {
            // In actual implementation, these would come from game data
            return new List<DebateResponse>
            {
                new DebateResponse
                {
                    ResponseId = "attack",
                    Text = "Attack opponent's position",
                    Strategy = "Attack",
                    BaseEffectiveness = 0.7f,
                    RiskFactor = 0.3f
                },
                new DebateResponse
                {
                    ResponseId = "defend",
                    Text = "Defend your record",
                    Strategy = "Defend",
                    BaseEffectiveness = 0.6f,
                    RiskFactor = 0.1f
                },
                new DebateResponse
                {
                    ResponseId = "pivot",
                    Text = "Pivot to your strengths",
                    Strategy = "Pivot",
                    BaseEffectiveness = 0.5f,
                    RiskFactor = 0.2f
                },
                new DebateResponse
                {
                    ResponseId = "appeal",
                    Text = "Appeal to voters directly",
                    Strategy = "Appeal",
                    BaseEffectiveness = 0.65f,
                    RiskFactor = 0.15f
                }
            };
        }
        
        private void SelectResponse(DebateResponse response)
        {
            _timerActive = false;
            
            // Calculate effectiveness
            float effectiveness = CalculateResponseEffectiveness(response);
            
            // Update scores
            _currentTopic.PlayerScore += effectiveness;
            var player = _debate.Participants.FirstOrDefault(p => p.IsPlayer);
            if (player != null)
            {
                player.CurrentScore += effectiveness;
            }
            
            // Show feedback
            ShowFeedback(response, effectiveness);
            
            OnResponseSelected?.Invoke(_currentTopic.TopicId, response.ResponseId);
            
            // Continue debate
            StartCoroutine(ContinueAfterResponse());
        }
        
        private float CalculateResponseEffectiveness(DebateResponse response)
        {
            float effectiveness = response.BaseEffectiveness;
            
            // Apply player advantage
            effectiveness *= (1 + _currentTopic.PlayerAdvantage * 0.3f);
            
            // Risk factor
            if (UnityEngine.Random.value < response.RiskFactor)
            {
                effectiveness *= 0.5f; // Backfired
            }
            
            // Clamp
            return Mathf.Clamp01(effectiveness);
        }
        
        private void ShowFeedback(DebateResponse response, float effectiveness)
        {
            if (_feedbackPanel == null) return;
            
            _feedbackPanel.SetActive(true);
            
            if (_feedbackText != null)
            {
                if (effectiveness >= 0.7f)
                    _feedbackText.text = "Excellent response!";
                else if (effectiveness >= 0.5f)
                    _feedbackText.text = "Good response.";
                else if (effectiveness >= 0.3f)
                    _feedbackText.text = "Mediocre response.";
                else
                    _feedbackText.text = "Poor response!";
            }
            
            if (_crowdReactionText != null)
            {
                _crowdReactionText.text = effectiveness >= 0.5f ? "👏 Crowd applauds" : "😐 Tepid reaction";
            }
            
            // Play sound
            var clip = effectiveness >= 0.5f ? _applauseSound : _booSound;
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, 0.5f);
            }
        }
        
        private IEnumerator SimulateOpponentResponse()
        {
            // Show opponent thinking
            if (_opponentStatementText != null)
            {
                _opponentStatementText.text = "Opponent is speaking...";
            }
            
            yield return new WaitForSeconds(2f);
            
            // Generate opponent score
            var opponent = _debate.Participants.FirstOrDefault(p => !p.IsPlayer);
            float opponentScore = UnityEngine.Random.Range(0.3f, 0.8f);
            _currentTopic.OpponentScore = opponentScore;
            
            if (opponent != null)
            {
                opponent.CurrentScore += opponentScore;
            }
            
            if (_opponentStatementText != null)
            {
                _opponentStatementText.text = opponentScore >= 0.6f 
                    ? "A strong rebuttal from your opponent." 
                    : "Your opponent struggled with this topic.";
            }
            
            yield return new WaitForSeconds(1.5f);
            
            // Now player's turn
            StartPlayerTurn();
        }
        
        private IEnumerator ContinueAfterResponse()
        {
            yield return new WaitForSeconds(2f);
            
            if (_feedbackPanel != null)
                _feedbackPanel.SetActive(false);
            
            // Move to opponent turn or next topic
            if (_playerTurn)
            {
                StartOpponentTurn();
            }
            else
            {
                _currentTopicIndex++;
                _currentTopic.IsCompleted = true;
                UpdateTopicsList();
                
                yield return new WaitForSeconds(1f);
                StartNextTopic();
            }
        }
        
        private void UpdateTopicsList()
        {
            int index = 0;
            foreach (Transform child in _topicsContainer)
            {
                var item = child.GetComponent<DebateTopicItem>();
                if (item != null && index < _debate.Topics.Count)
                {
                    item.UpdateStatus(_debate.Topics[index]);
                }
                index++;
            }
        }
        
        #endregion
        
        #region Timer
        
        private void UpdateTimer()
        {
            if (_timerText != null)
            {
                int seconds = Mathf.CeilToInt(_timeRemaining);
                _timerText.text = $"{seconds}s";
                _timerText.color = _timeRemaining <= _warningThreshold ? Color.red : Color.white;
            }
            
            if (_timerFill != null)
            {
                _timerFill.fillAmount = _timeRemaining / _debate.TimePerResponse;
            }
            
            // Tick sound for last 5 seconds
            if (_timeRemaining <= 5 && _timeRemaining > 0 && Mathf.CeilToInt(_timeRemaining) != Mathf.CeilToInt(_timeRemaining + Time.deltaTime))
            {
                if (_tickSound != null)
                {
                    AudioSource.PlayClipAtPoint(_tickSound, Camera.main.transform.position);
                }
            }
        }
        
        private void HandleTimeUp()
        {
            _timerActive = false;
            
            if (_buzzerSound != null)
            {
                AudioSource.PlayClipAtPoint(_buzzerSound, Camera.main.transform.position);
            }
            
            // Penalty for running out of time
            ShowFeedback(null, 0.2f);
            StartCoroutine(ContinueAfterResponse());
        }
        
        #endregion
        
        #region Actions
        
        private void AttemptInterrupt()
        {
            if (_playerTurn) return;
            
            // Interrupting has risk/reward
            float interruptChance = 0.4f;
            
            if (UnityEngine.Random.value < interruptChance)
            {
                // Successful interrupt
                UIManager.ShowAlert("Interrupt Successful", "You seized the moment!");
                StartPlayerTurn();
                OnInterrupt?.Invoke(_currentTopic.TopicId);
            }
            else
            {
                // Failed interrupt
                UIManager.ShowAlert("Interrupt Failed", "The moderator cuts you off. -5% favorability.");
                var player = _debate.Participants.FirstOrDefault(p => p.IsPlayer);
                if (player != null) player.CurrentScore -= 0.1f;
            }
        }
        
        private void UpdateViewerCount()
        {
            if (_viewerCountText != null && _debate.IsLive)
            {
                // Simulate fluctuating viewer count
                int viewers = _debate.ViewerCount + UnityEngine.Random.Range(-1000, 1000);
                _viewerCountText.text = $"👁 {viewers:N0} watching";
            }
        }
        
        private void EndDebate()
        {
            _timerActive = false;
            
            // Calculate final results
            var player = _debate.Participants.FirstOrDefault(p => p.IsPlayer);
            var opponent = _debate.Participants.FirstOrDefault(p => !p.IsPlayer);
            
            bool playerWon = player != null && opponent != null && player.CurrentScore > opponent.CurrentScore;
            
            OnDebateComplete?.Invoke();
            
            // Show results and return
            StartCoroutine(ShowDebateResults(playerWon));
        }
        
        private IEnumerator ShowDebateResults(bool playerWon)
        {
            UIManager.ShowModal(new ModalConfig
            {
                Title = playerWon ? "Debate Victory!" : "Debate Loss",
                Message = playerWon 
                    ? "You dominated the debate stage. Expect a polling boost!" 
                    : "Your opponent outperformed you. Your numbers may slip.",
                Priority = ModalPriority.High
            });
            
            yield return new WaitForSeconds(2f);
            
            NavigateTo(ScreenType.CampaignDashboard);
        }
        
        private void ExitDebate()
        {
            UIManager.ShowConfirmation(
                "Exit Debate",
                "Leaving early will severely damage your campaign. Are you sure?",
                () => NavigateTo(ScreenType.CampaignDashboard)
            );
        }
        
        #endregion
    }
    
    #region UI Component Classes
    
    public class AchievementDisplay : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _descriptionText;
        [SerializeField] private Text _pointsText;
        [SerializeField] private GameObject _rareBadge;
        
        public void Setup(AchievementUnlock achievement)
        {
            if (_icon != null && achievement.Icon != null) _icon.sprite = achievement.Icon;
            if (_nameText != null) _nameText.text = achievement.Name;
            if (_descriptionText != null) _descriptionText.text = achievement.Description;
            if (_pointsText != null) _pointsText.text = $"+{achievement.PointsAwarded}";
            if (_rareBadge != null) _rareBadge.SetActive(achievement.IsRare);
        }
    }
    
    public class PerkDisplay : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _effectText;
        
        public void Setup(LegacyPerk perk)
        {
            if (_icon != null && perk.Icon != null) _icon.sprite = perk.Icon;
            if (_nameText != null) _nameText.text = perk.Name;
            if (_effectText != null) _effectText.text = perk.Effect;
        }
    }
    
    public class StatRow : MonoBehaviour
    {
        [SerializeField] private Text _labelText;
        [SerializeField] private Text _valueText;
        [SerializeField] private Slider _valueSlider;
        
        public void Setup(string label, float value)
        {
            if (_labelText != null) _labelText.text = label;
            if (_valueText != null) _valueText.text = $"{value:F0}%";
            if (_valueSlider != null) _valueSlider.value = value / 100f;
        }
    }
    
    public class DebateParticipantDisplay : MonoBehaviour
    {
        [SerializeField] private Image _portrait;
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _partyText;
        [SerializeField] private Text _scoreText;
        [SerializeField] private Image _playerIndicator;
        
        private DebateParticipant _participant;
        
        public void Setup(DebateParticipant participant)
        {
            _participant = participant;
            
            if (_portrait != null && participant.Portrait != null) _portrait.sprite = participant.Portrait;
            if (_nameText != null) _nameText.text = participant.Name;
            if (_partyText != null) _partyText.text = participant.Party;
            if (_scoreText != null) _scoreText.text = "0";
            if (_playerIndicator != null) _playerIndicator.gameObject.SetActive(participant.IsPlayer);
        }
        
        public void UpdateScore(float score)
        {
            if (_scoreText != null) _scoreText.text = $"{score:F1}";
        }
    }
    
    public class DebateTopicItem : MonoBehaviour
    {
        [SerializeField] private Text _topicText;
        [SerializeField] private Image _statusIcon;
        [SerializeField] private Text _scoreText;
        
        [SerializeField] private Color _pendingColor = Color.gray;
        [SerializeField] private Color _activeColor = Color.yellow;
        [SerializeField] private Color _wonColor = Color.green;
        [SerializeField] private Color _lostColor = Color.red;
        
        public void Setup(DebateTopic topic)
        {
            if (_topicText != null) _topicText.text = topic.TopicName;
            if (_statusIcon != null) _statusIcon.color = _pendingColor;
            if (_scoreText != null) _scoreText.text = "";
        }
        
        public void UpdateStatus(DebateTopic topic)
        {
            if (!topic.IsCompleted)
            {
                if (_statusIcon != null) _statusIcon.color = _activeColor;
            }
            else
            {
                bool won = topic.PlayerScore > topic.OpponentScore;
                if (_statusIcon != null) _statusIcon.color = won ? _wonColor : _lostColor;
                if (_scoreText != null) _scoreText.text = $"{topic.PlayerScore:F1} vs {topic.OpponentScore:F1}";
            }
        }
    }
    
    public class DebateResponseButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Text _strategyText;
        [SerializeField] private Text _descriptionText;
        [SerializeField] private Slider _riskSlider;
        
        public void Setup(DebateResponse response, Action onClick)
        {
            if (_strategyText != null) _strategyText.text = response.Strategy;
            if (_descriptionText != null) _descriptionText.text = response.Text;
            if (_riskSlider != null) _riskSlider.value = response.RiskFactor;
            if (_button != null) _button.onClick.AddListener(() => onClick());
        }
    }
    
    #endregion
}
