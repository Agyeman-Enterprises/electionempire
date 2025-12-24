# ELECTION EMPIRE: COMPREHENSIVE DEVELOPER SPECIFICATION

## DOCUMENT VERSION: 1.0
## CREATED: April 1, 2025
## STATUS: DEVELOPMENT READY

---

## 1. EXECUTIVE SUMMARY

**Election Empire** is a darkly satirical political strategy game where players ascend from local politics to national leadership using strategy, alliances, and tactical maneuvers. The game blends roguelike unpredictability, turn-based planning, and sandbox narrative emergence in a cartoonish exaggerated world that mirrors real political dynamics through absurdist gameplay.

This specification document consolidates all gameplay systems, progression paths, character development, and technical requirements to create a cohesive blueprint for development. It integrates the background selection system, alignment mechanics, procedural generation systems, and monetization strategy into a unified design.

---

## 2. CORE GAMEPLAY ARCHITECTURE

### 2.1 Game Identity

**Tagline:** "Politics is a dirty game. Time to get your hands filthy."

**Genre:** Turn-based political strategy with roguelike elements

**Inspirations:** 
- *Crusader Kings III* (character-driven emergent storytelling)
- *Democracy 4* (policy implementation and consequences)
- *Slay the Spire* (roguelike progression and unlocks)
- *RimWorld* (procedural event generation)

**Tone:** Darkly satirical, absurdist humor with serious strategic depth

### 2.2 Core Loop

1. **Campaign Phase**: Run for office, build supporter networks, make promises
2. **Governance Phase**: Implement policies, manage staff, handle crises
3. **Consequence Phase**: Deal with policy fallout, scandals, and opponent reactions
4. **Election Phase**: Face voters or manage transition to new power structure
5. **Legacy Phase**: Record achievements, unlock perks for future runs

### 2.3 Session Structure

- **Play Session Length:** 30-90 minutes per election cycle
- **Full Playthrough:** 5-10 hours to reach highest office
- **Replayability:** High, with legacy unlocks and procedural content

---

## 3. CHARACTER SYSTEM

### 3.1 Background Selection

Players begin by selecting a background that provides starting attributes, relationships, and specialized abilities. Each background offers advantages and disadvantages for different political paths.

| Background | Starting Advantages | Starting Disadvantages | Special Ability |
|------------|---------------------|------------------------|-----------------|
| **Businessman** | +30% Campaign Funds<br>+2 Corporate Connections | -10% Working Class Trust<br>-5% Media Favorability | **Golden Rolodex**: Can call in one major financial favor per election cycle |
| **Local Politician** | +15% Party Loyalty<br>+1 Political Experience | -5% "Fresh Face" Appeal<br>-5% Anti-Establishment Support | **Party Insider**: 25% discount on party endorsements |
| **Teacher** | +20% Education Bloc Support<br>+10% Youth Support | -10% Campaign Funds<br>-5% Business Support | **Inspirational Speaker**: Convert 10% more undecided voters during debates |
| **Doctor** | +25% Healthcare Credibility<br>+10% Senior Support | -10% Economic Policy Credibility<br>-5% Toughness Perception | **Bedside Manner**: +15% crisis management effectiveness during health emergencies |
| **Police Officer** | +20% Law & Order Credibility<br>+15% Security Bloc Support | -10% Minority Bloc Support<br>-5% Youth Support | **Authority Presence**: +10% effectiveness at shutting down opponent interruptions |
| **Journalist** | +25% Media Connections<br>+15% Scandal Detection | -15% Party Loyalty<br>-10% Business Support | **Inside Scoop**: Reveal one opponent secret per campaign |
| **Activist** | +30% Grassroots Support<br>+20% Volunteer Recruitment | -25% Corporate Support<br>-15% Moderate Bloc Appeal | **Rabble Rouser**: Can organize one free major rally per election |
| **Religious Leader** | +30% Faith Community Support<br>+15% Moral Authority | -20% Secular Bloc Support<br>-10% Youth Support | **Moral High Ground**: 25% reduced impact from personal scandals |

### 3.2 Stat Distribution

