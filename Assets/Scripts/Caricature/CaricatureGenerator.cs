// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - Caricature Core System
// Sprint 10: Procedural Politician Portrait Generation
// Modular face components, expressions, and dynamic reactions
// ═══════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionEmpire.Caricature
{
    #region Enums & Constants
    
    /// <summary>
    /// Face shape categories for base head generation.
    /// </summary>
    public enum FaceShape
    {
        Oval,
        Round,
        Square,
        Oblong,
        Heart,
        Diamond,
        Triangle,
        Rectangle,
        Pear
    }
    
    /// <summary>
    /// Skin tone ranges for diverse character generation.
    /// </summary>
    public enum SkinTone
    {
        Porcelain,
        Ivory,
        Sand,
        Honey,
        Almond,
        Chestnut,
        Espresso,
        Mahogany,
        Ebony
    }
    
    /// <summary>
    /// Age categories affecting wrinkles, hair, etc.
    /// </summary>
    public enum AgeCategory
    {
        Young,          // 25-35
        MiddleAge,     // 36-50
        Mature,        // 51-65
        Senior,        // 66-80
        Elder          // 80+
    }
    
    /// <summary>
    /// Hair style categories.
    /// </summary>
    public enum HairStyle
    {
        Bald,
        Buzzcut,
        Short,
        Medium,
        Long,
        Slicked,
        Combover,
        Pompadour,
        Afro,
        Curly,
        Wavy,
        Ponytail,
        Bun,
        Braids,
        Mohawk,
        Mullet
    }
    
    /// <summary>
    /// Hair color options.
    /// </summary>
    public enum HairColor
    {
        Black,
        DarkBrown,
        Brown,
        LightBrown,
        Auburn,
        Red,
        Strawberry,
        Blonde,
        PlatinumBlonde,
        Gray,
        White,
        Silver,
        Dyed_Blue,
        Dyed_Purple,
        Dyed_Pink,
        Dyed_Green
    }
    
    /// <summary>
    /// Eye shapes for expression variety.
    /// </summary>
    public enum EyeShape
    {
        Almond,
        Round,
        Hooded,
        Downturned,
        Upturned,
        Monolid,
        DeepSet,
        WideSet,
        CloseSet
    }
    
    /// <summary>
    /// Eye colors.
    /// </summary>
    public enum EyeColor
    {
        Brown,
        DarkBrown,
        Hazel,
        Amber,
        Green,
        Blue,
        Gray,
        Black
    }
    
    /// <summary>
    /// Nose types for character variety.
    /// </summary>
    public enum NoseType
    {
        Button,
        Roman,
        Greek,
        Nubian,
        Hawk,
        Snub,
        Turned,
        Fleshy,
        Bumpy,
        Wide,
        Narrow
    }
    
    /// <summary>
    /// Mouth/lip types.
    /// </summary>
    public enum MouthType
    {
        Thin,
        Full,
        Wide,
        Small,
        HeartShaped,
        Downturned,
        Upturned,
        Bow
    }
    
    /// <summary>
    /// Facial hair options.
    /// </summary>
    public enum FacialHair
    {
        None,
        Stubble,
        Goatee,
        VanDyke,
        Mustache,
        Handlebar,
        FullBeard,
        ShortBeard,
        LongBeard,
        Sideburns,
        MuttonChops,
        SoulPatch
    }
    
    /// <summary>
    /// Accessory types for politicians.
    /// </summary>
    public enum Accessory
    {
        None,
        Glasses_Round,
        Glasses_Square,
        Glasses_Aviator,
        Glasses_Reading,
        Sunglasses,
        Monocle,
        Earring_Stud,
        Earring_Hoop,
        Piercing_Nose,
        Hearing_Aid,
        Eyepatch,
        Bandage
    }
    
    /// <summary>
    /// Expression states for dynamic portraits.
    /// </summary>
    public enum Expression
    {
        Neutral,
        Happy,
        Confident,
        Smug,
        Angry,
        Furious,
        Sad,
        Worried,
        Shocked,
        Surprised,
        Disgusted,
        Scheming,
        Nervous,
        Defeated,
        Victorious,
        Crying,
        Laughing,
        Sneering,
        Pleading,
        Determined,
        Exhausted,
        Drunk,
        Suspicious,
        Guilty
    }
    
    /// <summary>
    /// Special effects overlays for portraits.
    /// </summary>
    public enum PortraitEffect
    {
        None,
        Sweating,
        Blushing,
        Pallid,
        Bruised,
        Crying,
        Spotlight,
        Shadow,
        Glowing,
        Pixelated,          // For censored/scandal
        Redacted,           // Black bars
        Mugshot,
        Campaign_Poster,
        Newspaper,
        TV_Screen,
        Wanted_Poster,
        Memorial            // For political death
    }
    
    #endregion
    
    #region Data Structures
    
    /// <summary>
    /// Complete facial feature data for a character.
    /// </summary>
    [Serializable]
    public class FacialFeatures
    {
        // Base structure
        public FaceShape FaceShape;
        public SkinTone SkinTone;
        public AgeCategory Age;
        public float FaceWidth;         // 0-1 modifier
        public float FaceHeight;        // 0-1 modifier
        public float JawWidth;          // 0-1 modifier
        public float CheekboneHeight;   // 0-1 modifier
        
        // Hair
        public HairStyle HairStyle;
        public HairColor HairColor;
        public float HairVolume;        // 0-1
        public bool IsBalding;
        public float BaldingAmount;     // 0-1
        
        // Eyes
        public EyeShape EyeShape;
        public EyeColor EyeColor;
        public float EyeSize;           // 0-1
        public float EyeSpacing;        // 0-1
        public float EyebrowThickness;  // 0-1
        public float EyebrowArch;       // 0-1
        public bool HasBags;
        public float BagIntensity;      // 0-1
        
        // Nose
        public NoseType NoseType;
        public float NoseSize;          // 0-1
        public float NoseWidth;         // 0-1
        
        // Mouth
        public MouthType MouthType;
        public float MouthWidth;        // 0-1
        public float LipFullness;       // 0-1
        
        // Facial Hair
        public FacialHair FacialHair;
        public HairColor FacialHairColor;
        public float FacialHairDensity; // 0-1
        
        // Details
        public float WrinkleIntensity;  // 0-1
        public float FreckleDensity;    // 0-1
        public float MoleCount;         // 0-5
        public float ScarCount;         // 0-3
        
        // Accessories
        public List<Accessory> Accessories;
        
        // Unique identifiers
        public float Seed;              // For deterministic regeneration
        
        public FacialFeatures()
        {
            Accessories = new List<Accessory>();
        }
        
        /// <summary>
        /// Generate a hash for this face configuration.
        /// </summary>
        public string GetFaceHash()
        {
            return $"{FaceShape}_{SkinTone}_{HairStyle}_{EyeShape}_{NoseType}_{Seed:F4}";
        }
    }
    
    /// <summary>
    /// Expression configuration data.
    /// </summary>
    [Serializable]
    public class ExpressionData
    {
        public Expression Expression;
        
        // Eye modifications
        public float EyeOpenness;       // 0-1 (0=closed, 1=wide)
        public float EyebrowRaise;      // -1 to 1 (negative=frown)
        public float EyebrowFurrow;     // 0-1
        public float PupilDilation;     // 0-1
        public Vector2 EyeDirection;    // Look direction
        
        // Mouth modifications
        public float MouthOpenness;     // 0-1
        public float SmileAmount;       // -1 to 1 (negative=frown)
        public float LipPucker;         // 0-1
        public float TeethVisibility;   // 0-1
        public bool ShowTongue;
        
        // Face modifications
        public float CheekPuff;         // 0-1
        public float NostrilFlare;      // 0-1
        public float Blush;             // 0-1
        public float Pallor;            // 0-1
        public float SweatLevel;        // 0-1
        
        // Animation
        public float TransitionSpeed;   // Seconds to transition
        public bool IsLooping;
        public float LoopIntensity;     // For subtle movement
    }
    
    /// <summary>
    /// Complete portrait configuration.
    /// </summary>
    [Serializable]
    public class PortraitConfig
    {
        public string CharacterId;
        public string CharacterName;
        public FacialFeatures Features;
        public Expression CurrentExpression;
        public PortraitEffect CurrentEffect;
        
        // Clothing/presentation
        public string OutfitId;
        public string BackgroundId;
        
        // State
        public bool IsAnimated;
        public float AnimationSpeed;
        
        // Cached renders
        public Dictionary<Expression, Texture2D> CachedExpressions;
        
        public PortraitConfig()
        {
            CachedExpressions = new Dictionary<Expression, Texture2D>();
        }
    }
    
    #endregion
    
    #region Portrait Generator
    
    /// <summary>
    /// Main system for generating procedural politician portraits.
    /// </summary>
    public class CaricatureGenerator
    {
        private System.Random random;
        private ExpressionLibrary expressionLibrary;
        private FeatureWeights featureWeights;
        
        // Component sprite references (set in Unity)
        private Dictionary<FaceShape, Sprite[]> faceSprites;
        private Dictionary<HairStyle, Sprite[]> hairSprites;
        private Dictionary<EyeShape, Sprite[]> eyeSprites;
        private Dictionary<NoseType, Sprite[]> noseSprites;
        private Dictionary<MouthType, Sprite[]> mouthSprites;
        private Dictionary<FacialHair, Sprite[]> facialHairSprites;
        private Dictionary<Accessory, Sprite[]> accessorySprites;
        
        public CaricatureGenerator(int? seed = null)
        {
            random = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
            expressionLibrary = new ExpressionLibrary();
            featureWeights = new FeatureWeights();
            InitializeSpriteLibraries();
        }
        
        private void InitializeSpriteLibraries()
        {
            faceSprites = new Dictionary<FaceShape, Sprite[]>();
            hairSprites = new Dictionary<HairStyle, Sprite[]>();
            eyeSprites = new Dictionary<EyeShape, Sprite[]>();
            noseSprites = new Dictionary<NoseType, Sprite[]>();
            mouthSprites = new Dictionary<MouthType, Sprite[]>();
            facialHairSprites = new Dictionary<FacialHair, Sprite[]>();
            accessorySprites = new Dictionary<Accessory, Sprite[]>();
        }
        
        /// <summary>
        /// Generate a completely random politician portrait.
        /// </summary>
        public PortraitConfig GenerateRandom(string characterId, string characterName)
        {
            var features = GenerateRandomFeatures();
            return CreatePortrait(characterId, characterName, features);
        }
        
        /// <summary>
        /// Generate a portrait influenced by character background.
        /// </summary>
        public PortraitConfig GenerateFromBackground(
            string characterId,
            string characterName,
            string background,
            Dictionary<string, float> traits)
        {
            var features = GenerateFeaturesByBackground(background, traits);
            return CreatePortrait(characterId, characterName, features);
        }
        
        /// <summary>
        /// Generate a portrait from a seed for deterministic results.
        /// </summary>
        public PortraitConfig GenerateFromSeed(
            string characterId,
            string characterName,
            int seed)
        {
            var seededRandom = new System.Random(seed);
            var features = GenerateRandomFeatures(seededRandom);
            features.Seed = seed;
            return CreatePortrait(characterId, characterName, features);
        }
        
        private PortraitConfig CreatePortrait(
            string characterId,
            string characterName,
            FacialFeatures features)
        {
            return new PortraitConfig
            {
                CharacterId = characterId,
                CharacterName = characterName,
                Features = features,
                CurrentExpression = Expression.Neutral,
                CurrentEffect = PortraitEffect.None,
                OutfitId = SelectOutfit(features),
                BackgroundId = "default",
                IsAnimated = true,
                AnimationSpeed = 1.0f
            };
        }
        
        /// <summary>
        /// Generate completely random facial features.
        /// </summary>
        public FacialFeatures GenerateRandomFeatures(System.Random rng = null)
        {
            var r = rng ?? random;
            
            var features = new FacialFeatures
            {
                // Base structure
                FaceShape = GetRandomEnum<FaceShape>(r),
                SkinTone = GetRandomEnum<SkinTone>(r),
                Age = GetWeightedAge(r),
                FaceWidth = (float)r.NextDouble(),
                FaceHeight = (float)r.NextDouble(),
                JawWidth = (float)r.NextDouble(),
                CheekboneHeight = (float)r.NextDouble(),
                
                // Hair
                HairStyle = GetRandomEnum<HairStyle>(r),
                HairColor = GetRandomEnum<HairColor>(r),
                HairVolume = (float)r.NextDouble(),
                IsBalding = r.NextDouble() < 0.3f,
                BaldingAmount = (float)r.NextDouble(),
                
                // Eyes
                EyeShape = GetRandomEnum<EyeShape>(r),
                EyeColor = GetRandomEnum<EyeColor>(r),
                EyeSize = (float)r.NextDouble(),
                EyeSpacing = (float)r.NextDouble(),
                EyebrowThickness = (float)r.NextDouble(),
                EyebrowArch = (float)r.NextDouble(),
                HasBags = r.NextDouble() < 0.4f,
                BagIntensity = (float)r.NextDouble(),
                
                // Nose
                NoseType = GetRandomEnum<NoseType>(r),
                NoseSize = (float)r.NextDouble(),
                NoseWidth = (float)r.NextDouble(),
                
                // Mouth
                MouthType = GetRandomEnum<MouthType>(r),
                MouthWidth = (float)r.NextDouble(),
                LipFullness = (float)r.NextDouble(),
                
                // Facial hair (weighted by gender probability)
                FacialHair = r.NextDouble() < 0.4f ? GetRandomEnum<FacialHair>(r) : FacialHair.None,
                FacialHairColor = GetRandomEnum<HairColor>(r),
                FacialHairDensity = (float)r.NextDouble(),
                
                // Details
                WrinkleIntensity = (float)r.NextDouble(),
                FreckleDensity = (float)r.NextDouble() * 0.5f,
                MoleCount = r.Next(0, 4),
                ScarCount = r.Next(0, 2),
                
                // Accessories
                Accessories = GenerateAccessories(r),
                
                Seed = (float)r.NextDouble() * 10000
            };
            
            // Apply age-related modifications
            ApplyAgeModifications(features);
            
            return features;
        }
        
        /// <summary>
        /// Generate features influenced by character background.
        /// </summary>
        private FacialFeatures GenerateFeaturesByBackground(
            string background,
            Dictionary<string, float> traits)
        {
            var features = GenerateRandomFeatures();
            
            // Modify based on background
            switch (background.ToLower())
            {
                case "businessman":
                    features.HairStyle = PickFrom(HairStyle.Short, HairStyle.Slicked, HairStyle.Combover);
                    features.Accessories.Add(Accessory.Glasses_Square);
                    features.Age = PickFrom(AgeCategory.MiddleAge, AgeCategory.Mature);
                    break;
                    
                case "local_politician":
                    features.Age = PickFrom(AgeCategory.MiddleAge, AgeCategory.Mature, AgeCategory.Senior);
                    features.WrinkleIntensity = Mathf.Max(features.WrinkleIntensity, 0.4f);
                    break;
                    
                case "teacher":
                    features.Accessories.Add(Accessory.Glasses_Reading);
                    features.HasBags = true;
                    features.BagIntensity = 0.5f;
                    break;
                    
                case "doctor":
                    features.Age = PickFrom(AgeCategory.MiddleAge, AgeCategory.Mature);
                    if (random.NextDouble() < 0.5f)
                        features.Accessories.Add(Accessory.Glasses_Round);
                    break;
                    
                case "police_officer":
                    features.FaceShape = PickFrom(FaceShape.Square, FaceShape.Rectangle);
                    features.JawWidth = Mathf.Max(features.JawWidth, 0.6f);
                    features.HairStyle = PickFrom(HairStyle.Buzzcut, HairStyle.Short);
                    break;
                    
                case "journalist":
                    features.HasBags = true;
                    features.BagIntensity = 0.6f;
                    if (random.NextDouble() < 0.4f)
                        features.HairStyle = HairStyle.Medium; // Changed from "Messy" to valid enum
                    break;
                    
                case "activist":
                    features.Age = PickFrom(AgeCategory.Young, AgeCategory.MiddleAge);
                    if (random.NextDouble() < 0.3f)
                        features.HairColor = PickFrom(HairColor.Dyed_Blue, HairColor.Dyed_Purple, HairColor.Dyed_Pink);
                    break;
                    
                case "religious_leader":
                    features.Age = PickFrom(AgeCategory.Mature, AgeCategory.Senior);
                    features.FacialHair = PickFrom(FacialHair.FullBeard, FacialHair.ShortBeard);
                    features.HairColor = PickFrom(HairColor.Gray, HairColor.White, HairColor.Silver);
                    break;
            }
            
            // Modify based on traits
            if (traits != null)
            {
                if (traits.TryGetValue("charisma", out float charisma) && charisma > 7)
                {
                    // Attractive features
                    features.FaceShape = PickFrom(FaceShape.Oval, FaceShape.Heart);
                    features.EyeSize = Mathf.Max(features.EyeSize, 0.6f);
                }
                
                if (traits.TryGetValue("cunning", out float cunning) && cunning > 7)
                {
                    // Shrewd appearance
                    features.EyeShape = PickFrom(EyeShape.Hooded, EyeShape.DeepSet);
                    features.EyebrowArch = Mathf.Max(features.EyebrowArch, 0.7f);
                }
                
                if (traits.TryGetValue("resilience", out float resilience) && resilience > 7)
                {
                    // Weathered look
                    features.WrinkleIntensity = Mathf.Max(features.WrinkleIntensity, 0.5f);
                    features.JawWidth = Mathf.Max(features.JawWidth, 0.6f);
                }
            }
            
            return features;
        }
        
        /// <summary>
        /// Apply age-related modifications to features.
        /// </summary>
        private void ApplyAgeModifications(FacialFeatures features)
        {
            switch (features.Age)
            {
                case AgeCategory.Young:
                    features.WrinkleIntensity *= 0.1f;
                    features.HasBags = features.HasBags && random.NextDouble() < 0.2f;
                    features.IsBalding = features.IsBalding && random.NextDouble() < 0.1f;
                    break;
                    
                case AgeCategory.MiddleAge:
                    features.WrinkleIntensity = Mathf.Max(features.WrinkleIntensity, 0.2f);
                    if (random.NextDouble() < 0.3f)
                        features.HairColor = HairColor.Gray;
                    break;
                    
                case AgeCategory.Mature:
                    features.WrinkleIntensity = Mathf.Max(features.WrinkleIntensity, 0.4f);
                    features.HasBags = true;
                    features.BagIntensity = Mathf.Max(features.BagIntensity, 0.3f);
                    if (random.NextDouble() < 0.5f)
                        features.HairColor = PickFrom(HairColor.Gray, HairColor.Silver);
                    break;
                    
                case AgeCategory.Senior:
                    features.WrinkleIntensity = Mathf.Max(features.WrinkleIntensity, 0.6f);
                    features.HasBags = true;
                    features.BagIntensity = Mathf.Max(features.BagIntensity, 0.5f);
                    features.HairColor = PickFrom(HairColor.Gray, HairColor.White, HairColor.Silver);
                    features.IsBalding = random.NextDouble() < 0.6f;
                    break;
                    
                case AgeCategory.Elder:
                    features.WrinkleIntensity = Mathf.Max(features.WrinkleIntensity, 0.8f);
                    features.HasBags = true;
                    features.BagIntensity = Mathf.Max(features.BagIntensity, 0.7f);
                    features.HairColor = HairColor.White;
                    features.IsBalding = random.NextDouble() < 0.8f;
                    features.HairVolume *= 0.5f;
                    break;
            }
        }
        
        private List<Accessory> GenerateAccessories(System.Random r)
        {
            var accessories = new List<Accessory>();
            
            // Glasses (30% chance)
            if (r.NextDouble() < 0.3f)
            {
                accessories.Add(PickFrom(r,
                    Accessory.Glasses_Round,
                    Accessory.Glasses_Square,
                    Accessory.Glasses_Reading));
            }
            
            // Earrings (10% chance)
            if (r.NextDouble() < 0.1f)
            {
                accessories.Add(PickFrom(r,
                    Accessory.Earring_Stud,
                    Accessory.Earring_Hoop));
            }
            
            return accessories;
        }
        
        private AgeCategory GetWeightedAge(System.Random r)
        {
            // Politicians tend to be older
            var roll = r.NextDouble();
            if (roll < 0.1f) return AgeCategory.Young;
            if (roll < 0.35f) return AgeCategory.MiddleAge;
            if (roll < 0.65f) return AgeCategory.Mature;
            if (roll < 0.9f) return AgeCategory.Senior;
            return AgeCategory.Elder;
        }
        
        private string SelectOutfit(FacialFeatures features)
        {
            // Select appropriate outfit based on features
            return features.Age switch
            {
                AgeCategory.Young => "casual_professional",
                AgeCategory.Elder => "formal_traditional",
                _ => "formal_standard"
            };
        }
        
        #region Helper Methods
        
        private T GetRandomEnum<T>(System.Random r) where T : Enum
        {
            var values = Enum.GetValues(typeof(T));
            return (T)values.GetValue(r.Next(values.Length));
        }
        
        private T PickFrom<T>(params T[] options)
        {
            return options[random.Next(options.Length)];
        }
        
        private T PickFrom<T>(System.Random r, params T[] options)
        {
            return options[r.Next(options.Length)];
        }
        
        #endregion
    }
    
    #endregion
    
    #region Expression Library
    
    /// <summary>
    /// Library of expression configurations for different emotional states.
    /// </summary>
    public class ExpressionLibrary
    {
        private Dictionary<Expression, ExpressionData> expressions;
        
        public ExpressionLibrary()
        {
            expressions = new Dictionary<Expression, ExpressionData>();
            InitializeExpressions();
        }
        
        private void InitializeExpressions()
        {
            // Neutral
            expressions[Expression.Neutral] = new ExpressionData
            {
                Expression = Expression.Neutral,
                EyeOpenness = 0.7f,
                EyebrowRaise = 0f,
                EyebrowFurrow = 0f,
                MouthOpenness = 0f,
                SmileAmount = 0f,
                TransitionSpeed = 0.3f
            };
            
            // Happy
            expressions[Expression.Happy] = new ExpressionData
            {
                Expression = Expression.Happy,
                EyeOpenness = 0.6f,
                EyebrowRaise = 0.2f,
                SmileAmount = 0.7f,
                TeethVisibility = 0.3f,
                CheekPuff = 0.2f,
                TransitionSpeed = 0.4f
            };
            
            // Confident
            expressions[Expression.Confident] = new ExpressionData
            {
                Expression = Expression.Confident,
                EyeOpenness = 0.8f,
                EyebrowRaise = 0.1f,
                SmileAmount = 0.3f,
                EyeDirection = new Vector2(0, 0.1f), // Slight upward look
                TransitionSpeed = 0.5f
            };
            
            // Smug
            expressions[Expression.Smug] = new ExpressionData
            {
                Expression = Expression.Smug,
                EyeOpenness = 0.5f,
                EyebrowRaise = 0.3f,
                SmileAmount = 0.4f,
                EyeDirection = new Vector2(0.2f, 0.1f),
                TransitionSpeed = 0.4f
            };
            
            // Angry
            expressions[Expression.Angry] = new ExpressionData
            {
                Expression = Expression.Angry,
                EyeOpenness = 0.9f,
                EyebrowRaise = -0.5f,
                EyebrowFurrow = 0.8f,
                MouthOpenness = 0.1f,
                SmileAmount = -0.4f,
                NostrilFlare = 0.5f,
                TransitionSpeed = 0.2f
            };
            
            // Furious
            expressions[Expression.Furious] = new ExpressionData
            {
                Expression = Expression.Furious,
                EyeOpenness = 1.0f,
                EyebrowRaise = -0.7f,
                EyebrowFurrow = 1.0f,
                MouthOpenness = 0.4f,
                SmileAmount = -0.6f,
                TeethVisibility = 0.5f,
                NostrilFlare = 0.8f,
                Blush = 0.3f,
                SweatLevel = 0.2f,
                TransitionSpeed = 0.15f
            };
            
            // Sad
            expressions[Expression.Sad] = new ExpressionData
            {
                Expression = Expression.Sad,
                EyeOpenness = 0.5f,
                EyebrowRaise = 0.3f,
                EyebrowFurrow = 0.4f,
                SmileAmount = -0.5f,
                EyeDirection = new Vector2(0, -0.2f),
                Pallor = 0.2f,
                TransitionSpeed = 0.5f
            };
            
            // Worried
            expressions[Expression.Worried] = new ExpressionData
            {
                Expression = Expression.Worried,
                EyeOpenness = 0.9f,
                EyebrowRaise = 0.5f,
                EyebrowFurrow = 0.3f,
                SmileAmount = -0.2f,
                SweatLevel = 0.3f,
                TransitionSpeed = 0.3f
            };
            
            // Shocked
            expressions[Expression.Shocked] = new ExpressionData
            {
                Expression = Expression.Shocked,
                EyeOpenness = 1.0f,
                PupilDilation = 0.8f,
                EyebrowRaise = 0.9f,
                MouthOpenness = 0.7f,
                Pallor = 0.4f,
                TransitionSpeed = 0.1f
            };
            
            // Surprised
            expressions[Expression.Surprised] = new ExpressionData
            {
                Expression = Expression.Surprised,
                EyeOpenness = 1.0f,
                PupilDilation = 0.5f,
                EyebrowRaise = 0.7f,
                MouthOpenness = 0.4f,
                TransitionSpeed = 0.15f
            };
            
            // Disgusted
            expressions[Expression.Disgusted] = new ExpressionData
            {
                Expression = Expression.Disgusted,
                EyeOpenness = 0.4f,
                EyebrowFurrow = 0.6f,
                SmileAmount = -0.3f,
                NostrilFlare = 0.3f,
                LipPucker = 0.4f,
                TransitionSpeed = 0.3f
            };
            
            // Scheming
            expressions[Expression.Scheming] = new ExpressionData
            {
                Expression = Expression.Scheming,
                EyeOpenness = 0.5f,
                EyebrowRaise = 0.2f,
                SmileAmount = 0.3f,
                EyeDirection = new Vector2(0.3f, 0),
                TransitionSpeed = 0.5f,
                IsLooping = true,
                LoopIntensity = 0.1f
            };
            
            // Nervous
            expressions[Expression.Nervous] = new ExpressionData
            {
                Expression = Expression.Nervous,
                EyeOpenness = 0.9f,
                EyebrowRaise = 0.4f,
                SmileAmount = 0.1f,
                SweatLevel = 0.5f,
                TransitionSpeed = 0.2f,
                IsLooping = true,
                LoopIntensity = 0.2f
            };
            
            // Defeated
            expressions[Expression.Defeated] = new ExpressionData
            {
                Expression = Expression.Defeated,
                EyeOpenness = 0.4f,
                EyebrowRaise = 0.2f,
                SmileAmount = -0.4f,
                EyeDirection = new Vector2(0, -0.3f),
                Pallor = 0.3f,
                TransitionSpeed = 0.6f
            };
            
            // Victorious
            expressions[Expression.Victorious] = new ExpressionData
            {
                Expression = Expression.Victorious,
                EyeOpenness = 0.8f,
                EyebrowRaise = 0.4f,
                SmileAmount = 1.0f,
                TeethVisibility = 0.6f,
                Blush = 0.2f,
                TransitionSpeed = 0.3f
            };
            
            // Crying
            expressions[Expression.Crying] = new ExpressionData
            {
                Expression = Expression.Crying,
                EyeOpenness = 0.3f,
                EyebrowRaise = 0.5f,
                EyebrowFurrow = 0.5f,
                SmileAmount = -0.6f,
                Blush = 0.4f,
                SweatLevel = 0.3f, // Tears
                TransitionSpeed = 0.4f,
                IsLooping = true,
                LoopIntensity = 0.15f
            };
            
            // Laughing
            expressions[Expression.Laughing] = new ExpressionData
            {
                Expression = Expression.Laughing,
                EyeOpenness = 0.3f,
                EyebrowRaise = 0.3f,
                MouthOpenness = 0.6f,
                SmileAmount = 1.0f,
                TeethVisibility = 0.7f,
                CheekPuff = 0.3f,
                TransitionSpeed = 0.2f,
                IsLooping = true,
                LoopIntensity = 0.3f
            };
            
            // Sneering
            expressions[Expression.Sneering] = new ExpressionData
            {
                Expression = Expression.Sneering,
                EyeOpenness = 0.6f,
                EyebrowRaise = 0.2f,
                SmileAmount = 0.3f,
                LipPucker = 0.3f,
                NostrilFlare = 0.2f,
                TransitionSpeed = 0.4f
            };
            
            // Pleading
            expressions[Expression.Pleading] = new ExpressionData
            {
                Expression = Expression.Pleading,
                EyeOpenness = 1.0f,
                PupilDilation = 0.6f,
                EyebrowRaise = 0.7f,
                EyebrowFurrow = 0.3f,
                SmileAmount = -0.2f,
                EyeDirection = new Vector2(0, 0.2f),
                TransitionSpeed = 0.3f
            };
            
            // Determined
            expressions[Expression.Determined] = new ExpressionData
            {
                Expression = Expression.Determined,
                EyeOpenness = 0.8f,
                EyebrowFurrow = 0.5f,
                SmileAmount = 0f,
                MouthOpenness = 0.05f,
                TransitionSpeed = 0.4f
            };
            
            // Exhausted
            expressions[Expression.Exhausted] = new ExpressionData
            {
                Expression = Expression.Exhausted,
                EyeOpenness = 0.3f,
                EyebrowRaise = -0.1f,
                SmileAmount = -0.1f,
                Pallor = 0.4f,
                SweatLevel = 0.2f,
                TransitionSpeed = 0.6f
            };
            
            // Drunk
            expressions[Expression.Drunk] = new ExpressionData
            {
                Expression = Expression.Drunk,
                EyeOpenness = 0.4f,
                EyebrowRaise = 0.1f,
                SmileAmount = 0.4f,
                Blush = 0.6f,
                EyeDirection = new Vector2(0.1f, -0.1f),
                TransitionSpeed = 0.5f,
                IsLooping = true,
                LoopIntensity = 0.25f
            };
            
            // Suspicious
            expressions[Expression.Suspicious] = new ExpressionData
            {
                Expression = Expression.Suspicious,
                EyeOpenness = 0.5f,
                EyebrowFurrow = 0.4f,
                SmileAmount = -0.1f,
                EyeDirection = new Vector2(0.2f, 0),
                TransitionSpeed = 0.4f
            };
            
            // Guilty
            expressions[Expression.Guilty] = new ExpressionData
            {
                Expression = Expression.Guilty,
                EyeOpenness = 0.6f,
                EyebrowRaise = 0.3f,
                EyebrowFurrow = 0.2f,
                SmileAmount = -0.3f,
                EyeDirection = new Vector2(-0.2f, -0.2f),
                SweatLevel = 0.4f,
                Pallor = 0.2f,
                TransitionSpeed = 0.4f
            };
        }
        
        /// <summary>
        /// Get expression data for the specified expression.
        /// </summary>
        public ExpressionData GetExpression(Expression expression)
        {
            return expressions.TryGetValue(expression, out var data) 
                ? data 
                : expressions[Expression.Neutral];
        }
        
        /// <summary>
        /// Get all available expressions.
        /// </summary>
        public IEnumerable<Expression> GetAllExpressions()
        {
            return expressions.Keys;
        }
        
        /// <summary>
        /// Blend two expressions together.
        /// </summary>
        public ExpressionData BlendExpressions(
            Expression from,
            Expression to,
            float t)
        {
            var fromData = GetExpression(from);
            var toData = GetExpression(to);
            
            return new ExpressionData
            {
                Expression = t < 0.5f ? from : to,
                EyeOpenness = Mathf.Lerp(fromData.EyeOpenness, toData.EyeOpenness, t),
                EyebrowRaise = Mathf.Lerp(fromData.EyebrowRaise, toData.EyebrowRaise, t),
                EyebrowFurrow = Mathf.Lerp(fromData.EyebrowFurrow, toData.EyebrowFurrow, t),
                PupilDilation = Mathf.Lerp(fromData.PupilDilation, toData.PupilDilation, t),
                EyeDirection = Vector2.Lerp(fromData.EyeDirection, toData.EyeDirection, t),
                MouthOpenness = Mathf.Lerp(fromData.MouthOpenness, toData.MouthOpenness, t),
                SmileAmount = Mathf.Lerp(fromData.SmileAmount, toData.SmileAmount, t),
                LipPucker = Mathf.Lerp(fromData.LipPucker, toData.LipPucker, t),
                TeethVisibility = Mathf.Lerp(fromData.TeethVisibility, toData.TeethVisibility, t),
                CheekPuff = Mathf.Lerp(fromData.CheekPuff, toData.CheekPuff, t),
                NostrilFlare = Mathf.Lerp(fromData.NostrilFlare, toData.NostrilFlare, t),
                Blush = Mathf.Lerp(fromData.Blush, toData.Blush, t),
                Pallor = Mathf.Lerp(fromData.Pallor, toData.Pallor, t),
                SweatLevel = Mathf.Lerp(fromData.SweatLevel, toData.SweatLevel, t),
                TransitionSpeed = toData.TransitionSpeed,
                IsLooping = toData.IsLooping,
                LoopIntensity = Mathf.Lerp(fromData.LoopIntensity, toData.LoopIntensity, t)
            };
        }
    }
    
    #endregion
    
    #region Feature Weights
    
    /// <summary>
    /// Weights for feature generation based on demographics and statistics.
    /// </summary>
    public class FeatureWeights
    {
        // Skin tone distribution weights
        public Dictionary<SkinTone, float> SkinToneWeights;
        
        // Eye color distribution by skin tone
        public Dictionary<SkinTone, Dictionary<EyeColor, float>> EyeColorBySkinTone;
        
        // Hair color probability by age
        public Dictionary<AgeCategory, Dictionary<HairColor, float>> HairColorByAge;
        
        public FeatureWeights()
        {
            InitializeWeights();
        }
        
        private void InitializeWeights()
        {
            // Balanced global distribution
            SkinToneWeights = new Dictionary<SkinTone, float>
            {
                { SkinTone.Porcelain, 0.08f },
                { SkinTone.Ivory, 0.12f },
                { SkinTone.Sand, 0.12f },
                { SkinTone.Honey, 0.14f },
                { SkinTone.Almond, 0.14f },
                { SkinTone.Chestnut, 0.14f },
                { SkinTone.Espresso, 0.10f },
                { SkinTone.Mahogany, 0.08f },
                { SkinTone.Ebony, 0.08f }
            };
            
            // Eye color distributions
            EyeColorBySkinTone = new Dictionary<SkinTone, Dictionary<EyeColor, float>>();
            
            // Light skin tones - more eye color variety
            var lightEyes = new Dictionary<EyeColor, float>
            {
                { EyeColor.Blue, 0.25f },
                { EyeColor.Green, 0.15f },
                { EyeColor.Hazel, 0.15f },
                { EyeColor.Brown, 0.30f },
                { EyeColor.Gray, 0.10f },
                { EyeColor.Amber, 0.05f }
            };
            
            EyeColorBySkinTone[SkinTone.Porcelain] = lightEyes;
            EyeColorBySkinTone[SkinTone.Ivory] = lightEyes;
            
            // Medium skin tones
            var mediumEyes = new Dictionary<EyeColor, float>
            {
                { EyeColor.Brown, 0.50f },
                { EyeColor.DarkBrown, 0.20f },
                { EyeColor.Hazel, 0.15f },
                { EyeColor.Green, 0.08f },
                { EyeColor.Amber, 0.07f }
            };
            
            EyeColorBySkinTone[SkinTone.Sand] = mediumEyes;
            EyeColorBySkinTone[SkinTone.Honey] = mediumEyes;
            EyeColorBySkinTone[SkinTone.Almond] = mediumEyes;
            
            // Dark skin tones
            var darkEyes = new Dictionary<EyeColor, float>
            {
                { EyeColor.DarkBrown, 0.60f },
                { EyeColor.Brown, 0.30f },
                { EyeColor.Black, 0.08f },
                { EyeColor.Amber, 0.02f }
            };
            
            EyeColorBySkinTone[SkinTone.Chestnut] = darkEyes;
            EyeColorBySkinTone[SkinTone.Espresso] = darkEyes;
            EyeColorBySkinTone[SkinTone.Mahogany] = darkEyes;
            EyeColorBySkinTone[SkinTone.Ebony] = darkEyes;
        }
    }
    
    #endregion
}

