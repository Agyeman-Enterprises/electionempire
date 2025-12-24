using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ElectionEmpire.World;
using ElectionEmpire.Gameplay;

namespace ElectionEmpire.UI
{
    /// <summary>
    /// UI for tracking election progress and polling
    /// </summary>
    public class ElectionUI : MonoBehaviour
    {
        [Header("Election Info")]
        public TextMeshProUGUI OfficeNameText;
        public TextMeshProUGUI PhaseText;
        public TextMeshProUGUI DaysRemainingText;
        
        [Header("Polling Display")]
        public Transform PollingListContainer;
        public GameObject PollingItemPrefab;
        
        [Header("Upcoming Events")]
        public Transform EventsContainer;
        public TextMeshProUGUI UpcomingEventsText;
        
        [Header("Actions")]
        public Button CampaignActionsButton;
        public Button AttackOpponentButton;
        public Button ViewDistrictsButton;
        public Button ScheduleEventButton;
        
        private ElectionManager electionManager;
        
        private void Start()
        {
            SetupButtons();
        }
        
        private void SetupButtons()
        {
            if (CampaignActionsButton != null)
                CampaignActionsButton.onClick.AddListener(() => OnCampaignActions());
            
            if (AttackOpponentButton != null)
                AttackOpponentButton.onClick.AddListener(() => OnAttackOpponent());
            
            if (ViewDistrictsButton != null)
                ViewDistrictsButton.onClick.AddListener(() => OnViewDistricts());
        }
        
        public void DisplayElection(ElectionManager manager)
        {
            electionManager = manager;
            
            if (manager == null) return;
            
            // Update election info
            if (OfficeNameText != null)
                OfficeNameText.text = $"ELECTION FOR: {manager.GetTargetOffice()?.Name ?? "Unknown"}";
            
            if (PhaseText != null)
                PhaseText.text = $"Phase: {manager.CurrentPhase} ({manager.DaysUntilNextPhase} days remaining)";
            
            // Update polling
            UpdatePollingDisplay(manager.GetCandidates());
            
            // Update upcoming events
            UpdateUpcomingEvents(manager);
        }
        
        private void UpdatePollingDisplay(List<PlayerState> candidates)
        {
            if (PollingListContainer == null) return;
            
            // Clear existing
            foreach (Transform child in PollingListContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Sort by polling
            var sorted = candidates.OrderByDescending(c => c.CurrentPolling).ToList();
            
            for (int i = 0; i < sorted.Count; i++)
            {
                var candidate = sorted[i];
                CreatePollingItem(candidate, i + 1);
            }
        }
        
        private void CreatePollingItem(PlayerState candidate, int rank)
        {
            GameObject item;
            
            if (PollingItemPrefab != null)
            {
                item = Instantiate(PollingItemPrefab, PollingListContainer);
            }
            else
            {
                item = new GameObject($"PollingItem_{rank}");
                item.transform.SetParent(PollingListContainer);
                
                var layout = item.AddComponent<HorizontalLayoutGroup>();
                layout.spacing = 10;
            }
            
            // Rank
            GameObject rankObj = new GameObject("Rank");
            rankObj.transform.SetParent(item.transform);
            TextMeshProUGUI rankText = rankObj.AddComponent<TextMeshProUGUI>();
            rankText.text = $"{rank}.";
            rankText.fontSize = 18;
            rankText.fontStyle = FontStyles.Bold;
            
            // Name
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(item.transform);
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = candidate.IsPlayer ? "YOU" : candidate.Character.Name;
            nameText.fontSize = 16;
            nameText.fontStyle = candidate.IsPlayer ? FontStyles.Bold : FontStyles.Normal;
            
            // Polling
            GameObject pollingObj = new GameObject("Polling");
            pollingObj.transform.SetParent(item.transform);
            TextMeshProUGUI pollingText = pollingObj.AddComponent<TextMeshProUGUI>();
            pollingText.text = $"{candidate.CurrentPolling:F1}%";
            pollingText.fontSize = 16;
            
            // Bar
            GameObject barObj = new GameObject("Bar");
            barObj.transform.SetParent(item.transform);
            Slider bar = barObj.AddComponent<Slider>();
            bar.value = candidate.CurrentPolling / 100f;
            
            // Trend (simplified - would track previous polling)
            GameObject trendObj = new GameObject("Trend");
            trendObj.transform.SetParent(item.transform);
            TextMeshProUGUI trendText = trendObj.AddComponent<TextMeshProUGUI>();
            trendText.text = "─"; // Would show ↑ ↓ ─
            trendText.fontSize = 14;
        }
        
        private void UpdateUpcomingEvents(ElectionManager manager)
        {
            if (UpcomingEventsText == null) return;
            
            string events = "";
            
            switch (manager.CurrentPhase)
            {
                case ElectionManager.ElectionPhase.Campaign:
                    events = "• Debate in 7 days\n";
                    events += "• Major Rally opportunity (3 days)";
                    break;
                
                case ElectionManager.ElectionPhase.Debate:
                    events = "• Debate happening now!";
                    break;
                
                case ElectionManager.ElectionPhase.ElectionDay:
                    events = "• Election Day - Voting now!";
                    break;
            }
            
            UpcomingEventsText.text = events;
        }
        
        private void OnCampaignActions()
        {
            Debug.Log("Campaign actions clicked");
        }
        
        private void OnAttackOpponent()
        {
            Debug.Log("Attack opponent clicked");
        }
        
        private void OnViewDistricts()
        {
            Debug.Log("View districts clicked");
        }
    }
}

