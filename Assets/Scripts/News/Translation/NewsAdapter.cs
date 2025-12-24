using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.News.Templates;

namespace ElectionEmpire.News.Translation
{
    /// <summary>
    /// Adapter to convert ProcessedNews (old format) to ProcessedNewsItem (new format)
    /// </summary>
    public static class NewsAdapter
    {
        /// <summary>
        /// Convert ProcessedNews to ProcessedNewsItem
        /// </summary>
        public static ProcessedNewsItem ConvertToProcessedNewsItem(ProcessedNews processedNews)
        {
            var item = new ProcessedNewsItem
            {
                SourceId = processedNews.OriginalArticle?.Title?.GetHashCode().ToString() ?? Guid.NewGuid().ToString(),
                Headline = processedNews.OriginalArticle?.Title ?? "Unknown",
                Summary = processedNews.OriginalArticle?.Description ?? "",
                FullText = processedNews.OriginalArticle?.Content ?? processedNews.OriginalArticle?.Description ?? "",
                PublishedAt = processedNews.OriginalArticle?.PublishedDate ?? DateTime.Now,
                FetchedAt = processedNews.ProcessedDate,
                SourceUrl = processedNews.OriginalArticle?.URL ?? "",
                SourceBias = 0f, // Neutral by default
                SourceCredibility = GetSourceCredibility(processedNews.OriginalArticle?.Source ?? ""),
                
                PrimaryCategory = MapEventTypeToPoliticalCategory(processedNews.EventType),
                SecondaryCategories = new List<PoliticalCategory>(),
                CategoryConfidence = processedNews.PoliticalRelevance / 100f,
                
                Entities = ConvertEntities(processedNews.Entities, processedNews.OriginalArticle),
                
                OverallSentiment = processedNews.Sentiment?.Net ?? 0f,
                EntitySentiments = new Dictionary<string, float>(),
                ControversyScore = processedNews.ControversyScore / 100f,
                PartisanReactions = new PartisanPredictions
                {
                    Left = processedNews.Sentiment?.Net < 0 ? Mathf.Abs(processedNews.Sentiment.Net) : 0f,
                    Center = 50f,
                    Right = processedNews.Sentiment?.Net > 0 ? processedNews.Sentiment.Net : 0f
                },
                
                PoliticalRelevance = processedNews.PoliticalRelevance / 100f,
                ImpactScore = CalculateImpactScore(processedNews),
                TemporalClassification = MapTemporalClass(processedNews)
            };
            
            // Map issue categories to secondary categories
            if (processedNews.IssueCategories != null)
            {
                foreach (var issue in processedNews.IssueCategories)
                {
                    var category = MapIssueToPoliticalCategory(issue);
                    if (category != item.PrimaryCategory && !item.SecondaryCategories.Contains(category))
                    {
                        item.SecondaryCategories.Add(category);
                    }
                }
            }
            
            return item;
        }
        
        private static float GetSourceCredibility(string source)
        {
            string[] majorSources = {
                "Associated Press", "AP", "Reuters", "BBC", "CNN", 
                "New York Times", "Washington Post", "Wall Street Journal", "Politico"
            };
            
            if (majorSources.Any(s => source.Contains(s, StringComparison.OrdinalIgnoreCase)))
                return 1.0f;
            
            return 0.5f;
        }
        
        private static PoliticalCategory MapEventTypeToPoliticalCategory(EventType eventType)
        {
            return eventType switch
            {
                EventType.Legislation => PoliticalCategory.DomesticLegislation,
                EventType.Election => PoliticalCategory.ElectionCampaign,
                EventType.Scandal => PoliticalCategory.PoliticalScandal,
                EventType.Economic => PoliticalCategory.EconomicPolicy,
                EventType.International => PoliticalCategory.ForeignPolicy,
                EventType.Crisis => PoliticalCategory.HealthcarePolicy, // Default, could be multiple
                EventType.SocialUnrest => PoliticalCategory.CivilRights,
                EventType.CampaignEvent => PoliticalCategory.ElectionCampaign,
                EventType.PolicyAnnouncement => PoliticalCategory.DomesticLegislation,
                _ => PoliticalCategory.DomesticLegislation
            };
        }
        
