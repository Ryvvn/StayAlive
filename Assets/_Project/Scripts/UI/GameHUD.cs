using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Main HUD manager that coordinates all HUD elements.
/// Attach to the main HUD Canvas.
/// </summary>
public class GameHUD : MonoBehaviour
{
    #region Singleton
    public static GameHUD Instance { get; private set; }
    
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

    #region References
    [Header("Player Stats")]
    [SerializeField] private Slider _healthBar;
    [SerializeField] private TextMeshProUGUI _healthText;
    [SerializeField] private Slider _hungerBar;
    [SerializeField] private Slider _thirstBar;
    
    [Header("Weapon")]
    [SerializeField] private TextMeshProUGUI _ammoText;
    [SerializeField] private TextMeshProUGUI _weaponNameText;
    
    [Header("Wave Info")]
    [SerializeField] private TextMeshProUGUI _waveText;
    [SerializeField] private TextMeshProUGUI _enemiesText;
    [SerializeField] private Slider _waveProgressBar;
    
    [Header("Time")]
    [SerializeField] private TextMeshProUGUI _timeText;
    [SerializeField] private TextMeshProUGUI _phaseText;
    [SerializeField] private Image _phaseIcon;
    [SerializeField] private Sprite _dayIcon;
    [SerializeField] private Sprite _nightIcon;
    
    [Header("Tower Health")]
    [SerializeField] private Slider _towerHealthBar;
    [SerializeField] private TextMeshProUGUI _towerHealthText;
    
    [Header("Notifications")]
    [SerializeField] private TextMeshProUGUI _notificationText;
    [SerializeField] private float _notificationDuration = 3f;
    #endregion

    #region State
    private PlayerStats _localPlayerStats;
    private WeaponSystem _localWeaponSystem;
    private float _notificationTimer;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        // Find local player after spawn
        InvokeRepeating(nameof(FindLocalPlayer), 0.5f, 1f);
        
        // Subscribe to game events
        SubscribeToEvents();
        
