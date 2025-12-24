using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ElectionEmpire.AI;

namespace ElectionEmpire.UI
{
    /// <summary>
    /// UI list displaying all AI opponents
    /// </summary>
    public class AIOpponentList : MonoBehaviour
    {
        [Header("List Container")]
        public Transform ListContainer;
        public GameObject OpponentCardPrefab;
        
        [Header("Header")]
        public TextMeshProUGUI HeaderText;
        
        private List<AIOpponentCard> displayedCards = new List<AIOpponentCard>();
        
        /// <summary>
        /// Display list of AI opponents
        /// </summary>
        public void DisplayOpponents(List<AIOpponent> opponents)
        {
            ClearList();
            
            if (HeaderText != null)
                HeaderText.text = $"OPPONENTS ({opponents.Count})";
            
            foreach (var opponent in opponents)
            {
                CreateOpponentCard(opponent);
            }
        }
        
        private void CreateOpponentCard(AIOpponent opponent)
        {
            GameObject cardObj;
            
            if (OpponentCardPrefab != null)
            {
                cardObj = Instantiate(OpponentCardPrefab, ListContainer);
            }
            else
            {
                // Fallback: create simple card
                cardObj = new GameObject($"Card_{opponent.ID}");
                cardObj.transform.SetParent(ListContainer);
                
                var layout = cardObj.AddComponent<VerticalLayoutGroup>();
                layout.spacing = 5;
                layout.padding = new RectOffset(10, 10, 10, 10);
                
                var bg = cardObj.AddComponent<Image>();
                bg.color = new Color(0.2f, 0.2f, 0.2f);
            }
            
            var card = cardObj.GetComponent<AIOpponentCard>();
            if (card == null)
                card = cardObj.AddComponent<AIOpponentCard>();
            
            card.DisplayOpponent(opponent);
            displayedCards.Add(card);
        }
        
        private void ClearList()
        {
            foreach (var card in displayedCards)
            {
                if (card != null)
                    Destroy(card.gameObject);
            }
            
            displayedCards.Clear();
            
            // Also clear container children
            foreach (Transform child in ListContainer)
            {
                Destroy(child.gameObject);
            }
        }
        
        /// <summary>
        /// Refresh opponent data
        /// </summary>
        public void RefreshOpponents(List<AIOpponent> opponents)
        {
            foreach (var card in displayedCards)
            {
                if (card != null && card.Opponent != null)
                {
                    var updated = opponents.Find(o => o.ID == card.Opponent.ID);
                    if (updated != null)
                    {
                        card.DisplayOpponent(updated);
                    }
                }
            }
        }
    }
}

