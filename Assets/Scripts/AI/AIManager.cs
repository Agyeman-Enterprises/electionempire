using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.World;

namespace ElectionEmpire.AI
{
    /// <summary>
    /// Central manager for all AI opponents
    /// </summary>
    public class AIManager : MonoBehaviour
    {
        public static AIManager Instance { get; private set; }
        
        [Header("AI Settings")]
        public int NumberOfAIOpponents = 3;
        public AIDifficulty DefaultDifficulty = AIDifficulty.Normal;
        
        [Header("References")]
        public World CurrentWorld;
        public VoterSimulation VoterSimulation;
        
        private List<AIOpponent> aiOpponents = new List<AIOpponent>();
        private AIOpponentGenerator generator;
        private Dictionary<string, AIDecisionMaker> decisionMakers = new Dictionary<string, AIDecisionMaker>();
        private AIRelationshipManager relationshipManager;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                generator = new AIOpponentGenerator();
                relationshipManager = new AIRelationshipManager();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Generate AI opponents for a campaign
        /// </summary>
        public void GenerateAIOpponents(int count, AIDifficulty difficulty, int playerTier)
        {
            aiOpponents.Clear();
            decisionMakers.Clear();
            
            for (int i = 0; i < count; i++)
            {
                string seed = System.Guid.NewGuid().ToString();
                var opponent = generator.GenerateOpponent(playerTier, difficulty, seed);
                aiOpponents.Add(opponent);
                
                // Create decision maker
                if (CurrentWorld != null && VoterSimulation != null)
                {
                    decisionMakers[opponent.ID] = new AIDecisionMaker(
                        opponent, 
                        CurrentWorld, 
                        VoterSimulation,
                        aiOpponents
                    );
                }
            }
            
            Debug.Log($"Generated {count} AI opponents");
        }
        
        /// <summary>
        /// Process all AI turns
        /// </summary>
        public void ProcessAITurns(GameState gameState)
        {
            gameState.Opponents = aiOpponents;
            
            foreach (var opponent in aiOpponents)
            {
                if (decisionMakers.ContainsKey(opponent.ID))
                {
                    decisionMakers[opponent.ID].TakeTurn(gameState);
                }
            }
        }
        
        /// <summary>
        /// Get all AI opponents
        /// </summary>
        public List<AIOpponent> GetAIOpponents()
        {
            return new List<AIOpponent>(aiOpponents);
        }
        
        /// <summary>
        /// Get specific AI opponent
        /// </summary>
        public AIOpponent GetAIOpponent(string id)
        {
            return aiOpponents.FirstOrDefault(o => o.ID == id);
        }
        
        /// <summary>
        /// Update AI approval ratings
        /// </summary>
        public void UpdateAIApprovalRatings()
        {
            if (CurrentWorld == null || VoterSimulation == null)
                return;
            
            foreach (var opponent in aiOpponents)
            {
                var playerState = new PlayerState(opponent.Character);
                playerState.PolicyStances = opponent.PolicyStances;
                playerState.VoterBlocSupport = opponent.VoterBlocSupport;
                
                opponent.ApprovalRating = VoterSimulation.CalculateNationalApproval(playerState);
            }
        }
    }
}

