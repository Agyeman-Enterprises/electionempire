using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionEmpire.Character
{
    /// <summary>
    /// Complete character data structure with all components
    /// </summary>
    [Serializable]
    public class Character
    {
        [Header("Identity")]
        public string Name;
        public string GeneratedNickname;
        public string FullName;

        [Header("Core Components")]
        public BackgroundData Background;
        public List<string> PersonalHistory;
        public PublicImageData PublicImage;
        public List<SkillData> Skills;
        public List<QuirkData> PositiveQuirks;
        public List<QuirkData> NegativeQuirks;
        public List<HandicapData> Handicaps;
        public WeaponData Weapon;

        [Header("Calculated Properties")]
        public int ChaosRating;
        public float DifficultyMultiplier;
        public float LegacyPointBonus;

        [Header("Stats")]
        public CharacterStats Stats;

        [Header("Political Info")]
        public string CurrentOffice;
        public string CurrentOfficeId;
        public int CurrentTier;
        public string Party;

        [Header("Resources")]
        public Dictionary<string, float> Resources;

        [Header("Active Events")]
        public List<string> ActiveScandals;
        public List<string> ActiveCrises;

        [Header("Play History")]
        public int TotalTurnsPlayed;
        
        public Character()
        {
            PersonalHistory = new List<string>();
            Skills = new List<SkillData>();
            PositiveQuirks = new List<QuirkData>();
            NegativeQuirks = new List<QuirkData>();
            Handicaps = new List<HandicapData>();
            Stats = new CharacterStats();
            Resources = new Dictionary<string, float>();
            ActiveScandals = new List<string>();
            ActiveCrises = new List<string>();
            FullName = "";
            CurrentOffice = "";
            CurrentOfficeId = "";
            CurrentTier = 1;
            Party = "";
            TotalTurnsPlayed = 0;
        }
        
        public Character Clone()
        {
            var clone = new Character
            {
                Name = this.Name,
                GeneratedNickname = this.GeneratedNickname,
                Background = this.Background,
                PersonalHistory = new List<string>(this.PersonalHistory),
                PublicImage = this.PublicImage,
                Skills = new List<SkillData>(this.Skills),
                PositiveQuirks = new List<QuirkData>(this.PositiveQuirks),
                NegativeQuirks = new List<QuirkData>(this.NegativeQuirks),
                Handicaps = new List<HandicapData>(this.Handicaps),
                Weapon = this.Weapon,
                ChaosRating = this.ChaosRating,
                DifficultyMultiplier = this.DifficultyMultiplier,
                LegacyPointBonus = this.LegacyPointBonus,
                Stats = this.Stats
            };
            return clone;
        }
    }
    
    [Serializable]
    public class CharacterStats
    {
        public float Charisma;
        public float Intelligence;
        public float Cunning;
        public float Resilience;
        public float Networking;
        public float Intimidation;
        public float Sophistication;
        public float MediaAppeal;
        public float PublicTrust;
        public float NameRecognition;
        
        public CharacterStats()
        {
            // Default values
        }
    }
    
    [Serializable]
    public class BackgroundData
    {
        public string ID;
        public string Name;
        public string Tier; // respectable, questionable, absurd, criminal, celebrity
        public string Description;
        public Dictionary<string, float> Stats;
        public string SpecialAbility;
        public Dictionary<string, float> StartingResources;
        
        public BackgroundData()
        {
            Stats = new Dictionary<string, float>();
            StartingResources = new Dictionary<string, float>();
        }
    }
    
    [Serializable]
    public class PublicImageData
    {
        public string ID;
        public string Name;
        public string Description;
        public Dictionary<string, float> StatModifiers;
        
        public PublicImageData()
        {
            StatModifiers = new Dictionary<string, float>();
        }
    }
    
    [Serializable]
    public class SkillData
    {
        public string ID;
        public string Name;
        public string Category; // political, combat, media, shady, absurd
        public string Description;
        public Dictionary<string, float> Effects;
        
        public SkillData()
        {
            Effects = new Dictionary<string, float>();
        }
    }
    
    [Serializable]
    public class QuirkData
    {
        public string ID;
        public string Name;
        public bool IsPositive;
        public string Description;
        public Dictionary<string, float> Effects;
        
        public QuirkData()
        {
            Effects = new Dictionary<string, float>();
        }
    }
    
    [Serializable]
    public class HandicapData
    {
        public string ID;
        public string Name;
        public string Category; // legal, financial, absurd
        public string Description;
        public Dictionary<string, float> Penalties;
        public float LegacyPointBonus;
        
        public HandicapData()
        {
            Penalties = new Dictionary<string, float>();
        }
    }
    
    [Serializable]
    public class WeaponData
    {
        public string ID;
        public string Name;
        public string Category; // legitimate, questionable, absurd
        public string Description;
        public Dictionary<string, float> Effects;
        
        public WeaponData()
        {
            Effects = new Dictionary<string, float>();
        }
    }
}

