using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using ElectionEmpire.World;
using ElectionEmpire.Gameplay;

namespace ElectionEmpire.UI
{
    /// <summary>
    /// Displays victory condition progress
    /// </summary>
    public class VictoryProgressUI : MonoBehaviour
    {
        [Header("Victory Condition")]
        public TextMeshProUGUI ConditionNameText;
        public TextMeshProUGUI ProgressText;
        public Slider ProgressBar;
        
        [Header("Tier Progress")]
        public Transform TierListContainer;
        public GameObject TierItemPrefab;
        
        private VictoryConditionManager victoryManager;
        private PlayerState player;
        
        public void DisplayProgress(VictoryConditionManager manager, PlayerState playerState)
        {
            victoryManager = manager;
            player = playerState;
            
            if (manager == null || playerState == null) return;
            
            // Condition name
            if (ConditionNameText != null)
                ConditionNameText.text = $"VICTORY CONDITION: {manager.SelectedCondition}";
            
            // Progress
            UpdateProgress();
            
            // Tier progress
            UpdateTierProgress();
        }
        
        private void UpdateProgress()
        {
            if (victoryManager == null || player == null) return;
            
            float progress = 0f;
            string progressText = "";
            
            switch (victoryManager.SelectedCondition)
            {
                case VictoryConditionManager.VictoryType.ReachPresident:
                    progress = player.HighestTierHeld / 5f;
                    progressText = $"Tier {player.HighestTierHeld} / 5";
                    break;
                
                case VictoryConditionManager.VictoryType.ApprovalThreshold:
                    float days = victoryManager.ConditionProgress.TryGetValue("DaysAbove70", out float daysValue) ? daysValue : 0f;
                    float requiredApproval = victoryManager.ConditionProgress.TryGetValue("Required", out float requiredApprovalValue) ? requiredApprovalValue : 30f;
                    progress = days / requiredApproval;
                    progressText = $"{days:F1} / {requiredApproval} days above 70%";
                    break;
                
                case VictoryConditionManager.VictoryType.TotalDomination:
                    float regions = victoryManager.ConditionProgress.TryGetValue("RegionsControlled", out float regionsValue) ? regionsValue : 0f;
                    float totalRegions = victoryManager.ConditionProgress.TryGetValue("Required", out float totalRegionsValue) ? totalRegionsValue : 10f;
                    progress = regions / totalRegions;
                    progressText = $"{regions} / {totalRegions} regions";
                    break;
                
                case VictoryConditionManager.VictoryType.ScandalSurvival:
                    float scandals = victoryManager.ConditionProgress.TryGetValue("ScandalsSurvived", out float scandalsValue) ? scandalsValue : 0f;
                    float requiredScandals = victoryManager.ConditionProgress.TryGetValue("Required", out float requiredScandalsValue) ? requiredScandalsValue : 10f;
                    progress = scandals / requiredScandals;
                    progressText = $"{scandals} / {requiredScandals} scandals survived";
                    break;
            }
            
            if (ProgressText != null)
                ProgressText.text = progressText;
            
            if (ProgressBar != null)
                ProgressBar.value = Mathf.Clamp01(progress);
        }
        
        private void UpdateTierProgress()
        {
            if (TierListContainer == null || player == null) return;
            
            // Clear existing
            foreach (Transform child in TierListContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Create tier items
            for (int tier = 1; tier <= 5; tier++)
            {
                CreateTierItem(tier);
            }
        }
        
        private void CreateTierItem(int tier)
        {
            GameObject item;
            
            if (TierItemPrefab != null)
            {
                item = Instantiate(TierItemPrefab, TierListContainer);
            }
            else
            {
                item = new GameObject($"Tier_{tier}");
                item.transform.SetParent(TierListContainer);
                
                var layout = item.AddComponent<HorizontalLayoutGroup>();
                layout.spacing = 10;
            }
            
            // Status icon
            GameObject statusObj = new GameObject("Status");
            statusObj.transform.SetParent(item.transform);
            TextMeshProUGUI statusText = statusObj.AddComponent<TextMeshProUGUI>();
            
            if (player.HighestTierHeld > tier)
                statusText.text = "✓";
            else if (player.HighestTierHeld == tier)
                statusText.text = "►";
            else
                statusText.text = "○";
            
            statusText.fontSize = 20;
            
            // Tier name
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(item.transform);
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            
            string tierName = GetTierName(tier);
            nameText.text = $"Tier {tier} {tierName}";
            nameText.fontSize = 14;
            
            // Status text
            if (player.HighestTierHeld == tier && player.CurrentOffice != null)
            {
                GameObject statusTextObj = new GameObject("StatusText");
                statusTextObj.transform.SetParent(item.transform);
                TextMeshProUGUI status = statusTextObj.AddComponent<TextMeshProUGUI>();
                status.text = $"[IN PROGRESS]";
                status.fontSize = 12;
                status.color = Color.yellow;
            }
        }
        
        private string GetTierName(int tier)
        {
            switch (tier)
            {
                case 1: return "Local";
                case 2: return "City/County";
                case 3: return "State";
                case 4: return "Regional";
                case 5: return "National";
                default: return "";
            }
        }
    }
}

