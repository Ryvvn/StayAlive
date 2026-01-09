#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

/// <summary>
/// Editor utility to auto-create NetworkManager hierarchy.
/// Access via: Tools > StayAlive > Create Network Manager
/// </summary>
public class NetworkManagerCreator : Editor
{
    [MenuItem("Tools/StayAlive/Create Network Manager")]
    public static void CreateNetworkManager()
    {
        // Check if NetworkManager already exists
        if (FindObjectOfType<NetworkManager>() != null)
        {
            if (!EditorUtility.DisplayDialog("NetworkManager Exists", 
                "A NetworkManager already exists in the scene. Create another one?", 
                "Yes", "No"))
            {
                return;
            }
        }
        
        // Create NetworkManager GameObject
        GameObject networkManagerGO = new GameObject("NetworkManager");
        
        // Add NetworkManager component
        NetworkManager networkManager = networkManagerGO.AddComponent<NetworkManager>();
        
        // Add UnityTransport
        UnityTransport transport = networkManagerGO.AddComponent<UnityTransport>();
        
        // Configure transport
        transport.ConnectionData.Port = 7777;
        
        // Add our NetworkGameManager (this is NOT a NetworkBehaviour)
        // LobbyManager will be spawned dynamically when hosting/joining
        networkManagerGO.AddComponent<NetworkGameManager>();
        
        // Try to find and assign player prefab
        string[] playerPrefabGuids = AssetDatabase.FindAssets("Player t:Prefab", new[] { "Assets/_Project/Prefabs" });
        if (playerPrefabGuids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(playerPrefabGuids[0]);
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (playerPrefab != null && playerPrefab.GetComponent<NetworkObject>() != null)
            {
                networkManager.NetworkConfig.PlayerPrefab = playerPrefab;
                Debug.Log($"[NetworkManagerCreator] Assigned Player prefab: {path}");
            }
            else
            {
                Debug.LogWarning("[NetworkManagerCreator] Found Player prefab but it's missing NetworkObject component!");
            }
        }
        else
        {
            Debug.LogWarning("[NetworkManagerCreator] Player prefab not found in Assets/_Project/Prefabs/");
        }
        
        // Select the created object
        Selection.activeGameObject = networkManagerGO;
        
        Debug.Log("[NetworkManagerCreator] NetworkManager created successfully!");
        EditorUtility.DisplayDialog("NetworkManager Created", 
            "NetworkManager has been created with LobbyManager.\n\n" +
            "Make sure to:\n" +
            "1. Assign Player Prefab if not auto-detected\n" +
            "2. Add NetworkObject to Player prefab\n" +
            "3. Add this scene to Build Settings", "OK");
    }
    
    [MenuItem("Tools/StayAlive/Create Spawn Points (4)")]
    public static void CreateSpawnPoints()
    {
        // Create SpawnManager
        GameObject spawnManagerGO = new GameObject("SpawnManager");
        SpawnManager spawnManager = spawnManagerGO.AddComponent<SpawnManager>();
        
        // Create 4 spawn points in a square pattern
        Vector3[] positions = new Vector3[]
        {
            new Vector3(-5, 0, -5),
            new Vector3(5, 0, -5),
            new Vector3(-5, 0, 5),
            new Vector3(5, 0, 5)
        };
        
        Vector3[] rotations = new Vector3[]
        {
            new Vector3(0, 45, 0),
            new Vector3(0, -45, 0),
            new Vector3(0, 135, 0),
            new Vector3(0, -135, 0)
        };
        
        for (int i = 0; i < 4; i++)
        {
            GameObject spawnPoint = new GameObject($"SpawnPoint_{i + 1}");
            spawnPoint.transform.SetParent(spawnManagerGO.transform);
            spawnPoint.transform.position = positions[i];
            spawnPoint.transform.rotation = Quaternion.Euler(rotations[i]);
            spawnPoint.AddComponent<SpawnPoint>();
        }
        
        Selection.activeGameObject = spawnManagerGO;
        
        Debug.Log("[NetworkManagerCreator] SpawnManager with 4 SpawnPoints created!");
        EditorUtility.DisplayDialog("Spawn Points Created", 
            "SpawnManager with 4 SpawnPoints has been created.\n\n" +
            "Adjust positions as needed for your level.", "OK");
    }
    
    [MenuItem("Tools/StayAlive/Quick Setup - Complete Scene")]
    public static void QuickSetupCompleteScene()
    {
        bool proceed = EditorUtility.DisplayDialog("Quick Setup", 
            "This will create:\n" +
            "• NetworkManager (if not exists)\n" +
            "• SpawnManager with 4 SpawnPoints\n" +
            "• MainMenu UI Canvas\n\n" +
            "Continue?", "Yes", "No");
        
        if (!proceed) return;
        
        // Create NetworkManager if not exists
        if (FindObjectOfType<NetworkManager>() == null)
        {
            CreateNetworkManager();
        }
        
        // Create SpawnPoints if not exists
        if (FindObjectOfType<SpawnManager>() == null)
        {
            CreateSpawnPoints();
        }
        
        // Create MainMenu UI if not exists
        if (FindObjectOfType<MainMenuUI>() == null)
        {
            MainMenuUICreator.CreateMainMenuUI();
        }
        
        Debug.Log("[QuickSetup] Complete scene setup finished!");
        EditorUtility.DisplayDialog("Setup Complete", 
            "Scene is now configured with:\n" +
            "✓ NetworkManager\n" +
            "✓ SpawnManager with SpawnPoints\n" +
            "✓ MainMenu UI\n\n" +
            "Don't forget to add the scene to Build Settings!", "OK");
    }
}
#endif
