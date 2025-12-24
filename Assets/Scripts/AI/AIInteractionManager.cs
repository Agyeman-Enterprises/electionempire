using System.Collections.Generic;
using UnityEngine;

namespace ElectionEmpire.AI
{
    /// <summary>
    /// Manages AI dialogue and interactions
    /// </summary>
    public class AIInteractionManager
    {
        /// <summary>
        /// Generate AI dialogue based on personality and situation
        /// </summary>
        public string GenerateDialogue(AIOpponent ai, DialogueContext context)
        {
            string[] templates = GetDialogueTemplates(ai.Archetype, context.Type);
            string template = SelectTemplate(templates, ai.Personality);
            
            // Fill in template with context-specific info
            template = template.Replace("{OPPONENT}", context.Target ?? "opponent");
            template = template.Replace("{ISSUE}", context.Issue ?? "the issues");
            template = template.Replace("{POLLING}", context.PollingNumber.ToString("F1"));
            
            // Add personality-specific modifier
            template = AddPersonalityFlavor(template, ai.Personality);
            
            return template;
        }
        
        public enum DialogueType
        {
            DebateOpening,
            DebateAttack,
            DebateDefense,
            DebateClosing,
            PressConference,
            Interview,
            VictorySpeech,
            ConcessionSpeech,
            AllianceProposal,
            AllianceAccept,
            AllianceReject,
            ScandalResponse,
            Taunt,
            Compliment
        }
        
        private string[] GetDialogueTemplates(Archetype archetype, DialogueType type)
        {
            // Each archetype has characteristic speech patterns
            
            switch (archetype)
            {
                case Archetype.Idealist when type == DialogueType.DebateOpening:
                    return new[] {
                        "We stand at a crossroads. The policies I propose aren't just ideas—they're " +
                        "pathways to a better future for every citizen.",
                        
                        "I entered this race because I believe government should serve the people, not " +
                        "special interests. Let me show you how."
                    };
                
                case Archetype.Populist when type == DialogueType.DebateOpening:
                    return new[] {
                        "The elites don't want you to hear this, but I'm going to say it anyway: " +
                        "they've rigged the system against you!",
                        
                        "While my opponents wine and dine with lobbyists, I'm fighting for YOU. " +
                        "It's time to take our government back!"
                    };
                
                case Archetype.MachineBoss when type == DialogueType.DebateOpening:
                    return new[] {
                        "Experience matters. I've been working these halls for years, building " +
                        "coalitions, getting things done. That's what leadership looks like.",
                        
                        "Talk is cheap. I have the relationships and the know-how to actually " +
                        "deliver results. Can my opponents say the same?"
                    };
                
                case Archetype.Maverick when type == DialogueType.DebateOpening:
                    return new[] {
                        "You want the same old politics? Vote for one of these clowns. You want " +
                        "REAL change? I'm your only choice.",
                        
                        "I don't play by the rules because the rules are designed to keep you " +
                        "powerless. Time to shake things up!"
                    };
                
                case Archetype.Idealist when type == DialogueType.DebateAttack:
                    return new[] {
                        "I respect {OPPONENT}, but their record speaks for itself. We need real solutions, not empty promises.",
                        "While {OPPONENT} talks, I act. That's the difference between us."
                    };
                
                case Archetype.Populist when type == DialogueType.DebateAttack:
                    return new[] {
                        "{OPPONENT} is part of the problem! They're in bed with the elites!",
                        "Don't let {OPPONENT} fool you—they don't care about you, they care about power!"
                    };
                
                case Archetype.MachineBoss when type == DialogueType.DebateAttack:
                    return new[] {
                        "{OPPONENT} doesn't understand how things work. I do. That's why I get results.",
                        "I've seen {OPPONENT}'s type before. All talk, no action. I deliver."
                    };
            }
            
            // Fallback templates
            return new[] { 
                "I believe in doing what's right for the people.",
                "My record speaks for itself.",
                "We need real leadership, not empty promises."
            };
        }
        
        private string SelectTemplate(string[] templates, PersonalityMatrix personality)
        {
            if (templates.Length == 0) return "...";
            
            // Impulsive AI might pick random template
            if (personality.Impulsiveness > 70 && UnityEngine.Random.value < 0.3f)
            {
                return templates[UnityEngine.Random.Range(0, templates.Length)];
            }
            
            // Otherwise pick first (can be improved with scoring)
            return templates[0];
        }
        
