using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - Legacy Naming System
// Your name becomes part of the world's history - remembered for centuries!
// ═══════════════════════════════════════════════════════════════════════════════

namespace ElectionEmpire.Multiplayer.PersistentWorld
{
    /// <summary>
    /// Manages how player names become part of history - remembered for centuries
    /// </summary>
    public class LegacyNamingSystem : MonoBehaviour
    {
        public static LegacyNamingSystem Instance { get; private set; }
        
        private Dictionary<string, LegacyName> _legacyNames;
        private PersistentWorldState _worldState;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            _legacyNames = new Dictionary<string, LegacyName>();
        }
        
        /// <summary>
        /// Register a player's name for historical remembrance
        /// </summary>
        public LegacyName RegisterLegacyName(string playerId, string playerName, 
                                             float legacyScore, string reputation)
        {
            var legacyName = new LegacyName
            {
                PlayerId = playerId,
                OriginalName = playerName,
                HistoricalName = GenerateHistoricalName(playerName, reputation),
                LegacyScore = legacyScore,
                Reputation = reputation,
                FirstRecorded = DateTime.Now,
                LastReferenced = DateTime.Now,
                ReferenceCount = 0,
                HistoricalReferences = new List<HistoricalReference>()
            };
            
            _legacyNames[playerId] = legacyName;
            
            Debug.Log($"[LegacyNaming] Registered: {legacyName.HistoricalName}");
            
            return legacyName;
        }
        
        /// <summary>
        /// Reference a historical name (future players encounter it)
        /// </summary>
        public void ReferenceHistoricalName(string playerId, string context, int currentYear)
        {
            if (!_legacyNames.TryGetValue(playerId, out var legacyName))
                return;
            
            legacyName.ReferenceCount++;
            legacyName.LastReferenced = DateTime.Now;
            
            // Add historical reference
            legacyName.HistoricalReferences.Add(new HistoricalReference
            {
                Year = currentYear,
                Context = context,
                ReferencedBy = "Future Player"
            });
            
            // Strong legacy names are referenced more
            if (legacyName.LegacyScore > 70f)
            {
                // Name becomes part of common political discourse
                Debug.Log($"[LegacyNaming] {legacyName.HistoricalName} is mentioned in {context}");
            }
        }
        
        /// <summary>
        /// Get how a player is remembered in history
        /// </summary>
        public string GetHistoricalRemembrance(string playerId, int currentYear)
        {
            if (!_legacyNames.TryGetValue(playerId, out var legacyName))
                return "Forgotten";
            
            int yearsSince = currentYear - legacyName.FirstRecorded.Year;
            
            if (yearsSince > 100)
            {
                return $"The legendary {legacyName.HistoricalName}, remembered for {yearsSince} years";
            }
            else if (yearsSince > 50)
            {
                return $"The {legacyName.Reputation} {legacyName.HistoricalName}, still discussed after {yearsSince} years";
            }
            else if (yearsSince > 20)
            {
                return $"{legacyName.HistoricalName}, whose {legacyName.Reputation} presidency shaped modern politics";
            }
            else
            {
                return $"{legacyName.HistoricalName}, the {legacyName.Reputation}";
            }
        }
        
        /// <summary>
        /// Get historical names that future players will encounter
        /// </summary>
        public List<LegacyName> GetHistoricalNamesForNewPlayer(int currentYear, int yearsBack = 100)
        {
            return _legacyNames.Values
                .Where(n => (currentYear - n.FirstRecorded.Year) <= yearsBack)
                .OrderByDescending(n => n.LegacyScore)
                .ThenByDescending(n => n.ReferenceCount)
                .ToList();
        }
        
        /// <summary>
        /// Check if a player's name is cursed or praised by future players
        /// </summary>
        public List<PlayerOpinion> GetPlayerOpinions(string playerId)
        {
            if (!_legacyNames.TryGetValue(playerId, out var legacyName))
                return new List<PlayerOpinion>();
            
            var opinions = new List<PlayerOpinion>();
            
            // Generate opinions based on legacy
            if (legacyName.LegacyScore > 70f)
            {
                opinions.Add(new PlayerOpinion
                {
                    PlayerId = "FuturePlayer1",
                    Opinion = "They were a great leader",
                    Sentiment = OpinionSentiment.Praise
                });
            }
            else if (legacyName.LegacyScore < -50f)
            {
                opinions.Add(new PlayerOpinion
                {
                    PlayerId = "FuturePlayer2",
                    Opinion = "They ruined everything",
                    Sentiment = OpinionSentiment.Curse
                });
            }
            
            return opinions;
        }
        
        /// <summary>
        /// Generate a historical name based on reputation
        /// </summary>
        private string GenerateHistoricalName(string originalName, string reputation)
        {
            if (reputation == "The Great")
                return $"{originalName} the Great";
            if (reputation == "The Corrupt")
                return $"The Corrupt {originalName}";
            if (reputation == "The Disgraced")
                return $"The Disgraced {originalName}";
            if (reputation == "The Reformer")
                return $"{originalName} the Reformer";
            
            return originalName;
        }
        
        /// <summary>
        /// Get how many times a player's name has been referenced
        /// </summary>
        public int GetReferenceCount(string playerId)
        {
            if (!_legacyNames.TryGetValue(playerId, out var legacyName))
                return 0;
            
            return legacyName.ReferenceCount;
        }
    }
    
    #region Data Structures
    
    [Serializable]
    public class LegacyName
    {
        public string PlayerId;
        public string OriginalName;
        public string HistoricalName;
        public float LegacyScore;
        public string Reputation;
        public DateTime FirstRecorded;
        public DateTime LastReferenced;
        public int ReferenceCount;
        public List<HistoricalReference> HistoricalReferences;
    }
    
    [Serializable]
    public class HistoricalReference
    {
        public int Year;
        public string Context;
        public string ReferencedBy;
    }
    
    [Serializable]
    public class PlayerOpinion
    {
        public string PlayerId;
        public string Opinion;
        public OpinionSentiment Sentiment;
    }
    
    public enum OpinionSentiment
    {
        Praise,
        Curse,
        Neutral,
        Respect,
        Contempt
    }
    
    #endregion
}

