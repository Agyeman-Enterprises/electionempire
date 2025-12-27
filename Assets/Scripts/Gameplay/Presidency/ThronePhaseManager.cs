using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.World;
using ElectionEmpire.AI;
using ElectionEmpire.Multiplayer.PersistentWorld;
using ElectionEmpire.Core;

// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - The Throne Phase (Defensive Gameplay)
// You won! Now defend your position against challengers
// ═══════════════════════════════════════════════════════════════════════════════

namespace ElectionEmpire.Gameplay.Presidency
{
    /// <summary>
    /// Manages Phase 2: The Throne - defensive gameplay when player is president
    /// </summary>
    public class ThronePhaseManager : MonoBehaviour
    {
        public static ThronePhaseManager Instance { get; private set; }
        
        [Header("Throne Configuration")]
        public int TermLengthYears = 4;
        public int MaxTerms = 2;
        public float ApprovalDecayRate = 0.5f; // Per turn
        
        // Current State
        private PlayerState _president;
        private int _currentTerm;
        private int _yearsInOffice;
        private DateTime _termStartDate;
        
        // Defensive Resources
        public float PoliticalCapital { get; private set; }
        public float InstitutionalControl { get; private set; }
        public float PartyLoyalty { get; private set; }
        public float MilitaryLoyalty { get; private set; }
        public float IntelligenceNetwork { get; private set; }
        
        // Threats
        public List<Threat> ActiveThreats { get; private set; }
        public List<Challenger> PrimaryChallengers { get; private set; }
        public List<Challenger> GeneralElectionOpponents { get; private set; }
        
        // Events
        public event Action<Threat> OnThreatDetected;
        public event Action<Challenger> OnChallengerAnnounced;
        public event Action<Crisis> OnCrisisEscalated;
        public event Action<float> OnApprovalChanged;
        public event Action OnTermEnded;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            ActiveThreats = new List<Threat>();
            PrimaryChallengers = new List<Challenger>();
            GeneralElectionOpponents = new List<Challenger>();
        }
        
        /// <summary>
        /// Start the throne phase - player has won the presidency
        /// </summary>
        public void BeginPresidency(PlayerState president, int termNumber)
        {
            _president = president;
            _currentTerm = termNumber;
            _yearsInOffice = 0;
            _termStartDate = DateTime.Now;
            
            // Initialize defensive resources
            PoliticalCapital = 100f;
            InstitutionalControl = 70f; // Start with some control
            PartyLoyalty = 60f; // Your party supports you... for now
            MilitaryLoyalty = 50f; // Neutral
            IntelligenceNetwork = 30f; // Basic network
            
            // Generate initial threats
            GenerateInitialThreats();
            
            Debug.Log($"[ThronePhase] {president.Character.Name} begins term {termNumber}");
        }
        
        /// <summary>
        /// Process a turn in the throne phase
        /// </summary>
        public void ProcessThroneTurn()
        {
            _yearsInOffice++;
            
            // Morning Intelligence Briefing
            ProcessIntelligenceBriefing();
            
            // Natural decay
            PoliticalCapital = Mathf.Max(0, PoliticalCapital - ApprovalDecayRate);
            PartyLoyalty = Mathf.Max(0, PartyLoyalty - 0.2f);
            
            // Update approval (natural decay)
            float approvalChange = -ApprovalDecayRate;
            _president.ApprovalRating = Mathf.Clamp(_president.ApprovalRating + approvalChange, 0f, 100f);
            OnApprovalChanged?.Invoke(_president.ApprovalRating);
            
            // Check for new threats
            CheckForNewThreats();
            
            // Escalate existing threats
            EscalateThreats();
            
            // Check for term end
            if (_yearsInOffice >= TermLengthYears)
            {
                EndTerm(TermEndReason.TermLimit);
            }
        }
        
        /// <summary>
        /// Morning intelligence briefing - see what's coming
        /// </summary>
        private void ProcessIntelligenceBriefing()
        {
            var briefing = new IntelligenceBriefing
            {
                CurrentApproval = _president.ApprovalRating,
                PoliticalCapital = PoliticalCapital,
                PartyLoyalty = PartyLoyalty,
                ActiveThreats = new List<Threat>(ActiveThreats),
                UpcomingChallenges = new List<Challenger>(PrimaryChallengers.Concat(GeneralElectionOpponents)),
                Crises = GetActiveCrises(),
                IntelligenceLevel = IntelligenceNetwork
            };
            
            // Intelligence network reveals hidden threats
            if (IntelligenceNetwork > 50f)
            {
                briefing.HiddenThreats = DetectHiddenThreats();
            }
            
            // Trigger briefing event
            OnIntelligenceBriefing?.Invoke(briefing);
        }
        
