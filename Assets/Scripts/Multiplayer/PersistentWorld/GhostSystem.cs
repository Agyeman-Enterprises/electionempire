using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.AI;
using ElectionEmpire.Multiplayer.PersistentWorld;

// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - Ghost System
// AI opponents based on real player behavior profiles
// ═══════════════════════════════════════════════════════════════════════════════

namespace ElectionEmpire.Multiplayer.PersistentWorld
{
    /// <summary>
    /// A "ghost" - an AI opponent based on a real player's behavior profile.
    /// Makes decisions like the original player would.
    /// </summary>
    [Serializable]
    public class GhostPolitician
    {
        public string GhostId;
        public string PlayerId;              // Original player
        public string PlayerName;
        public PlayerBehaviorProfile BehaviorProfile;
        
        // Ghost State
        public string CurrentRole;           // "Former President", "Elder Statesman", etc.
        public int YearsActive;
        public float Influence;              // How much they affect the world
        public bool IsActive;                // Can appear as opponent
        
        // Ghost Personality (derived from behavior profile)
        public PersonalityMatrix Personality;
        public AIStrategy PreferredStrategy;
        public Archetype DerivedArchetype;
        
        public GhostPolitician()
        {
            GhostId = Guid.NewGuid().ToString("N").Substring(0, 8);
            IsActive = true;
        }
        
        /// <summary>
        /// Convert behavior profile to AI personality matrix.
        /// </summary>
        public void GeneratePersonalityFromProfile()
        {
            if (BehaviorProfile == null) return;
            
            Personality = new PersonalityMatrix
            {
                EthicalFlexibility = BehaviorProfile.DirtyTricksWillingness * 100f,
                RiskTolerance = BehaviorProfile.RiskTolerance * 100f,
                Loyalty = BehaviorProfile.LoyaltyToAllies * 100f,
                Aggression = BehaviorProfile.ActionFrequency.ContainsKey("attack") 
                    ? BehaviorProfile.ActionFrequency["attack"] * 100f 
                    : 50f,
                Adaptability = 70f, // Ghosts are somewhat adaptable
                Charisma = 60f,     // Based on original player's charisma
                Cunning = BehaviorProfile.DirtyTricksWillingness * 80f,
                Impulsiveness = (1f - BehaviorProfile.PanicThreshold) * 100f
            };
            
            // Determine strategy
            if (BehaviorProfile.DirtyTricksWillingness > 0.7f)
                PreferredStrategy = AIStrategy.Aggressive;
            else if (BehaviorProfile.LoyaltyToAllies > 0.7f)
                PreferredStrategy = AIStrategy.Cooperative;
            else if (BehaviorProfile.RiskTolerance < 0.3f)
                PreferredStrategy = AIStrategy.Defensive;
            else
                PreferredStrategy = AIStrategy.Strategic;
            
            // Determine archetype
            DerivedArchetype = DetermineArchetype();
        }
        
        private Archetype DetermineArchetype()
        {
            if (BehaviorProfile.DirtyTricksWillingness > 0.8f)
                return Archetype.Survivor;
            if (BehaviorProfile.PolicyFocus > 0.7f)
                return Archetype.Technocrat;
            if (BehaviorProfile.LoyaltyToAllies < 0.3f)
                return Archetype.Maverick;
            if (BehaviorProfile.FundraisingFocus > 0.7f)
                return Archetype.Corporate;
            
            return Archetype.Insider;
        }
        
        /// <summary>
        /// Make a decision like the original player would.
        /// </summary>
        public string MakeDecision(string situation, float currentStress)
        {
            if (BehaviorProfile == null) return "wait";
            
            // Get probabilities for each action
            var actionScores = new Dictionary<string, float>();
            
            foreach (var action in BehaviorProfile.ActionFrequency.Keys)
            {
                float prob = BehaviorProfile.GetActionProbability(action, situation, currentStress);
                actionScores[action] = prob;
            }
            
            // Select action based on probabilities
            if (actionScores.Count == 0) return "wait";
            
            float total = actionScores.Values.Sum();
            float random = UnityEngine.Random.Range(0f, total);
            
            foreach (var kvp in actionScores.OrderByDescending(x => x.Value))
            {
                random -= kvp.Value;
                if (random <= 0)
                {
                    return kvp.Key;
                }
            }
            
            return actionScores.OrderByDescending(x => x.Value).First().Key;
        }
        
        /// <summary>
        /// Get a catchphrase this ghost would say.
        /// </summary>
        public string GetCatchphrase()
        {
            return BehaviorProfile?.GetRandomCatchphrase() ?? "I have a plan.";
        }
        
        /// <summary>
        /// Get an exploitable weakness.
        /// </summary>
        public string GetWeakness()
        {
            return BehaviorProfile?.GetRandomWeakness() ?? "Overconfidence";
        }
    }
    
    /// <summary>
    /// A ghost that was a former president.
    /// </summary>
    [Serializable]
    public class GhostPresident : GhostPolitician
    {
        public int TermStartYear;
        public int TermEndYear;
        public float Legacy;                 // -100 to 100
        public string Reputation;            // "Corrupt", "Reformer", etc.
        public List<string> PoliciesEnacted;
        public int ScandalsSurvived;
        
