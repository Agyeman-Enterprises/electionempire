using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using ElectionEmpire.World;

namespace ElectionEmpire.Core
{
    /// <summary>
    /// Manages game saves - local auto-save, cloud sync, and archives
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        public string SaveName;
        public DateTime SaveDate;
        public DateTime GameTime;
        public ElectionEmpire.Character.Character Character;
        public ElectionEmpire.World.World World; // Save world data
        public string WorldSeed; // Also save seed for regeneration
        public Dictionary<string, object> GameState;
        
        public GameSaveData()
        {
            GameState = new Dictionary<string, object>();
        }
    }
    
    public class SaveManager : MonoBehaviour
    {
        [Header("Save Settings")]
        public float AutoSaveInterval = 300f; // 5 minutes in seconds
        public int MaxAutoSaves = 10;
        
        private string SaveDirectory => Path.Combine(Application.persistentDataPath, "Saves");
        private string AutoSavePath => Path.Combine(SaveDirectory, "autosave.json");
        private float lastAutoSaveTime;
        
        private void Start()
        {
            EnsureSaveDirectory();
            lastAutoSaveTime = Time.time;
        }
        
        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameActive)
            {
                if (Time.time - lastAutoSaveTime >= AutoSaveInterval)
                {
                    AutoSave();
                    lastAutoSaveTime = Time.time;
                }
            }
        }
        
        private void EnsureSaveDirectory()
        {
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
            }
        }
        
        public void SaveGame(string saveName, bool isAutoSave = false)
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsGameActive)
            {
                Debug.LogWarning("Cannot save: No active game");
                return;
            }
            
            var saveData = new GameSaveData
            {
                SaveName = saveName,
                SaveDate = DateTime.Now,
                GameTime = GameManager.Instance.TimeManager.GameTime,
                Character = GameManager.Instance.CurrentCharacter,
                World = GameManager.Instance.CurrentWorld,
                WorldSeed = GameManager.Instance.CurrentWorld?.Seed
            };
            
            string json = JsonUtility.ToJson(saveData, true);
            string filePath = isAutoSave 
                ? AutoSavePath 
                : Path.Combine(SaveDirectory, $"{saveName}.json");
            
            File.WriteAllText(filePath, json);
            Debug.Log($"Game saved: {filePath}");
        }
        
        public void AutoSave()
        {
            SaveGame("autosave", true);
        }
        
        public void QuickSave()
        {
            string quickSaveName = $"quicksave_{DateTime.Now:yyyyMMdd_HHmmss}";
            SaveGame(quickSaveName);
        }
        
        public GameSaveData LoadGame(string saveName)
        {
            string filePath = saveName == "autosave" 
                ? AutoSavePath 
                : Path.Combine(SaveDirectory, $"{saveName}.json");
            
            if (!File.Exists(filePath))
            {
                Debug.LogError($"Save file not found: {filePath}");
                return null;
            }
            
            try
            {
                string json = File.ReadAllText(filePath);
                GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);
                
                // If world is null but seed exists, regenerate world
                if (saveData.World == null && !string.IsNullOrEmpty(saveData.WorldSeed))
                {
                    var worldGenerator = new WorldGenerator();
                    saveData.World = worldGenerator.GenerateWorld(saveData.WorldSeed);
                    Debug.Log($"World regenerated from seed: {saveData.WorldSeed}");
                }
                
                Debug.Log($"Game loaded: {saveName}");
                return saveData;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading save: {e.Message}");
                return null;
            }
        }
        
        public List<string> GetAvailableSaves()
        {
            List<string> saves = new List<string>();
            
            if (!Directory.Exists(SaveDirectory))
                return saves;
            
            string[] files = Directory.GetFiles(SaveDirectory, "*.json");
            foreach (string file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                saves.Add(fileName);
            }
            
            return saves;
        }
        
        public void DeleteSave(string saveName)
        {
            string filePath = saveName == "autosave" 
                ? AutoSavePath 
                : Path.Combine(SaveDirectory, $"{saveName}.json");
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.Log($"Save deleted: {saveName}");
            }
        }
    }
}

