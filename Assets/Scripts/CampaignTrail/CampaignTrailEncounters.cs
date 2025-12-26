using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - DETAILED CAMPAIGN TRAIL ENCOUNTERS
// Full dialogue trees, dramatic revelations, physical confrontations
// The messy, unpredictable reality of pressing the flesh
// ═══════════════════════════════════════════════════════════════════════════════

namespace ElectionEmpire.CampaignTrail
{
    #region Detailed Encounter Scenarios
    
    /// <summary>
    /// A fully detailed encounter scenario with dialogue and choices
    /// </summary>
    [Serializable]
    public class DetailedEncounter
    {
        public Citizen Citizen;
        public string OpeningLine;
        public List<EncounterChoice> Choices;
        public EncounterOutcome Outcome;
        public string ResolutionText;
        public bool IsSecretRevelation;
        public bool IsPhysicalConfrontation;
        public float DramaLevel; // 0-100
        public string ViralPotential;
        
        public DetailedEncounter()
        {
            Choices = new List<EncounterChoice>();
        }
    }
    
    
    /// <summary>
    /// Generates detailed encounter scenarios with full dialogue
    /// </summary>
    public static class DetailedEncounterGenerator
    {
        /// <summary>
        /// Generate a full encounter scenario based on citizen
        /// </summary>
        public static DetailedEncounter GenerateEncounter(Citizen citizen, TrailEventType eventType)
        {
            var encounter = new DetailedEncounter
            {
                Citizen = citizen,
                DramaLevel = CalculateDramaLevel(citizen),
                IsSecretRevelation = citizen.HasSecretAboutCandidate,
                IsPhysicalConfrontation = citizen.HasProjectiles || citizen.Disposition >= CitizenDisposition.Heckler
            };
            
            // Generate opening line based on disposition
            encounter.OpeningLine = GenerateOpeningLine(citizen, eventType);
            
            // Generate choices based on citizen type
            encounter.Choices = GenerateChoices(citizen, encounter);
            
            // Generate viral potential
            encounter.ViralPotential = GenerateViralPotential(citizen);
            
            return encounter;
        }
        
        private static string GenerateOpeningLine(Citizen citizen, TrailEventType eventType)
        {
            if (citizen.IsDrunk)
            {
                return new[] {
                    $"*slurred* Hey! Hey you! You're that... that politician guy!",
                    "*hiccup* I got somethin' to say to you!",
                    "*swaying* My ex-wife... she voted for you... and she's a TERRIBLE person!",
                    "*unsteady* You know what your problem is? Lemme tell you...",
                    "*drunk* Buy me a drink and you got my vote! Or... wait, which one are you again?"
                }[UnityEngine.Random.Range(0, 5)];
            }
            
            if (citizen.HasSecretAboutCandidate)
            {
                // Convert enum to string for switch expression
                string secretTypeStr = citizen.SecretType?.ToString() ?? "unknown";
                return secretTypeStr switch
                {
                    "EmbarrassingPhoto" => 
                        $"*steps forward, phone out* Excuse me, but I seen you at The Golden Pole on Route 9 back in {UnityEngine.Random.Range(2015, 2023)}. You was in the VIP section with... well, I got pictures.",
                    "AffairEvidence" =>
                        $"I worked at the Holiday Inn on Miller Road. Room 204. November 12th, 2019. I KNOW what I saw, and I got the security footage.",
                    "SubstanceAbuse" =>
                        $"My cousin was at that party at State U. Everyone knows what you were doing in the back room. I got three people who'll testify.",
                    "BriberyEvidence" =>
                        $"I was the waiter at that dinner at The Capital Club. I saw the envelope change hands. $25,000 in cash. I got the receipt.",
                    "DiscriminationHistory" =>
                        $"My buddy recorded you at that private fundraiser. The one where you thought the mics were off. We got the audio. You want to hear it?",
                    "PastArrest" =>
                        $"You hit my mailbox in {UnityEngine.Random.Range(2010, 2020)} and drove off! I got your plate number, I got the damage, and I got witnesses!",
                    "FailedBusinessVenture" =>
                        $"Your company stiffed my contracting business for $47,000. I got the invoices, I got the emails, and I got a lawyer. You remember me?",
                    "EmployeeAbuse" =>
                        $"You know what you did at that Christmas party in 2018. And so do I. And so do five other people who were there.",
                    "BrokenPromiseProof" =>
                        $"You promised to help save my brother's job at the plant. You looked me in the eye and shook my hand. Then nothing. He lost everything.",
                    "HiddenFamily" =>
                        $"Ask your candidate about the summer of '09. There's a kid in Ohio who looks JUST like them. I got the DNA test results.",
                    _ => $"I know what you did. You know what I'm talking about. And I got proof."
                };
            }
            
            if (citizen.Disposition == CitizenDisposition.Supporter)
            {
                return new[] {
                    $"Oh my God! It's really you! Can I get a picture? My mama loves you!",
                    $"You're the only one who gets it! Finally, someone who'll fight for us!",
                    $"My whole family's voting for you! Screw the other guy!",
                    $"Can I shake your hand? This is amazing!",
                    $"You got my vote! You're the real deal!"
                }[UnityEngine.Random.Range(0, 5)];
            }
            
            if (citizen.Disposition == CitizenDisposition.HostileOpponent)
            {
                return new[] {
                    $"YOU PEOPLE ARE ALL THE SAME! WHERE WERE YOU WHEN THEY CLOSED THE PLANT?",
                    $"MY KIDS CAN'T AFFORD MEDICINE BECAUSE OF PEOPLE LIKE YOU!",
                    $"GO BACK WHERE YOU CAME FROM! WE DON'T WANT YOU HERE!",
                    $"YOU'RE A LIAR AND EVERYONE KNOWS IT!",
                    $"I TRUSTED YOU AND YOU SOLD US OUT! YOU'RE A TRAITOR!"
                }[UnityEngine.Random.Range(0, 5)];
            }
            
            if (citizen.Disposition == CitizenDisposition.Heckler)
            {
                return new[] {
                    $"*yelling* HEY! HEY! LOOK AT ME! YOU'RE A FRAUD!",
                    $"*waving sign* [CANDIDATE] = LIAR! EVERYONE LOOK!",
                    $"*chanting* NOT MY PRESIDENT! NOT MY PRESIDENT!",
                    $"*screaming* YOU DON'T REPRESENT US!",
                    $"*megaphone* SHAME! SHAME! SHAME!"
                }[UnityEngine.Random.Range(0, 5)];
            }
            
            if (citizen.HasSign)
            {
                return $"*holding sign* {citizen.SignText}";
            }
            
            // Default based on issue
            string party = UnityEngine.Random.value > 0.5f ? "Democrat" : "Republican";
            return new[] {
                $"I just want someone who'll actually do something about {citizen.PrimaryIssue}.",
                $"What are you gonna do about {citizen.PrimaryIssue}? Everyone makes promises, nobody delivers.",
                $"I ain't decided yet. Convince me you're different.",
                $"My whole family's been {party} for generations, but I dunno anymore...",
                $"Why should I believe you're any different from the rest of them?"
            }[UnityEngine.Random.Range(0, 5)];
        }
        
