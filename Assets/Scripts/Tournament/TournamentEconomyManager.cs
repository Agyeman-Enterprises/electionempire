// ═══════════════════════════════════════════════════════════════════════════════
// ELECTION EMPIRE - TOURNAMENT ECONOMY INTEGRATION
// Connects tournament system with Purrkoin, entry fees, and prize distribution
// ═══════════════════════════════════════════════════════════════════════════════

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ElectionEmpire.Economy;

namespace ElectionEmpire.Tournament
{
    /// <summary>
    /// Manages all economic aspects of tournaments
    /// </summary>
    public class TournamentEconomyManager
    {
        #region Singleton
        
        private static TournamentEconomyManager _instance;
        public static TournamentEconomyManager Instance => _instance ??= new TournamentEconomyManager();
        
        #endregion
        
        #region State
        
        private TournamentEconomyConfig _config;
        private Dictionary<string, TournamentPrizePool> _prizePools;
        private Dictionary<string, TournamentEntryFee> _entryFees;
        private Dictionary<string, List<string>> _registeredPlayers; // tournamentId -> playerIds
        private Dictionary<string, decimal> _collectedEntryFees; // tournamentId -> total
        
        #endregion
        
        #region Events
        
        public event Action<string, string, long> OnEntryFeePaid; // tournamentId, playerId, amount
        public event Action<string, string, long> OnEntryFeeRefunded;
        public event Action<string, TournamentPrizePool> OnPrizePoolUpdated;
        public event Action<string, string, long, string> OnPrizeAwarded; // tournamentId, playerId, amount, title
        
        #endregion
        
        #region Initialization
        
        public TournamentEconomyManager()
        {
            _config = new TournamentEconomyConfig();
            _prizePools = new Dictionary<string, TournamentPrizePool>();
            _entryFees = new Dictionary<string, TournamentEntryFee>();
            _registeredPlayers = new Dictionary<string, List<string>>();
            _collectedEntryFees = new Dictionary<string, decimal>();
        }
        
        public void Initialize(TournamentEconomyConfig config = null)
        {
            if (config != null)
            {
                _config = config;
            }
        }
        
        #endregion
        
        #region Entry Fees
        
        /// <summary>
        /// Configure entry fee for a tournament
        /// </summary>
        public TournamentEntryFee SetupEntryFee(string tournamentId, CurrencyType currency, 
            long amount, bool isRefundable = true, float refundPercentage = 100f, 
            int refundDeadlineHours = 24)
        {
            var fee = new TournamentEntryFee
            {
                TournamentId = tournamentId,
                Currency = currency,
                Amount = amount,
                IsRefundable = isRefundable,
                RefundPercentage = refundPercentage,
                RefundDeadline = DateTime.UtcNow.AddHours(refundDeadlineHours)
            };
            
            _entryFees[tournamentId] = fee;
            _registeredPlayers[tournamentId] = new List<string>();
            _collectedEntryFees[tournamentId] = 0;
            
            return fee;
        }
        
        /// <summary>
        /// Process a player paying the entry fee
        /// </summary>
        public EntryFeeResult ProcessEntryFee(string tournamentId, string playerId)
        {
            if (!_entryFees.TryGetValue(tournamentId, out var fee))
            {
                return new EntryFeeResult { Success = false, Error = "No entry fee configured" };
            }
            
            // Check if already registered
            if (_registeredPlayers.TryGetValue(tournamentId, out var players) && players.Contains(playerId))
            {
                return new EntryFeeResult { Success = false, Error = "Already registered" };
            }
            
            // Process payment through economy manager
            var result = EconomyManager.Instance.ProcessTournamentEntry(playerId, fee);
            
            if (!result.Success)
            {
                return new EntryFeeResult { Success = false, Error = result.Error };
            }
            
            // Track registration
            if (!_registeredPlayers.ContainsKey(tournamentId))
            {
                _registeredPlayers[tournamentId] = new List<string>();
            }
            _registeredPlayers[tournamentId].Add(playerId);
            
            // Update collected fees
            _collectedEntryFees[tournamentId] += fee.Amount;
            
            // Update prize pool if applicable
            UpdatePrizePoolFromEntries(tournamentId);
            
            OnEntryFeePaid?.Invoke(tournamentId, playerId, fee.Amount);
            
            return new EntryFeeResult
            {
                Success = true,
                AmountPaid = fee.Amount,
                Currency = fee.Currency
            };
        }
        
