using UnityEngine;

/// <summary>
/// Configuration for procedural world generation using Unity Terrain.
/// </summary>
[CreateAssetMenu(fileName = "WorldConfig", menuName = "StayAlive/World Config")]
public class WorldConfig : ScriptableObject
{
    #region Map Settings
    [Header("Map Settings")]
    [Header("Terrain Settings")]
    [Tooltip("Size of the terrain in world units (Square)")]
    public int MapSize = 512;

    [Header("Textures")]
    public TerrainLayer[] TerrainLayers;
    #endregion

    #region Height Generation
    [Header("Height Settings")]
    [Tooltip("Maximum height of terrain")]
    public float TerrainHeight = 100f;
    
    [Tooltip("Resolution of the heightmap (Power of 2 + 1, e.g. 129, 257, 513, 1025)")]
    public int HeightmapResolution = 513;

    [Header("Height Noise")]
    [Tooltip("Scale of height noise (lower = smoother hills)")]
    [Range(0.01f, 0.2f)]
    public float HeightNoiseScale = 0.02f;
    
    [Tooltip("Number of noise octaves for detail")]
    [Range(1, 6)]
    public int HeightOctaves = 4;
    
    [Tooltip("How much each octave contributes")]
    [Range(0.1f, 1f)]
    public float HeightPersistence = 0.5f;
    
    [Tooltip("How much detail each octave adds")]
    [Range(1f, 3f)]
    public float HeightLacunarity = 2f;
    #endregion

    #region Flat Areas
    [Header("Flat Areas (for building)")]
    [Tooltip("Radius around center that stays flat")]
    public float FlatCenterRadius = 15f;
    
    [Tooltip("How smoothly height transitions from flat to hills")]
    public float FlatTransitionWidth = 10f;
    #endregion

    #region Resource Placement
    [Header("Trees")]
    [Range(0f, 0.5f)]
    public float TreeDensity = 0.05f; // Decreased for terrain
    public float TreeNoiseScale = 0.15f;
    public float TreeMaxHeight = 15f;
    
    [Header("Rocks")]
    [Range(0f, 0.3f)]
    public float RockDensity = 0.03f;
    public float RockNoiseScale = 0.2f;
    public bool RocksPreferHighGround = true;
    #endregion

    #region Prefabs
    [Header("Resource Prefabs")]
    public GameObject[] TreePrefabs;
    public GameObject[] RockPrefabs;
    
    [Header("Special")]
    public GameObject SpawnPointPrefab;
    public GameObject TowerBasePrefab;
    #endregion

    #region Spawn Settings
    [Header("Spawn")]
    [Tooltip("Spawn points placed at this distance from center")]
    public float SpawnDistanceFromCenter = 8f;
    
    [Tooltip("Number of spawn points to create")]
    public int SpawnPointCount = 4;
    #endregion
}
