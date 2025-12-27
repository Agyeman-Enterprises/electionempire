using UnityEngine;
using UnityEngine.UI;
using ElectionEmpire.Managers;

namespace ElectionEmpire.UI
{
    /// <summary>
    /// Controls the main game HUD display.
    /// Shows resources, turn info, and provides basic interaction.
    /// </summary>
    public class GameHUDController : MonoBehaviour
    {
        [Header("Resource Display")]
        [SerializeField] private Text trustText;
        [SerializeField] private Text capitalText;
        [SerializeField] private Text fundsText;
        [SerializeField] private Text mediaText;
        [SerializeField] private Text partyText;
        
        [Header("Time Display")]
        [SerializeField] private Text dateText;
        [SerializeField] private Text turnText;
        [SerializeField] private Text phaseText;
        [SerializeField] private Text electionCountdownText;
        
        [Header("Action Buttons")]
        [SerializeField] private Button endTurnButton;
        [SerializeField] private Button campaignButton;
        [SerializeField] private Button policyButton;
        [SerializeField] private Button fundraiseButton;
        
        [Header("Status")]
        [SerializeField] private Text statusText;
        
        private void Awake()
        {
            Debug.Log("[GameHUDController] Initializing HUD...");
        }
        
        private void Start()
        {
            SetupUI();
            SubscribeToEvents();
            RefreshAllDisplays();
            
            Debug.Log("[GameHUDController] HUD Ready");
        }
        
        private void SetupUI()
        {
            // If we don't have UI elements assigned, create a simple text display
            if (statusText == null)
            {
                CreateSimpleStatusDisplay();
            }
            
            // Wire up buttons if they exist
            if (endTurnButton != null)
                endTurnButton.onClick.AddListener(OnEndTurnClicked);
            if (campaignButton != null)
                campaignButton.onClick.AddListener(OnCampaignClicked);
            if (policyButton != null)
                policyButton.onClick.AddListener(OnPolicyClicked);
            if (fundraiseButton != null)
                fundraiseButton.onClick.AddListener(OnFundraiseClicked);
        }
        
        private void CreateSimpleStatusDisplay()
        {
            // Create a simple status text in the corner
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = FindObjectOfType<Canvas>();
            }
            
            if (canvas == null)
            {
                Debug.LogError("[GameHUDController] No Canvas found!");
                return;
            }
            
            // Create status panel
            GameObject panel = new GameObject("StatusPanel");
            panel.transform.SetParent(canvas.transform, false);
            
            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.2f, 0.9f);
            
