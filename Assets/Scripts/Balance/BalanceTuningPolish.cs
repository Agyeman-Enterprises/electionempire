// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - Balance Tuning & Polish Systems
// Sprint 11: Game balance, difficulty scaling, analytics, save system improvements
// ═══════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using ElectionEmpire.Core;

namespace ElectionEmpire.Balance
{
    #region Difficulty System
    
    /// <summary>
    /// Game difficulty levels.
    /// </summary>
    public enum DifficultyLevel
    {
        Story,          // Easy mode for narrative focus
        Normal,         // Standard balanced experience
        Challenging,    // For experienced players
        Nightmare,      // Punishing difficulty
        Custom          // User-defined settings
    }
    
    /// <summary>
    /// Individual difficulty settings.
    /// </summary>
    [Serializable]
    public class DifficultySettings
    {
        public string Name;
        public string Description;
        public DifficultyLevel Level;
        
        // Resource multipliers (1.0 = normal)
        public float ResourceGenerationMultiplier = 1f;
        public float ResourceDecayMultiplier = 1f;
        public float FundraiseMultiplier = 1f;
        public float TrustGainMultiplier = 1f;
        public float TrustDecayMultiplier = 1f;
        
        // Scandal modifiers
        public float ScandalFrequencyMultiplier = 1f;
        public float ScandalSeverityMultiplier = 1f;
        public float ScandalRecoveryMultiplier = 1f;
        public float DirtBackfireChance = 0.3f;
        
        // AI opponent strength
        public float AIAggressiveness = 1f;
        public float AIResourceBonus = 0f;
        public float AIDecisionQuality = 1f;
        public float AIReactionSpeed = 1f;
        
        // Election mechanics
        public float ElectionMarginModifier = 0f;
        public float UndecidedVoterSwing = 0.1f;
        public float PollAccuracy = 0.9f;
        public float TurnoutVariance = 0.1f;
        
        // Crisis system
        public float CrisisFrequencyMultiplier = 1f;
        public float CrisisSeverityMultiplier = 1f;
        public float CrisisResponseWindow = 1f;
        
        // Rewards
        public float CloutBuxMultiplier = 1f;
        public float LegacyPointMultiplier = 1f;
        public float AchievementRequirementMultiplier = 1f;
        
        // Chaos mode specific
        public float ChaosEventMultiplier = 1f;
        public float ViralChanceMultiplier = 1f;
        
        public DifficultySettings Clone()
        {
            return (DifficultySettings)MemberwiseClone();
        }
    }
    
    /// <summary>
    /// Manages game difficulty and balance settings.
    /// </summary>
    public class DifficultyManager
    {
        private DifficultySettings currentSettings;
        private Dictionary<DifficultyLevel, DifficultySettings> presets;
        
        public DifficultySettings CurrentSettings => currentSettings;
        public event Action<DifficultySettings> OnDifficultyChanged;
        
        public DifficultyManager()
        {
            presets = new Dictionary<DifficultyLevel, DifficultySettings>();
            InitializePresets();
            SetDifficulty(DifficultyLevel.Normal);
        }
        
        private void InitializePresets()
        {
            // Story Mode - Focus on narrative, minimal challenge
            presets[DifficultyLevel.Story] = new DifficultySettings
            {
                Name = "Story Mode",
                Description = "Focus on the political narrative with minimal challenge.",
                Level = DifficultyLevel.Story,
                ResourceGenerationMultiplier = 1.5f,
                ResourceDecayMultiplier = 0.5f,
                FundraiseMultiplier = 1.5f,
                TrustGainMultiplier = 1.5f,
                TrustDecayMultiplier = 0.5f,
                ScandalFrequencyMultiplier = 0.5f,
                ScandalSeverityMultiplier = 0.7f,
                ScandalRecoveryMultiplier = 1.5f,
                DirtBackfireChance = 0.1f,
                AIAggressiveness = 0.5f,
                AIResourceBonus = -0.2f,
                AIDecisionQuality = 0.7f,
                AIReactionSpeed = 0.7f,
                ElectionMarginModifier = 0.1f,
                UndecidedVoterSwing = 0.15f,
                PollAccuracy = 0.95f,
                TurnoutVariance = 0.05f,
                CrisisFrequencyMultiplier = 0.5f,
                CrisisSeverityMultiplier = 0.7f,
                CrisisResponseWindow = 1.5f,
                CloutBuxMultiplier = 0.75f,
                LegacyPointMultiplier = 0.75f,
                ChaosEventMultiplier = 0.7f,
                ViralChanceMultiplier = 1.3f
            };
            
            // Normal - Balanced experience
            presets[DifficultyLevel.Normal] = new DifficultySettings
            {
                Name = "Normal",
                Description = "The balanced Election Empire experience.",
                Level = DifficultyLevel.Normal
                // All defaults at 1.0
            };
            
            // Challenging - For experienced players
            presets[DifficultyLevel.Challenging] = new DifficultySettings
            {
                Name = "Challenging",
                Description = "For seasoned politicians. AI is smarter, scandals hit harder.",
                Level = DifficultyLevel.Challenging,
                ResourceGenerationMultiplier = 0.85f,
                ResourceDecayMultiplier = 1.15f,
                FundraiseMultiplier = 0.9f,
                TrustGainMultiplier = 0.9f,
                TrustDecayMultiplier = 1.2f,
                ScandalFrequencyMultiplier = 1.25f,
                ScandalSeverityMultiplier = 1.2f,
                ScandalRecoveryMultiplier = 0.8f,
                DirtBackfireChance = 0.35f,
                AIAggressiveness = 1.3f,
                AIResourceBonus = 0.15f,
                AIDecisionQuality = 1.3f,
                AIReactionSpeed = 1.2f,
                ElectionMarginModifier = -0.05f,
                PollAccuracy = 0.85f,
                CrisisFrequencyMultiplier = 1.2f,
                CrisisSeverityMultiplier = 1.15f,
                CrisisResponseWindow = 0.85f,
                CloutBuxMultiplier = 1.25f,
                LegacyPointMultiplier = 1.25f,
                ChaosEventMultiplier = 1.2f
            };
            
            // Nightmare - Punishing difficulty
            presets[DifficultyLevel.Nightmare] = new DifficultySettings
            {
                Name = "Nightmare",
                Description = "Politics at its most brutal. Only for masochists.",
                Level = DifficultyLevel.Nightmare,
                ResourceGenerationMultiplier = 0.7f,
                ResourceDecayMultiplier = 1.5f,
                FundraiseMultiplier = 0.75f,
                TrustGainMultiplier = 0.75f,
                TrustDecayMultiplier = 1.5f,
                ScandalFrequencyMultiplier = 1.5f,
                ScandalSeverityMultiplier = 1.5f,
                ScandalRecoveryMultiplier = 0.6f,
                DirtBackfireChance = 0.45f,
                AIAggressiveness = 1.75f,
                AIResourceBonus = 0.3f,
                AIDecisionQuality = 1.5f,
                AIReactionSpeed = 1.5f,
                ElectionMarginModifier = -0.1f,
                PollAccuracy = 0.75f,
                TurnoutVariance = 0.15f,
                CrisisFrequencyMultiplier = 1.5f,
                CrisisSeverityMultiplier = 1.5f,
                CrisisResponseWindow = 0.6f,
                CloutBuxMultiplier = 1.5f,
                LegacyPointMultiplier = 1.5f,
                ChaosEventMultiplier = 1.5f,
                ViralChanceMultiplier = 0.75f
            };
        }
        
