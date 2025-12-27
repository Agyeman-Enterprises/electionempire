using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ElectionEmpire.World;
using ElectionEmpire.Scandal;
using ElectionEmpire.News;

namespace ElectionEmpire.UI
{
    /// <summary>
    /// UI for displaying and managing scandals
    /// </summary>
    public class ScandalUI : MonoBehaviour
    {
        [Header("Scandal List")]
        public Transform ScandalListContainer;
        public GameObject ScandalItemPrefab;
        
        [Header("Scandal Details")]
        public GameObject DetailsPanel;
        public TextMeshProUGUI ScandalTitleText;
        public TextMeshProUGUI ScandalDescriptionText;
        public TextMeshProUGUI StageText;
        public TextMeshProUGUI SeverityText;
        public Slider MediaCoverageBar;
        public Slider PublicInterestBar;
        public Slider EvidenceBar;
        public TextMeshProUGUI ImpactText;
        public TextMeshProUGUI LatestHeadlineText;
        
        [Header("Response Options")]
        public Transform ResponseOptionsContainer;
        public GameObject ResponseOptionPrefab;
        public Button ExecuteResponseButton;
        
        private Scandal.Scandal selectedScandal;
        private Scandal.ScandalManager scandalManager;
        private PlayerState player;
        
        public void Initialize(Scandal.ScandalManager manager, PlayerState playerState)
        {
            scandalManager = manager;
            player = playerState;
        }
        
        public void UpdateScandalDisplay()
        {
            if (player == null || player.ActiveScandals == null) return;
            
            // Clear existing
            if (ScandalListContainer != null)
            {
                foreach (Transform child in ScandalListContainer)
                {
                    Destroy(child.gameObject);
                }
            }
            
            // Create scandal items
            foreach (var scandal in player.ActiveScandals)
            {
                CreateScandalItem(scandal);
            }
        }
        
        private void CreateScandalItem(Scandal.Scandal scandal)
        {
            GameObject item;
            
            if (ScandalItemPrefab != null)
            {
                item = Instantiate(ScandalItemPrefab, ScandalListContainer);
            }
            else
            {
                item = new GameObject($"ScandalItem_{scandal.ID}");
                item.transform.SetParent(ScandalListContainer);
            }
            
            // Add click handler
            Button button = item.GetComponent<Button>();
            if (button == null)
                button = item.AddComponent<Button>();
            
            button.onClick.AddListener(() => ShowScandalDetails(scandal));
            
            // Populate with scandal info
            // (Would need UI components in prefab)
        }
        
        private void ShowScandalDetails(Scandal.Scandal scandal)
        {
            selectedScandal = scandal;
            
            if (DetailsPanel != null)
                DetailsPanel.SetActive(true);
            
            if (ScandalTitleText != null)
                ScandalTitleText.text = scandal.Title;
            
            if (ScandalDescriptionText != null)
                ScandalDescriptionText.text = scandal.Description;
            
            if (StageText != null)
                StageText.text = $"Stage: {scandal.CurrentStage} (Day {scandal.TurnsInStage})";
            
            if (SeverityText != null)
                SeverityText.text = $"Severity: {scandal.CurrentSeverity}/10";
            
            if (MediaCoverageBar != null)
                MediaCoverageBar.value = scandal.MediaCoverage / 100f;
            
            if (PublicInterestBar != null)
                PublicInterestBar.value = scandal.PublicInterest / 100f;
            
            if (EvidenceBar != null)
                EvidenceBar.value = scandal.EvidenceStrength / 100f;
            
            // Show impact
            if (ImpactText != null)
            {
                string impact = "Impact:\n";
                if (scandal.ResourceImpacts != null)
                {
                    foreach (var impactPair in scandal.ResourceImpacts)
                    {
                        impact += $"• {impactPair.Key}: {impactPair.Value:F1}\n";
                    }
                }
                ImpactText.text = impact;
            }
            
            // Show latest headline
            if (LatestHeadlineText != null && scandal.Headlines != null && scandal.Headlines.Count > 0)
            {
                LatestHeadlineText.text = $"Latest: \"{scandal.Headlines[scandal.Headlines.Count - 1]}\"";
            }
            
            // Show response options
            ShowResponseOptions(scandal);
        }
        
        private void ShowResponseOptions(Scandal.Scandal scandal)
        {
            if (ResponseOptionsContainer == null || scandalManager == null) return;
            
            // Clear existing
            foreach (Transform child in ResponseOptionsContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Get available responses
            var options = scandalManager.GetResponseOptions(scandal.ID);
            
            foreach (var option in options)
            {
                CreateResponseOption(option);
            }
        }
        
        private void CreateResponseOption(ResponseOption option)
        {
            GameObject item;
            
            if (ResponseOptionPrefab != null)
            {
                item = Instantiate(ResponseOptionPrefab, ResponseOptionsContainer);
            }
            else
            {
                item = new GameObject($"ResponseOption_{option.Type}");
                item.transform.SetParent(ResponseOptionsContainer);
            }
            
            // Add toggle/button
            Toggle toggle = item.GetComponent<Toggle>();
            if (toggle == null)
                toggle = item.AddComponent<Toggle>();
            
            // Populate with option info
            // (Would need UI components in prefab)
        }
        
        public void ExecuteSelectedResponse()
        {
            if (selectedScandal == null || scandalManager == null) return;
            
            // Get selected response type (would need UI state tracking)
            // For now, simplified
            var result = scandalManager.RespondToScandal(selectedScandal.ID, ResponseType.Deny);
            
            Debug.Log($"Response result: {result.Message}");
            
            // Update display
            UpdateScandalDisplay();
            ShowScandalDetails(selectedScandal);
        }
    }
}

