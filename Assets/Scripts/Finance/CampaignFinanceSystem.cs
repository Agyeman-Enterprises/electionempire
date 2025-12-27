using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.Monetization;
using ElectionEmpire.Finance;

// ============================================================================
// ELECTION EMPIRE - CAMPAIGN FINANCE & PAYMENT SYSTEM
// Complete financial management: donations, expenses, PACs, dark money, FEC
// ============================================================================

namespace ElectionEmpire.Finance
{
    #region Enums
    
    /// <summary>
    /// Types of financial accounts
    /// </summary>
    public enum AccountType
    {
        CampaignFund,           // Main campaign account (FEC regulated)
        PersonalFund,           // Candidate's personal money
        PAC,                    // Political Action Committee
        SuperPAC,               // Independent expenditure committee
        DarkMoney,              // 501(c)(4) - untraceable
        PartyFund,              // Party committee allocation
        TransitionFund,         // Post-election transition
        LegalDefenseFund,       // For investigations/lawsuits
        SlushFund               // Off-books money (illegal)
    }
    
    /// <summary>
    /// Donation source categories
    /// </summary>
    public enum DonorType
    {
        Individual,             // Regular citizen
        SmallDollar,            // Grassroots (<$50 average)
        MaxedOut,               // Individual at max contribution
        Bundler,                // Person collecting multiple donations
        Corporation,            // Business entity (PAC only)
        Union,                  // Labor organization
        SpecialInterest,        // Lobby groups
        SelfFunding,            // Candidate's own money
        Party,                  // Party committee
        Anonymous,              // Dark money source
        Foreign                 // Illegal foreign money
    }
    
    /// <summary>
    /// Expense categories
    /// </summary>
    public enum ExpenseCategory
    {
        // Payroll
        StaffSalaries,
        ConsultantFees,
        ContractorPayments,
        
        // Operations
        OfficeRent,
        OfficeSupplies,
        Travel,
        Events,
        Security,
        
        // Advertising
        TVAds,
        RadioAds,
        DigitalAds,
        PrintAds,
        Mailers,
        Billboards,
        
        // Outreach
        Polling,
        OppositionResearch,
        FieldOperations,
        PhoneBanking,
        Canvassing,
        
        // Legal/Compliance
        LegalFees,
        Compliance,
        FECFines,
        
        // Special
        DirtyTricks,
        Bribes,
        Hush_Money,
        OffBooks
    }
    
    /// <summary>
    /// FEC violation types
    /// </summary>
    public enum FECViolationType
    {
        None,
        LateReporting,              // Minor, just fines
        InaccurateReporting,        // Moderate
        ExcessiveContribution,      // Donor gave too much
        CorporateDonation,          // Illegal for campaign fund
        CoordinationViolation,      // SuperPAC coordination
        ForeignDonation,            // Very serious
        StrawDonor,                 // Using fake names
        UnreportedExpense,          // Off-books spending
        ConversionToPersonalUse,    // Using campaign $ personally
        WirefraudLaunderingBribery  // Federal crimes
    }
    
    /// <summary>
    /// Financial health status
    /// </summary>
    public enum FinancialHealth
    {
        Thriving,           // Plenty of cash, low burn
        Healthy,            // Comfortable position
        Stable,             // Getting by
        Tight,              // Careful spending needed
        Struggling,         // Making cuts
        Crisis,             // Can't make payroll
        Bankrupt            // Campaign over
    }
    
    #endregion
    
    #region Financial Data Structures
    /// <summary>
    /// A financial account
    /// </summary>
    [Serializable]
    public class FinancialAccount
    {
        public AccountType Type;
        public string Name;
        public float Balance;
        public float TotalRaised;
        public float TotalSpent;
        public bool IsActive;
        public bool RequiresFECReporting;
        public float ContributionLimit;         // Per donor, 0 = unlimited
        public List<Transaction> Transactions;
        public Dictionary<string, float> DonorTotals;  // Track per-donor amounts
        
        public FinancialAccount(AccountType type)
        {
            Type = type;
            Transactions = new List<Transaction>();
            DonorTotals = new Dictionary<string, float>();
            IsActive = true;
            
            // Set properties based on type
            switch (type)
            {
                case AccountType.CampaignFund:
                    Name = "Campaign Fund";
                    RequiresFECReporting = true;
                    ContributionLimit = 3300f;  // 2024 individual limit
                    break;
                case AccountType.PersonalFund:
                    Name = "Personal Funds";
                    RequiresFECReporting = true;
                    ContributionLimit = 0;      // No limit on self-funding
                    break;
                case AccountType.PAC:
                    Name = "PAC Account";
                    RequiresFECReporting = true;
                    ContributionLimit = 5000f;
                    break;
                case AccountType.SuperPAC:
                    Name = "Super PAC";
                    RequiresFECReporting = true;
                    ContributionLimit = 0;      // Unlimited
                    break;
                case AccountType.DarkMoney:
                    Name = "Social Welfare Organization";
                    RequiresFECReporting = false;
                    ContributionLimit = 0;
                    break;
                case AccountType.PartyFund:
                    Name = "Party Committee";
                    RequiresFECReporting = true;
                    ContributionLimit = 106500f; // Coordinated party limit
                    break;
                case AccountType.LegalDefenseFund:
                    Name = "Legal Defense Fund";
                    RequiresFECReporting = true;
                    ContributionLimit = 0;
                    break;
                case AccountType.SlushFund:
                    Name = "Miscellaneous Account";
                    RequiresFECReporting = false;
                    ContributionLimit = 0;
                    break;
            }
        }
        
