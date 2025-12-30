---
stepsCompleted: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14]
status: 'complete'
inputDocuments:
  - "_bmad-output/game-brief.md"
documentCounts:
  briefs: 1
  research: 0
  brainstorming: 0
  projectDocs: 0
workflowType: 'gdd'
lastStep: 14
project_name: 'StayAlive'
user_name: 'Ryan'
date: '2025-12-27'
game_type: 'survival'
game_name: 'StayAlive'
---

# StayAlive - Game Design Document

**Author:** Ryan
**Game Type:** Survival (with Tower Defense, Roguelike, and Shooter elements)
**Target Platform(s):** {{platforms}}

---

## Executive Summary

### Game Name

**StayAlive**

### Core Concept

StayAlive is a multiplayer roguelike survival FPS that combines the best elements of co-op survival games with strategic tower defense mechanics. Players work together to survive an escalating onslaught of enemies by day gathering resources across a procedurally generated island, and by night defending their central tower against waves of hostile creatures.

The game offers two core modes: a victory-condition mode where players must survive a set number of waves (10-20), and an endless mode for those seeking to push their limits. Every run is unique thanks to procedural world generation, randomized loot, and roguelike power-up choices inspired by games like Team Fight Tactics.

What sets StayAlive apart is its unique genre mashup - it's not just a survival game, but a strategic tower defense experience wrapped in satisfying FPS combat, all designed for chaotic co-op fun with 2-4 players. The signature respawn mechanic (retrieving fallen teammates' chips) creates those memorable "clutch save" moments that players will talk about long after the session ends.

### Game Type

**Primary Type:** Survival  
**Framework:** This GDD uses the Survival template with additional sections for:
- Tower Defense mechanics (tower types, wave design, placement strategy)
- Roguelike systems (procedural generation, run-based progression, power-up selection)
- Shooter combat (FPS mechanics, weapons, enemy design)

### Target Audience

**Primary Audience:** Ages 13-30, casual to hardcore gamers
- Low barrier to entry (easy mode for casual players)
- Quick sessions perfect for after work/school
- Social/co-op focused players who game with friends
- Seeking tower defense + survival fun without complexity

**Secondary Audience:** Content creators and competitive players
- Roguelike variety creates unique content
- Endless mode for challenge seekers
- Leaderboards for competitive drive

### Unique Selling Points (USPs)

1. **Genre Mashup** - Unique blend of co-op chaos, tower defense, roguelike variety, and FPS combat
2. **Every Run is Different** - Procedural generation and roguelike choices create unique stories
3. **Vote on Power-ups** - Team decision-making adds social dimension
4. **Chip Revive System** - Creates memorable "clutch save" moments
5. **Dual Audience Appeal** - Easy mode for casuals, Hard mode for roguelike fans

---

## Goals and Context

### Project Goals

1. **Ship to Steam** - Get StayAlive on Steam with wishlists
2. **Build Community** - YouTube devlog to grow audience during development
3. **Deliver the Vision** - The co-op tower defense roguelike FPS that Muck could have been
4. **Complete MVP** - Playable, fun experience that proves the concept

### Background and Rationale

**Why This Game:**
- Muck proved the market for co-op survival roguelikes
- Gap exists for FPS + tower defense + roguelike mashup
- Solo dev with Unity Netcode experience from thesis project
- Passion for "one more run" experiences with friends

**Technical Foundation:**
- Unity engine familiarity
- Existing multiplayer netcode experience
- Self-funded, bootstrapped development
- 1 month full-time, then nights/weekends

---

## Core Gameplay

### Game Pillars

1. **Co-op Chaos** - Multiplayer mayhem is the heart of the experience. Friends working together (and hilariously failing together).

2. **Roguelike Variety** - Every run is different. Procedural generation, varied loot, and strategic choices keep players coming back.

3. **Tower Defense** - Building and defending your base is core to survival. Strategic placement and upgrades matter.

