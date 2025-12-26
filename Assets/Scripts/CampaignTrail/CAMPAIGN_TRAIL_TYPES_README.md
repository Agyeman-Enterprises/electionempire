# CAMPAIGN TRAIL TYPES - Type Definitions

## Overview

`CampaignTrailTypes.cs` provides the **canonical type definitions** for the entire Campaign Trail system. All other campaign trail files should reference these types.

---

## 📋 Core Enums

### TrailEventType
**Planned Events:**
- `Walkabout` - Walking through town
- `TownHall` - Q&A with audience
- `DinerVisit` - Classic photo op
- `FactoryTour` - Blue collar appeal
- `FarmVisit` - Rural appeal
- `SchoolVisit` - Education focus
- `ChurchService` - Faith community
- `VFWHall` - Veterans
- `UnionHall` - Labor
- `CommunityCenter` - General outreach
- `MainStreetWalk` - Small business
- `FairgroundsRally` - Large outdoor event
- `BarVisit` - Risky "regular person" appeal
- `SeniorCenter` - Elder voters
- `CollegeCampus` - Youth vote
- `HousingProject` - Low-income outreach
- `SuburbanCookout` - Middle class

**Unplanned Events:**
- `AmbushInterview` - Reporter catches you
- `ProtestEncounter` - Stumble into protest
- `HecklerConfrontation` - Someone wants to yell
- `SecretWitnessEncounter` - Someone knows something
- `ViralMoment` - Unexpected viral potential
- `ChildEncounter` - Kid asks devastating question
- `CelebrityEncounter` - Famous person helps/hurts

### CrowdHostility
- `Friendly` - Home turf, supporters
- `Mixed` - Some supporters, some opposition
- `Neutral` - Undecided, waiting to judge
- `Skeptical` - Leaning negative but persuadable
- `Hostile` - Actively antagonistic
- `Dangerous` - Security concerns, potential violence

### CitizenDisposition
- `TrueBeliever` - Passionate supporter
- `Hopeful` - Wants to believe
- `Undecided` - Genuinely weighing options
- `Skeptical` - Doubts all politicians
- `Cynical` - Believes all are corrupt
- `Hostile` - Actively opposes you
- `Apathetic` - Doesn't care, won't vote
- `SingleIssue` - Only cares about one thing
- `Grievance` - Has specific complaint
- `SecretsToTell` - Knows something damaging

### CitizenAction
**Positive:** Cheer, AskForSelfie, SharePersonalStory, OfferEndorsement, VolunteerToHelp, MakeSmallDonation

**Neutral:** AskQuestion, ListenQuietly, RecordOnPhone, WatchFromDistance, IgnoreCompletely

**Negative:** HeckleLoudly, AskGotchaQuestion, BringUpPastFailure, ThrowProjectile, RevealSecret, ConfrontAboutBrokenPromise, CreateScene, CallYouALiar, DemandAnswers

**Special:** IntroduceSomeoneImportant, SetUpAmbush, RecordForViralVideo, MentionYourOpponent

### SecretType
**Personal:** AffairEvidence, SubstanceAbuse, EmbarrassingPhoto, HiddenFamily, PastArrest, CollegeIndiscretion

**Financial:** TaxEvasion, HiddenWealth, BriberyEvidence, ConflictOfInterest, CampaignFinanceViolation

**Professional:** EmployeeAbuse, DiscriminationHistory, FailedBusinessVenture, LawsuitSettlement

**Political:** SecretMeeting, PolicyFlipFlop, LobbyistConnection, BackroomDeal, BrokenPromiseProof

### ProjectileType
Egg, Tomato, Shoe, Milkshake, Water, Glitter, Pie, Flour, Paint, Produce, Beverage, Nothing

### EncounterOutcome
- `Triumph` - Couldn't have gone better
- `Positive` - Good result, helped campaign
- `Neutral` - Neither helped nor hurt
- `Negative` - Some damage done
- `Disaster` - Major problem
- `ViralMoment` - Could be good or bad, will spread
- `SecretRevealed` - Damaging information came out
- `Escaped` - Got away before it got worse

