# ELECTION EMPIRE - CANON DOCUMENTATION
## Complete System Architecture & Design Decisions

**Version**: 1.0  
**Last Updated**: Sprint 7 Complete  
**Status**: Production Ready

---

## Table of Contents

1. [Core Architecture](#core-architecture)
2. [Character System](#character-system)
3. [World Generation](#world-generation)
4. [AI Opponent System](#ai-opponent-system)
5. [Political Ladder](#political-ladder)
6. [Resource Management](#resource-management)
7. [Election System](#election-system)
8. [Scandal Engine](#scandal-engine)
9. [News Integration System](#news-integration-system)
10. [Chaos Mode](#chaos-mode)
11. [Data Flow](#data-flow)
12. [Technical Specifications](#technical-specifications)

---

## Core Architecture

### Game Manager Pattern
**Decision**: Singleton MonoBehaviour with static Instance property

**Rationale**: 
- Single source of truth for game state
- Easy access from anywhere
- Persists across scene loads
- Coordinates all subsystems

**Implementation**:
```csharp
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    // Coordinates: TimeManager, SaveManager, AIManager, World, Player
}
```

### State Management
**Decision**: Centralized `GameState` class with `PlayerState` for player-specific data

**Rationale**:
- Clear separation of concerns
- Easy to serialize for save/load
- Single source of truth
- Supports multiplayer expansion (multiple PlayerStates)

**Key Classes**:
- `GameState` - Overall game state (world, AI, time, elections)
- `PlayerState` - Player-specific (character, resources, office, history)
- `ResourceManager` - Manages resource transactions and decay

---

## Character System

### Character Generation
**Decision**: Dual-mode system (Random + Manual Builder)

**Modes**:
1. **Balanced** - Mix of good/bad traits, winnable
2. **Chaos** - Maximum weirdness, absurd combinations
3. **Hard** - Difficult but viable, challenge-focused

**Components**:
- Background (20+ options, 5 tiers)
- Personal History (15+ items)
- Public Image (12+ types)
- Skills (25+ options, select 3)
- Quirks (30+, 2 positive + 2 negative)
- Handicaps (15+, optional 0-3)
- Secret Weapon (12+ options)

**Chaos Rating**: Calculated from absurdity of components (1-5 scale)

### Character Library
**Decision**: Save/load/share system with character codes

**Features**:
- Save characters for reuse
- Import/export via share codes
- Track usage statistics
- Best result tracking

---

## World Generation

### Hierarchical Structure
```
World
└── Nation
    └── Regions (10)
        └── States (30-60 total)
            └── Districts (~900 total)
```

**Generation Algorithm**:
1. Generate regions with names
2. Distribute states across regions (3-6 per region)
3. Create districts (15-20 per state)
4. Assign demographics (12 voter blocs)
5. Generate political issues (12 categories)
6. Calculate initial polling

### Demographics
**12 Voter Blocs**:
- WorkingClass, Business, Seniors, Youth
- Educators, Activists, Rural, Urban
- Religious, Secular, Security, General

**Distribution**: Weighted random with regional biases

### Issues
**12 Categories**:
- Healthcare, Economy, Taxes, Crime
- Education, Immigration, Environment
- Infrastructure, ForeignPolicy, Defense
- SocialIssues, Justice

**Priority System**: Each district has priority issues affecting voting

---

## AI Opponent System

### Archetypes (12 Types)
1. **Idealist** - Honest, policy-focused
2. **MachineBoss** - Controls party apparatus
3. **Populist** - Appeals to masses
4. **Technocrat** - Data-driven expert
5. **Showman** - Media-savvy entertainer
6. **Insider** - Connected establishment
7. **Maverick** - Unpredictable, chaotic
8. **DynastyHeir** - Legacy connections
9. **Zealot** - Ideological purist
10. **Corporate** - Business-backed
11. **Revolutionary** - Wants systemic change
12. **Survivor** - Adaptable, does whatever it takes

### Personality Matrix
**8 Core Traits** (0-100):
- Aggression, RiskTolerance, EthicalFlexibility
- Loyalty, Adaptability, Charisma
- Cunning, Impulsiveness

**6 Human Foibles** (0-100):
- Ego, Paranoia, Hubris
- EmotionalVolatility, Obsession, Pride

**Decision Making**:
- Base score on personality + situation
- Apply irrationality factor (personality-driven)
- Random chance for "off-the-wall" decisions
- Emotional reactions override logic sometimes

### AI Behavior
**Strategies**:
- Aggressive, Defensive, Opportunistic
- Cooperative, Chaotic, Strategic

**Adaptation**:
- Adaptive difficulty learns from player
- Strategy changes based on performance
- Personality affects all decisions

---

## Political Ladder

### 5 Tiers

**Tier 1** (Local):
- City Council, School Board, County Commissioner
- Requirements: 35% approval, $3K-$5K funds
- Powers: Local ordinances, small budget

**Tier 2** (Municipal):
- Mayor, County Executive, District Attorney
- Requirements: Tier 1 office, 40% approval, $10K funds
- Powers: Executive orders, emergency declaration, $5M budget

**Tier 3** (State):
- State Senator, State AG, Lieutenant Governor
- Requirements: Tier 2 office, 45% approval, $100K funds, 3 allies
- Powers: State budget, party influence, $50M budget

**Tier 4** (National):
- Governor, US Representative, US Senator
- Requirements: Tier 3 office, 50% approval, $500K funds, 5 allies
- Powers: Veto, appointments, pardons, military command, $500M budget

**Tier 5** (Presidential):
- President
- Requirements: Tier 4 office, 55% approval, $5M funds, 8 allies, 4 regions
- Powers: All previous + treaties, $5B budget, 50 staff

### Progression
- Must win election at each tier
- Can skip tiers if requirements met
- Term limits apply (varies by office)
- Office powers unlock new gameplay options

---

## Resource Management

### Core Resources (6)
1. **PublicTrust** (0-100%) - Voter confidence
2. **PoliticalCapital** (0-300) - Influence currency
3. **CampaignFunds** ($, unlimited) - Money
4. **MediaInfluence** (0-100) - Press coverage
5. **PartyLoyalty** (0-100%) - Party support
6. **Blackmail** (items) - Dirt on opponents

### Resource Mechanics
**Decay**:
- PublicTrust: -0.25% per day
- PoliticalCapital: -5% per day
- MediaInfluence: -10 per day
- PartyLoyalty: -1% per day (if inactive)

**Generation**:
- Office salary (monthly, converted to daily)
- Office resource bonuses
- Event rewards
- Player actions

**Spending**:
- Campaign costs (scales with tier and intensity)
- Event responses
- Policy implementation
- Staff salaries

### Blackmail System
- Acquire through scandals, investigations
- Use to pressure opponents
- 30% backfire chance
- Expires over time
- Max 5 items per target

---

## Election System

### 7 Phases
1. **Announcement** (7 days) - Declare candidacy
2. **Primary** (15-30 days) - Party selection
3. **Campaign** (30-60 days) - Main campaign period
4. **Debate** (14 days) - Debates between candidates
5. **Election Day** (1 day) - Voting
6. **Results** (3 days) - Vote counting
7. **Transition** (30 days) - Winner takes office

### Vote Calculation
**District-by-District**:
- Calculate polling for each candidate per district
- Add election day volatility (-5% to +5%)
- Highest polling wins district
- Sum district votes for total

**Factors**:
- Base polling (from VoterSimulation)
- Campaign spending
- Scandals
- Policy stances
- Character traits
- Debate performance

### Debates
- 3 questions per debate
- Questions based on top issues
- Player responds manually
- AI responds based on personality
- Performance affects polling

---

## Scandal Engine

### 4-Stage Lifecycle
1. **Emergence** - Just discovered, limited awareness
2. **Development** - Growing evidence, media picking up
3. **Crisis** - Peak impact, full public awareness
4. **Resolution** - Being resolved or fading

### Scandal Categories (5)
- Financial (money, taxes, corruption)
- Personal (affairs, behavior, past)
- Policy (failed policies, harm)
- Administrative (staff, mismanagement)
- Electoral (campaign violations, fraud)

### Evolution System
- Scandals can evolve to more severe versions
- Evidence accumulates over time
- Plot twists can occur
- Multiple evolution paths possible

### Response System (8 Types)
- Deny, Apologize, CounterAttack
- Distract, SacrificeStaff, LegalDefense
- FullTransparency, SpinCampaign

**Success Calculation**:
- Base probability from template
- Modified by response type effectiveness
- Character traits affect success
- Resource costs required

### Consequences
- Resource impacts (Trust, Capital, Funds)
- Voter bloc impacts (12 blocs)
- Relationship impacts (allies, enemies)
- Long-term effects (reputation tags, vulnerabilities)

---

## News Integration System

### 8-Stage Pipeline

**Stage 1: Ingestion**
- 5 API sources: NewsAPI.org, GNews, Currents, AP RSS, Reuters RSS
- Priority system with fallback
- Rate limiting and deduplication
- 24-hour caching

**Stage 2: Processing**
- Political relevance (0-100%)
- Sentiment analysis (5 levels)
- Topic extraction (keywords)
- Issue classification (12 categories)
- Entity recognition (Person, Org, Location)
- Controversy scoring (0-100)
- Event type classification (9 types)

**Stage 3: Template Matching**
- 80+ event templates across 16 categories
- Weighted scoring algorithm:
  - Entity Match: 30%
  - Sentiment Alignment: 20%
  - Office Relevance: 25%
  - Controversy Fit: 15%
  - Recency Bonus: 10%
- Keyword bonus multiplier
- Impact penalty for mismatches

**Stage 4: Variable Resolution**
- Path-based extraction: `entities.people[type=govt][0].name`
- Entity filtering and indexing
- Content path resolution
- Sentiment path resolution
- Context path resolution (game state aware)

**Stage 5: Event Factory**
- Creates 4 event types:
  - Crisis (high controversy, negative sentiment)
  - PolicyPressure (issue-based decisions)
  - Opportunity (positive sentiment, relevance)
  - ScandalTrigger (scandal keywords, high controversy)
- Escalation stages for crises (4 stages)
- Urgency-based deadlines

**Stage 6: Temporal Management**
- 4-stage news cycle:
  - Breaking (Day 1)
  - Developing (Days 2-4)
  - Ongoing (Days 5-11)
  - Historical (Day 12+)
- Media fatigue (attention decay)
- Time scaling (real-world → game time)

**Stage 7: Player Response**
- Response options with:
  - Alignment requirements (Law/Chaos, Good/Evil)
  - Resource requirements
  - Success probability
  - Success/failure outcomes
- Stance history tracking
- Flip-flop detection
- Consistency scoring

**Stage 8: Consequences**
- Resource updates (Trust, Capital, Funds, Media, Party)
- Voter bloc impacts (12 blocs)
- Long-term reputation
- Policy opportunity creation

### Template Library
**16 Categories × 5 Templates = 80 Templates**

Categories:
- DomesticLegislation, ElectionCampaign, PoliticalScandal
- EconomicPolicy, ForeignPolicy, HealthcarePolicy
- Immigration, ClimateEnvironment, CrimeJustice
- CivilRights, Education, MilitaryDefense
- TechnologyPolicy, SocialIssues, PartyPolitics
- LocalGovernment

**Template Structure**:
- HeadlineTemplate, DescriptionTemplate, ContextTemplate
- Variable mappings with source paths
- MinImpactScore, MinControversy thresholds
- Required entities
- Trigger keywords
- Office scaling (0.3x to 2.0x by tier)
- Base effects (ranges for randomization)

### Advanced Features
- **Secondary Category Support** - Templates from related categories considered
- **Entity Quality Scoring** - Relevance-based, not just presence
- **Office Tier Optimization** - Penalizes mismatched content
- **Recency-Based Scoring** - Configurable thresholds (6h/24h/72h/168h)
- **Strong Keyword Matches** - Headline matches weighted higher
- **Performance Caching** - Keyword matches cached (30-min TTL)
- **Detailed Score Breakdowns** - For debugging and analysis

---

## Chaos Mode

### Extreme Content
**Character Components**:
- Extreme backgrounds (Criminal, Absurd tiers)
- Extreme negative quirks
- Extreme handicaps
- Absurd secret weapons

**Dirty Tricks**:
- Deepfake Porn, Plant Drugs, Order Hit
- Extreme scandal templates
- Viral chaos events

**Chaos Meter**:
- Tracks chaos score
- Affects game difficulty
- Unlocks evil victory paths

**Evil Victory Paths**:
- Alternative endings for chaotic play
- Higher legacy point rewards
- Unique achievements

---

## Data Flow

### News → Game Event Flow
```
1. NewsAPI/RSS → NewsArticle
2. NewsProcessor → ProcessedNews
3. NewsAdapter → ProcessedNewsItem
4. AdvancedTemplateMatcher → MatchedTemplate
5. VariableInjector → Enriched MatchedTemplate
6. NewsEventFactory → NewsGameEvent
7. NewsCycleManager → Temporal tracking
8. PlayerResponseSystem → Player interaction
9. ResourceManager → Effect application
10. PlayerState → State update
```

### Game Loop Integration
```
GameLoop.Update():
1. TimeManager.Update()
2. ResourceManager.UpdateResources()
3. AIManager.ProcessAITurns()
4. ElectionManager.UpdateElection()
5. ScandalManager.UpdateScandals()
6. NewsEventManager.Update() [fetches, processes, creates events]
7. VictoryConditionManager.Check()
8. DefeatConditionManager.Check()
9. UI Updates
```

---

## Technical Specifications

### Unity Version
- **Minimum**: Unity 2023.2.0f1
- **Target**: Unity 2023.2 LTS or later
- **Platform**: Windows, Mac, Linux (Desktop)

### .NET Version
- **Target Framework**: .NET Standard 2.1
- **Language Version**: C# 9.0

### Dependencies
- **UnityWebRequest** - For API calls
- **System.Text.Json** - For JSON parsing (or Unity's JsonUtility)
- **System.Linq** - For data manipulation
- **System.Collections.Generic** - For collections

### Performance Targets
- **Game Loop**: 60 FPS
- **Resource Updates**: < 1ms
- **Polling Calculation**: < 10ms
- **AI Turn Processing**: < 100ms per AI
- **Election Result Calculation**: < 500ms
- **News Processing**: < 50ms per article
- **Template Matching**: < 20ms per news item

### Save System
- **Format**: JSON (human-readable)
- **Location**: `Application.persistentDataPath`
- **Versioning**: Save format versioned for compatibility
- **Components Saved**:
  - GameState (world, time, elections)
  - PlayerState (character, resources, office, history)
  - AI Opponents (state, relationships)
  - Active Scandals
  - Active News Events
  - Character Library

### API Integration
- **NewsAPI.org** - Primary source
- **GNews API** - Secondary source
- **Currents API** - Tertiary source
- **AP RSS** - Fallback
- **Reuters RSS** - Fallback
- **Rate Limiting**: Per-source tracking
- **Caching**: 24-hour cache for offline play
- **Fallback**: Procedural news generation

### Data Files
**Location**: `Assets/StreamingAssets/Data/`

**Files**:
- `backgrounds.json` - Character backgrounds
- `personalHistory.json` - Character history items
- `publicImages.json` - Public image types
- `skills.json` - Character skills
- `quirks.json` - Character quirks
- `handicaps.json` - Character handicaps
- `weapons.json` - Secret weapons
- `scandal_templates.json` - Scandal templates

**Format**: JSON arrays of objects

---

## Design Decisions

### Why Procedural Generation?
- Infinite replayability
- Unique stories each playthrough
- Reduces content creation burden
- Emergent narratives

### Why Real-World News?
- Timely relevance
- Player engagement
- Educational value
- Dynamic content

### Why Template System?
- Flexible event creation
- Easy to add new templates
- Consistent formatting
- Variable substitution

### Why Weighted Scoring?
- More accurate template matching
- Configurable behavior
- Debuggable (score breakdowns)
- Performance optimized

### Why Office Tier Scaling?
- Realistic impact scaling
- Prevents low-tier players from irrelevant national news
- Prevents high-tier players from trivial local news
- Better gameplay balance

### Why Alignment System?
- Adds roleplay depth
- Restricts certain responses
- Affects AI behavior
- Enables chaos mode

---

## Known Limitations

### News Processing
- Sentiment analysis is keyword-based (not ML)
- Entity extraction is pattern-based (not NLP)
- Controversy scoring is heuristic-based
- No real-time fact-checking

### AI Behavior
- Decision-making is rule-based (not ML)
- Personality affects probability, not guarantees
- No learning from past games
- Limited memory of past interactions

### Performance
- Large world generation can take 5-10 seconds
- Election calculations scale with district count
- News processing is synchronous (could be async)

### Scalability
- Fixed world size (10 regions, ~900 districts)
- Fixed number of AI opponents (0-999, but performance degrades)
- News cache limited to 24 hours

---

## Future Enhancements

### Potential Additions
- Multiplayer support (multiple PlayerStates)
- Mod support (custom templates, characters)
- Advanced AI (ML-based decision making)
- Real-time news streaming
- Social media integration
- Campaign finance tracking
- Lobbying system
- Media manipulation mechanics
- International relations
- Economic simulation

### Technical Improvements
- Async news processing
- Object pooling for events
- Advanced caching strategies
- Performance profiling tools
- Save format compression
- Cloud save support

---

## Testing Strategy

### Unit Tests
- Character generation (all modes)
- World generation (various seeds)
- Template matching (edge cases)
- Resource calculations
- Vote calculations

### Integration Tests
- Full campaign flow
- Save/load cycles
- News → Event → Response → Consequence
- AI decision making
- Scandal evolution

### Manual Testing
- All character creation paths
- All office tiers
- All victory conditions
- All defeat conditions
- Chaos mode features
- News integration (online/offline)

---

## Code Quality Standards

### Required
- All public APIs documented
- Null checks for Unity objects
- Error handling for external APIs
- Fallback behavior for failures
- Performance considerations
- No magic numbers (use constants)

### Recommended
- Unit tests for complex algorithms
- Integration tests for critical paths
- Performance profiling
- Code reviews
- Documentation updates

---

## Version History

### Sprint 1 (Foundation)
- Core systems (GameManager, TimeManager, SaveManager)
- Basic character system
- Main menu

### Sprint 2 (Character System)
- Expanded character data (100+ components)
- Random generator (3 modes)
- Manual builder (8-step wizard)
- Character library

### Sprint 3 (World Generation)
- Procedural world (10 regions, 30-60 states, ~900 districts)
- Voter simulation
- Interactive map

### Sprint 4 (AI Opponents)
- 12 archetypes
- Personality matrix (14 traits)
- Decision-making engine
- Interaction system

### Sprint 5 (Core Gameplay)
- Political ladder (5 tiers)
- Resource management (6 resources)
- Election system (7 phases)
- Victory/defeat conditions

### Sprint 6 (Scandal Engine)
- Complete scandal lifecycle
- 40+ templates
- Evolution system
- Response system

### Sprint 7 (News Integration)
- Multi-source news fetching
- Content processing pipeline
- Advanced template matching
- Event translation system
- Player response system
- Temporal mechanics

---

## Support & Maintenance

### Logging
- Use `Debug.Log` for normal operation
- Use `Debug.LogWarning` for recoverable issues
- Use `Debug.LogError` for critical failures
- Include context in log messages

### Error Recovery
- Always provide fallback behavior
- Don't crash on API failures
- Gracefully handle missing data
- Validate inputs before processing

### Performance Monitoring
- Track processing times
- Monitor memory usage
- Profile hot paths
- Optimize based on data

---

**END OF CANON DOCUMENTATION**

*This document represents the complete, authoritative specification for Election Empire. All implementation should follow these patterns and decisions.*

