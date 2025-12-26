using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - COLORFUL TOWNSFOLK PERSONALITIES
// Memorable, quirky, dramatic citizens with full personalities
// The people you'll never forget meeting on the trail
// ═══════════════════════════════════════════════════════════════════════════════

namespace ElectionEmpire.CampaignTrail
{
    #region Colorful Citizen Archetypes
    
    /// <summary>
    /// A memorable, colorful citizen with a full personality
    /// </summary>
    [Serializable]
    public class ColorfulCitizen : Citizen
    {
        public CitizenArchetype Archetype;
        public string PersonalityQuirk;
        public string MemorableTrait;
        public List<string> Catchphrases;
        public string Backstory;
        public bool HasUniqueInteraction;
        public string UniqueInteractionType;
        public float Memorability; // 0-100, how memorable this encounter will be
        
        // Convenience properties for name parsing
        public string FirstName
        {
            get
            {
                if (string.IsNullOrEmpty(Name)) return "Unknown";
                var parts = Name.Split(' ');
                return parts.Length > 0 ? parts[0] : Name;
            }
        }
        
        public string LastName
        {
            get
            {
                if (string.IsNullOrEmpty(Name)) return "";
                var parts = Name.Split(' ');
                return parts.Length > 1 ? parts[parts.Length - 1] : "";
            }
        }
        
        public ColorfulCitizen()
        {
            Catchphrases = new List<string>();
        }
    }
    
    public enum CitizenArchetype
    {
        // Positive
        SuperFan,               // Loves you unconditionally
        TrueBeliever,           // Believes in your cause
        LocalHero,              // Respected community member
        WiseElder,              // Old person with wisdom
        InspiringYouth,         // Young person with hope
        
        // Neutral
        CynicalVeteran,         // Seen it all, trusts no one
        UndecidedSwingVoter,    // Genuinely on the fence
        IssueSingleVoter,       // Cares about ONE thing
        ConspiracyTheorist,     // Believes in wild theories
        LocalBusinessOwner,     // Small business perspective
        
        // Negative
        AngryFormerSupporter,   // Used to support you, feels betrayed
        ProfessionalHeckler,    // Does this at every rally
        GrievanceHolder,        // Has a specific grudge
        ProtestOrganizer,       // Leading the opposition
        Troublemaker,           // Just wants to cause chaos
        
        // Wild Cards
        EccentricMillionaire,   // Rich, weird, unpredictable
        FormerRival,            // Ran against you before
        ExEmployee,             // Used to work for you
        FamilyMemberOfVictim,   // Lost someone, blames you
        ProphecyDeliverer,       // Thinks they have a message from God
        MarriageProposer,       // Wants to marry you (weird)
        FanFictionAuthor,       // Wrote stories about you
        TimeTraveler             // Claims to be from the future
    }
    
