using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ============================================================================
// ELECTION EMPIRE - STAFF PERSONALITY & CHARACTER ARC SYSTEM
// Dynamic staff personalities that clash, synergize, and evolve mid-campaign
// Creates emergent storytelling moments and hilarious/tragic disasters
// ============================================================================

namespace ElectionEmpire.Staff
{
    #region Staff Values & Personality
    
    /// <summary>
    /// Core values that staff members hold - can conflict with player actions
    /// </summary>
    [Flags]
    public enum StaffValues
    {
        None = 0,
        
        // Ethical Stance
        Idealist = 1 << 0,          // Believes in doing things the right way
        Pragmatist = 1 << 1,        // Ends justify means
        Ruthless = 1 << 2,          // Will do anything to win
        
        // Political Leaning
        Progressive = 1 << 3,       // Left-leaning values
        Conservative = 1 << 4,      // Right-leaning values
        Libertarian = 1 << 5,       // Anti-government
        Populist = 1 << 6,          // Anti-establishment
        
        // Personal Beliefs
        Religious = 1 << 7,         // Strong faith, moral concerns
        Secular = 1 << 8,           // Skeptical of religion in politics
        FamilyFirst = 1 << 9,       // Prioritizes family over career
        Workaholic = 1 << 10,       // Lives for the job
        
        // Motivations
        TrueBeliever = 1 << 11,     // Genuinely believes in the cause
        Mercenary = 1 << 12,        // In it for the money
        PowerHungry = 1 << 13,      // Wants influence/position
        FameSeeker = 1 << 14,       // Wants recognition/celebrity
        
        // Loyalty Style
        Loyal = 1 << 15,            // Sticks with you through thick and thin
        Opportunist = 1 << 16,      // Will jump ship if better offer comes
        Principled = 1 << 17,       // Loyalty conditional on ethics
        
        // Work Style
        ByTheBook = 1 << 18,        // Follows rules and procedures
        Creative = 1 << 19,         // Bends rules, finds loopholes
        Reckless = 1 << 20,         // Takes big risks
        Cautious = 1 << 21,         // Risk-averse
        
        // Social
        Introvert = 1 << 22,        // Prefers behind-scenes work
        Extrovert = 1 << 23,        // Loves spotlight, networking
        TeamPlayer = 1 << 24,       // Works well with others
        LoneWolf = 1 << 25          // Works best alone
    }
    
    /// <summary>
    /// Personality traits that affect behavior and interactions
    /// </summary>
    public enum PersonalityTrait
    {
        // Positive
        Charismatic,
        Brilliant,
        Tireless,
        Connected,
        Diplomatic,
        Innovative,
        Resilient,
        Discreet,
        Inspiring,
        Analytical,
        
        // Neutral (context-dependent)
        Ambitious,
        Competitive,
        Perfectionist,
        Stubborn,
        Intense,
        Unconventional,
        Blunt,
        
        // Negative
        Arrogant,
        Paranoid,
        Volatile,
        Greedy,
        Lazy,
        Gossipy,
        Resentful,
        Insecure,
        Alcoholic,
        Philanderer,
        
        // Wild Cards
        SecretlyRecording,      // Documenting everything for tell-all
        DoubleLife,             // Has secret other existence
        DeepCover,              // Undercover journalist/investigator
        TimeБомб                // Has ticking scandal waiting to explode
    }
    
    /// <summary>
    /// Potential character arcs staff can undergo
    /// </summary>
    public enum CharacterArc
    {
        None,
        
        // Positive Arcs
        FindingPurpose,         // Mercenary becomes true believer
        Redemption,             // Fixer wants to go clean
        GrowingUp,              // Young idealist becomes effective pragmatist
        FindingLove,            // Romance changes priorities (good version)
        Mentorship,             // Becomes invested in developing others
        
        // Negative Arcs
        Disillusionment,        // True believer loses faith
        Corruption,             // Idealist becomes what they hated
        Burnout,                // Workaholic crashes
        Radicalization,         // Becomes extremist
        Addiction,              // Develops substance problem
        
        // Dramatic Arcs
        ReligiousAwakening,     // Finds faith mid-campaign (your example!)
        MoralCrisis,            // Can't reconcile actions with conscience
        Revenge,                // Develops grudge, plans payback
        Whistleblower,          // Decides to expose everything
        TellAll,                // Writing book about the campaign
        
        // Chaotic Arcs
        MidlifeCrisis,          // Dramatic personality shift
        RomanticEntanglement,   // Falls for wrong person
        FamilyDrama,            // Personal life explodes
        IdentityCrisis,         // Questions everything
        SecretRevealed          // Hidden past comes to light
    }
    
    /// <summary>
    /// Events that can trigger from staff personality dynamics
    /// </summary>
    public enum StaffEventType
    {
        // Value Conflicts
        RefusesOrder,           // Won't do something against their values
        PublicObjection,        // Voices disagreement publicly
        QuietSabotage,          // Undermines action they disagree with
        MoralUltimatum,         // "Do this or I quit"
        
        // Character Arc Events
        ReligiousConversion,    // Staff finds faith
        CrisisOfConscience,     // Staff struggles with campaign actions
        PublicConfession,       // Staff confesses sins on live TV
        TellAllAnnouncement,    // Staff announces they're writing book
        
        // Relationship Events
        OfficeRomance,          // Two staff members hook up
        RivalryEscalates,       // Staff conflict goes public
        MentorBond,             // Senior staff takes junior under wing
        FriendshipBetrayal,     // Close staff friends have falling out
        
        // External Complications
        FamilyEmergency,        // Staff needs time off
        PersonalScandal,        // Staff's own scandal breaks
        JobOffer,               // Staff gets poached
        PoliticalAwakening,     // Staff joins cause against yours
        
        // Dramatic Moments
        HeroicSacrifice,        // Staff takes bullet for you (metaphorically)
        SpectacularMeltdown,    // Staff has public breakdown
        BridgeBurning,          // Staff leaves in dramatic fashion
        UnexpectedLoyalty,      // Staff defends you against odds
        
        // Chaos Events
        DrunkInterview,         // Staff gives interview while intoxicated
        SocialMediaDisaster,    // Staff's personal posts cause scandal
        SecretLifeExposed,      // Staff's double life revealed
        WhistleblowerPress,     // Staff goes to media with dirt
        DocumentaryReveal       // Staff was secretly filming documentary
    }
    
    #endregion
    
    #region Staff Personality Profile
    
    /// <summary>
    /// Complete personality profile for a staff member
    /// </summary>
    [Serializable]
    public class StaffPersonality
    {
        // Core Values (flags, can have multiple)
        public StaffValues Values;
        
        // Personality Traits (2-4 traits)
        public List<PersonalityTrait> Traits;
        
        // Current Character Arc (if any)
        public CharacterArc CurrentArc;
        public float ArcProgress;           // 0-100, triggers at 100
        public int TurnsSinceArcStart;
        
        // Hidden traits (revealed through gameplay)
        public List<PersonalityTrait> HiddenTraits;
        public bool HiddenTraitsRevealed;
        
