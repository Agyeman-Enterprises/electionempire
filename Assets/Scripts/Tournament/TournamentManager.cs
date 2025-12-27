// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - TOURNAMENT MANAGER
// Core orchestration for tournament lifecycle management
// ═══════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.Monetization;
using ElectionEmpire.Economy;

namespace ElectionEmpire.Tournament
{
    /// <summary>
    /// Central manager for all tournament operations
    /// </summary>
    public class TournamentManager
    {
        #region Singleton
        
        private static TournamentManager _instance;
        public static TournamentManager Instance => _instance ??= new TournamentManager();
        
        #endregion
        
        #region State
        
        private TournamentSystemConfig _config;
        private Dictionary<string, Tournament> _activeTournaments;
        private Dictionary<string, TournamentHistory> _playerHistories;
        private TournamentSeason _currentSeason;
        private BracketGenerator _bracketGenerator;
        private RankingSystem _rankingSystem;
        private CurrencyManager _currencyManager;
        
        #endregion
        
        #region Events
        
        public event Action<Tournament> OnTournamentCreated;
        public event Action<TournamentStatusChangedEvent> OnTournamentStatusChanged;
        public event Action<Tournament, TournamentParticipant> OnPlayerRegistered;
        public event Action<Tournament, TournamentParticipant> OnPlayerEliminated;
        public event Action<MatchCompletedEvent> OnMatchCompleted;
        public event Action<TournamentCompletedEvent> OnTournamentCompleted;
        public event Action<Tournament, TournamentRound> OnRoundStarted;
        public event Action<Tournament, TournamentRound> OnRoundCompleted;
        
        #endregion
        
        #region Initialization
        
        public TournamentManager()
        {
            _config = new TournamentSystemConfig();
            _activeTournaments = new Dictionary<string, Tournament>();
            _playerHistories = new Dictionary<string, TournamentHistory>();
            _bracketGenerator = new BracketGenerator();
            _rankingSystem = new RankingSystem(_config);
            
            // Get currency manager from GameManagerIntegration if available
            if (Core.GameManagerIntegration.Instance != null)
            {
                _currencyManager = Core.GameManagerIntegration.Instance.CurrencyManager;
            }
        }
        
        public void Initialize(TournamentSystemConfig config = null, CurrencyManager currencyManager = null)
        {
            if (config != null)
            {
                _config = config;
                _rankingSystem = new RankingSystem(_config);
            }
            
            if (currencyManager != null)
            {
                _currencyManager = currencyManager;
            }
            else if (Core.GameManagerIntegration.Instance != null)
            {
                _currencyManager = Core.GameManagerIntegration.Instance.CurrencyManager;
            }
            
            // Initialize current season
            InitializeCurrentSeason();
        }
        
        private void InitializeCurrentSeason()
        {
            _currentSeason = new TournamentSeason
            {
                Name = $"Season {DateTime.UtcNow.Year}-{(DateTime.UtcNow.Month - 1) / 3 + 1}",
                SeasonNumber = CalculateSeasonNumber(),
                StartDate = GetSeasonStartDate(),
                EndDate = GetSeasonEndDate(),
                IsActive = true,
                QualifyingPointsThreshold = 1000
            };
        }
        
        private int CalculateSeasonNumber()
        {
            var baseDate = new DateTime(2025, 1, 1);
            var diff = DateTime.UtcNow - baseDate;
            return (int)(diff.TotalDays / _config.DaysPerSeason) + 1;
        }
        
        private DateTime GetSeasonStartDate()
        {
            var seasonLength = TimeSpan.FromDays(_config.DaysPerSeason);
            var baseDate = new DateTime(2025, 1, 1);
            var seasonNumber = CalculateSeasonNumber() - 1;
            return baseDate.AddDays(seasonNumber * _config.DaysPerSeason);
        }
        
        private DateTime GetSeasonEndDate()
        {
            return GetSeasonStartDate().AddDays(_config.DaysPerSeason);
        }
        
        #endregion
        
        #region Tournament Creation
        
