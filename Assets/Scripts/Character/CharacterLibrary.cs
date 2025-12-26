using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using System.Text;

namespace ElectionEmpire.Character
{
    [Serializable]
    public class SavedCharacter
    {
        public string ID;
        public string CustomName;
        public Character Character;
        public DateTime CreatedDate;
        public int TimesUsed;
        public string BestResult; // e.g., "Mayor", "President", etc.
    }
    
    [Serializable]
    public class SavedCharacterList
    {
        public List<SavedCharacter> Characters;
        
        public SavedCharacterList()
        {
            Characters = new List<SavedCharacter>();
        }
    }
    
    /// <summary>
    /// Manages saved characters - save, load, share, delete
    /// </summary>
    public class CharacterLibrary
    {
        private static CharacterLibrary _instance;
        public static CharacterLibrary Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new CharacterLibrary();
                return _instance;
            }
        }
        
        private List<SavedCharacter> _savedCharacters = new List<SavedCharacter>();
        private string LibraryPath => Path.Combine(Application.persistentDataPath, "CharacterLibrary.json");
        
        private CharacterLibrary()
        {
            LoadLibrary();
        }
        
        public void SaveCharacter(Character character, string customName)
        {
            var saved = new SavedCharacter
            {
                ID = Guid.NewGuid().ToString(),
                CustomName = customName,
                Character = character,
                CreatedDate = DateTime.Now,
                TimesUsed = 0,
                BestResult = null
            };
            
            _savedCharacters.Add(saved);
            SaveToFile();
            Debug.Log($"Character saved to library: {customName}");
        }
        
        public Character LoadCharacter(string characterID)
        {
            var saved = _savedCharacters.FirstOrDefault(c => c.ID == characterID);
            if (saved == null)
            {
                Debug.LogError($"Character not found: {characterID}");
                return null;
            }
            
            saved.TimesUsed++;
            SaveToFile();
            
            return saved.Character.Clone();
        }
        
        public void DeleteCharacter(string characterID)
        {
            _savedCharacters.RemoveAll(c => c.ID == characterID);
            SaveToFile();
            Debug.Log($"Character deleted: {characterID}");
        }
        
        public void UpdateBestResult(string characterID, string result)
        {
            var saved = _savedCharacters.FirstOrDefault(c => c.ID == characterID);
            if (saved != null)
            {
                saved.BestResult = result;
                SaveToFile();
            }
        }
        
        public string ShareCharacter(string characterID)
        {
            var character = _savedCharacters.FirstOrDefault(c => c.ID == characterID);
            if (character == null)
                return null;
            
            try
            {
                var json = JsonUtility.ToJson(character.Character);
                var bytes = Encoding.UTF8.GetBytes(json);
                var compressed = CompressString(json);
                var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(compressed));
                return "CHAR-" + encoded;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error sharing character: {e.Message}");
                return null;
            }
        }
        
        public bool ImportCharacter(string shareCode)
        {
            try
            {
                if (!shareCode.StartsWith("CHAR-"))
                    return false;
                
                var encoded = shareCode.Replace("CHAR-", "");
                var compressed = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
                var json = DecompressString(compressed);
                var character = JsonUtility.FromJson<ElectionEmpire.Character.Character>(json);
                
                if (character != null)
                {
                    string name = character.GeneratedNickname ?? character.Name;
                    SaveCharacter(character, $"{name} (Imported)");
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error importing character: {e.Message}");
            }
            
            return false;
        }
        
        public List<SavedCharacter> GetAllCharacters()
        {
            return new List<SavedCharacter>(_savedCharacters);
        }
        
        private void SaveToFile()
        {
            try
            {
                var wrapper = new SavedCharacterList { Characters = _savedCharacters };
                var json = JsonUtility.ToJson(wrapper, true);
                File.WriteAllText(LibraryPath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving library: {e.Message}");
            }
        }
        
        private void LoadLibrary()
        {
            if (!File.Exists(LibraryPath))
            {
                _savedCharacters = new List<SavedCharacter>();
                return;
            }
            
            try
            {
                var json = File.ReadAllText(LibraryPath);
                var wrapper = JsonUtility.FromJson<SavedCharacterList>(json);
                _savedCharacters = wrapper?.Characters ?? new List<SavedCharacter>();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading library: {e.Message}");
                _savedCharacters = new List<SavedCharacter>();
            }
        }
        
        // Simple compression (for production, use proper compression)
        private string CompressString(string text)
        {
            // For now, just return the text
            // In production, use System.IO.Compression
            return text;
        }
        
        private string DecompressString(string compressed)
        {
            return compressed;
        }
    }
}

