# ELECTION EMPIRE: AI ASSET GENERATION PIPELINE
## Practical Guide to Generating Game Art with AI

---

## OVERVIEW

**Goal:** Generate all visual assets for Election Empire using AI image generation
**Budget:** $400-600 in API costs (vs $15-25K for traditional artists)
**Timeline:** 1-2 weeks of generation + curation
**Output:** Standard game assets (PNG/JPG files) - no runtime AI needed

---

## STEP 1: SET UP YOUR GENERATION PIPELINE

### Recommended Tools

| Tool | Best For | Cost | Sign Up |
|------|----------|------|---------|
| **Midjourney** | Highest quality, best style control | $10/mo basic | discord.gg/midjourney |
| **Leonardo.ai** | Fast iteration, good for bulk | Free tier + $12/mo | leonardo.ai |
| **DALL-E 3** | Reliable, good text rendering | Pay per use ~$0.04 | platform.openai.com |
| **Stable Diffusion** | Free if local, very customizable | Free (local) | Use ComfyUI or Automatic1111 |

### My Recommendation: Midjourney + Leonardo

- **Midjourney** for hero assets (main characters, key locations)
- **Leonardo.ai** for bulk generation (NPCs, variants, icons)
- Total cost: ~$25/month subscriptions + usage

---

## STEP 2: ESTABLISH YOUR ART STYLE

### Create a Style Guide First

Generate 5-10 "reference" images that define your look:

```
STYLE PROMPT (use this as base for everything):
"Political caricature illustration, editorial cartoon style, 
exaggerated features, bold outlines, satirical, 
New Yorker magazine aesthetic, slightly grotesque, 
professional illustration, white background --ar 3:4"
```

### Style Variations to Test

1. **Classic Editorial Cartoon**
   - Bold lines, crosshatching, newspaper feel
   
2. **Modern Satirical**  
   - Cleaner lines, vibrant colors, digital illustration
   
3. **Grotesque Caricature**
   - Exaggerated features, ugly-beautiful, memorable
   
4. **Propaganda Poster**
   - Bold, striking, iconic silhouettes

Pick ONE and stick with it for consistency.

---

## STEP 3: CHARACTER PORTRAIT GENERATION

### Player Character Archetypes

Generate base portraits for each background:

```
BUSINESSMAN:
"Political caricature portrait of a wealthy businessman politician, 
expensive suit, slicked back hair, smug expression, gold watch, 
power tie, corporate shark energy, editorial cartoon style, 
exaggerated features, white background --ar 3:4"

TEACHER:
"Political caricature portrait of a teacher turned politician,
cardigan sweater, reading glasses, earnest expression, 
chalk dust on clothes, idealistic eyes, editorial cartoon style,
exaggerated features, white background --ar 3:4"

ACTIVIST:
"Political caricature portrait of a grassroots activist politician,
protest signs in background, passionate expression, megaphone,
tie-dye visible, righteous anger, editorial cartoon style,
exaggerated features, white background --ar 3:4"

[... continue for all 8 backgrounds]
```

### Generate Variations

For each archetype, generate:
- 3 age variations (30s, 50s, 70s)
- 2 gender presentations
- 3 expression sets (confident, worried, angry)
- Multiple ethnic representations

**Total: ~50-80 player portraits**

### NPC Gallery

```
TOWNSFOLK TYPES:

ANGRY VOTER:
"Political caricature of an angry working class voter,
red-faced, pointing finger, veins visible, baseball cap,
blue collar clothes, righteous fury, editorial cartoon style --ar 3:4"

WEALTHY DONOR:
"Political caricature of a wealthy political donor,
champagne glass, smug smile, expensive jewelry, country club attire,
expects favors, editorial cartoon style --ar 3:4"

INTREPID REPORTER:
"Political caricature of an aggressive investigative journalist,
notepad ready, suspicious squint, rumpled suit, coffee-stained,
hunting for scandal, editorial cartoon style --ar 3:4"

DRUNK HECKLER:
"Political caricature of a drunk heckler at a political rally,
beer in hand, swaying, slurred expression, disheveled,
belligerent energy, editorial cartoon style --ar 3:4"

CONSPIRACY THEORIST:
"Political caricature of a conspiracy theorist at town hall,
wild eyes, folder of 'evidence', tinfoil hat adjacent energy,
manic expression, editorial cartoon style --ar 3:4"

RELIGIOUS LEADER:
"Political caricature of a megachurch pastor in politics,
expensive suit, big hair, Bible in hand, prosperity gospel energy,
commanding presence, editorial cartoon style --ar 3:4"

[... continue for all encounter types]
```

**Total: ~150-200 NPC portraits**

---

## STEP 4: LOCATION/BACKGROUND GENERATION

### Campaign Trail Locations

