# WORLD SERVER & GHOST AI SYSTEM

## Overview

The World Server & Ghost AI System creates **persistent political worlds** where players' actions accumulate over time, and AI "ghosts" play like the original players did.

---

## 🌍 WORLD SERVER SYSTEM

### Core Concept

**Multiple Persistent Worlds:**
- Each world accumulates history from all players
- Players can join existing worlds or create new ones
- Worlds evolve continuously, even when players are offline
- Ghost AI plays for players when they're away

### WorldServerManager

**Key Features:**
- **World Matching** - Finds or creates worlds based on player preferences
- **Player Connection** - Manages player joining/leaving worlds
- **Ghost Control** - Handles transition between player control and ghost AI
- **World State Sync** - Syncs local state with server (or local simulation)

**World Matching Criteria:**
- `PreferNewWorld` - Want a fresh start
- `WantCompetition` - Want active competition
- `WantEasyStart` - Want easier starting conditions
- `DesiredGhostCount` - How many ghosts to seed
- `MaxPolarization` - Maximum world polarization
- `MaxCorruption` - Maximum world corruption

### ExtendedWorldState

**Properties:**
- `WorldId` - Unique world identifier
- `YearInWorld` - Current year in the world
- `TotalPlayersContributed` - How many players have played
- `CurrentPresidentId` - Who is currently president
- `PresidentIsPlayer` - Is president a player or AI?
- `ActivePoliticians` - List of all active ghosts/players
- `PresidentialHistory` - Historical presidents
- `CorruptionLevel` - World corruption (0-100)
- `OverallPolarization` - Political polarization (0-100)
- `MediaTrust` - Media credibility (0-100)
- `DemocraticNorms` - Democratic health (0-100)
- `IdeologicalBalance` - Economic/social/foreign policy balance
- `InstitutionalHealth` - Congress/courts/media/military health

---

## 👻 GHOST AI SYSTEM

### Core Concept

**AI That Plays Like You:**
- Ghosts learn from player behavior profiles
- They make decisions like the original player would
- They have the same weaknesses and strengths
- They use the same tactics and catchphrases

### GhostAISystem

**Key Features:**
- **Behavior-Based Decisions** - Uses player behavior profiles
- **Situation Analysis** - Analyzes what situation ghost is in
- **Action Execution** - Executes ghost actions
- **Decision Tracking** - Records decisions for analysis

**Decision Process:**
1. Analyze situation (election coming? under attack? opportunity?)
2. Check behavior profile for learned responses
3. Determine action based on profile
4. Add random variation (20% chance)
5. Execute action
6. Record decision

### GhostAIBehavior

**Properties:**
- `Aggression` - How aggressive (0-100)
- `Defensiveness` - How defensive (0-100)
- `OpportunismLevel` - How opportunistic (0-100)
- `CrisisCompetence` - Crisis handling ability (0-100)
- `WillAttackFirst` - Will initiate attacks?
- `WillFormAlliances` - Will form alliances?
- `WillBetrayAlliances` - Will betray allies?
- `WillUseDirtyTricks` - Will use dirty tactics?

**Generated From:**
- Player behavior profile
- Action frequency patterns
- Response patterns
- Risk tolerance
- Dirty tricks willingness

### Ghost Action Types

**Maintenance:**
- `MaintainPosition` - Hold current position
- `BuildSupport` - Increase approval
- `BuildAlliances` - Form alliances

**Advancement:**
- `SeekHigherOffice` - Try to advance
- `SeekOpportunity` - Look for openings

**Campaign:**
- `SafeCampaign` - Conservative campaign
- `AggressiveCampaign` - Risky campaign

**Combat:**
- `AttackOpponent` - Attack another politician
- `CounterAttack` - Respond to attack
- `Defend` - Defensive stance

**Crisis:**
- `HandleCrisis` - Competent crisis handling
- `DeflectBlame` - Risky deflection

**Response:**
- `Deny` - Deny allegations
- `Apologize` - Apologize
- `Spin` - Spin the narrative

**Dirty:**
- `UseDirtyTrick` - Use dirty tactics
- `PlantScandal` - Plant scandal on opponent
- `BribeOfficial` - Bribe official

---

## 🤝 PLAYER-GHOST INTERACTION

### Encounter System

**When Players Meet Ghosts:**
- Generate dialogue based on ghost's behavior profile
- Use original player's catchphrases
- Determine ghost's reaction (friendly, hostile, etc.)
- Special case: Meeting your own ghost