        public event Action<IntelligenceBriefing> OnIntelligenceBriefing;
        
        /// <summary>
        /// Use presidential power: Sign legislation
        /// </summary>
        public bool SignLegislation(string policyName, float legacyValue, float politicalCost)
        {
            if (PoliticalCapital < politicalCost)
            {
                Debug.LogWarning("[ThronePhase] Not enough political capital");
                return false;
            }
            
            PoliticalCapital -= politicalCost;
            
            // Track policy
            if (!_president.PoliciesImplemented.Contains(policyName))
            {
                _president.PoliciesImplemented.Add(policyName);
            }
            
            // Track policy impact
            if (!_president.PolicyImpacts.ContainsKey(policyName))
            {
                _president.PolicyImpacts[policyName] = 0f;
            }
            _president.PolicyImpacts[policyName] += legacyValue;
            
            // Build legacy
            if (_president.Resources.ContainsKey("Legacy"))
                _president.Resources["Legacy"] += legacyValue;
            
            // Apply policy effects
            ApplyPolicyEffects(policyName);
            
            // May trigger reactions
            if (UnityEngine.Random.value < 0.3f)
            {
                GeneratePolicyReaction(policyName);
            }
            
            Debug.Log($"[ThronePhase] Signed {policyName}. Legacy +{legacyValue}, Capital -{politicalCost}");
            return true;
        }
        