        // Value strength (how strongly they hold each value)
        public Dictionary<StaffValues, float> ValueStrength; // 0-100
        
        // Tolerance (how much they'll put up with before acting)
        public float ValueViolationTolerance;   // 0-100
        public float CurrentViolationStress;     // Accumulates when values violated
        
        // Relationships with other staff
        public Dictionary<string, StaffRelationship> Relationships;
        
        // Personal history that affects reactions
        public List<string> PersonalHistory;
        public string DarkSecret;
        public bool DarkSecretKnown;
        
        public StaffPersonality()
        {
            Traits = new List<PersonalityTrait>();
            HiddenTraits = new List<PersonalityTrait>();
            ValueStrength = new Dictionary<StaffValues, float>();
            Relationships = new Dictionary<string, StaffRelationship>();
            PersonalHistory = new List<string>();
            ValueViolationTolerance = 50f;
        }
        
        /// <summary>
        /// Check if staff has a specific value
        /// </summary>
        public bool HasValue(StaffValues value)
        {
            return (Values & value) == value;
        }
        
        /// <summary>
        /// Get how strongly staff holds a value (0-100)
        /// </summary>
        public float GetValueStrength(StaffValues value)
        {
            if (!HasValue(value)) return 0f;
            return ValueStrength.TryGetValue(value, out float strength) ? strength : 50f;
        }
        
        /// <summary>
        /// Check compatibility with a player action
        /// Returns negative value if incompatible, positive if aligned
        /// </summary>
        public float CheckActionCompatibility(string actionType, Dictionary<string, object> actionContext)
        {
            float compatibility = 0f;
            
            // Check for value conflicts
            if (actionType.Contains("dirty") || actionType.Contains("trick") || 
                actionType.Contains("blackmail") || actionType.Contains("bribe"))
            {
                if (HasValue(StaffValues.Idealist))
                    compatibility -= GetValueStrength(StaffValues.Idealist);
                if (HasValue(StaffValues.Religious))
                    compatibility -= GetValueStrength(StaffValues.Religious) * 0.8f;
                if (HasValue(StaffValues.Principled))
                    compatibility -= GetValueStrength(StaffValues.Principled) * 0.7f;
                if (HasValue(StaffValues.ByTheBook))
                    compatibility -= GetValueStrength(StaffValues.ByTheBook) * 0.5f;
                    
                // But some values support it
                if (HasValue(StaffValues.Ruthless))
                    compatibility += GetValueStrength(StaffValues.Ruthless);
                if (HasValue(StaffValues.Pragmatist))
                    compatibility += GetValueStrength(StaffValues.Pragmatist) * 0.5f;
            }
            
            // Policy-based compatibility
            if (actionContext != null && actionContext.TryGetValue("policy_type", out object policyType))
            {
                string policy = policyType.ToString().ToLower();
                
                if (policy.Contains("progressive") || policy.Contains("liberal"))
                {
                    if (HasValue(StaffValues.Progressive))
                        compatibility += GetValueStrength(StaffValues.Progressive) * 0.5f;
                    if (HasValue(StaffValues.Conservative))
                        compatibility -= GetValueStrength(StaffValues.Conservative) * 0.5f;
                }
                
                if (policy.Contains("conservative") || policy.Contains("traditional"))
                {
                    if (HasValue(StaffValues.Conservative))
                        compatibility += GetValueStrength(StaffValues.Conservative) * 0.5f;
                    if (HasValue(StaffValues.Progressive))
                        compatibility -= GetValueStrength(StaffValues.Progressive) * 0.5f;
                }
                
                if (policy.Contains("religious") || policy.Contains("faith"))
                {
                    if (HasValue(StaffValues.Religious))
                        compatibility += GetValueStrength(StaffValues.Religious) * 0.5f;
                    if (HasValue(StaffValues.Secular))
                        compatibility -= GetValueStrength(StaffValues.Secular) * 0.3f;
                }
            }
            
            return compatibility;
        }
        
        /// <summary>
        /// Process a value violation (player did something staff disagrees with)
        /// </summary>
        public void ProcessValueViolation(float severity)
        {
            CurrentViolationStress += severity;
            
            // Check if they've had enough
            if (CurrentViolationStress >= ValueViolationTolerance)
            {
                // They're going to do something about it
                // This triggers staff events
            }
        }
    }
    
    /// <summary>
    /// Relationship between two staff members
    /// </summary>
    [Serializable]
    public class StaffRelationship
    {
        public string OtherStaffId;
        public string OtherStaffName;
        public int Opinion;                 // -100 to 100
        public RelationshipType Type;
        public List<string> SharedHistory;
        public bool HasRomantic;
        public bool HasRivalry;
        public bool IsMentor;
        public bool IsProtege;
        
        public StaffRelationship()
        {
            SharedHistory = new List<string>();
        }
    }
    
    public enum RelationshipType
    {
        Strangers,
        Acquaintances,
        Colleagues,
        Friends,
        CloseFriends,
        BestFriends,
        Rivals,
        Enemies,
        RomanticInterest,
        Dating,
        Married,
        ExPartners,
        MentorProtege,
        FamilyMembers
    }
    
    #endregion
    
    #region Character Arc System
    