        public void SetDifficulty(DifficultyLevel level)
        {
            if (presets.TryGetValue(level, out var preset))
            {
                currentSettings = preset.Clone();
                OnDifficultyChanged?.Invoke(currentSettings);
            }
        }
        
        public void SetCustomDifficulty(DifficultySettings settings)
        {
            settings.Level = DifficultyLevel.Custom;
            currentSettings = settings;
            OnDifficultyChanged?.Invoke(currentSettings);
        }
        
        public DifficultySettings GetPreset(DifficultyLevel level)
        {
            return presets.TryGetValue(level, out var preset) ? preset.Clone() : null;
        }
        
        public float ApplyResourceModifier(float baseValue, bool isGeneration)
        {
            return baseValue * (isGeneration ? 
                currentSettings.ResourceGenerationMultiplier : 
                currentSettings.ResourceDecayMultiplier);
        }
        
        public float ApplyScandalModifier(float baseValue, string modifierType)
        {
            return modifierType switch
            {
                "frequency" => baseValue * currentSettings.ScandalFrequencyMultiplier,
                "severity" => baseValue * currentSettings.ScandalSeverityMultiplier,
                "recovery" => baseValue * currentSettings.ScandalRecoveryMultiplier,
                _ => baseValue
            };
        }
        
        public float ApplyAIModifier(float baseValue, string modifierType)
        {
            return modifierType switch
            {
                "aggression" => baseValue * currentSettings.AIAggressiveness,
                "resources" => baseValue * (1 + currentSettings.AIResourceBonus),
                "quality" => baseValue * currentSettings.AIDecisionQuality,
                "speed" => baseValue * currentSettings.AIReactionSpeed,
                _ => baseValue
            };
        }
    }
    
    #endregion
    
    #region Balance Tuning Data
    
    /// <summary>
    /// Background balance statistics.
    /// </summary>
    [Serializable]
    public class BackgroundBalanceData
    {
        public string BackgroundId;
        public float WinRate;
        public float AveragePlaytime;
        public float Tier5ReachRate;
        public int TotalGamesPlayed;
        public float AverageScandalsSurvived;
        public float PopularityScore;
    }
    
    /// <summary>
    /// Resource balance tracking.
    /// </summary>
    [Serializable]
    public class ResourceBalanceData
    {
        public string ResourceName;
        public float AverageAtTier1;
        public float AverageAtTier3;
        public float AverageAtTier5;
        public float GenerationRate;
        public float DecayRate;
        public float BottleneckFrequency;
    }
    
    /// <summary>
    /// Manages balance data and adjustments.
    /// </summary>
    public class BalanceDataManager
    {
        private Dictionary<string, BackgroundBalanceData> backgroundStats;
        private Dictionary<string, ResourceBalanceData> resourceStats;
        private List<BalanceAdjustment> pendingAdjustments;
        
        // Target values for balance
        private const float TargetWinRate = 0.45f;
        private const float TargetWinRateVariance = 0.1f;
        private const float TargetTier5Rate = 0.2f;
        
        public BalanceDataManager()
        {
            backgroundStats = new Dictionary<string, BackgroundBalanceData>();
            resourceStats = new Dictionary<string, ResourceBalanceData>();
            pendingAdjustments = new List<BalanceAdjustment>();
            InitializeBaselineData();
        }
        
        private void InitializeBaselineData()
        {
            // Baseline background data (would be updated from analytics)
            string[] backgrounds = { "Businessman", "LocalPolitician", "Teacher", "Doctor", 
                                    "PoliceOfficer", "Journalist", "Activist", "ReligiousLeader" };
            
            foreach (var bg in backgrounds)
            {
                backgroundStats[bg] = new BackgroundBalanceData
                {
                    BackgroundId = bg,
                    WinRate = 0.45f,
                    AveragePlaytime = 180f,
                    Tier5ReachRate = 0.2f,
                    TotalGamesPlayed = 0
                };
            }
            
            // Baseline resource data
            string[] resources = { "PublicTrust", "PoliticalCapital", "CampaignFunds", 
                                  "MediaInfluence", "PartyLoyalty", "Dirt" };
            
            foreach (var res in resources)
            {
                resourceStats[res] = new ResourceBalanceData
                {
                    ResourceName = res,
                    AverageAtTier1 = 50f,
                    AverageAtTier3 = 60f,
                    AverageAtTier5 = 70f
                };
            }
        }
        
