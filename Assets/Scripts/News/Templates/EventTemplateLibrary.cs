// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - Event Template Library
// Complete template definitions for News → Game Event translation
// ═══════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.News;

namespace ElectionEmpire.News.Templates
{
    #region Enums
    
    public enum PoliticalCategory
    {
        DomesticLegislation,
        ElectionCampaign,
        PoliticalScandal,
        EconomicPolicy,
        ForeignPolicy,
        HealthcarePolicy,
        Immigration,
        ClimateEnvironment,
        CrimeJustice,
        CivilRights,
        Education,
        MilitaryDefense,
        TechnologyPolicy,
        SocialIssues,
        PartyPolitics,
        LocalGovernment,
        General
    }
    
    public enum TemplateEventType
    {
        Crisis,
        PolicyPressure,
        Opportunity,
        ScandalTrigger,
        InformationalOnly
    }
    
    public enum UrgencyLevel
    {
        Breaking,       // 1-2 turns
        Urgent,         // 3-5 turns
        Developing,     // 6-10 turns
        Informational   // No deadline
    }
    
    public enum TemplateEntityType
    {
        Person,
        Organization,
        Location,
        Legislation,
        Event,
        Party,
        Agency
    }
    
    #endregion
    
    #region Data Structures
    
    [Serializable]
    public class VariableMapping
    {
        public string VariableName;
        public string SourcePath;        // e.g., "entities.people[0].name"
        public string FallbackValue;     // Default if source not found
        public bool Required;
    }
    
    [Serializable]
    public class OfficeScaling
    {
        public float Tier1 = 0.3f;   // Local
        public float Tier2 = 0.5f;   // Municipal
        public float Tier3 = 0.8f;   // State
        public float Tier4 = 1.2f;   // National
        public float Tier5 = 2.0f;   // Presidential
        
        public float GetMultiplier(int tier) => tier switch
        {
            1 => Tier1,
            2 => Tier2,
            3 => Tier3,
            4 => Tier4,
            5 => Tier5,
            _ => 1.0f
        };
    }
    
    [Serializable]
    public class EffectRange
    {
        public float Min;
        public float Max;
        
        public EffectRange(float min, float max)
        {
            Min = min;
            Max = max;
        }
        
        public float Roll() => UnityEngine.Random.Range(Min, Max);
    }
    
    [Serializable]
    public class BaseEffects
    {
        public EffectRange TrustDelta;
        public EffectRange CapitalDelta;
        public EffectRange FundsDelta;
        public EffectRange MediaDelta;
        public EffectRange PartyLoyaltyDelta;
        public Dictionary<string, EffectRange> VoterBlocEffects;
        
        public BaseEffects()
        {
            VoterBlocEffects = new Dictionary<string, EffectRange>();
        }
    }
    #endregion
    
    /// <summary>
    /// Central repository for all event templates.
    /// Templates are organized by PoliticalCategory with ~5-6 templates each.
    /// </summary>
    public static class EventTemplateLibrary
    {
        private static Dictionary<PoliticalCategory, List<EventTemplate>> _templates;
        private static bool _initialized = false;
        
        public static void Initialize()
        {
            if (_initialized) return;
            
            _templates = new Dictionary<PoliticalCategory, List<EventTemplate>>();
            
            // Initialize all categories
            foreach (PoliticalCategory category in Enum.GetValues(typeof(PoliticalCategory)))
            {
                _templates[category] = new List<EventTemplate>();
            }
            
            // Load all templates
            LoadDomesticLegislationTemplates();
            LoadElectionCampaignTemplates();
            LoadPoliticalScandalTemplates();
            LoadEconomicPolicyTemplates();
            LoadForeignPolicyTemplates();
            LoadHealthcarePolicyTemplates();
            LoadImmigrationTemplates();
            LoadClimateEnvironmentTemplates();
            LoadCrimeJusticeTemplates();
            LoadCivilRightsTemplates();
            LoadEducationTemplates();
            LoadMilitaryDefenseTemplates();
            LoadTechnologyPolicyTemplates();
            LoadSocialIssuesTemplates();
            LoadPartyPoliticsTemplates();
            LoadLocalGovernmentTemplates();
            
            _initialized = true;
            Debug.Log($"[EventTemplateLibrary] Initialized with {GetTotalTemplateCount()} templates");
        }
        
        public static List<EventTemplate> GetTemplatesForCategory(PoliticalCategory category)
        {
            if (!_initialized) Initialize();
            return _templates.TryGetValue(category, out var list) ? list : new List<EventTemplate>();
        }
        
