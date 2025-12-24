using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.World;
using ElectionEmpire.Gameplay;

namespace ElectionEmpire.Scandal
{
    /// <summary>
    /// Manages scandal evolution through stages
    /// </summary>
    public class ScandalEvolutionSystem
    {
        private ScandalTemplateLibrary templateLibrary;
        private GameState gameState;
        
        public ScandalEvolutionSystem(ScandalTemplateLibrary library, GameState gameState)
        {
            this.templateLibrary = library;
            this.gameState = gameState;
        }
        
        /// <summary>
        /// Update scandal state each turn
        /// </summary>
        public void UpdateScandal(Scandal scandal, float deltaTime)
        {
            // Increment time in stage
            scandal.TurnsInStage += (int)(deltaTime / 86400f); // Days
            
            // Update based on current stage
            switch (scandal.CurrentStage)
            {
                case ScandalStage.Emergence:
                    UpdateEmergenceStage(scandal);
                    break;
                
                case ScandalStage.Development:
                    UpdateDevelopmentStage(scandal);
                    break;
                
                case ScandalStage.Crisis:
                    UpdateCrisisStage(scandal);
                    break;
                
                case ScandalStage.Resolution:
                    UpdateResolutionStage(scandal);
                    break;
            }
            
            // Check for evolution
            if (scandal.CanEvolve && ShouldEvolve(scandal))
            {
                EvolveScandal(scandal);
            }
        }
        
        private void UpdateEmergenceStage(Scandal scandal)
        {
            // Slowly grow media coverage
            scandal.MediaCoverage += UnityEngine.Random.Range(1f, 5f);
            scandal.PublicInterest += UnityEngine.Random.Range(0.5f, 3f);
            
            // Check for advancement to Development
            if (scandal.TurnsInStage >= 3 || scandal.MediaCoverage > 30f)
            {
                AdvanceStage(scandal, ScandalStage.Development);
            }
        }
        
        private void UpdateDevelopmentStage(Scandal scandal)
        {
            // Evidence grows
            scandal.EvidenceStrength += UnityEngine.Random.Range(2f, 8f);
            scandal.MediaCoverage += UnityEngine.Random.Range(3f, 10f);
            scandal.PublicInterest += UnityEngine.Random.Range(2f, 8f);
            
            // Randomly discover new evidence
            if (UnityEngine.Random.value < 0.3f)
            {
                DiscoverNewEvidence(scandal);
            }
            
            // Media intensity increases
            scandal.MediaIntensity = Mathf.Min(10, scandal.MediaIntensity + 1);
            
            // Generate new developments
            if (UnityEngine.Random.value < 0.4f)
            {
                GenerateDevelopment(scandal);
            }
            
            // Check for advancement to Crisis
            if (scandal.TurnsInStage >= 5 || scandal.EvidenceStrength > 70f || scandal.MediaCoverage > 70f)
            {
                AdvanceStage(scandal, ScandalStage.Crisis);
            }
        }
        
        private void UpdateCrisisStage(Scandal scandal)
        {
            // Peak intensity - highest impact
            scandal.MediaCoverage = Mathf.Min(100f, scandal.MediaCoverage + UnityEngine.Random.Range(5f, 15f));
            scandal.PublicInterest = Mathf.Min(100f, scandal.PublicInterest + UnityEngine.Random.Range(5f, 15f));
            
            // Severity can increase in crisis
            if (UnityEngine.Random.value < 0.2f && scandal.CurrentSeverity < 10)
            {
                scandal.CurrentSeverity++;
                GeneratePlotTwist(scandal);
            }
            
            // Check for advancement to Resolution
            if (scandal.TurnsInStage >= 7 || (scandal.ResponseHistory != null && scandal.ResponseHistory.Count > 2))
            {
                AdvanceStage(scandal, ScandalStage.Resolution);
            }
        }
        