        public void RecordGameResult(string backgroundId, bool won, int tierReached, float playtime)
        {
            if (!backgroundStats.TryGetValue(backgroundId, out var data))
            {
                data = new BackgroundBalanceData { BackgroundId = backgroundId };
                backgroundStats[backgroundId] = data;
            }
            
            // Update running averages
            int n = data.TotalGamesPlayed;
            data.WinRate = (data.WinRate * n + (won ? 1 : 0)) / (n + 1);
            data.AveragePlaytime = (data.AveragePlaytime * n + playtime) / (n + 1);
            data.Tier5ReachRate = (data.Tier5ReachRate * n + (tierReached >= 5 ? 1 : 0)) / (n + 1);
            data.TotalGamesPlayed++;
            
            CheckForImbalance(backgroundId);
        }
        
        private void CheckForImbalance(string backgroundId)
        {
            var data = backgroundStats[backgroundId];
            
            if (data.TotalGamesPlayed < 100) return; // Need sufficient data
            
            float deviation = Math.Abs(data.WinRate - TargetWinRate);
            
            if (deviation > TargetWinRateVariance)
            {
                pendingAdjustments.Add(new BalanceAdjustment
                {
                    TargetId = backgroundId,
                    AdjustmentType = "WinRate",
                    CurrentValue = data.WinRate,
                    TargetValue = TargetWinRate,
                    SuggestedChange = data.WinRate > TargetWinRate ? -0.05f : 0.05f,
                    Priority = deviation > 0.15f ? 1 : 2,
                    Timestamp = DateTime.UtcNow
                });
            }
        }
        
        public List<BalanceAdjustment> GetPendingAdjustments()
        {
            return pendingAdjustments.OrderBy(a => a.Priority).ToList();
        }
        
        public BackgroundBalanceData GetBackgroundStats(string backgroundId)
        {
            return backgroundStats.TryGetValue(backgroundId, out var data) ? data : null;
        }
    }
    
    /// <summary>
    /// Suggested balance adjustment.
    /// </summary>
    [Serializable]
    public class BalanceAdjustment
    {
        public string TargetId;
        public string AdjustmentType;
        public float CurrentValue;
        public float TargetValue;
        public float SuggestedChange;
        public int Priority;
        public DateTime Timestamp;
        public bool Applied;
    }
    
    #endregion
    
    #region Analytics System
    
    /// <summary>
    /// Types of analytics events.
    /// </summary>
    public enum AnalyticsEventType
    {
        GameStart,
        GameEnd,
        ElectionWon,
        ElectionLost,
        ScandalTriggered,
        ScandalResolved,
        CrisisHandled,
        TierAdvanced,
        AchievementUnlocked,
        PurchaseMade,
        SessionStart,
        SessionEnd,
        TutorialStep,
        FeatureUsed,
        ErrorOccurred
    }
    
    /// <summary>
    /// Analytics event data.
    /// </summary>
    [Serializable]
    public class AnalyticsEvent
    {
        public string EventId;
        public AnalyticsEventType EventType;
        public DateTime Timestamp;
        public string SessionId;
        public string UserId;
        public Dictionary<string, object> Parameters;
        
        public AnalyticsEvent()
        {
            EventId = Guid.NewGuid().ToString();
            Timestamp = DateTime.UtcNow;
            Parameters = new Dictionary<string, object>();
        }
    }
    
    /// <summary>
    /// Session data for analytics.
    /// </summary>
    [Serializable]
    public class SessionData
    {
        public string SessionId;
        public DateTime StartTime;
        public DateTime? EndTime;
        public float TotalPlaytime;
        public int EventCount;
        public string Platform;
        public string GameVersion;
        public DifficultyLevel Difficulty;
        public string CurrentBackground;
        public int CurrentTier;
    }
    
    /// <summary>
    /// Manages game analytics and telemetry.
    /// </summary>
    public class AnalyticsManager
    {
        private SessionData currentSession;
        private Queue<AnalyticsEvent> eventQueue;
        private List<AnalyticsEvent> batchBuffer;
        private bool isEnabled = true;
        private string userId;
        
        private const int BatchSize = 50;
        private const float FlushIntervalSeconds = 60f;
        
        public event Action<AnalyticsEvent> OnEventLogged;
        
        public AnalyticsManager()
        {
            eventQueue = new Queue<AnalyticsEvent>();
            batchBuffer = new List<AnalyticsEvent>();
            userId = GetOrCreateUserId();
        }
        
        private string GetOrCreateUserId()
        {
            string id = PlayerPrefs.GetString("AnalyticsUserId", "");
            if (string.IsNullOrEmpty(id))
            {
                id = Guid.NewGuid().ToString();
                PlayerPrefs.SetString("AnalyticsUserId", id);
                PlayerPrefs.Save();
            }
            return id;
        }
        
        public void StartSession(string gameVersion, DifficultyLevel difficulty)
        {
            currentSession = new SessionData
            {
                SessionId = Guid.NewGuid().ToString(),
                StartTime = DateTime.UtcNow,
                Platform = Application.platform.ToString(),
                GameVersion = gameVersion,
                Difficulty = difficulty
            };
            
            LogEvent(AnalyticsEventType.SessionStart, new Dictionary<string, object>
            {
                { "platform", currentSession.Platform },
                { "version", gameVersion },
                { "difficulty", difficulty.ToString() }
            });
        }
        
        public void EndSession()
        {
            if (currentSession == null) return;
            
            currentSession.EndTime = DateTime.UtcNow;
            currentSession.TotalPlaytime = (float)(currentSession.EndTime.Value - currentSession.StartTime).TotalSeconds;
            
            LogEvent(AnalyticsEventType.SessionEnd, new Dictionary<string, object>
            {
                { "playtime", currentSession.TotalPlaytime },
                { "eventCount", currentSession.EventCount }
            });
            
            FlushEvents();
        }
        
        public void LogEvent(AnalyticsEventType eventType, Dictionary<string, object> parameters = null)
        {
            if (!isEnabled) return;
            
            var evt = new AnalyticsEvent
            {
                EventType = eventType,
                SessionId = currentSession?.SessionId ?? "no_session",
                UserId = userId,
                Parameters = parameters ?? new Dictionary<string, object>()
            };
            
            // Add common parameters
            evt.Parameters["timestamp"] = evt.Timestamp.ToString("O");
            evt.Parameters["tier"] = currentSession?.CurrentTier ?? 0;
            
            eventQueue.Enqueue(evt);
            
            if (currentSession != null)
            {
                currentSession.EventCount++;
            }
            
            OnEventLogged?.Invoke(evt);
            
            // Auto-flush if buffer is full
            if (eventQueue.Count >= BatchSize)
            {
                FlushEvents();
            }
        }
        
