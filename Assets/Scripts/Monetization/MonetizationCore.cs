// ELECTION EMPIRE - Monetization Core System
// Sprint 11: Currency management, IAP integration, and economy balancing

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using ElectionEmpire.Economy;

namespace ElectionEmpire.Monetization
{
    #region Data Structures
    
    /// <summary>
    /// Player's currency wallet.
    /// </summary>
    [Serializable]
    public class CurrencyWallet
    {
        public long CloutBux;
        public long Purrkoin;
        public DateTime LastUpdated;
        
        // Transaction history
        public List<CurrencyTransaction> RecentTransactions;
        
        public CurrencyWallet()
        {
            RecentTransactions = new List<CurrencyTransaction>();
        }
        
        public long GetBalance(CurrencyType type)
        {
            return type switch
            {
                CurrencyType.CloutBux => CloutBux,
                CurrencyType.Purrkoin => Purrkoin,
                _ => 0
            };
        }
        
        public void AddCurrency(CurrencyType type, long amount)
        {
            switch (type)
            {
                case CurrencyType.CloutBux:
                    CloutBux += amount;
                    break;
                case CurrencyType.Purrkoin:
                    Purrkoin += amount;
                    break;
            }
            LastUpdated = DateTime.UtcNow;
        }
        
        public bool SpendCurrency(CurrencyType type, long amount)
        {
            if (GetBalance(type) < amount) return false;

            switch (type)
            {
                case CurrencyType.CloutBux:
                    CloutBux -= amount;
                    break;
                case CurrencyType.Purrkoin:
                    Purrkoin -= amount;
                    break;
            }
            LastUpdated = DateTime.UtcNow;
            return true;
        }

        public void SetBalance(CurrencyType type, long amount)
        {
            switch (type)
            {
                case CurrencyType.CloutBux:
                    CloutBux = amount;
                    break;
                case CurrencyType.Purrkoin:
                    Purrkoin = amount;
                    break;
            }
            LastUpdated = DateTime.UtcNow;
        }
    }
    
    /// <summary>
    /// Record of a currency transaction.
    /// </summary>
    [Serializable]
    public class CurrencyTransaction
    {
        public string TransactionId;
        public CurrencyType Type;
        public long Amount;
        public string Source;
        public DateTime Timestamp;
        
        public CurrencyTransaction(CurrencyType type, long amount, string source)
        {
            TransactionId = Guid.NewGuid().ToString();
            Type = type;
            Amount = amount;
            Source = source;
            Timestamp = DateTime.UtcNow;
        }
    }
    
    /// <summary>
    /// Defines a purchasable item in the store (Monetization-specific version).
    /// For the main StoreItem type, see ElectionEmpire.Economy.StoreItem
    /// </summary>
    [Serializable]
    public class MonetizationStoreItem
    {
        public string ItemId;
        public string DisplayName;
        public string Description;
        public CurrencyType PriceType;
        public long Price;
        public PurchaseCategory Category;
        public ItemRarity Rarity;
        public bool IsAvailable;
        public Sprite Icon;

        // For cosmetics
        public CosmeticType CosmeticType;
        public string UnlockData;
    }
    
    /// <summary>
    /// Result of a purchase attempt.
    /// </summary>
    public class PurchaseResult
    {
        public bool Success;
        public string Message;
        public MonetizationStoreItem Item;
        public long NewBalance;

        public static PurchaseResult Succeeded(MonetizationStoreItem item, long balance) =>
            new PurchaseResult { Success = true, Item = item, NewBalance = balance };

        public static PurchaseResult Failed(string message) =>
            new PurchaseResult { Success = false, Message = message };
    }
    
    #endregion
    
    #region Currency Manager
    
    /// <summary>
    /// Manages all in-game currencies and transactions.
    /// </summary>
    public class CurrencyManager
    {
        private CurrencyWallet wallet;
        private List<MonetizationStoreItem> storeItems;
        private HashSet<string> ownedItems;

        public event Action<CurrencyType, long, long> OnCurrencyChanged;
        public event Action<MonetizationStoreItem> OnItemPurchased;
        
        public CurrencyManager()
        {
            wallet = new CurrencyWallet();
            storeItems = new List<MonetizationStoreItem>();
            ownedItems = new HashSet<string>();
            InitializeDefaultItems();
        }

        private void InitializeDefaultItems()
        {
            // Add default store items
            storeItems.Add(new MonetizationStoreItem
            {
                ItemId = "premium_hat",
                DisplayName = "Premium Top Hat",
                Description = "Show your political style",
                PriceType = CurrencyType.CloutBux,
                Price = 500,
                Category = PurchaseCategory.Cosmetic,
                Rarity = ItemRarity.Rare,
                IsAvailable = true,
                CosmeticType = CosmeticType.Hat
            });

            storeItems.Add(new MonetizationStoreItem
            {
                ItemId = "campaign_boost",
                DisplayName = "Campaign Boost",
                Description = "+10% campaign effectiveness for one election",
                PriceType = CurrencyType.Purrkoin,
                Price = 50,
                Category = PurchaseCategory.Boost,
                Rarity = ItemRarity.Uncommon,
                IsAvailable = true
            });
        }
        
