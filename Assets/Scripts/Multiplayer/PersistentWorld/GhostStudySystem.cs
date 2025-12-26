using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - Ghost Study System
// Players can study former players' patterns, find weaknesses, exploit them
// ═══════════════════════════════════════════════════════════════════════════════

namespace ElectionEmpire.Multiplayer.PersistentWorld
{
    /// <summary>
    /// System for studying ghosts to find exploitable patterns
    /// </summary>
    public class GhostStudySystem : MonoBehaviour
    {
        public static GhostStudySystem Instance { get; private set; }
        
        private PersistentWorldManager _worldManager;
        private Dictionary<string, GhostStudy> _activeStudies;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            _activeStudies = new Dictionary<string, GhostStudy>();
            _worldManager = PersistentWorldManager.Instance;
        }
        
        /// <summary>
        /// Start studying a ghost to learn their patterns
        /// </summary>
        public GhostStudy StartStudyingGhost(string ghostId, string playerId)
        {
            if (_worldManager == null) return null;
            
            var studyResult = _worldManager.StudyGhost(ghostId);
            if (studyResult == null) return null;
            
            var study = new GhostStudy
            {
                GhostId = ghostId,
                GhostName = studyResult.GhostName,
                StudyingPlayerId = playerId,
                StartTime = DateTime.Now,
                KnowledgeLevel = 0f,
                DiscoveredWeaknesses = new List<string>(),
                DiscoveredTactics = new List<string>(),
                DiscoveredCatchphrases = new List<string>(),
                ExploitationStrategies = new List<ExploitationStrategy>()
            };
            
            // Initial discoveries from basic study
            study.DiscoveredWeaknesses.AddRange(studyResult.Weaknesses.Take(1));
            study.DiscoveredTactics.AddRange(studyResult.FavoriteTactics.Take(2));
            study.DiscoveredCatchphrases.AddRange(studyResult.Catchphrases.Take(1));
            
            _activeStudies[ghostId] = study;
            
            Debug.Log($"[GhostStudy] {playerId} started studying {studyResult.GhostName}");
            
            return study;
        }
        
        /// <summary>
        /// Continue studying a ghost (spend time/resources to learn more)
        /// </summary>
        public bool ContinueStudying(string ghostId, float timeInvestment, float resourceCost)
        {
            if (!_activeStudies.TryGetValue(ghostId, out var study))
                return false;
            
            // Increase knowledge
            study.KnowledgeLevel = Mathf.Clamp(study.KnowledgeLevel + (timeInvestment * 0.1f), 0f, 100f);
            study.TimeSpent += timeInvestment;
            study.ResourcesSpent += resourceCost;
            
            // Discover more as knowledge increases
            if (study.KnowledgeLevel > 30f && study.DiscoveredWeaknesses.Count < 3)
            {
                DiscoverNewWeakness(study);
            }
            
            if (study.KnowledgeLevel > 50f && study.DiscoveredTactics.Count < 5)
            {
                DiscoverNewTactic(study);
            }
            
            if (study.KnowledgeLevel > 70f)
            {
                GenerateExploitationStrategy(study);
            }
            
            return true;
        }
        
        /// <summary>
        /// Get exploitation strategies for a ghost
        /// </summary>
        public List<ExploitationStrategy> GetExploitationStrategies(string ghostId)
        {
            if (!_activeStudies.TryGetValue(ghostId, out var study))
                return new List<ExploitationStrategy>();
            
            return study.ExploitationStrategies;
        }
        
        /// <summary>
        /// Use an exploitation strategy against a ghost
        /// </summary>
        public bool ExploitGhost(string ghostId, string strategyId, out float effectiveness)
        {
            effectiveness = 0f;
            
            if (!_activeStudies.TryGetValue(ghostId, out var study))
                return false;
            
            var strategy = study.ExploitationStrategies.FirstOrDefault(s => s.Id == strategyId);
            if (strategy == null) return false;
            
            // Calculate effectiveness based on knowledge level
            effectiveness = strategy.BaseEffectiveness * (study.KnowledgeLevel / 100f);
            
            // Apply exploitation
            strategy.TimesUsed++;
            
            // Track for behavior profile
            BehaviorTracker.Instance?.TrackDecision(
                study.StudyingPlayerId,
                $"exploit_ghost_{strategy.Type}",
                $"exploiting_{ghostId}",
                30f
            );
            
            Debug.Log($"[GhostStudy] Exploited {study.GhostName} using {strategy.Name}. Effectiveness: {effectiveness:F1}%");
            
            return true;
        }
        
