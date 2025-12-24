using System.Collections.Generic;
using UnityEngine;

namespace ElectionEmpire.AI
{
    /// <summary>
    /// Tracks relationships between AI and player/other AI
    /// </summary>
    public class AIRelationshipManager
    {
        // Track relationships: ID -> opinion (-100 to 100)
        private Dictionary<string, float> relationships = new Dictionary<string, float>();
        
        /// <summary>
        /// Update relationship based on action
        /// </summary>
        public void UpdateRelationship(string targetID, AIAction action)
        {
            float change = 0f;
            
            switch (action.Type)
            {
                case DecisionType.Attack:
                    change = -30f;
                    break;
                
                case DecisionType.Alliance:
                    change = +40f;
                    break;
                
                case DecisionType.Policy:
                    // Depends on if they agree with policy
                    change = UnityEngine.Random.Range(-10f, 10f);
                    break;
            }
            
            if (!relationships.ContainsKey(targetID))
                relationships[targetID] = 0f;
            
            relationships[targetID] += change;
            relationships[targetID] = Mathf.Clamp(relationships[targetID], -100f, 100f);
        }
        
        /// <summary>
        /// Get relationship value
        /// </summary>
        public float GetRelationship(string targetID)
        {
            return relationships.GetValueOrDefault(targetID, 0f);
        }
        
        /// <summary>
        /// Check if will form alliance
        /// </summary>
        public bool WillFormAlliance(AIOpponent ai, string targetID)
        {
            float opinion = GetRelationship(targetID);
            float threshold = 100f - ai.Personality.Loyalty;
            
            return opinion > threshold;
        }
        
        /// <summary>
        /// Check if will attack
        /// </summary>
        public bool WillAttack(AIOpponent ai, string targetID)
        {
            float opinion = GetRelationship(targetID);
            float threshold = -50f + (ai.Personality.Aggression - 50f);
            
            return opinion < threshold;
        }
    }
}

