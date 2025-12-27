using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.News;

namespace ElectionEmpire.News
{
    /// <summary>
    /// Library of event templates that translate news into game events
    /// </summary>
    public class EventTemplateLibraryCore
    {
        private Dictionary<string, EventTemplate> templates;
        
        public void Initialize()
        {
            templates = new Dictionary<string, EventTemplate>();
            
            // Load templates for all 16 political categories
            LoadElectionTemplates();
            LoadLegislationTemplates();
            LoadScandalTemplates();
            LoadCrisisTemplates();
            LoadPolicyTemplates();
            LoadInternationalTemplates();
            LoadEconomicTemplates();
            LoadSocialUnrestTemplates();
            LoadCampaignTemplates();
            LoadAdministrativeTemplates();
            LoadJudicialTemplates();
            LoadMediaTemplates();
            LoadHealthcareTemplates();
            LoadEducationTemplates();
            LoadEnvironmentTemplates();
            LoadImmigrationTemplates();
            
            Debug.Log($"Loaded {templates.Count} event templates");
        }
        
        private void LoadElectionTemplates()
        {
            templates["ELEC-001"] = new EventTemplate
            {
                ID = "ELEC-001",
                Name = "Election Polling Shift",
                Category = EventType.Election,
                RequiredIssues = new List<IssueCategory> { IssueCategory.Economy },
                MinRelevance = 60f,
                MinControversy = 30f,
                
                TitleTemplate = "Polling Shows {ENTITY} Leading in {LOCATION}",
                DescriptionTemplate = "Recent polls indicate {ENTITY} has gained significant support " +
                                     "following {TOPIC}. This could impact your campaign strategy.",
                
                VariableSlots = new Dictionary<string, VariableType>
                {
                    { "ENTITY", VariableType.Person },
                    { "LOCATION", VariableType.Location },
                    { "TOPIC", VariableType.Topic }
                },
                
                ImpactFormula = new ImpactFormula
                {
                    TrustImpact = 0f, // Neutral
                    VoterBlocImpacts = new Dictionary<IssueCategory, float>
                    {
                        { IssueCategory.Economy, 2f }
                    },
                    PolicyOpportunity = IssueCategory.Economy
                },
                
                ResponseOptions = new List<ResponseOption>
                {
                    new() { Type = "Align", Description = "Align with {ENTITY}'s position" },
                    new() { Type = "Oppose", Description = "Take opposite stance" },
                    new() { Type = "Ignore", Description = "Focus on other issues" }
                }
            };
            
            templates["ELEC-002"] = new EventTemplate
            {
                ID = "ELEC-002",
                Name = "Voter Turnout Concern",
                Category = EventType.Election,
                TitleTemplate = "Low Voter Turnout Predicted for {LOCATION}",
                DescriptionTemplate = "Analysts predict historically low turnout in {LOCATION}. " +
                                     "This could favor candidates with strong base support.",
                ImpactFormula = new ImpactFormula
                {
                    TrustImpact = -3f,
                    VoterBlocImpacts = new Dictionary<IssueCategory, float>
                    {
                        { IssueCategory.SocialIssues, -5f }
                    }
                }
            };
        }
        
        private void LoadLegislationTemplates()
        {
            templates["LEG-001"] = new EventTemplate
            {
                ID = "LEG-001",
                Name = "Major Bill Passes",
                Category = EventType.Legislation,
                RequiredIssues = new List<IssueCategory> { IssueCategory.Healthcare },
                TitleTemplate = "{ORGANIZATION} Passes {ISSUE} Bill",
                DescriptionTemplate = "{ORGANIZATION} has passed a major {ISSUE} bill. " +
                                     "Your position on this will affect voter support.",
                ImpactFormula = new ImpactFormula
                {
                    TrustImpact = 0f,
                    PolicyOpportunity = IssueCategory.Healthcare
                },
                ResponseOptions = new List<ResponseOption>
                {
                    new() { Type = "Support", Description = "Publicly support the bill" },
                    new() { Type = "Oppose", Description = "Criticize the bill" },
                    new() { Type = "Neutral", Description = "Take neutral position" }
                }
            };
            
            templates["LEG-002"] = new EventTemplate
            {
                ID = "LEG-002",
                Name = "Bill Stalled in Committee",
                Category = EventType.Legislation,
                TitleTemplate = "{ISSUE} Legislation Stalled",
                DescriptionTemplate = "A {ISSUE} bill has stalled in committee. " +
                                     "This creates an opportunity for you to take a position.",
                ImpactFormula = new ImpactFormula
                {
                    PolicyOpportunity = IssueCategory.Healthcare
                }
            };
        }
        
