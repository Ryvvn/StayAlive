---
title: 'Game Architecture'
project: 'StayAlive'
date: '2025-12-27'
author: 'Ryan'
version: '1.0'
stepsCompleted: [1, 2, 3, 4, 5, 6, 7, 8, 9]
status: 'complete'
engine: 'Unity 2022.3.55f1'
platform: 'PC (Steam)'

# Source Documents
gdd: '_bmad-output/gdd.md'
epics: null
brief: '_bmad-output/game-brief.md'
---

# StayAlive - Game Architecture

## Document Status

This architecture document is **COMPLETE** and ready to guide implementation.

**Steps Completed:** 9 of 9 ✅

---

## Executive Summary

**StayAlive** architecture is designed for **Unity 2022.3.55f1** targeting **PC (Steam)**.

**Key Architectural Decisions:**
- **Networking:** Host-client with join codes, Unity Netcode for GameObjects
- **State Management:** GameManager Singleton + Specialized Managers
- **Procedural Generation:** Seed-based deterministic world generation
- **Data Architecture:** ScriptableObjects for items, config, and balance

**Project Structure:** Hybrid organization with 9 core systems mapped to locations.

**Implementation Patterns:** 6 patterns defined (2 novel, 4 standard) ensuring AI agent consistency.

**Ready for:** Epic implementation phase

---

## Project Context

### Game Overview

**StayAlive** - Multiplayer roguelike survival FPS with tower defense mechanics where players defend their base at night and gather resources by day.

### Technical Scope

**Platform:** PC (Steam)
**Genre:** Survival + Tower Defense + Roguelike + FPS
**Project Level:** High Complexity

### Core Systems

| System | Complexity | Description |
|--------|------------|-------------|
| **Networking** | High | Host-client with join codes, 4-8 players |
| **Procedural Generation** | High | Island map generation each run |
| **Tower Defense** | Medium | Auto-targeting turrets with power system |
| **Survival Mechanics** | Medium | Hunger, thirst, resources, crafting |
| **Combat/Weapons** | Medium | FPS combat with multiple weapon types |
| **Wave System** | Medium | Enemy spawning, wave progression, bosses |
| **Roguelike Progression** | Medium | Power-up voting, meta unlocks |
| **Day/Night Cycle** | Low | Time management, phase transitions |

### Technical Requirements

| Constraint | Target |
|------------|--------|
| Frame Rate | 60 FPS minimum |
| Resolution | 1080p |
| Latency | Up to 200ms acceptable |
| Load Time | Under 1 minute |
| Hardware | Potato-friendly |

### Complexity Drivers

- **Multiplayer + Procedural:** Deterministic world generation for network sync
- **Novel Power System:** Towers require energy connection to base
- **Vote-Based Power-ups:** Team decision system after each wave
- **Mixed AI Targets:** Enemies attack both tower and players

### Technical Risks

| Risk | Impact | Mitigation |
|------|--------|------------|
| Networking Complexity | High | Start with simple host-client, iterate |
| Procedural Performance | Medium | Cache generation, optimize algorithms |
| Multiplayer Scaling | Medium | Test early with max players |
| Solo Dev Scope | High | Strict MVP, phased development |

---

## Engine & Framework

### Selected Engine

**Unity 2022.3.55f1** (2022 LTS)

**Rationale:**
- LTS version for production stability
- Mature Netcode for GameObjects support
- Existing experience from thesis project
- Strong community and asset ecosystem

### Engine-Provided Architecture

| Component | Solution | Notes |
|-----------|----------|-------|
| **Rendering** | URP (Universal Render Pipeline) | Recommended for performance |
| **Physics** | Unity Physics 3D | Built-in for FPS |
| **Audio** | Unity Audio System | Built-in mixer |
| **Input** | New Input System | Modern, rebindable |
| **Scene Management** | SceneManager | Additive loading |
| **Build System** | Unity Build Pipeline | Steam integration |
| **Networking** | Unity Netcode for GameObjects | Host-client model |

### Remaining Architectural Decisions

The following decisions must be designed explicitly:

1. **Game State Architecture** - How game state flows between systems
2. **Network Sync Strategy** - What to sync, authority rules
3. **Procedural Generation Pipeline** - Map generation approach
4. **Entity/Component Structure** - Player, enemies, towers, items
5. **Event System** - How systems communicate
6. **Inventory/Crafting** - Data structures and logic
7. **Tower Power System** - Energy connection mechanics
8. **Save/Load System** - Persistence strategy

---

## Architectural Decisions

### Decision Summary

