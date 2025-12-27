using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.Character;
using ElectionEmpire.World;
using ElectionEmpire.Core;

namespace ElectionEmpire.AI
{
    /// <summary>
    /// Generates AI opponents with distinct personalities and archetypes
    /// </summary>
    public class AIOpponentGenerator
    {
        private CharacterGenerator characterGen;
        private WorldNameGenerator nameGen;
        
        public AIOpponentGenerator()
        {
            characterGen = new CharacterGenerator();
            nameGen = new WorldNameGenerator();
        }
        
        public AIOpponent GenerateOpponent(int playerTier, AIDifficulty difficulty, string seed = null)
        {
            if (seed != null)
                UnityEngine.Random.InitState(seed.GetHashCode());
            
            var opponent = new AIOpponent
            {
                ID = System.Guid.NewGuid().ToString(),
                Difficulty = difficulty
            };
            
            // 1. Select archetype
            opponent.Archetype = SelectArchetype();
            
            // 2. Generate character (using existing system)
            opponent.Character = GenerateCharacterForArchetype(opponent.Archetype);
            
            // 3. Generate name
            opponent.Name = GenerateName(opponent.Character.Background);
            opponent.GeneratedNickname = GenerateNickname(opponent);
            
            // 4. Create personality matrix
            opponent.Personality = GeneratePersonality(opponent.Archetype, difficulty);
            
            // 5. Generate backstory
            opponent.Backstory = GenerateBackstory(opponent);
            
            // 6. Select signature moves
            opponent.SignatureMoves = SelectSignatureMoves(opponent.Archetype, 3);
            
            // 7. Initialize resources
            opponent.Resources = InitializeResources(playerTier, opponent.Character);
            
            // 8. Set initial strategy
            opponent.CurrentStrategy = DetermineInitialStrategy(opponent.Personality);
            
            // 9. Set goals
            opponent.Goals = GenerateGoals(opponent);
            
            // 10. Initialize voter bloc support
            InitializeVoterBlocSupport(opponent);
            
            // 11. Initialize policy stances
            InitializePolicyStances(opponent);
            
            return opponent;
        }
        
        private Archetype SelectArchetype()
        {
            // Weighted selection (some archetypes rarer than others)
            
            // Common archetypes (40% chance)
            var common = new[] { 
                Archetype.Idealist, Archetype.Populist, Archetype.Insider, Archetype.Survivor 
            };
            
            // Uncommon archetypes (40% chance)
            var uncommon = new[] { 
                Archetype.MachineBoss, Archetype.Technocrat, Archetype.Showman, Archetype.Corporate 
            };
            
            // Rare archetypes (20% chance)
            var rare = new[] { 
                Archetype.Maverick, Archetype.DynastyHeir, Archetype.Zealot, Archetype.Revolutionary 
            };
            
            float roll = UnityEngine.Random.value;
            if (roll < 0.4f)
                return common[UnityEngine.Random.Range(0, common.Length)];
            else if (roll < 0.8f)
                return uncommon[UnityEngine.Random.Range(0, uncommon.Length)];
            else
                return rare[UnityEngine.Random.Range(0, rare.Length)];
        }
        
        private ElectionEmpire.Character.Character GenerateCharacterForArchetype(Archetype archetype)
        {
            // Use CharacterGenerator but bias toward archetype-appropriate choices
            RandomMode mode = RandomMode.Balanced;
            
            switch (archetype)
            {
                case Archetype.Maverick:
                case Archetype.Revolutionary:
                    mode = RandomMode.Chaos;
                    break;
                
                case Archetype.Idealist:
                case Archetype.Technocrat:
                    mode = RandomMode.Balanced;
                    break;
            }
            
            var character = characterGen.GenerateRandom(mode);
            
            // Override some traits to match archetype (simplified for now)
            // In full implementation, would adjust background, skills, etc.
            
            return character;
        }
        
