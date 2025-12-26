// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - STORE UI CONTROLLER
// Unity MonoBehaviour for displaying store, Purrkoin purchases, and inventory
// ═══════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ElectionEmpire.Economy
{
    /// <summary>
    /// UI Controller for the in-game store and Purrkoin purchases
    /// </summary>
    public class StoreUIController : MonoBehaviour
    {
        #region UI References
        
        [Header("Main Panels")]
        [SerializeField] private GameObject storePanel;
        [SerializeField] private GameObject iapPanel;
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private GameObject confirmationDialog;
        [SerializeField] private GameObject purchaseSuccessPopup;
        [SerializeField] private GameObject purchaseFailedPopup;
        
        [Header("Balance Display")]
        [SerializeField] private TextMeshProUGUI cloutBuxBalanceText;
        [SerializeField] private TextMeshProUGUI purrkoinBalanceText;
        [SerializeField] private Button addPurrkoinButton;
        
        [Header("Store Grid")]
        [SerializeField] private Transform storeItemContainer;
        [SerializeField] private GameObject storeItemPrefab;
        [SerializeField] private TMP_Dropdown categoryFilter;
        [SerializeField] private TMP_Dropdown sortDropdown;
        
        [Header("IAP Panel")]
        [SerializeField] private Transform iapPackageContainer;
        [SerializeField] private GameObject iapPackagePrefab;
        [SerializeField] private TextMeshProUGUI iapHeaderText;
        [SerializeField] private Toggle spendingLimitToggle;
        [SerializeField] private TMP_InputField spendingLimitInput;
        
        [Header("Item Detail")]
        [SerializeField] private GameObject itemDetailPanel;
        [SerializeField] private Image itemDetailImage;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemDescriptionText;
        [SerializeField] private TextMeshProUGUI itemPriceText;
        [SerializeField] private TextMeshProUGUI itemRarityText;
        [SerializeField] private Button buyButton;
        [SerializeField] private Button previewButton;
        
        [Header("Confirmation Dialog")]
        [SerializeField] private TextMeshProUGUI confirmationText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        
        [Header("Inventory")]
        [SerializeField] private Transform inventoryItemContainer;
        [SerializeField] private GameObject inventoryItemPrefab;
        [SerializeField] private TMP_Dropdown inventoryFilter;
        
        [Header("Colors")]
        [SerializeField] private Color commonColor = Color.white;
        [SerializeField] private Color uncommonColor = Color.green;
        [SerializeField] private Color rareColor = Color.blue;
        [SerializeField] private Color epicColor = new Color(0.5f, 0, 0.5f);
        [SerializeField] private Color legendaryColor = new Color(1f, 0.5f, 0);
        [SerializeField] private Color mythicColor = new Color(1f, 0, 0);
        
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip purchaseSuccessClip;
        [SerializeField] private AudioClip purchaseFailClip;
        [SerializeField] private AudioClip buttonClickClip;
        
        #endregion
        
        #region State
        
        private string _playerId;
        private StoreItem _selectedItem;
        private PurrkoinPackage _selectedPackage;
        private List<GameObject> _storeItemObjects = new List<GameObject>();
        private List<GameObject> _iapPackageObjects = new List<GameObject>();
        private List<GameObject> _inventoryItemObjects = new List<GameObject>();
        
        #endregion
        
        #region Events
        
        public event Action<StoreItem> OnItemPurchased;
        public event Action<PurrkoinPackage> OnIAPPurchased;
        public event Action<OwnedItem> OnItemEquipped;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Start()
        {
            SetupUI();
            SubscribeToEvents();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize with player ID
        /// </summary>
        public void Initialize(string playerId)
        {
            _playerId = playerId;
            RefreshBalanceDisplay();
            RefreshStoreItems();
            RefreshIAPPackages();
        }
        
        private void SetupUI()
        {
            // Setup category filter
            if (categoryFilter != null)
            {
                categoryFilter.ClearOptions();
                categoryFilter.AddOptions(new List<string>
                {
                    "All Items",
                    "Cosmetics",
                    "Backgrounds",
                    "Content Packs",
                    "Tournament",
                    "Bundles"
                });
                categoryFilter.onValueChanged.AddListener(OnCategoryFilterChanged);
            }
            
            // Setup sort dropdown
            if (sortDropdown != null)
            {
                sortDropdown.ClearOptions();
                sortDropdown.AddOptions(new List<string>
                {
                    "Newest",
                    "Price: Low to High",
                    "Price: High to Low",
                    "Rarity"
                });
                sortDropdown.onValueChanged.AddListener(OnSortChanged);
            }
            
            // Setup buttons
            if (addPurrkoinButton != null)
            {
                addPurrkoinButton.onClick.AddListener(ShowIAPPanel);
            }
            
            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(OnConfirmPurchase);
            }
            
            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(HideConfirmationDialog);
            }
            
            if (buyButton != null)
            {
                buyButton.onClick.AddListener(OnBuyButtonClicked);
            }
            
            // Setup spending limit
            if (spendingLimitToggle != null)
            {
                spendingLimitToggle.onValueChanged.AddListener(OnSpendingLimitToggled);
            }
        }
        
        private void SubscribeToEvents()
        {
            EconomyManager.Instance.OnCurrencyChanged += HandleCurrencyChanged;
            EconomyManager.Instance.OnPurchaseCompleted += HandlePurchaseCompleted;
            EconomyManager.Instance.OnItemUnlocked += HandleItemUnlocked;
        }
        
        private void UnsubscribeFromEvents()
        {
            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.OnCurrencyChanged -= HandleCurrencyChanged;
                EconomyManager.Instance.OnPurchaseCompleted -= HandlePurchaseCompleted;
                EconomyManager.Instance.OnItemUnlocked -= HandleItemUnlocked;
            }
        }
        
        #endregion
        
        #region Balance Display
        
        private void RefreshBalanceDisplay()
        {
            if (string.IsNullOrEmpty(_playerId)) return;
            
            long cloutBux = EconomyManager.Instance.GetBalance(_playerId, CurrencyType.CloutBux);
            long purrkoin = EconomyManager.Instance.GetBalance(_playerId, CurrencyType.Purrkoin);
            
            if (cloutBuxBalanceText != null)
            {
                cloutBuxBalanceText.text = FormatCurrency(cloutBux);
            }
            
            if (purrkoinBalanceText != null)
            {
                purrkoinBalanceText.text = FormatCurrency(purrkoin);
            }
        }
        
        private string FormatCurrency(long amount)
        {
            if (amount >= 1000000)
            {
                return $"{amount / 1000000f:F1}M";
            }
            if (amount >= 1000)
            {
                return $"{amount / 1000f:F1}K";
            }
            return amount.ToString("N0");
        }
        
        #endregion
        
        #region Store Display
        
        public void ShowStore()
        {
            HideAllPanels();
            storePanel?.SetActive(true);
            RefreshStoreItems();
        }
        
        private void RefreshStoreItems()
        {
            ClearStoreItems();
            
            var items = EconomyManager.Instance.GetStoreItems();
            
            // Apply filters
            int categoryIndex = categoryFilter?.value ?? 0;
            if (categoryIndex > 0)
            {
                PurchaseCategory? filter = categoryIndex switch
                {
                    1 => PurchaseCategory.Cosmetic,
                    2 => PurchaseCategory.Background,
                    3 => PurchaseCategory.ContentPack,
                    4 => PurchaseCategory.TournamentEntry,
                    5 => PurchaseCategory.Bundle,
                    _ => null
                };
                
                if (filter.HasValue)
                {
                    items = items.Where(i => i.Category == filter.Value).ToList();
                }
            }
            
            // Apply sorting
            int sortIndex = sortDropdown?.value ?? 0;
            items = sortIndex switch
            {
                1 => items.OrderBy(i => i.PrimaryPrice).ToList(),
                2 => items.OrderByDescending(i => i.PrimaryPrice).ToList(),
                3 => items.OrderByDescending(i => i.Rarity).ToList(),
                _ => items
            };
            
            // Create item displays
            foreach (var item in items)
            {
                CreateStoreItemDisplay(item);
            }
        }
        
        private void CreateStoreItemDisplay(StoreItem item)
        {
            if (storeItemPrefab == null || storeItemContainer == null) return;
            
            var obj = Instantiate(storeItemPrefab, storeItemContainer);
            _storeItemObjects.Add(obj);
            
            // Configure display
            var nameText = obj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            var priceText = obj.transform.Find("PriceText")?.GetComponent<TextMeshProUGUI>();
            var rarityBorder = obj.transform.Find("RarityBorder")?.GetComponent<Image>();
            var iconImage = obj.transform.Find("Icon")?.GetComponent<Image>();
            var button = obj.GetComponent<Button>();
            
            if (nameText != null)
            {
                nameText.text = item.Name;
            }
            
            if (priceText != null)
            {
                string currencyIcon = item.PrimaryCurrency == CurrencyType.Purrkoin ? "💎" : "🏆";
                priceText.text = $"{currencyIcon} {item.PrimaryPrice}";
            }
            
            if (rarityBorder != null)
            {
                rarityBorder.color = GetRarityColor(item.Rarity);
            }
            
            if (button != null)
            {
                button.onClick.AddListener(() => ShowItemDetail(item));
            }
        }
        
        private void ClearStoreItems()
        {
            foreach (var obj in _storeItemObjects)
            {
                Destroy(obj);
            }
            _storeItemObjects.Clear();
        }
        
        #endregion
        
        #region Item Detail
        
        private void ShowItemDetail(StoreItem item)
        {
            _selectedItem = item;
            
            itemDetailPanel?.SetActive(true);
            
            if (itemNameText != null)
            {
                itemNameText.text = item.Name;
            }
            
            if (itemDescriptionText != null)
            {
                itemDescriptionText.text = item.Description;
            }
            
            if (itemPriceText != null)
            {
                string currencyIcon = item.PrimaryCurrency == CurrencyType.Purrkoin ? "💎" : "🏆";
                itemPriceText.text = $"{currencyIcon} {item.PrimaryPrice}";
                
                // Check if can afford
                var wallet = EconomyManager.Instance.GetWallet(_playerId);
                bool canAfford = wallet.CanAfford(item.PrimaryCurrency, item.PrimaryPrice);
                itemPriceText.color = canAfford ? Color.white : Color.red;
            }
            
            if (itemRarityText != null)
            {
                itemRarityText.text = item.Rarity.ToString();
                itemRarityText.color = GetRarityColor(item.Rarity);
            }
            
            // Check if already owned
            var inventory = EconomyManager.Instance.GetInventory(_playerId);
            bool owned = inventory.OwnsItem(item.Id);
            
            if (buyButton != null)
            {
                buyButton.interactable = !owned;
                var buyButtonText = buyButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buyButtonText != null)
                {
                    buyButtonText.text = owned ? "OWNED" : "BUY";
                }
            }
            
            PlaySound(buttonClickClip);
        }
        
        private void OnBuyButtonClicked()
        {
            if (_selectedItem == null) return;
            
            ShowConfirmationDialog(_selectedItem);
        }
        
        #endregion
        
        #region IAP Panel
        
        public void ShowIAPPanel()
        {
            HideAllPanels();
            iapPanel?.SetActive(true);
            RefreshIAPPackages();
        }
        
        private void RefreshIAPPackages()
        {
            ClearIAPPackages();
            
            var packages = EconomyManager.Instance.GetPurrkoinPackages();
            
            foreach (var package in packages.OrderBy(p => p.PriceUSD))
            {
                CreateIAPPackageDisplay(package);
            }
        }
        
        private void CreateIAPPackageDisplay(PurrkoinPackage package)
        {
            if (iapPackagePrefab == null || iapPackageContainer == null) return;
            
            var obj = Instantiate(iapPackagePrefab, iapPackageContainer);
            _iapPackageObjects.Add(obj);
            
            // Configure display
            var nameText = obj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            var amountText = obj.transform.Find("AmountText")?.GetComponent<TextMeshProUGUI>();
            var priceText = obj.transform.Find("PriceText")?.GetComponent<TextMeshProUGUI>();
            var bonusText = obj.transform.Find("BonusText")?.GetComponent<TextMeshProUGUI>();
            var bestValueBadge = obj.transform.Find("BestValueBadge");
            var popularBadge = obj.transform.Find("PopularBadge");
            var button = obj.GetComponent<Button>();
            
            if (nameText != null)
            {
                nameText.text = package.Name;
            }
            
            if (amountText != null)
            {
                amountText.text = $"💎 {package.TotalPurrkoin}";
            }
            
            if (priceText != null)
            {
                priceText.text = $"${package.PriceUSD:F2}";
            }
            
            if (bonusText != null)
            {
                if (package.BonusPurrkoin > 0)
                {
                    bonusText.text = $"+{package.BonusPurrkoin} BONUS!";
                    bonusText.gameObject.SetActive(true);
                }
                else
                {
                    bonusText.gameObject.SetActive(false);
                }
            }
            
            if (bestValueBadge != null)
            {
                bestValueBadge.gameObject.SetActive(package.IsBestValue);
            }
            
            if (popularBadge != null)
            {
                popularBadge.gameObject.SetActive(package.IsPopular);
            }
            
            if (button != null)
            {
                button.onClick.AddListener(() => OnIAPPackageSelected(package));
            }
        }
        
        private void ClearIAPPackages()
        {
            foreach (var obj in _iapPackageObjects)
            {
                Destroy(obj);
            }
            _iapPackageObjects.Clear();
        }
        
        private void OnIAPPackageSelected(PurrkoinPackage package)
        {
            _selectedPackage = package;
            
            // Show confirmation
            ShowIAPConfirmation(package);
        }
        
        private void ShowIAPConfirmation(PurrkoinPackage package)
        {
            if (confirmationDialog == null) return;
            
            confirmationDialog.SetActive(true);
            
            if (confirmationText != null)
            {
                confirmationText.text = $"Purchase {package.Name}?\n\n" +
                    $"💎 {package.TotalPurrkoin} Purrkoin\n" +
                    $"Price: ${package.PriceUSD:F2}";
            }
            
            PlaySound(buttonClickClip);
        }
        
        #endregion
        
        #region Confirmation Dialog
        
        private void ShowConfirmationDialog(StoreItem item)
        {
            if (confirmationDialog == null) return;
            
            _selectedPackage = null; // Clear IAP selection
            confirmationDialog.SetActive(true);
            
            string currencyIcon = item.PrimaryCurrency == CurrencyType.Purrkoin ? "💎" : "🏆";
            
            if (confirmationText != null)
            {
                confirmationText.text = $"Purchase {item.Name}?\n\n" +
                    $"Price: {currencyIcon} {item.PrimaryPrice}";
            }
            
            PlaySound(buttonClickClip);
        }
        
        private void HideConfirmationDialog()
        {
            confirmationDialog?.SetActive(false);
            PlaySound(buttonClickClip);
        }
        
        private void OnConfirmPurchase()
        {
            HideConfirmationDialog();
            
            if (_selectedPackage != null)
            {
                ProcessIAPPurchase(_selectedPackage);
            }
            else if (_selectedItem != null)
            {
                ProcessStorePurchase(_selectedItem);
            }
        }
        
        #endregion
        
        #region Purchase Processing
        
        private void ProcessStorePurchase(StoreItem item)
        {
            var result = EconomyManager.Instance.PurchaseItem(_playerId, item.Id);
            
            if (result.Success)
            {
                ShowPurchaseSuccess($"You purchased {item.Name}!");
                OnItemPurchased?.Invoke(item);
            }
            else
            {
                ShowPurchaseFailed(result.Error);
            }
        }
        
        private void ProcessIAPPurchase(PurrkoinPackage package)
        {
            // In a real implementation, this would go through the platform's IAP system
            // For now, we'll simulate success
            
            // Generate mock receipt
            string mockReceipt = $"MOCK_RECEIPT_{package.Id}_{DateTime.UtcNow.Ticks}";
            string mockTransactionId = Guid.NewGuid().ToString();
            
            var result = EconomyManager.Instance.ProcessIAPPurchase(
                _playerId,
                package.Id,
                mockTransactionId,
                "unity_editor", // Platform would be "steam", "ios", "android" in production
                mockReceipt
            );
            
            if (result.Success)
            {
                ShowPurchaseSuccess($"You received 💎 {result.PurrkoinGranted} Purrkoin!");
                OnIAPPurchased?.Invoke(package);
            }
            else
            {
                ShowPurchaseFailed(result.Error);
            }
        }
        
        private void ShowPurchaseSuccess(string message)
        {
            if (purchaseSuccessPopup != null)
            {
                purchaseSuccessPopup.SetActive(true);
                var text = purchaseSuccessPopup.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = message;
                }
                
                // Auto-hide after delay
                Invoke(nameof(HidePurchaseSuccess), 2f);
            }
            
            PlaySound(purchaseSuccessClip);
            RefreshBalanceDisplay();
            RefreshStoreItems();
        }
        
        private void HidePurchaseSuccess()
        {
            purchaseSuccessPopup?.SetActive(false);
        }
        
        private void ShowPurchaseFailed(string error)
        {
            if (purchaseFailedPopup != null)
            {
                purchaseFailedPopup.SetActive(true);
                var text = purchaseFailedPopup.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = $"Purchase failed: {error}";
                }
                
                Invoke(nameof(HidePurchaseFailed), 3f);
            }
            
            PlaySound(purchaseFailClip);
        }
        
        private void HidePurchaseFailed()
        {
            purchaseFailedPopup?.SetActive(false);
        }
        
        #endregion
        
        #region Inventory Display
        
        public void ShowInventory()
        {
            HideAllPanels();
            inventoryPanel?.SetActive(true);
            RefreshInventory();
        }
        
        private void RefreshInventory()
        {
            ClearInventoryItems();
            
            var inventory = EconomyManager.Instance.GetInventory(_playerId);
            
            foreach (var item in inventory.Items.OrderByDescending(i => i.AcquiredAt))
            {
                CreateInventoryItemDisplay(item);
            }
        }
        
        private void CreateInventoryItemDisplay(OwnedItem item)
        {
            if (inventoryItemPrefab == null || inventoryItemContainer == null) return;
            
            var obj = Instantiate(inventoryItemPrefab, inventoryItemContainer);
            _inventoryItemObjects.Add(obj);
            
            var nameText = obj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            var equippedBadge = obj.transform.Find("EquippedBadge");
            var rarityBorder = obj.transform.Find("RarityBorder")?.GetComponent<Image>();
            var equipButton = obj.transform.Find("EquipButton")?.GetComponent<Button>();
            
            if (nameText != null)
            {
                nameText.text = item.ItemName;
            }
            
            if (equippedBadge != null)
            {
                equippedBadge.gameObject.SetActive(item.IsEquipped);
            }
            
            if (rarityBorder != null)
            {
                rarityBorder.color = GetRarityColor(item.Rarity);
            }
            
            if (equipButton != null && item.Type.HasValue)
            {
                equipButton.onClick.AddListener(() => EquipItem(item));
            }
        }
        
        private void ClearInventoryItems()
        {
            foreach (var obj in _inventoryItemObjects)
            {
                Destroy(obj);
            }
            _inventoryItemObjects.Clear();
        }
        
        private void EquipItem(OwnedItem item)
        {
            var inventory = EconomyManager.Instance.GetInventory(_playerId);
            
            // Unequip current item of same type
            foreach (var owned in inventory.Items)
            {
                if (owned.Type == item.Type)
                {
                    owned.IsEquipped = false;
                }
            }
            
            // Equip new item
            item.IsEquipped = true;
            
            // Update equipped reference
            if (item.Type.HasValue)
            {
                switch (item.Type.Value)
                {
                    case CosmeticType.Portrait:
                        inventory.EquippedPortrait = item.ItemId;
                        break;
                    case CosmeticType.Outfit:
                        inventory.EquippedOutfit = item.ItemId;
                        break;
                    case CosmeticType.Accessory:
                        inventory.EquippedAccessory = item.ItemId;
                        break;
                    case CosmeticType.ProfileFrame:
                        inventory.EquippedProfileFrame = item.ItemId;
                        break;
                    case CosmeticType.CardBack:
                        inventory.EquippedCardBack = item.ItemId;
                        break;
                    case CosmeticType.VictoryAnimation:
                        inventory.EquippedVictoryAnimation = item.ItemId;
                        break;
                }
            }
            
            RefreshInventory();
            OnItemEquipped?.Invoke(item);
            PlaySound(buttonClickClip);
        }
        
        #endregion
        
        #region Event Handlers
        
        private void HandleCurrencyChanged(CurrencyChangedEvent e)
        {
            if (e.PlayerId == _playerId)
            {
                RefreshBalanceDisplay();
            }
        }
        
        private void HandlePurchaseCompleted(PurchaseCompletedEvent e)
        {
            if (e.PlayerId == _playerId)
            {
                RefreshStoreItems();
                RefreshInventory();
            }
        }
        
        private void HandleItemUnlocked(ItemUnlockedEvent e)
        {
            if (e.PlayerId == _playerId)
            {
                RefreshInventory();
            }
        }
        
        private void OnCategoryFilterChanged(int index)
        {
            RefreshStoreItems();
            PlaySound(buttonClickClip);
        }
        
        private void OnSortChanged(int index)
        {
            RefreshStoreItems();
            PlaySound(buttonClickClip);
        }
        
        private void OnSpendingLimitToggled(bool enabled)
        {
            if (spendingLimitInput != null)
            {
                spendingLimitInput.interactable = enabled;
                
                if (enabled && decimal.TryParse(spendingLimitInput.text, out decimal limit))
                {
                    EconomyManager.Instance.SetSpendingLimit(_playerId, limit);
                }
            }
        }
        
        #endregion
        
        #region Helpers
        
        private void HideAllPanels()
        {
            storePanel?.SetActive(false);
            iapPanel?.SetActive(false);
            inventoryPanel?.SetActive(false);
            itemDetailPanel?.SetActive(false);
            confirmationDialog?.SetActive(false);
        }
        
        private Color GetRarityColor(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => commonColor,
                ItemRarity.Uncommon => uncommonColor,
                ItemRarity.Rare => rareColor,
                ItemRarity.Epic => epicColor,
                ItemRarity.Legendary => legendaryColor,
                ItemRarity.Mythic => mythicColor,
                ItemRarity.Unique => mythicColor,
                _ => commonColor
            };
        }
        
        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
        
        #endregion
    }
}