        /// <summary>
        /// Use presidential power: Issue executive order
        /// </summary>
        public bool IssueExecutiveOrder(string orderName, float effect, bool risky)
        {
            if (InstitutionalControl < 50f && risky)
            {
                Debug.LogWarning("[ThronePhase] Not enough institutional control for risky order");
                return false;
            }
            
            // Quick but risky
            ApplyExecutiveOrderEffects(orderName, effect);
            
            if (risky)
            {
                InstitutionalControl -= 10f;
                
                // May trigger backlash
                if (UnityEngine.Random.value < 0.4f)
                {
                    GenerateExecutiveOrderBacklash(orderName);
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Attack a challenger (dirty but effective)
        /// </summary>
        public bool AttackChallenger(string challengerId, AttackType attackType)
        {
            var challenger = PrimaryChallengers.FirstOrDefault(c => c.Id == challengerId)
                ?? GeneralElectionOpponents.FirstOrDefault(c => c.Id == challengerId);
            
            if (challenger == null) return false;
            
            float effectiveness = attackType switch
            {
                AttackType.Smear => 0.6f,
                AttackType.DigDirt => 0.4f,
                AttackType.CharacterAssassination => 0.8f,
                AttackType.LegalInvestigation => 0.5f,
                _ => 0.5f
            };
            
            // Apply damage
            challenger.Strength -= effectiveness * 20f;
            challenger.Strength = Mathf.Clamp(challenger.Strength, 0f, 100f);
            
            // Risk of backlash
            if (UnityEngine.Random.value < 0.3f)
            {
                GenerateAttackBacklash(challenger, attackType);
            }
            
            // Track for behavior profile
            BehaviorTracker.Instance?.TrackDecision(
                _president.Character.Name,
                $"attack_challenger_{attackType}",
                "throne_defense",
                50f
            );
            
            return true;
        }
        
        /// <summary>
        /// Build intelligence network
        /// </summary>
        public bool BuildIntelligenceNetwork(float investment)
        {
            if (_president.CampaignFunds < investment) return false;
            
            _president.CampaignFunds -= investment;
            IntelligenceNetwork = Mathf.Clamp(IntelligenceNetwork + (investment / 10000f), 0f, 100f);
            
            return true;
        }
        
        /// <summary>
        /// Shore up party loyalty
        /// </summary>
        public bool ShoreUpPartyLoyalty(float investment)
        {
            if (_president.CampaignFunds < investment) return false;
            
            _president.CampaignFunds -= investment;
            PartyLoyalty = Mathf.Clamp(PartyLoyalty + (investment / 5000f), 0f, 100f);
            
            return true;
        }
        
        /// <summary>
        /// Check for new threats
        /// </summary>
        private void CheckForNewThreats()
        {
            // Primary challengers (if party loyalty low)
            if (PartyLoyalty < 50f && UnityEngine.Random.value < 0.1f)
            {
                GeneratePrimaryChallenger();
            }
            
            // General election opponents (always a threat)
            if (GeneralElectionOpponents.Count == 0 && UnityEngine.Random.value < 0.15f)
            {
                GenerateGeneralElectionOpponent();
            }
            
            // Media crusades (if approval low)
            if (_president.ApprovalRating < 40f && UnityEngine.Random.value < 0.2f)
            {
                GenerateMediaCrusade();
            }
            
            // Scandals cascading
            if (_president.ActiveScandals.Count > 0 && UnityEngine.Random.value < 0.1f)
            {
                EscalateScandal();
            }
        }
        
        /// <summary>
        /// Generate a primary challenger
        /// </summary>
        private void GeneratePrimaryChallenger()
        {
            var challenger = new Challenger
            {
                Id = Guid.NewGuid().ToString("N").Substring(0, 8),
                Name = GenerateChallengerName(),
                Type = ChallengerType.Primary,
                Strength = UnityEngine.Random.Range(30f, 70f),
                ThreatLevel = ThreatLevel.Moderate,
                Motivation = "Party wants new direction"
            };
            
            PrimaryChallengers.Add(challenger);
            OnChallengerAnnounced?.Invoke(challenger);
            
            Debug.Log($"[ThronePhase] Primary challenger announced: {challenger.Name}");
        }
        
        /// <summary>
        /// Generate a general election opponent
        /// </summary>
        private void GenerateGeneralElectionOpponent()
        {
            // Could be a ghost, active player, or AI
            var opponent = new Challenger
            {
                Id = Guid.NewGuid().ToString("N").Substring(0, 8),
                Name = GenerateChallengerName(),
                Type = ChallengerType.GeneralElection,
                Strength = UnityEngine.Random.Range(40f, 80f),
                ThreatLevel = ThreatLevel.High,
                Motivation = "Opposition party's best candidate"
            };
            
            GeneralElectionOpponents.Add(opponent);
            OnChallengerAnnounced?.Invoke(opponent);
            
            Debug.Log($"[ThronePhase] General election opponent announced: {opponent.Name}");
        }
        
        /// <summary>
        /// Generate media crusade
        /// </summary>
        private void GenerateMediaCrusade()
        {
            var threat = new Threat
            {
                Id = Guid.NewGuid().ToString("N").Substring(0, 8),
                Type = ThreatType.MediaCrusade,
                Severity = UnityEngine.Random.Range(40f, 80f),
                Description = "Major news outlet launches investigation",
                CanEscalate = true
            };
            
            ActiveThreats.Add(threat);
            OnThreatDetected?.Invoke(threat);
        }
        
        /// <summary>
        /// End the term
        /// </summary>
        public void EndTerm(TermEndReason reason)
        {
            Debug.Log($"[ThronePhase] Term ended. Reason: {reason}");
            
            // Create term record for legacy system
            var termRecord = CreateTermRecord(reason);
            
            // Transition to legacy phase
            LegacyPhaseManager.Instance?.BeginLegacyPhase(_president, termRecord);
            
            OnTermEnded?.Invoke();
        }
        
        private PlayerTermRecord CreateTermRecord(TermEndReason reason)
        {
            return new PlayerTermRecord
            {
                PlayerId = _president.Character.Name,
                PlayerName = _president.Character.Name,
                TermStartYear = _termStartDate.Year,
                TermEndYear = _termStartDate.Year + _yearsInOffice,
                HighestOfficeTier = 5, // President
                HighestOffice = "President",
                CorruptionLevel = CalculateCorruptionLevel(),
                ScandalsCount = _president.ScandalHistory.Count,
                PoliciesImplemented = GetImplementedPolicies(),
                PolicyImpact = CalculatePolicyImpact(),
                Legacy = CalculateLegacy(),
                ReputationTags = new List<string>(_president.ReputationTags),
                BehaviorProfile = BehaviorTracker.Instance?.GetProfile(_president.Character.Name),
                TermEndReason = reason
            };
        }
        
        private float CalculateCorruptionLevel()
        {
            // Based on dirty tricks, scandals, etc.
            float corruption = 0f;
            
            if (_president.Resources.ContainsKey("Blackmail"))
                corruption += _president.Resources["Blackmail"] * 0.1f;
            
            corruption += _president.ScandalHistory.Count * 5f;
            
            return Mathf.Clamp(corruption, 0f, 100f);
        }
        
        private List<string> GetImplementedPolicies()
        {
            return _president?.PoliciesImplemented ?? new List<string>();
        }
        
        private float CalculatePolicyImpact()
        {
            if (_president?.PolicyImpacts == null || _president.PolicyImpacts.Count == 0)
                return 0f;
            
            // Sum all policy impacts
            float totalImpact = _president.PolicyImpacts.Values.Sum();
            
            // Average impact per policy
            float avgImpact = totalImpact / _president.PolicyImpacts.Count;
            
            // Scale to 0-100 range
            return Mathf.Clamp(avgImpact, 0f, 100f);
        }
        
        private float CalculateLegacy()
        {
            float legacy = _president.ApprovalRating * 0.5f;
            legacy += (_president.Resources.ContainsKey("Legacy") ? _president.Resources["Legacy"] : 0f);
            legacy -= _president.ScandalHistory.Count * 10f;
            
            return Mathf.Clamp(legacy, -100f, 100f);
        }
        
        private void GenerateInitialThreats()
        {
            // Start with some baseline threats
            if (UnityEngine.Random.value < 0.5f)
            {
                GenerateGeneralElectionOpponent();
            }
        }
        
        private void EscalateThreats()
        {
            foreach (var threat in ActiveThreats.ToList())
            {
                if (threat.CanEscalate && UnityEngine.Random.value < 0.2f)
                {
                    threat.Severity += 10f;
                    if (threat.Severity >= 100f)
                    {
                        TriggerThreatCrisis(threat);
                    }
                }
            }
        }
        
        private void TriggerThreatCrisis(Threat threat)
        {
            var crisis = new Crisis
            {
                Type = threat.Type switch
                {
                    ThreatType.MediaCrusade => CrisisType.MediaInvestigation,
                    ThreatType.Impeachment => CrisisType.Impeachment,
                    ThreatType.Assassination => CrisisType.AssassinationAttempt,
                    _ => CrisisType.General
                },
                Severity = threat.Severity,
                Description = threat.Description
            };
            
            OnCrisisEscalated?.Invoke(crisis);
        }
        
        private List<Crisis> GetActiveCrises()
        {
            return new List<Crisis>(); // Would track active crises
        }
        
        private List<Threat> DetectHiddenThreats()
        {
            return new List<Threat>(); // Intelligence reveals hidden threats
        }
        
        private void ApplyPolicyEffects(string policyName) { }
        private void GeneratePolicyReaction(string policyName) { }
        private void ApplyExecutiveOrderEffects(string orderName, float effect) { }
        private void GenerateExecutiveOrderBacklash(string orderName) { }
        private void GenerateAttackBacklash(Challenger challenger, AttackType attackType) { }
        private void EscalateScandal() { }
        private string GenerateChallengerName() => "Challenger " + UnityEngine.Random.Range(1000, 9999);
    }
    
    #region Data Structures
    
    [Serializable]
    public class Threat
    {
        public string Id;
        public ThreatType Type;
        public float Severity; // 0-100
        public string Description;
        public bool CanEscalate;
    }
    
    public enum ThreatType
    {
        PrimaryChallenge,
        GeneralElection,
        MediaCrusade,
        CongressionalOpposition,
        ScandalCascade,
        Impeachment,
        Assassination,
        MilitaryCoup
    }
    
    public enum ThreatLevel
    {
        Low,
        Moderate,
        High,
        Critical
    }
    
    [Serializable]
    public class Challenger
    {
        public string Id;
        public string Name;
        public ChallengerType Type;
        public float Strength; // 0-100
        public ThreatLevel ThreatLevel;
        public string Motivation;
        public bool IsGhost;
        public string GhostId; // If it's a ghost
        public bool IsActivePlayer;
        public string PlayerId; // If it's an active player
    }
    
    public enum ChallengerType
    {
        Primary,
        GeneralElection,
        PopulistOutsider,
        ThirdParty
    }
    
    [Serializable]
    public class Crisis
    {
        public CrisisType Type;
        public float Severity;
        public string Description;
    }
    [Serializable]
    public class IntelligenceBriefing
    {
        public float CurrentApproval;
        public float PoliticalCapital;
        public float PartyLoyalty;
        public List<Threat> ActiveThreats;
        public List<Challenger> UpcomingChallenges;
        public List<Crisis> Crises;
        public float IntelligenceLevel;
        public List<Threat> HiddenThreats;
    }
    
    public enum AttackType
    {
        Smear,
        DigDirt,
        CharacterAssassination,
        LegalInvestigation
    }
    #endregion
}

