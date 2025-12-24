# Election Empire - Unity Project

## Overview
Election Empire is a darkly satirical political strategy game where players rise from local politics to national leadership. This repository contains the Unity project with Sprint 1-5 fully implemented.

## Project Status

### ✅ Sprint 1: Foundation (Complete)
- GameManager, TimeManager, SaveManager
- Character data structures
- JSON loading system

### ✅ Sprint 2: Character System (Complete)
- 100+ character components in JSON
- Random character generator (3 modes)
- Manual character builder (8-step wizard)
- Character library (save/load/share)
- Reroll system with trait locks

### ✅ Sprint 3: World Generation (Complete)
- Procedural world generator (10 regions, 30-60 states, ~900 districts)
- Voter simulation system
- Interactive map visualization
- Demographics and voter blocs
- World save/load with seed reproduction

### ✅ Sprint 4: AI Opponents (Complete)
- 12 distinct AI archetypes
- Procedural AI generation (name, personality, backstory)
- 8-trait personality matrix + 6 human foibles
- AI decision-making engine
- Situational awareness system
- Dynamic dialogue generation
- Relationship tracking
- Strategy adaptation
- **Human-like unpredictability** (ego, paranoia, hubris, etc.)

### ✅ Sprint 5: Core Gameplay Loop (Complete)
- Complete political ladder (5 tiers, 15+ offices)
- Full resource management (6 resources)
- Complete election system (7 phases)
- Victory/defeat conditions
- Legacy points system
- Game HUD and election UI
- **Fully playable campaign from start to finish**

## Project Structure

```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── GameManager.cs          # Main coordinator
│   │   ├── TimeManager.cs          # Real-time clock
│   │   └── SaveManager.cs          # Save/load
│   ├── Character/
│   │   ├── Character.cs            # Character data
│   │   ├── CharacterGenerator.cs   # Random generator
│   │   ├── CharacterLibrary.cs     # Save/load/share
│   │   └── RerollSystem.cs         # Reroll with locks
│   ├── World/
│   │   ├── WorldGenerator.cs       # World generation
│   │   ├── VoterSimulation.cs      # Polling calculations
│   │   ├── WorldMap.cs             # Interactive map
│   │   └── PlayerState.cs          # Player state
│   ├── AI/
│   │   ├── AIOpponent.cs           # AI data structures
│   │   ├── AIOpponentGenerator.cs  # AI generation
│   │   ├── AIDecisionMaker.cs      # Decision engine
│   │   ├── AIInteractionManager.cs # Dialogue system
│   │   └── AIManager.cs            # AI manager
│   ├── Gameplay/
│   │   ├── Office.cs                # Office definitions
│   │   ├── PoliticalLadder.cs      # Ladder system
│   │   ├── ResourceManager.cs      # Resource system
│   │   ├── ElectionManager.cs      # Election system
│   │   ├── VictoryConditionManager.cs
│   │   ├── DefeatConditionManager.cs
│   │   ├── GameLoop.cs             # Core loop
│   │   └── GameState.cs            # Game state
│   └── UI/
│       ├── MainMenu.cs
│       ├── CharacterCreationFlow.cs
│       ├── GameHUD.cs              # Main HUD
│       ├── ElectionUI.cs           # Election tracker
│       └── VictoryProgressUI.cs    # Victory progress
└── StreamingAssets/
    └── Data/
        └── [JSON data files]
```

## Quick Start

### Generate a Character
```csharp
var generator = new CharacterGenerator();
Character character = generator.GenerateRandom(RandomMode.Chaos);
// Creates: "The Drunk Wrestler with Psychic Powers"
```

### Generate a World
```csharp
var worldGen = new WorldGenerator();
World world = worldGen.GenerateWorld("my-seed");
// Creates: 10 regions, 30-60 states, ~900 districts
```

### Generate AI Opponents
```csharp
var aiGen = new AIOpponentGenerator();
AIOpponent ai = aiGen.GenerateOpponent(1, AIDifficulty.Hard, seed);
// Creates: Unique personality with human foibles
```

### Start Campaign
```csharp
GameManager.Instance.StartNewCampaign(character, worldSeed, 5, AIDifficulty.Normal);
// Starts complete campaign with game loop
```

## Features

### Character System
- Random generation (Balanced, Chaos, Hard modes)
- Manual builder (8-step wizard)
- Character library with sharing
- Reroll with trait locks

### World System
- Procedural generation (seed-based)
- 10 regions with unique profiles
- ~900 districts with demographics
- 12 voter blocs, 20 political issues
- Interactive map with zoom/pan

### AI System
- 12 distinct archetypes
- Personality-driven decisions
- **Human foibles** (ego, paranoia, hubris, pride, etc.)
- **Irrational decisions** (like real historical figures)
- **Off-the-wall dialogue** (10% random comments)
- Situational awareness
- Dynamic dialogue
- Relationship tracking

### Gameplay
- 5-tier political ladder
- 6 resource types with decay/generation
- 7-phase election system
- District-by-district vote calculation
- 6 victory conditions
- 7 defeat conditions
- Legacy points system

## Documentation

- `SETUP.md` - Installation and setup guide
- `SPRINT2_COMPLETE.md` - Character system details
- `SPRINT3_COMPLETE.md` - World generation details
- `SPRINT4_COMPLETE.md` - AI opponents details
- `SPRINT5_COMPLETE.md` - Core gameplay details

## Next Steps

Sprint 6: Scandal Engine
- 40+ scandal templates
- 4-stage evolution
- 8 response types
- Dynamic consequences

## License
[Your License Here]
