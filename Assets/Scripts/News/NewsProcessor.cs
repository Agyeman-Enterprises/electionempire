using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionEmpire.News
{
    /// <summary>
    /// Processes and classifies news articles for game integration
    /// </summary>
    public class NewsProcessor
    {
        private Dictionary<string, float> politicalKeywords;
        private Dictionary<string, IssueCategory> issueKeywords;
        
        public void Initialize()
        {
            // Keywords that indicate political relevance
            politicalKeywords = new Dictionary<string, float>
            {
                // High relevance
                ["president"] = 1.0f,
                ["congress"] = 1.0f,
                ["senate"] = 1.0f,
                ["election"] = 1.0f,
                ["campaign"] = 0.9f,
                ["vote"] = 0.9f,
                ["bill"] = 0.8f,
                ["legislation"] = 0.8f,
                ["governor"] = 0.8f,
                ["mayor"] = 0.7f,
                ["policy"] = 0.7f,
                ["scandal"] = 0.9f,
                ["investigation"] = 0.7f,
                
                // Medium relevance
                ["government"] = 0.6f,
                ["political"] = 0.6f,
                ["democrat"] = 0.6f,
                ["republican"] = 0.6f,
                ["partisan"] = 0.5f,
                
                // Issues
                ["healthcare"] = 0.5f,
                ["economy"] = 0.5f,
                ["immigration"] = 0.6f,
                ["climate"] = 0.5f,
                ["education"] = 0.4f,
                ["taxes"] = 0.6f
            };
            
            // Keywords mapped to game issues
            issueKeywords = new Dictionary<string, IssueCategory>
            {
                ["healthcare"] = IssueCategory.Healthcare,
                ["hospital"] = IssueCategory.Healthcare,
                ["medicare"] = IssueCategory.Healthcare,
                ["insurance"] = IssueCategory.Healthcare,
                ["health"] = IssueCategory.Healthcare,
                
                ["economy"] = IssueCategory.Economy,
                ["jobs"] = IssueCategory.Economy,
                ["unemployment"] = IssueCategory.Economy,
                ["inflation"] = IssueCategory.Economy,
                ["recession"] = IssueCategory.Economy,
                ["gdp"] = IssueCategory.Economy,
                
                ["taxes"] = IssueCategory.Taxes,
                ["irs"] = IssueCategory.Taxes,
                ["revenue"] = IssueCategory.Taxes,
                ["tax"] = IssueCategory.Taxes,
                
                ["crime"] = IssueCategory.Crime,
                ["police"] = IssueCategory.Crime,
                ["safety"] = IssueCategory.Crime,
                ["violence"] = IssueCategory.Crime,
                ["criminal"] = IssueCategory.Crime,
                
                ["education"] = IssueCategory.Education,
                ["school"] = IssueCategory.Education,
                ["teachers"] = IssueCategory.Education,
                ["college"] = IssueCategory.Education,
                ["university"] = IssueCategory.Education,
                
                ["immigration"] = IssueCategory.Immigration,
                ["border"] = IssueCategory.Immigration,
                ["migrants"] = IssueCategory.Immigration,
                ["immigrant"] = IssueCategory.Immigration,
                
                ["climate"] = IssueCategory.Environment,
                ["environment"] = IssueCategory.Environment,
                ["pollution"] = IssueCategory.Environment,
                ["energy"] = IssueCategory.Environment,
                ["green"] = IssueCategory.Environment,
                ["carbon"] = IssueCategory.Environment
            };
        }
        
        /// <summary>
        /// Process news article into game-usable data
        /// </summary>
        public ProcessedNews ProcessArticle(NewsArticle article)
        {
            var processed = new ProcessedNews
            {
                OriginalArticle = article,
                ProcessedDate = DateTime.Now
            };
            
            // 1. Calculate political relevance
            processed.PoliticalRelevance = CalculatePoliticalRelevance(article);
            
            // 2. Extract topics/issues
            processed.Topics = ExtractTopics(article);
            processed.IssueCategories = ExtractIssues(article);
            
            // 3. Entity recognition (people, organizations, places)
            processed.Entities = ExtractEntities(article);
            
            // 4. Sentiment analysis
            processed.Sentiment = AnalyzeSentiment(article);
            
            // 5. Controversy scoring
            processed.ControversyScore = CalculateControversy(article);
            
            // 6. Determine event type
            processed.EventType = ClassifyEventType(article);
            
            return processed;
        }
        
        private float CalculatePoliticalRelevance(NewsArticle article)
        {
            float score = 0f;
            string text = (article.Title + " " + article.Description).ToLower();
            
            foreach (var keyword in politicalKeywords)
            {
                if (text.Contains(keyword.Key))
                {
                    score += keyword.Value;
                }
            }
            
            // Normalize to 0-100
            score = Mathf.Min(100f, score * 10f);
            
            return score;
        }
        
        private List<string> ExtractTopics(NewsArticle article)
        {
            var topics = new List<string>();
            string text = (article.Title + " " + article.Description).ToLower();
            
            // Simple keyword extraction
            foreach (var keyword in politicalKeywords.Keys)
            {
                if (text.Contains(keyword))
                {
                    topics.Add(keyword);
                }
            }
            
            return topics.Distinct().ToList();
        }
        
        private List<IssueCategory> ExtractIssues(NewsArticle article)
        {
            var issues = new List<IssueCategory>();
            string text = (article.Title + " " + article.Description).ToLower();
            
            foreach (var keyword in issueKeywords)
            {
                if (text.Contains(keyword.Key))
                {
                    issues.Add(keyword.Value);
                }
            }
            
            return issues.Distinct().ToList();
        }
        
        private List<Entity> ExtractEntities(NewsArticle article)
        {
            var entities = new List<Entity>();
            string text = article.Title + " " + article.Description;
            
            // Simple named entity recognition
            // Look for capitalized words/phrases
            var words = text.Split(new[] { ' ', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            
            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i].Trim(new char[] { ',', '.', ':', ';', '!', '?', '(', ')', '[', ']' });
                
                if (string.IsNullOrEmpty(word) || word.Length < 2)
                    continue;
                
                // If capitalized (and not first word of sentence)
                if (char.IsUpper(word[0]) && !IsCommonWord(word))
                {
                    // Check if it's a person, organization, or place
                    EntityType type = ClassifyEntityType(word, words, i);
                    
                    entities.Add(new Entity
                    {
                        Name = word,
                        Type = type,
                        Relevance = 1.0f
                    });
                }
            }
            
            return entities;
        }
        
        private EntityType ClassifyEntityType(string word, string[] context, int index)
        {
            // Simple heuristics
            string[] titles = { "President", "Senator", "Governor", "Rep.", "Mayor", "Secretary" };
            string[] orgWords = { "Department", "Agency", "Committee", "Party", "Congress", "Senate" };
            
            if (titles.Any(t => word.Contains(t)))
                return EntityType.Person;
            
            if (orgWords.Any(o => word.Contains(o)))
                return EntityType.Organization;
            
            // Default to person if followed by action verb
            if (index < context.Length - 1)
            {
                string nextWord = context[index + 1].ToLower().Trim(new char[] { ',', '.', ':', ';' });
                if (new[] { "said", "announced", "promised", "criticized", "voted", "proposed" }.Contains(nextWord))
                    return EntityType.Person;
            }
            
            return EntityType.Organization;
        }
        
        private SentimentScore AnalyzeSentiment(NewsArticle article)
        {
            string text = (article.Title + " " + article.Description).ToLower();
            
            // Simple sentiment analysis via keyword matching
            float positiveScore = 0f;
            float negativeScore = 0f;
            
            string[] positiveWords = {
                "success", "victory", "win", "improve", "growth", "benefit",
                "support", "help", "progress", "agreement", "solution",
                "achievement", "breakthrough", "approval", "praise"
            };
            
            string[] negativeWords = {
                "crisis", "scandal", "fail", "corrupt", "problem", "threat",
                "decline", "attack", "controversy", "illegal", "investigation",
                "resign", "impeach", "fraud", "disaster", "criticism",
                "outrage", "protest", "violence", "death", "tragedy"
            };
            
            foreach (var word in positiveWords)
            {
                if (text.Contains(word))
                    positiveScore += 1f;
            }
            
            foreach (var word in negativeWords)
            {
                if (text.Contains(word))
                    negativeScore += 1f;
            }
            
            float netScore = positiveScore - negativeScore;
            
            return new SentimentScore
            {
                Positive = positiveScore,
                Negative = negativeScore,
                Net = netScore,
                Classification = ClassifySentiment(netScore)
            };
        }
        
        private SentimentType ClassifySentiment(float netScore)
        {
            if (netScore > 2f) return SentimentType.VeryPositive;
            if (netScore > 0f) return SentimentType.Positive;
            if (netScore < -2f) return SentimentType.VeryNegative;
            if (netScore < 0f) return SentimentType.Negative;
            return SentimentType.Neutral;
        }
        
        private float CalculateControversy(NewsArticle article)
        {
            string text = (article.Title + " " + article.Description).ToLower();
            
            float score = 0f;
            
            string[] controversyWords = {
                "scandal", "controversy", "outrage", "protest", "divided",
                "debate", "clash", "partisan", "polarizing", "backlash",
                "investigation", "allegations", "accused", "illegal",
                "conflict", "dispute", "criticism", "condemn"
            };
            
            foreach (var word in controversyWords)
            {
                if (text.Contains(word))
                    score += 10f;
            }
            
            return Mathf.Min(100f, score);
        }
        
        private EventType ClassifyEventType(NewsArticle article)
        {
            string text = (article.Title + " " + article.Description).ToLower();
            
            if (text.Contains("election") || text.Contains("vote") || text.Contains("poll"))
                return EventType.Election;
            
            if (text.Contains("bill") || text.Contains("legislation") || text.Contains("congress"))
                return EventType.Legislation;
            
            if (text.Contains("scandal") || text.Contains("investigation") || text.Contains("corrupt"))
                return EventType.Scandal;
            
            if (text.Contains("crisis") || text.Contains("emergency") || text.Contains("disaster"))
                return EventType.Crisis;
            
            if (text.Contains("protest") || text.Contains("riot") || text.Contains("unrest"))
                return EventType.SocialUnrest;
            
            if (text.Contains("debate") || text.Contains("speech") || text.Contains("rally"))
                return EventType.CampaignEvent;
            
            if (text.Contains("international") || text.Contains("foreign") || text.Contains("diplomacy"))
                return EventType.International;
            
            if (text.Contains("economy") || text.Contains("market") || text.Contains("trade"))
                return EventType.Economic;
            
            return EventType.PolicyAnnouncement;
        }
        
        private bool IsCommonWord(string word)
        {
            string[] common = {
                "The", "A", "An", "In", "On", "At", "To", "For",
                "With", "By", "From", "About", "As", "Into", "Through",
                "This", "That", "These", "Those", "Is", "Are", "Was", "Were"
            };
            
            return common.Contains(word);
        }
    }
    
    [Serializable]
    public class ProcessedNews
    {
        public NewsArticle OriginalArticle;
        public DateTime ProcessedDate;
        
        public float PoliticalRelevance;        // 0-100
        public List<string> Topics;
        public List<IssueCategory> IssueCategories;
        public List<Entity> Entities;
        public SentimentScore Sentiment;
        public float ControversyScore;          // 0-100
        public EventType EventType;
        
        public ProcessedNews()
        {
            Topics = new List<string>();
            IssueCategories = new List<IssueCategory>();
            Entities = new List<Entity>();
        }
    }
    
    [Serializable]
    public class Entity
    {
        public string Name;
        public EntityType Type;
        public float Relevance;
    }
    
    public enum EntityType
    {
        Person,
        Organization,
        Location,
        Legislation
    }
    
    [Serializable]
    public class SentimentScore
    {
        public float Positive;
        public float Negative;
        public float Net;
        public SentimentType Classification;
    }
    
    public enum SentimentType
    {
        VeryPositive,
        Positive,
        Neutral,
        Negative,
        VeryNegative
    }
    
    public enum EventType
    {
        Election,
        Legislation,
        Scandal,
        Crisis,
        SocialUnrest,
        CampaignEvent,
        PolicyAnnouncement,
        International,
        Economic
    }
    
    public enum IssueCategory
    {
        Healthcare,
        Economy,
        Taxes,
        Crime,
        Education,
        Immigration,
        Environment,
        Infrastructure,
        ForeignPolicy,
        Defense,
        SocialIssues,
        Justice,
        General
    }
}

