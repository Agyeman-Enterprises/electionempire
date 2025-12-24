using System.Collections.Generic;
using ElectionEmpire.Character;
using ElectionEmpire.Gameplay;
using ElectionEmpire.AI;
using ElectionEmpire.Scandal;

namespace ElectionEmpire.World
{
    /// <summary>
    /// Represents a player's state in the game world
    /// </summary>
    public class PlayerState
    {
        public Character Character;
        public Dictionary<Issue, float> PolicyStances; // 0-100 scale
        public Dictionary<VoterBloc, float> VoterBlocSupport; // 0-100 scale
        public List<Scandal.Scandal> ActiveScandals;
        public float MediaInfluence;
        public float CampaignFunds;
        public Dictionary<string, float> DistrictCampaignSpending; // District ID -> spending
        
        // Scandal tracking
        public bool UnderMediaInvestigation;
        public string Alignment; // For trigger system (simplified)
        public float MediaScrutinyLevel;
        public List<string> ReputationTags;
        public Dictionary<string, float> ScandalVulnerabilities;
        public List<string> ToxicPolicyAreas;
        public Dictionary<VoterBloc, float> DemographicPenalties;
        public int EndorsementLockoutTurns;
        
        // Staff (simplified for now)
        public List<StaffMember> Staff;
        
        // Office and progression
        public Office CurrentOffice;
        public int HighestTierHeld;
        public System.DateTime TermStartDate;
        public System.DateTime TermEndDate;
        public OfficePowers OfficePowers;
        
        // Resources
        public Dictionary<string, float> Resources;
        
        // Political
        public List<string> PoliticalAllies;
        public string Party;
        public float ApprovalRating;
        public float CurrentPolling; // For elections
        
        // Campaign state
        public bool IsInCampaign;
        public float CampaignIntensity;
        public int ElectionsWon;
        public int ElectionsLost;
        public int ConsecutiveElectionLosses;
        public int DaysBelowThreshold;
        public int DaysInDebt;
        
        // History
        public List<Scandal.Scandal> ScandalHistory;
        public int TotalLegacyPoints;
        public int CampaignDifficulty;
        public int VictoriesAchieved;
        
        // For AI
        public bool IsPlayer;
        public PersonalityMatrix Personality; // For AI opponents
        
        public PlayerState(Character character)
        {
            Character = character;
            PolicyStances = new Dictionary<Issue, float>();
            VoterBlocSupport = new Dictionary<VoterBloc, float>();
            ActiveScandals = new List<Scandal.Scandal>();
            DistrictCampaignSpending = new Dictionary<string, float>();
            Resources = new Dictionary<string, float>();
            PoliticalAllies = new List<string>();
            ScandalHistory = new List<Scandal.Scandal>();
            ReputationTags = new List<string>();
            ScandalVulnerabilities = new Dictionary<string, float>();
            ToxicPolicyAreas = new List<string>();
            DemographicPenalties = new Dictionary<VoterBloc, float>();
            Staff = new List<StaffMember>();
            IsPlayer = true;
            
            // Initialize default support (neutral)
            foreach (VoterBloc bloc in System.Enum.GetValues(typeof(VoterBloc)))
            {
                VoterBlocSupport[bloc] = 50f;
            }
            
            // Initialize default policy stances (neutral)
            foreach (Issue issue in System.Enum.GetValues(typeof(Issue)))
            {
                PolicyStances[issue] = 50f;
            }
            
            // Initialize resources
            Resources["PublicTrust"] = 50f;
            Resources["PoliticalCapital"] = 10;
            Resources["CampaignFunds"] = 10000f;
            Resources["MediaInfluence"] = 30f;
            Resources["PartyLoyalty"] = 50f;
            
            ApprovalRating = 50f;
            CampaignIntensity = 1.0f;
            HighestTierHeld = 0;
        }
    }
    
}

