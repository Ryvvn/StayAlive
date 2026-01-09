using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using System.Collections.Generic;

/// <summary>
/// Muck-style lobby UI showing connected players, ready status, and start game.
/// Shows after hosting/joining, before gameplay starts.
/// </summary>
public class LobbyUI : MonoBehaviour
{
    #region UI References
    [Header("Lobby Panel")]
    [SerializeField] private GameObject _lobbyPanel;
    
    [Header("Join Code Display")]
    [SerializeField] private TextMeshProUGUI _joinCodeText;
    [SerializeField] private Button _copyCodeButton;
    
    [Header("Player List")]
    [SerializeField] private Transform _playerListContainer;
    [SerializeField] private GameObject _playerEntryPrefab;
    
    [Header("Controls")]
    [SerializeField] private Button _readyButton;
    [SerializeField] private TextMeshProUGUI _readyButtonText;
    [SerializeField] private Button _startGameButton;
    [SerializeField] private Button _leaveLobbyButton;
    
    [Header("Status")]
    [SerializeField] private TextMeshProUGUI _statusText;
    #endregion

    #region State
    private Dictionary<ulong, GameObject> _playerEntries = new();
    private bool _isReady = false;
    #endregion

    #region Unity Lifecycle
    private bool _subscribedToLobbyManager = false;
    
    private void Start()
    {
        // Button listeners
        _copyCodeButton?.onClick.AddListener(OnCopyCodeClicked);
        _readyButton?.onClick.AddListener(OnReadyClicked);
        _startGameButton?.onClick.AddListener(OnStartGameClicked);
        _leaveLobbyButton?.onClick.AddListener(OnLeaveLobbyClicked);
        
        // Subscribe to NetworkGameManager
        if (NetworkGameManager.Instance != null)
        {
            NetworkGameManager.Instance.OnHostStarted += ShowLobby;
            NetworkGameManager.Instance.OnClientConnected += ShowLobby;
        }
        
        // Hide by default
        HideLobby();
    }
    
    private void SubscribeToLobbyManager()
    {
        if (_subscribedToLobbyManager) return;
        if (LobbyManager.Instance == null) return;
        
        LobbyManager.Instance.OnPlayerJoined += HandlePlayerJoined;
        LobbyManager.Instance.OnPlayerLeft += HandlePlayerLeft;
        LobbyManager.Instance.OnPlayerReadyChanged += HandlePlayerReadyChanged;
        LobbyManager.Instance.OnGameStarting += HandleGameStarting;
        
        if (LobbyManager.Instance.Players != null)
        {
            LobbyManager.Instance.Players.OnListChanged += HandlePlayersListChanged;
        }
        
        _subscribedToLobbyManager = true;
        Debug.Log("[LobbyUI] Subscribed to LobbyManager");
    }
    
    private void OnDestroy()
    {
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.OnPlayerJoined -= HandlePlayerJoined;
            LobbyManager.Instance.OnPlayerLeft -= HandlePlayerLeft;
            LobbyManager.Instance.OnPlayerReadyChanged -= HandlePlayerReadyChanged;
            LobbyManager.Instance.OnGameStarting -= HandleGameStarting;
            
            if (LobbyManager.Instance.Players != null)
                LobbyManager.Instance.Players.OnListChanged -= HandlePlayersListChanged;
        }
        