        private static List<EncounterChoice> GenerateChoices(Citizen citizen, DetailedEncounter encounter)
        {
            var choices = new List<EncounterChoice>();
            
            // SECRET REVELATION ENCOUNTER
            if (encounter.IsSecretRevelation)
            {
                choices.Add(new EncounterChoice
                {
                    Text = "Deny everything. 'I don't know what you're talking about.'",
                    Action = CandidateAction.Ignore,
                    ConsequenceText = "You dismiss them, but they're recording. This could blow up.",
                    TrustChange = -5,
                    MediaImpact = -20,
                    LeadsTo = EncounterOutcome.Negative,
                    FollowUpText = $"\"Oh, you don't remember? Let me refresh your memory...\" *pulls out phone*"
                });
                
                choices.Add(new EncounterChoice
                {
                    Text = "Try to de-escalate. 'Can we talk about this privately?'",
                    Action = CandidateAction.OfferToTalkPrivately,
                    ConsequenceText = "You try to move the conversation away from cameras.",
                    TrustChange = -2,
                    MediaImpact = -10,
                    RequiresResource = true,
                    ResourceCost = 15, // Political capital
                    LeadsTo = EncounterOutcome.Neutral,
                    FollowUpText = "They agree to talk privately, but the damage might already be done."
                });
                
                choices.Add(new EncounterChoice
                {
                    Text = "Confront them directly. 'That's a lie and you know it!'",
                    Action = CandidateAction.ConfrontDirectly,
                    ConsequenceText = "Aggressive response. High risk, high reward.",
                    TrustChange = UnityEngine.Random.value < 0.5f ? 3 : -8,
                    MediaImpact = UnityEngine.Random.value < 0.5f ? 15 : -25,
                    LeadsTo = UnityEngine.Random.value < 0.5f ? EncounterOutcome.ViralMoment : EncounterOutcome.Disaster,
                    FollowUpText = UnityEngine.Random.value < 0.5f 
                        ? "\"I got proof!\" *shows phone to crowd*" 
                        : "The crowd seems to believe you. For now."
                });
                
                choices.Add(new EncounterChoice
                {
                    Text = "Have security intervene. 'This person needs to leave.'",
                    Action = CandidateAction.HaveSecurityIntervene,
                    ConsequenceText = "Security removes them, but it looks bad on camera.",
                    TrustChange = -10,
                    MediaImpact = -30,
                    LeadsTo = EncounterOutcome.Negative,
                    FollowUpText = "*being dragged away* \"I GOT PROOF! YOU CAN'T SILENCE ME!\""
                });
            }
            
            // PHYSICAL CONFRONTATION (PROJECTILE)
            else if (encounter.IsPhysicalConfrontation && citizen.HasProjectiles)
            {
                choices.Add(new EncounterChoice
                {
                    Text = "Duck! Try to avoid it.",
                    Action = CandidateAction.Duck,
                    ConsequenceText = "You try to dodge the projectile.",
                    TrustChange = 0,
                    MediaImpact = 10, // Dodging looks good
                    LeadsTo = UnityEngine.Random.value < 0.2f ? EncounterOutcome.Negative : EncounterOutcome.Neutral,
                    FollowUpText = UnityEngine.Random.value < 0.2f 
                        ? "*SPLAT* The egg hits you square in the face. The crowd gasps." 
                        : "You duck just in time! The egg sails past. The crowd cheers."
                });
                
                choices.Add(new EncounterChoice
                {
                    Text = "Try to catch it! Show off your reflexes.",
                    Action = CandidateAction.CatchProjectile,
                    ConsequenceText = "High risk, but if you catch it, it's a viral moment.",
                    TrustChange = UnityEngine.Random.value < 0.3f ? 10 : -5,
                    MediaImpact = UnityEngine.Random.value < 0.3f ? 50 : -10,
                    LeadsTo = UnityEngine.Random.value < 0.3f ? EncounterOutcome.ViralMoment : EncounterOutcome.Negative,
                    FollowUpText = UnityEngine.Random.value < 0.3f 
                        ? "*CATCHES IT* The crowd goes WILD! \"Did you see that?!\"" 
                        : "*MISSES* You look foolish trying to catch an egg."
                });
                
                choices.Add(new EncounterChoice
                {
                    Text = "Stand your ground. Don't flinch.",
                    Action = CandidateAction.Acknowledge,
                    ConsequenceText = "You take it like a champ. Or get hit. 50/50.",
                    TrustChange = UnityEngine.Random.value < 0.5f ? 5 : -3,
                    MediaImpact = UnityEngine.Random.value < 0.5f ? 20 : -15,
                    LeadsTo = UnityEngine.Random.value < 0.5f ? EncounterOutcome.Positive : EncounterOutcome.Negative,
                    FollowUpText = UnityEngine.Random.value < 0.5f 
                        ? "*SPLAT* You wipe it off calmly. \"Is that all you got?\" The crowd respects your composure." 
                        : "*SPLAT* You get hit and look weak. The crowd murmurs."
                });
            }
            
            // SUPPORTER ENCOUNTER
            else if (citizen.Disposition == CitizenDisposition.Supporter)
            {
                choices.Add(new EncounterChoice
                {
                    Text = "Shake their hand warmly. 'Thank you for your support!'",
                    Action = CandidateAction.ShakeHand,
                    ConsequenceText = "Positive interaction with supporter.",
                    TrustChange = 2,
                    MediaImpact = 5,
                    LeadsTo = EncounterOutcome.Positive,
                    FollowUpText = "\"This means so much! Can I get a picture?\""
                });
                
                choices.Add(new EncounterChoice
                {
                    Text = "Take a photo with them.",
                    Action = CandidateAction.TakePhoto,
                    ConsequenceText = "Photo op with supporter.",
                    TrustChange = 3,
                    MediaImpact = 8,
                    LeadsTo = EncounterOutcome.Positive,
                    FollowUpText = "*snap* \"I'm posting this everywhere!\""
                });
                
                if (citizen.NumberOfChildren > 0)
                {
                    choices.Add(new EncounterChoice
                    {
                        Text = "Hold their baby for a photo. (Classic move)",
                        Action = CandidateAction.HoldBaby,
                        ConsequenceText = "Baby photo op. Always works.",
                        TrustChange = 5,
                        MediaImpact = 15,
                        LeadsTo = EncounterOutcome.Positive,
                        FollowUpText = "*baby coos* \"Aww! Look how comfortable they are with you!\""
                    });
                }
            }
            
            // HOSTILE OPPONENT
            else if (citizen.Disposition >= CitizenDisposition.HostileOpponent)
            {
                choices.Add(new EncounterChoice
                {
                    Text = "Listen to their concerns. Show empathy.",
                    Action = CandidateAction.ListenIntently,
                    ConsequenceText = "You try to understand their anger.",
                    TrustChange = UnityEngine.Random.value < 0.6f ? 3 : -2,
                    MediaImpact = 10,
                    LeadsTo = UnityEngine.Random.value < 0.6f ? EncounterOutcome.Positive : EncounterOutcome.Neutral,
                    FollowUpText = UnityEngine.Random.value < 0.6f 
                        ? "\"Well... I appreciate you listening. Maybe you're not all bad.\"" 
                        : "\"Empty words! You don't care about us!\""
                });
                
                choices.Add(new EncounterChoice
                {
                    Text = "Confront them. 'I understand you're angry, but let's talk facts.'",
                    Action = CandidateAction.ConfrontDirectly,
                    ConsequenceText = "Direct confrontation. Could go viral or backfire.",
                    TrustChange = UnityEngine.Random.value < 0.4f ? 5 : -8,
                    MediaImpact = UnityEngine.Random.value < 0.4f ? 30 : -20,
                    LeadsTo = UnityEngine.Random.value < 0.4f ? EncounterOutcome.ViralMoment : EncounterOutcome.Disaster,
                    FollowUpText = UnityEngine.Random.value < 0.4f 
                        ? "The crowd respects your directness. Even your opponent seems impressed." 
                        : "You come off as aggressive. The crowd turns against you."
                });
                
                choices.Add(new EncounterChoice
                {
                    Text = "Ignore them and keep walking.",
                    Action = CandidateAction.Ignore,
                    ConsequenceText = "You walk away. Looks weak, but avoids escalation.",
                    TrustChange = -5,
                    MediaImpact = -10,
                    LeadsTo = EncounterOutcome.Negative,
                    FollowUpText = "\"COWARD! YOU CAN'T EVEN FACE US!\" *crowd boos*"
                });
                
                choices.Add(new EncounterChoice
                {
                    Text = "Clap back. 'You know what? You're wrong, and here's why...'",
                    Action = CandidateAction.ClapBack,
                    ConsequenceText = "Aggressive response. High risk.",
                    TrustChange = UnityEngine.Random.value < 0.3f ? 8 : -12,
                    MediaImpact = UnityEngine.Random.value < 0.3f ? 40 : -30,
                    LeadsTo = UnityEngine.Random.value < 0.3f ? EncounterOutcome.ViralMoment : EncounterOutcome.Disaster,
                    FollowUpText = UnityEngine.Random.value < 0.3f 
                        ? "The crowd LOVES it! \"TELL 'EM!\" *cheers*" 
                        : "You went too far. The crowd is uncomfortable. This looks bad."
                });
            }
            
            // UNDECIDED VOTER
            else if (citizen.Disposition == CitizenDisposition.Undecided)
            {
                choices.Add(new EncounterChoice
                {
                    Text = "Listen to their concerns about " + citizen.PrimaryIssue + ".",
                    Action = CandidateAction.ListenIntently,
                    ConsequenceText = "You show you care about their issue.",
                    TrustChange = 4,
                    MediaImpact = 8,
                    LeadsTo = EncounterOutcome.Positive,
                    FollowUpText = "\"Well, at least you're listening. That's more than most.\""
                });
                
                choices.Add(new EncounterChoice
                {
                    Text = "Make a promise about " + citizen.PrimaryIssue + ".",
                    Action = CandidateAction.MakePromise,
                    ConsequenceText = "You make a specific promise. High risk if you break it later.",
                    TrustChange = 6,
                    MediaImpact = 12,
                    RequiresResource = true,
                    ResourceCost = 10,
                    LeadsTo = EncounterOutcome.Positive,
                    FollowUpText = "\"You promise? Because I'm holding you to that.\""
                });
                
                choices.Add(new EncounterChoice
                {
                    Text = "Give them your business card. 'Call my office, let's talk.'",
                    Action = CandidateAction.GiveBusinessCard,
                    ConsequenceText = "Professional approach.",
                    TrustChange = 3,
                    MediaImpact = 5,
                    LeadsTo = EncounterOutcome.Neutral,
                    FollowUpText = "\"I might just do that. Thanks.\""
                });
            }
            
            // DEFAULT CHOICES (fallback)
            if (choices.Count == 0)
            {
                choices.Add(new EncounterChoice
                {
                    Text = "Shake their hand.",
                    Action = CandidateAction.ShakeHand,
                    ConsequenceText = "Standard handshake.",
                    TrustChange = 1,
                    MediaImpact = 2,
                    LeadsTo = EncounterOutcome.Neutral,
                    FollowUpText = "\"Nice to meet you.\""
                });
                
                choices.Add(new EncounterChoice
                {
                    Text = "Acknowledge them and keep moving.",
                    Action = CandidateAction.Acknowledge,
                    ConsequenceText = "Quick acknowledgment.",
                    TrustChange = 0,
                    MediaImpact = 0,
                    LeadsTo = EncounterOutcome.Neutral,
                    FollowUpText = "*nods and moves on*"
                });
            }
            
            return choices;
        }
        