        /// <summary>
        /// Create a new tournament
        /// </summary>
        public Tournament CreateTournament(
            string name,
            string hostId,
            string hostName,
            TournamentFormat format,
            TournamentTier tier,
            int maxPlayers,
            TournamentConfig config = null,
            TournamentRules rules = null)
        {
            if (_activeTournaments.Count >= _config.MaxConcurrentTournaments)
            {
                throw new InvalidOperationException("Maximum concurrent tournaments reached");
            }
            
            if (maxPlayers > _config.MaxPlayersPerTournament)
            {
                maxPlayers = _config.MaxPlayersPerTournament;
            }
            
            var tournament = new Tournament
            {
                Name = name,
                HostId = hostId,
                HostName = hostName,
                Format = format,
                Tier = tier,
                MaxPlayers = maxPlayers,
                MinPlayers = Math.Max(4, maxPlayers / 4),
                Config = config ?? new TournamentConfig(),
                Rules = rules ?? new TournamentRules(),
                Status = TournamentStatus.Registration,
                IsRanked = tier >= TournamentTier.Weekly,
                IsPublic = true
            };
            
            // Set scoring and victory based on format
            SetDefaultScoringAndVictory(tournament);
            
            // Generate default rewards
            GenerateDefaultRewards(tournament);
            
            _activeTournaments[tournament.Id] = tournament;
            
            OnTournamentCreated?.Invoke(tournament);
            
            return tournament;
        }
        
        private void SetDefaultScoringAndVictory(Tournament tournament)
        {
            switch (tournament.Format)
            {
                case TournamentFormat.SingleElimination:
                case TournamentFormat.DoubleElimination:
                    tournament.Scoring = ScoringSystem.WinLoss;
                    tournament.Victory = VictoryCondition.MostVotes;
                    break;
                    
                case TournamentFormat.RoundRobin:
                case TournamentFormat.Swiss:
                case TournamentFormat.League:
                    tournament.Scoring = ScoringSystem.Points;
                    tournament.Victory = VictoryCondition.HighestApproval;
                    break;
                    
                case TournamentFormat.BattleRoyale:
                    tournament.Scoring = ScoringSystem.VotePercentage;
                    tournament.Victory = VictoryCondition.Elimination;
                    break;
                    
                case TournamentFormat.Ladder:
                    tournament.Scoring = ScoringSystem.Composite;
                    tournament.Victory = VictoryCondition.HighestApproval;
                    break;
            }
        }
        
        private void GenerateDefaultRewards(Tournament tournament)
        {
            int baseReward = GetBaseRewardForTier(tournament.Tier);
            
            tournament.Rewards = new List<TournamentReward>
            {
                // 1st place
                new TournamentReward
                {
                    MinPlacement = 1,
                    MaxPlacement = 1,
                    Type = RewardType.Currency,
                    RewardName = "Champion's Prize",
                    Amount = baseReward * 10,
                    Description = "Tournament Champion!"
                },
                new TournamentReward
                {
                    MinPlacement = 1,
                    MaxPlacement = 1,
                    Type = RewardType.Title,
                    RewardId = $"champion_{tournament.Tier}",
                    RewardName = GetChampionTitle(tournament.Tier),
                    Description = "Exclusive champion title"
                },
                
                // 2nd place
                new TournamentReward
                {
                    MinPlacement = 2,
                    MaxPlacement = 2,
                    Type = RewardType.Currency,
                    RewardName = "Runner-Up Prize",
                    Amount = baseReward * 5,
                    Description = "Tournament Runner-Up"
                },
                
                // 3rd-4th place
                new TournamentReward
                {
                    MinPlacement = 3,
                    MaxPlacement = 4,
                    Type = RewardType.Currency,
                    RewardName = "Semi-Finalist Prize",
                    Amount = baseReward * 2,
                    Description = "Made it to the Semi-Finals"
                },
                
                // Top 8
                new TournamentReward
                {
                    MinPlacement = 5,
                    MaxPlacement = 8,
                    Type = RewardType.Currency,
                    RewardName = "Quarter-Finalist Prize",
                    Amount = baseReward,
                    Description = "Quarter-Finals appearance"
                }
            };
            
            // Add season points for ranked tournaments
            if (tournament.IsRanked)
            {
                tournament.Rewards.Add(new TournamentReward
                {
                    MinPlacement = 1,
                    MaxPlacement = 1,
                    Type = RewardType.SeasonPoints,
                    RewardName = "Season Points",
                    Amount = GetSeasonPointsForPlacement(tournament.Tier, 1),
                    Description = "Season ranking points"
                });
            }
        }
        
        private int GetBaseRewardForTier(TournamentTier tier)
        {
            return tier switch
            {
                TournamentTier.Community => 100,
                TournamentTier.Weekly => 250,
                TournamentTier.Monthly => 500,
                TournamentTier.Seasonal => 1000,
                TournamentTier.Championship => 2500,
                TournamentTier.Official => 5000,
                _ => 100
            };
        }
        
        private string GetChampionTitle(TournamentTier tier)
        {
            return tier switch
            {
                TournamentTier.Community => "Community Champion",
                TournamentTier.Weekly => "Weekly Victor",
                TournamentTier.Monthly => "Monthly Master",
                TournamentTier.Seasonal => "Seasonal Supreme",
                TournamentTier.Championship => "Grand Champion",
                TournamentTier.Official => "World Champion",
                _ => "Tournament Winner"
            };
        }
        
