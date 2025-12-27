using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.World;
using ElectionEmpire.Gameplay;
using ElectionEmpire.News.Templates;

namespace ElectionEmpire.News
{
    /// <summary>
    /// Creates game events from processed news
    /// </summary>
    public class EventFactory
    {
        private VariableMapper variableMapper;
        private ImpactCalculator impactCalculator;
        
        public void Initialize()
        {
            // Initialize new template library (static)
            Templates.EventTemplateLibrary.Initialize();
            
            variableMapper = new VariableMapper();
            impactCalculator = new ImpactCalculator();
        }
        
        /// <summary>
        /// Create game event from processed news
        /// </summary>
        public NewsGameEvent CreateEvent(ProcessedNews news, PlayerState player)
        {
            // Find matching templates using new library
            var templates = Templates.EventTemplateLibrary.FindMatchingTemplates(news);
            
            if (templates.Count == 0)
            {
                // No match - create generic event
                return CreateGenericEvent(news, player);
            }
            
            // Use best matching template
            var template = templates[0];
            
            // Map variables (using new template structure)
            var variables = MapVariablesFromNews(news, template);
            
            // Fill template
            string title = FillTemplate(template.HeadlineTemplate, variables);
            string description = FillTemplate(template.DescriptionTemplate, variables);
            string context = FillTemplate(template.ContextTemplate, variables);
            
            // Calculate impact using new effect ranges
            float significance = impactCalculator.CalculateStorySignificance(news, null); // Pass null for now
            var scaledImpact = CalculateScaledImpact(template, player);
            
            // Convert TemplateEventType to EventType
            EventType eventType = ConvertTemplateEventType(template.DefaultEventType);
            
            // Create event
            var gameEvent = new NewsGameEvent
            {
                ID = System.Guid.NewGuid().ToString(),
                TemplateID = template.TemplateId,
                OriginalNews = news,
                Title = title,
                Description = description + "\n\n" + context,
                EventType = eventType,
                Significance = significance,
                ImpactFormula = scaledImpact,
                ResponseOptions = GenerateResponseOptions(template, variables),
                CreatedDate = DateTime.Now,
                ExpiresDate = DateTime.Now.AddDays(GetUrgencyDays(template.DefaultUrgency))
            };
            
            return gameEvent;
        }
        
        private Dictionary<string, string> MapVariablesFromNews(ProcessedNews news, EventTemplate template)
        {
            var variables = new Dictionary<string, string>();
            
            // Use VariableMapper for basic extraction
            var basicVars = variableMapper.MapVariables(news, null); // Pass null, we'll do custom mapping
            
            // Extract based on template variable mappings
            foreach (var varMapping in template.Variables)
            {
                string value = ExtractVariableValue(news, varMapping);
                variables[varMapping.VariableName] = value;
            }
            
            return variables;
        }
        
        private string ExtractVariableValue(ProcessedNews news, VariableMapping mapping)
        {
            // Simple extraction based on variable name patterns
            string varName = mapping.VariableName.ToLower();
            
            if (varName.Contains("person") || varName.Contains("name") || varName.Contains("candidate") || varName.Contains("politician"))
            {
                var person = news.Entities?.FirstOrDefault(e => e.Type == EntityType.Person);
                return person?.Name ?? mapping.FallbackValue;
            }
            
            if (varName.Contains("organization") || varName.Contains("body") || varName.Contains("company"))
            {
                var org = news.Entities?.FirstOrDefault(e => e.Type == EntityType.Organization);
                return org?.Name ?? mapping.FallbackValue;
            }
            
            if (varName.Contains("location") || varName.Contains("region") || varName.Contains("country"))
            {
                var loc = news.Entities?.FirstOrDefault(e => e.Type == EntityType.Location);
                return loc?.Name ?? mapping.FallbackValue;
            }
            
            if (varName.Contains("topic") || varName.Contains("issue") || varName.Contains("policy"))
            {
                if (news.IssueCategories != null && news.IssueCategories.Count > 0)
                    return news.IssueCategories[0].ToString();
                if (news.Topics != null && news.Topics.Count > 0)
                    return news.Topics[0];
            }
            
            return mapping.FallbackValue;
        }
        
        private string FillTemplate(string template, Dictionary<string, string> variables)
        {
            string result = template;
            foreach (var variable in variables)
            {
                result = result.Replace($"{{{variable.Key}}}", variable.Value);
            }
            return result;
        }
        