        public static EventTemplate GetTemplateById(string templateId)
        {
            if (!_initialized) Initialize();
            
            foreach (var kvp in _templates)
            {
                var template = kvp.Value.Find(t => t.TemplateId == templateId);
                if (template != null) return template;
            }
            return null;
        }
        
        public static int GetTotalTemplateCount()
        {
            if (!_initialized) Initialize();
            int count = 0;
            foreach (var kvp in _templates) count += kvp.Value.Count;
            return count;
        }
        
        /// <summary>
        /// Find matching templates for processed news
        /// </summary>
        public static List<EventTemplate> FindMatchingTemplates(ProcessedNews news)
        {
            if (!_initialized) Initialize();
            
            var matches = new List<EventTemplate>();
            
            // Map ProcessedNews EventType to PoliticalCategory
            PoliticalCategory? category = MapEventTypeToCategory(news.EventType);
            
            if (category.HasValue)
            {
                var categoryTemplates = GetTemplatesForCategory(category.Value);
                
                foreach (var template in categoryTemplates)
                {
                    if (MatchesTemplate(news, template))
                    {
                        matches.Add(template);
                    }
                }
            }
            
            // Score and sort by relevance
            matches = matches
                .OrderByDescending(t => ScoreTemplateMatch(news, t))
                .ToList();
            
            return matches;
        }
        
        private static PoliticalCategory? MapEventTypeToCategory(EventType eventType)
        {
            return eventType switch
            {
                EventType.Legislation => PoliticalCategory.DomesticLegislation,
                EventType.Election => PoliticalCategory.ElectionCampaign,
                EventType.Scandal => PoliticalCategory.PoliticalScandal,
                EventType.Economic => PoliticalCategory.EconomicPolicy,
                EventType.International => PoliticalCategory.ForeignPolicy,
                EventType.Crisis => PoliticalCategory.HealthcarePolicy, // Could be multiple
                EventType.SocialUnrest => PoliticalCategory.CivilRights,
                EventType.CampaignEvent => PoliticalCategory.ElectionCampaign,
                EventType.PolicyAnnouncement => PoliticalCategory.DomesticLegislation,
                _ => null
            };
        }
        
        private static bool MatchesTemplate(ProcessedNews news, EventTemplate template)
        {
            // Check minimum relevance (using PoliticalRelevance as ImpactScore)
            if (news.PoliticalRelevance < template.MinImpactScore * 10f)
                return false;
            
            // Check minimum controversy
            if (news.ControversyScore < template.MinControversy * 100f)
                return false;
            
            // Check required entities
            if (template.RequiredEntities != null && template.RequiredEntities.Count > 0)
            {
                bool hasRequired = false;
                foreach (var reqEntity in template.RequiredEntities)
                {
                    if (HasEntityType(news, reqEntity))
                    {
                        hasRequired = true;
                        break;
                    }
                }
                if (!hasRequired) return false;
            }
            
            // Check trigger keywords
            if (template.TriggerKeywords != null && template.TriggerKeywords.Count > 0)
            {
                string text = (news.OriginalArticle.Title + " " + news.OriginalArticle.Description).ToLower();
                bool hasKeyword = template.TriggerKeywords.Any(kw => text.Contains(kw.ToLower()));
                if (!hasKeyword) return false;
            }
            
            return true;
        }
        
        private static bool HasEntityType(ProcessedNews news, TemplateEntityType entityType)
        {
            if (news.Entities == null) return false;
            
            return entityType switch
            {
                TemplateEntityType.Person => news.Entities.Any(e => e.Type == EntityType.Person),
                TemplateEntityType.Organization => news.Entities.Any(e => e.Type == EntityType.Organization),
                TemplateEntityType.Location => news.Entities.Any(e => e.Type == EntityType.Location),
                TemplateEntityType.Legislation => news.IssueCategories != null && news.IssueCategories.Count > 0,
                _ => false
            };
        }
        
        private static float ScoreTemplateMatch(ProcessedNews news, EventTemplate template)
        {
            float score = 0f;
            
            // Base score from relevance
            score += news.PoliticalRelevance * 0.5f;
            
            // Bonus for controversy match
            if (news.ControversyScore >= template.MinControversy * 100f)
            {
                score += (news.ControversyScore - template.MinControversy * 100f) * 0.3f;
            }
            
            // Bonus for keyword matches
            if (template.TriggerKeywords != null)
            {
                string text = (news.OriginalArticle.Title + " " + news.OriginalArticle.Description).ToLower();
                int keywordMatches = template.TriggerKeywords.Count(kw => text.Contains(kw.ToLower()));
                score += keywordMatches * 10f;
            }
            
            return score;
        }
        
