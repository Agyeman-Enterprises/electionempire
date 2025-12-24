using System.Collections.Generic;
using UnityEngine;

namespace ElectionEmpire.Chaos
{
    /// <summary>
    /// Tracks chaos level and viral moments
    /// </summary>
    public class ChaosMeter
    {
        public float ChaosLevel; // 0-100
        public int ViralMoments;
        public long TotalViews;
        public int CriminalInvestigations;
        public List<string> UnlockedPaths;
        public List<string> ActiveWarnings;
        public List<string> Achievements;
        
        public ChaosMeter()
        {
            UnlockedPaths = new List<string>();
            ActiveWarnings = new List<string>();
            Achievements = new List<string>();
        }
        
        public void AddChaos(float amount, string source)
        {
            ChaosLevel += amount;
            ChaosLevel = Mathf.Clamp(ChaosLevel, 0f, 100f);
            
            // Check for path unlocks
            if (ChaosLevel >= 50f && !UnlockedPaths.Contains("AUTHORITARIAN PATH"))
            {
                UnlockedPaths.Add("AUTHORITARIAN PATH");
            }
            
            if (ChaosLevel >= 75f && !UnlockedPaths.Contains("MOB BOSS PATH"))
            {
                UnlockedPaths.Add("MOB BOSS PATH");
            }
            
            if (ChaosLevel >= 90f && !UnlockedPaths.Contains("CULT LEADER PATH"))
            {
                UnlockedPaths.Add("CULT LEADER PATH");
            }
        }
        
        public void AddViralMoment(int views, string title)
        {
            ViralMoments++;
            TotalViews += views;
            
            // Check for achievements
            if (TotalViews >= 100000000 && !Achievements.Contains("VIRAL LEGEND"))
            {
                Achievements.Add("VIRAL LEGEND");
            }
        }
        
        public string GetChaosStatus()
        {
            if (ChaosLevel < 20f) return "Normal";
            if (ChaosLevel < 40f) return "Controversial";
            if (ChaosLevel < 60f) return "Chaotic";
            if (ChaosLevel < 80f) return "LEGENDARY VILLAIN";
            return "ABSOLUTELY UNHINGED";
        }
    }
}