### Challenge System

**Challenging Ghosts:**
- Ghost responds based on behavior profile
- Aggressive ghosts counter-attack
- Defensive ghosts dig in
- Calm ghosts accept calmly

### History System

**Viewing Ghost History:**
- See what the original player did
- Discover exploitable weaknesses
- Learn preferred tactics
- Find panic threshold
- Check if they'll betray

---

## 🔄 SYSTEM FLOW

### Player Joins World

```
1. Player requests world (with criteria)
2. WorldServerManager finds/creates world
3. Player joins world
4. Check if player has existing ghost
5. If yes: Resume as ghost
6. If no: Start as new challenger
7. World state synced
```

### Player Leaves World

```
1. Player leaves world
2. If player has ghost: Ghost AI takes over
3. Ghost continues playing
4. World state saved
5. Player can return later
```

### Ghost Turn Processing

```
1. GhostAISystem analyzes situation
2. Checks behavior profile
3. Determines action
4. Adds randomness
5. Executes action
6. Updates world state
7. Records decision
```

### Player Encounters Ghost

```
1. PlayerGhostInteraction.ProcessGhostEncounter()
2. Generate dialogue from profile
3. Determine ghost reaction
4. Check if own ghost
5. Return encounter result
```

---

## 📊 DATA STRUCTURES

### ExtendedGhostPolitician

**Extends:** `GhostPolitician`

**Additional Properties:**
- `CurrentOffice` - Office tier (1-5)
- `SpecificOffice` - Office name
- `IsActivelyControlled` - Is player controlling?
- `LastPlayerControl` - When player last controlled
- `CurrentApproval` - Current approval rating
- `AIBehavior` - AI behavior parameters
- `OriginalPlayerId` - Original player ID
- `ActiveScandals` - Current scandals

### HistoricalPresident

**Properties:**
- `Id` - President ID
- `Name` - President name
- `YearElected` - Election year
- `AverageApproval` - Average approval
- `LegacyReputation` - How they're remembered
- `StillAlive` - Is still alive?
- `OngoingInfluence` - Current influence (0-100)

### ActiveChallenger

**Properties:**
- `ChallengerId` - Challenger ID
- `ChallengerName` - Challenger name
- `OfficeTier` - Office tier
- `ThreatLevel` - Threat level (0-100)
- `ThreatType` - Type of threat
- `ThreatStartTime` - When threat started

---

## 🎮 GAMEPLAY IMPLICATIONS

### For Players

**Joining a World:**
- Choose world based on criteria
- See world history before joining
- Encounter ghosts from past players
- Build on existing world state

**Playing in World:**
- Your actions affect the world
- Ghosts play when you're away
- Return to see what happened
- Face challenges from ghosts

**Leaving World:**
- Ghost takes over
- Continues playing like you
- Other players face your ghost
- Can return anytime

### For Ghosts

**Autonomous Play:**
- Make decisions based on profile
- Use original player's tactics
- Have same weaknesses
- Can advance or fail

**Player Encounters:**
- React based on behavior
- Use original catchphrases
- Show exploitable patterns
- Can be studied and defeated

---

## 🔗 INTEGRATION POINTS

### With Existing Systems

1. **PersistentWorldManager** - World state management
2. **GhostManager** - Ghost creation and management
3. **BehaviorTracker** - Player behavior tracking
4. **PlayerBehaviorProfile** - Behavior profile storage
5. **Campaign System** - Ghosts appear as opponents
6. **Election System** - Ghosts can run for office

### System Dependencies

- `GhostPolitician` (base class)
- `PlayerBehaviorProfile` (behavior data)
- `GhostAIBehavior` (AI parameters)
- `PersistentWorldState` (world state)

---

## 🚀 FUTURE ENHANCEMENTS

1. **Server Integration** - Real server sync
2. **Cross-World Ghosts** - Ghosts appear in multiple worlds
3. **Ghost Evolution** - Ghosts learn and adapt
4. **Player Reputation** - Track player reputation across worlds
5. **World Rankings** - Rank worlds by activity/history
6. **Ghost Tournaments** - Ghosts compete in tournaments

---

## 📝 SUMMARY

The World Server & Ghost AI System creates a **living, breathing political universe** where:

- **Multiple worlds** accumulate history
- **Ghosts play like you** when you're away
- **Players encounter** past players' ghosts
- **Worlds evolve** continuously
- **History matters** - every action shapes the future

**Your game becomes part of someone else's challenge.**

