using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - Player Behavior Profile
// Tracks how players actually play to create AI "ghosts"
// ═══════════════════════════════════════════════════════════════════════════════

namespace ElectionEmpire.Multiplayer.PersistentWorld
{
    /// <summary>
    /// Profile of how a player actually behaves in-game.
    /// Used to create AI "ghosts" that play like the original player.
    /// </summary>
    [Serializable]
    public class PlayerBehaviorProfile
    {
        // Core Behavior Metrics
        public float DirtyTricksWillingness;    // 0-1: How often YOU used dirty tricks
        public float ScandalResponseStyle;       // 0-1: 0=always deny, 1=always apologize
        public float LoyaltyToAllies;           // 0-1: 0=betray constantly, 1=never betray
        public float RiskTolerance;             // 0-1: 0=cautious, 1=bold/reckless
        public float PanicThreshold;            // 0-1: When you made mistakes (lower = panics sooner)
        
        // Personality Signatures
        public List<string> CatchPhrases;       // Things you actually said!
        public List<string> ExploitablePatterns; // Your weaknesses!
        public List<string> FavoriteTactics;    // Actions you used most
        
        // Decision Patterns
        public Dictionary<string, float> ActionFrequency; // Action type -> frequency (0-1)
        public Dictionary<string, float> ResponsePatterns;  // Situation -> typical response
        
        // Resource Management Style
        public float ResourceAggressiveness;     // 0-1: How aggressively you spend
        public float FundraisingFocus;          // 0-1: How much you prioritize fundraising
        public float PolicyFocus;               // 0-1: How much you prioritize policy
        public float PolicyConsistency;         // 0-1: How consistent your policy positions are
        public float AggressionInDebates;       // 0-1: How aggressive you are in debates
        
        // Relationship Patterns
        public float AllianceFormationRate;     // How often you form alliances
        public float BetrayalRate;              // How often you betray allies
        public float EnemyCreationRate;         // How often you make enemies
        
        // Stress Response
        public Dictionary<float, string> StressResponses; // Stress level -> typical action
        public float AverageStressWhenDeciding; // Average stress when making decisions
        
        // Historical Data
        public int TotalTurnsPlayed;
        public int TotalElectionsWon;
        public int TotalElectionsLost;
        public int TotalScandalsSurvived;
        public int TotalDirtyTricksUsed;
        public int TotalAlliancesFormed;
        public int TotalBetrayals;
        
        // Character Traits (from actual gameplay)
        public string PreferredArchetype;      // What archetype you played like
        public List<string> ReputationTags;      // How you were known
        public float AverageApprovalRating;      // Your typical approval
        public float AveragePollingPosition;     // Where you usually polled
        
        // Weaknesses (exploitable by other players)
        public List<BehaviorWeakness> Weaknesses;
        
        // Strengths (what you're good at)
        public List<BehaviorStrength> Strengths;
        
        // Timeline
        public DateTime ProfileCreated;
        public DateTime LastUpdated;
        public DateTime LastActive;              // When ghost was last used
        
        public PlayerBehaviorProfile()
        {
            CatchPhrases = new List<string>();
            ExploitablePatterns = new List<string>();
            FavoriteTactics = new List<string>();
            ActionFrequency = new Dictionary<string, float>();
            ResponsePatterns = new Dictionary<string, float>();
            StressResponses = new Dictionary<float, string>();
            ReputationTags = new List<string>();
            Weaknesses = new List<BehaviorWeakness>();
            Strengths = new List<BehaviorStrength>();
            ProfileCreated = DateTime.Now;
            LastUpdated = DateTime.Now;
        }
        
        /// <summary>
        /// Update profile based on a decision the player made.
        /// </summary>
        public void RecordDecision(string actionType, string situation, float stressLevel, 
                                   Dictionary<string, object> context)
        {
            TotalTurnsPlayed++;
            LastUpdated = DateTime.Now;
            
            // Update action frequency
            if (!ActionFrequency.ContainsKey(actionType))
                ActionFrequency[actionType] = 0f;
            ActionFrequency[actionType] = (ActionFrequency[actionType] * 0.9f) + 0.1f;
            
            // Update response patterns
            if (!ResponsePatterns.ContainsKey(situation))
                ResponsePatterns[situation] = 0f;
            ResponsePatterns[situation] = (ResponsePatterns[situation] * 0.9f) + 0.1f;
            
            // Track stress response
            float stressBucket = Mathf.Floor(stressLevel * 10f) / 10f;
            if (!StressResponses.ContainsKey(stressBucket))
                StressResponses[stressBucket] = actionType;
            
            // Update average stress
            AverageStressWhenDeciding = (AverageStressWhenDeciding * 0.9f) + (stressLevel * 0.1f);
            
            // Track specific actions
            if (actionType.Contains("dirty") || actionType.Contains("trick") || 
                actionType.Contains("blackmail") || actionType.Contains("bribe"))
            {
                TotalDirtyTricksUsed++;
                DirtyTricksWillingness = (DirtyTricksWillingness * 0.95f) + 0.05f;
            }
            else
            {
                DirtyTricksWillingness = DirtyTricksWillingness * 0.99f;
            }
            
            // Track favorite tactics
            if (!FavoriteTactics.Contains(actionType))
            {
                FavoriteTactics.Add(actionType);
                if (FavoriteTactics.Count > 5)
                    FavoriteTactics.RemoveAt(0);
            }
        }
        
