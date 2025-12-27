using System;
using System.Collections.Generic;
using UnityEngine;
using ElectionEmpire.World;
using ElectionEmpire.Gameplay.Presidency;
using ElectionEmpire.Multiplayer.PersistentWorld;
using ElectionEmpire.AI;
using ElectionEmpire.Core;

// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - Infinite Universe Manager
// Orchestrates the three phases: Climb → Throne → Legacy
// ═══════════════════════════════════════════════════════════════════════════════

namespace ElectionEmpire.Gameplay.Presidency
{
    /// <summary>
    /// Main orchestrator for the infinite political universe.
    /// Manages transitions between the three phases.
    /// </summary>
    public class InfiniteUniverseManager : MonoBehaviour
    {
        public static InfiniteUniverseManager Instance { get; private set; }
        
        public GamePhase CurrentPhase { get; private set; }
        
        // Phase Managers
        private ThronePhaseManager _throneManager;
        private LegacyPhaseManager _legacyManager;
        private PersistentWorldManager _worldManager;
        
        // Current Player
        private PlayerState _currentPlayer;
        
        // Events
        public event Action<GamePhase> OnPhaseChanged;
        public event Action<PlayerState> OnPresidencyWon;
        public event Action<PlayerState, TermEndReason> OnPresidencyEnded;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            CurrentPhase = GamePhase.TheClimb;
            
            // Initialize managers
            _throneManager = ThronePhaseManager.Instance ?? gameObject.AddComponent<ThronePhaseManager>();
            _legacyManager = LegacyPhaseManager.Instance ?? gameObject.AddComponent<LegacyPhaseManager>();
            _worldManager = PersistentWorldManager.Instance ?? gameObject.AddComponent<PersistentWorldManager>();
        }
        
        /// <summary>
        /// Called when player wins the presidency - transition to Throne phase
        /// </summary>
        public void HandlePresidencyWon(PlayerState president, int termNumber)
        {
            _currentPlayer = president;
            CurrentPhase = GamePhase.TheThrone;
            
            // Begin throne phase
            _throneManager?.BeginPresidency(president, termNumber);
            
            OnPresidencyWon?.Invoke(president);
            OnPhaseChanged?.Invoke(CurrentPhase);
            
            Debug.Log($"[InfiniteUniverse] {president.Character.Name} won! Entering Throne Phase");
        }
        
        /// <summary>
        /// Called when presidency ends - transition to Legacy phase
        /// </summary>
        public void HandlePresidencyEnded(PlayerState formerPresident, TermEndReason reason)
        {
            CurrentPhase = GamePhase.TheLegacy;
            
            // Create term record
            var termRecord = CreateTermRecord(formerPresident, reason);
            
            // Begin legacy phase
            _legacyManager?.BeginLegacyPhase(formerPresident, termRecord);
            
            OnPresidencyEnded?.Invoke(formerPresident, reason);
            OnPhaseChanged?.Invoke(CurrentPhase);
            
            Debug.Log($"[InfiniteUniverse] {formerPresident.Character.Name}'s presidency ended. Entering Legacy Phase");
            
            // After legacy phase, player can start a new climb or continue as ghost
            // The world continues, new players can start
        }
        
        /// <summary>
        /// Get opponents for a player in The Climb phase.
        /// Includes ghosts and active players.
        /// </summary>
        public List<AIOpponent> GetOpponentsForClimb(PlayerState player, int officeTier)
        {
            var opponents = new List<AIOpponent>();
            
            // Get ghosts from persistent world
            if (_worldManager != null)
            {
                var ghostOpponents = _worldManager.GetOpponentsForPlayer(
                    player.Character.Name, officeTier);
                opponents.AddRange(ghostOpponents);
            }
            
            // Get active players in same world (if multiplayer)
            // Would query multiplayer system for active players
            
            // Generate regular AI opponents
            // Would use existing AI generation system
            
            return opponents;
        }
        
        /// <summary>
        /// Check if the current president is a real player or ghost
        /// </summary>
        public bool IsCurrentPresidentActivePlayer(string officeId)
        {
            if (_worldManager == null) return false;
            
            // Check if office is held by active player
            var worldState = _worldManager.WorldState;
            if (worldState?.CurrentOfficeHolders == null) return false;
            
            // Check if office holder is an active player
            if (worldState.CurrentOfficeHolders.TryGetValue(officeId, out var holderId))
            {
                // Check if holder is actively controlled
                var ghosts = worldState.GetActiveGhosts(worldState.CurrentYear);
                var ghost = ghosts.FirstOrDefault(g => g.GhostId == holderId);
                return ghost == null; // If no ghost found, it's an active player
            }
            
            return false;
        }
        
        /// <summary>
        /// Get the current world state description for new players
        /// </summary>
        public string GetWorldIntroduction()
        {
            if (_worldManager == null)
                return "Welcome to Election Empire. The world is new.";
            
            return _worldManager.GetWorldStateDescription();
        }
        
        private PlayerTermRecord CreateTermRecord(PlayerState president, TermEndReason reason)
        {
            var policies = GetPolicies(president);
            var policyImpact = CalculatePolicyImpact(president);
            
            return new PlayerTermRecord
            {
                PlayerId = president.Character.Name,
                PlayerName = president.Character.Name,
                TermStartYear = president.TermStartDate.Year,
                TermEndYear = president.TermEndDate.Year > 0 ? president.TermEndDate.Year : DateTime.Now.Year,
                HighestOfficeTier = 5, // President
                HighestOffice = "President",
                CorruptionLevel = CalculateCorruption(president),
                ScandalsCount = president.ScandalHistory.Count,
                PoliciesImplemented = policies,
                PolicyImpact = policyImpact,
                Legacy = CalculateLegacy(president),
                ReputationTags = new List<string>(president.ReputationTags),
                BehaviorProfile = BehaviorTracker.Instance?.GetProfile(president.Character.Name),
                TermEndReason = reason
            };
        }
        
        private float CalculateCorruption(PlayerState president)
        {
            float corruption = 0f;
            if (president.Resources.ContainsKey("Blackmail"))
                corruption += president.Resources["Blackmail"] * 0.1f;
            corruption += president.ScandalHistory.Count * 5f;
            return Mathf.Clamp(corruption, 0f, 100f);
        }
        
        private List<string> GetPolicies(PlayerState president)
        {
            return president?.PoliciesImplemented ?? new List<string>();
        }
        
        private float CalculatePolicyImpact(PlayerState president)
        {
            if (president?.PolicyImpacts == null || president.PolicyImpacts.Count == 0)
                return 0f;
            
            // Sum all policy impacts
            float totalImpact = president.PolicyImpacts.Values.Sum();
            
            // Average impact per policy
            float avgImpact = totalImpact / president.PolicyImpacts.Count;
            
            // Scale to 0-100 range
            return Mathf.Clamp(avgImpact, 0f, 100f);
        }
        
        private float CalculateLegacy(PlayerState president)
        {
            float legacy = president.ApprovalRating * 0.5f;
            if (president.Resources.ContainsKey("Legacy"))
                legacy += president.Resources["Legacy"];
            legacy -= president.ScandalHistory.Count * 10f;
            return Mathf.Clamp(legacy, -100f, 100f);
        }
    }
}