    /// <summary>
    /// Manages character arc progression and triggers
    /// </summary>
    public class CharacterArcManager
    {
        private static readonly Dictionary<CharacterArc, CharacterArcDefinition> ArcDefinitions = new()
        {
            {
                CharacterArc.ReligiousAwakening, new CharacterArcDefinition
                {
                    Name = "Religious Awakening",
                    Description = "Staff member finds faith mid-campaign",
                    TriggerConditions = new[] { "assigned_dirty_tricks", "witnessed_corruption", "personal_crisis" },
                    ProgressRate = 5f,
                    RequiredProgress = 100f,
                    ResultingValueChanges = new Dictionary<StaffValues, float>
                    {
                        { StaffValues.Religious, 80f },
                        { StaffValues.Idealist, 60f },
                        { StaffValues.Ruthless, -100f }
                    },
                    ClimaxEvents = new[] 
                    {
                        "public_confession_live_tv",
                        "refuses_all_dirty_work",
                        "quits_to_become_minister",
                        "exposes_campaign_sins"
                    },
                    ForeshadowingDialogue = new[]
                    {
                        "I've been doing a lot of thinking lately...",
                        "My grandmother was very religious. I've been reading her Bible.",
                        "Sometimes I wonder if we're the good guys.",
                        "I visited a church last Sunday. First time in years."
                    }
                }
            },
            {
                CharacterArc.Disillusionment, new CharacterArcDefinition
                {
                    Name = "Disillusionment",
                    Description = "True believer loses faith in the cause",
                    TriggerConditions = new[] { "broken_promises", "policy_reversal", "values_violated" },
                    ProgressRate = 3f,
                    RequiredProgress = 100f,
                    ResultingValueChanges = new Dictionary<StaffValues, float>
                    {
                        { StaffValues.TrueBeliever, -100f },
                        { StaffValues.Mercenary, 50f },
                        { StaffValues.Opportunist, 40f }
                    },
                    ClimaxEvents = new[]
                    {
                        "quits_in_disgust",
                        "joins_opponent",
                        "writes_scathing_op_ed",
                        "becomes_cynical_operator"
                    },
                    ForeshadowingDialogue = new[]
                    {
                        "Remember when we actually stood for something?",
                        "I didn't sign up for this.",
                        "What happened to the candidate I believed in?",
                        "Maybe the other side has a point."
                    }
                }
            },
            {
                CharacterArc.Whistleblower, new CharacterArcDefinition
                {
                    Name = "Whistleblower",
                    Description = "Staff decides to expose campaign wrongdoing",
                    TriggerConditions = new[] { "witnessed_crime", "massive_corruption", "values_shattered" },
                    ProgressRate = 8f,
                    RequiredProgress = 100f,
                    ResultingValueChanges = new Dictionary<StaffValues, float>
                    {
                        { StaffValues.Principled, 100f },
                        { StaffValues.Loyal, -100f }
                    },
                    ClimaxEvents = new[]
                    {
                        "goes_to_fbi",
                        "media_expose",
                        "congressional_testimony",
                        "documentary_release"
                    },
                    ForeshadowingDialogue = new[]
                    {
                        "Someone should really document all this...",
                        "I've been keeping notes. Just in case.",
                        "My lawyer says I should keep records.",
                        "Have you ever testified before Congress?"
                    }
                }
            },
            {
                CharacterArc.TellAll, new CharacterArcDefinition
                {
                    Name = "Tell-All Book",
                    Description = "Staff is secretly writing an exposé",
                    TriggerConditions = new[] { "fame_seeker", "writer_background", "juicy_stories" },
                    ProgressRate = 2f,
                    RequiredProgress = 100f,
                    ResultingValueChanges = new Dictionary<StaffValues, float>
                    {
                        { StaffValues.FameSeeker, 80f }
                    },
                    ClimaxEvents = new[]
                    {
                        "book_announcement",
                        "publisher_bidding_war",
                        "excerpt_leaked",
                        "morning_show_interview"
                    },
                    ForeshadowingDialogue = new[]
                    {
                        "You know, this would make a great book someday.",
                        "I've been journaling a lot lately.",
                        "My friend works at a publishing house...",
                        "Do you think people would want to read about all this?"
                    }
                }
            },
            {
                CharacterArc.Burnout, new CharacterArcDefinition
                {
                    Name = "Burnout",
                    Description = "Workaholic staff member crashes hard",
                    TriggerConditions = new[] { "high_stress", "no_time_off", "crisis_after_crisis" },
                    ProgressRate = 4f,
                    RequiredProgress = 100f,
                    ResultingValueChanges = new Dictionary<StaffValues, float>
                    {
                        { StaffValues.Workaholic, -80f },
                        { StaffValues.FamilyFirst, 60f }
                    },
                    ClimaxEvents = new[]
                    {
                        "hospitalized_exhaustion",
                        "public_meltdown",
                        "quits_abruptly",
                        "takes_extended_leave"
                    },
                    ForeshadowingDialogue = new[]
                    {
                        "I can't remember the last time I saw my kids.",
                        "Sleep is overrated, right? *nervous laugh*",
                        "I've been having these headaches...",
                        "What day is it again?"
                    }
                }
            },
            {
                CharacterArc.Corruption, new CharacterArcDefinition
                {
                    Name = "Corruption",
                    Description = "Idealist becomes the monster they fought",
                    TriggerConditions = new[] { "power_exposure", "dirty_tricks_normalized", "greed_opportunity" },
                    ProgressRate = 2f,
                    RequiredProgress = 100f,
                    ResultingValueChanges = new Dictionary<StaffValues, float>
                    {
                        { StaffValues.Idealist, -100f },
                        { StaffValues.Ruthless, 70f },
                        { StaffValues.Mercenary, 60f } // Greedy is a trait, using Mercenary as equivalent
                    },
                    ClimaxEvents = new[]
                    {
                        "embezzlement_discovered",
                        "takes_bribes",
                        "blackmails_for_personal_gain",
                        "becomes_fixer"
                    },
                    ForeshadowingDialogue = new[]
                    {
                        "You know, the other side does this all the time...",
                        "Rules are for people who can't win otherwise.",
                        "I've learned so much about how power really works.",
                        "Everyone has a price. I'm just being realistic."
                    }
                }
            },
            {
                CharacterArc.RomanticEntanglement, new CharacterArcDefinition
                {
                    Name = "Romantic Entanglement",
                    Description = "Staff falls for someone problematic",
                    TriggerConditions = new[] { "attractive_opponent_staffer", "lonely", "high_stress_bonding" },
                    ProgressRate = 6f,
                    RequiredProgress = 100f,
                    ResultingValueChanges = new Dictionary<StaffValues, float>
                    {
                        { StaffValues.FamilyFirst, 40f },
                        { StaffValues.Loyal, -30f }
                    },
                    ClimaxEvents = new[]
                    {
                        "caught_with_opponent_staffer",
                        "leaks_secrets_to_lover",
                        "dramatic_love_triangle",
                        "quits_to_be_with_lover"
                    },
                    ForeshadowingDialogue = new[]
                    {
                        "The opposition's new press secretary seems... professional.",
                        "We keep running into each other at events.",
                        "They're not as bad as we make them out to be.",
                        "I need to take this call privately."
                    }
                }
            },
            {
                CharacterArc.Revenge, new CharacterArcDefinition
                {
                    Name = "Revenge",
                    Description = "Staff develops grudge and plots payback",
                    TriggerConditions = new[] { "publicly_humiliated", "passed_over_promotion", "blamed_unfairly" },
                    ProgressRate = 4f,
                    RequiredProgress = 100f,
                    ResultingValueChanges = new Dictionary<StaffValues, float>
                    {
                        { StaffValues.Loyal, -100f },
                        { StaffValues.Ruthless, 80f }
                    },
                    ClimaxEvents = new[]
                    {
                        "sabotages_campaign",
                        "leaks_to_press",
                        "joins_opponent_dramatically",
                        "releases_damaging_evidence"
                    },
                    ForeshadowingDialogue = new[]
                    {
                        "I won't forget this.",
                        "Funny how things have a way of coming back around.",
                        "I've been patient. Very patient.",
                        "You'll see. Everyone will see."
                    }
                }
            },
            {
                CharacterArc.MoralCrisis, new CharacterArcDefinition
                {
                    Name = "Moral Crisis",
                    Description = "Staff struggles to reconcile actions with conscience",
                    TriggerConditions = new[] { "asked_to_lie", "dirty_work_accumulating", "personal_reflection" },
                    ProgressRate = 3f,
                    RequiredProgress = 100f,
                    ResultingValueChanges = new Dictionary<StaffValues, float>
                    {
                        { StaffValues.Principled, 60f }
                    },
                    ClimaxEvents = new[]
                    {
                        "refuses_key_task_at_crucial_moment",
                        "public_moral_stand",
                        "seeks_redemption",
                        "quiet_resignation"
                    },
                    ForeshadowingDialogue = new[]
                    {
                        "Are we the baddies?",
                        "I've been having trouble sleeping lately.",
                        "My therapist says I need to 'align my values with my actions.'",
                        "I used to be so sure about all this."
                    }
                }
            },
            {
                CharacterArc.Redemption, new CharacterArcDefinition
                {
                    Name = "Redemption",
                    Description = "Former dirty operator wants to go clean",
                    TriggerConditions = new[] { "past_catches_up", "moment_of_clarity", "mentee_asks_hard_questions" },
                    ProgressRate = 2f,
                    RequiredProgress = 100f,
                    ResultingValueChanges = new Dictionary<StaffValues, float>
                    {
                        { StaffValues.Ruthless, -60f },
                        { StaffValues.Idealist, 40f },
                        { StaffValues.Principled, 50f }
                    },
                    ClimaxEvents = new[]
                    {
                        "refuses_signature_dirty_trick",
                        "confesses_past_to_boss",
                        "becomes_ethics_champion",
                        "protects_innocent_target"
                    },
                    ForeshadowingDialogue = new[]
                    {
                        "I've done things I'm not proud of.",
                        "Maybe it's time for a different approach.",
                        "The new kids... they still believe. I envy that.",
                        "I'd like to win one the right way. Just once."
                    }
                }
            }
        };
        