        public void LogGameStart(string backgroundId, DifficultyLevel difficulty, bool isChaosMode)
        {
            LogEvent(AnalyticsEventType.GameStart, new Dictionary<string, object>
            {
                { "background", backgroundId },
                { "difficulty", difficulty.ToString() },
                { "chaosMode", isChaosMode }
            });
            
            if (currentSession != null)
            {
                currentSession.CurrentBackground = backgroundId;
                currentSession.Difficulty = difficulty;
            }
        }
        
        public void LogGameEnd(bool won, int finalTier, float playtime, string endReason)
        {
            LogEvent(AnalyticsEventType.GameEnd, new Dictionary<string, object>
            {
                { "won", won },
                { "finalTier", finalTier },
                { "playtime", playtime },
                { "endReason", endReason },
                { "background", currentSession?.CurrentBackground ?? "unknown" }
            });
        }
        
        public void LogElection(bool won, int tier, float votePercent, string officeId)
        {
            LogEvent(won ? AnalyticsEventType.ElectionWon : AnalyticsEventType.ElectionLost,
                new Dictionary<string, object>
                {
                    { "tier", tier },
                    { "votePercent", votePercent },
                    { "office", officeId }
                });
            
            if (currentSession != null && won)
            {
                currentSession.CurrentTier = tier;
            }
        }
        
        public void LogScandal(string scandalType, int severity, string resolution)
        {
            LogEvent(AnalyticsEventType.ScandalTriggered, new Dictionary<string, object>
            {
                { "type", scandalType },
                { "severity", severity },
                { "resolution", resolution }
            });
        }
        
        public void LogPurchase(string itemId, string currencyType, long amount, bool success)
        {
            LogEvent(AnalyticsEventType.PurchaseMade, new Dictionary<string, object>
            {
                { "item", itemId },
                { "currency", currencyType },
                { "amount", amount },
                { "success", success }
            });
        }
        
        public void LogAchievement(string achievementId, string category)
        {
            LogEvent(AnalyticsEventType.AchievementUnlocked, new Dictionary<string, object>
            {
                { "achievement", achievementId },
                { "category", category }
            });
        }
        
        public void LogError(string errorType, string message, string stackTrace)
        {
            LogEvent(AnalyticsEventType.ErrorOccurred, new Dictionary<string, object>
            {
                { "errorType", errorType },
                { "message", message },
                { "stackTrace", stackTrace?.Substring(0, Math.Min(500, stackTrace?.Length ?? 0)) }
            });
        }
        
        private void FlushEvents()
        {
            while (eventQueue.Count > 0)
            {
                batchBuffer.Add(eventQueue.Dequeue());
            }
            
            if (batchBuffer.Count > 0)
            {
                // Send to analytics backend
                SendBatch(batchBuffer.ToList());
                batchBuffer.Clear();
            }
        }
        
        private void SendBatch(List<AnalyticsEvent> events)
        {
            // In production, this would send to analytics service
            string json = JsonUtility.ToJson(new { events = events });
            Debug.Log($"[Analytics] Sending batch of {events.Count} events");
            
            // Store locally as backup
            string path = Path.Combine(Application.persistentDataPath, "analytics_backup.json");
            try
            {
                File.AppendAllText(path, json + "\n");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to write analytics backup: {e.Message}");
            }
        }
        
        public void SetEnabled(bool enabled)
        {
            isEnabled = enabled;
        }
        
        public void SetUserProperty(string key, object value)
        {
            // Would set user properties in analytics service
            Debug.Log($"[Analytics] User property: {key} = {value}");
        }
    }
    
    #endregion
    
    #region Enhanced Save System
    
    /// <summary>
    /// Save file metadata.
    /// </summary>
    [Serializable]
    public class SaveMetadata
    {
        public string SaveId;
        public string SaveName;
        public int SaveSlot;
        public DateTime CreatedDate;
        public DateTime LastModifiedDate;
        public string GameVersion;
        public int PlaytimeMinutes;
        
        // Quick preview data
        public string CharacterName;
        public string BackgroundId;
        public string CurrentOffice;
        public int CurrentTier;
        public float ApprovalRating;
        public long CampaignFunds;
        public string AlignmentCategory;
        public bool IsChaosMode;
        public DifficultyLevel Difficulty;
        
        // Integrity
        public string Checksum;
        public bool IsCloudSynced;
        public DateTime? CloudSyncDate;
    }
    [Serializable]
    public class CharacterSaveData
    {
        public string CharacterId;
        public string Name;
        public string BackgroundId;
        public int Age;
        public Dictionary<string, int> Attributes;
        public List<string> PositiveTraits;
        public List<string> NegativeTraits;
        public int LawChaosAlignment;
        public int GoodEvilAlignment;
        public string CurrentOfficeId;
        public int CurrentTier;
        public int TermsServed;
        
        public CharacterSaveData()
        {
            Attributes = new Dictionary<string, int>();
            PositiveTraits = new List<string>();
            NegativeTraits = new List<string>();
        }
    }
    
    [Serializable]
    public class WorldSaveData
    {
        public DateTime GameDate;
        public int TurnNumber;
        public string CurrentPhase;
        public int ElectionYear;
        public Dictionary<string, float> RegionControl;
        public List<string> ActivePolicies;
        public Dictionary<string, float> VoterBlocSupport;
        public Dictionary<string, object> WorldFlags;
        
        public WorldSaveData()
        {
            RegionControl = new Dictionary<string, float>();
            ActivePolicies = new List<string>();
            VoterBlocSupport = new Dictionary<string, float>();
            WorldFlags = new Dictionary<string, object>();
        }
    }
    
    [Serializable]
    public class ResourceSaveData
    {
        public float PublicTrust;
        public int PoliticalCapital;
        public long CampaignFunds;
        public int MediaInfluence;
        public float PartyLoyalty;
        public Dictionary<string, int> DirtOnOpponents;
        public long CloutBux;
        public long Purrkoin;
        public int LegacyPoints;
        
