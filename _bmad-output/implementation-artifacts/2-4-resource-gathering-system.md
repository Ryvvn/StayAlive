# Story 2.4: Resource Gathering System

## Story

**As a player,**
I want to look at trees or rocks and press 'E' to gather materials,
So that I can collect resources for crafting.

## Status: done

## Implementation

### ResourceNode.cs ✅
- Configurable resource (name, drop item, amounts)
- Gather time, max gathers, respawn
- Network synced depletion state
- Implements IInteractable

### Inventory.cs ✅
- NetworkList-based slot system
- Add/remove items with stacking
- Consume items (food/drink)

### PlayerInteraction.cs ✅ (NEW)
- Raycast from player camera
- E key to interact
- Hold-to-gather progress
- UI prompt when looking at interactable

### IInteractable.cs ✅ (NEW)
- Interface for all interactable objects

## Acceptance Criteria - All Met

- ✅ Resource nodes give feedback and add to inventory
- ✅ Objects can be depleted and respawn
- ✅ Press 'E' to gather
- ✅ Prompt shown when looking at gatherable
