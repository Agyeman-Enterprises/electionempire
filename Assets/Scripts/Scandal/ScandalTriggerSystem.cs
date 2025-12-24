using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.World;
using ElectionEmpire.Gameplay;

namespace ElectionEmpire.Scandal
{
    /// <summary>
    /// System for triggering scandals based on conditions
    /// </summary>
    public class ScandalTriggerSystem
    {
        private ScandalTemplateLibrary templateLibrary;
        private PlayerState player;
        private GameState gameState;
        
        // Tracking
        private Dictionary<string, float> templateProbabilities;
        private List<PlayerAction> recentActions;
        private HashSet<string> activeTemplates; // Currently active scandal template IDs
        
        public void Initialize(PlayerState player, GameState gameState)
        {
            this.player = player;
            this.gameState = gameState;
            templateLibrary = new ScandalTemplateLibrary();
            templateLibrary.LoadTemplates();
            
            templateProbabilities = new Dictionary<string, float>();
            recentActions = new List<PlayerAction>();
            activeTemplates = new HashSet<string>();
        }
        
        /// <summary>
        /// Called every turn to evaluate triggers
        /// </summary>
        public void EvaluateTriggers(float deltaTime)
        {
            // Calculate probability for each template
            foreach (var template in templateLibrary.GetAllTemplates())
            {
                float probability = CalculateTriggerProbability(template);
                templateProbabilities[template.ID] = probability;
                
                // Roll for trigger (per day chance)
                float dailyChance = probability * (deltaTime / 86400f);
                if (UnityEngine.Random.value < dailyChance)
                {
                    TriggerScandal(template);
                }
            }
            
            // Clear old actions (only consider last 10 turns)
            if (recentActions.Count > 10)
                recentActions.RemoveRange(0, recentActions.Count - 10);
        }
        
        private float CalculateTriggerProbability(ScandalTemplate template)
        {
            float probability = template.BaseProbability;
            
            // Check each trigger condition
            foreach (var condition in template.TriggerConditions)
            {
                if (EvaluateCondition(condition))
                {
                    probability *= condition.Weight;
                }
            }
            
            // Modify by office scrutiny
            if (player.CurrentOffice != null)
            {
                float scrutiny = player.CurrentOffice.Tier * 0.5f; // Higher office = more scrutiny
                probability *= (1f + scrutiny);
            }
            
            // Modify by previous scandals (scandal fatigue reduces new scandals)
            int activeScandalCount = player.ActiveScandals != null ? player.ActiveScandals.Count : 0;
            probability *= Mathf.Max(0.3f, 1f - (activeScandalCount * 0.15f));
            
            // Modify by media investigation
            if (player.UnderMediaInvestigation)
                probability *= 2.0f;
            
            // Modify by difficulty (simplified - would use gameState.Difficulty)
            // probability *= gameState.DifficultyModifier;
            
            // Modify by alignment (certain alignments more scandal-prone)
            if (player.Alignment == "ChaoticEvil")
                probability *= 1.5f;
            else if (player.Alignment == "LawfulGood")
                probability *= 0.7f;
            
            return Mathf.Clamp01(probability);
        }
        
        private bool EvaluateCondition(TriggerCondition condition)
        {
            switch (condition.Type)
            {
                case "Action":
                    return HasRecentAction(condition.Condition);
                
                case "Resource":
                    return EvaluateResourceCondition(condition.Condition);
                
                case "Background":
                    return player.Character.Background != null && 
                           player.Character.Background.Name.Contains(condition.Condition);
                
                case "Office":
                    return player.CurrentOffice != null && 
                           player.CurrentOffice.Name.Contains(condition.Condition);
                
                case "Trait":
                    return (player.Character.PositiveQuirks != null && 
                            player.Character.PositiveQuirks.Any(q => q.Name == condition.Condition)) ||
                           (player.Character.NegativeQuirks != null && 
                            player.Character.NegativeQuirks.Any(q => q.Name == condition.Condition));
                
                case "History":
                    return player.Character.PersonalHistory != null && 
                           player.Character.PersonalHistory.Any(h => h.Contains(condition.Condition));
                
                case "Evolution":
                    // Check if another scandal is active that could evolve
                    string templateID = condition.Condition.Replace("From", "");
                    return HasActiveScandal(templateID);
                
                case "Attribute":
                    return EvaluateAttributeCondition(condition.Condition);
            }
            
            return false;
        }
        
        private bool HasRecentAction(string actionType)
        {
            return recentActions.Any(a => a.Type == actionType);
        }
        