| Category | Decision | Rationale |
|----------|----------|----------|
| State Management | GameManager Singleton + Specialized Managers | Unity-familiar, focused managers prevent god objects |
| Networking | Host Authoritative + Client Prediction | Cheat-resistant, smooth movement feel |
| Procedural Gen | Seed-based Deterministic | Same seed = same world on all clients |
| Entities | NetworkObject per entity type | Netcode-native, pooled for performance |
| Events | Unity Events + ScriptableObject Events | Simple, decoupled, designer-friendly |
| Inventory | ScriptableObject Database + Runtime Lists | Data-driven items, host-validated crafting |
| Power System | Graph-based Connections | Base radius check, visual power lines |
| Save System | PlayerPrefs (meta) / None (runs) | Roguelike = fresh runs, meta unlocks persist |

### State Management

**Approach:** GameManager Singleton + Specialized Managers

```
NetworkManager (Unity Netcode)
├── GameManager (game state, phase, win/loss)
├── WaveManager (enemy spawning, wave progress)
├── DayNightManager (time, phase transitions)
├── InventoryManager (items, crafting per player)
└── TowerManager (building, power system)
```

**Rules:**
- Each manager handles ONE responsibility
- Managers communicate via events, not direct references
- NetworkManager is the singleton root

### Network Synchronization

**Approach:** Host Authoritative + Client Prediction for Movement

| Data | Authority | Sync Method |
|------|-----------|-------------|
| Player Position | Client (predicted) | NetworkTransform |
| Player Actions | Client → Host | ServerRpc |
| Enemy AI/Position | Host | NetworkVariable |
| Tower State | Host | NetworkVariable |
| Wave Progress | Host | NetworkVariable |
| Day/Night Time | Host | NetworkVariable |
| Inventory | Host (validated) | NetworkVariable |
| Map Seed | Host → Clients | RPC on join |

**NetworkObject Types:**
- Players: Spawned per player, persistent
- Enemies: Pooled, host-spawned/despawned
- Towers: Player-built, host-validated
- Loot/Items: Short-lived, pooled

### Procedural Generation

**Approach:** Seed-based Deterministic Generation

- Host generates random seed at run start
- Seed broadcast to all clients via RPC
- All clients generate identical world from seed
- Library: FastNoise Lite or Unity.Mathematics

**Generation Pipeline:**
```
1. Host: Generate seed → Broadcast
2. All: Generate terrain heightmap
3. All: Place biomes based on height/moisture
4. All: Spawn resource nodes (deterministic)
5. Host: Spawn enemies (authority)
```

### Entity Structure

**Player:**
- NetworkObject + NetworkTransform
- PlayerController (input, movement)
- PlayerStats (health, hunger, thirst - synced)
- PlayerInventory (items - host-validated)

**Enemy:**
- NetworkObject (host-spawned)
- EnemyAI (host-driven NavMeshAgent)
- EnemyStats (health - synced)
- Target priority: Tower > Nearest Player

**Tower:**
- NetworkObject (player-placed, host-validated)
- TowerController (targeting, firing)
- PowerConnection (checks base range)

### Event System

**Pattern:** Hybrid Unity Events + ScriptableObject Events

**Local Events:** C# events / UnityEvents
- UI updates
- Single-system notifications

**Cross-System Events:** ScriptableObject GameEvents
- OnDayStart, OnNightStart
- OnWaveComplete
- OnPlayerDeath
- OnTowerBuilt

**Network Events:**
- Client → Host: ServerRpc
- Host → Clients: ClientRpc or NetworkVariable

### Inventory & Crafting

**Structure:**
```
ItemDatabase (ScriptableObject)
├── ItemDefinition (id, name, icon, stackSize)
└── RecipeDefinition (inputs, output, craftTime)

PlayerInventory (Runtime)
├── List<ItemStack> items
└── Methods: Add, Remove, CanCraft, Craft
```

**Sync:** Host validates all crafting requests, syncs inventory changes

### Tower Power System

**Pattern:** Graph-based Power Connections

- Base has power radius (upgradeable)
- Towers check if within power range on placement
- Visual power lines connect towers to base
- Power state managed by host, broadcast to clients

**Logic:**
```csharp
bool CanPlace(Vector3 position) {
    return Vector3.Distance(position, base.position) <= base.powerRadius;
}
```

### Save System

**MVP Approach:**
- **Run Progress:** Not saved (roguelike = fresh each run)
- **Meta Unlocks:** PlayerPrefs (simple key-value)
- **Settings:** PlayerPrefs

**Future Consideration:**
- JSON serialization for full save state
- Cloud saves via Steam

---

## Cross-cutting Concerns

These patterns apply to ALL systems and must be followed by every implementation.

### Error Handling

**Strategy:** Try-Catch + Unity Debug.LogError

| Error Type | Handling |
|------------|----------|
| **Critical** | Pause game, show message to player |
| **Recoverable** | Log error, continue operation |
| **Network** | Retry logic or graceful disconnect |