        private void LoadScandalTemplates()
        {
            templates["SCAN-001"] = new EventTemplate
            {
                ID = "SCAN-001",
                Name = "Political Scandal Breaks",
                Category = EventType.Scandal,
                MinControversy = 60f,
                TitleTemplate = "{ENTITY} Involved in {TOPIC} Scandal",
                DescriptionTemplate = "Breaking news: {ENTITY} is embroiled in a {TOPIC} scandal. " +
                                     "How you respond could affect your own reputation.",
                ImpactFormula = new ImpactFormula
                {
                    TrustImpact = -5f, // General political trust decline
                    VoterBlocImpacts = new Dictionary<IssueCategory, float>
                    {
                        { IssueCategory.SocialIssues, -8f }
                    }
                },
                ResponseOptions = new List<ResponseOption>
                {
                    new() { Type = "Condemn", Description = "Publicly condemn {ENTITY}" },
                    new() { Type = "Defend", Description = "Defend {ENTITY}" },
                    new() { Type = "Distance", Description = "Distance yourself" }
                }
            };
        }
        
        private void LoadCrisisTemplates()
        {
            templates["CRIS-001"] = new EventTemplate
            {
                ID = "CRIS-001",
                Name = "National Crisis Emerges",
                Category = EventType.Crisis,
                MinControversy = 70f,
                TitleTemplate = "{TOPIC} Crisis Demands Response",
                DescriptionTemplate = "A major {TOPIC} crisis has emerged. " +
                                     "Voters expect leadership. Your response will be scrutinized.",
                ImpactFormula = new ImpactFormula
                {
                    TrustImpact = -10f, // Crisis creates uncertainty
                    PolicyOpportunity = IssueCategory.Healthcare // Example
                },
                ResponseOptions = new List<ResponseOption>
                {
                    new() { Type = "Act", Description = "Propose immediate action" },
                    new() { Type = "Study", Description = "Call for investigation" },
                    new() { Type = "Delegate", Description = "Let others handle it" }
                }
            };
        }
        
        private void LoadPolicyTemplates()
        {
            templates["POL-001"] = new EventTemplate
            {
                ID = "POL-001",
                Name = "Policy Announcement",
                Category = EventType.PolicyAnnouncement,
                TitleTemplate = "{ENTITY} Announces {ISSUE} Policy",
                DescriptionTemplate = "{ENTITY} has announced a new {ISSUE} policy. " +
                                     "This shifts the political landscape.",
                ImpactFormula = new ImpactFormula
                {
                    PolicyOpportunity = IssueCategory.Healthcare
                }
            };
        }
        
        private void LoadInternationalTemplates()
        {
            templates["INT-001"] = new EventTemplate
            {
                ID = "INT-001",
                Name = "International Incident",
                Category = EventType.International,
                TitleTemplate = "{LOCATION} Incident Affects {TOPIC}",
                DescriptionTemplate = "An international incident in {LOCATION} affects {TOPIC}. " +
                                     "Your foreign policy stance matters now.",
                ImpactFormula = new ImpactFormula
                {
                    PolicyOpportunity = IssueCategory.ForeignPolicy
                }
            };
        }
        
        private void LoadEconomicTemplates()
        {
            templates["ECON-001"] = new EventTemplate
            {
                ID = "ECON-001",
                Name = "Economic Indicator Change",
                Category = EventType.Economic,
                RequiredIssues = new List<IssueCategory> { IssueCategory.Economy },
                TitleTemplate = "{TOPIC} Economic News Shakes Markets",
                DescriptionTemplate = "Major {TOPIC} economic news has emerged. " +
                                     "Voters will judge your economic competence.",
                ImpactFormula = new ImpactFormula
                {
                    TrustImpact = -5f,
                    VoterBlocImpacts = new Dictionary<IssueCategory, float>
                    {
                        { IssueCategory.Economy, -10f }
                    },
                    PolicyOpportunity = IssueCategory.Economy
                }
            };
        }
        
        private void LoadSocialUnrestTemplates()
        {
            templates["SOC-001"] = new EventTemplate
            {
                ID = "SOC-001",
                Name = "Social Unrest",
                Category = EventType.SocialUnrest,
                MinControversy = 70f,
                TitleTemplate = "{LOCATION} Protests Over {TOPIC}",
                DescriptionTemplate = "Protests in {LOCATION} over {TOPIC} demand a response. " +
                                     "Your position will define you.",
                ImpactFormula = new ImpactFormula
                {
                    TrustImpact = -8f,
                    PolicyOpportunity = IssueCategory.SocialIssues
                }
            };
        }
        
