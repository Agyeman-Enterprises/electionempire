using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ElectionEmpire.AI;

namespace ElectionEmpire.UI
{
    /// <summary>
    /// UI card displaying AI opponent information
    /// </summary>
    public class AIOpponentCard : MonoBehaviour
    {
        [Header("References")]
        public AIOpponent Opponent;
        
        [Header("Text Fields")]
        public TextMeshProUGUI NameText;
        public TextMeshProUGUI NicknameText;
        public TextMeshProUGUI ArchetypeText;
        public TextMeshProUGUI ApprovalText;
        public TextMeshProUGUI RankText;
        public TextMeshProUGUI BackstoryText;
        public TextMeshProUGUI SignatureMovesText;
        
        [Header("Personality Bars")]
        public Slider AggressionBar;
        public Slider CunningBar;
        public Slider CharismaBar;
        public Slider LoyaltyBar;
        
        [Header("Visual")]
        public Image Portrait;
        public Image Background;
        
        public void DisplayOpponent(AIOpponent opponent)
        {
            Opponent = opponent;
            
            if (NameText != null)
                NameText.text = opponent.Name;
            
            if (NicknameText != null)
                NicknameText.text = opponent.GeneratedNickname;
            
            if (ArchetypeText != null)
                ArchetypeText.text = opponent.Archetype.ToString();
            
            if (ApprovalText != null)
                ApprovalText.text = $"{opponent.ApprovalRating:F1}%";
            
            // Personality visualization
            if (AggressionBar != null)
                AggressionBar.value = opponent.Personality.Aggression / 100f;
            
            if (CunningBar != null)
                CunningBar.value = opponent.Personality.Cunning / 100f;
            
            if (CharismaBar != null)
                CharismaBar.value = opponent.Personality.Charisma / 100f;
            
            if (LoyaltyBar != null)
                LoyaltyBar.value = opponent.Personality.Loyalty / 100f;
            
            if (BackstoryText != null)
                BackstoryText.text = opponent.Backstory;
            
            if (SignatureMovesText != null)
            {
                string moves = "";
                foreach (var move in opponent.SignatureMoves)
                {
                    moves += $"• {move}\n";
                }
                SignatureMovesText.text = moves;
            }
            
            // Color code by archetype
            if (Portrait != null)
                Portrait.color = GetArchetypeColor(opponent.Archetype);
            
            if (Background != null)
                Background.color = GetArchetypeColor(opponent.Archetype) * 0.3f;
        }
        
        private Color GetArchetypeColor(Archetype archetype)
        {
            switch (archetype)
            {
                case Archetype.Idealist: 
                    return new Color(0.5f, 0.7f, 1f); // Light blue
                case Archetype.MachineBoss: 
                    return new Color(0.5f, 0.5f, 0.5f); // Gray
                case Archetype.Populist: 
                    return new Color(1f, 0.6f, 0.2f); // Orange
                case Archetype.Maverick: 
                    return new Color(0.8f, 0.2f, 0.8f); // Purple
                case Archetype.Revolutionary: 
                    return new Color(0.9f, 0.2f, 0.2f); // Red
                case Archetype.Technocrat:
                    return new Color(0.2f, 0.8f, 0.8f); // Cyan
                case Archetype.Showman:
                    return new Color(1f, 0.8f, 0.2f); // Yellow
                case Archetype.Corporate:
                    return new Color(0.3f, 0.3f, 0.8f); // Dark blue
                default: 
                    return Color.white;
            }
        }
    }
}