        private bool EvaluateResourceCondition(string condition)
        {
            // Parse condition like "CampaignFunds > 100000"
            var parts = condition.Split(' ');
            if (parts.Length < 3) return false;
            
            string resource = parts[0];
            string op = parts[1];
            if (!float.TryParse(parts[2], out float value))
                return false;
            
            float currentValue = player.Resources.GetValueOrDefault(resource, 0f);
            
            switch (op)
            {
                case ">": return currentValue > value;
                case "<": return currentValue < value;
                case ">=": return currentValue >= value;
                case "<=": return currentValue <= value;
                case "==": return Mathf.Approximately(currentValue, value);
            }
            
            return false;
        }
        
        private bool EvaluateAttributeCondition(string condition)
        {
            // Parse like "Charisma < 50"
            var parts = condition.Split(' ');
            if (parts.Length < 3) return false;
            
            string attribute = parts[0];
            string op = parts[1];
            if (!float.TryParse(parts[2], out float value))
                return false;
            
            float currentValue = GetAttributeValue(attribute);
            
            switch (op)
            {
                case ">": return currentValue > value;
                case "<": return currentValue < value;
            }
            
            return false;
        }
        
        private float GetAttributeValue(string attribute)
        {
            // Simplified - would need actual character stats
            // For now, return random or default
            return 50f;
        }
        
        private bool HasActiveScandal(string templateID)
        {
            return player.ActiveScandals != null && 
                   player.ActiveScandals.Any(s => s.TemplateID == templateID);
        }
        
        private void TriggerScandal(ScandalTemplate template)
        {
            // Don't trigger if already have this type
            if (HasActiveScandal(template.ID))
                return;
            
            // Generate scandal from template
            var scandal = GenerateScandalFromTemplate(template);
            
            // Add to player's active scandals
            if (player.ActiveScandals == null)
                player.ActiveScandals = new List<Scandal.Scandal>();
            
            player.ActiveScandals.Add(scandal);
            activeTemplates.Add(template.ID);
            
            Debug.Log($"Scandal triggered: {scandal.Title} (Severity: {scandal.BaseSeverity})");
        }
        
        private Scandal.Scandal GenerateScandalFromTemplate(ScandalTemplate template)
        {
            var scandal = new Scandal.Scandal
            {
                ID = System.Guid.NewGuid().ToString(),
                TemplateID = template.ID,
                Category = template.Category,
                CurrentStage = ScandalStage.Emergence,
                DiscoveryDate = gameState != null ? gameState.CurrentGameTime : System.DateTime.Now,
                TurnsInStage = 0
            };
            
            // Determine severity
            scandal.BaseSeverity = UnityEngine.Random.Range(template.SeverityMin, template.SeverityMax + 1);
            scandal.CurrentSeverity = scandal.BaseSeverity;
            
            // Generate title
            scandal.Title = GenerateFromTemplate(template.TitleTemplates);
            
            // Generate description
            scandal.Description = GenerateNarrative(template, scandal);
            
            // Generate initial headline
            scandal.Headlines = new List<string>
            {
                GenerateFromTemplate(template.HeadlineTemplates)
            };
            
            // Set initial values
            scandal.EvidenceStrength = UnityEngine.Random.Range(20f, 60f);
            scandal.MediaCoverage = UnityEngine.Random.Range(10f, 40f);
            scandal.PublicInterest = scandal.BaseSeverity * 10f;
            scandal.MediaIntensity = scandal.BaseSeverity;
            
            // Generate evidence items
            scandal.Evidence = GenerateEvidence(scandal);
            
            // Set evolution possibilities
            scandal.CanEvolve = template.CanEvolve;
            if (template.CanEvolve && template.TerminalEvolutions != null)
            {
                scandal.PossibleEvolutions = template.TerminalEvolutions.ToList();
            }
            
            // Initialize impact tracking
            scandal.ResourceImpacts = new Dictionary<string, float>();
            scandal.BlocImpacts = new Dictionary<VoterBloc, float>();
            
            return scandal;
        }
        
        private string GenerateFromTemplate(string[] templates)
        {
            if (templates == null || templates.Length == 0)
                return "Scandal";
            
            string template = templates[UnityEngine.Random.Range(0, templates.Length)];
            
            // Replace placeholders
            template = template.Replace("{PLAYER}", player.Character.Name);
            template = template.Replace("{YEAR}", (System.DateTime.Now.Year - UnityEngine.Random.Range(1, 5)).ToString());
            template = template.Replace("{AMOUNT}", "$" + (UnityEngine.Random.Range(10, 500) * 1000).ToString("N0"));
            
            return template;
        }
        
