using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ElectionEmpire.News;

namespace ElectionEmpire.UI
{
    /// <summary>
    /// UI for displaying real-world news in the game
    /// </summary>
    public class NewsUI : MonoBehaviour
    {
        [Header("News Feed")]
        public Transform NewsFeedContainer;
        public GameObject NewsItemPrefab;
        public Button RefreshButton;
        
        [Header("Trending Section")]
        public Transform TrendingContainer;
        public TextMeshProUGUI TrendingTitleText;
        
        [Header("News Details")]
        public GameObject DetailsPanel;
        public TextMeshProUGUI NewsTitleText;
        public TextMeshProUGUI NewsDescriptionText;
        public TextMeshProUGUI NewsSourceText;
        public TextMeshProUGUI NewsDateText;
        public TextMeshProUGUI RelevanceText;
        public TextMeshProUGUI SentimentText;
        public TextMeshProUGUI ControversyText;
        public TextMeshProUGUI TopicsText;
        public TextMeshProUGUI ImpactText;
        public Button OpenArticleButton;
        
        [Header("Active Events")]
        public Transform EventsContainer;
        public TextMeshProUGUI ActiveEventsTitleText;
        
        private NewsEventManager newsManager;
        private List<ProcessedNews> currentNews;
        
        public void Initialize(NewsEventManager manager)
        {
            newsManager = manager;
            
            if (RefreshButton != null)
                RefreshButton.onClick.AddListener(() => RefreshNews());
            
            if (OpenArticleButton != null)
                OpenArticleButton.onClick.AddListener(() => OpenArticleInBrowser());
        }
        
        public void DisplayNews(List<ProcessedNews> news)
        {
            currentNews = news;
            
            if (NewsFeedContainer == null) return;
            
            // Clear existing
            foreach (Transform child in NewsFeedContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Create news items
            foreach (var item in news)
            {
                CreateNewsItem(item);
            }
        }
        
        private void CreateNewsItem(ProcessedNews news)
        {
            GameObject item;
            
            if (NewsItemPrefab != null)
            {
                item = Instantiate(NewsItemPrefab, NewsFeedContainer);
            }
            else
            {
                item = new GameObject($"NewsItem_{news.OriginalArticle.Title}");
                item.transform.SetParent(NewsFeedContainer);
                
                // Add layout
                var layout = item.AddComponent<HorizontalLayoutGroup>();
                layout.spacing = 10;
            }
            
            // Add click handler
            Button button = item.GetComponent<Button>();
            if (button == null)
                button = item.AddComponent<Button>();
            
            button.onClick.AddListener(() => ShowNewsDetails(news));
            
            // Create title text
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(item.transform);
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = news.OriginalArticle.Title;
            titleText.fontSize = 14;
            titleText.fontStyle = FontStyles.Bold;
            
            // Create relevance badge
            GameObject relevanceObj = new GameObject("Relevance");
            relevanceObj.transform.SetParent(item.transform);
            TextMeshProUGUI relevanceText = relevanceObj.AddComponent<TextMeshProUGUI>();
            relevanceText.text = $"{news.PoliticalRelevance:F0}%";
            relevanceText.fontSize = 12;
            relevanceText.color = GetRelevanceColor(news.PoliticalRelevance);
            
            // Create sentiment indicator
            GameObject sentimentObj = new GameObject("Sentiment");
            sentimentObj.transform.SetParent(item.transform);
            TextMeshProUGUI sentimentText = sentimentObj.AddComponent<TextMeshProUGUI>();
            sentimentText.text = GetSentimentEmoji(news.Sentiment.Classification);
            sentimentText.fontSize = 16;
        }
        
        private void ShowNewsDetails(ProcessedNews news)
        {
            if (DetailsPanel != null)
                DetailsPanel.SetActive(true);
            
            if (NewsTitleText != null)
                NewsTitleText.text = news.OriginalArticle.Title;
            
            if (NewsDescriptionText != null)
                NewsDescriptionText.text = news.OriginalArticle.Description;
            
            if (NewsSourceText != null)
                NewsSourceText.text = $"Source: {news.OriginalArticle.Source}";
            
            if (NewsDateText != null)
                NewsDateText.text = $"Published: {news.OriginalArticle.PublishedDate:MMM dd, yyyy}";
            
            if (RelevanceText != null)
                RelevanceText.text = $"Political Relevance: {news.PoliticalRelevance:F0}%";
            
            if (SentimentText != null)
                SentimentText.text = $"Sentiment: {news.Sentiment.Classification} " +
                                    $"(Net: {news.Sentiment.Net:+#.##;-#.##;0})";
            
            if (ControversyText != null)
                ControversyText.text = $"Controversy Score: {news.ControversyScore:F0}/100";
            
            if (TopicsText != null)
            {
                string topics = "Topics: " + string.Join(", ", news.Topics.Take(5));
                topics += "\nIssues: " + string.Join(", ", news.IssueCategories);
                TopicsText.text = topics;
            }
            
            if (ImpactText != null)
            {
                string impact = "Game Impact:\n";
                impact += $"• Event Type: {news.EventType}\n";
                if (news.IssueCategories.Count > 0)
                {
                    impact += $"• Policy Opportunity: {news.IssueCategories[0]}\n";
                }
                ImpactText.text = impact;
            }
        }
        
        private void RefreshNews()
        {
            if (newsManager != null)
            {
                var trending = newsManager.GetTrendingNews(10);
                DisplayNews(trending);
            }
        }
        
        private void OpenArticleInBrowser()
        {
            // Would open article URL in browser
            // Application.OpenURL(news.OriginalArticle.URL);
            Debug.Log("Would open article in browser");
        }
        
        private Color GetRelevanceColor(float relevance)
        {
            if (relevance > 70f) return Color.red;
            if (relevance > 50f) return Color.yellow;
            return Color.green;
        }
        
        private string GetSentimentEmoji(SentimentType sentiment)
        {
            return sentiment switch
            {
                SentimentType.VeryPositive => "😊",
                SentimentType.Positive => "🙂",
                SentimentType.Neutral => "😐",
                SentimentType.Negative => "😟",
                SentimentType.VeryNegative => "😡",
                _ => "😐"
            };
        }
        
        public void DisplayActiveEvents(List<NewsEvent> events)
        {
            if (EventsContainer == null) return;
            
            // Clear existing
            foreach (Transform child in EventsContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Create event items
            foreach (var newsEvent in events)
            {
                CreateEventItem(newsEvent);
            }
        }
        
        private void CreateEventItem(NewsEvent newsEvent)
        {
            GameObject item = new GameObject($"EventItem_{newsEvent.ID}");
            item.transform.SetParent(EventsContainer);
            
            TextMeshProUGUI text = item.AddComponent<TextMeshProUGUI>();
            text.text = $"📰 {newsEvent.ProcessedNews.OriginalArticle.Title}";
            text.fontSize = 12;
        }
    }
}

