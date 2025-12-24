// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - Dashboard Screens
// Sprint 9: Campaign, Policy, Staff, and Media dashboards
// ═══════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ElectionEmpire.Core;

namespace ElectionEmpire.UI.Screens
{
    #region Data Classes
    
    /// <summary>
    /// Voter bloc display data.
    /// </summary>
    [Serializable]
    public class VoterBlocData
    {
        public string BlocId;
        public string BlocName;
        public float SupportPercentage;
        public float PopulationPercentage;
        public float Trend;
        public Color BlocColor;
        public Sprite Icon;
        public List<string> KeyIssues;
    }
    
    /// <summary>
    /// Polling data for display.
    /// </summary>
    [Serializable]
    public class PollingData
    {
        public string CandidateId;
        public string CandidateName;
        public float Percentage;
        public float PreviousPercentage;
        public Color PartyColor;
        public bool IsPlayer;
    }
    
    /// <summary>
    /// Campaign action option.
    /// </summary>
    [Serializable]
    public class CampaignAction
    {
        public string ActionId;
        public string ActionName;
        public string Description;
        public Sprite Icon;
        public int Cost;
        public float SuccessChance;
        public List<string> Requirements;
        public bool IsAvailable;
        public string CooldownRemaining;
    }
    
    /// <summary>
    /// Policy option data.
    /// </summary>
    [Serializable]
    public class PolicyData
    {
        public string PolicyId;
        public string PolicyName;
        public string Description;
        public PolicyCategory Category;
        public int PoliticalCapitalCost;
        public float TrustImpact;
        public Dictionary<string, float> BlocImpacts;
        public bool IsImplemented;
        public bool IsAvailable;
        public List<string> Prerequisites;
    }
    
    public enum PolicyCategory
    {
        Economic,
        Social,
        Security,
        Environmental,
        Education,
        Healthcare,
        Foreign
    }
    
    /// <summary>
    /// Staff member data.
    /// </summary>
    [Serializable]
    public class StaffMemberData
    {
        public string StaffId;
        public string Name;
        public StaffRole Role;
        public int Quality;
        public int Loyalty;
        public int Salary;
        public Sprite Portrait;
        public List<string> Traits;
        public List<string> Specializations;
    }
    
    public enum StaffRole
    {
        CampaignManager,
        PressSecretary,
        PolicyAdvisor,
        Fundraiser,
        Strategist,
        OppositionResearcher,
        SocialMediaManager,
        Fixer
    }
    
    /// <summary>
    /// Media outlet data.
    /// </summary>
    [Serializable]
    public class MediaOutletData
    {
        public string OutletId;
        public string OutletName;
        public MediaType Type;
        public int BiasScore; // -100 to +100
        public int Reach;
        public int RelationshipScore;
        public string CurrentCoverage;
        public Sprite Logo;
    }
    
    public enum MediaType
    {
        Mainstream,
        Partisan,
        Local,
        Digital,
        Social
    }
    
    #endregion
    
    /// <summary>
    /// Campaign Dashboard screen with polling, voter blocs, and campaign actions.
    /// </summary>
    public class CampaignDashboard : BaseScreen
    {
        #region Serialized Fields
        
        [Header("Polling Section")]
        [SerializeField] private Transform _pollingContainer;
        [SerializeField] private GameObject _pollingBarPrefab;
        [SerializeField] private Text _leadText;
        [SerializeField] private Text _marginOfErrorText;
        [SerializeField] private LineRenderer _pollingTrendLine;
        
        [Header("Voter Blocs Section")]
        [SerializeField] private Transform _voterBlocContainer;
        [SerializeField] private GameObject _voterBlocPrefab;
        [SerializeField] private Toggle _voterBlocDetailToggle;
        
        [Header("Campaign Actions Section")]
        [SerializeField] private Transform _actionContainer;
        [SerializeField] private GameObject _actionButtonPrefab;
        [SerializeField] private Text _actionsRemainingText;
        [SerializeField] private Image _actionPointsFill;
        
        [Header("Campaign Fund Display")]
        [SerializeField] private Text _fundsText;
        [SerializeField] private Text _burnRateText;
        [SerializeField] private Text _daysOfFundingText;
        [SerializeField] private Button _fundraiseButton;
        
        [Header("Key Stats")]
        [SerializeField] private Text _approvalText;
        [SerializeField] private Text _approvalTrendText;
        [SerializeField] private Text _daysToElectionText;
        [SerializeField] private Text _volunteersText;
        
        [Header("Navigation")]
        [SerializeField] private Button _policyButton;
        [SerializeField] private Button _staffButton;
        [SerializeField] private Button _mediaButton;
        [SerializeField] private Button _backButton;
        
        #endregion
        
        #region Private Fields
        
