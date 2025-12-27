using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.World;
using ElectionEmpire.Gameplay;

namespace ElectionEmpire.Gameplay
{
    /// <summary>
    /// Manages elections from announcement to inauguration
    /// </summary>
    public class ElectionManager
    {
        private Office targetOffice;
        private List<PlayerState> candidates;
        private VoterSimulation voterSim;
        private ElectionEmpire.World.World world;
        
        // Election phases
        public enum ElectionPhase
        {
            Announcement,    // Declare candidacy
            Primary,         // Party selection (if applicable)
            Campaign,        // Main campaign period
            Debate,          // Debates between candidates
            ElectionDay,     // Voting
            Results,         // Vote counting
            Transition       // Winner takes office
        }
        
        public ElectionPhase CurrentPhase;
        public int DaysInPhase;
        private Dictionary<ElectionPhase, int> phaseDurations;
        public int DaysUntilNextPhase => phaseDurations[CurrentPhase] - DaysInPhase;
        public bool IsElectionActive { get; private set; }
        
        private Dictionary<PlayerState, int> electionResults;
        private PlayerState electionWinner;
        
        /// <summary>
        /// Default constructor for deferred initialization
        /// </summary>
        public ElectionManager()
        {
            phaseDurations = new Dictionary<ElectionPhase, int>();
            candidates = new List<PlayerState>();
            electionResults = new Dictionary<PlayerState, int>();
        }

        public ElectionManager(ElectionEmpire.World.World world, VoterSimulation voterSim) : this()
        {
            this.world = world;
            this.voterSim = voterSim;
        }

        /// <summary>
        /// Initialize the manager with world and voter simulation (for deferred initialization)
        /// </summary>
        public void Initialize(ElectionEmpire.World.World world, VoterSimulation voterSim)
        {
            this.world = world;
            this.voterSim = voterSim;
        }
        
        public void StartElection(Office office, List<PlayerState> allCandidates)
        {
            targetOffice = office;
            candidates = new List<PlayerState>(allCandidates);
            CurrentPhase = ElectionPhase.Announcement;
            DaysInPhase = 0;
            IsElectionActive = true;
            
            // Calculate phase durations based on office tier
            CalculatePhaseDurations();
            
            // Announce election
            AnnounceElection();
        }
        
        private void CalculatePhaseDurations()
        {
            // Higher tier = longer campaigns
            int baseDays = 30;
            int tierMultiplier = targetOffice != null ? targetOffice.Tier : 1;
            
            phaseDurations[ElectionPhase.Announcement] = 7;
            phaseDurations[ElectionPhase.Primary] = baseDays * tierMultiplier / 2;
            phaseDurations[ElectionPhase.Campaign] = baseDays * tierMultiplier;
            phaseDurations[ElectionPhase.Debate] = 14;
            phaseDurations[ElectionPhase.ElectionDay] = 1;
            phaseDurations[ElectionPhase.Results] = 3;
            phaseDurations[ElectionPhase.Transition] = 30;
        }
        
        public void UpdateElection(float deltaTime)
        {
            if (!IsElectionActive) return;
            
            DaysInPhase += (int)(deltaTime / 86400f);
            
            if (DaysInPhase >= phaseDurations[CurrentPhase])
            {
                AdvancePhase();
            }
            
            // Phase-specific updates
            switch (CurrentPhase)
            {
                case ElectionPhase.Campaign:
                    UpdateCampaignPhase();
                    break;
                
                case ElectionPhase.Debate:
                    if (DaysInPhase == 1) // Trigger once at start
                        TriggerDebateEvent();
                    break;
                
                case ElectionPhase.ElectionDay:
                    if (DaysInPhase == 1) // Calculate once
                        CalculateResults();
                    break;
            }
        }
        