        private string GenerateName(BackgroundData background)
        {
            // First name pools
            string[] firstNames = new[] {
                "Alex", "Jordan", "Morgan", "Taylor", "Casey",
                "Riley", "Quinn", "Avery", "Parker", "Reese",
                "Cameron", "Dakota", "Skyler", "Sage", "River",
                "Blake", "Emery", "Finley", "Hayden", "Kai"
            };
            
            // Last name pools
            string[] lastNames = new[] {
                "Anderson", "Brooks", "Carter", "Davis", "Evans",
                "Foster", "Garcia", "Harris", "Irving", "Johnson",
                "Kennedy", "Lopez", "Martinez", "Nelson", "O'Brien",
                "Peterson", "Quinn", "Rodriguez", "Smith", "Thompson",
                "Walker", "White", "Williams", "Wilson", "Young"
            };
            
            string firstName = firstNames[UnityEngine.Random.Range(0, firstNames.Length)];
            string lastName = lastNames[UnityEngine.Random.Range(0, lastNames.Length)];
            
            return $"{firstName} {lastName}";
        }
        
        private string GenerateNickname(AIOpponent opponent)
        {
            // Generate descriptive nickname based on personality and background
            
            string[] adjectives;
            
            // Select adjectives based on personality
            if (opponent.Personality.Charisma > 70)
                adjectives = new[] { "Charismatic", "Silver-Tongued", "Magnetic" };
            else if (opponent.Personality.Cunning > 70)
                adjectives = new[] { "Cunning", "Shrewd", "Calculating" };
            else if (opponent.Personality.Aggression > 70)
                adjectives = new[] { "Fierce", "Ruthless", "Relentless" };
            else if (opponent.Personality.EthicalFlexibility < 30)
                adjectives = new[] { "Principled", "Honest", "Ethical" };
            else
                adjectives = new[] { "Ambitious", "Strategic", "Determined" };
            
            string adj = adjectives[UnityEngine.Random.Range(0, adjectives.Length)];
            string role = opponent.Character.Background != null 
                ? opponent.Character.Background.Name 
                : "Politician";
            
            return $"The {adj} {role}";
        }
        
