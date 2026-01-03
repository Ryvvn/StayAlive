---
project_name: 'StayAlive'
user_name: 'Ryan'
date: '2025-12-28'
sections_completed: ['technology_stack', 'engine_rules', 'performance', 'organization', 'platform', 'critical']
---

# Project Context for AI Agents

_This file contains critical rules and patterns that AI agents must follow when implementing game code in StayAlive. Focus on unobvious details that agents might otherwise miss._

---

## Technology Stack & Versions

| Technology | Version | Purpose |
|------------|---------|---------|
| **Unity** | 2022.3.55f1 (LTS) | Game engine |
| **Netcode** | Unity Netcode for GameObjects | Multiplayer |
| **Input** | New Input System | Controls |
| **Render** | URP | Graphics pipeline |
| **Platform** | PC (Steam) | Target |

---

## Critical Implementation Rules

### Unity Netcode Patterns

**Authority Rules:**
- Host is authoritative for: enemies, towers, waves, day/night, inventory validation
- Client is authoritative for: own movement (with server validation)

**NetworkVariable Usage:**
- Use for synced state: `public NetworkVariable<GamePhase> CurrentPhase = new();`
- Always initialize in declaration
- Change only on server side

**RPC Patterns:**
```csharp
// Client → Host: ServerRpc
[ServerRpc(RequireOwnership = false)]
public void ActionServerRpc(ActionData data, ServerRpcParams rpcParams = default) { }

// Host → Clients: ClientRpc  
[ClientRpc]
public void ResultClientRpc(ResultData data) { }
```

**NetworkObject Spawning:**
- Players: Spawned automatically on connect
- Enemies: Host spawns, use object pooling
- Towers: Player requests via ServerRpc, host validates and spawns
- Loot: Host spawns, short-lived

---

### Manager Hierarchy Pattern

**DO NOT** create arbitrary singletons. Follow this hierarchy:

```
NetworkManager (Netcode singleton)
├── GameManager (game state, phase, win/loss)
├── WaveManager (enemy spawning, throttling)
├── DayNightManager (time, lighting, music crossfade)
├── AudioManager (music layers, bark system)
├── WorldManager (proc gen, ruins, navmesh)
├── InventoryManager (items, crafting per player)
└── TowerManager (building, power grid graph)
```

**Rules:**
- Each manager handles ONE responsibility
- Managers communicate via events, not direct references
- Access managers via: `GameManager.Instance`, not `FindObjectOfType<>()`

---

### Event System Pattern

**Cross-System Events:** Use ScriptableObject GameEvents

```csharp
[CreateAssetMenu(menuName = "Events/GameEvent")]
public class GameEvent : ScriptableObject
{
    private List<GameEventListener> _listeners = new();
    public void Raise() => _listeners.ForEach(l => l.OnEventRaised());
}
```

**Standard Events to Use:**
- `OnDayStart`, `OnNightStart` (Triggers visual/audio shift)
- `OnWaveComplete`, `OnWaveStart` (Triggers bark/music intensity)
- `OnPlayerDeath`, `OnPlayerRespawn`
- `OnTowerBuilt`, `OnTowerDestroyed`

**Bark System Usage:**
- Use `OnDamageTaken` → Play "Grunt" bark
- Use `OnReload` → Play "Cover me!" bark

**DO NOT** use `FindObjectOfType<>()` for cross-system communication.

---

### Object Pooling (MANDATORY)

**Pool these entity types:**
- Enemies (all types)
- Projectiles
- Loot drops
- VFX particles

**Pattern:**
```csharp
public class EnemyPool : MonoBehaviour
{
    private Queue<GameObject> _pool = new();
    public GameObject Get() => _pool.Count > 0 ? _pool.Dequeue() : Instantiate(_prefab);
    public void Return(GameObject obj) { obj.SetActive(false); _pool.Enqueue(obj); }
}
```

**DO NOT** use `Instantiate()`/`Destroy()` in hot paths during gameplay.

---