        private List<PollingData> _pollingData = new List<PollingData>();
        private List<VoterBlocData> _voterBlocs = new List<VoterBlocData>();
        private List<CampaignAction> _actions = new List<CampaignAction>();
        private int _actionsRemaining;
        private int _maxActions;
        
        #endregion
        
        #region Events
        
        public event Action<string> OnActionSelected;
        public event Action OnFundraiseClicked;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            SetupButtons();
        }
        
        #endregion
        
        #region Screen Lifecycle
        
        public override void OnScreenEnter(object data)
        {
            base.OnScreenEnter(data);
            
            if (data is CampaignDashboardData dashboardData)
            {
                LoadData(dashboardData);
            }
            
            RefreshDisplay();
        }
        
        #endregion
        
        #region Setup
        
        private void SetupButtons()
        {
            if (_policyButton != null)
                _policyButton.onClick.AddListener(() => NavigateTo(ScreenType.PolicyMenu));
            
            if (_staffButton != null)
                _staffButton.onClick.AddListener(() => NavigateTo(ScreenType.StaffManagement));
            
            if (_mediaButton != null)
                _mediaButton.onClick.AddListener(() => NavigateTo(ScreenType.MediaCenter));
            
            if (_backButton != null)
                _backButton.onClick.AddListener(NavigateBack);
            
            if (_fundraiseButton != null)
                _fundraiseButton.onClick.AddListener(() => OnFundraiseClicked?.Invoke());
        }
        
        private void LoadData(CampaignDashboardData data)
        {
            _pollingData = data.Polling;
            _voterBlocs = data.VoterBlocs;
            _actions = data.AvailableActions;
            _actionsRemaining = data.ActionsRemaining;
            _maxActions = data.MaxActions;
        }
        
        #endregion
        
        #region Display Updates
        
        private void RefreshDisplay()
        {
            UpdatePollingDisplay();
            UpdateVoterBlocDisplay();
            UpdateActionsDisplay();
            UpdateFundsDisplay();
        }
        
        private void UpdatePollingDisplay()
        {
            // Clear existing
            if (_pollingContainer != null)
            {
                foreach (Transform child in _pollingContainer)
                {
                    Destroy(child.gameObject);
                }
            }
            
            // Sort by percentage
            var sorted = _pollingData.OrderByDescending(p => p.Percentage).ToList();
            
            foreach (var poll in sorted)
            {
                if (_pollingBarPrefab != null && _pollingContainer != null)
                {
                    var go = Instantiate(_pollingBarPrefab, _pollingContainer);
                    var bar = go.GetComponent<PollingBarUI>();
                    if (bar != null)
                    {
                        bar.Setup(poll);
                    }
                }
            }
            
            // Update lead text
            if (_leadText != null && sorted.Count >= 2)
            {
                var leader = sorted[0];
                var second = sorted[1];
                float margin = leader.Percentage - second.Percentage;
                
                if (leader.IsPlayer)
                {
                    _leadText.text = $"You lead by {margin:F1}%";
                    _leadText.color = Color.green;
                }
                else
                {
                    _leadText.text = $"You trail by {margin:F1}%";
                    _leadText.color = Color.red;
                }
            }
        }
        
        private void UpdateVoterBlocDisplay()
        {
            if (_voterBlocContainer == null) return;
            
            foreach (Transform child in _voterBlocContainer)
            {
                Destroy(child.gameObject);
            }
            
            foreach (var bloc in _voterBlocs.OrderByDescending(b => b.PopulationPercentage))
            {
                if (_voterBlocPrefab != null)
                {
                    var go = Instantiate(_voterBlocPrefab, _voterBlocContainer);
                    var ui = go.GetComponent<VoterBlocUI>();
                    if (ui != null)
                    {
                        ui.Setup(bloc);
                    }
                }
            }
        }
        
        private void UpdateActionsDisplay()
        {
            if (_actionContainer == null) return;
            
            foreach (Transform child in _actionContainer)
            {
                Destroy(child.gameObject);
            }
            
            foreach (var action in _actions.Where(a => a.IsAvailable))
            {
                if (_actionButtonPrefab != null)
                {
                    var go = Instantiate(_actionButtonPrefab, _actionContainer);
                    var button = go.GetComponent<CampaignActionButton>();
                    if (button != null)
                    {
                        button.Setup(action, () =>
                        {
                            OnActionSelected?.Invoke(action.ActionId);
                        });
                    }
                }
            }
            
            if (_actionsRemainingText != null)
            {
                _actionsRemainingText.text = $"{_actionsRemaining}/{_maxActions} Actions";
            }
            
            if (_actionPointsFill != null)
            {
                _actionPointsFill.fillAmount = (float)_actionsRemaining / _maxActions;
            }
        }
        
