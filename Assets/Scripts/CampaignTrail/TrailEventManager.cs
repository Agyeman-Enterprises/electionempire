// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - TRAIL EVENT MANAGER
// Main controller for campaign trail events, encounters, and reporter ambushes
// ═══════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionEmpire.CampaignTrail
{
    /// <summary>
    /// Event fired when an encounter begins
    /// </summary>
    public class EncounterStartedEvent
    {
        public TownsfolkEncounter Encounter;
        public TrailEvent ParentEvent;
    }
    
    /// <summary>
    /// Event fired when an encounter is resolved
    /// </summary>
    public class EncounterResolvedEvent
    {
        public TownsfolkEncounter Encounter;
        public EncounterResolutionResult Result;
        public TrailEvent ParentEvent;
    }
    
    /// <summary>
    /// Event fired when a trail event completes
    /// </summary>
    public class TrailEventCompletedEvent
    {
        public TrailEvent Event;
        public TrailEventResult Result;
    }
    
    /// <summary>
    /// Main manager for campaign trail events
    /// </summary>
    public class TrailEventManager
    {
        #region Events
        
        public event Action<EncounterStartedEvent> OnEncounterStarted;
        public event Action<EncounterResolvedEvent> OnEncounterResolved;
        public event Action<TrailEventCompletedEvent> OnTrailEventCompleted;
        public event Action<ReporterAmbush> OnReporterAmbush;
        public event Action<string> OnHeadlineGenerated;
        public event Action<SecretType, string> OnSecretExposed;
        
        #endregion
        
        #region Dependencies
        
        private readonly CitizenGenerator _citizenGenerator;
        private readonly EncounterGenerator _encounterGenerator;
        private readonly EncounterResolver _encounterResolver;
        private readonly IntrepidReporterSystem _reporterSystem;
        private readonly TrailEventConfig _config;
        
        #endregion
        
        #region State
        
        private TrailEvent _currentEvent;
        private TownsfolkEncounter _currentEncounter;
        private ReporterAmbush _pendingAmbush;
        private Queue<TownsfolkEncounter> _encounterQueue;
        private List<TrailEvent> _completedEvents;
        private string _candidateName;
        private IPlayerStats _playerStats;
        
        #endregion
        
        #region Location Data
        
        private static readonly Dictionary<TrailEventType, string[]> LocationNames = new Dictionary<TrailEventType, string[]>
        {
            { TrailEventType.TownHall, new[] {
                "Riverside Community Center", "Memorial Town Hall", "Jefferson Civic Center",
                "Lincoln Public Meeting Hall", "Downtown Community House", "Veterans Memorial Building"
            }},
            { TrailEventType.DinerVisit, new[] {
                "Rosie's Roadside Diner", "Main Street Cafe", "The Blue Plate Special",
                "Mom's Home Cooking", "The Greasy Spoon", "Sunrise Breakfast Joint"
            }},
            { TrailEventType.FactoryTour, new[] {
                "Allied Manufacturing Plant", "Midwest Steel Works", "American Auto Assembly",
                "Regional Processing Facility", "Industrial Park Complex", "Former Glory Manufacturing"
            }},
            { TrailEventType.BarVisit, new[] {
                "The Rusty Nail", "O'Malley's Pub", "Blue Collar Tavern",
                "The Working Man's Bar", "Corner Pocket Saloon", "The Dive"
            }},
            { TrailEventType.ChurchService, new[] {
                "First Baptist Church", "St. Mary's Catholic Church", "Grace Community Church",
                "Riverside Methodist", "Faith Temple", "Holy Cross Lutheran"
            }},
            { TrailEventType.CollegeCampus, new[] {
                "State University Main Quad", "Community College Student Union", "Tech Institute Commons",
                "Liberal Arts College Green", "University Student Center", "Campus Free Speech Zone"
            }},
            { TrailEventType.FairgroundsRally, new[] {
                "County Fairgrounds Main Stage", "Agricultural Exhibition Hall", "State Fair Arena",
                "Community Festival Grounds", "Regional Expo Center", "Freedom Park Amphitheater"
            }},
            { TrailEventType.UnionHall, new[] {
                "Local 302 Union Hall", "Steelworkers Association Building", "Teamsters Local Meeting Room",
                "AFL-CIO Regional Office", "United Workers Headquarters", "Labor Council Hall"
            }},
            { TrailEventType.SeniorCenter, new[] {
                "Golden Years Senior Center", "Community Elder Care Facility", "Retired Citizens Club",
                "Silver Linings Activity Center", "Sunset Living Community Room", "Heritage Senior Services"
            }},
            { TrailEventType.MainStreetWalk, new[] {
                "Historic Main Street District", "Downtown Shopping District", "Small Business Row",
                "Heritage Square", "Town Center Retail Area", "Community Business District"
            }}
        };
        
        private static readonly Dictionary<TrailEventType, string[]> LocationDescriptions = new Dictionary<TrailEventType, string[]>
        {
            { TrailEventType.TownHall, new[] {
                "Rows of folding chairs face a wooden podium. American flags flank the stage.",
                "Fluorescent lights buzz overhead. The crowd fidgets in uncomfortable seats.",
                "A hand-lettered banner reads 'MEET YOUR CANDIDATE'. Coffee urns line the back wall."
            }},
            { TrailEventType.DinerVisit, new[] {
                "The smell of bacon and coffee fills the air. Locals eye you over their newspapers.",
                "Chrome fixtures reflect the morning light. A waitress with tired eyes approaches.",
                "Country music plays on a jukebox. Every booth has someone with an opinion."
            }},
            { TrailEventType.FactoryTour, new[] {
                "The machinery stands silent - a monument to jobs that left years ago.",
                "Workers in hard hats pause their tasks to size you up.",
                "The foreman's skeptical expression says more than words ever could."
            }},
            { TrailEventType.BarVisit, new[] {
                "The air is thick with decades of spilled beer and broken dreams.",
                "Neon signs cast red shadows over weathered faces. Pool balls crack in the corner.",
                "Everyone stops talking when you walk in. This is their territory."
            }}
        };
        
        #endregion
        
        public TrailEventManager(TrailEventConfig config = null)
        {
            _config = config ?? new TrailEventConfig();
            _citizenGenerator = new CitizenGenerator();
            _encounterGenerator = new EncounterGenerator();
            _encounterResolver = new EncounterResolver();
            _reporterSystem = new IntrepidReporterSystem();
            _encounterQueue = new Queue<TownsfolkEncounter>();
            _completedEvents = new List<TrailEvent>();
        }
        
        #region Initialization
        
        /// <summary>
        /// Initialize the manager with player information
        /// </summary>
        public void Initialize(string candidateName, IPlayerStats playerStats)
        {
            _candidateName = candidateName;
            _playerStats = playerStats;
            
            // Initialize the reporter
            _reporterSystem.GenerateReporter();
        }
        
        #endregion
        
        #region Event Generation
        
        /// <summary>
        /// Start a new campaign trail event
        /// </summary>
        public TrailEvent StartEvent(TrailEventType eventType)
        {
            _currentEvent = GenerateTrailEvent(eventType);
            _encounterQueue.Clear();
            
            // Queue up encounters
            foreach (var encounter in _currentEvent.PlannedEncounters)
            {
                _encounterQueue.Enqueue(encounter);
            }
            
            // Check for reporter ambush
            if (_reporterSystem.ShouldAmbush(eventType, _playerStats.OfficeTier, _config.ChaosMultiplier))
            {
                _pendingAmbush = _reporterSystem.GenerateAmbush(eventType, _playerStats.OfficeTier);
                
                // Insert ambush at random point
                int ambushPosition = UnityEngine.Random.Range(0, _encounterQueue.Count);
                var encounters = _encounterQueue.ToList();
                _encounterQueue.Clear();
                
                for (int i = 0; i < encounters.Count; i++)
                {
                    if (i == ambushPosition && _pendingAmbush != null)
                    {
                        // Ambush will be handled separately
                    }
                    _encounterQueue.Enqueue(encounters[i]);
                }
            }
            
            return _currentEvent;
        }
        
        /// <summary>
        /// Generate a complete trail event
        /// </summary>
        private TrailEvent GenerateTrailEvent(TrailEventType eventType)
        {
            var trailEvent = new TrailEvent
            {
                Type = eventType,
                LocationName = GetLocationName(eventType),
                LocationDescription = GetLocationDescription(eventType),
                EventTime = DateTime.Now,
                ExpectedAttendance = EstimateAttendance(eventType),
                HostilityLevel = DetermineHostility(eventType),
                PressExpected = ShouldPressAttend(eventType),
                PressPresent = UnityEngine.Random.value < GetPressChance(eventType),
                ReporterAmbushPending = false
            };
            
            // Calculate actual attendance (might differ from expected)
            trailEvent.ActualAttendance = CalculateActualAttendance(trailEvent.ExpectedAttendance);
            
            // Determine reporter count
            if (trailEvent.PressPresent)
            {
                trailEvent.ReportersPresent = UnityEngine.Random.Range(1, Mathf.Min(_config.MaxSimultaneousReporters, 
                    1 + _playerStats.OfficeTier));
            }
            
            // Generate citizens present
            int citizenCount = UnityEngine.Random.Range(5, 15);
            for (int i = 0; i < citizenCount; i++)
            {
                trailEvent.CitizensPresent.Add(_citizenGenerator.GenerateCitizen());
            }
            
            // Generate planned encounters
            int encounterCount = UnityEngine.Random.Range(_config.MinEncountersPerEvent, _config.MaxEncountersPerEvent + 1);
            trailEvent.PlannedEncounters = GenerateEncounterSet(eventType, trailEvent.HostilityLevel, 
                encounterCount, trailEvent.PressPresent);
            
            return trailEvent;
        }
        
        /// <summary>
        /// Generate a set of encounters for an event
        /// </summary>
        private List<TownsfolkEncounter> GenerateEncounterSet(
            TrailEventType eventType, 
            CrowdHostility hostility,
            int count,
            bool pressPresent)
        {
            var encounters = new List<TownsfolkEncounter>();
            
            for (int i = 0; i < count; i++)
            {
                // Determine encounter type based on probabilities
                bool forceHostile = UnityEngine.Random.value < (_config.HostileEncounterChance * 
                    (1 + _config.HostilityByOfficeTier * _playerStats.OfficeTier));
                bool forceSecret = UnityEngine.Random.value < _config.SecretWitnessChance;
                bool projectile = UnityEngine.Random.value < _config.ProjectileChance;
                bool childEncounter = UnityEngine.Random.value < 0.1f; // 10% chance of adorable/devastating child
                
                TownsfolkEncounter encounter;
                
                if (projectile && forceHostile)
                {
                    encounter = _encounterGenerator.GenerateProjectileEncounter();
                }
                else if (childEncounter)
                {
                    encounter = _encounterGenerator.GenerateChildEncounter();
                }
                else if (forceSecret)
                {
                    encounter = _encounterGenerator.GenerateSecretWitnessEncounter();
                }
                else
                {
                    encounter = _encounterGenerator.GenerateEncounter(
                        eventType, hostility, pressPresent, forceHostile);
                }
                
                encounters.Add(encounter);
            }
            
            // Shuffle to randomize order
            return encounters.OrderBy(x => UnityEngine.Random.value).ToList();
        }
        
        #endregion
        
        #region Encounter Flow
        
        /// <summary>
        /// Get the next encounter in the queue
        /// </summary>
        public TownsfolkEncounter GetNextEncounter()
        {
            // Check for pending reporter ambush
            if (_pendingAmbush != null && UnityEngine.Random.value < 0.3f)
            {
                OnReporterAmbush?.Invoke(_pendingAmbush);
                _pendingAmbush = null;
                return null; // Signal that ambush is happening instead
            }
            
            if (_encounterQueue.Count == 0)
            {
                return null;
            }
            
            _currentEncounter = _encounterQueue.Dequeue();
            
            OnEncounterStarted?.Invoke(new EncounterStartedEvent
            {
                Encounter = _currentEncounter,
                ParentEvent = _currentEvent
            });
            
            return _currentEncounter;
        }
        
        /// <summary>
        /// Check if there are more encounters
        /// </summary>
        public bool HasMoreEncounters()
        {
            return _encounterQueue.Count > 0 || _pendingAmbush != null;
        }
        
        /// <summary>
        /// Resolve the current encounter with the player's chosen response
        /// </summary>
        public EncounterResolutionResult ResolveCurrentEncounter(EncounterChoice choice)
        {
            if (_currentEncounter == null)
            {
                throw new InvalidOperationException("No current encounter to resolve");
            }
            
            var result = _encounterResolver.ResolveEncounter(
                _currentEncounter, 
                choice, 
                _playerStats, 
                _candidateName
            );
            
            // Add to completed
            _currentEvent.CompletedEncounters.Add(_currentEncounter);
            
            // Fire events
            OnEncounterResolved?.Invoke(new EncounterResolvedEvent
            {
                Encounter = _currentEncounter,
                Result = result,
                ParentEvent = _currentEvent
            });
            
            if (!string.IsNullOrEmpty(result.HeadlineGenerated))
            {
                OnHeadlineGenerated?.Invoke(result.HeadlineGenerated);
            }
            
            if (result.SecretExposed && result.ExposedSecretType.HasValue)
            {
                OnSecretExposed?.Invoke(result.ExposedSecretType.Value, 
                    _currentEncounter.SecretDetails);
            }
            
            _currentEncounter = null;
            return result;
        }
        
        /// <summary>
        /// Resolve a reporter ambush
        /// </summary>
        public EncounterResolutionResult ResolveReporterAmbush(EncounterChoice choice)
        {
            if (_pendingAmbush == null)
            {
                throw new InvalidOperationException("No pending ambush to resolve");
            }
            
            // Create a pseudo-encounter for the ambush
            var ambushEncounter = new TownsfolkEncounter
            {
                Name = _pendingAmbush.Reporter.Name,
                Occupation = "reporter",
                Appearance = _pendingAmbush.Reporter.Appearance,
                EncounterType = "reporter_ambush",
                IsHostile = _pendingAmbush.Reporter.RelationshipScore < -20,
                HasAudience = true,
                AudienceSize = _pendingAmbush.CrowdWatching + _pendingAmbush.OtherReportersAttracted * 10,
                IsBeingRecorded = _pendingAmbush.CameraRolling,
                PressPresent = true,
                LiveBroadcast = _pendingAmbush.LiveBroadcast,
                OpeningDialogue = _pendingAmbush.Questions.FirstOrDefault(),
                Choices = _pendingAmbush.ResponseOptions
            };
            
            var result = _encounterResolver.ResolveEncounter(
                ambushEncounter,
                choice,
                _playerStats,
                _candidateName
            );
            
            // Update reporter relationship
            _reporterSystem.UpdateRelationship(result, choice);
            
            _pendingAmbush = null;
            return result;
        }
        
        #endregion
        
        #region Event Completion
        
        /// <summary>
        /// Complete the current trail event and get results
        /// </summary>
        public TrailEventResult CompleteEvent()
        {
            if (_currentEvent == null)
            {
                throw new InvalidOperationException("No current event to complete");
            }
            
            var result = CalculateEventResult();
            _currentEvent.Result = result;
            _completedEvents.Add(_currentEvent);
            
            OnTrailEventCompleted?.Invoke(new TrailEventCompletedEvent
            {
                Event = _currentEvent,
                Result = result
            });
            
            // Progress reporter investigation
            _reporterSystem.OnTurnAdvance(
                _playerStats.OfficeTier, 
                _playerStats.CurrentApproval,
                result.SecretsRevealed.Any()
            );
            
            _currentEvent = null;
            return result;
        }
        
        /// <summary>
        /// Calculate the overall result of the event
        /// </summary>
        private TrailEventResult CalculateEventResult()
        {
            var result = new TrailEventResult();
            
            foreach (var encounter in _currentEvent.CompletedEncounters)
            {
                result.TotalEncounters++;
                
                switch (encounter.Outcome)
                {
                    case EncounterOutcome.Triumph:
                    case EncounterOutcome.Positive:
                        result.PositiveEncounters++;
                        break;
                    case EncounterOutcome.Neutral:
                        result.NeutralEncounters++;
                        break;
                    case EncounterOutcome.Negative:
                        result.NegativeEncounters++;
                        break;
                    case EncounterOutcome.Disaster:
                        result.DisasterEncounters++;
                        break;
                    case EncounterOutcome.ViralMoment:
                        result.ViralMoments++;
                        break;
                    case EncounterOutcome.SecretRevealed:
                        result.SecretsRevealed.Add(encounter.SecretDetails);
                        break;
                }
                
                // Collect headlines
                if (!string.IsNullOrEmpty(encounter.HeadlineGenerated))
                {
                    result.HeadlinesGenerated.Add(encounter.HeadlineGenerated);
                }
                
                // Track memorable moments
                if (encounter.Outcome == EncounterOutcome.Triumph || 
                    encounter.Outcome == EncounterOutcome.Disaster ||
                    encounter.Outcome == EncounterOutcome.ViralMoment)
                {
                    result.MemorableMoments.Add(encounter.OutcomeDescription);
                }
            }
            
            // Determine overall outcome
            if (result.DisasterEncounters > 0)
            {
                result.OverallOutcome = EncounterOutcome.Negative;
            }
            else if (result.PositiveEncounters > result.NegativeEncounters * 2)
            {
                result.OverallOutcome = EncounterOutcome.Positive;
            }
            else if (result.NegativeEncounters > result.PositiveEncounters)
            {
                result.OverallOutcome = EncounterOutcome.Negative;
            }
            else
            {
                result.OverallOutcome = EncounterOutcome.Neutral;
            }
            
            // Select headline of the day
            result.HeadlineOfTheDay = result.HeadlinesGenerated.FirstOrDefault() 
                ?? $"{_candidateName} Campaigns at {_currentEvent.LocationName}";
            
            // Select most memorable moment
            result.MostMemorableMoment = result.MemorableMoments.FirstOrDefault();
            
            return result;
        }
        
        #endregion
        
        #region Helper Methods
        
        private string GetLocationName(TrailEventType eventType)
        {
            if (LocationNames.TryGetValue(eventType, out var names))
            {
                return names[UnityEngine.Random.Range(0, names.Length)];
            }
            return "Campaign Event Location";
        }
        
        private string GetLocationDescription(TrailEventType eventType)
        {
            if (LocationDescriptions.TryGetValue(eventType, out var descriptions))
            {
                return descriptions[UnityEngine.Random.Range(0, descriptions.Length)];
            }
            return "A typical campaign stop.";
        }
        
        private int EstimateAttendance(TrailEventType eventType)
        {
            switch (eventType)
            {
                case TrailEventType.FairgroundsRally: return UnityEngine.Random.Range(500, 5000);
                case TrailEventType.TownHall: return UnityEngine.Random.Range(50, 300);
                case TrailEventType.CollegeCampus: return UnityEngine.Random.Range(100, 1000);
                case TrailEventType.ChurchService: return UnityEngine.Random.Range(50, 500);
                case TrailEventType.UnionHall: return UnityEngine.Random.Range(30, 200);
                case TrailEventType.DinerVisit: return UnityEngine.Random.Range(10, 40);
                case TrailEventType.BarVisit: return UnityEngine.Random.Range(15, 50);
                default: return UnityEngine.Random.Range(30, 150);
            }
        }
        
        private int CalculateActualAttendance(int expected)
        {
            // Attendance can vary ±30% from expected
            float variance = UnityEngine.Random.Range(-0.3f, 0.3f);
            return Mathf.Max(5, Mathf.RoundToInt(expected * (1 + variance)));
        }
        
        private CrowdHostility DetermineHostility(TrailEventType eventType)
        {
            // Base hostility varies by event type
            float hostilityScore = 0.5f;
            
            switch (eventType)
            {
                case TrailEventType.FairgroundsRally:
                case TrailEventType.ChurchService:
                    hostilityScore = 0.2f; // Usually friendly
                    break;
                case TrailEventType.TownHall:
                    hostilityScore = 0.5f; // Mixed
                    break;
                case TrailEventType.BarVisit:
                    hostilityScore = 0.6f; // Can be rough
                    break;
                case TrailEventType.FactoryTour:
                    hostilityScore = 0.4f; // Depends on economic conditions
                    break;
                case TrailEventType.ProtestEncounter:
                    hostilityScore = 0.9f; // Very hostile
                    break;
            }
            
            // Modify by approval rating
            hostilityScore += (50f - _playerStats.CurrentApproval) / 100f;
            
            // Clamp and convert to enum
            if (hostilityScore < 0.2f) return CrowdHostility.Friendly;
            if (hostilityScore < 0.35f) return CrowdHostility.Mixed;
            if (hostilityScore < 0.5f) return CrowdHostility.Neutral;
            if (hostilityScore < 0.65f) return CrowdHostility.Skeptical;
            if (hostilityScore < 0.8f) return CrowdHostility.Hostile;
            return CrowdHostility.Dangerous;
        }
        
        private bool ShouldPressAttend(TrailEventType eventType)
        {
            float chance = 0.3f + (_playerStats.OfficeTier * 0.15f);
            
            // Some events are more press-friendly
            switch (eventType)
            {
                case TrailEventType.FairgroundsRally:
                case TrailEventType.TownHall:
                    chance += 0.2f;
                    break;
                case TrailEventType.BarVisit:
                case TrailEventType.ChurchService:
                    chance -= 0.1f;
                    break;
            }
            
            return UnityEngine.Random.value < chance;
        }
        
        private float GetPressChance(TrailEventType eventType)
        {
            float baseChance = 0.3f + (_playerStats.OfficeTier * 0.1f);
            return Mathf.Min(baseChance, 0.9f);
        }
        
        #endregion
        
        #region Public Queries
        
        /// <summary>
        /// Get the current reporter and their investigation status
        /// </summary>
        public (IntrepidReporter reporter, string warning) GetReporterStatus()
        {
            var reporter = _reporterSystem.GetReporter();
            var warning = _reporterSystem.GetInvestigationWarning();
            return (reporter, warning);
        }
        
        /// <summary>
        /// Get history of completed events
        /// </summary>
        public IReadOnlyList<TrailEvent> GetCompletedEvents()
        {
            return _completedEvents.AsReadOnly();
        }
        
        /// <summary>
        /// Get the current event in progress
        /// </summary>
        public TrailEvent GetCurrentEvent()
        {
            return _currentEvent;
        }
        
        #endregion
    }
}