        /// <summary>
        /// Check if any arcs should begin for a staff member
        /// </summary>
        public static CharacterArc CheckForArcTrigger(StaffMember staff, string eventType)
        {
            if (staff.Personality.CurrentArc != CharacterArc.None)
                return CharacterArc.None; // Already in an arc
            
            foreach (var arcDef in ArcDefinitions)
            {
                if (arcDef.Value.TriggerConditions.Contains(eventType))
                {
                    // Check if personality is compatible with this arc
                    if (IsArcCompatible(staff, arcDef.Key))
                    {
                        // Random chance to start
                        if (UnityEngine.Random.value < 0.3f)
                        {
                            return arcDef.Key;
                        }
                    }
                }
            }
            
            return CharacterArc.None;
        }
        
        /// <summary>
        /// Check if a staff member's personality is compatible with an arc
        /// </summary>
        private static bool IsArcCompatible(StaffMember staff, CharacterArc arc)
        {
            var personality = staff.Personality;
            
            return arc switch
            {
                CharacterArc.ReligiousAwakening => 
                    !personality.HasValue(StaffValues.Secular) &&
                    (personality.HasValue(StaffValues.Ruthless) || 
                     personality.Traits.Contains(PersonalityTrait.Volatile)),
                     
                CharacterArc.Disillusionment =>
                    personality.HasValue(StaffValues.TrueBeliever) ||
                    personality.HasValue(StaffValues.Idealist),
                    
                CharacterArc.Whistleblower =>
                    personality.HasValue(StaffValues.Principled) ||
                    personality.HasValue(StaffValues.Idealist),
                    
                CharacterArc.TellAll =>
                    personality.HasValue(StaffValues.FameSeeker) ||
                    personality.Traits.Contains(PersonalityTrait.Ambitious),
                    
                CharacterArc.Burnout =>
                    personality.HasValue(StaffValues.Workaholic) ||
                    staff.Stress > 70,
                    
                CharacterArc.Corruption =>
                    personality.HasValue(StaffValues.Idealist) &&
                    personality.Traits.Contains(PersonalityTrait.Ambitious), // Ambitious is a trait, not a value
                    
                CharacterArc.RomanticEntanglement =>
                    !personality.HasValue(StaffValues.FamilyFirst) &&
                    personality.Traits.Contains(PersonalityTrait.Philanderer) == false,
                    
                CharacterArc.Revenge =>
                    personality.Traits.Contains(PersonalityTrait.Resentful) ||
                    staff.OpinionOfPlayer < -30,
                    
                CharacterArc.MoralCrisis =>
                    personality.HasValue(StaffValues.Principled) ||
                    personality.HasValue(StaffValues.Religious),
                    
                CharacterArc.Redemption =>
                    personality.HasValue(StaffValues.Ruthless) &&
                    staff.TurnsEmployed > 20,
                    
                _ => true
            };
        }
        
        /// <summary>
        /// Get foreshadowing dialogue for staff's current arc
        /// </summary>
        public static string GetForeshadowingDialogue(CharacterArc arc, float progress)
        {
            if (!ArcDefinitions.TryGetValue(arc, out var def))
                return null;
            
            // Select dialogue based on progress
            int index = (int)(progress / 100f * def.ForeshadowingDialogue.Length);
            index = Mathf.Clamp(index, 0, def.ForeshadowingDialogue.Length - 1);
            
            return def.ForeshadowingDialogue[index];
        }
        
        /// <summary>
        /// Get the climax event when arc completes
        /// </summary>
        public static string GetClimaxEvent(CharacterArc arc)
        {
            if (!ArcDefinitions.TryGetValue(arc, out var def))
                return null;
            
            return def.ClimaxEvents[UnityEngine.Random.Range(0, def.ClimaxEvents.Length)];
        }
        
        /// <summary>
        /// Apply value changes when arc completes
        /// </summary>
        public static void ApplyArcValueChanges(StaffMember staff, CharacterArc arc)
        {
            if (!ArcDefinitions.TryGetValue(arc, out var def))
                return;
            
            foreach (var change in def.ResultingValueChanges)
            {
                if (change.Value > 0)
                {
                    // Add value
                    staff.Personality.Values |= change.Key;
                    staff.Personality.ValueStrength[change.Key] = change.Value;
                }
                else
                {
                    // Remove value
                    staff.Personality.Values &= ~change.Key;
                    staff.Personality.ValueStrength.Remove(change.Key);
                }
            }
        }
    }
    
    /// <summary>
    /// Definition for a character arc
    /// </summary>
    public class CharacterArcDefinition
    {
        public string Name;
        public string Description;
        public string[] TriggerConditions;
        public float ProgressRate;
        public float RequiredProgress;
        public Dictionary<StaffValues, float> ResultingValueChanges;
        public string[] ClimaxEvents;
        public string[] ForeshadowingDialogue;
    }
    
    #endregion
    
    #region Staff Event Generator
    
