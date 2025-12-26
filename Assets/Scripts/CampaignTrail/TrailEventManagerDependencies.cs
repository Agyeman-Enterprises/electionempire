// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - TRAIL EVENT MANAGER DEPENDENCIES
// Supporting classes and interfaces for TrailEventManager
// ═══════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionEmpire.CampaignTrail
{
    #region Interfaces
    
    /// <summary>
    /// Interface for player statistics needed by trail events
    /// </summary>
    public interface IPlayerStats
    {
        int OfficeTier { get; }
        float CurrentApproval { get; }
        string CandidateName { get; }
    }
    
    #endregion
    
    #region Result Classes
    
    /// <summary>
    /// Result from resolving an encounter
    /// </summary>
    [Serializable]
    public class EncounterResolutionResult
    {
        public EncounterOutcome Outcome;
        public string OutcomeDescription;
        public string HeadlineGenerated;
        public MediaImpactLevel MediaImpact;
        
        // Resource impacts
        public float TrustChange;
        public float MediaImpactChange;
        public float PartyLoyaltyChange;
        public Dictionary<string, float> BlocChanges;
        
        // Special flags
        public bool SecretExposed;
        public SecretType? ExposedSecretType;
        public bool ProjectileThrown;
        public ProjectileType? ProjectileType;
        public bool ViralMoment;
        
        public EncounterResolutionResult()
        {
            BlocChanges = new Dictionary<string, float>();
        }
    }
    
    #endregion
    
    #region Generator Classes
    
    /// <summary>
    /// Generates citizens for trail events
    /// </summary>
    public class CitizenGenerator
    {
        /// <summary>
        /// Generate a citizen for a trail event
        /// </summary>
        public static Citizen Generate(TrailEventType eventType, float hostilityLevel)
        {
            var generator = new CitizenGenerator();
            var citizen = generator.GenerateCitizen();
            
            // Adjust based on event type and hostility
            if (hostilityLevel > 0.5f)
            {
                citizen.Disposition = UnityEngine.Random.value < 0.7f 
                    ? CitizenDisposition.HostileOpponent 
                    : CitizenDisposition.Heckler;
                citizen.IsAngry = true;
            }
            else if (hostilityLevel < 0.3f)
            {
                citizen.Disposition = UnityEngine.Random.value < 0.6f 
                    ? CitizenDisposition.Supporter 
                    : CitizenDisposition.TrueBeliever;
            }
            
            return citizen;
        }
        
        public Citizen GenerateCitizen()
        {
            // Generate a basic citizen
            var citizen = new Citizen
            {
                Name = GenerateName(),
                Age = UnityEngine.Random.Range(18, 85),
                Gender = UnityEngine.Random.value < 0.5f ? "Male" : "Female",
                Occupation = GenerateOccupation(),
                Appearance = GenerateAppearance(),
                VoterBloc = GenerateVoterBloc(),
                Disposition = (CitizenDisposition)UnityEngine.Random.Range(0, Enum.GetValues(typeof(CitizenDisposition)).Length),
                Enthusiasm = UnityEngine.Random.Range(0f, 100f),
                Volatility = UnityEngine.Random.Range(0f, 100f),
                Articulateness = UnityEngine.Random.Range(0f, 100f),
                IsIntoxicated = UnityEngine.Random.value < 0.1f,
                IsAngry = UnityEngine.Random.value < 0.2f,
                IsArmed = UnityEngine.Random.value < 0.05f,
                HasCamera = UnityEngine.Random.value < 0.3f,
                HasSecret = UnityEngine.Random.value < 0.15f,
                KnowsCandidate = UnityEngine.Random.value < 0.1f,
                TrustInCandidate = UnityEngine.Random.Range(-50f, 50f),
                LikelyAction = (CitizenAction)UnityEngine.Random.Range(0, Enum.GetValues(typeof(CitizenAction)).Length)
            };
            
            if (citizen.HasSecret)
            {
                citizen.SecretKnown = (SecretType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(SecretType)).Length);
                citizen.SecretDetails = $"Evidence of {citizen.SecretKnown}";
            }
            
            return citizen;
        }
        
        private string GenerateName()
        {
            var firstNames = new[] { "John", "Jane", "Mike", "Sarah", "David", "Emily", "Chris", "Lisa" };
            var lastNames = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis" };
            return $"{firstNames[UnityEngine.Random.Range(0, firstNames.Length)]} {lastNames[UnityEngine.Random.Range(0, lastNames.Length)]}";
        }
        
        private string GenerateOccupation()
        {
            var occupations = new[] { "Teacher", "Factory Worker", "Small Business Owner", "Retired", "Student", "Farmer", "Nurse", "Mechanic" };
            return occupations[UnityEngine.Random.Range(0, occupations.Length)];
        }
        
        private string GenerateAppearance()
        {
            var appearances = new[] { "Average build", "Tall and thin", "Short and stocky", "Well-dressed", "Casual attire", "Work clothes" };
            return appearances[UnityEngine.Random.Range(0, appearances.Length)];
        }
        
        private string GenerateVoterBloc()
        {
            var blocs = new[] { "WorkingClass", "Business", "Seniors", "Youth", "Rural", "Urban" };
            return blocs[UnityEngine.Random.Range(0, blocs.Length)];
        }
    }
    
    /// <summary>
    /// Generates encounters for trail events
    /// </summary>
    public class EncounterGenerator
    {
        public TownsfolkEncounter GenerateEncounter(
            TrailEventType eventType, 
            CrowdHostility hostility, 
            bool pressPresent, 
            bool forceHostile)
        {
            // Generate a citizen first
            var citizen = ColorfulCitizenGenerator.GenerateRandomCitizen();
            
            // Adjust citizen based on hostility
            if (forceHostile)
            {
                citizen.Disposition = UnityEngine.Random.value < 0.5f 
                    ? CitizenDisposition.Hostile 
                    : CitizenDisposition.Cynical;
                citizen.IsAngry = true;
            }
            
            // Create encounter from citizen
            var encounter = new TownsfolkEncounter
            {
                Name = citizen.Name,
                Age = citizen.Age,
                Occupation = citizen.Occupation,
                Appearance = citizen.Appearance,
                OpeningDialogue = GenerateOpeningDialogue(citizen, eventType),
                Context = GenerateContext(citizen, eventType),
                IsHostile = citizen.Disposition >= CitizenDisposition.Hostile,
                HasAudience = UnityEngine.Random.value < 0.6f,
                AudienceSize = UnityEngine.Random.Range(5, 50),
                IsBeingRecorded = pressPresent && UnityEngine.Random.value < 0.7f,
                PressPresent = pressPresent,
                LiveBroadcast = pressPresent && UnityEngine.Random.value < 0.2f,
                HasSecret = citizen.HasSecret,
                SecretType = citizen.SecretKnown,
                SecretDetails = citizen.SecretDetails,
                Choices = GenerateChoices(citizen, eventType)
            };
            
            return encounter;
        }
        
        public TownsfolkEncounter GenerateProjectileEncounter()
        {
            var citizen = ColorfulCitizenGenerator.GenerateRandomCitizen();
            citizen.Disposition = CitizenDisposition.Hostile;
            citizen.IsAngry = true;
            citizen.IsArmed = true;
            
            var encounter = GenerateEncounter(TrailEventType.Walkabout, CrowdHostility.Hostile, true, true);
            encounter.OpeningDialogue = GenerateProjectileDialogue(citizen);
            encounter.EncounterType = "projectile_attack";
            
            return encounter;
        }
        
        public TownsfolkEncounter GenerateChildEncounter()
        {
            var child = new Citizen
            {
                Name = new[] { "Emma", "Liam", "Sophia", "Noah", "Olivia", "Ethan" }[UnityEngine.Random.Range(0, 6)],
                Age = UnityEngine.Random.Range(6, 12),
                Occupation = "Student",
                Disposition = CitizenDisposition.Undecided,
                Enthusiasm = 90f,
                Articulateness = UnityEngine.Random.Range(60f, 100f) // Kids can be surprisingly articulate
            };
            
            var encounter = new TownsfolkEncounter
            {
                Name = child.Name,
                Age = child.Age,
                Occupation = child.Occupation,
                OpeningDialogue = GenerateChildQuestion(),
                Context = "A child approaches with an innocent but potentially devastating question.",
                IsHostile = false,
                HasAudience = true,
                AudienceSize = UnityEngine.Random.Range(20, 100),
                IsBeingRecorded = true, // Always recorded - too cute not to film
                PressPresent = true,
                Choices = GenerateChildResponseChoices()
            };
            
            return encounter;
        }
        
        public TownsfolkEncounter GenerateSecretWitnessEncounter()
        {
            var witness = ColorfulCitizenGenerator.GenerateRandomCitizen();
            witness.Disposition = CitizenDisposition.SecretsToTell;
            witness.HasSecret = true;
            witness.SecretKnown = (SecretType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(SecretType)).Length);
            witness.SecretDetails = GenerateSecretDetails(witness.SecretKnown.Value);
            
            var encounter = GenerateEncounter(TrailEventType.Walkabout, CrowdHostility.Mixed, true, false);
            encounter.Name = witness.Name;
            encounter.OpeningDialogue = GenerateSecretRevelationDialogue(witness);
            encounter.EncounterType = "secret_witness";
            encounter.HasSecret = true;
            encounter.SecretType = witness.SecretKnown;
            encounter.SecretDetails = witness.SecretDetails;
            encounter.HasPhotoEvidence = UnityEngine.Random.value < 0.5f;
            encounter.HasDocumentEvidence = UnityEngine.Random.value < 0.3f;
            encounter.HasWitnessCorroboration = UnityEngine.Random.value < 0.4f;
            
            return encounter;
        }
        
        private string GenerateOpeningDialogue(Citizen citizen, TrailEventType eventType)
        {
            // Use existing generator if available, otherwise generate simple dialogue
            return $"{citizen.Name} approaches you...";
        }
        
        private string GenerateContext(Citizen citizen, TrailEventType eventType)
        {
            return $"At a {eventType} event, {citizen.Name} wants to speak with you.";
        }
        
        private List<EncounterChoice> GenerateChoices(Citizen citizen, TrailEventType eventType)
        {
            var choices = new List<EncounterChoice>();
            
            // Generate basic choices based on disposition
            choices.Add(new EncounterChoice
            {
                Text = "Listen to what they have to say",
                TrustImpact = citizen.Disposition == CitizenDisposition.TrueBeliever ? 2f : 0f,
                MediaImpact = 0f,
                SuccessChance = 0.7f
            });
            
            choices.Add(new EncounterChoice
            {
                Text = "Try to move on quickly",
                TrustImpact = -1f,
                MediaImpact = -5f,
                SuccessChance = 0.5f
            });
            
            if (citizen.HasSecret)
            {
                choices.Add(new EncounterChoice
                {
                    Text = "Try to de-escalate privately",
                    TrustImpact = -2f,
                    MediaImpact = -10f,
                    RiskLevel = 30,
                    SuccessChance = 0.4f
                });
            }
            
            return choices;
        }
        
        private string GenerateProjectileDialogue(Citizen citizen)
        {
            return new[] {
                $"*{citizen.Name} hurls something at you*",
                $"'{citizen.Name}! What are you doing?!'",
                $"Someone in the crowd throws something!"
            }[UnityEngine.Random.Range(0, 3)];
        }
        
        private string GenerateChildQuestion()
        {
            return new[] {
                "Why do people say mean things about you on TV?",
                "My mom says you're going to raise taxes. What does that mean?",
                "Are you going to make my school better?",
                "Why did you vote against the playground funding?",
                "My dad says you're a liar. Are you?",
                "Can you promise you won't break your promises?"
            }[UnityEngine.Random.Range(0, 6)];
        }
        
        private List<EncounterChoice> GenerateChildResponseChoices()
        {
            return new List<EncounterChoice>
            {
                new EncounterChoice
                {
                    Text = "Give an honest, age-appropriate answer",
                    TrustImpact = 5f,
                    MediaImpact = 10f,
                    SuccessChance = 0.8f
                },
                new EncounterChoice
                {
                    Text = "Deflect with a joke",
                    TrustImpact = 2f,
                    MediaImpact = 5f,
                    SuccessChance = 0.6f
                },
                new EncounterChoice
                {
                    Text = "Try to avoid answering",
                    TrustImpact = -5f,
                    MediaImpact = -15f,
                    SuccessChance = 0.3f
                }
            };
        }
        
        private string GenerateSecretDetails(SecretType secretType)
        {
            return $"Evidence of {secretType} has been discovered.";
        }
        
        private string GenerateSecretRevelationDialogue(Citizen witness)
        {
            return $"I know something about you. I've got proof.";
        }
    }
    
    /// <summary>
    /// Resolves encounters based on player choices
    /// </summary>
    public class EncounterResolver
    {
        public EncounterResolutionResult ResolveEncounter(
            TownsfolkEncounter encounter,
            EncounterChoice choice,
            IPlayerStats playerStats,
            string candidateName)
        {
            var result = new EncounterResolutionResult();
            
            // Apply choice impacts
            result.TrustChange = choice.TrustImpact;
            result.MediaImpactChange = choice.MediaImpact;
            result.PartyLoyaltyChange = choice.PartyLoyaltyImpact;
            
            // Determine outcome
            if (UnityEngine.Random.value < choice.SuccessChance)
            {
                // Success
                if (choice.TrustImpact > 3f)
                    result.Outcome = EncounterOutcome.Positive;
                else if (choice.TrustImpact > 0f)
                    result.Outcome = EncounterOutcome.Neutral;
                else
                    result.Outcome = EncounterOutcome.Negative;
            }
            else
            {
                // Failure
                result.Outcome = choice.TrustImpact < -5f ? EncounterOutcome.Disaster : EncounterOutcome.Negative;
            }
            
            // Check for secret exposure
            if (encounter.HasSecret && UnityEngine.Random.value < 0.4f)
            {
                result.SecretExposed = true;
                result.ExposedSecretType = encounter.SecretType;
                result.Outcome = EncounterOutcome.SecretRevealed;
            }
            
            // Generate headline if significant
            if (Mathf.Abs(result.TrustChange) > 5f || result.SecretExposed)
            {
                result.HeadlineGenerated = GenerateHeadline(encounter, result, candidateName);
                result.MediaImpact = result.TrustChange > 0 ? MediaImpactLevel.RegionalStory : MediaImpactLevel.NationalMention;
            }
            
            // Update encounter
            encounter.Outcome = result.Outcome;
            encounter.OutcomeDescription = result.OutcomeDescription;
            encounter.HeadlineGenerated = result.HeadlineGenerated;
            encounter.MediaImpact = result.MediaImpact;
            
            return result;
        }
        
        private string GenerateHeadline(TownsfolkEncounter encounter, EncounterResolutionResult result, string candidateName)
        {
            if (result.SecretExposed)
            {
                return $"{candidateName} Confronted About {result.ExposedSecretType} Allegations";
            }
            
            if (result.Outcome == EncounterOutcome.Disaster)
            {
                return $"{candidateName} Faces Backlash at Campaign Event";
            }
            
            if (result.TrustChange > 5f)
            {
                return $"{candidateName} Connects with Voters at {encounter.Occupation}";
            }
            
            return $"{candidateName} Campaigns in Local Community";
        }
    }
    
    /// <summary>
    /// Manages the intrepid reporter system
    /// </summary>
    public class IntrepidReporterSystem
    {
        private IntrepidReporter _reporter;
        
        public void GenerateReporter()
        {
            // Use existing ReporterAmbushGenerator if available
            _reporter = new IntrepidReporter
            {
                Name = new[] { "Sarah Chen", "Marcus Johnson", "Emily Rodriguez", "David Kim" }[UnityEngine.Random.Range(0, 4)],
                Outlet = new[] { "The Daily Post", "State News Network", "Investigative Weekly", "Capital Times" }[UnityEngine.Random.Range(0, 4)],
                Catchphrase = "Just one question, if I may...",
                Persistence = 70f,
                Ruthlessness = 50f,
                Credibility = 60f,
                IntelligenceGathering = 50f
            };
        }
        
        public bool ShouldAmbush(TrailEventType eventType, int officeTier, float chaosMultiplier)
        {
            float chance = 0.2f + (officeTier * 0.1f) * chaosMultiplier;
            return UnityEngine.Random.value < chance;
        }
        
        public ReporterAmbush GenerateAmbush(TrailEventType eventType, int officeTier)
        {
            if (_reporter == null)
                GenerateReporter();
            
            var ambush = new ReporterAmbush
            {
                Reporter = _reporter,
                Location = GetLocationName(eventType),
                Context = "The reporter has been tracking you and caught you off-guard.",
                OpeningLine = _reporter.Catchphrase,
                CameraRolling = true,
                LiveBroadcast = officeTier >= 4 && UnityEngine.Random.value < 0.3f,
                OtherReportersAttracted = UnityEngine.Random.Range(0, 3),
                CrowdWatching = UnityEngine.Random.Range(10, 50),
                Questions = GenerateQuestions(),
                ResponseOptions = GenerateResponseOptions()
            };
            
            return ambush;
        }
        
        public void UpdateRelationship(EncounterResolutionResult result, EncounterChoice choice)
        {
            if (_reporter == null) return;
            
            if (result.Outcome == EncounterOutcome.Positive)
                _reporter.RelationshipScore += 5f;
            else if (result.Outcome == EncounterOutcome.Negative)
                _reporter.RelationshipScore -= 5f;
            
            _reporter.RelationshipScore = Mathf.Clamp(_reporter.RelationshipScore, -100f, 100f);
        }
        
        public void OnTurnAdvance(int officeTier, float approval, bool secretsRevealed)
        {
            if (_reporter == null) return;
            
            // Progress investigation
            _reporter.StoryProgress += UnityEngine.Random.Range(1f, 5f);
            if (secretsRevealed)
                _reporter.StoryProgress += 10f;
            
            if (_reporter.StoryProgress >= 100f)
                _reporter.ReadyToPublish = true;
        }
        
        public IntrepidReporter GetReporter()
        {
            return _reporter;
        }
        
        public string GetInvestigationWarning()
        {
            if (_reporter == null || _reporter.StoryProgress < 50f)
                return null;
            
            if (_reporter.ReadyToPublish)
                return "WARNING: Reporter is ready to publish damaging story!";
            
            return $"Reporter investigation at {_reporter.StoryProgress:F0}% - be careful!";
        }
        
        private List<string> GenerateQuestions()
        {
            return new List<string>
            {
                "Can you comment on the allegations?",
                "What's your response to these claims?",
                "Is it true that...?"
            };
        }
        
        private List<EncounterChoice> GenerateResponseOptions()
        {
            return new List<EncounterChoice>
            {
                new EncounterChoice { Text = "Answer directly", TrustImpact = 0f, MediaImpact = 5f },
                new EncounterChoice { Text = "No comment", TrustImpact = -2f, MediaImpact = -10f },
                new EncounterChoice { Text = "Attack the reporter", TrustImpact = -5f, MediaImpact = -20f }
            };
        }
        
        private string GetLocationName(TrailEventType eventType)
        {
            return eventType.ToString();
        }
    }
    
    #endregion
}

