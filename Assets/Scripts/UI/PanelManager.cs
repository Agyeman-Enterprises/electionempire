using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

namespace ElectionEmpire.UI
{
    /// <summary>
    /// Manages UI panels within a scene. Handles showing/hiding panels with transitions.
    /// Attach to a Canvas or UI root object in each scene.
    /// </summary>
    public class PanelManager : MonoBehaviour
    {
        private static PanelManager _instance;
        public static PanelManager Instance => _instance;
        
        [Header("Panel Configuration")]
        [SerializeField] private List<PanelConfig> panels = new List<PanelConfig>();
        [SerializeField] private string defaultPanelId = "MainMenu";
        
        [Header("Transition Settings")]
        [SerializeField] private float transitionDuration = 0.3f;
        [SerializeField] private TransitionType defaultTransition = TransitionType.Fade;
        [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("Panel Stack (Back Button Support)")]
        [SerializeField] private bool enablePanelStack = true;
        private Stack<string> _panelHistory = new Stack<string>();
        
        private Dictionary<string, PanelConfig> _panelLookup = new Dictionary<string, PanelConfig>();
        private string _currentPanelId;
        private Coroutine _activeTransition;
        
        // Events
        public event System.Action<string, string> OnPanelChanged; // (fromPanel, toPanel)
        public event System.Action<string> OnPanelShown;
        public event System.Action<string> OnPanelHidden;
        
        [System.Serializable]
        public class PanelConfig
        {
            public string panelId;
            public GameObject panelObject;
            public CanvasGroup canvasGroup; // Optional - for fade transitions
            public bool pauseGameWhenActive = false;
            public TransitionType transitionIn = TransitionType.Fade;
            public TransitionType transitionOut = TransitionType.Fade;
            
            [Header("Audio")]
            public AudioClip openSound;
            public AudioClip closeSound;
        }
        
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
        
        private void Awake()
        {
            _instance = this;
            BuildPanelLookup();
            InitializePanels();
        }
        
        private void Start()
        {
            // Show default panel if specified
            if (!string.IsNullOrEmpty(defaultPanelId) && _panelLookup.ContainsKey(defaultPanelId))
            {
                ShowPanel(defaultPanelId, immediate: true);
            }
        }
        
        private void Update()
        {
            // Handle back button / Escape key (using new Input System)
            if (enablePanelStack && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                GoBack();
            }
        }
        
        private void BuildPanelLookup()
        {
            _panelLookup.Clear();
            foreach (var config in panels)
            {
                if (!string.IsNullOrEmpty(config.panelId) && config.panelObject != null)
                {
                    _panelLookup[config.panelId] = config;
                    
                    // Ensure CanvasGroup exists for fade transitions
                    if (config.canvasGroup == null)
                    {
                        config.canvasGroup = config.panelObject.GetComponent<CanvasGroup>();
                        if (config.canvasGroup == null && 
                            (config.transitionIn == TransitionType.Fade || config.transitionOut == TransitionType.Fade))
                        {
                            config.canvasGroup = config.panelObject.AddComponent<CanvasGroup>();
                        }
                    }
                }
            }
        }
        
        private void InitializePanels()
        {
            // Hide all panels initially
            foreach (var config in panels)
            {
                if (config.panelObject != null)
                {
                    config.panelObject.SetActive(false);
                    
                    if (config.canvasGroup != null)
                    {
                        config.canvasGroup.alpha = 0f;
                    }
                }
            }
        }
        
        #region Public API
        
        /// <summary>
        /// Show a panel by ID
        /// </summary>
        public void ShowPanel(string panelId, bool addToHistory = true, bool immediate = false)
        {
            if (!_panelLookup.TryGetValue(panelId, out var config))
            {
                Debug.LogWarning($"[PanelManager] Panel not found: {panelId}");
                return;
            }
            
            // Stop any active transition
            if (_activeTransition != null)
            {
                StopCoroutine(_activeTransition);
            }
            
            // Add current panel to history before switching
            if (enablePanelStack && addToHistory && !string.IsNullOrEmpty(_currentPanelId))
            {
                _panelHistory.Push(_currentPanelId);
            }
            
            string previousPanel = _currentPanelId;
            _currentPanelId = panelId;
            
            if (immediate)
            {
                // Immediate switch (no animation)
                HidePanelImmediate(previousPanel);
                ShowPanelImmediate(panelId);
            }
            else
            {
                _activeTransition = StartCoroutine(TransitionPanels(previousPanel, panelId));
            }
            
            OnPanelChanged?.Invoke(previousPanel, panelId);
        }
        
