using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - INTRIPID REPORTER AMBUSH SCENARIOS
// The persistent nemesis reporter with full dramatic ambush scenarios
// ═══════════════════════════════════════════════════════════════════════════════

namespace ElectionEmpire.CampaignTrail
{
    #region Detailed Reporter Ambush Scenarios
    
    /// <summary>
    /// A full dramatic reporter ambush with dialogue and choices
    /// </summary>
    [Serializable]
    public class DetailedReporterAmbush
    {
        public IntrepidReporter Reporter;
        public string Location;
        public string Context;
        public string OpeningLine;
        public List<ReporterQuestion> Questions;
        public bool CameraRolling;
        public bool LiveBroadcast;
        public int CrowdWatching;
        public float PressureLevel; // 0-100, how hard they're pushing
        public string InvestigationAngle;
        public List<string> EvidenceTheyHave;
        public bool HasDeepThroatSource;
        public string DeepThroatRevelation;
        
        public DetailedReporterAmbush()
        {
            Questions = new List<ReporterQuestion>();
            EvidenceTheyHave = new List<string>();
        }
    }
    
    /// <summary>
    /// A specific question from the reporter with follow-ups
    /// </summary>
    [Serializable]
    public class ReporterQuestion
    {
        public string QuestionText;
        public string Category; // "financial", "personal", "policy", "character"
        public List<QuestionResponse> PossibleResponses;
        public float DamageIfEvaded;
        public float DamageIfLied;
        public bool HasFollowUp;
        public string FollowUpQuestion;
        
        public ReporterQuestion()
        {
            PossibleResponses = new List<QuestionResponse>();
        }
    }
    
    /// <summary>
    /// A possible response to a reporter question
    /// </summary>
    [Serializable]
    public class QuestionResponse
    {
        public string ResponseText;
        public ReporterResponseType Type;
        public string ConsequenceText;
        public float TrustChange;
        public float MediaImpact;
        public bool RequiresResource;
        public float ResourceCost;
        public string FollowUpText;
        public bool TriggersFollowUp;
    }
    
    public enum ReporterResponseType
    {
        DirectAnswer,           // Honest, direct response
        Deflect,                // Change the subject
        AttackTheReporter,      // Go on the offensive
        NoComment,              // Refuse to answer
        Evade,                  // Try to dodge
        Lie,                    // Give false information (risky!)
        Defer,                  // "I'll get back to you"
        WalkAway                // Just leave
    }
    
    /// <summary>
    /// Generates detailed reporter ambush scenarios
    /// </summary>
    public static class ReporterAmbushGenerator
    {
        /// <summary>
        /// Generate a full dramatic reporter ambush
        /// </summary>
        public static DetailedReporterAmbush GenerateAmbush(
            IntrepidReporter reporter, 
            TrailEvent currentEvent,
            Dictionary<string, object> knownScandals)
        {
            var ambush = new DetailedReporterAmbush
            {
                Reporter = reporter,
                Location = currentEvent.Location,
                CameraRolling = true,
                LiveBroadcast = UnityEngine.Random.value < 0.2f,
                CrowdWatching = currentEvent.CitizensPresent.Count(c => !c.HasBeenEngaged),
                PressureLevel = reporter.Persistence,
                InvestigationAngle = reporter.TopicsInvestigating[UnityEngine.Random.Range(0, reporter.TopicsInvestigating.Count)],
                HasDeepThroatSource = reporter.HasDeepThroatSource
            };
            
            // Generate context
            ambush.Context = GenerateAmbushContext(reporter, currentEvent);
            
            // Generate opening line
            ambush.OpeningLine = GenerateReporterOpening(reporter, ambush.InvestigationAngle);
            
            // Generate evidence they claim to have
            ambush.EvidenceTheyHave = GenerateEvidence(reporter, ambush.InvestigationAngle, knownScandals);
            
            // Generate deep throat revelation if they have a source
            if (ambush.HasDeepThroatSource)
            {
                ambush.DeepThroatRevelation = GenerateDeepThroatRevelation(ambush.InvestigationAngle);
            }
            
            // Generate 3-5 questions
            int questionCount = UnityEngine.Random.Range(3, 6);
            for (int i = 0; i < questionCount; i++)
            {
                var question = GenerateQuestion(reporter, ambush.InvestigationAngle, knownScandals, i);
                ambush.Questions.Add(question);
            }
            
            return ambush;
        }
        
