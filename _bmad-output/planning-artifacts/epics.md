---
stepsCompleted: [1, 2, 3, 4]
inputDocuments:
  - "d:\\Unity\\StayAlive\\_bmad-output\\gdd.md"
  - "d:\\Unity\\StayAlive\\_bmad-output\\game-architecture.md"
  - "d:\\Unity\\StayAlive\\_bmad-output\\narrative-design.md"
---

# StayAlive - Epic Breakdown

## Overview

This document provides the complete epic and story breakdown for StayAlive, decomposing the requirements from the PRD, UX Design if it exists, and Architecture requirements into implementable stories.

## Requirements Inventory

### Functional Requirements

FR1: Core Gameplay Loop - Day/Night cycle (~8 min total) with distinct phases for gathering and defending.
FR2: Multiplayer - Host-Client architecture supporting 2-4 players via Join Codes.
FR3: Player Mechanics - FPS controls (Move, Shoot, Aim, Jump), Stats (Health, Hunger, Thirst).
FR4: Building System - Place towers, walls, and barricades; repair mechanisms.
FR5: Crafting System - Gather resources (Wood, Stone, Metal) to craft items, weapons, and defenses.
FR6: Tower Defense - Automated turrets (Damage, Support) requiring power connection to base.
FR7: Combat System - Weapons (Melee, Pistol, Automatic, Explosive) with ammo management.
FR8: Enemy System - Wave-based spawning, AI behaviors (Rusher, Tank, Boss), finding targets (Tower vs Player).
FR9: Revive Mechanic - "Chip Revive" system; players drop chips on death, teammates return them to base.
FR10: Roguelike Progression - Random power-up selection (Vote System) after each night; procedural map generation.
FR11: Win/Loss Conditions - Victory at Wave 20; Defeat if Tower destroyed or Total Party Kill.
FR12: UI/HUD - Vitals display, Ammo count, Wave timer, Build menu, Inventory screen.
FR13: Interaction - 'E' to gather/interact, distinct interactions for Revive and Crafting.

### NonFunctional Requirements

NFR1: Performance - Minimum 60 FPS on "potato" hardware; Load times < 1 minute.
NFR2: Networking - Tolerant to 200ms latency; Host authoritative for critical state (AI, loot).
NFR3: Scalability - Support 100+ enemies active simultaneously (using pooling).
NFR4: Stability - Robust error handling for network disconnects; crash rate < 1%.
NFR5: Usability - Join Code system for easy lobby finding (no server browser required for MVP).
NFR6: Audio - Dynamic music transitions between Day/Night; 3D spatial sound for combat.

### Additional Requirements

**From Architecture:**
- Project Structure: Follow strict Hybrid Domain-Driven directory layout.
- Networking Pattern: Use "Muck-style" optimization (Host handles all AI logic, Client facilitates prediction).
- Data Mgmt: Use ScriptableObjects for all static data (Items, Waves, Music).
- Code Standards: Managers initialize in `Awake` but communicate in `Start`; Global Exception Handling.
- Debug Tools: In-game console (`~`) and Gizmos must be implemented for testing.
- Novel Patterns: Implement "Power Grid" (LOS graph) and "Vote System" (Networked UI).

**From Narrative Design:**
- Bark System: Contextual audio lines for Reloading, Hit, Downed (Generic "soldier shouts").
- Environmental Storytelling: Procedurally placed "Ruins" with specific visual themes.
- Boss Phases: Boss death triggers specific world state changes or audio events.
- UI Tone: "Functional Utility" style textual feedback; no dialogue trees.

### FR Coverage Map

FR1 (Core Cycle): Epic 4 - The Threat
FR2 (Multiplayer): Epic 1 - The Multiplayer Foundation
FR3 (Player Mech): Epic 2 - Survival Basics
FR4 (Building): Epic 3 - Fortification
FR5 (Gather/Craft): Epic 2 & 3
FR6 (Tower Power): Epic 3 & 4
FR7 (Combat): Epic 5 - Combat & Response
FR8 (Enemies): Epic 4 & 6
FR9 (Revive): Epic 5 - Combat & Response
FR10 (Roguelike): Epic 2 & 6
FR11 (Win/Loss): Epic 4 & 6
FR12 (UI/HUD): Epic 2 - Survival Basics
FR13 (Interact): Epic 2 - Survival Basics

## Epic List

### Epic 1: The Multiplayer Foundation
Players can join a lobby, spawn into a shared world, and see each other move.
**FRs covered:** FR2, NFR2, NFR5

### Story 1.1: Network Manager Setup
As a dev,
I want to configure the Unity Netcode for GameObjects NetworkManager,
So that the game can handle host-client connections and object synchronization.

**Acceptance Criteria:**
**Given** a new Unity scene
**When** the scene loads
**Then** a NetworkManager component should be present and configured
**And** the transport should be set to Unity Transport
**And** the player prefab should be assigned in the NetworkManager

