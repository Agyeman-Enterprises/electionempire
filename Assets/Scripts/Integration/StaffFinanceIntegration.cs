using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.Staff;
using ElectionEmpire.Finance;

// ============================================================================
// ELECTION EMPIRE - STAFF & FINANCE INTEGRATION
// Connects payroll, manages emergent staff events, handles the chaos
// ============================================================================

namespace ElectionEmpire.Integration
{
    /// <summary>
    /// Integrates Staff and Finance systems - handles payroll and related events
    /// </summary>
    public class StaffPayrollManager : MonoBehaviour
    {
        public static StaffPayrollManager Instance { get; private set; }
        
        [Header("Payroll Settings")]
        public int PayrollTurnFrequency = 1;    // Pay every X turns
        public float LateBonusPenalty = 0.1f;   // 10% morale hit if late
        
        private StaffManager _staffManager;
        private CampaignFinanceManager _financeManager;
        private int _turnsSincePayroll;
        
        // Events
        public event Action<List<StaffMember>> OnPayrollProcessed;
        public event Action<List<StaffMember>> OnPayrollFailed;
        public event Action<StaffMember, float> OnBonusPaid;
        public event Action<StaffMember> OnStaffQuitOverPay;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        private void Start()
        {
            _staffManager = StaffManager.Instance;
            _financeManager = CampaignFinanceManager.Instance;
            
            // Subscribe to events
            if (_financeManager != null)
            {
                _financeManager.OnPayrollMissed += HandleMissedPayroll;
            }
        }
        
        /// <summary>
        /// Process payroll for all staff
        /// </summary>
        public PayrollResult ProcessPayroll()
        {
            var result = new PayrollResult();
            
            if (_staffManager?.Roster == null || _financeManager == null)
            {
                result.Success = false;
                result.Message = "Systems not initialized";
                return result;
            }
            
            float totalPayroll = _staffManager.Roster.TotalSalaryPerTurn;
            var account = _financeManager.Accounts[AccountType.CampaignFund];
            
            // Check if we can make payroll
            if (account.Balance < totalPayroll)
            {
                result.Success = false;
                result.TotalOwed = totalPayroll;
                result.Shortfall = totalPayroll - account.Balance;
                result.Message = $"Cannot make payroll! Short ${result.Shortfall:N0}";
                
                // Handle consequences
                HandlePayrollShortfall(result.Shortfall);
                OnPayrollFailed?.Invoke(_staffManager.Roster.ActiveStaff.ToList());
                
                return result;
            }
            
            // Process individual salaries
            foreach (var staff in _staffManager.Roster.ActiveStaff)
            {
                if (staff.State != StaffState.Active) continue;
                
                var expense = new RecurringExpense
                {
                    Name = $"Salary: {staff.FullName}",
                    Category = ExpenseCategory.StaffSalaries,
                    AmountPerTurn = staff.SalaryPerTurn,
                    RelatedEntityId = staff.Id,
                    IsEssential = true
                };
                
                var txResult = _financeManager.ProcessExpense(
                    staff.SalaryPerTurn,
                    ExpenseCategory.StaffSalaries,
                    $"Salary: {staff.FullName}",
                    relatedEntityId: staff.Id
                );
                
                if (txResult.Success)
                {
                    result.StaffPaid.Add(staff);
                    result.TotalPaid += staff.SalaryPerTurn;
                }
                else
                {
                    result.StaffUnpaid.Add(staff);
                }
            }
            
            result.Success = result.StaffUnpaid.Count == 0;
            result.Message = result.Success 
                ? $"Paid ${result.TotalPaid:N0} to {result.StaffPaid.Count} staff"
                : $"Paid {result.StaffPaid.Count}, failed to pay {result.StaffUnpaid.Count}";
            
            _turnsSincePayroll = 0;
            OnPayrollProcessed?.Invoke(result.StaffPaid);
            
            return result;
        }
        
        /// <summary>
        /// Handle consequences of not making payroll
        /// </summary>
        private void HandlePayrollShortfall(float shortfall)
        {
            foreach (var staff in _staffManager.Roster.ActiveStaff.ToList())
            {
                // Major morale hit
                _staffManager.Roster.UpdateMorale(staff, -3);
                staff.OpinionOfPlayer -= 20;
                staff.Stress += 25;
                
                staff.AddHistoryEntry("Paycheck bounced!", false);
                
                // High loyalty staff might stick around
                if (staff.Loyalty < 50 && UnityEngine.Random.value < 0.3f)
                {
                    // They quit
                    staff.State = StaffState.Resigned;
                    staff.WhyLeft = DepartureReason.BetterOffer;
                    staff.AddHistoryEntry("Quit after missed payroll", false);
                    
                    _staffManager.Roster.ActiveStaff.Remove(staff);
                    _staffManager.Roster.FormerStaff.Add(staff);
                    
                    OnStaffQuitOverPay?.Invoke(staff);
                }
            }
        }
        