        private static float CalculateDramaLevel(Citizen citizen)
        {
            float drama = 0;
            
            if (citizen.HasSecretAboutCandidate) drama += 50;
            if (citizen.HasProjectiles) drama += 30;
            if (citizen.IsAngry) drama += 20;
            if (citizen.IsRecording) drama += 15;
            if (citizen.HasSign) drama += 10;
            if (citizen.IsDrunk) drama += 15;
            if (citizen.Disposition >= CitizenDisposition.HostileOpponent) drama += 25;
            
            return Mathf.Clamp(drama, 0, 100);
        }
        
        private static string GenerateViralPotential(Citizen citizen)
        {
            if (citizen.HasProjectiles && citizen.IsRecording)
                return "HIGH - Projectile attack being recorded";
            
            if (citizen.HasSecretAboutCandidate && citizen.IsRecording)
                return "VERY HIGH - Secret revelation on camera";
            
            if (citizen.Disposition == CitizenDisposition.Heckler && citizen.IsRecording)
                return "MEDIUM - Heckler confrontation";
            
            if (citizen.IsDrunk && citizen.IsRecording)
                return "MEDIUM - Drunk citizen interaction";
            
            return "LOW - Standard interaction";
        }
    }
    
    #endregion
    
    #region Dramatic Secret Revelations
    