        public long GetBalance(CurrencyType type) => wallet.GetBalance(type);

        /// <summary>
        /// Check if the player can afford a purchase
        /// </summary>
        public bool CanAfford(CurrencyType type, long amount)
        {
            return wallet.GetBalance(type) >= amount;
        }

        /// <summary>
        /// Debit currency from wallet (spend)
        /// </summary>
        public bool Debit(CurrencyType type, long amount, string reason = "Purchase")
        {
            if (amount <= 0) return false;
            if (!CanAfford(type, amount)) return false;

            long oldBalance = wallet.GetBalance(type);
            wallet.SpendCurrency(type, amount);
            wallet.RecentTransactions.Add(new CurrencyTransaction(type, -amount, reason));

            // Keep only last 100 transactions
            if (wallet.RecentTransactions.Count > 100)
                wallet.RecentTransactions.RemoveAt(0);

            OnCurrencyChanged?.Invoke(type, oldBalance, wallet.GetBalance(type));
            return true;
        }

        /// <summary>
        /// Credit currency to wallet (earn/add)
        /// </summary>
        public bool Credit(CurrencyType type, long amount, string reason = "Reward", string source = "System")
        {
            if (amount <= 0) return false;

            long oldBalance = wallet.GetBalance(type);
            wallet.AddCurrency(type, amount);
            wallet.RecentTransactions.Add(new CurrencyTransaction(type, amount, $"{reason} ({source})"));

            // Keep only last 100 transactions
            if (wallet.RecentTransactions.Count > 100)
                wallet.RecentTransactions.RemoveAt(0);

            OnCurrencyChanged?.Invoke(type, oldBalance, wallet.GetBalance(type));
            return true;
        }

        /// <summary>
        /// Set the balance for a currency type directly
        /// </summary>
        public void SetBalance(CurrencyType type, long amount)
        {
            if (amount < 0) amount = 0;
            long oldBalance = wallet.GetBalance(type);
            wallet.SetBalance(type, amount);
            OnCurrencyChanged?.Invoke(type, oldBalance, wallet.GetBalance(type));
        }

        public void AddCurrency(CurrencyType type, long amount, string source = "System")
        {
            if (amount <= 0) return;

            long oldBalance = wallet.GetBalance(type);
            wallet.AddCurrency(type, amount);
            wallet.RecentTransactions.Add(new CurrencyTransaction(type, amount, source));

            // Keep only last 100 transactions
            if (wallet.RecentTransactions.Count > 100)
                wallet.RecentTransactions.RemoveAt(0);

            OnCurrencyChanged?.Invoke(type, oldBalance, wallet.GetBalance(type));
        }

        public PurchaseResult PurchaseItem(string itemId)
        {
            var item = storeItems.Find(i => i.ItemId == itemId);
            if (item == null)
                return PurchaseResult.Failed("Item not found");

            if (!item.IsAvailable)
                return PurchaseResult.Failed("Item not available");

            if (ownedItems.Contains(itemId))
                return PurchaseResult.Failed("Already owned");

            if (!wallet.SpendCurrency(item.PriceType, item.Price))
                return PurchaseResult.Failed("Insufficient funds");

            long oldBalance = wallet.GetBalance(item.PriceType);
            ownedItems.Add(itemId);
            OnItemPurchased?.Invoke(item);
            OnCurrencyChanged?.Invoke(item.PriceType, oldBalance, wallet.GetBalance(item.PriceType));

            return PurchaseResult.Succeeded(item, wallet.GetBalance(item.PriceType));
        }

        public List<MonetizationStoreItem> GetStoreItems() => new List<MonetizationStoreItem>(storeItems);
        public List<MonetizationStoreItem> GetStoreItems(PurchaseCategory category) =>
            storeItems.FindAll(i => i.Category == category);
        public bool OwnsItem(string itemId) => ownedItems.Contains(itemId);
        
        #region Balance Operations
        
        public void RewardElectionVictory(int tier)
        {
            long baseReward = 100 * tier;
            AddCurrency(CurrencyType.CloutBux, baseReward, $"Election Victory (Tier {tier})");
            
            if (tier >= 4)
                AddCurrency(CurrencyType.Purrkoin, tier * 5, "High Office Victory Bonus");
        }
        