        #region Template Definitions
        
        // ═══════════════════════════════════════════════════════════════════════
        // DOMESTIC LEGISLATION (5 templates)
        // ═══════════════════════════════════════════════════════════════════════
        
        private static void LoadDomesticLegislationTemplates()
        {
            var category = PoliticalCategory.DomesticLegislation;
            
            _templates[category].Add(new EventTemplate
            {
                TemplateId = "LEG_001",
                Category = category,
                DefaultEventType = TemplateEventType.PolicyPressure,
                DefaultUrgency = UrgencyLevel.Developing,
                HeadlineTemplate = "{legislative_body} Passes {bill_name}",
                DescriptionTemplate = "The {legislative_body} has passed {bill_name}, which will {primary_effect}. As {player_office}, constituents expect you to take a position on this landmark legislation.",
                ContextTemplate = "This legislation has been debated for {duration} and passed with a {vote_margin} vote. {party_stance}",
                Variables = new List<VariableMapping>
                {
                    new VariableMapping { VariableName = "legislative_body", SourcePath = "entities.organizations[type=govt][0]", FallbackValue = "Congress", Required = true },
                    new VariableMapping { VariableName = "bill_name", SourcePath = "entities.legislation[0].name", FallbackValue = "the new bill", Required = true },
                    new VariableMapping { VariableName = "primary_effect", SourcePath = "content.summary.effect", FallbackValue = "significantly impact policy", Required = false },
                    new VariableMapping { VariableName = "duration", SourcePath = "content.timeline", FallbackValue = "months", Required = false },
                    new VariableMapping { VariableName = "vote_margin", SourcePath = "content.vote_count", FallbackValue = "close", Required = false },
                    new VariableMapping { VariableName = "party_stance", SourcePath = "context.party_position", FallbackValue = "", Required = false }
                },
                MinImpactScore = 5.0f,
                MinControversy = 0.3f,
                RequiredEntities = new List<TemplateEntityType> { TemplateEntityType.Legislation },
                TriggerKeywords = new List<string> { "passes", "passed", "bill", "legislation", "law", "act", "vote" },
                Scaling = new OfficeScaling { Tier1 = 0.2f, Tier2 = 0.4f, Tier3 = 0.7f, Tier4 = 1.0f, Tier5 = 1.5f },
                Effects = new BaseEffects
                {
                    TrustDelta = new EffectRange(-5f, 10f),
                    CapitalDelta = new EffectRange(-3f, 5f),
                    MediaDelta = new EffectRange(5f, 15f)
                },
                Tags = new[] { "legislation", "policy", "voting" }
            });
            
            _templates[category].Add(new EventTemplate
            {
                TemplateId = "LEG_002",
                Category = category,
                DefaultEventType = TemplateEventType.PolicyPressure,
                DefaultUrgency = UrgencyLevel.Urgent,
                HeadlineTemplate = "Controversial {bill_type} Bill Heads to Vote",
                DescriptionTemplate = "A highly contentious {bill_type} bill is heading to a final vote. The legislation would {primary_effect}, dividing lawmakers along party lines. Your position could define your political identity.",
                ContextTemplate = "Supporters argue {pro_argument}. Critics counter that {con_argument}.",
                Variables = new List<VariableMapping>
                {
                    new VariableMapping { VariableName = "bill_type", SourcePath = "content.category", FallbackValue = "reform", Required = true },
                    new VariableMapping { VariableName = "primary_effect", SourcePath = "content.summary.effect", FallbackValue = "reshape current policy", Required = true },
                    new VariableMapping { VariableName = "pro_argument", SourcePath = "content.arguments.pro", FallbackValue = "this is necessary reform", Required = false },
                    new VariableMapping { VariableName = "con_argument", SourcePath = "content.arguments.con", FallbackValue = "the costs outweigh benefits", Required = false }
                },
                MinImpactScore = 6.0f,
                MinControversy = 0.5f,
                RequiredEntities = new List<TemplateEntityType> { TemplateEntityType.Legislation },
                TriggerKeywords = new List<string> { "controversial", "contentious", "divided", "partisan", "vote", "bill" },
                Scaling = new OfficeScaling { Tier1 = 0.3f, Tier2 = 0.5f, Tier3 = 0.8f, Tier4 = 1.2f, Tier5 = 1.8f },
                Effects = new BaseEffects
                {
                    TrustDelta = new EffectRange(-10f, 15f),
                    CapitalDelta = new EffectRange(-5f, 8f),
                    MediaDelta = new EffectRange(10f, 25f)
                },
                Tags = new[] { "controversial", "partisan", "high-stakes" }
            });
            
            _templates[category].Add(new EventTemplate
            {
                TemplateId = "LEG_003",
                Category = category,
                DefaultEventType = TemplateEventType.Crisis,
                DefaultUrgency = UrgencyLevel.Breaking,
                HeadlineTemplate = "Government Shutdown Looms as Budget Talks Collapse",
                DescriptionTemplate = "Budget negotiations have broken down, with a government shutdown now {time_remaining}. Essential services hang in the balance as lawmakers trade blame. The public demands leadership.",
                ContextTemplate = "The main sticking points are {dispute_issues}. Your party {party_position}.",
                Variables = new List<VariableMapping>
                {
                    new VariableMapping { VariableName = "time_remaining", SourcePath = "content.deadline", FallbackValue = "imminent", Required = true },
                    new VariableMapping { VariableName = "dispute_issues", SourcePath = "content.issues", FallbackValue = "spending levels and priorities", Required = false },
                    new VariableMapping { VariableName = "party_position", SourcePath = "context.party_stance", FallbackValue = "is under pressure to compromise", Required = false }
                },
                MinImpactScore = 8.0f,
                MinControversy = 0.7f,
                RequiredEntities = new List<TemplateEntityType> { TemplateEntityType.Organization },
                TriggerKeywords = new List<string> { "shutdown", "budget", "deadline", "collapse", "stalemate", "impasse" },
                Scaling = new OfficeScaling { Tier1 = 0.1f, Tier2 = 0.3f, Tier3 = 0.6f, Tier4 = 1.0f, Tier5 = 2.0f },
                Effects = new BaseEffects
                {
                    TrustDelta = new EffectRange(-20f, 15f),
                    CapitalDelta = new EffectRange(-10f, 10f),
                    MediaDelta = new EffectRange(20f, 40f)
                },
                Tags = new[] { "crisis", "budget", "shutdown", "urgent" }
            });
            
            _templates[category].Add(new EventTemplate
            {
                TemplateId = "LEG_004",
                Category = category,
                DefaultEventType = TemplateEventType.Opportunity,
                DefaultUrgency = UrgencyLevel.Developing,
                HeadlineTemplate = "Bipartisan {policy_area} Reform Gains Momentum",
                DescriptionTemplate = "A rare bipartisan coalition has formed around {policy_area} reform. Lawmakers from both parties are seeking co-sponsors for legislation that would {primary_effect}. This could be your chance to build cross-aisle credibility.",
                ContextTemplate = "Key supporters include {supporters}. The window for joining this coalition is limited.",
                Variables = new List<VariableMapping>
                {
                    new VariableMapping { VariableName = "policy_area", SourcePath = "content.category", FallbackValue = "infrastructure", Required = true },
                    new VariableMapping { VariableName = "primary_effect", SourcePath = "content.summary.effect", FallbackValue = "modernize current systems", Required = true },
                    new VariableMapping { VariableName = "supporters", SourcePath = "entities.people", FallbackValue = "senior members of both parties", Required = false }
                },
                MinImpactScore = 5.0f,
                MinControversy = 0.2f,
                RequiredEntities = new List<TemplateEntityType> { TemplateEntityType.Person },
                TriggerKeywords = new List<string> { "bipartisan", "coalition", "reform", "compromise", "agreement", "unity" },
                Scaling = new OfficeScaling { Tier1 = 0.4f, Tier2 = 0.6f, Tier3 = 0.9f, Tier4 = 1.1f, Tier5 = 1.3f },
                Effects = new BaseEffects
                {
                    TrustDelta = new EffectRange(5f, 15f),
                    CapitalDelta = new EffectRange(3f, 10f),
                    MediaDelta = new EffectRange(5f, 15f),
                    PartyLoyaltyDelta = new EffectRange(-5f, 5f)
                },
                Tags = new[] { "opportunity", "bipartisan", "coalition" }
            });
            
            _templates[category].Add(new EventTemplate
            {
                TemplateId = "LEG_005",
                Category = category,
                DefaultEventType = TemplateEventType.PolicyPressure,
                DefaultUrgency = UrgencyLevel.Urgent,
                HeadlineTemplate = "Supreme Court Strikes Down {law_name}",
                DescriptionTemplate = "In a landmark ruling, the Supreme Court has struck down {law_name}, declaring it {ruling_basis}. The decision has immediate implications for {affected_area}. Politicians across the spectrum are being pressed for reactions.",
                ContextTemplate = "The {vote_split} decision overturns {precedent}. {impact_statement}",
                Variables = new List<VariableMapping>
                {
                    new VariableMapping { VariableName = "law_name", SourcePath = "entities.legislation[0].name", FallbackValue = "the contested law", Required = true },
                    new VariableMapping { VariableName = "ruling_basis", SourcePath = "content.ruling_reason", FallbackValue = "unconstitutional", Required = true },
                    new VariableMapping { VariableName = "affected_area", SourcePath = "content.impact_area", FallbackValue = "millions of Americans", Required = false },
                    new VariableMapping { VariableName = "vote_split", SourcePath = "content.vote_count", FallbackValue = "divided", Required = false },
                    new VariableMapping { VariableName = "precedent", SourcePath = "content.precedent", FallbackValue = "established precedent", Required = false },
                    new VariableMapping { VariableName = "impact_statement", SourcePath = "content.summary.impact", FallbackValue = "", Required = false }
                },
                MinImpactScore = 7.0f,
                MinControversy = 0.6f,
                RequiredEntities = new List<TemplateEntityType> { TemplateEntityType.Organization, TemplateEntityType.Legislation },
                TriggerKeywords = new List<string> { "supreme court", "ruling", "struck down", "unconstitutional", "landmark", "overturn" },
                Scaling = new OfficeScaling { Tier1 = 0.2f, Tier2 = 0.4f, Tier3 = 0.7f, Tier4 = 1.0f, Tier5 = 1.5f },
                Effects = new BaseEffects
                {
                    TrustDelta = new EffectRange(-15f, 15f),
                    CapitalDelta = new EffectRange(-5f, 5f),
                    MediaDelta = new EffectRange(15f, 30f)
                },
                Tags = new[] { "judicial", "constitutional", "landmark" }
            });
        }
        