        if (NetworkGameManager.Instance != null)
        {
            NetworkGameManager.Instance.OnHostStarted -= ShowLobby;
            NetworkGameManager.Instance.OnClientConnected -= ShowLobby;
        }
    }
    #endregion

    #region Lobby Display
    public void ShowLobby()
    {
        // Subscribe to LobbyManager now that network is ready
        SubscribeToLobbyManager();
        
        _lobbyPanel?.SetActive(true);
        
        // Show join code
        if (_joinCodeText != null && NetworkGameManager.Instance != null)
        {
            string code = NetworkGameManager.Instance.CurrentJoinCode ?? "N/A";
            _joinCodeText.text = code;
        }
        
        // Show/hide host-only buttons
        bool isHost = NetworkManager.Singleton?.IsHost == true;
        _startGameButton?.gameObject.SetActive(isHost);
        
        // Sync ready state from server (host starts auto-ready)
        _isReady = isHost;
        
        // If host, also tell server we're ready
        if (isHost && LobbyManager.Instance != null)
        {
            // Host is already marked as ready in LobbyManager.OnNetworkSpawn
            // Just sync our local state
        }
        
        UpdateReadyButton();
        
        // Refresh player list
        RefreshPlayerList();
        
        UpdateStatus();
        
        Debug.Log($"[LobbyUI] Lobby shown. IsHost: {isHost}, IsReady: {_isReady}");
    }
    
    public void HideLobby()
    {
        _lobbyPanel?.SetActive(false);
    }
    
    private void RefreshPlayerList()
    {
        // Clear existing entries
        foreach (var entry in _playerEntries.Values)
        {
            if (entry != null) Destroy(entry);
        }
        _playerEntries.Clear();
        
        // Add entries for each player
        if (LobbyManager.Instance != null && LobbyManager.Instance.Players != null)
        {
            foreach (var playerData in LobbyManager.Instance.Players)
            {
                AddPlayerEntry(playerData);
            }
        }
    }
    
    private void AddPlayerEntry(LobbyManager.PlayerLobbyData playerData)
    {
        if (_playerListContainer == null || _playerEntryPrefab == null)
        {
            Debug.LogWarning("[LobbyUI] Player list container or prefab not assigned!");
            return;
        }
        
        // Don't duplicate
        if (_playerEntries.ContainsKey(playerData.ClientId))
        {
            UpdatePlayerEntry(playerData);
            return;
        }
        
        GameObject entry = Instantiate(_playerEntryPrefab, _playerListContainer);
        
        // Find and set name text
        var nameText = entry.transform.Find("PlayerName")?.GetComponent<TextMeshProUGUI>();
        if (nameText != null)
        {
            string displayName = playerData.PlayerName.ToString();
            if (playerData.IsHost) displayName += " (Host)";
            nameText.text = displayName;
        }
        
        // Find and set ready indicator
        var readyIndicator = entry.transform.Find("ReadyIndicator")?.GetComponent<Image>();
        if (readyIndicator != null)
        {
            readyIndicator.color = playerData.IsReady ? Color.green : Color.red;
        }
        
        var readyText = entry.transform.Find("ReadyText")?.GetComponent<TextMeshProUGUI>();
        if (readyText != null)
        {
            readyText.text = playerData.IsReady ? "READY" : "NOT READY";
            readyText.color = playerData.IsReady ? Color.green : Color.red;
        }
        
        _playerEntries[playerData.ClientId] = entry;
    }
    
    private void UpdatePlayerEntry(LobbyManager.PlayerLobbyData playerData)
    {
        if (!_playerEntries.TryGetValue(playerData.ClientId, out GameObject entry))
            return;
        
        var readyIndicator = entry.transform.Find("ReadyIndicator")?.GetComponent<Image>();
        if (readyIndicator != null)
        {
            readyIndicator.color = playerData.IsReady ? Color.green : Color.red;
        }
        
        var readyText = entry.transform.Find("ReadyText")?.GetComponent<TextMeshProUGUI>();
        if (readyText != null)
        {
            readyText.text = playerData.IsReady ? "READY" : "NOT READY";
            readyText.color = playerData.IsReady ? Color.green : Color.red;
        }
    }
    
    private void RemovePlayerEntry(ulong clientId)
    {
        if (_playerEntries.TryGetValue(clientId, out GameObject entry))
        {
            if (entry != null) Destroy(entry);
            _playerEntries.Remove(clientId);
        }
    }
    
    private void UpdateReadyButton()
    {
        if (_readyButtonText != null)
        {
            _readyButtonText.text = _isReady ? "READY!" : "NOT READY";
        }
        
        if (_readyButton != null)
        {
            // Change the button's image color directly
            var btnImage = _readyButton.GetComponent<Image>();
            if (btnImage != null)
            {
                btnImage.color = _isReady ? new Color(0.2f, 0.6f, 0.3f) : new Color(0.5f, 0.5f, 0.5f);
            }
        }
    }
    
    private void UpdateStatus()
    {
        if (_statusText == null) return;
        
        int playerCount = 0;
        int readyCount = 0;
        
        if (LobbyManager.Instance != null && LobbyManager.Instance.Players != null)
        {
            playerCount = LobbyManager.Instance.Players.Count;
            foreach (var p in LobbyManager.Instance.Players)
            {
                if (p.IsReady) readyCount++;
                Debug.Log($"[LobbyUI] Player {p.ClientId}: IsReady={p.IsReady}");
            }
        }
        else
        {
            Debug.LogWarning("[LobbyUI] LobbyManager.Instance or Players is null!");
        }
        
        _statusText.text = $"Players: {playerCount}/4  |  Ready: {readyCount}/{playerCount}";
        Debug.Log($"[LobbyUI] UpdateStatus - Players: {playerCount}, Ready: {readyCount}");
        
        // Enable/disable start button based on ready count
        if (_startGameButton != null && NetworkManager.Singleton?.IsHost == true)
        {
            bool canStart = readyCount == playerCount && playerCount > 0;
            _startGameButton.interactable = canStart;
            Debug.Log($"[LobbyUI] Start button interactable: {canStart}");
        }
    }
    #endregion

    #region Button Handlers
    private void OnCopyCodeClicked()
    {
        if (_joinCodeText != null)
        {
            GUIUtility.systemCopyBuffer = _joinCodeText.text;
            Debug.Log($"[LobbyUI] Copied join code: {_joinCodeText.text}");
        }
    }
    
    private void OnReadyClicked()
    {
        _isReady = !_isReady;
        UpdateReadyButton();
        
        // Tell server
        LobbyManager.Instance?.SetReadyServerRpc(_isReady);
        
        Debug.Log($"[LobbyUI] Ready status: {_isReady}");
    }
    
    private void OnStartGameClicked()
    {
        Debug.Log("[LobbyUI] Starting game...");
        LobbyManager.Instance?.RequestStartGameServerRpc();
    }
    
    private void OnLeaveLobbyClicked()
    {
        Debug.Log("[LobbyUI] Leaving lobby...");
        NetworkGameManager.Instance?.Disconnect();
        HideLobby();
        
        // Show main menu again
        var mainMenu = FindObjectOfType<MainMenuUI>();
        if (mainMenu != null)
        {
            mainMenu.ShowMainMenu();
        }
    }
    #endregion

    #region Event Handlers
    private void HandlePlayerJoined(ulong clientId)
    {
        Debug.Log($"[LobbyUI] Player joined: {clientId}");
        RefreshPlayerList();
        UpdateStatus();
    }
    
    private void HandlePlayerLeft(ulong clientId)
    {
        Debug.Log($"[LobbyUI] Player left: {clientId}");
        RemovePlayerEntry(clientId);
        UpdateStatus();
    }
    
    private void HandlePlayerReadyChanged(ulong clientId, bool isReady)
    {
        Debug.Log($"[LobbyUI] Player {clientId} ready: {isReady}");
        RefreshPlayerList();
        UpdateStatus();
    }
    
    private void HandlePlayersListChanged(NetworkListEvent<LobbyManager.PlayerLobbyData> changeEvent)
    {
        RefreshPlayerList();
        UpdateStatus();
    }
    
    private void HandleGameStarting()
    {
        Debug.Log("[LobbyUI] Game starting!");
        if (_statusText != null)
        {
            _statusText.text = "Starting game...";
        }
        HideLobby();
    }
    #endregion
}