### MediaImpactLevel
None, LocalMention, LocalHeadline, RegionalStory, NationalMention, NationalStory, ViralSensation, DefiningMoment

---

## 📦 Core Data Structures

### Citizen
**Properties:**
- Basic info: `Id`, `Name`, `Age`, `Gender`, `Occupation`, `Appearance`, `VoterBloc`
- Personality: `Disposition`, `Enthusiasm`, `Volatility`, `Articulateness`
- State: `IsIntoxicated`, `IsAngry`, `IsArmed`, `HasCamera`, `HasSecret`, `SecretKnown`, `SecretDetails`
- Relationships: `KnowsCandidate`, `HowTheyKnow`, `TrustInCandidate`
- Intent: `PrimaryGrievance`, `Question`, `LikelyAction`

### EncounterChoice
**Properties:**
- Display: `Text`, `Outcome`, `Descriptor`
- Impacts: `TrustImpact`, `MediaImpact`, `PartyLoyaltyImpact`, `SpecificBlocImpact`, `AffectedBloc`
- Risk: `RiskLevel`, `SuccessChance`, `RequiresSkillCheck`, `RequiredAttribute`, `DifficultyCheck`
- Flags: `EndsEncounter`, `EscalatesEncounter`, `TriggersFollowUp`, `FollowUpEncounterId`

### TownsfolkEncounter
**Properties:**
- Person: `Name`, `Age`, `Occupation`, `Appearance`
- Setup: `OpeningAction`, `OpeningDialogue`, `Context`
- Environment: `IsHostile`, `HasAudience`, `AudienceSize`, `IsBeingRecorded`, `PressPresent`, `LiveBroadcast`
- Evidence: `HasPhotoEvidence`, `HasDocumentEvidence`, `HasWitnessCorroboration`, `SecretType`, `SecretDetails`
- Options: `Choices` (List<EncounterChoice>)
- Resolution: `Outcome`, `OutcomeDescription`, `HeadlineGenerated`, `MediaImpact`

### TrailEvent
**Properties:**
- Basic: `Id`, `Type`, `LocationName`, `LocationDescription`, `EventTime`
- Crowd: `ExpectedAttendance`, `ActualAttendance`, `HostilityLevel`, `BlocRepresentation`
- Press: `PressExpected`, `PressPresent`, `ReportersPresent`, `LiveCoverage`, `ReporterAmbushPending`
- Encounters: `CitizensPresent`, `PlannedEncounters`, `RandomEncounters`, `CompletedEncounters`
- Results: `Result` (TrailEventResult)

### TrailEventResult
**Properties:**
- Counts: `TotalEncounters`, `PositiveEncounters`, `NeutralEncounters`, `NegativeEncounters`, `DisasterEncounters`, `ViralMoments`
- Resource Changes: `NetTrustChange`, `NetMediaImpact`, `NetFundsChange`, `NetPartyLoyaltyChange`, `BlocChanges`
- Notable Events: `SecretsRevealed`, `ProjectilesThrown`, `MemorableMoments`, `HeadlinesGenerated`
- Special: `ReporterAmbushOccurred`, `SecurityIncident`, `MedicalEmergency`, `ProtestErupted`
- Summary: `HeadlineOfTheDay`, `MostMemorableMoment`, `OverallOutcome`

---

## 🎤 Reporter System

### IntrepidReporter
**Properties:**
- Basic: `Id`, `Name`, `Outlet`, `Appearance`, `Personality`, `Catchphrase`
- Tracking: `TimesEncountered`, `SuccessfulGotchas`, `TimesEvaded`, `TimesCharmed`, `QuestionsAsked`, `TopicsInvestigating`
- Relationship: `RelationshipScore`, `HasGrudge`, `GrudgeReason`, `RespectEarned`
- Investigation: `CurrentStoryAngle`, `StoryProgress`, `EvidenceGathered`, `ReadyToPublish`
- Behavior: `Persistence`, `Ruthlessness`, `Credibility`, `IntelligenceGathering`