        /// <summary>
        /// Get a ghost's historical record (what they did in the past)
        /// </summary>
        public GhostHistoricalRecord GetGhostHistory(string ghostId)
        {
            if (_worldManager == null) return null;
            
            var ghosts = _worldManager.GetAvailableGhosts();
            var ghost = ghosts.FirstOrDefault(g => g.GhostId == ghostId);
            
            if (ghost == null) return null;
            
            // Get historical events involving this ghost
            var history = LivingHistorySystem.Instance?.GetEchoesFromPlayer(ghost.PlayerId, DateTime.Now.Year);
            
            return new GhostHistoricalRecord
            {
                GhostId = ghostId,
                GhostName = ghost.PlayerName,
                OriginalPlayerId = ghost.PlayerId,
                YearsActive = ghost.YearsActive,
                HistoricalEchoes = history ?? new List<Echo>(),
                KnownFor = GetKnownFor(ghost)
            };
        }
        
        private void DiscoverNewWeakness(GhostStudy study)
        {
            if (_worldManager == null) return;
            
            var studyResult = _worldManager.StudyGhost(study.GhostId);
            if (studyResult == null) return;
            
            var undiscovered = studyResult.Weaknesses
                .Where(w => !study.DiscoveredWeaknesses.Contains(w))
                .ToList();
            
            if (undiscovered.Count > 0)
            {
                var weakness = undiscovered[UnityEngine.Random.Range(0, undiscovered.Count)];
                study.DiscoveredWeaknesses.Add(weakness);
                Debug.Log($"[GhostStudy] Discovered weakness: {weakness}");
            }
        }
        
        private void DiscoverNewTactic(GhostStudy study)
        {
            if (_worldManager == null) return;
            
            var studyResult = _worldManager.StudyGhost(study.GhostId);
            if (studyResult == null) return;
            
            var undiscovered = studyResult.FavoriteTactics
                .Where(t => !study.DiscoveredTactics.Contains(t))
                .ToList();
            
            if (undiscovered.Count > 0)
            {
                var tactic = undiscovered[UnityEngine.Random.Range(0, undiscovered.Count)];
                study.DiscoveredTactics.Add(tactic);
                Debug.Log($"[GhostStudy] Discovered tactic: {tactic}");
            }
        }
        
        private void GenerateExploitationStrategy(GhostStudy study)
        {
            if (study.ExploitationStrategies.Count >= 5) return;
            
            // Generate strategy based on discovered weaknesses
            if (study.DiscoveredWeaknesses.Count > 0)
            {
                var weakness = study.DiscoveredWeaknesses[UnityEngine.Random.Range(0, study.DiscoveredWeaknesses.Count)];
                
                var strategy = new ExploitationStrategy
                {
                    Id = Guid.NewGuid().ToString("N").Substring(0, 8),
                    Name = $"Exploit {weakness}",
                    Description = $"Use their weakness: {weakness}",
                    Type = DetermineExploitationType(weakness),
                    BaseEffectiveness = UnityEngine.Random.Range(60f, 90f),
                    TargetsWeakness = weakness
                };
                
                study.ExploitationStrategies.Add(strategy);
                Debug.Log($"[GhostStudy] Generated exploitation strategy: {strategy.Name}");
            }
        }
        
        private ExploitationType DetermineExploitationType(string weakness)
        {
            if (weakness.ToLower().Contains("panic") || weakness.ToLower().Contains("stress"))
                return ExploitationType.PressureTactic;
            if (weakness.ToLower().Contains("risk") || weakness.ToLower().Contains("cautious"))
                return ExploitationType.BoldMoves;
            if (weakness.ToLower().Contains("loyalty") || weakness.ToLower().Contains("betray"))
                return ExploitationType.AllianceManipulation;
            
            return ExploitationType.GeneralExploitation;
        }
        
        private string GetKnownFor(GhostPolitician ghost)
        {
            if (ghost is GhostPresident president)
            {
                return president.Reputation;
            }
            
            return "Former Politician";
        }
    }
    
    #region Data Structures
    
    [Serializable]
    public class GhostStudy
    {
        public string GhostId;
        public string GhostName;
        public string StudyingPlayerId;
        public DateTime StartTime;
        public float KnowledgeLevel; // 0-100
        public float TimeSpent;
        public float ResourcesSpent;
        
        public List<string> DiscoveredWeaknesses;
        public List<string> DiscoveredTactics;
        public List<string> DiscoveredCatchphrases;
        public List<ExploitationStrategy> ExploitationStrategies;
    }
    
    [Serializable]
    public class ExploitationStrategy
    {
        public string Id;
        public string Name;
        public string Description;
        public ExploitationType Type;
        public float BaseEffectiveness; // 0-100
        public string TargetsWeakness;
        public int TimesUsed;
    }
    
    public enum ExploitationType
    {
        PressureTactic,
        BoldMoves,
        AllianceManipulation,
        MediaAttack,
        ScandalTrigger,
        GeneralExploitation
    }
    
    [Serializable]
    public class GhostHistoricalRecord
    {
        public string GhostId;
        public string GhostName;
        public string OriginalPlayerId;
        public int YearsActive;
        public List<Echo> HistoricalEchoes;
        public string KnownFor;
    }
    
    #endregion
}