Players allocate 10 points across core attributes that influence gameplay:

- **Charisma:** Affects speech effectiveness, debate performance
- **Intelligence:** Affects policy implementation, crisis management
- **Cunning:** Affects dirty trick effectiveness, scandal concealment
- **Resilience:** Affects scandal recovery, stress management
- **Networking:** Affects alliance building, fundraising

### 3.3 Character Traits

Players select 2 positive traits and 1 negative trait:

**Positive Traits Examples:**
- **Silver Tongue:** +15% debate effectiveness
- **Born Leader:** +10% staff loyalty
- **Media Darling:** +20% positive media coverage
- **Policy Wonk:** +15% policy effectiveness
- **Fundraising Genius:** +25% campaign fund generation

**Negative Traits Examples:**
- **Skeletons in Closet:** Start with a minor hidden scandal
- **Foot in Mouth:** 10% chance to cause a gaffe during speeches
- **Thin Skinned:** -15% effectiveness when attacked by opponents
- **Technophobe:** -20% social media effectiveness
- **Controversial Past:** Start with -10% trust from a random voter bloc

---

## 4. POLITICAL LADDER & PROGRESSION

### 4.1 Career Path Progression

| Tier | Office Options | Unlock Requirements | Office-Specific Powers | Special Challenges |
|------|----------------|---------------------|------------------------|-------------------|
| **1** | • City Council<br>• School Board<br>• County Commissioner<br>• Neighborhood Association | Default Starting Tier | • Local Ordinances<br>• Small Budget Control<br>• Community Projects | • Limited Resources<br>• Hyperlocal Issues<br>• Low Media Coverage |
| **2** | • Mayor<br>• County Executive<br>• District Attorney<br>• State Representative | • Win Tier 1 Election<br>• 40%+ Approval Rating<br>• $10K+ Campaign Funds | • Staff Hiring<br>• Local Law Enforcement<br>• Municipal Budgets<br>• Property Taxes | • First Major Scandals<br>• Rival Emergence<br>• Business Interference |
| **3** | • State Senator<br>• State Attorney General<br>• Lieutenant Governor<br>• Regional Director | • Win Tier 2 Election<br>• 45%+ Approval<br>• 50%+ Party Loyalty<br>• 3+ Political Allies | • Lobbyist Access<br>• State Regulatory Powers<br>• PAC Fundraising<br>• Committee Assignments | • Media Scrutiny<br>• Special Interest Groups<br>• Opposition Research |
| **4** | • Governor<br>• U.S. Representative<br>• U.S. Senator<br>• Cabinet Secretary | • Win Tier 3 Election<br>• $500K+ Campaign Funds<br>• 55%+ Approval<br>• 1+ Blackmail Asset | • Executive Orders<br>• National Profile<br>• Major Legislation<br>• Emergency Powers | • Constant Media Attention<br>• Party Infighting<br>• National Crises<br>• Corruption Investigations |
| **5** | • President<br>• Supreme Leader<br>• Shadow Government | • Win Tier 4 Election<br>• Control 4+ Regions<br>• 2+ Major Alliances<br>• 70%+ Base Loyalty | • Executive Authority<br>• Military Control<br>• Supreme Court Appointments<br>• Global Diplomacy | • Impeachment Risk<br>• Assassination Attempts<br>• Constitutional Crises<br>• International Conflicts |

### 4.2 Alternate Paths

#### 4.2.1 Party Loyalist Path
- Focus on maintaining party loyalty above 75%
- Gain party endorsements and resources
- Unlock unique "Party Leadership" positions
- Special ability: Party Mobilization (guaranteed voter turnout from base)

#### 4.2.2 Populist Path
- Focus on maintaining public approval above 65%
- Build direct voter connections that bypass party structures
- Unlock "Movement Leader" and "Political Revolution" options
- Special ability: Mass Rally (temporary huge polling boost)

#### 4.2.3 Shadow Operator Path
- Focus on accumulating blackmail and leverage
- Build hidden networks of influence
- Unlock "Kingmaker" and "Shadow Government" options
- Special ability: Leverage Network (force an opponent to drop out)