```
TOWN HALL MEETING:
"Interior of a small town American town hall during heated meeting,
folding chairs, angry constituents, fluorescent lighting,
American flags, podium, tense atmosphere, editorial illustration style,
wide shot, establishing shot --ar 16:9"

FACTORY FLOOR:
"Abandoned rust belt factory floor, broken windows,
remnants of industry, politician visiting workers,
economic anxiety atmosphere, gritty, editorial illustration --ar 16:9"

DINER VISIT:
"Classic American diner interior, checkered floor, 
red vinyl booths, coffee counter, working class patrons,
campaign photo op setting, nostalgic but worn --ar 16:9"

COUNTY FAIR:
"American county fair grounds, ferris wheel, food stalls,
bunting and flags, diverse crowd, summer day,
campaign rally backdrop, editorial illustration --ar 16:9"

CHURCH BASEMENT:
"Church basement meeting room, folding tables,
coffee urns, fluorescent lights, American heartland,
community gathering space, editorial illustration --ar 16:9"

[... continue for all location types]
```

### Office/Governance Locations

```
CITY COUNCIL CHAMBER:
"Small city council chamber, semicircle of desks,
local politicians, modest government building,
fluorescent lighting, American flags --ar 16:9"

GOVERNOR'S OFFICE:
"Imposing state governor's office, mahogany desk,
state seal, power and authority, leather chairs,
American flags, formal setting --ar 16:9"

OVAL OFFICE:
"Oval Office interior, resolute desk, presidential seal,
ultimate political power, American flags,
iconic setting, editorial illustration style --ar 16:9"
```

**Total: ~50-75 backgrounds**

---

## STEP 5: UI ELEMENTS

### Icons (Use DALL-E for text accuracy)

```
RESOURCE ICONS:
"Simple icon of a money bag with dollar sign, 
political cartoon style, bold outline, 
transparent background, game UI element"

"Simple icon of a newspaper with 'SCANDAL' headline,
political cartoon style, bold outline,
transparent background, game UI element"

"Simple icon of a ballot box with vote going in,
political cartoon style, bold outline,
transparent background, game UI element"

"Simple icon of a handshake deal,
political cartoon style, bold outline,
transparent background, game UI element"
```

### Decorative Elements

```
"Patriotic banner decoration, red white and blue,
stars and stripes, campaign rally style,
game UI border element, horizontal"

"Political campaign button collection, various slogans,
red and blue colors, vintage campaign aesthetic,
decorative game element"
```

**Total: ~100-150 UI elements**

---

## STEP 6: EXPRESSION SYSTEM

For key characters, generate expression variants:

### Expression Prompts

Take your base character and add:

```
CONFIDENT: "...confident smirk, winning expression, triumphant..."
WORRIED: "...furrowed brow, anxious eyes, sweating slightly..."  
ANGRY: "...furious expression, clenched jaw, red-faced..."
SHOCKED: "...wide eyes, open mouth, caught off guard..."
SCHEMING: "...sly smile, narrowed eyes, plotting..."
DEFEATED: "...slumped shoulders, tired eyes, beaten..."
INSPIRING: "...arms raised, triumphant, rallying cry..."
CORRUPT: "...shifty eyes, counting money, sleazy..."
```

**Total: ~200-300 expression variants for main characters**

---

## STEP 7: BATCH GENERATION WORKFLOW

### Using Midjourney

```
1. Set up private Discord server
2. Create channels for each asset type:
   #player-portraits
   #npc-portraits  
   #locations
   #ui-elements

3. Use /imagine with your prompts
4. Use "Make Variations" (V1-V4) for options
5. Use "Upscale" (U1-U4) for finals
6. Download and organize
```

### Using Leonardo.ai

```
1. Create "Generation" for each category
2. Use "PhotoReal" or "Illustration" model
3. Enable "Prompt Magic" for better results
4. Generate in batches of 4-8
5. Use "Alchemy" for upscaling
6. Download via bulk download
```

### File Organization

```
ElectionEmpire/
├── Art/
│   ├── Characters/
│   │   ├── Player/
│   │   │   ├── Businessman/
│   │   │   │   ├── businessman_male_40_confident.png
│   │   │   │   ├── businessman_male_40_worried.png
│   │   │   │   ├── businessman_female_50_angry.png
│   │   │   │   └── ...
│   │   │   ├── Teacher/
│   │   │   ├── Doctor/
│   │   │   └── ...
│   │   ├── NPCs/
│   │   │   ├── Voters/
│   │   │   ├── Staff/
│   │   │   ├── Rivals/
│   │   │   └── Reporters/
│   │   └── Expressions/
│   ├── Locations/
│   │   ├── CampaignTrail/
│   │   ├── Offices/
│   │   └── Events/
│   ├── UI/
│   │   ├── Icons/
│   │   ├── Buttons/
│   │   └── Decorative/
│   └── StyleGuide/
│       ├── reference_1.png
│       └── color_palette.png
```

---

## STEP 8: POST-PROCESSING

### Consistency Pass

1. **Color Correction**
   - Batch adjust all images to match color palette
   - Use Photoshop Actions or free alternatives (GIMP, Photopea)

