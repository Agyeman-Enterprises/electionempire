// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - UI Manager
// Sprint 9: Core UI system with screen management, transitions, and modals
// ═══════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace ElectionEmpire.UI
{
    #region Enums
    
    /// <summary>
    /// Types of UI screens in the game.
    /// </summary>
    public enum ScreenType
    {
        // Main Menus
        MainMenu,
        NewGame,
        LoadGame,
        Settings,
        Credits,
        
        // Character Creation
        BackgroundSelect,
        StatAllocation,
        TraitSelection,
        CharacterSummary,
        
        // Gameplay
        GameHUD,
        CampaignDashboard,
        PolicyMenu,
        StaffManagement,
        MediaCenter,
        OppositionResearch,
        
        // Events & Crises
        EventPopup,
        CrisisManagement,
        ScandalResponse,
        DebateArena,
        
        // Elections
        ElectionNight,
        VictoryScreen,
        DefeatScreen,
        
        // Multiplayer
        MultiplayerLobby,
        MatchmakingQueue,
        Leaderboard,
        
        // System
        PauseMenu,
        ConfirmationDialog,
        Tutorial
    }
    
    /// <summary>
    /// Screen transition types.
    /// </summary>
    public enum TransitionType
    {
        None,
        Fade,
        SlideLeft,
        SlideRight,
        SlideUp,
        SlideDown,
        Scale,
        Custom
    }
    
    /// <summary>
    /// Modal priority levels.
    /// </summary>
    public enum ModalPriority
    {
        Low = 0,
        Normal = 100,
        High = 200,
        Critical = 300,
        System = 400
    }
    
    #endregion
    
    #region Screen Configuration
    
    /// <summary>
    /// Configuration for a UI screen.
    /// </summary>
    [Serializable]
    public class ScreenConfig
    {
        public ScreenType Type;
        public GameObject Prefab;
        public TransitionType TransitionIn = TransitionType.Fade;
        public TransitionType TransitionOut = TransitionType.Fade;
        public float TransitionDuration = 0.3f;
        public bool PauseGameWhenActive = false;
        public bool AllowBackNavigation = true;
        public AudioClip OpenSound;
        public AudioClip CloseSound;
    }
    
    /// <summary>
    /// Active screen instance data.
    /// </summary>
    public class ScreenInstance
    {
        public ScreenType Type;
        public GameObject GameObject;
        public IScreen Controller;
        public CanvasGroup CanvasGroup;
        public bool IsTransitioning;
        public object NavigationData;
    }
    
    #endregion
    
    #region Screen Interface
    
    /// <summary>
    /// Interface for screen controllers.
    /// </summary>
    public interface IScreen
    {
        void OnScreenEnter(object data);
        void OnScreenExit();
        void OnScreenPause();
        void OnScreenResume();
        bool CanNavigateBack();
    }
    
    /// <summary>
    /// Base class for screen controllers.
    /// </summary>
    public abstract class BaseScreen : MonoBehaviour, IScreen
    {
        protected UIManager UIManager => UIManager.Instance;
        
        public virtual void OnScreenEnter(object data) { }
        public virtual void OnScreenExit() { }
        public virtual void OnScreenPause() { }
        public virtual void OnScreenResume() { }
        public virtual bool CanNavigateBack() => true;
        
        protected void NavigateTo(ScreenType screen, object data = null)
        {
            UIManager.NavigateTo(screen, data);
        }
        
        protected void NavigateBack()
        {
            UIManager.NavigateBack();
        }
        
        protected void ShowModal(ModalConfig config)
        {
            UIManager.ShowModal(config);
        }
    }
    
    #endregion
    
    #region Modal System
    
    /// <summary>
    /// Configuration for a modal dialog.
    /// </summary>
    [Serializable]
    public class ModalConfig
    {
        public string Title;
        public string Message;
        public Sprite Icon;
        public ModalPriority Priority = ModalPriority.Normal;
        public List<ModalButton> Buttons = new List<ModalButton>();
        public bool CloseOnBackgroundClick = true;
        public float AutoCloseTime = 0f;
        public Action OnClose;
    }
    
    [Serializable]
    public class ModalButton
    {
        public string Text;
        public Action OnClick;
        public bool CloseOnClick = true;
        public Color? ButtonColor;
    }
    
    /// <summary>
    /// Active modal instance.
    /// </summary>
    public class ModalInstance
    {
        public string Id;
        public ModalConfig Config;
        public GameObject GameObject;
        public ModalPriority Priority;
        public float OpenTime;
    }
    
    #endregion
    
    #region Notification System
    
    /// <summary>
    /// Types of toast notifications.
    /// </summary>
    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error,
        Achievement,
        NewsAlert
    }
    
    /// <summary>
    /// Toast notification data.
    /// </summary>
    [Serializable]
    public class ToastNotification
    {
        public string Title;
        public string Message;
        public NotificationType Type;
        public float Duration = 3f;
        public Sprite Icon;
        public Action OnClick;
        public bool Dismissable = true;
    }
    
    #endregion
    
    /// <summary>
    /// Central UI management system for Election Empire.
    /// Handles screen navigation, modals, notifications, and transitions.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        #region Singleton
        
        private static UIManager _instance;
        public static UIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<UIManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("UIManager");
                        _instance = go.AddComponent<UIManager>();
                    }
                }
                return _instance;
            }
        }
        
        #endregion
        
        #region Serialized Fields
        
        [Header("Screen Configuration")]
        [SerializeField] private List<ScreenConfig> _screenConfigs = new List<ScreenConfig>();
        [SerializeField] private Transform _screenContainer;
        [SerializeField] private ScreenType _initialScreen = ScreenType.MainMenu;
        
        [Header("Modal Configuration")]
        [SerializeField] private GameObject _modalPrefab;
        [SerializeField] private Transform _modalContainer;
        [SerializeField] private GameObject _modalBackdrop;
        
        [Header("Notification Configuration")]
        [SerializeField] private GameObject _toastPrefab;
        [SerializeField] private Transform _toastContainer;
        [SerializeField] private int _maxVisibleToasts = 3;
        
        [Header("Transition Settings")]
        [SerializeField] private AnimationCurve _transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private CanvasGroup _fadeOverlay;
        
        [Header("Audio")]
        [SerializeField] private AudioSource _uiAudioSource;
        [SerializeField] private AudioClip _defaultClickSound;
        [SerializeField] private AudioClip _defaultBackSound;
        
        #endregion
        
        #region Private Fields
        
        private Dictionary<ScreenType, ScreenConfig> _configLookup;
        private Stack<ScreenInstance> _screenStack;
        private ScreenInstance _currentScreen;
        
        private List<ModalInstance> _activeModals;
        private Queue<ToastNotification> _pendingToasts;
        private List<GameObject> _activeToasts;
        
        private bool _isTransitioning;
        private bool _inputBlocked;
        
        #endregion
        
        #region Events
        
        public event Action<ScreenType> OnScreenChanged;
        public event Action<ScreenType> OnScreenPushed;
        public event Action<ScreenType> OnScreenPopped;
        public event Action<string> OnModalOpened;
        public event Action<string> OnModalClosed;
        
        #endregion
        
        #region Properties
        
        public ScreenType CurrentScreenType => _currentScreen?.Type ?? ScreenType.MainMenu;
        public bool IsTransitioning => _isTransitioning;
        public bool HasActiveModal => _activeModals.Count > 0;
        public int ScreenStackDepth => _screenStack.Count;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            Initialize();
        }
        
        private void Start()
        {
            // Navigate to initial screen
            NavigateTo(_initialScreen);
        }
        
        private void Update()
        {
            // Handle back button/escape
            if (Input.GetKeyDown(KeyCode.Escape) && !_inputBlocked)
            {
                HandleBackInput();
            }
            
            // Process pending toasts
            ProcessToastQueue();
            
            // Update auto-close modals
            UpdateModals();
        }
        
        #endregion
        
        #region Initialization
        
        private void Initialize()
        {
            _configLookup = new Dictionary<ScreenType, ScreenConfig>();
            foreach (var config in _screenConfigs)
            {
                _configLookup[config.Type] = config;
            }
            
            _screenStack = new Stack<ScreenInstance>();
            _activeModals = new List<ModalInstance>();
            _pendingToasts = new Queue<ToastNotification>();
            _activeToasts = new List<GameObject>();
            
            // Ensure containers exist
            if (_screenContainer == null)
            {
                var canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    _screenContainer = canvas.transform;
                }
            }
            
            if (_fadeOverlay != null)
            {
                _fadeOverlay.alpha = 0;
                _fadeOverlay.blocksRaycasts = false;
            }
            
            if (_modalBackdrop != null)
            {
                _modalBackdrop.SetActive(false);
            }
        }
        
        #endregion
        
        #region Screen Navigation
        
        /// <summary>
        /// Navigate to a new screen, adding current to stack.
        /// </summary>
        public void NavigateTo(ScreenType screenType, object data = null)
        {
            if (_isTransitioning) return;
            
            StartCoroutine(NavigateCoroutine(screenType, data, true));
        }
        
        /// <summary>
        /// Replace current screen without adding to stack.
        /// </summary>
        public void ReplaceTo(ScreenType screenType, object data = null)
        {
            if (_isTransitioning) return;
            
            StartCoroutine(NavigateCoroutine(screenType, data, false));
        }
        
        /// <summary>
        /// Navigate back to previous screen.
        /// </summary>
        public void NavigateBack()
        {
            if (_isTransitioning) return;
            if (_screenStack.Count == 0) return;
            if (_currentScreen?.Controller != null && !_currentScreen.Controller.CanNavigateBack()) return;
            
            StartCoroutine(NavigateBackCoroutine());
        }
        
        /// <summary>
        /// Clear navigation stack and go to screen.
        /// </summary>
        public void NavigateToRoot(ScreenType screenType, object data = null)
        {
            if (_isTransitioning) return;
            
            // Clear stack
            while (_screenStack.Count > 0)
            {
                var instance = _screenStack.Pop();
                DestroyScreen(instance);
            }
            
            StartCoroutine(NavigateCoroutine(screenType, data, false));
        }
        
        private IEnumerator NavigateCoroutine(ScreenType screenType, object data, bool pushToStack)
        {
            _isTransitioning = true;
            _inputBlocked = true;
            
            // Get config
            if (!_configLookup.TryGetValue(screenType, out var config))
            {
                Debug.LogError($"[UIManager] No config found for screen: {screenType}");
                _isTransitioning = false;
                _inputBlocked = false;
                yield break;
            }
            
            // Transition out current screen
            if (_currentScreen != null)
            {
                if (pushToStack)
                {
                    _currentScreen.Controller?.OnScreenPause();
                    _screenStack.Push(_currentScreen);
                    OnScreenPushed?.Invoke(_currentScreen.Type);
                }
                else
                {
                    yield return TransitionOut(_currentScreen, config.TransitionOut, config.TransitionDuration);
                    _currentScreen.Controller?.OnScreenExit();
                    DestroyScreen(_currentScreen);
                }
                
                PlaySound(config.CloseSound);
            }
            
            // Create new screen
            var newScreen = CreateScreen(screenType, config);
            newScreen.NavigationData = data;
            
            // Transition in new screen
            yield return TransitionIn(newScreen, config.TransitionIn, config.TransitionDuration);
            
            _currentScreen = newScreen;
            _currentScreen.Controller?.OnScreenEnter(data);
            
            PlaySound(config.OpenSound);
            
            // Handle game pause
            if (config.PauseGameWhenActive)
            {
                Time.timeScale = 0f;
            }
            
            _isTransitioning = false;
            _inputBlocked = false;
            
            OnScreenChanged?.Invoke(screenType);
        }
        
        private IEnumerator NavigateBackCoroutine()
        {
            _isTransitioning = true;
            _inputBlocked = true;
            
            var previousScreen = _screenStack.Pop();
            var currentConfig = _configLookup.GetValueOrDefault(_currentScreen.Type);
            var previousConfig = _configLookup.GetValueOrDefault(previousScreen.Type);
            
            // Transition out current
            yield return TransitionOut(_currentScreen, currentConfig?.TransitionOut ?? TransitionType.Fade, 
                currentConfig?.TransitionDuration ?? 0.3f);
            
            _currentScreen.Controller?.OnScreenExit();
            DestroyScreen(_currentScreen);
            
            PlaySound(_defaultBackSound);
            
            // Show previous
            previousScreen.GameObject.SetActive(true);
            yield return TransitionIn(previousScreen, previousConfig?.TransitionIn ?? TransitionType.Fade,
                previousConfig?.TransitionDuration ?? 0.3f);
            
            _currentScreen = previousScreen;
            _currentScreen.Controller?.OnScreenResume();
            
            // Handle game unpause
            if (currentConfig?.PauseGameWhenActive == true && previousConfig?.PauseGameWhenActive != true)
            {
                Time.timeScale = 1f;
            }
            
            _isTransitioning = false;
            _inputBlocked = false;
            
            OnScreenPopped?.Invoke(previousScreen.Type);
            OnScreenChanged?.Invoke(previousScreen.Type);
        }
        
        private ScreenInstance CreateScreen(ScreenType type, ScreenConfig config)
        {
            GameObject go;
            
            if (config.Prefab != null)
            {
                go = Instantiate(config.Prefab, _screenContainer);
            }
            else
            {
                go = new GameObject(type.ToString());
                go.transform.SetParent(_screenContainer, false);
                
                var rect = go.AddComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
            
            var canvasGroup = go.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = go.AddComponent<CanvasGroup>();
            }
            
            var controller = go.GetComponent<IScreen>();
            
            return new ScreenInstance
            {
                Type = type,
                GameObject = go,
                Controller = controller,
                CanvasGroup = canvasGroup
            };
        }
        
        private void DestroyScreen(ScreenInstance instance)
        {
            if (instance?.GameObject != null)
            {
                Destroy(instance.GameObject);
            }
        }
        
        #endregion
        
        #region Transitions
        
        private IEnumerator TransitionIn(ScreenInstance screen, TransitionType type, float duration)
        {
            screen.IsTransitioning = true;
            screen.GameObject.SetActive(true);
            
            switch (type)
            {
                case TransitionType.Fade:
                    yield return FadeTransition(screen.CanvasGroup, 0, 1, duration);
                    break;
                    
                case TransitionType.SlideLeft:
                    yield return SlideTransition(screen.GameObject.transform, new Vector2(Screen.width, 0), Vector2.zero, duration);
                    break;
                    
                case TransitionType.SlideRight:
                    yield return SlideTransition(screen.GameObject.transform, new Vector2(-Screen.width, 0), Vector2.zero, duration);
                    break;
                    
                case TransitionType.SlideUp:
                    yield return SlideTransition(screen.GameObject.transform, new Vector2(0, -Screen.height), Vector2.zero, duration);
                    break;
                    
                case TransitionType.SlideDown:
                    yield return SlideTransition(screen.GameObject.transform, new Vector2(0, Screen.height), Vector2.zero, duration);
                    break;
                    
                case TransitionType.Scale:
                    yield return ScaleTransition(screen.GameObject.transform, Vector3.zero, Vector3.one, duration);
                    break;
                    
                case TransitionType.None:
                default:
                    screen.CanvasGroup.alpha = 1;
                    break;
            }
            
            screen.IsTransitioning = false;
        }
        
        private IEnumerator TransitionOut(ScreenInstance screen, TransitionType type, float duration)
        {
            screen.IsTransitioning = true;
            
            switch (type)
            {
                case TransitionType.Fade:
                    yield return FadeTransition(screen.CanvasGroup, 1, 0, duration);
                    break;
                    
                case TransitionType.SlideLeft:
                    yield return SlideTransition(screen.GameObject.transform, Vector2.zero, new Vector2(-Screen.width, 0), duration);
                    break;
                    
                case TransitionType.SlideRight:
                    yield return SlideTransition(screen.GameObject.transform, Vector2.zero, new Vector2(Screen.width, 0), duration);
                    break;
                    
                case TransitionType.SlideUp:
                    yield return SlideTransition(screen.GameObject.transform, Vector2.zero, new Vector2(0, Screen.height), duration);
                    break;
                    
                case TransitionType.SlideDown:
                    yield return SlideTransition(screen.GameObject.transform, Vector2.zero, new Vector2(0, -Screen.height), duration);
                    break;
                    
                case TransitionType.Scale:
                    yield return ScaleTransition(screen.GameObject.transform, Vector3.one, Vector3.zero, duration);
                    break;
                    
                case TransitionType.None:
                default:
                    screen.CanvasGroup.alpha = 0;
                    break;
            }
            
            screen.GameObject.SetActive(false);
            screen.IsTransitioning = false;
        }
        
        private IEnumerator FadeTransition(CanvasGroup group, float from, float to, float duration)
        {
            float elapsed = 0;
            
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = _transitionCurve.Evaluate(elapsed / duration);
                group.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }
            
            group.alpha = to;
        }
        
        private IEnumerator SlideTransition(Transform target, Vector2 from, Vector2 to, float duration)
        {
            var rect = target as RectTransform;
            if (rect == null) yield break;
            
            float elapsed = 0;
            
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = _transitionCurve.Evaluate(elapsed / duration);
                rect.anchoredPosition = Vector2.Lerp(from, to, t);
                yield return null;
            }
            
            rect.anchoredPosition = to;
        }
        
        private IEnumerator ScaleTransition(Transform target, Vector3 from, Vector3 to, float duration)
        {
            float elapsed = 0;
            
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = _transitionCurve.Evaluate(elapsed / duration);
                target.localScale = Vector3.Lerp(from, to, t);
                yield return null;
            }
            
            target.localScale = to;
        }
        
        #endregion
        
        #region Modal System
        
        /// <summary>
        /// Show a modal dialog.
        /// </summary>
        public string ShowModal(ModalConfig config)
        {
            var modalId = Guid.NewGuid().ToString();
            
            // Create modal instance
            GameObject modalGo;
            if (_modalPrefab != null)
            {
                modalGo = Instantiate(_modalPrefab, _modalContainer);
            }
            else
            {
                modalGo = CreateDefaultModal(config);
            }
            
            var modal = new ModalInstance
            {
                Id = modalId,
                Config = config,
                GameObject = modalGo,
                Priority = config.Priority,
                OpenTime = Time.unscaledTime
            };
            
            // Setup modal content
            SetupModalContent(modal);
            
            // Insert sorted by priority
            int insertIndex = _activeModals.FindIndex(m => m.Priority < config.Priority);
            if (insertIndex < 0) insertIndex = _activeModals.Count;
            _activeModals.Insert(insertIndex, modal);
            
            // Update display order
            UpdateModalOrder();
            
            // Show backdrop
            if (_modalBackdrop != null)
            {
                _modalBackdrop.SetActive(true);
            }
            
            OnModalOpened?.Invoke(modalId);
            
            return modalId;
        }
        
        /// <summary>
        /// Close a modal by ID.
        /// </summary>
        public void CloseModal(string modalId)
        {
            var modal = _activeModals.FirstOrDefault(m => m.Id == modalId);
            if (modal == null) return;
            
            _activeModals.Remove(modal);
            
            modal.Config.OnClose?.Invoke();
            Destroy(modal.GameObject);
            
            // Hide backdrop if no modals
            if (_activeModals.Count == 0 && _modalBackdrop != null)
            {
                _modalBackdrop.SetActive(false);
            }
            
            OnModalClosed?.Invoke(modalId);
        }
        
        /// <summary>
        /// Close all modals.
        /// </summary>
        public void CloseAllModals()
        {
            foreach (var modal in _activeModals.ToList())
            {
                CloseModal(modal.Id);
            }
        }
        
        private GameObject CreateDefaultModal(ModalConfig config)
        {
            var go = new GameObject("Modal");
            go.transform.SetParent(_modalContainer ?? _screenContainer, false);
            
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(400, 250);
            
            var image = go.AddComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);
            
            var canvasGroup = go.AddComponent<CanvasGroup>();
            
            return go;
        }
        
        private void SetupModalContent(ModalInstance modal)
        {
            var controller = modal.GameObject.GetComponent<ModalController>();
            if (controller != null)
            {
                controller.Setup(modal.Config, modal.Id, this);
            }
        }
        
        private void UpdateModalOrder()
        {
            for (int i = 0; i < _activeModals.Count; i++)
            {
                _activeModals[i].GameObject.transform.SetSiblingIndex(i);
            }
        }
        
        private void UpdateModals()
        {
            foreach (var modal in _activeModals.ToList())
            {
                if (modal.Config.AutoCloseTime > 0)
                {
                    if (Time.unscaledTime - modal.OpenTime >= modal.Config.AutoCloseTime)
                    {
                        CloseModal(modal.Id);
                    }
                }
            }
        }
        
        /// <summary>
        /// Show a simple confirmation dialog.
        /// </summary>
        public void ShowConfirmation(string title, string message, Action onConfirm, Action onCancel = null)
        {
            var config = new ModalConfig
            {
                Title = title,
                Message = message,
                Priority = ModalPriority.High,
                Buttons = new List<ModalButton>
                {
                    new ModalButton { Text = "Cancel", OnClick = onCancel },
                    new ModalButton { Text = "Confirm", OnClick = onConfirm, ButtonColor = new Color(0.2f, 0.6f, 0.3f) }
                }
            };
            
            ShowModal(config);
        }
        
        /// <summary>
        /// Show a simple alert dialog.
        /// </summary>
        public void ShowAlert(string title, string message, Action onOK = null)
        {
            var config = new ModalConfig
            {
                Title = title,
                Message = message,
                Priority = ModalPriority.Normal,
                Buttons = new List<ModalButton>
                {
                    new ModalButton { Text = "OK", OnClick = onOK }
                }
            };
            
            ShowModal(config);
        }
        
        #endregion
        
        #region Notification System
        
        /// <summary>
        /// Show a toast notification.
        /// </summary>
        public void ShowToast(ToastNotification notification)
        {
            _pendingToasts.Enqueue(notification);
        }
        
        /// <summary>
        /// Show a simple info toast.
        /// </summary>
        public void ShowToast(string title, string message, NotificationType type = NotificationType.Info)
        {
            ShowToast(new ToastNotification
            {
                Title = title,
                Message = message,
                Type = type
            });
        }
        
        private void ProcessToastQueue()
        {
            // Remove expired toasts
            _activeToasts.RemoveAll(t => t == null);
            
            // Show pending toasts up to max
            while (_pendingToasts.Count > 0 && _activeToasts.Count < _maxVisibleToasts)
            {
                var notification = _pendingToasts.Dequeue();
                var toast = CreateToast(notification);
                _activeToasts.Add(toast);
            }
        }
        
        private GameObject CreateToast(ToastNotification notification)
        {
            GameObject toastGo;
            
            if (_toastPrefab != null)
            {
                toastGo = Instantiate(_toastPrefab, _toastContainer ?? _screenContainer);
            }
            else
            {
                toastGo = CreateDefaultToast(notification);
            }
            
            var controller = toastGo.GetComponent<ToastController>();
            if (controller != null)
            {
                controller.Setup(notification, () => 
                {
                    _activeToasts.Remove(toastGo);
                    Destroy(toastGo);
                });
            }
            else
            {
                // Auto-destroy after duration
                Destroy(toastGo, notification.Duration);
            }
            
            return toastGo;
        }
        
        private GameObject CreateDefaultToast(ToastNotification notification)
        {
            var go = new GameObject("Toast");
            go.transform.SetParent(_toastContainer ?? _screenContainer, false);
            
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(-20, -20 - (_activeToasts.Count * 80));
            rect.sizeDelta = new Vector2(300, 70);
            
            var image = go.AddComponent<Image>();
            image.color = GetToastColor(notification.Type);
            
            return go;
        }
        
        private Color GetToastColor(NotificationType type)
        {
            return type switch
            {
                NotificationType.Success => new Color(0.2f, 0.6f, 0.3f, 0.95f),
                NotificationType.Warning => new Color(0.8f, 0.6f, 0.1f, 0.95f),
                NotificationType.Error => new Color(0.7f, 0.2f, 0.2f, 0.95f),
                NotificationType.Achievement => new Color(0.6f, 0.4f, 0.8f, 0.95f),
                NotificationType.NewsAlert => new Color(0.3f, 0.5f, 0.7f, 0.95f),
                _ => new Color(0.2f, 0.2f, 0.25f, 0.95f)
            };
        }
        
        #endregion
        
        #region Input Handling
        
        private void HandleBackInput()
        {
            // Close top modal first
            if (_activeModals.Count > 0)
            {
                var topModal = _activeModals.Last();
                if (topModal.Config.CloseOnBackgroundClick)
                {
                    CloseModal(topModal.Id);
                }
                return;
            }
            
            // Then try to navigate back
            NavigateBack();
        }
        
        /// <summary>
        /// Block input temporarily.
        /// </summary>
        public void BlockInput(float duration)
        {
            StartCoroutine(BlockInputCoroutine(duration));
        }
        
        private IEnumerator BlockInputCoroutine(float duration)
        {
            _inputBlocked = true;
            yield return new WaitForSecondsRealtime(duration);
            _inputBlocked = false;
        }
        
        #endregion
        
        #region Audio
        
        private void PlaySound(AudioClip clip)
        {
            if (clip == null || _uiAudioSource == null) return;
            _uiAudioSource.PlayOneShot(clip);
        }
        
        /// <summary>
        /// Play default click sound.
        /// </summary>
        public void PlayClickSound()
        {
            PlaySound(_defaultClickSound);
        }
        
        #endregion
        
        #region Utilities
        
        /// <summary>
        /// Get the current screen controller.
        /// </summary>
        public T GetCurrentScreen<T>() where T : class, IScreen
        {
            return _currentScreen?.Controller as T;
        }
        
        /// <summary>
        /// Check if a screen is in the navigation stack.
        /// </summary>
        public bool IsScreenInStack(ScreenType type)
        {
            return _screenStack.Any(s => s.Type == type);
        }
        
        /// <summary>
        /// Register a screen configuration at runtime.
        /// </summary>
        public void RegisterScreen(ScreenConfig config)
        {
            _configLookup[config.Type] = config;
            _screenConfigs.Add(config);
        }
        
        #endregion
    }
    
    #region Modal Controller
    
    /// <summary>
    /// Controller for modal dialogs.
    /// </summary>
    public class ModalController : MonoBehaviour
    {
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _messageText;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Transform _buttonContainer;
        [SerializeField] private GameObject _buttonPrefab;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _backgroundButton;
        
        private string _modalId;
        private UIManager _uiManager;
        private ModalConfig _config;
        
        public void Setup(ModalConfig config, string modalId, UIManager manager)
        {
            _config = config;
            _modalId = modalId;
            _uiManager = manager;
            
            if (_titleText != null) _titleText.text = config.Title;
            if (_messageText != null) _messageText.text = config.Message;
            if (_iconImage != null && config.Icon != null) _iconImage.sprite = config.Icon;
            
            // Setup buttons
            if (_buttonContainer != null && _buttonPrefab != null)
            {
                foreach (Transform child in _buttonContainer)
                {
                    Destroy(child.gameObject);
                }
                
                foreach (var buttonConfig in config.Buttons)
                {
                    var buttonGo = Instantiate(_buttonPrefab, _buttonContainer);
                    var button = buttonGo.GetComponent<Button>();
                    var text = buttonGo.GetComponentInChildren<Text>();
                    
                    if (text != null) text.text = buttonConfig.Text;
                    if (buttonConfig.ButtonColor.HasValue)
                    {
                        var image = button.GetComponent<Image>();
                        if (image != null) image.color = buttonConfig.ButtonColor.Value;
                    }
                    
                    button.onClick.AddListener(() =>
                    {
                        buttonConfig.OnClick?.Invoke();
                        if (buttonConfig.CloseOnClick)
                        {
                            _uiManager.CloseModal(_modalId);
                        }
                    });
                }
            }
            
            // Close button
            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(() => _uiManager.CloseModal(_modalId));
            }
            
            // Background click
            if (_backgroundButton != null && config.CloseOnBackgroundClick)
            {
                _backgroundButton.onClick.AddListener(() => _uiManager.CloseModal(_modalId));
            }
        }
    }
    
    #endregion
    
    #region Toast Controller
    
    /// <summary>
    /// Controller for toast notifications.
    /// </summary>
    public class ToastController : MonoBehaviour
    {
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _messageText;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Button _dismissButton;
        [SerializeField] private Button _clickButton;
        [SerializeField] private CanvasGroup _canvasGroup;
        
        private ToastNotification _notification;
        private Action _onDismiss;
        private float _startTime;
        
        public void Setup(ToastNotification notification, Action onDismiss)
        {
            _notification = notification;
            _onDismiss = onDismiss;
            _startTime = Time.unscaledTime;
            
            if (_titleText != null) _titleText.text = notification.Title;
            if (_messageText != null) _messageText.text = notification.Message;
            if (_iconImage != null && notification.Icon != null) _iconImage.sprite = notification.Icon;
            
            if (_dismissButton != null)
            {
                _dismissButton.gameObject.SetActive(notification.Dismissable);
                _dismissButton.onClick.AddListener(Dismiss);
            }
            
            if (_clickButton != null && notification.OnClick != null)
            {
                _clickButton.onClick.AddListener(() =>
                {
                    notification.OnClick();
                    Dismiss();
                });
            }
            
            StartCoroutine(AnimateIn());
        }
        
        private void Update()
        {
            float elapsed = Time.unscaledTime - _startTime;
            
            // Fade out near end
            if (elapsed > _notification.Duration - 0.5f && _canvasGroup != null)
            {
                float fadeProgress = (elapsed - (_notification.Duration - 0.5f)) / 0.5f;
                _canvasGroup.alpha = 1 - fadeProgress;
            }
            
            // Dismiss after duration
            if (elapsed >= _notification.Duration)
            {
                Dismiss();
            }
        }
        
        private IEnumerator AnimateIn()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0;
                float elapsed = 0;
                
                while (elapsed < 0.3f)
                {
                    elapsed += Time.unscaledDeltaTime;
                    _canvasGroup.alpha = elapsed / 0.3f;
                    yield return null;
                }
                
                _canvasGroup.alpha = 1;
            }
        }
        
        private void Dismiss()
        {
            _onDismiss?.Invoke();
        }
    }
    
    #endregion
}
