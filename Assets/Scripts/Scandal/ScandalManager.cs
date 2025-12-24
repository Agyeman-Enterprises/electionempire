using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.World;
using ElectionEmpire.Gameplay;

namespace ElectionEmpire.Scandal
{
    /// <summary>
    /// Main manager for all scandal systems
    /// </summary>
    public class ScandalManager : MonoBehaviour
    {
        // Sub-systems
        private ScandalTriggerSystem triggerSystem;
        private ScandalEvolutionSystem evolutionSystem;
        private ScandalResponseEngine responseEngine;
        private ScandalConsequenceSystem consequenceSystem;
        private ScandalTemplateLibrary templateLibrary;
        
        // State
        private GameState gameState;
        private PlayerState player;
        private ResourceManager resourceManager;
        
        public void Initialize(PlayerState player, GameState gameState, ResourceManager resourceManager)
        {
            this.player = player;
            this.gameState = gameState;
            this.resourceManager = resourceManager;
            
            // Initialize all sub-systems
            templateLibrary = new ScandalTemplateLibrary();
            templateLibrary.LoadTemplates();
            
            triggerSystem = new ScandalTriggerSystem();
            triggerSystem.Initialize(player, gameState);
            
            evolutionSystem = new ScandalEvolutionSystem(templateLibrary, gameState);
            
            responseEngine = new ScandalResponseEngine();
            responseEngine.Initialize(player, gameState, resourceManager);
            
            consequenceSystem = new ScandalConsequenceSystem();
            consequenceSystem.Initialize(player, gameState, resourceManager);
        }
        
        /// <summary>
        /// Called every game turn
        /// </summary>
        public void UpdateScandals(float deltaTime)
        {
            if (player == null || gameState == null) return;
            
            // 1. Evaluate triggers for new scandals
            if (triggerSystem != null)
                triggerSystem.EvaluateTriggers(deltaTime);
            
            // 2. Update all active scandals
            if (player.ActiveScandals != null)
            {
                foreach (var scandal in player.ActiveScandals.ToList())
                {
                    // Evolve scandal through stages
                    if (evolutionSystem != null)
                        evolutionSystem.UpdateScandal(scandal, deltaTime);
                    
                    // Apply consequences
                    if (consequenceSystem != null)
                        consequenceSystem.ApplyScandalConsequences(scandal, deltaTime);
                    
                    // Check for natural expiration
                    CheckScandalExpiration(scandal);
                }
            }
        }
        
        /// <summary>
        /// Player responds to scandal
        /// </summary>
        public ScandalResponseResult RespondToScandal(string scandalID, ResponseType responseType)
        {
            if (player.ActiveScandals == null)
                return new ScandalResponseResult { Success = false, Message = "No active scandals." };
            
            var scandal = player.ActiveScandals.Find(s => s.ID == scandalID);
            
            if (scandal == null)
            {
                return new ScandalResponseResult
                {
                    Success = false,
                    Message = "Scandal not found."
                };
            }
            
            return responseEngine.ExecuteResponse(scandal, responseType);
        }
        
        /// <summary>
        /// Get available responses for a scandal
        /// </summary>
        public List<ResponseOption> GetResponseOptions(string scandalID)
        {
            if (player.ActiveScandals == null)
                return new List<ResponseOption>();
            
            var scandal = player.ActiveScandals.Find(s => s.ID == scandalID);
            
            if (scandal == null)
                return new List<ResponseOption>();
            
            return responseEngine.GetAvailableResponses(scandal);
        }
        
        /// <summary>
        /// Record player action (for trigger system)
        /// </summary>
        public void RecordPlayerAction(PlayerAction action)
        {
            if (triggerSystem != null)
                triggerSystem.RecordAction(action);
        }
        
        private void CheckScandalExpiration(Scandal scandal)
        {
            // Scandals can expire naturally if ignored long enough
            if (scandal.CurrentStage == ScandalStage.Resolution && 
                scandal.TurnsInStage > 10 &&
                scandal.MediaCoverage < 5f)
            {
                ResolveScandal(scandal, ResolutionType.TimeFaded);
            }
        }
        
        private void ResolveScandal(Scandal scandal, ResolutionType resolution)
        {
            scandal.IsResolved = true;
            scandal.ResolvedDate = gameState != null ? gameState.CurrentGameTime : System.DateTime.Now;
            scandal.Resolution = resolution;
            
            // Move to history
            if (player.ScandalHistory == null)
                player.ScandalHistory = new List<Scandal>();
            
            player.ScandalHistory.Add(scandal);
            player.ActiveScandals.Remove(scandal);
            
            Debug.Log($"Scandal resolved: {scandal.Title} - {resolution}");
        }
    }
}