4. **Rewarding Exploration** - The world rewards those who venture out. Risk vs reward during dangerous resource runs.

**Pillar Priority:** When pillars conflict: Co-op Chaos > Tower Defense > Roguelike Variety > Exploration

### Core Gameplay Loop

**Day Phase (~5 minutes):**
- Explore the procedurally generated island
- Gather resources (wood, stone, etc.)
- Craft weapons and defensive walls
- Build and upgrade tower defenses
- Find loot with stat bonuses

**Night Phase (~3 minutes):**
- Enemy waves attack the tower
- Defend using weapons and tower turrets
- Survive the wave
- Choose roguelike power-ups after each night

**Loop Timing:** ~8 minutes per day/night cycle
**Full Run:** 80-160 minutes (10-20 waves)

**Loop Variation:**
- Procedural map layout each run
- Randomized resource/loot spawns
- Roguelike power-up choices after each night
- Stat bonus loot during day exploration

### Win/Loss Conditions

#### Victory Conditions

| Mode | Victory Condition |
|------|-------------------|
| **Standard** | Survive all waves (10-20 based on difficulty) |
| **Endless** | No victory - survive as long as possible, track wave/score |

#### Failure Conditions

| Condition | Result |
|-----------|--------|
| **Tower Destroyed** | Immediate game over - run ends |
| **Total Party Kill** | All players dead simultaneously = run ends |
| **Player Death** | Drops chip - teammates can retrieve and return to tower to revive |

#### Difficulty Modes

| Difficulty | On Failure |
|------------|------------|
| **Easy Mode** | Respawn at base, continue run (casual-friendly) |
| **Hard Mode** | Lose everything, roguelike reset (hardcore) |

**Tower Mechanics:**
- Tower has HP that can be depleted by enemy attacks
- Tower can be repaired during day phase using resources
- Tower destruction = run ends regardless of player status

---

## Game Mechanics

### Primary Mechanics

| Mechanic | Description |
|----------|-------------|
| **Shoot** | FPS combat with variety of weapons |
| **Build** | Place towers and walls for defense |
| **Gather** | Collect resources from the environment |
| **Craft** | Create weapons, walls, and consumables |
| **Defend** | Protect the tower during night waves |
| **Explore** | Venture out for resources and loot |

### Controls and Input

**Standard FPS Controls:**
- WASD movement
- Mouse aim/look
- Left click: Shoot/Use
- Right click: Aim/Secondary
- E: Interact/Gather
- B: Build menu
- Tab: Inventory
- Number keys: Weapon slots

---

## Survival Specific Design

### Resource Gathering & Crafting

**Resource Types:**
- Wood, Stone, Metal (basic tier, extensible later)
- Enemy loot drops

**Gathering Methods:**
- Mine ore nodes
- Chop trees
- Loot defeated enemies
- Scavenge world objects

**Crafting System:**
- Minecraft-style: Quick crafting + crafting tables
- MVP: 10-20 craftable items
- Categories: Weapons, walls, tower components, consumables

### Survival Needs

**Player Vitals:**
- Hunger system - depletes over time, affects performance
- Thirst system - depletes over time, affects performance
- Health - damaged by enemies, healed by consumables
- Food/water obtained through gathering and crafting

### Base Building

**Construction:**
- Central tower (main objective to protect)
- Expandable base perimeter
- Walls and barricades (player-placed)
- Build zone expands as base grows

---

## Tower Defense Specific Design

### Tower Types

**MVP Tower Types (2):**

| Tower | Type | Function |
|-------|------|----------|
| **Damage Turret** | Offensive | Auto-fires at enemies, primary DPS |
| **Support Tower** | Defensive | Shield generator or healing aura |

**Tower Behavior:** Auto-targeting (players focus on FPS combat)

**Upgrade Path:** Linear for MVP (Lvl 1→2→3), branching planned for future

### Enemy Wave Design

