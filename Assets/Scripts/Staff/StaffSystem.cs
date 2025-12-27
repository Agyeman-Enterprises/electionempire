using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ============================================================================
// ELECTION EMPIRE - STAFF SYSTEM
// Complete staff management: hiring, loyalty, specializations, betrayals
// ============================================================================

namespace ElectionEmpire.Staff
{
    #region Enums
    
    /// <summary>
    /// Staff member specializations - each provides unique bonuses
    /// </summary>
    public enum StaffSpecialization
    {
        // Campaign Staff
        CampaignManager,        // Overall campaign effectiveness
        PressSecretary,         // Media relations, crisis communication
        Pollster,               // Polling accuracy, voter analysis
        FundraisingDirector,    // Donation generation, donor relations
        FieldOrganizer,         // Grassroots, volunteer coordination
        DigitalDirector,        // Social media, online presence
        SpeechWriter,           // Speech effectiveness, debate prep
        OppositionResearcher,   // Dirt gathering, scandal detection
        
        // Governance Staff
        PolicyAdvisor,          // Policy effectiveness, implementation
        LegalCounsel,           // Legal protection, scandal defense
        ChiefOfStaff,           // Staff coordination, efficiency
        BudgetDirector,         // Financial management, resource optimization
        CommunicationsDirector, // Public messaging, approval management
        SecurityDirector,       // Protection, counter-intelligence
        
        // Special/Dark
        Fixer,                  // Dirty tricks, problem solving
        SpinDoctor,             // Scandal management, narrative control
        BagMan,                 // Dark money, off-books operations
        Enforcer,               // Intimidation, leverage execution
        Mole                    // Plant in opponent's campaign (hidden)
    }
    
    /// <summary>
    /// Staff quality tiers
    /// </summary>
    public enum StaffTier
    {
        Intern,         // Free but unreliable
        Junior,         // Cheap, learning
        Professional,   // Standard competence
        Senior,         // Experienced, reliable
        Expert,         // Top tier, expensive
        Legendary       // Best of the best, very rare
    }
    
    /// <summary>
    /// Current staff state
    /// </summary>
    public enum StaffState
    {
        Active,         // Working normally
        OnLeave,        // Temporary absence
        Suspended,      // Under investigation
        Compromised,    // Loyalty in question
        Burned,         // Sacrificed in scandal
        Resigned,       // Quit voluntarily
        Fired,          // Terminated by player
        Arrested,       // Legal troubles
        Flipped         // Working for opponent
    }
    
    /// <summary>
    /// Staff mood affects performance
    /// </summary>
    public enum StaffMorale
    {
        Ecstatic,       // 120% effectiveness
        Happy,          // 110% effectiveness
        Content,        // 100% effectiveness
        Neutral,        // 95% effectiveness
        Disgruntled,    // 80% effectiveness
        Unhappy,        // 60% effectiveness
        Mutinous        // 40% effectiveness, high betrayal risk
    }
    
    /// <summary>
    /// Reasons staff might leave or betray
    /// </summary>
    public enum DepartureReason
    {
        None,
        BetterOffer,
        MoralObjection,
        PersonalScandal,
        Burnout,
        PolicyDisagreement,
        LoyaltyToOpponent,
        Blackmailed,
        Arrested,
        Sacrificed,
        FiredWithCause,
        FiredWithoutCause,
        CampaignEnded,
        PlayerLost,
        Death
    }
    
    #endregion
    
    #region Staff Member
    
    /// <summary>
    /// Individual staff member with full attributes
    /// </summary>
    [Serializable]
    public class StaffMember
    {
        // Identity
        public string Id;
        public string FirstName;
        public string LastName;
        public string FullName => $"{FirstName} {LastName}";
        public string Name => FullName; // Alias for compatibility
        public string Nickname;
        public int Age;
        public string PortraitId;
        
        // Professional
        public StaffSpecialization PrimarySpecialization;
        public StaffSpecialization? SecondarySpecialization;
        public StaffTier Tier;
        public StaffState State;
        public int YearsExperience;
        
        // Stats (1-100 scale)
        public int Competence;          // Overall skill level
        public int Loyalty;             // Resistance to flipping/betrayal
        public int Discretion;          // Keeps secrets, avoids gaffes
        public int Ambition;            // Drive but also self-interest
        public int Connections;         // Network value
        public int Ruthlessness;        // Willing to do dirty work
        
        // State
        public StaffMorale Morale;
        public int Stress;              // 0-100, high stress = problems
        public float SalaryPerTurn;
        public int TurnsEmployed;
        public int ScandalsHandled;
        public int SuccessfulActions;
        public int FailedActions;
        
        // Relationships
        public Dictionary<string, int> RelationshipsWithStaff; // StaffId -> Opinion
        public int OpinionOfPlayer;     // -100 to 100
        public string FormerEmployer;   // Previous politician worked for
        public bool HasDirtOnPlayer;
        public int DirtSeverity;
        
        // Hidden/Secret
        public bool IsPlant;            // Secret mole for opponent
        public string PlantedBy;        // Opponent ID if plant
        public bool HasSecretAgenda;
        public string SecretAgendaDescription;
        
        // Personality
        public StaffPersonality Personality;
        
        // History
        public List<StaffHistoryEntry> History;
        public DateTime HireDate;
        public DateTime? DepartureDate;
        public DepartureReason? WhyLeft;
        
        public StaffMember()
        {
            Id = Guid.NewGuid().ToString("N").Substring(0, 8);
            RelationshipsWithStaff = new Dictionary<string, int>();
            History = new List<StaffHistoryEntry>();
            State = StaffState.Active;
            Morale = StaffMorale.Content;
        }
        
