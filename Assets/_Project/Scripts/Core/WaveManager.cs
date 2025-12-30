using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Manages enemy wave spawning during night phase.
/// Host-authoritative: only server spawns enemies.
/// </summary>
public class WaveManager : NetworkBehaviour
{
    #region Singleton
    public static WaveManager Instance { get; private set; }
    
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
    [Header("Wave Settings")]
    [SerializeField] private int _baseEnemiesPerWave = 5;
    [SerializeField] private float _enemiesPerWaveMultiplier = 1.2f;
    [SerializeField] private float _timeBetweenSpawns = 1f;
    [SerializeField] private float _delayBeforeFirstSpawn = 3f;
    
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject _rusherPrefab;
    [SerializeField] private GameObject _tankPrefab;
    [SerializeField] private GameObject _rangedPrefab;
    
    [Header("Spawn Points")]
    [SerializeField] private List<Transform> _spawnPoints = new();
    #endregion

    #region State
    public NetworkVariable<int> EnemiesRemaining = new(0);
    public NetworkVariable<int> TotalEnemiesThisWave = new(0);
    public NetworkVariable<bool> IsSpawning = new(false);
    
    private int _enemiesSpawned;
    private float _spawnTimer;
    private float _delayTimer;
    private List<EnemyAI> _activeEnemies = new();
    #endregion

    #region Events
    public event Action OnWaveStarted;
    public event Action OnWaveComplete;
    public event Action<int, int> OnEnemyCountChanged; // remaining, total
    #endregion

    #region Unity Lifecycle
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (DayNightManager.Instance != null)
        {
            DayNightManager.Instance.OnNightStart += StartWave;
            DayNightManager.Instance.OnDayStart += EndWaveEarly;
        }
        