2. **Background Removal** (for portraits)
   - Use remove.bg (free tier) or Photoshop
   - Export with transparency

3. **Resolution Standardization**
   - Portraits: 512x512 or 1024x1024
   - Backgrounds: 1920x1080
   - Icons: 128x128 or 256x256

4. **Style Filter** (optional)
   - Run all through same filter for consistency
   - Photoshop Neural Filters or similar

### Quality Control Checklist

For each asset, verify:
- [ ] Matches established art style
- [ ] No AI artifacts (weird hands, text gibberish)
- [ ] Appropriate for game rating
- [ ] Correct aspect ratio
- [ ] Transparent background (where needed)
- [ ] Named correctly for asset pipeline

---

## STEP 9: COST TRACKING

### Budget Template

```
GENERATION COSTS
────────────────
Midjourney subscription: $30/month × 2 months = $60
Leonardo.ai subscription: $12/month × 2 months = $24
DALL-E API usage: ~$50
Remove.bg credits: $20 (or free tier)
────────────────
Subtotal: ~$154

CONTINGENCY (2x for iterations): $154
────────────────
TOTAL BUDGET: ~$300-400
```

### Tracking Spreadsheet

| Asset Type | Needed | Generated | Approved | Remaining |
|------------|--------|-----------|----------|-----------|
| Player Portraits | 50 | 0 | 0 | 50 |
| NPC Portraits | 200 | 0 | 0 | 200 |
| Locations | 60 | 0 | 0 | 60 |
| UI Icons | 100 | 0 | 0 | 100 |
| Expressions | 300 | 0 | 0 | 300 |

---

## STEP 10: INTEGRATION WITH UNITY

### Sprite Atlas Setup

```csharp
// CharacterPortraitManager.cs
public class CharacterPortraitManager : MonoBehaviour
{
    [SerializeField] private Sprite[] businessmanPortraits;
    [SerializeField] private Sprite[] teacherPortraits;
    [SerializeField] private Sprite[] npcVoterPortraits;
    // etc.
    
    public Sprite GetPortrait(CharacterBackground background, 
                               int ageGroup, 
                               Expression expression)
    {
        // Map to correct sprite based on parameters
        string spriteName = $"{background}_{ageGroup}_{expression}";
        return Resources.Load<Sprite>($"Portraits/{spriteName}");
    }
}
```

### Addressables (Recommended for large asset count)

```csharp
// Load portraits on demand
public async Task<Sprite> LoadPortraitAsync(string portraitId)
{
    var handle = Addressables.LoadAssetAsync<Sprite>(portraitId);
    await handle.Task;
    return handle.Result;
}
```

---

## TIMELINE

### Week 1: Foundation
- Day 1-2: Establish art style, create reference images
- Day 3-4: Generate all player portraits
- Day 5-6: Generate NPC portraits (batch 1)
- Day 7: Quality review, regenerate failures

### Week 2: Completion  
- Day 1-2: Complete NPC portraits, expressions
- Day 3-4: Generate all locations
- Day 5: Generate UI elements
- Day 6: Post-processing pass
- Day 7: Integration into Unity

---

## TIPS FOR BEST RESULTS

### Prompt Engineering

1. **Be specific about style**
   - Always include "editorial cartoon style" or your chosen style
   - Reference real artists/publications if needed
   
2. **Use negative prompts**
   - "--no realistic, 3D, anime, photograph"
   
3. **Consistent framing**
   - Same aspect ratio for same asset types
   - Same composition guidelines
   
4. **Batch similar items**
   - Generate all "angry voters" together
   - Maintain consistency within category

### Common Issues & Fixes

| Issue | Solution |
|-------|----------|
| Hands look weird | Crop hands out, or use "--no hands" |
| Text is gibberish | Don't include text in image; add in post |
| Style inconsistent | Add style reference image with --sref |
| Too realistic | Add "illustration, cartoon, stylized" |
| AI artifacts | Regenerate or fix in Photoshop |

---

## LEGAL CONSIDERATIONS

### You Own the Output

- Midjourney: You own commercial rights (paid plans)
- DALL-E: You own the images you generate
- Leonardo: You own commercial rights (paid plans)
- Stable Diffusion: Fully open, you own everything

### Best Practices

1. Don't copy specific real people's likenesses
2. Don't copy trademarked characters/logos
3. Keep generation prompts for your records
4. Consider noting "AI-assisted art" in credits

---

## SUMMARY

**Total Cost: $300-600** (vs $15,000-25,000)
**Total Time: 1-2 weeks** (vs 2-3 months)
**Output: 700-1000 unique assets**

This approach gives you:
- ✅ Professional-looking game art
- ✅ Consistent style throughout
- ✅ Unlimited variations possible
- ✅ 97% cost reduction
- ✅ Ships as normal assets (no runtime AI needed)
- ✅ No ongoing API costs for players

The game doesn't know or care that AI made the art.
Players just see a polished political game with a distinctive style.
