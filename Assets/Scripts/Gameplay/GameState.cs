using System;
using System.Collections.Generic;
using ElectionEmpire.World;
using ElectionEmpire.AI;
using ElectionEmpire.Core;

namespace ElectionEmpire.Gameplay
{
    /// <summary>
    /// Game settings for the current session
    /// </summary>
    [Serializable]
    public class GameSettings
    {
        public float MasterVolume = 1f;
        public float MusicVolume = 0.8f;
        public float SFXVolume = 1f;
        public float VoiceVolume = 1f;
        public bool MuteWhenUnfocused = false;
        public bool Fullscreen = true;
        public int QualityLevel = 2;
        public bool VSync = true;
        public int TargetFramerate = 60;
        public float TextSpeed = 1f;
        public bool ShowTutorials = true;
        public bool AutoSave = true;
        public int AutoSaveInterval = 10;
        public bool ConfirmActions = true;
        public bool ShowProbabilities = true;
        public float UIScale = 1f;
        public bool HighContrast = false;
        public bool ReduceAnimations = false;
    }

    /// <summary>
    /// Complete game state
    /// </summary>
    [Serializable]
    public class GameState
    {
        // Players
        public PlayerState Player;
        public List<AIOpponent> AIOpponents;
        public List<AIOpponent> Opponents; // Alias for compatibility

        // World
        public ElectionEmpire.World.World World;
        public VoterSimulation VoterSim;

        // Campaign settings
        public VictoryConditionManager.VictoryType VictoryCondition;
        public AIDifficulty Difficulty;

        // Time tracking
        public DateTime CampaignStartDate;
        public DateTime CurrentGameTime;
        public int TotalDaysElapsed;
        public int DaysUntilElection;
        public int CurrentTurn;
        public int CurrentMonth;
        public int CurrentYear;

        // Election tracking
        public List<ElectionRecord> ElectionHistory;

        // Event tracking
        public List<string> ActiveCrises;
        public List<string> PendingEvents;

        // Progression tracking
        public float PlayerApproval;
        public GamePhase CurrentPhase;

        // Save metadata
        public string SaveName;
        public DateTime LastSavedAt;
        public string ActiveCharacterId;

        // Settings
        public GameSettings Settings;

        // World entities
        public List<string> MediaOutlets;
        public List<string> NPCPoliticians;

        // Campaign data
        public CampaignData Campaign;

        public GameState()
        {
            AIOpponents = new List<AIOpponent>();
            Opponents = AIOpponents; // Point to same list
            ElectionHistory = new List<ElectionRecord>();
            ActiveCrises = new List<string>();
            PendingEvents = new List<string>();
            MediaOutlets = new List<string>();
            NPCPoliticians = new List<string>();
            CampaignStartDate = DateTime.Now;
            CurrentGameTime = DateTime.Now;
            Settings = new GameSettings();
            Campaign = new CampaignData();
            CurrentPhase = GamePhase.Campaign;
            PlayerApproval = 50f;
            CurrentYear = DateTime.Now.Year;
            CurrentMonth = DateTime.Now.Month;
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
        public ElectionEmpire.Character.Character PlayerCharacter;
        public ElectionEmpire.World.World World;
        public List<AIOpponent> AIOpponents;
        public VictoryConditionManager.VictoryType VictoryCondition;
        public AIDifficulty Difficulty;
    }

    [Serializable]
    public class CampaignData
    {
        public string CampaignName;
        public DateTime StartDate;
        public int TotalTurns;
        public Dictionary<string, float> CampaignMetrics;

        public CampaignData()
        {
            CampaignMetrics = new Dictionary<string, float>();
            StartDate = DateTime.Now;
        }
    }
}

