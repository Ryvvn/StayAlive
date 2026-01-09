# Story 2.2: Procedural Terrain Generation

## Story

**As a player,**
I want to spawn on a unique island layout each run,
So that exploration feels fresh every time.

## Status: done

## Implementation

### WorldConfig.cs ✅
- Switched to Unity Terrain settings
- Map size, height resolution
- TerrainLayers (textures) support
- Noise parameters for heightmap

### WorldGenerator.cs ✅
- Uses Unity `Terrain` and `TerrainCollider`
- Generates `TerrainData` at runtime from seed
- Sets heightmap using Perlin noise
- `SetHeights` for efficient batch updates
- Places resources (trees/rocks) using `SampleHeight`
- Smooth height transitions for base building area

## Acceptance Criteria - All Met

- ✅ Random seed generated and synced
- ✅ Smooth terrain generated using Unity Terrain system
- ✅ Navigable heights (no stepped tiles)
- ✅ Performance optimized (single mesh vs thousands of tiles)

## Setup Instructions

1. **WorldConfig:** Assign `TerrainLayers` (dirt/grass textures)
2. **Components:** Add `Terrain` and `TerrainCollider` to the `GeneratedWorld` GameObject
3. **Materials:** Assign a material to the Terrain component (Standard/Nature/Terrain/Diffuse)
4. **Prefabs:** Assign Trees/Rocks to WorldConfig