    /// <summary>
    /// Generates events from staff personality dynamics
    /// </summary>
    public class StaffEventGenerator
    {
        /// <summary>
        /// Check for potential events based on staff personality and recent actions
        /// </summary>
        public static List<StaffEventData> CheckForEvents(
            StaffRoster roster, 
            string recentAction, 
            Dictionary<string, object> actionContext)
        {
            var events = new List<StaffEventData>();
            
            foreach (var staff in roster.ActiveStaff)
            {
                // Check value conflicts
                float compatibility = staff.Personality.CheckActionCompatibility(recentAction, actionContext);
                
                if (compatibility < -50)
                {
                    // Strong value conflict
                    var conflictEvent = GenerateValueConflictEvent(staff, recentAction, compatibility);
                    if (conflictEvent != null)
                        events.Add(conflictEvent);
                }
                
                // Check character arc progression
                if (staff.Personality.CurrentArc != CharacterArc.None)
                {
                    var arcEvent = CheckArcProgression(staff, recentAction);
                    if (arcEvent != null)
                        events.Add(arcEvent);
                }
                else
                {
                    // Check for new arc triggers
                    var newArc = CharacterArcManager.CheckForArcTrigger(staff, recentAction);
                    if (newArc != CharacterArc.None)
                    {
                        staff.Personality.CurrentArc = newArc;
                        staff.Personality.ArcProgress = 0;
                        staff.Personality.TurnsSinceArcStart = 0;
                        
                        events.Add(new StaffEventData
                        {
                            Type = StaffEventType.CrisisOfConscience,
                            StaffInvolved = new[] { staff },
                            Title = "Something's Changed",
                            Description = $"{staff.FullName} seems different lately...",
                            Severity = 1,
                            IsForeshadowing = true
                        });
                    }
                }
                
                // Check staff relationships
                var relationshipEvents = CheckRelationshipEvents(staff, roster);
                events.AddRange(relationshipEvents);
            }
            
            return events;
        }
        
        /// <summary>
        /// Generate event when staff's values are violated
        /// </summary>
        private static StaffEventData GenerateValueConflictEvent(
            StaffMember staff, 
            string action, 
            float incompatibility)
        {
            // Accumulate violation stress
            staff.Personality.ProcessValueViolation(Mathf.Abs(incompatibility) * 0.1f);
            
            // Check if they're going to do something about it
            if (staff.Personality.CurrentViolationStress < staff.Personality.ValueViolationTolerance)
            {
                // Not yet... but maybe foreshadow
                if (UnityEngine.Random.value < 0.3f)
                {
                    return new StaffEventData
                    {
                        Type = StaffEventType.CrisisOfConscience,
                        StaffInvolved = new[] { staff },
                        Title = "Concerned Staff",
                        Description = $"{staff.FullName} seems uncomfortable with recent decisions.",
                        Severity = 1,
                        IsForeshadowing = true,
                        ForeshadowingDialogue = GetValueConflictDialogue(staff, action)
                    };
                }
                return null;
            }
            
            // They've had enough!
            staff.Personality.CurrentViolationStress = 0; // Reset for next time
            
            // Determine what they do based on personality
            var eventType = DetermineConflictResponse(staff);
            
            return new StaffEventData
            {
                Type = eventType,
                StaffInvolved = new[] { staff },
                Title = GetConflictEventTitle(eventType, staff),
                Description = GetConflictEventDescription(eventType, staff, action),
                Severity = GetConflictSeverity(eventType),
                Choices = GetConflictChoices(eventType, staff),
                IsForeshadowing = false
            };
        }
        
        private static StaffEventType DetermineConflictResponse(StaffMember staff)
        {
            var personality = staff.Personality;
            
            // Loud types go public
            if (personality.HasValue(StaffValues.Principled) && 
                personality.Traits.Contains(PersonalityTrait.Blunt))
            {
                return StaffEventType.PublicObjection;
            }
            
            // Sneaky types sabotage
            if (personality.HasValue(StaffValues.Creative) || 
                personality.Traits.Contains(PersonalityTrait.Resentful))
            {
                return StaffEventType.QuietSabotage;
            }
            
            // Principled types draw lines
            if (personality.HasValue(StaffValues.Principled) ||
                personality.HasValue(StaffValues.Idealist))
            {
                return StaffEventType.MoralUltimatum;
            }
            
            // Default: just refuse
            return StaffEventType.RefusesOrder;
        }
        
        private static string GetValueConflictDialogue(StaffMember staff, string action)
        {
            if (staff.Personality.HasValue(StaffValues.Religious))
            {
                return new[]
                {
                    "I'm not sure I can be part of this.",
                    "This isn't what I signed up for.",
                    "There has to be another way.",
                    "What would my mother think if she knew?"
                }[UnityEngine.Random.Range(0, 4)];
            }
            
            if (staff.Personality.HasValue(StaffValues.Idealist))
            {
                return new[]
                {
                    "Remember when we talked about doing things differently?",
                    "I thought we were better than this.",
                    "This is exactly what we criticized them for.",
                    "Is winning really worth it?"
                }[UnityEngine.Random.Range(0, 4)];
            }
            
            return "I have some concerns about our approach.";
        }
        
        private static string GetConflictEventTitle(StaffEventType type, StaffMember staff)
        {
            return type switch
            {
                StaffEventType.RefusesOrder => $"{staff.FullName} Draws a Line",
                StaffEventType.PublicObjection => $"{staff.FullName} Goes Public",
                StaffEventType.QuietSabotage => "Suspicious Setback",
                StaffEventType.MoralUltimatum => $"Ultimatum from {staff.FullName}",
                _ => "Staff Conflict"
            };
        }
        
        private static string GetConflictEventDescription(StaffEventType type, StaffMember staff, string action)
        {
            return type switch
            {
                StaffEventType.RefusesOrder => 
                    $"{staff.FullName} has refused to participate in {action}. \"I won't be part of this,\" they said firmly.",
                    
                StaffEventType.PublicObjection =>
                    $"{staff.FullName} has publicly criticized campaign tactics in an interview with local media. " +
                    $"\"I have serious concerns about the direction we're heading,\" they told reporters.",
                    
                StaffEventType.QuietSabotage =>
                    $"Your {action} operation has mysteriously failed. Documents were misfiled, calls weren't returned, " +
                    $"and key information was 'accidentally' deleted. {staff.FullName} claims to know nothing about it.",
                    
                StaffEventType.MoralUltimatum =>
                    $"{staff.FullName} has requested an urgent private meeting. " +
                    $"\"Either we change course on {action}, or I walk. And I won't walk quietly.\"",
                    
                _ => $"{staff.FullName} is causing problems."
            };
        }
        
        private static int GetConflictSeverity(StaffEventType type)
        {
            return type switch
            {
                StaffEventType.RefusesOrder => 2,
                StaffEventType.PublicObjection => 4,
                StaffEventType.QuietSabotage => 3,
                StaffEventType.MoralUltimatum => 3,
                _ => 2
            };
        }
        