        // Continue with remaining categories - using helper for abbreviated ones
        // Full implementations would follow the same pattern as above
        
        private static void LoadElectionCampaignTemplates()
        {
            var category = PoliticalCategory.ElectionCampaign;
            // Add full templates similar to LEG_001 pattern
            _templates[category].Add(CreateGenericTemplate("ELEC_001", category, "{candidate_name} Surges in Polls After {event_type}", TemplateEventType.PolicyPressure));
            _templates[category].Add(CreateGenericTemplate("ELEC_002", category, "Major Endorsement Up for Grabs as {endorser} Remains Uncommitted", TemplateEventType.Opportunity));
            _templates[category].Add(CreateGenericTemplate("ELEC_003", category, "Debate Moment Goes Viral: {debate_moment}", TemplateEventType.PolicyPressure));
            _templates[category].Add(CreateGenericTemplate("ELEC_004", category, "Campaign Finance Irregularities Alleged Against {candidate_name}", TemplateEventType.ScandalTrigger));
            _templates[category].Add(CreateGenericTemplate("ELEC_005", category, "Election Security Concerns Raised in {location}", TemplateEventType.Crisis));
        }
        
        private static void LoadPoliticalScandalTemplates()
        {
            var category = PoliticalCategory.PoliticalScandal;
            _templates[category].Add(CreateGenericTemplate("SCAN_001", category, "{politician_name} Faces Resignation Calls Over {scandal_type}", TemplateEventType.ScandalTrigger));
            _templates[category].Add(CreateGenericTemplate("SCAN_002", category, "Investigation Launched Into {target_entity} Over {investigation_subject}", TemplateEventType.PolicyPressure));
            _templates[category].Add(CreateGenericTemplate("SCAN_003", category, "Leaked {document_type} Reveals {revelation}", TemplateEventType.ScandalTrigger));
            _templates[category].Add(CreateGenericTemplate("SCAN_004", category, "{politician_name} Accused of {ethics_violation} by Ethics Watchdog", TemplateEventType.PolicyPressure));
            _templates[category].Add(CreateGenericTemplate("SCAN_005", category, "{politician_name} Indicted on {charge_count} Counts of {charges}", TemplateEventType.Crisis));
        }
        