        private void UpdateFundsDisplay()
        {
            // Would be populated from game state
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Update polling data and refresh display.
        /// </summary>
        public void UpdatePolling(List<PollingData> polling)
        {
            _pollingData = polling;
            UpdatePollingDisplay();
        }
        
        /// <summary>
        /// Update voter bloc data and refresh display.
        /// </summary>
        public void UpdateVoterBlocs(List<VoterBlocData> blocs)
        {
            _voterBlocs = blocs;
            UpdateVoterBlocDisplay();
        }
        
        /// <summary>
        /// Update campaign funds display.
        /// </summary>
        public void UpdateFunds(float currentFunds, float burnRate, int daysRemaining)
        {
            if (_fundsText != null)
            {
                _fundsText.text = FormatMoney(currentFunds);
            }
            
            if (_burnRateText != null)
            {
                _burnRateText.text = $"-{FormatMoney(burnRate)}/day";
            }
            
            if (_daysOfFundingText != null)
            {
                _daysOfFundingText.text = $"{daysRemaining} days of funding";
                _daysOfFundingText.color = daysRemaining < 30 ? Color.red : Color.white;
            }
        }
        
        private string FormatMoney(float amount)
        {
            if (amount >= 1000000)
                return $"${amount / 1000000:F2}M";
            if (amount >= 1000)
                return $"${amount / 1000:F1}K";
            return $"${amount:F0}";
        }
        
        #endregion
    }
    
    /// <summary>
    /// Policy Menu screen for viewing and implementing policies.
    /// </summary>
    public class PolicyMenu : BaseScreen
    {
        #region Serialized Fields
        
        [Header("Category Tabs")]
        [SerializeField] private Transform _tabContainer;
        [SerializeField] private GameObject _tabPrefab;
        
        [Header("Policy List")]
        [SerializeField] private Transform _policyListContainer;
        [SerializeField] private GameObject _policyItemPrefab;
        [SerializeField] private ScrollRect _policyScrollRect;
        
        [Header("Policy Details Panel")]
        [SerializeField] private GameObject _detailsPanel;
        [SerializeField] private Text _policyNameText;
        [SerializeField] private Text _policyDescriptionText;
        [SerializeField] private Text _capitalCostText;
        [SerializeField] private Text _trustImpactText;
        [SerializeField] private Transform _blocImpactContainer;
        [SerializeField] private GameObject _blocImpactPrefab;
        [SerializeField] private Button _implementButton;
        [SerializeField] private Text _implementButtonText;
        
        [Header("Resources Display")]
        [SerializeField] private Text _politicalCapitalText;
        [SerializeField] private Image _capitalFill;
        
        [Header("Navigation")]
        [SerializeField] private Button _backButton;
        
        #endregion
        
        #region Private Fields
        
        private Dictionary<PolicyCategory, List<PolicyData>> _policiesByCategory;
        private PolicyCategory _currentCategory;
        private PolicyData _selectedPolicy;
        private int _currentPoliticalCapital;
        private int _maxPoliticalCapital;
        
        #endregion
        
        #region Events
        
        public event Action<string> OnPolicyImplemented;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            _policiesByCategory = new Dictionary<PolicyCategory, List<PolicyData>>();
            
            if (_backButton != null)
                _backButton.onClick.AddListener(NavigateBack);
            
            if (_implementButton != null)
                _implementButton.onClick.AddListener(ImplementSelectedPolicy);
        }
        
        #endregion
        
        #region Screen Lifecycle
        
        public override void OnScreenEnter(object data)
        {
            base.OnScreenEnter(data);
            
            if (data is PolicyMenuData menuData)
            {
                LoadPolicies(menuData.Policies);
                _currentPoliticalCapital = menuData.CurrentCapital;
                _maxPoliticalCapital = menuData.MaxCapital;
            }
            
            CreateCategoryTabs();
            SelectCategory(PolicyCategory.Economic);
            UpdateCapitalDisplay();
            
            if (_detailsPanel != null)
                _detailsPanel.SetActive(false);
        }
        
        #endregion
        
        #region Setup
        
        private void LoadPolicies(List<PolicyData> policies)
        {
            _policiesByCategory.Clear();
            
            foreach (PolicyCategory category in Enum.GetValues(typeof(PolicyCategory)))
            {
                _policiesByCategory[category] = new List<PolicyData>();
            }
            
            foreach (var policy in policies)
            {
                _policiesByCategory[policy.Category].Add(policy);
            }
        }
        
        private void CreateCategoryTabs()
        {
            if (_tabContainer == null || _tabPrefab == null) return;
            
            foreach (Transform child in _tabContainer)
            {
                Destroy(child.gameObject);
            }
            
            foreach (PolicyCategory category in Enum.GetValues(typeof(PolicyCategory)))
            {
                if (!_policiesByCategory.ContainsKey(category)) continue;
                
                var go = Instantiate(_tabPrefab, _tabContainer);
                var tab = go.GetComponent<CategoryTabUI>();
                if (tab != null)
                {
                    tab.Setup(category.ToString(), () => SelectCategory(category));
                }
            }
        }
        