        private string AddPersonalityFlavor(string template, PersonalityMatrix personality)
        {
            // Add interjections, modifiers based on personality
            
            if (personality.Aggression > 70)
                template = template.Replace(".", "!");
            
            if (personality.Charisma > 70 && UnityEngine.Random.value < 0.5f)
                template = "Friends, " + template;
            
            if (personality.Impulsiveness > 70 && UnityEngine.Random.value < 0.3f)
                template += " And another thing—actually, never mind.";
            
            // HUMAN FOIBLES: Add irrational, off-the-wall comments
            
            // Ego-driven arrogance
            if (personality.Ego > 80f && UnityEngine.Random.value < 0.3f)
            {
                string[] egoComments = new[] {
                    " Not that I need to explain myself to you.",
                    " I've forgotten more about politics than you'll ever know.",
                    " History will remember me, mark my words.",
                    " I'm the best candidate, obviously."
                };
                template += egoComments[UnityEngine.Random.Range(0, egoComments.Length)];
            }
            
            // Paranoia-driven comments
            if (personality.Paranoia > 70f && UnityEngine.Random.value < 0.25f)
            {
                string[] paranoidComments = new[] {
                    " They're all out to get me, you know.",
                    " I know what you're really up to.",
                    " This is all a conspiracy, I'm telling you.",
                    " You think I don't see what's happening here?"
                };
                template += paranoidComments[UnityEngine.Random.Range(0, paranoidComments.Length)];
            }
            
            // Hubris - overconfident statements
            if (personality.Hubris > 75f && UnityEngine.Random.value < 0.3f)
            {
                string[] hubrisComments = new[] {
                    " Victory is inevitable.",
                    " I can't lose, it's mathematically impossible.",
                    " My opponents don't stand a chance.",
                    " This election? Already won."
                };
                template += hubrisComments[UnityEngine.Random.Range(0, hubrisComments.Length)];
            }
            
            // Emotional volatility - mood swings in speech
            if (personality.EmotionalVolatility > 70f && UnityEngine.Random.value < 0.3f)
            {
                string[] emotionalComments = new[] {
                    " ...actually, I'm feeling great today!",
                    " You know what? I'm done being nice.",
                    " I'm just... so tired of this.",
                    " Wait, did I say that? Let me start over."
                };
                template += emotionalComments[UnityEngine.Random.Range(0, emotionalComments.Length)];
            }
            
            // Pride - won't admit mistakes
            if (personality.Pride > 80f && UnityEngine.Random.value < 0.25f)
            {
                string[] prideComments = new[] {
                    " I stand by everything I've said.",
                    " I don't make mistakes.",
                    " I was right then, I'm right now.",
                    " I have nothing to apologize for."
                };
                template += prideComments[UnityEngine.Random.Range(0, prideComments.Length)];
            }
            
            // Completely random off-the-wall comment (like Prince Andrew)
            if (UnityEngine.Random.value < 0.1f) // 10% chance
            {
                string[] randomComments = new[] {
                    " I once met a penguin who told me... never mind.",
                    " My horoscope said this would be a good day.",
                    " I read somewhere that... actually, forget it.",
                    " You know what they say about politicians and... wait, that's not appropriate.",
                    " I had a dream about this last night.",
                    " My gut is telling me something, and my gut is never wrong.",
                    " I'm not superstitious, but I am a little stitious.",
                    " I once won a bet by doing exactly this.",
                    " My grandmother always said... well, she said a lot of things.",
                    " I'm doing this because... because I can, that's why."
                };
                template += " " + randomComments[UnityEngine.Random.Range(0, randomComments.Length)];
            }
            
            return template;
        }
        
        /// <summary>
        /// Determine if AI responds to player action
        /// </summary>
        public bool WillRespond(AIOpponent ai, PlayerAction action)
        {
            // Personality determines response likelihood
            
            float responseChance = 0.3f; // Base 30%
            
            if (action.Type == "Attack")
            {
                // Always respond to attacks if aggressive
                if (ai.Personality.Aggression > 60)
                    return true;
                
                responseChance += 0.4f;
            }
            
            if (action.Target == ai.ID)
            {
                // Targeted directly
                responseChance += 0.5f;
            }
            
            // Impulsive AI respond more
            responseChance += ai.Personality.Impulsiveness * 0.005f;
            
            return UnityEngine.Random.value < responseChance;
        }
        
        /// <summary>
        /// Generate AI response to player action
        /// </summary>
        public string GenerateResponse(AIOpponent ai, PlayerAction action)
        {
            string response = "";
            
            switch (action.Type)
            {
                case "Attack":
                    if (ai.Personality.Aggression > 60)
                        response = $"You want to go there, {action.Source}? Let's talk about YOUR record!";
                    else if (ai.Personality.EthicalFlexibility < 40)
                        response = $"I won't stoop to mudslinging. Let's discuss the real issues.";
                    else
                        response = $"Interesting move, {action.Source}. But you've left yourself wide open...";
                    break;
                
                case "Alliance":
                    if (ai.Personality.Loyalty > 60)
                        response = $"I appreciate the offer. Let's discuss terms.";
                    else
                        response = $"Alliances are temporary. I'll work with you... for now.";
                    break;
            }
            
            return response;
        }
    }
    
    public class DialogueContext
    {
        public DialogueType Type;
        public string Target;
        public string Issue;
        public float PollingNumber;
    }
    
    public class PlayerAction
    {
        public string Type;
        public string Source;
        public string Target;
    }
}