        private int GetSeasonPointsForPlacement(TournamentTier tier, int placement)
        {
            int tierMultiplier = (int)tier + 1;
            int placementPoints = placement switch
            {
                1 => 100,
                2 => 75,
                3 => 50,
                4 => 50,
                <= 8 => 25,
                <= 16 => 10,
                _ => 5
            };
            return placementPoints * tierMultiplier;
        }
        
        #endregion
        
        #region Registration
        
        /// <summary>
        /// Register a player for a tournament
        /// </summary>
        public bool RegisterPlayer(string tournamentId, string playerId, string playerName, RankTier rank = RankTier.Unranked, int rankPoints = 0)
        {
            if (!_activeTournaments.TryGetValue(tournamentId, out var tournament))
            {
                return false;
            }
            
            if (tournament.Status != TournamentStatus.Registration)
            {
                return false;
            }
            
            if (tournament.CurrentPlayerCount >= tournament.MaxPlayers)
            {
                return false;
            }
            
            if (tournament.Participants.Any(p => p.PlayerId == playerId))
            {
                return false; // Already registered
            }
            
            if (tournament.BannedPlayerIds.Contains(playerId))
            {
                return false;
            }
            
            // Check entry requirements
            if (!MeetsEntryRequirements(tournament, playerId, rank, rankPoints))
            {
                return false;
            }
            
            var participant = new TournamentParticipant
            {
                PlayerId = playerId,
                PlayerName = playerName,
                DisplayName = playerName,
                Rank = rank,
                RankPoints = rankPoints,
                Seed = tournament.CurrentPlayerCount + 1
            };
            
            tournament.Participants.Add(participant);
            tournament.CurrentPlayerCount++;
            
            OnPlayerRegistered?.Invoke(tournament, participant);
            
            // Check for auto-start
            if (tournament.Config.AutoStartWhenFull && tournament.CurrentPlayerCount >= tournament.MaxPlayers)
            {
                StartTournament(tournamentId);
            }
            
            return true;
        }
        
        private bool MeetsEntryRequirements(Tournament tournament, string playerId, RankTier rank, int rankPoints)
        {
            switch (tournament.Config.EntryType)
            {
                case EntryRequirement.InviteOnly:
                    return tournament.InvitedPlayerIds.Contains(playerId);
                    
                case EntryRequirement.RankRequired:
                    return rank >= tournament.Config.RequiredRankTier;
                    
                case EntryRequirement.Qualified:
                    return rankPoints >= tournament.Config.MinimumRank;
                    
                default:
                    return true;
            }
        }
        
        /// <summary>
        /// Unregister a player from a tournament
        /// </summary>
        public bool UnregisterPlayer(string tournamentId, string playerId)
        {
            if (!_activeTournaments.TryGetValue(tournamentId, out var tournament))
            {
                return false;
            }
            
            if (tournament.Status != TournamentStatus.Registration)
            {
                return false;
            }
            
            var participant = tournament.Participants.FirstOrDefault(p => p.PlayerId == playerId);
            if (participant == null)
            {
                return false;
            }
            
            tournament.Participants.Remove(participant);
            tournament.CurrentPlayerCount--;
            
            // Re-seed remaining players
            ReseedParticipants(tournament);
            
            return true;
        }
        
        /// <summary>
        /// Check in a player for the tournament
        /// </summary>
        public bool CheckInPlayer(string tournamentId, string playerId)
        {
            if (!_activeTournaments.TryGetValue(tournamentId, out var tournament))
            {
                return false;
            }
            
            var participant = tournament.Participants.FirstOrDefault(p => p.PlayerId == playerId);
            if (participant == null)
            {
                return false;
            }
            
            participant.CheckedIn = true;
            participant.CheckedInAt = DateTime.UtcNow;
            
            return true;
        }
        
        #endregion
        
        #region Tournament Lifecycle
        
