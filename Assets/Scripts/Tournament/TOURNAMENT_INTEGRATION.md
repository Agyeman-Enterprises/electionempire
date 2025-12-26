# TOURNAMENT SYSTEM - INTEGRATION GUIDE

## Overview

The Tournament System is fully integrated with the Election Empire currency and reward systems.

## Files Created

1. **TournamentTypes.cs** - All enums and data structures
2. **TournamentManager.cs** - Main tournament orchestration
3. **TournamentSupportingClasses.cs** - BracketGenerator and RankingSystem

## Integration Points

### ✅ Currency System Integration

**Connected to:** `CurrencyManager` from `ElectionEmpire.Monetization`

**How it works:**
- Tournament rewards automatically credit `CloutBux` to players
- Rewards are distributed when tournament completes
- Currency is awarded via `CurrencyManager.Credit()` method
- Transaction history tracks tournament rewards

**Reward Types:**
- `RewardType.Currency` → Awards CloutBux
- `RewardType.SeasonPoints` → Tracked by RankingSystem
- `RewardType.Title/Cosmetic/Badge` → Added to PlayerInventory (when integrated)

### ✅ GameManagerIntegration Integration

**Connected to:** `GameManagerIntegration.Instance.CurrencyManager`

**Initialization:**
```csharp
// TournamentManager automatically finds CurrencyManager
TournamentManager.Instance.Initialize();

// Or explicitly provide it
TournamentManager.Instance.Initialize(config, currencyManager);
```

### ✅ Currency Reward Events

Tournament rewards are added to CurrencyManager's reward calculation:

**CloutBux Rewards:**
- `tournament_win_community` → 200 CB
- `tournament_win_weekly` → 500 CB
- `tournament_win_monthly` → 1000 CB
- `tournament_win_seasonal` → 2500 CB
- `tournament_win_championship` → 5000 CB
- `tournament_win_official` → 10000 CB
- `tournament_placement_2nd` → 100 CB
- `tournament_placement_3rd` → 50 CB
- `tournament_placement_top8` → 25 CB
- `tournament_participation` → 10 CB

**Purrkoin Rewards:**
- `tournament_champion_weekly` → 10 PK
- `tournament_champion_monthly` → 25 PK
- `tournament_champion_seasonal` → 50 PK
- `tournament_champion_championship` → 100 PK
- `tournament_champion_official` → 200 PK

## Usage Example

```csharp
// Create tournament
var tournament = TournamentManager.Instance.CreateTournament(
    name: "Weekly Championship",
    hostId: "player123",
    hostName: "Player Name",
    format: TournamentFormat.SingleElimination,
    tier: TournamentTier.Weekly,
    maxPlayers: 16
);

// Register players
TournamentManager.Instance.RegisterPlayer(
    tournament.Id, 
    "player456", 
    "Player 2",
    RankTier.Gold,
    1500
);

// Start tournament
TournamentManager.Instance.StartTournament(tournament.Id);

// Report match result
TournamentManager.Instance.ReportMatchResult(
    tournament.Id,
    matchId,
    winnerId: "player123",
    player1Score: 52,
    player2Score: 48,
    player1Votes: 52f,
    player2Votes: 48f
);

// When tournament completes, rewards are automatically distributed
// CurrencyManager will credit CloutBux to winners
```

## Event Flow

1. **Tournament Created** → `OnTournamentCreated` event
2. **Player Registered** → `OnPlayerRegistered` event
3. **Tournament Started** → `OnTournamentStatusChanged` event
4. **Round Started** → `OnRoundStarted` event
5. **Match Completed** → `OnMatchCompleted` event
6. **Round Completed** → `OnRoundCompleted` event
7. **Tournament Completed** → `OnTournamentCompleted` event
   - **Rewards Distributed** → CurrencyManager credits CloutBux
   - **Player Histories Updated**
   - **Rankings Updated** (if ranked)

## Reward Distribution

When a tournament completes:

1. Final positions are assigned
2. Rewards are matched to placements
3. `DistributeRewardsToPlayer()` is called for each participant
4. Currency rewards are credited via `CurrencyManager.Credit()`
5. Transaction is logged with source: `tournament_{tournamentId}`

## Dependencies

### Required:
- ✅ `ElectionEmpire.Monetization.CurrencyManager`
- ✅ `ElectionEmpire.Core.GameManagerIntegration` (optional, for auto-discovery)

### Optional (for full features):
- `PlayerInventory` - For cosmetic/title rewards
- `AchievementManager` - For tournament achievement unlocks
- Network system - For multiplayer tournaments

## Status

✅ **Fully Integrated**
- Currency rewards working
- Event system complete
- All dependencies satisfied
- No compilation errors

## Next Steps (Optional)

1. **UI Integration** - Create tournament lobby UI
2. **Network Integration** - Multiplayer tournament support
3. **PlayerInventory Integration** - For cosmetic rewards
4. **Achievement Integration** - Tournament-specific achievements
5. **Save/Load** - Persist tournament state

