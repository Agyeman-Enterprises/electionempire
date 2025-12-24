using System;
using System.Collections.Generic;
using ElectionEmpire.World;
using ElectionEmpire.AI;

namespace ElectionEmpire.Gameplay
{
    /// <summary>
    /// Complete game state
    /// </summary>
    [Serializable]
    public class GameState
    {
        // Players
        public PlayerState Player;
        public List<AIOpponent> AIOpponents;
        
        // World
        public World World;
        public VoterSimulation VoterSim;
        
        // Campaign settings
        public VictoryConditionManager.VictoryType VictoryCondition;
        public AIDifficulty Difficulty;
        
        // Time tracking
        public DateTime CampaignStartDate;
        public DateTime CurrentGameTime;
        public int TotalDaysElapsed;
        
        // Election tracking
        public List<ElectionRecord> ElectionHistory;
        
        // Event tracking
        public List<string> ActiveCrises;
        
        public GameState()
        {
            AIOpponents = new List<AIOpponent>();
            ElectionHistory = new List<ElectionRecord>();
            ActiveCrises = new List<string>();
            CampaignStartDate = DateTime.Now;
            CurrentGameTime = DateTime.Now;
        }
    }
    
    [Serializable]
    public class ElectionRecord
    {
        public Office Office;
        public List<string> CandidateNames;
        public Dictionary<string, int> Results;
        public string WinnerName;
        public DateTime ElectionDate;
    }
    
    [Serializable]
    public class CampaignSetup
    {
        public Character.Character PlayerCharacter;
        public World World;
        public List<AIOpponent> AIOpponents;
        public VictoryConditionManager.VictoryType VictoryCondition;
        public AIDifficulty Difficulty;
    }
}