        private void AdvancePhase()
        {
            DaysInPhase = 0;
            
            switch (CurrentPhase)
            {
                case ElectionPhase.Announcement:
                    CurrentPhase = ElectionPhase.Primary;
                    break;
                
                case ElectionPhase.Primary:
                    // Filter candidates by party
                    ConductPrimary();
                    CurrentPhase = ElectionPhase.Campaign;
                    break;
                
                case ElectionPhase.Campaign:
                    CurrentPhase = ElectionPhase.Debate;
                    break;
                
                case ElectionPhase.Debate:
                    CurrentPhase = ElectionPhase.ElectionDay;
                    break;
                
                case ElectionPhase.ElectionDay:
                    CurrentPhase = ElectionPhase.Results;
                    break;
                
                case ElectionPhase.Results:
                    DeclareWinner();
                    CurrentPhase = ElectionPhase.Transition;
                    break;
                
                case ElectionPhase.Transition:
                    InaugurateWinner();
                    EndElection();
                    break;
            }
        }
        
        private void AnnounceElection()
        {
            Debug.Log($"Election announced for: {targetOffice.Name}");
            // UI would show announcement
        }
        
        private void UpdateCampaignPhase()
        {
            // Update polling daily
            foreach (var candidate in candidates)
            {
                candidate.CurrentPolling = voterSim.CalculateNationalApproval(candidate);
            }
            
            // Random campaign events
            if (UnityEngine.Random.value < 0.1f) // 10% chance per day
            {
                TriggerCampaignEvent();
            }
        }
        
        private void ConductPrimary()
        {
            // Group candidates by party
            var parties = candidates.GroupBy(c => c.Party ?? "Independent");
            
            var primaryWinners = new List<PlayerState>();
            
            foreach (var party in parties)
            {
                if (party.Key == "Independent")
                {
                    // Independents all advance
                    primaryWinners.AddRange(party);
                }
                else
                {
                    // Find highest polling in each party
                    var winner = party.OrderByDescending(c => c.CurrentPolling).First();
                    primaryWinners.Add(winner);
                }
            }
            
            // Update candidate list to primary winners
            candidates = primaryWinners;
            
            Debug.Log($"Primary complete. {candidates.Count} candidates advance.");
        }
        
        private void TriggerDebateEvent()
        {
            // Create debate event
            var debate = new DebateEvent
            {
                Candidates = candidates,
                Questions = GenerateDebateQuestions(3),
                Format = UnityEngine.Random.value > 0.5f ? DebateFormat.FreeForAll : DebateFormat.OneOnOne
            };
            
            // Trigger debate (would be handled by EventManager)
            Debug.Log($"Debate triggered: {debate.Format} format with {debate.Questions.Count} questions");
        }
        
        private void TriggerCampaignEvent()
        {
            // Random campaign events (rallies, scandals, endorsements, etc.)
            Debug.Log("Campaign event triggered!");
        }
        
        private void CalculateResults()
        {
            // Calculate vote totals for each candidate
            electionResults = new Dictionary<PlayerState, int>();
            
            foreach (var candidate in candidates)
            {
                int totalVotes = 0;
                
                // Calculate district by district
                foreach (var region in world.Nation.Regions)
                {
                    foreach (var state in region.States)
                    {
                        foreach (var district in state.Districts)
                        {
                            float polling = voterSim.CalculateDistrictPolling(candidate, district);
                            
                            // Add some volatility (election day uncertainty)
                            polling += UnityEngine.Random.Range(-5f, 5f);
                            polling = Mathf.Clamp(polling, 0f, 100f);
                            
                            // Determine if this candidate wins the district
                            if (IsHighestInDistrict(candidate, district, candidates))
                            {
                                totalVotes += district.Population;
                            }
                        }
                    }
                }
                
                electionResults[candidate] = totalVotes;
            }
        }
        
