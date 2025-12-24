// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - Game HUD
// Sprint 9: Main in-game display with resources, turn info, news, and actions
// ═══════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ElectionEmpire.Core;
using ElectionEmpire.News;

namespace ElectionEmpire.UI
{
    #region HUD Enums
    
    /// <summary>
    /// Resource display types for HUD.
    /// </summary>
    public enum HUDResourceType
    {
        PublicTrust,
        PoliticalCapital,
        CampaignFunds,
        MediaInfluence,
        PartyLoyalty,
        Approval
    }
    
    /// <summary>
    /// Alert urgency levels.
    /// </summary>
    public enum AlertUrgency
    {
        Low,
        Medium,
        High,
        Critical
    }
    
    #endregion
    
    #region HUD Data Classes
    
    /// <summary>
    /// Resource bar display data.
    /// </summary>
    [Serializable]
    public class ResourceBarData
    {
        public HUDResourceType Type;
        public string Name;
        public float CurrentValue;
        public float MaxValue;
        public float PreviousValue;
        public Color BarColor;
        public Sprite Icon;
        public string Tooltip;
        public bool ShowAsPercentage;
        public bool ShowDelta;
    }
    
    /// <summary>
    /// News ticker item.
    /// </summary>
    [Serializable]
    public class TickerItem
    {
        public string Id;
        public string Headline;
        public TickerCategory Category;
        public AlertUrgency Urgency;
        public float Timestamp;
        public Action OnClick;
        public bool IsRead;
    }
    
    public enum TickerCategory
    {
        Politics,
        Economy,
        Social,
        Crisis,
        Personal,
        Opponent
    }
    
    /// <summary>
    /// Quick action button data.
    /// </summary>
    [Serializable]
    public class QuickActionData
    {
        public string Id;
        public string Name;
        public Sprite Icon;
        public Action OnClick;
        public bool IsAvailable;
        public string Tooltip;
        public int Hotkey; // 1-9
        public bool ShowNotification;
    }
    
    /// <summary>
    /// Advisor notification data.
    /// </summary>
    [Serializable]
    public class AdvisorMessage
    {
        public string AdvisorName;
        public Sprite AdvisorPortrait;
        public string Message;
        public AlertUrgency Urgency;
        public Action OnClick;
        public float DisplayDuration;
    }
    
    #endregion
    
    /// <summary>
    /// Main game HUD controller.
    /// Displays resources, turn info, news ticker, and quick actions.
    /// </summary>
    public class GameHUD : BaseScreen
    {
        #region Serialized Fields
        
        [Header("Resource Display")]
        [SerializeField] private Transform _resourceBarContainer;
        [SerializeField] private GameObject _resourceBarPrefab;
        [SerializeField] private ResourceBarUI[] _presetResourceBars;
        
        [Header("Turn Info")]
        [SerializeField] private Text _turnText;
        [SerializeField] private Text _yearText;
        [SerializeField] private Text _phaseText;
        [SerializeField] private Text _dateText;
        [SerializeField] private Button _endTurnButton;
        [SerializeField] private Text _endTurnButtonText;
        [SerializeField] private Image _turnProgressFill;
        [SerializeField] private Text _turnTimerText;
        
        [Header("Office Info")]
        [SerializeField] private Text _officeText;
        [SerializeField] private Text _officeTierText;
        [SerializeField] private Image _officeIcon;
        [SerializeField] private Text _termText;
        
        [Header("News Ticker")]
        [SerializeField] private Transform _tickerContainer;
        [SerializeField] private Text _tickerText;
        [SerializeField] private float _tickerScrollSpeed = 50f;
        [SerializeField] private Button _tickerExpandButton;
        [SerializeField] private GameObject _tickerPanel;
        [SerializeField] private Transform _tickerListContainer;
        [SerializeField] private GameObject _tickerItemPrefab;
        
        [Header("Alerts")]
        [SerializeField] private Transform _alertContainer;
        [SerializeField] private GameObject _alertPrefab;
        [SerializeField] private int _maxVisibleAlerts = 3;
        
        [Header("Quick Actions")]
        [SerializeField] private Transform _quickActionContainer;
        [SerializeField] private GameObject _quickActionPrefab;
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _menuButton;
        
