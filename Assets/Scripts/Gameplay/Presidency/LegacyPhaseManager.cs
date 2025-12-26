using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.World;
using ElectionEmpire.Multiplayer.PersistentWorld;

// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - The Legacy Phase (Transcendence)
// Your presidency ends - but your impact is PERMANENT
// ═══════════════════════════════════════════════════════════════════════════════

namespace ElectionEmpire.Gameplay.Presidency
{
    /// <summary>
    /// Manages Phase 3: The Legacy - what happens after your presidency
    /// </summary>
    public class LegacyPhaseManager : MonoBehaviour
    {
        public static LegacyPhaseManager Instance { get; private set; }
        
        private PersistentWorldManager _worldManager;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            _worldManager = PersistentWorldManager.Instance;
        }
        
        /// <summary>
        /// Begin the legacy phase - transition from throne to legacy
        /// </summary>
        public void BeginLegacyPhase(PlayerState formerPresident, PlayerTermRecord termRecord)
        {
            Debug.Log($"[LegacyPhase] {formerPresident.Character.Name} enters legacy phase");
            
            // 1. Create ghost from behavior profile
            CreateGhost(formerPresident, termRecord);
            
            // 2. Apply world effects
            ApplyWorldEffects(termRecord);
            
            // 3. Create/update dynasty
            UpdateDynasty(formerPresident, termRecord);
            
            // 4. Record legacy
            RecordLegacy(formerPresident, termRecord);
            
            // 5. Determine post-presidency role
            DeterminePostPresidencyRole(formerPresident, termRecord);
        }
        
        /// <summary>
        /// Create a ghost from the former president
        /// </summary>
        private void CreateGhost(PlayerState president, PlayerTermRecord record)
        {
            if (_worldManager == null) return;
            
            // The ghost is already created in PersistentWorldState.ApplyPlayerTermEffects
            // But we can enhance it here
            
            var ghost = new GhostPresident
            {
                PlayerId = president.Character.Name,
                PlayerName = president.Character.Name,
                BehaviorProfile = record.BehaviorProfile,
                TermStartYear = record.TermStartYear,
                TermEndYear = record.TermEndYear,
                Legacy = record.Legacy,
                Reputation = DetermineReputation(record),
                PoliciesEnacted = record.PoliciesImplemented,
                ScandalsSurvived = record.ScandalsCount
            };
            
            ghost.GeneratePersonalityFromProfile();
            
            Debug.Log($"[LegacyPhase] Ghost created: {ghost.PlayerName} ({ghost.Reputation})");
        }
        
        /// <summary>
        /// Apply effects to the persistent world
        /// </summary>
        private void ApplyWorldEffects(PlayerTermRecord record)
        {
            if (_worldManager == null) return;
            
            // Record the term in the world
            _worldManager.RecordPlayerTermEnd(
                record.PlayerId,
                record.PlayerName,
                record
            );
            
            Debug.Log($"[LegacyPhase] World effects applied. Corruption: {record.CorruptionLevel:F1}%");
        }
        
        /// <summary>
        /// Create or update political dynasty
        /// </summary>
        private void UpdateDynasty(PlayerState president, PlayerTermRecord record)
        {
            // Check if dynasty exists
            var existingDynasty = FindDynasty(president.Character.Name);
            
            if (existingDynasty == null && record.Legacy > 30f)
            {
                // Create new dynasty
                var dynasty = new PoliticalDynasty
                {
                    DynastyName = $"{president.Character.Name} Dynasty",
                    FounderPlayerId = president.Character.Name,
                    MemberPlayerIds = new List<string> { president.Character.Name },
                    Generations = 1,
                    Influence = record.Legacy / 2f,
                    ControlledOffices = new List<string> { "President" }
                };
                
                // Would add to world state
                Debug.Log($"[LegacyPhase] Dynasty created: {dynasty.DynastyName}");
            }
            else if (existingDynasty != null)
            {
                // Update existing dynasty
                existingDynasty.Influence = Mathf.Clamp(existingDynasty.Influence + (record.Legacy / 10f), 0f, 100f);
                existingDynasty.MemberPlayerIds.Add(president.Character.Name);
                
                Debug.Log($"[LegacyPhase] Dynasty updated: {existingDynasty.DynastyName}");
            }
        }
        
        private PoliticalDynasty FindDynasty(string playerId)
        {
            // Would search world state for dynasty
            return null;
        }
        
        /// <summary>
        /// Record legacy achievements
        /// </summary>
        private void RecordLegacy(PlayerState president, PlayerTermRecord record)
        {
            var legacy = new LegacyRecord
            {
                PlayerId = president.Character.Name,
                PlayerName = president.Character.Name,
                TermRecord = record,
                LegacyScore = record.Legacy,
                Reputation = DetermineReputation(record),
                HistoricalImpact = CalculateHistoricalImpact(record),
                GhostId = GenerateGhostId(president.Character.Name),
                DynastyId = FindDynasty(president.Character.Name)?.DynastyName
            };
            
            // Would save to persistent storage
            Debug.Log($"[LegacyPhase] Legacy recorded: {legacy.LegacyScore:F1} points");
        }
        
        /// <summary>
        /// Determine what role the former president takes
        /// </summary>
        private void DeterminePostPresidencyRole(PlayerState president, PlayerTermRecord record)
        {
            string role = record.Legacy switch
            {
                > 70f => "Elder Statesman",
                > 30f => "Former President",
                > -30f => "Controversial Former President",
                _ => "Disgraced Former President"
            };
            
            if (record.ScandalsCount > 10)
                role = "Scandal-Plagued Former President";
            
            if (record.TermEndReason == TermEndReason.Impeachment)
                role = "Impeached Former President";
            
            if (record.TermEndReason == TermEndReason.Assassination)
                role = "Martyred President";
            
            Debug.Log($"[LegacyPhase] Post-presidency role: {role}");
        }
        
        private string DetermineReputation(PlayerTermRecord record)
        {
            if (record.Legacy > 70f && record.ScandalsCount < 3)
                return "The Great";
            if (record.Legacy > 50f)
                return "The Reformer";
            if (record.CorruptionLevel > 70f)
                return "The Corrupt";
            if (record.ScandalsCount > 10)
                return "The Scandalous";
            if (record.Legacy < -50f)
                return "The Disgraced";
            
            return "The Average";
        }
        
        private float CalculateHistoricalImpact(PlayerTermRecord record)
        {
            float impact = record.Legacy;
            impact += record.PoliciesImplemented.Count * 10f;
            impact -= record.ScandalsCount * 5f;
            impact += record.PolicyImpact;
            
            return impact;
        }
        
        private string GenerateGhostId(string playerName)
        {
            return $"GHOST_{playerName}_{DateTime.Now.Year}";
        }
    }
    
    [Serializable]
    public class LegacyRecord
    {
        public string PlayerId;
        public string PlayerName;
        public PlayerTermRecord TermRecord;
        public float LegacyScore;
        public string Reputation;
        public float HistoricalImpact;
        public string GhostId;
        public string DynastyId;
    }
}

