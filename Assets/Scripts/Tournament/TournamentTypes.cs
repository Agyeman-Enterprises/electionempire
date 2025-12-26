// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - TOURNAMENT TYPES
// Core enums, data structures, and type definitions for tournament system
// ═══════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElectionEmpire.Tournament
{
    #region Enums
    
    /// <summary>
    /// Tournament format types
    /// </summary>
    public enum TournamentFormat
    {
        SingleElimination,    // Standard bracket, one loss = out
        DoubleElimination,     // Losers bracket, two losses = out
        RoundRobin,           // Everyone plays everyone
        Swiss,                // Swiss pairing system
        BattleRoyale,         // All players in one match
        League,               // Season-long league
        Ladder                // Challenge-based ladder
    }
    
    /// <summary>
    /// Tournament tier/importance level
    /// </summary>
    public enum TournamentTier
    {
        Community,      // Player-hosted, casual
        Weekly,         // Weekly ranked tournament
        Monthly,        // Monthly ranked tournament
        Seasonal,       // Season-long tournament
        Championship,   // Major championship event
        Official        // Official developer tournament
    }
    
    /// <summary>
    /// Tournament status
    /// </summary>
    public enum TournamentStatus
    {
        Registration,   // Accepting players
        Pending,        // Waiting to start
        InProgress,     // Currently running
        Completed,      // Finished
        Cancelled,      // Cancelled before completion
        Paused          // Temporarily paused
    }
    
    /// <summary>
    /// Scoring system for tournaments
    /// </summary>
    public enum ScoringSystem
    {
        WinLoss,        // Simple win/loss record
        Points,          // Point-based scoring
        VotePercentage,  // Based on vote percentages
        Composite        // Combination of factors
    }
    
    /// <summary>
    /// Victory condition for matches
    /// </summary>
    public enum VictoryCondition
    {
        MostVotes,          // Highest vote count
        HighestApproval,    // Highest approval rating
        Elimination,        // Last player standing
        Points              // Most points accumulated
    }
    
    /// <summary>
    /// Match status
    /// </summary>
    public enum MatchStatus
    {
        Scheduled,          // Scheduled but not started
        WaitingForPlayers,  // Waiting for players to join
        InProgress,         // Currently playing
        Completed,          // Finished normally
        Forfeit,            // One player forfeited
        Cancelled           // Match cancelled
    }
    
    /// <summary>
    /// Bracket side (for double elimination)
    /// </summary>
    public enum BracketSide
    {
        Winners,    // Winners bracket
        Losers      // Losers bracket
    }
    
    /// <summary>
    /// Rank tier for players
    /// </summary>
    public enum RankTier
    {
        Unranked,
        Bronze,
        Silver,
        Gold,
        Platinum,
        Diamond,
        Master,
        GrandMaster,
        Champion
    }
    
    /// <summary>
    /// Entry requirement type
    /// </summary>
    public enum EntryRequirement
    {
        Open,           // Anyone can join
        InviteOnly,     // Invitation required
        RankRequired,   // Minimum rank required
        Qualified       // Must have qualifying points
    }
    
    /// <summary>
    /// Reward type
    /// </summary>
    public enum RewardType
    {
        Currency,       // In-game currency
        Title,          // Title/cosmetic
        SeasonPoints,   // Season ranking points
        Cosmetic,       // Cosmetic item
        Badge           // Achievement badge
    }
    
    #endregion
    
    #region Core Data Structures
    
    /// <summary>
    /// A tournament participant
    /// </summary>
    [Serializable]
    public class TournamentParticipant
    {
        public string PlayerId;
        public string PlayerName;
        public string DisplayName;
        public RankTier Rank;
        public int RankPoints;
        public int Seed;
        public bool CheckedIn;
        public DateTime? CheckedInAt;
        
        // Match stats
        public int Wins;
        public int Losses;
        public int MatchesPlayed;
        public float PointsEarned;
        public float TotalVotePercentage;
        public float WinRate => MatchesPlayed > 0 ? (float)Wins / MatchesPlayed : 0f;
        
        // Tournament state
        public bool Eliminated;
        public DateTime? EliminatedAt;
        public int CurrentPosition;
        
        public TournamentParticipant()
        {
            Rank = RankTier.Unranked;
        }
    }
    
    /// <summary>
    /// A tournament match
    /// </summary>
    [Serializable]
    public class TournamentMatch
    {
        public string Id;
        public string TournamentId;
        public int RoundNumber;
        public int MatchNumber;
        public string Player1Id;
        public string Player1Name;
        public string Player2Id;
        public string Player2Name;
        public string WinnerId;
        public string LoserId;
        public int Player1Score;
        public int Player2Score;
        public float Player1VotePercentage;
        public float Player2VotePercentage;
        public BracketSide BracketSide;
        public MatchStatus Status;
        public DateTime? ScheduledTime;
        public DateTime? StartTime;
        public DateTime? EndTime;
        
        public TournamentMatch()
        {
            Id = Guid.NewGuid().ToString();
            Status = MatchStatus.Scheduled;
        }
    }
    
    /// <summary>
    /// A tournament round
    /// </summary>
    [Serializable]
    public class TournamentRound
    {
        public int RoundNumber;
        public string Name;
        public List<string> MatchIds;
        public int PlayersRemaining;
        public DateTime? StartTime;
        public DateTime? EndTime;
        public bool IsComplete;
        
        public TournamentRound()
        {
            MatchIds = new List<string>();
        }
    }
    
    /// <summary>
    /// Tournament configuration
    /// </summary>
    [Serializable]
    public class TournamentConfig
    {
        public EntryRequirement EntryType;
        public RankTier RequiredRankTier;
        public int MinimumRank;
        public bool RequireCheckIn;
        public bool AutoStartWhenFull;
        public bool RandomizeSeeds;
        public bool SeedByRank;
        public int MatchTimeLimit; // Minutes
        public bool AllowSpectators;
        
        public TournamentConfig()
        {
            EntryType = EntryRequirement.Open;
            MatchTimeLimit = 30;
            AllowSpectators = true;
        }
    }
    
    /// <summary>
    /// Tournament rules
    /// </summary>
    [Serializable]
    public class TournamentRules
    {
        public bool AllowDirtyTricks;
        public bool AllowScandals;
        public bool AllowAlliances;
        public bool AllowBetrayals;
        public int MaxScandalsPerMatch;
        public Dictionary<string, object> CustomRules;
        
        public TournamentRules()
        {
            AllowDirtyTricks = true;
            AllowScandals = true;
            AllowAlliances = true;
            AllowBetrayals = true;
            MaxScandalsPerMatch = 3;
            CustomRules = new Dictionary<string, object>();
        }
    }
    
    /// <summary>
    /// Tournament reward
    /// </summary>
    [Serializable]
    public class TournamentReward
    {
        public int MinPlacement;
        public int MaxPlacement;
        public RewardType Type;
        public string RewardId;
        public string RewardName;
        public string Description;
        public int Amount;
        public bool IsClaimed;
        
        public bool QualifiesForReward(int placement)
        {
            return placement >= MinPlacement && placement <= MaxPlacement;
        }
    }
    
    /// <summary>
    /// Tournament standing entry
    /// </summary>
    [Serializable]
    public class TournamentStanding
    {
        public int Position;
        public string PlayerId;
        public string PlayerName;
        public int Wins;
        public int Losses;
        public float Points;
        public int GamesPlayed;
        public float WinPercentage;
    }
    
    /// <summary>
    /// Main tournament data structure
    /// </summary>
    [Serializable]
    public class Tournament
    {
        public string Id;
        public string Name;
        public string HostId;
        public string HostName;
        public TournamentFormat Format;
        public TournamentTier Tier;
        public TournamentStatus Status;
        public ScoringSystem Scoring;
        public VictoryCondition Victory;
        public int MaxPlayers;
        public int MinPlayers;
        public int CurrentPlayerCount;
        public bool IsRanked;
        public bool IsPublic;
        
        public TournamentConfig Config;
        public TournamentRules Rules;
        public List<TournamentReward> Rewards;
        public List<TournamentParticipant> Participants;
        public List<TournamentMatch> Matches;
        public List<TournamentRound> Rounds;
        public List<TournamentStanding> Standings;
        public List<string> InvitedPlayerIds;
        public List<string> BannedPlayerIds;
        
        public int CurrentRound;
        public int TotalRounds;
        public DateTime? StartTime;
        public DateTime? EndTime;
        public string Bracket; // JSON or bracket structure
        
        public Tournament()
        {
            Id = Guid.NewGuid().ToString();
            Status = TournamentStatus.Registration;
            Participants = new List<TournamentParticipant>();
            Matches = new List<TournamentMatch>();
            Rounds = new List<TournamentRound>();
            Standings = new List<TournamentStanding>();
            Rewards = new List<TournamentReward>();
            InvitedPlayerIds = new List<string>();
            BannedPlayerIds = new List<string>();
        }
    }
    
    /// <summary>
    /// Tournament season
    /// </summary>
    [Serializable]
    public class TournamentSeason
    {
        public string Name;
        public int SeasonNumber;
        public DateTime StartDate;
        public DateTime EndDate;
        public bool IsActive;
        public int QualifyingPointsThreshold;
    }
    
    /// <summary>
    /// Player tournament history
    /// </summary>
    [Serializable]
    public class TournamentHistory
    {
        public string PlayerId;
        public int TournamentsEntered;
        public int TournamentsWon;
        public int TotalWins;
        public int TotalLosses;
        public int BestPlacement;
        public List<TournamentResult> RecentResults;
        
        public TournamentHistory()
        {
            RecentResults = new List<TournamentResult>();
        }
    }
    
    /// <summary>
    /// Individual tournament result for a player
    /// </summary>
    [Serializable]
    public class TournamentResult
    {
        public string TournamentId;
        public string TournamentName;
        public TournamentTier Tier;
        public int Placement;
        public int TotalParticipants;
        public int Wins;
        public int Losses;
        public DateTime CompletedAt;
    }
    
    /// <summary>
    /// Tournament system configuration
    /// </summary>
    [Serializable]
    public class TournamentSystemConfig
    {
        public int MaxConcurrentTournaments = 100;
        public int MaxPlayersPerTournament = 128;
        public int DaysPerSeason = 90;
        public float SeasonPointDecayRate = 0.1f;
        
        public TournamentSystemConfig()
        {
        }
    }
    
    #endregion
    
    #region Event Classes
    
    /// <summary>
    /// Tournament status changed event
    /// </summary>
    public class TournamentStatusChangedEvent
    {
        public string TournamentId;
        public TournamentStatus OldStatus;
        public TournamentStatus NewStatus;
        public DateTime Timestamp;
    }
    
    /// <summary>
    /// Match completed event
    /// </summary>
    public class MatchCompletedEvent
    {
        public string TournamentId;
        public TournamentMatch Match;
        public TournamentParticipant Winner;
        public TournamentParticipant Loser;
        public bool WasUpset;
        public DateTime Timestamp;
    }
    
    /// <summary>
    /// Tournament completed event
    /// </summary>
    public class TournamentCompletedEvent
    {
        public Tournament Tournament;
        public TournamentParticipant Champion;
        public List<TournamentStanding> FinalStandings;
        public Dictionary<string, List<TournamentReward>> RewardsByPlayer;
        public DateTime Timestamp;
    }
    
    #endregion
}