        public ResourceSaveData()
        {
            DirtOnOpponents = new Dictionary<string, int>();
        }
    }
    
    [Serializable]
    public class RelationshipSaveData
    {
        public Dictionary<string, int> NPCRelationships;
        public List<string> Allies;
        public List<string> Rivals;
        public Dictionary<string, float> FactionStanding;
        public Dictionary<string, float> MediaOutletRelations;
        
        public RelationshipSaveData()
        {
            NPCRelationships = new Dictionary<string, int>();
            Allies = new List<string>();
            Rivals = new List<string>();
            FactionStanding = new Dictionary<string, float>();
            MediaOutletRelations = new Dictionary<string, float>();
        }
    }
    
    [Serializable]
    public class ScandalSaveData
    {
        public string ScandalId;
        public string TemplateId;
        public int Severity;
        public int TurnsActive;
        public string CurrentStage;
        public float EvidenceLevel;
        public List<string> ResponseHistory;
        
        public ScandalSaveData()
        {
            ResponseHistory = new List<string>();
        }
    }
    
    [Serializable]
    public class CrisisSaveData
    {
        public string CrisisId;
        public string CrisisType;
        public int Severity;
        public int TurnsRemaining;
        public string CurrentStage;
        public Dictionary<string, object> CrisisData;
        
        public CrisisSaveData()
        {
            CrisisData = new Dictionary<string, object>();
        }
    }
    
    [Serializable]
    public class HistoryEntry
    {
        public DateTime Date;
        public string EventType;
        public string Description;
        public Dictionary<string, object> Data;
        
        public HistoryEntry()
        {
            Data = new Dictionary<string, object>();
        }
    }
    
    [Serializable]
    public class GameSettingsSaveData
    {
        public DifficultyLevel Difficulty;
        public DifficultySettings CustomDifficulty;
        public bool IsChaosMode;
        public float GameSpeed;
        public bool AutosaveEnabled;
    }
    
    /// <summary>
    /// Complete game save data structure
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        public SaveMetadata Metadata;
        public CharacterSaveData Character;
        public WorldSaveData World;
        public ResourceSaveData Resources;
        public RelationshipSaveData Relationships;
        public List<ScandalSaveData> ActiveScandals;
        public List<CrisisSaveData> ActiveCrises;
        public List<HistoryEntry> History;
        public GameSettingsSaveData Settings;