        #endregion
        
        #region Category & Policy Selection
        
        private void SelectCategory(PolicyCategory category)
        {
            _currentCategory = category;
            RefreshPolicyList();
        }
        
        private void RefreshPolicyList()
        {
            if (_policyListContainer == null) return;
            
            foreach (Transform child in _policyListContainer)
            {
                Destroy(child.gameObject);
            }
            
            if (!_policiesByCategory.TryGetValue(_currentCategory, out var policies)) return;
            
            foreach (var policy in policies)
            {
                if (_policyItemPrefab != null)
                {
                    var go = Instantiate(_policyItemPrefab, _policyListContainer);
                    var item = go.GetComponent<PolicyItemUI>();
                    if (item != null)
                    {
                        item.Setup(policy, () => SelectPolicy(policy));
                    }
                }
            }
        }
        
        private void SelectPolicy(PolicyData policy)
        {
            _selectedPolicy = policy;
            ShowPolicyDetails(policy);
        }
        
        private void ShowPolicyDetails(PolicyData policy)
        {
            if (_detailsPanel == null) return;
            
            _detailsPanel.SetActive(true);
            
            if (_policyNameText != null) _policyNameText.text = policy.PolicyName;
            if (_policyDescriptionText != null) _policyDescriptionText.text = policy.Description;
            if (_capitalCostText != null) _capitalCostText.text = $"Cost: {policy.PoliticalCapitalCost} Political Capital";
            
            if (_trustImpactText != null)
            {
                string sign = policy.TrustImpact >= 0 ? "+" : "";
                _trustImpactText.text = $"Trust Impact: {sign}{policy.TrustImpact:F0}%";
                _trustImpactText.color = policy.TrustImpact >= 0 ? Color.green : Color.red;
            }
            
            // Bloc impacts
            if (_blocImpactContainer != null)
            {
                foreach (Transform child in _blocImpactContainer)
                {
                    Destroy(child.gameObject);
                }
                
                if (policy.BlocImpacts != null && _blocImpactPrefab != null)
                {
                    foreach (var impact in policy.BlocImpacts)
                    {
                        var go = Instantiate(_blocImpactPrefab, _blocImpactContainer);
                        var text = go.GetComponent<Text>();
                        if (text != null)
                        {
                            string sign = impact.Value >= 0 ? "+" : "";
                            text.text = $"{impact.Key}: {sign}{impact.Value:F0}%";
                            text.color = impact.Value >= 0 ? Color.green : Color.red;
                        }
                    }
                }
            }
            
            // Update implement button
            if (_implementButton != null)
            {
                bool canImplement = policy.IsAvailable && 
                                   !policy.IsImplemented && 
                                   _currentPoliticalCapital >= policy.PoliticalCapitalCost;
                _implementButton.interactable = canImplement;
            }
            
            if (_implementButtonText != null)
            {
                if (policy.IsImplemented)
                    _implementButtonText.text = "Already Implemented";
                else if (!policy.IsAvailable)
                    _implementButtonText.text = "Requirements Not Met";
                else if (_currentPoliticalCapital < policy.PoliticalCapitalCost)
                    _implementButtonText.text = "Insufficient Capital";
                else
                    _implementButtonText.text = "Implement Policy";
            }
        }
        
        private void ImplementSelectedPolicy()
        {
            if (_selectedPolicy == null) return;
            
            UIManager.ShowConfirmation(
                "Implement Policy",
                $"Are you sure you want to implement {_selectedPolicy.PolicyName}? This will cost {_selectedPolicy.PoliticalCapitalCost} Political Capital.",
                () =>
                {
                    OnPolicyImplemented?.Invoke(_selectedPolicy.PolicyId);
                    _selectedPolicy.IsImplemented = true;
                    ShowPolicyDetails(_selectedPolicy);
                }
            );
        }
        
