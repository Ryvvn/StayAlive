# Story 2.3: Player Vitals & HUD

## Story

**As a player,**
I want to see my Health, Hunger, and Thirst bars,
So that I know when to eat or heal.

## Status: done

## Implementation

### PlayerStats.cs ✅
- Health, Hunger, Thirst as NetworkVariables
- Decay over time (hunger/thirst)
- Damage when starving/dehydrated
- Death when health reaches 0
- Events for UI updates

### GameHUD.cs ✅
- Health/Hunger/Thirst bars
- Wave info, time display, notifications
- Auto-finds local player and subscribes to events

### GameHUDCreator.cs ✅ (NEW)
- Editor tool: **Tools > StayAlive > Create Game HUD**
- Creates complete HUD Canvas with all UI elements
- Auto-assigns all references to GameHUD script

## Acceptance Criteria - All Met

- ✅ HUD overlay visible showing Health, Hunger, Thirst
- ✅ Hunger and Thirst decrease over time
- ✅ Death state triggers when Health reaches 0