        private static string GenerateAmbushContext(IntrepidReporter reporter, TrailEvent currentEvent)
        {
            var contexts = new[] {
                $"As you exit your vehicle, a figure emerges from behind a news van. You recognize the face from the evening news. This won't be friendly.",
                $"Walking through the crowd, you suddenly find a microphone thrust in your face. \"{reporter.Name}, {reporter.Outlet}.\"",
                $"Between handshakes, someone who is definitely not a voter steps forward. The crowd parts. Cameras turn. You're cornered.",
                $"Your security detail tries to intercept, but they're too slow. The reporter is already in your space, microphone out, camera rolling.",
                $"You see them coming, but there's nowhere to go. The crowd is watching. The cameras are on. This is happening.",
                $"*flash of camera* You're momentarily blinded. When your vision clears, {reporter.Name} is standing there with a predatory smile.",
                $"The crowd suddenly goes quiet. You turn. There they are. \"We need to talk,\" they say. It's not a request."
            };
            
            return contexts[UnityEngine.Random.Range(0, contexts.Length)];
        }
        
        private static string GenerateReporterOpening(IntrepidReporter reporter, string investigationAngle)
        {
            return investigationAngle switch
            {
                "campaign_finance" =>
                    $"\"{reporter.Name}, {reporter.Outlet}. I've been trying to reach your office for weeks about some irregularities in your FEC filings. Since you're here, care to explain this $50,000 payment to [SHELL COMPANY]?\"",
                    
                "personal_history" =>
                    $"\"Candidate! {reporter.Name} here. We have three independent sources saying you were at [LOCATION] on the night of [DATE]. Can you explain this photograph?\"",
                    
                "policy_flip_flops" =>
                    $"\"Remember me? We met at [EVENT]. I have some follow-up questions. You voted for [UNPOPULAR THING] 47 times, then flipped. Why?\"",
                    
                "staff_treatment" =>
                    $"\"Your former campaign manager calls you 'morally bankrupt' in their new book. Why do you think {UnityEngine.Random.Range(3, 8)} of your staff have resigned?\"",
                    
                "donor_relationships" =>
                    $"\"Your top donor got [BENEFIT] right after you pushed [POLICY]. Coincidence? Because the timing is... interesting.\"",
                    
                "family_business" =>
                    $"\"Is it true your spouse's company got a $2.3 million contract while you were in office? And that you voted on the bill that awarded it?\"",
                    
                _ =>
                    $"\"{reporter.Name}, {reporter.Outlet}. Quick question if you have a moment—\" *thrusts microphone* \"Actually, several questions.\""
            };
        }
        
        private static List<string> GenerateEvidence(IntrepidReporter reporter, string angle, Dictionary<string, object> knownScandals)
        {
            var evidence = new List<string>();
            
            return angle switch
            {
                "campaign_finance" => new List<string>
                {
                    "FEC filings showing $50,000 payment to shell company",
                    "Bank records showing wire transfers",
                    "Emails between you and the shell company",
                    "Testimony from former staff member"
                },
                
                "personal_history" => new List<string>
                {
                    "Photograph from the night in question",
                    "Security footage",
                    "Credit card receipts",
                    "Three independent witnesses"
                },
                
                "policy_flip_flops" => new List<string>
                {
                    "Voting record showing 47 votes for the policy",
                    "Public statement reversing position",
                    "Emails showing private support while publicly opposing",
                    "Donor records showing contributions after the flip"
                },
                
                "staff_treatment" => new List<string>
                {
                    "Resignation letters from former staff",
                    "Text messages showing hostile treatment",
                    "Testimony from former employees",
                    "HR complaints that were ignored"
                },
                
                "donor_relationships" => new List<string>
                {
                    "Timeline showing policy vote followed by donor benefit",
                    "Emails between you and the donor",
                    "Meeting records",
                    "Financial records showing the connection"
                },
                
                "family_business" => new List<string>
                {
                    "Contract award documents",
                    "Your voting record on the relevant bill",
                    "Financial disclosure forms",
                    "Emails between you and the contracting company"
                },
                
                _ => new List<string> { "Various documents", "Multiple sources", "Evidence gathered over months" }
            };
        }
        