        private bool IsHighestInDistrict(PlayerState candidate, District district, List<PlayerState> allCandidates)
        {
            float candidatePolling = voterSim.CalculateDistrictPolling(candidate, district);
            
            foreach (var other in allCandidates)
            {
                if (other == candidate) continue;
                
                float otherPolling = voterSim.CalculateDistrictPolling(other, district);
                if (otherPolling > candidatePolling)
                    return false;
            }
            
            return true;
        }
        
        private void DeclareWinner()
        {
            // Find candidate with most votes
            var winner = electionResults.OrderByDescending(kvp => kvp.Value).First().Key;
            
            electionWinner = winner;
            
            Debug.Log($"Election winner: {winner.Character.Name} with {electionResults[winner]:N0} votes");
            
            // Update all candidates
            foreach (var candidate in candidates)
            {
                if (candidate == winner)
                {
                    candidate.ElectionsWon++;
                    candidate.HighestTierHeld = Mathf.Max(candidate.HighestTierHeld, targetOffice.Tier);
                    candidate.ConsecutiveElectionLosses = 0;
                }
                else
                {
                    candidate.ElectionsLost++;
                    candidate.ConsecutiveElectionLosses++;
                }
            }
        }
        
        private void InaugurateWinner()
        {
            // Winner takes office
            electionWinner.CurrentOffice = targetOffice;
            electionWinner.TermStartDate = DateTime.Now;
            electionWinner.TermEndDate = DateTime.Now.AddDays(targetOffice.TermLengthDays);
            electionWinner.OfficePowers = targetOffice.Powers;
            electionWinner.IsInCampaign = false;
            
            Debug.Log($"{electionWinner.Character.Name} inaugurated as {targetOffice.Name}");
        }
        
        private void EndElection()
        {
            IsElectionActive = false;
            Debug.Log("Election complete.");
        }
        
        private List<DebateQuestion> GenerateDebateQuestions(int count)
        {
            var questions = new List<DebateQuestion>();
            
            string[] questionTemplates = new[]
            {
                "What is your plan to address {ISSUE}?",
                "Your opponent says you're weak on {ISSUE}. How do you respond?",
                "Recent polls show voters are concerned about {ISSUE}. What's your position?",
                "You've been criticized for your stance on {ISSUE}. Do you stand by it?",
                "If elected, what will you do about {ISSUE} in your first 100 days?"
            };
            
            // Select random issues from world priorities
            var topIssues = world.Nation.Regions
                .SelectMany(r => r.States)
                .SelectMany(s => s.Districts)
                .SelectMany(d => d.PriorityIssues)
                .GroupBy(i => i)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => g.Key)
                .ToList();
            
            if (topIssues.Count == 0)
            {
                // Fallback to all issues
                topIssues = System.Enum.GetValues(typeof(Issue)).Cast<Issue>().Take(5).ToList();
            }
            
            for (int i = 0; i < count && i < topIssues.Count; i++)
            {
                string template = questionTemplates[UnityEngine.Random.Range(0, questionTemplates.Length)];
                Issue issue = topIssues[i];
                
                questions.Add(new DebateQuestion
                {
                    Text = template.Replace("{ISSUE}", issue.ToString()),
                    Issue = issue
                });
            }
            
            return questions;
        }
        
        public List<PlayerState> GetCandidates()
        {
            return new List<PlayerState>(candidates);
        }
        
        public Dictionary<PlayerState, int> GetResults()
        {
            return electionResults != null ? new Dictionary<PlayerState, int>(electionResults) : null;
        }
        
        public PlayerState GetWinner()
        {
            return electionWinner;
        }
        
        public Office GetTargetOffice()
        {
            return targetOffice;
        }
    }
    
    public class DebateEvent
    {
        public List<PlayerState> Candidates;
        public List<DebateQuestion> Questions;
        public DebateFormat Format;
    }
    
    public enum DebateFormat
    {
        OneOnOne,     // Two candidates face off
        FreeForAll,   // All candidates together
        TownHall      // Candidates answer voter questions
    }
    
    public class DebateQuestion
    {
        public string Text;
        public Issue Issue;
    }
}

