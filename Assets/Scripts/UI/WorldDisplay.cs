using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using ElectionEmpire.World;

namespace ElectionEmpire.UI
{
    /// <summary>
    /// Displays world information and district details
    /// </summary>
    public class WorldDisplay : MonoBehaviour
    {
        [Header("World Stats")]
        public TextMeshProUGUI WorldStatsText;
        
        [Header("District Tooltip")]
        public GameObject TooltipPanel;
        public TextMeshProUGUI TooltipNameText;
        public TextMeshProUGUI TooltipInfoText;
        
        [Header("District Details Panel")]
        public GameObject DetailsPanel;
        public TextMeshProUGUI DetailsNameText;
        public TextMeshProUGUI DetailsInfoText;
        public Button CloseDetailsButton;
        
        [Header("Region/State Lists")]
        public Transform RegionListContainer;
        public Transform StateListContainer;
        public GameObject ListItemPrefab;
        
        private ElectionEmpire.World.World currentWorld;
        
        private void Start()
        {
            if (CloseDetailsButton != null)
                CloseDetailsButton.onClick.AddListener(() => DetailsPanel?.SetActive(false));
            
            if (TooltipPanel != null)
                TooltipPanel.SetActive(false);
            
            if (DetailsPanel != null)
                DetailsPanel.SetActive(false);
        }
        
        public void DisplayWorld(ElectionEmpire.World.World world)
        {
            currentWorld = world;
            
            if (WorldStatsText != null && world != null)
            {
                int totalDistricts = world.Nation.Regions
                    .SelectMany(r => r.States)
                    .Sum(s => s.Districts.Count);
                
                WorldStatsText.text = $"World: {world.Nation.Name}\n" +
                                     $"Regions: {world.Nation.Regions.Count}\n" +
                                     $"States: {world.Nation.Regions.SelectMany(r => r.States).Count()}\n" +
                                     $"Districts: {totalDistricts}\n" +
                                     $"Population: {world.Nation.TotalPopulation:N0}\n" +
                                     $"Seed: {world.Seed}";
            }
        }
        
        public void ShowDistrictTooltip(District district)
        {
            if (TooltipPanel == null || district == null) return;
            
            TooltipPanel.SetActive(true);
            
            if (TooltipNameText != null)
                TooltipNameText.text = district.Name;
            
            if (TooltipInfoText != null)
            {
                string info = $"Population: {district.Population:N0}\n" +
                             $"Type: {district.Type}\n" +
                             $"Political Lean: {(district.PoliticalLean > 0 ? "+" : "")}{district.PoliticalLean:F1}\n\n" +
                             $"Top Issues:\n";
                
                foreach (var issue in district.PriorityIssues.Take(3))
                {
                    info += $"• {issue}\n";
                }
                
                TooltipInfoText.text = info;
            }
        }
        
        public void HideDistrictTooltip()
        {
            if (TooltipPanel != null)
                TooltipPanel.SetActive(false);
        }
        
        public void ShowDistrictDetails(District district)
        {
            if (DetailsPanel == null || district == null) return;
            
            DetailsPanel.SetActive(true);
            
            if (DetailsNameText != null)
                DetailsNameText.text = district.Name;
            
            if (DetailsInfoText != null)
            {
                string info = $"Population: {district.Population:N0}\n" +
                             $"Type: {district.Type}\n" +
                             $"Political Lean: {(district.PoliticalLean > 0 ? "+" : "")}{district.PoliticalLean:F1}\n\n" +
                             
                             $"Demographics:\n" +
                             $"  Age: {district.Demographics.Youth18to29:F1}% Youth, " +
                             $"{district.Demographics.Seniors65Plus:F1}% Seniors\n" +
                             $"  Income: {district.Demographics.LowIncome:F1}% Low, " +
                             $"{district.Demographics.HighIncome:F1}% High\n" +
                             $"  Education: {district.Demographics.CollegeEducated:F1}% College\n\n" +
                             
                             $"Top Issues:\n";
                
                foreach (var issue in district.PriorityIssues)
                {
                    info += $"• {issue}\n";
                }
                
                info += $"\nVoter Blocs:\n";
                var topBlocs = district.BlocStrength
                    .OrderByDescending(b => b.Value)
                    .Take(5);
                
                foreach (var bloc in topBlocs)
                {
                    info += $"  {bloc.Key}: {bloc.Value:F1}%\n";
                }
                
                DetailsInfoText.text = info;
            }
        }
        
        public void PopulateRegionList(ElectionEmpire.World.World world)
        {
            if (RegionListContainer == null || world == null) return;
            
            ClearContainer(RegionListContainer);
            
            foreach (var region in world.Nation.Regions)
            {
                CreateListItem(RegionListContainer, 
                    $"{region.Name} ({region.States.Count} states)");
            }
        }
        
        public void PopulateStateList(Region region)
        {
            if (StateListContainer == null || region == null) return;
            
            ClearContainer(StateListContainer);
            
            foreach (var state in region.States)
            {
                CreateListItem(StateListContainer, 
                    $"{state.Name} (Pop: {state.Population:N0})");
            }
        }
        
        private void CreateListItem(Transform container, string text)
        {
            GameObject item;
            
            if (ListItemPrefab != null)
            {
                item = Instantiate(ListItemPrefab, container);
                var textComponent = item.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                    textComponent.text = text;
            }
            else
            {
                item = new GameObject("ListItem");
                item.transform.SetParent(container);
                var textComponent = item.AddComponent<TextMeshProUGUI>();
                textComponent.text = text;
                textComponent.fontSize = 14;
            }
        }
        
        private void ClearContainer(Transform container)
        {
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }
        }
    }
}