            RectTransform panelRt = panel.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0, 0);
            panelRt.anchorMax = new Vector2(0.35f, 0.4f);
            panelRt.offsetMin = new Vector2(20, 20);
            panelRt.offsetMax = new Vector2(-10, -10);
            
            // Create text
            GameObject textObj = new GameObject("StatusText");
            textObj.transform.SetParent(panel.transform, false);
            
            statusText = textObj.AddComponent<Text>();
            statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            statusText.fontSize = 16;
            statusText.color = Color.white;
            statusText.alignment = TextAnchor.UpperLeft;
            
            RectTransform textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(15, 15);
            textRt.offsetMax = new Vector2(-15, -15);
            
            // Create action buttons panel
            CreateActionButtons(canvas);
            
            Debug.Log("[GameHUDController] Created simple status display");
        }
        
        private void CreateActionButtons(Canvas canvas)
        {
            // Create button panel on the right
            GameObject buttonPanel = new GameObject("ActionPanel");
            buttonPanel.transform.SetParent(canvas.transform, false);
            
            Image bg = buttonPanel.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.25f, 0.9f);
            
            RectTransform panelRt = buttonPanel.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(1, 0);
            panelRt.anchorMax = new Vector2(1, 0.5f);
            panelRt.pivot = new Vector2(1, 0);
            panelRt.sizeDelta = new Vector2(200, 250);
            panelRt.anchoredPosition = new Vector2(-20, 20);
            
            // Create buttons
            string[] buttonNames = { "Campaign", "Policy", "Fundraise", "End Turn" };
            System.Action[] buttonActions = { OnCampaignClicked, OnPolicyClicked, OnFundraiseClicked, OnEndTurnClicked };
            
            for (int i = 0; i < buttonNames.Length; i++)
            {
                CreateButton(buttonPanel.transform, buttonNames[i], i, buttonActions[i]);
            }
        }
        
        private void CreateButton(Transform parent, string label, int index, System.Action onClick)
        {
            GameObject buttonObj = new GameObject(label + "Button");
            buttonObj.transform.SetParent(parent, false);
            
            Image buttonBg = buttonObj.AddComponent<Image>();
            buttonBg.color = new Color(0.3f, 0.5f, 0.7f, 1f);
            
            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = buttonBg;
            
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.4f, 0.6f, 0.8f, 1f);
            colors.pressedColor = new Color(0.2f, 0.4f, 0.6f, 1f);
            button.colors = colors;
            
            button.onClick.AddListener(() => onClick());
            
            RectTransform buttonRt = buttonObj.GetComponent<RectTransform>();
            buttonRt.anchorMin = new Vector2(0.1f, 0);
            buttonRt.anchorMax = new Vector2(0.9f, 0);
            buttonRt.pivot = new Vector2(0.5f, 0);
            buttonRt.sizeDelta = new Vector2(0, 45);
            buttonRt.anchoredPosition = new Vector2(0, 15 + (index * 55));
            
            // Add text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            Text buttonText = textObj.AddComponent<Text>();
            buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            buttonText.text = label;
            buttonText.fontSize = 18;
            buttonText.color = Color.white;
            buttonText.alignment = TextAnchor.MiddleCenter;
            
            RectTransform textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;
        }
        
        private void SubscribeToEvents()
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnResourceChanged += OnResourceChanged;
            }
            
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnTurnStart += RefreshAllDisplays;
                TimeManager.Instance.OnPhaseChanged += (_) => RefreshAllDisplays();
            }
        }
        
        private void OnResourceChanged(string resourceName, float oldValue, float newValue)
        {
            RefreshResourceDisplay();
        }
        
        private void RefreshAllDisplays()
        {
            RefreshResourceDisplay();
            RefreshTimeDisplay();
        }
        
        private void RefreshResourceDisplay()
        {
            if (statusText == null) return;
            
            string resourceInfo = "=== RESOURCES ===\n";
            
            if (ResourceManager.Instance != null)
            {
                resourceInfo += ResourceManager.Instance.GetResourceSummary();
            }
            else
            {
                resourceInfo += "(ResourceManager not found)";
            }
            
            resourceInfo += "\n\n=== STATUS ===\n";
            
            if (TimeManager.Instance != null)
            {
                resourceInfo += TimeManager.Instance.GetStatusSummary();
            }
            else
            {
                resourceInfo += "(TimeManager not found)";
            }
            
            resourceInfo += "\n\n=== CONTROLS ===\n";
            resourceInfo += "Use buttons on right →\n";
            resourceInfo += "F1: Toggle debug\n";
            resourceInfo += "ESC: Menu (TBD)";
            
            statusText.text = resourceInfo;
        }
        
        private void RefreshTimeDisplay()
        {
            // Individual text elements if assigned
            if (TimeManager.Instance == null) return;
            
            if (dateText != null)
                dateText.text = TimeManager.Instance.CurrentDateString;
            if (turnText != null)
                turnText.text = $"Turn {TimeManager.Instance.CurrentTurn}";
            if (phaseText != null)
                phaseText.text = TimeManager.Instance.GetPhaseDescription();
            if (electionCountdownText != null)
                electionCountdownText.text = $"Election in {TimeManager.Instance.TurnsUntilElection} months";
        }
        
        // ===== BUTTON HANDLERS =====
        
        private void OnCampaignClicked()
        {
            Debug.Log("[HUD] Campaign button clicked");
            
            if (TimeManager.Instance != null && TimeManager.Instance.UseAction("Campaign"))
            {
                // Spend some funds, gain some trust
                if (ResourceManager.Instance != null)
                {
                    ResourceManager.Instance.ModifyCampaignFunds(-500, "Campaign event");
                    ResourceManager.Instance.ModifyPublicTrust(2f, "Campaign event");
                }
            }
            
            RefreshAllDisplays();
        }
        
        private void OnPolicyClicked()
        {
            Debug.Log("[HUD] Policy button clicked");
            
            if (TimeManager.Instance != null && TimeManager.Instance.UseAction("Policy"))
            {
                // Spend political capital, various effects
                if (ResourceManager.Instance != null)
                {
                    ResourceManager.Instance.ModifyPoliticalCapital(-5, "Policy push");
                    ResourceManager.Instance.ModifyPublicTrust(1f, "Policy announcement");
                    ResourceManager.Instance.ModifyPartyLoyalty(-2f, "Independent stance");
                }
            }
            
            RefreshAllDisplays();
        }
        
        private void OnFundraiseClicked()
        {
            Debug.Log("[HUD] Fundraise button clicked");
            
            if (TimeManager.Instance != null && TimeManager.Instance.UseAction("Fundraise"))
            {
                // Gain funds, possible loyalty cost
                if (ResourceManager.Instance != null)
                {
                    ResourceManager.Instance.ModifyCampaignFunds(2500, "Fundraising event");
                    ResourceManager.Instance.ModifyPartyLoyalty(1f, "Donor alignment");
                }
            }
            
            RefreshAllDisplays();
        }
        
        private void OnEndTurnClicked()
        {
            Debug.Log("[HUD] End Turn button clicked");
            
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.EndTurn();
            }
            
            RefreshAllDisplays();
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnResourceChanged -= OnResourceChanged;
            }
        }
    }
}