        /// <summary>
        /// Start a tournament
        /// </summary>
        public bool StartTournament(string tournamentId)
        {
            if (!_activeTournaments.TryGetValue(tournamentId, out var tournament))
            {
                return false;
            }
            
            if (tournament.Status != TournamentStatus.Registration && 
                tournament.Status != TournamentStatus.Pending)
            {
                return false;
            }
            
            if (tournament.CurrentPlayerCount < tournament.MinPlayers)
            {
                return false;
            }
            
            // Remove players who didn't check in (if required)
            if (tournament.Config.RequireCheckIn)
            {
                var notCheckedIn = tournament.Participants.Where(p => !p.CheckedIn).ToList();
                foreach (var p in notCheckedIn)
                {
                    tournament.Participants.Remove(p);
                    tournament.CurrentPlayerCount--;
                }
            }
            
            // Seed players
            SeedParticipants(tournament);
            
            // Generate bracket
            tournament.Bracket = _bracketGenerator.GenerateBracket(tournament);
            
            // Calculate total rounds
            tournament.TotalRounds = CalculateTotalRounds(tournament);
            
            // Generate first round matches
            GenerateRoundMatches(tournament, 1);
            
            // Update status
            var oldStatus = tournament.Status;
            tournament.Status = TournamentStatus.InProgress;
            tournament.StartTime = DateTime.UtcNow;
            tournament.CurrentRound = 1;
            
            OnTournamentStatusChanged?.Invoke(new TournamentStatusChangedEvent
            {
                TournamentId = tournament.Id,
                OldStatus = oldStatus,
                NewStatus = TournamentStatus.InProgress,
                Timestamp = DateTime.UtcNow
            });
            
            // Start first round
            StartRound(tournament, 1);
            
            return true;
        }
        
        private void SeedParticipants(Tournament tournament)
        {
            if (tournament.Config.RandomizeSeeds)
            {
                // Random seeding
                var rng = new System.Random();
                var shuffled = tournament.Participants.OrderBy(_ => rng.Next()).ToList();
                for (int i = 0; i < shuffled.Count; i++)
                {
                    shuffled[i].Seed = i + 1;
                }
            }
            else if (tournament.Config.SeedByRank)
            {
                // Seed by rank points
                var sorted = tournament.Participants.OrderByDescending(p => p.RankPoints).ToList();
                for (int i = 0; i < sorted.Count; i++)
                {
                    sorted[i].Seed = i + 1;
                }
            }
            else
            {
                // Keep registration order
                ReseedParticipants(tournament);
            }
        }
        
        private void ReseedParticipants(Tournament tournament)
        {
            for (int i = 0; i < tournament.Participants.Count; i++)
            {
                tournament.Participants[i].Seed = i + 1;
            }
        }
        
        private int CalculateTotalRounds(Tournament tournament)
        {
            int playerCount = tournament.CurrentPlayerCount;
            
            switch (tournament.Format)
            {
                case TournamentFormat.SingleElimination:
                    return (int)Math.Ceiling(Math.Log(playerCount, 2));
                    
                case TournamentFormat.DoubleElimination:
                    return (int)Math.Ceiling(Math.Log(playerCount, 2)) * 2;
                    
                case TournamentFormat.RoundRobin:
                    return playerCount - 1;
                    
                case TournamentFormat.Swiss:
                    return (int)Math.Ceiling(Math.Log(playerCount, 2));
                    
                case TournamentFormat.BattleRoyale:
                    return 1;
                    
                default:
                    return (int)Math.Ceiling(Math.Log(playerCount, 2));
            }
        }
        
        private void GenerateRoundMatches(Tournament tournament, int roundNumber)
        {
            var round = new TournamentRound
            {
                RoundNumber = roundNumber,
                Name = GetRoundName(tournament, roundNumber),
                StartTime = DateTime.UtcNow
            };
            
            List<TournamentMatch> matches;
            
            switch (tournament.Format)
            {
                case TournamentFormat.SingleElimination:
                case TournamentFormat.DoubleElimination:
                    matches = GenerateEliminationRoundMatches(tournament, roundNumber);
                    break;
                    
                case TournamentFormat.RoundRobin:
                    matches = GenerateRoundRobinRoundMatches(tournament, roundNumber);
                    break;
                    
                case TournamentFormat.Swiss:
                    matches = GenerateSwissRoundMatches(tournament, roundNumber);
                    break;
                    
                default:
                    matches = GenerateEliminationRoundMatches(tournament, roundNumber);
                    break;
            }
            
            foreach (var match in matches)
            {
                match.TournamentId = tournament.Id;
                match.RoundNumber = roundNumber;
                tournament.Matches.Add(match);
                round.MatchIds.Add(match.Id);
            }
            
            round.PlayersRemaining = tournament.Participants.Count(p => !p.Eliminated);
            tournament.Rounds.Add(round);
        }
        
        private string GetRoundName(Tournament tournament, int roundNumber)
        {
            int remainingRounds = tournament.TotalRounds - roundNumber + 1;
            
            return remainingRounds switch
            {
                1 => "Finals",
                2 => "Semi-Finals",
                3 => "Quarter-Finals",
                _ => $"Round {roundNumber}"
            };
        }
        