    /// <summary>
    /// A full dramatic secret revelation event
    /// </summary>
    [Serializable]
    public class SecretRevelationEvent
    {
        public Citizen Witness;
        public string SecretType;
        public string FullRevelation;
        public bool HasPhotoEvidence;
        public bool HasAudioEvidence;
        public bool HasWitnesses;
        public int WitnessCount;
        public float Credibility;
        public string MediaHeadline;
        public List<string> CrowdReactions;
        public SecretRevelationOutcome Outcome;
        
        public SecretRevelationEvent()
        {
            CrowdReactions = new List<string>();
        }
    }
    
    public enum SecretRevelationOutcome
    {
        DeniedSuccessfully,      // You talked your way out
        PartialDamage,           // Some people believe it
        MajorDamage,             // Most people believe it
        ViralDisaster,           // It's everywhere, you're done
        TurnedIntoPositive        // You spun it somehow (rare)
    }
    
    /// <summary>
    /// Generates full dramatic secret revelation scenarios
    /// </summary>
    public static class SecretRevelationGenerator
    {
        public static SecretRevelationEvent GenerateRevelation(Citizen witness, TrailEvent currentEvent)
        {
            var revelation = new SecretRevelationEvent
            {
                Witness = witness,
                SecretType = witness.SecretType?.ToString() ?? "unknown",
                HasPhotoEvidence = witness.HasPhotoEvidence,
                Credibility = witness.SecretCredibility,
                HasWitnesses = UnityEngine.Random.value < 0.4f
            };
            
            if (revelation.HasWitnesses)
            {
                revelation.WitnessCount = UnityEngine.Random.Range(1, 5);
            }
            
            // Generate full dramatic revelation text
            revelation.FullRevelation = GenerateFullRevelationText(witness, revelation);
            
            // Generate crowd reactions
            revelation.CrowdReactions = GenerateCrowdReactions(revelation, currentEvent);
            
            // Generate media headline
            revelation.MediaHeadline = GenerateMediaHeadline(witness, revelation);
            
            return revelation;
        }
        