        /// <summary>
        /// Calculate overall effectiveness (0-150%)
        /// </summary>
        public float GetEffectiveness()
        {
            float baseEffectiveness = Competence / 100f;
            
            // Tier modifier
            float tierMod = Tier switch
            {
                StaffTier.Intern => 0.5f,
                StaffTier.Junior => 0.7f,
                StaffTier.Professional => 1.0f,
                StaffTier.Senior => 1.15f,
                StaffTier.Expert => 1.3f,
                StaffTier.Legendary => 1.5f,
                _ => 1.0f
            };
            
            // Morale modifier
            float moraleMod = Morale switch
            {
                StaffMorale.Ecstatic => 1.2f,
                StaffMorale.Happy => 1.1f,
                StaffMorale.Content => 1.0f,
                StaffMorale.Neutral => 0.95f,
                StaffMorale.Disgruntled => 0.8f,
                StaffMorale.Unhappy => 0.6f,
                StaffMorale.Mutinous => 0.4f,
                _ => 1.0f
            };
            
            // Stress penalty
            float stressMod = 1f - (Stress / 200f); // Max 50% penalty at 100 stress
            
            // State penalty
            float stateMod = State switch
            {
                StaffState.Active => 1.0f,
                StaffState.OnLeave => 0.0f,
                StaffState.Suspended => 0.0f,
                StaffState.Compromised => 0.5f,
                _ => 0.0f
            };
            
            return baseEffectiveness * tierMod * moraleMod * stressMod * stateMod;
        }
        
        /// <summary>
        /// Calculate betrayal risk (0-100%)
        /// </summary>
        public float GetBetrayalRisk()
        {
            if (State != StaffState.Active && State != StaffState.Compromised)
                return 0f;
            
            float baseRisk = 0f;
            
            // Low loyalty increases risk
            baseRisk += (100 - Loyalty) * 0.3f;
            
            // High ambition increases risk
            baseRisk += Ambition * 0.2f;
            
            // Bad morale increases risk
            baseRisk += Morale switch
            {
                StaffMorale.Ecstatic => -10f,
                StaffMorale.Happy => -5f,
                StaffMorale.Content => 0f,
                StaffMorale.Neutral => 5f,
                StaffMorale.Disgruntled => 15f,
                StaffMorale.Unhappy => 30f,
                StaffMorale.Mutinous => 50f,
                _ => 0f
            };
            
            // Negative opinion of player
            if (OpinionOfPlayer < 0)
                baseRisk += Mathf.Abs(OpinionOfPlayer) * 0.3f;
            
            // High stress increases risk
            baseRisk += Stress * 0.1f;
            
            // Having dirt on player increases risk
            if (HasDirtOnPlayer)
                baseRisk += DirtSeverity * 2f;
            
            // Plants always have high risk of "betrayal" (doing their job)
            if (IsPlant)
                baseRisk += 30f;
            
            return Mathf.Clamp(baseRisk, 0f, 100f);
        }
        
        /// <summary>
        /// Get specialization bonus for a specific action type
        /// </summary>
        public float GetSpecializationBonus(string actionType)
        {
            float bonus = 0f;
            
            // Primary specialization gives full bonus
            bonus += GetSpecBonusForAction(PrimarySpecialization, actionType, 1.0f);
            
            // Secondary gives half bonus
            if (SecondarySpecialization.HasValue)
                bonus += GetSpecBonusForAction(SecondarySpecialization.Value, actionType, 0.5f);
            
            return bonus;
        }
        
        private float GetSpecBonusForAction(StaffSpecialization spec, string action, float multiplier)
        {
            // Map specializations to action types
            Dictionary<StaffSpecialization, string[]> specActions = new()
            {
                { StaffSpecialization.CampaignManager, new[] { "campaign", "rally", "advertising", "strategy" } },
                { StaffSpecialization.PressSecretary, new[] { "press", "interview", "media", "crisis_comms" } },
                { StaffSpecialization.Pollster, new[] { "polling", "analysis", "targeting", "demographics" } },
                { StaffSpecialization.FundraisingDirector, new[] { "fundraising", "donors", "pac", "finance" } },
                { StaffSpecialization.FieldOrganizer, new[] { "grassroots", "volunteers", "canvassing", "gotv" } },
                { StaffSpecialization.DigitalDirector, new[] { "social_media", "digital", "online", "viral" } },
                { StaffSpecialization.SpeechWriter, new[] { "speech", "debate", "messaging", "rhetoric" } },
                { StaffSpecialization.OppositionResearcher, new[] { "oppo", "dirt", "investigation", "scandal_find" } },
                { StaffSpecialization.PolicyAdvisor, new[] { "policy", "legislation", "governance", "implementation" } },
                { StaffSpecialization.LegalCounsel, new[] { "legal", "defense", "compliance", "lawsuit" } },
                { StaffSpecialization.ChiefOfStaff, new[] { "coordination", "management", "efficiency", "staff" } },
                { StaffSpecialization.BudgetDirector, new[] { "budget", "spending", "allocation", "resources" } },
                { StaffSpecialization.CommunicationsDirector, new[] { "communications", "messaging", "approval", "public" } },
                { StaffSpecialization.SecurityDirector, new[] { "security", "protection", "counter_intel", "leaks" } },
                { StaffSpecialization.Fixer, new[] { "dirty_trick", "problem", "cover_up", "eliminate" } },
                { StaffSpecialization.SpinDoctor, new[] { "scandal_manage", "spin", "narrative", "damage_control" } },
                { StaffSpecialization.BagMan, new[] { "dark_money", "bribery", "payoff", "off_books" } },
                { StaffSpecialization.Enforcer, new[] { "intimidation", "blackmail", "leverage", "pressure" } },
                { StaffSpecialization.Mole, new[] { "intel", "sabotage", "infiltrate", "spy" } }
            };
            
            if (specActions.TryGetValue(spec, out var actions))
            {
                if (actions.Any(a => action.ToLower().Contains(a)))
                    return 0.25f * multiplier; // 25% bonus for matching spec
            }
            
            return 0f;
        }
        