        public bool CanAcceptDonation(string donorId, float amount)
        {
            if (ContributionLimit == 0) return true;
            
            float currentTotal = DonorTotals.TryGetValue(donorId, out float total) ? total : 0;
            return currentTotal + amount <= ContributionLimit;
        }
    }
    
    /// <summary>
    /// A donor entity
    /// </summary>
    [Serializable]
    public class Donor
    {
        public string Id;
        public string Name;
        public DonorType Type;
        public string Occupation;
        public string Employer;
        public string Location;
        public float TotalDonated;
        public float MaxContribution;
        public int DonationCount;
        public DateTime FirstDonation;
        public DateTime LastDonation;
        public float LoyaltyScore;              // 0-100, likelihood to donate again
        public bool IsMaxedOut;
        public bool IsSuspicious;
        public string SuspiciousReason;
        public Dictionary<string, float> DonationsByAccount;
        
        // For special interest donors
        public string IndustryAffiliation;
        public List<string> PolicyInterests;
        public float ExpectedQuidProQuo;        // What they want in return
        
        public Donor()
        {
            Id = Guid.NewGuid().ToString("N").Substring(0, 8);
            DonationsByAccount = new Dictionary<string, float>();
            PolicyInterests = new List<string>();
            FirstDonation = DateTime.Now;
        }
    }
    
    /// <summary>
    /// Recurring expense (like staff salaries)
    /// </summary>
    [Serializable]
    public class RecurringExpense
    {
        public string Id;
        public string Name;
        public ExpenseCategory Category;
        public float AmountPerTurn;
        public AccountType PayFromAccount;
        public bool IsActive;
        public bool IsEssential;                // Campaign can't function without
        public string RelatedEntityId;          // Staff ID, vendor ID, etc.
        public int TurnStarted;
        public int? TurnEnded;
        
        public RecurringExpense()
        {
            Id = Guid.NewGuid().ToString("N").Substring(0, 8);
            IsActive = true;
        }
    }
    
    /// <summary>
    /// FEC violation record
    /// </summary>
    [Serializable]
    public class FECViolation
    {
        public string Id;
        public FECViolationType Type;
        public string Description;
        public DateTime Occurred;
        public DateTime? Discovered;
        public float PotentialFine;
        public float ActualFine;
        public bool IsResolved;
        public bool TriggeredInvestigation;
        public float ScandalRisk;               // Chance this becomes public scandal
        public List<string> RelatedTransactions;
        
        public FECViolation()
        {
            Id = Guid.NewGuid().ToString("N").Substring(0, 8);
            RelatedTransactions = new List<string>();
        }
    }
    
    #endregion
    
    #region Campaign Finance Manager
    
    /// <summary>
    /// Main financial management system
    /// </summary>
    public class CampaignFinanceManager : MonoBehaviour
    {
        public static CampaignFinanceManager Instance { get; private set; }
        
        [Header("Configuration")]
        public float StartingCampaignFund = 5000f;
        public float StartingPersonalFund = 50000f;
        public int FECReportingPeriodTurns = 3;
        
        // Accounts
        public Dictionary<AccountType, FinancialAccount> Accounts { get; private set; }
        
        // Donors
        public Dictionary<string, Donor> Donors { get; private set; }
        
        // Recurring expenses
        public List<RecurringExpense> RecurringExpenses { get; private set; }
        
        // Violations
        public List<FECViolation> Violations { get; private set; }
        public List<FECViolation> PendingViolations { get; private set; }
        
        // State
        public FinancialHealth CurrentHealth { get; private set; }
        public float BurnRate { get; private set; }
        public int TurnsOfRunway { get; private set; }
        public float TotalRaisedAllTime { get; private set; }
        public float TotalSpentAllTime { get; private set; }
        
        // Events
        public event Action<Transaction> OnTransactionProcessed;
        public event Action<Donor, float> OnDonationReceived;
        public event Action<FECViolation> OnViolationDiscovered;
        public event Action<FinancialHealth> OnHealthChanged;
        public event Action OnPayrollMissed;
        public event Action<float> OnCashCrisis;
        
        private int _currentTurn;
        private int _lastFECReportTurn;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeAccounts();
            Donors = new Dictionary<string, Donor>();
            RecurringExpenses = new List<RecurringExpense>();
            Violations = new List<FECViolation>();
            PendingViolations = new List<FECViolation>();
        }
        