        private static string GenerateFullRevelationText(Citizen witness, SecretRevelationEvent revelation)
        {
            // Convert enum to string for switch expression
            string secretTypeStr = witness.SecretType?.ToString() ?? "unknown";
            return secretTypeStr switch
            {
                "EmbarrassingPhoto" => 
                    $"*steps forward, phone out* \"Excuse me, but I seen you at The Golden Pole on Route 9 back in {UnityEngine.Random.Range(2015, 2023)}. " +
                    $"You was in the VIP section with... well, let's just say it wasn't your wife. " +
                    $"I got pictures. I got receipts. And I got the bouncer who'll testify you were there until 3am.\"",
                    
                "AffairEvidence" =>
                    $"\"I worked at the Holiday Inn on Miller Road. Room 204. November 12th, 2019. " +
                    $"I KNOW what I saw, and I got the security footage. You checked in with someone who wasn't your spouse. " +
                    $"You stayed for four hours. And I got the credit card receipt with YOUR name on it.\"",
                    
                "SubstanceAbuse" =>
                    $"\"My cousin was at that party at State U. The one in the fraternity house. " +
                    $"Everyone knows what you were doing in the back room. I got three people who'll testify. " +
                    $"I got text messages. And I got the dealer who sold it to you. You want their name?\"",
                    
                "BriberyEvidence" =>
                    $"\"I was the waiter at that dinner at The Capital Club. The private room. " +
                    $"I saw the envelope change hands. $25,000 in cash. I counted it when I cleared the table. " +
                    $"I got the receipt. I got the security footage. And I got the other waiter who saw it too.\"",
                    
                "DiscriminationHistory" =>
                    $"\"My buddy recorded you at that private fundraiser. The one where you thought the mics were off. " +
                    $"You said some things. Some REAL things. We got the audio. You want to hear it? " +
                    $"*pulls out phone* Because I'll play it right now for everyone here.\"",
                    
                "PastArrest" =>
                    $"\"You hit my mailbox in {UnityEngine.Random.Range(2010, 2020)} and drove off! " +
                    $"I got your plate number, I got the damage, and I got three neighbors who saw you swerving. " +
                    $"You never reported it. You never paid for it. You just LEFT. " +
                    $"That mailbox was my grandfather's. It meant something to me.\"",
                    
                "FailedBusinessVenture" =>
                    $"\"Your company stiffed my contracting business for $47,000. I got the invoices, I got the emails, " +
                    $"and I got a lawyer. You remember me? Because I remember you. " +
                    $"You looked me in the eye and promised to pay. Then you ghosted me. " +
                    $"I had to lay off three employees because of you. Three families.\"",
                    
                "EmployeeAbuse" =>
                    $"\"You know what you did at that Christmas party in 2018. And so do I. And so do five other people who were there. " +
                    $"You thought no one would believe me. You thought I'd stay quiet. " +
                    $"Well, I'm not staying quiet anymore. And I'm not the only one.\"",
                    
                "BrokenPromiseProof" =>
                    $"\"You promised to help save my brother's job at the plant. You looked me in the eye and shook my hand. " +
                    $"You said 'I'll take care of it.' Then nothing. He lost everything. " +
                    $"His house. His car. His family left him. He killed himself last year. " +
                    $"You want to know what his last words were? 'I trusted them.'\"",
                    
                "HiddenFamily" =>
                    $"\"Ask your candidate about the summer of '09. There's a kid in Ohio who looks JUST like them. " +
                    $"I got the DNA test results. I got the birth certificate. " +
                    $"And I got the mother who's been trying to reach you for fifteen years. " +
                    $"You've been ignoring her. But you can't ignore this.\"",
                    
                _ => $"\"I know what you did. You know what I'm talking about. And I got proof. " +
                     $"I got witnesses. And I'm not going away until everyone knows the truth.\""
            };
        }
        
