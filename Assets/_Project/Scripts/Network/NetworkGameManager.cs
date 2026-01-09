using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

/// <summary>
/// Network game manager handling host/client setup and join codes (like Muck).
/// Following StayAlive Architecture: Host-client with join codes pattern.
/// </summary>
public class NetworkGameManager : MonoBehaviour
{
    #region Singleton
    public static NetworkGameManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    #region Configuration
    [Header("Network Settings")]
    [SerializeField] private ushort _defaultPort = 7777;
    [SerializeField] private int _maxPlayers = 4;
    
    [Header("Player")]
    [SerializeField] private GameObject _playerPrefab; // Assign this in Inspector
    #endregion

    #region State
    public string CurrentJoinCode { get; private set; }
    public bool IsHosting => NetworkManager.Singleton?.IsHost == true;
    public bool IsClient => NetworkManager.Singleton?.IsClient == true;
    public bool IsConnected => NetworkManager.Singleton?.IsConnectedClient == true;
    public int ConnectedPlayerCount => NetworkManager.Singleton?.ConnectedClients?.Count ?? 0;
    
    // Store player prefab so LobbyManager can access it for manual spawning
    public GameObject PlayerPrefab => _playerPrefab;
    #endregion

    #region Events
    public event Action OnHostStarted;
    public event Action OnClientConnected;
    public event Action OnClientDisconnected;
    public event Action<string> OnConnectionFailed;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }
    }
    #endregion

    #region Host Functions
    /// <summary>
    /// Start hosting a game. Generates a join code for others to connect.
    /// </summary>
    public bool StartHost()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("[NetworkGameManager] NetworkManager not found!");
            return false;
        }

        try
        {
            // Configure transport
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport != null)
            {
                transport.SetConnectionData("0.0.0.0", _defaultPort);
            }

            // Disable auto player spawn - we'll spawn manually after scene load
            NetworkManager.Singleton.NetworkConfig.PlayerPrefab = null;
            
            // Start host
            bool success = NetworkManager.Singleton.StartHost();
            
            if (success)
            {
                // Spawn LobbyManager dynamically (avoids memory leak from scene object)
                SpawnLobbyManager();
                
                // Generate simple join code (IP:Port for now, can use relay later)
                CurrentJoinCode = GenerateJoinCode();
                Debug.Log($"[NetworkGameManager] Host started. Join Code: {CurrentJoinCode}");
                OnHostStarted?.Invoke();
            }
            else
            {
                Debug.LogError("[NetworkGameManager] Failed to start host");
                OnConnectionFailed?.Invoke("Failed to start host");
            }

            return success;
        }
        catch (Exception e)
        {
            Debug.LogError($"[NetworkGameManager] Host error: {e.Message}");
            OnConnectionFailed?.Invoke(e.Message);
            return false;
        }
    }
    #endregion

    #region Client Functions
    /// <summary>
    /// Join a game using a join code.
    /// </summary>
    public bool JoinWithCode(string joinCode)
    {
        if (string.IsNullOrEmpty(joinCode))
        {
            OnConnectionFailed?.Invoke("Join code is empty");
            return false;
        }

        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("[NetworkGameManager] NetworkManager not found!");
            return false;
        }

        try
        {
            // Parse join code (IP:Port format for now)
            var connectionData = ParseJoinCode(joinCode);
            if (connectionData == null)
            {
                OnConnectionFailed?.Invoke("Invalid join code format");
                return false;
            }

            // Configure transport
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport != null)
            {
                transport.SetConnectionData(connectionData.Item1, connectionData.Item2);
            }

            // Connect
            bool success = NetworkManager.Singleton.StartClient();
            
            if (success)
            {
                Debug.Log($"[NetworkGameManager] Connecting to {joinCode}...");
            }
            else
            {
                Debug.LogError("[NetworkGameManager] Failed to start client");
                OnConnectionFailed?.Invoke("Failed to connect");
            }

            return success;
        }
        catch (Exception e)
        {
            Debug.LogError($"[NetworkGameManager] Join error: {e.Message}");
            OnConnectionFailed?.Invoke(e.Message);
            return false;
        }
    }
    #endregion

    #region Disconnect
    public void Disconnect()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            Debug.Log("[NetworkGameManager] Disconnected");
        }
        CurrentJoinCode = null;
    }
    #endregion

    #region Join Code Management
    private string GenerateJoinCode()
    {
        // Simple implementation: Use local IP and port
        // TODO: Implement Steam Relay or Unity Relay for NAT traversal
        string localIP = GetLocalIPAddress();
        return $"{localIP}:{_defaultPort}";
    }

    private Tuple<string, ushort> ParseJoinCode(string code)
    {
        try
        {
            var parts = code.Split(':');
            if (parts.Length == 2)
            {
                string ip = parts[0].Trim();
                ushort port = ushort.Parse(parts[1].Trim());
                return new Tuple<string, ushort>(ip, port);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[NetworkGameManager] Parse error: {e.Message}");
        }
        return null;
    }

    private string GetLocalIPAddress()
    {
        try
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[NetworkGameManager] IP lookup error: {e.Message}");
        }
        return "127.0.0.1";
    }
    #endregion

    #region Event Handlers
    private void HandleClientConnected(ulong clientId)
    {
        Debug.Log($"[NetworkGameManager] Client connected: {clientId}");
        
        // Only fire event for local client connection
        if (NetworkManager.Singleton != null && 
            clientId == NetworkManager.Singleton.LocalClientId)
        {
            OnClientConnected?.Invoke();
        }
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        Debug.Log($"[NetworkGameManager] Client disconnected: {clientId}");
        
        // Check if WE were disconnected (not just another player)
        if (NetworkManager.Singleton == null || 
            clientId == NetworkManager.Singleton.LocalClientId ||
            !NetworkManager.Singleton.IsConnectedClient)
        {
            // We got disconnected
            HandleLocalDisconnect("Connection lost");
        }
        else
        {
            // Another player disconnected
            OnClientDisconnected?.Invoke();
        }
    }
    #endregion

    #region Disconnect Handling
    /// <summary>
    /// Handle local client being disconnected from the network.
    /// </summary>
    private void HandleLocalDisconnect(string reason)
    {
        Debug.LogWarning($"[NetworkGameManager] Local disconnect: {reason}");
        
        // Clean up
        CurrentJoinCode = null;
        
        // Fire event for UI to show popup
        OnConnectionFailed?.Invoke(reason);
        
        // Return to main menu after a short delay
        Invoke(nameof(ReturnToMainMenu), 0.5f);
    }
    
    /// <summary>
    /// Safely return to main menu scene.
    /// </summary>
    public void ReturnToMainMenu()
    {
        // Ensure network is shut down
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }
        
        // Unlock cursor for menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Load main menu
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        Debug.Log("[NetworkGameManager] Returned to Main Menu");
    }
    
    /// <summary>
    /// Force disconnect with reason (for timeout, kick, etc.)
    /// </summary>
    public void ForceDisconnect(string reason)
    {
        HandleLocalDisconnect(reason);
    }
    #endregion
    
    #region LobbyManager Spawning
    private GameObject _lobbyManagerInstance;
    
    /// <summary>
    /// Spawns LobbyManager dynamically when hosting.
    /// This avoids memory leaks from NetworkList in scene objects.
    /// </summary>
    private void SpawnLobbyManager()
    {
        // Don't spawn if already exists
        if (LobbyManager.Instance != null)
        {
            Debug.Log("[NetworkGameManager] LobbyManager already exists");
            return;
        }
        
        // Create LobbyManager GameObject
        _lobbyManagerInstance = new GameObject("LobbyManager");
        _lobbyManagerInstance.transform.SetParent(transform);
        
        // Add NetworkObject (required for NetworkBehaviour)
        var networkObj = _lobbyManagerInstance.AddComponent<NetworkObject>();
        
        // Add LobbyManager
        _lobbyManagerInstance.AddComponent<LobbyManager>();
        
        // Spawn on network (as server object, not player object)
        networkObj.Spawn();
        
        Debug.Log("[NetworkGameManager] LobbyManager spawned dynamically");
    }
    
    /// <summary>
    /// Cleanup LobbyManager when disconnecting.
    /// </summary>
    private void CleanupLobbyManager()
    {
        if (_lobbyManagerInstance != null)
        {
            if (_lobbyManagerInstance.TryGetComponent<NetworkObject>(out var netObj) && netObj.IsSpawned)
            {
                netObj.Despawn();
            }
            Destroy(_lobbyManagerInstance);
            _lobbyManagerInstance = null;
        }
    }
    #endregion
}
