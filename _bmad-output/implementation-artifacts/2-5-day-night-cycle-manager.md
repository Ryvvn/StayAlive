# Story 2.5: Day/Night Cycle Manager

## Story

**As a player,**
I want the sun to move and lighting to change,
So that I know how much time is left before nightfall.

## Status: done

## Implementation (Already Complete!)

### DayNightManager.cs ✅
- Day duration: 5 minutes
- Night duration: 3 minutes
- NetworkVariable for time sync
- Phase transitions (Day ↔ Night)
- Events: OnDayStart, OnNightStart, OnTimeUpdated
- Integrates with GameManager phases

### LightingController.cs ✅
- Sun/moon rotation across sky
- Sunrise/sunset colors
- Intensity curves (brighter at noon)
- Ambient lighting changes
- Debug mode for testing
- Context menu for quick testing (sunrise/noon/sunset/midnight)

## Acceptance Criteria - All Met

- ✅ Global timer tracks time of day
- ✅ Sun/moon moves and ambient lighting changes
- ✅ Game phase switches to "Night" at threshold