#### 4.2.4 Authoritarian Path
- Focus on accumulating executive power and security support
- Gradually erode democratic norms
- Unlock "State of Emergency" and "Supreme Leader" options
- Special ability: Martial Law (suspend normal election rules)

---

## 5. ALIGNMENT SYSTEM

### 5.1 Alignment Axes

The game tracks player alignment across two hidden axes:

**Law vs. Chaos Axis:**
- **Lawful:** Follows established systems, upholds institutions, values order
- **Neutral:** Pragmatic approach to rules, selective enforcement
- **Chaotic:** Disrupts systems, creates new rules, values change

**Good vs. Evil Axis:**
- **Good:** Public welfare focus, transparency, ethical methods
- **Neutral:** Mixed motivations, situational ethics
- **Evil:** Self-interest focus, manipulation, exploitative methods

### 5.2 Alignment Determination

- Alignment shifts based on policy decisions, crisis responses, and campaign tactics
- Each major decision shifts alignment by small increments (1-5 points)
- Alignment is hidden from player but influences game events and NPC reactions
- Players receive subtle feedback about alignment via news headlines, staff comments, and voter responses

### 5.3 Alignment Categories & Special Abilities

| Alignment | Description | Special Powers | Weaknesses |
|-----------|-------------|----------------|------------|
| **Lawful Good** | Reformer, idealist, ethical leader | **Inspire the People**: +30% volunteer surge<br>**Ethical High Ground**: Immunity to certain scandal types | Restricted from using dirty tricks<br>Vulnerable to "too idealistic" attacks |
| **Neutral Good** | Pragmatic do-gooder, results-oriented | **Broad Appeal**: +20% support from opposing voters<br>**Coalition Builder**: Enhanced alliance formation | Moderate enthusiasm from base<br>Vulnerable to "flip-flopper" attacks |
| **Chaotic Good** | Revolutionary, disruptor, people's champion | **People's Shield**: Public protest shields from one scandal<br>**Revolutionary Spirit**: +40% youth turnout | Party establishment resistance<br>Media skepticism |
| **Lawful Neutral** | Bureaucrat, institutionalist, traditionalist | **Establishment Shield**: Party protection from scandals<br>**Rules Lawyer**: Nullify opponent attacks | Low enthusiasm<br>Status quo defender image |
| **True Neutral** | Opportunist, survivalist, weather vane | **Political Chameleon**: Can switch party once<br>**Independent Appeal**: Reduced partisan penalties | Trust deficit<br>No strong base |
| **Chaotic Neutral** | Wildcard, unpredictable, maverick | **Media Spectacle**: Convert scandals to publicity<br>**Unpredictable Surge**: Random voter support spikes | Unreliable allies<br>Organizational weakness |
| **Lawful Evil** | Corrupt insider, machine boss, manipulator | **Machine Politics**: Guaranteed minimum vote %<br>**Blackmail Network**: Enhanced dirt collection | Public distrust<br>Corruption vulnerability |
| **Neutral Evil** | Self-serving operator, mercenary | **Mercenary Support**: Buy allies at premium<br>**Plausible Deniability**: Distance from underlings' actions | Loyalty deficits<br>Sellout image |
| **Chaotic Evil** | Tyrant, demagogue, fear monger | **Rule by Fear**: Suppress opposition turnout<br>**Scapegoating**: Transfer scandals to allies | High backlash risk<br>Resistance movements |

---

## 6. RESOURCES & ECONOMY

### 6.1 Core Resources

| Resource | Generation | Usage | Decay | Cap |
|----------|------------|-------|-------|-----|
| **Public Trust** | Successful policies, crisis management, honest campaigns | Election chances, policy implementation | -2% per broken promise<br>-5-30% per scandal | 100% |
| **Political Capital** | Election wins, legislative victories, alliance building | Pass controversial policies, push agendas | -5% per turn<br>-20% per failed initiative | Varies by office tier |
| **Campaign Funds** | Fundraisers, PACs, donations, dark money | Advertising, staff hiring, events, dirty tricks | -5-20% per turn (burn rate) | Unlimited |
| **Dirt/Blackmail** | Opposition research, investigations, insider tips | Leverage against opponents, scandal defense | 10% chance of expiration per turn<br>30% chance of backfire when used | 5 per opponent |
| **Media Influence** | Press conferences, relationships, ad buys, stunts | Control narrative, crisis management, attack opponents | -10% per turn without refreshing | 100 points |
| **Party Loyalty** | Align with party platform, support party candidates, fundraise for party | Endorsements, resources, protection from scandals | -5% per stance against party<br>-15% per party leader opposed | 100% |

