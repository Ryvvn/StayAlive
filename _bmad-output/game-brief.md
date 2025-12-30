---
stepsCompleted: [1, 2, 3, 4, 5, 6, 7, 8]
inputDocuments: []
documentCounts:
  brainstorming: 0
  research: 0
  notes: 0
workflowType: 'game-brief'
lastStep: 8
project_name: 'StayAlive'
user_name: 'Ryan'
date: '2025-12-27'
game_name: 'StayAlive'
status: 'complete'
---

# Game Brief: StayAlive

**Date:** 2025-12-27
**Author:** Ryan
**Status:** Complete - Ready for GDD Development

---

## Executive Summary

**StayAlive** is a roguelike multiplayer survival FPS where players defend their tower at night and gather resources by day, combining co-op chaos, tower defense, and survival mechanics.

**Target Audience:** Ages 13-30, casual to hardcore gamers seeking co-op fun with friends

**Core Pillars:** Co-op Chaos, Roguelike Variety, Tower Defense, Rewarding Exploration

**Key Differentiators:** Genre mashup (FPS + tower defense + roguelike + survival) and unique co-op experiences every run

**Platform:** PC (Steam)

**Success Vision:** Get StayAlive on Steam with wishlists, build a community through devlogs, deliver the co-op tower defense roguelike FPS that Muck could have been.

---

## Game Vision

### Core Concept

A roguelike multiplayer survival FPS where players defend their tower at night and gather resources by day, featuring both victory-condition and endless game modes.

### Elevator Pitch

StayAlive is a multiplayer roguelike survival FPS where you and your friends defend your tower against waves of enemies at night, then venture out during the day to gather resources, craft weapons, and upgrade your defenses. Choose your path: survive the final wave for victory, or test your limits in endless mode. Every run is procedurally generated - no two games are the same.

### Vision Statement

StayAlive aims to deliver the ultimate "one more run" experience - a game where players are constantly driven to push further, try harder difficulties, and perfect their strategies. More than just survival, it's about creating memorable moments with friends: clutch saves, perfect builds, and triumphant victories against impossible odds. Inspired by Muck's untapped potential, StayAlive takes the survival roguelike formula and elevates it with deep FPS mechanics, meaningful progression, and endless replayability.

---

## Target Market

### Primary Audience

**Demographics:** Ages 13-30, primarily teens and young adults who enjoy gaming with friends.

**Gaming Preferences:**
- Casual to hardcore spectrum - easy mode for chill sessions, hard/endless mode for challenge seekers
- Quick session friendly (like Muck) - pick up and play
- Low barrier to entry - no deep roguelike knowledge required
- Social/co-op focused - gaming as social activity

**Motivations:** Players seeking tower defense and co-op multiplayer survival experiences to have fun with friends. Value both accessibility and depth.

### Secondary Audience

**Content Creators & Competitive Players**
- Streamers and YouTubers attracted by roguelike variety and co-op chaos
- Competitive players seeking hardcore challenges and achievements
- Speedrunners and leaderboard chasers

*Design consideration: Achievements, challenges, and leaderboards can attract this audience with minimal development effort.*

### Market Context

**Similar Successful Games:**
- **Muck** - Core inspiration, proves market for accessible co-op survival roguelike
- **Rust** - Demonstrates appetite for survival/building mechanics

**Market Opportunity:**
Tower defense roguelike with co-op experiences - a niche that combines popular genres in an underserved way. The dual casual/hardcore approach widens the addressable market.

**Discoverability Strategy:**
- Steam tags: Roguelike, Co-op, Survival, Tower Defense, FPS
- YouTube devlog channel documenting development process
- Community building before launch

---

## Game Fundamentals

### Core Gameplay Pillars

1. **Co-op Chaos** - Multiplayer mayhem is the heart of the experience. Friends working together (and hilariously failing together).

2. **Roguelike Variety** - Every run is different. Procedural generation, varied loot, and strategic choices keep players coming back.

3. **Tower Defense** - Building and defending your base is core to survival. Strategic placement and upgrades matter.

4. **Rewarding Exploration** - The world rewards those who venture out. Risk vs reward during dangerous resource runs.

