using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - Living History System
// Every scandal becomes historical record, every decision shapes future gameplay
// ═══════════════════════════════════════════════════════════════════════════════

namespace ElectionEmpire.Multiplayer.PersistentWorld
{
    /// <summary>
    /// Manages the living history - every player action becomes part of the world's story
    /// </summary>
    public class LivingHistorySystem : MonoBehaviour
    {
        public static LivingHistorySystem Instance { get; private set; }
        
        private PersistentWorldState _worldState;
        private List<HistoricalRecord> _historicalRecords;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            _historicalRecords = new List<HistoricalRecord>();
        }
        
        /// <summary>
        /// Record a historical event that will echo through time
        /// </summary>
        public void RecordHistoricalEvent(HistoricalEvent evt)
        {
            _historicalRecords.Add(new HistoricalRecord
            {
                Event = evt,
                RecordedAt = DateTime.Now,
                ImpactEchoes = new List<Echo>()
            });
            
            // Add to world state
            if (_worldState != null)
            {
                _worldState.HistoricalEvents.Add(new WorldEvent
                {
                    Year = evt.Year,
                    EventType = evt.Type.ToString(),
                    Description = evt.Description,
                    Impact = evt.Impact,
                    CausedByPlayerId = evt.PlayerId
                });
            }
            
            Debug.Log($"[LivingHistory] Recorded: {evt.Description} ({evt.Year})");
        }
        
        /// <summary>
        /// Record a scandal that becomes part of history
        /// </summary>
        public void RecordScandal(string playerId, string playerName, string scandalDescription, 
                                  int year, float severity)
        {
            var evt = new HistoricalEvent
            {
                PlayerId = playerId,
                PlayerName = playerName,
                Year = year,
                Type = HistoricalEventType.Scandal,
                Description = $"The {scandalDescription} scandal rocked {playerName}'s presidency",
                Impact = -severity,
                EchoStrength = severity,
                CanEcho = true
            };
            
            RecordHistoricalEvent(evt);
            
            // Scandal echoes affect future players
            CreateEcho(evt, EchoType.ScandalPrecedent, 
                $"Future politicians face increased scrutiny due to {playerName}'s scandal");
        }
        
        /// <summary>
        /// Record a policy decision that shapes the world
        /// </summary>
        public void RecordPolicy(string playerId, string playerName, string policyName, 
                                 int year, float impact)
        {
            var evt = new HistoricalEvent
            {
                PlayerId = playerId,
                PlayerName = playerName,
                Year = year,
                Type = HistoricalEventType.Policy,
                Description = $"{playerName} enacted {policyName}",
                Impact = impact,
                EchoStrength = Mathf.Abs(impact),
                CanEcho = true
            };
            
            RecordHistoricalEvent(evt);
            
            // Policy echoes affect future gameplay
            CreateEcho(evt, EchoType.PolicyLegacy,
                $"The {policyName} policy continues to affect politics");
        }
        
        /// <summary>
        /// Record a presidency that becomes legendary
        /// </summary>
        public void RecordPresidency(string playerId, string playerName, int startYear, 
                                    int endYear, float legacy, string reputation)
        {
            var evt = new HistoricalEvent
            {
                PlayerId = playerId,
                PlayerName = playerName,
                Year = startYear,
                Type = HistoricalEventType.Presidency,
                Description = $"{playerName} ({reputation}) served as President from {startYear}-{endYear}",
                Impact = legacy,
                EchoStrength = Mathf.Abs(legacy),
                CanEcho = true
            };
            
            RecordHistoricalEvent(evt);
            
            // Presidency echoes are strong
            CreateEcho(evt, EchoType.PresidentialLegacy,
                $"The {reputation} presidency of {playerName} shapes political discourse");
        }
        
        /// <summary>
        /// Create an echo - how past events affect current gameplay
        /// </summary>
        private void CreateEcho(HistoricalEvent sourceEvent, EchoType echoType, string description)
        {
            var echo = new Echo
            {
                SourceEvent = sourceEvent,
                Type = echoType,
                Description = description,
                Strength = sourceEvent.EchoStrength,
                CreatedAt = DateTime.Now,
                ExpiresAt = CalculateEchoExpiration(sourceEvent)
            };
            
            // Find the historical record and add echo
            var record = _historicalRecords.FirstOrDefault(r => r.Event.PlayerId == sourceEvent.PlayerId);
            if (record != null)
            {
                record.ImpactEchoes.Add(echo);
            }
        }
        
        /// <summary>
        /// Get echoes that affect current gameplay
        /// </summary>
        public List<Echo> GetActiveEchoes(int currentYear)
        {
            return _historicalRecords
                .SelectMany(r => r.ImpactEchoes)
                .Where(e => e.ExpiresAt == null || e.ExpiresAt.Value.Year >= currentYear)
                .OrderByDescending(e => e.Strength)
                .ToList();
        }
        