**Wave Scaling:**
- Both quantity AND strength increase per wave
- Wave 1: Few weak enemies
- Wave 20: Many strong enemies

**Boss Waves:**
- Every 5 waves (Wave 5, 10, 15, 20)
- Boss = elite enemy with special mechanics

**Spawn System:**
- Random spawn points around map perimeter
- Portal mechanic for variety (special waves)

---

## Shooter Specific Design

### Weapons

**Weapon Categories:**

| Category | Examples | Characteristics |
|----------|----------|----------------|
| **Starter** | Melee (axe/pickaxe) | Unlimited, weak, early game |
| **Sidearm** | Pistol | Reliable, common ammo |
| **Automatic** | SMG, Rifle | High fire rate, versatile |
| **Special** | Shotgun | High damage, close range |
| **Explosive** | Mortar, RPG | Area damage, rare ammo |

**Ammo System:**
- Limited ammo (adds resource management)
- Obtained via: Crafting, looting enemies, world scavenging
- Melee always available as backup

### Enemy Types

**MVP Enemies (2-4):**

| Type | Speed | Health | Behavior |
|------|-------|--------|----------|
| **Rusher** | Fast | Low | Swarms players and tower |
| **Tank** | Slow | High | Focuses tower, absorbs damage |
| **Ranged** (optional) | Medium | Medium | Attacks from distance |
| **Elite/Boss** | Varies | Very High | Special mechanics, every 5 waves |

**AI Behavior:**
- Mixed targeting: Some prioritize tower, some chase players
- Creates strategic decisions (defend tower vs. protect teammates)

---

## Progression and Balance

### Player Progression

#### Within-Run Progression

**Roguelike Power-ups (Vote System):**
- After each night, team votes on 1 of 3 tower/base upgrade options
- Examples: +Tower damage, +Base HP, +Resource gathering speed, New tower type unlocked

**Stat Boost Loot:**
- Enemies drop items that boost player stats
- Examples: +Damage, +Speed, +Max Health, +Crit chance
- Creates build variety within each run

**Random Effects Pool:**

| Category | Examples |
|----------|----------|
| **Offensive** | +10% Damage, +Fire Rate, Pierce, Explosive rounds |
| **Defensive** | +Max HP, Damage Reduction, Regen, Shield on kill |
| **Utility** | +Move Speed, +Gather Speed, Larger Inventory, Extended Day |
| **Tower** | +Tower Range, +Tower Attack Speed, Extra Tower Slot |
| **Survival** | Slower Hunger/Thirst, More Food from Gathering |

#### Meta Progression (Between Runs)

**Unlock System:**
- New weapons unlock as you reach higher waves
- New tower types unlock as you survive more runs
- Achievements unlock cosmetics (future feature)

**Leaderboards:**
- Endless mode tracks highest wave reached
- Co-op team scores

### Difficulty Curve

**Wave Scaling Formula:**
- Enemy Power = Base × (1 + Wave × ScaleFactor)
- Wave 1: Base difficulty
- Wave 10: ~2x difficulty
- Wave 20: ~3x difficulty

**Dynamic Difficulty Adjustment:**
- If team is underpowered, increase loot drop rates
- If team is overpowered, spawn bonus elite enemies for extra challenge/rewards

**Difficulty Modes:**

| Mode | Respawn | Scaling | Target Audience |
|------|---------|---------|----------------|
| **Easy** | At base | Gentler curve | Casual, new players |
| **Normal** | Chip revive only | Standard | Core audience |
| **Hard** | Permadeath | Aggressive | Hardcore, streamers |

**Endless Mode Scaling:**
- Exponential difficulty curve
- No cap - eventually becomes impossible
- Leaderboard glory is the goal

### Economy and Resources

**Resource Types:**

