// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - Monetization Core System
// Sprint 11: Currency management, IAP integration, and economy balancing
// ═══════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;

namespace ElectionEmpire.Monetization
{
    #region Enums & Constants
    
    /// <summary>
    /// Types of in-game currency.
    /// </summary>
    public enum CurrencyType
    {
        CloutBux,       // Earned in-game, soft currency
        Purrkoin        // Premium currency, optional purchase
    }
    
    /// <summary>
    /// Categories of purchasable content.
    /// </summary>
    public enum PurchaseCategory
    {
        Currency,           // Direct currency purchase
        ContentPack,        // DLC/expansion content
        Cosmetic,           // Visual customization
        Convenience,        // Quality of life items
        Starter,            // New player bundles
        Seasonal            // Limited time offers
    }
    
    /// <summary>
    /// Types of cosmetic items.
    /// </summary>
    public enum CosmeticType
    {
        Portrait_Background,
        Portrait_Frame,
        Portrait_Effect,
        Campaign_Theme,
        Office_Decoration,
        Staff_Outfit,
        Title_Badge,
        Emote,
        Victory_Animation,
        Defeat_Animation
    }
    
    /// <summary>
    /// Rarity tiers for items.
    /// </summary>
    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
    
    #endregion
    
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
    }
    
    /// <summary>
    /// Record of a currency transaction.
    /// </summary>
    [Serializable]
    public class CurrencyTransaction
    {
        public string TransactionId;
        public CurrencyType Currency;
        public long Amount;             // Positive = credit, negative = debit
        public string Reason;
        public DateTime Timestamp;
        public string SourceEvent;      // What triggered this transaction
    }
    
    /// <summary>
    /// Definition of a purchasable item.
    /// </summary>
    [Serializable]
    public class PurchasableItem
    {
        public string ItemId;
        public string DisplayName;
        public string Description;
        public PurchaseCategory Category;
        public ItemRarity Rarity;
        
        // Pricing
        public CurrencyType CurrencyType;
        public long Price;
        public float RealMoneyPrice;    // If direct purchase available
        public string StoreProductId;   // Platform store ID
        
        // Content
        public string ContentBundleId;
        public List<string> UnlockedFeatures;
        public Dictionary<string, object> Metadata;
        
        // Availability
        public bool IsAvailable;
        public DateTime? AvailableFrom;
        public DateTime? AvailableUntil;
        public int? PurchaseLimit;      // Max purchases per player
        
        // Display
        public string IconPath;
        public string PreviewPath;
        public Color RarityColor => GetRarityColor();
        
        public PurchasableItem()
        {
            UnlockedFeatures = new List<string>();
            Metadata = new Dictionary<string, object>();
        }
        
        private Color GetRarityColor()
        {
            return Rarity switch
            {
                ItemRarity.Common => new Color(0.7f, 0.7f, 0.7f),
                ItemRarity.Uncommon => new Color(0.2f, 0.8f, 0.2f),
                ItemRarity.Rare => new Color(0.2f, 0.4f, 1f),
                ItemRarity.Epic => new Color(0.7f, 0.2f, 0.9f),
                ItemRarity.Legendary => new Color(1f, 0.6f, 0.1f),
                _ => Color.white
            };
        }
    }
    
    /// <summary>
    /// A cosmetic item definition.
    /// </summary>
    [Serializable]
    public class CosmeticItem : PurchasableItem
    {
        public CosmeticType CosmeticType;
        public string AssetPath;
        public Dictionary<string, string> Variants;
        public bool IsAnimated;
        public string AnimationPath;
        
        public CosmeticItem()
        {
            Variants = new Dictionary<string, string>();
        }
    }
    
    /// <summary>
    /// A content pack/DLC definition.
    /// </summary>
    [Serializable]
    public class ContentPack : PurchasableItem
    {
        public List<string> IncludedBackgrounds;
        public List<string> IncludedTraits;
        public List<string> IncludedEvents;
        public List<string> IncludedScenarios;
        public List<CosmeticItem> IncludedCosmetics;
        public int BonusLegacyPoints;
        
        public ContentPack()
        {
            IncludedBackgrounds = new List<string>();
            IncludedTraits = new List<string>();
            IncludedEvents = new List<string>();
            IncludedScenarios = new List<string>();
            IncludedCosmetics = new List<CosmeticItem>();
        }
    }
    
    /// <summary>
    /// Player's inventory of owned items.
    /// </summary>
    [Serializable]
    public class PlayerInventory
    {
        public string PlayerId;
        public List<string> OwnedItemIds;
        public Dictionary<string, int> PurchaseCounts;
        public List<string> EquippedCosmetics;
        public DateTime LastUpdated;
        
        public PlayerInventory()
        {
            OwnedItemIds = new List<string>();
            PurchaseCounts = new Dictionary<string, int>();
            EquippedCosmetics = new List<string>();
        }
        
        public bool OwnsItem(string itemId)
        {
            return OwnedItemIds.Contains(itemId);
        }
        
        public int GetPurchaseCount(string itemId)
        {
            return PurchaseCounts.TryGetValue(itemId, out int count) ? count : 0;
        }
    }
    
    #endregion
    
    #region Currency Manager
    
    /// <summary>
    /// Manages all currency operations.
    /// </summary>
    public class CurrencyManager
    {
        private CurrencyWallet wallet;
        private List<CurrencyTransaction> pendingSync;
        private bool isDirty;
        
        // Events
        public event Action<CurrencyType, long, long> OnBalanceChanged; // type, oldBalance, newBalance
        public event Action<CurrencyTransaction> OnTransactionProcessed;
        public event Action<string> OnTransactionFailed;
        
        // Limits
        private const long MAX_CLOUTBUX = 999_999_999;
        private const long MAX_PURRKOIN = 999_999;
        private const int MAX_TRANSACTION_HISTORY = 100;
        
        public CurrencyManager()
        {
            wallet = new CurrencyWallet();
            pendingSync = new List<CurrencyTransaction>();
        }
        
        #region Balance Operations
        
        /// <summary>
        /// Get current balance for a currency type.
        /// </summary>
        public long GetBalance(CurrencyType type)
        {
            return wallet.GetBalance(type);
        }
        
        /// <summary>
        /// Check if player can afford a purchase.
        /// </summary>
        public bool CanAfford(CurrencyType type, long amount)
        {
            return wallet.GetBalance(type) >= amount;
        }
        
        /// <summary>
        /// Credit currency to the wallet.
        /// </summary>
        public bool Credit(CurrencyType type, long amount, string reason, string sourceEvent = null)
        {
            if (amount <= 0) return false;
            
            long oldBalance = wallet.GetBalance(type);
            long maxBalance = type == CurrencyType.CloutBux ? MAX_CLOUTBUX : MAX_PURRKOIN;
            long newBalance = Math.Min(oldBalance + amount, maxBalance);
            long actualCredit = newBalance - oldBalance;
            
            if (actualCredit <= 0) return false;
            
            ApplyBalanceChange(type, newBalance);
            RecordTransaction(type, actualCredit, reason, sourceEvent);
            
            OnBalanceChanged?.Invoke(type, oldBalance, newBalance);
            return true;
        }
        
        /// <summary>
        /// Debit currency from the wallet.
        /// </summary>
        public bool Debit(CurrencyType type, long amount, string reason, string sourceEvent = null)
        {
            if (amount <= 0) return false;
            
            long oldBalance = wallet.GetBalance(type);
            if (oldBalance < amount)
            {
                OnTransactionFailed?.Invoke($"Insufficient {type} balance");
                return false;
            }
            
            long newBalance = oldBalance - amount;
            ApplyBalanceChange(type, newBalance);
            RecordTransaction(type, -amount, reason, sourceEvent);
            
            OnBalanceChanged?.Invoke(type, oldBalance, newBalance);
            return true;
        }
        
        /// <summary>
        /// Transfer currency between types (conversion).
        /// </summary>
        public bool Convert(CurrencyType from, CurrencyType to, long amount, float rate)
        {
            if (!CanAfford(from, amount)) return false;
            
            long convertedAmount = (long)(amount * rate);
            if (convertedAmount <= 0) return false;
            
            if (Debit(from, amount, $"Converted to {to}"))
            {
                Credit(to, convertedAmount, $"Converted from {from}");
                return true;
            }
            
            return false;
        }
        
        private void ApplyBalanceChange(CurrencyType type, long newBalance)
        {
            switch (type)
            {
                case CurrencyType.CloutBux:
                    wallet.CloutBux = newBalance;
                    break;
                case CurrencyType.Purrkoin:
                    wallet.Purrkoin = newBalance;
                    break;
            }
            
            wallet.LastUpdated = DateTime.UtcNow;
            isDirty = true;
        }
        
        private void RecordTransaction(CurrencyType type, long amount, string reason, string sourceEvent)
        {
            var transaction = new CurrencyTransaction
            {
                TransactionId = Guid.NewGuid().ToString(),
                Currency = type,
                Amount = amount,
                Reason = reason,
                Timestamp = DateTime.UtcNow,
                SourceEvent = sourceEvent
            };
            
            wallet.RecentTransactions.Insert(0, transaction);
            
            // Trim history
            if (wallet.RecentTransactions.Count > MAX_TRANSACTION_HISTORY)
            {
                wallet.RecentTransactions.RemoveRange(
                    MAX_TRANSACTION_HISTORY,
                    wallet.RecentTransactions.Count - MAX_TRANSACTION_HISTORY);
            }
            
            pendingSync.Add(transaction);
            OnTransactionProcessed?.Invoke(transaction);
        }
        
        #endregion
        
        #region Earning Rates
        
        /// <summary>
        /// Calculate CloutBux reward for an event.
        /// </summary>
        public long CalculateReward(string eventType, Dictionary<string, float> modifiers = null)
        {
            long baseReward = GetBaseReward(eventType);
            
            if (modifiers != null)
            {
                foreach (var mod in modifiers)
                {
                    baseReward = (long)(baseReward * mod.Value);
                }
            }
            
            return Math.Max(baseReward, 0);
        }
        
        private long GetBaseReward(string eventType)
        {
            return eventType.ToLower() switch
            {
                // Elections
                "election_win_tier1" => 100,
                "election_win_tier2" => 250,
                "election_win_tier3" => 500,
                "election_win_tier4" => 1000,
                "election_win_tier5" => 5000,
                "election_landslide" => 500,        // Bonus for winning by >20%
                
                // Crises
                "crisis_survived" => 50,
                "crisis_resolved_perfectly" => 150,
                "scandal_survived" => 75,
                "impeachment_survived" => 1000,
                
                // Achievements
                "achievement_common" => 25,
                "achievement_uncommon" => 50,
                "achievement_rare" => 100,
                "achievement_epic" => 250,
                "achievement_legendary" => 500,
                
                // Milestones
                "first_election_win" => 200,
                "first_scandal" => 50,
                "reach_tier2" => 100,
                "reach_tier3" => 200,
                "reach_tier4" => 400,
                "reach_tier5" => 1000,
                "complete_playthrough" => 500,
                
                // Daily/Weekly
                "daily_login" => 10,
                "weekly_challenge" => 100,
                "streak_bonus" => 25,
                
                // Chaos Mode
                "chaos_moment" => 30,
                "viral_scandal" => 100,
                "drunk_speech_success" => 75,
                
                _ => 0
            };
        }
        
        /// <summary>
        /// Calculate Purrkoin reward for major achievements.
        /// </summary>
        public long CalculatePurrkoinReward(string eventType)
        {
            return eventType.ToLower() switch
            {
                "election_win_tier5" => 10,
                "impeachment_survived" => 25,
                "become_supreme_leader" => 100,
                "establish_shadow_government" => 50,
                "complete_all_alignments" => 200,
                "dynasty_established" => 150,
                "legendary_achievement" => 50,
                _ => 0
            };
        }
        
        #endregion
        
        #region Persistence
        
        /// <summary>
        /// Load wallet from save data.
        /// </summary>
        public void LoadWallet(CurrencyWallet savedWallet)
        {
            wallet = savedWallet ?? new CurrencyWallet();
            isDirty = false;
            pendingSync.Clear();
        }
        
        /// <summary>
        /// Get wallet for saving.
        /// </summary>
        public CurrencyWallet GetWalletForSave()
        {
            return wallet;
        }
        
        /// <summary>
        /// Get pending transactions for server sync.
        /// </summary>
        public List<CurrencyTransaction> GetPendingSync()
        {
            return new List<CurrencyTransaction>(pendingSync);
        }
        
        /// <summary>
        /// Clear pending sync after successful sync.
        /// </summary>
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
    /// </summary>
    public class IAPManager : IStoreListener
    {
        private IStoreController storeController;
        private IExtensionProvider storeExtensions;
        private CurrencyManager currencyManager;
        private PlayerInventory inventory;
        
        // Product catalog
        private Dictionary<string, PurchasableItem> productCatalog;
        
        // State
        private bool isInitialized;
        private bool isProcessingPurchase;
        private string currentPurchaseId;
        
        // Events
        public event Action OnStoreInitialized;
        public event Action<string> OnStoreInitializeFailed;
        public event Action<string, PurchasableItem> OnPurchaseSuccess;
        public event Action<string, string> OnPurchaseFailed;
        public event Action<string> OnPurchaseRestored;
        
        // Product IDs
        private static class ProductIds
        {
            // Currency packs
            public const string PURRKOIN_100 = "com.electionempire.purrkoin100";
            public const string PURRKOIN_500 = "com.electionempire.purrkoin500";
            public const string PURRKOIN_1200 = "com.electionempire.purrkoin1200";
            public const string PURRKOIN_2500 = "com.electionempire.purrkoin2500";
            public const string PURRKOIN_6500 = "com.electionempire.purrkoin6500";
            
            // Content packs
            public const string DLC_POLITICAL_MACHINES = "com.electionempire.dlc.machines";
            public const string DLC_GLOBAL_POLITICS = "com.electionempire.dlc.global";
            public const string DLC_POWER_THRONE = "com.electionempire.dlc.throne";
            
            // Starter packs
            public const string STARTER_PACK = "com.electionempire.starter";
            public const string CHAOS_PACK = "com.electionempire.chaospack";
        }
        
        public IAPManager(CurrencyManager currencyMgr, PlayerInventory inv)
        {
            currencyManager = currencyMgr;
            inventory = inv;
            productCatalog = new Dictionary<string, PurchasableItem>();
            
            InitializeProductCatalog();
        }
        
        #region Initialization
        
        private void InitializeProductCatalog()
        {
            // Currency packs
            AddProduct(new PurchasableItem
            {
                ItemId = ProductIds.PURRKOIN_100,
                DisplayName = "100 Purrkoin",
                Description = "A small pile of premium currency",
                Category = PurchaseCategory.Currency,
                Rarity = ItemRarity.Common,
                CurrencyType = CurrencyType.Purrkoin,
                Price = 0,
                RealMoneyPrice = 0.99f,
                StoreProductId = ProductIds.PURRKOIN_100,
                IsAvailable = true,
                Metadata = new Dictionary<string, object> { ["purrkoin_amount"] = 100 }
            });
            
            AddProduct(new PurchasableItem
            {
                ItemId = ProductIds.PURRKOIN_500,
                DisplayName = "500 Purrkoin (+50 Bonus)",
                Description = "A pouch of premium currency with bonus",
                Category = PurchaseCategory.Currency,
                Rarity = ItemRarity.Uncommon,
                RealMoneyPrice = 4.99f,
                StoreProductId = ProductIds.PURRKOIN_500,
                IsAvailable = true,
                Metadata = new Dictionary<string, object> { ["purrkoin_amount"] = 550 }
            });
            
            AddProduct(new PurchasableItem
            {
                ItemId = ProductIds.PURRKOIN_1200,
                DisplayName = "1200 Purrkoin (+200 Bonus)",
                Description = "A chest of premium currency with bonus",
                Category = PurchaseCategory.Currency,
                Rarity = ItemRarity.Rare,
                RealMoneyPrice = 9.99f,
                StoreProductId = ProductIds.PURRKOIN_1200,
                IsAvailable = true,
                Metadata = new Dictionary<string, object> { ["purrkoin_amount"] = 1400 }
            });
            
            AddProduct(new PurchasableItem
            {
                ItemId = ProductIds.PURRKOIN_2500,
                DisplayName = "2500 Purrkoin (+700 Bonus)",
                Description = "A vault of premium currency with big bonus",
                Category = PurchaseCategory.Currency,
                Rarity = ItemRarity.Epic,
                RealMoneyPrice = 19.99f,
                StoreProductId = ProductIds.PURRKOIN_2500,
                IsAvailable = true,
                Metadata = new Dictionary<string, object> { ["purrkoin_amount"] = 3200 }
            });
            
            AddProduct(new PurchasableItem
            {
                ItemId = ProductIds.PURRKOIN_6500,
                DisplayName = "6500 Purrkoin (+2500 Bonus)",
                Description = "A treasury of premium currency with massive bonus",
                Category = PurchaseCategory.Currency,
                Rarity = ItemRarity.Legendary,
                RealMoneyPrice = 49.99f,
                StoreProductId = ProductIds.PURRKOIN_6500,
                IsAvailable = true,
                Metadata = new Dictionary<string, object> { ["purrkoin_amount"] = 9000 }
            });
            
            // DLC packs
            AddProduct(new ContentPack
            {
                ItemId = ProductIds.DLC_POLITICAL_MACHINES,
                DisplayName = "Political Machines DLC",
                Description = "New party dynamics, enhanced electoral mechanics, and historical political machines",
                Category = PurchaseCategory.ContentPack,
                Rarity = ItemRarity.Epic,
                RealMoneyPrice = 9.99f,
                StoreProductId = ProductIds.DLC_POLITICAL_MACHINES,
                IsAvailable = true,
                IncludedBackgrounds = new List<string> { "machine_boss", "ward_heeler", "party_fixer" },
                IncludedEvents = new List<string> { "event_pack_machines" },
                IncludedScenarios = new List<string> { "tammany_hall", "chicago_machine", "boston_irish" },
                BonusLegacyPoints = 500
            });
            
            AddProduct(new ContentPack
            {
                ItemId = ProductIds.DLC_GLOBAL_POLITICS,
                DisplayName = "Global Politics DLC",
                Description = "International expansion with diplomatic relations and multi-country campaigns",
                Category = PurchaseCategory.ContentPack,
                Rarity = ItemRarity.Epic,
                RealMoneyPrice = 14.99f,
                StoreProductId = ProductIds.DLC_GLOBAL_POLITICS,
                IsAvailable = true,
                IncludedBackgrounds = new List<string> { "diplomat", "ambassador", "spy" },
                IncludedEvents = new List<string> { "event_pack_global" },
                IncludedScenarios = new List<string> { "european_union", "asian_superpower", "global_summit" },
                BonusLegacyPoints = 750
            });
            
            // Starter pack
            AddProduct(new ContentPack
            {
                ItemId = ProductIds.STARTER_PACK,
                DisplayName = "Politician Starter Pack",
                Description = "Everything a new politician needs: currency, cosmetics, and a head start",
                Category = PurchaseCategory.Starter,
                Rarity = ItemRarity.Rare,
                RealMoneyPrice = 4.99f,
                StoreProductId = ProductIds.STARTER_PACK,
                IsAvailable = true,
                PurchaseLimit = 1,
                Metadata = new Dictionary<string, object>
                {
                    ["purrkoin_amount"] = 300,
                    ["cloutbux_amount"] = 1000
                },
                IncludedCosmetics = new List<CosmeticItem>
                {
                    new CosmeticItem
                    {
                        ItemId = "frame_starter_gold",
                        CosmeticType = CosmeticType.Portrait_Frame,
                        DisplayName = "Starter Gold Frame"
                    }
                },
                BonusLegacyPoints = 100
            });
        }
        
        private void AddProduct(PurchasableItem item)
        {
            productCatalog[item.ItemId] = item;
        }
        
        /// <summary>
        /// Initialize the store.
        /// </summary>
        public void Initialize()
        {
            if (isInitialized) return;
            
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            
            // Add all products
            foreach (var product in productCatalog.Values)
            {
                if (!string.IsNullOrEmpty(product.StoreProductId))
                {
                    var productType = product.Category == PurchaseCategory.Currency
                        ? ProductType.Consumable
                        : ProductType.NonConsumable;
                    
                    builder.AddProduct(product.StoreProductId, productType);
                }
            }
            
            UnityPurchasing.Initialize(this, builder);
        }
        
        #endregion
        
        #region IStoreListener Implementation
        
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            storeController = controller;
            storeExtensions = extensions;
            isInitialized = true;
            
            Debug.Log("IAP Store initialized successfully");
            OnStoreInitialized?.Invoke();
        }
        
        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogError($"IAP Store initialization failed: {error}");
            OnStoreInitializeFailed?.Invoke(error.ToString());
        }
        
        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.LogError($"IAP Store initialization failed: {error} - {message}");
            OnStoreInitializeFailed?.Invoke($"{error}: {message}");
        }
        
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            var productId = args.purchasedProduct.definition.id;
            
            if (productCatalog.TryGetValue(productId, out var item))
            {
                // Process the purchase based on type
                ProcessPurchasedItem(item);
                
                OnPurchaseSuccess?.Invoke(productId, item);
                isProcessingPurchase = false;
                
                return PurchaseProcessingResult.Complete;
            }
            
            Debug.LogWarning($"Unknown product purchased: {productId}");
            return PurchaseProcessingResult.Complete;
        }
        
        public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
        {
            Debug.LogError($"Purchase failed for {product.definition.id}: {reason}");
            OnPurchaseFailed?.Invoke(product.definition.id, reason.ToString());
            isProcessingPurchase = false;
        }
        
        #endregion
        
        #region Purchase Operations
        
        /// <summary>
        /// Initiate a purchase.
        /// </summary>
        public bool InitiatePurchase(string productId)
        {
            if (!isInitialized)
            {
                OnPurchaseFailed?.Invoke(productId, "Store not initialized");
                return false;
            }
            
            if (isProcessingPurchase)
            {
                OnPurchaseFailed?.Invoke(productId, "Purchase already in progress");
                return false;
            }
            
            if (!productCatalog.TryGetValue(productId, out var item))
            {
                OnPurchaseFailed?.Invoke(productId, "Product not found");
                return false;
            }
            
            // Check purchase limit
            if (item.PurchaseLimit.HasValue)
            {
                int currentCount = inventory.GetPurchaseCount(productId);
                if (currentCount >= item.PurchaseLimit.Value)
                {
                    OnPurchaseFailed?.Invoke(productId, "Purchase limit reached");
                    return false;
                }
            }
            
            // Check availability
            if (!item.IsAvailable)
            {
                OnPurchaseFailed?.Invoke(productId, "Product not available");
                return false;
            }
            
            if (item.AvailableFrom.HasValue && DateTime.UtcNow < item.AvailableFrom.Value)
            {
                OnPurchaseFailed?.Invoke(productId, "Product not yet available");
                return false;
            }
            
            if (item.AvailableUntil.HasValue && DateTime.UtcNow > item.AvailableUntil.Value)
            {
                OnPurchaseFailed?.Invoke(productId, "Product no longer available");
                return false;
            }
            
            isProcessingPurchase = true;
            currentPurchaseId = productId;
            
            storeController.InitiatePurchase(productId);
            return true;
        }
        
        /// <summary>
        /// Purchase with in-game currency.
        /// </summary>
        public bool PurchaseWithCurrency(string itemId)
        {
            if (!productCatalog.TryGetValue(itemId, out var item))
            {
                OnPurchaseFailed?.Invoke(itemId, "Item not found");
                return false;
            }
            
            if (item.RealMoneyPrice > 0 && item.Price <= 0)
            {
                OnPurchaseFailed?.Invoke(itemId, "Item requires real money purchase");
                return false;
            }
            
            if (!currencyManager.CanAfford(item.CurrencyType, item.Price))
            {
                OnPurchaseFailed?.Invoke(itemId, "Insufficient currency");
                return false;
            }
            
            if (currencyManager.Debit(item.CurrencyType, item.Price, $"Purchase: {item.DisplayName}"))
            {
                ProcessPurchasedItem(item);
                OnPurchaseSuccess?.Invoke(itemId, item);
                return true;
            }
            
            return false;
        }
        
        private void ProcessPurchasedItem(PurchasableItem item)
        {
            // Add to inventory
            inventory.OwnedItemIds.Add(item.ItemId);
            inventory.PurchaseCounts[item.ItemId] = inventory.GetPurchaseCount(item.ItemId) + 1;
            inventory.LastUpdated = DateTime.UtcNow;
            
            // Process based on category
            switch (item.Category)
            {
                case PurchaseCategory.Currency:
                    ProcessCurrencyPurchase(item);
                    break;
                    
                case PurchaseCategory.ContentPack:
                    ProcessContentPackPurchase(item as ContentPack);
                    break;
                    
                case PurchaseCategory.Starter:
                    ProcessStarterPackPurchase(item as ContentPack);
                    break;
                    
                case PurchaseCategory.Cosmetic:
                    ProcessCosmeticPurchase(item as CosmeticItem);
                    break;
            }
        }
        
        private void ProcessCurrencyPurchase(PurchasableItem item)
        {
            if (item.Metadata.TryGetValue("purrkoin_amount", out var amount))
            {
                currencyManager.Credit(CurrencyType.Purrkoin, Convert.ToInt64(amount),
                    $"Purchased: {item.DisplayName}", "iap_purchase");
            }
        }
        
        private void ProcessContentPackPurchase(ContentPack pack)
        {
            if (pack == null) return;
            
            // Unlock content
            if (pack.IncludedBackgrounds != null)
            {
                foreach (var bg in pack.IncludedBackgrounds)
                {
                    // Unlock background in game data
                    Debug.Log($"Unlocked background: {bg}");
                }
            }
            
            if (pack.BonusLegacyPoints > 0)
            {
                // Add legacy points
                Debug.Log($"Added {pack.BonusLegacyPoints} legacy points");
            }
        }
        
        private void ProcessStarterPackPurchase(ContentPack pack)
        {
            if (pack == null) return;
            
            // Give currencies
            if (pack.Metadata.TryGetValue("purrkoin_amount", out var pk))
            {
                currencyManager.Credit(CurrencyType.Purrkoin, Convert.ToInt64(pk),
                    "Starter Pack Bonus", "starter_pack");
            }
            
            if (pack.Metadata.TryGetValue("cloutbux_amount", out var cb))
            {
                currencyManager.Credit(CurrencyType.CloutBux, Convert.ToInt64(cb),
                    "Starter Pack Bonus", "starter_pack");
            }
            
            // Unlock cosmetics
            if (pack.IncludedCosmetics != null)
            {
                foreach (var cosmetic in pack.IncludedCosmetics)
                {
                    inventory.OwnedItemIds.Add(cosmetic.ItemId);
                }
            }
        }
        
        private void ProcessCosmeticPurchase(CosmeticItem cosmetic)
        {
            if (cosmetic == null) return;
            
            Debug.Log($"Unlocked cosmetic: {cosmetic.DisplayName}");
        }
        
        /// <summary>
        /// Restore previous purchases.
        /// </summary>
        public void RestorePurchases()
        {
            if (!isInitialized) return;
            
#if UNITY_IOS
            var apple = storeExtensions.GetExtension<IAppleExtensions>();
            apple.RestoreTransactions((success) =>
            {
                Debug.Log($"Restore purchases: {(success ? "Success" : "Failed")}");
            });
#elif UNITY_ANDROID
            var google = storeExtensions.GetExtension<IGooglePlayStoreExtensions>();
            google.RestoreTransactions((success) =>
            {
                Debug.Log($"Restore purchases: {(success ? "Success" : "Failed")}");
            });
#endif
        }
        
        #endregion
        
        #region Catalog Access
        
        /// <summary>
        /// Get all available products.
        /// </summary>
        public IEnumerable<PurchasableItem> GetAvailableProducts()
        {
            return productCatalog.Values;
        }
        
        /// <summary>
        /// Get products by category.
        /// </summary>
        public IEnumerable<PurchasableItem> GetProductsByCategory(PurchaseCategory category)
        {
            foreach (var item in productCatalog.Values)
            {
                if (item.Category == category && item.IsAvailable)
                {
                    yield return item;
                }
            }
        }
        
        /// <summary>
        /// Get localized price for a product.
        /// </summary>
        public string GetLocalizedPrice(string productId)
        {
            if (!isInitialized || storeController == null) return "---";
            
            var product = storeController.products.WithID(productId);
            return product?.metadata.localizedPriceString ?? "---";
        }
        
        #endregion
    }
    
    #endregion
}