        private PersonalityMatrix GeneratePersonality(Archetype archetype, AIDifficulty difficulty)
        {
            var personality = new PersonalityMatrix();
            
            // Base traits on archetype
            switch (archetype)
            {
                case Archetype.Idealist:
                    personality.Aggression = UnityEngine.Random.Range(20f, 40f);
                    personality.RiskTolerance = UnityEngine.Random.Range(30f, 50f);
                    personality.EthicalFlexibility = UnityEngine.Random.Range(10f, 30f);
                    personality.Loyalty = UnityEngine.Random.Range(60f, 80f);
                    personality.Adaptability = UnityEngine.Random.Range(40f, 60f);
                    personality.Charisma = UnityEngine.Random.Range(50f, 70f);
                    personality.Cunning = UnityEngine.Random.Range(30f, 50f);
                    personality.Impulsiveness = UnityEngine.Random.Range(30f, 50f);
                    break;
                
                case Archetype.MachineBoss:
                    personality.Aggression = UnityEngine.Random.Range(50f, 70f);
                    personality.RiskTolerance = UnityEngine.Random.Range(30f, 50f);
                    personality.EthicalFlexibility = UnityEngine.Random.Range(60f, 80f);
                    personality.Loyalty = UnityEngine.Random.Range(70f, 90f);
                    personality.Adaptability = UnityEngine.Random.Range(40f, 60f);
                    personality.Charisma = UnityEngine.Random.Range(40f, 60f);
                    personality.Cunning = UnityEngine.Random.Range(70f, 90f);
                    personality.Impulsiveness = UnityEngine.Random.Range(20f, 40f);
                    break;
                
                case Archetype.Populist:
                    personality.Aggression = UnityEngine.Random.Range(60f, 80f);
                    personality.RiskTolerance = UnityEngine.Random.Range(50f, 70f);
                    personality.EthicalFlexibility = UnityEngine.Random.Range(40f, 60f);
                    personality.Loyalty = UnityEngine.Random.Range(40f, 60f);
                    personality.Adaptability = UnityEngine.Random.Range(50f, 70f);
                    personality.Charisma = UnityEngine.Random.Range(70f, 90f);
                    personality.Cunning = UnityEngine.Random.Range(50f, 70f);
                    personality.Impulsiveness = UnityEngine.Random.Range(50f, 70f);
                    break;
                
                case Archetype.Maverick:
                    personality.Aggression = UnityEngine.Random.Range(60f, 80f);
                    personality.RiskTolerance = UnityEngine.Random.Range(70f, 90f);
                    personality.EthicalFlexibility = UnityEngine.Random.Range(50f, 80f);
                    personality.Loyalty = UnityEngine.Random.Range(20f, 40f);
                    personality.Adaptability = UnityEngine.Random.Range(60f, 80f);
                    personality.Charisma = UnityEngine.Random.Range(60f, 80f);
                    personality.Cunning = UnityEngine.Random.Range(60f, 80f);
                    personality.Impulsiveness = UnityEngine.Random.Range(70f, 90f);
                    break;
                
                case Archetype.Technocrat:
                    personality.Aggression = UnityEngine.Random.Range(30f, 50f);
                    personality.RiskTolerance = UnityEngine.Random.Range(30f, 50f);
                    personality.EthicalFlexibility = UnityEngine.Random.Range(30f, 50f);
                    personality.Loyalty = UnityEngine.Random.Range(50f, 70f);
                    personality.Adaptability = UnityEngine.Random.Range(50f, 70f);
                    personality.Charisma = UnityEngine.Random.Range(30f, 50f);
                    personality.Cunning = UnityEngine.Random.Range(60f, 80f);
                    personality.Impulsiveness = UnityEngine.Random.Range(20f, 40f);
                    break;
                
                case Archetype.Showman:
                    personality.Aggression = UnityEngine.Random.Range(50f, 70f);
                    personality.RiskTolerance = UnityEngine.Random.Range(60f, 80f);
                    personality.EthicalFlexibility = UnityEngine.Random.Range(50f, 70f);
                    personality.Loyalty = UnityEngine.Random.Range(30f, 50f);
                    personality.Adaptability = UnityEngine.Random.Range(60f, 80f);
                    personality.Charisma = UnityEngine.Random.Range(80f, 100f);
                    personality.Cunning = UnityEngine.Random.Range(50f, 70f);
                    personality.Impulsiveness = UnityEngine.Random.Range(60f, 80f);
                    break;
                
                case Archetype.Insider:
                    personality.Aggression = UnityEngine.Random.Range(40f, 60f);
                    personality.RiskTolerance = UnityEngine.Random.Range(30f, 50f);
                    personality.EthicalFlexibility = UnityEngine.Random.Range(40f, 60f);
                    personality.Loyalty = UnityEngine.Random.Range(60f, 80f);
                    personality.Adaptability = UnityEngine.Random.Range(40f, 60f);
                    personality.Charisma = UnityEngine.Random.Range(50f, 70f);
                    personality.Cunning = UnityEngine.Random.Range(60f, 80f);
                    personality.Impulsiveness = UnityEngine.Random.Range(30f, 50f);
                    break;
                
                case Archetype.DynastyHeir:
                    personality.Aggression = UnityEngine.Random.Range(40f, 60f);
                    personality.RiskTolerance = UnityEngine.Random.Range(40f, 60f);
                    personality.EthicalFlexibility = UnityEngine.Random.Range(50f, 70f);
                    personality.Loyalty = UnityEngine.Random.Range(50f, 70f);
                    personality.Adaptability = UnityEngine.Random.Range(50f, 70f);
                    personality.Charisma = UnityEngine.Random.Range(60f, 80f);
                    personality.Cunning = UnityEngine.Random.Range(60f, 80f);
                    personality.Impulsiveness = UnityEngine.Random.Range(40f, 60f);
                    break;
                
                case Archetype.Zealot:
                    personality.Aggression = UnityEngine.Random.Range(70f, 90f);
                    personality.RiskTolerance = UnityEngine.Random.Range(50f, 70f);
                    personality.EthicalFlexibility = UnityEngine.Random.Range(20f, 40f);
                    personality.Loyalty = UnityEngine.Random.Range(80f, 100f);
                    personality.Adaptability = UnityEngine.Random.Range(20f, 40f);
                    personality.Charisma = UnityEngine.Random.Range(60f, 80f);
                    personality.Cunning = UnityEngine.Random.Range(50f, 70f);
                    personality.Impulsiveness = UnityEngine.Random.Range(40f, 60f);
                    break;
                
                case Archetype.Corporate:
                    personality.Aggression = UnityEngine.Random.Range(50f, 70f);
                    personality.RiskTolerance = UnityEngine.Random.Range(40f, 60f);
                    personality.EthicalFlexibility = UnityEngine.Random.Range(60f, 80f);
                    personality.Loyalty = UnityEngine.Random.Range(30f, 50f);
                    personality.Adaptability = UnityEngine.Random.Range(50f, 70f);
                    personality.Charisma = UnityEngine.Random.Range(50f, 70f);
                    personality.Cunning = UnityEngine.Random.Range(70f, 90f);
                    personality.Impulsiveness = UnityEngine.Random.Range(30f, 50f);
                    break;
                
                case Archetype.Revolutionary:
                    personality.Aggression = UnityEngine.Random.Range(70f, 90f);
                    personality.RiskTolerance = UnityEngine.Random.Range(70f, 90f);
                    personality.EthicalFlexibility = UnityEngine.Random.Range(40f, 60f);
                    personality.Loyalty = UnityEngine.Random.Range(30f, 50f);
                    personality.Adaptability = UnityEngine.Random.Range(60f, 80f);
                    personality.Charisma = UnityEngine.Random.Range(70f, 90f);
                    personality.Cunning = UnityEngine.Random.Range(60f, 80f);
                    personality.Impulsiveness = UnityEngine.Random.Range(60f, 80f);
                    break;
                
                case Archetype.Survivor:
                    personality.Aggression = UnityEngine.Random.Range(50f, 70f);
                    personality.RiskTolerance = UnityEngine.Random.Range(50f, 70f);
                    personality.EthicalFlexibility = UnityEngine.Random.Range(60f, 80f);
                    personality.Loyalty = UnityEngine.Random.Range(30f, 50f);
                    personality.Adaptability = UnityEngine.Random.Range(80f, 100f);
                    personality.Charisma = UnityEngine.Random.Range(50f, 70f);
                    personality.Cunning = UnityEngine.Random.Range(70f, 90f);
                    personality.Impulsiveness = UnityEngine.Random.Range(40f, 60f);
                    break;
            }
            
            // Add human foibles - make them flawed and unpredictable
            // These traits are independent of archetype, adding human unpredictability
            
            // Ego - some are arrogant (Napoleon, Caesar)
            personality.Ego = UnityEngine.Random.Range(30f, 90f);
            if (UnityEngine.Random.value < 0.2f) // 20% chance of extreme ego
                personality.Ego = UnityEngine.Random.Range(80f, 100f);
            
            // Paranoia - some are paranoid (Stalin-like)
            personality.Paranoia = UnityEngine.Random.Range(20f, 70f);
            if (UnityEngine.Random.value < 0.15f) // 15% chance of extreme paranoia
                personality.Paranoia = UnityEngine.Random.Range(70f, 100f);
            
            // Hubris - overconfidence that leads to downfall
            personality.Hubris = UnityEngine.Random.Range(40f, 80f);
            if (personality.Ego > 70f) // High ego = high hubris
                personality.Hubris = UnityEngine.Random.Range(70f, 100f);
            
            // Emotional volatility - mood swings (like historical figures under stress)
            personality.EmotionalVolatility = UnityEngine.Random.Range(20f, 70f);
            if (UnityEngine.Random.value < 0.25f) // 25% chance of being volatile
                personality.EmotionalVolatility = UnityEngine.Random.Range(60f, 100f);
            
            // Obsession - single-minded focus (can be good or bad)
            personality.Obsession = UnityEngine.Random.Range(30f, 80f);
            if (opponent.Archetype == Archetype.Zealot || opponent.Archetype == Archetype.Revolutionary)
                personality.Obsession = UnityEngine.Random.Range(70f, 100f);
            
            // Pride - won't back down even when wrong (King George VI, Prince Andrew)
            personality.Pride = UnityEngine.Random.Range(40f, 90f);
            if (personality.Ego > 60f)
                personality.Pride = UnityEngine.Random.Range(70f, 100f);
            
            // Modify based on difficulty
            if (difficulty == AIDifficulty.Hard)
            {
                // Make them better at everything
                personality.Cunning += 10f;
                personality.Adaptability += 10f;
                personality.Aggression += 10f;
            }
            else if (difficulty == AIDifficulty.Easy)
            {
                // Make them less effective
                personality.Cunning -= 10f;
                personality.Adaptability -= 10f;
            }
            
            // Clamp all values to 0-100
            personality.Aggression = Mathf.Clamp(personality.Aggression, 0f, 100f);
            personality.RiskTolerance = Mathf.Clamp(personality.RiskTolerance, 0f, 100f);
            personality.EthicalFlexibility = Mathf.Clamp(personality.EthicalFlexibility, 0f, 100f);
            personality.Loyalty = Mathf.Clamp(personality.Loyalty, 0f, 100f);
            personality.Adaptability = Mathf.Clamp(personality.Adaptability, 0f, 100f);
            personality.Charisma = Mathf.Clamp(personality.Charisma, 0f, 100f);
            personality.Cunning = Mathf.Clamp(personality.Cunning, 0f, 100f);
            personality.Impulsiveness = Mathf.Clamp(personality.Impulsiveness, 0f, 100f);
            
            // Clamp foible traits
            personality.Ego = Mathf.Clamp(personality.Ego, 0f, 100f);
            personality.Paranoia = Mathf.Clamp(personality.Paranoia, 0f, 100f);
            personality.Hubris = Mathf.Clamp(personality.Hubris, 0f, 100f);
            personality.EmotionalVolatility = Mathf.Clamp(personality.EmotionalVolatility, 0f, 100f);
            personality.Obsession = Mathf.Clamp(personality.Obsession, 0f, 100f);
            personality.Pride = Mathf.Clamp(personality.Pride, 0f, 100f);
            
            return personality;
        }
        