| Resource | Source | Use |
|----------|--------|-----|
| **Wood** | Trees | Walls, basic crafting |
| **Stone** | Rocks | Towers, upgrades |
| **Metal** | Ore nodes, enemy drops | Weapons, advanced items |
| **Food** | Gathering, hunting | Hunger restoration |
| **Water** | Wells, rivers | Thirst restoration |
| **Ammo** | Crafting, enemy drops | Weapon ammunition |

**Economy Flow:**
- Day: Gather → Craft → Build → Prepare
- Night: Spend ammo → Earn loot drops → Power-up choice

---

## Level Design Framework

### Structure Type

**Procedural Island Generation**
- New island layout generated each run
- Medium-sized map (exploration focus, optimized for performance)
- Contained scope with room for future expansion

### Map Design

**Biome Zones (If Assets Allow):**

| Biome | Resources | Hazards |
|-------|-----------|--------|
| **Forest** | Wood, Food | Standard enemies |
| **Beach** | Starting area | Fewer resources |
| **Rocky/Mountains** | Stone, Metal | Tougher enemies |
| **Swamp/Marsh** | Water, rare loot | Environmental hazards |

*Note: Biome variety depends on available low-poly asset packs. Start with 1-2 biomes for MVP.*

**Points of Interest:**
- Resource nodes (trees, rocks, ore veins)
- Loot chests (weapons, power-ups)
- Mini-boss spawns (rare, high-reward encounters)
- Random mob spawns

### Base & Tower System

**Base Expansion:**
- Central base with power generator
- Towers require energy connection to function
- Build zone expands as base grows
- Power management adds strategic layer

**Tower Placement:**
- Towers must be within power range of base
- Upgrading base increases power radius
- Creates meaningful expansion decisions

### Procedural Elements

| Element | Generation |
|---------|------------|
| **Map Layout** | Always procedural |
| **Resource Nodes** | Random placement per run |
| **Enemy Spawns** | Procedural spawn points |
| **Loot Drops** | Randomized per enemy/chest |
| **Tower Position** | Player-placed, base-centered |

### Tutorial Integration

**Learn by Play:**
- First run includes contextual tooltips
- Key mechanics highlighted as encountered
- No separate tutorial mode
- Players discover systems organically

### Level Design Principles

1. "Something interesting every 30 seconds of exploration"
2. "Risk scales with distance from base"
3. "Every biome has unique resources worth the journey"

---

## Art and Audio Direction

### Art Style

**Visual Direction:**
- Low-poly 3D, colorful palette (flexible based on available assets)
- Muck-inspired visual simplicity
- Simple, readable animations
- Consistent style unified by color and lighting

**Asset Strategy:**
- Asset Store + free resources as foundation
- AI-generated where viable
- Custom assets for unique elements
- Prioritize consistency over complexity

### Audio and Music

**Music:**
- Chill/ambient during day phase
- Intensity increases during night combat
- Victory/defeat stings

**Sound Effects:**
- Functional, arcade-style SFX
- Clear feedback for actions (gather, craft, shoot)
- Distinct sounds per weapon type
- Enemy audio cues for awareness

**Voice:**
- Grunts and effort sounds only
- No narration or dialogue required

---

## Technical Specifications

### Performance Requirements

| Target | Specification |
|--------|---------------|
| **Frame Rate** | 60 FPS minimum |
| **Resolution** | 1080p (1920x1080) |
| **Hardware** | Potato-friendly target, may require slightly stronger for unoptimized MVP |
| **Load Times** | Under 1 minute acceptable, faster preferred |

### Platform Requirements (PC/Steam)

**Minimum Specs (Target):**
- OS: Windows 10
- CPU: Intel i3 or equivalent
- RAM: 4-8 GB
- GPU: Integrated graphics or entry-level dedicated
- Storage: TBD (depends on assets)

**Steam Features:**
- Steam multiplayer integration
- Cloud saves (future)
- Achievements (future)
- Workshop support (future consideration)

### Networking Architecture