### Novel Patterns (Critical)

**Power Grid System:**
- Towers must have LOS to Base or Relay to function.
- `PowerManager` maintains a graph of connected nodes.
- Towers check connection state throttled (e.g., every 0.5s).

**Vote System:**
- Post-wave power-up selection via majority vote.
- `VoteManager` collects votes from clients, applies result on host.

---

### Procedural Generation Rules

**Deterministic Seeding:**
- Host generates seed at run start
- Broadcast seed to all clients via RPC
- ALL clients must generate identical worlds from same seed

**DO NOT:**
- Use `Random.value` outside seeded context
- Generate differently on host vs client
- Sync terrain data over network (unnecessary)

---

### Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| **Classes** | PascalCase | `PlayerController` |
| **Methods** | PascalCase | `TakeDamage()` |
| **Variables** | camelCase | `playerHealth` |
| **Private Fields** | _camelCase | `_currentWave` |
| **Constants** | UPPER_SNAKE | `MAX_PLAYERS` |
| **Prefabs** | PascalCase | `Enemy_Rusher` |
| **ScriptableObjects** | Type_Name | `Item_Pistol` |

---

### Performance Rules

**Frame Budget:** 60 FPS minimum = 16.67ms per frame

**Hot Path Optimization:**
- NO allocations in Update/FixedUpdate
- NO LINQ in gameplay code
- NO string concatenation in loops
- NO string concatenation in loops
- Cache component references in Awake()

**AI Throttling (Muck-Style):**
- Distant enemies update logic every 10-20 frames
- Disable Animators on non-visible enemies
- Stop NavMeshAgents when far from players

**Logging in Builds:**
```csharp
#if UNITY_EDITOR
Debug.Log($"[{GetType().Name}] {message}");
#endif
```

---

### Critical Anti-Patterns (DO NOT)

❌ `FindObjectOfType<>()` in Update  
❌ `GetComponent<>()` every frame  
❌ `Instantiate()`/`Destroy()` in combat  
❌ Non-seeded random in procedural gen  
❌ Direct manager references (use events)  
❌ Unvalidated client actions (always ServerRpc)  
❌ NetworkVariable changes on clients  

---

### Folder Structure

```
Assets/_Project/Scripts/
├── Core/          → GameManager, WaveManager, DayNightManager
├── Player/        → PlayerController, PlayerStats, PlayerInventory
├── Enemies/       → EnemyAI, EnemySpawner, EnemyStats
├── Towers/        → TowerController, PowerSystem
├── Survival/      → CraftingSystem, ResourceGathering
├── World/         → WorldGenerator, BiomeManager
├── Network/       → NetworkSetup, LobbyManager
├── UI/            → HUD, Menus
└── Utils/         → Helpers, Extensions
```

**DO NOT** create scripts outside this structure.

---

## Quick Reference

### Common Patterns

| Need | Pattern |
|------|---------|
| Synced state | `NetworkVariable<T>` |
| Client action | `[ServerRpc]` |
| Broadcast result | `[ClientRpc]` |
| Cross-system event | ScriptableObject GameEvent |
| Spawning entities | Object Pool |
| Balance values | ScriptableObject |
| Debug logging | `Debug.Log($"[{GetType().Name}]...")` |

---

### Official Asset Sources (Mandatory)
Use these specific packs to ensure visual consistency and development speed.

**Environment & Props:**
- **Quaternius Survival Pack** (Trees, Rocks, Build Parts)
- **Kenney Nature Kit** (Terrain, Foliage)

**Characters & Animations:**
- **Mixamo** (Y-Bot / SWAT)
- **Animations:** Rifle Walk, Idle, Run, Jump, Death, Gathering

**Weapons:**
- **Quaternius Guns Pack** (Pistol, Shotgun, Rifle)

**UI:**
- **Kenney UI Pack** (Buttons, Panels, Crosshair)

**Audio:**
- **Kenney Impact & Interface Sounds**
- **Freesound.org** (Specific SFX)