### Story 1.2: Host & Client Connection UI
As a player,
I want to use a main menu to host a game or join one via a code,
So that I can play with my friends without port forwarding.

**Acceptance Criteria:**
**Given** the Main Menu scene
**When** I click "Host Game"
**Then** a lobby is created and a Join Code is displayed
**When** I enter a valid code and click "Join"
**Then** I should successfully connect to the host's session

### Story 1.3: Player Spawning & Movement Synchronization
As a player,
I want my character to spawn and my movement to be synced to other players,
So that we can see each other and explore the world together.

**Acceptance Criteria:**
**Given** I am in a networked session
**When** the game starts
**Then** my player character should spawn at a spawn point
**When** I move my character using WASD
**Then** my movement should be smooth on my screen (Client Prediction)
**And** other clients should see my character move correctly

### Story 1.4: Connection Stability Handling
As a player,
I want the game to handle connection failures gracefully,
So that I know if I've been disconnected rather than the game just freezing.

**Acceptance Criteria:**
**Given** I am in a multiplayer session
**When** the internet connection drops
**Then** the game should detect the disconnection
**And** a "Connection Lost" popup should appear
**And** I should be returned to the Main Menu

### Epic 2: Survival Basics (The Day Loop)
Players can move, explore a procedural map, gather resources, and manage vitals.
**FRs covered:** FR3, FR5, FR10, FR12, FR13

### Story 2.1: First Person Controller Implementation
As a player,
I want responsive FPS controls (Move, Look, Jump),
So that I can navigate the world effectively.

**Acceptance Criteria:**
**Given** I am spawned in the game world
**When** I use WASD and Mouse
**Then** I should move around and look freely
**When** I press Space
**Then** I should jump
**And** the movement should feel responsive (not floaty)

### Story 2.2: Procedural Terrain Generation
As a player,
I want to spawn on a unique island layout each run,
So that exploration feels fresh every time.

**Acceptance Criteria:**
**Given** the host starts a new game
**Then** a random seed should be generated and synced to all clients
**When** the scene loads
**Then** the terrain and obstacles should be generated deterministically based on that seed
**And** the map should be navigable (no stuck spots)

### Story 2.3: Player Vitals & HUD
As a player,
I want to see my Health, Hunger, and Thirst bars,
So that I know when to eat or heal.

**Acceptance Criteria:**
**Given** I am playing the game
**Then** a HUD overlay should be visible showing Health, Hunger, and Thirst
**When** time passes
**Then** Hunger and Thirst should slowly decrease
**When** Health reaches 0
**Then** the "Death" state should trigger

### Story 2.4: Resource Gathering System
As a player,
I want to look at trees or rocks and press 'E' to gather materials,
So that I can collect resources for crafting.

**Acceptance Criteria:**
**Given** I am looking at a gatherable object (Tree/Rock)
**When** I press 'E'
**Then** the object should give resource feedback (sound/particle)
**And** the resource (Wood/Stone) should be added to my inventory
**And** the object should eventually be destroyed/depleted

### Story 2.5: Day/Night Cycle Manager
As a player,
I want the sun to move and lighting to change,
So that I know how much time is left before nightfall.

**Acceptance Criteria:**
**Given** the game has started
**Then** a global timer should track the time of day
**When** the timer progresses
**Then** the sun/moon should move and ambient lighting should change
**When** the timer reaches the Night threshold
**Then** the game phase should switch to "Night"

### Epic 3: Fortification (The Build Loop)
Players can craft items, build walls, and place powered towers.
**FRs covered:** FR4, FR5, FR6

### Story 3.1: Crafting System Backend & UI
As a player,
I want to open a crafting menu and combine collected resources into items,
So that I can create weapons and building parts.

**Acceptance Criteria:**
**Given** I have sufficient resources in my inventory
**When** I open the Crafting Menu and select a recipe
**Then** the resources should be consumed
**And** the crafted item should be added to my inventory
**And** I should hear a crafting sound

### Story 3.2: Building System Implementation
As a player,
I want to place walls and floors freely in the world,
So that I can construct a custom base for defense.

**Acceptance Criteria:**
**Given** I have a building item equipped
**When** I aim at the ground
**Then** a "Ghost" preview of the structure should appear
**When** I click to build
**Then** the structure should be instantiated efficiently across the network
**And** it should block player and enemy movement

### Story 3.3: Power Grid System
As a player,
I want towers to only work near the base,
So that I have to manage my base layout strategically.

**Acceptance Criteria:**
**Given** a central "Power Source" (Base)
**When** I try to place a Tower outside its range
**Then** the placement should clearly show it is "Unpowered"
**When** I place a Tower within range
**Then** it should register as "Powered" and function
**And** the Power Manager should efficiently check Line-of-Sight (Novel Pattern)

### Story 3.4: Basic Tower Implementation
As a player,
I want to place a turret that automatically tracks enemies,
So that it can help defend the base.

