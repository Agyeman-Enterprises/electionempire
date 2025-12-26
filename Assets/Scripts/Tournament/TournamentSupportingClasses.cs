// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - TOURNAMENT SUPPORTING CLASSES
// Bracket generation, ranking system, and supporting utilities
// ═══════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ElectionEmpire.Tournament
{
    /// <summary>
    /// Generates tournament brackets
    /// </summary>
    public class BracketGenerator
    {
        /// <summary>
        /// Generate bracket structure for a tournament
        /// </summary>
        public string GenerateBracket(Tournament tournament)
        {
            // For now, return a simple JSON representation
            // In full implementation, this would generate a visual bracket structure
            var bracketData = new
            {
                format = tournament.Format.ToString(),
                rounds = tournament.TotalRounds,
                participants = tournament.Participants.Count,
                matches = tournament.Matches.Count
            };
            
            return JsonUtility.ToJson(bracketData);
        }
    }
    
    /// <summary>
    /// Ranking system for tournaments
    /// </summary>
    public class RankingSystem
    {
        private TournamentSystemConfig _config;
        
        public RankingSystem(TournamentSystemConfig config)
        {
            _config = config;
        }
        
        /// <summary>
        /// Process tournament results and update rankings
        /// </summary>
        public void ProcessTournamentResults(Tournament tournament)
        {
            // Update season points for participants
            foreach (var participant in tournament.Participants)
            {
                int seasonPoints = CalculateSeasonPoints(tournament, participant);
                
                // Apply decay to existing points
                // In full implementation, would update player's season points
                Debug.Log($"[RankingSystem] {participant.PlayerName} earned {seasonPoints} season points");
            }
        }
        
        private int CalculateSeasonPoints(Tournament tournament, TournamentParticipant participant)
        {
            int tierMultiplier = (int)tournament.Tier + 1;
            int placementPoints = participant.CurrentPosition switch
            {
                1 => 100,
                2 => 75,
                3 => 50,
                4 => 50,
                <= 8 => 25,
                <= 16 => 10,
                _ => 5
            };
            return placementPoints * tierMultiplier;
        }
    }
}