### 6.2 Secondary Resources

| Resource | Generation | Usage | Special Properties |
|----------|------------|-------|-------------------|
| **Staff Quality** | Hiring, training, experience gains | Enhanced operations, scandal management, policy effectiveness | Affected by morale, can resign or betray |
| **Voter Bloc Support** | Targeted policies, demographic appeals, identity politics | Electoral victories, pressure campaigns | 12 distinct blocs with interrelated reactions |
| **Special Interest Favor** | Support industry policies, regulatory decisions | Campaign funding, election support, crisis assistance | Can conflict with voter interests |
| **Legacy Points** | Achievements, milestone completions, dramatic victories/losses | Unlock perks for future runs | Permanent progression |

### 6.3 In-Game Currencies

| Currency | Real Money? | Acquisition | Usage |
|----------|-------------|-------------|-------|
| **CloutBux** | No (earned in-game) | Victory bonuses, crisis survival, achievements | Cosmetics, minor gameplay boosts, staff recruitment |
| **Purrkoin** | Optional (can purchase) | Major achievements, optional purchases | Premium content packs, major gameplay advantages, legacy transfers |

---

## 7. TURN-BASED MECHANICS

### 7.1 Turn Structure

Each turn represents one month of political time and includes:

1. **Morning Briefing Phase**
   - Staff reports
   - Poll updates
   - Media headlines
   - Crisis alerts

2. **Action Phase** (3 actions per turn)
   - Campaign activities
   - Policy implementation
   - Staff management
   - Crisis response
   - Dirty tricks

3. **Reaction Phase**
   - NPC reactions
   - Media coverage
   - Public opinion shifts
   - Scandal development

4. **End of Turn**
   - Resource updates
   - Random events
   - Calendar advancement

### 7.2 Election Cycles

| Office Tier | Term Length | Re-election Limit | Special Rules |
|-------------|-------------|-------------------|--------------|
| Tier 1 | 2 years | None | Low visibility, local issues dominant |
| Tier 2 | 4 years | 2 terms | Media coverage begins, party alignment matters |
| Tier 3 | 4 years | 2-3 terms | National attention, ideology becomes factor |
| Tier 4 | 4-6 years | 1-2 terms | High scrutiny, legacy concerns arise |
| Tier 5 | 4 years | 2 terms (unless changed) | Global impacts, constitutional powers |

### 7.3 Crisis Mechanics

1. **Crisis Types**
   - Natural Disasters
   - Economic Downturns
   - Scandals (personal or administration)
   - Security Threats
   - Social Unrest
   - Health Emergencies

2. **Crisis Response Options**
   - Address Immediately (high risk/reward)
   - Delegate to Staff (moderate outcomes)
   - Divert Attention (temporary solution)
   - Ignore (escalation risk)

3. **Crisis Resolution Effects**
   - Trust impact (-30% to +20%)
   - Resource consumption/generation
   - New opportunities or threats
   - Alignment shifts

---

## 8. MEDIA & NARRATIVE SYSTEMS

### 8.1 Media System

The game features a dynamic media ecosystem that responds to player actions and world events.

1. **Media Outlets Types**
   - Mainstream Networks (balanced, wide reach)
   - Partisan Publications (biased, targeted reach)
   - Social Media (viral, unpredictable reach)
   - Alternative/Independent (niche, passionate audience)

2. **Media Mechanics**
   - Bias Score (-100 to +100 on partisan scale)
   - Reach Rating (audience size)
   - Focus Area (economy, scandal, social issues, etc.)
   - Ownership (corporate, party-aligned, independent)

