// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - CAMPAIGN TRAIL TYPES
// Core enums, data structures, and type definitions for street-level encounters
// ═══════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionEmpire.CampaignTrail
{
    #region Enums
    
    /// <summary>
    /// Types of campaign trail events/locations
    /// </summary>
    public enum TrailEventType
    {
        // Planned Events - Standard Campaign Stops
        Walkabout,              // Walking through town, meeting random people
        TownHall,               // Q&A with audience - high risk, high reward
        DinerVisit,             // Classic "eating at local diner" photo op
        FactoryTour,            // Blue collar appeal, rust belt energy
        FarmVisit,              // Rural appeal, agricultural community
        SchoolVisit,            // Education focus, family values
        ChurchService,          // Faith community outreach
        VFWHall,                // Veterans community
        UnionHall,              // Labor endorsement territory
        CommunityCenter,        // General community outreach
        MainStreetWalk,         // Small business retail politics
        FairgroundsRally,       // Large outdoor event - maximum exposure
        BarVisit,               // "Regular person" appeal - RISKY!
        SeniorCenter,           // Elder voter outreach
        CollegeCampus,          // Youth vote mobilization
        HousingProject,         // Low-income community outreach
        SuburbanCookout,        // Middle class backyard politics
        
        // Unplanned Events - Things That Just Happen
        AmbushInterview,        // Reporter catches you off-guard
        ProtestEncounter,       // Stumble into opposition protest
        HecklerConfrontation,   // Someone really wants to yell at you
        SecretWitnessEncounter, // Someone who knows something
        ViralMoment,            // Unexpected thing that could go either way
        ChildEncounter,         // Kid asks innocent but devastating question
        CelebrityEncounter      // Famous person either helps or hurts
    }
    
    /// <summary>
    /// How hostile is the crowd/environment
    /// </summary>
    public enum CrowdHostility
    {
        Friendly,       // Home turf, supporters
        Mixed,          // Some supporters, some opposition
        Neutral,        // Undecided, waiting to judge
        Skeptical,      // Leaning negative but persuadable
        Hostile,        // Actively antagonistic
        Dangerous       // Security concerns, potential violence
    }
    
    /// <summary>
    /// Citizen disposition toward politicians generally
    /// </summary>
    public enum CitizenDisposition
    {
        TrueBeliever,       // Passionate supporter
        Supporter,          // Supports the candidate
        Hopeful,            // Wants to believe
        Undecided,          // Genuinely weighing options
        Skeptical,          // Doubts all politicians
        Cynical,            // Believes all are corrupt
        Hostile,            // Actively opposes you
        HostileOpponent,    // Strongly opposes the candidate
        Heckler,            // Actively heckling/disrupting
        Apathetic,          // Doesn't care, won't vote
        SingleIssue,        // Only cares about one thing
        Grievance,          // Has specific complaint
        SecretsToTell       // Knows something damaging
    }
    
    /// <summary>
    /// What the citizen might do during encounter
    /// </summary>
    public enum CitizenAction
    {
        // Positive
        Cheer,
        AskForSelfie,
        SharePersonalStory,
        OfferEndorsement,
        VolunteerToHelp,
        MakeSmallDonation,
        
        // Neutral
        AskQuestion,
        ListenQuietly,
        RecordOnPhone,
        WatchFromDistance,
        IgnoreCompletely,
        
        // Negative  
        HeckleLoudly,
        AskGotchaQuestion,
        BringUpPastFailure,
        ThrowProjectile,
        RevealSecret,
        ConfrontAboutBrokenPromise,
        CreateScene,
        CallYouALiar,
        DemandAnswers,
        
        // Special
        IntroduceSomeoneImportant,
        SetUpAmbush,
        RecordForViralVideo,
        MentionYourOpponent
    }
    
    /// <summary>
    /// Actions the candidate (player) can take during encounters
    /// </summary>
    public enum CandidateAction
    {
        // Defensive/Evasion
        Duck,
        CatchProjectile,
        StandYourGround,
        GetInCar,
        HaveSecurityIntervene,
        
        // Engagement
        Ignore,
        Acknowledge,
        ShakeHand,
        TakePhoto,
        HoldBaby,
        ListenIntently,
        
        // Communication
        OfferToTalkPrivately,
        ConfrontDirectly,
        ClapBack,
        MakePromise,
        GiveBusinessCard
    }
    
    /// <summary>
    /// Types of secrets/dirt a witness might reveal
    /// </summary>
    public enum SecretType
    {
        // Personal
        AffairEvidence,
        SubstanceAbuse,
        EmbarrassingPhoto,
        HiddenFamily,
        PastArrest,
        CollegeIndiscretion,
        
        // Financial
        TaxEvasion,
        HiddenWealth,
        BriberyEvidence,
        ConflictOfInterest,
        CampaignFinanceViolation,
        
        // Professional
        EmployeeAbuse,
        DiscriminationHistory,
        FailedBusinessVenture,
        LawsuitSettlement,
        
        // Political
        SecretMeeting,
        PolicyFlipFlop,
        LobbyistConnection,
        BackroomDeal,
        BrokenPromiseProof
    }
    
    /// <summary>
    /// Types of projectiles that might be thrown
    /// </summary>
    public enum ProjectileType
    {
        Egg,
        Tomato,
        Shoe,
        Milkshake,
        Water,
        Glitter,
        Pie,
        Flour,
        Paint,
        Produce,        // Generic fruits/vegetables
        Beverage,       // Generic drink
        Nothing         // Threw a punch instead
    }
    
    /// <summary>
    /// Outcome quality of an encounter
    /// </summary>
    public enum EncounterOutcome
    {
        Triumph,        // Couldn't have gone better
        Positive,       // Good result, helped campaign
        Neutral,        // Neither helped nor hurt
        Negative,       // Some damage done
        Disaster,       // Major problem
        ViralMoment,    // Could be good or bad, will spread
        SecretRevealed, // Damaging information came out
        Escaped         // Got away before it got worse
    }
    
    /// <summary>
    /// How the encounter affects the news cycle
    /// </summary>
    public enum MediaImpactLevel
    {
        None,           // No coverage
        LocalMention,   // Local news footnote
        LocalHeadline,  // Local news story
        RegionalStory,  // Regional pickup
        NationalMention,// Brief national coverage
        NationalStory,  // National news segment
        ViralSensation, // Social media explosion
        DefiningMoment  // Will be remembered forever
    }
    
    #endregion
    
    #region Core Data Structures
    
    /// <summary>
    /// A citizen the candidate might encounter
    /// </summary>
    [Serializable]
    public class Citizen
    {
        public string Id;
        public string Name;
        public int Age;
        public string Gender;
        public string Occupation;
        public string Appearance;
        public string VoterBloc;
        
        // Personality
        public CitizenDisposition Disposition;
        public float Enthusiasm;        // 0-100, how energetic
        public float Volatility;        // 0-100, how unpredictable
        public float Articulateness;    // 0-100, how well they speak
        
        // State
        public bool IsIntoxicated;
        public bool IsAngry;
        public bool IsArmed;            // With projectiles
        public bool HasCamera;
        public bool IsRecording;        // Currently recording video/audio
        public bool HasSecret;
        public SecretType? SecretKnown;
        public string SecretDetails;
        public bool HasSign;            // Carrying a sign
        public string SignText;         // Text on the sign (if HasSign is true)
        
        // Evidence/Secrets
        public bool HasPhotoEvidence;   // Has photographic evidence
        public float SecretCredibility; // 0-100, how credible is their secret
        
        // Relationships
        public bool KnowsCandidate;     // Have they met before?
        public string HowTheyKnow;      // Context of prior meeting
        public float TrustInCandidate;  // -100 to 100
        public bool HasBeenEngaged;     // Has this citizen been engaged in an encounter?
        
        // What they want
        public string PrimaryGrievance;
        public string GrievanceIfAny;   // Specific grievance if they have one
        public string PrimaryIssue;      // The main policy issue they care about
        public string Question;
        public CitizenAction LikelyAction;
        public int NumberOfChildren;    // Number of children (for family-related encounters)
        
        // Convenience properties
        public SecretType? SecretType => SecretKnown;
        public bool HasSecretAboutCandidate => HasSecret;
        public bool HasProjectiles => IsArmed;
        public bool IsDrunk => IsIntoxicated;
        
        public Citizen()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
    
    /// <summary>
    /// A choice the player can make during an encounter
    /// </summary>
    [Serializable]
    public class EncounterChoice
    {
        public string Id;
        public string Text;                 // What the player says/does
        public CandidateAction Action;       // The action the candidate takes
        public string Outcome;              // What happens as a result
        public string Descriptor;           // Media description of action
        public string ConsequenceText;      // Detailed consequence description
        public EncounterOutcome LeadsTo;    // Where this choice leads
        public string FollowUpText;         // Text for follow-up
        
        // Impacts
        public float TrustChange;           // Public trust change (used by DetailedEncounterGenerator)
        public float TrustImpact;           // Public trust change (used by other systems)
        public float MediaImpact;           // Media influence change
        public float PartyLoyaltyImpact;    // Party reaction
        public float SpecificBlocImpact;    // Impact on specific voter bloc
        public string AffectedBloc;         // Which bloc is affected
        
        // Resource costs
        public bool RequiresResource;       // Political capital, etc.
        public float ResourceCost;
        
        // Risk assessment
        public int RiskLevel;               // 0-100, how risky is this choice
        public float SuccessChance;         // Probability of good outcome
        public bool RequiresSkillCheck;
        public string RequiredAttribute;    // Charisma, Cunning, etc.
        public int DifficultyCheck;
        
        // Flags
        public bool EndsEncounter;
        public bool EscalatesEncounter;
        public bool TriggersFollowUp;
        public string FollowUpEncounterId;
        
        public EncounterChoice()
        {
            Id = Guid.NewGuid().ToString();
            SuccessChance = 0.5f;
        }
    }
    
    /// <summary>
    /// A full encounter with a townsfolk
    /// </summary>
    [Serializable]
    public class TownsfolkEncounter
    {
        public string Id;
        public string EncounterType;        // Category for logging
        
        // The Person
        public string Name;
        public int Age;
        public string Occupation;
        public string Appearance;
        
        // The Setup
        public string OpeningAction;        // What happens as they approach
        public string OpeningDialogue;      // What they say
        public string Context;              // Background situation
        
        // Environment
        public bool IsHostile;
        public bool HasAudience;
        public int AudienceSize;
        public bool IsBeingRecorded;
        public bool PressPresent;
        public bool LiveBroadcast;
        
        // Evidence/Secrets
        public bool HasSecret;
        public bool HasPhotoEvidence;
        public bool HasDocumentEvidence;
        public bool HasWitnessCorroboration;
        public SecretType? SecretType;
        public string SecretDetails;
        
        // Player Options
        public List<EncounterChoice> Choices;
        
        // Resolution
        public EncounterOutcome? Outcome;
        public string OutcomeDescription;
        public string HeadlineGenerated;
        public MediaImpactLevel MediaImpact;
        
        public TownsfolkEncounter()
        {
            Id = Guid.NewGuid().ToString();
            Choices = new List<EncounterChoice>();
        }
    }
    
    /// <summary>
    /// A complete campaign trail event (contains multiple encounters)
    /// </summary>
    [Serializable]
    public class TrailEvent
    {
        public string Id;
        public TrailEventType Type;
        public string LocationName;
        public string LocationDescription;
        public DateTime EventTime;
        
        // Convenience property
        public string Location => LocationName;
        
        // Crowd
        public int ExpectedAttendance;
        public int ActualAttendance;
        public CrowdHostility HostilityLevel;
        public Dictionary<string, float> BlocRepresentation; // Which voter blocs present
        
        // Press
        public bool PressExpected;
        public bool PressPresent;
        public int ReportersPresent;
        public bool LiveCoverage;
        public bool ReporterAmbushPending;
        
        // Encounters
        public List<Citizen> CitizensPresent;
        public List<TownsfolkEncounter> PlannedEncounters;
        public List<TownsfolkEncounter> RandomEncounters;
        public List<TownsfolkEncounter> CompletedEncounters;
        
        // Results
        public TrailEventResult Result;
        
        public TrailEvent()
        {
            Id = Guid.NewGuid().ToString();
            BlocRepresentation = new Dictionary<string, float>();
            CitizensPresent = new List<Citizen>();
            PlannedEncounters = new List<TownsfolkEncounter>();
            RandomEncounters = new List<TownsfolkEncounter>();
            CompletedEncounters = new List<TownsfolkEncounter>();
        }
    }
    
    /// <summary>
    /// Results from completing a trail event
    /// </summary>
    [Serializable]
    public class TrailEventResult
    {
        // Counts
        public int TotalEncounters;
        public int PositiveEncounters;
        public int NeutralEncounters;
        public int NegativeEncounters;
        public int DisasterEncounters;
        public int ViralMoments;
        
        // Resource Changes
        public float NetTrustChange;
        public float NetMediaImpact;
        public float NetFundsChange;
        public float NetPartyLoyaltyChange;
        public Dictionary<string, float> BlocChanges;
        
        // Notable Events
        public List<string> SecretsRevealed;
        public List<string> ProjectilesThrown;
        public List<string> MemorableMoments;
        public List<string> HeadlinesGenerated;
        
        // Special
        public bool ReporterAmbushOccurred;
        public bool SecurityIncident;
        public bool MedicalEmergency;
        public bool ProtestErupted;
        
        // Summary
        public string HeadlineOfTheDay;
        public string MostMemorableMoment;
        public EncounterOutcome OverallOutcome;
        
        public TrailEventResult()
        {
            BlocChanges = new Dictionary<string, float>();
            SecretsRevealed = new List<string>();
            ProjectilesThrown = new List<string>();
            MemorableMoments = new List<string>();
            HeadlinesGenerated = new List<string>();
        }
    }
    
    #endregion
    
    #region Reporter System
    
    /// <summary>
    /// The Intrepid Reporter - a persistent nemesis who pops up at random
    /// </summary>
    [Serializable]
    public class IntrepidReporter
    {
        public string Id;
        public string Name;
        public string Outlet;               // News organization
        public string Appearance;
        public string Personality;
        public string Catchphrase;          // Their signature opening
        
        // Tracking
        public int TimesEncountered;
        public int SuccessfulGotchas;       // Times they got you
        public int TimesEvaded;             // Times you escaped
        public int TimesCharmed;            // Times you won them over
        public List<string> QuestionsAsked;
        public List<string> TopicsInvestigating;
        
        // Relationship
        public float RelationshipScore;     // -100 (nemesis) to 100 (friendly)
        public bool HasGrudge;
        public string GrudgeReason;
        public bool RespectEarned;
        
        // Current Investigation
        public string CurrentStoryAngle;
        public float StoryProgress;         // 0-100, how close to publishing
        public List<string> EvidenceGathered;
        public bool ReadyToPublish;
        
        // Behavior
        public float Persistence;           // 0-100, how hard they push
        public float Ruthlessness;          // 0-100, willingness to ambush
        public float Credibility;           // 0-100, how trusted they are
        public float IntelligenceGathering; // 0-100, how good at finding dirt
        public bool HasDeepThroatSource;    // Has an anonymous insider source
        
        public IntrepidReporter()
        {
            Id = Guid.NewGuid().ToString();
            QuestionsAsked = new List<string>();
            TopicsInvestigating = new List<string>();
            EvidenceGathered = new List<string>();
            Persistence = 70f;
            Ruthlessness = 50f;
            Credibility = 60f;
            IntelligenceGathering = 50f;
        }
    }
    
    /// <summary>
    /// An ambush by the intrepid reporter
    /// </summary>
    [Serializable]
    public class ReporterAmbush
    {
        public string Id;
        public IntrepidReporter Reporter;
        public string Location;
        public string Context;              // Why they're here
        public string OpeningLine;          // Their approach
        public List<string> Questions;      // What they'll ask
        public bool CameraRolling;
        public bool LiveBroadcast;
        public int OtherReportersAttracted; // Did the ambush draw others
        public int CrowdWatching;
        
        // Player options
        public List<EncounterChoice> ResponseOptions;
        
        public ReporterAmbush()
        {
            Id = Guid.NewGuid().ToString();
            Questions = new List<string>();
            ResponseOptions = new List<EncounterChoice>();
        }
    }
    
    #endregion
    
    #region Configuration
    
    /// <summary>
    /// Configuration for trail event generation
    /// </summary>
    [Serializable]
    public class TrailEventConfig
    {
        // Probability weights
        public float HostileEncounterChance;
        public float SecretWitnessChance;
        public float ReporterAmbushChance;
        public float ProjectileChance;
        public float ViralMomentChance;
        
        // Scaling
        public float HostilityByOfficeTier;     // Higher office = more hostile
        public float ScrutinyByApproval;        // Lower approval = more scrutiny
        public float ChaosMultiplier;           // Global chaos setting
        
        // Limits
        public int MaxEncountersPerEvent;
        public int MinEncountersPerEvent;
        public int MaxSimultaneousReporters;
        
        public TrailEventConfig()
        {
            HostileEncounterChance = 0.3f;
            SecretWitnessChance = 0.1f;
            ReporterAmbushChance = 0.2f;
            ProjectileChance = 0.05f;
            ViralMomentChance = 0.15f;
            
            HostilityByOfficeTier = 0.1f;
            ScrutinyByApproval = 0.1f;
            ChaosMultiplier = 1.0f;
            
            MaxEncountersPerEvent = 10;
            MinEncountersPerEvent = 3;
            MaxSimultaneousReporters = 5;
        }
    }
    
    #endregion
}