        private List<TournamentMatch> GenerateEliminationRoundMatches(Tournament tournament, int roundNumber)
        {
            var matches = new List<TournamentMatch>();
            
            if (roundNumber == 1)
            {
                // First round - pair by seeding
                var sortedParticipants = tournament.Participants.OrderBy(p => p.Seed).ToList();
                int matchCount = sortedParticipants.Count / 2;
                
                for (int i = 0; i < matchCount; i++)
                {
                    var p1 = sortedParticipants[i];
                    var p2 = sortedParticipants[sortedParticipants.Count - 1 - i];
                    
                    matches.Add(new TournamentMatch
                    {
                        MatchNumber = i + 1,
                        Player1Id = p1.PlayerId,
                        Player1Name = p1.PlayerName,
                        Player2Id = p2.PlayerId,
                        Player2Name = p2.PlayerName,
                        BracketSide = BracketSide.Winners,
                        Status = MatchStatus.Scheduled
                    });
                }
                
                // Handle odd player count with bye
                if (sortedParticipants.Count % 2 == 1)
                {
                    var byePlayer = sortedParticipants[sortedParticipants.Count / 2];
                    // Bye player automatically advances
                }
            }
            else
            {
                // Subsequent rounds - pair previous round winners
                var previousRound = tournament.Rounds.LastOrDefault();
                if (previousRound == null) return matches;
                
                var previousMatches = tournament.Matches
                    .Where(m => m.RoundNumber == roundNumber - 1 && m.Status == MatchStatus.Completed)
                    .ToList();
                
                var winners = previousMatches.Select(m => m.WinnerId).ToList();
                
                for (int i = 0; i < winners.Count / 2; i++)
                {
                    var p1Id = winners[i * 2];
                    var p2Id = winners[i * 2 + 1];
                    var p1 = tournament.Participants.First(p => p.PlayerId == p1Id);
                    var p2 = tournament.Participants.First(p => p.PlayerId == p2Id);
                    
                    matches.Add(new TournamentMatch
                    {
                        MatchNumber = i + 1,
                        Player1Id = p1.PlayerId,
                        Player1Name = p1.PlayerName,
                        Player2Id = p2.PlayerId,
                        Player2Name = p2.PlayerName,
                        BracketSide = BracketSide.Winners,
                        Status = MatchStatus.Scheduled
                    });
                }
            }
            
            return matches;
        }
        
        private List<TournamentMatch> GenerateRoundRobinRoundMatches(Tournament tournament, int roundNumber)
        {
            var matches = new List<TournamentMatch>();
            var players = tournament.Participants.Where(p => !p.Eliminated).ToList();
            
            // Circle method for round robin
            int n = players.Count;
            if (n % 2 == 1) n++; // Add dummy player for bye
            
            int round = roundNumber - 1;
            
            for (int i = 0; i < n / 2; i++)
            {
                int p1Index = (round + i) % (n - 1);
                int p2Index = (n - 1 - i + round) % (n - 1);
                
                if (i == 0) p2Index = n - 1;
                
                if (p1Index < players.Count && p2Index < players.Count)
                {
                    var p1 = players[p1Index];
                    var p2 = players[p2Index];
                    
                    matches.Add(new TournamentMatch
                    {
                        MatchNumber = i + 1,
                        Player1Id = p1.PlayerId,
                        Player1Name = p1.PlayerName,
                        Player2Id = p2.PlayerId,
                        Player2Name = p2.PlayerName,
                        Status = MatchStatus.Scheduled
                    });
                }
            }
            
            return matches;
        }
        
        private List<TournamentMatch> GenerateSwissRoundMatches(Tournament tournament, int roundNumber)
        {
            var matches = new List<TournamentMatch>();
            
            // Sort by current points, then by seed
            var players = tournament.Participants
                .Where(p => !p.Eliminated)
                .OrderByDescending(p => p.PointsEarned)
                .ThenBy(p => p.Seed)
                .ToList();
            
            var paired = new HashSet<string>();
            
            for (int i = 0; i < players.Count; i++)
            {
                if (paired.Contains(players[i].PlayerId)) continue;
                
                // Find opponent with similar score who hasn't been paired
                for (int j = i + 1; j < players.Count; j++)
                {
                    if (paired.Contains(players[j].PlayerId)) continue;
                    
                    // Check if they've played before
                    bool alreadyPlayed = tournament.Matches.Any(m =>
                        (m.Player1Id == players[i].PlayerId && m.Player2Id == players[j].PlayerId) ||
                        (m.Player2Id == players[i].PlayerId && m.Player1Id == players[j].PlayerId));
                    
                    if (!alreadyPlayed)
                    {
                        paired.Add(players[i].PlayerId);
                        paired.Add(players[j].PlayerId);
                        
                        matches.Add(new TournamentMatch
                        {
                            MatchNumber = matches.Count + 1,
                            Player1Id = players[i].PlayerId,
                            Player1Name = players[i].PlayerName,
                            Player2Id = players[j].PlayerId,
                            Player2Name = players[j].PlayerName,
                            Status = MatchStatus.Scheduled
                        });
                        break;
                    }
                }
            }
            
            return matches;
        }
        
