using UnityEngine;
using System;
using ElectionEmpire.World;
using ElectionEmpire.AI;
using ElectionEmpire.Gameplay;
using ElectionEmpire.Managers;
using GameplayGameState = ElectionEmpire.Gameplay.GameState;

namespace ElectionEmpire.Core
{
    /// <summary>
    /// Main game manager that coordinates all systems
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        
        [Header("Managers")]
        public TimeManager TimeManager;
        public SaveManager SaveManager;
        public ElectionEmpire.AI.AIManager AIManager;
        
        [Header("Current Game State")]
        public ElectionEmpire.Character.Character CurrentCharacter;
        public ElectionEmpire.World.World CurrentWorld;
        public PlayerState CurrentPlayer;
        public VoterSimulation VoterSimulation;
        public bool IsGameActive = false;
        public bool HasActiveGame => IsGameActive;
        
        [Header("AI Settings")]
        public int NumberOfAIOpponents = 3;
        public AIDifficulty AIDifficulty = AIDifficulty.Normal;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeManagers();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeManagers()
        {
            if (TimeManager == null)
                TimeManager = GetComponent<TimeManager>();
            if (TimeManager == null)
                TimeManager = gameObject.AddComponent<TimeManager>();
                
            if (SaveManager == null)
                SaveManager = GetComponent<SaveManager>();
            if (SaveManager == null)
                SaveManager = gameObject.AddComponent<SaveManager>();
            
            if (AIManager == null)
                AIManager = FindFirstObjectByType<ElectionEmpire.AI.AIManager>();
            if (AIManager == null)
            {
                GameObject aiManagerObj = new GameObject("AIManager");
                AIManager = aiManagerObj.AddComponent<AIManager>();
            }
        }
        
        public void StartNewCampaign(ElectionEmpire.Character.Character character, string worldSeed = null, int aiCount = 3, AIDifficulty aiDifficulty = AIDifficulty.Normal)
        {
            CurrentCharacter = character;
            NumberOfAIOpponents = aiCount;
            AIDifficulty = aiDifficulty;
            
            // Generate world
            var worldGenerator = new WorldGenerator();
            CurrentWorld = worldGenerator.GenerateWorld(worldSeed);
            
            // Create player state
            CurrentPlayer = new PlayerState(character);
            
            // Initialize voter simulation
            VoterSimulation = new VoterSimulation(CurrentWorld);
            
            // Setup AI Manager
            if (AIManager != null)
            {
                AIManager.CurrentWorld = CurrentWorld;
                AIManager.VoterSimulation = VoterSimulation;
                AIManager.GenerateAIOpponents(aiCount, aiDifficulty, 1); // Player tier 1 for now
            }
            
            // Start game loop
            var gameLoop = FindFirstObjectByType<GameLoop>();
            if (gameLoop == null)
            {
                GameObject loopObj = new GameObject("GameLoop");
                gameLoop = loopObj.AddComponent<GameLoop>();
            }
            
            var setup = new CampaignSetup
            {
                PlayerCharacter = character,
                World = CurrentWorld,
                AIOpponents = AIManager != null ? AIManager.GetAIOpponents() : new System.Collections.Generic.List<AIOpponent>(),
                VictoryCondition = VictoryConditionManager.VictoryType.ReachPresident,
                Difficulty = aiDifficulty
            };
            
            gameLoop.StartCampaign(setup);
            
            IsGameActive = true;
            TimeManager.StartGame();
            
            Debug.Log($"Started new campaign with character: {character.Name}");
            Debug.Log($"World generated: {CurrentWorld.Nation.Regions.Count} regions, " +
                     $"{CurrentWorld.Nation.TotalPopulation:N0} population, seed: {CurrentWorld.Seed}");
            Debug.Log($"Generated {aiCount} AI opponents");
        }
        
        /// <summary>
        /// Process AI turns (called each game turn)
        /// </summary>
        public void ProcessAITurns()
        {
            if (AIManager == null || !IsGameActive)
                return;
            
            // Create game state for AI
            int daysUntilElection = 365; // Default
            if (CurrentPlayer?.CurrentOffice != null && CurrentPlayer.TermEndDate != default)
            {
                daysUntilElection = Mathf.Max(0, (int)(CurrentPlayer.TermEndDate - System.DateTime.Now).TotalDays);
            }
            
            var gameState = new GameplayGameState
            {
                Opponents = AIManager.GetAIOpponents(),
                PlayerApproval = VoterSimulation.CalculateNationalApproval(CurrentPlayer),
                DaysUntilElection = daysUntilElection
            };
            
            // Process all AI turns
            AIManager.ProcessAITurns(gameState);
            
            // Update AI approval ratings
            AIManager.UpdateAIApprovalRatings();
        }
        
        public void LoadCampaign(LegacyGameSaveData saveData)
        {
            CurrentCharacter = saveData.Character;
            CurrentWorld = saveData.World;
            
            // Create player state
            if (CurrentCharacter != null)
            {
                CurrentPlayer = new PlayerState(CurrentCharacter);
            }
            
            // Initialize voter simulation
            if (CurrentWorld != null)
            {
                VoterSimulation = new VoterSimulation(CurrentWorld);
            }
            
            IsGameActive = true;
            // LoadGameTime expects a float (seconds), so we just start fresh
            // The actual game time tracking will be managed by TimeManager
            TimeManager.LoadGameTime(0f);
            Debug.Log($"Loaded campaign: {saveData.SaveName}");
        }
        
        /// <summary>
        /// Regenerate world from seed (useful for loading)
        /// </summary>
        public void RegenerateWorldFromSeed(string seed)
        {
            if (string.IsNullOrEmpty(seed)) return;
            
            var worldGenerator = new WorldGenerator();
            CurrentWorld = worldGenerator.GenerateWorld(seed);
            
            if (VoterSimulation != null)
            {
                VoterSimulation = new VoterSimulation(CurrentWorld);
            }
            
            Debug.Log($"World regenerated from seed: {seed}");
        }
    }
}

