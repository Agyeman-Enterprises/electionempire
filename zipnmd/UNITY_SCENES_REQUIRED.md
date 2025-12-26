# UNITY SCENES REQUIRED - ELECTION EMPIRE

## 📋 COMPLETE SCENE LIST

### ✅ **CRITICAL SCENES** (Required for Game to Run)

#### 1. **MainMenu** 
**File Name**: `MainMenu.unity`  
**Script**: `Assets/Scripts/UI/MainMenu.cs`  
**Purpose**: Entry point of the game  
**Contains**:
- Main menu buttons (New Campaign, Load Game, Character Library, Quit)
- Panels for Character Creation, Load Game, Character Library (can be in same scene or separate)
- UIManager component
- GameManagerIntegration component (DontDestroyOnLoad)

**Scene Setup**:
- Add `MainMenu.cs` script to a GameObject
- Add `UIManager.cs` script to a GameObject (DontDestroyOnLoad)
- Add `GameManagerIntegration.cs` script to a GameObject (DontDestroyOnLoad)
- Create UI Canvas with buttons
- Assign button references to MainMenu script

---

#### 2. **CharacterCreation**
**File Name**: `CharacterCreation.unity`  
**Script**: `Assets/Scripts/UI/CharacterCreationFlow.cs`  
**Purpose**: Character creation/selection  
**Contains**:
- Mode selection (Random, Build, Library)
- Random generation panel
- Manual builder panel
- Character library panel
- Character display components

**Scene Setup**:
- Add `CharacterCreationFlow.cs` script
- Create UI panels for each mode
- Add CharacterGenerator, RerollSystem components
- Connect to GameManager for starting campaigns

**Note**: Can be a panel in MainMenu scene instead of separate scene

---

#### 3. **Game**
**File Name**: `Game.unity`  
**Script**: `Assets/Scripts/Gameplay/GameLoop.cs`  
**Purpose**: Main gameplay scene  
**Contains**:
- GameLoop component
- GameManager component
- TimeManager component
- AIManager component
- ResourceManager (runtime)
- ElectionManager (runtime)
- ScandalManager component
- NewsEventManager component
- GameHUD UI
- All gameplay UI elements

**Scene Setup**:
- Add `GameLoop.cs` to a GameObject
- Add `GameManager.cs` to a GameObject (DontDestroyOnLoad)
- Add `TimeManager.cs` to a GameObject
- Add `AIManager.cs` to a GameObject
- Add `ScandalManager.cs` to a GameObject
- Add `NewsEventManager.cs` to a GameObject
- Create GameHUD Canvas with all resource displays
- Add ElectionUI, VictoryProgressUI components

**Critical**: This is the main gameplay scene where all game logic runs

---

### ⚠️ **OPTIONAL SCENES** (Can be UI Panels Instead)

#### 4. **Settings**
**File Name**: `Settings.unity` (Optional - can be panel)  
**Script**: `Assets/Scripts/UI/Screens/MainMenuScreens.cs` (SettingsScreen class)  
**Purpose**: Game settings (audio, graphics, gameplay)  
**Note**: Can be a panel in MainMenu scene

---

#### 5. **Credits**
**File Name**: `Credits.unity` (Optional - can be panel)  
**Purpose**: Credits screen  
**Note**: Can be a panel in MainMenu scene

---

#### 6. **MultiplayerLobby**
**File Name**: `MultiplayerLobby.unity` (Optional - multiplayer feature)  
**Script**: `Assets/Scripts/Multiplayer/MultiplayerClient.cs`  
**Purpose**: Multiplayer lobby  
**Note**: Only needed if multiplayer is enabled

---

### 🎮 **GAMEPLAY SCENES** (Can be UI Overlays)

#### 7. **PauseMenu**
**File Name**: `PauseMenu.unity` (Optional - can be overlay)  
**Purpose**: Pause menu overlay  
**Note**: Usually an overlay panel in Game scene, not separate scene

---

#### 8. **EventPopup**
**File Name**: `EventPopup.unity` (Optional - can be overlay)  
**Purpose**: Event popup dialogs  
**Note**: Usually an overlay panel in Game scene

---

#### 9. **CrisisManagement**
**File Name**: `CrisisManagement.unity` (Optional - can be overlay)  
**Purpose**: Crisis management interface  
**Note**: Usually an overlay panel in Game scene

---

#### 10. **ScandalResponse**
**File Name**: `ScandalResponse.unity` (Optional - can be overlay)  
**Purpose**: Scandal response interface  
**Note**: Usually an overlay panel in Game scene

---

#### 11. **ElectionNight**
**File Name**: `ElectionNight.unity` (Optional - can be overlay)  
**Purpose**: Election night results screen  
**Note**: Usually an overlay panel in Game scene

---

#### 12. **VictoryScreen**
**File Name**: `VictoryScreen.unity` (Optional - can be overlay)  
**Purpose**: Victory screen  
**Note**: Usually an overlay panel in Game scene