**Example:**
```csharp
try {
    // Risky operation
} catch (Exception e) {
    Debug.LogError($"[{GetType().Name}] {e.Message}");
    // Handle gracefully
}
```

### Logging

**Format:** `[System] Message`
**Destination:** Unity Console (Debug.Log family)

| Level | Method | Use For |
|-------|--------|--------|
| **ERROR** | Debug.LogError | Something broke |
| **WARN** | Debug.LogWarning | Unexpected but handled |
| **INFO** | Debug.Log | Key milestones |
| **DEBUG** | Debug.Log (conditional) | Development only |

**Example:**
```csharp
Debug.Log($"[WaveManager] Wave {waveNumber} started");
Debug.LogWarning($"[NetworkManager] High latency detected: {ping}ms");
Debug.LogError($"[InventoryManager] Failed to add item: {itemId}");
```

### Configuration

| Config Type | Storage | Access |
|-------------|---------|--------|
| **Game Constants** | Static C# classes | `GameConstants.MaxPlayers` |
| **Balance Values** | ScriptableObjects | Drag into inspector |
| **Player Settings** | PlayerPrefs | `PlayerPrefs.GetInt()` |
| **Network Config** | ScriptableObject | `NetworkConfig.MaxLatency` |

### Event System

**Pattern:** Unity Events + ScriptableObject GameEvents

**Local Events:** C# events / UnityEvents for single-system
**Cross-System:** ScriptableObject GameEvents for decoupling
**Network:** ServerRpc (client→host), ClientRpc (host→clients)

**Standard Events:**
- `OnDayStart`, `OnNightStart`
- `OnWaveComplete`, `OnWaveStart`
- `OnPlayerDeath`, `OnPlayerRespawn`
- `OnTowerBuilt`, `OnTowerDestroyed`

### Debug Tools

| Tool | Activation | Function |
|------|------------|----------|
| **Debug Console** | Tilde (~) key | Command input |
| **FPS Counter** | F3 key | Performance monitor |
| **God Mode** | Console: `godmode` | Invincibility |
| **Skip Wave** | Console: `skipwave` | Testing |
| **Spawn Item** | Console: `spawn [item]` | Testing |

**Build Configuration:**
- Debug tools: Disabled in Release by default
- Enable with launch flag: `--enable-debug`

---

## Project Structure

### Organization Pattern

**Pattern:** Hybrid (Types at top, Features within)

**Rationale:** Unity standard, clear separation, easy navigation

### Directory Structure

```
StayAlive/
├── Assets/
│   ├── _Project/              # Game code & assets
│   │   ├── Scripts/
│   │   │   ├── Core/          # Managers, singletons
│   │   │   ├── Player/        # Player systems
│   │   │   ├── Enemies/       # Enemy systems
│   │   │   ├── Towers/        # Tower defense
│   │   │   ├── Survival/      # Survival mechanics
│   │   │   ├── World/         # Procedural gen
│   │   │   ├── Network/       # Netcode
│   │   │   ├── UI/            # UI scripts
│   │   │   └── Utils/         # Utilities
│   │   ├── Prefabs/
│   │   ├── Scenes/
│   │   ├── ScriptableObjects/
│   │   ├── Art/
│   │   ├── Audio/
│   │   └── Data/
│   ├── Plugins/               # Third-party
│   └── Resources/             # Runtime loaded
├── Packages/
└── ProjectSettings/
```

### System Location Mapping

| System | Location | Key Files |
|--------|----------|----------|
| Game State | Scripts/Core/ | GameManager, NetworkGameManager |
| Day/Night | Scripts/Core/ | DayNightManager |
| Waves | Scripts/Core/ | WaveManager |
| Player | Scripts/Player/ | PlayerController, PlayerStats |
| Enemies | Scripts/Enemies/ | EnemyAI, EnemySpawner |
| Towers | Scripts/Towers/ | TowerController, PowerSystem |
| Survival | Scripts/Survival/ | CraftingSystem, ResourceGathering |
| World Gen | Scripts/World/ | WorldGenerator, BiomeManager |
| Network | Scripts/Network/ | NetworkSetup, LobbyManager |

### Naming Conventions

| Element | Convention | Example |
|---------|------------|--------|
| **Classes** | PascalCase | `PlayerController` |
| **Methods** | PascalCase | `TakeDamage()` |
| **Variables** | camelCase | `playerHealth` |
| **Private Fields** | _camelCase | `_currentWave` |
| **Constants** | UPPER_SNAKE | `MAX_PLAYERS` |
| **Prefabs** | PascalCase | `Enemy_Rusher` |
| **ScriptableObjects** | Type_Name | `Item_Pistol` |
| **Scenes** | PascalCase | `MainMenu`, `GameScene` |

