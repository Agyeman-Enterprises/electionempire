# ELECTION EMPIRE
## Visual Artist Brief

**Document Version:** 1.0  
**Project:** Election Empire  
**Developer:** Agyeman Enterprises LLC  
**Contact:** [Your Email]  
**Date:** December 2024

---

# OVERVIEW

## About the Game

**Election Empire** is a darkly satirical political strategy game where players climb from local politics to the presidency using strategy, manipulation, and tactical maneuvering. Think *Crusader Kings III* meets *Veep* meets editorial cartoons.

**Tagline:** "Politics is a dirty game. Time to get your hands filthy."

**Platform:** PC (Steam), potential mobile  
**Engine:** Unity  
**Target Release:** 2025

## Tone & Feel

| Aspect | Description |
|--------|-------------|
| **Genre** | Political strategy / satire |
| **Mood** | Cynical humor, power fantasy, dark comedy |
| **NOT** | Grimdark, depressing, preachy, partisan |
| **Think** | House of Cards + The Simpsons + MAD Magazine |

**Key Emotional Beats:**
- Scheming and plotting (delicious)
- Scandal survival (tense, then relieved)
- Election victory (triumphant)
- Getting caught (comedic doom)
- Rising to power (intoxicating)

---

# ART DIRECTION

## Visual Style Summary

**Primary Style:** Satirical Caricature with Clean Modern UI

We want exaggerated, memorable character portraits paired with a polished, readable interface. The characters are the stars—they should pop against clean backgrounds and UI elements.

**Visual Pillars:**

1. **EXAGGERATION** - Big features, distinctive silhouettes, memorable faces
2. **EXPRESSIVENESS** - Characters wear their emotions openly
3. **READABILITY** - Clear at small sizes, instant recognition
4. **PERSONALITY** - Every face tells a story
5. **DARK HUMOR** - Cynical content, inviting presentation

## Style References

### Primary References (Capture This Feel)

**Spitting Image (TV Series)**
- Grotesque but lovable caricatures
- Exaggerated features that capture personality
- Political satire through visual design

**Renowned Explorers (Game)**
- Layered character portraits
- Expressive, readable at small sizes
- Consistent style across many characters

**Editorial Political Cartoons**
- Artists: Steve Brodner, David Levine, Pat Oliphant
- Captures essence in exaggeration
- Tells story through visual shorthand

**Slay the Spire (Game)**
- Bold lines, strong silhouettes
- Character personality in every pose
- Works at multiple scales

### Secondary References (Borrow Elements)

| Reference | What to Borrow |
|-----------|----------------|
| Crusader Kings III | Portrait layering system, gravitas |
| Hades | UI polish, color usage, effects |
| Darkest Dungeon | Dramatic lighting, stress/tension |
| Tropico series | Political humor, bright colors |
| Disco Elysium | Painterly backgrounds, mood |

### What We're NOT Going For

❌ Realistic proportions  
❌ Anime/manga style  
❌ Pixel art  
❌ Chibi/super-deformed  
❌ Generic mobile game art  
❌ Photo-realistic  
❌ Grimdark/horror aesthetic

---

# COLOR PALETTE

## Primary Colors

```
DEEP NAVY        #1a1a2e    Primary backgrounds, authority, power
CRIMSON RED      #c41e3a    Scandals, crises, warnings, passion
GOLD             #d4af37    Wealth, success, achievements, premium
```

## Secondary Colors

```
SLATE GRAY       #4a5568    UI panels, neutral elements
CREAM WHITE      #f5f5dc    Text, highlights, clean space
FOREST GREEN     #2d5a27    Money, growth, positive outcomes
```

## Accent Colors

```
ELECTRIC PURPLE  #8b5cf6    Chaos mode, special events, wild cards
WARNING ORANGE   #f59e0b    Alerts, time pressure, urgent
TEAL             #14b8a6    Media, information, neutral-positive
```

## Skin Tone Range

Portraits must support diverse representation:

```
PALE             #ffe4c4
LIGHT            #deb887  
MEDIUM LIGHT     #c19a6b
MEDIUM           #a0785a
MEDIUM DARK      #8b6914
DARK             #654321
DEEP             #3d2314
RICH             #2c1810
DEEP RICH        #1a0f0a
```

These should be achievable through base art + color tinting in-engine.

---

# CHARACTER PORTRAITS

## System Overview

Portraits are **procedurally generated** from modular components. Each portrait is assembled from layers that combine to create unique characters.

**This is critical:** All components must:
- Share consistent anchor points
- Use identical line weights
- Follow the same style guide
- Be tintable where appropriate
- Work in combination with ALL other pieces

## Layer Structure (Bottom to Top)