        public void AddHistoryEntry(string description, bool isPositive)
        {
            History.Add(new StaffHistoryEntry
            {
                Timestamp = DateTime.Now,
                Description = description,
                IsPositive = isPositive
            });
        }
    }
    
    [Serializable]
    public class StaffHistoryEntry
    {
        public DateTime Timestamp;
        public string Description;
        public bool IsPositive;
    }
    
    #endregion
    
    #region Staff Roster
    
    /// <summary>
    /// Manages the player's staff roster
    /// </summary>
    public class StaffRoster
    {
        public List<StaffMember> ActiveStaff { get; private set; }
        public List<StaffMember> FormerStaff { get; private set; }
        public int MaxStaffSize { get; private set; }
        public float TotalSalaryPerTurn => ActiveStaff.Sum(s => s.SalaryPerTurn);
        
        // Events
        public event Action<StaffMember> OnStaffHired;
        public event Action<StaffMember, DepartureReason> OnStaffDeparted;
        public event Action<StaffMember> OnStaffBetrayed;
        public event Action<StaffMember> OnStaffPromoted;
        public event Action<StaffMember, StaffMorale> OnMoraleChanged;
        
        public StaffRoster()
        {
            ActiveStaff = new List<StaffMember>();
            FormerStaff = new List<StaffMember>();
            MaxStaffSize = 2; // Starting capacity
        }
        
        /// <summary>
        /// Update max staff based on office tier
        /// </summary>
        public void UpdateCapacity(int officeTier)
        {
            MaxStaffSize = officeTier switch
            {
                1 => 3,   // City Council: 3 staff
                2 => 6,   // Mayor: 6 staff
                3 => 12,  // State level: 12 staff
                4 => 25,  // National: 25 staff
                5 => 50,  // President: 50 staff
                _ => 3
            };
        }
        
        /// <summary>
        /// Hire a new staff member
        /// </summary>
        public bool HireStaff(StaffMember staff, float signingBonus = 0)
        {
            if (ActiveStaff.Count >= MaxStaffSize)
            {
                Debug.LogWarning("Staff roster at capacity!");
                return false;
            }
            
            if (ActiveStaff.Any(s => s.Id == staff.Id))
            {
                Debug.LogWarning("Staff member already on roster!");
                return false;
            }
            
            staff.HireDate = DateTime.Now;
            staff.TurnsEmployed = 0;
            staff.State = StaffState.Active;
            staff.Morale = StaffMorale.Happy; // New hires start happy
            staff.OpinionOfPlayer = 50; // Neutral positive
            
            if (signingBonus > 0)
            {
                staff.OpinionOfPlayer += (int)(signingBonus / 1000); // $1k = +1 opinion
                staff.AddHistoryEntry($"Hired with ${signingBonus:N0} signing bonus", true);
            }
            else
            {
                staff.AddHistoryEntry("Joined the campaign", true);
            }
            
            ActiveStaff.Add(staff);
            OnStaffHired?.Invoke(staff);
            
            return true;
        }
        
        /// <summary>
        /// Fire a staff member
        /// </summary>
        public void FireStaff(StaffMember staff, bool withCause, float severancePay = 0)
        {
            if (!ActiveStaff.Contains(staff))
                return;
            
            staff.State = StaffState.Fired;
            staff.DepartureDate = DateTime.Now;
            staff.WhyLeft = withCause ? DepartureReason.FiredWithCause : DepartureReason.FiredWithoutCause;
            
            if (!withCause && severancePay == 0)
            {
                // No severance = disgruntled former employee
                staff.OpinionOfPlayer -= 50;
                staff.AddHistoryEntry("Fired without cause or severance - bitter departure", false);
                
                // May leak dirt
                if (staff.HasDirtOnPlayer && UnityEngine.Random.value < 0.5f)
                {
                    // Trigger scandal event
                    Debug.Log($"{staff.FullName} is leaking dirt after being fired!");
                }
            }
            else
            {
                staff.AddHistoryEntry($"Departed with ${severancePay:N0} severance", true);
            }
            
            ActiveStaff.Remove(staff);
            FormerStaff.Add(staff);
            
            // Affect remaining staff morale
            foreach (var s in ActiveStaff)
            {
                if (s.RelationshipsWithStaff.TryGetValue(staff.Id, out int opinion))
                {
                    if (opinion > 50) // Friend was fired
                    {
                        s.OpinionOfPlayer -= 10;
                        UpdateMorale(s, -1);
                    }
                    else if (opinion < -50) // Enemy was fired
                    {
                        s.OpinionOfPlayer += 5;
                        UpdateMorale(s, 1);
                    }
                }
            }
            
            OnStaffDeparted?.Invoke(staff, staff.WhyLeft.Value);
        }
        
        /// <summary>
        /// Sacrifice a staff member to contain a scandal
        /// </summary>
        public void SacrificeStaff(StaffMember staff, string scandalDescription)
        {
            if (!ActiveStaff.Contains(staff))
                return;
            
            staff.State = StaffState.Burned;
            staff.DepartureDate = DateTime.Now;
            staff.WhyLeft = DepartureReason.Sacrificed;
            staff.OpinionOfPlayer = -100; // They hate you now
            staff.HasDirtOnPlayer = true; // They definitely know things
            staff.DirtSeverity = Mathf.Max(staff.DirtSeverity, 50);
            
            staff.AddHistoryEntry($"Sacrificed to cover scandal: {scandalDescription}", false);
            
            ActiveStaff.Remove(staff);
            FormerStaff.Add(staff);
            
            // Major morale hit to remaining staff
            foreach (var s in ActiveStaff)
            {
                s.OpinionOfPlayer -= 15;
                UpdateMorale(s, -2);
                s.AddHistoryEntry($"Witnessed colleague {staff.FullName} being sacrificed", false);
            }
            
            OnStaffDeparted?.Invoke(staff, DepartureReason.Sacrificed);
        }
        