        private void InitializeAccounts()
        {
            Accounts = new Dictionary<AccountType, FinancialAccount>
            {
                { AccountType.CampaignFund, new FinancialAccount(AccountType.CampaignFund) 
                    { Balance = StartingCampaignFund } },
                { AccountType.PersonalFund, new FinancialAccount(AccountType.PersonalFund) 
                    { Balance = StartingPersonalFund } }
            };
        }
        
        #region Donations
        
        /// <summary>
        /// Process a donation
        /// </summary>
        public TransactionResult ProcessDonation(
            float amount, 
            Donor donor, 
            AccountType targetAccount = AccountType.CampaignFund,
            bool isReported = true)
        {
            var result = new TransactionResult();
            
            // Validate account exists
            if (!Accounts.TryGetValue(targetAccount, out var account))
            {
                result.Success = false;
                result.Message = "Account does not exist";
                return result;
            }
            
            // Check contribution limits
            if (!account.CanAcceptDonation(donor.Id, amount))
            {
                result.Success = false;
                result.Message = "Contribution would exceed legal limits";
                result.Violation = CreateViolation(
                    FECViolationType.ExcessiveContribution,
                    $"Attempted excessive contribution of ${amount:N0} from {donor.Name}"
                );
                return result;
            }
            
            // Check for illegal donation types
            if (donor.Type == DonorType.Foreign)
            {
                result.Success = false;
                result.Message = "Foreign donations are illegal";
                result.Violation = CreateViolation(
                    FECViolationType.ForeignDonation,
                    $"Attempted foreign donation of ${amount:N0} from {donor.Name}"
                );
                return result;
            }
            
            if (donor.Type == DonorType.Corporation && targetAccount == AccountType.CampaignFund)
            {
                result.Success = false;
                result.Message = "Corporate donations cannot go directly to campaign fund";
                result.Violation = CreateViolation(
                    FECViolationType.CorporateDonation,
                    $"Attempted corporate donation of ${amount:N0} from {donor.Name}"
                );
                return result;
            }
            
            // Process the donation
            var transaction = new Transaction
            {
                Type = TransactionType.Donation,
                Amount = amount,
                DestinationAccount = targetAccount,
                Description = $"Donation from {donor.Name}",
                Category = donor.Type.ToString(),
                RelatedEntityId = donor.Id,
                RelatedEntityName = donor.Name,
                IsReported = isReported
            };
            
            // Check if unreported donation is suspicious
            if (!isReported && targetAccount != AccountType.DarkMoney)
            {
                transaction.IsSuspicious = true;
                result.Violation = CreateViolation(
                    FECViolationType.UnreportedExpense,
                    $"Unreported donation of ${amount:N0}",
                    false // Not yet discovered
                );
            }
            
            // Apply transaction
            account.Balance += amount;
            account.TotalRaised += amount;
            account.Transactions.Add(transaction);
            
            // Update donor tracking
            if (!account.DonorTotals.ContainsKey(donor.Id))
                account.DonorTotals[donor.Id] = 0;
            account.DonorTotals[donor.Id] += amount;
            
            // Update donor record
            UpdateDonorRecord(donor, amount, targetAccount);
            
            // Update totals
            TotalRaisedAllTime += amount;
            
            result.Success = true;
            result.Transaction = transaction;
            result.Message = $"Successfully processed ${amount:N0} donation";
            
            OnTransactionProcessed?.Invoke(transaction);
            OnDonationReceived?.Invoke(donor, amount);
            
            UpdateFinancialHealth();
            
            return result;
        }
        
        /// <summary>
        /// Process bulk small dollar donations (grassroots)
        /// </summary>
        public TransactionResult ProcessGrassrootsDonations(
            int donorCount, 
            float averageAmount, 
            float variance = 0.3f)
        {
            float totalAmount = 0;
            
            for (int i = 0; i < donorCount; i++)
            {
                float amount = averageAmount * (1f + UnityEngine.Random.Range(-variance, variance));
                totalAmount += amount;
            }
            
            // Create aggregate donor
            var aggregateDonor = new Donor
            {
                Name = $"Small Dollar Donors ({donorCount} contributions)",
                Type = DonorType.SmallDollar,
                TotalDonated = totalAmount
            };
            
            return ProcessDonation(totalAmount, aggregateDonor);
        }
        
        private void UpdateDonorRecord(Donor donor, float amount, AccountType account)
        {
            if (!Donors.ContainsKey(donor.Id))
            {
                Donors[donor.Id] = donor;
            }
            
            var d = Donors[donor.Id];
            d.TotalDonated += amount;
            d.DonationCount++;
            d.LastDonation = DateTime.Now;
            
            if (!d.DonationsByAccount.ContainsKey(account.ToString()))
                d.DonationsByAccount[account.ToString()] = 0;
            d.DonationsByAccount[account.ToString()] += amount;
            
            // Check if maxed out for campaign fund
            if (account == AccountType.CampaignFund)
            {
                var campaignAccount = Accounts[AccountType.CampaignFund];
                if (campaignAccount.DonorTotals.TryGetValue(donor.Id, out float total))
                {
                    d.IsMaxedOut = total >= campaignAccount.ContributionLimit;
                }
            }
        }
        
