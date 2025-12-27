using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.InfiniteUniverse;

// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - Ghost AI System
// AI system that emulates player behavior for ghost politicians
// ═══════════════════════════════════════════════════════════════════════════════

namespace ElectionEmpire.InfiniteUniverse
{
    /// <summary>
    /// AI system that emulates player behavior for ghost politicians
    /// </summary>
    public class GhostAISystem : MonoBehaviour
    {
        public static GhostAISystem Instance { get; private set; }
        
        // Configuration
        public float DecisionRandomness = 0.2f;     // How much random variation
        public float LearningRate = 0.1f;           // How quickly ghosts adapt
        
        // Tracking
        private Dictionary<string, List<GhostDecision>> _decisionHistory;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _decisionHistory = new Dictionary<string, List<GhostDecision>>();
        }
        
        /// <summary>
        /// Process a ghost politician's turn when player is offline
        /// </summary>
        public GhostDecision ProcessGhostDecision(ExtendedGhostPolitician ghost, ExtendedWorldState world)
        {
            var decision = new GhostDecision
            {
                GhostId = ghost.Id,
                Timestamp = DateTime.UtcNow,
                WorldState = SnapshotWorldState(world)
            };
            
            // Determine what situation the ghost is in
            var situation = AnalyzeSituation(ghost, world);
            
            // Check behavior profile for learned responses
            if (ghost.BehaviorProfile != null)
            {
                decision.Action = DetermineActionFromProfile(ghost, situation);
                decision.Reasoning = $"Based on learned player behavior: {ghost.AIBehavior?.PreferredPath ?? 0.5f}";
            }
            else
            {
                decision.Action = DetermineDefaultAction(ghost, situation);
                decision.Reasoning = "Default AI behavior";
            }
            
            // Add some randomness
            if (UnityEngine.Random.value < DecisionRandomness)
            {
                decision.Action = AddRandomVariation(decision.Action, ghost);
                decision.Reasoning += " (with random variation)";
            }
            
            // Execute the action
            ExecuteGhostAction(ghost, decision, world);
            
            // Record for analysis
            TrackDecision(ghost.Id, decision);
            
            return decision;
        }
        
        /// <summary>
        /// Analyze what situation the ghost is facing
        /// </summary>
        private GhostSituation AnalyzeSituation(ExtendedGhostPolitician ghost, ExtendedWorldState world)
        {
            var situation = new GhostSituation
            {
                CurrentTier = ghost.CurrentOffice,
                ApprovalRating = ghost.CurrentApproval,
                IsUnderAttack = ghost.ActiveScandals.Count > 0,
                HasOpportunity = false, // Would check for openings
                CrisisActive = false
            };
            
            // Check if there's an election coming
            situation.ElectionIncoming = IsElectionComing(ghost, world);
            
            // Check if higher office is available
            situation.HigherOfficeAvailable = IsHigherOfficeAvailable(ghost, world);
            
            // Check for threats
            var threats = world.ActivePoliticians
                .Where(g => g.CurrentOffice >= ghost.CurrentOffice && g.Id != ghost.Id)
                .ToList();
            situation.CompetitorCount = threats.Count;
            
            return situation;
        }
        
        /// <summary>
        /// Determine action based on learned player behavior profile
        /// </summary>
        private GhostActionType DetermineActionFromProfile(ExtendedGhostPolitician ghost, GhostSituation situation)
        {
            var profile = ghost.BehaviorProfile;
            var behavior = ghost.AIBehavior;
            
            if (profile == null || behavior == null)
                return DetermineDefaultAction(ghost, situation);
            
            // Crisis handling
            if (situation.CrisisActive)
            {
                if (behavior.CrisisCompetence > 60)
                    return GhostActionType.HandleCrisis;
                else
                    return GhostActionType.DeflectBlame;
            }
            
            // Election mode
            if (situation.ElectionIncoming)
            {
                if (profile.RiskTolerance > 0.6f && profile.DirtyTricksWillingness > 0.5f)
                    return GhostActionType.AggressiveCampaign;
                else
                    return GhostActionType.SafeCampaign;
            }
            
            // Opportunity seeking
            if (situation.HigherOfficeAvailable)
            {
                if (profile.RiskTolerance > 0.5f && ghost.CurrentApproval > 50)
                    return GhostActionType.SeekHigherOffice;
            }
            
            // Under attack
            if (situation.IsUnderAttack)
            {
                if (behavior.Aggression > 60)
                    return GhostActionType.CounterAttack;
                else if (profile.ScandalResponseStyle < 0.4f)
                    return GhostActionType.Deny;
                else
                    return GhostActionType.Apologize;
            }
            
            // Default: build strength
            if (behavior.WillFormAlliances && ghost.CurrentApproval < 50)
                return GhostActionType.BuildAlliances;
            
            if (behavior.OpportunismLevel > 60)
                return GhostActionType.SeekOpportunity;
            
            return GhostActionType.MaintainPosition;
        }
        
