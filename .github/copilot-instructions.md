# Election Empire - AI Coding Agent Instructions

## Project Overview
Election Empire is a darkly satirical political strategy game built in Unity 6 using URP 2D. The game simulates campaign management, voter dynamics, scandals, and AI opponents in a procedural political landscape.

## Architecture
- **Core Systems**: GameManager singleton coordinates all subsystems (TimeManager, AIManager, SaveManager, etc.)
- **Modular Design**: Scripts organized by domain in `Assets/Scripts/` (AI/, Finance/, Scandal/, UI/, World/, etc.)
- **Namespace Convention**: `ElectionEmpire.{Subsystem}` (e.g., `ElectionEmpire.Core`, `ElectionEmpire.AI`)
- **Data Flow**: World generation → Player/AI setup → Voter simulation → Game loop with turn-based mechanics

## Key Patterns
- **Enums for Types**: Use detailed enums like `ScandalCategory` (40+ types), `TransactionType`, `VoterBloc` (30+ demographics)
- **Manager Classes**: Each subsystem has a manager (AIManager, ScandalManager, etc.) with singleton patterns
- **UI Navigation**: All screens inherit from `BaseScreen` with `NavigateTo(ScreenType)` and `OnScreenEnter/Exit` lifecycle
- **Serializable Classes**: Game state uses `[Serializable]` classes with properties for save/load compatibility

## Development Workflow
- **Scene Setup**: Use `Tools/Election Empire/Scene Setup Wizard` to create MainMenu and Game scenes with proper hierarchies
- **Build Process**: Standard Unity build via editor; no custom build scripts
- **Testing**: Run in Unity Play mode; no automated tests currently implemented
- **Dependencies**: Standard Unity packages (URP, Input System, 2D tools); no external SDKs

## Code Conventions
- **File Organization**: One class per file, matching folder structure (e.g., `AI/AIManager.cs`)
- **Constructor Patterns**: Use parameterless constructors with property setters; avoid complex initialization logic in Awake()
- **Error Handling**: Return result objects (e.g., `TransactionResult`) with success flags and error codes
- **Collections**: Prefer `List<T>` and `Dictionary<string, T>` for runtime data; arrays for serialized fields

## Common Integration Points
- **GameManager.Instance**: Access central state and managers
- **Event System**: Use UnityEvents for UI interactions; custom events for game state changes
- **Save/Load**: Implement `ISerializable` or use JsonUtility for persistence
- **AI Decisions**: Extend `AIDecisionMaker` for new opponent behaviors

## Examples
- **Adding New Scandal Type**: Add to `ScandalCategory` enum, update `ScandalTemplateLibrary`
- **UI Screen**: Inherit `BaseScreen`, override `OnScreenEnter`, use `NavigateTo` for transitions
- **Financial Transaction**: Create `Transaction` instance, validate via `CampaignFinanceSystem`, handle `TransactionResult`</content>
<parameter name="filePath">c:\DEV\ElectionEmpire\.github\copilot-instructions.md