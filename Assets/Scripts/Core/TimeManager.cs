using UnityEngine;
using System;

namespace ElectionEmpire.Managers
{
    /// <summary>
    /// Manages game time, turns, and election cycles.
    /// Central coordinator for the turn-based flow.
    /// </summary>
    public class TimeManager : MonoBehaviour
    {
        public static TimeManager Instance { get; private set; }
        
        // ===== TIME STATE =====
        [Header("Current Time")]
        [SerializeField] private int currentYear = 2025;
        [SerializeField] private int currentMonth = 1;
        [SerializeField] private int currentTurn = 1;
        
        [Header("Election Cycle")]
        [SerializeField] private int electionYear = 2026;
        [SerializeField] private int electionMonth = 11;
        [SerializeField] private int turnsUntilElection = 22;
        
        [Header("Turn Settings")]
        [SerializeField] private int actionsPerTurn = 3;
        [SerializeField] private int actionsRemaining = 3;
        
        // ===== GAME PHASE =====
        public enum GamePhase
        {
            Morning,      // Briefing, news, events
            Action,       // Player takes actions
            Reaction,     // NPCs react, media covers
            EndOfTurn,    // Decay, random events
            Election,     // Special election phase
            Transition    // Post-election, pre-term
        }
        
        [Header("Current Phase")]
        [SerializeField] private GamePhase currentPhase = GamePhase.Morning;
        
        // ===== EVENTS =====
        public event Action OnTurnStart;
        public event Action OnTurnEnd;
        public event Action<GamePhase> OnPhaseChanged;
        public event Action OnElectionTriggered;
        public event Action OnMonthChanged;
        public event Action OnYearChanged;
        
        // ===== PROPERTIES =====
        public int CurrentYear => currentYear;
        public int CurrentMonth => currentMonth;
        public int CurrentTurn => currentTurn;
        public int TurnsUntilElection => turnsUntilElection;
        public int ActionsRemaining => actionsRemaining;
        public GamePhase CurrentPhase => currentPhase;
        
        public string CurrentDateString => $"{GetMonthName(currentMonth)} {currentYear}";
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            Debug.Log("[TimeManager] Initialized");
            Debug.Log($"  - Current Date: {CurrentDateString}");
            Debug.Log($"  - Turn: {currentTurn}");
            Debug.Log($"  - Election in: {turnsUntilElection} turns ({GetMonthName(electionMonth)} {electionYear})");
        }
        
        private void Start()
        {
            // Begin first turn
            StartTurn();
        }
        
        // ===== TURN FLOW =====
        
        public void StartTurn()
        {
            Debug.Log($"\n========== TURN {currentTurn} START ==========");
            Debug.Log($"Date: {CurrentDateString}");
            Debug.Log($"Election in: {turnsUntilElection} months");
            
            actionsRemaining = actionsPerTurn;
            SetPhase(GamePhase.Morning);
            
            OnTurnStart?.Invoke();
        }
        
        public void AdvanceToActionPhase()
        {
            if (currentPhase == GamePhase.Morning)
            {
                SetPhase(GamePhase.Action);
                Debug.Log($"[TimeManager] Action phase - {actionsRemaining} actions available");
            }
        }
        
        /// <summary>
        /// Use an action - returns false if no actions remain
        /// </summary>
        public bool UseAction(string actionName = "Action")
        {
            if (actionsRemaining <= 0)
            {
                Debug.LogWarning("[TimeManager] No actions remaining this turn!");
                return false;
            }
            
            actionsRemaining--;
            Debug.Log($"[TimeManager] Action used: {actionName} | {actionsRemaining} remaining");
            
            if (actionsRemaining <= 0)
            {
                Debug.Log("[TimeManager] All actions used - proceed to Reaction phase");
            }
            
            return true;
        }
        
        public void AdvanceToReactionPhase()
        {
            if (currentPhase == GamePhase.Action)
            {
                SetPhase(GamePhase.Reaction);
                Debug.Log("[TimeManager] Reaction phase - NPCs and media responding...");
                
                // In a full implementation, this would trigger AI responses
                // For now, auto-advance after a delay or button press
            }
        }
        
        public void EndTurn()
        {
            SetPhase(GamePhase.EndOfTurn);
            Debug.Log("[TimeManager] End of turn - processing...");
            
            // Apply decay to resources
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.ApplyTurnDecay();
            }
            
            OnTurnEnd?.Invoke();
            
            // Advance calendar
            AdvanceMonth();
            
