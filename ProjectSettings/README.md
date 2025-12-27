# Election Empire - Unity Project Configuration

## Overview

This package contains updated Unity ProjectSettings for **Election Empire**, a darkly satirical political strategy game by Agyeman Enterprises LLC.

**Unity Version:** 6000.3.2f1 (Unity 6)
**Template:** Universal 2D (URP)

---

## Files Included

### 1. ProjectSettings.asset
Core Unity project configuration with the following updates:

| Setting | Old Value | New Value |
|---------|-----------|-----------|
| Company Name | DefaultCompany | Agyeman Enterprises LLC |
| Product Name | Election Empire_As | Election Empire |
| Bundle ID (Standalone) | com.DefaultCompany.2D-URP | com.AgyemanEnterprises.ElectionEmpire |
| Bundle ID (Android) | - | com.AgyemanEnterprises.ElectionEmpire |
| Bundle ID (iOS) | - | com.AgyemanEnterprises.ElectionEmpire |
| Run In Background | Off | On |
| Metro Package Name | Election Empire_As | Election Empire |
| Metro Description | Election Empire_As | Politics is a dirty game. Time to get your hands filthy. A darkly satirical political strategy game. |

### 2. TagManager.asset
Comprehensive tags, layers, and sorting layers for game systems:

#### Tags (20 custom tags)
| Tag | Purpose |
|-----|---------|
| Player | Player character/avatar |
| NPC | AI politicians and characters |
| Opponent | Political opponents |
| Ally | Political allies |
| Staff | Campaign/office staff members |
| VoterBloc | Voter bloc representations |
| Media | Media elements |
| Campaign | Campaign-related objects |
| Scandal | Scandal event objects |
| Crisis | Crisis event objects |
| Interactive | General interactive elements |
| Clickable | Mouse-clickable objects |
| Tooltip | Tooltip trigger objects |
| Portrait | Character portraits |
| Card | UI card elements |
| Button | Custom buttons |
| Panel | UI panels |
| Notification | Notification elements |
| Resource | Resource display objects |
| Achievement | Achievement-related objects |

#### Layers (18 layers)
| Layer # | Name | Purpose |
|---------|------|---------|
| 0 | Default | Default Unity layer |
| 1 | TransparentFX | Transparent effects |
| 2 | Ignore Raycast | Non-raycastable objects |
| 4 | Water | Water effects |
| 5 | UI | Standard UI layer |
| 6 | Characters | Character sprites |
| 7 | Background | Background elements |
| 8 | Midground | Mid-layer elements |
| 9 | Foreground | Foreground elements |
| 10 | Effects | VFX and particles |
| 11 | Interactive | Clickable/interactive objects |
| 12 | Popup | Popup windows |
| 13 | Modal | Modal dialogs |
| 14 | Overlay | Full-screen overlays |
| 15 | Tooltip | Tooltip layer |
| 16 | Notification | Notification overlays |
| 17 | Debug | Debug visualization |

#### Sorting Layers (17 layers)
| Order | Name | Purpose |
|-------|------|---------|
| 0 | Default | Default sorting |
| 1 | Background_Far | Distant backgrounds |
| 2 | Background_Near | Near backgrounds |
| 3 | Environment | Environmental elements |
| 4 | Characters_Back | Characters behind main |
| 5 | Characters | Main character layer |
| 6 | Characters_Front | Characters in front |
| 7 | Foreground | Foreground props |
| 8 | Effects | Visual effects |
| 9 | UI_Background | UI background panels |
| 10 | UI_Main | Main UI elements |
| 11 | UI_Cards | Card-based UI |
| 12 | UI_Popup | Popup windows |
| 13 | UI_Modal | Modal dialogs |
| 14 | UI_Tooltip | Tooltips |
| 15 | UI_Notification | Notifications |
| 16 | UI_Overlay | Full overlays |

### 3. EditorBuildSettings.asset
Recommended scene structure for the game:

