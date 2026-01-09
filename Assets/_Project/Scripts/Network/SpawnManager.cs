using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Manages player spawn points and handles spawn logic.
/// Finds available spawn points and assigns them to players.
/// </summary>
public class SpawnManager : NetworkBehaviour
{
    #region Singleton
    public static SpawnManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    #endregion

    #region Configuration
    [Header("Spawn Settings")]
    [SerializeField] private bool _autoFindSpawnPoints = true;
    [SerializeField] private List<SpawnPoint> _spawnPoints = new();
    [SerializeField] private Transform _fallbackSpawnPoint;
    #endregion

    #region State
    private int _nextSpawnIndex = 0;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        if (_autoFindSpawnPoints)
        {
            FindAllSpawnPoints();
        }
        
        Debug.Log($"[SpawnManager] Found {_spawnPoints.Count} spawn points");
    }
    #endregion

    #region Public API
    /// <summary>
    /// Get the next available spawn position and rotation.
    /// Uses round-robin with fallback to random if occupied.
    /// </summary>
    public (Vector3 position, Quaternion rotation) GetNextSpawnPoint()
    {
        if (_spawnPoints.Count == 0)
        {
            Debug.LogWarning("[SpawnManager] No spawn points configured, using fallback");
            return GetFallbackSpawn();
        }
        
        // Try round-robin first
        SpawnPoint point = GetAvailableSpawnPoint();
        
        if (point != null)
        {
            point.SetOccupied();
            Debug.Log($"[SpawnManager] Assigning spawn point at {point.Position}");
            return (point.Position, point.Rotation);
        }
        
        // All occupied, use random
        point = _spawnPoints[Random.Range(0, _spawnPoints.Count)];
        Debug.Log($"[SpawnManager] All spawn points occupied, using random: {point.Position}");
        return (point.Position, point.Rotation);
    }
    
    /// <summary>
    /// Register a new spawn point.
    /// </summary>
    public void RegisterSpawnPoint(SpawnPoint point)
    {
        if (!_spawnPoints.Contains(point))
        {
            _spawnPoints.Add(point);
        }
    }
    
    /// <summary>
    /// Unregister a spawn point.
    /// </summary>
    public void UnregisterSpawnPoint(SpawnPoint point)
    {
        _spawnPoints.Remove(point);
    }
    #endregion

    #region Private Methods
    private void FindAllSpawnPoints()
    {
        _spawnPoints.Clear();
        _spawnPoints.AddRange(FindObjectsOfType<SpawnPoint>());
    }
    
    private SpawnPoint GetAvailableSpawnPoint()
    {
        // Try each spawn point starting from next index
        for (int i = 0; i < _spawnPoints.Count; i++)
        {
            int index = (_nextSpawnIndex + i) % _spawnPoints.Count;
            SpawnPoint point = _spawnPoints[index];
            
            if (!point.IsOccupied)
            {
                _nextSpawnIndex = (index + 1) % _spawnPoints.Count;
                return point;
            }
        }
        
        return null;
    }
    
    private (Vector3, Quaternion) GetFallbackSpawn()
    {
        if (_fallbackSpawnPoint != null)
        {
            return (_fallbackSpawnPoint.position, _fallbackSpawnPoint.rotation);
        }
        
        // Ultimate fallback: origin
        return (Vector3.up * 2f, Quaternion.identity);
    }
    #endregion
}