        private string GenerateNarrative(ScandalTemplate template, Scandal.Scandal scandal)
        {
            string narrative = GenerateFromTemplate(template.DescriptionTemplates);
            
            // Add context-specific details
            narrative = narrative.Replace("{EVENT}", GetRandomEvent());
            narrative = narrative.Replace("{POLICY}", GetRandomPolicy());
            narrative = narrative.Replace("{PROBLEM}", GetRandomProblem());
            narrative = narrative.Replace("{STAFF_ROLE}", GetRandomStaffRole());
            narrative = narrative.Replace("{PERSON}", GeneratePersonName());
            narrative = narrative.Replace("{YEARS}", UnityEngine.Random.Range(2, 6).ToString());
            
            return narrative;
        }
        
        private List<EvidenceItem> GenerateEvidence(Scandal.Scandal scandal)
        {
            int evidenceCount = UnityEngine.Random.Range(1, 4);
            var evidence = new List<EvidenceItem>();
            
            for (int i = 0; i < evidenceCount; i++)
            {
                evidence.Add(new EvidenceItem
                {
                    ID = System.Guid.NewGuid().ToString(),
                    Description = GenerateEvidenceDescription(scandal.Category),
                    Strength = UnityEngine.Random.Range(10f, 80f),
                    DiscoveredDate = gameState != null ? gameState.CurrentGameTime : System.DateTime.Now,
                    Source = GetRandomSource(),
                    IsPublic = UnityEngine.Random.value < 0.3f // 30% chance already public
                });
            }
            
            return evidence;
        }
        
        private string GenerateEvidenceDescription(ScandalCategory category)
        {
            switch (category)
            {
                case ScandalCategory.Financial:
                    return new[] {
                        "Bank records showing suspicious transactions",
                        "Tax documents with irregularities",
                        "Email discussing financial arrangements",
                        "Testimony from former accountant"
                    }[UnityEngine.Random.Range(0, 4)];
                
                case ScandalCategory.Personal:
                    return new[] {
                        "Photographs from the incident",
                        "Text messages between parties",
                        "Eyewitness testimony",
                        "Social media posts"
                    }[UnityEngine.Range(0, 4)];
                
                case ScandalCategory.Policy:
                    return new[] {
                        "Impact study showing negative effects",
                        "Expert testimony on policy failures",
                        "Data showing unintended consequences",
                        "Complaints from affected citizens"
                    }[UnityEngine.Random.Range(0, 4)];
                
                default:
                    return "Documentary evidence";
            }
        }
        
        private string GetRandomSource()
        {
            return new[] {
                "Anonymous leak",
                "Investigative journalism",
                "Government investigation",
                "Whistleblower",
                "Public records request",
                "Rival campaign"
            }[UnityEngine.Random.Range(0, 6)];
        }
        
        /// <summary>
        /// Track player actions for trigger evaluation
        /// </summary>
        public void RecordAction(PlayerAction action)
        {
            recentActions.Add(action);
        }
        
        // Helper methods for narrative generation
        private string GetRandomEvent() => new[] { "rally", "town hall", "press conference", "interview" }[UnityEngine.Random.Range(0, 4)];
        private string GetRandomPolicy() => new[] { "tax reform", "healthcare", "education", "infrastructure" }[UnityEngine.Random.Range(0, 4)];
        private string GetRandomProblem() => new[] { "budget shortfalls", "implementation delays", "public backlash", "legal challenges" }[UnityEngine.Random.Range(0, 4)];
        private string GetRandomStaffRole() => new[] { "campaign manager", "advisor", "staffer", "consultant" }[UnityEngine.Random.Range(0, 4)];
        private string GeneratePersonName() => $"{new[] { "Sarah", "John", "Maria", "David" }[UnityEngine.Random.Range(0, 4)]} " +
                                              $"{new[] { "Smith", "Johnson", "Garcia", "Lee" }[UnityEngine.Random.Range(0, 4)]}";
    }
    
    /// <summary>
    /// Represents a player action (for trigger system)
    /// </summary>
    public class PlayerAction
    {
        public string Type;
        public string Description;
        public System.DateTime Timestamp;
        
        public PlayerAction(string type, string description = "")
        {
            Type = type;
            Description = description;
            Timestamp = System.DateTime.Now;
        }
    }
}

