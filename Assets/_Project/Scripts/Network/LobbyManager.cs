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
    [Header("Lobby Settings")]
    [SerializeField] private int _minPlayersToStart = 1;
    [SerializeField] private int _maxPlayers = 4;
    #endregion

    #region State
    public NetworkVariable<bool> IsGameStarting = new(false);
    public NetworkList<PlayerLobbyData> Players;
    
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
    private void Start()
    {
        Players = new NetworkList<PlayerLobbyData>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
            
            // Add host as first player
            AddPlayer(NetworkManager.Singleton.LocalClientId, "Host", true);
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
        
        Players.OnListChanged -= HandlePlayersChanged;
        base.OnNetworkDespawn();
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
        OnGameStarting?.Invoke();
        
        // Tell GameManager to start
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGameServerRpc();
        }
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
