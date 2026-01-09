# Story 1.4: Connection Stability Handling

Status: done

## Story

As a **player**,
I want the game to handle connection failures gracefully,
So that I know if I've been disconnected rather than the game just freezing.

## Acceptance Criteria

1. **Given** I am in a multiplayer session
   **When** the internet connection drops
   **Then** the game should detect the disconnection
   **And** a "Connection Lost" popup should appear
   **And** I should be returned to the Main Menu
   ✅ **VERIFIED:** Local disconnect detection + scene transition implemented

## Tasks / Subtasks

- [x] Task 1: Detect Disconnection Events
  - [x] Subscribe to `NetworkManager.OnClientDisconnectCallback`
  - [x] Detect local disconnect vs other player disconnect
  - [x] Added `HandleLocalDisconnect()` method

- [x] Task 2: Connection Feedback
  - [x] `OnConnectionFailed` event fired with reason
  - [x] MainMenuUI already subscribes to this event
  - [x] Error message shown to user

- [x] Task 3: Return to Main Menu
  - [x] Added `ReturnToMainMenu()` method
  - [x] Network shutdown before scene load
  - [x] Cursor unlocked for menu navigation

## Dev Notes

### Existing Implementation

**NetworkGameManager.cs:**
- ✅ Already has `OnClientDisconnected` event
- ✅ `Disconnect()` method exists
- ❓ Need to add scene transition on disconnect

**MainMenuUI.cs:**
- ✅ Has event subscriptions to NetworkGameManager
- ✅ `HandleConnectionFailed()` method exists
- ❓ Need disconnect popup

### Architecture Requirements

From game-architecture.md:
- **Error Levels:** Critical → Disconnect user / Show Error Dialog
- **Graceful handling:** Show feedback, return to safe state

### Code Hints

```csharp
// In NetworkGameManager - handle disconnection
private void HandleClientDisconnected(ulong clientId)
{
    if (clientId == NetworkManager.Singleton.LocalClientId)
    {
        // We were disconnected
        ShowDisconnectPopup("Connection Lost");
        ReturnToMainMenu();
    }
}
```

### References

- [NetworkGameManager.cs](file:///d:/UnityForWork/Project/StayAlive/Assets/_Project/Scripts/Network/NetworkGameManager.cs)
- [MainMenuUI.cs](file:///d:/UnityForWork/Project/StayAlive/Assets/_Project/Scripts/UI/MainMenuUI.cs)

## Dev Agent Record

### Agent Model Used

Claude (Anthropic)

### Debug Log References

- `[NetworkGameManager] Local disconnect: {reason}`
- `[NetworkGameManager] Returned to Main Menu`

### Completion Notes List

- ✅ Enhanced HandleClientDisconnected() to detect local disconnect
- ✅ Added HandleLocalDisconnect() with cleanup and event firing
- ✅ Added ReturnToMainMenu() with network shutdown
- ✅ Added ForceDisconnect() API for timeout/kick scenarios

### File List

| File | Action |
|------|--------|
| `Scripts/Network/NetworkGameManager.cs` | MODIFIED - Added disconnect handling |
