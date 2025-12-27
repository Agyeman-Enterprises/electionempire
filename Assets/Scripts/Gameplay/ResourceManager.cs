using UnityEngine;
using System;

namespace ElectionEmpire.Managers
{
    /// <summary>
    /// Manages all player resources: Public Trust, Political Capital, Campaign Funds, etc.
    /// This is a core manager that other systems depend on.
    /// </summary>
    public class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance { get; private set; }
        
        // ===== CORE RESOURCES =====
        [Header("Core Resources")]
        [SerializeField, Range(0, 100)] private float publicTrust = 50f;
        [SerializeField, Range(0, 150)] private float politicalCapital = 30f;
        [SerializeField] private float campaignFunds = 10000f;
        [SerializeField, Range(0, 100)] private float mediaInfluence = 25f;
        [SerializeField, Range(0, 100)] private float partyLoyalty = 50f;
        
        // ===== SECONDARY RESOURCES =====
        [Header("Secondary Resources")]
        [SerializeField, Range(1, 10)] private int staffQuality = 5;
        [SerializeField] private int legacyPoints = 0;
        [SerializeField] private int dirtCount = 0;
        
        // ===== IN-GAME CURRENCIES =====
        [Header("Currencies")]
        [SerializeField] private int cloutBux = 0;
        [SerializeField] private int purrkoin = 0;
        
        // ===== EVENTS =====
        public event Action<string, float, float> OnResourceChanged; // resourceName, oldValue, newValue
        public event Action OnFundsExhausted;
        public event Action OnTrustCritical;
        