        #endregion
        
        #region Expenses
        
        /// <summary>
        /// Process an expense
        /// </summary>
        public TransactionResult ProcessExpense(
            float amount,
            ExpenseCategory category,
            string description,
            AccountType fromAccount = AccountType.CampaignFund,
            bool isReported = true,
            string relatedEntityId = null)
        {
            var result = new TransactionResult();
            
            if (!Accounts.TryGetValue(fromAccount, out var account))
            {
                result.Success = false;
                result.Message = "Account does not exist";
                return result;
            }
            
            if (account.Balance < amount)
            {
                result.Success = false;
                result.Message = "Insufficient funds";
                return result;
            }
            
            var transaction = new Transaction
            {
                Type = TransactionType.Expense,
                Amount = -amount,
                SourceAccount = fromAccount,
                Description = description,
                Category = category.ToString(),
                RelatedEntityId = relatedEntityId,
                IsReported = isReported
            };
            
            // Check for suspicious categories
            if (category is ExpenseCategory.DirtyTricks or 
                ExpenseCategory.Bribes or 
                ExpenseCategory.Hush_Money or
                ExpenseCategory.OffBooks)
            {
                transaction.IsSuspicious = true;
                transaction.IsReported = false;
                
                // High chance of creating violation if discovered
                if (UnityEngine.Random.value < 0.1f) // 10% chance per transaction
                {
                    result.Violation = CreateViolation(
                        FECViolationType.UnreportedExpense,
                        $"Unreported {category} expense of ${amount:N0}",
                        false
                    );
                    PendingViolations.Add(result.Violation);
                }
            }
            
            // Apply transaction
            account.Balance -= amount;
            account.TotalSpent += amount;
            account.Transactions.Add(transaction);
            
            TotalSpentAllTime += amount;
            
            result.Success = true;
            result.Transaction = transaction;
            result.Message = $"Expense of ${amount:N0} processed";
            
            OnTransactionProcessed?.Invoke(transaction);
            UpdateFinancialHealth();
            
            return result;
        }
        
        /// <summary>
        /// Add a recurring expense (like staff salary)
        /// </summary>
        public void AddRecurringExpense(RecurringExpense expense)
        {
            expense.TurnStarted = _currentTurn;
            RecurringExpenses.Add(expense);
            UpdateBurnRate();
        }
        
        /// <summary>
        /// Remove/end a recurring expense
        /// </summary>
        public void EndRecurringExpense(string expenseId)
        {
            var expense = RecurringExpenses.FirstOrDefault(e => e.Id == expenseId);
            if (expense != null)
            {
                expense.IsActive = false;
                expense.TurnEnded = _currentTurn;
                UpdateBurnRate();
            }
        }
        
        /// <summary>
        /// Process all recurring expenses for end of turn
        /// </summary>
        public List<TransactionResult> ProcessRecurringExpenses()
        {
            var results = new List<TransactionResult>();
            bool missedPayroll = false;
            
            foreach (var expense in RecurringExpenses.Where(e => e.IsActive))
            {
                var result = ProcessExpense(
                    expense.AmountPerTurn,
                    expense.Category,
                    $"Recurring: {expense.Name}",
                    expense.PayFromAccount,
                    relatedEntityId: expense.RelatedEntityId
                );
                
                results.Add(result);
                
                if (!result.Success && expense.IsEssential)
                {
                    if (expense.Category == ExpenseCategory.StaffSalaries)
                    {
                        missedPayroll = true;
                    }
                }
            }
            
            if (missedPayroll)
            {
                OnPayrollMissed?.Invoke();
            }
            
            return results;
        }
        
        #endregion
        
        #region Fundraising
        
