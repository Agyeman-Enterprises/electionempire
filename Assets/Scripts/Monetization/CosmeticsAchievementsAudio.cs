// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - Cosmetics, Achievements & Audio Systems
// Sprint 11: Shop interface, achievement tracking, sound/music management
// ═══════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace ElectionEmpire.Monetization
{
    #region Cosmetics Shop System
    
    /// <summary>
    /// Filter options for the cosmetics shop.
    /// </summary>
    public enum ShopFilter
    {
        All,
        Owned,
        NotOwned,
        Affordable,
        New,
        Sale,
        ByType,
        ByRarity
    }
    
    /// <summary>
    /// Sort options for shop display.
    /// </summary>
    public enum ShopSort
    {
        Featured,
        PriceLowToHigh,
        PriceHighToLow,
        RarityAscending,
        RarityDescending,
        Alphabetical,
        NewestFirst,
        Popularity
    }
    
    /// <summary>
    /// A featured item in the shop.
    /// </summary>
    [Serializable]
    public class FeaturedItem
    {
        public string ItemId;
        public int Position;
        public string BannerText;
        public Color BannerColor;
        public DateTime FeaturedUntil;
        public float DiscountPercent;
    }
    
    /// <summary>
    /// Manages the cosmetics shop interface and inventory.
    /// </summary>
    public class CosmeticsShop
    {
        private Dictionary<string, CosmeticItem> cosmeticCatalog;
        private List<FeaturedItem> featuredItems;
        private PlayerInventory playerInventory;
        private CurrencyManager currencyManager;
        
        // Events
        public event Action<CosmeticItem> OnItemPreviewed;
        public event Action<CosmeticItem> OnItemEquipped;
        public event Action<CosmeticItem> OnItemPurchased;
        public event Action OnShopRefreshed;
        
        // Shop state
        private DateTime lastRefresh;
        private TimeSpan refreshInterval = TimeSpan.FromHours(24);
        
        public CosmeticsShop(PlayerInventory inventory, CurrencyManager currency)
        {
            playerInventory = inventory;
            currencyManager = currency;
            cosmeticCatalog = new Dictionary<string, CosmeticItem>();
            featuredItems = new List<FeaturedItem>();
            
            InitializeCatalog();
            RefreshFeaturedItems();
        }
        
        #region Catalog Initialization
        
        private void InitializeCatalog()
        {
            // Portrait Backgrounds
            AddCosmetic(new CosmeticItem
            {
                ItemId = "bg_capitol_dome",
                DisplayName = "Capitol Dome",
                Description = "The iconic dome of democracy behind you",
                CosmeticType = CosmeticType.Portrait_Background,
                Rarity = ItemRarity.Common,
                CurrencyType = CurrencyType.CloutBux,
                Price = 500,
                IsAvailable = true,
                AssetPath = "Backgrounds/capitol_dome"
            });
            
            AddCosmetic(new CosmeticItem
            {
                ItemId = "bg_oval_office",
                DisplayName = "Oval Office",
                Description = "Presidential prestige in every photo",
                CosmeticType = CosmeticType.Portrait_Background,
                Rarity = ItemRarity.Rare,
                CurrencyType = CurrencyType.Purrkoin,
                Price = 100,
                IsAvailable = true,
                AssetPath = "Backgrounds/oval_office"
            });
            
            AddCosmetic(new CosmeticItem
            {
                ItemId = "bg_campaign_rally",
                DisplayName = "Campaign Rally",
                Description = "Cheering crowds and waving flags",
                CosmeticType = CosmeticType.Portrait_Background,
                Rarity = ItemRarity.Uncommon,
                CurrencyType = CurrencyType.CloutBux,
                Price = 750,
                IsAvailable = true,
                AssetPath = "Backgrounds/rally"
            });
            
            AddCosmetic(new CosmeticItem
            {
                ItemId = "bg_smoke_filled_room",
                DisplayName = "Smoke-Filled Room",
                Description = "Where the real deals are made",
                CosmeticType = CosmeticType.Portrait_Background,
                Rarity = ItemRarity.Epic,
                CurrencyType = CurrencyType.Purrkoin,
                Price = 250,
                IsAvailable = true,
                AssetPath = "Backgrounds/smoke_room"
            });
            
            AddCosmetic(new CosmeticItem
            {
                ItemId = "bg_chaos_explosion",
                DisplayName = "Chaos Explosion",
                Description = "For when everything is on fire (metaphorically)",
                CosmeticType = CosmeticType.Portrait_Background,
                Rarity = ItemRarity.Legendary,
                CurrencyType = CurrencyType.Purrkoin,
                Price = 500,
                IsAvailable = true,
                AssetPath = "Backgrounds/chaos_fire"
            });
            
            // Portrait Frames
            AddCosmetic(new CosmeticItem
            {
                ItemId = "frame_bronze_seal",
                DisplayName = "Bronze Seal",
                Description = "An official-looking bronze frame",
                CosmeticType = CosmeticType.Portrait_Frame,
                Rarity = ItemRarity.Common,
                CurrencyType = CurrencyType.CloutBux,
                Price = 300,
                IsAvailable = true,
                AssetPath = "Frames/bronze_seal"
            });
            
            AddCosmetic(new CosmeticItem
            {
                ItemId = "frame_gold_eagle",
                DisplayName = "Golden Eagle",
                Description = "Majestic golden eagles frame your portrait",
                CosmeticType = CosmeticType.Portrait_Frame,
                Rarity = ItemRarity.Rare,
                CurrencyType = CurrencyType.CloutBux,
                Price = 1000,
                IsAvailable = true,
                AssetPath = "Frames/gold_eagle"
            });
            
            AddCosmetic(new CosmeticItem
            {
                ItemId = "frame_scandal_tabloid",
                DisplayName = "Tabloid Cover",
                Description = "Your portrait as a shocking headline",
                CosmeticType = CosmeticType.Portrait_Frame,
                Rarity = ItemRarity.Uncommon,
                CurrencyType = CurrencyType.CloutBux,
                Price = 600,
                IsAvailable = true,
                AssetPath = "Frames/tabloid"
            });
            
            AddCosmetic(new CosmeticItem
            {
                ItemId = "frame_money_border",
                DisplayName = "Money Border",
                Description = "Framed in cold, hard cash",
                CosmeticType = CosmeticType.Portrait_Frame,
                Rarity = ItemRarity.Epic,
                CurrencyType = CurrencyType.Purrkoin,
                Price = 200,
                IsAvailable = true,
                AssetPath = "Frames/money"
            });
            
            // Campaign Themes
            AddCosmetic(new CosmeticItem
            {
                ItemId = "theme_patriot",
                DisplayName = "Patriot Theme",
                Description = "Red, white, and blue everything",
                CosmeticType = CosmeticType.Campaign_Theme,
                Rarity = ItemRarity.Common,
                CurrencyType = CurrencyType.CloutBux,
                Price = 400,
                IsAvailable = true,
                AssetPath = "Themes/patriot"
            });
            
            AddCosmetic(new CosmeticItem
            {
                ItemId = "theme_dark_horse",
                DisplayName = "Dark Horse",
                Description = "Mysterious and unexpected",
                CosmeticType = CosmeticType.Campaign_Theme,
                Rarity = ItemRarity.Rare,
                CurrencyType = CurrencyType.CloutBux,
                Price = 800,
                IsAvailable = true,
                AssetPath = "Themes/dark_horse"
            });
            
            AddCosmetic(new CosmeticItem
            {
                ItemId = "theme_chaos_mode",
                DisplayName = "Chaos Mode Theme",
                Description = "Everything is fine. EVERYTHING IS FINE.",
                CosmeticType = CosmeticType.Campaign_Theme,
                Rarity = ItemRarity.Legendary,
                CurrencyType = CurrencyType.Purrkoin,
                Price = 400,
                IsAvailable = true,
                AssetPath = "Themes/chaos"
            });
            
            // Victory Animations
            AddCosmetic(new CosmeticItem
            {
                ItemId = "victory_confetti",
                DisplayName = "Confetti Shower",
                Description = "Classic celebration confetti",
                CosmeticType = CosmeticType.Victory_Animation,
                Rarity = ItemRarity.Common,
                CurrencyType = CurrencyType.CloutBux,
                Price = 250,
                IsAvailable = true,
                IsAnimated = true,
                AssetPath = "Animations/victory_confetti"
            });
            
            AddCosmetic(new CosmeticItem
            {
                ItemId = "victory_fireworks",
                DisplayName = "Fireworks Display",
                Description = "Explosive celebration",
                CosmeticType = CosmeticType.Victory_Animation,
                Rarity = ItemRarity.Rare,
                CurrencyType = CurrencyType.CloutBux,
                Price = 750,
                IsAvailable = true,
                IsAnimated = true,
                AssetPath = "Animations/victory_fireworks"
            });
            
            AddCosmetic(new CosmeticItem
            {
                ItemId = "victory_money_rain",
                DisplayName = "Money Rain",
                Description = "It's raining campaign contributions!",
                CosmeticType = CosmeticType.Victory_Animation,
                Rarity = ItemRarity.Epic,
                CurrencyType = CurrencyType.Purrkoin,
                Price = 300,
                IsAvailable = true,
                IsAnimated = true,
                AssetPath = "Animations/victory_money"
            });
            
            // Title Badges
            AddCosmetic(new CosmeticItem
            {
                ItemId = "badge_newcomer",
                DisplayName = "Political Newcomer",
                Description = "Fresh face in the game",
                CosmeticType = CosmeticType.Title_Badge,
                Rarity = ItemRarity.Common,
                CurrencyType = CurrencyType.CloutBux,
                Price = 100,
                IsAvailable = true,
                AssetPath = "Badges/newcomer"
            });
            
            AddCosmetic(new CosmeticItem
            {
                ItemId = "badge_scandal_survivor",
                DisplayName = "Scandal Survivor",
                Description = "I've seen things...",
                CosmeticType = CosmeticType.Title_Badge,
                Rarity = ItemRarity.Uncommon,
                CurrencyType = CurrencyType.CloutBux,
                Price = 350,
                IsAvailable = true,
                AssetPath = "Badges/survivor"
            });
            
            AddCosmetic(new CosmeticItem
            {
                ItemId = "badge_kingmaker",
                DisplayName = "Kingmaker",
                Description = "The power behind the throne",
                CosmeticType = CosmeticType.Title_Badge,
                Rarity = ItemRarity.Epic,
                CurrencyType = CurrencyType.Purrkoin,
                Price = 250,
                IsAvailable = true,
                AssetPath = "Badges/kingmaker"
            });
            
            AddCosmetic(new CosmeticItem
            {
                ItemId = "badge_supreme_leader",
                DisplayName = "Supreme Leader",
                Description = "Absolute power achieved",
                CosmeticType = CosmeticType.Title_Badge,
                Rarity = ItemRarity.Legendary,
                CurrencyType = CurrencyType.Purrkoin,
                Price = 750,
                IsAvailable = true,
                AssetPath = "Badges/supreme"
            });
        }
        
        private void AddCosmetic(CosmeticItem item)
        {
            item.Category = PurchaseCategory.Cosmetic;
            cosmeticCatalog[item.ItemId] = item;
        }
        
        #endregion
        
        #region Shop Operations
        
        /// <summary>
        /// Get filtered and sorted cosmetic items.
        /// </summary>
        public List<CosmeticItem> GetItems(
            ShopFilter filter = ShopFilter.All,
            ShopSort sort = ShopSort.Featured,
            CosmeticType? typeFilter = null,
            ItemRarity? rarityFilter = null)
        {
            IEnumerable<CosmeticItem> items = cosmeticCatalog.Values;
            
            // Apply type filter
            if (typeFilter.HasValue)
            {
                items = items.Where(i => i.CosmeticType == typeFilter.Value);
            }
            
            // Apply rarity filter
            if (rarityFilter.HasValue)
            {
                items = items.Where(i => i.Rarity == rarityFilter.Value);
            }
            
            // Apply main filter
            items = filter switch
            {
                ShopFilter.Owned => items.Where(i => playerInventory.OwnsItem(i.ItemId)),
                ShopFilter.NotOwned => items.Where(i => !playerInventory.OwnsItem(i.ItemId)),
                ShopFilter.Affordable => items.Where(i => currencyManager.CanAfford(i.CurrencyType, i.Price)),
                ShopFilter.New => items.Where(i => IsNewItem(i)),
                ShopFilter.Sale => items.Where(i => IsOnSale(i)),
                _ => items
            };
            
            // Apply sort
            items = sort switch
            {
                ShopSort.PriceLowToHigh => items.OrderBy(i => i.Price),
                ShopSort.PriceHighToLow => items.OrderByDescending(i => i.Price),
                ShopSort.RarityAscending => items.OrderBy(i => i.Rarity),
                ShopSort.RarityDescending => items.OrderByDescending(i => i.Rarity),
                ShopSort.Alphabetical => items.OrderBy(i => i.DisplayName),
                ShopSort.Featured => items.OrderByDescending(i => GetFeaturedScore(i.ItemId)),
                _ => items
            };
            
            return items.ToList();
        }
        
        /// <summary>
        /// Get featured items for the shop front.
        /// </summary>
        public List<FeaturedItem> GetFeaturedItems()
        {
            return featuredItems
                .Where(f => f.FeaturedUntil > DateTime.UtcNow)
                .OrderBy(f => f.Position)
                .ToList();
        }
        
        /// <summary>
        /// Preview a cosmetic item.
        /// </summary>
        public void PreviewItem(string itemId)
        {
            if (cosmeticCatalog.TryGetValue(itemId, out var item))
            {
                OnItemPreviewed?.Invoke(item);
            }
        }
        
        /// <summary>
        /// Purchase a cosmetic item.
        /// </summary>
        public bool PurchaseItem(string itemId)
        {
            if (!cosmeticCatalog.TryGetValue(itemId, out var item))
            {
                return false;
            }
            
            if (playerInventory.OwnsItem(itemId))
            {
                return false;
            }
            
            // Apply any active discount
            long finalPrice = GetFinalPrice(itemId);
            
            if (!currencyManager.CanAfford(item.CurrencyType, finalPrice))
            {
                return false;
            }
            
            if (currencyManager.Debit(item.CurrencyType, finalPrice, $"Purchase: {item.DisplayName}"))
            {
                playerInventory.OwnedItemIds.Add(itemId);
                playerInventory.LastUpdated = DateTime.UtcNow;
                OnItemPurchased?.Invoke(item);
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Equip a cosmetic item.
        /// </summary>
        public bool EquipItem(string itemId)
        {
            if (!playerInventory.OwnsItem(itemId))
            {
                return false;
            }
            
            if (!cosmeticCatalog.TryGetValue(itemId, out var item))
            {
                return false;
            }
            
            // Unequip current item of same type
            var equipped = playerInventory.EquippedCosmetics
                .FirstOrDefault(e => cosmeticCatalog.TryGetValue(e, out var c) && c.CosmeticType == item.CosmeticType);
            
            if (!string.IsNullOrEmpty(equipped))
            {
                playerInventory.EquippedCosmetics.Remove(equipped);
            }
            
            playerInventory.EquippedCosmetics.Add(itemId);
            OnItemEquipped?.Invoke(item);
            return true;
        }
        
        /// <summary>
        /// Get currently equipped item of a type.
        /// </summary>
        public CosmeticItem GetEquippedItem(CosmeticType type)
        {
            foreach (var itemId in playerInventory.EquippedCosmetics)
            {
                if (cosmeticCatalog.TryGetValue(itemId, out var item) && item.CosmeticType == type)
                {
                    return item;
                }
            }
            return null;
        }
        
        private void RefreshFeaturedItems()
        {
            featuredItems.Clear();
            
            // Add daily deals
            var random = new System.Random((int)(DateTime.UtcNow.Ticks / TimeSpan.TicksPerDay));
            var availableItems = cosmeticCatalog.Values
                .Where(i => i.IsAvailable && !playerInventory.OwnsItem(i.ItemId))
                .ToList();
            
            if (availableItems.Count > 0)
            {
                // Feature 3 random items
                for (int i = 0; i < Math.Min(3, availableItems.Count); i++)
                {
                    int index = random.Next(availableItems.Count);
                    var item = availableItems[index];
                    
                    featuredItems.Add(new FeaturedItem
                    {
                        ItemId = item.ItemId,
                        Position = i,
                        BannerText = i == 0 ? "DAILY DEAL" : "FEATURED",
                        BannerColor = i == 0 ? Color.yellow : Color.cyan,
                        FeaturedUntil = DateTime.UtcNow.Date.AddDays(1),
                        DiscountPercent = i == 0 ? 0.25f : 0f // 25% off for daily deal
                    });
                    
                    availableItems.RemoveAt(index);
                }
            }
            
            lastRefresh = DateTime.UtcNow;
            OnShopRefreshed?.Invoke();
        }
        
        private bool IsNewItem(CosmeticItem item)
        {
            // Consider items "new" for 7 days
            return false; // Would check release date in production
        }
        
        private bool IsOnSale(CosmeticItem item)
        {
            return featuredItems.Any(f => f.ItemId == item.ItemId && f.DiscountPercent > 0);
        }
        
        private int GetFeaturedScore(string itemId)
        {
            var featured = featuredItems.FirstOrDefault(f => f.ItemId == itemId);
            return featured != null ? 1000 - featured.Position : 0;
        }
        
        private long GetFinalPrice(string itemId)
        {
            if (!cosmeticCatalog.TryGetValue(itemId, out var item))
            {
                return 0;
            }
            
            var featured = featuredItems.FirstOrDefault(f => f.ItemId == itemId);
            if (featured != null && featured.DiscountPercent > 0)
            {
                return (long)(item.Price * (1 - featured.DiscountPercent));
            }
            
            return item.Price;
        }
        
        #endregion
    }
    
    #endregion
    
    #region Achievement System
    
    /// <summary>
    /// Categories of achievements.
    /// </summary>
    public enum AchievementCategory
    {
        Campaign,
        Elections,
        Scandals,
        Governance,
        Relationships,
        Chaos,
        Meta,
        Secret
    }
    
    /// <summary>
    /// Definition of an achievement.
    /// </summary>
    [Serializable]
    public class Achievement
    {
        public string AchievementId;
        public string DisplayName;
        public string Description;
        public string HiddenDescription;    // For secret achievements
        public AchievementCategory Category;
        public ItemRarity Rarity;
        
        // Requirements
        public string TriggerCondition;     // Condition code
        public Dictionary<string, float> RequiredStats;
        public int RequiredCount;           // For cumulative achievements
        
        // Rewards
        public long CloutBuxReward;
        public long PurrkoinReward;
        public int LegacyPointReward;
        public string UnlockItemId;         // Cosmetic unlock
        
        // Display
        public string IconPath;
        public string CompletionIconPath;
        public bool IsSecret;
        public bool IsSteamAchievement;
        public string SteamAchievementId;
        
        // Progress
        public float ProgressPercent;
        public int CurrentCount;
        public bool IsCompleted;
        public DateTime? CompletedDate;
        
        public Achievement()
        {
            RequiredStats = new Dictionary<string, float>();
        }
    }
    
    /// <summary>
    /// Player's achievement progress.
    /// </summary>
    [Serializable]
    public class AchievementProgress
    {
        public string AchievementId;
        public int CurrentCount;
        public float ProgressValue;
        public bool IsCompleted;
        public DateTime? CompletedDate;
        public Dictionary<string, object> TrackedData;
        
        public AchievementProgress()
        {
            TrackedData = new Dictionary<string, object>();
        }
    }
    
    /// <summary>
    /// Manages achievement tracking and rewards.
    /// </summary>
    public class AchievementManager
    {
        private Dictionary<string, Achievement> achievements;
        private Dictionary<string, AchievementProgress> playerProgress;
        private CurrencyManager currencyManager;
        private PlayerInventory inventory;
        
        // Events
        public event Action<Achievement> OnAchievementUnlocked;
        public event Action<Achievement, float> OnAchievementProgress;
        
        // Stats tracking
        private Dictionary<string, float> sessionStats;
        private Dictionary<string, float> lifetimeStats;
        
        public AchievementManager(CurrencyManager currency, PlayerInventory inv)
        {
            currencyManager = currency;
            inventory = inv;
            achievements = new Dictionary<string, Achievement>();
            playerProgress = new Dictionary<string, AchievementProgress>();
            sessionStats = new Dictionary<string, float>();
            lifetimeStats = new Dictionary<string, float>();
            
            InitializeAchievements();
        }
        
        #region Initialization
        
        private void InitializeAchievements()
        {
            // Campaign Achievements
            AddAchievement(new Achievement
            {
                AchievementId = "first_campaign",
                DisplayName = "The Journey Begins",
                Description = "Complete your first campaign",
                Category = AchievementCategory.Campaign,
                Rarity = ItemRarity.Common,
                CloutBuxReward = 100,
                IsSteamAchievement = true,
                SteamAchievementId = "ACH_FIRST_CAMPAIGN"
            });
            
            AddAchievement(new Achievement
            {
                AchievementId = "underdog_victory",
                DisplayName = "Underdog",
                Description = "Win an election when polling below 30%",
                Category = AchievementCategory.Elections,
                Rarity = ItemRarity.Rare,
                CloutBuxReward = 300,
                LegacyPointReward = 50,
                IsSteamAchievement = true,
                SteamAchievementId = "ACH_UNDERDOG"
            });
            
            AddAchievement(new Achievement
            {
                AchievementId = "landslide",
                DisplayName = "Landslide Victory",
                Description = "Win an election with more than 70% of the vote",
                Category = AchievementCategory.Elections,
                Rarity = ItemRarity.Rare,
                CloutBuxReward = 250,
                IsSteamAchievement = true,
                SteamAchievementId = "ACH_LANDSLIDE"
            });
            
            AddAchievement(new Achievement
            {
                AchievementId = "reach_president",
                DisplayName = "Mr./Madam President",
                Description = "Reach the office of President",
                Category = AchievementCategory.Elections,
                Rarity = ItemRarity.Epic,
                CloutBuxReward = 1000,
                PurrkoinReward = 25,
                LegacyPointReward = 200,
                IsSteamAchievement = true,
                SteamAchievementId = "ACH_PRESIDENT"
            });
            
            // Scandal Achievements
            AddAchievement(new Achievement
            {
                AchievementId = "scandal_survivor",
                DisplayName = "Scandal Survivor",
                Description = "Survive 10 scandals across all playthroughs",
                Category = AchievementCategory.Scandals,
                Rarity = ItemRarity.Uncommon,
                RequiredCount = 10,
                CloutBuxReward = 200,
                UnlockItemId = "badge_scandal_survivor"
            });
            
            AddAchievement(new Achievement
            {
                AchievementId = "clean_politician",
                DisplayName = "Clean Record",
                Description = "Win an election with zero scandals",
                Category = AchievementCategory.Scandals,
                Rarity = ItemRarity.Rare,
                CloutBuxReward = 300,
                LegacyPointReward = 75,
                IsSteamAchievement = true,
                SteamAchievementId = "ACH_CLEAN"
            });
            
            AddAchievement(new Achievement
            {
                AchievementId = "scandal_master",
                DisplayName = "Scandal Master",
                Description = "Cause 25 scandals for your opponents",
                Category = AchievementCategory.Scandals,
                Rarity = ItemRarity.Epic,
                RequiredCount = 25,
                CloutBuxReward = 500,
                PurrkoinReward = 15
            });
            
            // Governance Achievements
            AddAchievement(new Achievement
            {
                AchievementId = "policy_master",
                DisplayName = "Policy Wonk",
                Description = "Successfully implement 50 policies",
                Category = AchievementCategory.Governance,
                Rarity = ItemRarity.Uncommon,
                RequiredCount = 50,
                CloutBuxReward = 300
            });
            
            AddAchievement(new Achievement
            {
                AchievementId = "crisis_manager",
                DisplayName = "Crisis Manager",
                Description = "Resolve 20 crises with positive outcomes",
                Category = AchievementCategory.Governance,
                Rarity = ItemRarity.Rare,
                RequiredCount = 20,
                CloutBuxReward = 400,
                LegacyPointReward = 50
            });
            
            AddAchievement(new Achievement
            {
                AchievementId = "approval_king",
                DisplayName = "Approval King",
                Description = "Maintain 80%+ approval for a full term",
                Category = AchievementCategory.Governance,
                Rarity = ItemRarity.Epic,
                CloutBuxReward = 600,
                LegacyPointReward = 100,
                IsSteamAchievement = true,
                SteamAchievementId = "ACH_APPROVAL_KING"
            });
            
            // Chaos Achievements
            AddAchievement(new Achievement
            {
                AchievementId = "chaos_mode_complete",
                DisplayName = "Chaos Champion",
                Description = "Complete a game in Chaos Mode",
                Category = AchievementCategory.Chaos,
                Rarity = ItemRarity.Rare,
                CloutBuxReward = 500,
                UnlockItemId = "theme_chaos_mode",
                IsSteamAchievement = true,
                SteamAchievementId = "ACH_CHAOS"
            });
            
            AddAchievement(new Achievement
            {
                AchievementId = "drunk_victory",
                DisplayName = "Hold My Beer",
                Description = "Win an election while under 'refreshed' status",
                Category = AchievementCategory.Chaos,
                Rarity = ItemRarity.Epic,
                CloutBuxReward = 400,
                PurrkoinReward = 20,
                IsSteamAchievement = true,
                SteamAchievementId = "ACH_DRUNK"
            });
            
            AddAchievement(new Achievement
            {
                AchievementId = "viral_master",
                DisplayName = "Viral Sensation",
                Description = "Go viral 10 times",
                Category = AchievementCategory.Chaos,
                Rarity = ItemRarity.Rare,
                RequiredCount = 10,
                CloutBuxReward = 350
            });
            
            // Relationship Achievements
            AddAchievement(new Achievement
            {
                AchievementId = "coalition_builder",
                DisplayName = "Coalition Builder",
                Description = "Form alliances with 5 different factions",
                Category = AchievementCategory.Relationships,
                Rarity = ItemRarity.Uncommon,
                RequiredCount = 5,
                CloutBuxReward = 250
            });
            
            AddAchievement(new Achievement
            {
                AchievementId = "kingmaker",
                DisplayName = "Kingmaker",
                Description = "Get 3 allied candidates elected",
                Category = AchievementCategory.Relationships,
                Rarity = ItemRarity.Epic,
                RequiredCount = 3,
                CloutBuxReward = 500,
                UnlockItemId = "badge_kingmaker"
            });
            
            // Secret Achievements
            AddAchievement(new Achievement
            {
                AchievementId = "secret_supreme_leader",
                DisplayName = "???",
                Description = "???",
                HiddenDescription = "Become Supreme Leader through the Authoritarian path",
                Category = AchievementCategory.Secret,
                Rarity = ItemRarity.Legendary,
                IsSecret = true,
                CloutBuxReward = 1000,
                PurrkoinReward = 50,
                UnlockItemId = "badge_supreme_leader",
                IsSteamAchievement = true,
                SteamAchievementId = "ACH_SECRET_SUPREME"
            });
            
            AddAchievement(new Achievement
            {
                AchievementId = "secret_shadow_government",
                DisplayName = "???",
                Description = "???",
                HiddenDescription = "Establish the Shadow Government",
                Category = AchievementCategory.Secret,
                Rarity = ItemRarity.Legendary,
                IsSecret = true,
                CloutBuxReward = 1000,
                PurrkoinReward = 50,
                IsSteamAchievement = true,
                SteamAchievementId = "ACH_SECRET_SHADOW"
            });
            
            // Meta Achievements
            AddAchievement(new Achievement
            {
                AchievementId = "all_backgrounds",
                DisplayName = "Renaissance Politician",
                Description = "Win an election with each background",
                Category = AchievementCategory.Meta,
                Rarity = ItemRarity.Epic,
                RequiredCount = 8,
                CloutBuxReward = 800,
                LegacyPointReward = 200
            });
            
            AddAchievement(new Achievement
            {
                AchievementId = "all_alignments",
                DisplayName = "Political Spectrum",
                Description = "Reach Tier 5 with each alignment",
                Category = AchievementCategory.Meta,
                Rarity = ItemRarity.Legendary,
                RequiredCount = 9,
                CloutBuxReward = 2000,
                PurrkoinReward = 100,
                LegacyPointReward = 500,
                IsSteamAchievement = true,
                SteamAchievementId = "ACH_ALL_ALIGNMENTS"
            });
        }
        
        private void AddAchievement(Achievement ach)
        {
            achievements[ach.AchievementId] = ach;
        }
        
        #endregion
        
        #region Progress Tracking
        
        /// <summary>
        /// Update a statistic.
        /// </summary>
        public void UpdateStat(string statName, float value, bool increment = true)
        {
            if (increment)
            {
                sessionStats[statName] = sessionStats.GetValueOrDefault(statName) + value;
                lifetimeStats[statName] = lifetimeStats.GetValueOrDefault(statName) + value;
            }
            else
            {
                sessionStats[statName] = value;
                if (value > lifetimeStats.GetValueOrDefault(statName))
                {
                    lifetimeStats[statName] = value;
                }
            }
            
            CheckAchievements(statName);
        }
        
        /// <summary>
        /// Trigger an achievement check for a specific event.
        /// </summary>
        public void TriggerEvent(string eventName, Dictionary<string, object> context = null)
        {
            foreach (var ach in achievements.Values)
            {
                if (ach.IsCompleted) continue;
                if (ach.TriggerCondition == eventName)
                {
                    if (CheckCondition(ach, context))
                    {
                        UnlockAchievement(ach.AchievementId);
                    }
                }
            }
        }
        
        private void CheckAchievements(string changedStat)
        {
            foreach (var ach in achievements.Values)
            {
                if (ach.IsCompleted) continue;
                
                if (ach.RequiredStats != null && ach.RequiredStats.ContainsKey(changedStat))
                {
                    if (CheckStatRequirements(ach))
                    {
                        if (ach.RequiredCount > 0)
                        {
                            UpdateProgress(ach.AchievementId, lifetimeStats.GetValueOrDefault(changedStat));
                        }
                        else
                        {
                            UnlockAchievement(ach.AchievementId);
                        }
                    }
                }
            }
        }
        
        private bool CheckStatRequirements(Achievement ach)
        {
            if (ach.RequiredStats == null) return true;
            
            foreach (var req in ach.RequiredStats)
            {
                float current = lifetimeStats.GetValueOrDefault(req.Key);
                if (current < req.Value) return false;
            }
            
            return true;
        }
        
        private bool CheckCondition(Achievement ach, Dictionary<string, object> context)
        {
            // Simplified condition checking
            // In production, this would parse and evaluate condition expressions
            return true;
        }
        
        private void UpdateProgress(string achievementId, float value)
        {
            if (!achievements.TryGetValue(achievementId, out var ach)) return;
            
            if (!playerProgress.TryGetValue(achievementId, out var progress))
            {
                progress = new AchievementProgress { AchievementId = achievementId };
                playerProgress[achievementId] = progress;
            }
            
            progress.CurrentCount = (int)value;
            progress.ProgressValue = Math.Min(1f, value / ach.RequiredCount);
            
            OnAchievementProgress?.Invoke(ach, progress.ProgressValue);
            
            if (progress.CurrentCount >= ach.RequiredCount)
            {
                UnlockAchievement(achievementId);
            }
        }
        
        #endregion
        
        #region Achievement Unlocking
        
        /// <summary>
        /// Unlock an achievement.
        /// </summary>
        public void UnlockAchievement(string achievementId)
        {
            if (!achievements.TryGetValue(achievementId, out var ach)) return;
            if (ach.IsCompleted) return;
            
            ach.IsCompleted = true;
            ach.CompletedDate = DateTime.UtcNow;
            
            // Update progress
            if (!playerProgress.TryGetValue(achievementId, out var progress))
            {
                progress = new AchievementProgress { AchievementId = achievementId };
                playerProgress[achievementId] = progress;
            }
            progress.IsCompleted = true;
            progress.CompletedDate = DateTime.UtcNow;
            progress.ProgressValue = 1f;
            
            // Award rewards
            if (ach.CloutBuxReward > 0)
            {
                currencyManager.Credit(CurrencyType.CloutBux, ach.CloutBuxReward,
                    $"Achievement: {ach.DisplayName}", "achievement");
            }
            
            if (ach.PurrkoinReward > 0)
            {
                currencyManager.Credit(CurrencyType.Purrkoin, ach.PurrkoinReward,
                    $"Achievement: {ach.DisplayName}", "achievement");
            }
            
            if (!string.IsNullOrEmpty(ach.UnlockItemId))
            {
                inventory.OwnedItemIds.Add(ach.UnlockItemId);
            }
            
            // Report to Steam
            if (ach.IsSteamAchievement)
            {
                ReportToSteam(ach.SteamAchievementId);
            }
            
            OnAchievementUnlocked?.Invoke(ach);
        }
        
        private void ReportToSteam(string steamAchievementId)
        {
            // Steam integration handled in SteamManager
            Debug.Log($"Steam achievement unlocked: {steamAchievementId}");
        }
        
        #endregion
        
        #region Query Methods
        
        /// <summary>
        /// Get achievements by category.
        /// </summary>
        public List<Achievement> GetAchievements(AchievementCategory? category = null, bool includeSecret = false)
        {
            return achievements.Values
                .Where(a => (!category.HasValue || a.Category == category.Value) &&
                           (includeSecret || !a.IsSecret || a.IsCompleted))
                .OrderBy(a => a.IsCompleted ? 1 : 0)
                .ThenBy(a => a.Rarity)
                .ToList();
        }
        
        /// <summary>
        /// Get completion percentage.
        /// </summary>
        public float GetCompletionPercent(AchievementCategory? category = null)
        {
            var filtered = achievements.Values
                .Where(a => !category.HasValue || a.Category == category.Value);
            
            int total = filtered.Count();
            int completed = filtered.Count(a => a.IsCompleted);
            
            return total > 0 ? (float)completed / total : 0f;
        }
        
        /// <summary>
        /// Get progress for an achievement.
        /// </summary>
        public AchievementProgress GetProgress(string achievementId)
        {
            return playerProgress.TryGetValue(achievementId, out var progress) ? progress : null;
        }
        
        #endregion
    }
    
    #endregion
    
    #region Audio System
    
    /// <summary>
    /// Categories of audio.
    /// </summary>
    public enum AudioCategory
    {
        Music,
        SFX,
        UI,
        Ambience,
        Voice
    }
    
    /// <summary>
    /// Music tracks.
    /// </summary>
    public enum MusicTrack
    {
        MainMenu,
        Campaign_Upbeat,
        Campaign_Tense,
        Governance,
        Crisis,
        Election_Night,
        Victory,
        Defeat,
        Scandal,
        Chaos_Mode,
        Credits
    }
    
    /// <summary>
    /// Sound effects.
    /// </summary>
    public enum SFXType
    {
        // UI
        UI_Click,
        UI_Hover,
        UI_Open,
        UI_Close,
        UI_Error,
        UI_Success,
        UI_Notification,
        
        // Game events
        Event_Poll_Up,
        Event_Poll_Down,
        Event_Scandal,
        Event_Crisis,
        Event_Victory,
        Event_Defeat,
        Event_Money,
        Event_Applause,
        Event_Boo,
        
        // Actions
        Action_Speech,
        Action_Debate,
        Action_Vote,
        Action_Sign,
        Action_Phone,
        Action_Meeting,
        
        // Chaos
        Chaos_Alarm,
        Chaos_Explosion,
        Chaos_Crowd_Gasp,
        Chaos_Record_Scratch,
        Chaos_Dramatic_Sting,
        
        // Achievement
        Achievement_Unlock,
        Achievement_Rare,
        Achievement_Legendary,
        
        // Currency
        Currency_Earn,
        Currency_Spend
    }
    
    /// <summary>
    /// Audio clip definition.
    /// </summary>
    [Serializable]
    public class AudioClipData
    {
        public string ClipId;
        public AudioCategory Category;
        public string AssetPath;
        public float DefaultVolume = 1f;
        public float PitchVariation = 0f;
        public bool Loop = false;
        public float FadeInDuration = 0f;
        public float FadeOutDuration = 0f;
    }
    
    /// <summary>
    /// Manages all game audio.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Mixers")]
        [SerializeField] private AudioMixer masterMixer;
        [SerializeField] private AudioMixerGroup musicGroup;
        [SerializeField] private AudioMixerGroup sfxGroup;
        [SerializeField] private AudioMixerGroup uiGroup;
        [SerializeField] private AudioMixerGroup ambienceGroup;
        [SerializeField] private AudioMixerGroup voiceGroup;
        
        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource ambienceSource;
        [SerializeField] private AudioSource[] sfxSources;
        
        // Settings
        private float masterVolume = 1f;
        private float musicVolume = 0.8f;
        private float sfxVolume = 1f;
        private float uiVolume = 1f;
        private bool isMuted = false;
        
        // State
        private MusicTrack currentTrack;
        private bool isFading;
        private Dictionary<string, AudioClip> loadedClips;
        private int currentSfxSourceIndex;
        
        // Events
        public event Action<MusicTrack> OnMusicChanged;
        
        private void Awake()
        {
            loadedClips = new Dictionary<string, AudioClip>();
            InitializeSfxSources();
        }
        
        private void InitializeSfxSources()
        {
            if (sfxSources == null || sfxSources.Length == 0)
            {
                sfxSources = new AudioSource[8];
                for (int i = 0; i < sfxSources.Length; i++)
                {
                    var go = new GameObject($"SFX_Source_{i}");
                    go.transform.SetParent(transform);
                    sfxSources[i] = go.AddComponent<AudioSource>();
                    sfxSources[i].playOnAwake = false;
                    if (sfxGroup != null)
                    {
                        sfxSources[i].outputAudioMixerGroup = sfxGroup;
                    }
                }
            }
        }
        
        #region Volume Control
        
        /// <summary>
        /// Set master volume.
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            if (masterMixer != null)
            {
                masterMixer.SetFloat("MasterVolume", LinearToDecibel(masterVolume));
            }
        }
        
        /// <summary>
        /// Set music volume.
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (masterMixer != null)
            {
                masterMixer.SetFloat("MusicVolume", LinearToDecibel(musicVolume));
            }
        }
        
        /// <summary>
        /// Set SFX volume.
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            if (masterMixer != null)
            {
                masterMixer.SetFloat("SFXVolume", LinearToDecibel(sfxVolume));
            }
        }
        
        /// <summary>
        /// Toggle mute.
        /// </summary>
        public void ToggleMute()
        {
            isMuted = !isMuted;
            SetMasterVolume(isMuted ? 0f : masterVolume);
        }
        
        private float LinearToDecibel(float linear)
        {
            return linear > 0.0001f ? 20f * Mathf.Log10(linear) : -80f;
        }
        
        #endregion
        
        #region Music
        
        /// <summary>
        /// Play a music track.
        /// </summary>
        public void PlayMusic(MusicTrack track, float fadeInTime = 1f)
        {
            if (track == currentTrack && musicSource != null && musicSource.isPlaying) return;
            
            StartCoroutine(TransitionMusic(track, fadeInTime));
        }
        
        /// <summary>
        /// Stop music.
        /// </summary>
        public void StopMusic(float fadeOutTime = 1f)
        {
            StartCoroutine(FadeOutMusic(fadeOutTime));
        }
        
        private System.Collections.IEnumerator TransitionMusic(MusicTrack track, float fadeTime)
        {
            // Fade out current
            if (musicSource != null && musicSource.isPlaying)
            {
                yield return FadeOutMusic(fadeTime * 0.5f);
            }
            
            // Load and play new track
            var clip = LoadMusicClip(track);
            if (clip != null && musicSource != null)
            {
                musicSource.clip = clip;
                musicSource.Play();
                
                // Fade in
                float elapsed = 0;
                while (elapsed < fadeTime * 0.5f)
                {
                    elapsed += Time.deltaTime;
                    musicSource.volume = Mathf.Lerp(0, musicVolume, elapsed / (fadeTime * 0.5f));
                    yield return null;
                }
                
                musicSource.volume = musicVolume;
            }
            
            currentTrack = track;
            OnMusicChanged?.Invoke(track);
        }
        
        private System.Collections.IEnumerator FadeOutMusic(float fadeTime)
        {
            if (musicSource == null) yield break;
            
            float startVolume = musicSource.volume;
            float elapsed = 0;
            
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0, elapsed / fadeTime);
                yield return null;
            }
            
            musicSource.Stop();
            musicSource.volume = startVolume;
        }
        
        private AudioClip LoadMusicClip(MusicTrack track)
        {
            string path = GetMusicPath(track);
            
            if (loadedClips.TryGetValue(path, out var clip))
            {
                return clip;
            }
            
            clip = Resources.Load<AudioClip>(path);
            if (clip != null)
            {
                loadedClips[path] = clip;
            }
            
            return clip;
        }
        
        private string GetMusicPath(MusicTrack track)
        {
            return $"Audio/Music/{track}";
        }
        
        #endregion
        
        #region Sound Effects
        
        /// <summary>
        /// Play a sound effect.
        /// </summary>
        public void PlaySFX(SFXType sfx, float volumeScale = 1f)
        {
            var clip = LoadSFXClip(sfx);
            if (clip != null)
            {
                var source = GetAvailableSfxSource();
                if (source != null)
                {
                    source.clip = clip;
                    source.volume = sfxVolume * volumeScale;
                    source.pitch = 1f + UnityEngine.Random.Range(-0.05f, 0.05f);
                    source.Play();
                }
            }
        }
        
        /// <summary>
        /// Play a UI sound.
        /// </summary>
        public void PlayUI(SFXType sfx)
        {
            var clip = LoadSFXClip(sfx);
            if (clip != null)
            {
                var source = GetAvailableSfxSource();
                if (source != null)
                {
                    if (uiGroup != null)
                    {
                        source.outputAudioMixerGroup = uiGroup;
                    }
                    source.clip = clip;
                    source.volume = uiVolume;
                    source.Play();
                    if (sfxGroup != null)
                    {
                        source.outputAudioMixerGroup = sfxGroup; // Reset
                    }
                }
            }
        }
        
        private AudioSource GetAvailableSfxSource()
        {
            if (sfxSources == null || sfxSources.Length == 0) return null;
            
            // Find non-playing source
            foreach (var source in sfxSources)
            {
                if (source != null && !source.isPlaying)
                {
                    return source;
                }
            }
            
            // Round robin if all playing
            currentSfxSourceIndex = (currentSfxSourceIndex + 1) % sfxSources.Length;
            return sfxSources[currentSfxSourceIndex];
        }
        
        private AudioClip LoadSFXClip(SFXType sfx)
        {
            string path = $"Audio/SFX/{sfx}";
            
            if (loadedClips.TryGetValue(path, out var clip))
            {
                return clip;
            }
            
            clip = Resources.Load<AudioClip>(path);
            if (clip != null)
            {
                loadedClips[path] = clip;
            }
            
            return clip;
        }
        
        #endregion
        
        #region Ambience
        
        /// <summary>
        /// Play ambient sound.
        /// </summary>
        public void PlayAmbience(string ambienceId, float fadeIn = 2f)
        {
            var clip = Resources.Load<AudioClip>($"Audio/Ambience/{ambienceId}");
            if (clip != null && ambienceSource != null)
            {
                StartCoroutine(FadeInAmbience(clip, fadeIn));
            }
        }
        
        private System.Collections.IEnumerator FadeInAmbience(AudioClip clip, float fadeTime)
        {
            if (ambienceSource == null) yield break;
            
            if (ambienceSource.isPlaying)
            {
                float elapsed = 0;
                float startVol = ambienceSource.volume;
                while (elapsed < fadeTime * 0.5f)
                {
                    elapsed += Time.deltaTime;
                    ambienceSource.volume = Mathf.Lerp(startVol, 0, elapsed / (fadeTime * 0.5f));
                    yield return null;
                }
                ambienceSource.Stop();
            }
            
            ambienceSource.clip = clip;
            ambienceSource.loop = true;
            ambienceSource.Play();
            
            float elapsed2 = 0;
            while (elapsed2 < fadeTime * 0.5f)
            {
                elapsed2 += Time.deltaTime;
                ambienceSource.volume = Mathf.Lerp(0, 0.5f, elapsed2 / (fadeTime * 0.5f));
                yield return null;
            }
        }
        
        #endregion
        
        #region Convenience Methods
        
        /// <summary>
        /// Play appropriate music for game state.
        /// </summary>
        public void SetGameState(string state)
        {
            var track = state.ToLower() switch
            {
                "menu" => MusicTrack.MainMenu,
                "campaign" => MusicTrack.Campaign_Upbeat,
                "campaign_tense" => MusicTrack.Campaign_Tense,
                "governance" => MusicTrack.Governance,
                "crisis" => MusicTrack.Crisis,
                "election" => MusicTrack.Election_Night,
                "victory" => MusicTrack.Victory,
                "defeat" => MusicTrack.Defeat,
                "scandal" => MusicTrack.Scandal,
                "chaos" => MusicTrack.Chaos_Mode,
                _ => currentTrack
            };
            
            PlayMusic(track);
        }
        
        /// <summary>
        /// Preload commonly used clips.
        /// </summary>
        public void PreloadCommonClips()
        {
            // Preload UI sounds
            LoadSFXClip(SFXType.UI_Click);
            LoadSFXClip(SFXType.UI_Hover);
            LoadSFXClip(SFXType.UI_Notification);
            LoadSFXClip(SFXType.Achievement_Unlock);
        }
        
        #endregion
    }
    
    #endregion
}

