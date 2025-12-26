// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - CORE GAME TYPES
// Fundamental type definitions used throughout the entire game
// ═══════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionEmpire.Core
{
    #region Character Enums
    
    /// <summary>
    /// Character background/profession before politics
    /// </summary>
    public enum CharacterBackground
    {
        Businessman,
        LocalPolitician,
        Teacher,
        Doctor,
        PoliceOfficer,
        Journalist,
        Activist,
        ReligiousLeader,
        Lawyer,
        MilitaryVeteran,
        Celebrity,
        UnionLeader
    }
    
    /// <summary>
    /// Political party affiliation
    /// </summary>
    public enum PartyAffiliation
    {
        Democratic,
        Republican,
        Independent,
        Libertarian,
        Green,
        Progressive,
        Conservative,
        Centrist
    }
    
    /// <summary>
    /// Character traits (positive)
    /// </summary>
    public enum PositiveTrait
    {
        SilverTongue,       // +15% debate effectiveness
        BornLeader,         // +10% staff loyalty
        MediaDarling,       // +20% positive media coverage
        PolicyWonk,         // +15% policy effectiveness
        FundraisingGenius,  // +25% campaign fund generation
        CrisisManager,      // +20% crisis handling
        GrassrootsHero,     // +30% volunteer recruitment
        Networker,          // +25% alliance building
        Resilient,          // +20% scandal recovery
        Charismatic         // +15% all speech effects
    }
    
    /// <summary>
    /// Character traits (negative)
    /// </summary>
    public enum NegativeTrait
    {
        SkeletonsInCloset,  // Start with hidden scandal
        FootInMouth,        // 10% gaffe chance
        ThinSkinned,        // -15% when attacked
        Technophobe,        // -20% social media
        ControversialPast,  // -10% trust from random bloc
        Impatient,          // -10% policy effectiveness
        Spendthrift,        // +20% campaign burn rate
        Paranoid,           // -15% staff loyalty
        Arrogant,           // -10% with working class
        Indecisive          // -15% crisis management
    }
    
    /// <summary>
    /// Political office tiers
    /// </summary>
    public enum OfficeTier
    {
        Tier1_Local = 1,      // City Council, School Board
        Tier2_Municipal = 2,  // Mayor, DA, State Rep
        Tier3_State = 3,      // State Senator, AG, Lt Gov
        Tier4_National = 4,   // Governor, US Rep, Senator
        Tier5_Executive = 5   // President, Supreme Leader
    }
    
    /// <summary>
    /// Specific political offices
    /// </summary>
    public enum PoliticalOffice
    {
        // Tier 1
        CityCouncil,
        SchoolBoard,
        CountyCommissioner,
        NeighborhoodAssociation,
        
        // Tier 2
        Mayor,
        CountyExecutive,
        DistrictAttorney,
        StateRepresentative,
        
        // Tier 3
        StateSenator,
        StateAttorneyGeneral,
        LieutenantGovernor,
        RegionalDirector,
        
        // Tier 4
        Governor,
        USRepresentative,
        USSenator,
        CabinetSecretary,
        
        // Tier 5
        President,
        SupremeLeader,
        ShadowGovernment
    }
    
    /// <summary>
    /// Alignment on Law-Chaos axis
    /// </summary>
    public enum LawChaosAlignment
    {
        Lawful = -1,
        Neutral = 0,
        Chaotic = 1
    }
    
    /// <summary>
    /// Alignment on Good-Evil axis
    /// </summary>
    public enum GoodEvilAlignment
    {
        Good = -1,
        Neutral = 0,
        Evil = 1
    }
    
    /// <summary>
    /// Full alignment category
    /// </summary>
    public enum AlignmentCategory
    {
        LawfulGood,
        NeutralGood,
        ChaoticGood,
        LawfulNeutral,
        TrueNeutral,
        ChaoticNeutral,
        LawfulEvil,
        NeutralEvil,
        ChaoticEvil
    }
    
    /// <summary>
    /// Voter demographic blocs
    /// </summary>
    public enum VoterBloc
    {
        WorkingClass,
        Business,
        Youth,
        Seniors,
        Suburban,
        Urban,
        Rural,
        Religious,
        Secular,
        Minority,
        Education,
        Healthcare,
        Security,
        Environmental,
        Progressive,
        Conservative
    }
    
    /// <summary>
    /// Game difficulty settings
    /// </summary>
    public enum GameDifficulty
    {
        Story,      // Very easy, focus on narrative
        Easy,       // Reduced challenge
        Normal,     // Standard experience
        Hard,       // Increased challenge
        Nightmare,  // Maximum difficulty
        Ironman     // Permadeath, no saves
    }
    
    #endregion
    
    #region Resource Enums
    
    /// <summary>
    /// Core resource types
    /// </summary>
    public enum ResourceType
    {
        PublicTrust,
        PoliticalCapital,
        CampaignFunds,
        MediaInfluence,
        PartyLoyalty,
        StaffQuality
    }
    
    /// <summary>
    /// Crisis types
    /// </summary>
    public enum CrisisType
    {
        NaturalDisaster,
        EconomicDownturn,
        PersonalScandal,
        AdministrationScandal,
        SecurityThreat,
        SocialUnrest,
        HealthEmergency,
        InternationalIncident,
        ConstitutionalCrisis,
        MediaFrenzy
    }
    
    /// <summary>
    /// Scandal categories
    /// </summary>
    public enum ScandalCategory
    {
        Financial,
        Personal,
        Policy,
        Administrative,
        Electoral
    }
    
    /// <summary>
    /// Game phases
    /// </summary>
    public enum GamePhase
    {
        CharacterCreation,
        Campaign,
        Governance,
        Consequence,
        Election,
        Legacy,
        GameOver
    }
    
    #endregion
    
    #region Core Data Classes
    
    /// <summary>
    /// Core character attributes
    /// </summary>
    [Serializable]
    public class CharacterAttributes
    {
        [Range(1, 10)] public int Charisma = 5;
        [Range(1, 10)] public int Intelligence = 5;
        [Range(1, 10)] public int Cunning = 5;
        [Range(1, 10)] public int Resilience = 5;
        [Range(1, 10)] public int Networking = 5;
        
        public int TotalPoints => Charisma + Intelligence + Cunning + Resilience + Networking;
        
        public CharacterAttributes Clone()
        {
            return new CharacterAttributes
            {
                Charisma = Charisma,
                Intelligence = Intelligence,
                Cunning = Cunning,
                Resilience = Resilience,
                Networking = Networking
            };
        }
    }
    
    /// <summary>
    /// Hidden alignment tracking
    /// </summary>
    [Serializable]
    public class AlignmentState
    {
        // -100 (Lawful) to +100 (Chaotic)
        [Range(-100, 100)] public int LawChaosScore = 0;
        
        // -100 (Good) to +100 (Evil)
        [Range(-100, 100)] public int GoodEvilScore = 0;
        
        public LawChaosAlignment LawChaos
        {
            get
            {
                if (LawChaosScore <= -34) return LawChaosAlignment.Lawful;
                if (LawChaosScore >= 34) return LawChaosAlignment.Chaotic;
                return LawChaosAlignment.Neutral;
            }
        }
        
        public GoodEvilAlignment GoodEvil
        {
            get
            {
                if (GoodEvilScore <= -34) return GoodEvilAlignment.Good;
                if (GoodEvilScore >= 34) return GoodEvilAlignment.Evil;
                return GoodEvilAlignment.Neutral;
            }
        }
        
        public AlignmentCategory Category
        {
            get
            {
                return (LawChaos, GoodEvil) switch
                {
                    (LawChaosAlignment.Lawful, GoodEvilAlignment.Good) => AlignmentCategory.LawfulGood,
                    (LawChaosAlignment.Neutral, GoodEvilAlignment.Good) => AlignmentCategory.NeutralGood,
                    (LawChaosAlignment.Chaotic, GoodEvilAlignment.Good) => AlignmentCategory.ChaoticGood,
                    (LawChaosAlignment.Lawful, GoodEvilAlignment.Neutral) => AlignmentCategory.LawfulNeutral,
                    (LawChaosAlignment.Neutral, GoodEvilAlignment.Neutral) => AlignmentCategory.TrueNeutral,
                    (LawChaosAlignment.Chaotic, GoodEvilAlignment.Neutral) => AlignmentCategory.ChaoticNeutral,
                    (LawChaosAlignment.Lawful, GoodEvilAlignment.Evil) => AlignmentCategory.LawfulEvil,
                    (LawChaosAlignment.Neutral, GoodEvilAlignment.Evil) => AlignmentCategory.NeutralEvil,
                    (LawChaosAlignment.Chaotic, GoodEvilAlignment.Evil) => AlignmentCategory.ChaoticEvil,
                    _ => AlignmentCategory.TrueNeutral
                };
            }
        }
        
        public void ShiftAlignment(int lawChaosChange, int goodEvilChange)
        {
            LawChaosScore = Mathf.Clamp(LawChaosScore + lawChaosChange, -100, 100);
            GoodEvilScore = Mathf.Clamp(GoodEvilScore + goodEvilChange, -100, 100);
        }
    }
    
    /// <summary>
    /// Player's resource state
    /// </summary>
    [Serializable]
    public class ResourceState
    {
        // Core resources (0-100 scale unless noted)
        [Range(0, 100)] public float PublicTrust = 50f;
        public int PoliticalCapital = 10;
        public long CampaignFunds = 10000;
        [Range(0, 100)] public float MediaInfluence = 25f;
        [Range(0, 100)] public float PartyLoyalty = 50f;
        [Range(1, 10)] public float StaffQuality = 3f;
        
        // Caps
        public int PoliticalCapitalCap = 30; // Increases with tier
        
        // Secondary
        public Dictionary<VoterBloc, float> VoterBlocSupport;
        public List<string> DirtOnOpponents;
        public List<string> SpecialInterestFavors;
        
        public ResourceState()
        {
            VoterBlocSupport = new Dictionary<VoterBloc, float>();
            DirtOnOpponents = new List<string>();
            SpecialInterestFavors = new List<string>();
            
            // Initialize voter bloc support
            foreach (VoterBloc bloc in Enum.GetValues(typeof(VoterBloc)))
            {
                VoterBlocSupport[bloc] = 50f;
            }
        }
        
        public void ApplyChange(ResourceType type, float amount)
        {
            switch (type)
            {
                case ResourceType.PublicTrust:
                    PublicTrust = Mathf.Clamp(PublicTrust + amount, 0, 100);
                    break;
                case ResourceType.PoliticalCapital:
                    PoliticalCapital = Mathf.Clamp(PoliticalCapital + (int)amount, 0, PoliticalCapitalCap);
                    break;
                case ResourceType.CampaignFunds:
                    CampaignFunds = Math.Max(0, CampaignFunds + (long)amount);
                    break;
                case ResourceType.MediaInfluence:
                    MediaInfluence = Mathf.Clamp(MediaInfluence + amount, 0, 100);
                    break;
                case ResourceType.PartyLoyalty:
                    PartyLoyalty = Mathf.Clamp(PartyLoyalty + amount, 0, 100);
                    break;
                case ResourceType.StaffQuality:
                    StaffQuality = Mathf.Clamp(StaffQuality + amount, 1, 10);
                    break;
            }
        }
    }
    
    /// <summary>
    /// Background definition with bonuses
    /// </summary>
    [Serializable]
    public class BackgroundDefinition
    {
        public CharacterBackground Background;
        public string DisplayName;
        public string Description;
        
        // Starting bonuses
        public Dictionary<ResourceType, float> ResourceBonuses;
        public Dictionary<VoterBloc, float> VoterBlocBonuses;
        public CharacterAttributes AttributeModifiers;
        
        // Special ability
        public string SpecialAbilityId;
        public string SpecialAbilityName;
        public string SpecialAbilityDescription;
        
        // Vulnerabilities
        public List<ScandalCategory> ScandalVulnerabilities;
        public float ScandalVulnerabilityMultiplier;
        
        public BackgroundDefinition()
        {
            ResourceBonuses = new Dictionary<ResourceType, float>();
            VoterBlocBonuses = new Dictionary<VoterBloc, float>();
            AttributeModifiers = new CharacterAttributes();
            ScandalVulnerabilities = new List<ScandalCategory>();
        }
    }
    
    /// <summary>
    /// Political office definition
    /// </summary>
    [Serializable]
    public class OfficeDefinition
    {
        public PoliticalOffice Office;
        public OfficeTier Tier;
        public string DisplayName;
        public string Description;
        
        // Requirements to run
        public OfficeTier MinimumPreviousTier;
        public float MinimumApproval;
        public long MinimumCampaignFunds;
        public float MinimumPartyLoyalty;
        public int MinimumAllies;
        
        // Office properties
        public int TermLengthYears;
        public int TermLimit; // 0 = unlimited
        public int PoliticalCapitalCap;
        public List<string> OfficePowers;
        
        // Election properties
        public float BaseWinChance;
        public float MediaScrutinyMultiplier;
        public float ScandalImpactMultiplier;
        
        public OfficeDefinition()
        {
            OfficePowers = new List<string>();
        }
    }
    
    /// <summary>
    /// Staff member
    /// </summary>
    [Serializable]
    public class StaffMember
    {
        public string Id;
        public string Name;
        public string Role;
        public int Quality; // 1-10
        public float Loyalty; // 0-100
        public float Morale; // 0-100
        public List<string> Specializations;
        public bool HasSecrets;
        public string SecretType;
        public DateTime HiredAt;
        
        public StaffMember()
        {
            Id = Guid.NewGuid().ToString();
            Specializations = new List<string>();
        }
    }
    
    /// <summary>
    /// Political ally
    /// </summary>
    [Serializable]
    public class PoliticalAlly
    {
        public string Id;
        public string Name;
        public PoliticalOffice Office;
        public PartyAffiliation Party;
        public float RelationshipStrength; // 0-100
        public float Influence; // Their political power
        public List<string> SharedInterests;
        public List<string> Conflicts;
        public bool IsBlackmailable;
        
        public PoliticalAlly()
        {
            Id = Guid.NewGuid().ToString();
            SharedInterests = new List<string>();
            Conflicts = new List<string>();
        }
    }
    
    /// <summary>
    /// Active scandal
    /// </summary>
    [Serializable]
    public class ActiveScandal
    {
        public string Id;
        public string TemplateId;
        public string Title;
        public string Description;
        public ScandalCategory Category;
        public int Severity; // 1-10
        public float EvidenceLevel; // 0-100
        public float PublicAwareness; // 0-100
        public int TurnsActive;
        public bool IsTerminal;
        public List<string> ResponsesUsed;
        public DateTime DiscoveredAt;
        
        public ActiveScandal()
        {
            Id = Guid.NewGuid().ToString();
            ResponsesUsed = new List<string>();
        }
    }
    
    /// <summary>
    /// Active crisis
    /// </summary>
    [Serializable]
    public class ActiveCrisis
    {
        public string Id;
        public string Title;
        public string Description;
        public CrisisType Type;
        public int Severity; // 1-10
        public int TurnsRemaining;
        public int TurnsToRespond;
        public bool IsEscalating;
        public List<string> AvailableResponses;
        public string SelectedResponse;
        public DateTime StartedAt;
        
        public ActiveCrisis()
        {
            Id = Guid.NewGuid().ToString();
            AvailableResponses = new List<string>();
        }
    }
    
    /// <summary>
    /// Election result
    /// </summary>
    [Serializable]
    public class ElectionResult
    {
        public string ElectionId;
        public PoliticalOffice Office;
        public bool Won;
        public float VotePercentage;
        public float OpponentVotePercentage;
        public int TotalVotes;
        public Dictionary<VoterBloc, float> BlocBreakdown;
        public List<string> KeyFactors;
        public DateTime ElectionDate;
        
        public ElectionResult()
        {
            BlocBreakdown = new Dictionary<VoterBloc, float>();
            KeyFactors = new List<string>();
        }
    }
    
    /// <summary>
    /// Legacy achievement
    /// </summary>
    [Serializable]
    public class LegacyAchievement
    {
        public string Id;
        public string Name;
        public string Description;
        public string UnlockCondition;
        public string PerkId;
        public string PerkName;
        public string PerkDescription;
        public bool IsUnlocked;
        public DateTime? UnlockedAt;
        public int LegacyPointsAwarded;
    }
    
    /// <summary>
    /// Game settings
    /// </summary>
    [Serializable]
    public class GameSettings
    {
        public GameDifficulty Difficulty = GameDifficulty.Normal;
        public bool TutorialEnabled = true;
        public bool AutoSaveEnabled = true;
        public int AutoSaveIntervalTurns = 3;
        public float GameSpeed = 1.0f;
        public bool RealTimeMode = false;
        public float RealTimeMultiplier = 1.0f;
        
        // Difficulty modifiers
        public float ScandalFrequencyMultiplier = 1.0f;
        public float CrisisFrequencyMultiplier = 1.0f;
        public float ResourceGenerationMultiplier = 1.0f;
        public float OpponentStrengthMultiplier = 1.0f;
        
        public void ApplyDifficultyPreset()
        {
            switch (Difficulty)
            {
                case GameDifficulty.Story:
                    ScandalFrequencyMultiplier = 0.5f;
                    CrisisFrequencyMultiplier = 0.5f;
                    ResourceGenerationMultiplier = 1.5f;
                    OpponentStrengthMultiplier = 0.5f;
                    break;
                case GameDifficulty.Easy:
                    ScandalFrequencyMultiplier = 0.75f;
                    CrisisFrequencyMultiplier = 0.75f;
                    ResourceGenerationMultiplier = 1.25f;
                    OpponentStrengthMultiplier = 0.75f;
                    break;
                case GameDifficulty.Normal:
                    ScandalFrequencyMultiplier = 1.0f;
                    CrisisFrequencyMultiplier = 1.0f;
                    ResourceGenerationMultiplier = 1.0f;
                    OpponentStrengthMultiplier = 1.0f;
                    break;
                case GameDifficulty.Hard:
                    ScandalFrequencyMultiplier = 1.25f;
                    CrisisFrequencyMultiplier = 1.25f;
                    ResourceGenerationMultiplier = 0.85f;
                    OpponentStrengthMultiplier = 1.25f;
                    break;
                case GameDifficulty.Nightmare:
                    ScandalFrequencyMultiplier = 1.5f;
                    CrisisFrequencyMultiplier = 1.5f;
                    ResourceGenerationMultiplier = 0.7f;
                    OpponentStrengthMultiplier = 1.5f;
                    break;
                case GameDifficulty.Ironman:
                    ScandalFrequencyMultiplier = 1.5f;
                    CrisisFrequencyMultiplier = 1.5f;
                    ResourceGenerationMultiplier = 0.7f;
                    OpponentStrengthMultiplier = 1.5f;
                    AutoSaveEnabled = false;
                    break;
            }
        }
    }
    
    #endregion
    
    #region Event Classes
    
    /// <summary>
    /// Base class for game events
    /// </summary>
    [Serializable]
    public abstract class GameEvent
    {
        public string Id;
        public DateTime Timestamp;
        
        protected GameEvent()
        {
            Id = Guid.NewGuid().ToString();
            Timestamp = DateTime.UtcNow;
        }
    }
    
    /// <summary>
    /// Resource changed event
    /// </summary>
    [Serializable]
    public class ResourceChangedEvent : GameEvent
    {
        public ResourceType Resource;
        public float OldValue;
        public float NewValue;
        public string Reason;
    }
    
    /// <summary>
    /// Phase changed event
    /// </summary>
    [Serializable]
    public class PhaseChangedEvent : GameEvent
    {
        public GamePhase OldPhase;
        public GamePhase NewPhase;
    }
    
    /// <summary>
    /// Scandal triggered event
    /// </summary>
    [Serializable]
    public class ScandalTriggeredEvent : GameEvent
    {
        public ActiveScandal Scandal;
        public string TriggerReason;
    }
    
    /// <summary>
    /// Crisis started event
    /// </summary>
    [Serializable]
    public class CrisisStartedEvent : GameEvent
    {
        public ActiveCrisis Crisis;
    }
    
    /// <summary>
    /// Election completed event
    /// </summary>
    [Serializable]
    public class ElectionCompletedEvent : GameEvent
    {
        public ElectionResult Result;
    }
    
    /// <summary>
    /// Alignment shifted event
    /// </summary>
    [Serializable]
    public class AlignmentShiftedEvent : GameEvent
    {
        public AlignmentCategory OldCategory;
        public AlignmentCategory NewCategory;
        public int LawChaosChange;
        public int GoodEvilChange;
        public string Reason;
    }
    
    #endregion
}