        private void UpdateResolutionStage(Scandal scandal)
        {
            // Media coverage fades
            scandal.MediaCoverage -= UnityEngine.Random.Range(5f, 15f);
            scandal.MediaCoverage = Mathf.Max(0f, scandal.MediaCoverage);
            
            scandal.PublicInterest -= UnityEngine.Random.Range(3f, 10f);
            scandal.PublicInterest = Mathf.Max(0f, scandal.PublicInterest);
            
            // Check if fully resolved
            if (scandal.MediaCoverage < 10f && scandal.PublicInterest < 10f)
            {
                ResolveScandal(scandal, ResolutionType.TimeFaded);
            }
        }
        
        private void AdvanceStage(Scandal scandal, ScandalStage newStage)
        {
            scandal.CurrentStage = newStage;
            scandal.TurnsInStage = 0;
            
            // Generate headline for stage change
            string headline = GenerateStageHeadline(scandal, newStage);
            if (scandal.Headlines == null)
                scandal.Headlines = new List<string>();
            scandal.Headlines.Add(headline);
            
            Debug.Log($"Scandal evolving: {scandal.Title} → {newStage}: {headline}");
        }
        
        private bool ShouldEvolve(Scandal scandal)
        {
            // Can only evolve from Development or Crisis
            if (scandal.CurrentStage != ScandalStage.Development && 
                scandal.CurrentStage != ScandalStage.Crisis)
                return false;
            
            // Check conditions for evolution
            float evolutionChance = 0.1f; // 10% base
            
            // Higher chance if evidence strong
            if (scandal.EvidenceStrength > 80f)
                evolutionChance += 0.2f;
            
            // Higher chance if media coverage intense
            if (scandal.MediaCoverage > 80f)
                evolutionChance += 0.15f;
            
            // Higher chance if poorly managed
            if (scandal.ResponseHistory != null)
            {
                int failedResponses = scandal.ResponseHistory.Count(r => !r.Successful);
                evolutionChance += failedResponses * 0.1f;
            }
            
            // Higher chance in crisis stage
            if (scandal.CurrentStage == ScandalStage.Crisis)
                evolutionChance += 0.2f;
            
            return UnityEngine.Random.value < evolutionChance;
        }
        
        private void EvolveScandal(Scandal scandal)
        {
            if (scandal.PossibleEvolutions == null || scandal.PossibleEvolutions.Count == 0)
                return;
            
            // Select evolution path
            string evolutionTemplateID = scandal.PossibleEvolutions[UnityEngine.Random.Range(0, scandal.PossibleEvolutions.Count)];
            var evolutionTemplate = templateLibrary.GetTemplate(evolutionTemplateID);
            
            if (evolutionTemplate == null)
                return;
            
            // Create evolved scandal
            var evolvedScandal = GenerateEvolvedScandal(scandal, evolutionTemplate);
            
            // Replace old scandal with evolved version
            var player = gameState.Player;
            if (player.ActiveScandals != null)
            {
                int index = player.ActiveScandals.FindIndex(s => s.ID == scandal.ID);
                if (index >= 0)
                {
                    player.ActiveScandals[index] = evolvedScandal;
                }
            }
            
            Debug.Log($"Scandal evolved: {scandal.Title} → {evolvedScandal.Title}");
        }
        
