using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ElectionEmpire.World;
using ElectionEmpire.Gameplay;

namespace ElectionEmpire.UI
{
    /// <summary>
    /// Main game HUD displaying all player information
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        [Header("Character Info")]
        public TextMeshProUGUI CharacterNameText;
        public TextMeshProUGUI CurrentOfficeText;
        public TextMeshProUGUI DaysInOfficeText;
        
        [Header("Resources")]
        public TextMeshProUGUI PublicTrustText;
        public Slider PublicTrustBar;
        public TextMeshProUGUI PoliticalCapitalText;
        public TextMeshProUGUI CampaignFundsText;
        public TextMeshProUGUI MediaInfluenceText;
        public Slider MediaInfluenceBar;
        public TextMeshProUGUI PartyLoyaltyText;
        public Slider PartyLoyaltyBar;
        
        [Header("Status")]
        public TextMeshProUGUI ActiveScandalsText;
        public TextMeshProUGUI PoliticalAlliesText;
        public TextMeshProUGUI NationalApprovalText;
        public TextMeshProUGUI NextElectionText;
        
        [Header("Actions")]
        public Button CampaignButton;
        public Button PolicyButton;
        public Button MediaButton;
        public Button StaffButton;
        public Button OpponentsButton;
        public Button MapButton;
        public Button EventsButton;
        public Button MenuButton;
        
        private GameState currentGameState;
        
        private void Start()
        {
            SetupButtons();
        }
        
        private void SetupButtons()
        {
            if (CampaignButton != null)
                CampaignButton.onClick.AddListener(() => OnCampaign());
            
            if (PolicyButton != null)
                PolicyButton.onClick.AddListener(() => OnPolicy());
            
            if (MediaButton != null)
                MediaButton.onClick.AddListener(() => OnMedia());
            
            if (OpponentsButton != null)
                OpponentsButton.onClick.AddListener(() => OnOpponents());
            
            if (MapButton != null)
                MapButton.onClick.AddListener(() => OnMap());
            
            if (MenuButton != null)
                MenuButton.onClick.AddListener(() => OnMenu());
        }
        
        public void UpdateHUD(GameState gameState)
        {
            currentGameState = gameState;
            
            if (gameState == null || gameState.Player == null)
                return;
            
            var player = gameState.Player;
            
            // Character info
            if (CharacterNameText != null)
                CharacterNameText.text = player.Character.Name;
            
            if (CurrentOfficeText != null)
            {
                if (player.CurrentOffice != null)
                    CurrentOfficeText.text = $"Current Office: {player.CurrentOffice.Name}";
                else
                    CurrentOfficeText.text = "Current Office: None";
            }
            
            if (DaysInOfficeText != null && player.CurrentOffice != null)
            {
                int daysInOffice = (int)(System.DateTime.Now - player.TermStartDate).TotalDays;
                int termLength = player.CurrentOffice.TermLengthDays;
                DaysInOfficeText.text = $"Days in Office: {daysInOffice} / {termLength}";
            }
            
            // Resources
            if (PublicTrustText != null)
            {
                float trust = player.Resources.GetValueOrDefault("PublicTrust", 50f);
                PublicTrustText.text = $"Public Trust: {trust:F1}%";
            }
            
            if (PublicTrustBar != null)
            {
                float trust = player.Resources.GetValueOrDefault("PublicTrust", 50f);
                PublicTrustBar.value = trust / 100f;
            }
            
            if (PoliticalCapitalText != null)
            {
                int capital = (int)player.Resources.GetValueOrDefault("PoliticalCapital", 0f);
                int maxCapital = player.CurrentOffice != null ? player.CurrentOffice.Tier * 30 : 30;
                PoliticalCapitalText.text = $"Political Capital: {capital} / {maxCapital}";
            }
            
            if (CampaignFundsText != null)
            {
                float funds = player.Resources.GetValueOrDefault("CampaignFunds", 0f);
                CampaignFundsText.text = $"Campaign Funds: ${funds:N0}";
            }
            
            if (MediaInfluenceText != null)
            {
                int media = (int)player.Resources.GetValueOrDefault("MediaInfluence", 0f);
                MediaInfluenceText.text = $"Media Influence: {media} / 100";
            }
            
            if (MediaInfluenceBar != null)
            {
                int media = (int)player.Resources.GetValueOrDefault("MediaInfluence", 0f);
                MediaInfluenceBar.value = media / 100f;
            }
            
            if (PartyLoyaltyText != null)
            {
                float loyalty = player.Resources.GetValueOrDefault("PartyLoyalty", 50f);
                PartyLoyaltyText.text = $"Party Loyalty: {loyalty:F1}%";
            }
            
            if (PartyLoyaltyBar != null)
            {
                float loyalty = player.Resources.GetValueOrDefault("PartyLoyalty", 50f);
                PartyLoyaltyBar.value = loyalty / 100f;
            }
            
            // Status
            if (ActiveScandalsText != null)
            {
                int scandals = player.ActiveScandals != null ? player.ActiveScandals.Count : 0;
                ActiveScandalsText.text = $"Active Scandals: {scandals}";
            }
            
            if (PoliticalAlliesText != null)
            {
                int allies = player.PoliticalAllies != null ? player.PoliticalAllies.Count : 0;
                PoliticalAlliesText.text = $"Political Allies: {allies}";
            }
            
            if (NationalApprovalText != null)
            {
                NationalApprovalText.text = $"National Approval: {player.ApprovalRating:F1}%";
            }
            
            if (NextElectionText != null)
            {
                if (player.CurrentOffice != null)
                {
                    int daysUntil = player.CurrentOffice.TermLengthDays - 
                        (int)(System.DateTime.Now - player.TermStartDate).TotalDays;
                    NextElectionText.text = $"Next Election: {daysUntil} days";
                }
                else
                {
                    NextElectionText.text = "Next Election: N/A";
                }
            }
        }
        
        private void OnCampaign()
        {
            Debug.Log("Campaign action clicked");
            // Open campaign menu
        }
        
        private void OnPolicy()
        {
            Debug.Log("Policy action clicked");
            // Open policy menu
        }
        
        private void OnMedia()
        {
            Debug.Log("Media action clicked");
            // Open media menu
        }
        
        private void OnOpponents()
        {
            Debug.Log("Opponents clicked");
            // Show opponent list
        }
        
        private void OnMap()
        {
            Debug.Log("Map clicked");
            // Show world map
        }
        
        private void OnMenu()
        {
            Debug.Log("Menu clicked");
            // Open pause menu
        }
    }
}

