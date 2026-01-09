using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System.Collections.Generic;

/// <summary>
/// Procedural world generator using Unity Terrain.
/// Generates smooth, Muck-like terrain with height variation.
/// </summary>
[RequireComponent(typeof(Terrain), typeof(TerrainCollider))]
public class WorldGenerator : NetworkBehaviour
{
    #region Singleton
    public static WorldGenerator Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        _terrain = GetComponent<Terrain>();
        _terrainCollider = GetComponent<TerrainCollider>();
        
        // Fix Pink Terrain: Ensure material is set
        if (_terrain.materialTemplate == null)
        {
            // Try to find standard terrain shader
            Shader terrainShader = Shader.Find("Nature/Terrain/Diffuse");
            if (terrainShader == null) terrainShader = Shader.Find("Standard");
            
            if (terrainShader != null)
            {
                _terrain.materialTemplate = new Material(terrainShader);
            }
        }
    }
    #endregion

    #region Configuration
    [Header("Configuration")]
    [SerializeField] private WorldConfig _config;
    
    [Header("Debug")]
    [SerializeField] private bool _generateOnStart = false;
    [SerializeField] private int _debugSeed = 0;
    #endregion

    #region Network State
    public NetworkVariable<int> WorldSeed = new NetworkVariable<int>(0);
    public NetworkVariable<bool> IsGenerated = new NetworkVariable<bool>(false);
    #endregion

    #region State
    private Terrain _terrain;
    private TerrainCollider _terrainCollider;
    private GameObject _worldContainer; // Container for instantiated objects (trees, rocks)
    private List<Vector3> _spawnPositions = new List<Vector3>();
    private Vector3 _towerBasePosition;
    private NavMeshSurface _navMeshSurface;
    #endregion

    #region Unity Lifecycle
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        WorldSeed.OnValueChanged += OnSeedChanged;
        
        if (IsServer && _generateOnStart)
        {
            GenerateWorld(_debugSeed != 0 ? _debugSeed : Random.Range(1, int.MaxValue));
        }
    }

    public override void OnNetworkDespawn()
    {
        WorldSeed.OnValueChanged -= OnSeedChanged;
        base.OnNetworkDespawn();
    }
    #endregion

    #region Generation - Server
    public void GenerateWorld()
    {
        if (!IsServer) return;
        GenerateWorld(Random.Range(1, int.MaxValue));
    }

    public void GenerateWorld(int seed)
    {
        if (!IsServer) return;
        
        Debug.Log($"[WorldGenerator] Server generating world with seed: {seed}");
        WorldSeed.Value = seed;
        IsGenerated.Value = false;
        
        GenerateFromSeed(seed);
        IsGenerated.Value = true;

        _navMeshSurface = GetComponent<NavMeshSurface>();   
        // Generate NavMesh
        _navMeshSurface.BuildNavMesh(); 
    }

    private void OnSeedChanged(int previousValue, int newValue)
    {
        if (newValue == 0) return;
        
        if (!IsServer)
        {
            Debug.Log($"[WorldGenerator] Client received seed: {newValue}");
            GenerateFromSeed(newValue);
        }
    }
    #endregion

    #region Core Generation
    private void GenerateFromSeed(int seed)
    {
        if (_config == null)
        {
            Debug.LogError("[WorldGenerator] WorldConfig not assigned!");
            return;
        }
        
        ClearWorld();
        Random.InitState(seed);
        
        // Setup Terrain Data
        TerrainData terrainData = new TerrainData();
        terrainData.heightmapResolution = _config.HeightmapResolution;
        terrainData.size = new Vector3(_config.MapSize, Mathf.Abs(_config.TerrainHeight), _config.MapSize);
        
        // Generate Heights
        float[,] heights = GenerateHeights(seed);
        terrainData.SetHeights(0, 0, heights);
        
        // Set Textures (if any)
        if (_config.TerrainLayers != null && _config.TerrainLayers.Length > 0)
        {
            terrainData.terrainLayers = _config.TerrainLayers;
        }
        
        // Apply to Terrain
        // Apply to Terrain
        _terrain.terrainData = terrainData;
        
        // Force collider update by re-assigning
        _terrainCollider.terrainData = null;
        _terrainCollider.terrainData = terrainData;
        _terrain.Flush(); // Force visual update

        
        // Center terrain so (0,0,0) is in the middle (optional, but keeps logic similar)
        // Default Unity terrain starts at (0,0,0) and extends to +x,+z.
        // Let's offset the transform so (0,0,0) is center of map.
        float offset = -_config.MapSize / 2f;
        transform.position = new Vector3(offset, 0, offset);

        // Container for objects
        _worldContainer = new GameObject("WorldObjects");
        _worldContainer.transform.SetParent(transform);
        _worldContainer.transform.localPosition = Vector3.zero; // Local to terrain root
        
        // Place Objects
        PlaceResources();
        PlaceSpecialObjects();
        
        Debug.Log($"[WorldGenerator] World generated using Terrain system. Seed: {seed}");
    }

    private void ClearWorld()
    {
        if (_worldContainer != null) Destroy(_worldContainer);
        
        // Clean up spawned objects container if found by name (cleanup from previous runs)
        var existing = transform.Find("WorldObjects");
        if (existing != null) Destroy(existing.gameObject);
        
        // Note: We don't destroy Terrain component, just update its data
    }
    #endregion

    #region Height Map
    private float[,] GenerateHeights(int seed)
    {
        int res = _config.HeightmapResolution;
        float[,] heights = new float[res, res];
        
        float mapSize = _config.MapSize;
        float halfMap = mapSize / 2f;
        
        float minNoise = float.MaxValue;
        float maxNoise = float.MinValue;

        // Pass 1: Generate & Find Range
        for (int x = 0; x < res; x++)
        {
            for (int z = 0; z < res; z++)
            {
                // Calculate world position for noise
                float normalizedX = x / (float)(res - 1);
                float normalizedZ = z / (float)(res - 1);
                
                float worldX = normalizedX * mapSize;
                float worldZ = normalizedZ * mapSize;
                
                // Centered coords for distance checks
                float centerX = worldX - halfMap;
                float centerZ = worldZ - halfMap;
                
                // --- Noise Generation ---
                float height = 0f;
                float amplitude = 1f;
                float frequency = _config.HeightNoiseScale;
                float maxValue = 0f;

                float seedOffsetX = (seed % 10000) * 0.1f;  // Keep it reasonable
                float seedOffsetZ = (seed % 13337) * 0.1f;  // Different prime for variation
                
                for (int i = 0; i < _config.HeightOctaves; i++)
                {
                    // Use seed offset
                    float sampleX = (centerX + seedOffsetX) * frequency;
                    float sampleZ = (centerZ + seedOffsetZ) * frequency;
                    
                    // Unity Perlin is 0-1, we want some variance
                    height += Mathf.PerlinNoise(sampleX, sampleZ) * amplitude;
                    maxValue += amplitude;
                    
                    amplitude *= _config.HeightPersistence;
                    frequency *= _config.HeightLacunarity;
                }
                
                height /= maxValue; // 0-1 range approx
                
                heights[z, x] = height; // Temporary store
                
                if (height < minNoise) minNoise = height;
                if (height > maxNoise) maxNoise = height;
            }
        }
        
        // Pass 2: Normalize and Apply Curves
        float range = maxNoise - minNoise;
        if (range < 0.001f) range = 1f;

        for (int x = 0; x < res; x++)
        {
            for (int z = 0; z < res; z++)
            {
                float h = heights[z, x];
                
                // Normalize to 0-1
                h = (h - minNoise) / range;
                
                // Power curve for valleys/peaks
                h = Mathf.Pow(h, 1.5f);
                
                heights[z, x] = h;
            }
        }
        
        return heights;
    }
    #endregion

    #region Object Placement
    private void PlaceResources()
    {
        // Simple random placement attempt loop
        // Alternatively could iterate grid, but randomness is more natural for non-grid terrain
        
        int attemptCount = (int)(_config.MapSize * _config.MapSize * 0.5f); // Heuristic based on area
        
        for (int i = 0; i < attemptCount; i++)
        {
            // Random local position (0 to MapSize)
            float lx = Random.Range(0f, _config.MapSize);
            float lz = Random.Range(0f, _config.MapSize);
            
            // World position (adjusted by terrain transform offset)
            float wx = lx + transform.position.x;
            float wz = lz + transform.position.z;
            
            // Check center safety
            float dist = Vector3.Distance(new Vector3(wx, 0, wz), Vector3.zero);
            if (dist < _config.FlatCenterRadius + 5f) continue;
            
            // Get Height
            float h = _terrain.SampleHeight(new Vector3(wx, 0, wz));
            
            // Trees
            if (_config.TreePrefabs != null && _config.TreePrefabs.Length > 0)
            {
                if (h < _config.TreeMaxHeight && Random.value < _config.TreeDensity * 0.05f) // Adjust probability for loop count
                {
                     // Noise check for clustering
                    float noise = Mathf.PerlinNoise((wx + 500) * _config.TreeNoiseScale, (wz + 500) * _config.TreeNoiseScale);
                    if (noise > 0.4f)
                    {
                        PlacePrefab(_config.TreePrefabs, new Vector3(wx, h, wz));
                    }
                    continue; // Slot taken
                }
            }
            
            // Rocks
            if (_config.RockPrefabs != null && _config.RockPrefabs.Length > 0)
            {
                float chance = _config.RockDensity * 0.05f;
                if (_config.RocksPreferHighGround)
                {
                    float ratio = h / _config.TerrainHeight;
                    chance *= (0.5f + ratio * 2f);
                }
                
                if (Random.value < chance)
                {
                    float noise = Mathf.PerlinNoise((wx + 1000) * _config.RockNoiseScale, (wz + 1000) * _config.RockNoiseScale);
                    if (noise > 0.5f)
                    {
                        PlacePrefab(_config.RockPrefabs, new Vector3(wx, h, wz));
                    }
                }
            }
        }
    }

    private void PlacePrefab(GameObject[] prefabs, Vector3 position)
    {
        if (prefabs == null || prefabs.Length == 0) return;
        GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
        if (prefab == null) return;
        
        Quaternion rot = Quaternion.Euler(0, Random.Range(0, 360f), 0);
        
        // Instantiate as child of container
        // Note: Position is world space
        GameObject obj = Instantiate(prefab, position, rot, _worldContainer.transform);
    }

    private void PlaceSpecialObjects()
    {
        // Tower Base at (0, y, 0)
        float centerH = _terrain.SampleHeight(Vector3.zero);
        _towerBasePosition = new Vector3(0, centerH + transform.position.y, 0); // terrain Y is usually 0 but just in case
        
        if (_config.TowerBasePrefab != null)
        {
            Instantiate(_config.TowerBasePrefab, _towerBasePosition, Quaternion.identity, _worldContainer.transform);
        }
        
        // Spawn Points
        _spawnPositions.Clear();
        if (_config.SpawnPointPrefab != null)
        {
            for (int i = 0; i < _config.SpawnPointCount; i++)
            {
                float angle = (i / (float)_config.SpawnPointCount) * Mathf.PI * 2f;
                float x = Mathf.Cos(angle) * _config.SpawnDistanceFromCenter;
                float z = Mathf.Sin(angle) * _config.SpawnDistanceFromCenter;
                
                float h = _terrain.SampleHeight(new Vector3(x, 0, z));
                Vector3 pos = new Vector3(x, h + 1f, z);
                
                _spawnPositions.Add(pos);
                Instantiate(_config.SpawnPointPrefab, pos, Quaternion.identity, _worldContainer.transform);
            }
        }
    }
    #endregion

    #region Public API
    public Vector3 GetRandomSpawnPosition()
    {
        if (_spawnPositions.Count == 0) return new Vector3(0, 2, 0);
        return _spawnPositions[Random.Range(0, _spawnPositions.Count)];
    }
    
    public Vector3 GetTowerBasePosition() => _towerBasePosition;
    #endregion
}