        private void StartRound(Tournament tournament, int roundNumber)
        {
            var round = tournament.Rounds.FirstOrDefault(r => r.RoundNumber == roundNumber);
            if (round == null) return;
            
            round.StartTime = DateTime.UtcNow;
            
            // Set all matches to waiting
            foreach (var matchId in round.MatchIds)
            {
                var match = tournament.Matches.First(m => m.Id == matchId);
                match.Status = MatchStatus.WaitingForPlayers;
                match.ScheduledTime = DateTime.UtcNow;
            }
            
            OnRoundStarted?.Invoke(tournament, round);
        }
        
        #endregion
        
        #region Match Management
        
        /// <summary>
        /// Start a match
        /// </summary>
        public bool StartMatch(string tournamentId, string matchId)
        {
            if (!_activeTournaments.TryGetValue(tournamentId, out var tournament))
            {
                return false;
            }
            
            var match = tournament.Matches.FirstOrDefault(m => m.Id == matchId);
            if (match == null || match.Status != MatchStatus.WaitingForPlayers)
            {
                return false;
            }
            
            match.Status = MatchStatus.InProgress;
            match.StartTime = DateTime.UtcNow;
            
            return true;
        }
        
        /// <summary>
        /// Report match result
        /// </summary>
        public bool ReportMatchResult(
            string tournamentId,
            string matchId,
            string winnerId,
            int player1Score,
            int player2Score,
            float player1Votes = 0f,
            float player2Votes = 0f)
        {
            if (!_activeTournaments.TryGetValue(tournamentId, out var tournament))
            {
                return false;
            }
            
            var match = tournament.Matches.FirstOrDefault(m => m.Id == matchId);
            if (match == null)
            {
                return false;
            }
            
            // Determine winner
            match.WinnerId = winnerId;
            match.LoserId = winnerId == match.Player1Id ? match.Player2Id : match.Player1Id;
            match.Player1Score = player1Score;
            match.Player2Score = player2Score;
            match.Player1VotePercentage = player1Votes;
            match.Player2VotePercentage = player2Votes;
            match.Status = MatchStatus.Completed;
            match.EndTime = DateTime.UtcNow;
            
            // Update participant stats
            var winner = tournament.Participants.First(p => p.PlayerId == winnerId);
            var loser = tournament.Participants.First(p => p.PlayerId == match.LoserId);
            
            winner.Wins++;
            winner.MatchesPlayed++;
            winner.PointsEarned += 3; // 3 points for win
            winner.TotalVotePercentage += player1Votes > player2Votes ? player1Votes : player2Votes;
            
            loser.Losses++;
            loser.MatchesPlayed++;
            loser.TotalVotePercentage += player1Votes < player2Votes ? player1Votes : player2Votes;
            
            // Check if loser is eliminated
            if (tournament.Format == TournamentFormat.SingleElimination ||
                (tournament.Format == TournamentFormat.DoubleElimination && loser.Losses >= 2))
            {
                loser.Eliminated = true;
                loser.EliminatedAt = DateTime.UtcNow;
                loser.CurrentPosition = tournament.Participants.Count(p => !p.Eliminated) + 1;
                OnPlayerEliminated?.Invoke(tournament, loser);
            }
            
            // Check for upset
            bool wasUpset = winner.Seed > loser.Seed;
            
            OnMatchCompleted?.Invoke(new MatchCompletedEvent
            {
                TournamentId = tournament.Id,
                Match = match,
                Winner = winner,
                Loser = loser,
                WasUpset = wasUpset,
                Timestamp = DateTime.UtcNow
            });
            
            // Check if round is complete
            CheckRoundComplete(tournament);
            
            return true;
        }
        