### Architectural Boundaries

- **Core/** can be referenced by any system
- **Feature folders** should not cross-reference each other directly
- Use **events** for cross-feature communication
- **Network/** handles all Netcode specifics
- **Utils/** for pure utility functions only

---

## Implementation Patterns

These patterns ensure consistent implementation across all AI agents.

### Novel Patterns

#### Power System Pattern

**Purpose:** Connect towers to base via energy radius

**Components:**
- `PowerSystem` (on Base) - Manages power radius
- `PowerConnection` (on Towers) - Validates placement

**Implementation:**
```csharp
public class PowerSystem : MonoBehaviour
{
    [SerializeField] private float _powerRadius = 20f;
    public event Action<float> OnPowerRadiusChanged;
    
    public bool CanPlaceTower(Vector3 position)
    {
        return Vector3.Distance(transform.position, position) <= _powerRadius;
    }
    
    public void UpgradePowerRadius(float bonus)
    {
        _powerRadius += bonus;
        OnPowerRadiusChanged?.Invoke(_powerRadius);
    }
}
```

#### Vote System Pattern

**Purpose:** Team voting on post-wave power-ups

**Components:**
- `VoteManager` (NetworkBehaviour) - Collects votes, determines winner
- `VoteUI` - Displays options, captures input

**Implementation:**
```csharp
public class VoteManager : NetworkBehaviour
{
    private Dictionary<ulong, int> _playerVotes = new();
    
    [ServerRpc(RequireOwnership = false)]
    public void CastVoteServerRpc(int optionIndex, ServerRpcParams rpcParams = default)
    {
        _playerVotes[rpcParams.Receive.SenderClientId] = optionIndex;
        CheckVotingComplete();
    }
    
    private void CheckVotingComplete()
    {
        if (_playerVotes.Count >= NetworkManager.ConnectedClients.Count)
        {
            int winner = GetMajorityVote();
            ApplyPowerupClientRpc(winner);
        }
    }
}
```

### Standard Patterns

#### Communication Pattern

**Pattern:** Event-based (ScriptableObject Events)

```csharp
// GameEvent ScriptableObject
[CreateAssetMenu(menuName = "Events/GameEvent")]
public class GameEvent : ScriptableObject
{
    private List<GameEventListener> _listeners = new();
    
    public void Raise() => _listeners.ForEach(l => l.OnEventRaised());
    public void Register(GameEventListener listener) => _listeners.Add(listener);
    public void Unregister(GameEventListener listener) => _listeners.Remove(listener);
}
```

#### Entity Creation Pattern

**Pattern:** Object Pooling + Prefab Instantiation

```csharp
public class EnemyPool : MonoBehaviour
{
    [SerializeField] private GameObject _enemyPrefab;
    private Queue<GameObject> _pool = new();
    
    public GameObject Get()
    {
        return _pool.Count > 0 ? _pool.Dequeue() : Instantiate(_enemyPrefab);
    }
    
    public void Return(GameObject enemy)
    {
        enemy.SetActive(false);
        _pool.Enqueue(enemy);
    }
}
```

#### State Pattern

**Pattern:** Simple State Machine

```csharp
public enum GamePhase { Menu, Day, Night, Victory, Defeat }

public class GameManager : MonoBehaviour
{
    public NetworkVariable<GamePhase> CurrentPhase = new();
    
    public void TransitionTo(GamePhase newPhase)
    {
        CurrentPhase.Value = newPhase;
        OnPhaseChanged?.Invoke(newPhase);
    }
}
```

#### Data Access Pattern

**Pattern:** ScriptableObject Databases

```csharp
[CreateAssetMenu(menuName = "Data/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<ItemDefinition> _items;
    
    public ItemDefinition GetById(string id) 
        => _items.FirstOrDefault(i => i.Id == id);
}
```

### Consistency Rules

| Pattern | Convention | Enforcement |
|---------|------------|------------|
| Events | ScriptableObject GameEvents | All cross-system communication |
| Pooling | Use for enemies, projectiles, loot | Performance-critical entities |
| State | NetworkVariable for synced state | All game phase changes |
| Data | ScriptableObjects for balance | All tweakable values |

---

## Architecture Validation

### Validation Summary

| Check | Result | Notes |
|-------|--------|-------|
| Decision Compatibility | ✅ PASS | All Unity-native, no conflicts |
| GDD Coverage | ✅ PASS | All 8 systems have architecture support |
| Pattern Completeness | ✅ PASS | All scenarios covered with examples |
| Document Completeness | ✅ PASS | No placeholders, all sections filled |

### Coverage Report

- **Systems Covered:** 8/8
- **Patterns Defined:** 6 (2 novel + 4 standard)
- **Decisions Made:** 8

### Validation Date

2025-12-28