        /// <summary>
        /// Hold a fundraising event
        /// </summary>
        public FundraisingResult HoldFundraisingEvent(FundraisingEvent eventData)
        {
            var result = new FundraisingResult
            {
                EventName = eventData.Name,
                EventCost = eventData.Cost
            };
            
            // Pay for event
            var costResult = ProcessExpense(
                eventData.Cost,
                ExpenseCategory.Events,
                $"Fundraising event: {eventData.Name}"
            );
            
            if (!costResult.Success)
            {
                result.Success = false;
                result.Message = "Could not afford event";
                return result;
            }
            
            // Calculate attendance and donations
            float attendanceMultiplier = CalculateAttendanceMultiplier(eventData);
            int actualAttendance = (int)(eventData.ExpectedAttendance * attendanceMultiplier);
            
            // Generate donations
            float totalRaised = 0;
            int donorCount = 0;
            
            // High dollar donors
            int highDollarCount = (int)(actualAttendance * eventData.HighDollarRatio);
            for (int i = 0; i < highDollarCount; i++)
            {
                float amount = eventData.TicketPrice * UnityEngine.Random.Range(1f, 5f);
                var donor = GenerateEventDonor(eventData, true);
                
                var donationResult = ProcessDonation(amount, donor);
                if (donationResult.Success)
                {
                    totalRaised += amount;
                    donorCount++;
                    result.DonorIds.Add(donor.Id);
                }
            }
            
            // Regular donors
            int regularCount = actualAttendance - highDollarCount;
            float regularTotal = regularCount * eventData.TicketPrice;
            var grassrootsResult = ProcessGrassrootsDonations(regularCount, eventData.TicketPrice);
            if (grassrootsResult.Success)
            {
                totalRaised += regularTotal;
                donorCount += regularCount;
            }
            
            result.Success = true;
            result.TotalRaised = totalRaised;
            result.NetRaised = totalRaised - eventData.Cost;
            result.DonorCount = donorCount;
            result.Attendance = actualAttendance;
            result.Message = $"Raised ${totalRaised:N0} from {donorCount} donors (net: ${result.NetRaised:N0})";
            
            // Special interest donors might expect something
            result.QuidProQuoExpectations = eventData.SpecialInterestPresent 
                ? GenerateQuidProQuoExpectations(eventData) 
                : new List<string>();
            
            return result;
        }
        
        private float CalculateAttendanceMultiplier(FundraisingEvent eventData)
        {
            float multiplier = 1f;
            
            // Factors affecting attendance
            // - Campaign momentum
            // - Event quality/venue
            // - Candidate charisma
            // - Weather (random)
            // - Competing events
            
            multiplier += UnityEngine.Random.Range(-0.2f, 0.3f);
            
            return Mathf.Clamp(multiplier, 0.5f, 1.5f);
        }
        
        private Donor GenerateEventDonor(FundraisingEvent eventData, bool isHighDollar)
        {
            var donor = new Donor
            {
                Type = isHighDollar ? DonorType.Individual : DonorType.SmallDollar,
                Name = GenerateDonorName(),
                Occupation = GenerateOccupation(eventData.TargetDemographic),
                LoyaltyScore = UnityEngine.Random.Range(30f, 80f)
            };
            
            if (eventData.SpecialInterestPresent && isHighDollar && UnityEngine.Random.value < 0.3f)
            {
                donor.Type = DonorType.SpecialInterest;
                donor.IndustryAffiliation = eventData.SpecialInterestType;
                donor.PolicyInterests.Add(eventData.SpecialInterestType);
                donor.ExpectedQuidProQuo = UnityEngine.Random.Range(0.3f, 0.8f);
            }
            
            return donor;
        }
        
        private string GenerateDonorName()
        {
            string[] firstNames = { "James", "Mary", "Robert", "Patricia", "John", "Jennifer", 
                                   "Michael", "Linda", "David", "Elizabeth", "William", "Barbara" };
            string[] lastNames = { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", 
                                  "Miller", "Davis", "Rodriguez", "Martinez", "Hernandez", "Lopez" };
            
            return $"{firstNames[UnityEngine.Random.Range(0, firstNames.Length)]} " +
                   $"{lastNames[UnityEngine.Random.Range(0, lastNames.Length)]}";
        }
        
        private string GenerateOccupation(string demographic)
        {
            // Simplified - would be more detailed
            string[] occupations = { "Business Owner", "Attorney", "Physician", "Executive", 
                                    "Consultant", "Investor", "Retired", "Developer" };
            return occupations[UnityEngine.Random.Range(0, occupations.Length)];
        }
        
        private List<string> GenerateQuidProQuoExpectations(FundraisingEvent eventData)
        {
            var expectations = new List<string>();
            
            if (eventData.SpecialInterestType.Contains("Oil"))
                expectations.Add("Oppose environmental regulations");
            else if (eventData.SpecialInterestType.Contains("Pharma"))
                expectations.Add("Oppose drug price controls");
            else if (eventData.SpecialInterestType.Contains("Tech"))
                expectations.Add("Oppose privacy regulations");
            else if (eventData.SpecialInterestType.Contains("Finance"))
                expectations.Add("Oppose banking regulations");
            else
                expectations.Add("Favorable regulatory treatment");
            
            return expectations;
        }
        
        #endregion
        
        #region Dark Money & Illegal Finance
        
        /// <summary>
        /// Activate dark money account (creates 501c4)
        /// </summary>
        public void ActivateDarkMoneyAccount()
        {
            if (!Accounts.ContainsKey(AccountType.DarkMoney))
            {
                Accounts[AccountType.DarkMoney] = new FinancialAccount(AccountType.DarkMoney);
            }
        }
        