        public GameSaveData()
        {
            ActiveScandals = new List<ScandalSaveData>();
            ActiveCrises = new List<CrisisSaveData>();
            History = new List<HistoryEntry>();
        }
    }

    public class EnhancedSaveManager
    {
        private const int MaxSaveSlots = 10;
        private const int AutosaveSlot = 0;
        private const string SaveFileExtension = ".save";
        private const string MetadataExtension = ".meta";

        private Dictionary<int, SaveMetadata> saveSlots = new Dictionary<int, SaveMetadata>();
        private GameSaveData currentGame;
        private string savePath;

        // Events
        public event Action<int> OnSaveStarted;
        public event Action<int, bool> OnSaveCompleted;
        public event Action<int> OnLoadStarted;
        public event Action<int, bool> OnLoadCompleted;
        public event Action<float> OnCloudSyncProgress;

        public EnhancedSaveManager()
        {
            savePath = Application.persistentDataPath;
            LoadSaveMetadata();
        }
        
        private void LoadSaveMetadata()
        {
            for (int i = 0; i <= MaxSaveSlots; i++)
            {
                string metaPath = GetMetadataPath(i);
                if (File.Exists(metaPath))
                {
                    try
                    {
                        string json = File.ReadAllText(metaPath);
                        var metadata = JsonUtility.FromJson<SaveMetadata>(json);
                        saveSlots[i] = metadata;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Failed to load save metadata for slot {i}: {e.Message}");
                    }
                }
            }
        }
        
        public SaveMetadata[] GetAllSaves()
        {
            return saveSlots.Values.OrderByDescending(s => s.LastModifiedDate).ToArray();
        }
        
        public SaveMetadata GetSaveMetadata(int slot)
        {
            return saveSlots.TryGetValue(slot, out var meta) ? meta : null;
        }
        
        public bool SaveGame(int slot, GameSaveData data, string saveName = null)
        {
            OnSaveStarted?.Invoke(slot);
            
            try
            {
                // Update metadata
                data.Metadata = data.Metadata ?? new SaveMetadata();
                data.Metadata.SaveId = data.Metadata.SaveId ?? Guid.NewGuid().ToString();
                data.Metadata.SaveSlot = slot;
                data.Metadata.SaveName = saveName ?? $"Save {slot}";
                data.Metadata.LastModifiedDate = DateTime.UtcNow;
                data.Metadata.GameVersion = Application.version;
                
                if (data.Metadata.CreatedDate == default)
                {
                    data.Metadata.CreatedDate = DateTime.UtcNow;
                }
                
                // Update preview data
                if (data.Character != null)
                {
                    data.Metadata.CharacterName = data.Character.Name;
                    data.Metadata.BackgroundId = data.Character.BackgroundId;
                    data.Metadata.CurrentOffice = data.Character.CurrentOfficeId;
                    data.Metadata.CurrentTier = data.Character.CurrentTier;
                }
                
                if (data.Resources != null)
                {
                    data.Metadata.ApprovalRating = data.Resources.PublicTrust;
                    data.Metadata.CampaignFunds = data.Resources.CampaignFunds;
                }
                
                if (data.Settings != null)
                {
                    data.Metadata.IsChaosMode = data.Settings.IsChaosMode;
                    data.Metadata.Difficulty = data.Settings.Difficulty;
                }
                
                // Serialize and save
                string gameJson = JsonUtility.ToJson(data, true);
                string metaJson = JsonUtility.ToJson(data.Metadata, true);
                
                // Calculate checksum
                data.Metadata.Checksum = CalculateChecksum(gameJson);
                metaJson = JsonUtility.ToJson(data.Metadata, true);
                
                // Write files
                File.WriteAllText(GetSavePath(slot), gameJson);
                File.WriteAllText(GetMetadataPath(slot), metaJson);
                
                saveSlots[slot] = data.Metadata;
                
                OnSaveCompleted?.Invoke(slot, true);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save game: {e.Message}");
                OnSaveCompleted?.Invoke(slot, false);
                return false;
            }
        }
        
        public GameSaveData LoadGame(int slot)
        {
            OnLoadStarted?.Invoke(slot);
            
            try
            {
                string savePath = GetSavePath(slot);
                if (!File.Exists(savePath))
                {
                    OnLoadCompleted?.Invoke(slot, false);
                    return null;
                }
                
                string json = File.ReadAllText(savePath);
                
                // Verify checksum
                var metadata = saveSlots.TryGetValue(slot, out var m) ? m : null;
                if (metadata != null && !string.IsNullOrEmpty(metadata.Checksum))
                {
                    string currentChecksum = CalculateChecksum(json);
                    if (currentChecksum != metadata.Checksum)
                    {
                        Debug.LogWarning($"Save file checksum mismatch for slot {slot}");
                        // Could prompt user or allow loading anyway
                    }
                }
                
                var data = JsonUtility.FromJson<GameSaveData>(json);
                currentGame = data;
                
                OnLoadCompleted?.Invoke(slot, true);
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load game: {e.Message}");
                OnLoadCompleted?.Invoke(slot, false);
                return null;
            }
        }
        
        public bool DeleteSave(int slot)
        {
            try
            {
                string savePath = GetSavePath(slot);
                string metaPath = GetMetadataPath(slot);
                
                if (File.Exists(savePath)) File.Delete(savePath);
                if (File.Exists(metaPath)) File.Delete(metaPath);
                
                saveSlots.Remove(slot);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete save: {e.Message}");
                return false;
            }
        }
        
        public void Autosave(GameSaveData data)
        {
            SaveGame(AutosaveSlot, data, "Autosave");
        }
        
        public int GetNextAvailableSlot()
        {
            for (int i = 1; i <= MaxSaveSlots; i++)
            {
                if (!saveSlots.ContainsKey(i))
                {
                    return i;
                }
            }
            return -1; // All slots full
        }
        
        public bool HasSaveInSlot(int slot)
        {
            return saveSlots.ContainsKey(slot);
        }
        
        private string GetSavePath(int slot)
        {
            return Path.Combine(savePath, $"save_{slot}{SaveFileExtension}");
        }
        
        private string GetMetadataPath(int slot)
        {
            return Path.Combine(savePath, $"save_{slot}{MetadataExtension}");
        }
        
        private string CalculateChecksum(string data)
        {
            try
            {
                using (var sha = System.Security.Cryptography.SHA256.Create())
                {
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(data);
                    byte[] hash = sha.ComputeHash(bytes);
                    return Convert.ToBase64String(hash);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to calculate checksum: {e.Message}");
                return "";
            }
        }
        
        #region Cloud Sync
        
        public async void SyncToCloud(int slot)
        {
            if (!saveSlots.TryGetValue(slot, out var metadata)) return;
            
            OnCloudSyncProgress?.Invoke(0f);
            
            // Simulate cloud sync
            await System.Threading.Tasks.Task.Delay(1000);
            OnCloudSyncProgress?.Invoke(0.5f);
            
            await System.Threading.Tasks.Task.Delay(1000);
            
            metadata.IsCloudSynced = true;
            metadata.CloudSyncDate = DateTime.UtcNow;
            
            // Update metadata file
            string metaJson = JsonUtility.ToJson(metadata, true);
            File.WriteAllText(GetMetadataPath(slot), metaJson);
            
            OnCloudSyncProgress?.Invoke(1f);
        }
        
        public async void SyncFromCloud(int slot)
        {
            OnCloudSyncProgress?.Invoke(0f);
            
            // Would download from cloud service
            await System.Threading.Tasks.Task.Delay(2000);
            
            OnCloudSyncProgress?.Invoke(1f);
            LoadSaveMetadata();
        }
        
        #endregion
    }
    
    #endregion
    
    #region Tutorial System
    
    /// <summary>
    /// Tutorial step definition.
    /// </summary>
    [Serializable]
    public class TutorialStep
    {
        public string StepId;
        public string Title;
        public string Description;
        public string TargetElement;        // UI element to highlight
        public string RequiredAction;       // Action to complete step
        public Vector2 TooltipPosition;
        public bool AllowSkip;
        public string NextStepId;
        public string[] Prerequisites;
        
        public TutorialStep()
        {
            Prerequisites = new string[0];
        }
    }
    
    /// <summary>
    /// Tutorial sequence.
    /// </summary>
    [Serializable]
    public class TutorialSequence
    {
        public string SequenceId;
        public string Name;
        public string Description;
        public List<TutorialStep> Steps;
        public bool IsRequired;
        public int UnlockTier;
        
        public TutorialSequence()
        {
            Steps = new List<TutorialStep>();
        }
    }
    
    /// <summary>
    /// Manages tutorial and onboarding.
    /// </summary>
    public class TutorialManager
    {
        private Dictionary<string, TutorialSequence> sequences;
        private HashSet<string> completedSteps;
        private HashSet<string> completedSequences;
        private TutorialStep currentStep;
        private TutorialSequence currentSequence;
        
        public event Action<TutorialStep> OnStepStarted;
        public event Action<TutorialStep> OnStepCompleted;
        public event Action<TutorialSequence> OnSequenceCompleted;
        
        public bool IsTutorialActive => currentStep != null;
        public TutorialStep CurrentStep => currentStep;
        
        public TutorialManager()
        {
            sequences = new Dictionary<string, TutorialSequence>();
            completedSteps = new HashSet<string>();
            completedSequences = new HashSet<string>();
            
            InitializeTutorials();
            LoadProgress();
        }
        
        private void InitializeTutorials()
        {
            // Basic gameplay tutorial
            sequences["basic_gameplay"] = new TutorialSequence
            {
                SequenceId = "basic_gameplay",
                Name = "Getting Started",
                Description = "Learn the basics of Election Empire",
                IsRequired = true,
                UnlockTier = 0,
                Steps = new List<TutorialStep>
                {
                    new TutorialStep
                    {
                        StepId = "welcome",
                        Title = "Welcome to Election Empire!",
                        Description = "Politics is a dirty game. Let's get your hands filthy.",
                        AllowSkip = false,
                        NextStepId = "select_background"
                    },
                    new TutorialStep
                    {
                        StepId = "select_background",
                        Title = "Choose Your Background",
                        Description = "Your background determines your starting advantages and special ability.",
                        TargetElement = "BackgroundSelection",
                        RequiredAction = "select_background",
                        AllowSkip = false,
                        NextStepId = "allocate_stats"
                    },
                    new TutorialStep
                    {
                        StepId = "allocate_stats",
                        Title = "Allocate Your Stats",
                        Description = "Distribute points across Charisma, Intelligence, Cunning, Resilience, and Networking.",
                        TargetElement = "StatAllocation",
                        RequiredAction = "confirm_stats",
                        AllowSkip = true,
                        NextStepId = "first_campaign"
                    },
                    new TutorialStep
                    {
                        StepId = "first_campaign",
                        Title = "Your First Campaign",
                        Description = "Start your political journey by running for local office.",
                        TargetElement = "CampaignPanel",
                        RequiredAction = "start_campaign",
                        AllowSkip = false,
                        NextStepId = "actions_intro"
                    },
                    new TutorialStep
                    {
                        StepId = "actions_intro",
                        Title = "Taking Actions",
                        Description = "Each turn you have action points to spend on campaign activities, governance, or other political maneuvers.",
                        TargetElement = "ActionPanel",
                        RequiredAction = "take_action",
                        AllowSkip = true,
                        NextStepId = "resources_intro"
                    },
                    new TutorialStep
                    {
                        StepId = "resources_intro",
                        Title = "Managing Resources",
                        Description = "Track your Public Trust, Campaign Funds, Political Capital, and other vital resources.",
                        TargetElement = "ResourcePanel",
                        AllowSkip = true,
                        NextStepId = "tutorial_complete"
                    },
                    new TutorialStep
                    {
                        StepId = "tutorial_complete",
                        Title = "You're Ready!",
                        Description = "You've learned the basics. Now go win that election!",
                        AllowSkip = false
                    }
                }
            };
            
            // Scandal handling tutorial
            sequences["scandal_tutorial"] = new TutorialSequence
            {
                SequenceId = "scandal_tutorial",
                Name = "Scandal Management",
                Description = "Learn how to handle political scandals",
                IsRequired = false,
                UnlockTier = 1,
                Steps = new List<TutorialStep>
                {
                    new TutorialStep
                    {
                        StepId = "scandal_intro",
                        Title = "Scandal Alert!",
                        Description = "Scandals are a fact of political life. How you handle them matters.",
                        TargetElement = "ScandalAlert",
                        AllowSkip = true,
                        NextStepId = "scandal_options"
                    },
                    new TutorialStep
                    {
                        StepId = "scandal_options",
                        Title = "Response Options",
                        Description = "You can Deny, Apologize, Counter-Attack, Distract, or Sacrifice a subordinate.",
                        TargetElement = "ScandalResponsePanel",
                        AllowSkip = true,
                        NextStepId = "scandal_consequences"
                    },
                    new TutorialStep
                    {
                        StepId = "scandal_consequences",
                        Title = "Consequences",
                        Description = "Your response affects Public Trust, Media Coverage, and may spawn new scandals.",
                        AllowSkip = true
                    }
                }
            };
            
            // Advanced mechanics
            sequences["advanced_mechanics"] = new TutorialSequence
            {
                SequenceId = "advanced_mechanics",
                Name = "Advanced Politics",
                Description = "Master the deeper mechanics",
                IsRequired = false,
                UnlockTier = 2,
                Steps = new List<TutorialStep>
                {
                    new TutorialStep
                    {
                        StepId = "alignment_intro",
                        Title = "Hidden Alignment",
                        Description = "Your decisions shape your political alignment along Law/Chaos and Good/Evil axes.",
                        AllowSkip = true,
                        NextStepId = "dirty_tricks"
                    },
                    new TutorialStep
                    {
                        StepId = "dirty_tricks",
                        Title = "Dirty Tricks",
                        Description = "Sometimes winning requires getting your hands dirty. But be careful of backfire!",
                        TargetElement = "DirtyTricksPanel",
                        AllowSkip = true,
                        NextStepId = "coalition_building"
                    },
                    new TutorialStep
                    {
                        StepId = "coalition_building",
                        Title = "Building Coalitions",
                        Description = "Form alliances with factions and other politicians to strengthen your position.",
                        AllowSkip = true
                    }
                }
            };
        }
        
        public void StartSequence(string sequenceId)
        {
            if (!sequences.TryGetValue(sequenceId, out var sequence)) return;
            if (completedSequences.Contains(sequenceId)) return;
            
            currentSequence = sequence;
            if (sequence.Steps.Count > 0)
            {
                StartStep(sequence.Steps[0]);
            }
        }
        
        private void StartStep(TutorialStep step)
        {
            currentStep = step;
            OnStepStarted?.Invoke(step);
        }
        
        public void CompleteCurrentStep()
        {
            if (currentStep == null) return;
            
            completedSteps.Add(currentStep.StepId);
            OnStepCompleted?.Invoke(currentStep);
            
            // Find next step
            if (!string.IsNullOrEmpty(currentStep.NextStepId))
            {
                var nextStep = currentSequence?.Steps.Find(s => s.StepId == currentStep.NextStepId);
                if (nextStep != null)
                {
                    StartStep(nextStep);
                    return;
                }
            }
            
            // Sequence complete
            if (currentSequence != null)
            {
                completedSequences.Add(currentSequence.SequenceId);
                OnSequenceCompleted?.Invoke(currentSequence);
            }
            
            currentStep = null;
            currentSequence = null;
            SaveProgress();
        }
        
        public void SkipCurrentStep()
        {
            if (currentStep?.AllowSkip == true)
            {
                CompleteCurrentStep();
            }
        }
        
        public void SkipAllTutorials()
        {
            foreach (var seq in sequences.Values)
            {
                completedSequences.Add(seq.SequenceId);
            }
            currentStep = null;
            currentSequence = null;
            SaveProgress();
        }
        
        public bool IsSequenceCompleted(string sequenceId)
        {
            return completedSequences.Contains(sequenceId);
        }
        
        public List<TutorialSequence> GetAvailableSequences(int currentTier)
        {
            return sequences.Values
                .Where(s => !completedSequences.Contains(s.SequenceId) && s.UnlockTier <= currentTier)
                .ToList();
        }
        
        private void SaveProgress()
        {
            string completed = string.Join(",", completedSequences);
            PlayerPrefs.SetString("TutorialProgress", completed);
            PlayerPrefs.Save();
        }
        
        private void LoadProgress()
        {
            string completed = PlayerPrefs.GetString("TutorialProgress", "");
            if (!string.IsNullOrEmpty(completed))
            {
                foreach (var id in completed.Split(','))
                {
                    if (!string.IsNullOrEmpty(id))
                    {
                        completedSequences.Add(id);
                    }
                }
            }
        }
        
        public void ResetProgress()
        {
            completedSteps.Clear();
            completedSequences.Clear();
            PlayerPrefs.DeleteKey("TutorialProgress");
        }
    }
    
    #endregion
    
    #region Polish Effects
    
    /// <summary>
    /// Screen shake parameters.
    /// </summary>
    [Serializable]
    public class ScreenShakeParams
    {
        public float Duration = 0.5f;
        public float Magnitude = 0.1f;
        public float Frequency = 25f;
        public AnimationCurve FalloffCurve;
    }
    
    /// <summary>
    /// Manages visual polish effects.
    /// </summary>
    public class PolishEffectsManager : MonoBehaviour
    {
        [Header("Screen Shake")]
        [SerializeField] private Transform cameraTransform;
        private Vector3 originalCameraPosition;
        private Coroutine shakeCoroutine;
        
        [Header("Slow Motion")]
        [SerializeField] private float slowMotionScale = 0.3f;
        private float normalTimeScale = 1f;
        
        [Header("Screen Flash")]
        [SerializeField] private CanvasGroup flashOverlay;
        
        private void Awake()
        {
            if (cameraTransform != null)
            {
                originalCameraPosition = cameraTransform.localPosition;
            }
        }
        
        #region Screen Shake
        
        public void ShakeScreen(ScreenShakeParams shakeParams)
        {
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
            }
            shakeCoroutine = StartCoroutine(ShakeCoroutine(shakeParams));
        }
        
        public void ShakeScreen(float duration = 0.5f, float magnitude = 0.1f)
        {
            ShakeScreen(new ScreenShakeParams
            {
                Duration = duration,
                Magnitude = magnitude
            });
        }
        
        private System.Collections.IEnumerator ShakeCoroutine(ScreenShakeParams p)
        {
            float elapsed = 0f;
            
            while (elapsed < p.Duration)
            {
                float progress = elapsed / p.Duration;
                float dampening = p.FalloffCurve != null ? p.FalloffCurve.Evaluate(progress) : (1f - progress);
                
                float x = (Mathf.PerlinNoise(Time.time * p.Frequency, 0) * 2 - 1) * p.Magnitude * dampening;
                float y = (Mathf.PerlinNoise(0, Time.time * p.Frequency) * 2 - 1) * p.Magnitude * dampening;
                
                if (cameraTransform != null)
                {
                    cameraTransform.localPosition = originalCameraPosition + new Vector3(x, y, 0);
                }
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            if (cameraTransform != null)
            {
                cameraTransform.localPosition = originalCameraPosition;
            }
        }
        
        #endregion
        
        #region Slow Motion
        
        public void TriggerSlowMotion(float duration)
        {
            StartCoroutine(SlowMotionCoroutine(duration));
        }
        
        private System.Collections.IEnumerator SlowMotionCoroutine(float duration)
        {
            normalTimeScale = Time.timeScale;
            Time.timeScale = slowMotionScale;
            Time.fixedDeltaTime = 0.02f * slowMotionScale;
            
            yield return new WaitForSecondsRealtime(duration);
            
            Time.timeScale = normalTimeScale;
            Time.fixedDeltaTime = 0.02f;
        }
        
        public void SetTimeScale(float scale)
        {
            Time.timeScale = scale;
            Time.fixedDeltaTime = 0.02f * scale;
        }
        
        #endregion
        
        #region Screen Flash
        
        public void FlashScreen(Color color, float duration = 0.3f)
        {
            StartCoroutine(FlashCoroutine(color, duration));
        }
        
        private System.Collections.IEnumerator FlashCoroutine(Color color, float duration)
        {
            if (flashOverlay == null) yield break;
            
            var image = flashOverlay.GetComponent<UnityEngine.UI.Image>();
            if (image != null)
            {
                image.color = color;
            }
            
            flashOverlay.alpha = 1f;
            
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                flashOverlay.alpha = 1f - (elapsed / duration);
                yield return null;
            }
            
            flashOverlay.alpha = 0f;
        }
        
        #endregion
        
        #region Convenience Methods
        
        public void OnScandalRevealed(int severity)
        {
            float magnitude = 0.05f + (severity * 0.02f);
            ShakeScreen(0.4f, magnitude);
            FlashScreen(new Color(1f, 0.3f, 0.3f, 0.5f), 0.2f);
        }
        
        public void OnElectionWon()
        {
            FlashScreen(new Color(1f, 0.84f, 0f, 0.6f), 0.5f);
        }
        
        public void OnElectionLost()
        {
            ShakeScreen(0.6f, 0.15f);
            FlashScreen(new Color(0.2f, 0.2f, 0.2f, 0.7f), 0.4f);
        }
        
        public void OnCriticalMoment()
        {
            TriggerSlowMotion(1.5f);
        }
        
        public void OnChaosEvent()
        {
            ShakeScreen(0.8f, 0.2f);
            FlashScreen(new Color(1f, 0f, 1f, 0.4f), 0.3f);
        }
        
        #endregion
    }
    
    #endregion
}

