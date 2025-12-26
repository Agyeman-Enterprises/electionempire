using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.InfiniteUniverse;

// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - Player-to-Ghost Interaction
// Handles interactions between active players and ghost politicians
// ═══════════════════════════════════════════════════════════════════════════════

namespace ElectionEmpire.InfiniteUniverse
{
    /// <summary>
    /// Handles interactions between active players and ghost politicians
    /// </summary>
    public class PlayerGhostInteraction : MonoBehaviour
    {
        /// <summary>
        /// When a player encounters a ghost politician
        /// </summary>
        public static GhostEncounterResult ProcessGhostEncounter(
            string playerId, 
            ExtendedGhostPolitician ghost,
            EncounterType encounterType)
        {
            var result = new GhostEncounterResult
            {
                GhostName = ghost.CharacterName,
                GhostOriginalPlayer = ghost.OriginalPlayerId,
                EncounterType = encounterType
            };
            
            // Generate dialogue based on ghost's behavior profile
            result.GhostDialogue = GenerateGhostDialogue(ghost, encounterType);
            
            // Determine ghost's reaction to player
            result.GhostReaction = DetermineGhostReaction(ghost, playerId, encounterType);
            
            // Special: Is this ghost the original player's former character?
            if (ghost.OriginalPlayerId == playerId)
            {
                result.IsOwnGhost = true;
                result.SpecialMessage = "You encounter your former self, now operating on autopilot...";
            }
            
            return result;
        }
        
        private static string GenerateGhostDialogue(ExtendedGhostPolitician ghost, EncounterType type)
        {
            var profile = ghost.BehaviorProfile;
            
            // Generate dialogue that matches original player's style
            if (profile != null && profile.CatchPhrases.Count > 0)
            {
                // Use their actual phrases!
                return profile.CatchPhrases[UnityEngine.Random.Range(0, profile.CatchPhrases.Count)];
            }
            
            // Default dialogue based on personality
            float aggression = ghost.AIBehavior?.Aggression ?? 50f;
            
            if (aggression > 70)
            {
                return type switch
                {
                    EncounterType.Debate => "You don't have what it takes. Step aside.",
                    EncounterType.Negotiation => "My terms are non-negotiable.",
                    EncounterType.CampaignTrail => "Another pretender. This district is MINE.",
                    _ => "You're in my way."
                };
            }
            else if (aggression < 30)
            {
                return type switch
                {
                    EncounterType.Debate => "I look forward to a civil exchange of ideas.",
                    EncounterType.Negotiation => "Perhaps we can find common ground.",
                    EncounterType.CampaignTrail => "May the best candidate win.",
                    _ => "Good to meet you."
                };
            }
            
            return "Let's see what you've got.";
        }
        
        private static GhostReactionType DetermineGhostReaction(
            ExtendedGhostPolitician ghost, 
            string playerId,
            EncounterType encounterType)
        {
            var behavior = ghost.AIBehavior;
            if (behavior == null) return GhostReactionType.Neutral;
            
            // Check if ghost would be hostile
            if (behavior.WillAttackFirst && behavior.Aggression > 60)
                return GhostReactionType.Hostile;
            
            // Check if ghost would ally
            if (behavior.WillFormAlliances && behavior.Aggression < 40)
                return GhostReactionType.Friendly;
            
            return GhostReactionType.Cautious;
        }
        
        /// <summary>
        /// Challenge a ghost politician directly
        /// </summary>
        public static ChallengeResult ChallengeGhost(
            string playerId,
            ExtendedGhostPolitician ghost,
            ChallengeType challengeType)
        {
            var result = new ChallengeResult();
            
            // Ghost responds based on behavior profile
            if (ghost.AIBehavior?.WillAttackFirst ?? false)
            {
                result.GhostResponse = GhostChallengeResponse.AcceptsAggresively;
                result.GhostCounterMove = "The ghost immediately launches a counter-attack!";
            }
            else if (ghost.AIBehavior?.Defensiveness > 70)
            {
                result.GhostResponse = GhostChallengeResponse.DefendsPosition;
                result.GhostCounterMove = "The ghost digs in and prepares defenses.";
            }
            else
            {
                result.GhostResponse = GhostChallengeResponse.AcceptsCalmly;
                result.GhostCounterMove = "The ghost calmly accepts the challenge.";
            }
            
            return result;
        }
        
        /// <summary>
        /// View a ghost's history (what the original player did)
        /// </summary>
        public static GhostHistoryReport GetGhostHistory(ExtendedGhostPolitician ghost)
        {
            return new GhostHistoryReport
            {
                Name = ghost.CharacterName,
                OriginalPlayerId = ghost.OriginalPlayerId,
                HighestOfficeReached = ghost.SpecificOffice,
                
                // Expose exploitable patterns!
                KnownWeaknesses = ghost.BehaviorProfile?.ExploitablePatterns ?? new List<string>(),
                TypicalMistake = ghost.BehaviorProfile?.GetRandomWeakness() ?? "Unknown",
                PanicThreshold = ghost.BehaviorProfile?.PanicThreshold ?? 0.3f,
                
                // Strategy hints
                PreferredTactics = ghost.BehaviorProfile?.FavoriteTactics ?? new List<string>(),
                WillBetray = ghost.AIBehavior?.WillBetrayAlliances ?? false,
                AggressionLevel = ghost.AIBehavior?.Aggression ?? 50f
            };
        }
    }
    
    #region Interaction Data Structures
    
    public enum EncounterType
    {
        CampaignTrail,
        Debate,
        Negotiation,
        Committee,
        SocialEvent,
        Crisis
    }
    
    public enum GhostReactionType
    {
        Friendly,
        Neutral,
        Cautious,
        Hostile,
        Terrified      // If player has beaten them before
    }
    
    public enum ChallengeType
    {
        ElectionChallenge,
        PolicyDebate,
        PublicConfrontation,
        AlliancePoaching,
        ScandalAttack
    }
    
    public enum GhostChallengeResponse
    {
        AcceptsCalmly,
        AcceptsAggresively,
        DefendsPosition,
        Retreats,
        SeeksAllies,
        UsesDirtyTricks
    }
    
    [Serializable]
    public class GhostEncounterResult
    {
        public string GhostName;
        public string GhostOriginalPlayer;
        public EncounterType EncounterType;
        public string GhostDialogue;
        public GhostReactionType GhostReaction;
        public bool IsOwnGhost;
        public string SpecialMessage;
    }
    
    [Serializable]
    public class ChallengeResult
    {
        public GhostChallengeResponse GhostResponse;
        public string GhostCounterMove;
    }
    
    [Serializable]
    public class GhostHistoryReport
    {
        public string Name;
        public string OriginalPlayerId;
        public string HighestOfficeReached;
        public List<string> KnownWeaknesses;
        public string TypicalMistake;
        public float PanicThreshold;
        public List<string> PreferredTactics;
        public bool WillBetray;
        public float AggressionLevel;
    }
    
    #endregion
}