        [Header("Advisor Panel")]
        [SerializeField] private GameObject _advisorPanel;
        [SerializeField] private Image _advisorPortrait;
        [SerializeField] private Text _advisorName;
        [SerializeField] private Text _advisorMessage;
        [SerializeField] private Button _advisorDismissButton;
        [SerializeField] private Button _advisorActionButton;
        
        [Header("Mini Map / Overview")]
        [SerializeField] private RawImage _miniMapDisplay;
        [SerializeField] private Text _supportPercentageText;
        [SerializeField] private Image _supportTrendIcon;
        
        [Header("Notifications")]
        [SerializeField] private Transform _notificationContainer;
        [SerializeField] private Text _notificationBadge;
        
        [Header("Audio")]
        [SerializeField] private AudioClip _alertSound;
        [SerializeField] private AudioClip _crisisSound;
        [SerializeField] private AudioClip _newsSound;
        
        [Header("Colors")]
        [SerializeField] private Color _positiveColor = new Color(0.2f, 0.7f, 0.3f);
        [SerializeField] private Color _negativeColor = new Color(0.8f, 0.2f, 0.2f);
        [SerializeField] private Color _neutralColor = new Color(0.5f, 0.5f, 0.5f);
        [SerializeField] private Color _urgentColor = new Color(0.9f, 0.4f, 0.1f);
        
        #endregion
        
        #region Private Fields
        
        private Dictionary<HUDResourceType, ResourceBarUI> _resourceBars;
        private List<TickerItem> _tickerItems;
        private List<GameObject> _activeAlerts;
        private List<QuickActionData> _quickActions;
        private Queue<AdvisorMessage> _advisorQueue;
        
        private int _currentTickerIndex;
        private float _tickerPosition;
        private bool _isTickerPaused;
        private AdvisorMessage _currentAdvisorMessage;
        
        private int _unreadNotifications;
        private float _lastTickerUpdate;
        
        // Cached game state reference
        private IGameStateProvider _gameState;
        
        #endregion
        
        #region Events
        
        public event Action OnEndTurnClicked;
        public event Action OnPauseClicked;
        public event Action OnMenuClicked;
        public event Action<string> OnTickerItemClicked;
        public event Action<string> OnQuickActionClicked;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            _resourceBars = new Dictionary<HUDResourceType, ResourceBarUI>();
            _tickerItems = new List<TickerItem>();
            _activeAlerts = new List<GameObject>();
            _quickActions = new List<QuickActionData>();
            _advisorQueue = new Queue<AdvisorMessage>();
            
            InitializeResourceBars();
            SetupButtonListeners();
        }
        
        private void Update()
        {
            UpdateTicker();
            UpdateTurnTimer();
            ProcessAdvisorQueue();
        }
        
        #endregion
        
        #region Screen Lifecycle
        
        public override void OnScreenEnter(object data)
        {
            base.OnScreenEnter(data);
            
            if (data is IGameStateProvider gameState)
            {
                _gameState = gameState;
            }
            
            RefreshAllDisplays();
            
            if (_tickerPanel != null)
            {
                _tickerPanel.SetActive(false);
            }
            
            if (_advisorPanel != null)
            {
                _advisorPanel.SetActive(false);
            }
        }
        
        public override void OnScreenPause()
        {
            base.OnScreenPause();
            _isTickerPaused = true;
        }
        