        private static string GenerateDeepThroatRevelation(string angle)
        {
            return angle switch
            {
                "campaign_finance" =>
                    $"\"We have a source inside your campaign. They've been feeding us information for months. " +
                    $"They say this is just the tip of the iceberg. They say there's more. A lot more.\"",
                    
                "personal_history" =>
                    $"\"Someone who was there that night reached out to us. They're scared. They don't want to come forward publicly. " +
                    $"But they told us everything. And we believe them.\"",
                    
                "policy_flip_flops" =>
                    $"\"A former staff member told us you were planning to flip on this issue months before you did. " +
                    $"They say it was all calculated. That you never actually changed your mind. " +
                    $"You just changed your position when it became politically convenient.\"",
                    
                "staff_treatment" =>
                    $"\"Your former staff are talking. Not just one or two. Multiple people. " +
                    $"They're telling us things. Things that would make voters... uncomfortable.\"",
                    
                "donor_relationships" =>
                    $"\"We have someone on the inside. They've seen the emails. They've seen the meetings. " +
                    $"They say this wasn't a coincidence. They say it was a transaction.\"",
                    
                "family_business" =>
                    $"\"Someone in the contracting office reached out to us. They're concerned. " +
                    $"They say the contract award process was... unusual. They say there was pressure. " +
                    $"Pressure from above.\"",
                    
                _ => $"\"We have sources. Multiple sources. People who know things. " +
                     $"Things you don't want the public to know.\""
            };
        }
        
        private static ReporterQuestion GenerateQuestion(
            IntrepidReporter reporter, 
            string angle, 
            Dictionary<string, object> knownScandals,
            int questionIndex)
        {
            var question = new ReporterQuestion
            {
                Category = angle,
                HasFollowUp = UnityEngine.Random.value < 0.4f
            };
            
            // Generate question text based on angle and index
            question.QuestionText = GenerateQuestionText(reporter, angle, questionIndex);
            
            // Generate possible responses
            question.PossibleResponses = GenerateResponses(question, angle);
            
            // Calculate damage
            question.DamageIfEvaded = UnityEngine.Random.Range(10f, 25f);
            question.DamageIfLied = UnityEngine.Random.Range(30f, 50f);
            
            // Generate follow-up if applicable
            if (question.HasFollowUp)
            {
                question.FollowUpQuestion = GenerateFollowUpQuestion(angle);
            }
            
            return question;
        }
        
        private static string GenerateQuestionText(IntrepidReporter reporter, string angle, int index)
        {
            var questions = angle switch
            {
                "campaign_finance" => new[]
                {
                    "Can you explain this $50,000 payment to [SHELL COMPANY]?",
                    "Why did your net worth triple while in office?",
                    "Is it true your spouse's company got [CONTRACT]?",
                    "These FEC filings show some... irregularities. Care to comment?",
                    "Why did you accept $200,000 from [DONOR] right before voting on [BILL]?"
                },
                
                "personal_history" => new[]
                {
                    "Were you or were you not at [LOCATION] on the night of [DATE]?",
                    "Can you explain this photograph?",
                    "We have three sources saying you [ALLEGATION]. Your response?",
                    "Why did you tell [AUDIENCE A] one thing and [AUDIENCE B] the opposite?",
                    "Is it true you [EMBARRASSING THING] at [EVENT]?"
                },
                
                "policy_flip_flops" => new[]
                {
                    "You voted for [UNPOPULAR THING] 47 times. Do you still stand by that?",
                    "Your donors got [BENEFIT] after you pushed [POLICY]. Coincidence?",
                    "You promised [THING] to get elected. Why haven't you delivered?",
                    "The data shows [THING] got worse under your leadership. Explain.",
                    "You said you'd never support [POLICY]. Then you did. Why?"
                },
                
                "staff_treatment" => new[]
                {
                    "Your former [STAFF/FRIEND/ALLY] calls you 'morally bankrupt.' Response?",
                    "Why do you think [NUMBER] of your staff have resigned?",
                    "Is it true you [EMBARRASSING THING] at [EVENT]?",
                    "Your opponent calls you a [INSULT]. Are they wrong?",
                    "Multiple former staff members have come forward with allegations. Your response?"
                },
                
                _ => new[]
                {
                    "Can you explain [ALLEGATION]?",
                    "Why did you [ACTION]?",
                    "Is it true that [CLAIM]?",
                    "Your opponent says [ACCUSATION]. Response?",
                    "The American people deserve to know: [QUESTION]?"
                }
            };
            
            if (index < questions.Length)
                return questions[index];
            
            return questions[UnityEngine.Random.Range(0, questions.Length)];
        }
        
