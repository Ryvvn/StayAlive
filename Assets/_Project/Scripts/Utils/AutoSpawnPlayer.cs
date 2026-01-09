using UnityEngine;
using Unity.Netcode;
using System.Collections;

/// <summary>
/// Debug utility to auto-start host and spawn player directly in Gameplay scene.
/// Useful for testing without going through MainMenu.
/// Add this to an empty GameObject in your Gameplay scene.
/// </summary>
public class AutoSpawnPlayer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _startDelay = 0.5f;
    [SerializeField] private bool _autoStartOnPlay = true;
    
    [Header("Debug")]
    [SerializeField] private bool _showDebugLogs = true;

    private void Start()
    {
        if (_autoStartOnPlay)
        {
            StartCoroutine(AutoStartAndSpawn());
        }
    }

    private IEnumerator AutoStartAndSpawn()
    {
        Log("Starting auto host and spawn sequence...");
        
        // Wait a frame for everything to initialize
        yield return new WaitForSeconds(_startDelay);
        
        // Check NetworkManager
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("[AutoSpawnPlayer] NetworkManager.Singleton is null!");
            yield break;
        }
        
        // Start host if not already started
        if (!NetworkManager.Singleton.IsListening)
        {
            Log("Starting host...");
            
            // Disable auto player spawn (we'll spawn manually)
            NetworkManager.Singleton.NetworkConfig.PlayerPrefab = null;
            
            bool success = NetworkManager.Singleton.StartHost();
            if (!success)
            {
                Debug.LogError("[AutoSpawnPlayer] Failed to start host!");
                yield break;
            }
            
            Log("Host started successfully!");
        }
        else
        {
            Log("Network already listening, skipping host start");
        }
        
        // Wait for network to be fully ready
        yield return new WaitUntil(() => NetworkManager.Singleton.IsServer && NetworkManager.Singleton.IsListening);
        yield return new WaitForSeconds(0.2f); // Small additional delay for stability
        
        Log("Network ready, spawning player...");
        
        // Get player prefab
        GameObject playerPrefab = GetPlayerPrefab();
        if (playerPrefab == null)
        {
            Debug.LogError("[AutoSpawnPlayer] No player prefab found!");
            yield break;
        }
        
        // Get spawn position
        Vector3 spawnPos = Vector3.up * 2f; // Default spawn above ground
        Quaternion spawnRot = Quaternion.identity;
        
        if (SpawnManager.Instance != null)
        {
            var spawnData = SpawnManager.Instance.GetNextSpawnPoint();
            spawnPos = spawnData.position;
            spawnRot = spawnData.rotation;
            Log($"Using SpawnManager position: {spawnPos}");
        }
        else
        {
            // Try to find a spawn point in scene
            var spawnPoint = FindObjectOfType<SpawnPoint>();
            if (spawnPoint != null)
            {
                spawnPos = spawnPoint.transform.position;
                spawnRot = spawnPoint.transform.rotation;
                Log($"Found SpawnPoint in scene: {spawnPos}");
            }
            else
            {
                Log("No spawn point found, using default position");
            }
        }
        
        // Instantiate and spawn player
        GameObject playerObj = Instantiate(playerPrefab, spawnPos, spawnRot);
        NetworkObject networkObj = playerObj.GetComponent<NetworkObject>();
        
        if (networkObj != null)
        {
            networkObj.SpawnAsPlayerObject(NetworkManager.Singleton.LocalClientId, true);
            Log($"Player spawned successfully at {spawnPos}!");
            
            // Lock cursor for FPS controls
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Debug.LogError("[AutoSpawnPlayer] Player prefab missing NetworkObject component!");
            Destroy(playerObj);
        }
    }

    private GameObject GetPlayerPrefab()
    {
        // Try NetworkGameManager first
        if (NetworkGameManager.Instance != null && NetworkGameManager.Instance.PlayerPrefab != null)
        {
            Log("Got player prefab from NetworkGameManager");
            return NetworkGameManager.Instance.PlayerPrefab;
        }
        
        // Try to find in Resources
        GameObject prefab = Resources.Load<GameObject>("Player");
        if (prefab != null)
        {
            Log("Got player prefab from Resources");
            return prefab;
        }
        
        // Try to find prefab by name in scene (for testing)
        var existingPlayer = GameObject.Find("Player");
        if (existingPlayer != null && existingPlayer.GetComponent<NetworkObject>() != null)
        {
            Log("Found existing Player in scene, using as template");
            return existingPlayer;
        }
        
        return null;
    }

    private void Log(string message)
    {
        if (_showDebugLogs)
        {
            Debug.Log($"[AutoSpawnPlayer] {message}");
        }
    }

    [ContextMenu("Force Start Host and Spawn")]
    public void ForceStartAndSpawn()
    {
        StartCoroutine(AutoStartAndSpawn());
    }
}