        private string GenerateBackstory(AIOpponent opponent)
        {
            // Generate narrative backstory based on character and archetype
            
            string[] templates = GetBackstoryTemplates(opponent.Archetype);
            string template = templates[UnityEngine.Random.Range(0, templates.Length)];
            
            // Fill in template with character-specific details
            template = template.Replace("{NAME}", opponent.Name);
            template = template.Replace("{BACKGROUND}", 
                opponent.Character.Background != null ? opponent.Character.Background.Name : "Politician");
            
            if (opponent.Character.PositiveQuirks != null && opponent.Character.PositiveQuirks.Count > 0)
                template = template.Replace("{TRAIT}", opponent.Character.PositiveQuirks[0].Name);
            else
                template = template.Replace("{TRAIT}", "determination");
            
            return template;
        }
        
        private string[] GetBackstoryTemplates(Archetype archetype)
        {
            switch (archetype)
            {
                case Archetype.Idealist:
                    return new[] {
                        "{NAME} grew up believing in the power of good government. As a {BACKGROUND}, " +
                        "they saw firsthand how policy affects real people. Their {TRAIT} made them a " +
                        "natural leader in their community.",
                        
                        "After years as a {BACKGROUND}, {NAME} couldn't stand by while corruption " +
                        "flourished. Despite their {TRAIT}, they entered politics to make a real difference."
                    };
                
                case Archetype.MachineBoss:
                    return new[] {
                        "{NAME} learned early that politics is about power. Starting as a {BACKGROUND}, " +
                        "they built connections methodically. Their {TRAIT} helped them climb, but their " +
                        "willingness to play the game got them to the top.",
                        
                        "The party machinery runs on loyalty, and {NAME} knows how to earn it. As a " +
                        "{BACKGROUND}, they learned everyone has a price. Their {TRAIT} is just camouflage."
                    };
                
                case Archetype.Populist:
                    return new[] {
                        "{NAME} rose from humble beginnings as a {BACKGROUND}. They've never forgotten " +
                        "where they came from, and their {TRAIT} connects them to ordinary people in a way " +
                        "elite politicians never could.",
                        
                        "The establishment fears {NAME}. As a {BACKGROUND}, they understand the struggles " +
                        "of working families. Their {TRAIT} makes them a voice for the voiceless."
                    };
                
                case Archetype.Maverick:
                    return new[] {
                        "{NAME} doesn't play by the rules. As a {BACKGROUND}, they learned that sometimes " +
                        "you have to break the system to fix it. Their {TRAIT} makes them unpredictable—" +
                        "and dangerous to the status quo.",
                        
                        "Conventional wisdom says {NAME} can't win. But as a {BACKGROUND}, they've always " +
                        "defied expectations. Their {TRAIT} is both their greatest strength and weakness."
                    };
                
                default:
                    return new[] {
                        "{NAME} entered politics from a career as a {BACKGROUND}. Their {TRAIT} " +
                        "has served them well in their rapid rise through the ranks."
                    };
            }
        }
        
