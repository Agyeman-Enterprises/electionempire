using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ElectionEmpire.Character;

namespace ElectionEmpire.UI
{
    /// <summary>
    /// Displays character information in UI
    /// </summary>
    public class CharacterDisplay : MonoBehaviour
    {
        [Header("Character Info")]
        public TextMeshProUGUI NameText;
        public TextMeshProUGUI NicknameText;
        public TextMeshProUGUI BackgroundText;
        public TextMeshProUGUI ChaosRatingText;
        public TextMeshProUGUI DifficultyText;
        public TextMeshProUGUI LegacyBonusText;
        
        [Header("Details")]
        public Transform HistoryContainer;
        public Transform SkillsContainer;
        public Transform QuirksContainer;
        public Transform HandicapsContainer;
        public TextMeshProUGUI WeaponText;
        public TextMeshProUGUI PublicImageText;
        
        [Header("Prefabs")]
        public GameObject ListItemPrefab;
        
        public void DisplayCharacter(Character character)
        {
            if (character == null)
                return;
            
            // Basic info
            if (NameText != null)
                NameText.text = character.Name;
            
            if (NicknameText != null)
                NicknameText.text = character.GeneratedNickname ?? character.Name;
            
            if (BackgroundText != null && character.Background != null)
                BackgroundText.text = character.Background.Name;
            
            // Chaos rating
            if (ChaosRatingText != null)
            {
                string stars = "";
                for (int i = 0; i < character.ChaosRating; i++)
                    stars += "🔥";
                ChaosRatingText.text = $"Chaos: {stars}";
            }
            
            // Difficulty
            if (DifficultyText != null)
            {
                string difficulty = "EASY";
                if (character.DifficultyMultiplier >= 1.5f) difficulty = "NIGHTMARE";
                else if (character.DifficultyMultiplier >= 1.3f) difficulty = "HARD";
                else if (character.DifficultyMultiplier >= 1.1f) difficulty = "NORMAL";
                
                DifficultyText.text = $"Difficulty: {difficulty}";
            }
            
            // Legacy bonus
            if (LegacyBonusText != null)
            {
                int bonusPercent = Mathf.RoundToInt((character.LegacyPointBonus - 1f) * 100f);
                LegacyBonusText.text = $"Victory Bonus: +{bonusPercent}% Legacy Points";
            }
            
            // History
            if (HistoryContainer != null)
            {
                ClearContainer(HistoryContainer);
                if (character.PersonalHistory != null)
                {
                    foreach (var history in character.PersonalHistory)
                    {
                        AddListItem(HistoryContainer, history);
                    }
                }
            }
            
            // Skills
            if (SkillsContainer != null)
            {
                ClearContainer(SkillsContainer);
                if (character.Skills != null)
                {
                    foreach (var skill in character.Skills)
                    {
                        AddListItem(SkillsContainer, skill.Name);
                    }
                }
            }
            
            // Quirks
            if (QuirksContainer != null)
            {
                ClearContainer(QuirksContainer);
                if (character.PositiveQuirks != null)
                {
                    foreach (var quirk in character.PositiveQuirks)
                    {
                        AddListItem(QuirksContainer, $"+ {quirk.Name}");
                    }
                }
                if (character.NegativeQuirks != null)
                {
                    foreach (var quirk in character.NegativeQuirks)
                    {
                        AddListItem(QuirksContainer, $"- {quirk.Name}");
                    }
                }
            }
            
            // Handicaps
            if (HandicapsContainer != null)
            {
                ClearContainer(HandicapsContainer);
                if (character.Handicaps != null && character.Handicaps.Count > 0)
                {
                    foreach (var handicap in character.Handicaps)
                    {
                        AddListItem(HandicapsContainer, handicap.Name);
                    }
                }
                else
                {
                    AddListItem(HandicapsContainer, "None");
                }
            }
            
            // Weapon
            if (WeaponText != null)
            {
                if (character.Weapon != null)
                    WeaponText.text = $"Secret Weapon: {character.Weapon.Name}";
                else
                    WeaponText.text = "Secret Weapon: None";
            }
            
            // Public Image
            if (PublicImageText != null)
            {
                if (character.PublicImage != null)
                    PublicImageText.text = $"Public Image: {character.PublicImage.Name}";
                else
                    PublicImageText.text = "Public Image: Unknown";
            }
        }
        
        private void ClearContainer(Transform container)
        {
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }
        }
        
        private void AddListItem(Transform container, string text)
        {
            if (ListItemPrefab != null)
            {
                GameObject item = Instantiate(ListItemPrefab, container);
                TextMeshProUGUI textComponent = item.GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                    textComponent.text = text;
            }
            else
            {
                // Fallback: create simple text
                GameObject item = new GameObject("ListItem");
                item.transform.SetParent(container);
                TextMeshProUGUI textComponent = item.AddComponent<TextMeshProUGUI>();
                textComponent.text = text;
                textComponent.fontSize = 14;
            }
        }
    }
}

