using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ElectionEmpire.News
{
    /// <summary>
    /// Connects to multiple news APIs and RSS feeds
    /// </summary>
    public class NewsAPIConnector : MonoBehaviour
    {
        // Multiple API sources for redundancy
        private Dictionary<string, NewsSource> sources;
        
        public void Initialize()
        {
            sources = new Dictionary<string, NewsSource>
            {
                // Free/affordable APIs
                ["newsapi"] = new NewsSource
                {
                    Name = "NewsAPI.org",
                    Endpoint = "https://newsapi.org/v2/everything",
                    APIKey = LoadAPIKey("newsapi"),
                    Category = "politics",
                    Country = "us",
                    RateLimit = 100, // requests per day (free tier)
                    Priority = 1
                },
                
                ["gnews"] = new NewsSource
                {
                    Name = "GNews API",
                    Endpoint = "https://gnews.io/api/v4/search",
                    APIKey = LoadAPIKey("gnews"),
                    Category = "nation",
                    RateLimit = 100,
                    Priority = 2
                },
                
                ["currents"] = new NewsSource
                {
                    Name = "Currents API",
                    Endpoint = "https://api.currentsapi.services/v1/search",
                    APIKey = LoadAPIKey("currentsapi"),
                    Category = "politics",
                    RateLimit = 600,
                    Priority = 3
                },
                
                // RSS fallback (always available)
                ["rss_ap"] = new NewsSource
                {
                    Name = "AP Politics RSS",
                    Endpoint = "https://apnews.com/rss/politics",
                    Type = SourceType.RSS,
                    RateLimit = -1, // Unlimited
                    Priority = 4
                },
                
                ["rss_reuters"] = new NewsSource
                {
                    Name = "Reuters Politics RSS",
                    Endpoint = "https://www.reuters.com/rssfeed/politics",
                    Type = SourceType.RSS,
                    RateLimit = -1,
                    Priority = 5
                }
            };
        }
        
        /// <summary>
        /// Fetch latest political news
        /// </summary>
        public async Task<List<NewsArticle>> FetchLatestNews(int maxArticles = 20)
        {
            var articles = new List<NewsArticle>();
            
            // Try sources in priority order
            foreach (var source in sources.Values.OrderBy(s => s.Priority))
            {
                try
                {
                    if (CheckRateLimit(source))
                    {
                        var fetchedArticles = await FetchFromSource(source, maxArticles);
                        articles.AddRange(fetchedArticles);
                        
                        if (articles.Count >= maxArticles)
                            break;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to fetch from {source.Name}: {e.Message}");
                    // Continue to next source
                }
            }
            
            // Deduplicate
            articles = DeduplicateArticles(articles);
            
            // Cache for offline use
            CacheArticles(articles);
            
            return articles.Take(maxArticles).ToList();
        }
        
        private async Task<List<NewsArticle>> FetchFromSource(NewsSource source, int count)
        {
            var articles = new List<NewsArticle>();
            
            if (source.Type == SourceType.REST_API)
            {
                articles = await FetchFromRESTAPI(source, count);
            }
            else if (source.Type == SourceType.RSS)
            {
                articles = await FetchFromRSS(source, count);
            }
            
            // Mark source
            articles.ForEach(a => a.SourceName = source.Name);
            
            return articles;
        }
        
        private async Task<List<NewsArticle>> FetchFromRESTAPI(NewsSource source, int count)
        {
            var articles = new List<NewsArticle>();
            
            // Build URL with parameters
            string url = BuildAPIURL(source, count);
            
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                // Add API key header if needed
                if (!string.IsNullOrEmpty(source.APIKey) && !url.Contains("apiKey="))
                {
                    request.SetRequestHeader("Authorization", $"Bearer {source.APIKey}");
                }
                
                var operation = request.SendWebRequest();
                
                while (!operation.isDone)
                {
                    await Task.Yield();
                }
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    string json = request.downloadHandler.text;
                    articles = ParseAPIResponse(json, source.Name);
                    
                    // Update rate limit
                    UpdateRateLimit(source);
                }
                else
                {
                    Debug.LogError($"API Error from {source.Name}: {request.error}");
                }
            }
            
            return articles;
        }
        
        private async Task<List<NewsArticle>> FetchFromRSS(NewsSource source, int count)
        {
            var articles = new List<NewsArticle>();
            
            using (UnityWebRequest request = UnityWebRequest.Get(source.Endpoint))
            {
                var operation = request.SendWebRequest();
                
                while (!operation.isDone)
                {
                    await Task.Yield();
                }
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    string xml = request.downloadHandler.text;
                    articles = ParseRSSFeed(xml);
                }
            }
            
            return articles.Take(count).ToList();
        }
        
        private string BuildAPIURL(NewsSource source, int count)
        {
            string url = source.Endpoint;
            
            switch (source.Name)
            {
                case "NewsAPI.org":
                    url += $"?q=politics&language=en&pageSize={count}&apiKey={source.APIKey}";
                    break;
                
                case "GNews API":
                    url += $"?q=politics&lang=en&max={count}&token={source.APIKey}";
                    break;
                
                case "Currents API":
                    url += $"?keywords=politics&language=en&apiKey={source.APIKey}";
                    break;
            }
            
            return url;
        }
        
        private List<NewsArticle> ParseAPIResponse(string json, string sourceName)
        {
            var articles = new List<NewsArticle>();
            
            // Parse JSON based on source
            try
            {
                if (sourceName == "NewsAPI.org")
                {
                    var response = JsonUtility.FromJson<NewsAPIResponse>(json);
                    if (response != null && response.articles != null)
                    {
                        articles = response.articles.Select(a => new NewsArticle
                        {
                            Title = a.title ?? "",
                            Description = a.description ?? "",
                            Content = a.content ?? "",
                            URL = a.url ?? "",
                            PublishedDate = ParseDate(a.publishedAt),
                            Source = a.source?.name ?? "Unknown"
                        }).ToList();
                    }
                }
                // Add parsers for other APIs...
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to parse {sourceName} response: {e.Message}");
            }
            
            return articles;
        }
        
        private List<NewsArticle> ParseRSSFeed(string xml)
        {
            var articles = new List<NewsArticle>();
            
            try
            {
                // Simple XML parsing (Unity doesn't have XmlDocument by default)
                // Would need to add XML parsing library or use regex
                // For now, simplified version
                
                // Extract items using string parsing
                string[] items = xml.Split(new[] { "<item>" }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var item in items.Skip(1)) // Skip first (before first <item>)
                {
                    var article = new NewsArticle();
                    
                    // Extract title
                    int titleStart = item.IndexOf("<title>");
                    int titleEnd = item.IndexOf("</title>");
                    if (titleStart >= 0 && titleEnd > titleStart)
                    {
                        article.Title = item.Substring(titleStart + 7, titleEnd - titleStart - 7)
                            .Replace("<![CDATA[", "").Replace("]]>", "").Trim();
                    }
                    
                    // Extract description
                    int descStart = item.IndexOf("<description>");
                    int descEnd = item.IndexOf("</description>");
                    if (descStart >= 0 && descEnd > descStart)
                    {
                        article.Description = item.Substring(descStart + 13, descEnd - descStart - 13)
                            .Replace("<![CDATA[", "").Replace("]]>", "").Trim();
                    }
                    
                    // Extract link
                    int linkStart = item.IndexOf("<link>");
                    int linkEnd = item.IndexOf("</link>");
                    if (linkStart >= 0 && linkEnd > linkStart)
                    {
                        article.URL = item.Substring(linkStart + 6, linkEnd - linkStart - 6).Trim();
                    }
                    
                    // Extract pubDate
                    int dateStart = item.IndexOf("<pubDate>");
                    int dateEnd = item.IndexOf("</pubDate>");
                    if (dateStart >= 0 && dateEnd > dateStart)
                    {
                        string dateStr = item.Substring(dateStart + 9, dateEnd - dateStart - 9).Trim();
                        article.PublishedDate = ParseRSSDate(dateStr);
                    }
                    
                    if (!string.IsNullOrEmpty(article.Title))
                    {
                        article.Source = "RSS Feed";
                        articles.Add(article);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to parse RSS: {e.Message}");
            }
            
            return articles;
        }
        
        private DateTime ParseRSSDate(string dateStr)
        {
            // Try common RSS date formats
            if (DateTime.TryParse(dateStr, out DateTime result))
            {
                return result;
            }
            
            return DateTime.Now;
        }
        
        private DateTime ParseDate(string dateStr)
        {
            if (string.IsNullOrEmpty(dateStr))
                return DateTime.Now;
            
            if (DateTime.TryParse(dateStr, out DateTime result))
            {
                return result;
            }
            
            return DateTime.Now;
        }
        
        private List<NewsArticle> DeduplicateArticles(List<NewsArticle> articles)
        {
            // Remove duplicate headlines (same story from multiple sources)
            return articles
                .GroupBy(a => NormalizeHeadline(a.Title))
                .Select(g => g.First())
                .ToList();
        }
        
        private string NormalizeHeadline(string headline)
        {
            if (string.IsNullOrEmpty(headline))
                return "";
            
            // Remove punctuation, lowercase, trim for comparison
            return headline.ToLower()
                .Replace(".", "")
                .Replace(",", "")
                .Replace(":", "")
                .Replace("!", "")
                .Replace("?", "")
                .Trim();
        }
        
        private void CacheArticles(List<NewsArticle> articles)
        {
            // Save to local cache for offline play
            var cache = new NewsCache
            {
                Articles = articles,
                CachedDate = DateTime.Now
            };
            
            string json = JsonUtility.ToJson(cache);
            PlayerPrefs.SetString("NewsCache", json);
            PlayerPrefs.Save();
        }
        
        public List<NewsArticle> LoadCachedNews()
        {
            string json = PlayerPrefs.GetString("NewsCache", "");
            
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var cache = JsonUtility.FromJson<NewsCache>(json);
                    
                    // Only use cache if less than 24 hours old
                    if (cache != null && cache.Articles != null && 
                        (DateTime.Now - cache.CachedDate).TotalHours < 24)
                    {
                        return cache.Articles;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to load cached news: {e.Message}");
                }
            }
            
            return new List<NewsArticle>();
        }
        
        private bool CheckRateLimit(NewsSource source)
        {
            if (source.RateLimit < 0) return true; // Unlimited
            
            string key = $"RateLimit_{source.Name}";
            string dateKey = $"RateLimitDate_{source.Name}";
            
            // Check if it's a new day
            string lastDate = PlayerPrefs.GetString(dateKey, "");
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            
            if (lastDate != today)
            {
                // Reset for new day
                PlayerPrefs.SetInt(key, 0);
                PlayerPrefs.SetString(dateKey, today);
            }
            
            int requestsToday = PlayerPrefs.GetInt(key, 0);
            
            return requestsToday < source.RateLimit;
        }
        
        private void UpdateRateLimit(NewsSource source)
        {
            if (source.RateLimit < 0) return;
            
            string key = $"RateLimit_{source.Name}";
            int requestsToday = PlayerPrefs.GetInt(key, 0);
            PlayerPrefs.SetInt(key, requestsToday + 1);
            PlayerPrefs.Save();
        }
        
        private string LoadAPIKey(string sourceName)
        {
            // Load from PlayerPrefs or config file
            // In production, would use secure storage
            return PlayerPrefs.GetString($"APIKey_{sourceName}", "");
        }
    }
    
    [Serializable]
    public class NewsArticle
    {
        public string Title;
        public string Description;
        public string Content;
        public string URL;
        public DateTime PublishedDate;
        public string Source;
        public string SourceName;
        
        // Processed fields (set by NewsProcessor)
        public float PoliticalRelevance;
        public List<string> Topics;
        public List<string> Entities;
        public SentimentScore Sentiment;
        public float ControversyScore;
        
        public NewsArticle()
        {
            Topics = new List<string>();
            Entities = new List<string>();
        }
    }
    
    [Serializable]
    public class NewsSource
    {
        public string Name;
        public string Endpoint;
        public string APIKey;
        public SourceType Type = SourceType.REST_API;
        public string Category;
        public string Country;
        public int RateLimit;
        public int Priority;
    }
    
    public enum SourceType
    {
        REST_API,
        RSS,
        Webhook
    }
    
    // Response models for different APIs
    [Serializable]
    public class NewsAPIResponse
    {
        public string status;
        public int totalResults;
        public List<NewsAPIArticle> articles;
    }
    
    [Serializable]
    public class NewsAPIArticle
    {
        public NewsAPISource source;
        public string title;
        public string description;
        public string content;
        public string url;
        public string publishedAt;
    }
    
    [Serializable]
    public class NewsAPISource
    {
        public string name;
    }
    
    [Serializable]
    public class NewsCache
    {
        public List<NewsArticle> Articles;
        public DateTime CachedDate;
    }
}