        /// <summary>
        /// Refund entry fee for a player
        /// </summary>
        public EntryFeeResult RefundEntryFee(string tournamentId, string playerId)
        {
            if (!_entryFees.TryGetValue(tournamentId, out var fee))
            {
                return new EntryFeeResult { Success = false, Error = "No entry fee configured" };
            }
            
            if (!_registeredPlayers.TryGetValue(tournamentId, out var players) || !players.Contains(playerId))
            {
                return new EntryFeeResult { Success = false, Error = "Player not registered" };
            }
            
            var result = EconomyManager.Instance.RefundTournamentEntry(playerId, fee);
            
            if (!result.Success)
            {
                return new EntryFeeResult { Success = false, Error = result.Error };
            }
            
            // Remove from registered
            players.Remove(playerId);
            
            // Update collected fees
            long refundAmount = (long)(fee.Amount * fee.RefundPercentage / 100f);
            _collectedEntryFees[tournamentId] -= refundAmount;
            
            OnEntryFeeRefunded?.Invoke(tournamentId, playerId, refundAmount);
            
            return new EntryFeeResult
            {
                Success = true,
                AmountRefunded = refundAmount,
                Currency = fee.Currency
            };
        }
        
        #endregion
        
        #region Prize Pools
        
        /// <summary>
        /// Create a prize pool for a tournament
        /// </summary>
        public TournamentPrizePool CreatePrizePool(string tournamentId, long basePrize, 
            CurrencyType currency, bool contributedByEntryFees = true, 
            float entryFeeContribution = 80f)
        {
            var pool = new TournamentPrizePool
            {
                TournamentId = tournamentId,
                BasePrizePool = basePrize,
                TotalPool = basePrize,
                Currency = currency,
                IsContributedByEntryFees = contributedByEntryFees,
                EntryFeeContributionPercent = entryFeeContribution
            };
            
            // Set up default placements
            pool.Placements = GetDefaultPlacements(tournamentId);
            
            _prizePools[tournamentId] = pool;
            
            return pool;
        }
        
        /// <summary>
        /// Create a prize pool with custom placements
        /// </summary>
        public TournamentPrizePool CreateCustomPrizePool(string tournamentId, long basePrize,
            CurrencyType currency, List<PrizePlacement> placements)
        {
            var pool = CreatePrizePool(tournamentId, basePrize, currency, true, 80f);
            pool.Placements = placements;
            return pool;
        }
        
        private void UpdatePrizePoolFromEntries(string tournamentId)
        {
            if (!_prizePools.TryGetValue(tournamentId, out var pool))
            {
                return;
            }
            
            if (!pool.IsContributedByEntryFees)
            {
                return;
            }
            
            if (!_collectedEntryFees.TryGetValue(tournamentId, out var collected))
            {
                return;
            }
            
            long contribution = (long)((float)collected * pool.EntryFeeContributionPercent / 100f);
            pool.TotalPool = pool.BasePrizePool + contribution;
            
            OnPrizePoolUpdated?.Invoke(tournamentId, pool);
        }
        
        private List<PrizePlacement> GetDefaultPlacements(string tournamentId)
        {
            return new List<PrizePlacement>
            {
                new PrizePlacement
                {
                    MinPlace = 1,
                    MaxPlace = 1,
                    PoolPercentage = 50f,
                    Title = "Champion",
                    BonusItemIds = new List<string> { "cosmetic_champion_frame", "title_champion" }
                },
                new PrizePlacement
                {
                    MinPlace = 2,
                    MaxPlace = 2,
                    PoolPercentage = 25f,
                    Title = "Runner-Up",
                    BonusItemIds = new List<string> { "cosmetic_runnerup_frame" }
                },
                new PrizePlacement
                {
                    MinPlace = 3,
                    MaxPlace = 4,
                    PoolPercentage = 10f,
                    Title = "Semi-Finalist",
                    BonusItemIds = new List<string>()
                },
                new PrizePlacement
                {
                    MinPlace = 5,
                    MaxPlace = 8,
                    PoolPercentage = 3.75f,
                    Title = "Quarter-Finalist",
                    BonusItemIds = new List<string>()
                }
            };
        }
        
