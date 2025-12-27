using UnityEngine;
using System;
using System.Collections;

namespace ElectionEmpire.Core
{
    /// <summary>
    /// Manages game time, pause states, and auto-pause scheduling
    /// </summary>
    public class TimeManager : MonoBehaviour
    {
        [Header("Time Settings")]
        [Tooltip("Game time multiplier: 1 real hour = X game hours")]
        public float TimeScale = 2.0f;
        
        [Header("Pause Settings")]
        public bool IsPaused = false;
        public bool AutoPauseSleep = true;
        public bool AutoPauseWork = false;
        
        [Header("Offline Settings")]
        [Tooltip("Speed multiplier when player is offline")]
        public float OfflineSpeedMultiplier = 0.2f;
        
        [Header("Schedule")]
        public int SleepStartHour = 23; // 11 PM
        public int SleepEndHour = 7;    // 7 AM
        public int WorkStartHour = 9;   // 9 AM
        public int WorkEndHour = 17;    // 5 PM
        
        // Game time state
        public DateTime GameTime { get; private set; }
        public DateTime LastOnlineTime { get; private set; }
        
        // Events
        public event Action<DateTime> OnTimeUpdated;
        public event Action OnPaused;
        public event Action OnResumed;
        
        private void Start()
        {
            if (GameTime == default(DateTime))
            {
                GameTime = new DateTime(2024, 1, 1, 8, 0, 0); // Start at 8 AM
            }
            LastOnlineTime = DateTime.Now;
        }
        
        private void Update()
        {
            if (!IsPaused)
            {
                float deltaTime = Time.deltaTime * TimeScale;
                GameTime = GameTime.AddSeconds(deltaTime);
                OnTimeUpdated?.Invoke(GameTime);
            }
            
            CheckAutoPause();
        }
        
        public void StartGame()
        {
            IsPaused = false;
            LastOnlineTime = DateTime.Now;
        }
        
        public void Pause()
        {
            IsPaused = true;
            OnPaused?.Invoke();
        }
        
        public void Resume()
        {
            IsPaused = false;
            OnResumed?.Invoke();
        }

        public void PauseTime()
        {
            Pause();
        }

        public void ResumeTime()
        {
            Resume();
        }

        public void TogglePause()
        {
            if (IsPaused)
                Resume();
            else
                Pause();
        }
        
        public void SetTimeScale(float scale)
        {
            TimeScale = Mathf.Max(0f, scale);
        }
        
        private void CheckAutoPause()
        {
            if (IsPaused) return;
            
            int currentHour = GameTime.Hour;
            
            // Auto-pause for sleep
            if (AutoPauseSleep)
            {
                if (currentHour >= SleepStartHour || currentHour < SleepEndHour)
                {
                    Pause();
                    return;
                }
            }
            
            // Auto-pause for work
            if (AutoPauseWork)
            {
                if (currentHour >= WorkStartHour && currentHour < WorkEndHour)
                {
                    Pause();
                    return;
                }
            }
        }
        
        public void ProcessOfflineTime()
        {
            DateTime now = DateTime.Now;
            TimeSpan offlineDuration = now - LastOnlineTime;
            
            if (offlineDuration.TotalHours > 0.1f) // Only process if offline for more than 6 minutes
            {
                float offlineGameHours = (float)offlineDuration.TotalHours * TimeScale * OfflineSpeedMultiplier;
                GameTime = GameTime.AddHours(offlineGameHours);
                Debug.Log($"Processed {offlineGameHours:F2} game hours of offline time");
            }
            
            LastOnlineTime = now;
        }
        
        public void LoadGameTime(DateTime gameTime)
        {
            GameTime = gameTime;
            ProcessOfflineTime();
        }
        
        public string GetTimeString()
        {
            return GameTime.ToString("yyyy-MM-dd HH:mm");
        }
        
        public string GetFormattedDate()
        {
            return GameTime.ToString("MMMM dd, yyyy");
        }
    }
}

