using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Unity.Collections;

/// <summary>
/// Manages lobby state, player connections, and ready status.
/// Works with NetworkGameManager for connection handling.
/// </summary>
public class LobbyManager : NetworkBehaviour
{
    #region Singleton
    public static LobbyManager Instance { get; private set; }
    #endregion

    #region Configuration
    [Header("Lobby Settings")]
    [SerializeField] private int _minPlayersToStart = 1;
    [SerializeField] private int _maxPlayers = 4;
    #endregion

    #region State
    public NetworkVariable<bool> IsGameStarting = new(false);
    // NetworkList MUST be initialized in field declaration for Netcode to work!
    public NetworkList<PlayerLobbyData> Players = new();
    
    private Dictionary<ulong, bool> _playerReadyStatus = new();
    #endregion

    #region Events
    public event Action<ulong> OnPlayerJoined;
    public event Action<ulong> OnPlayerLeft;
    public event Action<ulong, bool> OnPlayerReadyChanged;
    public event Action OnAllPlayersReady;
    public event Action OnGameStarting;
    #endregion

    #region Data Structures
    [Serializable]
    public struct PlayerLobbyData : INetworkSerializable, IEquatable<PlayerLobbyData>
    {
        public ulong ClientId;
        public FixedString64Bytes PlayerName;
        public bool IsReady;
        public bool IsHost;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref PlayerName);
            serializer.SerializeValue(ref IsReady);
            serializer.SerializeValue(ref IsHost);
        }

        public bool Equals(PlayerLobbyData other)
        {
            return ClientId == other.ClientId;
        }
    }
    #endregion

    #region Unity Lifecycle
    private System.Collections.Generic.List<ulong> _cachedClientIds = new();
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // Dispose NetworkList before destroying to prevent memory leak
            if (Players != null)
            {
                Players.Dispose();
            }
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Keep LobbyManager alive across scene loads
        DontDestroyOnLoad(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        Debug.Log($"[LobbyManager] OnNetworkSpawn - IsServer: {IsServer}, IsClient: {IsClient}");
        
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
            
            // Add host as first player
            AddPlayer(NetworkManager.Singleton.LocalClientId, "Host", true);
            Debug.Log($"[LobbyManager] Added host player. Players count: {Players.Count}");
        }
        
        Players.OnListChanged += HandlePlayersChanged;
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }
        
        if (Players != null)
        {
            Players.OnListChanged -= HandlePlayersChanged;
        }
        base.OnNetworkDespawn();
    }
    
    private void OnDestroy()
    {
        // Dispose NetworkList to prevent memory leak
        if (Players != null)
        {
            Players.Dispose();
        }
        
        if (Instance == this)
        {
            Instance = null;
        }
    }
    #endregion

    #region Server - Player Management
    private void HandleClientConnected(ulong clientId)
    {
        if (!IsServer) return;
        
        // Don't re-add host
        if (clientId == NetworkManager.Singleton.LocalClientId) return;
        
        AddPlayer(clientId, $"Player {clientId}", false);
        Debug.Log($"[LobbyManager] Player joined: {clientId}");
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        if (!IsServer) return;
        
        RemovePlayer(clientId);
        Debug.Log($"[LobbyManager] Player left: {clientId}");
    }

    private void AddPlayer(ulong clientId, string name, bool isHost)
    {
        var playerData = new PlayerLobbyData
        {
            ClientId = clientId,
            PlayerName = name,
            IsReady = isHost, // Host is auto-ready
            IsHost = isHost
        };
        
        Players.Add(playerData);
        OnPlayerJoined?.Invoke(clientId);
    }

    private void RemovePlayer(ulong clientId)
    {
        for (int i = 0; i < Players.Count; i++)
        {
            if (Players[i].ClientId == clientId)
            {
                Players.RemoveAt(i);
                OnPlayerLeft?.Invoke(clientId);
                break;
            }
        }
    }
    #endregion

    #region Ready System
    [ServerRpc(RequireOwnership = false)]
    public void SetReadyServerRpc(bool isReady, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        
        for (int i = 0; i < Players.Count; i++)
        {
            if (Players[i].ClientId == clientId)
            {
                var playerData = Players[i];
                playerData.IsReady = isReady;
                Players[i] = playerData;
                
                Debug.Log($"[LobbyManager] Player {clientId} ready: {isReady}");
                OnPlayerReadyChanged?.Invoke(clientId, isReady);
                
                CheckAllReady();
                break;
            }
        }
    }

    private void CheckAllReady()
    {
        if (!IsServer) return;
        if (Players.Count < _minPlayersToStart) return;
        
        bool allReady = true;
        foreach (var player in Players)
        {
            if (!player.IsReady)
            {
                allReady = false;
                break;
            }
        }
        
        if (allReady)
        {
            Debug.Log("[LobbyManager] All players ready!");
            OnAllPlayersReady?.Invoke();
        }
    }
    #endregion

    #region Game Start
    [ServerRpc(RequireOwnership = false)]
    public void RequestStartGameServerRpc(ServerRpcParams rpcParams = default)
    {
        
        // Only host can start game
        if (rpcParams.Receive.SenderClientId != NetworkManager.Singleton.LocalClientId) return;
        
        if (Players.Count < _minPlayersToStart)
        {
            Debug.LogWarning($"[LobbyManager] Not enough players to start. Need {_minPlayersToStart}");
            return;
        }
        
        StartGame();
    }

    private void StartGame()
    {
        if (!IsServer) return;
        
        Debug.Log("[LobbyManager] Starting game...");
        IsGameStarting.Value = true;
        
        // Cache client IDs BEFORE scene load (NetworkList may get disposed during scene change)
        _cachedClientIds.Clear();
        foreach (var player in Players)
        {
            _cachedClientIds.Add(player.ClientId);
        }
        Debug.Log($"[LobbyManager] Cached {_cachedClientIds.Count} client IDs for spawning");
        
        // Notify clients that game is starting
        NotifyGameStartingClientRpc();
        
        // Load gameplay scene using NetworkManager's scene manager
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
        {
            Debug.Log("[LobbyManager] Loading Gameplay scene...");
            
            // Subscribe to scene loaded event to spawn players
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnGameplaySceneLoaded;
            
            NetworkManager.Singleton.SceneManager.LoadScene("Gameplay", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
        else
        {
            Debug.LogError("[LobbyManager] Cannot load scene - NetworkManager.SceneManager is null!");
        }
    }
    
    private void OnGameplaySceneLoaded(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, System.Collections.Generic.List<ulong> clientsCompleted, System.Collections.Generic.List<ulong> clientsTimedOut)
    {
        if (!IsServer) return;
        if (sceneName != "Gameplay") return;
        
        Debug.Log($"[LobbyManager] Gameplay scene loaded. Spawning {_cachedClientIds.Count} players...");
        
        // Unsubscribe
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnGameplaySceneLoaded;
        
        // Spawn players for all connected clients using CACHED list
        foreach (var clientId in _cachedClientIds)
        {
            SpawnPlayerForClient(clientId);
        }
    }
    
    private void SpawnPlayerForClient(ulong clientId)
    {
        if (!IsServer) return;
        
        // Get player prefab from NetworkGameManager (not NetworkConfig - that's for auto-spawn)
        GameObject playerPrefab = NetworkGameManager.Instance?.PlayerPrefab;
        if (playerPrefab == null)
        {
            Debug.LogError("[LobbyManager] Player prefab not assigned in NetworkGameManager!");
            return;
        }
        
        // Get spawn position from SpawnManager
        Vector3 spawnPos = Vector3.zero;
        Quaternion spawnRot = Quaternion.identity;
        
        if (SpawnManager.Instance != null)
        {
            var spawnData = SpawnManager.Instance.GetNextSpawnPoint();
            spawnPos = spawnData.position;
            spawnRot = spawnData.rotation;
        }
        
        Debug.Log($"[LobbyManager] Spawning player for client {clientId} at {spawnPos}");
        
        // Instantiate and spawn
        GameObject playerObj = Instantiate(playerPrefab, spawnPos, spawnRot);
        NetworkObject networkObj = playerObj.GetComponent<NetworkObject>();
        
        if (networkObj != null)
        {
            networkObj.SpawnAsPlayerObject(clientId, true);
            Debug.Log($"[LobbyManager] Player spawned for client {clientId}");
        }
        else
        {
            Debug.LogError("[LobbyManager] Player prefab missing NetworkObject component!");
            Destroy(playerObj);
        }
    }
    
    [ClientRpc]
    private void NotifyGameStartingClientRpc()
    {
        Debug.Log("[LobbyManager] Game starting notification received");
        OnGameStarting?.Invoke();
    }
    #endregion

    #region Event Handlers
    private void HandlePlayersChanged(NetworkListEvent<PlayerLobbyData> changeEvent)
    {
        Debug.Log($"[LobbyManager] Players list changed. Count: {Players.Count}");
    }
    #endregion

    #region Public Helpers
    public int PlayerCount => Players?.Count ?? 0;
    public bool CanStartGame => IsServer && Players.Count >= _minPlayersToStart;
    
    public PlayerLobbyData? GetPlayerData(ulong clientId)
    {
        foreach (var player in Players)
        {
            if (player.ClientId == clientId)
                return player;
        }
        return null;
    }
    #endregion
}