#### Core Scenes
- `Assets/Scenes/Core/BootLoader.unity` - Initial loading/splash
- `Assets/Scenes/Core/MainMenu.unity` - Main menu
- `Assets/Scenes/Core/Settings.unity` - Settings screen
- `Assets/Scenes/Core/Credits.unity` - Credits screen

#### Character Creation Scenes
- `Assets/Scenes/CharacterCreation/CharacterCreation.unity` - Main character creation
- `Assets/Scenes/CharacterCreation/BackgroundSelection.unity` - Background selection
- `Assets/Scenes/CharacterCreation/TraitSelection.unity` - Trait selection

#### Gameplay Scenes
- `Assets/Scenes/Gameplay/CampaignHQ.unity` - Campaign headquarters
- `Assets/Scenes/Gameplay/CampaignTrail.unity` - Campaign trail map
- `Assets/Scenes/Gameplay/Governance.unity` - Governance screen
- `Assets/Scenes/Gameplay/PolicyCenter.unity` - Policy management
- `Assets/Scenes/Gameplay/StaffManagement.unity` - Staff management

#### Event Scenes
- `Assets/Scenes/Events/Debate.unity` - Debate gameplay
- `Assets/Scenes/Events/Rally.unity` - Rally events
- `Assets/Scenes/Events/PressConference.unity` - Press conferences
- `Assets/Scenes/Events/Interview.unity` - Interview events
- `Assets/Scenes/Events/Crisis.unity` - Crisis management
- `Assets/Scenes/Events/Scandal.unity` - Scandal response

#### Election Scenes
- `Assets/Scenes/Election/ElectionDay.unity` - Election day
- `Assets/Scenes/Election/Results.unity` - Results display
- `Assets/Scenes/Election/Victory.unity` - Victory screen
- `Assets/Scenes/Election/Defeat.unity` - Defeat screen

#### Legacy Scenes
- `Assets/Scenes/Legacy/LegacyScreen.unity` - Legacy overview
- `Assets/Scenes/Legacy/HallOfFame.unity` - Hall of Fame
- `Assets/Scenes/Legacy/DynastyTree.unity` - Dynasty family tree

#### Tutorial Scenes
- `Assets/Scenes/Tutorial/TutorialIntro.unity` - Introduction tutorial
- `Assets/Scenes/Tutorial/TutorialCampaign.unity` - Campaign tutorial
- `Assets/Scenes/Tutorial/TutorialGovernance.unity` - Governance tutorial

#### Multiplayer Scenes
- `Assets/Scenes/Multiplayer/Lobby.unity` - Multiplayer lobby
- `Assets/Scenes/Multiplayer/Tournament.unity` - Tournament brackets
- `Assets/Scenes/Multiplayer/Matchmaking.unity` - Matchmaking screen

### 4. Editor/ElectionEmpireProjectSetup.cs
Unity Editor script with two menu commands:

- **Tools > Election Empire > Setup Project Structure**
  Creates the complete folder hierarchy for the project

- **Tools > Election Empire > Create Empty Scenes**
  Creates empty placeholder scenes at all specified paths

---

## Installation Instructions

### Method 1: Replace Existing ProjectSettings

1. Close Unity Editor
2. Navigate to your Unity project folder
3. **Backup** your existing `ProjectSettings` folder
4. Copy the contents of this package's `ProjectSettings` folder to replace your existing files
5. Copy the `Editor` folder to your `Assets` folder
6. Open Unity Editor
7. Run **Tools > Election Empire > Setup Project Structure**
8. Run **Tools > Election Empire > Create Empty Scenes**
9. Go to **File > Build Settings** and re-add scenes from the created folders

### Method 2: Selective Import

1. Open Unity Editor
2. Copy only the files you need:
   - `ProjectSettings.asset` - Company/product settings
   - `TagManager.asset` - Tags, layers, sorting layers
   - `EditorBuildSettings.asset` - Build scene list