### ReporterAmbush
**Properties:**
- Basic: `Id`, `Reporter`, `Location`, `Context`, `OpeningLine`, `Questions`
- Environment: `CameraRolling`, `LiveBroadcast`, `OtherReportersAttracted`, `CrowdWatching`
- Options: `ResponseOptions` (List<EncounterChoice>)

---

## ⚙️ Configuration

### TrailEventConfig
**Probability Weights:**
- `HostileEncounterChance` (default: 0.3)
- `SecretWitnessChance` (default: 0.1)
- `ReporterAmbushChance` (default: 0.2)
- `ProjectileChance` (default: 0.05)
- `ViralMomentChance` (default: 0.15)

**Scaling:**
- `HostilityByOfficeTier` - Higher office = more hostile
- `ScrutinyByApproval` - Lower approval = more scrutiny
- `ChaosMultiplier` - Global chaos setting

**Limits:**
- `MaxEncountersPerEvent` (default: 10)
- `MinEncountersPerEvent` (default: 3)
- `MaxSimultaneousReporters` (default: 5)

---

## 🔗 Integration Notes

### Using These Types

All campaign trail files should reference types from `CampaignTrailTypes.cs`:

```csharp
using ElectionEmpire.CampaignTrail;

// Use the enums
TrailEventType eventType = TrailEventType.TownHall;
CitizenDisposition disposition = CitizenDisposition.Undecided;
EncounterOutcome outcome = EncounterOutcome.Positive;

// Use the data structures
Citizen citizen = new Citizen();
TrailEvent trailEvent = new TrailEvent();
TownsfolkEncounter encounter = new TownsfolkEncounter();
```

### File Organization

- **CampaignTrailTypes.cs** - Type definitions (this file)
- **CampaignTrailEncounters.cs** - Detailed encounter generation
- **ColorfulTownsfolk.cs** - Citizen archetype generation
- **ReporterAmbushScenarios.cs** - Reporter system
- **CampaignTrailManager.cs** - Main manager (if exists)

### Compatibility

The types in this file are designed to be:
- **Comprehensive** - Cover all campaign trail scenarios
- **Extensible** - Easy to add new event types
- **Serializable** - All data structures are `[Serializable]`
- **Type-safe** - Uses enums for fixed sets of values

---

## 📝 Usage Examples

### Creating a Trail Event

```csharp
var trailEvent = new TrailEvent
{
    Type = TrailEventType.TownHall,
    LocationName = "Springfield Community Center",
    LocationDescription = "A packed room of 200 voters",
    EventTime = DateTime.Now,
    ExpectedAttendance = 200,
    HostilityLevel = CrowdHostility.Mixed,
    PressPresent = true,
    ReporterAmbushPending = false
};
```

### Creating a Citizen

```csharp
var citizen = new Citizen
{
    Name = "Bob Johnson",
    Age = 45,
    Occupation = "Factory Worker",
    Disposition = CitizenDisposition.Skeptical,
    HasSecret = true,
    SecretKnown = SecretType.BrokenPromiseProof,
    SecretDetails = "You promised to save the factory in 2020",
    LikelyAction = CitizenAction.ConfrontAboutBrokenPromise
};
```

### Creating an Encounter Choice

```csharp
var choice = new EncounterChoice
{
    Text = "I understand your frustration. Let me explain what happened.",
    TrustImpact = 5.0f,
    MediaImpact = 2.0f,
    RiskLevel = 20,
    SuccessChance = 0.7f,
    RequiresSkillCheck = true,
    RequiredAttribute = "Charisma",
    DifficultyCheck = 15
};
```

---

## ✅ Status

All types are defined and ready for use. The file compiles without errors and integrates with the existing campaign trail system.

