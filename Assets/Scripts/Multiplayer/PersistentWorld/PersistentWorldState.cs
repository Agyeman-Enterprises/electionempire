using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - Persistent World State
// Shared world that accumulates player decisions over time
// ═══════════════════════════════════════════════════════════════════════════════

namespace ElectionEmpire.Multiplayer.PersistentWorld
{
    /// <summary>
    /// The persistent world state that all players share.
    /// Accumulates changes from every player's actions.
    /// </summary>
    [Serializable]
    public class PersistentWorldState
    {
        // World Timeline
        public int CurrentYear;
        public DateTime WorldStartDate;
        public DateTime LastUpdated;
        
        // Global Metrics (0-100 scale)
        public float WorldCorruption;           // How corrupt the world is
        public float VoterCynicism;              // How much voters trust politicians
        public float MediaCredibility;          // How much media is trusted
        public float EconomicStability;         // Economic health
        public float SocialCohesion;            // How divided society is
        public float PoliticalPolarization;     // Left/right divide
        
        // Historical Events
        public List<WorldEvent> HistoricalEvents;
        
        // Political Landscape
        public List<PoliticalDynasty> Dynasties;
        public List<GhostPresident> FormerPresidents;
        public List<GhostPolitician> FormerPoliticians;
        
        // Current Office Holders (ghosts)
        public Dictionary<string, string> CurrentOfficeHolders; // Office ID -> Ghost ID
        
        // Policy Legacy
        public Dictionary<string, float> PolicyLegacies; // Policy name -> impact score
        public List<string> ActivePolicies;
        public List<string> RepealedPolicies;
        
        // Regional States
        public Dictionary<string, RegionState> RegionStates;
        
        // World Reputation
        public Dictionary<string, float> WorldReputationTags; // Tag -> strength
        
        public PersistentWorldState()
        {
            CurrentYear = 2025;
            WorldStartDate = DateTime.Now;
            LastUpdated = DateTime.Now;
            HistoricalEvents = new List<WorldEvent>();
            Dynasties = new List<PoliticalDynasty>();
            FormerPresidents = new List<GhostPresident>();
            FormerPoliticians = new List<GhostPolitician>();
            CurrentOfficeHolders = new Dictionary<string, string>();
            PolicyLegacies = new Dictionary<string, float>();
            ActivePolicies = new List<string>();
            RepealedPolicies = new List<string>();
            RegionStates = new Dictionary<string, RegionState>();
            WorldReputationTags = new Dictionary<string, float>();
            
            // Initialize default values
            WorldCorruption = 30f;
            VoterCynicism = 40f;
            MediaCredibility = 60f;
            EconomicStability = 70f;
            SocialCohesion = 60f;
            PoliticalPolarization = 50f;
        }
        
        /// <summary>
        /// Apply effects from a player's presidency/term.
        /// </summary>
        public void ApplyPlayerTermEffects(PlayerTermRecord record)
        {
            LastUpdated = DateTime.Now;
            
            // Update global metrics based on player actions
            if (record.CorruptionLevel > 0.5f)
            {
                WorldCorruption = Mathf.Clamp(WorldCorruption + 20f, 0f, 100f);
                VoterCynicism = Mathf.Clamp(VoterCynicism + 15f, 0f, 100f);
            }
            
            if (record.ScandalsCount > 5)
            {
                MediaCredibility = Mathf.Clamp(MediaCredibility - 10f, 0f, 100f);
                VoterCynicism = Mathf.Clamp(VoterCynicism + 10f, 0f, 100f);
            }
            
            if (record.PoliciesImplemented.Count > 0)
            {
                foreach (var policy in record.PoliciesImplemented)
                {
                    if (!ActivePolicies.Contains(policy))
                        ActivePolicies.Add(policy);
                    
                    if (!PolicyLegacies.ContainsKey(policy))
                        PolicyLegacies[policy] = 0f;
                    PolicyLegacies[policy] += record.PolicyImpact;
                }
            }
            
            // Create ghost if they reached high office
            if (record.HighestOfficeTier >= 4) // National level
            {
                var ghost = new GhostPresident
                {
                    PlayerId = record.PlayerId,
                    PlayerName = record.PlayerName,
                    BehaviorProfile = record.BehaviorProfile,
                    TermStartYear = record.TermStartYear,
                    TermEndYear = record.TermEndYear,
                    Legacy = record.Legacy,
                    Reputation = record.Reputation,
                    PoliciesEnacted = new List<string>(record.PoliciesImplemented),
                    ScandalsSurvived = record.ScandalsCount
                };
                
                FormerPresidents.Add(ghost);
                
                // Add to historical events
                HistoricalEvents.Add(new WorldEvent
                {
                    Year = record.TermStartYear,
                    EventType = "Presidency",
                    Description = $"{record.PlayerName} became President",
                    Impact = record.Legacy
                });
            }
            else if (record.HighestOfficeTier >= 2)
            {
                // Create regular ghost politician
                var ghost = new GhostPolitician
                {
                    PlayerId = record.PlayerId,
                    PlayerName = record.PlayerName,
                    BehaviorProfile = record.BehaviorProfile,
                    HighestOffice = record.HighestOffice,
                    YearsActive = record.TermEndYear - record.TermStartYear
                };
                
                FormerPoliticians.Add(ghost);
            }
            
            // Update world reputation tags
            foreach (var tag in record.ReputationTags)
            {
                if (!WorldReputationTags.ContainsKey(tag))
                    WorldReputationTags[tag] = 0f;
                WorldReputationTags[tag] = Mathf.Clamp(WorldReputationTags[tag] + 5f, 0f, 100f);
            }
        }
        