        public GhostPresident()
        {
            CurrentRole = "Former President";
            Influence = 80f; // High influence
        }
    }
    
    /// <summary>
    /// Manages all ghosts in the persistent world.
    /// </summary>
    public class GhostManager
    {
        private PersistentWorldState _worldState;
        private Dictionary<string, AIOpponent> _ghostOpponents;
        
        public GhostManager(PersistentWorldState worldState)
        {
            _worldState = worldState;
            _ghostOpponents = new Dictionary<string, AIOpponent>();
        }
        
        /// <summary>
        /// Create an AI opponent from a ghost.
        /// </summary>
        public AIOpponent CreateOpponentFromGhost(GhostPolitician ghost, int currentYear)
        {
            if (_ghostOpponents.TryGetValue(ghost.GhostId, out var existing))
                return existing;
            
            // Generate personality if not done
            if (ghost.Personality == null)
            {
                ghost.GeneratePersonalityFromProfile();
            }
            
            // Create AI opponent
            var opponent = new AIOpponent
            {
                ID = ghost.GhostId,
                Name = ghost.PlayerName,
                GeneratedNickname = GetGhostNickname(ghost),
                Personality = ghost.Personality,
                CurrentStrategy = ghost.PreferredStrategy,
                Archetype = ghost.DerivedArchetype,
                Difficulty = AIDifficulty.Adaptive, // Ghosts adapt
                ApprovalRating = ghost.BehaviorProfile?.AverageApprovalRating ?? 50f
            };
            
            // Add ghost-specific traits
            opponent.SignatureMoves = new List<string>(ghost.BehaviorProfile?.FavoriteTactics ?? new List<string>());
            
            // Add catchphrases to backstory
            if (ghost.BehaviorProfile?.CatchPhrases.Count > 0)
            {
                opponent.Backstory = $"Known for saying: \"{string.Join("\", \"", ghost.BehaviorProfile.CatchPhrases.Take(3))}\"";
            }
            
            _ghostOpponents[ghost.GhostId] = opponent;
            
            return opponent;
        }
        
        /// <summary>
        /// Get ghosts that should appear as opponents for a new player.
        /// </summary>
        public List<AIOpponent> GetOpponentsForNewPlayer(int currentYear, int officeTier)
        {
            var opponents = new List<AIOpponent>();
            
            // Get active ghosts
            var activeGhosts = _worldState.GetActiveGhosts(currentYear);
            
            // Filter by relevance
            var relevantGhosts = activeGhosts.Where(g =>
            {
                if (g is GhostPresident)
                    return officeTier >= 3; // Presidents only appear in high-tier races
                
                return true;
            }).ToList();
            
            // Select 2-4 ghosts to appear
            int count = Mathf.Clamp(relevantGhosts.Count, 2, 4);
            var selected = relevantGhosts
                .OrderByDescending(g => g.Influence)
                .Take(count)
                .ToList();
            
            foreach (var ghost in selected)
            {
                var opponent = CreateOpponentFromGhost(ghost, currentYear);
                opponents.Add(opponent);
            }
            
            return opponents;
        }
        
        private string GetGhostNickname(GhostPolitician ghost)
        {
            if (ghost is GhostPresident president)
            {
                if (president.Legacy < -50f)
                    return "The Disgraced";
                if (president.Legacy > 50f)
                    return "The Great";
                if (president.ScandalsSurvived > 5)
                    return "The Survivor";
            }
            
            if (ghost.BehaviorProfile?.DirtyTricksWillingness > 0.7f)
                return "The Schemer";
            if (ghost.BehaviorProfile?.LoyaltyToAllies < 0.3f)
                return "The Betrayer";
            if (ghost.BehaviorProfile?.RiskTolerance < 0.3f)
                return "The Cautious";
            
            return "The Ghost";
        }
        
        /// <summary>
        /// Study a ghost to learn their weaknesses.
        /// </summary>
        public GhostStudyResult StudyGhost(GhostPolitician ghost)
        {
            var result = new GhostStudyResult
            {
                GhostId = ghost.GhostId,
                GhostName = ghost.PlayerName,
                Weaknesses = new List<string>(ghost.BehaviorProfile?.ExploitablePatterns ?? new List<string>()),
                FavoriteTactics = new List<string>(ghost.BehaviorProfile?.FavoriteTactics ?? new List<string>()),
                Catchphrases = new List<string>(ghost.BehaviorProfile?.CatchPhrases ?? new List<string>()),
                RiskTolerance = ghost.BehaviorProfile?.RiskTolerance ?? 0.5f,
                PanicThreshold = ghost.BehaviorProfile?.PanicThreshold ?? 0.5f
            };
            
            return result;
        }
    }
    
    [Serializable]
    public class GhostStudyResult
    {
        public string GhostId;
        public string GhostName;
        public List<string> Weaknesses;
        public List<string> FavoriteTactics;
        public List<string> Catchphrases;
        public float RiskTolerance;
        public float PanicThreshold;
    }
}

