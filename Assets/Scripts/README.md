# Election Empire - Missing Type Fixes
## Instructions for Applying These Fixes

### Step 1: Add New Files

Copy these 3 new files to your Unity project:

1. **VoterBloc.cs** → `Assets/Scripts/Core/Enums/VoterBloc.cs`
2. **Transaction.cs** → `Assets/Scripts/Finance/Transaction.cs`  
3. **ScandalCategory.cs** → `Assets/Scripts/Scandal/ScandalCategory.cs`

### Step 2: Add Using Directives

**File: Assets/Scripts/AI/AIOpponent.cs**
Add at the top with other using statements:
```csharp
using ElectionEmpire.Core;
```

**File: Assets/Scripts/Finance/CampaignFinanceSystem.cs**
Add at the top (if not already present):
```csharp
using ElectionEmpire.Finance;
```

**File: Assets/Scripts/Gameplay/ResourceManager.cs**
Add at the top (if not already present):
```csharp
using ElectionEmpire.Scandal;
```

### Step 3: Verify and Rebuild

1. In Unity, go to **Assets → Reimport All** (optional but recommended)
2. Wait for compilation
3. Report any remaining errors

---

## Quick Reference: What Each File Contains

### VoterBloc.cs (ElectionEmpire.Core namespace)
- `VoterBloc` enum - 30+ voter demographic types (WorkingClass, Youth, Religious, etc.)

### Transaction.cs (ElectionEmpire.Finance namespace)
- `Transaction` class - Financial transaction data
- `TransactionResult` class - Result of transaction attempts  
- `TransactionType` enum - Types of income/expenses
- `TransactionStatus` enum - Pending, Completed, Failed, etc.
- `TransactionErrorCode` enum - Error types

### ScandalCategory.cs (ElectionEmpire.Scandal namespace)
- `ScandalCategory` enum - 40+ scandal types matching the design doc
- `ScandalSeverity` enum - 1-10 severity scale
- `ScandalStage` enum - Scandal lifecycle stages
