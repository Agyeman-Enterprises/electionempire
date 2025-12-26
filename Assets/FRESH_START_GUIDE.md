# Election Empire - Fresh Start Guide

## ✅ Project Cleanup Complete

Your project has been prepared for a fresh Unity start. All necessary files are in place and the SceneSetupWizard is ready to use.

## 📋 What Was Done

### 1. **File Organization**
- All scripts moved to proper folders matching their namespaces
- Orphaned .meta files removed
- Duplicate files cleaned up

### 2. **Scene Setup Wizard Enhanced**
- ✅ Menu path fixed: **Tools > Election Empire > Scene Setup Wizard**
- ✅ Automatically adds required scripts to GameObjects:
  - MainMenu scene: `SceneBootstrapper`, `PanelManager`, `SimpleMainMenu`
  - Game scene: `GameSceneInitializer`, `PanelManager`
- ✅ Warns before overwriting existing scenes
- ✅ Creates proper scene structure with all UI panels

### 3. **Existing Scenes Backed Up**
- Old `MainMenu.unity` → `Assets/Scenes_Backup/MainMenu.unity.backup`
- Old `Game.unity` → `Assets/Scenes_Backup/Game.unity.backup`
- Scenes folder is now empty and ready for fresh creation

## 🚀 How to Start Fresh in Unity

### ⚡ **FASTEST METHOD: Auto Setup (Recommended)**

1. **Open Unity Project**
   - Open Unity Hub
   - Open the Election Empire project (now at `D:\DEV\ElectionEmpire`)
   - Wait for Unity to import all assets (may take a few minutes)

2. **Run Auto Setup**
   - In Unity Editor, go to: **Tools > Election Empire > Auto Setup Complete Project**
   - Click **"Yes, Setup Everything"** in the dialog
   - Wait for the progress bar to complete (30-60 seconds)
   - Done! Everything is configured automatically

The auto setup will:
- ✅ Backup existing scenes
- ✅ Create MainMenu scene with all UI panels
- ✅ Create Game scene with all managers
- ✅ Configure PanelManager with all panels
- ✅ Wire up all button references
- ✅ Add scenes to Build Settings in correct order
- ✅ Configure all components automatically

### 📝 **Manual Method: Step-by-Step Wizard**

If you prefer manual control:

1. In Unity Editor, go to: **Tools > Election Empire > Scene Setup Wizard**
2. Click **"Create MainMenu Scene"** button
3. Click **"Create Game Scene"** button
4. Manually add scenes to Build Settings
5. Manually configure PanelManager and button references

### Step 6: Configure Scripts (Optional)
The wizard has already added the scripts, but you may need to:
1. Open MainMenu scene
2. Select the Canvas GameObject
3. In Inspector, configure `PanelManager`:
   - Add panels to the list
   - Set panel IDs: "MainMenu", "CharacterCreation", "LoadGame", "Settings"
   - Set Default Panel ID to "MainMenu"
4. Configure `SimpleMainMenu`:
   - Assign button references (drag buttons from hierarchy)
   - Set panel IDs to match PanelManager

### Step 7: Test
1. Open MainMenu scene
2. Press Play
3. You should see the main menu with buttons
4. Click "New Campaign" to test navigation

## 📁 Project Structure

```
Assets/
├── Scenes/                    ← Empty, ready for wizard
├── Scenes_Backup/            ← Old scenes (can delete later)
├── Scripts/
│   ├── Core/                 ← CoreTypes, GameStateManager, SceneBootstrapper
│   ├── Editor/               ← SceneSetupWizard (Editor only)
│   ├── Finance/              ← CampaignFinanceSystem
│   ├── Integration/           ← StaffFinanceIntegration
│   ├── Monetization/          ← EconomyManager, EconomyTypes
│   ├── Staff/                 ← StaffSystem, StaffPersonality
│   ├── Tournament/            ← TournamentManager, TournamentEconomyManager
│   ├── UI/                    ← PanelManager, SimpleMainMenu, StoreUIController
│   └── ... (other folders)
└── ...
```

## ⚠️ Important Notes

1. **TextMeshPro**: Unity may prompt you to import TextMeshPro. Click "Import TMP Essentials" when prompted.

2. **Script Compilation**: After opening Unity, wait for all scripts to compile before using the wizard.

3. **Manager Scripts**: The wizard creates empty GameObjects for managers. You'll need to attach the actual manager scripts later:
   - TimeManager → `Assets/Scripts/Core/TimeManager.cs`
   - AIManager → `Assets/Scripts/AI/AIManager.cs`
   - ScandalManager → `Assets/Scripts/Scandal/ScandalManager.cs`
   - etc.

4. **Button Wiring**: After creating scenes, you'll need to wire up button OnClick events in the Unity Inspector.

## 🐛 Troubleshooting

### Wizard doesn't appear in menu
- Make sure `SceneSetupWizard.cs` is in `Assets/Scripts/Editor/` folder
- Check that Unity has finished compiling scripts
- Try: **Assets > Reimport All**

### Scripts not found errors
- Wait for Unity to finish importing
- Check that all scripts are in `Assets/Scripts/` folder
- Verify namespaces match (ElectionEmpire.UI, ElectionEmpire.Core, etc.)

### Scenes won't save
- Make sure you have write permissions
- Check that `Assets/Scenes/` folder exists
- Try saving manually: **File > Save Scene**

## 📚 Next Steps

After creating scenes:
1. Attach manager scripts to manager GameObjects
2. Wire up button OnClick events
3. Configure PanelManager panel lists
4. Test scene transitions
5. Add your game logic

---

**Ready to start!** Open Unity and use **Tools > Election Empire > Scene Setup Wizard** to begin.

