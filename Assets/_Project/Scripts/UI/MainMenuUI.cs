using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Main Menu UI handling host/join functionality with join codes.
/// Story 1.2: Host & Client Connection UI
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    #region UI References
    [Header("Main Menu Container")]
    [SerializeField] private GameObject _mainMenuContainer; // Hide this when in lobby
    
    [Header("Main Menu Buttons")]
    [SerializeField] private Button _hostButton;
    [SerializeField] private Button _joinButton;
    [SerializeField] private Button _quitButton;
    
    [Header("Join Panel")]
    [SerializeField] private GameObject _joinPanel;
    [SerializeField] private TMP_InputField _joinCodeInput;
    [SerializeField] private Button _connectButton;
    [SerializeField] private Button _cancelJoinButton;
    
    [Header("Host Panel (Join Code Display)")]
    [SerializeField] private GameObject _hostPanel;
    [SerializeField] private TextMeshProUGUI _joinCodeDisplay;
    [SerializeField] private Button _copyCodeButton;
    [SerializeField] private Button _cancelHostButton;
    
    [Header("Status Feedback")]
    [SerializeField] private GameObject _connectingPanel;
    [SerializeField] private TextMeshProUGUI _statusText;
    [SerializeField] private TextMeshProUGUI _errorText;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        // Main menu buttons
        _hostButton?.onClick.AddListener(OnHostClicked);
        _joinButton?.onClick.AddListener(OnShowJoinPanel);
        _quitButton?.onClick.AddListener(OnQuitClicked);
        
        // Join panel buttons
        _connectButton?.onClick.AddListener(OnConnectClicked);
        _cancelJoinButton?.onClick.AddListener(OnCancelJoinClicked);
        
        // Host panel buttons
        _copyCodeButton?.onClick.AddListener(OnCopyCodeClicked);
        _cancelHostButton?.onClick.AddListener(OnCancelHostClicked);
        
        // Subscribe to network events
        if (NetworkGameManager.Instance != null)
        {
            NetworkGameManager.Instance.OnHostStarted += HandleHostStarted;
            NetworkGameManager.Instance.OnClientConnected += HandleClientConnected;
            NetworkGameManager.Instance.OnConnectionFailed += HandleConnectionFailed;
        }
        
        // Initialize UI state
        ShowMainMenu();
    }
    
    private void OnDestroy()
    {
        if (NetworkGameManager.Instance != null)
        {
            NetworkGameManager.Instance.OnHostStarted -= HandleHostStarted;
            NetworkGameManager.Instance.OnClientConnected -= HandleClientConnected;
            NetworkGameManager.Instance.OnConnectionFailed -= HandleConnectionFailed;
        }
    }
    #endregion

    #region UI State Management
    private void ShowJoinPanel()
    {
        _joinPanel?.SetActive(true);
        _hostPanel?.SetActive(false);
        _connectingPanel?.SetActive(false);
        _joinCodeInput?.Select();
        ClearError();
    }
    
    private void ShowHostPanel(string joinCode)
    {
        _joinPanel?.SetActive(false);
        _hostPanel?.SetActive(true);
        _connectingPanel?.SetActive(false);
        
        if (_joinCodeDisplay != null)
        {
            _joinCodeDisplay.text = joinCode;
        }
        ClearError();
    }
    
    private void ShowConnecting()
    {
        _connectingPanel?.SetActive(true);
        SetStatus("Connecting...");
    }
    
    private void SetStatus(string message)
    {
        if (_statusText != null)
        {
            _statusText.text = message;
        }
        Debug.Log($"[MainMenuUI] {message}");
    }
    
    private void ShowError(string message)
    {
        if (_errorText != null)
        {
            _errorText.text = message;
            _errorText.gameObject.SetActive(true);
        }
        Debug.LogWarning($"[MainMenuUI] Error: {message}");
    }
    
    private void ClearError()
    {
        if (_errorText != null)
        {
            _errorText.text = "";
            _errorText.gameObject.SetActive(false);
        }
    }
    #endregion

    #region Button Handlers
    private void OnHostClicked()
    {
        Debug.Log("[MainMenuUI] Starting Host...");
        ClearError();
        
        if (NetworkGameManager.Instance == null)
        {
            ShowError("NetworkGameManager not found!");
            return;
        }
        
        bool success = NetworkGameManager.Instance.StartHost();
        if (!success)
        {
            ShowError("Failed to start host");
        }
        // OnHostStarted event will trigger ShowHostPanel
    }
    
    private void OnShowJoinPanel()
    {
        ShowJoinPanel();
    }
    
    private void OnConnectClicked()
    {
        ClearError();
        
        if (_joinCodeInput == null)
        {
            ShowError("Join code input not configured");
            return;
        }
        
        string code = _joinCodeInput.text.Trim();
        
        if (string.IsNullOrEmpty(code))
        {
            ShowError("Please enter a join code");
            return;
        }
        
        if (NetworkGameManager.Instance == null)
        {
            ShowError("NetworkGameManager not found!");
            return;
        }
        
        Debug.Log($"[MainMenuUI] Joining with code: {code}");
        ShowConnecting();
        
        bool success = NetworkGameManager.Instance.JoinWithCode(code);
        if (!success)
        {
            _connectingPanel?.SetActive(false);
            ShowError("Failed to connect");
        }
    }
    
    private void OnCancelJoinClicked()
    {
        ShowMainMenu();
    }
    
    private void OnCancelHostClicked()
    {
        NetworkGameManager.Instance?.Disconnect();
        ShowMainMenu();
    }
    
    private void OnCopyCodeClicked()
    {
        if (_joinCodeDisplay != null)
        {
            GUIUtility.systemCopyBuffer = _joinCodeDisplay.text;
            SetStatus("Join code copied!");
            Debug.Log($"[MainMenuUI] Copied join code: {_joinCodeDisplay.text}");
        }
    }
    
    private void OnQuitClicked()
    {
        Debug.Log("[MainMenuUI] Quitting game...");
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    #endregion

    #region Network Event Handlers
    private void HandleHostStarted()
    {
        Debug.Log("[MainMenuUI] Host started successfully");
        
        // Hide main menu - LobbyUI will show itself
        HideAllPanels();
        
        // LobbyUI handles the lobby display
    }
    
    private void HandleClientConnected()
    {
        Debug.Log("[MainMenuUI] Connected to host!");
        _connectingPanel?.SetActive(false);
        
        // Hide main menu - LobbyUI will show itself
        HideAllPanels();
    }
    
    private void HandleConnectionFailed(string reason)
    {
        Debug.LogError($"[MainMenuUI] Connection failed: {reason}");
        _connectingPanel?.SetActive(false);
        ShowError($"Connection failed: {reason}");
    }
    #endregion
    
    #region Public Methods
    /// <summary>
    /// Called by LobbyUI when leaving lobby to show main menu again.
    /// </summary>
    public void ShowMainMenu()
    {
        // Show main menu container
        _mainMenuContainer?.SetActive(true);
        
        _joinPanel?.SetActive(false);
        _hostPanel?.SetActive(false);
        _connectingPanel?.SetActive(false);
        ClearError();
    }
    
    private void HideAllPanels()
    {
        // Hide entire main menu when lobby is shown
        _mainMenuContainer?.SetActive(false);
        
        _joinPanel?.SetActive(false);
        _hostPanel?.SetActive(false);
        _connectingPanel?.SetActive(false);
        ClearError();
    }
    #endregion
}