        private ImpactFormula CalculateScaledImpact(EventTemplate template, PlayerState player)
        {
            var impact = new ImpactFormula();
            
            // Get office tier multiplier
            int tier = player.CurrentOffice?.Tier ?? 1;
            float multiplier = template.Scaling.GetMultiplier(tier);
            
            // Apply scaled effects
            if (template.Effects.TrustDelta != null)
                impact.TrustImpact = template.Effects.TrustDelta.Roll() * multiplier;
            
            if (template.Effects.CapitalDelta != null)
                impact.ResourceCost = template.Effects.CapitalDelta.Roll() * multiplier;
            
            if (template.Effects.MediaDelta != null)
                impact.ResourceGain = template.Effects.MediaDelta.Roll() * multiplier;
            
            // Voter bloc effects
            if (template.Effects.VoterBlocEffects != null)
            {
                foreach (var blocEffect in template.Effects.VoterBlocEffects)
                {
                    // Map string to IssueCategory
                    if (System.Enum.TryParse<IssueCategory>(blocEffect.Key, out IssueCategory category))
                    {
                        impact.VoterBlocImpacts[category] = blocEffect.Value.Roll() * multiplier;
                    }
                }
            }
            
            // Policy opportunity from category
            impact.PolicyOpportunity = MapCategoryToIssue(template.Category);
            
            return impact;
        }
        
        private IssueCategory? MapCategoryToIssue(PoliticalCategory category)
        {
            return category switch
            {
                PoliticalCategory.HealthcarePolicy => IssueCategory.Healthcare,
                PoliticalCategory.EconomicPolicy => IssueCategory.Economy,
                PoliticalCategory.Education => IssueCategory.Education,
                PoliticalCategory.Immigration => IssueCategory.Immigration,
                PoliticalCategory.ClimateEnvironment => IssueCategory.Environment,
                PoliticalCategory.CrimeJustice => IssueCategory.Crime,
                _ => null
            };
        }
        
        private EventType ConvertTemplateEventType(TemplateEventType templateType)
        {
            return templateType switch
            {
                TemplateEventType.Crisis => EventType.Crisis,
                TemplateEventType.PolicyPressure => EventType.PolicyAnnouncement,
                TemplateEventType.Opportunity => EventType.PolicyAnnouncement,
                TemplateEventType.ScandalTrigger => EventType.Scandal,
                TemplateEventType.InformationalOnly => EventType.PolicyAnnouncement,
                _ => EventType.PolicyAnnouncement
            };
        }
        
        private List<ResponseOption> GenerateResponseOptions(EventTemplate template, Dictionary<string, string> variables)
        {
            var options = new List<ResponseOption>();
            
            // Generate based on event type
            switch (template.DefaultEventType)
            {
                case TemplateEventType.Crisis:
                    options.Add(new ResponseOption { Type = "Act", Description = "Take immediate action" });
                    options.Add(new ResponseOption { Type = "Investigate", Description = "Call for investigation" });
                    options.Add(new ResponseOption { Type = "Delegate", Description = "Let others handle it" });
                    break;
                
                case TemplateEventType.PolicyPressure:
                    options.Add(new ResponseOption { Type = "Support", Description = "Publicly support this development" });
                    options.Add(new ResponseOption { Type = "Oppose", Description = "Criticize this development" });
                    options.Add(new ResponseOption { Type = "Neutral", Description = "Take neutral position" });
                    break;
                
                case TemplateEventType.Opportunity:
                    options.Add(new ResponseOption { Type = "Seize", Description = "Capitalize on this opportunity" });
                    options.Add(new ResponseOption { Type = "Ignore", Description = "Focus elsewhere" });
                    break;
                
                case TemplateEventType.ScandalTrigger:
                    options.Add(new ResponseOption { Type = "Condemn", Description = "Publicly condemn" });
                    options.Add(new ResponseOption { Type = "Defend", Description = "Defend the accused" });
                    options.Add(new ResponseOption { Type = "Distance", Description = "Distance yourself" });
                    break;
            }
            
            // Fill variables in descriptions
            foreach (var option in options)
            {
                option.Description = FillTemplate(option.Description, variables);
            }
            
            return options;
        }
        
        private int GetUrgencyDays(UrgencyLevel urgency)
        {
            return urgency switch
            {
                UrgencyLevel.Breaking => 2,
                UrgencyLevel.Urgent => 5,
                UrgencyLevel.Developing => 10,
                UrgencyLevel.Informational => 7,
                _ => 7
            };
        }
        