        /// <summary>
        /// Default AI behavior when no profile exists
        /// </summary>
        private GhostActionType DetermineDefaultAction(ExtendedGhostPolitician ghost, GhostSituation situation)
        {
            // Simple rule-based AI
            if (situation.CrisisActive) return GhostActionType.HandleCrisis;
            if (situation.ElectionIncoming) return GhostActionType.SafeCampaign;
            if (situation.IsUnderAttack) return GhostActionType.Defend;
            if (situation.HigherOfficeAvailable && ghost.CurrentApproval > 55)
                return GhostActionType.SeekHigherOffice;
            
            return GhostActionType.MaintainPosition;
        }
        
        /// <summary>
        /// Add random variation to make ghost less predictable
        /// </summary>
        private GhostActionType AddRandomVariation(GhostActionType baseAction, ExtendedGhostPolitician ghost)
        {
            var allActions = Enum.GetValues(typeof(GhostActionType)).Cast<GhostActionType>().ToList();
            
            // 70% chance to keep original, 30% chance to do something random
            if (UnityEngine.Random.value > 0.3f)
                return baseAction;
            
            return allActions[UnityEngine.Random.Range(0, allActions.Count)];
        }
        
        /// <summary>
        /// Execute the ghost's chosen action
        /// </summary>
        private void ExecuteGhostAction(ExtendedGhostPolitician ghost, GhostDecision decision, ExtendedWorldState world)
        {
            switch (decision.Action)
            {
                case GhostActionType.MaintainPosition:
                    // Small approval maintenance
                    ghost.CurrentApproval += UnityEngine.Random.Range(-2f, 3f);
                    break;
                    
                case GhostActionType.SeekHigherOffice:
                    // Attempt to advance
                    AttemptOfficeAdvancement(ghost, world);
                    break;
                    
                case GhostActionType.AggressiveCampaign:
                    // Big swings, big risks
                    ghost.CurrentApproval += UnityEngine.Random.Range(-10f, 15f);
                    break;
                    
                case GhostActionType.SafeCampaign:
                    // Steady progress
                    ghost.CurrentApproval += UnityEngine.Random.Range(0f, 5f);
                    break;
                    
                case GhostActionType.AttackOpponent:
                    // Find and attack another politician
                    ExecuteGhostAttack(ghost, world);
                    break;
                    
                case GhostActionType.BuildAlliances:
                    // Improve relationships
                    break;
                    
                case GhostActionType.HandleCrisis:
                    // Crisis management
                    ghost.CurrentApproval += UnityEngine.Random.Range(-5f, 10f);
                    break;
                    
                case GhostActionType.DeflectBlame:
                    // Risky deflection
                    ghost.CurrentApproval += UnityEngine.Random.Range(-15f, 5f);
                    break;
                    
                case GhostActionType.UseDirtyTrick:
                    // Dirty politics
                    ExecuteGhostDirtyTrick(ghost, world);
                    break;
            }
            
            // Clamp approval
            ghost.CurrentApproval = Mathf.Clamp(ghost.CurrentApproval, 0f, 100f);
        }
        
        private void AttemptOfficeAdvancement(ExtendedGhostPolitician ghost, ExtendedWorldState world)
        {
            // Check if advancement succeeds
            float successChance = ghost.CurrentApproval / 100f * 0.5f + 0.2f;
            
            if (UnityEngine.Random.value < successChance)
            {
                ghost.CurrentOffice++;
                ghost.SpecificOffice = GetNextOffice(ghost.CurrentOffice);
                ghost.CurrentApproval = 50f; // Reset for new position
                
                Debug.Log($"Ghost {ghost.CharacterName} advanced to {ghost.SpecificOffice}!");
            }
        }
        