**Pillar Priority:** When pillars conflict: Co-op Chaos > Tower Defense > Roguelike Variety > Exploration

### Primary Mechanics

| Mechanic | Description |
|----------|-------------|
| **Shoot** | FPS combat against waves of enemies |
| **Build** | Place and upgrade tower defense structures |
| **Gather** | Collect resources during daytime expeditions |
| **Craft** | Create weapons, gear, and base upgrades |
| **Defend** | Survive increasingly difficult night waves |
| **Explore** | Discover procedurally generated world for loot and secrets |

**Core Loop:** Day: Explore → Gather → Craft → Build | Night: Defend → Survive → Repeat

### Player Experience Goals

**Primary Emotions:**
- **Fun & Joy** - Laughing with friends after intense runs
- **Triumph** - Clutch saves and highlight moments that become stories
- **Chaos Comedy** - When friends mess up spectacularly, it's content gold

**Key Design Insight - Respawn Mechanic:**
When a player dies, teammates must retrieve their chip/tag and return it to base to respawn them. This creates:
- Heroic rescue moments
- "Last one standing" tension
- Stories players share forever

**Emotional Journey:** Tension builds through the night → Relief at dawn → Pride in survival → "One more run" motivation

---

## Scope and Constraints

### Target Platforms

**Primary:** PC (Steam)
**Marketing:** itch.io (for visibility, not primary distribution)

### Development Timeline

- 1 month full-time development initially
- Nights/weekends development thereafter
- MVP target: TBD based on scope refinement

### Budget Considerations

**Approach:** Self-funded, bootstrapped development
- Free/low-cost assets from Unity Asset Store, itch.io, web sources
- AI-assisted content generation where viable
- Outsourcing: Minimal to none initially

**Key Constraint:** Art and audio will rely on free assets or AI generation until revenue allows investment.

### Team Resources

**Team Size:** Solo developer
**Availability:** 
- 1 month full-time development
- Nights/weekends thereafter

**Skills:**
- ✅ Programming (primary strength)
- ⚠️ Art (relying on free assets)
- ⚠️ Audio (relying on free assets)
- ⚠️ Marketing (YouTube devlog strategy)

**Skill Gaps:** Art creation, audio/music, marketing experience

### Technical Constraints

**Engine:** Unity
**Networking:** Unity Netcode (prior experience from 2D multiplayer thesis project)

**Technical Considerations:**
- 3D FPS with procedural world generation
- Multiplayer networking (building on existing netcode knowledge)
- Performance optimization for wave defense scenarios

**Technical Risk:** Scaling from 2D thesis project to 3D multiplayer FPS with procedural generation is a significant step up.

---

## Reference Framework

### Inspiration Games

**Muck**
- Taking: Co-op survival loop, quick sessions, accessible gameplay
- Not Taking: Simple melee combat (we want deeper FPS mechanics)

**Team Fight Tactics**
- Taking: Roguelike powerup selection mechanics
- Not Taking: Auto-battler gameplay, competitive PvP focus

**Minecraft**
- Taking: Day/night cycle with mob spawning
- Not Taking: Open-ended sandbox, infinite world

**Rust**
- Taking: Crafting mechanics and resource gathering
- Not Taking: PvP focus, harsh survival, persistent servers

**Kingdom Rush Vengeance**
- Taking: Tower defense upgrade mechanics
- Not Taking: 2D perspective, linear level progression

### Competitive Analysis

**Direct Competitors:**
- **Muck** - Closest comparison, but lacks tower defense depth
- **Valheim** - Longer sessions, more hardcore
- **Deep Rock Galactic** - Co-op FPS, but different loop

**Competitor Strengths:** Muck's accessibility, Valheim's atmosphere, DRG's teamwork

**Gap in Market:** No game combines FPS + tower defense + roguelike + co-op survival in this specific way

### Key Differentiators

1. **Genre Mashup** - Combining co-op chaos, tower defense, and survival in a unique blend. Not just "survival with friends" but strategic base building meets FPS action.

2. **Roguelike Co-op Variety** - Every run is different, so co-op experiences never repeat. Friends build unique stories each session.

**Unique Value Proposition:** "The co-op tower defense roguelike FPS that Muck could have been."