        /// <summary>
        /// Staff member betrays player
        /// </summary>
        public void StaffBetrays(StaffMember staff, string betrayalType)
        {
            if (!ActiveStaff.Contains(staff))
                return;
            
            staff.State = StaffState.Flipped;
            staff.DepartureDate = DateTime.Now;
            staff.WhyLeft = DepartureReason.LoyaltyToOpponent;
            
            staff.AddHistoryEntry($"Betrayed the campaign: {betrayalType}", false);
            
            ActiveStaff.Remove(staff);
            FormerStaff.Add(staff);
            
            // Shock to remaining staff
            foreach (var s in ActiveStaff)
            {
                s.Stress += 15;
                UpdateMorale(s, -1);
                s.AddHistoryEntry($"Shocked by {staff.FullName}'s betrayal", false);
            }
            
            OnStaffBetrayed?.Invoke(staff);
        }
        
        /// <summary>
        /// Promote a staff member to higher tier
        /// </summary>
        public void PromoteStaff(StaffMember staff, float raiseAmount)
        {
            if (!ActiveStaff.Contains(staff))
                return;
            
            if (staff.Tier >= StaffTier.Legendary)
                return;
            
            staff.Tier = (StaffTier)((int)staff.Tier + 1);
            staff.SalaryPerTurn += raiseAmount;
            staff.OpinionOfPlayer += 20;
            staff.Competence = Mathf.Min(staff.Competence + 5, 100);
            UpdateMorale(staff, 2);
            
            staff.AddHistoryEntry($"Promoted to {staff.Tier} with ${raiseAmount:N0} raise", true);
            
            OnStaffPromoted?.Invoke(staff);
        }
        
        /// <summary>
        /// Give staff a raise (without promotion)
        /// </summary>
        public void GiveRaise(StaffMember staff, float amount)
        {
            if (!ActiveStaff.Contains(staff))
                return;
            
            staff.SalaryPerTurn += amount;
            staff.OpinionOfPlayer += (int)(amount / 200); // $200 = +1 opinion
            UpdateMorale(staff, 1);
            
            staff.AddHistoryEntry($"Received ${amount:N0} raise", true);
        }
        
        /// <summary>
        /// Update staff morale
        /// </summary>
        public void UpdateMorale(StaffMember staff, int change)
        {
            int currentMorale = (int)staff.Morale;
            int newMorale = Mathf.Clamp(currentMorale - change, 0, 6); // Lower enum = better morale
            
            StaffMorale oldMorale = staff.Morale;
            staff.Morale = (StaffMorale)newMorale;
            
            if (oldMorale != staff.Morale)
            {
                OnMoraleChanged?.Invoke(staff, staff.Morale);
            }
        }
        
        /// <summary>
        /// Process end of turn updates for all staff
        /// </summary>
        public void ProcessTurn()
        {
            foreach (var staff in ActiveStaff.ToList()) // ToList to allow modification
            {
                staff.TurnsEmployed++;
                
                // Natural stress decay
                staff.Stress = Mathf.Max(0, staff.Stress - 5);
                
                // Loyalty slowly increases with tenure
                if (staff.TurnsEmployed % 10 == 0)
                {
                    staff.Loyalty = Mathf.Min(100, staff.Loyalty + 1);
                }
                
                // Check for resignation
                if (staff.Morale == StaffMorale.Mutinous && UnityEngine.Random.value < 0.2f)
                {
                    staff.State = StaffState.Resigned;
                    staff.WhyLeft = DepartureReason.Burnout;
                    staff.DepartureDate = DateTime.Now;
                    staff.AddHistoryEntry("Resigned due to poor morale", false);
                    
                    ActiveStaff.Remove(staff);
                    FormerStaff.Add(staff);
                    OnStaffDeparted?.Invoke(staff, DepartureReason.Burnout);
                    continue;
                }
                
                // Check for betrayal
                float betrayalRisk = staff.GetBetrayalRisk();
                if (betrayalRisk > 50 && UnityEngine.Random.value * 100 < betrayalRisk * 0.1f)
                {
                    string betrayalType = staff.IsPlant ? "Revealed as a plant" : "Leaked information to opponent";
                    StaffBetrays(staff, betrayalType);
                }
            }
        }
        
        /// <summary>
        /// Get staff member by specialization
        /// </summary>
        public StaffMember GetBySpecialization(StaffSpecialization spec)
        {
            return ActiveStaff
                .Where(s => s.State == StaffState.Active)
                .FirstOrDefault(s => s.PrimarySpecialization == spec || s.SecondarySpecialization == spec);
        }
        
        /// <summary>
        /// Get all staff with a specific specialization
        /// </summary>
        public List<StaffMember> GetAllBySpecialization(StaffSpecialization spec)
        {
            return ActiveStaff
                .Where(s => s.State == StaffState.Active)
                .Where(s => s.PrimarySpecialization == spec || s.SecondarySpecialization == spec)
                .ToList();
        }
        
        /// <summary>
        /// Calculate aggregate effectiveness for an action type
        /// </summary>
        public float GetTeamEffectiveness(string actionType)
        {
            float total = 0f;
            
            foreach (var staff in ActiveStaff.Where(s => s.State == StaffState.Active))
            {
                float effectiveness = staff.GetEffectiveness();
                float specBonus = staff.GetSpecializationBonus(actionType);
                total += effectiveness * (1 + specBonus);
            }
            
            // Diminishing returns after a point
            return Mathf.Log10(total + 1) * 30f;
        }
        