        /// <summary>
        /// Report a forfeit
        /// </summary>
        public bool ReportForfeit(string tournamentId, string matchId, string forfeitingPlayerId)
        {
            if (!_activeTournaments.TryGetValue(tournamentId, out var tournament))
            {
                return false;
            }
            
            var match = tournament.Matches.FirstOrDefault(m => m.Id == matchId);
            if (match == null)
            {
                return false;
            }
            
            string winnerId = forfeitingPlayerId == match.Player1Id ? match.Player2Id : match.Player1Id;
            
            match.WinnerId = winnerId;
            match.LoserId = forfeitingPlayerId;
            match.Status = MatchStatus.Forfeit;
            match.EndTime = DateTime.UtcNow;
            
            // Update stats
            var winner = tournament.Participants.First(p => p.PlayerId == winnerId);
            var loser = tournament.Participants.First(p => p.PlayerId == forfeitingPlayerId);
            
            winner.Wins++;
            winner.MatchesPlayed++;
            
            loser.Losses++;
            loser.MatchesPlayed++;
            loser.Eliminated = true;
            loser.EliminatedAt = DateTime.UtcNow;
            
            OnPlayerEliminated?.Invoke(tournament, loser);
            CheckRoundComplete(tournament);
            
            return true;
        }
        
        private void CheckRoundComplete(Tournament tournament)
        {
            var currentRound = tournament.Rounds.FirstOrDefault(r => r.RoundNumber == tournament.CurrentRound);
            if (currentRound == null) return;
            
            var roundMatches = tournament.Matches.Where(m => currentRound.MatchIds.Contains(m.Id)).ToList();
            bool allComplete = roundMatches.All(m => 
                m.Status == MatchStatus.Completed || 
                m.Status == MatchStatus.Forfeit ||
                m.Status == MatchStatus.Cancelled);
            
            if (allComplete)
            {
                CompleteRound(tournament, currentRound);
            }
        }
        
        private void CompleteRound(Tournament tournament, TournamentRound round)
        {
            round.EndTime = DateTime.UtcNow;
            round.IsComplete = true;
            
            OnRoundCompleted?.Invoke(tournament, round);
            
            // Update standings
            UpdateStandings(tournament);
            
            // Check if tournament is complete
            int remaining = tournament.Participants.Count(p => !p.Eliminated);
            
            if (remaining <= 1 || tournament.CurrentRound >= tournament.TotalRounds)
            {
                CompleteTournament(tournament);
            }
            else
            {
                // Start next round
                tournament.CurrentRound++;
                GenerateRoundMatches(tournament, tournament.CurrentRound);
                StartRound(tournament, tournament.CurrentRound);
            }
        }
        
        #endregion
        
        #region Tournament Completion
        
        private void CompleteTournament(Tournament tournament)
        {
            tournament.Status = TournamentStatus.Completed;
            tournament.EndTime = DateTime.UtcNow;
            
            // Final standings
            UpdateStandings(tournament);
            
            // Determine champion
            var champion = tournament.Participants
                .OrderBy(p => p.Eliminated ? 1 : 0)
                .ThenByDescending(p => p.Wins)
                .ThenByDescending(p => p.PointsEarned)
                .First();
            
            champion.CurrentPosition = 1;
            
            // Assign final positions
            AssignFinalPositions(tournament);
            
            // Distribute rewards
            var rewardsByPlayer = DistributeRewards(tournament);
            
            // Update player histories
            UpdatePlayerHistories(tournament);
            
            // Update rankings
            if (tournament.IsRanked)
            {
                _rankingSystem.ProcessTournamentResults(tournament);
            }
            
            OnTournamentCompleted?.Invoke(new TournamentCompletedEvent
            {
                Tournament = tournament,
                Champion = champion,
                FinalStandings = tournament.Standings,
                RewardsByPlayer = rewardsByPlayer,
                Timestamp = DateTime.UtcNow
            });
        }
        
        private void AssignFinalPositions(Tournament tournament)
        {
            var sorted = tournament.Participants
                .OrderBy(p => p.Eliminated ? 1 : 0)
                .ThenByDescending(p => p.Wins)
                .ThenBy(p => p.Losses)
                .ThenByDescending(p => p.PointsEarned)
                .ThenByDescending(p => p.TotalVotePercentage)
                .ToList();
            
            for (int i = 0; i < sorted.Count; i++)
            {
                sorted[i].CurrentPosition = i + 1;
            }
        }
        
        private void UpdateStandings(Tournament tournament)
        {
            tournament.Standings.Clear();
            
            var sorted = tournament.Participants
                .OrderBy(p => p.Eliminated ? 1 : 0)
                .ThenByDescending(p => p.Wins)
                .ThenBy(p => p.Losses)
                .ThenByDescending(p => p.PointsEarned)
                .ToList();
            
            for (int i = 0; i < sorted.Count; i++)
            {
                tournament.Standings.Add(new TournamentStanding
                {
                    Position = i + 1,
                    PlayerId = sorted[i].PlayerId,
                    PlayerName = sorted[i].PlayerName,
                    Wins = sorted[i].Wins,
                    Losses = sorted[i].Losses,
                    Points = sorted[i].PointsEarned,
                    GamesPlayed = sorted[i].MatchesPlayed,
                    WinPercentage = sorted[i].WinRate
                });
            }
        }
        