3. Place files in your project's `ProjectSettings` folder
4. Unity will automatically reload the settings

---

## Post-Installation Checklist

### Immediate
- [ ] Verify company name shows correctly in Player Settings
- [ ] Verify product name shows correctly
- [ ] Check that new tags appear in Inspector dropdowns
- [ ] Check that new layers appear in Layer dropdown
- [ ] Check that sorting layers appear in Sprite Renderer

### Scene Setup
- [ ] Run "Setup Project Structure" from Tools menu
- [ ] Run "Create Empty Scenes" from Tools menu
- [ ] Update Build Settings with actual scene references
- [ ] Set BootLoader as Scene 0 in Build Settings

### Configuration
- [ ] Set up Input Actions for new Input System
- [ ] Configure Audio settings if needed
- [ ] Set up Quality Settings for target platforms
- [ ] Configure URP render pipeline asset

### Art Pipeline
- [ ] Import character portraits to Assets/Art/Characters/Portraits
- [ ] Import UI kit to Assets/Art/UI
- [ ] Import icons to Assets/Art/Icons
- [ ] Set up sprite atlases for performance

---

## Layer Usage Guide

### Rendering Order
For 2D sprite rendering, use Sorting Layers (not physics Layers):

```
Background_Far (furthest back)
  ↓
Background_Near
  ↓
Environment
  ↓
Characters_Back → Characters → Characters_Front
  ↓
Foreground
  ↓
Effects
  ↓
UI_Background → UI_Main → UI_Cards → UI_Popup → UI_Modal → UI_Tooltip → UI_Notification → UI_Overlay (closest)
```

### Physics/Collision Layers
Use these for camera culling and raycasting:

- **Characters** (6) - For character collision detection
- **Interactive** (11) - For mouse/touch raycasting
- **UI** (5) - For UI raycasting
- **Debug** (17) - Enable/disable in camera culling mask

### Recommended Camera Layer Masks
```
Main Camera: Everything except UI, Debug
UI Camera: UI only
Debug Camera: Debug only (editor/dev builds)
```

---

## Tag Usage Guide

### Character Tags
```csharp
// Finding player character
GameObject player = GameObject.FindGameObjectWithTag("Player");

// Finding all opponents
GameObject[] opponents = GameObject.FindGameObjectsWithTag("Opponent");

// Finding all staff
GameObject[] staff = GameObject.FindGameObjectsWithTag("Staff");
```

### UI Tags
```csharp
// Finding interactive elements
if (hitObject.CompareTag("Clickable") || hitObject.CompareTag("Interactive"))
{
    // Handle click
}

// Finding tooltip triggers
if (hitObject.CompareTag("Tooltip"))
{
    ShowTooltip(hitObject);
}
```

---

## Scene Workflow

### Recommended Scene Loading Pattern
```csharp
// Boot sequence
1. BootLoader → Initializes systems, shows splash
2. MainMenu → Player selects new game, continue, settings
3. CharacterCreation → New game character setup
4. CampaignHQ → Main gameplay hub

// Gameplay loop
CampaignHQ ↔ CampaignTrail ↔ Events (Debate, Rally, etc.)
     ↓
Governance ↔ PolicyCenter ↔ StaffManagement
     ↓
ElectionDay → Results → Victory/Defeat
     ↓
LegacyScreen → MainMenu (new run) or DynastyTree (continue dynasty)
```

### Additive Scene Loading
Consider loading these additively:
- UI overlay scenes
- Audio manager scene
- Notification system scene
- Tutorial overlays

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-12-26 | Initial configuration package |

---

## Support

For questions about this configuration:
- Review the Election Empire Developer Specification document
- Check Unity documentation for ProjectSettings format
- Contact development team for game-specific questions

---

*Election Empire © 2025 Agyeman Enterprises LLC. All rights reserved.*
*"Politics is a dirty game. Time to get your hands filthy."*