        /// <summary>
        /// Get current prize pool
        /// </summary>
        public TournamentPrizePool GetPrizePool(string tournamentId)
        {
            _prizePools.TryGetValue(tournamentId, out var pool);
            return pool;
        }
        
        #endregion
        
        #region Prize Distribution
        
        /// <summary>
        /// Distribute prizes when tournament ends
        /// </summary>
        public PrizeDistributionResult DistributePrizes(string tournamentId, 
            List<TournamentStanding> finalStandings)
        {
            if (!_prizePools.TryGetValue(tournamentId, out var pool))
            {
                return new PrizeDistributionResult 
                { 
                    Success = false, 
                    Error = "No prize pool found" 
                };
            }
            
            var playerPlacements = finalStandings.ToDictionary(
                s => s.PlayerId,
                s => s.Position
            );
            
            // Use economy manager to distribute
            EconomyManager.Instance.DistributeTournamentPrizes(pool, playerPlacements);
            
            var result = new PrizeDistributionResult
            {
                Success = true,
                TotalDistributed = pool.TotalPool,
                WinnerPayouts = new Dictionary<string, long>()
            };
            
            foreach (var standing in finalStandings)
            {
                foreach (var placement in pool.Placements)
                {
                    if (standing.Position >= placement.MinPlace && standing.Position <= placement.MaxPlace)
                    {
                        long prize = placement.CalculatePrize(pool.TotalPool);
                        result.WinnerPayouts[standing.PlayerId] = prize;
                        
                        OnPrizeAwarded?.Invoke(tournamentId, standing.PlayerId, prize, placement.Title);
                        break;
                    }
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Calculate expected prize for a placement
        /// </summary>
        public long CalculateExpectedPrize(string tournamentId, int placement)
        {
            if (!_prizePools.TryGetValue(tournamentId, out var pool))
            {
                return 0;
            }
            
            foreach (var prizePlacement in pool.Placements)
            {
                if (placement >= prizePlacement.MinPlace && placement <= prizePlacement.MaxPlace)
                {
                    return prizePlacement.CalculatePrize(pool.TotalPool);
                }
            }
            
            return 0;
        }
        
        #endregion
        
        #region Tournament Tier Presets
        
        /// <summary>
        /// Get preset configuration for a tournament tier
        /// </summary>
        public TournamentEconomyPreset GetPresetForTier(TournamentTier tier)
        {
            return tier switch
            {
                TournamentTier.Community => new TournamentEconomyPreset
                {
                    EntryFee = 0,
                    EntryCurrency = CurrencyType.CloutBux,
                    BasePrize = 500,
                    PrizeCurrency = CurrencyType.CloutBux,
                    EntryFeeContribution = 0
                },
                
                TournamentTier.Weekly => new TournamentEconomyPreset
                {
                    EntryFee = 100,
                    EntryCurrency = CurrencyType.CloutBux,
                    BasePrize = 2000,
                    PrizeCurrency = CurrencyType.CloutBux,
                    EntryFeeContribution = 80
                },
                
                TournamentTier.Monthly => new TournamentEconomyPreset
                {
                    EntryFee = 50,
                    EntryCurrency = CurrencyType.Purrkoin,
                    BasePrize = 500,
                    PrizeCurrency = CurrencyType.Purrkoin,
                    EntryFeeContribution = 75
                },
                
                TournamentTier.Seasonal => new TournamentEconomyPreset
                {
                    EntryFee = 100,
                    EntryCurrency = CurrencyType.Purrkoin,
                    BasePrize = 2000,
                    PrizeCurrency = CurrencyType.Purrkoin,
                    EntryFeeContribution = 70
                },
                
                TournamentTier.Championship => new TournamentEconomyPreset
                {
                    EntryFee = 250,
                    EntryCurrency = CurrencyType.Purrkoin,
                    BasePrize = 10000,
                    PrizeCurrency = CurrencyType.Purrkoin,
                    EntryFeeContribution = 60
                },
                
                TournamentTier.Official => new TournamentEconomyPreset
                {
                    EntryFee = 0, // Free entry
                    EntryCurrency = CurrencyType.Purrkoin,
                    BasePrize = 50000,
                    PrizeCurrency = CurrencyType.Purrkoin,
                    EntryFeeContribution = 0 // Developer-funded
                },
                
                _ => new TournamentEconomyPreset
                {
                    EntryFee = 50,
                    EntryCurrency = CurrencyType.CloutBux,
                    BasePrize = 1000,
                    PrizeCurrency = CurrencyType.CloutBux,
                    EntryFeeContribution = 80
                }
            };
        }
        
        /// <summary>
        /// Set up tournament economy from preset
        /// </summary>
        public void SetupFromPreset(string tournamentId, TournamentTier tier)
        {
            var preset = GetPresetForTier(tier);
            
            if (preset.EntryFee > 0)
            {
                SetupEntryFee(tournamentId, preset.EntryCurrency, preset.EntryFee, 
                    isRefundable: true, refundPercentage: 80f);
            }
            
            CreatePrizePool(tournamentId, preset.BasePrize, preset.PrizeCurrency,
                preset.EntryFeeContribution > 0, preset.EntryFeeContribution);
        }
        
        #endregion
        
        #region Participation Rewards
        
        /// <summary>
        /// Award participation rewards (everyone gets something)
        /// </summary>
        public void AwardParticipationRewards(string tournamentId, List<string> participants, 
            TournamentTier tier)
        {
            long participationReward = tier switch
            {
                TournamentTier.Community => 25,
                TournamentTier.Weekly => 50,
                TournamentTier.Monthly => 75,
                TournamentTier.Seasonal => 100,
                TournamentTier.Championship => 200,
                TournamentTier.Official => 500,
                _ => 50
            };
            
            foreach (var playerId in participants)
            {
                EconomyManager.Instance.GrantCurrency(playerId, CurrencyType.CloutBux,
                    participationReward, TransactionType.TournamentReward,
                    $"Tournament participation: {tournamentId}", tournamentId);
            }
        }
        
        /// <summary>
        /// Award bonus for match win
        /// </summary>
        public void AwardMatchWinBonus(string playerId, string tournamentId, bool wasUpset)
        {
            long baseBonus = 20;
            if (wasUpset)
            {
                baseBonus = (long)(baseBonus * 1.5f); // 50% bonus for upsets
            }
            
            EconomyManager.Instance.GrantCurrency(playerId, CurrencyType.CloutBux,
                baseBonus, TransactionType.TournamentReward,
                wasUpset ? "Upset victory!" : "Match victory", tournamentId);
        }
        
        #endregion
        
        #region Stake Tournaments
        
        /// <summary>
        /// Create a high-stakes tournament where players wager
        /// </summary>
        public StakeTournamentResult CreateStakeTournament(string tournamentId, 
            long stakeAmount, CurrencyType currency, List<string> playerIds)
        {
            if (playerIds.Count < 2)
            {
                return new StakeTournamentResult { Success = false, Error = "Need at least 2 players" };
            }
            
            // Verify all players can afford stake
            foreach (var playerId in playerIds)
            {
                long balance = EconomyManager.Instance.GetBalance(playerId, currency);
                if (balance < stakeAmount)
                {
                    return new StakeTournamentResult 
                    { 
                        Success = false, 
                        Error = $"Player {playerId} cannot afford stake" 
                    };
                }
            }
            
            // Collect stakes
            foreach (var playerId in playerIds)
            {
                var result = EconomyManager.Instance.DeductCurrency(playerId, currency, stakeAmount,
                    TransactionType.EntryFee, $"Stake tournament wager", tournamentId);
                
                if (!result.Success)
                {
                    // Refund already collected
                    // TODO: Handle partial collection rollback
                    return new StakeTournamentResult { Success = false, Error = result.Error };
                }
            }
            
            long totalPot = stakeAmount * playerIds.Count;
            
            // Set up prize pool (winner takes all by default)
            var pool = new TournamentPrizePool
            {
                TournamentId = tournamentId,
                TotalPool = totalPot,
                BasePrizePool = totalPot,
                Currency = currency,
                IsContributedByEntryFees = false,
                Placements = new List<PrizePlacement>
                {
                    new PrizePlacement
                    {
                        MinPlace = 1,
                        MaxPlace = 1,
                        PoolPercentage = 100f,
                        Title = "Winner Takes All"
                    }
                }
            };
            
            _prizePools[tournamentId] = pool;
            
            return new StakeTournamentResult
            {
                Success = true,
                TotalPot = totalPot,
                StakePerPlayer = stakeAmount,
                PlayerCount = playerIds.Count
            };
        }
        
        #endregion
        
        #region Analytics
        
        /// <summary>
        /// Get tournament economy stats
        /// </summary>
        public TournamentEconomyStats GetTournamentStats(string tournamentId)
        {
            var stats = new TournamentEconomyStats
            {
                TournamentId = tournamentId
            };
            
            if (_entryFees.TryGetValue(tournamentId, out var fee))
            {
                stats.EntryFee = fee.Amount;
                stats.EntryCurrency = fee.Currency;
            }
            
            if (_collectedEntryFees.TryGetValue(tournamentId, out var collected))
            {
                stats.TotalFeesCollected = (long)collected;
            }
            
            if (_registeredPlayers.TryGetValue(tournamentId, out var players))
            {
                stats.RegisteredPlayers = players.Count;
            }
            
            if (_prizePools.TryGetValue(tournamentId, out var pool))
            {
                stats.CurrentPrizePool = pool.TotalPool;
                stats.BasePrizePool = pool.BasePrizePool;
                stats.PrizeCurrency = pool.Currency;
            }
            
            return stats;
        }
        
        #endregion
    }
    
    #region Supporting Types
    
    /// <summary>
    /// Configuration for tournament economy
    /// </summary>
    [Serializable]
    public class TournamentEconomyConfig
    {
        public float DefaultEntryFeeContribution = 80f;
        public bool AllowStakeTournaments = true;
        public long MaxStakeAmount = 10000;
        public float HousePercentage = 5f; // Platform takes 5%
        public bool RefundsEnabled = true;
        public int DefaultRefundHoursBeforeStart = 24;
    }
    
    /// <summary>
    /// Preset economy configuration for a tournament tier
    /// </summary>
    [Serializable]
    public class TournamentEconomyPreset
    {
        public long EntryFee;
        public CurrencyType EntryCurrency;
        public long BasePrize;
        public CurrencyType PrizeCurrency;
        public float EntryFeeContribution;
    }
    
    /// <summary>
    /// Result of entry fee processing
    /// </summary>
    public class EntryFeeResult
    {
        public bool Success;
        public string Error;
        public long AmountPaid;
        public long AmountRefunded;
        public CurrencyType Currency;
    }
    
    /// <summary>
    /// Result of prize distribution
    /// </summary>
    public class PrizeDistributionResult
    {
        public bool Success;
        public string Error;
        public long TotalDistributed;
        public Dictionary<string, long> WinnerPayouts;
    }
    
    /// <summary>
    /// Result of stake tournament creation
    /// </summary>
    public class StakeTournamentResult
    {
        public bool Success;
        public string Error;
        public long TotalPot;
        public long StakePerPlayer;
        public int PlayerCount;
    }
    
    /// <summary>
    /// Statistics for tournament economy
    /// </summary>
    [Serializable]
    public class TournamentEconomyStats
    {
        public string TournamentId;
        public long EntryFee;
        public CurrencyType EntryCurrency;
        public long TotalFeesCollected;
        public int RegisteredPlayers;
        public long BasePrizePool;
        public long CurrentPrizePool;
        public CurrencyType PrizeCurrency;
    }
    
    #endregion
}