    /// <summary>
    /// Generates colorful, memorable citizens
    /// </summary>
    public static class ColorfulCitizenGenerator
    {
        /// <summary>
        /// Generate a random colorful citizen
        /// </summary>
        public static ColorfulCitizen GenerateRandomCitizen()
        {
            float hostilityLevel = UnityEngine.Random.value;
            TrailEventType eventType = (TrailEventType)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(TrailEventType)).Length);
            return GenerateColorful(eventType, hostilityLevel);
        }
        
        /// <summary>
        /// Generate a colorful citizen with full personality
        /// </summary>
        public static ColorfulCitizen GenerateColorful(TrailEventType eventType, float hostilityLevel)
        {
            var citizen = CitizenGenerator.Generate(eventType, hostilityLevel) as ColorfulCitizen;
            if (citizen == null)
            {
                citizen = new ColorfulCitizen();
                // Copy base properties
                var baseCitizen = CitizenGenerator.Generate(eventType, hostilityLevel);
                // ... copy properties ...
            }
            
            // Determine archetype
            citizen.Archetype = DetermineArchetype(citizen, hostilityLevel);
            
            // Generate personality based on archetype
            GeneratePersonality(citizen);
            
            // Generate memorable traits
            citizen.MemorableTrait = GenerateMemorableTrait(citizen);
            
            // Generate catchphrases
            citizen.Catchphrases = GenerateCatchphrases(citizen);
            
            // Generate backstory
            citizen.Backstory = GenerateBackstory(citizen);
            
            // Generate unique interaction
            if (UnityEngine.Random.value < 0.3f) // 30% chance
            {
                citizen.HasUniqueInteraction = true;
                citizen.UniqueInteractionType = GenerateUniqueInteraction(citizen);
            }
            
            // Calculate memorability
            citizen.Memorability = CalculateMemorability(citizen);
            
            return citizen;
        }
        
        private static CitizenArchetype DetermineArchetype(Citizen citizen, float hostilityLevel)
        {
            float roll = UnityEngine.Random.value;
            
            // Adjust based on disposition
            if (citizen.Disposition == CitizenDisposition.Supporter)
            {
                if (roll < 0.2f) return CitizenArchetype.SuperFan;
                if (roll < 0.4f) return CitizenArchetype.TrueBeliever;
                if (roll < 0.5f) return CitizenArchetype.LocalHero;
                return CitizenArchetype.InspiringYouth;
            }
            
            if (citizen.Disposition == CitizenDisposition.Undecided)
            {
                if (roll < 0.3f) return CitizenArchetype.UndecidedSwingVoter;
                if (roll < 0.5f) return CitizenArchetype.IssueSingleVoter;
                if (roll < 0.7f) return CitizenArchetype.CynicalVeteran;
                return CitizenArchetype.LocalBusinessOwner;
            }
            
            if (citizen.Disposition >= CitizenDisposition.HostileOpponent)
            {
                if (roll < 0.2f) return CitizenArchetype.AngryFormerSupporter;
                if (roll < 0.4f) return CitizenArchetype.ProfessionalHeckler;
                if (roll < 0.6f) return CitizenArchetype.GrievanceHolder;
                if (roll < 0.8f) return CitizenArchetype.ProtestOrganizer;
                return CitizenArchetype.Troublemaker;
            }
            
            // Wild cards (rare)
            if (roll < 0.05f) return CitizenArchetype.EccentricMillionaire;
            if (roll < 0.08f) return CitizenArchetype.FormerRival;
            if (roll < 0.1f) return CitizenArchetype.ExEmployee;
            if (roll < 0.12f) return CitizenArchetype.FamilyMemberOfVictim;
            if (roll < 0.14f) return CitizenArchetype.ProphecyDeliverer;
            if (roll < 0.16f) return CitizenArchetype.MarriageProposer;
            if (roll < 0.18f) return CitizenArchetype.FanFictionAuthor;
            if (roll < 0.2f) return CitizenArchetype.TimeTraveler;
            
            return CitizenArchetype.UndecidedSwingVoter;
        }
        
        private static void GeneratePersonality(ColorfulCitizen citizen)
        {
            citizen.PersonalityQuirk = citizen.Archetype switch
            {
                CitizenArchetype.SuperFan => new[] {
                    "Has a tattoo of your face",
                    "Named their pet after you",
                    "Has a shrine to you in their house",
                    "Follows you to every event",
                    "Has memorized all your speeches"
                }[UnityEngine.Random.Range(0, 5)],
                
                CitizenArchetype.TrueBeliever => new[] {
                    "Sees you as the chosen one",
                    "Believes you'll save the country",
                    "Has been waiting for someone like you",
                    "Thinks you're the answer to all problems",
                    "Sees your campaign as a movement"
                }[UnityEngine.Random.Range(0, 5)],
                
                CitizenArchetype.CynicalVeteran => new[] {
                    "Has seen 50 years of broken promises",
                    "Trusts no one, but might trust you",
                    "Has a 'prove it' attitude",
                    "Seen every politician fail",
                    "Wants to believe but can't"
                }[UnityEngine.Random.Range(0, 5)],
                
                CitizenArchetype.ConspiracyTheorist => new[] {
                    "Believes in chemtrails",
                    "Thinks the government is run by lizards",
                    "Has 'proof' of a cover-up",
                    "Wants to tell you about 'the truth'",
                    "Has a binder full of 'evidence'"
                }[UnityEngine.Random.Range(0, 5)],
                
                CitizenArchetype.AngryFormerSupporter => new[] {
                    "Voted for you last time, feels betrayed",
                    "Believed in you, now feels foolish",
                    "Thinks you sold out",
                    "Wants to know why you changed",
                    "Feels personally betrayed"
                }[UnityEngine.Random.Range(0, 5)],
                
                CitizenArchetype.ProphecyDeliverer => new[] {
                    "Claims God sent them with a message",
                    "Has a 'vision' about your future",
                    "Thinks you're part of a prophecy",
                    "Wants to lay hands on you",
                    "Has a scroll with your name on it"
                }[UnityEngine.Random.Range(0, 5)],
                
                CitizenArchetype.MarriageProposer => new[] {
                    "Wants to marry you right now",
                    "Has a ring ready",
                    "Thinks you're their soulmate",
                    "Has been waiting for this moment",
                    "Brought their family to witness"
                }[UnityEngine.Random.Range(0, 5)],
                
                CitizenArchetype.FanFictionAuthor => new[] {
                    "Wrote a 200-page novel about you",
                    "Has fan art of you",
                    "Wants you to read their work",
                    "Thinks you're perfect for their story",
                    "Has shipped you with your opponent"
                }[UnityEngine.Random.Range(0, 5)],
                
                CitizenArchetype.TimeTraveler => new[] {
                    "Claims to be from 2050",
                    "Says they know how your campaign ends",
                    "Has 'proof' they're from the future",
                    "Wants to warn you about something",
                    "Thinks you're important to history"
                }[UnityEngine.Random.Range(0, 5)],
                
                _ => "Standard personality"
            };
        }
        
        private static string GenerateMemorableTrait(ColorfulCitizen citizen)
        {
            return citizen.Archetype switch
            {
                CitizenArchetype.SuperFan => new[] {
                    "Wearing a homemade t-shirt with your face on it",
                    "Holding a sign with your name misspelled",
                    "Has brought you a gift (homemade cookies)",
                    "Crying tears of joy at seeing you",
                    "Has their entire family dressed in your campaign colors"
                }[UnityEngine.Random.Range(0, 5)],
                
                CitizenArchetype.EccentricMillionaire => new[] {
                    "Wearing a $10,000 suit but covered in cat hair",
                    "Has a bodyguard who looks confused",
                    "Driving a gold-plated car",
                    "Offering you money directly",
                    "Has a pet monkey on their shoulder"
                }[UnityEngine.Random.Range(0, 5)],
                
                CitizenArchetype.ProphecyDeliverer => new[] {
                    "Holding a scroll with ancient-looking writing",
                    "Wearing robes and a staff",
                    "Has a group of followers",
                    "Speaking in what they claim is tongues",
                    "Has a crystal ball"
                }[UnityEngine.Random.Range(0, 5)],
                
                CitizenArchetype.MarriageProposer => new[] {
                    "Down on one knee with a ring",
                    "Has a wedding dress on",
                    "Brought a priest",
                    "Has 'MARRY ME' written on their shirt",
                    "Holding a bouquet of flowers"
                }[UnityEngine.Random.Range(0, 5)],
                
                CitizenArchetype.FanFictionAuthor => new[] {
                    "Holding a binder full of printed pages",
                    "Has fan art of you",
                    "Wearing a t-shirt with your face photoshopped",
                    "Has a laptop open to their story",
                    "Surrounded by notebooks"
                }[UnityEngine.Random.Range(0, 5)],
                
                CitizenArchetype.TimeTraveler => new[] {
                    "Wearing clothes that look futuristic (or just weird)",
                    "Has a device that beeps",
                    "Speaking in what they claim is future slang",
                    "Has 'proof' of time travel (probably fake)",
                    "Acting like they know everything"
                }[UnityEngine.Random.Range(0, 5)],
                
                _ => "Standard appearance"
            };
        }
        
        private static List<string> GenerateCatchphrases(ColorfulCitizen citizen)
        {
            var catchphrases = new List<string>();
            
            switch (citizen.Archetype)
            {
                case CitizenArchetype.SuperFan:
                    catchphrases.AddRange(new[] {
                        "I LOVE YOU!",
                        "YOU'RE THE BEST!",
                        "I'VE BEEN WAITING FOR THIS MOMENT!",
                        "CAN I HUG YOU?",
                        "YOU'RE GOING TO SAVE US ALL!"
                    });
                    break;
                    
                case CitizenArchetype.CynicalVeteran:
                    catchphrases.AddRange(new[] {
                        "I've heard it all before.",
                        "Prove it.",
                        "Words are cheap.",
                        "I'll believe it when I see it.",
                        "You're all the same."
                    });
                    break;
                    
                case CitizenArchetype.ConspiracyTheorist:
                    catchphrases.AddRange(new[] {
                        "But what about the chemtrails?",
                        "The government doesn't want you to know...",
                        "I have proof!",
                        "They're hiding the truth!",
                        "Wake up, sheeple!"
                    });
                    break;
                    
                case CitizenArchetype.AngryFormerSupporter:
                    catchphrases.AddRange(new[] {
                        "I believed in you!",
                        "You promised!",
                        "How could you?",
                        "I trusted you!",
                        "You sold us out!"
                    });
                    break;
                    
                case CitizenArchetype.ProphecyDeliverer:
                    catchphrases.AddRange(new[] {
                        "The prophecy foretold your coming!",
                        "You are the chosen one!",
                        "The stars have aligned!",
                        "God has sent me with a message!",
                        "The ancient texts speak of you!"
                    });
                    break;
            }
            
            return catchphrases;
        }
        
        private static string GenerateBackstory(ColorfulCitizen citizen)
        {
            // Pre-calculate values that need computation
            string circumstance = UnityEngine.Random.value > 0.5f ? "mysterious" : "hostile";
            string familyMember = new[] { "son", "daughter", "brother", "sister", "father", "mother" }[UnityEngine.Random.Range(0, 6)];
            
            return citizen.Archetype switch
            {
                CitizenArchetype.AngryFormerSupporter =>
                    $"Voted for you in {UnityEngine.Random.Range(2016, 2024)}. " +
                    $"Believed your promises. Watched you break them. " +
                    $"Feels personally betrayed. Wants answers.",
                    
                CitizenArchetype.FamilyMemberOfVictim =>
                    $"Lost their {familyMember} " +
                    $"due to {citizen.GrievanceIfAny}. Blames you. " +
                    $"Has been waiting for this moment. Wants you to look them in the eye.",
                    
                CitizenArchetype.ExEmployee =>
                    $"Worked for your campaign in {UnityEngine.Random.Range(2018, 2024)}. " +
                    $"Was fired/quit under {circumstance} circumstances. " +
                    $"Knows things. Might reveal them.",
                    
                CitizenArchetype.FormerRival =>
                    $"Ran against you in {UnityEngine.Random.Range(2016, 2024)}. " +
                    $"Lost. Still bitter. " +
                    $"Wants to know why you won. Might have dirt.",
                    
                CitizenArchetype.ProphecyDeliverer =>
                    $"Had a vision {UnityEngine.Random.Range(1, 10)} years ago. " +
                    $"Saw your face. Knew you were coming. " +
                    $"Has been preparing. This is their moment.",
                    
                CitizenArchetype.TimeTraveler =>
                    $"Claims to be from the year {UnityEngine.Random.Range(2050, 2100)}. " +
                    $"Says they know how your campaign ends. " +
                    $"Wants to warn you about something. " +
                    $"Or maybe they're just crazy.",
                    
                _ => "Standard backstory"
            };
        }
        
        private static string GenerateUniqueInteraction(ColorfulCitizen citizen)
        {
            return citizen.Archetype switch
            {
                CitizenArchetype.MarriageProposer => "marriage_proposal",
                CitizenArchetype.ProphecyDeliverer => "prophecy_delivery",
                CitizenArchetype.FanFictionAuthor => "fan_fiction_revelation",
                CitizenArchetype.TimeTraveler => "future_warning",
                CitizenArchetype.EccentricMillionaire => "direct_donation_offer",
                CitizenArchetype.ExEmployee => "former_employee_confrontation",
                CitizenArchetype.FamilyMemberOfVictim => "emotional_confrontation",
                _ => "standard"
            };
        }
        
        private static float CalculateMemorability(ColorfulCitizen citizen)
        {
            float memorability = 0;
            
            if (citizen.Archetype == CitizenArchetype.MarriageProposer) memorability += 50;
            if (citizen.Archetype == CitizenArchetype.TimeTraveler) memorability += 40;
            if (citizen.Archetype == CitizenArchetype.ProphecyDeliverer) memorability += 35;
            if (citizen.Archetype == CitizenArchetype.FanFictionAuthor) memorability += 30;
            if (citizen.HasSecretAboutCandidate) memorability += 40;
            if (citizen.HasProjectiles) memorability += 25;
            if (citizen.IsDrunk) memorability += 15;
            if (citizen.HasUniqueInteraction) memorability += 20;
            
            return Mathf.Clamp(memorability, 0, 100);
        }
    }
    
    #endregion
    
    #region Unique Interaction Handlers
    
    /// <summary>
    /// Handles unique, memorable interactions
    /// </summary>
    public static class UniqueInteractionHandler
    {
        public static string HandleMarriageProposal(ColorfulCitizen citizen)
        {
            return $"*drops to one knee* \"{citizen.FirstName} {citizen.LastName}, " +
                   $"I've been waiting for this moment my whole life. " +
                   $"Will you marry me? I have a ring! I have a priest! " +
                   $"I have everything ready!\" *crowd gasps*";
        }
        
        public static string HandleProphecyDelivery(ColorfulCitizen citizen)
        {
            return $"*unrolls scroll* \"The prophecy foretold your coming! " +
                   $"In the year of the great reckoning, a leader shall rise! " +
                   $"You are the chosen one! The stars have aligned! " +
                   $"But beware... the dark forces gather...\" *crowd looks confused*";
        }
        
        public static string HandleFanFictionRevelation(ColorfulCitizen citizen)
        {
            return $"*pulls out binder* \"I wrote a 200-page novel about you! " +
                   $"It's called 'The Candidate's Secret Love.' " +
                   $"Can you read it? I think you'll love it! " +
                   $"Especially chapter 47...\" *winks* *crowd cringes*";
        }
        
        public static string HandleTimeTravelerWarning(ColorfulCitizen citizen)
        {
            return $"*device beeps* \"I'm from the year 2050! " +
                   $"I came back to warn you! " +
                   $"If you win, the timeline will be destroyed! " +
                   $"Or... wait, maybe it's if you lose? " +
                   $"I can't remember...\" *device beeps again*";
        }
        
        public static string HandleEccentricMillionaireOffer(ColorfulCitizen citizen)
        {
            return $"*pulls out checkbook* \"I'll give you $1 million right now. " +
                   $"Cash. No questions asked. " +
                   $"Just promise me one thing...\" *monkey on shoulder chatters*";
        }
    }
    
    #endregion
}