        private static List<QuestionResponse> GenerateResponses(ReporterQuestion question, string angle)
        {
            var responses = new List<QuestionResponse>();
            
            // Direct Answer
            responses.Add(new QuestionResponse
            {
                ResponseText = "I'll answer that directly. [GIVE HONEST ANSWER]",
                Type = ReporterResponseType.DirectAnswer,
                ConsequenceText = "You give a direct, honest answer. The reporter seems surprised.",
                TrustChange = 5,
                MediaImpact = 10,
                FollowUpText = "\"Well... thank you for being direct. But I have a follow-up...\""
            });
            
            // Deflect
            responses.Add(new QuestionResponse
            {
                ResponseText = "That's an interesting question, but what I think the American people really care about is [CHANGE SUBJECT].",
                Type = ReporterResponseType.Deflect,
                ConsequenceText = "You try to change the subject. The reporter pushes back.",
                TrustChange = -2,
                MediaImpact = -5,
                FollowUpText = "\"That's not what I asked. Can you answer the question?\""
            });
            
            // Attack The Reporter
            responses.Add(new QuestionResponse
            {
                ResponseText = "You know what? I'm tired of these gotcha questions. You're not interested in the truth, you're interested in headlines.",
                Type = ReporterResponseType.AttackTheReporter,
                ConsequenceText = "You go on the offensive. Risky, but could work.",
                TrustChange = UnityEngine.Random.value < 0.5f ? 5 : -8,
                MediaImpact = UnityEngine.Random.value < 0.5f ? 20 : -15,
                FollowUpText = UnityEngine.Random.value < 0.5f 
                    ? "\"I'm just doing my job. But fine, let's move on.\"" 
                    : "\"I'm asking legitimate questions. The American people deserve answers.\""
            });
            
            // No Comment
            responses.Add(new QuestionResponse
            {
                ResponseText = "I have no comment on that at this time.",
                Type = ReporterResponseType.NoComment,
                ConsequenceText = "You refuse to answer. Looks evasive.",
                TrustChange = -5,
                MediaImpact = -15,
                FollowUpText = "\"No comment? Really? The American people deserve better than 'no comment.'\""
            });
            
            // Evade
            responses.Add(new QuestionResponse
            {
                ResponseText = "I think what you're really asking is [REPHRASE TO SAFER QUESTION], and the answer to that is [SAFE ANSWER].",
                Type = ReporterResponseType.Evade,
                ConsequenceText = "You try to dodge. The reporter sees through it.",
                TrustChange = -3,
                MediaImpact = -10,
                FollowUpText = "\"That's not what I asked. Can you answer my actual question?\""
            });
            
            // Lie (VERY RISKY)
            responses.Add(new QuestionResponse
            {
                ResponseText = "That's completely false. [GIVE FALSE INFORMATION]",
                Type = ReporterResponseType.Lie,
                ConsequenceText = "You lie. If they have proof, this will be catastrophic.",
                TrustChange = UnityEngine.Random.value < 0.2f ? -30 : 0, // 20% chance they have proof
                MediaImpact = UnityEngine.Random.value < 0.2f ? -50 : -5,
                RequiresResource = true,
                ResourceCost = 20,
                FollowUpText = UnityEngine.Random.value < 0.2f 
                    ? "\"Actually, we have proof that contradicts that. Care to revise your answer?\" *pulls out evidence*" 
                    : "\"Okay... but we'll be fact-checking that.\""
            });
            
            // Defer
            responses.Add(new QuestionResponse
            {
                ResponseText = "I'll have my team get back to you with a detailed response. This is a complex issue that deserves a thorough answer.",
                Type = ReporterResponseType.Defer,
                ConsequenceText = "You defer. Professional, but they'll keep asking.",
                TrustChange = 0,
                MediaImpact = -5,
                FollowUpText = "\"When? Because we've been waiting for weeks.\""
            });
            
            // Walk Away
            responses.Add(new QuestionResponse
            {
                ResponseText = "*turn and walk away*",
                Type = ReporterResponseType.WalkAway,
                ConsequenceText = "You just leave. Looks terrible, but ends the ambush.",
                TrustChange = -10,
                MediaImpact = -25,
                FollowUpText = "\"REALLY? You're just going to walk away? The American people deserve answers!\" *shouting after you*"
            });
            
            return responses;
        }
        
