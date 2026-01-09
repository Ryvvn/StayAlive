# Story 3-1: Crafting System

## Story

**As a player,**
I want to combine gathered resources into useful items and building blocks,
So that I can survive longer and fortify my base.

## Acceptance Criteria

- [ ] Can define Crafting Recipes (Input Items -> Output Item)
- [ ] Crafting UI displaying available recipes
- [ ] Can craft an item if resources are available (consumes inputs, adds output)
- [ ] Cannot craft if resources are missing
- [ ] Crafting takes time (optional, instant for now)
- [ ] Support for multiple categories (e.g. "Basics", "Building", "Weapons")

## Technical Design

### Data Structures
- **`CraftingRecipe` (ScriptableObject):**
  - `string RecipeName`
  - `Sprite Icon`
  - `List<InventoryItem> Inputs` (Item + Count)
  - `InventoryItem Output`
  - `float CraftingDuration`

- **`CraftingCategory` (Enum/SO):** Group recipes.

### Components
- **`CraftingManager`:** 
  - Central registry of all recipes (loaded from Resources/Addressables).
  - Validates if a player has resources.
  - Handles the `Craft(Recipe, PlayerInventory)` action.
  
- **`CraftingUI`:**
  - List view of recipes.
  - Detail view showing costs and "Craft" button.
  - Updates availability status (gray out if can't afford).

### Integration
- **`PlayerController`:** Needs to toggle Crafting UI (e.g. 'Tab' or 'C' key).
- **`Inventory`:** Needs `HasItems(List<InventoryItem>)` and `RemoveItems(List<InventoryItem>)` methods.

## Dependencies
- Inventory System (Story 2-4)
- Interaction System (Input)