        private static List<StaffEventChoice> GetConflictChoices(StaffEventType type, StaffMember staff)
        {
            var choices = new List<StaffEventChoice>();
            
            switch (type)
            {
                case StaffEventType.RefusesOrder:
                    choices.Add(new StaffEventChoice
                    {
                        Text = "Accept their limits",
                        Effects = new Dictionary<string, float> { { "staff_loyalty", 10 }, { "morale", 5 } },
                        Consequences = "They stay, but won't do certain tasks."
                    });
                    choices.Add(new StaffEventChoice
                    {
                        Text = "Fire them for insubordination",
                        Effects = new Dictionary<string, float> { { "team_morale", -15 }, { "dirt_risk", 0.3f } },
                        Consequences = "They might talk to the press."
                    });
                    choices.Add(new StaffEventChoice
                    {
                        Text = "Pressure them to comply",
                        Effects = new Dictionary<string, float> { { "staff_stress", 30 }, { "betrayal_risk", 0.2f } },
                        Consequences = "They might do it... or they might snap."
                    });
                    break;
                    
                case StaffEventType.PublicObjection:
                    choices.Add(new StaffEventChoice
                    {
                        Text = "Downplay it - 'internal discussions'",
                        Effects = new Dictionary<string, float> { { "media_influence", -10 }, { "trust", -3 } },
                        Consequences = "Might contain the damage... for now."
                    });
                    choices.Add(new StaffEventChoice
                    {
                        Text = "Fire them publicly",
                        Effects = new Dictionary<string, float> { { "trust", -8 }, { "base_loyalty", 5 } },
                        Consequences = "Your base loves loyalty, but it confirms the story."
                    });
                    choices.Add(new StaffEventChoice
                    {
                        Text = "Have a public reconciliation",
                        Effects = new Dictionary<string, float> { { "trust", 5 }, { "political_capital", -10 } },
                        Consequences = "Shows maturity, but you'll have to change course."
                    });
                    break;
                    
                case StaffEventType.QuietSabotage:
                    choices.Add(new StaffEventChoice
                    {
                        Text = "Launch internal investigation",
                        Effects = new Dictionary<string, float> { { "staff_morale", -10 }, { "paranoia", 20 } },
                        Consequences = "Find the culprit, but poison the atmosphere."
                    });
                    choices.Add(new StaffEventChoice
                    {
                        Text = "Let it go, try again",
                        Effects = new Dictionary<string, float> { { "time", -1 }, { "funds", -5000 } },
                        Consequences = "Move on, but they might do it again."
                    });
                    choices.Add(new StaffEventChoice
                    {
                        Text = "Confront the likely suspect",
                        Effects = new Dictionary<string, float> { { "relationship_with_staff", -30 } },
                        Consequences = "If you're right, you have leverage. If wrong, you made an enemy."
                    });
                    break;
                    
                case StaffEventType.MoralUltimatum:
                    choices.Add(new StaffEventChoice
                    {
                        Text = "Change course as they demand",
                        Effects = new Dictionary<string, float> { { "dirty_trick_effectiveness", -0.3f }, { "staff_loyalty", 20 } },
                        Consequences = "Campaign becomes cleaner. For better or worse."
                    });
                    choices.Add(new StaffEventChoice
                    {
                        Text = "Call their bluff",
                        Effects = new Dictionary<string, float> { { "betrayal_risk", 0.5f }, { "scandal_risk", 0.3f } },
                        Consequences = "Either they back down... or they don't."
                    });
                    choices.Add(new StaffEventChoice
                    {
                        Text = "Negotiate a compromise",
                        Effects = new Dictionary<string, float> { { "staff_loyalty", 5 }, { "restrictions", 1 } },
                        Consequences = "They stay, but with conditions."
                    });
                    choices.Add(new StaffEventChoice
                    {
                        Text = "Preemptively fire them with severance",
                        Effects = new Dictionary<string, float> { { "funds", -20000 }, { "team_morale", -5 } },
                        Consequences = "Golden parachute might buy their silence."
                    });
                    break;
            }
            
            return choices;
        }
        
        /// <summary>
        /// Check for arc progression and potential climax
        /// </summary>
        private static StaffEventData CheckArcProgression(StaffMember staff, string action)
        {
            var arc = staff.Personality.CurrentArc;
            var definition = GetArcDefinition(arc);
            
            if (definition == null) return null;
            
            // Check if action accelerates arc
            bool accelerates = definition.TriggerConditions.Any(t => action.Contains(t));
            float progressGain = accelerates ? definition.ProgressRate * 2 : definition.ProgressRate;
            
            staff.Personality.ArcProgress += progressGain;
            staff.Personality.TurnsSinceArcStart++;
            
            // Random foreshadowing
            if (staff.Personality.ArcProgress > 30 && 
                staff.Personality.ArcProgress < 90 && 
                UnityEngine.Random.value < 0.15f)
            {
                string dialogue = CharacterArcManager.GetForeshadowingDialogue(arc, staff.Personality.ArcProgress);
                
                return new StaffEventData
                {
                    Type = StaffEventType.CrisisOfConscience,
                    StaffInvolved = new[] { staff },
                    Title = "A Moment With Staff",
                    Description = $"{staff.FullName} catches you in the hallway. \"{dialogue}\"",
                    Severity = 1,
                    IsForeshadowing = true,
                    ForeshadowingDialogue = dialogue
                };
            }
            
            // Check for climax
            if (staff.Personality.ArcProgress >= 100)
            {
                return GenerateArcClimaxEvent(staff, arc);
            }
            
            return null;
        }
        
        private static CharacterArcDefinition GetArcDefinition(CharacterArc arc)
        {
            // Would normally be in the ArcDefinitions dictionary
            // Simplified for this example
            return null;
        }
        
        private static StaffEventData GenerateArcClimaxEvent(StaffMember staff, CharacterArc arc)
        {
            string climaxEvent = CharacterArcManager.GetClimaxEvent(arc);
            CharacterArcManager.ApplyArcValueChanges(staff, arc);
            
            // Reset arc state
            staff.Personality.CurrentArc = CharacterArc.None;
            staff.Personality.ArcProgress = 0;
            
            var eventData = new StaffEventData
            {
                Type = MapArcToEventType(arc),
                StaffInvolved = new[] { staff },
                Title = GetArcClimaxTitle(arc, staff),
                Description = GetArcClimaxDescription(arc, staff, climaxEvent),
                Severity = GetArcClimaxSeverity(arc),
                Choices = GetArcClimaxChoices(arc, staff),
                IsForeshadowing = false
            };
            
            return eventData;
        }
        
        private static StaffEventType MapArcToEventType(CharacterArc arc)
        {
            return arc switch
            {
                CharacterArc.ReligiousAwakening => StaffEventType.PublicConfession,
                CharacterArc.Whistleblower => StaffEventType.WhistleblowerPress,
                CharacterArc.TellAll => StaffEventType.TellAllAnnouncement,
                CharacterArc.Burnout => StaffEventType.SpectacularMeltdown,
                CharacterArc.Disillusionment => StaffEventType.BridgeBurning,
                CharacterArc.Revenge => StaffEventType.QuietSabotage,
                CharacterArc.RomanticEntanglement => StaffEventType.OfficeRomance,
                _ => StaffEventType.CrisisOfConscience
            };
        }
        