        public void RewardCrisisSurvival(int severity)
        {
            long reward = 25 * severity;
            AddCurrency(CurrencyType.CloutBux, reward, $"Crisis Survived (Severity {severity})");
        }
        
        public void RewardAchievement(string achievementId, int difficulty)
        {
            long cloutReward = 50 * difficulty;
            AddCurrency(CurrencyType.CloutBux, cloutReward, $"Achievement: {achievementId}");
            
            if (difficulty >= 3)
                AddCurrency(CurrencyType.Purrkoin, difficulty * 2, $"Achievement Bonus: {achievementId}");
        }
        
        public void ApplyDailyBonus()
        {
            AddCurrency(CurrencyType.CloutBux, 25, "Daily Bonus");
        }
        
        public void ApplyStreakBonus(int streakDays)
        {
            long bonus = Math.Min(streakDays * 10, 100);
            AddCurrency(CurrencyType.CloutBux, bonus, $"Streak Bonus (Day {streakDays})");
            
            if (streakDays >= 7)
                AddCurrency(CurrencyType.Purrkoin, 5, "Weekly Streak Bonus");
        }
        
        #endregion
        
        #region Earning Rates
        
        public float GetEarningMultiplier()
        {
            float multiplier = 1.0f;
            
            if (OwnsItem("double_earnings"))
                multiplier *= 2.0f;
            if (OwnsItem("vip_pass"))
                multiplier *= 1.5f;
                
            return multiplier;
        }
        
        public long CalculateEarnings(long baseAmount)
        {
            return (long)(baseAmount * GetEarningMultiplier());
        }
        
        #endregion
        
        #region Persistence
        
        private const string WALLET_SAVE_KEY = "currency_wallet";
        private const string OWNED_ITEMS_KEY = "owned_items";
        
        private Dictionary<string, long> pendingSync = new Dictionary<string, long>();
        private bool isDirty = false;
        
        public void SaveData()
        {
            try
            {
                string walletJson = JsonUtility.ToJson(wallet);
                PlayerPrefs.SetString(WALLET_SAVE_KEY, walletJson);
                
                string ownedJson = string.Join(",", ownedItems);
                PlayerPrefs.SetString(OWNED_ITEMS_KEY, ownedJson);
                
                PlayerPrefs.Save();
                isDirty = false;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save currency data: {e.Message}");
            }
        }
        
        public void LoadData()
        {
            try
            {
                if (PlayerPrefs.HasKey(WALLET_SAVE_KEY))
                {
                    string walletJson = PlayerPrefs.GetString(WALLET_SAVE_KEY);
                    wallet = JsonUtility.FromJson<CurrencyWallet>(walletJson);
                }
                
                if (PlayerPrefs.HasKey(OWNED_ITEMS_KEY))
                {
                    string ownedJson = PlayerPrefs.GetString(OWNED_ITEMS_KEY);
                    if (!string.IsNullOrEmpty(ownedJson))
                    {
                        foreach (var id in ownedJson.Split(','))
                            ownedItems.Add(id);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load currency data: {e.Message}");
                wallet = new CurrencyWallet();
                ownedItems.Clear();
            }
        }
        
        public void MarkDirty()
        {
            isDirty = true;
        }
        
        public bool IsDirty => isDirty;
        
        public void ClearPendingSync()
        {
            pendingSync.Clear();
            isDirty = false;
        }
        
        #endregion
    }
    
    #endregion
    
    #region IAP Manager
    
    /// <summary>
    /// Handles in-app purchases and platform store integration.
    /// Stub implementation - requires Unity Purchasing package for full functionality.
    /// </summary>
    public class IAPManager
    {
        public event Action<string> OnPurchaseSuccess;
        public event Action<string, string> OnPurchaseFailed;

        private bool isInitialized = false;
        private CurrencyManager currencyManager;
        private PlayerInventory playerInventory;

        public IAPManager() { }

        public IAPManager(CurrencyManager currency, PlayerInventory inventory)
        {
            currencyManager = currency;
            playerInventory = inventory;
        }

        public void Initialize()
        {
            // In a full implementation, this would initialize Unity Purchasing
            Debug.Log("IAPManager initialized (stub mode - install Unity Purchasing for full IAP support)");
            isInitialized = true;
        }
        
        public void PurchaseProduct(string productId)
        {
            if (!isInitialized)
            {
                OnPurchaseFailed?.Invoke(productId, "Store not initialized");
                return;
            }
            
            // Stub implementation - in production, this would initiate a real purchase
            Debug.Log($"Purchase requested for: {productId} (stub mode)");
            OnPurchaseFailed?.Invoke(productId, "Unity Purchasing not installed - stub mode");
        }
        
        public bool IsInitialized => isInitialized;
        
        public string GetLocalizedPrice(string productId) => "N/A";
    }
    
    #endregion
}