        private static List<string> GenerateCrowdReactions(SecretRevelationEvent revelation, TrailEvent currentEvent)
        {
            var reactions = new List<string>();
            
            int crowdSize = currentEvent.CitizensPresent.Count(c => !c.HasBeenEngaged);
            
            // Some people are shocked
            reactions.Add($"*gasps from the crowd*");
            reactions.Add($"\"Wait, what?\"");
            reactions.Add($"\"Is this for real?\"");
            
            // Some people believe it immediately
            if (revelation.Credibility > 0.6f)
            {
                reactions.Add($"\"I KNEW IT!\"");
                reactions.Add($"\"This doesn't surprise me at all!\"");
                reactions.Add($"\"Typical politician!\"");
            }
            
            // Some people are skeptical
            if (revelation.Credibility < 0.7f)
            {
                reactions.Add($"\"This sounds like a setup!\"");
                reactions.Add($"\"Where's the proof?\"");
                reactions.Add($"\"Don't believe everything you hear!\"");
            }
            
            // If there's photo evidence
            if (revelation.HasPhotoEvidence)
            {
                reactions.Add($"\"OH MY GOD, LOOK AT THE PICTURE!\"");
                reactions.Add($"*crowd gathers around phone*");
                reactions.Add($"\"That's definitely them!\"");
            }
            
            // If there are multiple witnesses
            if (revelation.HasWitnesses && revelation.WitnessCount > 1)
            {
                reactions.Add($"\"I was there too!\" *another person steps forward*");
                reactions.Add($"\"Me too! I saw it!\"");
                reactions.Add($"\"There's more of us!\"");
            }
            
            // Some people start recording
            reactions.Add($"*dozens of phones come out*");
            reactions.Add($"\"This is going viral!\"");
            reactions.Add($"\"I'm live-streaming this!\"");
            
            return reactions;
        }
        
        private static string GenerateMediaHeadline(Citizen witness, SecretRevelationEvent revelation)
        {
            // Use the string SecretType from the revelation event
            return revelation.SecretType switch
            {
                "strip_club_sighting" => 
                    $"CANDIDATE CAUGHT AT STRIP CLUB: WITNESS PRODUCES PHOTOS",
                "affair_witness" =>
                    $"AFFAIR ALLEGATION ROCKS CAMPAIGN: HOTEL EMPLOYEE CLAIMS TO HAVE EVIDENCE",
                "drug_use_witness" =>
                    $"DRUG USE ALLEGATION SURFACES: MULTIPLE WITNESSES COME FORWARD",
                "bribe_witness" =>
                    $"BRIBE ALLEGATION: WAITER CLAIMS TO HAVE SEEN $25K CASH EXCHANGE",
                "racist_comment_witness" =>
                    $"RACIST COMMENTS CAUGHT ON TAPE: AUDIO LEAKED AT RALLY",
                "drunk_driving_witness" =>
                    $"HIT-AND-RUN ALLEGATION: CANDIDATE ACCUSED OF FLEEING ACCIDENT SCENE",
                "business_fraud_victim" =>
                    $"BUSINESS FRAUD ALLEGATION: CONTRACTOR CLAIMS $47K UNPAID",
                "sexual_harassment_victim" =>
                    $"SEXUAL HARASSMENT ALLEGATION SURFACES AT RALLY",
                "broken_promise_recipient" =>
                    $"BROKEN PROMISE REVEALED: FAMILY MEMBER COMMITS SUICIDE AFTER CANDIDATE FAILS TO DELIVER",
                "illegitimate_child_claim" =>
                    $"ILLEGITIMATE CHILD CLAIM: DNA TEST ALLEGEDLY CONFIRMS PATERNITY",
                _ => $"SHOCKING ALLEGATION AT RALLY: WITNESS MAKES BOMBSHELL CLAIM"
            };
        }
    }
    