        /// <summary>
        /// Activate slush fund (illegal off-books account)
        /// </summary>
        public void ActivateSlushFund(float initialDeposit = 0)
        {
            if (!Accounts.ContainsKey(AccountType.SlushFund))
            {
                Accounts[AccountType.SlushFund] = new FinancialAccount(AccountType.SlushFund)
                {
                    Balance = initialDeposit
                };
            }
            
            // This is always illegal
            CreateViolation(
                FECViolationType.UnreportedExpense,
                "Maintaining off-books slush fund",
                false // Not discovered yet
            );
        }
        
        /// <summary>
        /// Launder money between accounts
        /// </summary>
        public TransactionResult LaunderMoney(
            float amount,
            AccountType fromAccount,
            AccountType toAccount,
            string coverStory)
        {
            var result = new TransactionResult();
            
            if (!Accounts.TryGetValue(fromAccount, out var source) ||
                !Accounts.TryGetValue(toAccount, out var dest))
            {
                result.Success = false;
                result.Message = "Invalid accounts";
                return result;
            }
            
            if (source.Balance < amount)
            {
                result.Success = false;
                result.Message = "Insufficient funds";
                return result;
            }
            
            // Process transfer
            source.Balance -= amount;
            dest.Balance += amount;
            
            var transaction = new Transaction
            {
                Type = TransactionType.Transfer,
                Amount = amount,
                SourceAccount = fromAccount,
                DestinationAccount = toAccount,
                Description = coverStory,
                IsSuspicious = true,
                IsReported = false
            };
            
            source.Transactions.Add(transaction);
            dest.Transactions.Add(transaction);
            
            // High risk of violation
            if (UnityEngine.Random.value < 0.2f)
            {
                result.Violation = CreateViolation(
                    FECViolationType.WirefraudLaunderingBribery,
                    $"Suspicious transfer of ${amount:N0}: {coverStory}",
                    false
                );
                PendingViolations.Add(result.Violation);
            }
            
            result.Success = true;
            result.Transaction = transaction;
            result.Message = $"Transferred ${amount:N0}";
            
            return result;
        }
        
        /// <summary>
        /// Accept a bribe (goes to slush fund or personal)
        /// </summary>
        public TransactionResult AcceptBribe(
            float amount,
            string sourceDescription,
            string expectedFavor)
        {
            // Ensure slush fund exists
            ActivateSlushFund();
            
            var donor = new Donor
            {
                Name = sourceDescription,
                Type = DonorType.Anonymous,
                IsSuspicious = true,
                SuspiciousReason = "Potential bribery",
                ExpectedQuidProQuo = 1.0f
            };
            donor.PolicyInterests.Add(expectedFavor);
            
            var result = ProcessDonation(amount, donor, AccountType.SlushFund, false);
            
            if (result.Success)
            {
                // Always creates a violation (just not yet discovered)
                result.Violation = CreateViolation(
                    FECViolationType.WirefraudLaunderingBribery,
                    $"Accepted bribe of ${amount:N0} in exchange for: {expectedFavor}",
                    false
                );
                PendingViolations.Add(result.Violation);
                
                result.Message = $"Accepted ${amount:N0}. They expect: {expectedFavor}";
            }
            
            return result;
        }
        
        /// <summary>
        /// Pay hush money to cover something up
        /// </summary>
        public TransactionResult PayHushMoney(
            float amount,
            string recipientDescription,
            string whatTheyKnow,
            AccountType fromAccount = AccountType.SlushFund)
        {
            var result = ProcessExpense(
                amount,
                ExpenseCategory.Hush_Money,
                $"Payment to {recipientDescription}",
                fromAccount,
                isReported: false
            );
            
            if (result.Success)
            {
                result.Violation = CreateViolation(
                    FECViolationType.UnreportedExpense,
                    $"Hush money payment of ${amount:N0} to conceal: {whatTheyKnow}",
                    false
                );
                PendingViolations.Add(result.Violation);
            }
            
            return result;
        }
        
        #endregion
        
        #region FEC & Compliance
        