        /// <summary>
        /// Record a catchphrase the player used.
        /// </summary>
        public void RecordCatchphrase(string phrase)
        {
            if (string.IsNullOrEmpty(phrase)) return;
            
            if (!CatchPhrases.Contains(phrase))
            {
                CatchPhrases.Add(phrase);
                if (CatchPhrases.Count > 10)
                    CatchPhrases.RemoveAt(0);
            }
        }
        
        /// <summary>
        /// Record a mistake or exploitable pattern.
        /// </summary>
        public void RecordMistake(string pattern, float severity)
        {
            if (!ExploitablePatterns.Contains(pattern))
            {
                ExploitablePatterns.Add(pattern);
                if (ExploitablePatterns.Count > 10)
                    ExploitablePatterns.RemoveAt(0);
            }
            
            // Update panic threshold based on when mistakes happen
            PanicThreshold = Mathf.Clamp01(PanicThreshold - (severity * 0.01f));
        }
        
        /// <summary>
        /// Record a scandal response.
        /// </summary>
        public void RecordScandalResponse(string responseType)
        {
            TotalScandalsSurvived++;
            
            // Update response style
            float adjustment = responseType.ToLower() switch
            {
                "deny" or "deflect" or "attack" => -0.1f,
                "apologize" or "admit" or "confess" => 0.1f,
                "ignore" or "silence" => 0f,
                _ => 0f
            };
            
            ScandalResponseStyle = Mathf.Clamp01(ScandalResponseStyle + adjustment);
        }
        
        /// <summary>
        /// Record an alliance action.
        /// </summary>
        public void RecordAllianceAction(bool formed, bool betrayed)
        {
            if (formed)
            {
                TotalAlliancesFormed++;
                AllianceFormationRate = (AllianceFormationRate * 0.9f) + 0.1f;
            }
            else
            {
                AllianceFormationRate = AllianceFormationRate * 0.99f;
            }
            
            if (betrayed)
            {
                TotalBetrayals++;
                BetrayalRate = (BetrayalRate * 0.9f) + 0.1f;
                LoyaltyToAllies = Mathf.Clamp01(LoyaltyToAllies - 0.1f);
            }
            else
            {
                BetrayalRate = BetrayalRate * 0.99f;
                LoyaltyToAllies = Mathf.Clamp01(LoyaltyToAllies + 0.01f);
            }
        }
        
        /// <summary>
        /// Get how likely this profile is to take a specific action.
        /// </summary>
        public float GetActionProbability(string actionType, string situation, float currentStress)
        {
            float baseProb = ActionFrequency.TryGetValue(actionType, out float freq) ? freq : 0.1f;
            
            // Modify by situation
            if (ResponsePatterns.TryGetValue(situation, out float response))
            {
                baseProb = (baseProb + response) / 2f;
            }
            
            // Modify by stress
            if (currentStress > PanicThreshold)
            {
                // More likely to make mistakes under stress
                baseProb *= 1.2f;
            }
            
            // Modify by risk tolerance
            if (actionType.Contains("risky") || actionType.Contains("bold"))
            {
                baseProb *= RiskTolerance;
            }
            else if (actionType.Contains("safe") || actionType.Contains("cautious"))
            {
                baseProb *= (1f - RiskTolerance);
            }
            
            return Mathf.Clamp01(baseProb);
        }
        
        /// <summary>
        /// Get a catchphrase this player would say.
        /// </summary>
        public string GetRandomCatchphrase()
        {
            if (CatchPhrases.Count == 0) return null;
            return CatchPhrases[UnityEngine.Random.Range(0, CatchPhrases.Count)];
        }
        
        /// <summary>
        /// Get an exploitable weakness.
        /// </summary>
        public string GetRandomWeakness()
        {
            if (ExploitablePatterns.Count == 0) return null;
            return ExploitablePatterns[UnityEngine.Random.Range(0, ExploitablePatterns.Count)];
        }
    }
    
    [Serializable]
    public class BehaviorWeakness
    {
        public string Description;
        public string ExploitMethod;
        public float Severity; // 0-1
        public int TimesExploited;
    }
    
    [Serializable]
    public class BehaviorStrength
    {
        public string Description;
        public string Advantage;
        public float Effectiveness; // 0-1
        public int TimesUsed;
    }
}