        public override void OnScreenResume()
        {
            base.OnScreenResume();
            _isTickerPaused = false;
            RefreshAllDisplays();
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeResourceBars()
        {
            // Use preset bars if available
            if (_presetResourceBars != null)
            {
                foreach (var bar in _presetResourceBars)
                {
                    if (bar != null)
                    {
                        _resourceBars[bar.ResourceType] = bar;
                    }
                }
            }
        }
        
        private void SetupButtonListeners()
        {
            if (_endTurnButton != null)
            {
                _endTurnButton.onClick.AddListener(HandleEndTurn);
            }
            
            if (_pauseButton != null)
            {
                _pauseButton.onClick.AddListener(() => OnPauseClicked?.Invoke());
            }
            
            if (_menuButton != null)
            {
                _menuButton.onClick.AddListener(() => OnMenuClicked?.Invoke());
            }
            
            if (_tickerExpandButton != null)
            {
                _tickerExpandButton.onClick.AddListener(ToggleTickerPanel);
            }
            
            if (_advisorDismissButton != null)
            {
                _advisorDismissButton.onClick.AddListener(DismissAdvisorMessage);
            }
        }
        
        #endregion
        
        #region Resource Display
        
        /// <summary>
        /// Update a resource bar display.
        /// </summary>
        public void UpdateResource(ResourceBarData data)
        {
            if (!_resourceBars.TryGetValue(data.Type, out var bar))
            {
                bar = CreateResourceBar(data.Type);
            }
            
            bar.UpdateDisplay(data);
        }
        
        /// <summary>
        /// Update all resources from game state.
        /// </summary>
        public void UpdateAllResources(Dictionary<HUDResourceType, ResourceBarData> resources)
        {
            foreach (var kvp in resources)
            {
                UpdateResource(kvp.Value);
            }
        }
        
        private ResourceBarUI CreateResourceBar(HUDResourceType type)
        {
            if (_resourceBarPrefab == null || _resourceBarContainer == null)
                return null;
            
            var go = Instantiate(_resourceBarPrefab, _resourceBarContainer);
            var bar = go.GetComponent<ResourceBarUI>();
            bar.ResourceType = type;
            _resourceBars[type] = bar;
            
            return bar;
        }
        
        /// <summary>
        /// Flash a resource bar to draw attention.
        /// </summary>
        public void FlashResource(HUDResourceType type, Color color)
        {
            if (_resourceBars.TryGetValue(type, out var bar))
            {
                bar.Flash(color);
            }
        }
        
        #endregion
        
        #region Turn Info Display
        
        /// <summary>
        /// Update turn display.
        /// </summary>
        public void UpdateTurnInfo(int turn, int year, string phase, string date = null)
        {
            if (_turnText != null) _turnText.text = $"Turn {turn}";
            if (_yearText != null) _yearText.text = year.ToString();
            if (_phaseText != null) _phaseText.text = phase;
            if (_dateText != null) _dateText.text = date ?? $"January {year}";
        }
        
        /// <summary>
        /// Update office display.
        /// </summary>
        public void UpdateOfficeInfo(string officeName, int tier, int currentTerm, int maxTerms)
        {
            if (_officeText != null) _officeText.text = officeName;
            if (_officeTierText != null) _officeTierText.text = $"Tier {tier}";
            if (_termText != null) _termText.text = $"Term {currentTerm}/{maxTerms}";
        }
        
        /// <summary>
        /// Update turn timer display.
        /// </summary>
        public void UpdateTurnTimer(float timeRemaining, float totalTime)
        {
            if (_turnTimerText != null)
            {
                int minutes = (int)(timeRemaining / 60);
                int seconds = (int)(timeRemaining % 60);
                _turnTimerText.text = $"{minutes:00}:{seconds:00}";
                
                // Color based on urgency
                if (timeRemaining < 30)
                {
                    _turnTimerText.color = _urgentColor;
                }
                else
                {
                    _turnTimerText.color = Color.white;
                }
            }
            
            if (_turnProgressFill != null)
            {
                _turnProgressFill.fillAmount = 1 - (timeRemaining / totalTime);
            }
        }
        
        /// <summary>
        /// Set end turn button state.
        /// </summary>
        public void SetEndTurnButtonState(bool canEndTurn, string buttonText = null)
        {
            if (_endTurnButton != null)
            {
                _endTurnButton.interactable = canEndTurn;
            }
            
            if (_endTurnButtonText != null)
            {
                _endTurnButtonText.text = buttonText ?? "End Turn";
            }
        }
        
        private void HandleEndTurn()
        {
            UIManager.PlayClickSound();
            OnEndTurnClicked?.Invoke();
        }
        
        private void UpdateTurnTimer()
        {
            // Auto-update from game state if available
            // This would be driven by external calls in actual implementation
        }
        
        #endregion
        
        #region News Ticker
        
        /// <summary>
        /// Add an item to the news ticker.
        /// </summary>
        public void AddTickerItem(TickerItem item)
        {
            item.Timestamp = Time.time;
            _tickerItems.Insert(0, item);
            
            // Keep ticker at reasonable size
            while (_tickerItems.Count > 50)
            {
                _tickerItems.RemoveAt(_tickerItems.Count - 1);
            }
            
            // Play sound based on urgency
            if (item.Urgency >= AlertUrgency.High)
            {
                PlaySound(_crisisSound ?? _newsSound);
            }
            else
            {
                PlaySound(_newsSound);
            }
            
            // Create in list if panel is open
            if (_tickerPanel != null && _tickerPanel.activeSelf)
            {
                CreateTickerListItem(item);
            }
        }
        
        /// <summary>
        /// Add a simple headline to the ticker.
        /// </summary>
        public void AddHeadline(string headline, TickerCategory category = TickerCategory.Politics, 
            AlertUrgency urgency = AlertUrgency.Low, Action onClick = null)
        {
            AddTickerItem(new TickerItem
            {
                Id = Guid.NewGuid().ToString(),
                Headline = headline,
                Category = category,
                Urgency = urgency,
                OnClick = onClick
            });
        }
        
        private void UpdateTicker()
        {
            if (_isTickerPaused || _tickerItems.Count == 0) return;
            if (_tickerText == null) return;
            
            // Build scrolling text
            string tickerContent = "";
            foreach (var item in _tickerItems.Take(10))
            {
                string prefix = GetTickerPrefix(item.Category);
                tickerContent += $"  {prefix} {item.Headline}  •";
            }
            
            // Scroll position
            _tickerPosition -= _tickerScrollSpeed * Time.deltaTime;
            
            // Calculate visible portion based on position
            _tickerText.text = tickerContent;
            
            // Reset when scrolled through
            float textWidth = _tickerText.preferredWidth;
            if (_tickerPosition < -textWidth)
            {
                _tickerPosition = Screen.width;
            }
            
            // Apply position
            var rect = _tickerText.rectTransform;
            rect.anchoredPosition = new Vector2(_tickerPosition, rect.anchoredPosition.y);
        }
        
        private string GetTickerPrefix(TickerCategory category)
        {
            return category switch
            {
                TickerCategory.Politics => "🏛️",
                TickerCategory.Economy => "📈",
                TickerCategory.Social => "👥",
                TickerCategory.Crisis => "⚠️",
                TickerCategory.Personal => "📋",
                TickerCategory.Opponent => "👤",
                _ => "📰"
            };
        }
        
        private void ToggleTickerPanel()
        {
            if (_tickerPanel == null) return;
            
            bool show = !_tickerPanel.activeSelf;
            _tickerPanel.SetActive(show);
            
            if (show)
            {
                RefreshTickerList();
            }
        }
        
        private void RefreshTickerList()
        {
            if (_tickerListContainer == null) return;
            
            // Clear existing
            foreach (Transform child in _tickerListContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Create items
            foreach (var item in _tickerItems)
            {
                CreateTickerListItem(item);
            }
        }
        
        private void CreateTickerListItem(TickerItem item)
        {
            if (_tickerItemPrefab == null || _tickerListContainer == null) return;
            
            var go = Instantiate(_tickerItemPrefab, _tickerListContainer);
            var controller = go.GetComponent<TickerItemUI>();
            
            if (controller != null)
            {
                controller.Setup(item, () =>
                {
                    item.IsRead = true;
                    item.OnClick?.Invoke();
                    OnTickerItemClicked?.Invoke(item.Id);
                });
            }
        }
        
        #endregion
        
        #region Alerts
        
        /// <summary>
        /// Show an alert on the HUD.
        /// </summary>
        public void ShowAlert(string title, string message, AlertUrgency urgency, Action onClick = null)
        {
            // Remove oldest if at max
            while (_activeAlerts.Count >= _maxVisibleAlerts)
            {
                var oldest = _activeAlerts[0];
                _activeAlerts.RemoveAt(0);
                Destroy(oldest);
            }
            
            // Create new alert
            if (_alertPrefab != null && _alertContainer != null)
            {
                var go = Instantiate(_alertPrefab, _alertContainer);
                var alert = go.GetComponent<AlertUI>();
                
                if (alert != null)
                {
                    alert.Setup(title, message, urgency, () =>
                    {
                        onClick?.Invoke();
                        _activeAlerts.Remove(go);
                        Destroy(go);
                    });
                }
                
                _activeAlerts.Add(go);
            }
            
            // Play sound
            if (urgency >= AlertUrgency.High)
            {
                PlaySound(_crisisSound);
            }
            else
            {
                PlaySound(_alertSound);
            }
        }
        
        /// <summary>
        /// Clear all alerts.
        /// </summary>
        public void ClearAlerts()
        {
            foreach (var alert in _activeAlerts)
            {
                if (alert != null) Destroy(alert);
            }
            _activeAlerts.Clear();
        }
        
        #endregion
        
        #region Quick Actions
        
        /// <summary>
        /// Set available quick actions.
        /// </summary>
        public void SetQuickActions(List<QuickActionData> actions)
        {
            _quickActions = actions;
            RefreshQuickActions();
        }
        
        /// <summary>
        /// Add a quick action.
        /// </summary>
        public void AddQuickAction(QuickActionData action)
        {
            _quickActions.Add(action);
            RefreshQuickActions();
        }
        
        /// <summary>
        /// Update a quick action's availability.
        /// </summary>
        public void UpdateQuickActionAvailability(string actionId, bool isAvailable)
        {
            var action = _quickActions.FirstOrDefault(a => a.Id == actionId);
            if (action != null)
            {
                action.IsAvailable = isAvailable;
                RefreshQuickActions();
            }
        }
        
        private void RefreshQuickActions()
        {
            if (_quickActionContainer == null || _quickActionPrefab == null) return;
            
            // Clear existing
            foreach (Transform child in _quickActionContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Create buttons
            foreach (var action in _quickActions)
            {
                var go = Instantiate(_quickActionPrefab, _quickActionContainer);
                var button = go.GetComponent<QuickActionButton>();
                
                if (button != null)
                {
                    button.Setup(action, () =>
                    {
                        if (action.IsAvailable)
                        {
                            action.OnClick?.Invoke();
                            OnQuickActionClicked?.Invoke(action.Id);
                        }
                    });
                }
            }
        }
        
        #endregion
        
        #region Advisor System
        
        /// <summary>
        /// Queue an advisor message.
        /// </summary>
        public void QueueAdvisorMessage(AdvisorMessage message)
        {
            _advisorQueue.Enqueue(message);
        }
        
        /// <summary>
        /// Show advisor message immediately.
        /// </summary>
        public void ShowAdvisorMessage(AdvisorMessage message)
        {
            if (_advisorPanel == null) return;
            
            _currentAdvisorMessage = message;
            
            if (_advisorPortrait != null && message.AdvisorPortrait != null)
            {
                _advisorPortrait.sprite = message.AdvisorPortrait;
            }
            
            if (_advisorName != null) _advisorName.text = message.AdvisorName;
            if (_advisorMessage != null) _advisorMessage.text = message.Message;
            
            if (_advisorActionButton != null)
            {
                _advisorActionButton.gameObject.SetActive(message.OnClick != null);
                _advisorActionButton.onClick.RemoveAllListeners();
                _advisorActionButton.onClick.AddListener(() =>
                {
                    message.OnClick?.Invoke();
                    DismissAdvisorMessage();
                });
            }
            
            _advisorPanel.SetActive(true);
            
            // Auto-dismiss after duration
            if (message.DisplayDuration > 0)
            {
                StartCoroutine(AutoDismissAdvisor(message.DisplayDuration));
            }
        }
        
        private void DismissAdvisorMessage()
        {
            if (_advisorPanel != null)
            {
                _advisorPanel.SetActive(false);
            }
            _currentAdvisorMessage = null;
        }
        
        private IEnumerator AutoDismissAdvisor(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (_currentAdvisorMessage != null)
            {
                DismissAdvisorMessage();
            }
        }
        
        private void ProcessAdvisorQueue()
        {
            if (_currentAdvisorMessage != null) return;
            if (_advisorQueue.Count == 0) return;
            
            var message = _advisorQueue.Dequeue();
            ShowAdvisorMessage(message);
        }
        
        #endregion
        
        #region Notifications
        
        /// <summary>
        /// Update notification badge.
        /// </summary>
        public void UpdateNotificationBadge(int count)
        {
            _unreadNotifications = count;
            
            if (_notificationBadge != null)
            {
                _notificationBadge.text = count > 99 ? "99+" : count.ToString();
                _notificationBadge.transform.parent.gameObject.SetActive(count > 0);
            }
        }
        
        /// <summary>
        /// Increment notification count.
        /// </summary>
        public void AddNotification()
        {
            UpdateNotificationBadge(_unreadNotifications + 1);
        }
        
        /// <summary>
        /// Clear notifications.
        /// </summary>
        public void ClearNotifications()
        {
            UpdateNotificationBadge(0);
        }
        
        #endregion
        
        #region Support Display
        
        /// <summary>
        /// Update support percentage display.
        /// </summary>
        public void UpdateSupportDisplay(float currentSupport, float trend)
        {
            if (_supportPercentageText != null)
            {
                _supportPercentageText.text = $"{currentSupport:F1}%";
                _supportPercentageText.color = currentSupport >= 50 ? _positiveColor : _negativeColor;
            }
            
            if (_supportTrendIcon != null)
            {
                // Rotate arrow based on trend
                float rotation = trend > 0 ? 0 : trend < 0 ? 180 : 90;
                _supportTrendIcon.transform.rotation = Quaternion.Euler(0, 0, rotation);
                _supportTrendIcon.color = trend > 0 ? _positiveColor : trend < 0 ? _negativeColor : _neutralColor;
            }
        }
        
        #endregion
        
        #region Refresh
        
        /// <summary>
        /// Refresh all HUD displays.
        /// </summary>
        public void RefreshAllDisplays()
        {
            if (_gameState != null)
            {
                // Would pull from game state and update all displays
            }
            
            RefreshQuickActions();
        }
        
        #endregion
        
        #region Audio
        
        private void PlaySound(AudioClip clip)
        {
            if (clip != null)
            {
                // Use UI audio source
                UIManager.Instance?.PlayClickSound();
            }
        }
        
        #endregion
    }
    
    #region HUD Component Classes
    
    /// <summary>
    /// Resource bar UI component.
    /// </summary>
    public class ResourceBarUI : MonoBehaviour
    {
        [SerializeField] private HUDResourceType _resourceType;
        [SerializeField] private Image _fillBar;
        [SerializeField] private Image _deltaBar;
        [SerializeField] private Text _valueText;
        [SerializeField] private Text _nameText;
        [SerializeField] private Image _icon;
        [SerializeField] private Image _background;
        [SerializeField] private CanvasGroup _canvasGroup;
        
        public HUDResourceType ResourceType 
        { 
            get => _resourceType; 
            set => _resourceType = value; 
        }
        
        private Coroutine _flashCoroutine;
        
        public void UpdateDisplay(ResourceBarData data)
        {
            if (_fillBar != null)
            {
                float fillAmount = data.MaxValue > 0 ? data.CurrentValue / data.MaxValue : 0;
                _fillBar.fillAmount = fillAmount;
                _fillBar.color = data.BarColor;
            }
            
            if (_deltaBar != null && data.ShowDelta)
            {
                float delta = data.CurrentValue - data.PreviousValue;
                if (delta != 0)
                {
                    _deltaBar.gameObject.SetActive(true);
                    // Show delta indicator
                }
                else
                {
                    _deltaBar.gameObject.SetActive(false);
                }
            }
            
            if (_valueText != null)
            {
                if (data.ShowAsPercentage)
                {
                    _valueText.text = $"{data.CurrentValue:F0}%";
                }
                else
                {
                    _valueText.text = FormatValue(data.CurrentValue);
                }
            }
            
            if (_nameText != null)
            {
                _nameText.text = data.Name;
            }
            
            if (_icon != null && data.Icon != null)
            {
                _icon.sprite = data.Icon;
            }
        }
        
        private string FormatValue(float value)
        {
            if (value >= 1000000)
                return $"{value / 1000000:F1}M";
            if (value >= 1000)
                return $"{value / 1000:F1}K";
            return $"{value:F0}";
        }
        
        public void Flash(Color color)
        {
            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
            }
            _flashCoroutine = StartCoroutine(FlashCoroutine(color));
        }
        
        private IEnumerator FlashCoroutine(Color color)
        {
            Color originalColor = _background != null ? _background.color : Color.clear;
            
            for (int i = 0; i < 3; i++)
            {
                if (_background != null) _background.color = color;
                yield return new WaitForSeconds(0.1f);
                if (_background != null) _background.color = originalColor;
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
    
    /// <summary>
    /// Ticker list item UI component.
    /// </summary>
    public class TickerItemUI : MonoBehaviour
    {
        [SerializeField] private Text _headlineText;
        [SerializeField] private Text _categoryText;
        [SerializeField] private Text _timeText;
        [SerializeField] private Image _urgencyIndicator;
        [SerializeField] private Image _readIndicator;
        [SerializeField] private Button _button;
        
        public void Setup(TickerItem item, Action onClick)
        {
            if (_headlineText != null) _headlineText.text = item.Headline;
            if (_categoryText != null) _categoryText.text = item.Category.ToString();
            
            if (_timeText != null)
            {
                float age = Time.time - item.Timestamp;
                _timeText.text = FormatTimeAgo(age);
            }
            
            if (_urgencyIndicator != null)
            {
                _urgencyIndicator.color = GetUrgencyColor(item.Urgency);
            }
            
            if (_readIndicator != null)
            {
                _readIndicator.gameObject.SetActive(!item.IsRead);
            }
            
            if (_button != null)
            {
                _button.onClick.AddListener(() => onClick());
            }
        }
        
        private string FormatTimeAgo(float seconds)
        {
            if (seconds < 60) return "Just now";
            if (seconds < 3600) return $"{(int)(seconds / 60)}m ago";
            return $"{(int)(seconds / 3600)}h ago";
        }
        
        private Color GetUrgencyColor(AlertUrgency urgency)
        {
            return urgency switch
            {
                AlertUrgency.Critical => new Color(0.8f, 0.1f, 0.1f),
                AlertUrgency.High => new Color(0.9f, 0.5f, 0.1f),
                AlertUrgency.Medium => new Color(0.9f, 0.8f, 0.2f),
                _ => new Color(0.5f, 0.5f, 0.5f)
            };
        }
    }
    
    /// <summary>
    /// Alert UI component.
    /// </summary>
    public class AlertUI : MonoBehaviour
    {
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _messageText;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _urgencyBar;
        [SerializeField] private Button _button;
        [SerializeField] private Button _dismissButton;
        [SerializeField] private float _displayDuration = 5f;
        [SerializeField] private CanvasGroup _canvasGroup;
        
        private Action _onClick;
        
        public void Setup(string title, string message, AlertUrgency urgency, Action onClick)
        {
            _onClick = onClick;
            
            if (_titleText != null) _titleText.text = title;
            if (_messageText != null) _messageText.text = message;
            
            Color urgencyColor = GetUrgencyColor(urgency);
            if (_urgencyBar != null) _urgencyBar.color = urgencyColor;
            
            if (_button != null)
            {
                _button.onClick.AddListener(() => _onClick?.Invoke());
            }
            
            if (_dismissButton != null)
            {
                _dismissButton.onClick.AddListener(() => _onClick?.Invoke());
            }
            
            StartCoroutine(FadeAndDestroy());
        }
        
        private Color GetUrgencyColor(AlertUrgency urgency)
        {
            return urgency switch
            {
                AlertUrgency.Critical => new Color(0.8f, 0.1f, 0.1f),
                AlertUrgency.High => new Color(0.9f, 0.5f, 0.1f),
                AlertUrgency.Medium => new Color(0.9f, 0.8f, 0.2f),
                _ => new Color(0.3f, 0.6f, 0.9f)
            };
        }
        
        private IEnumerator FadeAndDestroy()
        {
            yield return new WaitForSeconds(_displayDuration - 0.5f);
            
            if (_canvasGroup != null)
            {
                float elapsed = 0;
                while (elapsed < 0.5f)
                {
                    elapsed += Time.deltaTime;
                    _canvasGroup.alpha = 1 - (elapsed / 0.5f);
                    yield return null;
                }
            }
            
            _onClick?.Invoke();
        }
    }
    
    /// <summary>
    /// Quick action button UI component.
    /// </summary>
    public class QuickActionButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _hotkeyText;
        [SerializeField] private GameObject _notificationDot;
        [SerializeField] private CanvasGroup _canvasGroup;
        
        private QuickActionData _data;
        
        public void Setup(QuickActionData data, Action onClick)
        {
            _data = data;
            
            if (_icon != null && data.Icon != null)
            {
                _icon.sprite = data.Icon;
            }
            
            if (_nameText != null) _nameText.text = data.Name;
            if (_hotkeyText != null) _hotkeyText.text = data.Hotkey > 0 ? data.Hotkey.ToString() : "";
            if (_notificationDot != null) _notificationDot.SetActive(data.ShowNotification);
            
            if (_button != null)
            {
                _button.onClick.AddListener(() => onClick());
                _button.interactable = data.IsAvailable;
            }
            
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = data.IsAvailable ? 1f : 0.5f;
            }
        }
        
        private void Update()
        {
            // Check for hotkey
            if (_data != null && _data.Hotkey > 0 && _data.IsAvailable)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + _data.Hotkey - 1))
                {
                    _button?.onClick.Invoke();
                }
            }
        }
    }
    
    #endregion
}