        /// <summary>
        /// Create a violation record
        /// </summary>
        private FECViolation CreateViolation(
            FECViolationType type, 
            string description, 
            bool isDiscovered = true)
        {
            var violation = new FECViolation
            {
                Type = type,
                Description = description,
                Occurred = DateTime.Now,
                Discovered = isDiscovered ? DateTime.Now : null,
                IsResolved = false
            };
            
            // Calculate potential fine
            violation.PotentialFine = type switch
            {
                FECViolationType.LateReporting => 500f,
                FECViolationType.InaccurateReporting => 2000f,
                FECViolationType.ExcessiveContribution => 5000f,
                FECViolationType.CorporateDonation => 10000f,
                FECViolationType.CoordinationViolation => 25000f,
                FECViolationType.ForeignDonation => 50000f,
                FECViolationType.StrawDonor => 25000f,
                FECViolationType.UnreportedExpense => 15000f,
                FECViolationType.ConversionToPersonalUse => 50000f,
                FECViolationType.WirefraudLaunderingBribery => 100000f,
                _ => 1000f
            };
            
            // Scandal risk
            violation.ScandalRisk = type switch
            {
                FECViolationType.LateReporting => 0.05f,
                FECViolationType.InaccurateReporting => 0.1f,
                FECViolationType.ExcessiveContribution => 0.2f,
                FECViolationType.ForeignDonation => 0.8f,
                FECViolationType.WirefraudLaunderingBribery => 0.95f,
                _ => 0.3f
            };
            
            // Investigation trigger
            violation.TriggeredInvestigation = type switch
            {
                FECViolationType.ForeignDonation => true,
                FECViolationType.WirefraudLaunderingBribery => true,
                FECViolationType.ConversionToPersonalUse => true,
                _ => UnityEngine.Random.value < 0.2f
            };
            
            if (isDiscovered)
            {
                Violations.Add(violation);
                OnViolationDiscovered?.Invoke(violation);
            }
            
            return violation;
        }
        
        /// <summary>
        /// Check for discovery of pending violations
        /// </summary>
        public void CheckViolationDiscovery()
        {
            foreach (var violation in PendingViolations.ToList())
            {
                float discoveryChance = 0.05f; // Base 5% per turn
                
                // Higher chance if investigation ongoing
                if (Violations.Any(v => v.TriggeredInvestigation && !v.IsResolved))
                    discoveryChance += 0.15f;
                
                // Higher for more serious violations
                discoveryChance += violation.ScandalRisk * 0.1f;
                
                if (UnityEngine.Random.value < discoveryChance)
                {
                    violation.Discovered = DateTime.Now;
                    PendingViolations.Remove(violation);
                    Violations.Add(violation);
                    OnViolationDiscovered?.Invoke(violation);
                }
            }
        }
        
        /// <summary>
        /// Pay FEC fine to resolve violation
        /// </summary>
        public TransactionResult PayFECFine(FECViolation violation)
        {
            var result = ProcessExpense(
                violation.PotentialFine,
                ExpenseCategory.FECFines,
                $"FEC Fine: {violation.Description}"
            );
            
            if (result.Success)
            {
                violation.ActualFine = violation.PotentialFine;
                violation.IsResolved = true;
            }
            
            return result;
        }
        
        /// <summary>
        /// Generate FEC report for current period
        /// </summary>
        public FECReport GenerateFECReport()
        {
            var report = new FECReport
            {
                PeriodStart = _lastFECReportTurn,
                PeriodEnd = _currentTurn,
                GeneratedAt = DateTime.Now
            };
            
            foreach (var account in Accounts.Values.Where(a => a.RequiresFECReporting))
            {
                var periodTransactions = account.Transactions
                    .Where(t => t.IsReported)
                    .ToList();
                
                report.TotalReceipts += periodTransactions
                    .Where(t => t.Amount > 0)
                    .Sum(t => t.Amount);
                    
                report.TotalDisbursements += periodTransactions
                    .Where(t => t.Amount < 0)
                    .Sum(t => Math.Abs(t.Amount));
                
                report.ReportedTransactions.AddRange(periodTransactions);
            }
            
            report.CashOnHand = Accounts.Values
                .Where(a => a.RequiresFECReporting)
                .Sum(a => a.Balance);
            
            // Check for discrepancies (unreported transactions)
            float unreportedTotal = Accounts.Values
                .SelectMany(a => a.Transactions)
                .Where(t => !t.IsReported)
                .Sum(t => Math.Abs(t.Amount));
            
            if (unreportedTotal > 0)
            {
                report.HasDiscrepancies = true;
                report.DiscrepancyAmount = unreportedTotal;
            }
            
            _lastFECReportTurn = _currentTurn;
            
            return report;
        }
        
        #endregion
        
        #region Financial Health & Analysis
        
        /// <summary>
        /// Update burn rate and financial health metrics
        /// </summary>
        private void UpdateBurnRate()
        {
            BurnRate = RecurringExpenses
                .Where(e => e.IsActive)
                .Sum(e => e.AmountPerTurn);
            
            UpdateFinancialHealth();
        }
        
