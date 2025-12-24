# News → Game Event Translation: Complete Data Flow Architecture

## Overview
This document outlines the complete data pipeline from real-world news ingestion to player consequences in Election Empire.

## Pipeline Stages

### Stage 1: News Ingestion
**Components**: `NewsAPIConnector`, `FallbackNewsSystem`

```
External Sources → API Gateway → Normalization → Article List
     ↓
[NewsAPI.org] ─┐
[GNews API]   ├─→ [NewsAPIConnector] → [Normalized Articles]
[Currents API]│
[AP RSS]      ─┤
[Reuters RSS] ─┘
     ↓
[FallbackNewsSystem] (if APIs fail)
     ↓
[Procedural News] OR [Cached News]
```

**Output**: `List<NewsArticle>`

---

### Stage 2: Content Processing
**Components**: `NewsProcessor`

```
Raw Articles → Filter → Classify → Analyze → Processed News
     ↓
[Political Relevance] (0-100%)
[Sentiment Analysis] (VeryPositive → VeryNegative)
[Topic Extraction] (keywords)
[Issue Classification] (12 categories)
[Entity Recognition] (Person, Org, Location)
[Controversy Scoring] (0-100)
[Event Type] (9 types)
```

**Output**: `ProcessedNews`

---

### Stage 3: Template Matching
**Components**: `EventTemplateLibrary`, `VariableMapper`

```
Processed News → Template Matching → Variable Extraction → Filled Template
     ↓
[EventTemplateLibrary] → Find matching templates (80+)
     ↓
Score by: Relevance, Issue Match, Controversy
     ↓
[VariableMapper] → Extract:
  - Person entities
  - Organizations
  - Locations
  - Topics
  - Issues
  - Numbers/Dates
     ↓
Fill template: "{ENTITY} Announces {ISSUE} Policy"
```

**Output**: `EventTemplate` with filled variables

---

### Stage 4: Event Factory
**Components**: `EventFactory`, `ImpactCalculator`

```
Filled Template → Impact Calculation → Context Scaling → Game Event
     ↓
[ImpactCalculator] → Calculate:
  - Story Significance (0-100)
  - Source Credibility
  - Entity Prominence
  - Sentiment Impact
     ↓
[Context Scaler] → Adjust by:
  - Office Tier (0.8x to 1.6x)
  - Player Level
     ↓
[EventFactory] → Create:
  - NewsGameEvent
  - PolicyChallenge (if issue-based)
  - CrisisEvent (if controversy > 70)
  - OpportunityEvent (if positive sentiment)
```

**Output**: `NewsGameEvent`, `PolicyChallenge`, `CrisisEvent`, `OpportunityEvent`

---

### Stage 5: Temporal Management
**Components**: `NewsCycleManager`

```
Game Event → News Cycle → Stage Progression → Fatigue Application
     ↓
[NewsCycleManager] → Create cycle:
  - Breaking (Day 1)
  - Developing (Days 2-4)
  - Ongoing (Days 5-11)
  - Historical (Day 12+)
     ↓
Apply Media Fatigue:
  - Attention decay: -0.1 per day
  - Public interest decay: -0.08 per day
     ↓
Time Scaling:
  - Real-world days → Game turns
  - Configurable compression
```

**Output**: `NewsCycle` with current stage and attention levels

---

### Stage 6: Player Interface
**Components**: `NewsUI`, `PlayerResponseSystem`

```
Game Event → UI Display → Player Response → Stance Recording
     ↓
[NewsUI] → Display:
  - Event title/description
  - Impact preview
  - Response options
  - Cost/benefit analysis
     ↓
[PlayerResponseSystem] → Handle:
  - Event response selection
  - Policy stance taking
  - Crisis response
  - Opportunity claiming
     ↓
[Stance History] → Track:
  - Issue positions
  - Consistency score
  - Flip-flop detection
```

**Output**: `ResponseResult`, `StanceRecord`

---

### Stage 7: Consequence Engine
**Components**: `PlayerResponseSystem`, `ResourceManager`

