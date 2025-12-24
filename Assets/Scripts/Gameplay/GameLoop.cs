using UnityEngine;
using System;
using ElectionEmpire.World;
using ElectionEmpire.AI;
using ElectionEmpire.Core;
using ElectionEmpire.Scandal;
using ElectionEmpire.News;

namespace ElectionEmpire.Gameplay
{
    /// <summary>
    /// Main game loop that coordinates all systems
    /// </summary>
    public class GameLoop : MonoBehaviour
    {
        // Managers
        private TimeManager timeManager;
        private ResourceManager resourceManager;
        private ElectionManager electionManager;
        private AIManager aiManager;
        private VictoryConditionManager victoryManager;
        private DefeatConditionManager defeatManager;
        private Scandal.ScandalManager scandalManager;
        private News.NewsEventManager newsEventManager;
        
        // State
        private GameState gameState;
        private bool isRunning = false;
        
        private float lastTurnProcessTime = 0f;
        private float turnInterval = 86400f; // 1 day in seconds
        
        void Update()
        {
            if (!isRunning) return;
            
            float deltaTime = Time.deltaTime * (timeManager != null ? timeManager.TimeScale : 1f);
            
            // 1. Update time
            if (timeManager != null)
            {
                timeManager.ProcessOfflineTime();
            }
            
            // 2. Update resources
            if (resourceManager != null)
            {
                resourceManager.UpdateResources(deltaTime);
            }
            
            // 3. Update AI opponents (once per day)
            if (Time.time - lastTurnProcessTime >= turnInterval)
            {
                if (aiManager != null && gameState != null)
                {
                    aiManager.ProcessAITurns(gameState);
                    aiManager.UpdateAIApprovalRatings();
                }
                lastTurnProcessTime = Time.time;
            }
            
            // 4. Update election (if active)
            if (electionManager != null && electionManager.IsElectionActive)
            {
                electionManager.UpdateElection(deltaTime);
            }
            
            // 5. Update scandals
            if (scandalManager != null)
            {
                scandalManager.UpdateScandals(deltaTime);
            }
            
            // 6. Check victory conditions
            if (victoryManager != null && gameState != null && gameState.Player != null)
            {
                victoryManager.CheckVictoryConditions(gameState.Player, deltaTime);
            }
            
            // 7. Check defeat conditions
            if (defeatManager != null && gameState != null && gameState.Player != null)
            {
                defeatManager.CheckDefeatConditions(gameState.Player);
            }
            
            // 7. Update player approval
            if (gameState != null && gameState.Player != null && gameState.VoterSim != null)
            {
                gameState.Player.ApprovalRating = gameState.VoterSim.CalculateNationalApproval(gameState.Player);
            }
        }
        
        public void StartCampaign(CampaignSetup setup)
        {
            // Initialize game state
            gameState = new GameState
            {
                Player = setup.PlayerCharacter != null ? 
                    new PlayerState(setup.PlayerCharacter) : null,
                World = setup.World,
                AIOpponents = setup.AIOpponents ?? new System.Collections.Generic.List<AIOpponent>(),
                VictoryCondition = setup.VictoryCondition,
                Difficulty = setup.Difficulty
            };
            
            if (gameState.Player != null)
            {
                gameState.Player.CampaignDifficulty = (int)setup.Difficulty;
            }
            
            // Initialize voter simulation
            if (gameState.World != null)
            {
                gameState.VoterSim = new VoterSimulation(gameState.World);
            }
            
            // Initialize managers
            timeManager = GameManager.Instance != null ? GameManager.Instance.TimeManager : null;
            
            if (gameState.Player != null)
            {
                resourceManager = new ResourceManager(gameState.Player);
            }
            
            if (gameState.World != null && gameState.VoterSim != null)
            {
                electionManager = new ElectionManager(gameState.World, gameState.VoterSim);
            }
            
            if (AIManager.Instance != null)
            {
                aiManager = AIManager.Instance;
                aiManager.CurrentWorld = gameState.World;
                aiManager.VoterSimulation = gameState.VoterSim;
            }
            
            victoryManager = new VictoryConditionManager(gameState);
            victoryManager.Initialize(setup.VictoryCondition);
            
            defeatManager = new DefeatConditionManager(gameState);
            
            // Initialize scandal manager
            var scandalManagerObj = new GameObject("ScandalManager");
            scandalManager = scandalManagerObj.AddComponent<Scandal.ScandalManager>();
            scandalManager.Initialize(gameState.Player, gameState, resourceManager);
            
            // Initialize political ladder
            PoliticalLadder.Instance.Initialize();
            
            // Initialize news event manager
            var newsManagerObj = new GameObject("NewsEventManager");
            newsEventManager = newsManagerObj.AddComponent<NewsEventManager>();
            newsEventManager.Initialize(gameState, gameState.Player, resourceManager);
            
            // Set starting office (Tier 1)
            PromptOfficeSelection();
            
            isRunning = true;
            
            Debug.Log("Campaign started!");
        }
        
        private void PromptOfficeSelection()
        {
            var tier1Offices = PoliticalLadder.Instance.GetTier1Offices();
            
            if (tier1Offices.Count > 0)
            {
                // For now, auto-select first office
                // In full implementation, would show UI dialog
                var selectedOffice = tier1Offices[0];
                StartElection(selectedOffice);
            }
        }
        
        private void StartElection(Office office)
        {
            // Add AI opponents running for same office
            var competitors = SelectAICompetitors(office, 2, 4);
            
            var allCandidates = new System.Collections.Generic.List<PlayerState> { gameState.Player };
            
            // Convert AI opponents to PlayerState for election
            foreach (var ai in competitors)
            {
                var aiPlayerState = new PlayerState(ai.Character)
                {
                    IsPlayer = false,
                    Personality = ai.Personality,
                    PolicyStances = ai.PolicyStances,
                    VoterBlocSupport = ai.VoterBlocSupport,
                    ApprovalRating = ai.ApprovalRating,
                    Resources = ai.Resources
                };
                allCandidates.Add(aiPlayerState);
            }
            
            electionManager.StartElection(office, allCandidates);
            
            gameState.Player.IsInCampaign = true;
        }
        
        private System.Collections.Generic.List<AIOpponent> SelectAICompetitors(Office office, int min, int max)
        {
            int count = UnityEngine.Random.Range(min, max + 1);
            
            return gameState.AIOpponents
                .OrderBy(x => UnityEngine.Random.value)
                .Take(count)
                .ToList();
        }
        
        public GameState GetGameState()
        {
            return gameState;
        }
        
        public void Pause()
        {
            isRunning = false;
        }
        
        public void Resume()
        {
            isRunning = true;
        }
    }
}