**Acceptance Criteria:**
**Given** a powered turret is placed
**When** a target comes within range
**Then** the turret head should rotate to face the target
**And** the turret should visually indicate it is "active"
**Note:** Firing logic is in Epic 4.

### Epic 4: The Threat (The Night Loop)
Enemies spawn in waves, attack the base, and the cycle of Day/Night is established.
**FRs covered:** FR1, FR8, FR11, FR6

### Epic 4: The Threat (The Night Loop)
Enemies spawn in waves, attack the base, and the cycle of Day/Night is established.
**FRs covered:** FR1, FR8, FR11, FR6

### Story 4.1: Enemy AI Framework & Pooling
As a dev,
I want a robust, pooled enemy management system,
So that the game can handle hundreds of enemies without lag.

**Acceptance Criteria:**
**Given** a wave starts
**When** enemies are spawned
**Then** they should be retrieved from a Network Object Pool (Muck-style)
**And** they should sync their position from Host to Clients
**And** they should navigate using NavMeshAgents (Host side only)

### Story 4.2: Wave Manager System
As a player,
I want enemies to spawn in escalating waves at night,
So that the challenge increases over time.

**Acceptance Criteria:**
**Given** it is Night time
**Then** the Wave Manager should spawn groups of enemies at designated points
**When** all enemies are defeated
**Then** the Night should end (or next sub-wave starts)
**And** the wave number should increase

### Story 4.3: Enemy Attack Logic
As a player,
I want enemies to attack my base and me,
So that I feel threatened and have to defend.

**Acceptance Criteria:**
**Given** an active enemy
**When** it gets within range of a Target (Player or Base)
**Then** it should play an attack animation
**And** deal damage to the target
**And** prioritize the Base if no Player is closer/aggroed

### Story 4.4: Active Tower Combat
As a player,
I want my turrets to shoot enemies,
So that my defenses actually significantly help.

**Acceptance Criteria:**
**Given** a powered turret with a target
**When** the turret fires
**Then** a projectile should launch towards the enemy
**And** deal damage on impact
**And** play a firing sound and effect

### Story 4.5: Win/Loss State Logic
As a player,
I want to win the night or lose the game if the base falls,
So that the stakes are real.

**Acceptance Criteria:**
**Given** the Base HP reaches 0
**Then** the "Game Over" screen should appear for all players
**Given** all enemies in a wave are dead
**Then** the game should transition back to Day phase

### Epic 5: Combat & Response
Players can fight back with weapons and revive fallen teammates.
**FRs covered:** FR7, FR9, NFR6

### Epic 5: Combat & Response
Players can fight back with weapons and revive fallen teammates.
**FRs covered:** FR7, FR9, NFR6

### Epic 6: Progression & Variety
Players experience unique runs with Bosses, Power-up Voting, and Victory conditions.
**FRs covered:** FR10, FR11, FR8

### Story 6.1: Vote System Implementation
As a player,
I want to vote on a power-up after each night,
So that the team can decide on the best upgrade for the run.

**Acceptance Criteria:**
**Given** a night wave is cleared
**Then** a "Voting Screen" UI should appear for all players
**When** players click an option
**Then** the vote count should update on all screens
**When** the timer ends
**Then** the winning power-up should be applied to the team/base

### Story 6.2: Boss Logic & Spawning
As a player,
I want to face a massive Boss enemy every 5 waves,
So that there is a climactic challenge.

**Acceptance Criteria:**
**Given** it is Wave 5, 10, 15, or 20
**Then** a Boss enemy should spawn
**When** the Boss dies
**Then** a "Phase Shift" event may trigger (e.g., harder enemies start spawning)
**And** rare loot should drop

### Story 6.3: Victory Condition & Run Tracking
As a player,
I want to successfully escape (Win) at Wave 20,
So that the run has a satisfying conclusion.

**Acceptance Criteria:**
**Given** Wave 20 is cleared
**Then** the "Victory" sequence should trigger (UI, Music, Effects)
**And** the run stats (Time, Kills) should be displayed
**And** I should be returned to the Lobby

### Story 6.4: Ruins & POI Generation
As a player,
I want to find interesting "Ruins" in the world,
So that exploration feels rewarding and tells a story.

**Acceptance Criteria:**
**Given** the world is generating
**Then** "Ruin" prefabs should be placed at random valid locations
**When** I explore a Ruin
**Then** I should find a Loot Chest
**And** I should see environmental storytelling elements (skeletons, broken tech)

<!-- Repeat for each epic in epics_list (N = 1, 2, 3...) -->

## Epic {{N}}: {{epic_title_N}}

{{epic_goal_N}}

<!-- Repeat for each story (M = 1, 2, 3...) within epic N -->

### Story {{N}}.{{M}}: {{story_title_N_M}}

As a {{user_type}},
I want {{capability}},
So that {{value_benefit}}.

**Acceptance Criteria:**

<!-- for each AC on this story -->

**Given** {{precondition}}
**When** {{action}}
**Then** {{expected_outcome}}
**And** {{additional_criteria}}

<!-- End story repeat -->
