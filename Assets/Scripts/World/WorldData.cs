using System;
using System.Collections.Generic;
using UnityEngine;
using ElectionEmpire.Core;

namespace ElectionEmpire.World
{
    /// <summary>
    /// Complete world data structure
    /// </summary>
    [Serializable]
    public class World
    {
        public string Seed;              // For reproducible generation
        public Nation Nation;
        public DateTime CreatedDate;

        // Global state metrics
        public float EconomyHealth;      // 0-100 scale
        public float PoliticalPolarization; // 0-100 scale
        public float NationalMorale;     // 0-100 scale
        public float DisasterRiskLevel;  // 0-100 scale

        // Game progression
        public int TurnNumber;           // Current turn number

        public World()
        {
            EconomyHealth = 70f;
            PoliticalPolarization = 50f;
            NationalMorale = 60f;
            DisasterRiskLevel = 20f;
            TurnNumber = 0;
        }
    }
    
    [Serializable]
    public class Nation
    {
        public string Name = "The Republic";
        public List<Region> Regions;     // 10 regions
        public int TotalPopulation;
        public GovernmentType Type = GovernmentType.Democracy;
        
        public Nation()
        {
            Regions = new List<Region>();
        }
    }
    
    [Serializable]
    public enum GovernmentType
    {
        Democracy,
        Republic,
        Federation,
        Confederation
    }
    
    [Serializable]
    public class Region
    {
        public string ID;
        public string Name;              // "Northern Coalition", "Coastal Alliance"
        public List<State> States;       // 3-6 states per region
        public RegionProfile Profile;
        public Vector2 MapPosition;      // For visualization
        
        public Region()
        {
            States = new List<State>();
        }
    }
    
    [Serializable]
    public class RegionProfile
    {
        public float Urbanization;       // 0.0 to 1.0 (30% to 90% urban)
        public int WealthLevel;          // 1-5 scale
        public float Education;          // % college educated (30-80%)
        public float PoliticalLean;      // -50 (left) to +50 (right)
        public List<string> KeyIndustries; // Manufacturing, Tech, Agriculture, etc.
        public List<VoterBloc> DominantBlocs; // Which blocs are strongest here
        
        public RegionProfile()
        {
            KeyIndustries = new List<string>();
            DominantBlocs = new List<VoterBloc>();
        }
    }
    
    [Serializable]
    public class State
    {
        public string ID;
        public string Name;              // "New Liberty", "Harbor State"
        public int Population;           // 500k - 15M
        public List<District> Districts; // 30 per state
        public DemographicProfile Demographics;
        public EconomicProfile Economy;
        public float PoliticalLean;      // Inherited from region with variation
        public Vector2 MapPosition;
        
        public State()
        {
            Districts = new List<District>();
        }
    }
    
    [Serializable]
    public class DemographicProfile
    {
        public float Youth18to29;
        public float Adults30to49;
        public float MiddleAge50to64;
        public float Seniors65Plus;
    }
    
    [Serializable]
    public class EconomicProfile
    {
        public float GDP;
        public float Unemployment;
        public Dictionary<string, float> IndustryShares;
        
        public EconomicProfile()
        {
            IndustryShares = new Dictionary<string, float>();
        }
    }
    
    [Serializable]
    public class District
    {
        public string ID;
        public string Name;              // "Liberty District 5", "Harbor County"
        public int Population;           // 10k - 500k
        public DistrictType Type;        // Urban, Suburban, Rural
        public DemographicData Demographics;
        public Dictionary<VoterBloc, float> BlocStrength; // % of each bloc
        public List<Issue> PriorityIssues; // Top 5 issues for this district
        public ElectionHistory History;  // Past results
        public Vector2 MapPosition;
        public float PoliticalLean;
        
        public District()
        {
            BlocStrength = new Dictionary<VoterBloc, float>();
            PriorityIssues = new List<Issue>();
        }
    }
    
    [Serializable]
    public enum DistrictType 
    { 
        Urban, 
        Suburban, 
        Rural 
    }
    
    [Serializable]
    public class DemographicData
    {
        // Age distribution (must total 100%)
        public float Youth18to29;        // %
        public float Adults30to49;       // %
        public float MiddleAge50to64;    // %
        public float Seniors65Plus;      // %
        
        // Income distribution (must total 100%)
        public float LowIncome;          // <$35k
        public float MiddleIncome;       // $35k-$100k
        public float HighIncome;         // >$100k
        
        // Education
        public float HighSchoolOrLess;   // %
        public float CollegeEducated;    // %
        public float PostGrad;           // %
        
        // Employment sectors
        public Dictionary<string, float> EmploymentSectors;
        // "Manufacturing", "Services", "Technology", "Healthcare",
        // "Education", "Government", "Agriculture", "Finance"
        
        public DemographicData()
        {
            EmploymentSectors = new Dictionary<string, float>();
        }
    }
    
    [Serializable]
    public enum Issue
    {
        Economy, Jobs, Healthcare, Education, Environment,
        Crime, Immigration, Taxes, Infrastructure, Housing,
        Technology, Trade, Defense, ForeignPolicy, Ethics,
        CivilRights, Agriculture, Energy, Transportation, Corruption
    }
    
    [Serializable]
    public class ElectionHistory
    {
        public Dictionary<int, string> PastWinners; // Year -> Winner name
        public Dictionary<int, float> Turnout;      // Year -> Turnout %
        
        public ElectionHistory()
        {
            PastWinners = new Dictionary<int, string>();
            Turnout = new Dictionary<int, float>();
        }
    }
}