        private static PoliticalCategory MapIssueToPoliticalCategory(IssueCategory issue)
        {
            return issue switch
            {
                IssueCategory.Healthcare => PoliticalCategory.HealthcarePolicy,
                IssueCategory.Economy => PoliticalCategory.EconomicPolicy,
                IssueCategory.Education => PoliticalCategory.Education,
                IssueCategory.Immigration => PoliticalCategory.Immigration,
                IssueCategory.Environment => PoliticalCategory.ClimateEnvironment,
                IssueCategory.Crime => PoliticalCategory.CrimeJustice,
                IssueCategory.SocialIssues => PoliticalCategory.SocialIssues,
                _ => PoliticalCategory.DomesticLegislation
            };
        }
        
        private static ExtractedEntities ConvertEntities(List<Entity> entities, NewsArticle article)
        {
            var extracted = new ExtractedEntities();
            
            if (entities != null)
            {
                foreach (var entity in entities)
                {
                    var extractedEntity = new ExtractedEntity
                    {
                        Name = entity.Name,
                        Type = MapEntityType(entity.Type),
                        SubType = DetermineSubType(entity, article),
                        Relevance = entity.Relevance,
                        Role = DetermineRole(entity, article),
                        Attributes = new Dictionary<string, string>()
                    };
                    
                    switch (extractedEntity.Type)
                    {
                        case TemplateEntityType.Person:
                            extracted.People.Add(extractedEntity);
                            break;
                        case TemplateEntityType.Organization:
                            extracted.Organizations.Add(extractedEntity);
                            break;
                        case TemplateEntityType.Location:
                            extracted.Locations.Add(extractedEntity);
                            break;
                        case TemplateEntityType.Legislation:
                            extracted.Legislation.Add(extractedEntity);
                            break;
                        case TemplateEntityType.Event:
                            extracted.Events.Add(extractedEntity);
                            break;
                    }
                }
            }
            
            return extracted;
        }
        
        private static TemplateEntityType MapEntityType(EntityType entityType)
        {
            return entityType switch
            {
                EntityType.Person => TemplateEntityType.Person,
                EntityType.Organization => TemplateEntityType.Organization,
                EntityType.Location => TemplateEntityType.Location,
                EntityType.Legislation => TemplateEntityType.Legislation,
                _ => TemplateEntityType.Organization
            };
        }
        
        private static string DetermineSubType(Entity entity, NewsArticle article)
        {
            string name = entity.Name.ToLower();
            string text = (article?.Title + " " + article?.Description).ToLower();
            
            // Determine organization subtype
            if (entity.Type == EntityType.Organization)
            {
                if (name.Contains("congress") || name.Contains("senate") || name.Contains("house"))
                    return "govt";
                if (name.Contains("party") || name.Contains("democrat") || name.Contains("republican"))
                    return "party";
                if (name.Contains("company") || name.Contains("corp") || name.Contains("inc"))
                    return "company";
                if (name.Contains("court") || name.Contains("supreme"))
                    return "judicial";
            }
            
            return "";
        }
        
        private static string DetermineRole(Entity entity, NewsArticle article)
        {
            string text = (article?.Title + " " + article?.Description).ToLower();
            string name = entity.Name.ToLower();
            
            if (text.Contains("accused") || text.Contains("alleged"))
                return "accused";
            if (text.Contains("sponsor") || text.Contains("introduced"))
                return "bill_sponsor";
            if (text.Contains("announced") || text.Contains("said"))
                return "spokesperson";
            
            return "";
        }
        
        private static float CalculateImpactScore(ProcessedNews news)
        {
            float score = 1f;
            
            // Base from relevance
            score += news.PoliticalRelevance / 20f;
            
            // Controversy adds
            score += news.ControversyScore / 10f;
            
            // Sentiment extremes add
            if (news.Sentiment != null)
            {
                float absSentiment = Mathf.Abs(news.Sentiment.Net);
                score += absSentiment / 20f;
            }
            
            return Mathf.Clamp(score, 1f, 10f);
        }
        
        private static TemporalClass MapTemporalClass(ProcessedNews news)
        {
            var age = DateTime.Now - news.ProcessedDate;
            
            if (age.TotalHours < 6)
                return TemporalClass.Breaking;
            if (age.TotalHours < 24)
                return TemporalClass.Developing;
            if (age.TotalDays < 7)
                return TemporalClass.Ongoing;
            
            return TemporalClass.Historical;
        }
    }
}