        private List<string> SelectSignatureMoves(Archetype archetype, int count)
        {
            // Each archetype has favorite tactics
            Dictionary<Archetype, string[]> signatureMovesByArchetype = new()
            {
                {
                    Archetype.Idealist,
                    new[] {
                        "Inspiring Policy Speech",
                        "Grassroots Rally",
                        "Transparency Initiative",
                        "Reform Proposal",
                        "Ethical Challenge"
                    }
                },
                {
                    Archetype.MachineBoss,
                    new[] {
                        "Backroom Deal",
                        "Party Mobilization",
                        "Favor Exchange",
                        "Opposition Buyout",
                        "Controlled Leak"
                    }
                },
                {
                    Archetype.Populist,
                    new[] {
                        "Mass Rally",
                        "Anti-Elite Rhetoric",
                        "Working Class Appeal",
                        "Scapegoat Strategy",
                        "Fear Campaign"
                    }
                },
                {
                    Archetype.Maverick,
                    new[] {
                        "Unpredictable Attack",
                        "Norm-Breaking Move",
                        "Chaos Injection",
                        "Rule Violation",
                        "Shocking Statement"
                    }
                },
                {
                    Archetype.Technocrat,
                    new[] {
                        "Data-Driven Policy",
                        "Expert Panel",
                        "Cost-Benefit Analysis",
                        "Evidence Presentation",
                        "Systematic Reform"
                    }
                },
                {
                    Archetype.Showman,
                    new[] {
                        "Media Spectacle",
                        "Viral Moment",
                        "Entertainment Value",
                        "Celebrity Endorsement",
                        "Photo Op"
                    }
                },
                {
                    Archetype.Corporate,
                    new[] {
                        "Business Endorsement",
                        "Economic Argument",
                        "Industry Partnership",
                        "Job Creation Promise",
                        "Market Solution"
                    }
                },
                {
                    Archetype.Revolutionary,
                    new[] {
                        "System Overhaul",
                        "Radical Proposal",
                        "Movement Building",
                        "Institutional Challenge",
                        "Paradigm Shift"
                    }
                }
            };
            
            if (signatureMovesByArchetype.ContainsKey(archetype))
            {
                var moves = signatureMovesByArchetype[archetype];
                return moves.OrderBy(x => UnityEngine.Random.value).Take(count).ToList();
            }
            
            // Fallback
            return new List<string> { "Standard Campaign", "Policy Proposal", "Public Appearance" };
        }
        
