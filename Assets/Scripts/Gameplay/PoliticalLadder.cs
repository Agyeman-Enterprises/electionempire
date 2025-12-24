using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.World;

namespace ElectionEmpire.Gameplay
{
    /// <summary>
    /// Manages the political ladder and office progression
    /// </summary>
    public class PoliticalLadder
    {
        private static PoliticalLadder _instance;
        public static PoliticalLadder Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new PoliticalLadder();
                return _instance;
            }
        }
        
        private Dictionary<OfficeType, Office> allOffices;
        
        public void Initialize()
        {
            allOffices = new Dictionary<OfficeType, Office>();
            
            // TIER 1 OFFICES
            allOffices[OfficeType.CityCouncil] = new Office
            {
                ID = "city_council",
                Name = "City Council Member",
                Tier = 1,
                Type = OfficeType.CityCouncil,
                TermLengthDays = 730, // 2 years
                ReelectionLimit = 0,  // Unlimited
                Salary = 5000,
                Requirements = new OfficeRequirements
                {
                    MinimumTier = 0,
                    MinimumApproval = 35f,
                    MinimumCampaignFunds = 5000f
                },
                Powers = new OfficePowers
                {
                    CanPassLocalOrdinances = true,
                    CanControlBudget = true,
                    BudgetSize = 100000,
                    CanHireStaff = true,
                    MaxStaffSize = 2,
                    PolicyImpactMultiplier = 1,
                    MediaAttentionMultiplier = 1
                }
            };
            
            allOffices[OfficeType.SchoolBoard] = new Office
            {
                ID = "school_board",
                Name = "School Board Member",
                Tier = 1,
                Type = OfficeType.SchoolBoard,
                TermLengthDays = 1460, // 4 years
                ReelectionLimit = 0,
                Salary = 3000,
                Requirements = new OfficeRequirements
                {
                    MinimumTier = 0,
                    MinimumApproval = 35f,
                    MinimumCampaignFunds = 3000f
                },
                Powers = new OfficePowers
                {
                    CanPassLocalOrdinances = true,
                    BudgetSize = 50000,
                    CanHireStaff = true,
                    MaxStaffSize = 1,
                    PolicyImpactMultiplier = 1,
                    MediaAttentionMultiplier = 1
                }
            };
            
            allOffices[OfficeType.CountyCommissioner] = new Office
            {
                ID = "county_commissioner",
                Name = "County Commissioner",
                Tier = 1,
                Type = OfficeType.CountyCommissioner,
                TermLengthDays = 1460,
                ReelectionLimit = 0,
                Salary = 4000,
                Requirements = new OfficeRequirements
                {
                    MinimumTier = 0,
                    MinimumApproval = 35f,
                    MinimumCampaignFunds = 4000f
                },
                Powers = new OfficePowers
                {
                    CanPassLocalOrdinances = true,
                    CanControlBudget = true,
                    BudgetSize = 200000,
                    CanHireStaff = true,
                    MaxStaffSize = 2,
                    PolicyImpactMultiplier = 1,
                    MediaAttentionMultiplier = 1
                }
            };
            
            // TIER 2 OFFICES
            allOffices[OfficeType.Mayor] = new Office
            {
                ID = "mayor",
                Name = "Mayor",
                Tier = 2,
                Type = OfficeType.Mayor,
                TermLengthDays = 1460, // 4 years
                ReelectionLimit = 2,
                Salary = 15000,
                Requirements = new OfficeRequirements
                {
                    MinimumTier = 1,
                    MinimumApproval = 40f,
                    MinimumCampaignFunds = 10000f
                },
                Powers = new OfficePowers
                {
                    CanPassLocalOrdinances = true,
                    CanControlBudget = true,
                    BudgetSize = 5000000,
                    CanHireStaff = true,
                    MaxStaffSize = 5,
                    CanIssueExecutiveOrders = true,
                    CanDeclareEmergency = true,
                    PolicyImpactMultiplier = 3,
                    MediaAttentionMultiplier = 3
                }
            };
            
            allOffices[OfficeType.StateRepresentative] = new Office
            {
                ID = "state_representative",
                Name = "State Representative",
                Tier = 2,
                Type = OfficeType.StateRepresentative,
                TermLengthDays = 730, // 2 years
                ReelectionLimit = 0,
                Salary = 12000,
                Requirements = new OfficeRequirements
                {
                    MinimumTier = 1,
                    MinimumApproval = 40f,
                    MinimumCampaignFunds = 15000f,
                    MinimumPartyLoyalty = 40f
                },
                Powers = new OfficePowers
                {
                    CanControlBudget = true,
                    BudgetSize = 10000000,
                    CanHireStaff = true,
                    MaxStaffSize = 4,
                    CanInfluenceParty = true,
                    PolicyImpactMultiplier = 2,
                    MediaAttentionMultiplier = 2
                }
            };
            
            // TIER 3 OFFICES
            allOffices[OfficeType.StateSenator] = new Office
            {
                ID = "state_senator",
                Name = "State Senator",
                Tier = 3,
                Type = OfficeType.StateSenator,
                TermLengthDays = 1460, // 4 years
                ReelectionLimit = 3,
                Salary = 25000,
                Requirements = new OfficeRequirements
                {
                    MinimumTier = 2,
                    MinimumApproval = 45f,
                    MinimumCampaignFunds = 100000f,
                    MinimumPartyLoyalty = 50f,
                    MinimumPoliticalAllies = 3
                },
                Powers = new OfficePowers
                {
                    CanControlBudget = true,
                    BudgetSize = 50000000,
                    CanHireStaff = true,
                    MaxStaffSize = 8,
                    CanInfluenceParty = true,
                    CanEndorseOthers = true,
                    PolicyImpactMultiplier = 5,
                    MediaAttentionMultiplier = 5
                }
            };
            
            allOffices[OfficeType.LieutenantGovernor] = new Office
            {
                ID = "lieutenant_governor",
                Name = "Lieutenant Governor",
                Tier = 3,
                Type = OfficeType.LieutenantGovernor,
                TermLengthDays = 1460,
                ReelectionLimit = 2,
                Salary = 30000,
                Requirements = new OfficeRequirements
                {
                    MinimumTier = 2,
                    MinimumApproval = 45f,
                    MinimumCampaignFunds = 150000f,
                    MinimumPartyLoyalty = 55f,
                    MinimumPoliticalAllies = 4
                },
                Powers = new OfficePowers
                {
                    CanControlBudget = true,
                    BudgetSize = 100000000,
                    CanHireStaff = true,
                    MaxStaffSize = 10,
                    CanInfluenceParty = true,
                    CanEndorseOthers = true,
                    PolicyImpactMultiplier = 6,
                    MediaAttentionMultiplier = 6
                }
            };
            
            // TIER 4 OFFICES
            allOffices[OfficeType.Governor] = new Office
            {
                ID = "governor",
                Name = "Governor",
                Tier = 4,
                Type = OfficeType.Governor,
                TermLengthDays = 1460, // 4 years
                ReelectionLimit = 2,
                Salary = 50000,
                Requirements = new OfficeRequirements
                {
                    MinimumTier = 3,
                    MinimumApproval = 50f,
                    MinimumCampaignFunds = 500000f,
                    MinimumPoliticalAllies = 5
                },
                Powers = new OfficePowers
                {
                    CanIssueExecutiveOrders = true,
                    CanVetoLegislation = true,
                    CanDeclareEmergency = true,
                    CanControlBudget = true,
                    BudgetSize = 500000000,
                    CanHireStaff = true,
                    MaxStaffSize = 15,
                    CanAppointJudges = true,
                    CanPardonCriminals = true,
                    CanInfluenceParty = true,
                    CanEndorseOthers = true,
                    PolicyImpactMultiplier = 7,
                    MediaAttentionMultiplier = 8
                }
            };
            
            allOffices[OfficeType.USSenator] = new Office
            {
                ID = "us_senator",
                Name = "U.S. Senator",
                Tier = 4,
                Type = OfficeType.USSenator,
                TermLengthDays = 2190, // 6 years
                ReelectionLimit = 0,
                Salary = 45000,
                Requirements = new OfficeRequirements
                {
                    MinimumTier = 3,
                    MinimumApproval = 50f,
                    MinimumCampaignFunds = 1000000f,
                    MinimumPartyLoyalty = 60f,
                    MinimumPoliticalAllies = 6
                },
                Powers = new OfficePowers
                {
                    CanControlBudget = true,
                    BudgetSize = 1000000000,
                    CanHireStaff = true,
                    MaxStaffSize = 20,
                    CanInfluenceParty = true,
                    CanEndorseOthers = true,
                    PolicyImpactMultiplier = 8,
                    MediaAttentionMultiplier = 9
                }
            };
            
            // TIER 5 OFFICES
            allOffices[OfficeType.President] = new Office
            {
                ID = "president",
                Name = "President",
                Tier = 5,
                Type = OfficeType.President,
                TermLengthDays = 1460, // 4 years
                ReelectionLimit = 2,
                Salary = 100000,
                Requirements = new OfficeRequirements
                {
                    MinimumTier = 4,
                    MinimumApproval = 55f,
                    MinimumCampaignFunds = 5000000f,
                    MinimumRegionsControlled = 4,
                    MinimumPoliticalAllies = 8
                },
                Powers = new OfficePowers
                {
                    CanIssueExecutiveOrders = true,
                    CanVetoLegislation = true,
                    CanDeclareEmergency = true,
                    CanControlBudget = true,
                    BudgetSize = 5000000000,
                    CanHireStaff = true,
                    MaxStaffSize = 50,
                    CanAppointJudges = true,
                    CanPardonCriminals = true,
                    CanCommandMilitary = true,
                    CanNegotiateTreaties = true,
                    CanInfluenceParty = true,
                    CanEndorseOthers = true,
                    PolicyImpactMultiplier = 10,
                    MediaAttentionMultiplier = 10
                }
            };
        }
        
        /// <summary>
        /// Check if player meets requirements for office
        /// </summary>
        public bool CanRunForOffice(PlayerState player, OfficeType officeType)
        {
            if (!allOffices.ContainsKey(officeType))
                return false;
            
            var office = allOffices[officeType];
            var req = office.Requirements;
            
            // Check tier progression
            if (player.HighestTierHeld < req.MinimumTier)
                return false;
            
            // Check approval
            if (player.ApprovalRating < req.MinimumApproval)
                return false;
            
            // Check funds
            if (!player.Resources.ContainsKey("CampaignFunds") || 
                player.Resources["CampaignFunds"] < req.MinimumCampaignFunds)
                return false;
            
            // Check party loyalty
            if (req.MinimumPartyLoyalty > 0)
            {
                if (!player.Resources.ContainsKey("PartyLoyalty") || 
                    player.Resources["PartyLoyalty"] < req.MinimumPartyLoyalty)
                    return false;
            }
            
            // Check allies (simplified - would need actual ally tracking)
            // For now, assume player has allies if they have relationships
            
            // Check regions (for president)
            if (req.MinimumRegionsControlled > 0)
            {
                // Would need to calculate regions controlled
                // Simplified for now
            }
            
            return true;
        }
        
        /// <summary>
        /// Get available offices player can run for
        /// </summary>
        public List<Office> GetAvailableOffices(PlayerState player)
        {
            var available = new List<Office>();
            
            foreach (var office in allOffices.Values)
            {
                if (CanRunForOffice(player, office.Type))
                    available.Add(office);
            }
            
            return available;
        }
        
        /// <summary>
        /// Get next tier offices
        /// </summary>
        public List<Office> GetNextTierOffices(PlayerState player)
        {
            int nextTier = player.HighestTierHeld + 1;
            
            return allOffices.Values
                .Where(o => o.Tier == nextTier)
                .ToList();
        }
        
        /// <summary>
        /// Get Tier 1 offices (starting offices)
        /// </summary>
        public List<Office> GetTier1Offices()
        {
            return allOffices.Values
                .Where(o => o.Tier == 1)
                .ToList();
        }
        
        /// <summary>
        /// Get office by type
        /// </summary>
        public Office GetOffice(OfficeType type)
        {
            return allOffices.ContainsKey(type) ? allOffices[type] : null;
        }
    }
}