        private NewsGameEvent CreateGenericEvent(ProcessedNews news, PlayerState player)
        {
            // Fallback generic event
            return new NewsGameEvent
            {
                ID = System.Guid.NewGuid().ToString(),
                OriginalNews = news,
                Title = news.OriginalArticle.Title,
                Description = news.OriginalArticle.Description,
                EventType = news.EventType,
                Significance = news.PoliticalRelevance,
                ImpactFormula = new ImpactFormula
                {
                    TrustImpact = news.Sentiment.Net * 0.1f
                },
                ResponseOptions = new List<ResponseOption>
                {
                    new() { Type = "Acknowledge", Description = "Acknowledge the news" },
                    new() { Type = "Ignore", Description = "Ignore it" }
                },
                CreatedDate = DateTime.Now,
                ExpiresDate = DateTime.Now.AddDays(7)
            };
        }
        
        /// <summary>
        /// Create policy challenge from news
        /// </summary>
        public PolicyChallenge CreatePolicyChallenge(ProcessedNews news, PlayerState player)
        {
            if (news.IssueCategories == null || news.IssueCategories.Count == 0)
                return null;
            
            var challenge = new PolicyChallenge
            {
                ID = System.Guid.NewGuid().ToString(),
                SourceNews = news,
                Issue = news.IssueCategories[0],
                Question = GeneratePolicyQuestion(news),
                StanceOptions = GenerateStanceOptions(news.IssueCategories[0]),
                Deadline = DateTime.Now.AddDays(3),
                CreatedDate = DateTime.Now
            };
            
            return challenge;
        }
        
        /// <summary>
        /// Create crisis event from high-controversy news
        /// </summary>
        public CrisisEvent CreateCrisisEvent(ProcessedNews news, PlayerState player)
        {
            if (news.ControversyScore < 70f)
                return null;
            
            var crisis = new CrisisEvent
            {
                ID = System.Guid.NewGuid().ToString(),
                SourceNews = news,
                Title = $"Crisis: {news.OriginalArticle.Title}",
                Description = news.OriginalArticle.Description,
                Severity = news.ControversyScore / 10f,
                ResponsePaths = GenerateCrisisResponses(news),
                CreatedDate = DateTime.Now,
                EscalationDate = DateTime.Now.AddDays(2)
            };
            
            return crisis;
        }
        
        /// <summary>
        /// Create opportunity from positive news
        /// </summary>
        public OpportunityEvent CreateOpportunityEvent(ProcessedNews news, PlayerState player)
        {
            if (news.Sentiment.Classification != SentimentType.Positive && 
                news.Sentiment.Classification != SentimentType.VeryPositive)
                return null;
            
            var opportunity = new OpportunityEvent
            {
                ID = System.Guid.NewGuid().ToString(),
                SourceNews = news,
                Title = $"Opportunity: {news.OriginalArticle.Title}",
                Description = news.OriginalArticle.Description,
                BenefitType = DetermineBenefitType(news),
                Benefits = CalculateOpportunityBenefits(news),
                CreatedDate = DateTime.Now,
                ExpiresDate = DateTime.Now.AddDays(5)
            };
            
            return opportunity;
        }
        
        private string GeneratePolicyQuestion(ProcessedNews news)
        {
            if (news.IssueCategories == null || news.IssueCategories.Count == 0)
                return "How do you respond to this development?";
            
            string issue = news.IssueCategories[0].ToString();
            return $"The news highlights {issue}. What is your position?";
        }
        
        private List<StanceOption> GenerateStanceOptions(IssueCategory issue)
        {
            return new List<StanceOption>
            {
                new() { Stance = "Support", Alignment = 80f },
                new() { Stance = "Oppose", Alignment = 20f },
                new() { Stance = "Moderate", Alignment = 50f },
                new() { Stance = "No Comment", Alignment = 50f }
            };
        }
        
        private List<CrisisResponse> GenerateCrisisResponses(ProcessedNews news)
        {
            return new List<CrisisResponse>
            {
                new() { Type = "Act", Description = "Take immediate action" },
                new() { Type = "Investigate", Description = "Call for investigation" },
                new() { Type = "Delegate", Description = "Let others handle it" },
                new() { Type = "Deny", Description = "Deny the crisis exists" }
            };
        }
        
        private string DetermineBenefitType(ProcessedNews news)
        {
            if (news.IssueCategories != null && news.IssueCategories.Count > 0)
            {
                return $"Policy Opportunity: {news.IssueCategories[0]}";
            }
            
            return "General Opportunity";
        }
        