        private static string GetArcClimaxTitle(CharacterArc arc, StaffMember staff)
        {
            return arc switch
            {
                CharacterArc.ReligiousAwakening => $"{staff.FullName} Has Found Jesus",
                CharacterArc.Whistleblower => "BREAKING: Campaign Insider Goes to Press",
                CharacterArc.TellAll => $"'{staff.FullName}' Announces Tell-All Book Deal",
                CharacterArc.Burnout => $"{staff.FullName} Hospitalized for Exhaustion",
                CharacterArc.Disillusionment => $"{staff.FullName} Quits in Scathing Letter",
                CharacterArc.Revenge => "Devastating Leak Rocks Campaign",
                CharacterArc.RomanticEntanglement => "Scandal: Staff Affair with Opposition",
                CharacterArc.Corruption => $"FBI Investigating {staff.FullName}",
                CharacterArc.MoralCrisis => $"{staff.FullName} Takes Moral Stand",
                CharacterArc.Redemption => $"{staff.FullName} Refuses Final Dirty Trick",
                _ => "Staff Crisis"
            };
        }
        
        private static string GetArcClimaxDescription(CharacterArc arc, StaffMember staff, string climaxEvent)
        {
            return arc switch
            {
                CharacterArc.ReligiousAwakening => 
                    $"In a stunning development, {staff.FullName} appeared on morning television to announce " +
                    $"they've 'found God' and can no longer be part of 'morally bankrupt political operations.' " +
                    $"They proceeded to confess to several campaign dirty tricks on live TV, asking for forgiveness " +
                    $"from both the Lord and the American people. Your phone is ringing off the hook.",
                    
                CharacterArc.Whistleblower =>
                    $"{staff.FullName} has gone to the press with documents detailing campaign operations. " +
                    $"Major outlets are running the story. 'I couldn't stay silent anymore,' they told reporters. " +
                    $"'The American people deserve to know what really goes on.'",
                    
                CharacterArc.TellAll =>
                    $"Publishers are in a bidding war for {staff.FullName}'s tell-all memoir about life inside your campaign. " +
                    $"Early excerpts leaked to the press include detailed accounts of strategy meetings, " +
                    $"personal observations about you, and several very unflattering anecdotes.",
                    
                CharacterArc.Burnout =>
                    $"{staff.FullName} collapsed at campaign headquarters and has been hospitalized. " +
                    $"Doctors cite 'severe exhaustion and stress-related illness.' " +
                    $"The media is running stories about 'toxic campaign culture.'",
                    
                CharacterArc.Disillusionment =>
                    $"{staff.FullName} has resigned and released a public letter: " +
                    $"'I joined this campaign believing in change. Instead, I watched everything I believed in " +
                    $"get sacrificed for political expediency. I cannot be part of this anymore.'",
                    
                _ => $"{staff.FullName} has caused a major crisis."
            };
        }
        
        private static int GetArcClimaxSeverity(CharacterArc arc)
        {
            return arc switch
            {
                CharacterArc.ReligiousAwakening => 5, // Maximum chaos
                CharacterArc.Whistleblower => 5,
                CharacterArc.TellAll => 4,
                CharacterArc.Burnout => 3,
                CharacterArc.Disillusionment => 3,
                CharacterArc.Revenge => 4,
                CharacterArc.RomanticEntanglement => 4,
                CharacterArc.Corruption => 5,
                _ => 3
            };
        }
        
        private static List<StaffEventChoice> GetArcClimaxChoices(CharacterArc arc, StaffMember staff)
        {
            // Specific choices based on arc
            var choices = new List<StaffEventChoice>();
            
            // Add common damage control options
            choices.Add(new StaffEventChoice
            {
                Text = "Damage control - deny everything",
                Effects = new Dictionary<string, float> { { "trust", -10 }, { "media_influence", -20 } },
                Consequences = "They have receipts. This could backfire."
            });
            
            choices.Add(new StaffEventChoice
            {
                Text = "Get ahead of it - partial admission",
                Effects = new Dictionary<string, float> { { "trust", -5 }, { "political_capital", -15 } },
                Consequences = "Controlled burn. Painful but survivable."
            });
            
            choices.Add(new StaffEventChoice
            {
                Text = "Attack their credibility",
                Effects = new Dictionary<string, float> { { "alignment_evil", 5 }, { "media_influence", -10 } },
                Consequences = "Risky. If they're sympathetic, you look like the villain."
            });
            
            return choices;
        }
        
        /// <summary>
        /// Check for relationship-based events between staff
        /// </summary>
        private static List<StaffEventData> CheckRelationshipEvents(StaffMember staff, StaffRoster roster)
        {
            var events = new List<StaffEventData>();
            
            // Check each relationship
            foreach (var rel in staff.Personality.Relationships.Values)
            {
                var otherStaff = roster.ActiveStaff.FirstOrDefault(s => s.Id == rel.OtherStaffId);
                if (otherStaff == null) continue;
                
                // Office romance potential
                if (rel.Opinion > 70 && !rel.HasRomantic && UnityEngine.Random.value < 0.02f)
                {
                    if (!staff.Personality.HasValue(StaffValues.FamilyFirst) &&
                        !otherStaff.Personality.HasValue(StaffValues.FamilyFirst))
                    {
                        rel.HasRomantic = true;
                        events.Add(new StaffEventData
                        {
                            Type = StaffEventType.OfficeRomance,
                            StaffInvolved = new[] { staff, otherStaff },
                            Title = "Office Romance",
                            Description = $"Rumor has it that {staff.FullName} and {otherStaff.FullName} " +
                                         $"have become... close. Very close.",
                            Severity = 2
                        });
                    }
                }
                
                // Rivalry escalation
                if (rel.Opinion < -50 && rel.HasRivalry && UnityEngine.Random.value < 0.05f)
                {
                    events.Add(new StaffEventData
                    {
                        Type = StaffEventType.RivalryEscalates,
                        StaffInvolved = new[] { staff, otherStaff },
                        Title = "Staff Feud Goes Public",
                        Description = $"The feud between {staff.FullName} and {otherStaff.FullName} " +
                                     $"has spilled into public view. Reporters are asking questions.",
                        Severity = 3
                    });
                }
            }
            
            return events;
        }
    }
    
    #endregion
    
    #region Staff Event Data
    
    /// <summary>
    /// Complete event data for staff-related events
    /// </summary>
    [Serializable]
    public class StaffEventData
    {
        public StaffEventType Type;
        public StaffMember[] StaffInvolved;
        public string Title;
        public string Description;
        public int Severity;                    // 1-5 scale
        public List<StaffEventChoice> Choices;
        public bool IsForeshadowing;
        public string ForeshadowingDialogue;
        public bool RequiresResponse;
        public int TurnsToRespond;
        
        public StaffEventData()
        {
            Choices = new List<StaffEventChoice>();
            RequiresResponse = true;
            TurnsToRespond = 1;
        }
    }
    
    /// <summary>
    /// Choice option for staff events
    /// </summary>
    [Serializable]
    public class StaffEventChoice
    {
        public string Text;
        public Dictionary<string, float> Effects;
        public string Consequences;
        public string[] RequiredResources;
        public float SuccessChance;             // 0-1, for risky options
        
        public StaffEventChoice()
        {
            Effects = new Dictionary<string, float>();
            SuccessChance = 1.0f;
        }
    }
    
    #endregion
    
    #region Personality Generator Extension
    