        /// <summary>
        /// Update overall financial health assessment
        /// </summary>
        private void UpdateFinancialHealth()
        {
            float totalCash = Accounts.Values
                .Where(a => a.Type != AccountType.SlushFund) // Don't count illegal money
                .Sum(a => a.Balance);
            
            // Calculate runway
            if (BurnRate > 0)
            {
                TurnsOfRunway = (int)(totalCash / BurnRate);
            }
            else
            {
                TurnsOfRunway = int.MaxValue;
            }
            
            // Determine health status
            FinancialHealth newHealth;
            
            if (totalCash < 0)
            {
                newHealth = FinancialHealth.Bankrupt;
            }
            else if (TurnsOfRunway < 1)
            {
                newHealth = FinancialHealth.Crisis;
            }
            else if (TurnsOfRunway < 3)
            {
                newHealth = FinancialHealth.Struggling;
            }
            else if (TurnsOfRunway < 6)
            {
                newHealth = FinancialHealth.Tight;
            }
            else if (TurnsOfRunway < 12)
            {
                newHealth = FinancialHealth.Stable;
            }
            else if (TurnsOfRunway < 24)
            {
                newHealth = FinancialHealth.Healthy;
            }
            else
            {
                newHealth = FinancialHealth.Thriving;
            }
            
            if (newHealth != CurrentHealth)
            {
                CurrentHealth = newHealth;
                OnHealthChanged?.Invoke(CurrentHealth);
                
                if (CurrentHealth == FinancialHealth.Crisis)
                {
                    OnCashCrisis?.Invoke(totalCash);
                }
            }
        }
        
        /// <summary>
        /// Get summary of current financial state
        /// </summary>
        public FinancialSummary GetSummary()
        {
            return new FinancialSummary
            {
                TotalCash = Accounts.Values.Sum(a => a.Balance),
                CampaignFundBalance = Accounts.TryGetValue(AccountType.CampaignFund, out var cf) ? cf.Balance : 0,
                PersonalFundBalance = Accounts.TryGetValue(AccountType.PersonalFund, out var pf) ? pf.Balance : 0,
                PACBalance = Accounts.TryGetValue(AccountType.PAC, out var pac) ? pac.Balance : 0,
                DarkMoneyBalance = Accounts.TryGetValue(AccountType.DarkMoney, out var dm) ? dm.Balance : 0,
                SlushFundBalance = Accounts.TryGetValue(AccountType.SlushFund, out var sf) ? sf.Balance : 0,
                
                MonthlyBurnRate = BurnRate,
                TurnsOfRunway = TurnsOfRunway,
                Health = CurrentHealth,
                
                TotalRaisedAllTime = TotalRaisedAllTime,
                TotalSpentAllTime = TotalSpentAllTime,
                
                ActiveDonorCount = Donors.Count,
                MaxedOutDonorCount = Donors.Values.Count(d => d.IsMaxedOut),
                
                PendingViolationCount = PendingViolations.Count,
                ActiveViolationCount = Violations.Count(v => !v.IsResolved),
                TotalFinesPaid = Violations.Sum(v => v.ActualFine)
            };
        }
        
        #endregion
        
        #region Turn Processing
        
        /// <summary>
        /// Process end of turn financial updates
        /// </summary>
        public void ProcessTurn()
        {
            _currentTurn++;
            
            // Process recurring expenses
            ProcessRecurringExpenses();
            
            // Check for violation discovery
            CheckViolationDiscovery();
            
            // Update health
            UpdateFinancialHealth();
            
            // FEC reporting period check
            if (_currentTurn - _lastFECReportTurn >= FECReportingPeriodTurns)
            {
                // Auto-generate report, check for late filing
                var report = GenerateFECReport();
                
                if (report.HasDiscrepancies)
                {
                    CreateViolation(
                        FECViolationType.InaccurateReporting,
                        $"FEC report discrepancy of ${report.DiscrepancyAmount:N0}"
                    );
                }
            }
        }
        
        #endregion
    }
    
    #endregion
    
    #region Supporting Data Types
    public class FundraisingEvent
    {
        public string Name;
        public float Cost;
        public float TicketPrice;
        public int ExpectedAttendance;
        public float HighDollarRatio;           // % who give more than ticket price
        public string TargetDemographic;
        public bool SpecialInterestPresent;
        public string SpecialInterestType;
    }
    
    public class FundraisingResult
    {
        public bool Success;
        public string Message;
        public string EventName;
        public float EventCost;
        public float TotalRaised;
        public float NetRaised;
        public int DonorCount;
        public int Attendance;
        public List<string> DonorIds = new();
        public List<string> QuidProQuoExpectations = new();
    }
    
    public class FECReport
    {
        public int PeriodStart;
        public int PeriodEnd;
        public DateTime GeneratedAt;
        public float TotalReceipts;
        public float TotalDisbursements;
        public float CashOnHand;
        public List<Transaction> ReportedTransactions = new();
        public bool HasDiscrepancies;
        public float DiscrepancyAmount;
    }
    
    public class FinancialSummary
    {
        public float TotalCash;
        public float CampaignFundBalance;
        public float PersonalFundBalance;
        public float PACBalance;
        public float DarkMoneyBalance;
        public float SlushFundBalance;
        public float MonthlyBurnRate;
        public int TurnsOfRunway;
        public FinancialHealth Health;
        public float TotalRaisedAllTime;
        public float TotalSpentAllTime;
        public int ActiveDonorCount;
        public int MaxedOutDonorCount;
        public int PendingViolationCount;
        public int ActiveViolationCount;
        public float TotalFinesPaid;
    }
    
    #endregion
}
