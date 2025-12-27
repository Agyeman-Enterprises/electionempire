# Election Empire Scene Setup Guide

## Quick Start (15-30 minutes)

### Prerequisites
- Unity 2023.2+ installed
- Election Empire codebase imported
- TextMeshPro package installed (Unity will prompt on first use)

---

## Step 1: Create Scenes Folder

```
Assets/
└── Scenes/
    ├── MainMenu.unity
    └── Game.unity
```

---

## Step 2: Use the Scene Setup Wizard (Recommended)

1. Open Unity
2. Go to **Tools > Election Empire > Scene Setup Wizard**
3. Click **"Create MainMenu Scene"**
4. Click **"Create Game Scene"**

This auto-creates both scenes with proper structure.

---

## Step 3: Manual Scene Setup (If Not Using Wizard)

### MainMenu Scene

1. Create new scene: **File > New Scene > Empty**
2. Save as `Assets/Scenes/MainMenu.unity`

3. Create these GameObjects:

```
Hierarchy:
├── Main Camera (Camera, AudioListener)
├── [SceneBootstrapper] (SceneBootstrapper.cs)
├── EventSystem (EventSystem, StandaloneInputModule)
└── MainMenuCanvas (Canvas, CanvasScaler, GraphicRaycaster, PanelManager.cs, SimpleMainMenu.cs)
    ├── MainMenuPanel (Image, CanvasGroup)
    │   ├── TitleText (TextMeshProUGUI)
    │   ├── NewCampaignButton (Button, Image)
    │   ├── ContinueButton (Button, Image)
    │   ├── LoadGameButton (Button, Image)
    │   ├── SettingsButton (Button, Image)
    │   └── QuitButton (Button, Image)
    ├── CharacterCreationPanel (Image, CanvasGroup) [inactive]
    ├── LoadGamePanel (Image, CanvasGroup) [inactive]
    └── SettingsPanel (Image, CanvasGroup) [inactive]
```

4. Configure PanelManager:
   - Add each panel to the Panels list
   - Set panel IDs: "MainMenu", "CharacterCreation", "LoadGame", "Settings"
   - Set Default Panel ID to "MainMenu"

5. Configure SimpleMainMenu:
   - Assign button references
   - Set panel IDs to match PanelManager

### Game Scene

1. Create new scene: **File > New Scene > Empty**
2. Save as `Assets/Scenes/Game.unity`

3. Create these GameObjects:

```
Hierarchy:
├── Main Camera (Camera, AudioListener)
├── [GameController] (GameSceneInitializer.cs)
├── [Managers]
│   ├── TimeManager
│   ├── AIManager
│   ├── ScandalManager
│   ├── CrisisManager
│   ├── ElectionManager
│   ├── ResourceManager
│   ├── MediaManager
│   └── NewsEventManager
├── EventSystem (EventSystem, StandaloneInputModule)
└── GameCanvas (Canvas, CanvasScaler, GraphicRaycaster, PanelManager.cs)
    ├── GameHUDPanel (Image, CanvasGroup)
    ├── PauseMenuPanel (Image, CanvasGroup) [inactive]
    ├── EventPopupPanel (Image, CanvasGroup) [inactive]
    ├── ScandalResponsePanel (Image, CanvasGroup) [inactive]
    └── ElectionNightPanel (Image, CanvasGroup) [inactive]
```

---

## Step 4: Configure Build Settings

1. Open **File > Build Settings**
2. Click "Add Open Scenes" for each scene, or drag scenes from Project
3. Ensure order is:
   - **Index 0**: MainMenu
   - **Index 1**: Game

---

## Step 5: Wire Up References

### SceneBootstrapper (MainMenu scene)
| Field | Value |
|-------|-------|
| Main Menu Scene Name | "MainMenu" |
| Character Creation Scene Name | "CharacterCreation" (or "MainMenu" if using panels) |
| Game Scene Name | "Game" |

### PanelManager (Both scenes)
For each panel:
1. Click "+" to add a panel config
2. Set Panel ID (e.g., "MainMenu")
3. Drag the panel GameObject to Panel Object field
4. CanvasGroup auto-assigns if exists

### SimpleMainMenu (MainMenu scene)
- Drag each button to its field
- Ensure Panel IDs match PanelManager config

### GameSceneInitializer (Game scene)
- Drag manager GameObjects to their fields
- Or leave null for auto-find (slower but works)

---

## Step 6: Test Scene Transitions

1. Play the MainMenu scene
2. Click "New Campaign"
3. Should transition to Character Creation panel (or scene)
4. Complete character creation
5. Should transition to Game scene
6. Press Escape to pause
7. Click "Main Menu" to return

---

## Common Issues & Solutions

### Issue: "Scene not found" error
**Solution**: Add scene to Build Settings

### Issue: "Button does nothing"
**Solution**: Check button reference is assigned in SimpleMainMenu inspector

### Issue: "Panel doesn't appear"
**Solution**: 
1. Check panel is in PanelManager's list
2. Check Panel ID matches what you're calling
3. Check panel has CanvasGroup component

### Issue: "Managers not initializing"
**Solution**: Check GameSceneInitializer has references or managers have correct class names

### Issue: "DontDestroyOnLoad objects duplicating"
**Solution**: SceneBootstrapper has singleton protection - only first instance survives

---

## Testing Checklist

### MainMenu Scene
- [ ] Scene loads without errors
- [ ] Title/buttons visible
- [ ] New Campaign button works
- [ ] Settings panel opens
- [ ] Quit button works (in build)
- [ ] Panel transitions animate smoothly

### Game Scene
- [ ] Scene loads without errors
- [ ] HUD displays
- [ ] Pause menu opens on Escape
- [ ] Return to Main Menu works
- [ ] Managers initialize (check console logs)

### Scene Transitions
- [ ] MainMenu → Game works
- [ ] Game → MainMenu works
- [ ] No duplicate persistent objects

---

## File Reference

| File | Purpose | Scene |
|------|---------|-------|
| `SceneBootstrapper.cs` | Persistent manager initialization, scene loading | MainMenu |
| `PanelManager.cs` | UI panel transitions with animations | Both |
| `SimpleMainMenu.cs` | Main menu button handling | MainMenu |
| `GameSceneInitializer.cs` | Game manager initialization | Game |
| `SceneSetupWizard.cs` | Editor tool for quick scene creation | Editor only |

---

## Next Steps After Scene Setup

1. **Add your existing manager scripts** to the manager GameObjects
2. **Create UI prefabs** for consistent styling
3. **Add audio** - create AudioManager and assign clips
4. **Add visual assets** - character portraits, backgrounds
5. **Test full game loop** - create character, play, save, load

---

## Notes for Integration

If you already have manager implementations from previous sessions:

1. Remove the stub classes at the bottom of `GameSceneInitializer.cs`
2. Add `#define ELECTION_EMPIRE_MANAGERS_DEFINED` at top of that file
3. Attach your real manager scripts to the manager GameObjects
4. Update `GameSceneInitializer` to call your managers' initialization methods

The scene setup scripts are designed to work with placeholder managers initially, then integrate with your full implementations.
