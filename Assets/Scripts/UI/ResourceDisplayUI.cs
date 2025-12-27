using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ElectionEmpire.Managers;

namespace ElectionEmpire.UI
{
    /// <summary>
    /// Handles the visual display of player resources using custom UI elements.
    /// Attach to a Canvas and assign the sliced sprite elements.
    /// </summary>
    public class ResourceDisplayUI : MonoBehaviour
    {
        [Header("Approval Rating")]
        [SerializeField] private Image approvalBarFill;
        [SerializeField] private TextMeshProUGUI approvalPercentText;
        [SerializeField] private float approvalFillMax = 1f; // Slider fill amount at 100%

        [Header("Campaign Funds")]
        [SerializeField] private TextMeshProUGUI fundsText;
        [SerializeField] private string fundsFormat = "000,000"; // Format for the counter

        [Header("Trust Meter")]
        [SerializeField] private RectTransform trustNeedle;
        [SerializeField] private float needleMinAngle = 135f;  // Angle when trust is 0 (Low)
        [SerializeField] private float needleMaxAngle = 45f;   // Angle when trust is 100 (High)

        [Header("Animation Settings")]
        [SerializeField] private float updateSpeed = 5f; // How fast values animate
        [SerializeField] private bool animateChanges = true;

        // Current displayed values (for animation)
        private float displayedApproval;
        private float displayedTrust;
        private float displayedFunds;

        // Target values
        private float targetApproval;
        private float targetTrust;
        private float targetFunds;

        private ResourceManager resourceManager;

        private void Start()
        {
            // Find ResourceManager
            resourceManager = ResourceManager.Instance;

            if (resourceManager != null)
            {
                // Subscribe to resource changes
                resourceManager.OnResourceChanged += OnResourceChanged;

                // Initialize with current values
                InitializeValues();
            }
            else
            {
                Debug.LogWarning("[ResourceDisplayUI] ResourceManager not found. Will retry...");
                InvokeRepeating(nameof(TryFindResourceManager), 0.5f, 0.5f);
            }
        }

        private void TryFindResourceManager()
        {
            resourceManager = ResourceManager.Instance;
            if (resourceManager != null)
            {
                CancelInvoke(nameof(TryFindResourceManager));
                resourceManager.OnResourceChanged += OnResourceChanged;
                InitializeValues();
                Debug.Log("[ResourceDisplayUI] ResourceManager found and connected");
            }
        }

        private void InitializeValues()
        {
            if (resourceManager == null) return;

            // Set initial values without animation
            displayedApproval = targetApproval = resourceManager.PublicTrust;
            displayedTrust = targetTrust = resourceManager.PublicTrust;
            displayedFunds = targetFunds = resourceManager.CampaignFunds;

            UpdateAllDisplays();
        }

        private void OnDestroy()
        {
            if (resourceManager != null)
            {
                resourceManager.OnResourceChanged -= OnResourceChanged;
            }
        }

        private void OnResourceChanged(string resourceName, float oldValue, float newValue)
        {
            switch (resourceName)
            {
                case "PublicTrust":
                    targetApproval = newValue;
                    targetTrust = newValue;
                    break;
                case "CampaignFunds":
                    targetFunds = newValue;
                    break;
            }
        }

        private void Update()
        {
            if (animateChanges)
            {
                // Smoothly animate towards target values
                displayedApproval = Mathf.Lerp(displayedApproval, targetApproval, Time.deltaTime * updateSpeed);
                displayedTrust = Mathf.Lerp(displayedTrust, targetTrust, Time.deltaTime * updateSpeed);
                displayedFunds = Mathf.Lerp(displayedFunds, targetFunds, Time.deltaTime * updateSpeed);
            }
            else
            {
                displayedApproval = targetApproval;
                displayedTrust = targetTrust;
                displayedFunds = targetFunds;
            }

            UpdateAllDisplays();
        }

        private void UpdateAllDisplays()
        {
            UpdateApprovalDisplay();
            UpdateFundsDisplay();
            UpdateTrustMeter();
        }

        private void UpdateApprovalDisplay()
        {
            // Update the fill bar
            if (approvalBarFill != null)
            {
                approvalBarFill.fillAmount = (displayedApproval / 100f) * approvalFillMax;
            }

            // Update the percentage text
            if (approvalPercentText != null)
            {
                approvalPercentText.text = $"{Mathf.RoundToInt(displayedApproval)}%";
            }
        }

        private void UpdateFundsDisplay()
        {
            if (fundsText != null)
            {
                // Format as counter display (e.g., "000,075")
                int funds = Mathf.RoundToInt(displayedFunds);
                fundsText.text = funds.ToString("N0");
            }
        }

        private void UpdateTrustMeter()
        {
            if (trustNeedle != null)
            {
                // Calculate needle rotation based on trust (0-100)
                // Trust 0 = needleMinAngle (pointing to Low)
                // Trust 50 = middle (pointing to Neutral)
                // Trust 100 = needleMaxAngle (pointing to High)
                float normalizedTrust = displayedTrust / 100f;
                float angle = Mathf.Lerp(needleMinAngle, needleMaxAngle, normalizedTrust);

                trustNeedle.localRotation = Quaternion.Euler(0, 0, angle);
            }
        }

        /// <summary>
        /// Manually set values (useful for testing or non-ResourceManager scenarios)
        /// </summary>
        public void SetValues(float approval, float funds, float trust)
        {
            targetApproval = approval;
            targetFunds = funds;
            targetTrust = trust;
        }

        /// <summary>
        /// Force immediate update without animation
        /// </summary>
        public void ForceUpdate()
        {
            displayedApproval = targetApproval;
            displayedFunds = targetFunds;
            displayedTrust = targetTrust;
            UpdateAllDisplays();
        }

        #region Editor Testing

        [Header("Editor Testing")]
        [SerializeField] private bool testMode = false;
        [SerializeField, Range(0, 100)] private float testApproval = 70f;
        [SerializeField, Range(0, 1000000)] private float testFunds = 75f;
        [SerializeField, Range(0, 100)] private float testTrust = 50f;

        private void OnValidate()
        {
            if (testMode && Application.isPlaying)
            {
                SetValues(testApproval, testFunds, testTrust);
            }
        }

        #endregion
    }
}