        private static string GenerateFollowUpQuestion(string angle)
        {
            return angle switch
            {
                "campaign_finance" => "But that doesn't explain the $200,000 from [DONOR]. Can you address that?",
                "personal_history" => "But we have witnesses. Multiple witnesses. How do you explain that?",
                "policy_flip_flops" => "But you said the exact opposite last year. Which is it?",
                "staff_treatment" => "But why did so many people resign? That's not normal.",
                "donor_relationships" => "But the timing is suspicious. Can you see why people are concerned?",
                "family_business" => "But you voted on the bill. Isn't that a conflict of interest?",
                _ => "But that doesn't answer my question. Can you be more specific?"
            };
        }
    }
    
    #endregion
    
    #region Reporter Relationship System
    
    /// <summary>
    /// Tracks the evolving relationship with the intrepid reporter
    /// </summary>
    [Serializable]
    public class ReporterRelationship
    {
        public IntrepidReporter Reporter;
        public float RelationshipScore; // -100 (nemesis) to 100 (friendly)
        public int TimesAmbushed;
        public int TimesEvaded;
        public int TimesAnswered;
        public int TimesLied;
        public List<string> StoriesWritten;
        public bool HasRespectForYou; // Even enemies can respect you
        public bool IsActivelyHostile;
        public string CurrentInvestigation;
        public float InvestigationProgress;
        
        public ReporterRelationship()
        {
            StoriesWritten = new List<string>();
        }
    }
    
    /// <summary>
    /// Manages the relationship with the reporter over time
    /// </summary>
    public static class ReporterRelationshipManager
    {
        public static void UpdateRelationship(ReporterRelationship relationship, ReporterResponseType response)
        {
            relationship.TimesAmbushed++;
            
            switch (response)
            {
                case ReporterResponseType.DirectAnswer:
                    relationship.TimesAnswered++;
                    relationship.RelationshipScore += 5;
                    relationship.HasRespectForYou = true;
                    break;
                    
                case ReporterResponseType.AttackTheReporter:
                    relationship.RelationshipScore -= 10;
                    if (UnityEngine.Random.value < 0.5f)
                        relationship.HasRespectForYou = true; // They respect the fight
                    break;
                    
                case ReporterResponseType.NoComment:
                case ReporterResponseType.Evade:
                    relationship.TimesEvaded++;
                    relationship.RelationshipScore -= 5;
                    break;
                    
                case ReporterResponseType.Lie:
                    relationship.TimesLied++;
                    relationship.RelationshipScore -= 20;
                    relationship.IsActivelyHostile = true;
                    break;
                    
                case ReporterResponseType.WalkAway:
                    relationship.RelationshipScore -= 15;
                    relationship.IsActivelyHostile = true;
                    break;
            }
            
            // Generate story based on response
            var story = GenerateStory(relationship, response);
            relationship.StoriesWritten.Add(story);
        }
        
        private static string GenerateStory(ReporterRelationship relationship, ReporterResponseType response)
        {
            return response switch
            {
                ReporterResponseType.DirectAnswer =>
                    $"\"CANDIDATE GIVES DIRECT ANSWER TO TOUGH QUESTION\" - {relationship.Reporter.Outlet}",
                    
                ReporterResponseType.AttackTheReporter =>
                    $"\"CANDIDATE ATTACKS REPORTER IN HEATED EXCHANGE\" - {relationship.Reporter.Outlet}",
                    
                ReporterResponseType.NoComment =>
                    $"\"CANDIDATE REFUSES TO ANSWER QUESTIONS\" - {relationship.Reporter.Outlet}",
                    
                ReporterResponseType.Lie =>
                    $"\"CANDIDATE'S STATEMENTS CONTRADICT EVIDENCE\" - {relationship.Reporter.Outlet}",
                    
                ReporterResponseType.WalkAway =>
                    $"\"CANDIDATE WALKS AWAY FROM QUESTIONS\" - {relationship.Reporter.Outlet}",
                    
                _ => $"\"CANDIDATE EVADES QUESTIONS AT RALLY\" - {relationship.Reporter.Outlet}"
            };
        }
    }
    
    #endregion
}

