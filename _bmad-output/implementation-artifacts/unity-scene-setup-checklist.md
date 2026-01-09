# Unity Scene Setup Checklist - StayAlive Multiplayer

This checklist ensures all Epic 1 multiplayer features are correctly configured in Unity Editor.

---

## ⚡ QUICK SETUP (Recommended)

Use the auto-creation tools for fast setup:

| Menu Path | Creates |
|-----------|---------|
| `Tools > StayAlive > Quick Setup - Complete Scene` | Everything below in one click! |
| `Tools > StayAlive > Create Network Manager` | NetworkManager + UnityTransport |
| `Tools > StayAlive > Create Spawn Points (4)` | SpawnManager + 4 SpawnPoints |
| `Tools > StayAlive > Create Main Menu UI` | Full UI Canvas with all references |

> After using Quick Setup, just verify the Player Prefab is assigned and add scenes to Build Settings!

---

## 🔧 Prerequisites

- [ ] Unity Netcode for GameObjects package installed
- [ ] Unity Transport package installed
- [ ] TextMeshPro package installed
- [ ] New Input System package installed

---

## 📦 NetworkManager Setup

### Create/Verify NetworkManager Object

1. **In your starting scene (e.g., MainMenu):**
   - [ ] Create empty GameObject named `NetworkManager`
   - [ ] Add component: `NetworkManager` (Unity.Netcode)
   - [ ] Add component: `UnityTransport`
   - [ ] Add component: `NetworkGameManager` (our script)

2. **Configure NetworkManager component:**
   - [ ] Player Prefab: Assign `Player.prefab` from `Assets/_Project/Prefabs/Player/`
   - [ ] Network Prefabs: Add any networked prefabs to the list
   - [ ] Tick Rate: 30 (recommended for this game type)

3. **Configure UnityTransport component:**
   - [ ] Connection Data Address: `127.0.0.1` (for testing)
   - [ ] Connection Data Port: `7777`
   - [ ] Max Connect Attempts: `5`

---

## 🎮 Player Prefab Setup

### Verify Player.prefab Components

Location: `Assets/_Project/Prefabs/Player/Player.prefab`

- [ ] `NetworkObject` component attached
- [ ] `ClientNetworkTransform` component attached (NOT regular NetworkTransform)
  - Sync Position X, Y, Z: ✓
  - Sync Rotation Y: ✓ (for FPS, typically only Y)
  - Interpolate: ✓
- [ ] `PlayerController` script attached
- [ ] `CharacterController` component attached
- [ ] `PlayerInput` component attached
  - Actions: Assign PlayerInputActions asset
  - Default Map: "Player"
- [ ] Camera Holder child object with Camera

### Player Prefab Hierarchy

```
Player (NetworkObject, ClientNetworkTransform, PlayerController, CharacterController, PlayerInput)
├── CameraHolder (Transform)
│   └── PlayerCamera (Camera, AudioListener)
├── Model (Character mesh/animator)
└── GroundCheck (empty transform at feet)
```

---

## 🎯 Spawn Points Setup

### Gameplay Scene Spawn Points

1. **Create SpawnPoint objects:**
   - [ ] Create empty GameObject named `SpawnPoint_1`
   - [ ] Add `SpawnPoint` component
   - [ ] Position where players should spawn
   - [ ] Rotate to face desired direction

2. **Create additional spawn points:**
   - [ ] `SpawnPoint_2` (for 2nd player)
   - [ ] `SpawnPoint_3` (for 3rd player)
   - [ ] `SpawnPoint_4` (for 4th player)

3. **Create SpawnManager:**
   - [ ] Create empty GameObject named `SpawnManager`
   - [ ] Add `SpawnManager` component
   - [ ] Check "Auto Find Spawn Points" is enabled

---

## 🖥️ MainMenu UI Setup

### MainMenuUI Component Configuration

1. **Create Canvas with UI elements:**
   - [ ] Canvas (Screen Space - Overlay)

2. **Assign to MainMenuUI script:**
   - [ ] `_hostButton`: Button for hosting
   - [ ] `_joinButton`: Button to show join panel
   - [ ] `_quitButton`: Button to quit game
   - [ ] `_joinPanel`: Panel with join code input
   - [ ] `_joinCodeInput`: TMP_InputField for join code
   - [ ] `_connectButton`: Button to connect
   - [ ] `_cancelJoinButton`: Button to cancel
   - [ ] `_hostPanel`: Panel showing join code after hosting
   - [ ] `_joinCodeDisplay`: TextMeshProUGUI showing the code
   - [ ] `_copyCodeButton`: Button to copy code
   - [ ] `_cancelHostButton`: Button to cancel hosting
   - [ ] `_connectingPanel`: Panel shown during connection
   - [ ] `_statusText`: TextMeshProUGUI for status
   - [ ] `_errorText`: TextMeshProUGUI for errors

---

## 🎬 Scene Setup

### Required Scenes in Build Settings

1. **File > Build Settings > Scenes In Build:**
   - [ ] `MainMenu` scene (index 0 - first to load)
   - [ ] `Gameplay` scene (index 1)

### MainMenu Scene Objects

- [ ] NetworkManager (DontDestroyOnLoad)
- [ ] Canvas with MainMenuUI
- [ ] EventSystem

### Gameplay Scene Objects

- [ ] SpawnManager with SpawnPoints
- [ ] LobbyManager (if using in-game lobby)
- [ ] GameManager
- [ ] DayNightManager
- [ ] Terrain/World

---

## ✅ Verification Steps

### Test Host Mode

1. [ ] Play in Unity Editor
2. [ ] Click "Host Game"
3. [ ] Verify join code appears
4. [ ] Verify player spawns at spawn point
5. [ ] Verify movement works (WASD)
6. [ ] Verify camera look works (mouse)

### Test Client Mode (requires 2nd instance or build)

1. [ ] Build the game (Development build recommended)
2. [ ] Start host in editor
3. [ ] Start client in build, enter join code
4. [ ] Verify connection succeeds
5. [ ] Verify both players visible to each other
6. [ ] Verify movement syncs between players

### Test Disconnect

1. [ ] While connected as client, close the window
2. [ ] Verify host sees player disconnect
3. [ ] While hosting, stop play mode
4. [ ] Verify client shows "Connection Lost" and returns to menu

---

## 🐛 Common Issues

| Issue | Solution |
|-------|----------|
| Player not spawning | Check Player Prefab assigned in NetworkManager |
| Movement not syncing | Add ClientNetworkTransform to Player prefab |
| Can't connect | Check firewall, verify IP/Port correct |
| UI not responding | Check EventSystem exists in scene |
| Null reference in NetworkGameManager | Ensure NetworkManager object has all required components |