        /// <summary>
        /// Get historical context for a player starting a new game
        /// </summary>
        public HistoricalContext GetHistoricalContext(int currentYear, int yearsBack = 50)
        {
            var relevantEvents = _historicalRecords
                .Where(r => r.Event.Year >= currentYear - yearsBack)
                .OrderByDescending(r => r.Event.Year)
                .ToList();
            
            return new HistoricalContext
            {
                CurrentYear = currentYear,
                RecentPresidents = GetRecentPresidents(currentYear, yearsBack),
                MajorScandals = GetMajorScandals(currentYear, yearsBack),
                PolicyLegacies = GetActivePolicyLegacies(),
                WorldState = GetWorldStateDescription(),
                Echoes = GetActiveEchoes(currentYear)
            };
        }
        
        /// <summary>
        /// Get how a past player's actions affect current gameplay
        /// </summary>
        public List<Echo> GetEchoesFromPlayer(string playerId, int currentYear)
        {
            return _historicalRecords
                .Where(r => r.Event.PlayerId == playerId)
                .SelectMany(r => r.ImpactEchoes)
                .Where(e => e.ExpiresAt == null || e.ExpiresAt.Value.Year >= currentYear)
                .ToList();
        }
        
        private DateTime? CalculateEchoExpiration(HistoricalEvent evt)
        {
            // Strong events echo longer
            if (evt.EchoStrength > 80f)
                return DateTime.Now.AddYears(100); // Centuries
            if (evt.EchoStrength > 50f)
                return DateTime.Now.AddYears(50);
            if (evt.EchoStrength > 20f)
                return DateTime.Now.AddYears(20);
            
            return DateTime.Now.AddYears(10);
        }
        
        private List<HistoricalEvent> GetRecentPresidents(int currentYear, int yearsBack)
        {
            return _historicalRecords
                .Where(r => r.Event.Type == HistoricalEventType.Presidency)
                .Where(r => r.Event.Year >= currentYear - yearsBack)
                .Select(r => r.Event)
                .OrderByDescending(e => e.Year)
                .ToList();
        }
        
        private List<HistoricalEvent> GetMajorScandals(int currentYear, int yearsBack)
        {
            return _historicalRecords
                .Where(r => r.Event.Type == HistoricalEventType.Scandal)
                .Where(r => r.Event.Year >= currentYear - yearsBack)
                .Where(r => r.Event.EchoStrength > 50f)
                .Select(r => r.Event)
                .OrderByDescending(e => e.EchoStrength)
                .ToList();
        }
        
        private List<string> GetActivePolicyLegacies()
        {
            return _historicalRecords
                .Where(r => r.Event.Type == HistoricalEventType.Policy)
                .Where(r => r.ImpactEchoes.Any(e => e.ExpiresAt == null || e.ExpiresAt.Value > DateTime.Now))
                .Select(r => r.Event.Description)
                .ToList();
        }
        
        private string GetWorldStateDescription()
        {
            if (_worldState == null) return "The world is new.";
            return _worldState.GetWorldStateDescription();
        }
    }
    
    #region Data Structures
    
    [Serializable]
    public class HistoricalEvent
    {
        public string PlayerId;
        public string PlayerName;
        public int Year;
        public HistoricalEventType Type;
        public string Description;
        public float Impact; // -100 to 100
        public float EchoStrength; // How long it echoes
        public bool CanEcho;
    }
    
    public enum HistoricalEventType
    {
        Presidency,
        Scandal,
        Policy,
        Election,
        Crisis,
        Assassination,
        Impeachment,
        War,
        EconomicEvent
    }
    
    [Serializable]
    public class HistoricalRecord
    {
        public HistoricalEvent Event;
        public DateTime RecordedAt;
        public List<Echo> ImpactEchoes;
    }
    
    [Serializable]
    public class Echo
    {
        public HistoricalEvent SourceEvent;
        public EchoType Type;
        public string Description;
        public float Strength;
        public DateTime CreatedAt;
        public DateTime? ExpiresAt;
    }
    
    public enum EchoType
    {
        ScandalPrecedent,
        PolicyLegacy,
        PresidentialLegacy,
        InstitutionalChange,
        VoterAttitude,
        MediaBehavior,
        PartyDynamics
    }
    
    [Serializable]
    public class HistoricalContext
    {
        public int CurrentYear;
        public List<HistoricalEvent> RecentPresidents;
        public List<HistoricalEvent> MajorScandals;
        public List<string> PolicyLegacies;
        public string WorldState;
        public List<Echo> Echoes;
    }
    
    #endregion
}