3. **Media Interactions**
   - Press Conferences (controlled messaging)
   - Interviews (risk/reward exposure)
   - Leaks (anonymous information)
   - Ad Buys (paid messaging)
   - Social Media Posts (direct but unfiltered)

### 8.2 Scandal Engine

1. **Scandal Generation**
   - Procedurally generated based on player actions
   - Historical vulnerability based on background
   - Contextual triggers from policy decisions
   - Random event integration

2. **Scandal Categories**
   - Financial (corruption, tax issues, conflicts of interest)
   - Personal (affairs, embarrassing behaviors, past mistakes)
   - Policy (harmful effects, unintended consequences)
   - Administrative (staff misconduct, mismanagement)
   - Electoral (voting irregularities, campaign violations)

3. **Scandal Management**
   - Deny (high risk/reward)
   - Apologize (measured impact)
   - Counter-Attack (transfer damage)
   - Distract (temporary relief)
   - Sacrifice Subordinate (contains damage but costs loyalty)

### 8.3 Event Generation

1. **Event Categories**
   - Political (debates, votes, party conventions)
   - Social (protests, cultural shifts, demographic changes)
   - Economic (market changes, industry developments)
   - Global (international relations, conflicts, treaties)
   - Environmental (disasters, climate effects, resources)

2. **Event Adaptation**
   - Events respond to player alignment
   - World state influences probability weights
   - Player history affects event framing
   - Interconnected event chains with branching outcomes

---

## 9. LEGACY SYSTEM

### 9.1 Legacy Unlocks

| Achievement | Requirement | Legacy Perk |
|-------------|-------------|-------------|
| **Clean Politics** | Win campaign with no scandals | "Honest Face" - Start with +15% Public Trust |
| **Machine Politics** | Control party apparatus completely | "Party Connections" - Start with +2 Political Allies |
| **Fall from Grace** | Lose election after major scandal | "Scandal Resilience" - Reduce scandal impact by 20% |
| **Comeback Kid** | Win election after previous defeat | "Underdog Appeal" - +10% polling when behind |
| **Master Manipulator** | Successfully execute 10+ dirty tricks | "Shadow Networks" - Start with 1 blackmail material |
| **People's Champion** | Maintain 70%+ approval for full term | "Grassroots Support" - +25% volunteer effectiveness |
| **Impeached** | Get removed from office | "Political Martyr" - Create sympathetic voter bloc in next run |
| **Supreme Leader** | Establish authoritarian control | "Cult of Personality" - Start with fanatical supporters |
| **Political Dynasty** | Reach highest office 3+ times | "Famous Name" - Unlock dynasty mode (play as descendants) |

### 9.2 Meta-Progression

1. **Political Landscape Evolution**
   - Previous runs influence world state
   - Party platforms shift based on successful strategies
   - Media landscape adapts to previous manipulation

2. **Unlock System**
   - Background options expand with achievements
   - New starting locations become available
   - Special scenario challenges unlock

3. **Record Keeping**
   - Hall of Fame tracking
   - Dynasty history
   - World state evolution

---

## 10. TECHNICAL SPECIFICATIONS

### 10.1 Platform Requirements

**Primary Platform:**
- PC (Windows 10+)
- Mac (macOS 10.14+)

**Minimum System Requirements:**
- CPU: Intel Core i5 or equivalent
- RAM: 8GB
- Storage: 10GB
- GPU: Integrated graphics (dedicated for higher settings)

### 10.2 Development Environment

- **Engine:** Unity 2023.2 or newer
- **Languages:** C# (primary), JavaScript (UI)
- **Version Control:** Git with branching strategy
- **Build Pipeline:** Jenkins with automated testing

### 10.3 Data Architecture

1. **Save System**
   - Cloud synchronization
   - Multiple save slots
   - Auto-save by turn and critical events

2. **Content Management**
   - Modular content packs
   - Event database with tagging system
   - Media message templates

3. **Analytics Integration**
   - Gameplay telemetry
   - Player choice tracking
   - Balance monitoring

### 10.4 Monetization Technical Implementation

1. **In-App Purchase System**
   - Platform-native IAP integration
   - Content entitlement management
   - Offline access to purchased content

