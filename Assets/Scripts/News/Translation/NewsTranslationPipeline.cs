// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - News Translation Pipeline
// Complete pipeline for translating real news to game events
// ═══════════════════════════════════════════════════════════════════════════════

using System;
using UnityEngine;
using ElectionEmpire.News.Templates;
using ElectionEmpire.Core;

namespace ElectionEmpire.News.Translation
{
    /// <summary>
    /// Complete pipeline that processes news items through template matching and event generation
    /// </summary>
    public class NewsTranslationPipeline
    {
        private readonly INewsTranslationCoreGameStateProvider _gameState;
        private readonly TemplateMatcher _templateMatcher;
        private readonly VariableInjector _variableInjector;
        private readonly NewsEventFactory _eventFactory;
        private readonly TemplateMatcherConfig _config;

        public NewsTranslationPipeline(INewsTranslationCoreGameStateProvider gameState, TemplateMatcherConfig config = null)
        {
            _gameState = gameState;
            _config = config ?? new TemplateMatcherConfig();

            _templateMatcher = new TemplateMatcher(gameState);
            _variableInjector = new VariableInjector(gameState);
            _eventFactory = new NewsEventFactory(gameState);

            Debug.Log("[NewsTranslationPipeline] Initialized with template matcher and event factory");
        }

        /// <summary>
        /// Process a news item through the full pipeline
        /// </summary>
        public NewsGameEvent Process(ProcessedNewsItem newsItem)
        {
            if (newsItem == null)
            {
                Debug.LogWarning("[NewsTranslationPipeline] Null news item provided");
                return null;
            }

            try
            {
                // Stage 1: Find best matching template
                var matchedTemplate = _templateMatcher.FindBestMatch(newsItem);

                if (matchedTemplate == null)
                {
                    Debug.LogWarning($"[NewsTranslationPipeline] No template match for: {newsItem.Headline}");
                    return CreateFallbackEvent(newsItem);
                }

                Debug.Log($"[NewsTranslationPipeline] Matched template {matchedTemplate.Template.TemplateId} " +
                         $"with score {matchedTemplate.MatchScore:F2} for: {newsItem.Headline}");

                // Stage 2: Inject variables
                _variableInjector.InjectVariables(matchedTemplate);

                // Stage 3: Create game event
                var gameEvent = _eventFactory.CreateEvent(matchedTemplate);

                Debug.Log($"[NewsTranslationPipeline] Created event: {gameEvent.Headline}");

                return gameEvent;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NewsTranslationPipeline] Error processing news: {ex.Message}\n{ex.StackTrace}");
                return CreateFallbackEvent(newsItem);
            }
        }

        /// <summary>
        /// Create a basic fallback event when template matching fails
        /// </summary>
        private NewsGameEvent CreateFallbackEvent(ProcessedNewsItem newsItem)
        {
            Debug.Log($"[NewsTranslationPipeline] Creating fallback event for: {newsItem.Headline}");

            return new NewsGameEvent
            {
                EventId = Guid.NewGuid().ToString(),
                Headline = newsItem.Headline,
                Description = newsItem.Summary,
                ContextText = "This event requires your attention.",
                Type = TemplateEventType.PolicyPressure,
                Urgency = UrgencyLevel.Developing,
                Category = newsItem.PrimaryCategory.ToString(),
                SourceNewsId = newsItem.SourceId,
                RealWorldNote = $"Based on news from {newsItem.PublishedAt:MMM d, yyyy}",

                CreatedTurn = _gameState.GetCurrentTurn(),
                ExpirationTurn = _gameState.GetCurrentTurn() + 10,

                ResponseOptions = new System.Collections.Generic.List<ResponseOption>
                {
                    new ResponseOption
                    {
                        OptionId = "fallback_acknowledge",
                        Label = "Acknowledge",
                        Description = "Acknowledge this development",
                        SuccessProbability = 1.0f
                    },
                    new ResponseOption
                    {
                        OptionId = "fallback_ignore",
                        Label = "Ignore",
                        Description = "Focus on other matters",
                        SuccessProbability = 1.0f
                    }
                },

                Effects = new NewsEventEffects
                {
                    TrustDelta = newsItem.OverallSentiment * 0.1f,
                    MediaDelta = newsItem.ImpactScore,
                    VoterBlocDeltas = new System.Collections.Generic.Dictionary<string, float>()
                },

                Tags = new System.Collections.Generic.List<string> { "fallback", "auto-generated" },
                IsChaosModeContent = false,
                PlayerResponded = false
            };
        }
    }
}