```
Player Response → Effect Calculation → Resource Updates → Voter Impact
     ↓
[PlayerResponseSystem] → Calculate:
  - Trust impact (modified by response success)
  - Voter bloc impacts (issue-based)
  - Resource costs/gains
  - Long-term reputation
     ↓
[ResourceManager] → Apply:
  - PublicTrust +/- 
  - PoliticalCapital +/-
  - CampaignFunds -/+
  - MediaInfluence +/-
  - PartyLoyalty +/-
     ↓
[PlayerState] → Update:
  - VoterBlocSupport (12 blocs)
  - PolicyStances
  - ReputationTags
  - ScandalVulnerabilities
```

**Output**: Updated `PlayerState`, `ResourceManager` state

---

### Stage 8: Fallback System
**Components**: `FallbackNewsSystem`, `NewsSettings`

```
API Failure → Cache Check → Procedural Generation → Blended Output
     ↓
[FallbackNewsSystem] → Options:
  1. Load cached news (< 24 hours old)
  2. Generate procedural news
  3. Blend real + procedural (reality slider)
     ↓
[NewsSettings] → Apply:
  - News frequency (0.5x to 2x)
  - Category preferences
  - Ignored categories
  - Reality blend (0.0 to 1.0)
  - Max news per day
```

**Output**: `List<NewsArticle>` (real, cached, or procedural)

---

## Data Structures

### Input → Output Flow

```
NewsArticle (Raw)
    ↓
ProcessedNews (Analyzed)
    ↓
EventTemplate (Matched)
    ↓
NewsGameEvent (Created)
    ↓
PlayerResponse (Selected)
    ↓
ConsequenceResult (Applied)
```

### Key Classes

- **NewsArticle**: Raw article from API/RSS
- **ProcessedNews**: Analyzed with relevance, sentiment, issues
- **EventTemplate**: Template with variable slots
- **NewsGameEvent**: Final game event with impacts
- **PolicyChallenge**: Issue-based decision point
- **CrisisEvent**: High-controversy event
- **OpportunityEvent**: Positive news benefit
- **StanceRecord**: Player position history
- **NewsCycle**: Temporal tracking

---

## Integration Points

### With Game Systems

1. **ResourceManager**: Applies trust, capital, funds impacts
2. **VoterSimulation**: Updates voter bloc support
3. **ScandalSystem**: Can trigger scandals from news
4. **ElectionManager**: News affects polling
5. **AIManager**: AI responds to same news events
6. **GameLoop**: Updates news system each turn

---

## Performance Considerations

- **API Rate Limiting**: Automatic per-source tracking
- **Caching**: 24-hour cache for offline play
- **Deduplication**: Removes duplicate stories
- **Filtering**: Only processes relevant news (30%+ relevance)
- **Batch Processing**: Processes multiple articles efficiently

---

## Configuration

### API Keys (Optional)
- `APIKey_newsapi` - NewsAPI.org
- `APIKey_gnews` - GNews API
- `APIKey_currentsapi` - Currents API

**Note**: RSS feeds work without API keys!

### Settings
- News Frequency: 0.5x to 2x
- Reality Blend: 0.0 (all procedural) to 1.0 (all real)
- Category Preferences: Select preferred event types
- Max News Per Day: 1-50

---

## Example Complete Flow

**Real News**: "Senate Passes Healthcare Bill Amid Controversy"

1. **Ingestion**: Fetched from AP RSS feed
2. **Processing**: 
   - Relevance: 95%
   - Sentiment: Negative
   - Issues: Healthcare
   - Controversy: 70%
   - Event Type: Legislation
3. **Template Match**: LEG-001 (Major Bill Passes)
4. **Variable Mapping**: {ORGANIZATION} = "Senate", {ISSUE} = "Healthcare"
5. **Event Creation**: "Senate Passes Healthcare Bill"
6. **Impact Calculation**: Trust -5%, Seniors -8%, Policy Opportunity: Healthcare
7. **Context Scaling**: Multiplied by office tier (1.2x for Tier 3)
8. **Cycle Creation**: Breaking stage, 70% attention
9. **UI Display**: Shows event with 3 response options
10. **Player Response**: Selects "Support" stance
11. **Effect Calculation**: Trust +2%, Seniors +5%, Consistency maintained
12. **Consequence**: Applied to PlayerState and resources
13. **Stance Recorded**: Healthcare: Support (Date: Now)

---

## Status: COMPLETE ✅

All 8 pipeline stages implemented and integrated!