        /// <summary>
        /// Apply global morale effect (election win/loss, major event)
        /// </summary>
        public void ApplyGlobalMoraleEffect(int change, string reason)
        {
            foreach (var staff in ActiveStaff)
            {
                UpdateMorale(staff, change);
                
                if (change > 0)
                    staff.AddHistoryEntry($"Morale boosted: {reason}", true);
                else if (change < 0)
                    staff.AddHistoryEntry($"Morale hit: {reason}", false);
            }
        }
    }
    
    #endregion
    
    #region Staff Generator
    
    /// <summary>
    /// Procedurally generates staff members for hiring pool
    /// </summary>
    public static class StaffGenerator
    {
        // Name pools
        private static readonly string[] FirstNamesMale = {
            "James", "Michael", "Robert", "David", "William", "John", "Richard", "Thomas",
            "Charles", "Daniel", "Marcus", "Kevin", "Brian", "Anthony", "Steven", "Paul",
            "Jose", "Carlos", "Miguel", "Andre", "Darius", "Malik", "Jamal", "Terrence"
        };
        
        private static readonly string[] FirstNamesFemale = {
            "Jennifer", "Elizabeth", "Sarah", "Jessica", "Michelle", "Ashley", "Amanda", "Stephanie",
            "Nicole", "Melissa", "Rebecca", "Katherine", "Maria", "Patricia", "Angela", "Diana",
            "Carmen", "Rosa", "Luz", "Aaliyah", "Jasmine", "Keisha", "Tamara", "Crystal"
        };
        
        private static readonly string[] LastNames = {
            "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis",
            "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson",
            "Thomas", "Taylor", "Moore", "Jackson", "Martin", "Lee", "Thompson", "White",
            "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson", "Walker", "Young",
            "Allen", "King", "Wright", "Scott", "Torres", "Nguyen", "Hill", "Flores", "Green",
            "Adams", "Nelson", "Baker", "Hall", "Rivera", "Campbell", "Mitchell", "Carter"
        };
        
        private static readonly string[] Nicknames = {
            "The Shark", "Numbers", "The Fixer", "Spinmaster", "The Hammer", "Old Guard",
            "The Kid", "Silver Tongue", "Ice", "The Professor", "Bulldog", "The Shadow",
            "Rainmaker", "The Closer", "Pit Bull", "The Whisperer", "Iron Lady", "The Brain"
        };
        
        /// <summary>
        /// Generate a random staff member
        /// </summary>
        public static StaffMember Generate(StaffTier minTier = StaffTier.Junior, 
                                           StaffTier maxTier = StaffTier.Senior,
                                           StaffSpecialization? forcedSpec = null)
        {
            var staff = new StaffMember();
            
            // Gender and name
            bool isMale = UnityEngine.Random.value > 0.5f;
            staff.FirstName = isMale 
                ? FirstNamesMale[UnityEngine.Random.Range(0, FirstNamesMale.Length)]
                : FirstNamesFemale[UnityEngine.Random.Range(0, FirstNamesFemale.Length)];
            staff.LastName = LastNames[UnityEngine.Random.Range(0, LastNames.Length)];
            
            // Maybe a nickname
            if (UnityEngine.Random.value < 0.2f)
                staff.Nickname = Nicknames[UnityEngine.Random.Range(0, Nicknames.Length)];
            
            // Tier
            int tierRange = (int)maxTier - (int)minTier + 1;
            staff.Tier = (StaffTier)((int)minTier + UnityEngine.Random.Range(0, tierRange));
            
            // Age based on tier
            staff.Age = staff.Tier switch
            {
                StaffTier.Intern => UnityEngine.Random.Range(18, 24),
                StaffTier.Junior => UnityEngine.Random.Range(22, 30),
                StaffTier.Professional => UnityEngine.Random.Range(28, 45),
                StaffTier.Senior => UnityEngine.Random.Range(35, 55),
                StaffTier.Expert => UnityEngine.Random.Range(40, 65),
                StaffTier.Legendary => UnityEngine.Random.Range(50, 75),
                _ => 30
            };
            
            staff.YearsExperience = Mathf.Max(0, staff.Age - 22 - UnityEngine.Random.Range(0, 10));
            
            // Specialization
            if (forcedSpec.HasValue)
            {
                staff.PrimarySpecialization = forcedSpec.Value;
            }
            else
            {
                var specs = Enum.GetValues(typeof(StaffSpecialization));
                staff.PrimarySpecialization = (StaffSpecialization)specs.GetValue(
                    UnityEngine.Random.Range(0, specs.Length));
            }
            
            // Maybe secondary specialization
            if (staff.Tier >= StaffTier.Professional && UnityEngine.Random.value < 0.4f)
            {
                var specs = Enum.GetValues(typeof(StaffSpecialization));
                StaffSpecialization secondary;
                do
                {
                    secondary = (StaffSpecialization)specs.GetValue(
                        UnityEngine.Random.Range(0, specs.Length));
                } while (secondary == staff.PrimarySpecialization);
                
                staff.SecondarySpecialization = secondary;
            }
            
            // Stats based on tier
            int baseStat = staff.Tier switch
            {
                StaffTier.Intern => 20,
                StaffTier.Junior => 35,
                StaffTier.Professional => 50,
                StaffTier.Senior => 65,
                StaffTier.Expert => 80,
                StaffTier.Legendary => 90,
                _ => 50
            };
            
            int variance = 15;
            staff.Competence = Mathf.Clamp(baseStat + UnityEngine.Random.Range(-variance, variance), 1, 100);
            staff.Loyalty = Mathf.Clamp(50 + UnityEngine.Random.Range(-20, 30), 1, 100);
            staff.Discretion = Mathf.Clamp(baseStat + UnityEngine.Random.Range(-variance, variance), 1, 100);
            staff.Ambition = UnityEngine.Random.Range(20, 90);
            staff.Connections = Mathf.Clamp(baseStat + UnityEngine.Random.Range(-variance, variance), 1, 100);
            staff.Ruthlessness = UnityEngine.Random.Range(10, 80);
            
            // Dark specializations have higher ruthlessness
            if (staff.PrimarySpecialization is StaffSpecialization.Fixer or 
                StaffSpecialization.SpinDoctor or StaffSpecialization.BagMan or 
                StaffSpecialization.Enforcer or StaffSpecialization.Mole)
            {
                staff.Ruthlessness = Mathf.Clamp(staff.Ruthlessness + 30, 1, 100);
                staff.Discretion = Mathf.Clamp(staff.Discretion + 20, 1, 100);
            }
            
            // Salary
            staff.SalaryPerTurn = staff.Tier switch
            {
                StaffTier.Intern => 0f,
                StaffTier.Junior => 500f + UnityEngine.Random.Range(0, 300),
                StaffTier.Professional => 1500f + UnityEngine.Random.Range(0, 500),
                StaffTier.Senior => 3000f + UnityEngine.Random.Range(0, 1000),
                StaffTier.Expert => 5000f + UnityEngine.Random.Range(0, 2000),
                StaffTier.Legendary => 10000f + UnityEngine.Random.Range(0, 5000),
                _ => 1000f
            };
            
            // Dark specialists cost more
            if (staff.PrimarySpecialization is StaffSpecialization.Fixer or 
                StaffSpecialization.BagMan or StaffSpecialization.Enforcer)
            {
                staff.SalaryPerTurn *= 1.5f;
            }
            
            // Initial state
            staff.Morale = StaffMorale.Content;
            staff.Stress = UnityEngine.Random.Range(0, 30);
            staff.OpinionOfPlayer = 0;
            
            // Small chance to be a secret plant
            if (UnityEngine.Random.value < 0.05f)
            {
                staff.IsPlant = true;
                staff.Loyalty = Mathf.Min(staff.Loyalty, 30);
            }
            
            // Small chance to have a secret agenda
            if (!staff.IsPlant && UnityEngine.Random.value < 0.1f)
            {
                staff.HasSecretAgenda = true;
                staff.SecretAgendaDescription = GenerateSecretAgenda();
            }
            
            return staff;
        }
        