        /// <summary>
        /// Get all active ghosts that new players might face.
        /// </summary>
        public List<GhostPolitician> GetActiveGhosts(int currentYear)
        {
            var active = new List<GhostPolitician>();
            
            // Former presidents are always active (they're "elder statesmen")
            active.AddRange(FormerPresidents);
            
            // Recent politicians (within last 20 years)
            active.AddRange(FormerPoliticians.Where(g => 
                (currentYear - g.YearsActive) < 20));
            
            return active;
        }
        
        /// <summary>
        /// Get the world state description for new players.
        /// </summary>
        public string GetWorldStateDescription()
        {
            var desc = $"The year is {CurrentYear}. ";
            
            if (WorldCorruption > 70f)
                desc += "Corruption has reached unprecedented levels. ";
            else if (WorldCorruption < 30f)
                desc += "The political system remains relatively clean. ";
            
            if (VoterCynicism > 70f)
                desc += "Voters have lost all faith in politicians. ";
            else if (VoterCynicism < 30f)
                desc += "Voters still believe in the system. ";
            
            if (FormerPresidents.Count > 0)
            {
                desc += $"{FormerPresidents.Count} former president(s) still influence politics. ";
            }
            
            if (Dynasties.Count > 0)
            {
                desc += $"{Dynasties.Count} political dynasty(ies) control key positions. ";
            }
            
            return desc;
        }
    }
    
    [Serializable]
    public class WorldEvent
    {
        public int Year;
        public string EventType;
        public string Description;
        public float Impact; // -100 to 100
        public List<string> AffectedRegions;
        public string CausedByPlayerId;
    }
    
    [Serializable]
    public class PoliticalDynasty
    {
        public string DynastyName;
        public string FounderPlayerId;
        public List<string> MemberPlayerIds;
        public int Generations;
        public float Influence; // 0-100
        public List<string> ControlledOffices;
    }
    
    [Serializable]
    public class RegionState
    {
        public string RegionId;
        public string RegionName;
        public float Corruption;
        public float EconomicHealth;
        public float VoterTurnout;
        public string DominantParty;
        public List<string> RecentOfficeHolders;
    }
    
    [Serializable]
    public class PlayerTermRecord
    {
        public string PlayerId;
        public string PlayerName;
        public int TermStartYear;
        public int TermEndYear;
        public int HighestOfficeTier;
        public string HighestOffice;
        public float CorruptionLevel;
        public int ScandalsCount;
        public List<string> PoliciesImplemented;
        public float PolicyImpact;
        public float Legacy;
        public List<string> ReputationTags;
        public PlayerBehaviorProfile BehaviorProfile;
        public TermEndReason TermEndReason;
    }
    
    public enum TermEndReason
    {
        TermLimit,
        ElectoralDefeat,
        Resignation,
        Impeachment,
        Assassination,
        Dictatorship
    }
}