        private static void LoadEconomicPolicyTemplates()
        {
            var category = PoliticalCategory.EconomicPolicy;
            _templates[category].Add(CreateGenericTemplate("ECON_001", category, "Markets Plunge as {economic_trigger} Sparks Fear", TemplateEventType.Crisis));
            _templates[category].Add(CreateGenericTemplate("ECON_002", category, "Unemployment Hits {unemployment_level} as {sector} Sector {direction}", TemplateEventType.PolicyPressure));
            _templates[category].Add(CreateGenericTemplate("ECON_003", category, "{fed_action} Announced: {rate_change}", TemplateEventType.PolicyPressure));
            _templates[category].Add(CreateGenericTemplate("ECON_004", category, "{company_name} Announces {announcement_type} in {location}", TemplateEventType.Opportunity));
            _templates[category].Add(CreateGenericTemplate("ECON_005", category, "Inflation Hits {inflation_rate} as {cause} Drives Prices Higher", TemplateEventType.PolicyPressure));
        }
        
        private static void LoadForeignPolicyTemplates()
        {
            var category = PoliticalCategory.ForeignPolicy;
            _templates[category].Add(CreateGenericTemplate("FRGN_001", category, "International Crisis: {crisis_description} in {region}", TemplateEventType.Crisis));
            _templates[category].Add(CreateGenericTemplate("FRGN_002", category, "Trade Deal {status} Between US and {country}", TemplateEventType.PolicyPressure));
            _templates[category].Add(CreateGenericTemplate("FRGN_003", category, "{leader_name} Visit Sparks {reaction}", TemplateEventType.Opportunity));
            _templates[category].Add(CreateGenericTemplate("FRGN_004", category, "Sanctions Announced Against {target_country}", TemplateEventType.PolicyPressure));
            _templates[category].Add(CreateGenericTemplate("FRGN_005", category, "Embassy {incident_type} in {location}", TemplateEventType.Crisis));
        }
        
