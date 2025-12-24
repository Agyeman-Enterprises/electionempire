using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionEmpire.News
{
    /// <summary>
    /// Manages news cycle simulation and temporal mechanics
    /// </summary>
    public class NewsCycleManager
    {
        private Dictionary<string, NewsCycle> activeCycles;
        private float timeScale; // Real-world days to game turns
        
        public NewsCycleManager()
        {
            activeCycles = new Dictionary<string, NewsCycle>();
            timeScale = 1f; // 1 real day = 1 game day (adjustable)
        }
        
        /// <summary>
        /// Update news cycles each turn
        /// </summary>
        public void UpdateCycles(float deltaTime)
        {
            var cyclesToRemove = new List<string>();
            
            foreach (var cycle in activeCycles.Values.ToList())
            {
                UpdateCycle(cycle, deltaTime);
                
                // Remove expired cycles
                if (cycle.CurrentStage == NewsCycleStage.Historical)
                {
                    cyclesToRemove.Add(cycle.ID);
                }
            }
            
            foreach (var id in cyclesToRemove)
            {
                activeCycles.Remove(id);
            }
        }
        
        private void UpdateCycle(NewsCycle cycle, float deltaTime)
        {
            cycle.TimeInStage += deltaTime;
            
            // Advance stage based on time
            switch (cycle.CurrentStage)
            {
                case NewsCycleStage.Breaking:
                    if (cycle.TimeInStage > 86400f) // 1 day
                    {
                        AdvanceStage(cycle, NewsCycleStage.Developing);
                    }
                    break;
                
                case NewsCycleStage.Developing:
                    if (cycle.TimeInStage > 259200f) // 3 days
                    {
                        AdvanceStage(cycle, NewsCycleStage.Ongoing);
                    }
                    break;
                
                case NewsCycleStage.Ongoing:
                    if (cycle.TimeInStage > 604800f) // 7 days
                    {
                        AdvanceStage(cycle, NewsCycleStage.Historical);
                    }
                    break;
            }
            
            // Apply media fatigue
            ApplyMediaFatigue(cycle, deltaTime);
        }
        
        private void AdvanceStage(NewsCycle cycle, NewsCycleStage newStage)
        {
            cycle.CurrentStage = newStage;
            cycle.TimeInStage = 0f;
            
            Debug.Log($"News cycle advanced: {cycle.NewsTitle} → {newStage}");
        }
        
        private void ApplyMediaFatigue(NewsCycle cycle, float deltaTime)
        {
            // Media attention decays over time
            float fatigueRate = 0.1f; // Per day
            
            cycle.MediaAttention -= fatigueRate * (deltaTime / 86400f);
            cycle.MediaAttention = Mathf.Max(0f, cycle.MediaAttention);
            
            // Public interest also decays
            cycle.PublicInterest -= fatigueRate * 0.8f * (deltaTime / 86400f);
            cycle.PublicInterest = Mathf.Max(0f, cycle.PublicInterest);
        }
        
        /// <summary>
        /// Create new news cycle from processed news
        /// </summary>
        public NewsCycle CreateCycle(ProcessedNews news)
        {
            var cycle = new NewsCycle
            {
                ID = System.Guid.NewGuid().ToString(),
                NewsID = news.OriginalArticle.Title,
                NewsTitle = news.OriginalArticle.Title,
                CurrentStage = NewsCycleStage.Breaking,
                TimeInStage = 0f,
                MediaAttention = news.ControversyScore,
                PublicInterest = news.PoliticalRelevance,
                CreatedDate = DateTime.Now
            };
            
            activeCycles[cycle.ID] = cycle;
            
            return cycle;
        }
        
        /// <summary>
        /// Get active cycles
        /// </summary>
        public List<NewsCycle> GetActiveCycles()
        {
            return activeCycles.Values
                .Where(c => c.CurrentStage != NewsCycleStage.Historical)
                .ToList();
        }
        
        /// <summary>
        /// Scale real-world time to game time
        /// </summary>
        public float ScaleTime(float realWorldDays)
        {
            return realWorldDays * timeScale;
        }
        
        public void SetTimeScale(float scale)
        {
            timeScale = scale;
        }
    }
    
    [Serializable]
    public class NewsCycle
    {
        public string ID;
        public string NewsID;
        public string NewsTitle;
        public NewsCycleStage CurrentStage;
        public float TimeInStage;
        public float MediaAttention; // 0-100
        public float PublicInterest; // 0-100
        public DateTime CreatedDate;
    }
    
    public enum NewsCycleStage
    {
        Breaking,      // Just broke, maximum attention
        Developing,    // Story developing, high attention
        Ongoing,       // Continued coverage, moderate attention
        Historical     // Old news, minimal attention
    }
}