        private Dictionary<string, List<TournamentReward>> DistributeRewards(Tournament tournament)
        {
            var rewards = new Dictionary<string, List<TournamentReward>>();
            
            foreach (var participant in tournament.Participants)
            {
                var playerRewards = tournament.Rewards
                    .Where(r => r.QualifiesForReward(participant.CurrentPosition))
                    .ToList();
                
                if (playerRewards.Any())
                {
                    rewards[participant.PlayerId] = playerRewards;
                    
                    // Distribute rewards to currency system
                    DistributeRewardsToPlayer(participant.PlayerId, playerRewards, tournament);
                    
                    // Mark as claimed
                    foreach (var reward in playerRewards)
                    {
                        reward.IsClaimed = true;
                    }
                }
            }
            
            return rewards;
        }
        
        /// <summary>
        /// Distribute rewards to a player's currency/inventory
        /// </summary>
        private void DistributeRewardsToPlayer(string playerId, List<TournamentReward> rewards, Tournament tournament)
        {
            if (_currencyManager == null) return;
            
            foreach (var reward in rewards)
            {
                switch (reward.Type)
                {
                    case RewardType.Currency:
                        // Award CloutBux
                        _currencyManager.Credit(
                            CurrencyType.CloutBux,
                            reward.Amount,
                            $"Tournament Reward: {reward.RewardName}",
                            $"tournament_{tournament.Id}"
                        );
                        Debug.Log($"[Tournament] Awarded {reward.Amount} CloutBux to {playerId} for {reward.RewardName}");
                        break;
                        
                    case RewardType.SeasonPoints:
                        // Season points are tracked separately in ranking system
                        // Already handled by _rankingSystem.ProcessTournamentResults()
                        Debug.Log($"[Tournament] Awarded {reward.Amount} Season Points to {playerId}");
                        break;
                        
                    case RewardType.Title:
                    case RewardType.Cosmetic:
                    case RewardType.Badge:
                        // These would be added to player inventory
                        // Would need PlayerInventory integration
                        Debug.Log($"[Tournament] Awarded {reward.RewardName} ({reward.Type}) to {playerId}");
                        break;
                }
            }
        }
        
        private void UpdatePlayerHistories(Tournament tournament)
        {
            foreach (var participant in tournament.Participants)
            {
                if (!_playerHistories.TryGetValue(participant.PlayerId, out var history))
                {
                    history = new TournamentHistory { PlayerId = participant.PlayerId };
                    _playerHistories[participant.PlayerId] = history;
                }
                
                history.TournamentsEntered++;
                history.TotalWins += participant.Wins;
                history.TotalLosses += participant.Losses;
                
                if (participant.CurrentPosition == 1)
                {
                    history.TournamentsWon++;
                }
                
                if (participant.CurrentPosition < history.BestPlacement || history.BestPlacement == 0)
                {
                    history.BestPlacement = participant.CurrentPosition;
                }
                
                history.RecentResults.Add(new TournamentResult
                {
                    TournamentId = tournament.Id,
                    TournamentName = tournament.Name,
                    Tier = tournament.Tier,
                    Placement = participant.CurrentPosition,
                    TotalParticipants = tournament.CurrentPlayerCount,
                    Wins = participant.Wins,
                    Losses = participant.Losses,
                    CompletedAt = DateTime.UtcNow
                });
                
                // Keep only last 20 results
                while (history.RecentResults.Count > 20)
                {
                    history.RecentResults.RemoveAt(0);
                }
            }
        }
        
        #endregion
        
        #region Queries
        
        public Tournament GetTournament(string tournamentId)
        {
            return _activeTournaments.TryGetValue(tournamentId, out var t) ? t : null;
        }
        
        public List<Tournament> GetActiveTournaments()
        {
            return _activeTournaments.Values
                .Where(t => t.Status != TournamentStatus.Completed && t.Status != TournamentStatus.Cancelled)
                .ToList();
        }
        
        public List<Tournament> GetOpenTournaments()
        {
            return _activeTournaments.Values
                .Where(t => t.Status == TournamentStatus.Registration && t.IsPublic)
                .ToList();
        }
        
        public TournamentHistory GetPlayerHistory(string playerId)
        {
            return _playerHistories.TryGetValue(playerId, out var h) ? h : null;
        }
        
        public TournamentSeason GetCurrentSeason()
        {
            return _currentSeason;
        }
        
        #endregion
    }
}