        private void ExecuteGhostAttack(ExtendedGhostPolitician ghost, ExtendedWorldState world)
        {
            // Find a target
            var targets = world.ActivePoliticians
                .Where(g => g.Id != ghost.Id && g.CurrentOffice >= ghost.CurrentOffice - 1)
                .ToList();
            
            if (targets.Count == 0) return;
            
            var target = targets[UnityEngine.Random.Range(0, targets.Count)];
            
            // Attack effectiveness based on ghost's profile
            float effectiveness = ghost.BehaviorProfile?.AggressionInDebates ?? 50f;
            effectiveness *= UnityEngine.Random.Range(0.5f, 1.5f);
            
            // Apply damage
            target.CurrentApproval -= effectiveness * 0.1f;
            
            Debug.Log($"Ghost {ghost.CharacterName} attacked {target.CharacterName}!");
        }
        
        private void ExecuteGhostDirtyTrick(ExtendedGhostPolitician ghost, ExtendedWorldState world)
        {
            // Risk of backfire
            if (UnityEngine.Random.value < 0.3f)
            {
                // Backfired!
                ghost.CurrentApproval -= 15f;
                ghost.ActiveScandals.Add("Dirty tricks exposed!");
            }
            else
            {
                // Success - damage an opponent
                var targets = world.ActivePoliticians
                    .Where(g => g.Id != ghost.Id)
                    .ToList();
                
                if (targets.Count > 0)
                {
                    var target = targets[UnityEngine.Random.Range(0, targets.Count)];
                    target.ActiveScandals.Add($"Scandal planted by {ghost.CharacterName}");
                    target.CurrentApproval -= 20f;
                }
            }
        }
        
        private void TrackDecision(string ghostId, GhostDecision decision)
        {
            if (!_decisionHistory.ContainsKey(ghostId))
                _decisionHistory[ghostId] = new List<GhostDecision>();
            
            _decisionHistory[ghostId].Add(decision);
            
            // Limit history size
            if (_decisionHistory[ghostId].Count > 100)
                _decisionHistory[ghostId].RemoveAt(0);
        }
        
        private WorldStateSnapshot SnapshotWorldState(ExtendedWorldState world)
        {
            return new WorldStateSnapshot
            {
                Year = world.YearInWorld,
                Polarization = world.OverallPolarization,
                ActivePoliticianCount = world.ActivePoliticians.Count
            };
        }
        
        private bool IsElectionComing(ExtendedGhostPolitician ghost, ExtendedWorldState world)
        {
            // Elections happen every 2-4 years depending on office
            return UnityEngine.Random.value < 0.2f; // Simplified
        }
        
        private bool IsHigherOfficeAvailable(ExtendedGhostPolitician ghost, ExtendedWorldState world)
        {
            return ghost.CurrentOffice < 5 && UnityEngine.Random.value < 0.3f;
        }
        
        private string GetNextOffice(int tier)
        {
            return tier switch
            {
                2 => "Mayor",
                3 => "State Senator",
                4 => "Governor",
                5 => "President",
                _ => "Higher Office"
            };
        }
    }
    
    #region Ghost AI Data Structures
    
    public enum GhostActionType
    {
        // Maintenance
        MaintainPosition,
        BuildSupport,
        BuildAlliances,
        
        // Advancement
        SeekHigherOffice,
        SeekOpportunity,
        
        // Campaign
        SafeCampaign,
        AggressiveCampaign,
        
        // Combat
        AttackOpponent,
        CounterAttack,
        Defend,
        
        // Crisis
        HandleCrisis,
        DeflectBlame,
        
        // Response
        Deny,
        Apologize,
        Spin,
        
        // Dirty
        UseDirtyTrick,
        PlantScandal,
        BribeOfficial,
        
        // Special
        CallForUnity,
        MakeGrandGesture,
        Retire
    }
    
    [Serializable]
    public class GhostSituation
    {
        public int CurrentTier;
        public float ApprovalRating;
        public bool IsUnderAttack;
        public bool HasOpportunity;
        public bool CrisisActive;
        public bool ElectionIncoming;
        public bool HigherOfficeAvailable;
        public int CompetitorCount;
    }
    
    [Serializable]
    public class GhostDecision
    {
        public string GhostId;
        public DateTime Timestamp;
        public GhostActionType Action;
        public string Reasoning;
        public WorldStateSnapshot WorldState;
        public float OutcomeScore;

        // Additional properties for compatibility
        public string Decision { get { return Action.ToString(); } set { } }
        public string Context { get { return Reasoning; } set { Reasoning = value; } }
    }
    
    [Serializable]
    public class WorldStateSnapshot
    {
        public int Year;
        public float Polarization;
        public int ActivePoliticianCount;
    }
    
    #endregion
}