        private Dictionary<string, float> CalculateOpportunityBenefits(ProcessedNews news)
        {
            var benefits = new Dictionary<string, float>
            {
                { "PublicTrust", news.Sentiment.Positive * 2f },
                { "MediaInfluence", 10f }
            };
            
            return benefits;
        }
    }
    
    [Serializable]
    public class NewsEventEffects
    {
        public float TrustDelta;
        public float CapitalDelta;
        public float FundsDelta;
        public float MediaDelta;
        public float PartyLoyaltyDelta;
        public Dictionary<string, float> VoterBlocDeltas;

        public NewsEventEffects()
        {
            VoterBlocDeltas = new Dictionary<string, float>();
        }

        /// <summary>
        /// Create NewsEventEffects from ScaledEffects
        /// </summary>
        public static NewsEventEffects FromScaledEffects(Translation.ScaledEffects scaled)
        {
            return new NewsEventEffects
            {
                TrustDelta = scaled.TrustDelta,
                CapitalDelta = scaled.CapitalDelta,
                FundsDelta = scaled.FundsDelta,
                MediaDelta = scaled.MediaDelta,
                PartyLoyaltyDelta = scaled.PartyLoyaltyDelta,
                VoterBlocDeltas = new Dictionary<string, float>(scaled.VoterBlocDeltas)
            };
        }
    }

    [Serializable]
    public class NewsGameEvent
    {
        public string ID;
        public string TemplateID;
        public ProcessedNews OriginalNews;
        public string Title;
        public string Description;
        public EventType EventType;
        public float Significance;
        public ImpactFormula ImpactFormula;
        public List<ResponseOption> ResponseOptions;
        public DateTime CreatedDate;
        public DateTime ExpiresDate;
        public bool IsResolved;

        // Additional properties for compatibility
        public string EventId { get { return ID; } set { ID = value; } }
        public EventType Type { get { return EventType; } set { EventType = value; } }
        public UrgencyLevel Urgency;
        public string SourceNewsId;
        public string Headline { get { return Title; } set { Title = value; } }
        public string ContextText;
        public string RealWorldNote;

        // Turn tracking
        public int CreatedTurn;
        public int ExpirationTurn;
        public int DeadlineTurn;
        public int CurrentStage;

        // Response tracking
        public List<string> ResponseHistory;
        public string Category;
        public List<string> Tags;
        public bool IsChaosModeContent;

        // Evolution
        public List<string> EscalationStages;
        public NewsEventEffects Effects;

        // Player interaction tracking
        public bool PlayerResponded;

        public NewsGameEvent()
        {
            ResponseOptions = new List<ResponseOption>();
            ResponseHistory = new List<string>();
            Tags = new List<string>();
            EscalationStages = new List<string>();
            Effects = new NewsEventEffects();
        }
    }
    
    [Serializable]
    public class PolicyChallenge
    {
        public string ID;
        public ProcessedNews SourceNews;
        public IssueCategory Issue;
        public string Question;
        public List<StanceOption> StanceOptions;
        public DateTime Deadline;
        public DateTime CreatedDate;
        public string SelectedStance;
        
        public PolicyChallenge()
        {
            StanceOptions = new List<StanceOption>();
        }
    }
    
    [Serializable]
    public class StanceOption
    {
        public string Stance;
        public float Alignment; // 0-100, how aligned with issue
        public Dictionary<string, float> Effects;
        
        public StanceOption()
        {
            Effects = new Dictionary<string, float>();
        }
    }
    
    [Serializable]
    public class CrisisEvent
    {
        public string ID;
        public ProcessedNews SourceNews;
        public string Title;
        public string Description;
        public float Severity; // 1-10
        public List<CrisisResponse> ResponsePaths;
        public DateTime CreatedDate;
        public DateTime EscalationDate;
        public bool Escalated;
        
        public CrisisEvent()
        {
            ResponsePaths = new List<CrisisResponse>();
        }
    }
    
    [Serializable]
    public class CrisisResponse
    {
        public string Type;
        public string Description;
        public Dictionary<string, float> Costs;
        public Dictionary<string, float> Effects;
        
        public CrisisResponse()
        {
            Costs = new Dictionary<string, float>();
            Effects = new Dictionary<string, float>();
        }
    }
    
    [Serializable]
    public class OpportunityEvent
    {
        public string ID;
        public ProcessedNews SourceNews;
        public string Title;
        public string Description;
        public string BenefitType;
        public Dictionary<string, float> Benefits;
        public DateTime CreatedDate;
        public DateTime ExpiresDate;
        public bool Claimed;
        
        public OpportunityEvent()
        {
            Benefits = new Dictionary<string, float>();
        }
    }
}