    /// <summary>
    /// Extension to generate rich personalities for staff members
    /// </summary>
    public static class PersonalityGenerator
    {
        /// <summary>
        /// Generate a complete personality for a staff member
        /// </summary>
        public static StaffPersonality Generate(StaffMember staff)
        {
            var personality = new StaffPersonality();
            
            // Generate values based on specialization
            personality.Values = GenerateValuesForSpec(staff.PrimarySpecialization);
            
            // Adjust based on tier (higher tier = more defined values)
            if (staff.Tier >= StaffTier.Senior)
            {
                // Add secondary values
                var additionalValues = GenerateRandomValues(2);
                personality.Values |= additionalValues;
            }
            
            // Set value strengths
            foreach (StaffValues value in Enum.GetValues(typeof(StaffValues)))
            {
                if (personality.HasValue(value))
                {
                    personality.ValueStrength[value] = UnityEngine.Random.Range(40f, 90f);
                }
            }
            
            // Generate traits (2-4)
            int traitCount = UnityEngine.Random.Range(2, 5);
            personality.Traits = GenerateTraits(traitCount, staff);
            
            // Maybe hidden traits
            if (UnityEngine.Random.value < 0.3f)
            {
                personality.HiddenTraits.Add(GenerateHiddenTrait());
            }
            
            // Dark secret (20% chance)
            if (UnityEngine.Random.value < 0.2f)
            {
                personality.DarkSecret = GenerateDarkSecret();
            }
            
            // Tolerance based on personality
            personality.ValueViolationTolerance = CalculateTolerance(personality);
            
            return personality;
        }
        
        private static StaffValues GenerateValuesForSpec(StaffSpecialization spec)
        {
            return spec switch
            {
                // Campaign staff tend to be pragmatic
                StaffSpecialization.CampaignManager => 
                    StaffValues.Pragmatist | StaffValues.Workaholic | StaffValues.TeamPlayer,
                    
                StaffSpecialization.PressSecretary =>
                    StaffValues.Extrovert | StaffValues.Creative | StaffValues.Pragmatist,
                    
                // Dark specialists are ruthless
                StaffSpecialization.Fixer =>
                    StaffValues.Ruthless | StaffValues.Mercenary | StaffValues.Cautious, // Discretion is a stat, using Cautious as equivalent
                    
                StaffSpecialization.OppositionResearcher =>
                    StaffValues.Ruthless | StaffValues.LoneWolf | StaffValues.Cautious,
                    
                StaffSpecialization.BagMan =>
                    StaffValues.Ruthless | StaffValues.Mercenary | StaffValues.Reckless,
                    
                // Support staff more likely to be idealistic
                StaffSpecialization.PolicyAdvisor =>
                    StaffValues.Idealist | StaffValues.TrueBeliever | StaffValues.ByTheBook,
                    
                StaffSpecialization.FieldOrganizer =>
                    StaffValues.TrueBeliever | StaffValues.Extrovert | StaffValues.Populist,
                    
                // Lawyers are by-the-book
                StaffSpecialization.LegalCounsel =>
                    StaffValues.ByTheBook | StaffValues.Cautious | StaffValues.Pragmatist,
                    
                _ => StaffValues.Pragmatist | StaffValues.TeamPlayer
            };
        }
        
        private static StaffValues GenerateRandomValues(int count)
        {
            StaffValues result = StaffValues.None;
            var allValues = Enum.GetValues(typeof(StaffValues)).Cast<StaffValues>()
                .Where(v => v != StaffValues.None).ToList();
            
            for (int i = 0; i < count && allValues.Count > 0; i++)
            {
                int idx = UnityEngine.Random.Range(0, allValues.Count);
                result |= allValues[idx];
                allValues.RemoveAt(idx);
            }
            
            return result;
        }
        
        private static List<PersonalityTrait> GenerateTraits(int count, StaffMember staff)
        {
            var traits = new List<PersonalityTrait>();
            var allTraits = Enum.GetValues(typeof(PersonalityTrait)).Cast<PersonalityTrait>().ToList();
            
            // Remove hidden/special traits from random pool
            allTraits.RemoveAll(t => t >= PersonalityTrait.SecretlyRecording);
            
            // Higher competence = more positive traits
            float positiveChance = staff.Competence / 100f;
            
            for (int i = 0; i < count; i++)
            {
                var trait = allTraits[UnityEngine.Random.Range(0, allTraits.Count)];
                
                // Bias towards appropriate traits
                if (UnityEngine.Random.value < positiveChance)
                {
                    // Try to get positive trait
                    trait = allTraits.Where(t => (int)t <= 9)
                        .OrderBy(_ => UnityEngine.Random.value)
                        .FirstOrDefault();
                }
                
                if (!traits.Contains(trait))
                    traits.Add(trait);
            }
            
            return traits;
        }
        
        private static PersonalityTrait GenerateHiddenTrait()
        {
            var hiddenTraits = new[]
            {
                PersonalityTrait.SecretlyRecording,
                PersonalityTrait.DoubleLife,
                PersonalityTrait.DeepCover,
                PersonalityTrait.TimeБомб
            };
            
            return hiddenTraits[UnityEngine.Random.Range(0, hiddenTraits.Length)];
        }
        
        private static string GenerateDarkSecret()
        {
            var secrets = new[]
            {
                "Past affair with a married politician",
                "Secretly worked for opponent's campaign previously",
                "Has a substance abuse problem they're hiding",
                "Fabricated parts of their resume",
                "Was fired from previous job for misconduct",
                "Has massive gambling debts",
                "Is being blackmailed by unknown party",
                "Previously leaked info to the press",
                "Has family member with criminal connections",
                "Is writing a tell-all book",
                "Has undisclosed romantic relationship with journalist",
                "Secretly disagrees with everything you stand for"
            };
            
            return secrets[UnityEngine.Random.Range(0, secrets.Length)];
        }
        
        private static float CalculateTolerance(StaffPersonality personality)
        {
            float tolerance = 50f;
            
            // Idealists have lower tolerance
            if (personality.HasValue(StaffValues.Idealist))
                tolerance -= 20f;
            if (personality.HasValue(StaffValues.Principled))
                tolerance -= 15f;
            if (personality.HasValue(StaffValues.Religious))
                tolerance -= 10f;
                
            // Pragmatists have higher tolerance
            if (personality.HasValue(StaffValues.Pragmatist))
                tolerance += 20f;
            if (personality.HasValue(StaffValues.Ruthless))
                tolerance += 30f;
            if (personality.HasValue(StaffValues.Mercenary))
                tolerance += 15f;
                
            // Traits affect it too
            if (personality.Traits.Contains(PersonalityTrait.Stubborn))
                tolerance -= 10f;
            if (personality.Traits.Contains(PersonalityTrait.Volatile))
                tolerance -= 15f;
            if (personality.Traits.Contains(PersonalityTrait.Resilient))
                tolerance += 10f;
            
            return Mathf.Clamp(tolerance, 10f, 100f);
        }
    }
    
    #endregion
}
