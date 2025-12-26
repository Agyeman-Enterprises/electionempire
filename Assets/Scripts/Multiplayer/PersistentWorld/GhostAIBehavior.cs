using System;
using System.Collections.Generic;
using UnityEngine;

// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - Ghost AI Behavior
// Defines how a ghost AI behaves based on learned player patterns
// ═══════════════════════════════════════════════════════════════════════════════

namespace ElectionEmpire.Multiplayer.PersistentWorld
{
    /// <summary>
    /// AI behavior parameters for a ghost politician.
    /// Derived from player behavior profile but with AI-specific traits.
    /// </summary>
    [Serializable]
    public class GhostAIBehavior
    {
        // Core Behavior Traits
        public float Aggression;              // 0-100: How aggressive the ghost is
        public float Defensiveness;           // 0-100: How defensive they get when attacked
        public float OpportunismLevel;        // 0-100: How much they seek opportunities
        public float CrisisCompetence;        // 0-100: How well they handle crises
        
        // Strategic Preferences
        public bool WillAttackFirst;          // Will they initiate attacks?
        public bool WillFormAlliances;        // Will they form alliances?
        public bool WillBetrayAlliances;      // Will they betray allies?
        public bool WillUseDirtyTricks;      // Will they use dirty tactics?
        
        // Decision Patterns
        public float PreferredPath;           // 0-1: Preferred strategic path
        public float Adaptability;            // 0-100: How quickly they adapt to new situations
        public float Consistency;             // 0-100: How consistent their behavior is
        
        // Response Patterns
        public Dictionary<string, float> SituationResponses; // Situation -> response probability
        
        public GhostAIBehavior()
        {
            SituationResponses = new Dictionary<string, float>();
        }
        
        /// <summary>
        /// Generate AI behavior from a player behavior profile.
        /// </summary>
        public static GhostAIBehavior FromBehaviorProfile(PlayerBehaviorProfile profile)
        {
            if (profile == null)
            {
                return GenerateDefault();
            }
            
            var behavior = new GhostAIBehavior
            {
                Aggression = profile.ActionFrequency.ContainsKey("attack") 
                    ? profile.ActionFrequency["attack"] * 100f 
                    : 50f,
                Defensiveness = (1f - profile.RiskTolerance) * 100f,
                OpportunismLevel = profile.RiskTolerance * 100f,
                CrisisCompetence = profile.AverageApprovalRating / 100f * 80f + 20f,
                
                WillAttackFirst = profile.DirtyTricksWillingness > 0.5f,
                WillFormAlliances = profile.AllianceFormationRate > 0.3f,
                WillBetrayAlliances = profile.BetrayalRate > 0.3f,
                WillUseDirtyTricks = profile.DirtyTricksWillingness > 0.4f,
                
                PreferredPath = profile.RiskTolerance,
                Adaptability = 60f, // Ghosts are somewhat adaptable
                Consistency = 70f  // Ghosts are fairly consistent
            };
            
            // Copy response patterns
            foreach (var kvp in profile.ResponsePatterns)
            {
                behavior.SituationResponses[kvp.Key] = kvp.Value;
            }
            
            return behavior;
        }
        
        /// <summary>
        /// Generate default AI behavior for ghosts without profiles.
        /// </summary>
        public static GhostAIBehavior GenerateDefault()
        {
            return new GhostAIBehavior
            {
                Aggression = 50f,
                Defensiveness = 50f,
                OpportunismLevel = 50f,
                CrisisCompetence = 50f,
                WillAttackFirst = false,
                WillFormAlliances = true,
                WillBetrayAlliances = false,
                WillUseDirtyTricks = false,
                PreferredPath = 0.5f,
                Adaptability = 50f,
                Consistency = 50f
            };
        }
    }
}

