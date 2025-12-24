using System;
using System.Collections.Generic;
using UnityEngine;
using ElectionEmpire.UI.Screens;

namespace ElectionEmpire.UI
{
    /// <summary>
    /// Screen types for navigation.
    /// </summary>
    public enum ScreenType
    {
        MainMenu,
        NewGame,
        LoadGame,
        MultiplayerLobby,
        Settings,
        Credits,
        GameHUD,
        CharacterCreation,
        PauseMenu,
        EventPopup,
        CrisisManagement,
        ScandalResponse,
        ElectionNight,
        VictoryScreen,
        DefeatScreen,
        DebateArena
    }
    
    /// <summary>
    /// Notification types for UI alerts.
    /// </summary>
    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error
    }
    
    /// <summary>
    /// Central UI manager for screen navigation and notifications.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        private static UIManager _instance;
        public static UIManager Instance => _instance;
        
        [Header("Screen Prefabs")]
        [SerializeField] private GameObject _mainMenuPrefab;
        [SerializeField] private GameObject _newGamePrefab;
        [SerializeField] private GameObject _loadGamePrefab;
        [SerializeField] private GameObject _settingsPrefab;
        
        [Header("Notification")]
        [SerializeField] private GameObject _notificationPrefab;
        [SerializeField] private Transform _notificationContainer;
        
        private Dictionary<ScreenType, BaseScreen> _screens = new Dictionary<ScreenType, BaseScreen>();
        private Stack<ScreenType> _screenHistory = new Stack<ScreenType>();
        private ScreenType _currentScreen;
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        public void NavigateToScreen(ScreenType screenType, object data = null)
        {
            if (_currentScreen != ScreenType.MainMenu)
            {
                _screenHistory.Push(_currentScreen);
            }
            
            ShowScreen(screenType, data);
        }
        
        public void NavigateBack()
        {
            if (_screenHistory.Count > 0)
            {
                var previousScreen = _screenHistory.Pop();
                ShowScreen(previousScreen);
            }
            else
            {
                ShowScreen(ScreenType.MainMenu);
            }
        }
        
        private void ShowScreen(ScreenType screenType, object data = null)
        {
            // Hide current screen
            if (_screens.TryGetValue(_currentScreen, out var current))
            {
                if (current != null && current.CanNavigateBack())
                {
                    current.OnScreenExit();
                }
            }
            
            // Show new screen
            if (!_screens.TryGetValue(screenType, out var screen))
            {
                screen = CreateScreen(screenType);
                if (screen != null)
                {
                    _screens[screenType] = screen;
                }
            }
            
            if (screen != null)
            {
                screen.OnScreenEnter(data);
                _currentScreen = screenType;
            }
        }
        
        private BaseScreen CreateScreen(ScreenType screenType)
        {
            GameObject prefab = screenType switch
            {
                ScreenType.MainMenu => _mainMenuPrefab,
                ScreenType.NewGame => _newGamePrefab,
                ScreenType.LoadGame => _loadGamePrefab,
                ScreenType.Settings => _settingsPrefab,
                _ => null
            };
            
            if (prefab != null)
            {
                var go = Instantiate(prefab, transform);
                return go.GetComponent<BaseScreen>();
            }
            
            return null;
        }
        
        public static void ShowAlert(string title, string message)
        {
            if (_instance != null)
            {
                _instance.ShowAlertInternal(title, message);
            }
            else
            {
                Debug.LogWarning($"[UI] {title}: {message}");
            }
        }
        
        public static void ShowConfirmation(string title, string message, Action onConfirm)
        {
            if (_instance != null)
            {
                _instance.ShowConfirmationInternal(title, message, onConfirm);
            }
            else
            {
                Debug.LogWarning($"[UI] {title}: {message}");
                onConfirm?.Invoke();
            }
        }
        
        public static void ShowToast(string title, string message, NotificationType type = NotificationType.Info)
        {
            if (_instance != null)
            {
                _instance.ShowToastInternal(title, message, type);
            }
            else
            {
                Debug.Log($"[UI] {title}: {message}");
            }
        }
        
        private void ShowAlertInternal(string title, string message)
        {
            // Would show alert dialog
            Debug.LogWarning($"[UI Alert] {title}: {message}");
        }
        
        private void ShowConfirmationInternal(string title, string message, Action onConfirm)
        {
            // Would show confirmation dialog
            Debug.Log($"[UI Confirm] {title}: {message}");
            onConfirm?.Invoke();
        }
        
        private void ShowToastInternal(string title, string message, NotificationType type)
        {
            if (_notificationPrefab != null && _notificationContainer != null)
            {
                var go = Instantiate(_notificationPrefab, _notificationContainer);
                // Would setup notification UI
                Destroy(go, 3f);
            }
            Debug.Log($"[UI Toast] {title}: {message}");
        }
    }
}

