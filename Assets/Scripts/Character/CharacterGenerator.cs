using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ElectionEmpire.Character
{
    public enum RandomMode
    {
        Balanced,  // Mix of good and bad, winnable
        Chaos,     // Maximum weirdness, high chaos
        Hard       // Difficult but viable
    }
    
    /// <summary>
    /// Generates random characters with 3 modes: Balanced, Chaos, Hard
    /// </summary>
    public class CharacterGenerator
    {
        private CharacterDataLoader _dataLoader;
        
        public CharacterGenerator()
        {
            _dataLoader = CharacterDataLoader.Instance;
        }
        
        public Character GenerateRandom(RandomMode mode)
        {
            var character = new Character();
            
            // 1. Select background (weighted by mode)
            character.Background = SelectWeightedBackground(mode);
            
            // 2. Personal history (1-3 items)
            int historyCount = mode == RandomMode.Chaos 
                ? UnityEngine.Random.Range(2, 4) 
                : UnityEngine.Random.Range(1, 3);
            character.PersonalHistory = SelectRandomHistory(historyCount);
            
            // 3. Public image (weighted negative in chaos)
            character.PublicImage = SelectWeightedImage(mode);
            
            // 4. Skills (always 3)
            character.Skills = SelectRandomSkills(3, mode);
            
            // 5. Quirks (2 positive, 2 negative)
            character.PositiveQuirks = SelectRandomQuirks(2, true);
            character.NegativeQuirks = SelectRandomQuirks(2, false);
            
            // 6. Handicaps (0-3, weighted by mode)
            int handicapCount = mode == RandomMode.Chaos 
                ? UnityEngine.Random.Range(1, 4) 
                : (mode == RandomMode.Hard ? UnityEngine.Random.Range(1, 3) : UnityEngine.Random.Range(0, 2));
            character.Handicaps = SelectRandomHandicaps(handicapCount);
            
            // 7. Secret weapon
            character.Weapon = SelectRandomWeapon(mode);
            
            // 8. Generate name
            character.Name = GenerateCharacterName(character);
            character.GeneratedNickname = GenerateNickname(character);
            
            // 9. Calculate chaos rating
            character.ChaosRating = CalculateChaosRating(character);
            
            // 10. Calculate difficulty and legacy bonus
            character.DifficultyMultiplier = CalculateDifficultyMultiplier(character);
            character.LegacyPointBonus = CalculateLegacyBonus(character);
            
            // 11. Ensure viability
            if (!IsViable(character))
            {
                // Reroll if unwinnable (max 5 attempts)
                for (int i = 0; i < 5; i++)
                {
                    character = GenerateRandom(mode);
                    if (IsViable(character))
                        break;
                }
            }
            
            return character;
        }
        
        private BackgroundData SelectWeightedBackground(RandomMode mode)
        {
            var backgrounds = _dataLoader.GetBackgrounds();
            if (backgrounds == null || backgrounds.Count == 0)
                return null;
            
            List<BackgroundData> candidates = new List<BackgroundData>();
            
            switch (mode)
            {
                case RandomMode.Balanced:
                    // Mix of all tiers
                    candidates = backgrounds;
                    break;
                    
                case RandomMode.Chaos:
                    // Weighted toward absurd/criminal
                    candidates = backgrounds.Where(b => 
                        b.Tier == "absurd" || b.Tier == "criminal" || b.Tier == "questionable"
                    ).ToList();
                    if (candidates.Count == 0)
                        candidates = backgrounds;
                    break;
                    
                case RandomMode.Hard:
                    // Mix but avoid easiest
                    candidates = backgrounds.Where(b => 
                        b.Tier != "respectable" || UnityEngine.Random.value > 0.3f
                    ).ToList();
                    if (candidates.Count == 0)
                        candidates = backgrounds;
                    break;
            }
            
            return candidates[UnityEngine.Random.Range(0, candidates.Count)];
        }
        
        private List<string> SelectRandomHistory(int count)
        {
            var history = _dataLoader.GetPersonalHistory();
            if (history == null || history.Count == 0)
                return new List<string>();
            
            var selected = new List<string>();
            var available = new List<string>(history);
            
            for (int i = 0; i < count && available.Count > 0; i++)
            {
                int index = UnityEngine.Random.Range(0, available.Count);
                selected.Add(available[index]);
                available.RemoveAt(index);
            }
            
            return selected;
        }
        
        private PublicImageData SelectWeightedImage(RandomMode mode)
        {
            var images = _dataLoader.GetPublicImages();
            if (images == null || images.Count == 0)
                return null;
            
            // In chaos mode, weight toward negative images
            if (mode == RandomMode.Chaos && UnityEngine.Random.value > 0.3f)
            {
                var negativeImages = images.Where(img => 
                    img.ID == "knowndrunk" || 
                    img.ID == "conspiracytheorist" || 
                    img.ID == "scandalprone"
                ).ToList();
                
                if (negativeImages.Count > 0)
                    return negativeImages[UnityEngine.Random.Range(0, negativeImages.Count)];
            }
            
            return images[UnityEngine.Random.Range(0, images.Count)];
        }
        
        private List<SkillData> SelectRandomSkills(int count, RandomMode mode)
        {
            var skills = _dataLoader.GetSkills();
            if (skills == null || skills.Count == 0)
                return new List<SkillData>();
            
            var selected = new List<SkillData>();
            var available = new List<SkillData>(skills);
            
            // In chaos mode, prefer absurd skills
            if (mode == RandomMode.Chaos)
            {
                var absurdSkills = available.Where(s => s.Category == "absurd").ToList();
                if (absurdSkills.Count > 0 && UnityEngine.Random.value > 0.5f)
                {
                    selected.Add(absurdSkills[UnityEngine.Random.Range(0, absurdSkills.Count)]);
                    available.RemoveAll(s => s.ID == selected[0].ID);
                    count--;
                }
            }
            
            for (int i = 0; i < count && available.Count > 0; i++)
            {
                int index = UnityEngine.Random.Range(0, available.Count);
                selected.Add(available[index]);
                available.RemoveAt(index);
            }
            
            return selected;
        }
        
        private List<QuirkData> SelectRandomQuirks(int count, bool positive)
        {
            var quirks = positive 
                ? _dataLoader.GetPositiveQuirks() 
                : _dataLoader.GetNegativeQuirks();
            
            if (quirks == null || quirks.Count == 0)
                return new List<QuirkData>();
            
            var selected = new List<QuirkData>();
            var available = new List<QuirkData>(quirks);
            
            for (int i = 0; i < count && available.Count > 0; i++)
            {
                int index = UnityEngine.Random.Range(0, available.Count);
                selected.Add(available[index]);
                available.RemoveAt(index);
            }
            
            return selected;
        }
        
        private List<HandicapData> SelectRandomHandicaps(int count)
        {
            var handicaps = _dataLoader.GetHandicaps();
            if (handicaps == null || handicaps.Count == 0)
                return new List<HandicapData>();
            
            var selected = new List<HandicapData>();
            var available = new List<HandicapData>(handicaps);
            
            for (int i = 0; i < count && available.Count > 0; i++)
            {
                int index = UnityEngine.Random.Range(0, available.Count);
                selected.Add(available[index]);
                available.RemoveAt(index);
            }
            
            return selected;
        }
        
        private WeaponData SelectRandomWeapon(RandomMode mode)
        {
            var weapons = _dataLoader.GetWeapons();
            if (weapons == null || weapons.Count == 0)
                return null;
            
            List<WeaponData> candidates = new List<WeaponData>();
            
            switch (mode)
            {
                case RandomMode.Balanced:
                    candidates = weapons;
                    break;
                    
                case RandomMode.Chaos:
                    // Prefer absurd weapons
                    var absurdWeapons = weapons.Where(w => w.Category == "absurd").ToList();
                    candidates = absurdWeapons.Count > 0 && UnityEngine.Random.value > 0.4f
                        ? absurdWeapons
                        : weapons;
                    break;
                    
                case RandomMode.Hard:
                    // Avoid easiest weapons
                    candidates = weapons.Where(w => w.Category != "legitimate" || UnityEngine.Random.value > 0.3f).ToList();
                    if (candidates.Count == 0)
                        candidates = weapons;
                    break;
            }
            
            return candidates[UnityEngine.Random.Range(0, candidates.Count)];
        }
        
        public string GenerateCharacterName(Character character)
        {
            // Simple name generation - can be expanded
            string[] firstNames = { "Alex", "Jordan", "Morgan", "Casey", "Taylor", "Riley", "Avery", "Quinn" };
            string[] lastNames = { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis" };
            
            return $"{firstNames[UnityEngine.Random.Range(0, firstNames.Length)]} {lastNames[UnityEngine.Random.Range(0, lastNames.Length)]}";
        }
        
        public string GenerateNickname(Character character)
        {
            if (character.Background == null)
                return character.Name;
            
            // Generate nickname based on background and traits
            string adjective = "";
            
            // Check for specific traits that create nicknames
            if (character.NegativeQuirks.Any(q => q.ID == "alcoholproblem" || q.ID == "knowndrunk"))
                adjective = "The Drunk ";
            else if (character.Background.Tier == "absurd")
                adjective = "The Absurd ";
            else if (character.PositiveQuirks.Any(q => q.ID == "actuallypsychic"))
                adjective = "The Psychic ";
            
            return $"{adjective}{character.Background.Name}";
        }
        
        public int CalculateChaosRating(Character character)
        {
            int points = 0;
            
            // Background tier
            if (character.Background != null)
            {
                switch (character.Background.Tier)
                {
                    case "absurd": points += 20; break;
                    case "criminal": points += 15; break;
                    case "questionable": points += 10; break;
                }
            }
            
            // Personal history (5 per item)
            points += character.PersonalHistory.Count * 5;
            
            // Public image
            if (character.PublicImage != null)
            {
                if (character.PublicImage.ID == "knowndrunk" || 
                    character.PublicImage.ID == "conspiracytheorist" ||
                    character.PublicImage.ID == "scandalprone")
                    points += 15;
            }
            
            // Absurd skills (10 per skill)
            points += character.Skills.Count(s => s.Category == "absurd") * 10;
            
            // Negative quirks (5 per quirk)
            points += character.NegativeQuirks.Count * 5;
            
            // Handicaps (10 per handicap)
            points += character.Handicaps.Count * 10;
            
            // Absurd weapons (15 points)
            if (character.Weapon != null && character.Weapon.Category == "absurd")
                points += 15;
            
            // Convert to 1-5 rating
            if (points <= 20) return 1;
            if (points <= 40) return 2;
            if (points <= 60) return 3;
            if (points <= 80) return 4;
            return 5;
        }
        
        public float CalculateDifficultyMultiplier(Character character)
        {
            float baseDifficulty = 1.0f;
            
            // Chaos rating increases difficulty
            baseDifficulty += (character.ChaosRating - 1) * 0.2f;
            
            // Handicaps increase difficulty
            baseDifficulty += character.Handicaps.Count * 0.15f;
            
            // Negative quirks increase difficulty
            baseDifficulty += character.NegativeQuirks.Count * 0.1f;
            
            return baseDifficulty;
        }
        
        public float CalculateLegacyBonus(Character character)
        {
            float bonus = 1.0f;
            
            // Higher chaos = bigger rewards
            bonus += (character.ChaosRating - 1) * 0.5f;
            
            // Handicaps give legacy bonuses
            foreach (var handicap in character.Handicaps)
            {
                bonus += handicap.LegacyPointBonus / 100f;
            }
            
            return bonus;
        }
        
        private bool IsViable(Character character)
        {
            // Calculate positive score
            float positiveScore = 0f;
            
            if (character.Background != null)
            {
                positiveScore += 20; // Base background value
                if (character.Background.Stats != null)
                {
                    foreach (var stat in character.Background.Stats.Values)
                    {
                        if (stat > 0) positiveScore += stat;
                    }
                }
            }
            
            positiveScore += character.Skills.Count * 15;
            positiveScore += character.PositiveQuirks.Count * 10;
            
            if (character.Weapon != null)
                positiveScore += 20;
            
            // Calculate negative score
            float negativeScore = 0f;
            
            negativeScore += character.Handicaps.Count * 25;
            negativeScore += character.NegativeQuirks.Count * 15;
            
            if (character.PublicImage != null && character.PublicImage.StatModifiers != null)
            {
                foreach (var mod in character.PublicImage.StatModifiers.Values)
                {
                    if (mod < 0) negativeScore += Mathf.Abs(mod);
                }
            }
            
            // Must have at least 30% positive vs negative
            return positiveScore >= negativeScore * 0.3f;
        }
    }
}