        private static void LoadHealthcarePolicyTemplates()
        {
            var category = PoliticalCategory.HealthcarePolicy;
            _templates[category].Add(CreateGenericTemplate("HLTH_001", category, "Health Emergency: {disease} Outbreak Reported in {location}", TemplateEventType.Crisis));
            _templates[category].Add(CreateGenericTemplate("HLTH_002", category, "Drug Pricing Reform Advances in {body}", TemplateEventType.PolicyPressure));
            _templates[category].Add(CreateGenericTemplate("HLTH_003", category, "Insurance Coverage Changes Affect {affected_group}", TemplateEventType.PolicyPressure));
            _templates[category].Add(CreateGenericTemplate("HLTH_004", category, "Hospital Closure Crisis in {location}", TemplateEventType.Crisis));
            _templates[category].Add(CreateGenericTemplate("HLTH_005", category, "Mental Health Funding {action_type} Proposed", TemplateEventType.Opportunity));
        }
        
        private static void LoadImmigrationTemplates()
        {
            var category = PoliticalCategory.Immigration;
            _templates[category].Add(CreateGenericTemplate("IMMG_001", category, "Border {situation_type} Intensifies", TemplateEventType.Crisis));
            _templates[category].Add(CreateGenericTemplate("IMMG_002", category, "{action_type} Immigration Policy Announced", TemplateEventType.PolicyPressure));
            _templates[category].Add(CreateGenericTemplate("IMMG_003", category, "Deportation Case of {person_description} Draws Attention", TemplateEventType.PolicyPressure));
            _templates[category].Add(CreateGenericTemplate("IMMG_004", category, "Refugee Program {status} Amid Debate", TemplateEventType.PolicyPressure));
            _templates[category].Add(CreateGenericTemplate("IMMG_005", category, "Visa Policy Changes Impact {affected_group}", TemplateEventType.PolicyPressure));
        }
        
        private static void LoadClimateEnvironmentTemplates()
        {
            var category = PoliticalCategory.ClimateEnvironment;
            _templates[category].Add(CreateGenericTemplate("CLIM_001", category, "Extreme Weather: {disaster_type} Devastates {location}", TemplateEventType.Crisis));
            _templates[category].Add(CreateGenericTemplate("CLIM_002", category, "Climate Legislation {status} in Congress", TemplateEventType.PolicyPressure));
            _templates[category].Add(CreateGenericTemplate("CLIM_003", category, "EPA Announces {regulation_type} Regulations", TemplateEventType.PolicyPressure));
            _templates[category].Add(CreateGenericTemplate("CLIM_004", category, "Environmental Contamination Discovered in {location}", TemplateEventType.Crisis));
            _templates[category].Add(CreateGenericTemplate("CLIM_005", category, "Clean Energy Investment Opportunity in {sector}", TemplateEventType.Opportunity));
        }
        