```
Layer 01: Background (separate system)
Layer 02: Body/Shoulders (if visible)
Layer 03: Face Shape (base head)
Layer 04: Skin Texture/Details
Layer 05: Ears
Layer 06: Hair - Back Layer
Layer 07: Eyes
Layer 08: Eyebrows
Layer 09: Nose
Layer 10: Mouth
Layer 11: Facial Hair
Layer 12: Hair - Front Layer
Layer 13: Accessories (glasses, earrings, etc.)
Layer 14: Expression Overlay (sweat, blush, etc.)
```

## Component Requirements

### Face Shapes (9 required)

| ID | Shape | Notes |
|----|-------|-------|
| F01 | Oval | Classic, neutral base |
| F02 | Round | Soft, approachable |
| F03 | Square | Strong, authoritative |
| F04 | Oblong | Elongated, distinctive |
| F05 | Heart | Pointed chin, wide forehead |
| F06 | Diamond | Angular, sharp |
| F07 | Pear | Wide jaw, narrow forehead |
| F08 | Rectangle | Long, serious |
| F09 | Triangle | Wide forehead, narrow chin |

**Specifications:**
- Canvas size: 512x512 pixels
- Centered, consistent positioning
- Slight 3/4 angle (not straight-on)
- Room for all feature placements

### Hair Styles (16 required)

| ID | Style | Gender | Notes |
|----|-------|--------|-------|
| H01 | Short Professional | M/F | Conservative politician |
| H02 | Slicked Back | M | Power broker look |
| H03 | Combover | M | Trying to hide something |
| H04 | Bald | M | Clean, powerful |
| H05 | Buzz Cut | M | Military/no-nonsense |
| H06 | Wavy Medium | M | Kennedy-esque |
| H07 | Messy Politician | M | Just got caught |
| H08 | Bob Cut | F | Professional woman |
| H09 | Long Straight | F | Composed, formal |
| H10 | Updo/Bun | F | Serious, authoritative |
| H11 | Big Texas Hair | F | Southern politician |
| H12 | Pixie Cut | F | Modern, fresh |
| H13 | Curly/Natural | M/F | Distinctive, memorable |
| H14 | Gray Distinguished | M/F | Elder statesperson |
| H15 | Wild/Unkempt | M/F | Chaos mode/crisis |
| H16 | Ponytail | M/F | Casual/populist |

**Specifications:**
- Split into BACK and FRONT layers
- Must work with all face shapes
- Tintable to any hair color
- Include gray/white variants

### Eye Shapes (9 required)

| ID | Shape | Expression Base |
|----|-------|-----------------|
| E01 | Standard/Neutral | Default calm |
| E02 | Wide/Alert | Surprised base |
| E03 | Narrow/Suspicious | Scheming base |
| E04 | Tired/Droopy | Exhausted base |
| E05 | Sharp/Intense | Aggressive base |
| E06 | Soft/Friendly | Approachable base |
| E07 | Beady/Small | Untrustworthy |
| E08 | Large/Innocent | "Who, me?" |
| E09 | Hooded | Mysterious |

**Specifications:**
- Include eyebrows as separate layer
- Must show expression range
- Consistent eye spacing across all

### Eye Colors (8 required)
- Brown, Blue, Green, Hazel, Gray, Amber, Dark Brown, Light Blue
- Achievable through tinting

### Nose Types (11 required)

| ID | Type | Character Implication |
|----|------|----------------------|
| N01 | Standard | Neutral |
| N02 | Bulbous | Drinker, jolly |
| N03 | Pointed | Sharp, cunning |
| N04 | Upturned | Snooty, elite |
| N05 | Roman/Aquiline | Distinguished |
| N06 | Wide | Strong, grounded |
| N07 | Small | Delicate |
| N08 | Crooked | Fighter, survivor |
| N09 | Long | Nosy, inquisitive |
| N10 | Button | Youthful |
| N11 | Hawk | Predatory |

### Mouth Types (8 required)

| ID | Type | Resting Expression |
|----|------|-------------------|
| M01 | Neutral | Calm |
| M02 | Slight Smile | Friendly |
| M03 | Thin Lips | Stern |
| M04 | Full Lips | Expressive |
| M05 | Downturned | Disapproving |
| M06 | Crooked/Smirk | Scheming |
| M07 | Open/Broad | Talkative |
| M08 | Pursed | Judgmental |

### Facial Hair (12 required)

