// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - ECONOMY MANAGER
// Core manager for all currency operations, purchases, and Purrkoin transactions
// ═══════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace ElectionEmpire.Economy
{
    /// <summary>
    /// Central manager for all economy operations
    /// </summary>
    public class EconomyManager
    {
        #region Singleton
        
        private static EconomyManager _instance;
        public static EconomyManager Instance => _instance ??= new EconomyManager();
        
        #endregion
        
        #region State
        
        private EconomyConfig _config;
        private Dictionary<string, PlayerWallet> _wallets;
        private Dictionary<string, PlayerInventory> _inventories;
        private Dictionary<string, List<Transaction>> _transactionHistory;
        private Dictionary<string, StoreItem> _storeItems;
        private Dictionary<string, PurrkoinPackage> _iapPackages;
        private List<DailyLoginReward> _loginRewards;
        private SeasonPass _currentSeasonPass;
        private Dictionary<string, PlayerSeasonProgress> _seasonProgress;
        private Dictionary<string, PlayerSpendingProfile> _spendingProfiles;
        
        // Earning tracking
        private Dictionary<string, Dictionary<string, int>> _dailyEarnings; // playerId -> {source -> count}
        private Dictionary<string, long> _weeklyEarnings; // playerId -> total
        
        #endregion
        
        #region Events
        
        public event Action<CurrencyChangedEvent> OnCurrencyChanged;
        public event Action<PurchaseCompletedEvent> OnPurchaseCompleted;
        public event Action<ItemUnlockedEvent> OnItemUnlocked;
        public event Action<string, int> OnDailyLoginRewardClaimed;
        public event Action<string, int> OnSeasonPassLevelUp;
        public event Action<IAPPurchase> OnIAPCompleted;
        
        #endregion
        
        #region Initialization
        
        public EconomyManager()
        {
            _config = new EconomyConfig();
            _wallets = new Dictionary<string, PlayerWallet>();
            _inventories = new Dictionary<string, PlayerInventory>();
            _transactionHistory = new Dictionary<string, List<Transaction>>();
            _storeItems = new Dictionary<string, StoreItem>();
            _iapPackages = new Dictionary<string, PurrkoinPackage>();
            _seasonProgress = new Dictionary<string, PlayerSeasonProgress>();
            _spendingProfiles = new Dictionary<string, PlayerSpendingProfile>();
            _dailyEarnings = new Dictionary<string, Dictionary<string, int>>();
            _weeklyEarnings = new Dictionary<string, long>();
            
            InitializeLoginRewards();
            InitializeDefaultStore();
            InitializePurrkoinPackages();
        }
        
        public void Initialize(EconomyConfig config = null)
        {
            if (config != null)
            {
                _config = config;
            }
        }
        
        private void InitializeLoginRewards()
        {
            _loginRewards = new List<DailyLoginReward>();
            
            for (int day = 1; day <= 30; day++)
            {
                var reward = new DailyLoginReward
                {
                    Day = day,
                    Currency = CurrencyType.CloutBux,
                    IsMilestone = day == 7 || day == 14 || day == 21 || day == 30
                };
                
                // Progressive rewards
                if (day == 7)
                {
                    reward.Amount = EarningRates.DailyLoginStreak7;
                    reward.BonusItemId = "cosmetic_week1_frame";
                }
                else if (day == 14)
                {
                    reward.Amount = 200;
                    reward.BonusItemId = "cosmetic_week2_outfit";
                }
                else if (day == 21)
                {
                    reward.Amount = 350;
                    reward.BonusItemId = "cosmetic_week3_accessory";
                }
                else if (day == 30)
                {
                    reward.Amount = EarningRates.DailyLoginStreak30;
                    reward.Currency = CurrencyType.Purrkoin;
                    reward.Amount = 50; // Special: Purrkoin on day 30
                    reward.BonusItemId = "cosmetic_month_legendary";
                }
                else
                {
                    reward.Amount = EarningRates.DailyLogin + (day * 2); // Slight increase each day
                }
                
                _loginRewards.Add(reward);
            }
        }
        
        private void InitializeDefaultStore()
        {
            // Cosmetics
            AddStoreItem(new StoreItem
            {
                Name = "Presidential Portrait",
                Description = "A distinguished portrait befitting a future leader",
                Category = PurchaseCategory.Cosmetic,
                CosmeticType = CosmeticType.Portrait,
                Rarity = ItemRarity.Rare,
                PrimaryCurrency = CurrencyType.CloutBux,
                PrimaryPrice = 500
            });
            
            AddStoreItem(new StoreItem
            {
                Name = "Power Suit - Navy",
                Description = "The classic power look for any politician",
                Category = PurchaseCategory.Cosmetic,
                CosmeticType = CosmeticType.Outfit,
                Rarity = ItemRarity.Common,
                PrimaryCurrency = CurrencyType.CloutBux,
                PrimaryPrice = 250
            });
            
            AddStoreItem(new StoreItem
            {
                Name = "Golden Microphone",
                Description = "Because your voice deserves the best",
                Category = PurchaseCategory.Cosmetic,
                CosmeticType = CosmeticType.Accessory,
                Rarity = ItemRarity.Epic,
                PrimaryCurrency = CurrencyType.Purrkoin,
                PrimaryPrice = 150
            });
            
            // Content packs
            AddStoreItem(new StoreItem
            {
                Name = "Political Machines DLC",
                Description = "Unlock historical political machine gameplay",
                Category = PurchaseCategory.ContentPack,
                Rarity = ItemRarity.Legendary,
                PrimaryCurrency = CurrencyType.Purrkoin,
                PrimaryPrice = 500,
                AlternateCurrency = null,
                RealMoneyPrice = 9.99m
            });
            
            // Backgrounds
            AddStoreItem(new StoreItem
            {
                Name = "Military Veteran Background",
                Description = "Start your political career as a decorated veteran",
                Category = PurchaseCategory.Background,
                Rarity = ItemRarity.Epic,
                PrimaryCurrency = CurrencyType.Purrkoin,
                PrimaryPrice = 300
            });
            
            // Tournament entries
            AddStoreItem(new StoreItem
            {
                Name = "Championship Entry Token",
                Description = "Entry to the next Championship tournament",
                Category = PurchaseCategory.TournamentEntry,
                Rarity = ItemRarity.Rare,
                PrimaryCurrency = CurrencyType.Purrkoin,
                PrimaryPrice = 100,
                MaxPurchases = 1
            });
        }
        
        private void InitializePurrkoinPackages()
        {
            _iapPackages["pk_starter"] = new PurrkoinPackage
            {
                Id = "pk_starter",
                Name = "Starter Pack",
                PurrkoinAmount = 100,
                BonusPurrkoin = 0,
                PriceUSD = 0.99m,
                PlatformProductId = "com.electionempire.pk100"
            };
            
            _iapPackages["pk_small"] = new PurrkoinPackage
            {
                Id = "pk_small",
                Name = "Small Bundle",
                PurrkoinAmount = 500,
                BonusPurrkoin = 50,
                PriceUSD = 4.99m,
                PlatformProductId = "com.electionempire.pk550"
            };
            
            _iapPackages["pk_medium"] = new PurrkoinPackage
            {
                Id = "pk_medium",
                Name = "Medium Bundle",
                PurrkoinAmount = 1200,
                BonusPurrkoin = 200,
                PriceUSD = 9.99m,
                PlatformProductId = "com.electionempire.pk1400",
                IsPopular = true
            };
            
            _iapPackages["pk_large"] = new PurrkoinPackage
            {
                Id = "pk_large",
                Name = "Large Bundle",
                PurrkoinAmount = 2800,
                BonusPurrkoin = 700,
                PriceUSD = 19.99m,
                PlatformProductId = "com.electionempire.pk3500"
            };
            
            _iapPackages["pk_mega"] = new PurrkoinPackage
            {
                Id = "pk_mega",
                Name = "Mega Bundle",
                PurrkoinAmount = 6500,
                BonusPurrkoin = 2000,
                PriceUSD = 49.99m,
                PlatformProductId = "com.electionempire.pk8500",
                IsBestValue = true
            };
            
            _iapPackages["pk_whale"] = new PurrkoinPackage
            {
                Id = "pk_whale",
                Name = "Campaign War Chest",
                PurrkoinAmount = 15000,
                BonusPurrkoin = 5000,
                PriceUSD = 99.99m,
                PlatformProductId = "com.electionempire.pk20000"
            };
        }
        
        private void AddStoreItem(StoreItem item)
        {
            if (string.IsNullOrEmpty(item.Id))
            {
                item.Id = Guid.NewGuid().ToString();
            }
            _storeItems[item.Id] = item;
        }
        
        #endregion
        
        #region Wallet Management
        
        /// <summary>
        /// Get or create a player's wallet
        /// </summary>
        public PlayerWallet GetWallet(string playerId)
        {
            if (!_wallets.TryGetValue(playerId, out var wallet))
            {
                wallet = new PlayerWallet(playerId);
                _wallets[playerId] = wallet;
                _transactionHistory[playerId] = new List<Transaction>();
            }
            return wallet;
        }
        
        /// <summary>
        /// Get player's inventory
        /// </summary>
        public PlayerInventory GetInventory(string playerId)
        {
            if (!_inventories.TryGetValue(playerId, out var inventory))
            {
                inventory = new PlayerInventory(playerId);
                _inventories[playerId] = inventory;
            }
            return inventory;
        }
        
        /// <summary>
        /// Get player's balance for a currency
        /// </summary>
        public long GetBalance(string playerId, CurrencyType currency)
        {
            var wallet = GetWallet(playerId);
            return currency switch
            {
                CurrencyType.CloutBux => wallet.CloutBux,
                CurrencyType.Purrkoin => wallet.Purrkoin,
                CurrencyType.TournamentTokens => wallet.TournamentTokens,
                CurrencyType.SeasonCredits => wallet.SeasonCredits,
                _ => 0
            };
        }
        
        #endregion
        
        #region Currency Operations
        
        /// <summary>
        /// Grant currency to a player
        /// </summary>
        public TransactionResult GrantCurrency(string playerId, CurrencyType currency, long amount, 
            EconomyTransactionType type, string description, string referenceId = null)
        {
            if (amount <= 0)
            {
                return new TransactionResult { Success = false, Error = "Amount must be positive" };
            }
            
            // Check daily/weekly caps for CloutBux
            if (currency == CurrencyType.CloutBux && type != EconomyTransactionType.AdminGrant)
            {
                if (!CanEarnMoreCloutBux(playerId, amount))
                {
                    return new TransactionResult { Success = false, Error = "Daily/weekly earning cap reached" };
                }
            }
            
            var wallet = GetWallet(playerId);
            long previousBalance = GetBalance(playerId, currency);
            
            // Apply the change
            switch (currency)
            {
                case CurrencyType.CloutBux:
                    wallet.CloutBux += amount;
                    wallet.LifetimeCloutBuxEarned += amount;
                    break;
                case CurrencyType.Purrkoin:
                    wallet.Purrkoin += amount;
                    wallet.LifetimePurrkoinEarned += amount;
                    break;
                case CurrencyType.TournamentTokens:
                    wallet.TournamentTokens += amount;
                    break;
                case CurrencyType.SeasonCredits:
                    wallet.SeasonCredits += amount;
                    break;
            }
            
            wallet.LastUpdated = DateTime.UtcNow;
            
            // Record transaction
            var transaction = new Transaction
            {
                PlayerId = playerId,
                Currency = currency,
                Type = type,
                Amount = amount,
                BalanceBefore = previousBalance,
                BalanceAfter = GetBalance(playerId, currency),
                Description = description,
                ReferenceId = referenceId,
                Verified = true,
                VerificationHash = GenerateTransactionHash(playerId, currency, amount)
            };
            
            RecordTransaction(playerId, transaction);
            TrackEarnings(playerId, type.ToString(), amount);
            
            // Fire event
            OnCurrencyChanged?.Invoke(new CurrencyChangedEvent
            {
                PlayerId = playerId,
                Currency = currency,
                PreviousBalance = previousBalance,
                NewBalance = transaction.BalanceAfter,
                ChangeAmount = amount,
                Type = type,
                Description = description,
                Timestamp = DateTime.UtcNow
            });
            
            return new TransactionResult 
            { 
                Success = true, 
                TransactionId = transaction.Id,
                NewBalance = transaction.BalanceAfter
            };
        }
        
        /// <summary>
        /// Deduct currency from a player
        /// </summary>
        public TransactionResult DeductCurrency(string playerId, CurrencyType currency, long amount,
            EconomyTransactionType type, string description, string referenceId = null)
        {
            if (amount <= 0)
            {
                return new TransactionResult { Success = false, Error = "Amount must be positive" };
            }
            
            var wallet = GetWallet(playerId);
            
            if (!wallet.CanAfford(currency, amount))
            {
                return new TransactionResult { Success = false, Error = "Insufficient funds" };
            }
            
            long previousBalance = GetBalance(playerId, currency);
            
            // Apply the change
            switch (currency)
            {
                case CurrencyType.CloutBux:
                    wallet.CloutBux -= amount;
                    break;
                case CurrencyType.Purrkoin:
                    wallet.Purrkoin -= amount;
                    wallet.LifetimePurrkoinSpent += amount;
                    break;
                case CurrencyType.TournamentTokens:
                    wallet.TournamentTokens -= amount;
                    break;
                case CurrencyType.SeasonCredits:
                    wallet.SeasonCredits -= amount;
                    break;
            }
            
            wallet.LastUpdated = DateTime.UtcNow;
            
            // Record transaction
            var transaction = new Transaction
            {
                PlayerId = playerId,
                Currency = currency,
                Type = type,
                Amount = -amount,
                BalanceBefore = previousBalance,
                BalanceAfter = GetBalance(playerId, currency),
                Description = description,
                ReferenceId = referenceId,
                Verified = true,
                VerificationHash = GenerateTransactionHash(playerId, currency, -amount)
            };
            
            RecordTransaction(playerId, transaction);
            
            // Fire event
            OnCurrencyChanged?.Invoke(new CurrencyChangedEvent
            {
                PlayerId = playerId,
                Currency = currency,
                PreviousBalance = previousBalance,
                NewBalance = transaction.BalanceAfter,
                ChangeAmount = -amount,
                Type = type,
                Description = description,
                Timestamp = DateTime.UtcNow
            });
            
            return new TransactionResult 
            { 
                Success = true, 
                TransactionId = transaction.Id,
                NewBalance = transaction.BalanceAfter
            };
        }
        
        private bool CanEarnMoreCloutBux(string playerId, long amount)
        {
            // Check daily cap
            if (!_dailyEarnings.TryGetValue(playerId, out var daily))
            {
                daily = new Dictionary<string, int>();
                _dailyEarnings[playerId] = daily;
            }
            
            long todayTotal = daily.Values.Sum();
            if (todayTotal + amount > _config.DailyCloutBuxCap)
            {
                return false;
            }
            
            // Check weekly cap
            if (!_weeklyEarnings.TryGetValue(playerId, out var weeklyTotal))
            {
                weeklyTotal = 0;
            }
            
            if (weeklyTotal + amount > _config.WeeklyCloutBuxCap)
            {
                return false;
            }
            
            return true;
        }
        
        private void TrackEarnings(string playerId, string source, long amount)
        {
            if (!_dailyEarnings.TryGetValue(playerId, out var daily))
            {
                daily = new Dictionary<string, int>();
                _dailyEarnings[playerId] = daily;
            }
            
            if (!daily.ContainsKey(source))
            {
                daily[source] = 0;
            }
            daily[source] += (int)amount;
            
            if (!_weeklyEarnings.ContainsKey(playerId))
            {
                _weeklyEarnings[playerId] = 0;
            }
            _weeklyEarnings[playerId] += amount;
        }
        
        private void RecordTransaction(string playerId, Transaction transaction)
        {
            if (!_transactionHistory.TryGetValue(playerId, out var history))
            {
                history = new List<Transaction>();
                _transactionHistory[playerId] = history;
            }
            history.Add(transaction);
        }
        
        private string GenerateTransactionHash(string playerId, CurrencyType currency, long amount)
        {
            string input = $"{playerId}|{currency}|{amount}|{DateTime.UtcNow.Ticks}|ELECTION_EMPIRE_SALT";
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(bytes);
        }
        
        #endregion
        
        #region Store Operations
        
        /// <summary>
        /// Get all available store items
        /// </summary>
        public List<StoreItem> GetStoreItems(PurchaseCategory? category = null)
        {
            var items = _storeItems.Values.Where(i => i.IsCurrentlyAvailable());
            
            if (category.HasValue)
            {
                items = items.Where(i => i.Category == category.Value);
            }
            
            return items.ToList();
        }
        
        /// <summary>
        /// Purchase an item from the store
        /// </summary>
        public PurchaseResult PurchaseItem(string playerId, string itemId, CurrencyType? preferredCurrency = null)
        {
            if (!_storeItems.TryGetValue(itemId, out var item))
            {
                return new PurchaseResult { Success = false, Error = "Item not found" };
            }
            
            if (!item.IsCurrentlyAvailable())
            {
                return new PurchaseResult { Success = false, Error = "Item is not available" };
            }
            
            var inventory = GetInventory(playerId);
            
            // Check if already owned (for non-consumables)
            if (item.Category != PurchaseCategory.TournamentEntry && inventory.OwnsItem(itemId))
            {
                return new PurchaseResult { Success = false, Error = "Item already owned" };
            }
            
            // Determine which currency to use
            CurrencyType currency = preferredCurrency ?? item.PrimaryCurrency;
            long price;
            
            if (currency == item.PrimaryCurrency)
            {
                price = item.PrimaryPrice;
            }
            else if (item.AlternateCurrency.HasValue && currency == item.AlternateCurrency.Value)
            {
                price = item.AlternatePrice ?? item.PrimaryPrice;
            }
            else
            {
                return new PurchaseResult { Success = false, Error = "Invalid currency for this item" };
            }
            
            // Check affordability
            var wallet = GetWallet(playerId);
            if (!wallet.CanAfford(currency, price))
            {
                return new PurchaseResult { Success = false, Error = "Insufficient funds" };
            }
            
            // Deduct currency
            var deductResult = DeductCurrency(playerId, currency, price, 
                EconomyTransactionType.Purchase, $"Purchased: {item.Name}", itemId);
            
            if (!deductResult.Success)
            {
                return new PurchaseResult { Success = false, Error = deductResult.Error };
            }
            
            // Create purchase record
            var purchase = new Purchase
            {
                PlayerId = playerId,
                ItemId = itemId,
                ItemName = item.Name,
                CurrencyUsed = currency,
                AmountPaid = price
            };
            
            // Add to inventory
            AddItemToInventory(playerId, item, "store");
            
            // Update stock if limited
            if (item.GlobalStock.HasValue)
            {
                item.CurrentStock--;
            }
            
            // Fire event
            OnPurchaseCompleted?.Invoke(new PurchaseCompletedEvent
            {
                PlayerId = playerId,
                Purchase = purchase,
                Item = item,
                Timestamp = DateTime.UtcNow
            });
            
            return new PurchaseResult
            {
                Success = true,
                Purchase = purchase
            };
        }
        
        private void AddItemToInventory(string playerId, StoreItem item, string source)
        {
            var inventory = GetInventory(playerId);
            
            var ownedItem = new OwnedItem
            {
                ItemId = item.Id,
                ItemName = item.Name,
                Type = item.CosmeticType,
                Rarity = item.Rarity,
                AcquiredAt = DateTime.UtcNow,
                AcquisitionSource = source,
                IsTradeable = _config.AllowTrading,
                IsGiftable = _config.AllowGifting
            };
            
            inventory.Items.Add(ownedItem);
            
            // Handle special unlocks
            if (item.Category == PurchaseCategory.Background)
            {
                inventory.UnlockedBackgrounds.Add(item.ContentId ?? item.Id);
            }
            else if (item.Category == PurchaseCategory.Trait)
            {
                inventory.UnlockedTraits.Add(item.ContentId ?? item.Id);
            }
            else if (item.Category == PurchaseCategory.ContentPack)
            {
                inventory.OwnedContentPacks.Add(item.ContentId ?? item.Id);
            }
            
            // Fire unlock event
            OnItemUnlocked?.Invoke(new ItemUnlockedEvent
            {
                PlayerId = playerId,
                Item = ownedItem,
                UnlockSource = source,
                Timestamp = DateTime.UtcNow
            });
        }
        
        #endregion
        
        #region IAP / Purrkoin Purchase
        
        /// <summary>
        /// Get available Purrkoin packages
        /// </summary>
        public List<PurrkoinPackage> GetPurrkoinPackages()
        {
            return _iapPackages.Values.ToList();
        }
        
        /// <summary>
        /// Process a real-money Purrkoin purchase
        /// </summary>
        public IAPResult ProcessIAPPurchase(string playerId, string packageId, 
            string platformTransactionId, string platform, string receipt)
        {
            if (!_iapPackages.TryGetValue(packageId, out var package))
            {
                return new IAPResult { Success = false, Error = "Package not found" };
            }
            
            // Check spending limits
            if (_config.EnableSpendingLimits)
            {
                var profile = GetSpendingProfile(playerId);
                if (profile.HasOptedIntoLimits && 
                    profile.CurrentMonthSpending + package.PriceUSD > profile.MonthlySpendingLimit)
                {
                    return new IAPResult 
                    { 
                        Success = false, 
                        Error = "Monthly spending limit would be exceeded" 
                    };
                }
            }
            
            // TODO: Verify receipt with platform (Steam, iOS, Android)
            // This would be a server-side call in production
            bool receiptValid = VerifyPurchaseReceipt(platform, receipt);
            
            if (!receiptValid)
            {
                return new IAPResult { Success = false, Error = "Receipt verification failed" };
            }
            
            // Create IAP record
            var iapPurchase = new IAPPurchase
            {
                PlayerId = playerId,
                PackageId = packageId,
                PlatformTransactionId = platformTransactionId,
                Platform = platform,
                AmountPaid = package.PriceUSD,
                Currency = "USD",
                PurrkoinGranted = package.TotalPurrkoin,
                IsVerified = true,
                VerificationReceipt = receipt
            };
            
            // Grant Purrkoin
            var grantResult = GrantCurrency(playerId, CurrencyType.Purrkoin, 
                package.TotalPurrkoin, EconomyTransactionType.RealMoneyPurchase,
                $"IAP: {package.Name}", iapPurchase.Id);
            
            if (!grantResult.Success)
            {
                return new IAPResult { Success = false, Error = grantResult.Error };
            }
            
            // Update spending profile
            UpdateSpendingProfile(playerId, package.PriceUSD);
            
            // Update wallet stats
            var wallet = GetWallet(playerId);
            wallet.LifetimeRealMoneySpent += package.PriceUSD;
            
            // Fire event
            OnIAPCompleted?.Invoke(iapPurchase);
            
            return new IAPResult
            {
                Success = true,
                PurrkoinGranted = package.TotalPurrkoin,
                Purchase = iapPurchase
            };
        }
        
        private bool VerifyPurchaseReceipt(string platform, string receipt)
        {
            // In production, this would call the appropriate platform API
            // Steam, Apple App Store, Google Play, etc.
            // For now, always return true in development
            Debug.Log($"[Economy] Verifying {platform} receipt (mock verification)");
            return true;
        }
        
        private PlayerSpendingProfile GetSpendingProfile(string playerId)
        {
            if (!_spendingProfiles.TryGetValue(playerId, out var profile))
            {
                profile = new PlayerSpendingProfile
                {
                    PlayerId = playerId,
                    Tier = SpenderTier.NonSpender,
                    MonthlySpendingLimit = _config.DefaultMonthlyLimit
                };
                _spendingProfiles[playerId] = profile;
            }
            return profile;
        }
        
        private void UpdateSpendingProfile(string playerId, decimal amount)
        {
            var profile = GetSpendingProfile(playerId);
            
            profile.TotalSpent += amount;
            profile.TotalPurchases++;
            profile.CurrentMonthSpending += amount;
            profile.LastPurchase = DateTime.UtcNow;
            
            if (!profile.FirstPurchase.HasValue)
            {
                profile.FirstPurchase = DateTime.UtcNow;
            }
            
            // Update tier
            if (profile.TotalSpent >= 200)
                profile.Tier = SpenderTier.MegaWhale;
            else if (profile.TotalSpent >= 50)
                profile.Tier = SpenderTier.Whale;
            else if (profile.TotalSpent >= 10)
                profile.Tier = SpenderTier.Dolphin;
            else
                profile.Tier = SpenderTier.Minnow;
        }
        
        /// <summary>
        /// Set a monthly spending limit (responsible gaming)
        /// </summary>
        public void SetSpendingLimit(string playerId, decimal monthlyLimit)
        {
            var profile = GetSpendingProfile(playerId);
            profile.MonthlySpendingLimit = Math.Min(monthlyLimit, _config.MaxMonthlyLimit);
            profile.HasOptedIntoLimits = true;
        }
        
        #endregion
        
        #region Tournament Integration
        
        /// <summary>
        /// Process tournament entry fee
        /// </summary>
        public TransactionResult ProcessTournamentEntry(string playerId, TournamentEntryFee fee)
        {
            return DeductCurrency(playerId, fee.Currency, fee.Amount,
                EconomyTransactionType.EntryFee, $"Tournament entry: {fee.TournamentId}", fee.TournamentId);
        }
        
        /// <summary>
        /// Refund tournament entry fee
        /// </summary>
        public TransactionResult RefundTournamentEntry(string playerId, TournamentEntryFee fee)
        {
            if (!fee.IsRefundable || DateTime.UtcNow > fee.RefundDeadline)
            {
                return new TransactionResult { Success = false, Error = "Refund not available" };
            }
            
            long refundAmount = (long)(fee.Amount * fee.RefundPercentage / 100f);
            return GrantCurrency(playerId, fee.Currency, refundAmount,
                EconomyTransactionType.Refund, $"Tournament refund: {fee.TournamentId}", fee.TournamentId);
        }
        
        /// <summary>
        /// Distribute tournament prizes
        /// </summary>
        public void DistributeTournamentPrizes(TournamentPrizePool prizePool, 
            Dictionary<string, int> playerPlacements)
        {
            foreach (var (playerId, placement) in playerPlacements)
            {
                foreach (var prizePlacement in prizePool.Placements)
                {
                    if (placement >= prizePlacement.MinPlace && placement <= prizePlacement.MaxPlace)
                    {
                        long prizeAmount = prizePlacement.CalculatePrize(prizePool.TotalPool);
                        
                        // Grant prize currency
                        GrantCurrency(playerId, prizePool.Currency, prizeAmount,
                            EconomyTransactionType.TournamentReward,
                            $"Tournament prize: {prizePlacement.Title} (#{placement})",
                            prizePool.TournamentId);
                        
                        // Grant bonus items
                        foreach (var itemId in prizePlacement.BonusItemIds)
                        {
                            if (_storeItems.TryGetValue(itemId, out var item))
                            {
                                AddItemToInventory(playerId, item, $"tournament_{prizePool.TournamentId}");
                            }
                        }
                        
                        break; // Only one placement reward per player
                    }
                }
            }
        }
        
        #endregion
        
        #region Gameplay Rewards
        
        /// <summary>
        /// Reward for winning an election
        /// </summary>
        public TransactionResult RewardElectionVictory(string playerId, int officeTier)
        {
            long reward = officeTier switch
            {
                1 => EarningRates.Tier1Victory,
                2 => EarningRates.Tier2Victory,
                3 => EarningRates.Tier3Victory,
                4 => EarningRates.Tier4Victory,
                5 => EarningRates.Tier5Victory,
                _ => 50
            };
            
            return GrantCurrency(playerId, CurrencyType.CloutBux, reward,
                EconomyTransactionType.Earned, $"Tier {officeTier} election victory");
        }
        
        /// <summary>
        /// Reward for surviving a scandal
        /// </summary>
        public TransactionResult RewardScandalSurvival(string playerId)
        {
            return GrantCurrency(playerId, CurrencyType.CloutBux, EarningRates.ScandalSurvived,
                EconomyTransactionType.Earned, "Survived scandal");
        }
        
        /// <summary>
        /// Reward for managing a crisis
        /// </summary>
        public TransactionResult RewardCrisisManagement(string playerId)
        {
            return GrantCurrency(playerId, CurrencyType.CloutBux, EarningRates.CrisisManaged,
                EconomyTransactionType.Earned, "Successfully managed crisis");
        }
        
        /// <summary>
        /// Process daily login
        /// </summary>
        public DailyLoginResult ProcessDailyLogin(string playerId, int consecutiveDay)
        {
            if (consecutiveDay < 1 || consecutiveDay > 30)
            {
                consecutiveDay = ((consecutiveDay - 1) % 30) + 1;
            }
            
            var reward = _loginRewards.FirstOrDefault(r => r.Day == consecutiveDay);
            if (reward == null)
            {
                return new DailyLoginResult { Success = false, Error = "Invalid day" };
            }
            
            // Grant currency
            GrantCurrency(playerId, reward.Currency, reward.Amount,
                EconomyTransactionType.DailyLogin, $"Daily login day {consecutiveDay}");
            
            // Grant bonus item if any
            if (!string.IsNullOrEmpty(reward.BonusItemId) && _storeItems.TryGetValue(reward.BonusItemId, out var item))
            {
                AddItemToInventory(playerId, item, "daily_login");
            }
            
            OnDailyLoginRewardClaimed?.Invoke(playerId, consecutiveDay);
            
            return new DailyLoginResult
            {
                Success = true,
                Day = consecutiveDay,
                CurrencyRewarded = reward.Currency,
                AmountRewarded = reward.Amount,
                BonusItemUnlocked = reward.BonusItemId,
                IsMilestone = reward.IsMilestone
            };
        }
        
        /// <summary>
        /// Reward for completing an achievement
        /// </summary>
        public TransactionResult RewardAchievement(string playerId, string achievementId, ItemRarity difficulty)
        {
            long reward = difficulty switch
            {
                ItemRarity.Common => EarningRates.AchievementEasy,
                ItemRarity.Uncommon => EarningRates.AchievementEasy,
                ItemRarity.Rare => EarningRates.AchievementMedium,
                ItemRarity.Epic => EarningRates.AchievementHard,
                ItemRarity.Legendary => EarningRates.AchievementExtreme,
                _ => 50
            };
            
            return GrantCurrency(playerId, CurrencyType.CloutBux, reward,
                EconomyTransactionType.Achievement, $"Achievement: {achievementId}", achievementId);
        }
        
        #endregion
        
        #region Daily/Weekly Reset
        
        /// <summary>
        /// Call at start of new day (server time)
        /// </summary>
        public void OnNewDay()
        {
            _dailyEarnings.Clear();
            Debug.Log("[Economy] Daily earnings reset");
        }
        
        /// <summary>
        /// Call at start of new week (server time)
        /// </summary>
        public void OnNewWeek()
        {
            _weeklyEarnings.Clear();
            
            // Reset monthly spending for new month (if applicable)
            if (DateTime.UtcNow.Day <= 7)
            {
                foreach (var profile in _spendingProfiles.Values)
                {
                    profile.CurrentMonthSpending = 0;
                }
            }
            
            Debug.Log("[Economy] Weekly earnings reset");
        }
        
        #endregion
        
        #region Transaction History
        
        /// <summary>
        /// Get transaction history for a player
        /// </summary>
        public List<Transaction> GetTransactionHistory(string playerId, int limit = 50)
        {
            if (!_transactionHistory.TryGetValue(playerId, out var history))
            {
                return new List<Transaction>();
            }
            
            return history.OrderByDescending(t => t.Timestamp).Take(limit).ToList();
        }
        
        #endregion
    }
    
    #region Result Classes
    
    /// <summary>
    /// Result of a currency transaction
    /// </summary>
    public class TransactionResult
    {
        public bool Success;
        public string Error;
        public long NewBalance;
        public Transaction Transaction;
        public string TransactionId;
    }

    /// <summary>
    /// Represents a currency transaction record
    /// </summary>
    [Serializable]
    public class Transaction
    {
        public string TransactionId;
        public string Id { get => TransactionId; set => TransactionId = value; }
        public string PlayerId;
        public CurrencyType Currency;
        public long Amount;
        public long BalanceBefore;
        public long BalanceAfter;
        public EconomyTransactionType Type;
        public string Description;
        public string ReferenceId;
        public DateTime Timestamp;
        public bool IsDebit;
        public bool Verified;
        public string VerificationHash;

        public Transaction()
        {
            TransactionId = Guid.NewGuid().ToString();
            Timestamp = DateTime.UtcNow;
        }
    }
    
    /// <summary>
    /// Result of a store purchase
    /// </summary>
    public class PurchaseResult
    {
        public bool Success;
        public string Error;
        public Purchase Purchase;
    }
    
    /// <summary>
    /// Result of an IAP purchase
    /// </summary>
    public class IAPResult
    {
        public bool Success;
        public string Error;
        public long PurrkoinGranted;
        public IAPPurchase Purchase;
    }
    
    /// <summary>
    /// Result of daily login
    /// </summary>
    public class DailyLoginResult
    {
        public bool Success;
        public string Error;
        public int Day;
        public CurrencyType CurrencyRewarded;
        public long AmountRewarded;
        public string BonusItemUnlocked;
        public bool IsMilestone;
    }
    
    #endregion
}
