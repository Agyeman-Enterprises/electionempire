using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionEmpire.Gameplay
{
    /// <summary>
    /// Represents a political office
    /// </summary>
    [Serializable]
    public class Office
    {
        public string ID;
        public string Name;              // "Mayor", "Governor", "President"
        public int Tier;                 // 1-5
        public OfficeType Type;
        
        // Requirements to run for this office
        public OfficeRequirements Requirements;
        
        // Powers and limitations
        public OfficePowers Powers;
        
        // Term details
        public int TermLengthDays;       // In-game days
        public int ReelectionLimit;      // 0 = unlimited, 2 = max 2 terms
        
        // Salary and perks
        public float Salary;             // Per month in-game
        public Dictionary<string, float> ResourceBonuses;
        
        public Office()
        {
            ResourceBonuses = new Dictionary<string, float>();
        }
    }
    
    [Serializable]
    public enum OfficeType
    {
        // Tier 1
        CityCouncil,
        SchoolBoard,
        CountyCommissioner,
        NeighborhoodAssociation,
        
        // Tier 2
        Mayor,
        CountyExecutive,
        DistrictAttorney,
        StateRepresentative,
        
        // Tier 3
        StateSenator,
        StateAttorneyGeneral,
        LieutenantGovernor,
        RegionalDirector,
        
        // Tier 4
        Governor,
        USRepresentative,
        USSenator,
        CabinetSecretary,
        
        // Tier 5
        President,
        SupremeLeader,
        ShadowGovernment
    }
    
    [Serializable]
    public class OfficeRequirements
    {
        public int MinimumTier;          // Must have held Tier X office
        public float MinimumApproval;    // % required
        public float MinimumCampaignFunds; // $ required
        public float MinimumPartyLoyalty; // % required (or -1 if independent)
        public int MinimumPoliticalAllies; // Number of allies needed
        public int MinimumBlackmailAssets; // For shadow paths
        public int MinimumRegionsControlled; // For president
        public List<string> SpecialRequirements; // Custom conditions
        
        public OfficeRequirements()
        {
            SpecialRequirements = new List<string>();
            MinimumPartyLoyalty = -1f; // -1 means not required
        }
    }
    
    [Serializable]
    public class OfficePowers
    {
        // What can you do in this office?
        public bool CanPassLocalOrdinances;
        public bool CanControlBudget;
        public int BudgetSize;           // $ available for policies
        
        public bool CanHireStaff;
        public int MaxStaffSize;
        
        public bool CanIssueExecutiveOrders;
        public bool CanVetoLegislation;
        public bool CanDeclareEmergency;
        
        public bool CanAppointJudges;
        public bool CanPardonCriminals;
        
        public bool CanCommandMilitary;
        public bool CanNegotiateTreaties;
        
        public bool CanInfluenceParty;
        public bool CanEndorseOthers;
        
        public int PolicyImpactMultiplier; // How much your policies matter (1-10)
        public int MediaAttentionMultiplier; // How much coverage (1-10)
    }
}

