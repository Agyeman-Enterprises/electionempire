using System.Collections.Generic;
using ElectionEmpire.World;

namespace ElectionEmpire.AI
{
    /// <summary>
    /// Analysis of current game situation for AI decision-making
    /// </summary>
    public class SituationAnalysis
    {
        public float CurrentPolling;
        public int PollingRank; // 1 = first place, higher = worse
        
        public bool ResourcesLow;
        public bool ResourcesHigh;
        
        public bool UnderAttack;
        public bool ScandalActive;
        public bool CrisisActive;
        
        public List<string> OpponentVulnerable; // IDs of vulnerable opponents
        public List<string> AllianceOpportunity; // IDs of potential allies
        
        public int DaysUntilElection;
        public bool TimePressure; // Less than 30 days
        
        public SituationAnalysis()
        {
            OpponentVulnerable = new List<string>();
            AllianceOpportunity = new List<string>();
        }
    }
}

