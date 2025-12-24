using System;
using System.Collections.Generic;
using UnityEngine;
using ElectionEmpire.World;
using ElectionEmpire.Gameplay;
using ElectionEmpire.News.Translation;
using ElectionEmpire.News.Templates;

namespace ElectionEmpire.Core
{
    /// <summary>
    /// Implementation of IGameStateProvider for news translation system
    /// </summary>
    public class GameStateProvider : MonoBehaviour, IGameStateProvider
    {
        private GameState gameState;
        private PlayerState player;
        private GameLoop gameLoop;
        private bool chaosModeEnabled = false;
        
        public void Initialize(GameState gameState, PlayerState player)
        {
            this.gameState = gameState;
            this.player = player;
            gameLoop = FindObjectOfType<GameLoop>();
        }
        
        public int GetPlayerOfficeTier()
        {
            return player?.CurrentOffice?.Tier ?? 1;
        }
        
        public string GetPlayerOfficeTitle()
        {
            return player?.CurrentOffice?.Name ?? "Citizen";
        }
        
        public string GetPlayerName()
        {
            return player?.Character?.Name ?? "Unknown";
        }
        
        public string GetPlayerParty()
        {
            return player?.Party ?? "Independent";
        }
        
        public string GetPlayerState()
        {
            // Get state from world if available
            if (gameState?.World != null && gameState.World.Nation != null)
            {
                if (gameState.World.Nation.Regions.Count > 0)
                {
                    var firstState = gameState.World.Nation.Regions[0].States?[0];
                    return firstState?.Name ?? "Unknown State";
                }
            }
            return "Unknown State";
        }
        
        public int GetCurrentTurn()
        {
            if (gameLoop != null && gameState != null)
            {
                // Calculate turns from days elapsed
                return gameState.TotalDaysElapsed;
            }
            return 0;
        }
        
        public int GetTurnsUntilElection()
        {
            if (player?.CurrentOffice != null && player.TermEndDate != default)
            {
                var daysRemaining = (player.TermEndDate - DateTime.Now).TotalDays;
                return Mathf.Max(0, (int)daysRemaining);
            }
            return 365; // Default
        }
        
        public float GetPlayerApproval()
        {
            return player?.ApprovalRating ?? 50f;
        }
        
        public PlayerAlignment GetPlayerAlignment()
        {
            // Calculate alignment from character traits and actions
            int lawChaos = 0;
            int goodEvil = 0;
            
            if (player?.Character != null)
            {
                // Map character traits to alignment
                // This is simplified - in full implementation would track actual actions
                
                // Check for chaos-related traits
                if (player.Character.NegativeQuirks != null)
                {
                    foreach (var quirk in player.Character.NegativeQuirks)
                    {
                        if (quirk.Name.Contains("Chaos") || quirk.Name.Contains("Unpredictable"))
                            lawChaos += 20;
                        if (quirk.Name.Contains("Evil") || quirk.Name.Contains("Villain"))
                            goodEvil += 20;
                    }
                }
                
                // Check for lawful/good traits
                if (player.Character.PositiveQuirks != null)
                {
                    foreach (var quirk in player.Character.PositiveQuirks)
                    {
                        if (quirk.Name.Contains("Honest") || quirk.Name.Contains("Principled"))
                            lawChaos -= 10;
                        if (quirk.Name.Contains("Compassionate") || quirk.Name.Contains("Ethical"))
                            goodEvil -= 10;
                    }
                }
            }
            
            // Check chaos mode
            if (chaosModeEnabled)
            {
                lawChaos += 30;
                goodEvil += 30;
            }
            
            return new PlayerAlignment
            {
                LawChaos = Mathf.Clamp(lawChaos, -100, 100),
                GoodEvil = Mathf.Clamp(goodEvil, -100, 100)
            };
        }
        
        public string GetPlayerPartyPosition(PoliticalCategory category)
        {
            // Determine party position based on category
            // This would be more sophisticated in full implementation
            string party = GetPlayerParty();
            
            return category switch
            {
                PoliticalCategory.HealthcarePolicy => $"{party} supports healthcare reform",
                PoliticalCategory.EconomicPolicy => $"{party} focuses on economic growth",
                PoliticalCategory.Immigration => $"{party} has strong immigration stance",
                PoliticalCategory.ClimateEnvironment => $"{party} addresses climate concerns",
                _ => $"{party} is monitoring this issue"
            };
        }
        
        public bool IsChaosModeEnabled()
        {
            return chaosModeEnabled;
        }
        
        public void SetChaosMode(bool enabled)
        {
            chaosModeEnabled = enabled;
        }
    }
}