        /// <summary>
        /// Hide the current panel (shows nothing)
        /// </summary>
        public void HideCurrentPanel(bool immediate = false)
        {
            if (string.IsNullOrEmpty(_currentPanelId))
                return;
            
            if (_activeTransition != null)
            {
                StopCoroutine(_activeTransition);
            }
            
            if (immediate)
            {
                HidePanelImmediate(_currentPanelId);
            }
            else
            {
                _activeTransition = StartCoroutine(HidePanelAnimated(_currentPanelId));
            }
            
            _currentPanelId = null;
        }
        
        /// <summary>
        /// Go back to previous panel (if panel stack is enabled)
        /// </summary>
        public bool GoBack()
        {
            if (!enablePanelStack || _panelHistory.Count == 0)
            {
                return false;
            }
            
            string previousPanel = _panelHistory.Pop();
            ShowPanel(previousPanel, addToHistory: false);
            return true;
        }
        
        /// <summary>
        /// Check if a panel is currently visible
        /// </summary>
        public bool IsPanelVisible(string panelId)
        {
            return _currentPanelId == panelId;
        }
        
        /// <summary>
        /// Get the current panel ID
        /// </summary>
        public string CurrentPanelId => _currentPanelId;
        
        /// <summary>
        /// Clear the panel navigation history
        /// </summary>
        public void ClearHistory()
        {
            _panelHistory.Clear();
        }
        
        /// <summary>
        /// Check if we can go back
        /// </summary>
        public bool CanGoBack => enablePanelStack && _panelHistory.Count > 0;
        
        #endregion
        
        #region Transitions
        
        private IEnumerator TransitionPanels(string fromPanelId, string toPanelId)
        {
            PanelConfig fromConfig = null;
            PanelConfig toConfig = null;
            
            if (!string.IsNullOrEmpty(fromPanelId))
            {
                _panelLookup.TryGetValue(fromPanelId, out fromConfig);
            }
            
            if (!string.IsNullOrEmpty(toPanelId))
            {
                _panelLookup.TryGetValue(toPanelId, out toConfig);
            }
            
            // Start showing the new panel
            if (toConfig != null)
            {
                toConfig.panelObject.SetActive(true);
                
                // Play open sound
                if (toConfig.openSound != null)
                {
                    // AudioManager.Instance?.PlaySFX(toConfig.openSound);
                }
            }
            
            // Animate transition
            float elapsed = 0f;
            
            while (elapsed < transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = transitionCurve.Evaluate(elapsed / transitionDuration);
                
                // Animate out
                if (fromConfig != null)
                {
                    ApplyTransition(fromConfig, fromConfig.transitionOut, 1f - t, false);
                }
                
                // Animate in
                if (toConfig != null)
                {
                    ApplyTransition(toConfig, toConfig.transitionIn, t, true);
                }
                
                yield return null;
            }
            
            // Finalize
            if (fromConfig != null)
            {
                fromConfig.panelObject.SetActive(false);
                
                // Play close sound
                if (fromConfig.closeSound != null)
                {
                    // AudioManager.Instance?.PlaySFX(fromConfig.closeSound);
                }
                
                OnPanelHidden?.Invoke(fromPanelId);
            }
            
            if (toConfig != null)
            {
                // Ensure fully visible
                ApplyTransition(toConfig, toConfig.transitionIn, 1f, true);
                
                // Handle pause
                if (toConfig.pauseGameWhenActive)
                {
                    Time.timeScale = 0f;
                }
                
                OnPanelShown?.Invoke(toPanelId);
            }
            
            _activeTransition = null;
        }
        
        private IEnumerator HidePanelAnimated(string panelId)
        {
            if (!_panelLookup.TryGetValue(panelId, out var config))
            {
                yield break;
            }
            
            float elapsed = 0f;
            
            while (elapsed < transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = transitionCurve.Evaluate(elapsed / transitionDuration);
                ApplyTransition(config, config.transitionOut, 1f - t, false);
                yield return null;
            }
            
            config.panelObject.SetActive(false);
            OnPanelHidden?.Invoke(panelId);
            
            // Restore time scale if this panel paused the game
            if (config.pauseGameWhenActive)
            {
                Time.timeScale = 1f;
            }
            
            _activeTransition = null;
        }
        