2. **Purrkoin Integration** (Optional Crypto Feature)
   - Secure wallet connection
   - Transaction verification
   - Anti-abuse measures

---

## 11. EXPANSION & POST-LAUNCH PLANS

### 11.1 DLC Roadmap

1. **Political Machines** (3 months post-launch)
   - New party dynamics
   - Enhanced electoral mechanics
   - Historical political machines

2. **Global Politics** (6 months post-launch)
   - International expansion
   - Diplomatic relations
   - Multi-country campaigns

3. **Power Behind the Throne** (9 months post-launch)
   - Enhanced shadow government paths
   - Conspiracy gameplay
   - Deep state mechanics

### 11.2 Feature Expansion Timeline

| Timeline | Feature | Description |
|----------|---------|-------------|
| Launch | Core Political Ladder | Full local-to-national progression |
| Launch | Basic Media System | Dynamic news, scandal engine |
| Launch | Primary Alignment System | Hidden Law/Chaos, Good/Evil tracking |
| Launch+3mo | Expanded Staff System | Deeper staff management, betrayals, loyalty |
| Launch+6mo | Multiplayer Campaigning | Competitive election races |
| Launch+9mo | Dynasty Mode | Multi-generation political families |
| Launch+12mo | Constitutional Crisis | Government type modification mechanics |

### 11.3 Community Engagement

1. **Modding Support**
   - Event creation tools
   - Character customization
   - World-state editors

2. **Community Challenges**
   - Weekly scenarios
   - Special electoral conditions
   - Historical reimaginings

3. **Viral Integration**
   - Share your scandals
   - Export your political world
   - Campaign comparison tools

---

## 12. DEVELOPMENT PRIORITIES

### 12.1 MVP Requirements (Phase 1)

1. **Core Political Ladder**
   - Complete tier progression system
   - Basic alignment tracking
   - Fundamental resource management

2. **Procedural Systems**
   - Character generation
   - Basic scandal engine
   - Simple event system

3. **Turn-Based Mechanics**
   - Action point system
   - Election cycle management
   - Crisis handling

### 12.2 Phase 2 Enhancements

1. **Enhanced Media System**
   - Full procedural headline generation
   - Media ownership mechanics
   - Social media simulation

2. **Deep Background System**
   - Complete professional background options
   - Origin story customization
   - Starting relationship networks

3. **Legacy Implementation**
   - Permanent progression system
   - Achievement-based unlocks
   - World state persistence

### 12.3 Phase 3 Refinements

1. **Monetization Integration**
   - Optional Purrkoin implementation
   - Premium content packs
   - Cosmetic system

2. **Advanced AI Opponents**
   - Adaptive rival strategies
   - Personality-driven decision making
   - Coalition formation behavior

3. **Narrative Depth**
   - Branching story events
   - Character development arcs
   - World history simulation

---

## 13. RISK ASSESSMENT & MITIGATION

| Risk Area | Potential Issues | Mitigation Strategy |
|-----------|------------------|---------------------|
| **Balancing** | Career paths imbalanced, resources too easy/hard to acquire | Extensive playtesting, telemetry analysis, rapid patching capability |
| **Content Volume** | Repetitive events, predictable scandals | Procedural generation systems, modular content design, community content tools |
| **Learning Curve** | Complex systems overwhelming new players | Tiered tutorial system, difficulty settings, contextual helper system |
| **Technical Performance** | Slowdown with complex simulation | Optimization sprints, scalable content loading, performance profiling |
| **Monetization Reception** | Player backlash to monetization model | Ethical design focus, non-predatory systems, value transparency |

---

## 14. APPENDICES

### Appendix A: Detailed Background System

[Full breakdown of all starting backgrounds, their stats, and special abilities]

### Appendix B: Complete Political Ladder

[Detailed progression tree with all branches and requirements]

### Appendix C: Alignment Consequence Matrix

[Complete table of how alignment affects events, options, and endings]

### Appendix D: Resource Economy Balancing

[Mathematical models for resource generation, usage, and balanced progression]

### Appendix E: Glossary of Terms

[Definitions of all game-specific terminology and systems]