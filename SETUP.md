# Election Empire - Setup Guide

## Quick Start

### 1. Install Unity
- Download Unity Hub
- Install Unity 2023.2 or newer
- Make sure to include:
  - Windows/Mac/Linux Build Support
  - TextMeshPro (usually included by default)

### 2. Create/Open Project
- Open Unity Hub
- Click "New Project"
- Select "2D" or "3D" template (doesn't matter, we're using UI)
- Name it "ElectionEmpire"
- Click "Create"

### 3. Install Required Packages

#### Newtonsoft.Json (Required)
1. Open Unity
2. Go to **Window → Package Manager**
3. Click the **"+"** button (top left)
4. Select **"Add package from git URL"**
5. Enter: `com.unity.nuget.newtonsoft-json`
6. Click "Add"

Alternatively, add to `Packages/manifest.json`:
```json
{
  "dependencies": {
    "com.unity.nuget.newtonsoft-json": "3.2.1"
  }
}
```

### 4. Copy Project Files
Copy all files from this repository into your Unity project:
- `Assets/Scripts/` → Your project's `Assets/Scripts/`
- `Assets/StreamingAssets/` → Your project's `Assets/StreamingAssets/`

### 5. Create Scenes

#### MainMenu Scene
1. File → New Scene → Basic (Built-in)
2. Create empty GameObject, name it "MainMenuManager"
3. Add component: `ElectionEmpire.UI.MainMenu`
4. Create UI Canvas:
   - Right-click Hierarchy → UI → Canvas
   - Add buttons: New Campaign, Load Game, Character Library, Quit
   - Wire up button references in MainMenu component

#### CharacterCreation Scene
1. File → New Scene → Basic (Built-in)
2. Create empty GameObject, name it "CharacterCreationManager"
3. Add component: `ElectionEmpire.UI.CharacterCreationFlow`
4. Create UI Canvas with panels for:
   - Mode selection (Random/Build/Library)
   - Random generation display
   - Manual builder (8 steps)
   - Library browser
5. Wire up all references

#### Game Scene (Placeholder)
1. File → New Scene → Basic (Built-in)
2. Create empty GameObject, name it "GameManager"
3. Add component: `ElectionEmpire.Core.GameManager`
4. This will be populated in Sprint 3

### 6. Test Character Generation
1. Open CharacterCreation scene
2. Press Play
3. Click "Random" button
4. You should see a randomly generated character!

## Project Structure

```
ElectionEmpire/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/              # Game foundation
│   │   ├── Character/         # Character system
│   │   └── UI/                # User interface
│   └── StreamingAssets/
│       └── Data/             # JSON character data
├── Packages/
│   └── manifest.json         # Unity packages
└── README.md
```

## Troubleshooting

### "Newtonsoft.Json not found"
- Make sure you installed the package (see step 3)
- Check Package Manager: Window → Package Manager → In Project
- Should see "Newtonsoft Json" in the list

### "Data files not found"
- Make sure `StreamingAssets/Data/` folder exists
- JSON files must be in `Assets/StreamingAssets/Data/`
- Unity creates StreamingAssets folder automatically

### "CharacterDataLoader.Instance is null"
- Make sure CharacterDataLoader component exists in scene
- Or it will auto-create on first access
- Check that JSON files are readable

### UI Not Showing
- Make sure you have a Canvas in the scene
- Check that UI components are children of Canvas
- Verify button references are wired up in Inspector

## Next Steps

After setup is complete:
1. Test random character generation
2. Test manual character builder
3. Test character library (save/load)
4. Ready for Sprint 3: Game World!

## Development Notes

- All saves go to: `%USERPROFILE%\AppData\LocalLow\<CompanyName>\<ProductName>\` (Windows)
- Character library: `Application.persistentDataPath/CharacterLibrary.json`
- Game saves: `Application.persistentDataPath/Saves/`