        // Hide notification
        if (_notificationText != null)
        {
            _notificationText.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void Update()
    {
        UpdatePlayerStats();
        UpdateWeaponInfo();
        UpdateTimeDisplay();
        UpdateNotification();
    }
    #endregion

    #region Event Subscriptions
    private void SubscribeToEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPhaseChanged += HandlePhaseChanged;
            GameManager.Instance.OnWaveChanged += HandleWaveChanged;
        }
        
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnWaveStarted += HandleWaveStarted;
            WaveManager.Instance.OnWaveComplete += HandleWaveComplete;
            WaveManager.Instance.OnEnemyCountChanged += HandleEnemyCountChanged;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPhaseChanged -= HandlePhaseChanged;
            GameManager.Instance.OnWaveChanged -= HandleWaveChanged;
        }
        
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnWaveStarted -= HandleWaveStarted;
            WaveManager.Instance.OnWaveComplete -= HandleWaveComplete;
            WaveManager.Instance.OnEnemyCountChanged -= HandleEnemyCountChanged;
        }
    }
    #endregion

    #region Find Local Player
    private void FindLocalPlayer()
    {
        if (_localPlayerStats != null) return;
        
        foreach (var player in FindObjectsOfType<PlayerStats>())
        {
            if (player.IsOwner)
            {
                _localPlayerStats = player;
                _localWeaponSystem = player.GetComponent<WeaponSystem>();
                
                // Subscribe to player events
                _localPlayerStats.OnHealthChanged += UpdateHealthBar;
                _localPlayerStats.OnHungerChanged += UpdateHungerBar;
                _localPlayerStats.OnThirstChanged += UpdateThirstBar;
                _localPlayerStats.OnDeath += HandlePlayerDeath;
                
                CancelInvoke(nameof(FindLocalPlayer));
                Debug.Log("[GameHUD] Local player found");
                break;
            }
        }
    }
    #endregion

    #region Update Methods
    private void UpdatePlayerStats()
    {
        if (_localPlayerStats == null) return;
        
        // Health
        if (_healthBar != null)
        {
            _healthBar.value = _localPlayerStats.HealthPercentage;
        }
        if (_healthText != null)
        {
            _healthText.text = $"{Mathf.CeilToInt(_localPlayerStats.Health.Value)}/{_localPlayerStats.MaxHealth}";
        }
        
        // Hunger
        if (_hungerBar != null)
        {
            _hungerBar.value = _localPlayerStats.HungerPercentage;
        }
        
        // Thirst
        if (_thirstBar != null)
        {
            _thirstBar.value = _localPlayerStats.ThirstPercentage;
        }
    }

    private void UpdateWeaponInfo()
    {
        if (_localWeaponSystem == null) return;
        
        var weapon = _localWeaponSystem.GetCurrentWeapon();
        
        if (_ammoText != null)
        {
            _ammoText.text = $"{_localWeaponSystem.GetCurrentAmmo()} / {_localWeaponSystem.GetMagazineSize()}";
        }
        
        if (_weaponNameText != null && weapon != null)
        {
            _weaponNameText.text = weapon.WeaponName;
        }
    }

    private void UpdateTimeDisplay()
    {
        if (DayNightManager.Instance == null) return;
        
        if (_timeText != null)
        {
            _timeText.text = DayNightManager.Instance.GetFormattedTimeRemaining();
        }
        
        if (_phaseIcon != null)
        {
            bool isDay = GameManager.Instance?.IsDayPhase == true;
            _phaseIcon.sprite = isDay ? _dayIcon : _nightIcon;
        }
    }

    private void UpdateNotification()
    {
        if (_notificationText == null) return;
        
        if (_notificationTimer > 0)
        {
            _notificationTimer -= Time.deltaTime;
            if (_notificationTimer <= 0)
            {
                _notificationText.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateHealthBar(float current, float max)
    {
        if (_healthBar != null)
        {
            _healthBar.value = max > 0 ? current / max : 0;
        }
        if (_healthText != null)
        {
            _healthText.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
        }
    }

    private void UpdateHungerBar(float current, float max)
    {
        if (_hungerBar != null)
        {
            _hungerBar.value = max > 0 ? current / max : 0;
        }
    }

    private void UpdateThirstBar(float current, float max)
    {
        if (_thirstBar != null)
        {
            _thirstBar.value = max > 0 ? current / max : 0;
        }
    }
    #endregion

    #region Event Handlers
    private void HandlePhaseChanged(GameManager.GamePhase phase)
    {
        if (_phaseText != null)
        {
            _phaseText.text = phase.ToString().ToUpper();
        }
        
        // Show notification
        switch (phase)
        {
            case GameManager.GamePhase.Day:
                ShowNotification("☀️ DAY - Explore, gather, build!");
                break;
            case GameManager.GamePhase.Night:
                ShowNotification("🌙 NIGHT - Defend the tower!");
                break;
            case GameManager.GamePhase.Victory:
                ShowNotification("🎉 VICTORY! You survived!");
                break;
            case GameManager.GamePhase.Defeat:
                ShowNotification("💀 DEFEAT - The tower has fallen...");
                break;
        }
    }

    private void HandleWaveChanged(int wave)
    {
        if (_waveText != null)
        {
            _waveText.text = $"Wave {wave}";
        }
    }

    private void HandleWaveStarted()
    {
        ShowNotification("⚔️ Wave incoming!");
    }

    private void HandleWaveComplete()
    {
        ShowNotification("✅ Wave complete!");
    }

    private void HandleEnemyCountChanged(int remaining, int total)
    {
        if (_enemiesText != null)
        {
            _enemiesText.text = $"{remaining}/{total}";
        }
        
        if (_waveProgressBar != null)
        {
            _waveProgressBar.value = total > 0 ? 1f - ((float)remaining / total) : 1f;
        }
    }

    private void HandlePlayerDeath()
    {
        ShowNotification("💀 You died! Waiting for revive...");
    }
    #endregion

    #region Public Methods
    public void ShowNotification(string message)
    {
        if (_notificationText != null)
        {
            _notificationText.text = message;
            _notificationText.gameObject.SetActive(true);
            _notificationTimer = _notificationDuration;
        }
        
        Debug.Log($"[GameHUD] {message}");
    }

    public void UpdateTowerHealth(float current, float max)
    {
        if (_towerHealthBar != null)
        {
            _towerHealthBar.value = max > 0 ? current / max : 0;
        }
        if (_towerHealthText != null)
        {
            _towerHealthText.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
        }
    }
    #endregion
}