        /// <summary>
        /// Generate a hiring pool of candidates
        /// </summary>
        public static List<StaffMember> GenerateHiringPool(int count, int officeTier)
        {
            var pool = new List<StaffMember>();
            
            StaffTier minTier = officeTier switch
            {
                1 => StaffTier.Intern,
                2 => StaffTier.Junior,
                3 => StaffTier.Professional,
                4 => StaffTier.Senior,
                5 => StaffTier.Expert,
                _ => StaffTier.Junior
            };
            
            StaffTier maxTier = officeTier switch
            {
                1 => StaffTier.Professional,
                2 => StaffTier.Senior,
                3 => StaffTier.Expert,
                4 => StaffTier.Expert,
                5 => StaffTier.Legendary,
                _ => StaffTier.Professional
            };
            
            for (int i = 0; i < count; i++)
            {
                pool.Add(Generate(minTier, maxTier));
            }
            
            return pool;
        }
        
        private static string GenerateSecretAgenda()
        {
            string[] agendas = {
                "Planning to write tell-all book after leaving",
                "Secretly recording conversations",
                "Building own political career on your coattails",
                "Funneling donor money to personal account",
                "Planning to replace you as candidate",
                "Working to install their preferred policy regardless of your wishes",
                "Building leverage for future blackmail",
                "Romantically pursuing another staff member inappropriately"
            };
            
            return agendas[UnityEngine.Random.Range(0, agendas.Length)];
        }
    }
    
    #endregion
    
    #region Staff Actions
    
    /// <summary>
    /// Actions that can be taken by or with staff
    /// </summary>
    public class StaffAction
    {
        public string Id;
        public string Name;
        public string Description;
        public StaffSpecialization RequiredSpecialization;
        public StaffTier MinimumTier;
        public float BaseCost;
        public float BaseSuccessChance;
        public int StressIncrease;
        public Dictionary<string, float> Effects;
        
        // Calculate success chance with assigned staff
        public float GetSuccessChance(StaffMember staff)
        {
            if (staff == null) return BaseSuccessChance * 0.5f;
            
            float chance = BaseSuccessChance;
            
            // Staff effectiveness
            chance *= (0.5f + staff.GetEffectiveness() * 0.5f);
            
            // Specialization bonus
            if (staff.PrimarySpecialization == RequiredSpecialization)
                chance *= 1.25f;
            else if (staff.SecondarySpecialization == RequiredSpecialization)
                chance *= 1.1f;
            
            // Tier bonus
            if (staff.Tier >= MinimumTier)
                chance *= 1.0f + ((int)staff.Tier - (int)MinimumTier) * 0.1f;
            
            return Mathf.Clamp01(chance);
        }
    }
    
    /// <summary>
    /// Predefined staff actions
    /// </summary>
    public static class StaffActions
    {
        public static StaffAction CoordinateCampaign = new()
        {
            Id = "coordinate_campaign",
            Name = "Coordinate Campaign",
            Description = "Organize overall campaign operations for improved efficiency",
            RequiredSpecialization = StaffSpecialization.CampaignManager,
            MinimumTier = StaffTier.Professional,
            BaseCost = 1000f,
            BaseSuccessChance = 0.7f,
            StressIncrease = 10,
            Effects = new Dictionary<string, float>
            {
                { "campaign_effectiveness", 0.15f },
                { "action_efficiency", 0.1f }
            }
        };
        