            // Check for election
            turnsUntilElection--;
            if (turnsUntilElection <= 0)
            {
                TriggerElection();
            }
            else
            {
                // Start next turn
                currentTurn++;
                StartTurn();
            }
        }
        
        // ===== CALENDAR =====
        
        private void AdvanceMonth()
        {
            currentMonth++;
            if (currentMonth > 12)
            {
                currentMonth = 1;
                currentYear++;
                Debug.Log($"[TimeManager] New Year: {currentYear}");
                OnYearChanged?.Invoke();
            }
            
            OnMonthChanged?.Invoke();
            Debug.Log($"[TimeManager] Date advanced to {CurrentDateString}");
        }
        
        private string GetMonthName(int month)
        {
            string[] months = { "", "January", "February", "March", "April", "May", "June",
                               "July", "August", "September", "October", "November", "December" };
            return months[Mathf.Clamp(month, 1, 12)];
        }
        
        // ===== ELECTION =====
        
        private void TriggerElection()
        {
            SetPhase(GamePhase.Election);
            Debug.Log("\n!!! ELECTION DAY !!!");
            Debug.Log($"[TimeManager] {GetMonthName(currentMonth)} {currentYear} - The people decide!");
            
            OnElectionTriggered?.Invoke();
            
            // Election resolution would happen here
            // For now, just log it
        }
        
        /// <summary>
        /// Set up next election cycle after winning
        /// </summary>
        public void SetupNextElectionCycle(int yearsUntilNext)
        {
            turnsUntilElection = yearsUntilNext * 12;
            electionYear = currentYear + yearsUntilNext;
            electionMonth = 11; // November
            
            Debug.Log($"[TimeManager] Next election: {GetMonthName(electionMonth)} {electionYear} ({turnsUntilElection} turns)");
            
            SetPhase(GamePhase.Transition);
        }
        
        // ===== PHASE MANAGEMENT =====
        
        private void SetPhase(GamePhase newPhase)
        {
            if (currentPhase != newPhase)
            {
                Debug.Log($"[TimeManager] Phase: {currentPhase} → {newPhase}");
                currentPhase = newPhase;
                OnPhaseChanged?.Invoke(newPhase);
            }
        }
        
        // ===== UI HELPERS =====
        
        public string GetPhaseDescription()
        {
            switch (currentPhase)
            {
                case GamePhase.Morning:
                    return "Morning Briefing - Review news and events";
                case GamePhase.Action:
                    return $"Action Phase - {actionsRemaining} actions remaining";
                case GamePhase.Reaction:
                    return "Reaction Phase - Awaiting responses...";
                case GamePhase.EndOfTurn:
                    return "End of Turn - Processing...";
                case GamePhase.Election:
                    return "ELECTION DAY";
                case GamePhase.Transition:
                    return "Transition Period";
                default:
                    return "Unknown Phase";
            }
        }
        
        public string GetStatusSummary()
        {
            return $"Turn {currentTurn} | {CurrentDateString}\n" +
                   $"{GetPhaseDescription()}\n" +
                   $"Election in {turnsUntilElection} months";
        }

        // ===== COMPATIBILITY METHODS =====
        // These methods support legacy code that expects different API

        /// <summary>
        /// Time scale multiplier for game speed (1.0 = normal)
        /// </summary>
        public float TimeScale { get; set; } = 1.0f;

        /// <summary>
        /// Current game time in seconds since game start
        /// </summary>
        public float GameTime { get; private set; } = 0f;

        private float lastSaveTime = 0f;
        private bool isPaused = false;

        /// <summary>
        /// Process any time that passed while game was closed (idle game feature)
        /// </summary>
        public void ProcessOfflineTime()
        {
            // For now, just update game time
            if (!isPaused)
            {
                GameTime += Time.deltaTime * TimeScale;
            }
        }

        /// <summary>
        /// Start game time tracking
        /// </summary>
        public void StartGame()
        {
            GameTime = 0f;
            lastSaveTime = 0f;
            isPaused = false;
            Debug.Log("[TimeManager] Game time started");
        }

        /// <summary>
        /// Load game time from save data
        /// </summary>
        public void LoadGameTime(float savedTime)
        {
            GameTime = savedTime;
            lastSaveTime = savedTime;
            Debug.Log($"[TimeManager] Loaded game time: {savedTime}");
        }

        /// <summary>
        /// Pause time progression
        /// </summary>
        public void PauseTime()
        {
            isPaused = true;
            Debug.Log("[TimeManager] Time paused");
        }

        /// <summary>
        /// Resume time progression
        /// </summary>
        public void ResumeTime()
        {
            isPaused = false;
            Debug.Log("[TimeManager] Time resumed");
        }
    }
}