        private static void LoadCrimeJusticeTemplates()
        {
            var category = PoliticalCategory.CrimeJustice;
            _templates[category].Add(CreateGenericTemplate("CRIM_001", category, "High-Profile {crime_type} Case Sparks Debate", TemplateEventType.PolicyPressure));
            _templates[category].Add(CreateGenericTemplate("CRIM_002", category, "Police Reform Legislation {status}", TemplateEventType.PolicyPressure));
            _templates[category].Add(CreateGenericTemplate("CRIM_003", category, "Crime Rate {direction} in {location}", TemplateEventType.PolicyPressure));
            _templates[category].Add(CreateGenericTemplate("CRIM_004", category, "Prison Reform Initiative Gains {momentum}", TemplateEventType.Opportunity));
            _templates[category].Add(CreateGenericTemplate("CRIM_005", category, "Mass {incident_type} in {location}", TemplateEventType.Crisis));
        }
        
        private static void LoadCivilRightsTemplates()
        {
            var category = PoliticalCategory.CivilRights;
            _templates[category].Add(CreateGenericTemplate("CIVR_001", category, "Discrimination Lawsuit Against {target} Advances", TemplateEventType.PolicyPressure));
            _templates[category].Add(CreateGenericTemplate("CIVR_002", category, "Voting Rights {action_type} in {location}", TemplateEventType.PolicyPressure));
            _templates[category].Add(CreateGenericTemplate("CIVR_003", category, "Protests Erupt Over {cause}", TemplateEventType.Crisis));
            _templates[category].Add(CreateGenericTemplate("CIVR_004", category, "{rights_type} Rights Legislation Proposed", TemplateEventType.PolicyPressure));
            _templates[category].Add(CreateGenericTemplate("CIVR_005", category, "Historic {milestone_type} Achievement Celebrated", TemplateEventType.Opportunity));
        }
        
        private static void LoadEducationTemplates()
        {
            var category = PoliticalCategory.Education;
            _templates[category].Add(CreateGenericTemplate("EDUC_001", category, "School Funding {crisis_type} in {location}", TemplateEventType.Crisis));
            _templates[category].Add(CreateGenericTemplate("EDUC_002", category, "Curriculum Controversy Over {subject}", TemplateEventType.PolicyPressure));
            _templates[category].Add(CreateGenericTemplate("EDUC_003", category, "Teacher {action_type} Threatens Schools", TemplateEventType.Crisis));
            _templates[category].Add(CreateGenericTemplate("EDUC_004", category, "Higher Education {policy_type} Debated", TemplateEventType.PolicyPressure));
            _templates[category].Add(CreateGenericTemplate("EDUC_005", category, "Education Innovation Grant Available", TemplateEventType.Opportunity));
        }
        
        private static void LoadMilitaryDefenseTemplates()
        {
            var category = PoliticalCategory.MilitaryDefense;
            _templates[category].Add(CreateGenericTemplate("MILT_001", category, "Military {action_type} Ordered by President", TemplateEventType.Crisis));
            _templates[category].Add(CreateGenericTemplate("MILT_002", category, "Defense Budget {status} Debated", TemplateEventType.PolicyPressure));
            _templates[category].Add(CreateGenericTemplate("MILT_003", category, "Veterans {issue_type} Crisis Exposed", TemplateEventType.Crisis));
            _templates[category].Add(CreateGenericTemplate("MILT_004", category, "Military Base {decision_type} in {location}", TemplateEventType.PolicyPressure));
            _templates[category].Add(CreateGenericTemplate("MILT_005", category, "Defense Contract Award Opportunity", TemplateEventType.Opportunity));
        }
        
        private static void LoadTechnologyPolicyTemplates()
        {
            var category = PoliticalCategory.TechnologyPolicy;
            _templates[category].Add(CreateGenericTemplate("TECH_001", category, "Big Tech Antitrust {action_type} Announced", TemplateEventType.PolicyPressure));
            _templates[category].Add(CreateGenericTemplate("TECH_002", category, "Data Privacy {legislation_type} Advances", TemplateEventType.PolicyPressure));
            _templates[category].Add(CreateGenericTemplate("TECH_003", category, "AI Regulation Debate Intensifies", TemplateEventType.PolicyPressure));
            _templates[category].Add(CreateGenericTemplate("TECH_004", category, "Cybersecurity {incident_type} Affects {target}", TemplateEventType.Crisis));
            _templates[category].Add(CreateGenericTemplate("TECH_005", category, "Tech Innovation Hub Coming to {location}", TemplateEventType.Opportunity));
        }
        