        private void UpdateCapitalDisplay()
        {
            if (_politicalCapitalText != null)
            {
                _politicalCapitalText.text = $"{_currentPoliticalCapital}/{_maxPoliticalCapital}";
            }
            
            if (_capitalFill != null)
            {
                _capitalFill.fillAmount = (float)_currentPoliticalCapital / _maxPoliticalCapital;
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// Staff Management screen for hiring, firing, and managing staff.
    /// </summary>
    public class StaffManagement : BaseScreen
    {
        #region Serialized Fields
        
        [Header("Staff List")]
        [SerializeField] private Transform _staffListContainer;
        [SerializeField] private GameObject _staffCardPrefab;
        
        [Header("Staff Details")]
        [SerializeField] private GameObject _detailsPanel;
        [SerializeField] private Image _staffPortrait;
        [SerializeField] private Text _staffNameText;
        [SerializeField] private Text _staffRoleText;
        [SerializeField] private Slider _qualitySlider;
        [SerializeField] private Slider _loyaltySlider;
        [SerializeField] private Text _salaryText;
        [SerializeField] private Transform _traitsContainer;
        [SerializeField] private Button _fireButton;
        [SerializeField] private Button _promoteButton;
        [SerializeField] private Button _assignButton;
        
        [Header("Hiring")]
        [SerializeField] private Button _hireButton;
        [SerializeField] private Transform _candidatesContainer;
        [SerializeField] private GameObject _candidateCardPrefab;
        [SerializeField] private GameObject _hiringPanel;
        
        [Header("Budget")]
        [SerializeField] private Text _totalSalariesText;
        [SerializeField] private Text _budgetCapText;
        [SerializeField] private Image _budgetFill;
        
        [Header("Navigation")]
        [SerializeField] private Button _backButton;
        
        #endregion
        
        #region Private Fields
        
        private List<StaffMemberData> _staff = new List<StaffMemberData>();
        private List<StaffMemberData> _candidates = new List<StaffMemberData>();
        private StaffMemberData _selectedStaff;
        
        #endregion
        
        #region Events
        
        public event Action<string> OnStaffFired;
        public event Action<string> OnStaffHired;
        public event Action<string> OnStaffPromoted;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            SetupButtons();
        }
        
        #endregion
        
        #region Screen Lifecycle
        
        public override void OnScreenEnter(object data)
        {
            base.OnScreenEnter(data);
            
            if (data is StaffManagementData staffData)
            {
                _staff = staffData.CurrentStaff;
                _candidates = staffData.AvailableCandidates;
            }
            
            RefreshStaffList();
            UpdateBudgetDisplay();
            
            if (_detailsPanel != null) _detailsPanel.SetActive(false);
            if (_hiringPanel != null) _hiringPanel.SetActive(false);
        }
        
        #endregion
        
        #region Setup
        
        private void SetupButtons()
        {
            if (_backButton != null)
                _backButton.onClick.AddListener(NavigateBack);
            
            if (_hireButton != null)
                _hireButton.onClick.AddListener(ShowHiringPanel);
            
            if (_fireButton != null)
                _fireButton.onClick.AddListener(FireSelectedStaff);
            
            if (_promoteButton != null)
                _promoteButton.onClick.AddListener(PromoteSelectedStaff);
        }
        
        #endregion
        
        #region Display
        
        private void RefreshStaffList()
        {
            if (_staffListContainer == null) return;
            
            foreach (Transform child in _staffListContainer)
            {
                Destroy(child.gameObject);
            }
            
            foreach (var staff in _staff)
            {
                if (_staffCardPrefab != null)
                {
                    var go = Instantiate(_staffCardPrefab, _staffListContainer);
                    var card = go.GetComponent<StaffCardUI>();
                    if (card != null)
                    {
                        card.Setup(staff, () => SelectStaff(staff));
                    }
                }
            }
        }
        
        private void SelectStaff(StaffMemberData staff)
        {
            _selectedStaff = staff;
            ShowStaffDetails(staff);
        }
        
        private void ShowStaffDetails(StaffMemberData staff)
        {
            if (_detailsPanel == null) return;
            
            _detailsPanel.SetActive(true);
            
            if (_staffPortrait != null && staff.Portrait != null)
                _staffPortrait.sprite = staff.Portrait;
            
            if (_staffNameText != null) _staffNameText.text = staff.Name;
            if (_staffRoleText != null) _staffRoleText.text = staff.Role.ToString();
            if (_qualitySlider != null) _qualitySlider.value = staff.Quality / 10f;
            if (_loyaltySlider != null) _loyaltySlider.value = staff.Loyalty / 100f;
            if (_salaryText != null) _salaryText.text = $"${staff.Salary:N0}/month";
            
            // Traits
            if (_traitsContainer != null)
            {
                foreach (Transform child in _traitsContainer)
                {
                    Destroy(child.gameObject);
                }
                
                foreach (var trait in staff.Traits ?? new List<string>())
                {
                    var traitGo = new GameObject("Trait");
                    traitGo.transform.SetParent(_traitsContainer, false);
                    var text = traitGo.AddComponent<Text>();
                    text.text = trait;
                    text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                }
            }
        }
        
        private void ShowHiringPanel()
        {
            if (_hiringPanel == null) return;
            
            _hiringPanel.SetActive(true);
            
            if (_candidatesContainer != null)
            {
                foreach (Transform child in _candidatesContainer)
                {
                    Destroy(child.gameObject);
                }
                
                foreach (var candidate in _candidates)
                {
                    if (_candidateCardPrefab != null)
                    {
                        var go = Instantiate(_candidateCardPrefab, _candidatesContainer);
                        var card = go.GetComponent<StaffCardUI>();
                        if (card != null)
                        {
                            card.Setup(candidate, () => HireCandidate(candidate));
                        }
                    }
                }
            }
        }
        
        private void HireCandidate(StaffMemberData candidate)
        {
            UIManager.ShowConfirmation(
                "Hire Staff",
                $"Hire {candidate.Name} as {candidate.Role} for ${candidate.Salary:N0}/month?",
                () =>
                {
                    OnStaffHired?.Invoke(candidate.StaffId);
                    _staff.Add(candidate);
                    _candidates.Remove(candidate);
                    RefreshStaffList();
                    
                    if (_hiringPanel != null) _hiringPanel.SetActive(false);
                }
            );
        }
        
        private void FireSelectedStaff()
        {
            if (_selectedStaff == null) return;
            
            UIManager.ShowConfirmation(
                "Fire Staff",
                $"Are you sure you want to fire {_selectedStaff.Name}? This may affect staff morale.",
                () =>
                {
                    OnStaffFired?.Invoke(_selectedStaff.StaffId);
                    _staff.Remove(_selectedStaff);
                    RefreshStaffList();
                    
                    if (_detailsPanel != null) _detailsPanel.SetActive(false);
                    _selectedStaff = null;
                }
            );
        }
        
        private void PromoteSelectedStaff()
        {
            if (_selectedStaff == null) return;
            
            OnStaffPromoted?.Invoke(_selectedStaff.StaffId);
            UIManager.ShowToast("Staff Promoted", $"{_selectedStaff.Name} has been promoted!", NotificationType.Success);
        }
        
        private void UpdateBudgetDisplay()
        {
            int totalSalaries = _staff.Sum(s => s.Salary);
            int budgetCap = 50000; // Would come from game state
            
            if (_totalSalariesText != null)
                _totalSalariesText.text = $"${totalSalaries:N0}";
            
            if (_budgetCapText != null)
                _budgetCapText.text = $"/ ${budgetCap:N0}";
            
            if (_budgetFill != null)
                _budgetFill.fillAmount = (float)totalSalaries / budgetCap;
        }
        
        #endregion
    }
    
    /// <summary>
    /// Media Center screen for managing media relationships and coverage.
    /// </summary>
    public class MediaCenter : BaseScreen
    {
        #region Serialized Fields
        
        [Header("Media Outlets")]
        [SerializeField] private Transform _outletListContainer;
        [SerializeField] private GameObject _outletCardPrefab;
        [SerializeField] private Dropdown _filterDropdown;
        
        [Header("Outlet Details")]
        [SerializeField] private GameObject _detailsPanel;
        [SerializeField] private Image _outletLogo;
        [SerializeField] private Text _outletNameText;
        [SerializeField] private Text _outletTypeText;
        [SerializeField] private Slider _biasSlider;
        [SerializeField] private Text _biasLabel;
        [SerializeField] private Text _reachText;
        [SerializeField] private Slider _relationshipSlider;
        [SerializeField] private Text _currentCoverageText;
        
        [Header("Actions")]
        [SerializeField] private Button _pressConferenceButton;
        [SerializeField] private Button _interviewButton;
        [SerializeField] private Button _adBuyButton;
        [SerializeField] private Button _leakButton;
        
        [Header("Media Influence")]
        [SerializeField] private Text _mediaInfluenceText;
        [SerializeField] private Slider _mediaInfluenceSlider;
        [SerializeField] private Text _overallCoverageText;
        
        [Header("Navigation")]
        [SerializeField] private Button _backButton;
        
        #endregion
        
        #region Private Fields
        
        private List<MediaOutletData> _outlets = new List<MediaOutletData>();
        private MediaOutletData _selectedOutlet;
        private MediaType? _currentFilter;
        
        #endregion
        
        #region Events
        
        public event Action<string, string> OnMediaAction; // outletId, actionType
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            SetupButtons();
        }
        
        #endregion
        
        #region Screen Lifecycle
        
        public override void OnScreenEnter(object data)
        {
            base.OnScreenEnter(data);
            
            if (data is MediaCenterData mediaData)
            {
                _outlets = mediaData.Outlets;
            }
            
            RefreshOutletList();
            UpdateMediaInfluenceDisplay();
            
            if (_detailsPanel != null) _detailsPanel.SetActive(false);
        }
        
        #endregion
        
        #region Setup
        
        private void SetupButtons()
        {
            if (_backButton != null)
                _backButton.onClick.AddListener(NavigateBack);
            
            if (_pressConferenceButton != null)
                _pressConferenceButton.onClick.AddListener(() => 
                    OnMediaAction?.Invoke(_selectedOutlet?.OutletId, "PressConference"));
            
            if (_interviewButton != null)
                _interviewButton.onClick.AddListener(() => 
                    OnMediaAction?.Invoke(_selectedOutlet?.OutletId, "Interview"));
            
            if (_adBuyButton != null)
                _adBuyButton.onClick.AddListener(() => 
                    OnMediaAction?.Invoke(_selectedOutlet?.OutletId, "AdBuy"));
            
            if (_leakButton != null)
                _leakButton.onClick.AddListener(() => 
                    OnMediaAction?.Invoke(_selectedOutlet?.OutletId, "Leak"));
            
            if (_filterDropdown != null)
            {
                _filterDropdown.ClearOptions();
                _filterDropdown.AddOptions(new List<string> { "All" });
                foreach (MediaType type in Enum.GetValues(typeof(MediaType)))
                {
                    _filterDropdown.AddOptions(new List<string> { type.ToString() });
                }
                _filterDropdown.onValueChanged.AddListener(OnFilterChanged);
            }
        }
        
        private void OnFilterChanged(int index)
        {
            if (index == 0)
                _currentFilter = null;
            else
                _currentFilter = (MediaType)(index - 1);
            
            RefreshOutletList();
        }
        
        #endregion
        
        #region Display
        
        private void RefreshOutletList()
        {
            if (_outletListContainer == null) return;
            
            foreach (Transform child in _outletListContainer)
            {
                Destroy(child.gameObject);
            }
            
            var filtered = _currentFilter.HasValue 
                ? _outlets.Where(o => o.Type == _currentFilter.Value) 
                : _outlets;
            
            foreach (var outlet in filtered.OrderByDescending(o => o.Reach))
            {
                if (_outletCardPrefab != null)
                {
                    var go = Instantiate(_outletCardPrefab, _outletListContainer);
                    var card = go.GetComponent<MediaOutletCardUI>();
                    if (card != null)
                    {
                        card.Setup(outlet, () => SelectOutlet(outlet));
                    }
                }
            }
        }
        
        private void SelectOutlet(MediaOutletData outlet)
        {
            _selectedOutlet = outlet;
            ShowOutletDetails(outlet);
        }
        
        private void ShowOutletDetails(MediaOutletData outlet)
        {
            if (_detailsPanel == null) return;
            
            _detailsPanel.SetActive(true);
            
            if (_outletLogo != null && outlet.Logo != null)
                _outletLogo.sprite = outlet.Logo;
            
            if (_outletNameText != null) _outletNameText.text = outlet.OutletName;
            if (_outletTypeText != null) _outletTypeText.text = outlet.Type.ToString();
            
            if (_biasSlider != null)
            {
                _biasSlider.value = (outlet.BiasScore + 100) / 200f;
            }
            
            if (_biasLabel != null)
            {
                if (outlet.BiasScore < -30)
                    _biasLabel.text = "Left-leaning";
                else if (outlet.BiasScore > 30)
                    _biasLabel.text = "Right-leaning";
                else
                    _biasLabel.text = "Neutral";
            }
            
            if (_reachText != null)
                _reachText.text = $"Reach: {outlet.Reach:N0} viewers";
            
            if (_relationshipSlider != null)
                _relationshipSlider.value = outlet.RelationshipScore / 100f;
            
            if (_currentCoverageText != null)
                _currentCoverageText.text = outlet.CurrentCoverage ?? "No recent coverage";
        }
        
        private void UpdateMediaInfluenceDisplay()
        {
            // Would be populated from game state
            if (_mediaInfluenceText != null)
                _mediaInfluenceText.text = "75/100";
            
            if (_mediaInfluenceSlider != null)
                _mediaInfluenceSlider.value = 0.75f;
        }
        
        #endregion
    }
    
