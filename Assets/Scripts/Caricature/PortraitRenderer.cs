// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - Portrait Renderer System
// Sprint 10: Dynamic portrait rendering with expressions and effects
// ═══════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ElectionEmpire.Caricature
{
    #region Render Configuration
    
    /// <summary>
    /// Configuration for portrait rendering.
    /// </summary>
    [Serializable]
    public class RenderConfig
    {
        public int TextureWidth = 512;
        public int TextureHeight = 512;
        public bool EnableAntiAliasing = true;
        public int AntiAliasLevel = 4;
        public bool EnableShadows = true;
        public float ShadowIntensity = 0.3f;
        public bool EnableOutline = true;
        public float OutlineWidth = 2f;
        public Color OutlineColor = Color.black;
    }
    
    /// <summary>
    /// Layer ordering for portrait composition.
    /// </summary>
    public enum PortraitLayer
    {
        Background = 0,
        BackHair = 10,
        Ears = 20,
        Neck = 30,
        Face = 40,
        FaceDetails = 50,      // Wrinkles, moles, scars
        Eyes = 60,
        Eyebrows = 70,
        Nose = 80,
        Mouth = 90,
        FacialHair = 100,
        FrontHair = 110,
        Accessories = 120,
        Effects = 130,
        Overlay = 140
    }
    
    #endregion
    
    #region Portrait Renderer
    
    /// <summary>
    /// Renders procedural politician portraits with dynamic expressions.
    /// </summary>
    public class PortraitRenderer : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private RenderConfig config;
        
        [Header("Sprite Libraries")]
        [SerializeField] private FaceSpriteLibrary faceSpriteLibrary;
        [SerializeField] private HairSpriteLibrary hairSpriteLibrary;
        [SerializeField] private EyeSpriteLibrary eyeSpriteLibrary;
        [SerializeField] private NoseSpriteLibrary noseSpriteLibrary;
        [SerializeField] private MouthSpriteLibrary mouthSpriteLibrary;
        [SerializeField] private AccessorySpriteLibrary accessorySpriteLibrary;
        [SerializeField] private EffectSpriteLibrary effectSpriteLibrary;
        
        [Header("Rendering")]
        [SerializeField] private RenderTexture renderTexture;
        [SerializeField] private Camera portraitCamera;
        
        // Current state
        private PortraitConfig currentPortrait;
        private Expression currentExpression;
        private Expression targetExpression;
        private float expressionTransitionProgress;
        private bool isTransitioning;
        
        // Cached expression data
        private ExpressionLibrary expressionLibrary;
        private ExpressionData currentExpressionData;
        private ExpressionData targetExpressionData;
        
        // Layer renderers
        private Dictionary<PortraitLayer, SpriteRenderer> layerRenderers;
        
        // Animation state
        private float animationTime;
        private float blinkTimer;
        private float nextBlinkTime;
        private bool isBlinking;
        
        // Events
        public event Action<Expression> OnExpressionChanged;
        public event Action<PortraitEffect> OnEffectApplied;
        
        private void Awake()
        {
            expressionLibrary = new ExpressionLibrary();
            layerRenderers = new Dictionary<PortraitLayer, SpriteRenderer>();
            
            if (config == null)
            {
                config = new RenderConfig();
            }
            
            InitializeRenderTexture();
            InitializeLayerRenderers();
        }
        
        private void Update()
        {
            if (currentPortrait == null) return;
            
            animationTime += Time.deltaTime;
            
            // Handle expression transition
            if (isTransitioning)
            {
                UpdateExpressionTransition();
            }
            
            // Handle idle animations
            UpdateIdleAnimations();
            
            // Handle blinking
            UpdateBlinking();
        }
        
        #region Initialization
        
        private void InitializeRenderTexture()
        {
            if (renderTexture == null)
            {
                renderTexture = new RenderTexture(
                    config.TextureWidth,
                    config.TextureHeight,
                    24,
                    RenderTextureFormat.ARGB32);
                
                if (config.EnableAntiAliasing)
                {
                    renderTexture.antiAliasing = config.AntiAliasLevel;
                }
            }
            
            if (portraitCamera != null)
            {
                portraitCamera.targetTexture = renderTexture;
            }
        }
        
        private void InitializeLayerRenderers()
        {
            foreach (PortraitLayer layer in Enum.GetValues(typeof(PortraitLayer)))
            {
                var go = new GameObject($"Layer_{layer}");
                go.transform.SetParent(transform);
                go.transform.localPosition = Vector3.zero;
                
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sortingOrder = (int)layer;
                
                layerRenderers[layer] = sr;
            }
        }
        
        #endregion
        
        #region Portrait Loading
        
        /// <summary>
        /// Load and render a portrait configuration.
        /// </summary>
        public void LoadPortrait(PortraitConfig portrait)
        {
            currentPortrait = portrait;
            currentExpression = portrait.CurrentExpression;
            currentExpressionData = expressionLibrary.GetExpression(currentExpression);
            
            // Clear existing layers
            ClearAllLayers();
            
            // Render each component
            RenderFace(portrait.Features);
            RenderHair(portrait.Features);
            RenderEyes(portrait.Features, currentExpressionData);
            RenderEyebrows(portrait.Features, currentExpressionData);
            RenderNose(portrait.Features);
            RenderMouth(portrait.Features, currentExpressionData);
            RenderFacialHair(portrait.Features);
            RenderAccessories(portrait.Features);
            RenderFaceDetails(portrait.Features);
            
            // Apply any active effects
            if (portrait.CurrentEffect != PortraitEffect.None)
            {
                ApplyEffect(portrait.CurrentEffect);
            }
            
            // Reset animation timers
            ResetAnimationTimers();
        }
        
        private void ClearAllLayers()
        {
            foreach (var kvp in layerRenderers)
            {
                kvp.Value.sprite = null;
                kvp.Value.color = Color.white;
            }
        }
        
        private void ResetAnimationTimers()
        {
            animationTime = 0f;
            blinkTimer = 0f;
            nextBlinkTime = UnityEngine.Random.Range(2f, 6f);
            isBlinking = false;
        }
        
        #endregion
        
        #region Component Rendering
        
        private void RenderFace(FacialFeatures features)
        {
            if (faceSpriteLibrary == null) return;
            
            var faceSprite = faceSpriteLibrary.GetFaceSprite(
                features.FaceShape,
                features.SkinTone,
                features.FaceWidth,
                features.FaceHeight);
            
            if (faceSprite != null)
            {
                layerRenderers[PortraitLayer.Face].sprite = faceSprite;
                
                // Apply skin tone tint if using grayscale sprites
                var skinColor = GetSkinColor(features.SkinTone);
                layerRenderers[PortraitLayer.Face].color = skinColor;
            }
        }
        
        private void RenderHair(FacialFeatures features)
        {
            if (hairSpriteLibrary == null) return;
            
            // Back hair layer (for styles that go behind head)
            var backHairSprite = hairSpriteLibrary.GetBackHairSprite(
                features.HairStyle,
                features.IsBalding,
                features.BaldingAmount);
            
            if (backHairSprite != null)
            {
                layerRenderers[PortraitLayer.BackHair].sprite = backHairSprite;
                layerRenderers[PortraitLayer.BackHair].color = GetHairColor(features.HairColor);
            }
            
            // Front hair layer
            var frontHairSprite = hairSpriteLibrary.GetFrontHairSprite(
                features.HairStyle,
                features.HairVolume,
                features.IsBalding,
                features.BaldingAmount);
            
            if (frontHairSprite != null)
            {
                layerRenderers[PortraitLayer.FrontHair].sprite = frontHairSprite;
                layerRenderers[PortraitLayer.FrontHair].color = GetHairColor(features.HairColor);
            }
        }
        
        private void RenderEyes(FacialFeatures features, ExpressionData expression)
        {
            if (eyeSpriteLibrary == null) return;
            
            var eyeSprite = eyeSpriteLibrary.GetEyeSprite(
                features.EyeShape,
                features.EyeSize,
                expression.EyeOpenness,
                features.HasBags,
                features.BagIntensity);
            
            if (eyeSprite != null)
            {
                layerRenderers[PortraitLayer.Eyes].sprite = eyeSprite;
                
                // Apply eye color to iris portion
                // Note: This would ideally use a shader with multiple color regions
                var irisColor = GetEyeColor(features.EyeColor);
                if (layerRenderers[PortraitLayer.Eyes].material != null)
                {
                    layerRenderers[PortraitLayer.Eyes].material.SetColor("_IrisColor", irisColor);
                }
            }
            
            // Apply expression-based eye direction
            ApplyEyeDirection(expression.EyeDirection);
        }
        
        private void RenderEyebrows(FacialFeatures features, ExpressionData expression)
        {
            if (eyeSpriteLibrary == null) return;
            
            var eyebrowSprite = eyeSpriteLibrary.GetEyebrowSprite(
                features.EyebrowThickness,
                features.EyebrowArch,
                expression.EyebrowRaise,
                expression.EyebrowFurrow);
            
            if (eyebrowSprite != null)
            {
                layerRenderers[PortraitLayer.Eyebrows].sprite = eyebrowSprite;
                layerRenderers[PortraitLayer.Eyebrows].color = GetHairColor(features.HairColor);
            }
        }
        
        private void RenderNose(FacialFeatures features)
        {
            if (noseSpriteLibrary == null) return;
            
            var noseSprite = noseSpriteLibrary.GetNoseSprite(
                features.NoseType,
                features.NoseSize,
                features.NoseWidth);
            
            if (noseSprite != null)
            {
                layerRenderers[PortraitLayer.Nose].sprite = noseSprite;
                layerRenderers[PortraitLayer.Nose].color = GetSkinColor(features.SkinTone);
            }
        }
        
        private void RenderMouth(FacialFeatures features, ExpressionData expression)
        {
            if (mouthSpriteLibrary == null) return;
            
            var mouthSprite = mouthSpriteLibrary.GetMouthSprite(
                features.MouthType,
                features.MouthWidth,
                features.LipFullness,
                expression.MouthOpenness,
                expression.SmileAmount,
                expression.TeethVisibility);
            
            if (mouthSprite != null)
            {
                layerRenderers[PortraitLayer.Mouth].sprite = mouthSprite;
                
                // Lip color - slightly darker/redder than skin
                var lipColor = GetLipColor(features.SkinTone);
                layerRenderers[PortraitLayer.Mouth].color = lipColor;
            }
        }
        
        private void RenderFacialHair(FacialFeatures features)
        {
            if (features.FacialHair == FacialHair.None) return;
            if (faceSpriteLibrary == null) return;
            
            var facialHairSprite = faceSpriteLibrary.GetFacialHairSprite(
                features.FacialHair,
                features.FacialHairDensity);
            
            if (facialHairSprite != null)
            {
                layerRenderers[PortraitLayer.FacialHair].sprite = facialHairSprite;
                layerRenderers[PortraitLayer.FacialHair].color = GetHairColor(features.FacialHairColor);
            }
        }
        
        private void RenderAccessories(FacialFeatures features)
        {
            if (accessorySpriteLibrary == null) return;
            if (features.Accessories == null || features.Accessories.Count == 0) return;
            
            // For simplicity, render the first accessory
            // Full implementation would layer multiple accessories
            var accessory = features.Accessories[0];
            var accessorySprite = accessorySpriteLibrary.GetAccessorySprite(accessory);
            
            if (accessorySprite != null)
            {
                layerRenderers[PortraitLayer.Accessories].sprite = accessorySprite;
            }
        }
        
        private void RenderFaceDetails(FacialFeatures features)
        {
            if (faceSpriteLibrary == null) return;
            
            // Wrinkles
            if (features.WrinkleIntensity > 0.1f)
            {
                var wrinkleSprite = faceSpriteLibrary.GetWrinkleSprite(
                    features.Age,
                    features.WrinkleIntensity);
                
                if (wrinkleSprite != null)
                {
                    // Use additive or multiply blend
                    var detailRenderer = layerRenderers[PortraitLayer.FaceDetails];
                    detailRenderer.sprite = wrinkleSprite;
                    detailRenderer.color = new Color(1, 1, 1, features.WrinkleIntensity * 0.5f);
                }
            }
        }
        
        #endregion
        
        #region Expression System
        
        /// <summary>
        /// Change the portrait expression with smooth transition.
        /// </summary>
        public void SetExpression(Expression expression, bool instant = false)
        {
            if (expression == currentExpression && !instant) return;
            
            targetExpression = expression;
            targetExpressionData = expressionLibrary.GetExpression(expression);
            
            if (instant)
            {
                currentExpression = expression;
                currentExpressionData = targetExpressionData;
                ApplyExpressionImmediate();
            }
            else
            {
                isTransitioning = true;
                expressionTransitionProgress = 0f;
            }
        }
        
        private void UpdateExpressionTransition()
        {
            if (targetExpressionData == null) return;
            
            float transitionSpeed = targetExpressionData.TransitionSpeed;
            expressionTransitionProgress += Time.deltaTime / transitionSpeed;
            
            if (expressionTransitionProgress >= 1f)
            {
                expressionTransitionProgress = 1f;
                isTransitioning = false;
                currentExpression = targetExpression;
                currentExpressionData = targetExpressionData;
                OnExpressionChanged?.Invoke(currentExpression);
            }
            
            // Blend expressions
            var blendedExpression = expressionLibrary.BlendExpressions(
                currentExpression,
                targetExpression,
                Mathf.SmoothStep(0, 1, expressionTransitionProgress));
            
            ApplyExpressionData(blendedExpression);
        }
        
        private void ApplyExpressionImmediate()
        {
            ApplyExpressionData(currentExpressionData);
            OnExpressionChanged?.Invoke(currentExpression);
        }
        
        private void ApplyExpressionData(ExpressionData data)
        {
            if (currentPortrait == null) return;
            
            // Update eyes
            RenderEyes(currentPortrait.Features, data);
            RenderEyebrows(currentPortrait.Features, data);
            
            // Update mouth
            RenderMouth(currentPortrait.Features, data);
            
            // Apply face modifications
            ApplyFaceModifiers(data);
        }
        
        private void ApplyFaceModifiers(ExpressionData data)
        {
            // Apply blush
            if (data.Blush > 0.05f)
            {
                ApplyBlush(data.Blush);
            }
            
            // Apply pallor
            if (data.Pallor > 0.05f)
            {
                ApplyPallor(data.Pallor);
            }
            
            // Apply sweat
            if (data.SweatLevel > 0.05f)
            {
                ApplySweat(data.SweatLevel);
            }
        }
        
        private void ApplyBlush(float intensity)
        {
            // Overlay red tint on cheeks
            if (effectSpriteLibrary != null)
            {
                var blushSprite = effectSpriteLibrary.GetBlushSprite();
                if (blushSprite != null)
                {
                    layerRenderers[PortraitLayer.Effects].sprite = blushSprite;
                    layerRenderers[PortraitLayer.Effects].color = new Color(1, 0.3f, 0.3f, intensity * 0.5f);
                }
            }
        }
        
        private void ApplyPallor(float intensity)
        {
            // Reduce saturation on face layer
            var faceColor = layerRenderers[PortraitLayer.Face].color;
            var desaturated = Color.Lerp(faceColor, new Color(0.8f, 0.85f, 0.8f), intensity);
            layerRenderers[PortraitLayer.Face].color = desaturated;
        }
        
        private void ApplySweat(float intensity)
        {
            if (effectSpriteLibrary != null)
            {
                var sweatSprite = effectSpriteLibrary.GetSweatSprite();
                if (sweatSprite != null)
                {
                    // Add sweat drops overlay
                    var effectRenderer = layerRenderers[PortraitLayer.Effects];
                    effectRenderer.sprite = sweatSprite;
                    effectRenderer.color = new Color(1, 1, 1, intensity * 0.7f);
                }
            }
        }
        
        private void ApplyEyeDirection(Vector2 direction)
        {
            // Shift eye sprite position based on look direction
            if (layerRenderers.TryGetValue(PortraitLayer.Eyes, out var eyeRenderer))
            {
                var offset = direction * 0.02f; // Small offset
                eyeRenderer.transform.localPosition = new Vector3(offset.x, offset.y, 0);
            }
        }
        
        #endregion
        
        #region Idle Animations
        
        private void UpdateIdleAnimations()
        {
            if (currentExpressionData == null) return;
            
            // Subtle idle movement for looping expressions
            if (currentExpressionData.IsLooping && currentExpressionData.LoopIntensity > 0)
            {
                float loopOffset = Mathf.Sin(animationTime * 2f) * currentExpressionData.LoopIntensity;
                
                // Apply subtle position/rotation variations
                ApplyIdleOffset(loopOffset);
            }
        }
        
        private void ApplyIdleOffset(float offset)
        {
            // Subtle head movement
            transform.localPosition = new Vector3(
                offset * 0.01f,
                Mathf.Abs(offset) * 0.005f,
                0);
            
            // Subtle rotation
            transform.localRotation = Quaternion.Euler(0, 0, offset * 2f);
        }
        
        private void UpdateBlinking()
        {
            blinkTimer += Time.deltaTime;
            
            if (!isBlinking && blinkTimer >= nextBlinkTime)
            {
                StartBlink();
            }
            
            if (isBlinking)
            {
                UpdateBlinkAnimation();
            }
        }
        
        private void StartBlink()
        {
            isBlinking = true;
            blinkTimer = 0f;
        }
        
        private void UpdateBlinkAnimation()
        {
            float blinkDuration = 0.15f;
            float blinkProgress = blinkTimer / blinkDuration;
            
            if (blinkProgress >= 1f)
            {
                // End blink
                isBlinking = false;
                blinkTimer = 0f;
                nextBlinkTime = UnityEngine.Random.Range(2f, 6f);
                
                // Restore normal eye openness
                if (eyeSpriteLibrary != null && currentPortrait != null)
                {
                    RenderEyes(currentPortrait.Features, currentExpressionData);
                }
            }
            else
            {
                // Calculate blink curve (close then open)
                float blinkCurve = blinkProgress < 0.5f
                    ? 1f - (blinkProgress * 2f)  // Close
                    : (blinkProgress - 0.5f) * 2f; // Open
                
                // Apply blink to eye sprite
                ApplyBlinkFrame(blinkCurve);
            }
        }
        
        private void ApplyBlinkFrame(float openness)
        {
            if (eyeSpriteLibrary != null && currentPortrait != null)
            {
                var blinkExpression = new ExpressionData
                {
                    EyeOpenness = currentExpressionData.EyeOpenness * openness
                };
                
                var eyeSprite = eyeSpriteLibrary.GetEyeSprite(
                    currentPortrait.Features.EyeShape,
                    currentPortrait.Features.EyeSize,
                    blinkExpression.EyeOpenness,
                    currentPortrait.Features.HasBags,
                    currentPortrait.Features.BagIntensity);
                
                if (eyeSprite != null)
                {
                    layerRenderers[PortraitLayer.Eyes].sprite = eyeSprite;
                }
            }
        }
        
        #endregion
        
        #region Portrait Effects
        
        /// <summary>
        /// Apply a special effect to the portrait.
        /// </summary>
        public void ApplyEffect(PortraitEffect effect)
        {
            if (effectSpriteLibrary == null) return;
            
            switch (effect)
            {
                case PortraitEffect.Sweating:
                    ApplySweat(0.8f);
                    break;
                    
                case PortraitEffect.Blushing:
                    ApplyBlush(0.6f);
                    break;
                    
                case PortraitEffect.Pallid:
                    ApplyPallor(0.5f);
                    break;
                    
                case PortraitEffect.Spotlight:
                    ApplySpotlight();
                    break;
                    
                case PortraitEffect.Shadow:
                    ApplyShadow();
                    break;
                    
                case PortraitEffect.Pixelated:
                    ApplyPixelation();
                    break;
                    
                case PortraitEffect.Redacted:
                    ApplyRedaction();
                    break;
                    
                case PortraitEffect.Mugshot:
                    ApplyMugshotFrame();
                    break;
                    
                case PortraitEffect.Campaign_Poster:
                    ApplyCampaignPosterFrame();
                    break;
                    
                case PortraitEffect.Newspaper:
                    ApplyNewspaperFrame();
                    break;
                    
                case PortraitEffect.Wanted_Poster:
                    ApplyWantedPosterFrame();
                    break;
            }
            
            if (currentPortrait != null)
            {
                currentPortrait.CurrentEffect = effect;
            }
            
            OnEffectApplied?.Invoke(effect);
        }
        
        /// <summary>
        /// Remove all effects from the portrait.
        /// </summary>
        public void ClearEffects()
        {
            layerRenderers[PortraitLayer.Effects].sprite = null;
            layerRenderers[PortraitLayer.Overlay].sprite = null;
            
            // Reset face color
            if (currentPortrait != null)
            {
                layerRenderers[PortraitLayer.Face].color = 
                    GetSkinColor(currentPortrait.Features.SkinTone);
                currentPortrait.CurrentEffect = PortraitEffect.None;
            }
        }
        
        private void ApplySpotlight()
        {
            var overlaySprite = effectSpriteLibrary.GetOverlaySprite("spotlight");
            if (overlaySprite != null)
            {
                layerRenderers[PortraitLayer.Overlay].sprite = overlaySprite;
                layerRenderers[PortraitLayer.Overlay].color = new Color(1, 1, 0.9f, 0.3f);
            }
        }
        
        private void ApplyShadow()
        {
            var overlaySprite = effectSpriteLibrary.GetOverlaySprite("shadow");
            if (overlaySprite != null)
            {
                layerRenderers[PortraitLayer.Overlay].sprite = overlaySprite;
                layerRenderers[PortraitLayer.Overlay].color = new Color(0, 0, 0, 0.5f);
            }
        }
        
        private void ApplyPixelation()
        {
            // Apply pixelation shader to render texture
            // This would use a post-processing effect
            Debug.Log("Pixelation effect applied - requires shader implementation");
        }
        
        private void ApplyRedaction()
        {
            var overlaySprite = effectSpriteLibrary.GetOverlaySprite("redacted_bars");
            if (overlaySprite != null)
            {
                layerRenderers[PortraitLayer.Overlay].sprite = overlaySprite;
                layerRenderers[PortraitLayer.Overlay].color = Color.black;
            }
        }
        
        private void ApplyMugshotFrame()
        {
            var frameSprite = effectSpriteLibrary.GetFrameSprite("mugshot");
            if (frameSprite != null)
            {
                layerRenderers[PortraitLayer.Overlay].sprite = frameSprite;
            }
            
            // Set background to mugshot gray
            layerRenderers[PortraitLayer.Background].color = new Color(0.7f, 0.7f, 0.7f);
        }
        
        private void ApplyCampaignPosterFrame()
        {
            var frameSprite = effectSpriteLibrary.GetFrameSprite("campaign_poster");
            if (frameSprite != null)
            {
                layerRenderers[PortraitLayer.Overlay].sprite = frameSprite;
            }
            
            // Apply posterize effect to colors
            // This would use a shader
        }
        
        private void ApplyNewspaperFrame()
        {
            var frameSprite = effectSpriteLibrary.GetFrameSprite("newspaper");
            if (frameSprite != null)
            {
                layerRenderers[PortraitLayer.Overlay].sprite = frameSprite;
            }
            
            // Desaturate for newspaper look
            foreach (var kvp in layerRenderers)
            {
                if (kvp.Key != PortraitLayer.Overlay && kvp.Key != PortraitLayer.Background)
                {
                    var col = kvp.Value.color;
                    float gray = col.r * 0.3f + col.g * 0.59f + col.b * 0.11f;
                    kvp.Value.color = new Color(gray, gray * 0.95f, gray * 0.9f, col.a);
                }
            }
        }
        
        private void ApplyWantedPosterFrame()
        {
            var frameSprite = effectSpriteLibrary.GetFrameSprite("wanted_poster");
            if (frameSprite != null)
            {
                layerRenderers[PortraitLayer.Overlay].sprite = frameSprite;
            }
            
            // Sepia tone
            foreach (var kvp in layerRenderers)
            {
                if (kvp.Key != PortraitLayer.Overlay)
                {
                    var col = kvp.Value.color;
                    float gray = col.r * 0.3f + col.g * 0.59f + col.b * 0.11f;
                    kvp.Value.color = new Color(
                        gray + 0.2f,
                        gray + 0.1f,
                        gray - 0.1f,
                        col.a);
                }
            }
        }
        
        #endregion
        
        #region Color Utilities
        
        private Color GetSkinColor(SkinTone tone)
        {
            return tone switch
            {
                SkinTone.Porcelain => new Color(0.98f, 0.93f, 0.90f),
                SkinTone.Ivory => new Color(0.95f, 0.87f, 0.80f),
                SkinTone.Sand => new Color(0.90f, 0.78f, 0.65f),
                SkinTone.Honey => new Color(0.85f, 0.70f, 0.55f),
                SkinTone.Almond => new Color(0.78f, 0.60f, 0.45f),
                SkinTone.Chestnut => new Color(0.65f, 0.45f, 0.32f),
                SkinTone.Espresso => new Color(0.50f, 0.35f, 0.25f),
                SkinTone.Mahogany => new Color(0.40f, 0.28f, 0.20f),
                SkinTone.Ebony => new Color(0.30f, 0.22f, 0.18f),
                _ => Color.white
            };
        }
        
        private Color GetHairColor(HairColor color)
        {
            return color switch
            {
                HairColor.Black => new Color(0.1f, 0.08f, 0.08f),
                HairColor.DarkBrown => new Color(0.25f, 0.15f, 0.10f),
                HairColor.Brown => new Color(0.40f, 0.28f, 0.18f),
                HairColor.LightBrown => new Color(0.55f, 0.40f, 0.25f),
                HairColor.Auburn => new Color(0.55f, 0.25f, 0.15f),
                HairColor.Red => new Color(0.65f, 0.20f, 0.12f),
                HairColor.Strawberry => new Color(0.80f, 0.45f, 0.35f),
                HairColor.Blonde => new Color(0.85f, 0.75f, 0.55f),
                HairColor.PlatinumBlonde => new Color(0.95f, 0.92f, 0.85f),
                HairColor.Gray => new Color(0.60f, 0.60f, 0.62f),
                HairColor.White => new Color(0.92f, 0.92f, 0.95f),
                HairColor.Silver => new Color(0.75f, 0.78f, 0.82f),
                HairColor.Dyed_Blue => new Color(0.20f, 0.40f, 0.85f),
                HairColor.Dyed_Purple => new Color(0.55f, 0.20f, 0.70f),
                HairColor.Dyed_Pink => new Color(0.90f, 0.45f, 0.70f),
                HairColor.Dyed_Green => new Color(0.20f, 0.70f, 0.40f),
                _ => Color.gray
            };
        }
        
        private Color GetEyeColor(EyeColor color)
        {
            return color switch
            {
                EyeColor.Brown => new Color(0.45f, 0.30f, 0.18f),
                EyeColor.DarkBrown => new Color(0.28f, 0.18f, 0.10f),
                EyeColor.Hazel => new Color(0.55f, 0.45f, 0.25f),
                EyeColor.Amber => new Color(0.70f, 0.50f, 0.20f),
                EyeColor.Green => new Color(0.35f, 0.55f, 0.35f),
                EyeColor.Blue => new Color(0.35f, 0.55f, 0.80f),
                EyeColor.Gray => new Color(0.55f, 0.58f, 0.62f),
                EyeColor.Black => new Color(0.12f, 0.10f, 0.10f),
                _ => Color.gray
            };
        }
        
        private Color GetLipColor(SkinTone skinTone)
        {
            var skinColor = GetSkinColor(skinTone);
            // Lips are slightly darker and redder than skin
            return new Color(
                skinColor.r * 0.85f + 0.1f,
                skinColor.g * 0.70f,
                skinColor.b * 0.70f);
        }
        
        #endregion
        
        #region Export
        
        /// <summary>
        /// Export the current portrait as a Texture2D.
        /// </summary>
        public Texture2D ExportPortrait()
        {
            // Render to texture
            if (portraitCamera != null)
            {
                portraitCamera.Render();
            }
            
            // Read from render texture
            var texture = new Texture2D(
                config.TextureWidth,
                config.TextureHeight,
                TextureFormat.ARGB32,
                false);
            
            RenderTexture.active = renderTexture;
            texture.ReadPixels(new Rect(0, 0, config.TextureWidth, config.TextureHeight), 0, 0);
            texture.Apply();
            RenderTexture.active = null;
            
            return texture;
        }
        
        /// <summary>
        /// Export portrait for a specific expression.
        /// </summary>
        public Texture2D ExportPortraitWithExpression(Expression expression)
        {
            var previousExpression = currentExpression;
            SetExpression(expression, instant: true);
            var texture = ExportPortrait();
            SetExpression(previousExpression, instant: true);
            return texture;
        }
        
        /// <summary>
        /// Cache all expressions for faster runtime switching.
        /// </summary>
        public void CacheAllExpressions()
        {
            if (currentPortrait == null) return;
            
            foreach (Expression expr in Enum.GetValues(typeof(Expression)))
            {
                if (!currentPortrait.CachedExpressions.ContainsKey(expr))
                {
                    var texture = ExportPortraitWithExpression(expr);
                    currentPortrait.CachedExpressions[expr] = texture;
                }
            }
        }
        
        #endregion
    }
    
    #endregion
    
    #region Sprite Libraries (Interfaces)
    
    /// <summary>
    /// Interface for face sprite library.
    /// </summary>
    public abstract class FaceSpriteLibrary : ScriptableObject
    {
        public abstract Sprite GetFaceSprite(FaceShape shape, SkinTone tone, float width, float height);
        public abstract Sprite GetFacialHairSprite(FacialHair style, float density);
        public abstract Sprite GetWrinkleSprite(AgeCategory age, float intensity);
    }
    
    /// <summary>
    /// Interface for hair sprite library.
    /// </summary>
    public abstract class HairSpriteLibrary : ScriptableObject
    {
        public abstract Sprite GetBackHairSprite(HairStyle style, bool isBalding, float baldingAmount);
        public abstract Sprite GetFrontHairSprite(HairStyle style, float volume, bool isBalding, float baldingAmount);
    }
    
    /// <summary>
    /// Interface for eye sprite library.
    /// </summary>
    public abstract class EyeSpriteLibrary : ScriptableObject
    {
        public abstract Sprite GetEyeSprite(EyeShape shape, float size, float openness, bool hasBags, float bagIntensity);
        public abstract Sprite GetEyebrowSprite(float thickness, float arch, float raise, float furrow);
    }
    
    /// <summary>
    /// Interface for nose sprite library.
    /// </summary>
    public abstract class NoseSpriteLibrary : ScriptableObject
    {
        public abstract Sprite GetNoseSprite(NoseType type, float size, float width);
    }
    
    /// <summary>
    /// Interface for mouth sprite library.
    /// </summary>
    public abstract class MouthSpriteLibrary : ScriptableObject
    {
        public abstract Sprite GetMouthSprite(MouthType type, float width, float fullness, 
            float openness, float smile, float teethVisibility);
    }
    
    /// <summary>
    /// Interface for accessory sprite library.
    /// </summary>
    public abstract class AccessorySpriteLibrary : ScriptableObject
    {
        public abstract Sprite GetAccessorySprite(Accessory accessory);
    }
    
    /// <summary>
    /// Interface for effect sprite library.
    /// </summary>
    public abstract class EffectSpriteLibrary : ScriptableObject
    {
        public abstract Sprite GetBlushSprite();
        public abstract Sprite GetSweatSprite();
        public abstract Sprite GetOverlaySprite(string effectName);
        public abstract Sprite GetFrameSprite(string frameName);
    }
    
    #endregion
}

