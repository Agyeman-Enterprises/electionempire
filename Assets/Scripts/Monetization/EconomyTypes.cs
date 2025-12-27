// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - ECONOMY SYSTEM TYPES
// Core type definitions for Purrkoin, CloutBux, and all economy systems
// ═══════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionEmpire.Economy
{
    #region Enums
    
    /// <summary>
    /// Currency types in the game
    /// </summary>
    public enum CurrencyType
    {
        /// <summary>Free earned currency for cosmetics and minor boosts</summary>
        CloutBux,
        
        /// <summary>Premium currency for major content and advantages</summary>
        Purrkoin,
        
        /// <summary>In-game campaign funds (not persistent)</summary>
        CampaignFunds,
        
        /// <summary>Tournament-specific tokens</summary>
        TournamentTokens,
        
        /// <summary>Season pass currency</summary>
        SeasonCredits
    }
    
    /// <summary>
    /// Transaction types for ledger tracking
    /// </summary>
    public enum EconomyTransactionType
    {
        // Earning
        Earned,
        Achievement,
        TournamentReward,
        DailyLogin,
        QuestComplete,
        Refund,
        
        // Spending
        Purchase,
        EntryFee,
        Upgrade,
        Unlock,
        
        // Trading
        Trade,
        Gift,
        
        // IAP
        RealMoneyPurchase,
        
        // Admin
        AdminGrant,
        AdminRevoke,
        Correction
    }
    
    /// <summary>
    /// Purchase categories
    /// </summary>
    public enum PurchaseCategory
    {
        Cosmetic,
        Background,
        Trait,
        ContentPack,
        SeasonPass,
        TournamentEntry,
        Boost,
        LegacyTransfer,
        Bundle
    }
    
    /// <summary>
    /// Item rarity tiers
    /// </summary>
    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Mythic,
        Unique  // One-of-a-kind tournament prizes
    }
    
    /// <summary>
    /// Cosmetic item types
    /// </summary>
    public enum CosmeticType
    {
        // Character
        Portrait,
        Outfit,
        Accessory,
        Hat,

        // Campaign
        PodiumStyle,
        SignDesign,
        BusWrap,

        // UI
        ProfileFrame,
        CardBack,
        VictoryAnimation,

        // Effects
        SpeechBubble,
        ConfettiStyle,
        Fireworks,

        // Audio
        VictoryTheme,
        CampaignMusic,
        SoundPack,

        // Portrait specific
        Portrait_Background,
        Portrait_Frame,

        // Campaign Themes
        Campaign_Theme,

        // Victory animations
        Victory_Animation,

        // Badges
        Title_Badge
    }
    
    /// <summary>
    /// Wallet status
    /// </summary>
    public enum WalletStatus
    {
        Active,
        Suspended,
        Locked,
        Closed
    }
    
    #endregion
    
    #region Core Classes
    
    /// <summary>
    /// Player's wallet containing all currencies
    /// </summary>
    [Serializable]
    public class PlayerWallet
    {
        public string PlayerId;
        public WalletStatus Status;
        public DateTime CreatedAt;
        public DateTime LastUpdated;
        
        // Currency balances
        public long CloutBux;
        public long Purrkoin;
        public long TournamentTokens;
        public long SeasonCredits;
        
        // Lifetime stats
        public long LifetimeCloutBuxEarned;
        public long LifetimePurrkoinEarned;
        public long LifetimePurrkoinSpent;
        public decimal LifetimeRealMoneySpent;
        
        // Security
        public string SecurityHash;
        public int FailedTransactionAttempts;
        public DateTime? LockoutExpiry;
        
        public PlayerWallet(string playerId)
        {
            PlayerId = playerId;
            Status = WalletStatus.Active;
            CreatedAt = DateTime.UtcNow;
            LastUpdated = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Check if player can afford a purchase
        /// </summary>
        public bool CanAfford(CurrencyType currency, long amount)
        {
            if (Status != WalletStatus.Active) return false;
            
            return currency switch
            {
                CurrencyType.CloutBux => CloutBux >= amount,
                CurrencyType.Purrkoin => Purrkoin >= amount,
                CurrencyType.TournamentTokens => TournamentTokens >= amount,
                CurrencyType.SeasonCredits => SeasonCredits >= amount,
                _ => false
            };
        }
    }
    
    /// <summary>
    /// Record of a currency transaction
    /// </summary>
    
    /// <summary>
    /// A purchasable item in the store
    /// </summary>
    [Serializable]
    public class StoreItem
    {
        public string Id;
        public string Name;
        public string Description;
        public PurchaseCategory Category;
        public ItemRarity Rarity;
        
        // Pricing
        public CurrencyType PrimaryCurrency;
        public long PrimaryPrice;
        public CurrencyType? AlternateCurrency;
        public long? AlternatePrice;
        public decimal? RealMoneyPrice; // USD
        
        // Content
        public string ContentId; // Links to actual content
        public CosmeticType? CosmeticType;
        public string IconPath;
        public string PreviewPath;
        
        // Availability
        public bool IsAvailable;
        public bool IsLimitedTime;
        public DateTime? AvailableFrom;
        public DateTime? AvailableUntil;
        public int? MaxPurchases; // Per player
        public int? GlobalStock;
        public int CurrentStock;
        
        // Requirements
        public int? MinimumLevel;
        public string RequiredAchievement;
        public List<string> Prerequisites;
        
        // Bundle info
        public bool IsBundle;
        public List<string> BundleContents;
        public float BundleDiscount;
        
        public StoreItem()
        {
            Id = Guid.NewGuid().ToString();
            IsAvailable = true;
            Prerequisites = new List<string>();
            BundleContents = new List<string>();
        }
        
        public bool IsCurrentlyAvailable()
        {
            if (!IsAvailable) return false;
            
            var now = DateTime.UtcNow;
            if (AvailableFrom.HasValue && now < AvailableFrom.Value) return false;
            if (AvailableUntil.HasValue && now > AvailableUntil.Value) return false;
            if (GlobalStock.HasValue && CurrentStock <= 0) return false;
            
            return true;
        }
    }
    
    /// <summary>
    /// A completed purchase
    /// </summary>
    [Serializable]
    public class Purchase
    {
        public string Id;
        public string PlayerId;
        public string ItemId;
        public string ItemName;
        public CurrencyType CurrencyUsed;
        public long AmountPaid;
        public decimal? RealMoneyPaid;
        public DateTime PurchasedAt;
        public bool IsRefunded;
        public DateTime? RefundedAt;
        public string RefundReason;
        
        public Purchase()
        {
            Id = Guid.NewGuid().ToString();
            PurchasedAt = DateTime.UtcNow;
        }
    }
    
    /// <summary>
    /// Player's inventory of owned items
    /// </summary>
    [Serializable]
    public class PlayerInventory
    {
        public string PlayerId;
        public List<OwnedItem> Items;
        public List<string> UnlockedBackgrounds;
        public List<string> UnlockedTraits;
        public List<string> OwnedContentPacks;
        public List<string> ActiveSeasonPasses;

        // Quick access lists for owned items and equipped cosmetics
        public List<string> OwnedItemIds;
        public List<string> EquippedCosmetics;
        public DateTime LastUpdated;

        // Currently equipped
        public string EquippedPortrait;
        public string EquippedOutfit;
        public string EquippedAccessory;
        public string EquippedProfileFrame;
        public string EquippedCardBack;
        public string EquippedVictoryAnimation;

        public PlayerInventory()
        {
            Items = new List<OwnedItem>();
            UnlockedBackgrounds = new List<string>();
            UnlockedTraits = new List<string>();
            OwnedContentPacks = new List<string>();
            ActiveSeasonPasses = new List<string>();
            OwnedItemIds = new List<string>();
            EquippedCosmetics = new List<string>();
        }

        public PlayerInventory(string playerId) : this()
        {
            PlayerId = playerId;
        }

        public bool OwnsItem(string itemId)
        {
            return Items.Exists(i => i.ItemId == itemId) || OwnedItemIds.Contains(itemId);
        }
    }
    
    /// <summary>
    /// An item owned by a player
    /// </summary>
    [Serializable]
    public class OwnedItem
    {
        public string ItemId;
        public string ItemName;
        public CosmeticType? Type;
        public ItemRarity Rarity;
        public DateTime AcquiredAt;
        public string AcquisitionSource; // "store", "tournament", "achievement", etc.
        public bool IsEquipped;
        public bool IsTradeable;
        public bool IsGiftable;
    }
    
    /// <summary>
    /// Daily login reward
    /// </summary>
    [Serializable]
    public class DailyLoginReward
    {
        public int Day;
        public CurrencyType Currency;
        public long Amount;
        public string BonusItemId;
        public bool IsMilestone; // Days 7, 14, 30, etc.
    }
    
    /// <summary>
    /// Season pass definition
    /// </summary>
    [Serializable]
    public class SeasonPass
    {
        public string Id;
        public string Name;
        public string SeasonId;
        public int TotalLevels;
        public int PointsPerLevel;
        
        // Pricing
        public long PurrkoinPrice;
        public decimal RealMoneyPrice;
        
        // Rewards
        public List<SeasonPassReward> FreeTrackRewards;
        public List<SeasonPassReward> PremiumTrackRewards;
        
        public SeasonPass()
        {
            Id = Guid.NewGuid().ToString();
            FreeTrackRewards = new List<SeasonPassReward>();
            PremiumTrackRewards = new List<SeasonPassReward>();
        }
    }
    
    /// <summary>
    /// Reward at a season pass level
    /// </summary>
    [Serializable]
    public class SeasonPassReward
    {
        public int Level;
        public CurrencyType? CurrencyReward;
        public long? CurrencyAmount;
        public string ItemId;
        public string ItemName;
        public ItemRarity? Rarity;
    }
    
    /// <summary>
    /// Player's season pass progress
    /// </summary>
    [Serializable]
    public class PlayerSeasonProgress
    {
        public string PlayerId;
        public string SeasonPassId;
        public bool HasPremium;
        public int CurrentLevel;
        public int CurrentPoints;
        public List<int> ClaimedFreeRewards;
        public List<int> ClaimedPremiumRewards;
        public DateTime LastUpdated;
        
        public PlayerSeasonProgress()
        {
            ClaimedFreeRewards = new List<int>();
            ClaimedPremiumRewards = new List<int>();
        }
    }
    
    #endregion
    
    #region Purrkoin Specific
    
    /// <summary>
    /// Real money purchase package
    /// </summary>
    [Serializable]
    public class PurrkoinPackage
    {
        public string Id;
        public string Name;
        public long PurrkoinAmount;
        public long BonusPurrkoin;
        public decimal PriceUSD;
        public string PlatformProductId; // For App Store / Google Play
        public bool IsBestValue;
        public bool IsPopular;
        public float? DiscountPercent;
        public DateTime? DiscountExpiry;
        
        public long TotalPurrkoin => PurrkoinAmount + BonusPurrkoin;
        public decimal PricePerPurrkoin => TotalPurrkoin > 0 ? PriceUSD / TotalPurrkoin : 0;
    }
    
    /// <summary>
    /// Real money purchase record
    /// </summary>
    [Serializable]
    public class IAPPurchase
    {
        public string Id;
        public string PlayerId;
        public string PackageId;
        public string PlatformTransactionId;
        public string Platform; // "steam", "ios", "android", "web"
        public decimal AmountPaid;
        public string Currency; // "USD", "EUR", etc.
        public long PurrkoinGranted;
        public DateTime PurchasedAt;
        public bool IsVerified;
        public string VerificationReceipt;
        public bool IsRefunded;
        
        public IAPPurchase()
        {
            Id = Guid.NewGuid().ToString();
            PurchasedAt = DateTime.UtcNow;
        }
    }
    
    /// <summary>
    /// Spending analytics for a player
    /// </summary>
    [Serializable]
    public class PlayerSpendingProfile
    {
        public string PlayerId;
        public decimal TotalSpent;
        public int TotalPurchases;
        public DateTime? FirstPurchase;
        public DateTime? LastPurchase;
        public string PreferredPackage;
        public SpenderTier Tier;
        
        // For responsible gaming
        public decimal? MonthlySpendingLimit;
        public decimal CurrentMonthSpending;
        public bool HasOptedIntoLimits;
    }
    
    /// <summary>
    /// Spender classification
    /// </summary>
    public enum SpenderTier
    {
        NonSpender,
        Minnow,      // < $10
        Dolphin,     // $10 - $50
        Whale,       // $50 - $200
        MegaWhale    // > $200
    }
    
    #endregion
    
    #region Tournament Economy Integration
    
    /// <summary>
    /// Tournament entry fee configuration
    /// </summary>
    [Serializable]
    public class TournamentEntryFee
    {
        public string TournamentId;
        public CurrencyType Currency;
        public long Amount;
        public bool IsRefundable;
        public float RefundPercentage; // If refundable
        public DateTime RefundDeadline;
    }
    
    /// <summary>
    /// Tournament prize pool
    /// </summary>
    [Serializable]
    public class TournamentPrizePool
    {
        public string TournamentId;
        public long TotalPool;
        public CurrencyType Currency;
        public bool IsContributedByEntryFees;
        public float EntryFeeContributionPercent;
        public long BasePrizePool; // Guaranteed minimum
        public List<PrizePlacement> Placements;
        
        public TournamentPrizePool()
        {
            Placements = new List<PrizePlacement>();
        }
    }
    
    /// <summary>
    /// Prize for a specific placement
    /// </summary>
    [Serializable]
    public class PrizePlacement
    {
        public int MinPlace;
        public int MaxPlace;
        public float PoolPercentage;
        public long FixedAmount;
        public List<string> BonusItemIds;
        public string Title; // "Champion", "Runner-Up", etc.
        
        public PrizePlacement()
        {
            BonusItemIds = new List<string>();
        }
        
        /// <summary>
        /// Calculate prize for this placement given total pool
        /// </summary>
        public long CalculatePrize(long totalPool)
        {
            return FixedAmount + (long)(totalPool * PoolPercentage / 100f);
        }
    }
    
    #endregion
    
    #region Earning Sources
    
    /// <summary>
    /// Ways to earn CloutBux
    /// </summary>
    [Serializable]
    public class CloutBuxEarningSource
    {
        public string Id;
        public string Name;
        public string Description;
        public long Amount;
        public bool IsRepeatable;
        public int? DailyLimit;
        public int? LifetimeLimit;
        public string TriggerType; // "election_win", "scandal_survive", etc.
        public Dictionary<string, object> TriggerConditions;
        
        public CloutBuxEarningSource()
        {
            TriggerConditions = new Dictionary<string, object>();
        }
    }
    
    /// <summary>
    /// Standard earning rates
    /// </summary>
    public static class EarningRates
    {
        // Election victories
        public const long Tier1Victory = 100;
        public const long Tier2Victory = 250;
        public const long Tier3Victory = 500;
        public const long Tier4Victory = 1000;
        public const long Tier5Victory = 2500;
        
        // Other gameplay
        public const long ScandalSurvived = 50;
        public const long CrisisManaged = 75;
        public const long DebateWon = 30;
        public const long FirstPlaythrough = 500;
        
        // Daily
        public const long DailyLogin = 25;
        public const long DailyLoginStreak7 = 100;
        public const long DailyLoginStreak30 = 500;
        
        // Achievements
        public const long AchievementEasy = 50;
        public const long AchievementMedium = 150;
        public const long AchievementHard = 500;
        public const long AchievementExtreme = 1500;
        
        // Tournament
        public const long TournamentParticipation = 50;
        public const long TournamentTop8 = 200;
        public const long TournamentTop4 = 400;
        public const long TournamentRunnerUp = 750;
        public const long TournamentChampion = 1500;
    }
    
    /// <summary>
    /// Ways to earn Purrkoin (without paying)
    /// </summary>
    public static class PurrkoinEarningRates
    {
        // Very rare free earning
        public const long SeasonPassMilestone = 50;
        public const long ChampionshipWin = 100;
        public const long SpecialEventCompletion = 25;
        public const long ReferralBonus = 100;
        
        // Lifetime achievements
        public const long First1000Hours = 200;
        public const long AllBackgroundsUnlocked = 150;
        public const long LegendaryRank = 500;
    }
    
    #endregion
    
    #region Events
    
    /// <summary>
    /// Event fired when currency is earned or spent
    /// </summary>
    [Serializable]
    public class CurrencyChangedEvent
    {
        public string PlayerId;
        public CurrencyType Currency;
        public long PreviousBalance;
        public long NewBalance;
        public long ChangeAmount;
        public EconomyTransactionType Type;
        public string Description;
        public DateTime Timestamp;
    }
    
    /// <summary>
    /// Event fired when a purchase is made
    /// </summary>
    [Serializable]
    public class PurchaseCompletedEvent
    {
        public string PlayerId;
        public Purchase Purchase;
        public StoreItem Item;
        public DateTime Timestamp;
    }
    
    /// <summary>
    /// Event fired when an item is unlocked
    /// </summary>
    [Serializable]
    public class ItemUnlockedEvent
    {
        public string PlayerId;
        public OwnedItem Item;
        public string UnlockSource;
        public DateTime Timestamp;
    }
    
    #endregion
    
    #region Configuration
    
    /// <summary>
    /// Economy system configuration
    /// </summary>
    [Serializable]
    public class EconomyConfig
    {
        // Currency exchange (if ever implemented)
        public float CloutBuxToPurrkoinRate; // 0 = no exchange
        public long MinimumExchangeAmount;
        
        // Anti-inflation
        public long DailyCloutBuxCap;
        public long WeeklyCloutBuxCap;
        
        // Spending limits (responsible gaming)
        public bool EnableSpendingLimits;
        public decimal DefaultMonthlyLimit;
        public decimal MaxMonthlyLimit;
        
        // Gifting
        public bool AllowGifting;
        public int GiftCooldownHours;
        public long MaxGiftValuePerDay;
        
        // Trading
        public bool AllowTrading;
        public float TradingFeePercent;
        
        public EconomyConfig()
        {
            CloutBuxToPurrkoinRate = 0; // No direct exchange
            DailyCloutBuxCap = 5000;
            WeeklyCloutBuxCap = 25000;
            EnableSpendingLimits = true;
            DefaultMonthlyLimit = 100m;
            MaxMonthlyLimit = 500m;
            AllowGifting = true;
            GiftCooldownHours = 24;
            MaxGiftValuePerDay = 1000;
            AllowTrading = false;
            TradingFeePercent = 10f;
        }
    }
    
    #endregion

    /// <summary>
    /// Represents a cosmetic item that can be equipped.
    /// </summary>
    [Serializable]
    public class CosmeticItem
    {
        public string ItemId;
        public string DisplayName;
        public string Description;
        public CosmeticType CosmeticType;
        public CosmeticType Type { get => CosmeticType; set => CosmeticType = value; }
        public ItemRarity Rarity;
        public Sprite Icon;
        public string UnlockData;
        public bool IsUnlocked;
        public bool IsEquipped;

        // Pricing
        public CurrencyType CurrencyType;
        public long Price;
        public bool IsAvailable;

        // Display
        public string AssetPath;
        public bool IsAnimated;

        // Category
        public PurchaseCategory Category;

        public CosmeticItem() { }

        public CosmeticItem(string id, string name, CosmeticType type, ItemRarity rarity = ItemRarity.Common)
        {
            ItemId = id;
            DisplayName = name;
            CosmeticType = type;
            Rarity = rarity;
            IsAvailable = true;
        }
    }
    
    /// <summary>
    /// Represents an item available for purchase.
    /// </summary>
    [Serializable]
    public class PurchasableItem
    {
        public string ItemId;
        public string DisplayName;
        public string Description;
        public CurrencyType PriceType;
        public CurrencyType CurrencyType { get => PriceType; set => PriceType = value; }
        public long Price;
        public PurchaseCategory Category;
        public ItemRarity Rarity;
        public bool IsAvailable;
        public Sprite Icon;

        // For real-money purchases
        public string StoreProductId;
        public string LocalizedPrice;

        public PurchasableItem() { }

        public PurchasableItem(string id, string name, long price, CurrencyType priceType = CurrencyType.CloutBux)
        {
            ItemId = id;
            DisplayName = name;
            Price = price;
            PriceType = priceType;
            IsAvailable = true;
        }
    }
}