        private void LoadCampaignTemplates()
        {
            templates["CAMP-001"] = new EventTemplate
            {
                ID = "CAMP-001",
                Name = "Campaign Event",
                Category = EventType.CampaignEvent,
                TitleTemplate = "{ENTITY} Makes {TOPIC} Statement",
                DescriptionTemplate = "{ENTITY} made a major statement on {TOPIC} at a campaign event. " +
                                     "This affects the race.",
                ImpactFormula = new ImpactFormula
                {
                    PolicyOpportunity = IssueCategory.Healthcare
                }
            };
        }
        
        // Additional category loaders (simplified for space)
        private void LoadAdministrativeTemplates()
        {
            templates["ADM-001"] = new EventTemplate
            {
                ID = "ADM-001",
                Name = "Administrative Change",
                Category = EventType.PolicyAnnouncement,
                TitleTemplate = "{ORGANIZATION} Announces {TOPIC} Changes",
                DescriptionTemplate = "{ORGANIZATION} has announced administrative changes to {TOPIC}.",
                ImpactFormula = new ImpactFormula { }
            };
        }
        
        private void LoadJudicialTemplates()
        {
            templates["JUD-001"] = new EventTemplate
            {
                ID = "JUD-001",
                Name = "Court Decision",
                Category = EventType.PolicyAnnouncement,
                TitleTemplate = "Court Rules on {TOPIC}",
                DescriptionTemplate = "A major court decision on {TOPIC} has been announced.",
                ImpactFormula = new ImpactFormula { }
            };
        }
        
        private void LoadMediaTemplates()
        {
            templates["MED-001"] = new EventTemplate
            {
                ID = "MED-001",
                Name = "Media Coverage",
                Category = EventType.PolicyAnnouncement,
                TitleTemplate = "Media Focuses on {TOPIC}",
                DescriptionTemplate = "Media attention has shifted to {TOPIC}.",
                ImpactFormula = new ImpactFormula { }
            };
        }
        
        private void LoadHealthcareTemplates()
        {
            templates["HEALTH-001"] = new EventTemplate
            {
                ID = "HEALTH-001",
                Name = "Healthcare News",
                Category = EventType.PolicyAnnouncement,
                RequiredIssues = new List<IssueCategory> { IssueCategory.Healthcare },
                TitleTemplate = "{TOPIC} Healthcare Development",
                DescriptionTemplate = "Major healthcare news: {TOPIC}",
                ImpactFormula = new ImpactFormula
                {
                    PolicyOpportunity = IssueCategory.Healthcare
                }
            };
        }
        
        private void LoadEducationTemplates()
        {
            templates["EDU-001"] = new EventTemplate
            {
                ID = "EDU-001",
                Name = "Education News",
                Category = EventType.PolicyAnnouncement,
                RequiredIssues = new List<IssueCategory> { IssueCategory.Education },
                TitleTemplate = "{TOPIC} Education Development",
                DescriptionTemplate = "Education news: {TOPIC}",
                ImpactFormula = new ImpactFormula
                {
                    PolicyOpportunity = IssueCategory.Education
                }
            };
        }
        
        private void LoadEnvironmentTemplates()
        {
            templates["ENV-001"] = new EventTemplate
            {
                ID = "ENV-001",
                Name = "Environmental News",
                Category = EventType.PolicyAnnouncement,
                RequiredIssues = new List<IssueCategory> { IssueCategory.Environment },
                TitleTemplate = "{TOPIC} Environmental Development",
                DescriptionTemplate = "Environmental news: {TOPIC}",
                ImpactFormula = new ImpactFormula
                {
                    PolicyOpportunity = IssueCategory.Environment
                }
            };
        }
        
        private void LoadImmigrationTemplates()
        {
            templates["IMM-001"] = new EventTemplate
            {
                ID = "IMM-001",
                Name = "Immigration News",
                Category = EventType.PolicyAnnouncement,
                RequiredIssues = new List<IssueCategory> { IssueCategory.Immigration },
                TitleTemplate = "{TOPIC} Immigration Development",
                DescriptionTemplate = "Immigration news: {TOPIC}",
                ImpactFormula = new ImpactFormula
                {
                    PolicyOpportunity = IssueCategory.Immigration
                }
            };
        }
        