        private Dictionary<string, float> InitializeResources(int playerTier, ElectionEmpire.Character.Character character)
        {
            // AI starts with similar resources to player, modified by character
            var resources = new Dictionary<string, float>
            {
                {"PublicTrust", 50f + (character.Background?.StartingResources?.GetValueOrDefault("publicTrust", 0f) ?? 0f)},
                {"PoliticalCapital", playerTier * 10f},
                {"CampaignFunds", 10000f * playerTier * (1f + (character.Background?.StartingResources?.GetValueOrDefault("funds", 0f) ?? 0f) / 100f)},
                {"MediaInfluence", 30f},
                {"PartyLoyalty", 50f}
            };
            
            return resources;
        }
        
        private AIStrategy DetermineInitialStrategy(PersonalityMatrix personality)
        {
            // Strategy based on personality
            if (personality.Aggression > 70)
                return AIStrategy.Aggressive;
            else if (personality.Impulsiveness > 70)
                return AIStrategy.Chaotic;
            else if (personality.Loyalty > 70)
                return AIStrategy.Cooperative;
            else if (personality.Adaptability > 70)
                return AIStrategy.Opportunistic;
            else if (personality.Cunning > 70)
                return AIStrategy.Strategic;
            else
                return AIStrategy.Defensive;
        }
        