    #region Data Transfer Classes
    
    public class CampaignDashboardData
    {
        public List<PollingData> Polling = new List<PollingData>();
        public List<VoterBlocData> VoterBlocs = new List<VoterBlocData>();
        public List<CampaignAction> AvailableActions = new List<CampaignAction>();
        public int ActionsRemaining;
        public int MaxActions;
    }
    
    public class PolicyMenuData
    {
        public List<PolicyData> Policies = new List<PolicyData>();
        public int CurrentCapital;
        public int MaxCapital;
    }
    
    public class StaffManagementData
    {
        public List<StaffMemberData> CurrentStaff = new List<StaffMemberData>();
        public List<StaffMemberData> AvailableCandidates = new List<StaffMemberData>();
    }
    
    public class MediaCenterData
    {
        public List<MediaOutletData> Outlets = new List<MediaOutletData>();
    }
    
    #endregion
    
    #region UI Component Classes
    
    public class PollingBarUI : MonoBehaviour
    {
        [SerializeField] private Image _fillBar;
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _percentageText;
        [SerializeField] private Text _changeText;
        [SerializeField] private Image _playerIndicator;
        
        public void Setup(PollingData data)
        {
            if (_fillBar != null)
            {
                _fillBar.fillAmount = data.Percentage / 100f;
                _fillBar.color = data.PartyColor;
            }
            
            if (_nameText != null) _nameText.text = data.CandidateName;
            if (_percentageText != null) _percentageText.text = $"{data.Percentage:F1}%";
            
            if (_changeText != null)
            {
                float change = data.Percentage - data.PreviousPercentage;
                string sign = change >= 0 ? "+" : "";
                _changeText.text = $"{sign}{change:F1}%";
                _changeText.color = change >= 0 ? Color.green : Color.red;
            }
            
            if (_playerIndicator != null)
                _playerIndicator.gameObject.SetActive(data.IsPlayer);
        }
    }
    
