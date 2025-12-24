using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.World;
using ElectionEmpire.Gameplay;

namespace ElectionEmpire.News
{
    /// <summary>
    /// Calculates impact scores for news events
    /// </summary>
    public class ImpactCalculator
    {
        /// <summary>
        /// Calculate story significance based on source, coverage, entities
        /// </summary>
        public float CalculateStorySignificance(ProcessedNews news, EventTemplate template)
        {
            float significance = 0f;
            
            // Base significance from relevance
            significance += news.PoliticalRelevance * 0.4f;
            
            // Controversy multiplier
            significance += news.ControversyScore * 0.3f;
            
            // Source credibility
            float sourceCredibility = GetSourceCredibility(news.OriginalArticle.Source);
            significance *= (1f + sourceCredibility * 0.2f);
            
            // Entity prominence
            float entityProminence = CalculateEntityProminence(news);
            significance += entityProminence * 20f;
            
            // Sentiment impact (negative news is more significant)
            if (news.Sentiment.Classification == SentimentType.VeryNegative ||
                news.Sentiment.Classification == SentimentType.Negative)
            {
                significance *= 1.3f;
            }
            
            return Mathf.Clamp(significance, 0f, 100f);
        }
        
        private float GetSourceCredibility(string source)
        {
            // Major news sources have higher credibility
            string[] majorSources = {
                "Associated Press", "Reuters", "BBC", "CNN", "New York Times",
                "Washington Post", "Wall Street Journal", "Politico"
            };
            
            if (majorSources.Any(s => source.Contains(s)))
                return 1.0f;
            
            return 0.5f; // Default credibility
        }
        
        private float CalculateEntityProminence(ProcessedNews news)
        {
            float prominence = 0f;
            
            if (news.Entities == null)
                return 0f;
            
            // Count entities
            prominence += news.Entities.Count * 0.2f;
            
            // Person entities are more prominent
            int personCount = news.Entities.Count(e => e.Type == EntityType.Person);
            prominence += personCount * 0.3f;
            
            // Organization entities
            int orgCount = news.Entities.Count(e => e.Type == EntityType.Organization);
            prominence += orgCount * 0.2f;
            
            return Mathf.Min(1f, prominence);
        }
        
        /// <summary>
        /// Calculate scaled impact based on player's office tier
        /// </summary>
        public ImpactFormula ScaleImpactByContext(ImpactFormula baseImpact, PlayerState player)
        {
            var scaled = new ImpactFormula
            {
                TrustImpact = baseImpact.TrustImpact,
                VoterBlocImpacts = new Dictionary<IssueCategory, float>(baseImpact.VoterBlocImpacts),
                PolicyOpportunity = baseImpact.PolicyOpportunity,
                ResourceCost = baseImpact.ResourceCost,
                ResourceGain = baseImpact.ResourceGain
            };
            
            // Scale by office tier
            float tierMultiplier = 1f;
            if (player.CurrentOffice != null)
            {
                tierMultiplier = player.CurrentOffice.Tier * 0.2f + 0.6f; // 0.8x to 1.6x
            }
            
            // Higher office = more impact
            scaled.TrustImpact *= tierMultiplier;
            
            foreach (var key in scaled.VoterBlocImpacts.Keys.ToList())
            {
                scaled.VoterBlocImpacts[key] *= tierMultiplier;
            }
            
            // Higher office = more resource cost
            scaled.ResourceCost *= tierMultiplier;
            scaled.ResourceGain *= tierMultiplier;
            
            return scaled;
        }
        
        /// <summary>
        /// Calculate final impact after player response
        /// </summary>
        public Dictionary<string, float> CalculateFinalImpact(
            ImpactFormula baseImpact, 
            string responseType,
            bool responseSuccessful)
        {
            var finalImpact = new Dictionary<string, float>();
            
            // Base trust impact
            float trustImpact = baseImpact.TrustImpact;
            
            // Modify by response
            if (responseSuccessful)
            {
                trustImpact *= 0.5f; // Successful response reduces impact
            }
            else
            {
                trustImpact *= 1.5f; // Failed response increases impact
            }
            
            finalImpact["PublicTrust"] = trustImpact;
            
            // Voter bloc impacts
            foreach (var impact in baseImpact.VoterBlocImpacts)
            {
                float blocImpact = impact.Value;
                
                if (responseSuccessful)
                    blocImpact *= 0.6f;
                else
                    blocImpact *= 1.3f;
                
                finalImpact[$"VoterBloc_{impact.Key}"] = blocImpact;
            }
            
            // Resource costs/gains
            if (baseImpact.ResourceCost > 0f)
            {
                finalImpact["ResourceCost"] = baseImpact.ResourceCost;
            }
            
            if (baseImpact.ResourceGain > 0f)
            {
                finalImpact["ResourceGain"] = baseImpact.ResourceGain;
            }
            
            return finalImpact;
        }
    }
}