        // ===== PROPERTIES =====
        public float PublicTrust => publicTrust;
        public float PoliticalCapital => politicalCapital;
        public float CampaignFunds => campaignFunds;
        public float MediaInfluence => mediaInfluence;
        public float PartyLoyalty => partyLoyalty;
        public int StaffQuality => staffQuality;
        public int LegacyPoints => legacyPoints;
        public int CloutBux => cloutBux;
        public int Purrkoin => purrkoin;
        
        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[ResourceManager] Duplicate instance destroyed");
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            Debug.Log("[ResourceManager] Initialized");
            Debug.Log($"  - Public Trust: {publicTrust}%");
            Debug.Log($"  - Political Capital: {politicalCapital}");
            Debug.Log($"  - Campaign Funds: ${campaignFunds:N0}");
            Debug.Log($"  - Media Influence: {mediaInfluence}");
            Debug.Log($"  - Party Loyalty: {partyLoyalty}%");
        }
        
        private void Start()
        {
            Debug.Log("[ResourceManager] Ready for game events");
        }
        
        // ===== PUBLIC METHODS =====
        
        /// <summary>
        /// Modify public trust (clamped 0-100)
        /// </summary>
        public void ModifyPublicTrust(float amount, string reason = "")
        {
            float oldValue = publicTrust;
            publicTrust = Mathf.Clamp(publicTrust + amount, 0f, 100f);
            
            string sign = amount >= 0 ? "+" : "";
            Debug.Log($"[ResourceManager] Public Trust {sign}{amount}% ({reason}) | {oldValue:F1}% → {publicTrust:F1}%");
            
            OnResourceChanged?.Invoke("PublicTrust", oldValue, publicTrust);
            
            if (publicTrust <= 20f)
            {
                Debug.LogWarning("[ResourceManager] PUBLIC TRUST CRITICAL!");
                OnTrustCritical?.Invoke();
            }
        }
        
        /// <summary>
        /// Modify political capital (clamped 0-cap based on office tier)
        /// </summary>
        public void ModifyPoliticalCapital(float amount, string reason = "")
        {
            float oldValue = politicalCapital;
            politicalCapital = Mathf.Clamp(politicalCapital + amount, 0f, 150f);
            
            string sign = amount >= 0 ? "+" : "";
            Debug.Log($"[ResourceManager] Political Capital {sign}{amount} ({reason}) | {oldValue:F1} → {politicalCapital:F1}");
            
            OnResourceChanged?.Invoke("PoliticalCapital", oldValue, politicalCapital);
        }
        
        /// <summary>
        /// Modify campaign funds (no upper cap, can go negative = debt)
        /// </summary>
        public void ModifyCampaignFunds(float amount, string reason = "")
        {
            float oldValue = campaignFunds;
            campaignFunds += amount;
            
            string sign = amount >= 0 ? "+" : "";
            Debug.Log($"[ResourceManager] Campaign Funds {sign}${amount:N0} ({reason}) | ${oldValue:N0} → ${campaignFunds:N0}");
            
            OnResourceChanged?.Invoke("CampaignFunds", oldValue, campaignFunds);
            
            if (campaignFunds <= 0)
            {
                Debug.LogWarning("[ResourceManager] CAMPAIGN FUNDS EXHAUSTED!");
                OnFundsExhausted?.Invoke();
            }
        }
        
        /// <summary>
        /// Modify media influence (clamped 0-100)
        /// </summary>
        public void ModifyMediaInfluence(float amount, string reason = "")
        {
            float oldValue = mediaInfluence;
            mediaInfluence = Mathf.Clamp(mediaInfluence + amount, 0f, 100f);
            
            string sign = amount >= 0 ? "+" : "";
            Debug.Log($"[ResourceManager] Media Influence {sign}{amount} ({reason}) | {oldValue:F1} → {mediaInfluence:F1}");
            
            OnResourceChanged?.Invoke("MediaInfluence", oldValue, mediaInfluence);
        }
        
        /// <summary>
        /// Modify party loyalty (clamped 0-100)
        /// </summary>
        public void ModifyPartyLoyalty(float amount, string reason = "")
        {
            float oldValue = partyLoyalty;
            partyLoyalty = Mathf.Clamp(partyLoyalty + amount, 0f, 100f);
            
            string sign = amount >= 0 ? "+" : "";
            Debug.Log($"[ResourceManager] Party Loyalty {sign}{amount}% ({reason}) | {oldValue:F1}% → {partyLoyalty:F1}%");
            
            OnResourceChanged?.Invoke("PartyLoyalty", oldValue, partyLoyalty);
        }
        
        /// <summary>
        /// Add CloutBux (in-game currency)
        /// </summary>
        public void AddCloutBux(int amount, string reason = "")
        {
            cloutBux += amount;
            Debug.Log($"[ResourceManager] CloutBux +{amount} ({reason}) | Total: {cloutBux}");
        }
        
        /// <summary>
        /// Spend CloutBux - returns false if insufficient
        /// </summary>
        public bool SpendCloutBux(int amount, string reason = "")
        {
            if (cloutBux >= amount)
            {
                cloutBux -= amount;
                Debug.Log($"[ResourceManager] CloutBux -{amount} ({reason}) | Remaining: {cloutBux}");
                return true;
            }
            Debug.LogWarning($"[ResourceManager] Cannot spend {amount} CloutBux - only have {cloutBux}");
            return false;
        }
        
        /// <summary>
        /// Check if player can afford an action
        /// </summary>
        public bool CanAfford(float fundsCost = 0, float capitalCost = 0)
        {
            return campaignFunds >= fundsCost && politicalCapital >= capitalCost;
        }
        
        /// <summary>
        /// Apply end-of-turn decay to resources
        /// </summary>
        public void ApplyTurnDecay()
        {
            Debug.Log("[ResourceManager] Applying end-of-turn decay...");
            
            // Political capital decays 5% per turn
            ModifyPoliticalCapital(-politicalCapital * 0.05f, "Natural decay");
            
            // Media influence decays 10% per turn
            ModifyMediaInfluence(-mediaInfluence * 0.10f, "News cycle");
            
            // Campaign burn rate (varies by tier, placeholder 5%)
            ModifyCampaignFunds(-campaignFunds * 0.05f, "Campaign operations");
        }
        
        /// <summary>
        /// Get a summary of all resources for UI display
        /// </summary>
        public string GetResourceSummary()
        {
            return $"Trust: {publicTrust:F0}% | Capital: {politicalCapital:F0} | Funds: ${campaignFunds:N0}\n" +
                   $"Media: {mediaInfluence:F0} | Party: {partyLoyalty:F0}% | CloutBux: {cloutBux}";
        }
    }
}