---

#### 13. **DefeatScreen**
**File Name**: `DefeatScreen.unity` (Optional - can be overlay)  
**Purpose**: Defeat screen  
**Note**: Usually an overlay panel in Game scene

---

#### 14. **DebateArena**
**File Name**: `DebateArena.unity` (Optional - can be overlay)  
**Purpose**: Debate interface  
**Note**: Usually an overlay panel in Game scene

---

## 🎯 **MINIMUM REQUIRED SCENES** (For Basic Functionality)

### **Option 1: Separate Scenes** (Recommended for Organization)
1. ✅ **MainMenu.unity** - Main menu
2. ✅ **CharacterCreation.unity** - Character creation
3. ✅ **Game.unity** - Main gameplay

### **Option 2: Single Scene with Panels** (Simpler Setup)
1. ✅ **MainMenu.unity** - Contains all menu panels
2. ✅ **Game.unity** - Contains all gameplay and overlays

---

## 📝 **SCENE SETUP CHECKLIST**

### MainMenu Scene:
- [ ] Create scene `MainMenu.unity`
- [ ] Add `MainMenu.cs` script to GameObject
- [ ] Add `UIManager.cs` script to GameObject (DontDestroyOnLoad)
- [ ] Add `GameManagerIntegration.cs` script to GameObject (DontDestroyOnLoad)
- [ ] Create Canvas for UI
- [ ] Create buttons: NewCampaignButton, LoadGameButton, CharacterLibraryButton, QuitButton
- [ ] Create panels: CharacterCreationPanel, LoadGamePanel, CharacterLibraryPanel
- [ ] Assign all references in MainMenu inspector
- [ ] Set as first scene in Build Settings

### CharacterCreation Scene (if separate):
- [ ] Create scene `CharacterCreation.unity`
- [ ] Add `CharacterCreationFlow.cs` script
- [ ] Create UI panels for mode selection
- [ ] Create panels for Random, Build, Library modes
- [ ] Add CharacterDisplay components
- [ ] Connect to GameManager
- [ ] Add to Build Settings

### Game Scene:
- [ ] Create scene `Game.unity`
- [ ] Add `GameLoop.cs` script to GameObject
- [ ] Add `GameManager.cs` script to GameObject (DontDestroyOnLoad)
- [ ] Add `TimeManager.cs` script to GameObject
- [ ] Add `AIManager.cs` script to GameObject
- [ ] Add `ScandalManager.cs` script to GameObject
- [ ] Add `NewsEventManager.cs` script to GameObject
- [ ] Create GameHUD Canvas
- [ ] Add GameHUD, ElectionUI, VictoryProgressUI components
- [ ] Create overlay panels for PauseMenu, EventPopup, etc.
- [ ] Add to Build Settings

---

## 🔧 **BUILD SETTINGS CONFIGURATION**

### Scene Order (in Build Settings):
1. **MainMenu** (Index 0 - First scene)
2. **CharacterCreation** (Index 1 - Optional if separate)
3. **Game** (Index 2 - Main gameplay)

### DontDestroyOnLoad Objects:
- GameManager
- GameManagerIntegration
- UIManager
- TimeManager
- Any persistent managers

---

## 📊 **SCENE REFERENCE SUMMARY**

### Direct Scene References in Code:
```csharp
// MainMenu.cs
SceneManager.LoadScene("CharacterCreation");  // Line 53
SceneManager.LoadScene("Game");               // Line 67

// CharacterCreationFlow.cs
SceneManager.LoadScene("Game");                // Line 222
```

### ScreenType Enum (UI Panels - can be in same scene):
- MainMenu
- NewGame
- LoadGame
- MultiplayerLobby
- Settings
- Credits
- GameHUD
- CharacterCreation
- PauseMenu
- EventPopup
- CrisisManagement
- ScandalResponse
- ElectionNight
- VictoryScreen
- DefeatScreen
- DebateArena

---

## ✅ **FINAL RECOMMENDATION**

### **Minimum Setup** (3 Scenes):
1. **MainMenu.unity** - All menu screens as panels
2. **CharacterCreation.unity** - Character creation (or panel in MainMenu)
3. **Game.unity** - All gameplay with overlay panels

### **Full Setup** (14+ Scenes):
- Separate scenes for each major screen
- Better organization but more complex
- Recommended for larger teams

### **Hybrid Approach** (Recommended):
- **MainMenu.unity** - Menu screens as panels
- **Game.unity** - Gameplay with overlay panels
- Separate scenes only for major transitions

---

## 🚀 **QUICK START**

1. Create `MainMenu.unity` scene
2. Create `Game.unity` scene
3. Add scenes to Build Settings (MainMenu first)
4. Set up GameManagerIntegration in MainMenu (DontDestroyOnLoad)
5. Test scene transitions

**All other screens can be UI panels within these scenes!**

---

*Last Updated: December 2024*


