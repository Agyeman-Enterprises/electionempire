// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - Portrait Reaction & AI Integration System
// Sprint 10: Dynamic event reactions and photo-to-caricature generation
// ═══════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ElectionEmpire.Caricature
{
    #region Event Reaction Mappings
    
    /// <summary>
    /// Maps game events to appropriate portrait expressions.
    /// </summary>
    [Serializable]
    public class EventReactionMapping
    {
        public string EventType;
        public Expression PrimaryExpression;
        public Expression SecondaryExpression;
        public float SecondaryChance;
        public float ExpressionDuration;
        public PortraitEffect OptionalEffect;
        public float EffectChance;
    }
    
    /// <summary>
    /// Reaction intensity based on event severity.
    /// </summary>
    public enum ReactionIntensity
    {
        Subtle,         // Minor events
        Moderate,       // Standard events
        Strong,         // Major events
        Extreme         // Crisis-level events
    }
    
    #endregion
    
    #region Portrait Reaction System
    
    /// <summary>
    /// Manages dynamic portrait reactions to game events.
    /// </summary>
    public class PortraitReactionSystem : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private float defaultReactionDuration = 3f;
        [SerializeField] private float reactionCooldown = 0.5f;
        [SerializeField] private bool enableRandomVariation = true;
        [SerializeField] private float variationAmount = 0.2f;
        
        [Header("References")]
        [SerializeField] private PortraitRenderer portraitRenderer;
        
        // Reaction mappings
        private Dictionary<string, EventReactionMapping> reactionMappings;
        
        // State
        private Expression baseExpression = Expression.Neutral;
        private Expression currentReactionExpression;
        private bool isReacting;
        private float reactionTimer;
        private Queue<PendingReaction> reactionQueue;
        private float cooldownTimer;
        
        // Events
        public event Action<Expression, string> OnReactionTriggered;
        public event Action OnReactionComplete;
        
        private void Awake()
        {
            reactionMappings = new Dictionary<string, EventReactionMapping>();
            reactionQueue = new Queue<PendingReaction>();
            InitializeDefaultMappings();
        }
        
        private void Update()
        {
            // Update cooldown
            if (cooldownTimer > 0)
            {
                cooldownTimer -= Time.deltaTime;
            }
            
            // Update active reaction
            if (isReacting)
            {
                reactionTimer -= Time.deltaTime;
                if (reactionTimer <= 0)
                {
                    EndReaction();
                }
            }
            
            // Process queued reactions
            if (!isReacting && cooldownTimer <= 0 && reactionQueue.Count > 0)
            {
                var next = reactionQueue.Dequeue();
                ExecuteReaction(next);
            }
        }
        
        #region Initialization
        
        private void InitializeDefaultMappings()
        {
            // Election Events
            AddMapping("election_win", Expression.Victorious, Expression.Happy, 0.3f, 5f);
            AddMapping("election_loss", Expression.Defeated, Expression.Sad, 0.4f, 4f);
            AddMapping("polling_up", Expression.Happy, Expression.Confident, 0.5f, 2f);
            AddMapping("polling_down", Expression.Worried, Expression.Nervous, 0.4f, 2f);
            AddMapping("endorsement_received", Expression.Happy, Expression.Smug, 0.3f, 3f);
            AddMapping("endorsement_lost", Expression.Sad, Expression.Worried, 0.4f, 2f);
            
            // Scandal Events
            AddMapping("scandal_minor", Expression.Nervous, Expression.Worried, 0.5f, 3f, PortraitEffect.Sweating, 0.3f);
            AddMapping("scandal_major", Expression.Shocked, Expression.Worried, 0.3f, 4f, PortraitEffect.Sweating, 0.6f);
            AddMapping("scandal_career_ending", Expression.Shocked, Expression.Defeated, 0.2f, 6f, PortraitEffect.Pallid, 0.8f);
            AddMapping("scandal_survived", Expression.Confident, Expression.Smug, 0.4f, 3f);
            AddMapping("scandal_denied", Expression.Angry, Expression.Determined, 0.3f, 2f);
            AddMapping("scandal_apologized", Expression.Sad, Expression.Pleading, 0.4f, 3f);
            
            // Crisis Events
            AddMapping("crisis_started", Expression.Worried, Expression.Determined, 0.4f, 3f);
            AddMapping("crisis_resolved_well", Expression.Confident, Expression.Happy, 0.5f, 4f);
            AddMapping("crisis_resolved_badly", Expression.Worried, Expression.Sad, 0.4f, 3f);
            AddMapping("crisis_escalated", Expression.Shocked, Expression.Worried, 0.3f, 3f, PortraitEffect.Sweating, 0.5f);
            
            // Policy Events
            AddMapping("policy_passed", Expression.Happy, Expression.Confident, 0.5f, 2f);
            AddMapping("policy_failed", Expression.Sad, Expression.Worried, 0.4f, 2f);
            AddMapping("policy_controversial", Expression.Determined, Expression.Nervous, 0.3f, 2f);
            
            // Debate Events
            AddMapping("debate_won", Expression.Victorious, Expression.Smug, 0.4f, 3f);
            AddMapping("debate_lost", Expression.Defeated, Expression.Sad, 0.3f, 3f);
            AddMapping("debate_gaffe", Expression.Shocked, Expression.Nervous, 0.3f, 2f, PortraitEffect.Blushing, 0.6f);
            AddMapping("debate_zinged", Expression.Smug, Expression.Laughing, 0.5f, 2f);
            AddMapping("debate_attacked", Expression.Angry, Expression.Determined, 0.4f, 2f);
            
            // Campaign Events
            AddMapping("fundraiser_success", Expression.Happy, Expression.Smug, 0.3f, 2f);
            AddMapping("fundraiser_failure", Expression.Sad, Expression.Worried, 0.4f, 2f);
            AddMapping("rally_success", Expression.Victorious, Expression.Happy, 0.5f, 3f);
            AddMapping("rally_disrupted", Expression.Angry, Expression.Determined, 0.3f, 2f);
            
            // Staff Events
            AddMapping("staff_betrayal", Expression.Shocked, Expression.Angry, 0.3f, 4f);
            AddMapping("staff_loyalty", Expression.Happy, Expression.Confident, 0.4f, 2f);
            AddMapping("staff_scandal", Expression.Shocked, Expression.Worried, 0.4f, 3f);
            
            // Media Events
            AddMapping("positive_coverage", Expression.Happy, Expression.Confident, 0.4f, 2f);
            AddMapping("negative_coverage", Expression.Angry, Expression.Worried, 0.4f, 2f);
            AddMapping("interview_good", Expression.Confident, Expression.Smug, 0.4f, 2f);
            AddMapping("interview_bad", Expression.Nervous, Expression.Worried, 0.5f, 2f, PortraitEffect.Sweating, 0.4f);
            
            // Relationship Events
            AddMapping("ally_gained", Expression.Happy, Expression.Confident, 0.4f, 2f);
            AddMapping("ally_lost", Expression.Sad, Expression.Worried, 0.4f, 2f);
            AddMapping("rival_defeated", Expression.Smug, Expression.Victorious, 0.5f, 3f);
            AddMapping("rival_attack", Expression.Angry, Expression.Determined, 0.3f, 2f);
            
            // Special Events
            AddMapping("impeachment_started", Expression.Shocked, Expression.Worried, 0.2f, 5f, PortraitEffect.Pallid, 0.7f);
            AddMapping("impeachment_survived", Expression.Victorious, Expression.Smug, 0.4f, 4f);
            AddMapping("impeachment_removed", Expression.Defeated, Expression.Crying, 0.3f, 6f);
            AddMapping("assassination_attempt", Expression.Shocked, Expression.Worried, 0.1f, 5f, PortraitEffect.Pallid, 0.9f);
            AddMapping("became_president", Expression.Victorious, Expression.Happy, 0.4f, 6f, PortraitEffect.Spotlight, 0.8f);
            
            // Dirty Tricks
            AddMapping("dirty_trick_success", Expression.Scheming, Expression.Smug, 0.5f, 2f);
            AddMapping("dirty_trick_backfired", Expression.Shocked, Expression.Nervous, 0.3f, 3f, PortraitEffect.Sweating, 0.5f);
            AddMapping("blackmail_used", Expression.Scheming, Expression.Smug, 0.4f, 2f);
            AddMapping("blackmail_revealed", Expression.Shocked, Expression.Guilty, 0.3f, 4f);
            
            // Chaos Mode Events
            AddMapping("chaos_event", Expression.Shocked, Expression.Laughing, 0.5f, 3f);
            AddMapping("drunk_speech", Expression.Drunk, Expression.Happy, 0.6f, 4f, PortraitEffect.Blushing, 0.7f);
            AddMapping("viral_moment", Expression.Shocked, Expression.Happy, 0.4f, 3f, PortraitEffect.Spotlight, 0.5f);
        }
        
        private void AddMapping(string eventType, Expression primary, Expression secondary, 
            float secondaryChance, float duration, PortraitEffect effect = PortraitEffect.None, float effectChance = 0f)
        {
            reactionMappings[eventType] = new EventReactionMapping
            {
                EventType = eventType,
                PrimaryExpression = primary,
                SecondaryExpression = secondary,
                SecondaryChance = secondaryChance,
                ExpressionDuration = duration,
                OptionalEffect = effect,
                EffectChance = effectChance
            };
        }
        
        #endregion
        
        #region Reaction Triggering
        
        /// <summary>
        /// Trigger a reaction based on event type.
        /// </summary>
        public void TriggerReaction(string eventType, ReactionIntensity intensity = ReactionIntensity.Moderate)
        {
            if (!reactionMappings.TryGetValue(eventType, out var mapping))
            {
                Debug.LogWarning($"No reaction mapping found for event: {eventType}");
                return;
            }
            
            var reaction = new PendingReaction
            {
                EventType = eventType,
                Mapping = mapping,
                Intensity = intensity,
                Timestamp = Time.time
            };
            
            // High intensity reactions can interrupt
            if (intensity == ReactionIntensity.Extreme && isReacting)
            {
                EndReaction();
                ExecuteReaction(reaction);
            }
            else if (isReacting)
            {
                reactionQueue.Enqueue(reaction);
            }
            else if (cooldownTimer <= 0)
            {
                ExecuteReaction(reaction);
            }
            else
            {
                reactionQueue.Enqueue(reaction);
            }
        }
        
        /// <summary>
        /// Trigger a reaction with a specific expression.
        /// </summary>
        public void TriggerExpression(Expression expression, float duration = -1f)
        {
            if (duration < 0) duration = defaultReactionDuration;
            
            var reaction = new PendingReaction
            {
                EventType = "custom",
                DirectExpression = expression,
                Duration = duration,
                Timestamp = Time.time
            };
            
            if (isReacting)
            {
                reactionQueue.Enqueue(reaction);
            }
            else
            {
                ExecuteReaction(reaction);
            }
        }
        
        private void ExecuteReaction(PendingReaction reaction)
        {
            isReacting = true;
            
            Expression expression;
            float duration;
            PortraitEffect effect = PortraitEffect.None;
            
            if (reaction.DirectExpression.HasValue)
            {
                expression = reaction.DirectExpression.Value;
                duration = reaction.Duration;
            }
            else
            {
                var mapping = reaction.Mapping;
                
                // Choose primary or secondary expression
                if (UnityEngine.Random.value < mapping.SecondaryChance)
                {
                    expression = mapping.SecondaryExpression;
                }
                else
                {
                    expression = mapping.PrimaryExpression;
                }
                
                // Apply intensity modifier
                duration = mapping.ExpressionDuration * GetIntensityMultiplier(reaction.Intensity);
                
                // Check for effect
                if (mapping.OptionalEffect != PortraitEffect.None && 
                    UnityEngine.Random.value < mapping.EffectChance * GetIntensityMultiplier(reaction.Intensity))
                {
                    effect = mapping.OptionalEffect;
                }
                
                // Apply random variation
                if (enableRandomVariation)
                {
                    duration *= 1f + (UnityEngine.Random.value - 0.5f) * variationAmount * 2f;
                }
            }
            
            currentReactionExpression = expression;
            reactionTimer = duration;
            
            // Apply to portrait renderer
            if (portraitRenderer != null)
            {
                portraitRenderer.SetExpression(expression);
                
                if (effect != PortraitEffect.None)
                {
                    portraitRenderer.ApplyEffect(effect);
                }
            }
            
            OnReactionTriggered?.Invoke(expression, reaction.EventType);
        }
        
        private void EndReaction()
        {
            isReacting = false;
            cooldownTimer = reactionCooldown;
            
            // Return to base expression
            if (portraitRenderer != null)
            {
                portraitRenderer.SetExpression(baseExpression);
                portraitRenderer.ClearEffects();
            }
            
            OnReactionComplete?.Invoke();
        }
        
        private float GetIntensityMultiplier(ReactionIntensity intensity)
        {
            return intensity switch
            {
                ReactionIntensity.Subtle => 0.6f,
                ReactionIntensity.Moderate => 1.0f,
                ReactionIntensity.Strong => 1.4f,
                ReactionIntensity.Extreme => 2.0f,
                _ => 1.0f
            };
        }
        
        #endregion
        
        #region Base Expression
        
        /// <summary>
        /// Set the base expression to return to after reactions.
        /// </summary>
        public void SetBaseExpression(Expression expression)
        {
            baseExpression = expression;
            
            if (!isReacting && portraitRenderer != null)
            {
                portraitRenderer.SetExpression(expression);
            }
        }
        
        /// <summary>
        /// Get the base expression based on character state.
        /// </summary>
        public Expression GetBaseExpressionForState(float approval, float stress, float alignment)
        {
            // High stress overrides
            if (stress > 0.8f) return Expression.Exhausted;
            if (stress > 0.6f) return Expression.Worried;
            
            // Alignment-based defaults
            if (alignment > 50) // Evil alignment
            {
                return approval > 0.6f ? Expression.Smug : Expression.Scheming;
            }
            else if (alignment < -50) // Good alignment
            {
                return approval > 0.6f ? Expression.Happy : Expression.Determined;
            }
            
            // Neutral - based on approval
            if (approval > 0.7f) return Expression.Confident;
            if (approval > 0.5f) return Expression.Neutral;
            if (approval > 0.3f) return Expression.Worried;
            return Expression.Sad;
        }
        
        #endregion
        
        #region Helper Classes
        
        private class PendingReaction
        {
            public string EventType;
            public EventReactionMapping Mapping;
            public ReactionIntensity Intensity;
            public Expression? DirectExpression;
            public float Duration;
            public float Timestamp;
        }
        
        #endregion
    }
    
    #endregion
    
    #region AI Photo Integration
    
    /// <summary>
    /// Configuration for AI photo-to-caricature generation.
    /// </summary>
    [Serializable]
    public class AICaricatureConfig
    {
        public string APIEndpoint = "https://api.example.com/caricature";
        public string APIKey;
        public int MaxImageSize = 1024;
        public float CaricatureIntensity = 0.7f;  // How exaggerated
        public string StylePreset = "political_cartoon";
        public bool AllowPhotoUpload = true;
        public float TimeoutSeconds = 30f;
    }
    
    /// <summary>
    /// Response from AI caricature generation.
    /// </summary>
    [Serializable]
    public class AICaricatureResponse
    {
        public bool Success;
        public string ErrorMessage;
        public string GeneratedImageBase64;
        public FacialFeatures ExtractedFeatures;
        public Dictionary<string, float> ConfidenceScores;
    }
    
    /// <summary>
    /// Handles AI-powered photo-to-caricature generation.
    /// </summary>
    public class AICaricatureGenerator : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private AICaricatureConfig config;
        
        [Header("References")]
        [SerializeField] private CaricatureGenerator fallbackGenerator;
        
        // State
        private bool isProcessing;
        
        // Events
        public event Action<Texture2D, FacialFeatures> OnCaricatureGenerated;
        public event Action<string> OnGenerationFailed;
        public event Action<float> OnProgressUpdated;
        
        /// <summary>
        /// Generate a caricature from a photo.
        /// </summary>
        public async Task<AICaricatureResponse> GenerateFromPhoto(Texture2D photo)
        {
            if (!config.AllowPhotoUpload)
            {
                return new AICaricatureResponse
                {
                    Success = false,
                    ErrorMessage = "Photo upload is disabled"
                };
            }
            
            if (isProcessing)
            {
                return new AICaricatureResponse
                {
                    Success = false,
                    ErrorMessage = "Already processing a photo"
                };
            }
            
            isProcessing = true;
            OnProgressUpdated?.Invoke(0.1f);
            
            try
            {
                // Resize image if needed
                var processedPhoto = ResizeImage(photo, config.MaxImageSize);
                OnProgressUpdated?.Invoke(0.2f);
                
                // Convert to base64
                var imageBytes = processedPhoto.EncodeToPNG();
                var base64Image = Convert.ToBase64String(imageBytes);
                OnProgressUpdated?.Invoke(0.3f);
                
                // Send to AI service
                var response = await SendToAIService(base64Image);
                OnProgressUpdated?.Invoke(0.9f);
                
                if (response.Success && !string.IsNullOrEmpty(response.GeneratedImageBase64))
                {
                    // Convert response to texture
                    var caricatureBytes = Convert.FromBase64String(response.GeneratedImageBase64);
                    var caricatureTexture = new Texture2D(2, 2);
                    caricatureTexture.LoadImage(caricatureBytes);
                    
                    OnCaricatureGenerated?.Invoke(caricatureTexture, response.ExtractedFeatures);
                }
                else
                {
                    OnGenerationFailed?.Invoke(response.ErrorMessage ?? "Unknown error");
                }
                
                OnProgressUpdated?.Invoke(1f);
                return response;
            }
            catch (Exception ex)
            {
                var errorResponse = new AICaricatureResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
                OnGenerationFailed?.Invoke(ex.Message);
                return errorResponse;
            }
            finally
            {
                isProcessing = false;
            }
        }
        
        /// <summary>
        /// Generate a caricature from a photo with fallback to procedural generation.
        /// </summary>
        public async Task<PortraitConfig> GeneratePortraitFromPhoto(
            Texture2D photo,
            string characterId,
            string characterName)
        {
            var response = await GenerateFromPhoto(photo);
            
            if (response.Success && response.ExtractedFeatures != null)
            {
                // Use extracted features to create portrait
                return new PortraitConfig
                {
                    CharacterId = characterId,
                    CharacterName = characterName,
                    Features = response.ExtractedFeatures,
                    CurrentExpression = Expression.Neutral,
                    CurrentEffect = PortraitEffect.None,
                    IsAnimated = true,
                    AnimationSpeed = 1f
                };
            }
            else if (fallbackGenerator != null)
            {
                // Fallback to procedural generation
                Debug.LogWarning($"AI generation failed, using procedural fallback: {response.ErrorMessage}");
                return fallbackGenerator.GenerateRandom(characterId, characterName);
            }
            else
            {
                throw new Exception($"AI generation failed and no fallback available: {response.ErrorMessage}");
            }
        }
        
        private Texture2D ResizeImage(Texture2D original, int maxSize)
        {
            if (original.width <= maxSize && original.height <= maxSize)
            {
                return original;
            }
            
            float scale = (float)maxSize / Mathf.Max(original.width, original.height);
            int newWidth = Mathf.RoundToInt(original.width * scale);
            int newHeight = Mathf.RoundToInt(original.height * scale);
            
            var resized = new Texture2D(newWidth, newHeight, original.format, false);
            
            // Simple point sampling resize
            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    float u = (float)x / newWidth;
                    float v = (float)y / newHeight;
                    resized.SetPixel(x, y, original.GetPixelBilinear(u, v));
                }
            }
            
            resized.Apply();
            return resized;
        }
        
        private async Task<AICaricatureResponse> SendToAIService(string base64Image)
        {
            // Build request
            var requestData = new Dictionary<string, object>
            {
                ["image"] = base64Image,
                ["style"] = config.StylePreset,
                ["intensity"] = config.CaricatureIntensity
            };
            
            // Convert dictionary to JSON manually since JsonUtility doesn't support Dictionary
            string jsonRequest = "{\"image\":\"" + base64Image + "\",\"style\":\"" + config.StylePreset + "\",\"intensity\":" + config.CaricatureIntensity + "}";
            
            using (var www = new UnityWebRequest(config.APIEndpoint, "POST"))
            {
                www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonRequest));
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                if (!string.IsNullOrEmpty(config.APIKey))
                {
                    www.SetRequestHeader("Authorization", $"Bearer {config.APIKey}");
                }
                www.timeout = (int)config.TimeoutSeconds;
                
                var operation = www.SendWebRequest();
                
                while (!operation.isDone)
                {
                    await Task.Yield();
                }
                
                if (www.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        return JsonUtility.FromJson<AICaricatureResponse>(www.downloadHandler.text);
                    }
                    catch (Exception ex)
                    {
                        return new AICaricatureResponse
                        {
                            Success = false,
                            ErrorMessage = $"Failed to parse response: {ex.Message}"
                        };
                    }
                }
                else
                {
                    return new AICaricatureResponse
                    {
                        Success = false,
                        ErrorMessage = www.error
                    };
                }
            }
        }
        
        /// <summary>
        /// Extract facial features from a photo using local analysis.
        /// </summary>
        public FacialFeatures ExtractFeaturesLocally(Texture2D photo)
        {
            // Simplified local feature extraction
            // In production, this would use a local ML model
            
            var features = new FacialFeatures();
            
            // Analyze dominant colors for skin tone estimation
            var skinTone = EstimateSkinTone(photo);
            features.SkinTone = skinTone;
            
            // Estimate age from image characteristics
            features.Age = AgeCategory.MiddleAge; // Default
            
            // Random but consistent features based on image hash
            int seed = GetImageHash(photo);
            var random = new System.Random(seed);
            
            features.FaceShape = (FaceShape)random.Next(Enum.GetValues(typeof(FaceShape)).Length);
            features.HairStyle = (HairStyle)random.Next(Enum.GetValues(typeof(HairStyle)).Length);
            features.HairColor = (HairColor)random.Next(Enum.GetValues(typeof(HairColor)).Length);
            features.EyeShape = (EyeShape)random.Next(Enum.GetValues(typeof(EyeShape)).Length);
            features.EyeColor = (EyeColor)random.Next(Enum.GetValues(typeof(EyeColor)).Length);
            features.NoseType = (NoseType)random.Next(Enum.GetValues(typeof(NoseType)).Length);
            features.MouthType = (MouthType)random.Next(Enum.GetValues(typeof(MouthType)).Length);
            
            // Fill in other features
            features.FaceWidth = (float)random.NextDouble();
            features.FaceHeight = (float)random.NextDouble();
            features.JawWidth = (float)random.NextDouble();
            features.CheekboneHeight = (float)random.NextDouble();
            features.HairVolume = (float)random.NextDouble();
            features.EyeSize = (float)random.NextDouble();
            features.EyeSpacing = (float)random.NextDouble();
            features.EyebrowThickness = (float)random.NextDouble();
            features.EyebrowArch = (float)random.NextDouble();
            features.NoseSize = (float)random.NextDouble();
            features.NoseWidth = (float)random.NextDouble();
            features.MouthWidth = (float)random.NextDouble();
            features.LipFullness = (float)random.NextDouble();
            
            features.Seed = seed;
            features.Accessories = new List<Accessory>();
            
            return features;
        }
        
        private SkinTone EstimateSkinTone(Texture2D photo)
        {
            // Sample center region for skin color
            int centerX = photo.width / 2;
            int centerY = photo.height / 2;
            int sampleSize = Mathf.Min(photo.width, photo.height) / 4;
            
            float totalR = 0, totalG = 0, totalB = 0;
            int sampleCount = 0;
            
            for (int y = centerY - sampleSize / 2; y < centerY + sampleSize / 2; y += 5)
            {
                for (int x = centerX - sampleSize / 2; x < centerX + sampleSize / 2; x += 5)
                {
                    if (x >= 0 && x < photo.width && y >= 0 && y < photo.height)
                    {
                        var pixel = photo.GetPixel(x, y);
                        totalR += pixel.r;
                        totalG += pixel.g;
                        totalB += pixel.b;
                        sampleCount++;
                    }
                }
            }
            
            if (sampleCount == 0) return SkinTone.Honey;
            
            float avgR = totalR / sampleCount;
            float avgG = totalG / sampleCount;
            float avgB = totalB / sampleCount;
            float luminance = (avgR + avgG + avgB) / 3f;
            
            // Map luminance to skin tone
            if (luminance > 0.85f) return SkinTone.Porcelain;
            if (luminance > 0.75f) return SkinTone.Ivory;
            if (luminance > 0.65f) return SkinTone.Sand;
            if (luminance > 0.55f) return SkinTone.Honey;
            if (luminance > 0.45f) return SkinTone.Almond;
            if (luminance > 0.35f) return SkinTone.Chestnut;
            if (luminance > 0.25f) return SkinTone.Espresso;
            if (luminance > 0.15f) return SkinTone.Mahogany;
            return SkinTone.Ebony;
        }
        
        private int GetImageHash(Texture2D photo)
        {
            // Simple hash based on sampled pixels
            int hash = 17;
            int step = Mathf.Max(1, (photo.width * photo.height) / 100);
            
            for (int i = 0; i < photo.width * photo.height; i += step)
            {
                int x = i % photo.width;
                int y = i / photo.width;
                var pixel = photo.GetPixel(x, y);
                hash = hash * 31 + pixel.GetHashCode();
            }
            
            return hash;
        }
    }
    
    #endregion
    
    #region Portrait Manager
    
    /// <summary>
    /// Central manager for all portrait-related functionality.
    /// </summary>
    public class PortraitManager : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private CaricatureGenerator generator;
        [SerializeField] private PortraitRenderer portraitRenderer;
        [SerializeField] private PortraitReactionSystem reactionSystem;
        [SerializeField] private AICaricatureGenerator aiGenerator;
        
        [Header("Configuration")]
        [SerializeField] private bool enableAIGeneration = true;
        [SerializeField] private bool cachePortraits = true;
        [SerializeField] private int maxCachedPortraits = 50;
        
        // Cache
        private Dictionary<string, PortraitConfig> portraitCache;
        private Queue<string> cacheOrder;
        
        // Current state
        private PortraitConfig currentPortrait;
        
        // Events
        public event Action<PortraitConfig> OnPortraitLoaded;
        public event Action<string, Expression> OnCharacterReaction;
        
        private void Awake()
        {
            portraitCache = new Dictionary<string, PortraitConfig>();
            cacheOrder = new Queue<string>();
            
            if (generator == null)
            {
                generator = new CaricatureGenerator();
            }
            
            // Wire up events
            if (reactionSystem != null)
            {
                reactionSystem.OnReactionTriggered += (expr, eventType) =>
                {
                    OnCharacterReaction?.Invoke(currentPortrait?.CharacterId, expr);
                };
            }
        }
        
        #region Portrait Management
        
        /// <summary>
        /// Load or generate a portrait for a character.
        /// </summary>
        public void LoadPortrait(string characterId, string characterName, 
            string background = null, Dictionary<string, float> traits = null)
        {
            // Check cache first
            if (cachePortraits && portraitCache.TryGetValue(characterId, out var cached))
            {
                SetActivePortrait(cached);
                return;
            }
            
            // Generate new portrait
            PortraitConfig portrait;
            
            if (!string.IsNullOrEmpty(background) && traits != null)
            {
                portrait = generator.GenerateFromBackground(characterId, characterName, background, traits);
            }
            else
            {
                portrait = generator.GenerateRandom(characterId, characterName);
            }
            
            // Cache
            if (cachePortraits)
            {
                CachePortrait(characterId, portrait);
            }
            
            SetActivePortrait(portrait);
        }
        
        /// <summary>
        /// Load portrait from a player photo using AI.
        /// </summary>
        public async Task LoadPortraitFromPhoto(string characterId, string characterName, Texture2D photo)
        {
            if (!enableAIGeneration || aiGenerator == null)
            {
                Debug.LogWarning("AI generation not available, using procedural generation");
                LoadPortrait(characterId, characterName);
                return;
            }
            
            var portrait = await aiGenerator.GeneratePortraitFromPhoto(photo, characterId, characterName);
            
            if (cachePortraits)
            {
                CachePortrait(characterId, portrait);
            }
            
            SetActivePortrait(portrait);
        }
        
        /// <summary>
        /// Load a deterministic portrait from a seed.
        /// </summary>
        public void LoadPortraitFromSeed(string characterId, string characterName, int seed)
        {
            var portrait = generator.GenerateFromSeed(characterId, characterName, seed);
            
            if (cachePortraits)
            {
                CachePortrait(characterId, portrait);
            }
            
            SetActivePortrait(portrait);
        }
        
        private void SetActivePortrait(PortraitConfig portrait)
        {
            currentPortrait = portrait;
            
            if (GetComponent<Renderer>() != null)
            {
                portraitRenderer.LoadPortrait(portrait);
            }
            
            OnPortraitLoaded?.Invoke(portrait);
        }
        
        private void CachePortrait(string characterId, PortraitConfig portrait)
        {
            // Evict oldest if at capacity
            while (portraitCache.Count >= maxCachedPortraits && cacheOrder.Count > 0)
            {
                var oldest = cacheOrder.Dequeue();
                portraitCache.Remove(oldest);
            }
            
            portraitCache[characterId] = portrait;
            cacheOrder.Enqueue(characterId);
        }
        
        /// <summary>
        /// Clear the portrait cache.
        /// </summary>
        public void ClearCache()
        {
            portraitCache.Clear();
            cacheOrder.Clear();
        }
        
        #endregion
        
        #region Reactions
        
        /// <summary>
        /// Trigger a reaction on the current portrait.
        /// </summary>
        public void TriggerReaction(string eventType, ReactionIntensity intensity = ReactionIntensity.Moderate)
        {
            if (reactionSystem != null)
            {
                reactionSystem.TriggerReaction(eventType, intensity);
            }
        }
        
        /// <summary>
        /// Set the base expression for the current portrait.
        /// </summary>
        public void SetBaseExpression(Expression expression)
        {
            if (reactionSystem != null)
            {
                reactionSystem.SetBaseExpression(expression);
            }
        }
        
        /// <summary>
        /// Update base expression based on character state.
        /// </summary>
        public void UpdateBaseExpressionFromState(float approval, float stress, float alignment)
        {
            if (reactionSystem != null)
            {
                var expression = reactionSystem.GetBaseExpressionForState(approval, stress, alignment);
                reactionSystem.SetBaseExpression(expression);
            }
        }
        
        #endregion
        
        #region Effects
        
        /// <summary>
        /// Apply a special effect to the current portrait.
        /// </summary>
        public void ApplyEffect(PortraitEffect effect)
        {
            if (GetComponent<Renderer>() != null)
            {
                portraitRenderer.ApplyEffect(effect);
            }
        }
        
        /// <summary>
        /// Clear all effects from the current portrait.
        /// </summary>
        public void ClearEffects()
        {
            if (GetComponent<Renderer>() != null)
            {
                portraitRenderer.ClearEffects();
            }
        }
        
        #endregion
        
        #region Export
        
        /// <summary>
        /// Export the current portrait as a texture.
        /// </summary>
        public Texture2D ExportCurrentPortrait()
        {
            return GetComponent<Renderer>()?.ExportPortrait();
        }
        
        /// <summary>
        /// Export the current portrait with a specific expression.
        /// </summary>
        public Texture2D ExportWithExpression(Expression expression)
        {
            return GetComponent<Renderer>()?.ExportPortraitWithExpression(expression);
        }
        
        /// <summary>
        /// Cache all expression variants for the current portrait.
        /// </summary>
        public void CacheAllExpressions()
        {
            GetComponent<Renderer>()?.CacheAllExpressions();
        }
        
        #endregion
    }
    
    #endregion
}