        public static StaffAction HandlePress = new()
        {
            Id = "handle_press",
            Name = "Handle Press Relations",
            Description = "Manage media relationships and spin stories",
            RequiredSpecialization = StaffSpecialization.PressSecretary,
            MinimumTier = StaffTier.Junior,
            BaseCost = 500f,
            BaseSuccessChance = 0.6f,
            StressIncrease = 15,
            Effects = new Dictionary<string, float>
            {
                { "media_influence", 10f },
                { "scandal_chance", -0.1f }
            }
        };
        
        public static StaffAction ConductPoll = new()
        {
            Id = "conduct_poll",
            Name = "Conduct Internal Polling",
            Description = "Get accurate read on voter sentiment",
            RequiredSpecialization = StaffSpecialization.Pollster,
            MinimumTier = StaffTier.Professional,
            BaseCost = 2000f,
            BaseSuccessChance = 0.8f,
            StressIncrease = 5,
            Effects = new Dictionary<string, float>
            {
                { "poll_accuracy", 0.9f },
                { "voter_insight", 1f }
            }
        };
        
        public static StaffAction DigDirt = new()
        {
            Id = "dig_dirt",
            Name = "Dig Up Dirt",
            Description = "Research opposition for damaging information",
            RequiredSpecialization = StaffSpecialization.OppositionResearcher,
            MinimumTier = StaffTier.Professional,
            BaseCost = 3000f,
            BaseSuccessChance = 0.4f,
            StressIncrease = 20,
            Effects = new Dictionary<string, float>
            {
                { "dirt_found", 1f },
                { "backfire_risk", 0.1f }
            }
        };
        
        public static StaffAction ManageScandal = new()
        {
            Id = "manage_scandal",
            Name = "Manage Scandal",
            Description = "Contain and spin a developing scandal",
            RequiredSpecialization = StaffSpecialization.SpinDoctor,
            MinimumTier = StaffTier.Senior,
            BaseCost = 5000f,
            BaseSuccessChance = 0.5f,
            StressIncrease = 35,
            Effects = new Dictionary<string, float>
            {
                { "scandal_severity", -0.3f },
                { "trust_recovery", 0.1f }
            }
        };
        
        public static StaffAction ExecuteDirtyTrick = new()
        {
            Id = "dirty_trick",
            Name = "Execute Dirty Trick",
            Description = "Underhanded tactics against opponent",
            RequiredSpecialization = StaffSpecialization.Fixer,
            MinimumTier = StaffTier.Senior,
            BaseCost = 10000f,
            BaseSuccessChance = 0.45f,
            StressIncrease = 40,
            Effects = new Dictionary<string, float>
            {
                { "opponent_damage", 0.2f },
                { "scandal_risk", 0.25f },
                { "alignment_evil", 5f }
            }
        };
        
        public static StaffAction CoordinateFundraising = new()
        {
            Id = "fundraising",
            Name = "Coordinate Fundraising",
            Description = "Organize donor outreach and events",
            RequiredSpecialization = StaffSpecialization.FundraisingDirector,
            MinimumTier = StaffTier.Professional,
            BaseCost = 500f,
            BaseSuccessChance = 0.65f,
            StressIncrease = 15,
            Effects = new Dictionary<string, float>
            {
                { "fundraising_multiplier", 0.25f },
                { "donor_relations", 0.1f }
            }
        };
        
        public static StaffAction LegalDefense = new()
        {
            Id = "legal_defense",
            Name = "Legal Defense",
            Description = "Prepare legal strategy for investigations or lawsuits",
            RequiredSpecialization = StaffSpecialization.LegalCounsel,
            MinimumTier = StaffTier.Senior,
            BaseCost = 8000f,
            BaseSuccessChance = 0.6f,
            StressIncrease = 25,
            Effects = new Dictionary<string, float>
            {
                { "legal_protection", 0.5f },
                { "investigation_block", 0.3f }
            }
        };
    }
    
    #endregion
    
    #region Staff Manager (Main System)
    
    /// <summary>
    /// Main staff management system
    /// </summary>
    public class StaffManager : MonoBehaviour
    {
        public static StaffManager Instance { get; private set; }
        
        [Header("Staff Configuration")]
        public int HiringPoolRefreshTurns = 5;
        public int HiringPoolSize = 8;
        
        // Core systems
        public StaffRoster Roster { get; private set; }
        public List<StaffMember> HiringPool { get; private set; }
        
        // State
        private int _turnsSincePoolRefresh;
        private int _currentOfficeTier = 1;
        
        // Events
        public event Action OnHiringPoolRefreshed;
        public event Action<StaffAction, StaffMember, bool> OnActionCompleted;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            Roster = new StaffRoster();
            HiringPool = new List<StaffMember>();
            
            RefreshHiringPool();
        }
        
        /// <summary>
        /// Update office tier (affects capacity and hiring pool quality)
        /// </summary>
        public void SetOfficeTier(int tier)
        {
            _currentOfficeTier = tier;
            Roster.UpdateCapacity(tier);
            RefreshHiringPool();
        }
        
        /// <summary>
        /// Refresh the hiring pool with new candidates
        /// </summary>
        public void RefreshHiringPool()
        {
            HiringPool = StaffGenerator.GenerateHiringPool(HiringPoolSize, _currentOfficeTier);
            _turnsSincePoolRefresh = 0;
            OnHiringPoolRefreshed?.Invoke();
        }
        