        private static void LoadSocialIssuesTemplates()
        {
            var category = PoliticalCategory.SocialIssues;
            _templates[category].Add(CreateGenericTemplate("SOCL_001", category, "{social_issue} Debate Reignites Nationally", TemplateEventType.PolicyPressure));
            _templates[category].Add(CreateGenericTemplate("SOCL_002", category, "Supreme Court to Hear {case_type} Case", TemplateEventType.PolicyPressure));
            _templates[category].Add(CreateGenericTemplate("SOCL_003", category, "Religious Freedom vs. {rights_type} Clash", TemplateEventType.PolicyPressure));
            _templates[category].Add(CreateGenericTemplate("SOCL_004", category, "Social Movement Gains {momentum} Nationally", TemplateEventType.Opportunity));
            _templates[category].Add(CreateGenericTemplate("SOCL_005", category, "Community {crisis_type} Divides {location}", TemplateEventType.Crisis));
        }
        
        private static void LoadPartyPoliticsTemplates()
        {
            var category = PoliticalCategory.PartyPolitics;
            _templates[category].Add(CreateGenericTemplate("PRTY_001", category, "Party Leadership {battle_type} Erupts", TemplateEventType.PolicyPressure));
            _templates[category].Add(CreateGenericTemplate("PRTY_002", category, "Faction {action_type} Threatens Party Unity", TemplateEventType.Crisis));
            _templates[category].Add(CreateGenericTemplate("PRTY_003", category, "Party Platform {change_type} Proposed", TemplateEventType.PolicyPressure));
            _templates[category].Add(CreateGenericTemplate("PRTY_004", category, "Major Donor {action_type} Shakes Party", TemplateEventType.Crisis));
            _templates[category].Add(CreateGenericTemplate("PRTY_005", category, "Rising Star {politician_name} Gains Influence", TemplateEventType.Opportunity));
        }
        
        private static void LoadLocalGovernmentTemplates()
        {
            var category = PoliticalCategory.LocalGovernment;
            _templates[category].Add(CreateGenericTemplate("LOCL_001", category, "City Council {battle_type} Over {issue}", TemplateEventType.PolicyPressure));
            _templates[category].Add(CreateGenericTemplate("LOCL_002", category, "Local {service_type} Crisis Worsens", TemplateEventType.Crisis));
            _templates[category].Add(CreateGenericTemplate("LOCL_003", category, "Zoning Dispute {status} in {neighborhood}", TemplateEventType.PolicyPressure));
            _templates[category].Add(CreateGenericTemplate("LOCL_004", category, "Municipal {election_type} Draws Attention", TemplateEventType.Opportunity));
            _templates[category].Add(CreateGenericTemplate("LOCL_005", category, "Community Development Grant Available", TemplateEventType.Opportunity));
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// Creates a generic template for categories with abbreviated definitions.
        /// In production, these would be fully fleshed out like the detailed examples.
        /// </summary>
        private static EventTemplate CreateGenericTemplate(string id, PoliticalCategory category, string headline, TemplateEventType eventType)
        {
            return new EventTemplate
            {
                TemplateId = id,
                Category = category,
                DefaultEventType = eventType,
                DefaultUrgency = eventType == TemplateEventType.Crisis ? UrgencyLevel.Breaking : UrgencyLevel.Developing,
                HeadlineTemplate = headline,
                DescriptionTemplate = $"A significant development regarding {headline.ToLower()}. Political leaders are expected to respond.",
                ContextTemplate = "This development has implications for policy and public opinion.",
                Variables = new List<VariableMapping>(),
                MinImpactScore = 5.0f,
                MinControversy = 0.4f,
                RequiredEntities = new List<TemplateEntityType>(),
                TriggerKeywords = new List<string>(headline.ToLower().Split(' ')),
                Scaling = new OfficeScaling(),
                Effects = new BaseEffects
                {
                    TrustDelta = new EffectRange(-10f, 10f),
                    CapitalDelta = new EffectRange(-5f, 5f),
                    MediaDelta = new EffectRange(10f, 25f)
                },
                Tags = new[] { category.ToString().ToLower() }
            };
        }
        
        #endregion
    }
}