| ID | Type | Notes |
|----|------|-------|
| FH00 | None | Clean shaven |
| FH01 | 5 O'Clock Shadow | Tired/rugged |
| FH02 | Full Beard | Distinguished |
| FH03 | Goatee | Alternative |
| FH04 | Mustache - Standard | Classic |
| FH05 | Mustache - Handlebar | Distinctive |
| FH06 | Mustache - Pencil | Slick |
| FH07 | Soul Patch | Trying too hard |
| FH08 | Mutton Chops | Old fashioned |
| FH09 | Stubble | Just campaigning |
| FH10 | Van Dyke | Intellectual |
| FH11 | Long Beard | Outsider/prophet |

### Age Variations (5 overlays)

Applied as texture overlays:
- **Young (25-35):** Smooth, minimal lines
- **Middle (36-50):** Some lines, character
- **Mature (51-65):** Pronounced lines, gravitas  
- **Senior (66-80):** Heavy lines, jowls
- **Elder (80+):** Deep wrinkles, weathered

### Basic Accessories

**Glasses:**
- Rimless
- Wire frame
- Thick frame
- Reading glasses (half-moon)
- Sunglasses

**Other:**
- Earrings (studs, hoops)
- Hearing aid
- Bandage (post-scandal)
- Campaign button

---

# EXPRESSIONS

## Expression System

Characters must convey emotion through changeable components. The system swaps/modifies eyes, eyebrows, and mouth to create expressions.

## Required Expressions (24)

| ID | Expression | Usage |
|----|------------|-------|
| EX01 | Neutral | Default state |
| EX02 | Happy | Victory, good news |
| EX03 | Angry | Attacked, crisis |
| EX04 | Sad | Defeat, loss |
| EX05 | Shocked | Scandal revealed |
| EX06 | Smug | Winning, scheming |
| EX07 | Worried | Polls dropping |
| EX08 | Determined | Campaign mode |
| EX09 | Confused | Unexpected event |
| EX10 | Disgusted | Opponent's tactics |
| EX11 | Fearful | Investigation |
| EX12 | Triumphant | Major victory |
| EX13 | Exhausted | Crisis mode |
| EX14 | Suspicious | Opponent action |
| EX15 | Pleading | Asking for support |
| EX16 | Defiant | Under attack |
| EX17 | Scheming | Planning dirty trick |
| EX18 | Innocent | "I did nothing wrong" |
| EX19 | Guilty | Caught |
| EX20 | Drunk | Chaos mode |
| EX21 | Manic | Chaos mode extreme |
| EX22 | Devastated | Career ended |
| EX23 | Villainous | Evil alignment |
| EX24 | Saintly | Good alignment |

## Expression Effects (Overlays)

| Effect | Visual | Usage |
|--------|--------|-------|
| Sweat Drops | Beads on forehead | Stress, caught |
| Blush | Red cheeks | Embarrassed |
| Pallor | Pale/gray tint | Sick, scared |
| Tears | Streaming | Sad/crocodile tears |
| Steam | Ears steaming | Angry |
| Stars | Circling head | Stunned |
| Dollar Signs | In eyes | Greedy/money |
| Hearts | In eyes | Charmed |

---

# PORTRAIT EFFECTS

## Frame Effects

Portraits appear in different contexts:

| Effect | Description | Usage |
|--------|-------------|-------|
| Normal | Standard portrait | Default |
| Spotlight | Dramatic lighting | Speech, debate |
| Shadow | Darkened, ominous | Evil actions |
| Newspaper | Halftone dots, B&W | Media coverage |
| Mugshot | Harsh lighting, numbers | Arrested |
| Campaign Poster | Stylized, patriotic | Campaign materials |
| Wanted Poster | Old West style | Scandal |
| Pixelated | Privacy blur | Hidden identity |
| Redacted | Black bars over eyes | Classified |
| Memorial | Soft, faded, RIP | Political death |

---

# UI DESIGN

## Overall Aesthetic

- **Clean and Modern** with subtle political motifs
- **Art Deco influences** in frames and borders
- **Newspaper typography** for headlines
- **Highly readable** - function over form

## UI Components Needed

### Buttons
- Primary (gold accent)
- Secondary (navy)
- Danger (red)
- Disabled (gray)
- States: Normal, Hover, Pressed, Disabled

### Panels
- Main content panels (dark, subtle border)
- Popup/modal panels (lighter, prominent)
- Tooltip panels (small, temporary)

### Icons (200+)
*See separate icon specification document*

Categories:
- Resources (trust, capital, funds, dirt, etc.)
- Actions (campaign, govern, investigate, etc.)
- Voter blocs (12 groups)
- Offices (all tiers)
- Achievements
- Cosmetics
- Navigation
- Status effects

### Progress Bars
- Resource bars (colored by type)
- Election polling bars
- Timer bars
- Loading bars

### Typography