        /// <summary>
        /// Process end of turn
        /// </summary>
        public void ProcessTurn()
        {
            // Update staff
            Roster.ProcessTurn();
            
            // Maybe refresh hiring pool
            _turnsSincePoolRefresh++;
            if (_turnsSincePoolRefresh >= HiringPoolRefreshTurns)
            {
                RefreshHiringPool();
            }
        }
        
        /// <summary>
        /// Execute a staff action
        /// </summary>
        public bool ExecuteAction(StaffAction action, StaffMember assignedStaff)
        {
            if (assignedStaff == null || assignedStaff.State != StaffState.Active)
            {
                Debug.LogWarning("No active staff member assigned to action");
                return false;
            }
            
            // Check tier requirement
            if (assignedStaff.Tier < action.MinimumTier)
            {
                Debug.LogWarning($"{assignedStaff.FullName} does not meet tier requirement for {action.Name}");
                return false;
            }
            
            // Calculate success
            float successChance = action.GetSuccessChance(assignedStaff);
            bool success = UnityEngine.Random.value < successChance;
            
            // Apply stress
            assignedStaff.Stress = Mathf.Min(100, assignedStaff.Stress + action.StressIncrease);
            
            // Track results
            if (success)
            {
                assignedStaff.SuccessfulActions++;
                assignedStaff.AddHistoryEntry($"Successfully completed: {action.Name}", true);
                
                // Increase opinion slightly
                assignedStaff.OpinionOfPlayer += 2;
            }
            else
            {
                assignedStaff.FailedActions++;
                assignedStaff.AddHistoryEntry($"Failed action: {action.Name}", false);
                
                // High stress from failure
                assignedStaff.Stress = Mathf.Min(100, assignedStaff.Stress + 10);
                
                // Check for morale hit
                if (UnityEngine.Random.value < 0.3f)
                    Roster.UpdateMorale(assignedStaff, -1);
            }
            
            OnActionCompleted?.Invoke(action, assignedStaff, success);
            
            return success;
        }
        
        /// <summary>
        /// Find best staff member for a given action
        /// </summary>
        public StaffMember GetBestStaffForAction(StaffAction action)
        {
            return Roster.ActiveStaff
                .Where(s => s.State == StaffState.Active)
                .Where(s => s.Tier >= action.MinimumTier)
                .OrderByDescending(s => action.GetSuccessChance(s))
                .FirstOrDefault();
        }
        
        /// <summary>
        /// Get staff recommendations for player
        /// </summary>
        public List<(StaffMember staff, string reason)> GetHiringRecommendations()
        {
            var recommendations = new List<(StaffMember, string)>();
            
            // Check what specializations we're missing
            var currentSpecs = Roster.ActiveStaff
                .Select(s => s.PrimarySpecialization)
                .ToHashSet();
            
            // Essential specs by tier
            var essentialSpecs = new List<StaffSpecialization>
            {
                StaffSpecialization.CampaignManager,
                StaffSpecialization.PressSecretary,
                StaffSpecialization.FundraisingDirector
            };
            
            if (_currentOfficeTier >= 2)
            {
                essentialSpecs.Add(StaffSpecialization.Pollster);
                essentialSpecs.Add(StaffSpecialization.OppositionResearcher);
            }
            
            if (_currentOfficeTier >= 3)
            {
                essentialSpecs.Add(StaffSpecialization.PolicyAdvisor);
                essentialSpecs.Add(StaffSpecialization.LegalCounsel);
            }
            
            if (_currentOfficeTier >= 4)
            {
                essentialSpecs.Add(StaffSpecialization.SpinDoctor);
                essentialSpecs.Add(StaffSpecialization.ChiefOfStaff);
            }
            
            foreach (var spec in essentialSpecs)
            {
                if (!currentSpecs.Contains(spec))
                {
                    var candidate = HiringPool
                        .Where(s => s.PrimarySpecialization == spec)
                        .OrderByDescending(s => s.Competence)
                        .FirstOrDefault();
                    
                    if (candidate != null)
                    {
                        recommendations.Add((candidate, $"Fill missing {spec} role"));
                    }
                }
            }
            
            // Also recommend high-quality candidates
            var topTier = HiringPool
                .Where(s => s.Tier >= StaffTier.Senior)
                .Where(s => !recommendations.Any(r => r.Item1.Id == s.Id))
                .OrderByDescending(s => s.Competence)
                .Take(2);
            
            foreach (var candidate in topTier)
            {
                recommendations.Add((candidate, "Exceptional talent available"));
            }
            
            return recommendations;
        }
        
        /// <summary>
        /// Handle event that affects all staff (election result, scandal, etc.)
        /// </summary>
        public void HandleMajorEvent(string eventType, bool positive)
        {
            int moraleChange = positive ? 2 : -2;
            string reason = eventType;
            
            switch (eventType.ToLower())
            {
                case "election_won":
                    moraleChange = 3;
                    reason = "Election victory!";
                    break;
                case "election_lost":
                    moraleChange = -3;
                    reason = "Election defeat";
                    break;
                case "major_scandal":
                    moraleChange = -2;
                    reason = "Major scandal rocking the campaign";
                    // Also increase stress
                    foreach (var s in Roster.ActiveStaff)
                        s.Stress = Mathf.Min(100, s.Stress + 20);
                    break;
                case "scandal_survived":
                    moraleChange = 1;
                    reason = "Survived scandal intact";
                    break;
                case "promotion":
                    moraleChange = 2;
                    reason = "Candidate advanced to higher office";
                    break;
                case "pay_raise":
                    moraleChange = 1;
                    reason = "Team-wide bonus";
                    break;
                case "staff_sacrificed":
                    moraleChange = -3;
                    reason = "Colleague thrown under the bus";
                    break;
            }
            
            Roster.ApplyGlobalMoraleEffect(moraleChange, reason);
        }
    }
    
    #endregion
}