| Aspect | Specification |
|--------|---------------|
| **Model** | Host-Client (like Muck) |
| **Authority** | Host is authoritative |
| **Hosting** | Player-hosted (free, no server costs) |
| **Technology** | Unity Netcode for GameObjects |
| **Max Players** | 4 (MVP), expandable to 6-8 |
| **Latency Tolerance** | Up to 200ms (co-op is forgiving) |

**Lobby System:**
- Join Codes (like Muck) - Simple alphanumeric code to join lobbies
- Host creates game → Gets code → Shares with friends
- No server browser needed, reduces complexity

**Network Considerations:**
- Steam Relay or Unity Relay for NAT traversal
- Join codes work regardless of player network setup
- Public lobbies/matchmaking (future feature)

### Asset Requirements

**Art Assets:**
- Flexible style: Mix of low-poly and standard 3D based on available free assets
- Unified by color palette and lighting
- Focus on consistency over perfection

**Asset Sources:**
- Unity Asset Store (free/low-cost)
- itch.io game assets
- AI-generated where appropriate
- Custom assets as needed

**Estimated MVP Assets:**

| Category | Estimate |
|----------|----------|
| **Player Model** | 1 (+ variants) |
| **Enemy Models** | 2-4 types |
| **Tower Models** | 2 types |
| **Weapons** | 3-4 types |
| **Environment** | Trees, rocks, base pieces |
| **VFX** | Muzzle flash, explosions, impacts |
| **UI** | HUD, menus, tooltips |

### Technical Constraints

- Unity engine (familiar from thesis)
- Unity Netcode for GameObjects (existing experience)
- Procedural generation performance budget
- Optimization critical for potato-friendly target

---

## Development Epics

### Epic Structure

**Phase 1: Core Foundation**
- Basic player movement and FPS controls
- Multiplayer networking (host-client with join codes)
- Day/night cycle system

**Phase 2: Survival Systems**
- Resource gathering and crafting
- Hunger/thirst mechanics
- Basic inventory system

**Phase 3: Tower Defense**
- Tower placement and building
- Auto-targeting turrets
- Wave spawning system

**Phase 4: Combat & Enemies**
- Weapon system (3-4 types)
- Enemy AI (2-4 types)
- Boss encounters

**Phase 5: Roguelike Systems**
- Power-up selection (vote system)
- Procedural map generation
- Meta progression/unlocks

**Phase 6: Polish & Release**
- UI/UX polish
- Balancing and tuning
- Steam integration
- Launch!

---

## Success Metrics

### Technical Metrics

| Metric | Target |
|--------|--------|
| **Frame Rate** | 60 FPS minimum |
| **Load Time** | Under 1 minute |
| **Network Latency** | Playable up to 200ms |
| **Crash Rate** | < 1% of sessions |
| **Max Players** | 4 (MVP), 6-8 (future) |

### Gameplay Metrics

| Metric | Target |
|--------|--------|
| **Session Length** | 30-60 min average |
| **Retry Rate** | > 50% play again |
| **Wave Completion** | 60% reach wave 10+ |
| **Co-op Rate** | 70%+ play multiplayer |

---

## Out of Scope (MVP)

**Not in MVP:**
- PvP modes
- Character customization/skins
- Multiple playable characters
- Workshop/mod support
- Public matchmaking (friends-only via codes)
- Host migration
- Console/mobile ports
- Story/narrative content
- Achievements (Steam integration future)

---

## Assumptions and Dependencies

### Assumptions

- Players have stable internet for multiplayer
- Target audience owns gaming-capable PC
- Steam remains primary distribution platform
- Free/low-cost assets meet quality bar

### Dependencies

- Unity Engine (2022 LTS or newer)
- Unity Netcode for GameObjects
- Steam SDK for multiplayer relay
- Asset Store packages (TBD)

### Known Risks

| Risk | Mitigation |
|------|------------|
| Asset consistency | Curate core pack first |
| Scope creep | Strict MVP definition |
| Technical scaling | Prototype early |
| Solo dev burnout | Sustainable pace, small wins |