        EnemiesRemaining.OnValueChanged += (prev, curr) => 
            OnEnemyCountChanged?.Invoke(curr, TotalEnemiesThisWave.Value);
    }

    public override void OnNetworkDespawn()
    {
        if (DayNightManager.Instance != null)
        {
            DayNightManager.Instance.OnNightStart -= StartWave;
            DayNightManager.Instance.OnDayStart -= EndWaveEarly;
        }
        base.OnNetworkDespawn();
    }

    private void Update()
    {
        if (!IsServer || !IsSpawning.Value) return;
        
        // Handle initial delay
        if (_delayTimer > 0)
        {
            _delayTimer -= Time.deltaTime;
            return;
        }
        
        // Spawn enemies over time
        _spawnTimer -= Time.deltaTime;
        if (_spawnTimer <= 0 && _enemiesSpawned < TotalEnemiesThisWave.Value)
        {
            SpawnEnemy();
            _spawnTimer = _timeBetweenSpawns;
        }
        
        // Check if spawning complete
        if (_enemiesSpawned >= TotalEnemiesThisWave.Value)
        {
            IsSpawning.Value = false;
        }
    }
    #endregion

    #region Wave Control - Server Only
    private void StartWave()
    {
        if (!IsServer) return;
        
        int currentWave = GameManager.Instance?.CurrentWave.Value ?? 1;
        
        // Calculate enemies for this wave
        int enemyCount = Mathf.RoundToInt(_baseEnemiesPerWave * Mathf.Pow(_enemiesPerWaveMultiplier, currentWave - 1));
        
        Debug.Log($"[WaveManager] Starting wave {currentWave} with {enemyCount} enemies");
        
        TotalEnemiesThisWave.Value = enemyCount;
        EnemiesRemaining.Value = enemyCount;
        _enemiesSpawned = 0;
        _delayTimer = _delayBeforeFirstSpawn;
        _spawnTimer = 0f;
        IsSpawning.Value = true;
        _activeEnemies.Clear();
        
        NotifyWaveStartedClientRpc(currentWave, enemyCount);
    }

    private void EndWaveEarly()
    {
        if (!IsServer) return;
        
        // Day started - kill remaining enemies or let them despawn
        IsSpawning.Value = false;
        
        // Despawn all active enemies
        foreach (var enemy in _activeEnemies)
        {
            if (enemy != null && enemy.NetworkObject != null && enemy.NetworkObject.IsSpawned)
            {
                enemy.NetworkObject.Despawn();
            }
        }
        _activeEnemies.Clear();
        
        Debug.Log("[WaveManager] Wave ended early (day started)");
    }
    #endregion

    #region Spawning - Server Only
    private void SpawnEnemy()
    {
        if (_spawnPoints.Count == 0)
        {
            Debug.LogWarning("[WaveManager] No spawn points configured!");
            return;
        }
        
        // Pick random spawn point
        Transform spawnPoint = _spawnPoints[UnityEngine.Random.Range(0, _spawnPoints.Count)];
        
        // Pick enemy type based on wave progression
        GameObject prefab = GetEnemyPrefabForWave();
        
        if (prefab == null)
        {
            Debug.LogError("[WaveManager] No enemy prefab assigned!");
            return;
        }
        
        // Spawn enemy
        GameObject enemyObj = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        NetworkObject netObj = enemyObj.GetComponent<NetworkObject>();
        
        if (netObj != null)
        {
            netObj.Spawn();
            
            EnemyAI enemy = enemyObj.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                _activeEnemies.Add(enemy);
                enemy.OnDeath += HandleEnemyDeath;
            }
        }
        
        _enemiesSpawned++;
        Debug.Log($"[WaveManager] Spawned enemy {_enemiesSpawned}/{TotalEnemiesThisWave.Value}");
    }

    private GameObject GetEnemyPrefabForWave()
    {
        int currentWave = GameManager.Instance?.CurrentWave.Value ?? 1;
        
        // Simple enemy type selection based on wave
        // Early waves: mostly rushers
        // Later waves: mix in tanks and ranged
        
        if (_rusherPrefab == null) return null;
        
        float roll = UnityEngine.Random.value;
        
        if (currentWave < 3)
        {
            // Waves 1-2: Only rushers
            return _rusherPrefab;
        }
        else if (currentWave < 5)
        {
            // Waves 3-4: 80% rushers, 20% tanks
            return roll < 0.8f ? _rusherPrefab : (_tankPrefab ?? _rusherPrefab);
        }
        else
        {
            // Wave 5+: 60% rushers, 25% tanks, 15% ranged
            if (roll < 0.6f) return _rusherPrefab;
            if (roll < 0.85f) return _tankPrefab ?? _rusherPrefab;
            return _rangedPrefab ?? _rusherPrefab;
        }
    }
    #endregion

    #region Enemy Death
    public void HandleEnemyDeath(EnemyAI enemy)
    {
        if (!IsServer) return;
        
        _activeEnemies.Remove(enemy);
        EnemiesRemaining.Value--;
        
        Debug.Log($"[WaveManager] Enemy died. Remaining: {EnemiesRemaining.Value}");
        
        // Check for wave complete
        if (EnemiesRemaining.Value <= 0 && !IsSpawning.Value)
        {
            Debug.Log("[WaveManager] Wave complete!");
            NotifyWaveCompleteClientRpc();
        }
    }
    #endregion

    #region Client RPCs
    [ClientRpc]
    private void NotifyWaveStartedClientRpc(int waveNumber, int enemyCount)
    {
        Debug.Log($"[WaveManager] Wave {waveNumber} started! Enemies: {enemyCount}");
        OnWaveStarted?.Invoke();
    }

    [ClientRpc]
    private void NotifyWaveCompleteClientRpc()
    {
        Debug.Log("[WaveManager] Wave complete!");
        OnWaveComplete?.Invoke();
    }
    #endregion

    #region Public Helpers
    public float GetWaveProgress()
    {
        if (TotalEnemiesThisWave.Value == 0) return 1f;
        return 1f - ((float)EnemiesRemaining.Value / TotalEnemiesThisWave.Value);
    }
    #endregion
}
