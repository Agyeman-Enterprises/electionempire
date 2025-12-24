using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.Character;
using ElectionEmpire.World;

namespace ElectionEmpire.AI
{
    /// <summary>
    /// Represents an AI opponent in the game
    /// </summary>
    [Serializable]
    public class AIOpponent
    {
        // Identity
        public string ID;
        public string Name;
        public string GeneratedNickname;      // "The Silver-Tongued Lawyer"
        public Archetype Archetype;
        public Character Character;            // Uses same Character class as player
        
        // Personality (drives behavior)
        public PersonalityMatrix Personality;
        
        // State
        public Office CurrentOffice;
        public Dictionary<string, float> Resources; // Same as player resources
        public List<string> Relationships;     // IDs of allies/enemies
        public List<string> ActiveScandals;
        public Dictionary<VoterBloc, float> VoterBlocSupport;
        public Dictionary<Issue, float> PolicyStances;
        
        // History
        public string Backstory;               // Procedurally generated
        public List<string> SignatureMoves;    // 3 signature tactics
        public List<AIAction> ActionHistory;   // What they've done
        public List<string> Storylines;        // Narrative arcs they're in
        
        // Behavior
        public AIDifficulty Difficulty;
        public AIStrategy CurrentStrategy;
        public Dictionary<string, float> Goals; // What they're trying to achieve
        
        // Performance tracking
        public float ApprovalRating;
        public int TurnsInOffice;
        public int ElectionsWon;
        public int ElectionsLost;
        
        public AIOpponent()
        {
            Relationships = new List<string>();
            ActiveScandals = new List<string>();
            VoterBlocSupport = new Dictionary<VoterBloc, float>();
            PolicyStances = new Dictionary<Issue, float>();
            SignatureMoves = new List<string>();
            ActionHistory = new List<AIAction>();
            Storylines = new List<string>();
            Resources = new Dictionary<string, float>();
            Goals = new Dictionary<string, float>();
        }
    }
    
    [Serializable]
    public enum Archetype
    {
        Idealist,        // Honest, policy-focused, avoids dirty tricks
        MachineBoss,     // Controls party apparatus, machine politics
        Populist,        // Appeals to masses, anti-establishment
        Technocrat,      // Data-driven, expert, policy wonk
        Showman,         // Media-savvy, entertainment value
        Insider,         // Connected, establishment, traditional
        Maverick,        // Unpredictable, breaks norms, chaotic
        DynastyHeir,     // Legacy connections, inherited advantages
        Zealot,          // Ideological purist, uncompromising
        Corporate,       // Business-backed, pro-industry
        Revolutionary,   // Wants systemic change, radical
        Survivor         // Adaptable, does whatever it takes
    }
    
    [Serializable]
    public class PersonalityMatrix
    {
        // Each trait 0-100
        public float Aggression;           // 0=passive, 100=attacks constantly
        public float RiskTolerance;        // 0=cautious, 100=reckless
        public float EthicalFlexibility;   // 0=clean, 100=dirty tricks
        public float Loyalty;              // 0=betrays, 100=keeps allies
        public float Adaptability;         // 0=rigid, 100=flexible
        public float Charisma;             // 0=boring, 100=captivating
        public float Cunning;              // 0=direct, 100=scheming
        public float Impulsiveness;        // 0=calculated, 100=spontaneous
        
        // Human foibles - traits that make AI unpredictable and flawed
        public float Ego;                  // 0=humble, 100=arrogant (Napoleon, Caesar)
        public float Paranoia;             // 0=trusting, 100=paranoid (Stalin-like)
        public float Hubris;               // 0=realistic, 100=overconfident (leads to downfall)
        public float EmotionalVolatility;  // 0=stable, 100=unstable (mood swings)
        public float Obsession;             // 0=balanced, 100=obsessive (single-minded focus)
        public float Pride;                // 0=flexible, 100=stubborn (won't back down)
        
        /// <summary>
        /// Calculate overall complexity score
        /// </summary>
        public int ComplexityScore()
        {
            // More extreme = more complex/interesting
            float totalDeviation = 0;
            totalDeviation += Mathf.Abs(Aggression - 50);
            totalDeviation += Mathf.Abs(RiskTolerance - 50);
            totalDeviation += Mathf.Abs(EthicalFlexibility - 50);
            totalDeviation += Mathf.Abs(Loyalty - 50);
            return (int)(totalDeviation / 10);
        }
        
        /// <summary>
        /// Calculate "human foible" score - how likely to make irrational decisions
        /// </summary>
        public float GetFoibleScore()
        {
            // Higher foible score = more unpredictable, more human-like mistakes
            float foibles = 0f;
            foibles += Ego * 0.2f;
            foibles += Paranoia * 0.15f;
            foibles += Hubris * 0.2f;
            foibles += EmotionalVolatility * 0.25f;
            foibles += Obsession * 0.1f;
            foibles += Pride * 0.1f;
            return Mathf.Clamp(foibles / 100f, 0f, 1f);
        }
    }
    
    [Serializable]
    public enum AIDifficulty
    {
        Easy,       // Makes mistakes, predictable
        Normal,     // Competent, reasonable challenge
        Hard,       // Smart, aggressive, few mistakes
        Adaptive    // Learns from player, scales difficulty
    }
    
    [Serializable]
    public enum AIStrategy
    {
        Aggressive,      // Attack opponents, negative campaigning
        Defensive,       // Build support, avoid conflict
        Opportunistic,   // Strike when weak, avoid when strong
        Cooperative,     // Build alliances, help others
        Chaotic,         // Unpredictable, disruptive
        Strategic        // Long-term planning, calculated
    }
    
    [Serializable]
    public class AIAction
    {
        public string ActionType;        // "Attack", "Alliance", "Policy", etc.
        public string Target;            // Who/what affected
        public DateTime Timestamp;
        public bool Successful;
        public float ImpactScore;        // How significant was this
        
        // For decision-making
        public DecisionType Type;
        public string Subtype;
    }
    
    [Serializable]
    public enum DecisionType
    {
        Policy,          // Implement policy
        Campaign,        // Campaign action (rally, ads, etc.)
        Attack,          // Negative attack on opponent
        Alliance,        // Form or break alliance
        Scandal,         // Use dirty trick, scandal creation
        Crisis,          // Respond to crisis
        Resource,        // Allocate resources
        Media           // Media action (press conference, interview)
    }
    
    [Serializable]
    public class Office
    {
        public string Name;
        public int Tier;
        public string RegionID;
        public string StateID;
        public string DistrictID;
    }
}