    public class VoterBlocUI : MonoBehaviour
    {
        [SerializeField] private Text _blocNameText;
        [SerializeField] private Text _supportText;
        [SerializeField] private Image _supportFill;
        [SerializeField] private Text _populationText;
        [SerializeField] private Image _trendIcon;
        [SerializeField] private Image _blocIcon;
        
        public void Setup(VoterBlocData data)
        {
            if (_blocNameText != null) _blocNameText.text = data.BlocName;
            if (_supportText != null) _supportText.text = $"{data.SupportPercentage:F0}%";
            if (_supportFill != null)
            {
                _supportFill.fillAmount = data.SupportPercentage / 100f;
                _supportFill.color = data.BlocColor;
            }
            if (_populationText != null) _populationText.text = $"{data.PopulationPercentage:F0}% of voters";
            if (_blocIcon != null && data.Icon != null) _blocIcon.sprite = data.Icon;
        }
    }
    
    public class CampaignActionButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _costText;
        [SerializeField] private Text _cooldownText;
        [SerializeField] private CanvasGroup _canvasGroup;
        
        public void Setup(CampaignAction action, Action onClick)
        {
            if (_icon != null && action.Icon != null) _icon.sprite = action.Icon;
            if (_nameText != null) _nameText.text = action.ActionName;
            if (_costText != null) _costText.text = $"${action.Cost:N0}";
            
            if (_cooldownText != null)
            {
                if (!string.IsNullOrEmpty(action.CooldownRemaining))
                {
                    _cooldownText.text = action.CooldownRemaining;
                    _cooldownText.gameObject.SetActive(true);
                }
                else
                {
                    _cooldownText.gameObject.SetActive(false);
                }
            }
            
            if (_button != null)
            {
                _button.onClick.AddListener(() => onClick());
                _button.interactable = action.IsAvailable;
            }
            
            if (_canvasGroup != null)
                _canvasGroup.alpha = action.IsAvailable ? 1f : 0.5f;
        }
    }
    
    public class CategoryTabUI : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Text _labelText;
        [SerializeField] private Image _selectedIndicator;
        
        private bool _isSelected;
        
        public void Setup(string label, Action onClick)
        {
            if (_labelText != null) _labelText.text = label;
            if (_button != null) _button.onClick.AddListener(() => onClick());
        }
        
        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            if (_selectedIndicator != null)
                _selectedIndicator.gameObject.SetActive(selected);
        }
    }
    
    public class PolicyItemUI : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _costText;
        [SerializeField] private Image _implementedIcon;
        
        public void Setup(PolicyData policy, Action onClick)
        {
            if (_nameText != null) _nameText.text = policy.PolicyName;
            if (_costText != null) _costText.text = $"{policy.PoliticalCapitalCost} PC";
            if (_implementedIcon != null) _implementedIcon.gameObject.SetActive(policy.IsImplemented);
            if (_button != null) _button.onClick.AddListener(() => onClick());
        }
    }
    
    public class StaffCardUI : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _portrait;
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _roleText;
        [SerializeField] private Slider _qualitySlider;
        [SerializeField] private Text _salaryText;
        
        public void Setup(StaffMemberData staff, Action onClick)
        {
            if (_portrait != null && staff.Portrait != null) _portrait.sprite = staff.Portrait;
            if (_nameText != null) _nameText.text = staff.Name;
            if (_roleText != null) _roleText.text = staff.Role.ToString();
            if (_qualitySlider != null) _qualitySlider.value = staff.Quality / 10f;
            if (_salaryText != null) _salaryText.text = $"${staff.Salary:N0}";
            if (_button != null) _button.onClick.AddListener(() => onClick());
        }
    }
    
    public class MediaOutletCardUI : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _logo;
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _typeText;
        [SerializeField] private Slider _relationshipSlider;
        
        public void Setup(MediaOutletData outlet, Action onClick)
        {
            if (_logo != null && outlet.Logo != null) _logo.sprite = outlet.Logo;
            if (_nameText != null) _nameText.text = outlet.OutletName;
            if (_typeText != null) _typeText.text = outlet.Type.ToString();
            if (_relationshipSlider != null) _relationshipSlider.value = outlet.RelationshipScore / 100f;
            if (_button != null) _button.onClick.AddListener(() => onClick());
        }
    }
    
    #endregion
}