        private void ApplyTransition(PanelConfig config, TransitionType type, float t, bool isIn)
        {
            RectTransform rect = config.panelObject.GetComponent<RectTransform>();
            
            switch (type)
            {
                case TransitionType.None:
                    if (config.canvasGroup != null)
                    {
                        config.canvasGroup.alpha = t > 0.5f ? 1f : 0f;
                    }
                    break;
                    
                case TransitionType.Fade:
                    if (config.canvasGroup != null)
                    {
                        config.canvasGroup.alpha = t;
                    }
                    break;
                    
                case TransitionType.SlideLeft:
                    if (rect != null)
                    {
                        float xOffset = isIn ? (1f - t) * Screen.width : t * -Screen.width;
                        rect.anchoredPosition = new Vector2(xOffset, 0);
                    }
                    if (config.canvasGroup != null)
                    {
                        config.canvasGroup.alpha = t;
                    }
                    break;
                    
                case TransitionType.SlideRight:
                    if (rect != null)
                    {
                        float xOffset = isIn ? (1f - t) * -Screen.width : t * Screen.width;
                        rect.anchoredPosition = new Vector2(xOffset, 0);
                    }
                    if (config.canvasGroup != null)
                    {
                        config.canvasGroup.alpha = t;
                    }
                    break;
                    
                case TransitionType.SlideUp:
                    if (rect != null)
                    {
                        float yOffset = isIn ? (1f - t) * -Screen.height : t * Screen.height;
                        rect.anchoredPosition = new Vector2(0, yOffset);
                    }
                    if (config.canvasGroup != null)
                    {
                        config.canvasGroup.alpha = t;
                    }
                    break;
                    
                case TransitionType.SlideDown:
                    if (rect != null)
                    {
                        float yOffset = isIn ? (1f - t) * Screen.height : t * -Screen.height;
                        rect.anchoredPosition = new Vector2(0, yOffset);
                    }
                    if (config.canvasGroup != null)
                    {
                        config.canvasGroup.alpha = t;
                    }
                    break;
                    
                case TransitionType.Scale:
                    config.panelObject.transform.localScale = Vector3.one * t;
                    if (config.canvasGroup != null)
                    {
                        config.canvasGroup.alpha = t;
                    }
                    break;
            }
        }
        
        private void ShowPanelImmediate(string panelId)
        {
            if (_panelLookup.TryGetValue(panelId, out var config))
            {
                config.panelObject.SetActive(true);
                
                if (config.canvasGroup != null)
                {
                    config.canvasGroup.alpha = 1f;
                }
                
                config.panelObject.transform.localScale = Vector3.one;
                
                var rect = config.panelObject.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchoredPosition = Vector2.zero;
                }
                
                if (config.pauseGameWhenActive)
                {
                    Time.timeScale = 0f;
                }
                
                OnPanelShown?.Invoke(panelId);
            }
        }
        
        private void HidePanelImmediate(string panelId)
        {
            if (!string.IsNullOrEmpty(panelId) && _panelLookup.TryGetValue(panelId, out var config))
            {
                config.panelObject.SetActive(false);
                
                if (config.pauseGameWhenActive)
                {
                    Time.timeScale = 1f;
                }
                
                OnPanelHidden?.Invoke(panelId);
            }
        }
        
        #endregion
        
        #region Editor Helpers
        
        /// <summary>
        /// Auto-find panels in children (call from editor script or context menu)
        /// </summary>
        [ContextMenu("Auto-Find Panels")]
        public void AutoFindPanels()
        {
            panels.Clear();
            
            // Find all direct children with "Panel" in the name
            foreach (Transform child in transform)
            {
                if (child.name.Contains("Panel") || child.GetComponent<CanvasGroup>() != null)
                {
                    panels.Add(new PanelConfig
                    {
                        panelId = child.name.Replace("Panel", "").Trim(),
                        panelObject = child.gameObject,
                        canvasGroup = child.GetComponent<CanvasGroup>(),
                        transitionIn = TransitionType.Fade,
                        transitionOut = TransitionType.Fade
                    });
                }
            }
            
            Debug.Log($"[PanelManager] Found {panels.Count} panels");
        }
        
        #endregion
    }
}