        /// <summary>
        /// Give a bonus to a staff member
        /// </summary>
        public bool GiveBonus(StaffMember staff, float amount, string reason)
        {
            var result = _financeManager.ProcessExpense(
                amount,
                ExpenseCategory.StaffSalaries,
                $"Bonus for {staff.FullName}: {reason}",
                relatedEntityId: staff.Id
            );
            
            if (result.Success)
            {
                staff.OpinionOfPlayer += (int)(amount / 500);
                _staffManager.Roster.UpdateMorale(staff, 2);
                staff.AddHistoryEntry($"Received ${amount:N0} bonus: {reason}", true);
                
                OnBonusPaid?.Invoke(staff, amount);
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Register staff salaries as recurring expenses
        /// </summary>
        public void RegisterStaffSalaries()
        {
            foreach (var staff in _staffManager.Roster.ActiveStaff)
            {
                var expense = new RecurringExpense
                {
                    Name = $"Salary: {staff.FullName}",
                    Category = ExpenseCategory.StaffSalaries,
                    AmountPerTurn = staff.SalaryPerTurn,
                    RelatedEntityId = staff.Id,
                    IsEssential = true,
                    PayFromAccount = AccountType.CampaignFund
                };
                
                _financeManager.AddRecurringExpense(expense);
            }
        }
        
        private void HandleMissedPayroll()
        {
            Debug.LogWarning("PAYROLL MISSED - Staff will be unhappy!");
            HandlePayrollShortfall(_staffManager.Roster.TotalSalaryPerTurn);
        }
    }
    
    public class PayrollResult
    {
        public bool Success;
        public string Message;
        public float TotalPaid;
        public float TotalOwed;
        public float Shortfall;
        public List<StaffMember> StaffPaid = new();
        public List<StaffMember> StaffUnpaid = new();
    }
    
    // ========================================================================
    // EMERGENT STAFF EVENT LIBRARY
    // The really fun, chaotic, memorable moments
    // ========================================================================
    
    /// <summary>
    /// Library of emergent staff events that create memorable moments
    /// </summary>
    public static class EmergentStaffEvents
    {
        /// <summary>
        /// Get a random emergent event based on current staff state
        /// </summary>
        public static StaffEventData GenerateEmergentEvent(StaffRoster roster)
        {
            var events = new List<Func<StaffRoster, StaffEventData>>
            {
                GenerateReligiousConversionEvent,
                GenerateTellAllBookEvent,
                GenerateRomanceWithOpponentEvent,
                GenerateDrunkInterviewEvent,
                GenerateSecretlyAPlantEvent,
                GenerateWhistleblowerEvent,
                GenerateMidlifeCrisisEvent,
                GenerateStaffFeudEvent,
                GenerateViralTweetEvent,
                GenerateFoundingMomentEvent,
                GenerateDramaticResignationEvent,
                GenerateUnexpectedHeroEvent,
                GenerateDocumentaryRevealEvent,
                GenerateFamilyEmergencyEvent,
                GeneratePodcastAppearanceEvent
            };
            
            // Try each until we find one that applies
            foreach (var generator in events.OrderBy(_ => UnityEngine.Random.value))
            {
                var evt = generator(roster);
                if (evt != null) return evt;
            }
            
            return null;
        }
        
        #region Religious/Moral Events
        
        private static StaffEventData GenerateReligiousConversionEvent(StaffRoster roster)
        {
            // Find a ruthless staffer who's done dirty work
            var candidate = roster.ActiveStaff
                .Where(s => s.Personality.HasValue(StaffValues.Ruthless))
                .Where(s => s.ScandalsHandled > 0 || s.PrimarySpecialization == StaffSpecialization.Fixer)
                .OrderByDescending(s => s.TurnsEmployed)
                .FirstOrDefault();
            
            if (candidate == null) return null;
            if (UnityEngine.Random.value > 0.02f) return null; // 2% chance
            
            return new StaffEventData
            {
                Type = StaffEventType.ReligiousConversion,
                StaffInvolved = new[] { candidate },
                Title = $"🙏 {candidate.FullName} Has Found Jesus",
                Description = $"Your fixer {candidate.FullName} appeared on Good Morning America this morning " +
                              $"to announce they've had a 'spiritual awakening' and can no longer participate in " +
                              $"'morally bankrupt political operations.' They then proceeded to confess to " +
                              $"EVERYTHING they've done for the campaign on live television, weeping and asking " +
                              $"forgiveness from the Lord and the American people.\n\n" +
                              $"Your phone is melting. Reporters are already at headquarters.",
                Severity = 5,
                Choices = new List<StaffEventChoice>
                {
                    new() {
                        Text = "Deny everything - they're clearly having a breakdown",
                        Effects = new Dictionary<string, float> { 
                            { "trust", -15 }, { "media_influence", -30 }, { "scandal_severity", 0.8f } 
                        },
                        Consequences = "They have receipts. And they're giving them to 60 Minutes."
                    },
                    new() {
                        Text = "Get ahead of it - announce 'internal review'",
                        Effects = new Dictionary<string, float> { 
                            { "trust", -8 }, { "political_capital", -20 } 
                        },
                        Consequences = "Damage control mode. Prepare for a long week."
                    },
                    new() {
                        Text = "Have your own spiritual moment - join them in confession",
                        Effects = new Dictionary<string, float> { 
                            { "trust", -25 }, { "alignment_good", 10 }, { "base_support", -30 } 
                        },
                        Consequences = "Bold. Very bold. Either career-ending or transformative.",
                        SuccessChance = 0.3f
                    },
                    new() {
                        Text = "Leak that they're under investigation for unrelated crime",
                        Effects = new Dictionary<string, float> { 
                            { "alignment_evil", 10 }, { "scandal_severity", 0.5f }, { "karma", -50 } 
                        },
                        Consequences = "Attack their credibility. It's dirty, but it might work."
                    }
                }
            };
        }
        
        #endregion
        
        #region Tell-All & Exposure Events
        
        private static StaffEventData GenerateTellAllBookEvent(StaffRoster roster)
        {
            var candidate = roster.ActiveStaff
                .Where(s => s.Personality.HasValue(StaffValues.FameSeeker) || 
                            s.Personality.Traits.Contains(PersonalityTrait.Ambitious))
                .Where(s => s.TurnsEmployed > 10)
                .OrderByDescending(s => s.TurnsEmployed)
                .FirstOrDefault();
            
            if (candidate == null) return null;
            if (UnityEngine.Random.value > 0.015f) return null;
            
            return new StaffEventData
            {
                Type = StaffEventType.TellAllAnnouncement,
                StaffInvolved = new[] { candidate },
                Title = $"📚 '{candidate.FullName}' Announces Tell-All Memoir",
                Description = $"Publishers are in a bidding war for {candidate.FullName}'s upcoming memoir, " +
                              $"tentatively titled 'Inside the Machine: My Years in Political Hell.'\n\n" +
                              $"Early excerpts leaked to Politico include:\n" +
                              $"• Detailed accounts of strategy meetings\n" +
                              $"• Your 'concerning' personal habits\n" +
                              $"• Several very unflattering anecdotes about your temper\n" +
                              $"• A chapter called 'The Night Everything Changed'\n\n" +
                              $"{candidate.FullName} is still at their desk, acting like nothing happened.",
                Severity = 4,
                Choices = new List<StaffEventChoice>
                {
                    new() {
                        Text = "Fire them immediately",
                        Effects = new Dictionary<string, float> { 
                            { "book_worse", 0.8f }, { "team_morale", -10 } 
                        },
                        Consequences = "They'll add a chapter about being fired. You'll look petty."
                    },
                    new() {
                        Text = "Offer a 'consulting fee' for favorable edits",
                        Effects = new Dictionary<string, float> { 
                            { "funds", -50000 }, { "book_severity", -0.5f } 
                        },
                        Consequences = "Expensive, but might soften the blow. Or they take the money AND write the hit piece."
                    },
                    new() {
                        Text = "Threaten legal action - NDAs exist for a reason",
                        Effects = new Dictionary<string, float> { 
                            { "legal_fees", 25000 }, { "media_attention", 0.3f } 
                        },
                        Consequences = "Lawyers cost money. And 'Campaign Sues Employee' is its own headline."
                    },
                    new() {
                        Text = "Commission your OWN tell-all about THEM",
                        Effects = new Dictionary<string, float> { 
                            { "alignment_chaos", 5 }, { "funds", -30000 }, { "entertainment_value", 100 } 
                        },
                        Consequences = "Mutually assured destruction. At least it'll be entertaining."
                    },
                    new() {
                        Text = "Shrug it off publicly - 'I wish them well'",
                        Effects = new Dictionary<string, float> { 
                            { "trust", 5 }, { "media_spin", 0.3f } 
                        },
                        Consequences = "Take the high road. Let people wonder what you're hiding."
                    }
                }
            };
        }
        
        private static StaffEventData GenerateDocumentaryRevealEvent(StaffRoster roster)
        {
            var candidate = roster.ActiveStaff
                .Where(s => s.Personality.HiddenTraits.Contains(PersonalityTrait.SecretlyRecording))
                .FirstOrDefault();
            
            if (candidate == null)
            {
                // Maybe create one retrospectively
                candidate = roster.ActiveStaff
                    .Where(s => s.TurnsEmployed > 15)
                    .OrderBy(_ => UnityEngine.Random.value)
                    .FirstOrDefault();
                    
                if (candidate == null) return null;
                if (UnityEngine.Random.value > 0.005f) return null; // Very rare
            }
            
            return new StaffEventData
            {
                Type = StaffEventType.DocumentaryReveal,
                StaffInvolved = new[] { candidate },
                Title = "🎬 Sundance Premiere: 'The Campaign'",
                Description = $"A documentary titled 'The Campaign: An Insider's View' just premiered at Sundance " +
                              $"to standing ovations. It was secretly filmed by {candidate.FullName}, who " +
                              $"was apparently wearing a hidden camera for the past {candidate.TurnsEmployed} months.\n\n" +
                              $"Reviewers are calling it 'a devastating portrait of modern politics' and " +
                              $"'the most uncomfortable two hours I've ever spent in a theater.'\n\n" +
                              $"Netflix is reportedly offering $10 million for streaming rights.\n\n" +
                              $"You appear in approximately 80% of the footage.",
                Severity = 5,
                Choices = new List<StaffEventChoice>
                {
                    new() {
                        Text = "Sue for privacy violation",
                        Effects = new Dictionary<string, float> { 
                            { "legal_fees", 100000 }, { "streisand_effect", 0.9f } 
                        },
                        Consequences = "It's probably legal in single-party consent states. And you'll look scared."
                    },
                    new() {
                        Text = "Demand a cut of the profits",
                        Effects = new Dictionary<string, float> { 
                            { "potential_income", 500000 }, { "dignity", -50 } 
                        },
                        Consequences = "If you can't beat 'em, monetize 'em?"
                    },
                    new() {
                        Text = "Claim you knew all along - it was performance art",
                        Effects = new Dictionary<string, float> { 
                            { "confusion", 100 }, { "avant_garde_credibility", 50 } 
                        },
                        Consequences = "Nobody will believe you. But nobody will know what to think either."
                    },
                    new() {
                        Text = "Watch it yourself first to assess the damage",
                        Effects = new Dictionary<string, float> { 
                            { "mental_health", -30 }, { "strategic_info", 50 } 
                        },
                        Consequences = "Do you really want to see yourself through their eyes?"
                    }
                }
            };
        }
        
        #endregion
        
        #region Romance & Personal Drama
        
        private static StaffEventData GenerateRomanceWithOpponentEvent(StaffRoster roster)
        {
            var candidate = roster.ActiveStaff
                .Where(s => !s.Personality.HasValue(StaffValues.FamilyFirst))
                .Where(s => s.TurnsEmployed > 5)
                .OrderBy(_ => UnityEngine.Random.value)
                .FirstOrDefault();
            
            if (candidate == null) return null;
            if (UnityEngine.Random.value > 0.02f) return null;
            
            string opponentStafferName = GenerateRandomName();
            
            return new StaffEventData
            {
                Type = StaffEventType.OfficeRomance,
                StaffInvolved = new[] { candidate },
                Title = $"❤️ Sleeping With The Enemy",
                Description = $"TMZ has photos of {candidate.FullName} leaving the apartment of {opponentStafferName}, " +
                              $"who happens to be your opponent's Deputy Campaign Manager.\n\n" +
                              $"Sources say they've been seeing each other for 'several weeks' and that " +
                              $"it's 'quite serious.'\n\n" +
                              $"When confronted, {candidate.FullName} said: 'Love doesn't care about " +
                              $"political parties. We're just two people who found each other.'\n\n" +
                              $"Your opponent's campaign has declined to comment, but they're definitely laughing.",
                Severity = 3,
                Choices = new List<StaffEventChoice>
                {
                    new() {
                        Text = "Fire them for security risk",
                        Effects = new Dictionary<string, float> { 
                            { "team_morale", -10 }, { "security", 30 }, { "cold_hearted", 20 } 
                        },
                        Consequences = "Necessary? Probably. Romantic? Definitely not."
                    },
                    new() {
                        Text = "Quarantine them from sensitive information",
                        Effects = new Dictionary<string, float> { 
                            { "staff_effectiveness", -0.3f }, { "awkwardness", 100 } 
                        },
                        Consequences = "They can stay, but they're on the 'need to know nothing' list."
                    },
                    new() {
                        Text = "Use them as a double agent",
                        Effects = new Dictionary<string, float> { 
                            { "alignment_chaos", 5 }, { "intel_potential", 50 }, { "karma", -30 } 
                        },
                        Consequences = "Ask them to 'casually mention' a few things at dinner..."
                    },
                    new() {
                        Text = "Embrace it - 'Love Crosses Party Lines' photo op",
                        Effects = new Dictionary<string, float> { 
                            { "media_coverage", 30 }, { "moderate_appeal", 15 }, { "base_confusion", 20 } 
                        },
                        Consequences = "Turn a scandal into a message about unity. It's crazy enough to work."
                    }
                }
            };
        }
        
        private static StaffEventData GenerateMidlifeCrisisEvent(StaffRoster roster)
        {
            var candidate = roster.ActiveStaff
                .Where(s => s.Age > 40 && s.Age < 55)
                .Where(s => s.Tier >= StaffTier.Senior)
                .OrderBy(_ => UnityEngine.Random.value)
                .FirstOrDefault();
            
            if (candidate == null) return null;
            if (UnityEngine.Random.value > 0.01f) return null;
            
            return new StaffEventData
            {
                Type = StaffEventType.SpectacularMeltdown,
                StaffInvolved = new[] { candidate },
                Title = $"🏍️ {candidate.FullName} Buys a Motorcycle",
                Description = $"Your trusted {candidate.PrimarySpecialization}, {candidate.FullName}, showed up to " +
                              $"work today on a brand new Harley-Davidson, wearing leather, with a new tattoo " +
                              $"and an earring.\n\n" +
                              $"They've also announced they're:\n" +
                              $"• Getting divorced\n" +
                              $"• 'Finally living their truth'\n" +
                              $"• Starting a podcast about 'authentic leadership'\n" +
                              $"• Possibly going to Burning Man\n\n" +
                              $"They seem really happy. Their work has become... erratic.",
                Severity = 2,
                Choices = new List<StaffEventChoice>
                {
                    new() {
                        Text = "Support their journey (while monitoring their work)",
                        Effects = new Dictionary<string, float> { 
                            { "staff_loyalty", 20 }, { "unpredictability", 30 } 
                        },
                        Consequences = "Everyone deserves to find themselves. Even mid-campaign."
                    },
                    new() {
                        Text = "Suggest they take some time off",
                        Effects = new Dictionary<string, float> { 
                            { "staff_temporary_loss", 1 }, { "stability", 20 } 
                        },
                        Consequences = "A sabbatical might be best for everyone."
                    },
                    new() {
                        Text = "Put them in charge of 'youth outreach'",
                        Effects = new Dictionary<string, float> { 
                            { "youth_confusion", 50 }, { "entertainment", 30 } 
                        },
                        Consequences = "Sure, let the 52-year-old in leather talk to the college kids."
                    },
                    new() {
                        Text = "Ask to ride the motorcycle",
                        Effects = new Dictionary<string, float> { 
                            { "staff_bonding", 40 }, { "candidate_image", -10 } 
                        },
                        Consequences = "Could be a fun photo op. Or a lawsuit waiting to happen."
                    }
                }
            };
        }
        
        #endregion
        
        #region Social Media & Public Events
        
        private static StaffEventData GenerateDrunkInterviewEvent(StaffRoster roster)
        {
            var candidate = roster.ActiveStaff
                .Where(s => s.Stress > 60)
                .OrderBy(_ => UnityEngine.Random.value)
                .FirstOrDefault();
            
            if (candidate == null) return null;
            if (UnityEngine.Random.value > 0.02f) return null;
            
            return new StaffEventData
            {
                Type = StaffEventType.DrunkInterview,
                StaffInvolved = new[] { candidate },
                Title = $"🍷 {candidate.FullName} Goes On Cable News... Drunk",
                Description = $"{candidate.FullName} appeared on cable news last night to discuss campaign strategy. " +
                              $"They were... not sober.\n\n" +
                              $"Highlights include:\n" +
                              $"• Calling the anchor 'sweetheart' three times\n" +
                              $"• Saying 'between you and me and the American people'\n" +
                              $"• Accidentally revealing next week's strategy\n" +
                              $"• Telling a story about you that was 'supposed to stay in Vegas'\n" +
                              $"• Falling asleep briefly during a commercial break\n\n" +
                              $"The clip has 2 million views and counting.",
                Severity = 4,
                Choices = new List<StaffEventChoice>
                {
                    new() {
                        Text = "Claim they had a 'medical reaction'",
                        Effects = new Dictionary<string, float> { 
                            { "credibility", -20 }, { "sympathy", 10 } 
                        },
                        Consequences = "Nobody believes it, but it gives everyone an out."
                    },
                    new() {
                        Text = "Lean in - 'Relatable campaign staff'",
                        Effects = new Dictionary<string, float> { 
                            { "working_class_appeal", 20 }, { "professional_image", -30 } 
                        },
                        Consequences = "Some voters appreciate authenticity. Others don't."
                    },
                    new() {
                        Text = "Mandatory media training for all staff",
                        Effects = new Dictionary<string, float> { 
                            { "funds", -5000 }, { "staff_annoyance", 20 }, { "future_incidents", -0.5f } 
                        },
                        Consequences = "Close the barn door. The horse is on Twitter."
                    },
                    new() {
                        Text = "Send them to 'campaign rehab' (paid leave)",
                        Effects = new Dictionary<string, float> { 
                            { "funds", -3000 }, { "staff_loyalty", 30 }, { "appearance", 20 } 
                        },
                        Consequences = "Show you care about your people. Even the messy ones."
                    }
                }
            };
        }
        
        private static StaffEventData GenerateViralTweetEvent(StaffRoster roster)
        {
            var candidate = roster.ActiveStaff
                .OrderBy(_ => UnityEngine.Random.value)
                .FirstOrDefault();
            
            if (candidate == null) return null;
            if (UnityEngine.Random.value > 0.03f) return null;
            
            string[] tweetTypes = {
                "liked a controversial political meme from 2014",
                "accidentally posted from the campaign account instead of personal",
                "got into a 47-tweet argument with a random egg account at 3am",
                "shared a hot take about a beloved celebrity",
                "made a joke that aged poorly within hours due to breaking news"
            };
            
            string tweetType = tweetTypes[UnityEngine.Random.Range(0, tweetTypes.Length)];
            
            return new StaffEventData
            {
                Type = StaffEventType.SocialMediaDisaster,
                StaffInvolved = new[] { candidate },
                Title = "📱 Staff Tweet Goes Viral (Not the Good Kind)",
                Description = $"{candidate.FullName} {tweetType}.\n\n" +
                              $"It's currently trending under #CampaignInCrisis.\n\n" +
                              $"Your opponent has already quote-tweeted it with three crying-laughing emojis.\n\n" +
                              $"Cable news is asking for comment.",
                Severity = 3,
                Choices = new List<StaffEventChoice>
                {
                    new() {
                        Text = "Delete and pretend it never happened",
                        Effects = new Dictionary<string, float> { 
                            { "screenshots", 100 }, { "streisand_effect", 0.5f } 
                        },
                        Consequences = "The internet never forgets. Someone already has screenshots."
                    },
                    new() {
                        Text = "Issue a formal apology",
                        Effects = new Dictionary<string, float> { 
                            { "authenticity", -10 }, { "story_length", -0.3f } 
                        },
                        Consequences = "Standard crisis PR. Won't make news, won't go away either."
                    },
                    new() {
                        Text = "Double down with a worse tweet",
                        Effects = new Dictionary<string, float> { 
                            { "chaos", 50 }, { "base_entertainment", 20 }, { "professionalism", -40 } 
                        },
                        Consequences = "Fight fire with fire. Or more fire. Definitely more fire."
                    },
                    new() {
                        Text = "Go dark on social media for 48 hours",
                        Effects = new Dictionary<string, float> { 
                            { "attention", -30 }, { "social_momentum", -20 } 
                        },
                        Consequences = "Sometimes the best move is not to play."
                    }
                }
            };
        }
        
        #endregion
        
        #region Loyalty & Betrayal Events
        
        private static StaffEventData GenerateSecretlyAPlantEvent(StaffRoster roster)
        {
            var candidate = roster.ActiveStaff
                .Where(s => s.IsPlant)
                .OrderBy(_ => UnityEngine.Random.value)
                .FirstOrDefault();
            
            if (candidate == null) return null;
            if (UnityEngine.Random.value > 0.1f) return null; // Only if we have a plant
            
            return new StaffEventData
            {
                Type = StaffEventType.WhistleblowerPress,
                StaffInvolved = new[] { candidate },
                Title = "🕵️ The Mole Is Revealed",
                Description = $"Your security consultant just handed you a report.\n\n" +
                              $"{candidate.FullName}, your trusted {candidate.PrimarySpecialization}, " +
                              $"has been passing information to your opponent since day one.\n\n" +
                              $"They know:\n" +
                              $"• Your polling numbers\n" +
                              $"• Your advertising strategy\n" +
                              $"• The contents of your private meetings\n" +
                              $"• That thing you said about your opponent's hairpiece\n\n" +
                              $"They're currently at their desk, unaware you know.",
                Severity = 5,
                Choices = new List<StaffEventChoice>
                {
                    new() {
                        Text = "Fire them dramatically in front of everyone",
                        Effects = new Dictionary<string, float> { 
                            { "staff_fear", 40 }, { "loyalty_boost", 20 }, { "drama", 100 } 
                        },
                        Consequences = "Send a message. Make a scene. TV movie material."
                    },
                    new() {
                        Text = "Feed them false information",
                        Effects = new Dictionary<string, float> { 
                            { "strategic_advantage", 50 }, { "alignment_cunning", 5 } 
                        },
                        Consequences = "Use the mole against them. Sun Tzu would approve."
                    },
                    new() {
                        Text = "Quietly let them go with a generous severance",
                        Effects = new Dictionary<string, float> { 
                            { "funds", -20000 }, { "quiet_resolution", 50 } 
                        },
                        Consequences = "No drama. No story. They get paid to go away quietly."
                    },
                    new() {
                        Text = "Have a private confrontation",
                        Effects = new Dictionary<string, float> { 
                            { "closure", 50 }, { "time", -1 } 
                        },
                        Consequences = "You deserve to look them in the eye. Ask them why."
                    }
                }
            };
        }
        
        private static StaffEventData GenerateWhistleblowerEvent(StaffRoster roster)
        {
            var candidate = roster.ActiveStaff
                .Where(s => s.Personality.HasValue(StaffValues.Principled) || 
                            s.Personality.HasValue(StaffValues.Idealist))
                .Where(s => s.Personality.CurrentViolationStress > 50)
                .OrderByDescending(s => s.Personality.CurrentViolationStress)
                .FirstOrDefault();
            
            if (candidate == null) return null;
            if (UnityEngine.Random.value > 0.05f) return null;
            
            return new StaffEventData
            {
                Type = StaffEventType.WhistleblowerPress,
                StaffInvolved = new[] { candidate },
                Title = "📣 Conscience Over Career",
                Description = $"{candidate.FullName} has contacted the press.\n\n" +
                              $"They've provided documentation of campaign activities they consider 'unethical,' " +
                              $"including internal communications and financial records.\n\n" +
                              $"In their statement: 'I joined this campaign believing we would be different. " +
                              $"I was wrong. The American people deserve to know the truth about how " +
                              $"political campaigns really operate.'\n\n" +
                              $"Their lawyer has already contacted the FEC.",
                Severity = 5,
                Choices = new List<StaffEventChoice>
                {
                    new() {
                        Text = "Discredit them - focus on their mental state",
                        Effects = new Dictionary<string, float> { 
                            { "alignment_evil", 10 }, { "effectiveness", 0.3f }, { "karma", -50 } 
                        },
                        Consequences = "Attack the messenger. Classic but risky."
                    },
                    new() {
                        Text = "Full transparency - release everything yourself",
                        Effects = new Dictionary<string, float> { 
                            { "trust_short_term", -20 }, { "trust_long_term", 20 }, { "alignment_good", 10 } 
                        },
                        Consequences = "Get ahead of it. Control the narrative. Take the hit."
                    },
                    new() {
                        Text = "Settle quietly - make it go away",
                        Effects = new Dictionary<string, float> { 
                            { "funds", -100000 }, { "nda_protection", 0.7f } 
                        },
                        Consequences = "NDAs and payments. The traditional solution."
                    }
                }
            };
        }
        
        private static StaffEventData GenerateUnexpectedHeroEvent(StaffRoster roster)
        {
            var candidate = roster.ActiveStaff
                .Where(s => s.Loyalty > 70)
                .OrderByDescending(s => s.Loyalty)
                .FirstOrDefault();
            
            if (candidate == null) return null;
            if (UnityEngine.Random.value > 0.02f) return null;
            
            return new StaffEventData
            {
                Type = StaffEventType.UnexpectedLoyalty,
                StaffInvolved = new[] { candidate },
                Title = "🦸 An Unlikely Defender",
                Description = $"When a reporter ambushed you with tough questions about a developing scandal, " +
                              $"{candidate.FullName} stepped in front of the cameras.\n\n" +
                              $"In an impassioned 10-minute defense, they:\n" +
                              $"• Vouched for your character personally\n" +
                              $"• Provided context the reporter lacked\n" +
                              $"• Got slightly emotional in a compelling way\n" +
                              $"• Ended with 'I believe in this campaign, and so should you'\n\n" +
                              $"The clip is going viral for all the right reasons.",
                Severity = 1, // Low severity because it's positive!
                Choices = new List<StaffEventChoice>
                {
                    new() {
                        Text = "Publicly thank them and give a bonus",
                        Effects = new Dictionary<string, float> { 
                            { "staff_loyalty_all", 10 }, { "funds", -10000 }, { "authenticity", 20 } 
                        },
                        Consequences = "Show that loyalty is rewarded. Good for morale."
                    },
                    new() {
                        Text = "Make them your new spokesperson",
                        Effects = new Dictionary<string, float> { 
                            { "media_effectiveness", 30 }, { "other_staff_jealousy", 20 } 
                        },
                        Consequences = "They clearly have a gift. Use it."
                    },
                    new() {
                        Text = "Quietly appreciate it - don't make a big deal",
                        Effects = new Dictionary<string, float> { 
                            { "humility", 20 }, { "missed_opportunity", 20 } 
                        },
                        Consequences = "Sometimes less is more. Or sometimes you miss the moment."
                    }
                }
            };
        }
        
        #endregion
        
        #region Feud & Drama Events
        
        private static StaffEventData GenerateStaffFeudEvent(StaffRoster roster)
        {
            if (roster.ActiveStaff.Count < 2) return null;
            if (UnityEngine.Random.value > 0.03f) return null;
            
            var staff1 = roster.ActiveStaff[UnityEngine.Random.Range(0, roster.ActiveStaff.Count)];
            StaffMember staff2;
            do {
                staff2 = roster.ActiveStaff[UnityEngine.Random.Range(0, roster.ActiveStaff.Count)];
            } while (staff2.Id == staff1.Id);
            
            return new StaffEventData
            {
                Type = StaffEventType.RivalryEscalates,
                StaffInvolved = new[] { staff1, staff2 },
                Title = "⚔️ Staff Civil War",
                Description = $"The tension between {staff1.FullName} and {staff2.FullName} " +
                              $"has reached a breaking point.\n\n" +
                              $"Yesterday's incident:\n" +
                              $"• Screaming match in the break room\n" +
                              $"• Accusations of sabotage and credit-stealing\n" +
                              $"• One of them cc'd the ENTIRE staff on a scathing email\n" +
                              $"• The other responded with 'Reply All'\n" +
                              $"• Someone may have cried\n\n" +
                              $"Staff are taking sides. Productivity has plummeted.",
                Severity = 3,
                Choices = new List<StaffEventChoice>
                {
                    new() {
                        Text = "Fire them both - zero tolerance",
                        Effects = new Dictionary<string, float> { 
                            { "staff_count", -2 }, { "discipline", 50 }, { "fear", 30 } 
                        },
                        Consequences = "Decisive. Maybe too decisive. You'll be short-staffed."
                    },
                    new() {
                        Text = "Mandatory mediation session",
                        Effects = new Dictionary<string, float> { 
                            { "time", -2 }, { "resolution_chance", 0.6f }, { "awkwardness", 100 } 
                        },
                        Consequences = "Make them work it out like adults. Or make it worse."
                    },
                    new() {
                        Text = "Separate them - different shifts/locations",
                        Effects = new Dictionary<string, float> { 
                            { "logistics_nightmare", 50 }, { "cold_peace", 70 } 
                        },
                        Consequences = "If they can't see each other, they can't fight. Probably."
                    },
                    new() {
                        Text = "Pick a side publicly",
                        Effects = new Dictionary<string, float> { 
                            { "favoritism", 50 }, { "loser_betrayal_risk", 0.7f } 
                        },
                        Consequences = "One will love you. One will plot revenge."
                    },
                    new() {
                        Text = "Ignore it - they'll work it out",
                        Effects = new Dictionary<string, float> { 
                            { "productivity", -30 }, { "drama_continuation", 0.8f } 
                        },
                        Consequences = "Wishful thinking. This never resolves itself."
                    }
                }
            };
        }
        
        private static StaffEventData GenerateDramaticResignationEvent(StaffRoster roster)
        {
            var candidate = roster.ActiveStaff
                .Where(s => s.Morale <= StaffMorale.Unhappy)
                .Where(s => s.Personality.Traits.Contains(PersonalityTrait.Volatile) ||
                            s.Personality.Traits.Contains(PersonalityTrait.Blunt))
                .OrderBy(_ => UnityEngine.Random.value)
                .FirstOrDefault();
            
            if (candidate == null) return null;
            if (UnityEngine.Random.value > 0.05f) return null;
            
            return new StaffEventData
            {
                Type = StaffEventType.BridgeBurning,
                StaffInvolved = new[] { candidate },
                Title = "🔥 The Resignation Letter",
                Description = $"{candidate.FullName} has resigned.\n\n" +
                              $"Not quietly. They sent a 3,000-word email to the entire staff, " +
                              $"every major political journalist in their contacts, and for some reason, " +
                              $"their mother.\n\n" +
                              $"Key quotes:\n" +
                              $"• 'I refuse to compromise my integrity for this circus'\n" +
                              $"• 'The candidate has no idea what they're doing'\n" +
                              $"• 'The [specific policy you pushed] is morally indefensible'\n" +
                              $"• 'I look forward to voting for the other candidate'\n\n" +
                              $"They also took the office coffee maker.",
                Severity = 4,
                Choices = new List<StaffEventChoice>
                {
                    new() {
                        Text = "Respond with grace and professionalism",
                        Effects = new Dictionary<string, float> { 
                            { "moral_high_ground", 30 }, { "story_dies", 0.4f } 
                        },
                        Consequences = "Kill them with kindness. Contrast your response with theirs."
                    },
                    new() {
                        Text = "Point-by-point rebuttal email",
                        Effects = new Dictionary<string, float> { 
                            { "story_extends", 0.8f }, { "pettiness", 50 } 
                        },
                        Consequences = "Continue the drama. Nobody wins."
                    },
                    new() {
                        Text = "Find dirt on them and leak it",
                        Effects = new Dictionary<string, float> { 
                            { "alignment_evil", 10 }, { "revenge_satisfaction", 30 }, { "karma", -40 } 
                        },
                        Consequences = "If you can't win the argument, change the subject."
                    },
                    new() {
                        Text = "Call them directly - one professional to another",
                        Effects = new Dictionary<string, float> { 
                            { "reconciliation_chance", 0.3f }, { "closure", 40 } 
                        },
                        Consequences = "Maybe there's still a bridge. Maybe not."
                    }
                }
            };
        }
        
        #endregion
        
        #region Misc Events
        
        private static StaffEventData GenerateFoundingMomentEvent(StaffRoster roster)
        {
            if (roster.ActiveStaff.Count < 4) return null;
            if (UnityEngine.Random.value > 0.02f) return null;
            
            return new StaffEventData
            {
                Type = StaffEventType.HeroicSacrifice, // Positive version
                StaffInvolved = roster.ActiveStaff.ToArray(),
                Title = "⭐ The Night Everything Clicked",
                Description = $"After a grueling 18-hour day, something magical happened.\n\n" +
                              $"The whole team stayed late. Someone ordered pizza. " +
                              $"Someone else brought whiskey. Stories were shared. " +
                              $"Strategies were debated passionately but respectfully.\n\n" +
                              $"By 3am, you had:\n" +
                              $"• A new campaign slogan everyone believes in\n" +
                              $"• Three new policy ideas that actually make sense\n" +
                              $"• A team that would walk through fire for each other\n" +
                              $"• Several embarrassing karaoke videos\n\n" +
                              $"This is the night everyone will remember.",
                Severity = 1, // Low severity = positive
                Choices = new List<StaffEventChoice>
                {
                    new() {
                        Text = "Give everyone the next day off (with pay)",
                        Effects = new Dictionary<string, float> { 
                            { "staff_morale_all", 30 }, { "funds", -5000 }, { "team_cohesion", 50 } 
                        },
                        Consequences = "They've earned it. And memories like this build campaigns."
                    },
                    new() {
                        Text = "Frame this moment - team photo for the office",
                        Effects = new Dictionary<string, float> { 
                            { "nostalgia_bank", 100 }, { "shared_history", 50 } 
                        },
                        Consequences = "Something to look at when times get tough."
                    },
                    new() {
                        Text = "Channel this energy - let's get to work",
                        Effects = new Dictionary<string, float> { 
                            { "momentum", 40 }, { "productivity_spike", 0.3f }, { "burnout_risk", 0.2f } 
                        },
                        Consequences = "Strike while the iron is hot. Sleep is for the weak."
                    }
                }
            };
        }
        
        private static StaffEventData GenerateFamilyEmergencyEvent(StaffRoster roster)
        {
            var candidate = roster.ActiveStaff
                .OrderBy(_ => UnityEngine.Random.value)
                .FirstOrDefault();
            
            if (candidate == null) return null;
            if (UnityEngine.Random.value > 0.03f) return null;
            
            return new StaffEventData
            {
                Type = StaffEventType.FamilyEmergency,
                StaffInvolved = new[] { candidate },
                Title = $"🏥 {candidate.FullName} - Family Emergency",
                Description = $"{candidate.FullName} has just received terrible news.\n\n" +
                              $"A family member is seriously ill. They need to leave immediately " +
                              $"and may be gone for... an unknown amount of time.\n\n" +
                              $"This comes at the worst possible moment - they're in the middle of " +
                              $"three critical projects and the [upcoming event] is in five days.\n\n" +
                              $"They're standing in your office, barely holding it together.",
                Severity = 2,
                Choices = new List<StaffEventChoice>
                {
                    new() {
                        Text = "Go. Family first. We'll figure it out.",
                        Effects = new Dictionary<string, float> { 
                            { "staff_loyalty_all", 20 }, { "humanity", 50 }, { "workload_increase", 0.3f } 
                        },
                        Consequences = "It's the right thing to do. Everyone's watching how you handle this."
                    },
                    new() {
                        Text = "Can you give us 48 hours to transition your work?",
                        Effects = new Dictionary<string, float> { 
                            { "coldness", 30 }, { "efficiency", 20 }, { "staff_opinion", -10 } 
                        },
                        Consequences = "Practical. But remember this moment when you need loyalty."
                    },
                    new() {
                        Text = "We need you here. Can you work remotely?",
                        Effects = new Dictionary<string, float> { 
                            { "coldness", 50 }, { "resentment", 70 }, { "staff_opinion_all", -15 } 
                        },
                        Consequences = "Everyone heard that. Everyone will remember."
                    },
                    new() {
                        Text = "Go, and I'm coming with you to help.",
                        Effects = new Dictionary<string, float> { 
                            { "staff_loyalty_this_person", 100 }, { "time_away", 2 }, { "legend", 50 } 
                        },
                        Consequences = "Extraordinary. They'll never forget this. Neither will the campaign."
                    }
                }
            };
        }
        
        private static StaffEventData GeneratePodcastAppearanceEvent(StaffRoster roster)
        {
            var candidate = roster.ActiveStaff
                .Where(s => s.Personality.HasValue(StaffValues.Extrovert) || 
                            s.Personality.HasValue(StaffValues.FameSeeker))
                .OrderBy(_ => UnityEngine.Random.value)
                .FirstOrDefault();
            
            if (candidate == null) return null;
            if (UnityEngine.Random.value > 0.03f) return null;
            
            return new StaffEventData
            {
                Type = StaffEventType.SocialMediaDisaster, // Can go either way
                StaffInvolved = new[] { candidate },
                Title = "🎙️ Staff Podcast Appearance",
                Description = $"{candidate.FullName} appeared on a popular political podcast without clearing it.\n\n" +
                              $"It was a 90-minute freewheeling conversation covering:\n" +
                              $"• Campaign behind-the-scenes (some you wish they hadn't shared)\n" +
                              $"• Their personal political views (some differ from yours)\n" +
                              $"• Hot takes on current events (very hot)\n" +
                              $"• An anecdote about you that's being clipped everywhere\n\n" +
                              $"It's currently #3 on Apple Podcasts.",
                Severity = 3,
                Choices = new List<StaffEventChoice>
                {
                    new() {
                        Text = "Damage control - create talking points to walk things back",
                        Effects = new Dictionary<string, float> { 
                            { "time", -1 }, { "cleanup_effectiveness", 0.5f } 
                        },
                        Consequences = "Clarify, contextualize, contain."
                    },
                    new() {
                        Text = "Embrace it - book them on more podcasts",
                        Effects = new Dictionary<string, float> { 
                            { "media_presence", 30 }, { "message_discipline", -30 } 
                        },
                        Consequences = "If you can't control it, own it."
                    },
                    new() {
                        Text = "Establish strict media approval policy",
                        Effects = new Dictionary<string, float> { 
                            { "staff_frustration", 30 }, { "future_freelancing", -0.7f } 
                        },
                        Consequences = "Lock it down. No more surprises."
                    },
                    new() {
                        Text = "Start your OWN podcast",
                        Effects = new Dictionary<string, float> { 
                            { "time", -5 }, { "direct_audience", 50 }, { "sanity", -20 } 
                        },
                        Consequences = "Control the narrative completely. At great personal cost."
                    }
                }
            };
        }
        
        #endregion
        
        #region Helpers
        
        private static string GenerateRandomName()
        {
            string[] firstNames = { "Alex", "Jordan", "Taylor", "Casey", "Morgan", "Riley", "Quinn", "Avery" };
            string[] lastNames = { "Smith", "Johnson", "Williams", "Brown", "Davis", "Miller", "Wilson", "Moore" };
            
            return $"{firstNames[UnityEngine.Random.Range(0, firstNames.Length)]} " +
                   $"{lastNames[UnityEngine.Random.Range(0, lastNames.Length)]}";
        }
        
        #endregion
    }
}