        private Scandal GenerateEvolvedScandal(Scandal original, ScandalTemplate evolutionTemplate)
        {
            var evolved = new Scandal
            {
                ID = System.Guid.NewGuid().ToString(),
                TemplateID = evolutionTemplate.ID,
                Title = GenerateFromTemplate(evolutionTemplate.TitleTemplates),
                Category = evolutionTemplate.Category,
                CurrentStage = ScandalStage.Development, // Restart at Development
                DiscoveryDate = gameState != null ? gameState.CurrentGameTime : System.DateTime.Now,
                TurnsInStage = 0
            };
            
            // Inherit and amplify severity
            evolved.BaseSeverity = Mathf.Min(10, original.CurrentSeverity + UnityEngine.Random.Range(1, 3));
            evolved.CurrentSeverity = evolved.BaseSeverity;
            
            // Inherit evidence and add new
            evolved.Evidence = new List<EvidenceItem>(original.Evidence);
            evolved.Evidence.AddRange(GenerateNewEvidence(evolved.Category, 1, 3));
            evolved.EvidenceStrength = Mathf.Min(100f, original.EvidenceStrength + UnityEngine.Random.Range(10f, 30f));
            
            // Higher media coverage for evolved scandals
            evolved.MediaCoverage = Mathf.Min(100f, original.MediaCoverage + UnityEngine.Random.Range(20f, 40f));
            evolved.PublicInterest = Mathf.Min(100f, original.PublicInterest + UnityEngine.Random.Range(15f, 30f));
            evolved.MediaIntensity = Mathf.Min(10, original.MediaIntensity + 2);
            
            // Generate narrative linking to original
            evolved.Description = GenerateEvolutionNarrative(original, evolutionTemplate);
            evolved.Headlines = new List<string>
            {
                $"BREAKING: {original.Title} Escalates to {evolved.Title}"
            };
            
            // Copy response history
            if (original.ResponseHistory != null)
                evolved.ResponseHistory = new List<ScandalResponse>(original.ResponseHistory);
            
            // Set evolution possibilities
            evolved.CanEvolve = evolutionTemplate.CanEvolve;
            if (evolutionTemplate.CanEvolve && evolutionTemplate.TerminalEvolutions != null)
            {
                evolved.PossibleEvolutions = evolutionTemplate.TerminalEvolutions.ToList();
            }
            
            return evolved;
        }
        
        private void DiscoverNewEvidence(Scandal scandal)
        {
            var evidence = new EvidenceItem
            {
                ID = System.Guid.NewGuid().ToString(),
                Description = GenerateEvidenceDescription(scandal.Category),
                Strength = UnityEngine.Random.Range(20f, 90f),
                DiscoveredDate = gameState != null ? gameState.CurrentGameTime : System.DateTime.Now,
                Source = GetRandomSource(),
                IsPublic = true // New evidence is always public
            };
            
            if (scandal.Evidence == null)
                scandal.Evidence = new List<EvidenceItem>();
            
            scandal.Evidence.Add(evidence);
            scandal.EvidenceStrength = Mathf.Min(100f, scandal.EvidenceStrength + evidence.Strength * 0.2f);
            
            // Generate headline
            string headline = $"New Evidence Surfaces in {scandal.Title}";
            if (scandal.Headlines == null)
                scandal.Headlines = new List<string>();
            scandal.Headlines.Add(headline);
            
            Debug.Log($"New evidence: {headline}");
        }
        
        private void GenerateDevelopment(Scandal scandal)
        {
            string[] developments = new[]
            {
                "Additional sources come forward",
                "Investigation expands",
                "Opposition seizes on scandal",
                "Allies distance themselves",
                "Call for investigation grows",
                "Timeline of events emerges",
                "Pattern of behavior alleged"
            };
            
            string development = developments[UnityEngine.Random.Range(0, developments.Length)];
            if (scandal.Developments == null)
                scandal.Developments = new List<string>();
            scandal.Developments.Add(development);
            
            Debug.Log($"Scandal development: {development}");
        }
        
        private void GeneratePlotTwist(Scandal scandal)
        {
            string[] twists = new[]
            {
                "Bombshell revelation changes everything",
                "Previously unknown details emerge",
                "Key witness changes testimony",
                "Damaging document leaked",
                "Scope of scandal much larger than thought"
            };
            
            string twist = twists[UnityEngine.Random.Range(0, twists.Length)];
            if (scandal.Developments == null)
                scandal.Developments = new List<string>();
            scandal.Developments.Add($"PLOT TWIST: {twist}");
            
            if (scandal.Headlines == null)
                scandal.Headlines = new List<string>();
            scandal.Headlines.Add($"{scandal.Title}: {twist}");
            
            Debug.Log($"Plot twist: {twist}");
        }
        