        private Dictionary<string, float> GenerateGoals(AIOpponent opponent)
        {
            var goals = new Dictionary<string, float>();
            
            // All AI want to win
            goals["WinElection"] = 100f;
            
            // Archetype-specific goals
            switch (opponent.Archetype)
            {
                case Archetype.Idealist:
                    goals["ImplementPolicies"] = 80f;
                    goals["MaintainEthics"] = 90f;
                    goals["BuildSupport"] = 70f;
                    break;
                
                case Archetype.MachineBoss:
                    goals["ControlParty"] = 90f;
                    goals["BuildAlliances"] = 80f;
                    goals["AccumulatePower"] = 85f;
                    break;
                
                case Archetype.Populist:
                    goals["MaximizeApproval"] = 90f;
                    goals["AttackElites"] = 80f;
                    goals["BuildMovement"] = 75f;
                    break;
                
                case Archetype.Maverick:
                    goals["DisruptSystem"] = 85f;
                    goals["GainAttention"] = 80f;
                    goals["BreakNorms"] = 75f;
                    break;
                
                case Archetype.Technocrat:
                    goals["ImplementPolicies"] = 90f;
                    goals["DataDriven"] = 85f;
                    goals["ExpertCredibility"] = 80f;
                    break;
                
                default:
                    goals["BuildSupport"] = 70f;
                    goals["AccumulatePower"] = 60f;
                    break;
            }
            
            return goals;
        }
        
        private void InitializeVoterBlocSupport(AIOpponent opponent)
        {
            // Initialize neutral support for all blocs
            foreach (VoterBloc bloc in System.Enum.GetValues(typeof(VoterBloc)))
            {
                opponent.VoterBlocSupport[bloc] = 50f; // Neutral
            }
        }
        
        private void InitializePolicyStances(AIOpponent opponent)
        {
            // Initialize policy stances based on archetype
            float baseStance = 50f; // Neutral
            
            // Adjust based on archetype
            switch (opponent.Archetype)
            {
                case Archetype.Idealist:
                    baseStance = 60f; // Slightly left
                    break;
                case Archetype.Corporate:
                    baseStance = 40f; // Slightly right
                    break;
                case Archetype.Revolutionary:
                    baseStance = 20f; // Far left
                    break;
                case Archetype.Zealot:
                    baseStance = 80f; // Far right
                    break;
            }
            
            foreach (Issue issue in System.Enum.GetValues(typeof(Issue)))
            {
                // Add some variation
                opponent.PolicyStances[issue] = baseStance + UnityEngine.Random.Range(-10f, 10f);
            }
        }
    }
}