        /// <summary>
        /// Find matching templates for processed news
        /// </summary>
        public List<EventTemplate> FindMatchingTemplates(ProcessedNews news)
        {
            var matches = new List<EventTemplate>();
            
            foreach (var template in templates.Values)
            {
                if (MatchesTemplate(news, template))
                {
                    matches.Add(template);
                }
            }
            
            // Score and sort by relevance
            matches = matches
                .OrderByDescending(t => ScoreTemplateMatch(news, t))
                .ToList();
            
            return matches;
        }
        
        private bool MatchesTemplate(ProcessedNews news, EventTemplate template)
        {
            // Check category match
            if (template.Category != news.EventType)
                return false;
            
            // Check minimum relevance
            if (news.PoliticalRelevance < template.MinRelevance)
                return false;
            
            // Check minimum controversy
            if (news.ControversyScore < template.MinControversy)
                return false;
            
            // Check required issues
            if (template.RequiredIssues != null && template.RequiredIssues.Count > 0)
            {
                bool hasRequiredIssue = template.RequiredIssues.Any(issue => 
                    news.IssueCategories.Contains(issue));
                
                if (!hasRequiredIssue)
                    return false;
            }
            
            return true;
        }
        
        private float ScoreTemplateMatch(ProcessedNews news, EventTemplate template)
        {
            float score = 0f;
            
            // Base score from relevance
            score += news.PoliticalRelevance * 0.5f;
            
            // Bonus for issue match
            if (template.RequiredIssues != null && news.IssueCategories != null)
            {
                int matchingIssues = template.RequiredIssues.Count(issue => 
                    news.IssueCategories.Contains(issue));
                score += matchingIssues * 20f;
            }
            
            // Bonus for controversy match
            if (news.ControversyScore >= template.MinControversy)
            {
                score += (news.ControversyScore - template.MinControversy) * 0.3f;
            }
            
            return score;
        }
        
        public EventTemplate GetTemplate(string templateID)
        {
            return templates.GetValueOrDefault(templateID);
        }
    }
    
    [Serializable]
    public class EventTemplate
    {
        public string ID;
        public string TemplateId; // Alias for ID
        public string Name;
        public EventType Category;
        public List<IssueCategory> RequiredIssues;
        public float MinRelevance = 0f;
        public float MinControversy = 0f;
        public float MinImpactScore = 0f;

        public string TitleTemplate;
        public string HeadlineTemplate; // Alias for TitleTemplate
        public string DescriptionTemplate;
        public string ContextTemplate;

        public Dictionary<string, VariableType> VariableSlots;
        public List<Templates.VariableMapping> Variables;
        public ImpactFormula ImpactFormula;
        public List<ResponseOption> ResponseOptions;

        // Template classification
        public Templates.TemplateEventType DefaultEventType;
        public Templates.UrgencyLevel DefaultUrgency;

        // Office tier scaling
        public Templates.OfficeScaling Scaling;

        // Base effects
        public Templates.BaseEffects Effects;

        // Additional metadata
        public List<Templates.TemplateEntityType> RequiredEntities;
        public List<string> TriggerKeywords;
        public List<string> Tags;
        public bool ChaosModeOnly;

        public EventTemplate()
        {
            RequiredIssues = new List<IssueCategory>();
            VariableSlots = new Dictionary<string, VariableType>();
            Variables = new List<Templates.VariableMapping>();
            ResponseOptions = new List<ResponseOption>();
            ImpactFormula = new ImpactFormula();
            RequiredEntities = new List<Templates.TemplateEntityType>();
            TriggerKeywords = new List<string>();
            Tags = new List<string>();
            Scaling = new Templates.OfficeScaling();
            Effects = new Templates.BaseEffects();
            DefaultUrgency = Templates.UrgencyLevel.Developing;
            DefaultEventType = Templates.TemplateEventType.PolicyPressure;
        }
    }
    
    public enum VariableType
    {
        Person,
        Organization,
        Location,
        Topic,
        Issue,
        Number,
        Date
    }
    
    [Serializable]
    public class ImpactFormula
    {
        public float TrustImpact;
        public Dictionary<IssueCategory, float> VoterBlocImpacts;
        public IssueCategory? PolicyOpportunity;
        public float ResourceCost;
        public float ResourceGain;
        
        public ImpactFormula()
        {
            VoterBlocImpacts = new Dictionary<IssueCategory, float>();
        }
    }
    
    [Serializable]
    public class ResponseOption
    {
        public string Type;
        public string Description;
        public Dictionary<string, float> Costs;
        public Dictionary<string, float> Effects;
        
        public ResponseOption()
        {
            Costs = new Dictionary<string, float>();
            Effects = new Dictionary<string, float>();
        }
    }
}