        private void ResolveScandal(Scandal scandal, ResolutionType resolution)
        {
            scandal.IsResolved = true;
            scandal.ResolvedDate = gameState != null ? gameState.CurrentGameTime : System.DateTime.Now;
            scandal.Resolution = resolution;
            
            // Move to scandal history
            if (gameState != null && gameState.Player != null)
            {
                if (gameState.Player.ScandalHistory == null)
                    gameState.Player.ScandalHistory = new List<Scandal>();
                
                gameState.Player.ScandalHistory.Add(scandal);
                
                if (gameState.Player.ActiveScandals != null)
                    gameState.Player.ActiveScandals.Remove(scandal);
            }
            
            // Final headline
            string headline = GenerateResolutionHeadline(scandal, resolution);
            if (scandal.Headlines == null)
                scandal.Headlines = new List<string>();
            scandal.Headlines.Add(headline);
            
            Debug.Log($"Scandal resolved: {headline}");
        }
        
        private string GenerateStageHeadline(Scandal scandal, ScandalStage stage)
        {
            switch (stage)
            {
                case ScandalStage.Development:
                    return $"{scandal.Title} Investigation Deepens";
                case ScandalStage.Crisis:
                    return $"CRISIS: {scandal.Title} Reaches Fever Pitch";
                case ScandalStage.Resolution:
                    return $"{scandal.Title} Begins to Fade";
                default:
                    return scandal.Title;
            }
        }
        
        private string GenerateResolutionHeadline(Scandal scandal, ResolutionType resolution)
        {
            string playerName = gameState != null && gameState.Player != null ? gameState.Player.Character.Name : "Player";
            
            switch (resolution)
            {
                case ResolutionType.Denied:
                    return $"{playerName} Successfully Denies {scandal.Title}";
                case ResolutionType.Apologized:
                    return $"{playerName} Apologizes for {scandal.Title}";
                case ResolutionType.TimeFaded:
                    return $"{scandal.Title} Fades from Headlines";
                case ResolutionType.CounterAttacked:
                    return $"{playerName} Turns Tables on Accusers";
                default:
                    return $"{scandal.Title} Resolved";
            }
        }
        
        private List<EvidenceItem> GenerateNewEvidence(ScandalCategory category, int min, int max)
        {
            int count = UnityEngine.Random.Range(min, max + 1);
            var evidence = new List<EvidenceItem>();
            
            for (int i = 0; i < count; i++)
            {
                evidence.Add(new EvidenceItem
                {
                    ID = System.Guid.NewGuid().ToString(),
                    Description = GenerateEvidenceDescription(category),
                    Strength = UnityEngine.Random.Range(30f, 90f),
                    DiscoveredDate = gameState != null ? gameState.CurrentGameTime : System.DateTime.Now,
                    Source = GetRandomSource(),
                    IsPublic = true
                });
            }
            
            return evidence;
        }
        
        private string GenerateFromTemplate(string[] templates)
        {
            if (templates == null || templates.Length == 0)
                return "Scandal";
            
            string template = templates[UnityEngine.Random.Range(0, templates.Length)];
            string playerName = gameState != null && gameState.Player != null ? gameState.Player.Character.Name : "Player";
            template = template.Replace("{PLAYER}", playerName);
            return template;
        }
        
        private string GenerateEvolutionNarrative(Scandal original, ScandalTemplate evolutionTemplate)
        {
            string narrative = GenerateFromTemplate(evolutionTemplate.DescriptionTemplates);
            narrative = $"{original.Description}\n\n{narrative}";
            return narrative;
        }
        
        private string GenerateEvidenceDescription(ScandalCategory category)
        {
            switch (category)
            {
                case ScandalCategory.Financial:
                    return "Bank records showing suspicious transactions";
                case ScandalCategory.Personal:
                    return "Photographs from the incident";
                case ScandalCategory.Policy:
                    return "Impact study showing negative effects";
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
                "Whistleblower"
            }[UnityEngine.Random.Range(0, 4)];
        }
    }
}