    #endregion
    
    #region Physical Confrontation Outcomes
    
    /// <summary>
    /// Detailed outcome of a physical confrontation (projectile throwing, etc.)
    /// </summary>
    [Serializable]
    public class PhysicalConfrontationOutcome
    {
        public ProjectileType ProjectileType;
        public bool Hit;
        public string HitLocation; // "face", "suit", "missed"
        public string CrowdReaction;
        public string MediaReaction;
        public float TrustImpact;
        public float MediaImpact;
        public bool WentViral;
        public string ViralClipDescription;
        public CandidateAction PlayerAction;
        public string OutcomeDescription;
    }
    
    /// <summary>
    /// Generates detailed physical confrontation outcomes
    /// </summary>
    public static class PhysicalConfrontationGenerator
    {
        public static PhysicalConfrontationOutcome GenerateOutcome(
            Citizen attacker, 
            ProjectileType projectileType, 
            CandidateAction playerAction)
        {
            var outcome = new PhysicalConfrontationOutcome
            {
                ProjectileType = projectileType,
                PlayerAction = playerAction
            };
            
            // Determine if it hits based on player action
            outcome.Hit = DetermineHit(playerAction, projectileType);
            
            if (outcome.Hit)
            {
                outcome.HitLocation = DetermineHitLocation();
                outcome.OutcomeDescription = GenerateHitDescription(projectileType, outcome.HitLocation);
            }
            else
            {
                outcome.OutcomeDescription = GenerateMissDescription(playerAction, projectileType);
            }
            
            // Generate crowd reaction
            outcome.CrowdReaction = GenerateCrowdReaction(outcome);
            
            // Generate media reaction
            outcome.MediaReaction = GenerateMediaReaction(outcome);
            
            // Calculate impacts
            outcome.TrustImpact = CalculateTrustImpact(outcome);
            outcome.MediaImpact = CalculateMediaImpact(outcome);
            
            // Did it go viral?
            outcome.WentViral = DetermineViralStatus(outcome);
            if (outcome.WentViral)
            {
                outcome.ViralClipDescription = GenerateViralDescription(outcome);
            }
            
            return outcome;
        }
        
        private static bool DetermineHit(CandidateAction action, ProjectileType projectileType)
        {
            return action switch
            {
                CandidateAction.Duck => UnityEngine.Random.value < 0.2f, // 20% chance to still get hit
                CandidateAction.CatchProjectile => false, // You caught it!
                CandidateAction.StandYourGround => UnityEngine.Random.value < 0.5f, // 50/50
                CandidateAction.GetInCar => false, // You're safe in the car
                _ => UnityEngine.Random.value < 0.6f // 60% default hit chance
            };
        }
        
        private static string DetermineHitLocation()
        {
            var locations = new[] { "face", "chest", "shoulder", "back of head", "suit" };
            return locations[UnityEngine.Random.Range(0, locations.Length)];
        }
        
        private static string GenerateHitDescription(ProjectileType projectileType, string location)
        {
            return projectileType switch
            {
                ProjectileType.Egg => 
                    location == "face" 
                        ? "*SPLAT* The egg explodes on your face. Yolk drips down your suit. The crowd gasps."
                        : $"*SPLAT* The egg hits you in the {location}. It's messy.",
                        
                ProjectileType.Tomato =>
                    location == "face"
                        ? "*SPLAT* The tomato explodes on your face. Red juice everywhere. You're a mess."
                        : $"*SPLAT* The tomato hits your {location}. Your suit is ruined.",
                        
                ProjectileType.Beverage =>
                    location == "face"
                        ? "*SPLASH* The drink soaks your face and suit. You're drenched."
                        : $"*SPLASH* The drink hits your {location}. You're wet and sticky.",
                        
                ProjectileType.Shoe =>
                    location == "face"
                        ? "*THWACK* The shoe hits you square in the face. It hurts. A lot."
                        : $"*THWACK* The shoe hits your {location}. It stings.",
                        
                ProjectileType.Glitter =>
                    location == "face"
                        ? "*POOF* Glitter explodes everywhere. You're covered. It's in your eyes, your hair, everywhere."
                        : $"*POOF* Glitter covers your {location}. You're sparkly now.",
                        
                ProjectileType.Pie =>
                    location == "face"
                        ? "*SPLAT* The pie hits you square in the face. Cream and crust everywhere. Classic."
                        : $"*SPLAT* The pie hits your {location}. It's a mess.",
                        
                _ => $"*IMPACT* Something hits your {location}."
            };
        }
        