---

## Content Framework

### World and Setting

**Setting:** Island environment - contained scope that can expand later
**Atmosphere:** Colorful, Muck-like vibes - fun chaos over dark survival
**World-building:** Minimal lore, emergent storytelling through gameplay

### Narrative Approach

**Approach:** Minimal/Emergent (like Muck)
- No heavy story - players create their own narratives through co-op experiences
- Story emerges from gameplay moments, not cutscenes
- No dedicated narrative workflow needed

### Content Volume

- Procedurally generated island maps
- Variety through roguelike power-ups and crafting options
- Tower types and upgrade paths
- Enemy wave configurations

---

## Art and Audio Direction

### Visual Style

**Art Style:** Low-poly 3D
**Color Palette:** Colorful, accessible (flexible based on available assets)
**Animation:** Simple, functional
**References:** Muck's visual simplicity

### Audio Style

**Music:** Chill/ambient with intensity during night waves
**SFX:** Functional, arcade-y feedback
**Voice Acting:** Grunts only (no dialogue VO)

### Production Approach

**Strategy:** Asset store + free resources, unified by consistent low-poly style
**Flexibility:** Art direction adapts to available assets
**AI Tools:** Where viable for generation

---

## Risk Assessment

### Key Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| **Asset Consistency** | High | Medium | Choose unified low-poly pack or style guide |
| **Scope Creep** | Medium | High | Strict MVP definition, feature freeze |
| **Technical Scaling** | Medium | High | Start with core loop, iterate |
| **Solo Dev Burnout** | Medium | High | Sustainable pace, milestones |

### Technical Challenges

- 3D multiplayer networking at scale
- Procedural world generation
- Performance with tower defense + FPS combat

### Market Risks

- Discoverability in crowded indie market
- Competing with established titles (Muck, Valheim)
- Standing out with limited marketing budget

### Mitigation Strategies

1. **Asset Risk:** Curate a core asset pack first, supplement carefully
2. **Technical:** Build on thesis netcode experience, prototype early
3. **Scope:** Define MVP strictly, cut features ruthlessly
4. **Burnout:** Set boundaries, celebrate small wins
5. **Marketing:** YouTube devlog to build audience during development

---

## Success Criteria

### MVP Definition

**Minimum Playable Version:**
- Basic procedural island map
- Day/night cycle with mob spawning
- Core mechanics: Gather, Craft, Build towers, Shoot enemies
- Multiplayer: 2-4 players
- 10-20 waves for victory mode
- 2 tower types
- 3-4 weapons
- 2 enemy types

**MVP Goal:** A complete, fun co-op experience that proves the core concept.

### Success Metrics

| Metric | Target |
|--------|--------|
| **Steam Wishlist** | Get listed on Steam |
| **Community** | Build devlog following |
| **Quality** | Positive early feedback |
| **Completion** | Ship playable MVP |

### Launch Goals

- Steam store page live with wishlists
- Playable demo or early access
- YouTube devlog documenting journey
- Community feedback loop established

---

## Next Steps

### Immediate Actions

1. **Create GDD** - Transform this brief into detailed game design document
2. **Prototype core loop** - Day/night cycle + basic combat
3. **Asset research** - Identify low-poly asset packs that work together
4. **Netcode foundation** - Build on thesis experience

### Research Needs

- Best low-poly asset packs for consistency
- Unity procedural generation approaches
- Multiplayer architecture patterns
- Steam publishing requirements

### Open Questions

- Exact procedural generation algorithm?
- Progression between runs (meta-progression)?
- Monetization model (premium, early access)?
- Community building strategy?

---

## Appendices

### A. Research Summary

*To be populated during GDD phase with technical research.*

### B. Stakeholder Input

*Solo developer project - stakeholder is Ryan.*

### C. References

- **Muck** - Core gameplay inspiration
- **Team Fight Tactics** - Roguelike powerup mechanics
- **Minecraft** - Day/night cycle
- **Rust** - Crafting systems
- **Kingdom Rush** - Tower defense upgrades

---

_This Game Brief serves as the foundational input for Game Design Document (GDD) creation._

_Next Steps: Use the `workflow gdd` command to create detailed game design documentation._