| Use | Font Style | Example |
|-----|------------|---------|
| Headlines | Bold Serif | "SCANDAL ROCKS CAMPAIGN" |
| Body Text | Clean Sans | "Your approval rating..." |
| Numbers | Tabular Sans | "$1,234,567" |
| Buttons | Medium Sans | "Start Campaign" |
| Captions | Light Sans | "Updated 2 turns ago" |

Suggested fonts:
- Headlines: Playfair Display, Libre Baskerville
- Body: Inter, Source Sans Pro, Open Sans

---

# BACKGROUNDS

## Scene List

| Scene | Mood | Key Elements |
|-------|------|--------------|
| Main Menu | Grand, slightly ominous | Capitol silhouette, dramatic sky |
| Campaign Office T1 | Scrappy, grassroots | Folding tables, pizza boxes, phones |
| Campaign Office T3 | Professional | Glass walls, monitors, staff bustling |
| Campaign Office T5 | War room | Massive screens, situation room |
| City Council | Fluorescent, mundane | Cheap chairs, wood paneling |
| Mayor's Office | First taste of power | Flag, city photos, nicer desk |
| Governor's Office | Imposing | Portraits, dark wood, state flag |
| Senate Floor | Ultimate power | Marble, columns, desks |
| Oval Office | Peak | Iconic, reverent |
| Debate Stage | Gladiatorial | Podiums, audience, lights |
| Rally Venue | Energetic | Crowd, banners, confetti |
| Press Room | Clinical | Podium, microphones, cameras |
| Crisis Room | Tense | Red lighting, screens, phones |

**Style:** Painterly, atmospheric, don't compete with portraits

---

# DELIVERABLE SPECIFICATIONS

## File Formats

| Asset Type | Source Format | Export Format |
|------------|---------------|---------------|
| Portraits | PSD (layered) | PNG (transparent) |
| UI Elements | Figma/PSD | PNG, SVG |
| Backgrounds | PSD | PNG, JPG |
| Icons | AI/Figma | SVG, PNG |

## Resolution Requirements

| Asset | Size | Notes |
|-------|------|-------|
| Portrait components | 512x512 | 2x for retina |
| Full portraits | 512x512 | Assembled |
| UI elements | Varies | 2x versions |
| Backgrounds | 1920x1080 min | 4K preferred |
| Icons | 64x64, 128x128 | Multiple sizes |

## Naming Convention

```
category_subcategory_variant_state.png

Examples:
portrait_face_oval_base.png
portrait_hair_slicked_back.png
portrait_expression_angry_eyes.png
ui_button_primary_hover.png
bg_office_governor_day.png
icon_resource_trust_64.png
```

---

# PROJECT SCOPE

## Phase 1: Character Portraits (Priority)

**Deliverables:**
- 9 face shapes
- 16 hair styles (front + back)
- 9 eye shapes
- 11 nose types
- 8 mouth types
- 12 facial hair options
- 5 age overlays
- 24 expression sets
- 8 effect overlays
- Basic accessories (10-15 items)

**Total Components:** ~150-200 individual assets

**Timeline:** 6-10 weeks

**Budget Range:** $4,000 - $8,000

## Phase 2: UI Kit

**Deliverables:**
- Complete button set (4 types × 4 states)
- Panel designs (5 types)
- 200+ icons
- Progress bars and meters
- Typography guidelines
- Component library

**Timeline:** 3-5 weeks

**Budget Range:** $2,000 - $4,000

## Phase 3: Backgrounds

**Deliverables:**
- 15-20 scene backgrounds
- Day/night variants for key scenes
- Parallax layers where appropriate

**Timeline:** 4-6 weeks

**Budget Range:** $2,500 - $5,000

---

# EVALUATION CRITERIA

## What We're Looking For

1. **Style Match** - Captures satirical caricature tone
2. **Consistency** - All pieces feel unified
3. **Expressiveness** - Characters have personality
4. **Technical Skill** - Clean lines, proper formatting
5. **Reliability** - Delivers on time, communicates well

## Test Assignment

Before full engagement, we'll request:

**Paid Test ($150-300):**
Create ONE complete character with:
- 1 face shape
- 1 hair style (front + back)
- 1 set of features (eyes, nose, mouth)
- 3 expressions (neutral, happy, angry)

This tests style, layer compatibility, and working relationship.

---

# CONTACT & NEXT STEPS

## To Apply

Please send:
1. Portfolio link (relevant work)
2. Rate/pricing structure
3. Availability/timeline
4. Brief note on what interests you about this project

## Questions?

Contact: [Your Email]
Discord: [If applicable]
Website: [If applicable]

---

**Thank you for your interest in Election Empire!**

*"Politics is a dirty game. Time to get your hands filthy."*