        private static string GenerateMissDescription(CandidateAction action, ProjectileType projectileType)
        {
            return action switch
            {
                CandidateAction.Duck =>
                    $"You duck just in time! The {projectileType.ToString().ToLower()} sails over your head. The crowd cheers!",
                    
                CandidateAction.CatchProjectile =>
                    $"*CATCH* You snatch the {projectileType.ToString().ToLower()} out of the air! The crowd goes WILD! \"DID YOU SEE THAT?!\"",
                    
                CandidateAction.GetInCar =>
                    $"You dive into your car just as the {projectileType.ToString().ToLower()} hits the window. Safe, but it looks like you're running away.",
                    
                _ => $"The {projectileType.ToString().ToLower()} misses you by inches. Close call!"
            };
        }
        
        private static string GenerateCrowdReaction(PhysicalConfrontationOutcome outcome)
        {
            if (outcome.PlayerAction == CandidateAction.CatchProjectile && !outcome.Hit)
            {
                return "\"OH MY GOD!\" \"DID YOU SEE THAT?!\" \"THAT WAS AMAZING!\" *crowd erupts in cheers*";
            }
            
            if (outcome.Hit && outcome.HitLocation == "face")
            {
                return "*crowd gasps* \"OH NO!\" \"Are they okay?\" \"That's assault!\" *mixed reactions*";
            }
            
            if (!outcome.Hit && outcome.PlayerAction == CandidateAction.Duck)
            {
                return "\"Nice dodge!\" \"Quick reflexes!\" *applause*";
            }
            
            return "*crowd murmurs* *some people recording* *security moves in*";
        }
        
        private static string GenerateMediaReaction(PhysicalConfrontationOutcome outcome)
        {
            if (outcome.PlayerAction == CandidateAction.CatchProjectile && !outcome.Hit)
            {
                return "MEDIA: \"Incredible reflexes! This is going viral!\" \"Best campaign moment of the year!\"";
            }
            
            if (outcome.Hit && outcome.HitLocation == "face")
            {
                return "MEDIA: \"Assault at campaign rally!\" \"Security concerns raised!\" \"This is a security failure!\"";
            }
            
            return "MEDIA: \"Close call at rally.\" \"Security intervened quickly.\"";
        }
        
        private static float CalculateTrustImpact(PhysicalConfrontationOutcome outcome)
        {
            if (outcome.PlayerAction == CandidateAction.CatchProjectile && !outcome.Hit)
                return 10; // Big win
            
            if (outcome.Hit && outcome.HitLocation == "face")
                return -8; // Looks weak/victimized
            
            if (!outcome.Hit)
                return 3; // Dodged it, looks good
            
            return -3; // Got hit, looks bad
        }
        
        private static float CalculateMediaImpact(PhysicalConfrontationOutcome outcome)
        {
            if (outcome.PlayerAction == CandidateAction.CatchProjectile && !outcome.Hit)
                return 50; // Viral potential
            
            if (outcome.Hit && outcome.HitLocation == "face")
                return -30; // Bad look
            
            if (!outcome.Hit)
                return 15; // Good look
            
            return -10; // Got hit
        }
        
        private static bool DetermineViralStatus(PhysicalConfrontationOutcome outcome)
        {
            if (outcome.PlayerAction == CandidateAction.CatchProjectile && !outcome.Hit)
                return true; // Always viral
            
            if (outcome.Hit && outcome.HitLocation == "face")
                return UnityEngine.Random.value < 0.7f; // 70% chance
            
            return UnityEngine.Random.value < 0.3f; // 30% chance otherwise
        }
        
        private static string GenerateViralDescription(PhysicalConfrontationOutcome outcome)
        {
            if (outcome.PlayerAction == CandidateAction.CatchProjectile && !outcome.Hit)
            {
                return $"\"CANDIDATE CATCHES PROJECTILE MID-AIR\" - 2.3M views, 450K likes. " +
                       $"\"Best campaign moment ever!\" \"This person has reflexes!\" \"I'm voting for them now!\"";
            }
            
            if (outcome.Hit && outcome.HitLocation == "face")
            {
                return $"\"CANDIDATE HIT BY {outcome.ProjectileType.ToString().ToUpper()}\" - 1.8M views. " +
                       $"\"Assault!\" \"Security failure!\" \"Are they okay?\" \"This is unacceptable!\"";
            }
            
            return $"\"CLOSE CALL AT RALLY\" - 500K views. \"That was intense!\" \"Good reflexes!\"";
        }
    }
    
    #endregion
}

