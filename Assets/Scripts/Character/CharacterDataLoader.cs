using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace ElectionEmpire.Character
{
    /// <summary>
    /// Loads character component data from JSON files
    /// </summary>
    public class CharacterDataLoader : MonoBehaviour
    {
        private static CharacterDataLoader _instance;
        public static CharacterDataLoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("CharacterDataLoader");
                    _instance = go.AddComponent<CharacterDataLoader>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        
        private string DataPath => Path.Combine(Application.streamingAssetsPath, "Data");
        
        // Cached data
        private List<BackgroundData> _backgrounds;
        private List<string> _personalHistory;
        private List<PublicImageData> _publicImages;
        private List<SkillData> _skills;
        private List<QuirkData> _quirks;
        private List<HandicapData> _handicaps;
        private List<WeaponData> _weapons;
        
        private bool _isLoaded = false;
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                LoadAllData();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        public void LoadAllData()
        {
            if (_isLoaded) return;
            
            _backgrounds = LoadJson<List<BackgroundData>>("backgrounds.json");
            _personalHistory = LoadJson<List<string>>("personalHistory.json");
            _publicImages = LoadJson<List<PublicImageData>>("publicImages.json");
            _skills = LoadJson<List<SkillData>>("skills.json");
            _quirks = LoadJson<List<QuirkData>>("quirks.json");
            _handicaps = LoadJson<List<HandicapData>>("handicaps.json");
            _weapons = LoadJson<List<WeaponData>>("weapons.json");
            
            _isLoaded = true;
            Debug.Log("Character data loaded successfully");
        }
        
        private T LoadJson<T>(string filename)
        {
            string filePath = Path.Combine(DataPath, filename);
            
            if (!File.Exists(filePath))
            {
                Debug.LogError($"Data file not found: {filePath}");
                return default(T);
            }
            
            try
            {
                string json = File.ReadAllText(filePath);
                T data = JsonConvert.DeserializeObject<T>(json);
                return data;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading {filename}: {e.Message}");
                return default(T);
            }
        }
        
        // Getters
        public List<BackgroundData> GetBackgrounds() => _backgrounds ?? new List<BackgroundData>();
        public List<string> GetPersonalHistory() => _personalHistory ?? new List<string>();
        public List<PublicImageData> GetPublicImages() => _publicImages ?? new List<PublicImageData>();
        public List<SkillData> GetSkills() => _skills ?? new List<SkillData>();
        public List<QuirkData> GetQuirks() => _quirks ?? new List<QuirkData>();
        public List<HandicapData> GetHandicaps() => _handicaps ?? new List<HandicapData>();
        public List<WeaponData> GetWeapons() => _weapons ?? new List<WeaponData>();
        
        // Filtered getters
        public List<BackgroundData> GetBackgroundsByTier(string tier)
        {
            return GetBackgrounds().FindAll(b => b.Tier == tier);
        }
        
        public List<SkillData> GetSkillsByCategory(string category)
        {
            return GetSkills().FindAll(s => s.Category == category);
        }
        
        public List<QuirkData> GetPositiveQuirks()
        {
            return GetQuirks().FindAll(q => q.IsPositive);
        }
        
        public List<QuirkData> GetNegativeQuirks()
        {
            return GetQuirks().FindAll(q => !q.IsPositive);
        }
    }
}

