using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Manages day/night cycle timing and phase transitions.
/// Day (~5 min): Explore, gather, craft, build
/// Night (~3 min): Defend tower, survive waves
/// </summary>
public class DayNightManager : NetworkBehaviour
{
    #region Singleton
    public static DayNightManager Instance { get; private set; }
    
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
    [Header("Cycle Duration (seconds)")]
    [SerializeField] private float _dayDuration = 300f;   // 5 minutes
    [SerializeField] private float _nightDuration = 180f; // 3 minutes
    
    [Header("Time Settings")]
    [SerializeField] private float _transitionDuration = 3f; // Visual transition time
    #endregion

    #region State
    public NetworkVariable<float> TimeRemaining = new(0f);
    public NetworkVariable<float> TimeInPhase = new(0f);
    
    private bool _isTimerRunning;
    #endregion

    #region Events
    public event Action OnDayStart;
    public event Action OnNightStart;
    public event Action<float> OnTimeUpdated; // Normalized time (0-1)
    public event Action OnTransitionStarted;
    #endregion

    #region Unity Lifecycle
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPhaseChanged += HandlePhaseChanged;
        }

        TimeRemaining.OnValueChanged += (prev, curr) => OnTimeUpdated?.Invoke(GetNormalizedTime());
    }

    public override void OnNetworkDespawn()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPhaseChanged -= HandlePhaseChanged;
        }
        base.OnNetworkDespawn();
    }

    private void Update()
    {
        if (!IsServer || !_isTimerRunning) return;
        
        TimeRemaining.Value -= Time.deltaTime;
        TimeInPhase.Value += Time.deltaTime;
        
        if (TimeRemaining.Value <= 0f)
        {
            EndCurrentPhase();
        }
    }
    #endregion

    #region Phase Management - Server Only
    private void HandlePhaseChanged(GameManager.GamePhase newPhase)
    {
        if (!IsServer) return;
        
        switch (newPhase)
        {
            case GameManager.GamePhase.Day:
                StartDay();
                break;
            case GameManager.GamePhase.Night:
                StartNight();
                break;
            default:
                StopTimer();
                break;
        }
    }

    private void StartDay()
    {
        Debug.Log($"[DayNightManager] Day phase started. Duration: {_dayDuration}s");
        TimeRemaining.Value = _dayDuration;
        TimeInPhase.Value = 0f;
        _isTimerRunning = true;
        
        NotifyDayStartClientRpc();
    }

    private void StartNight()
    {
        Debug.Log($"[DayNightManager] Night phase started. Duration: {_nightDuration}s");
        TimeRemaining.Value = _nightDuration;
        TimeInPhase.Value = 0f;
        _isTimerRunning = true;
        
        NotifyNightStartClientRpc();
    }

    private void EndCurrentPhase()
    {
        _isTimerRunning = false;
        
        if (GameManager.Instance == null) return;
        
        if (GameManager.Instance.IsDayPhase)
        {
            // Day ended -> Start night
            GameManager.Instance.TransitionToPhase(GameManager.GamePhase.Night);
        }
        else if (GameManager.Instance.IsNightPhase)
        {
            // Night ended -> Advance wave and start new day
            GameManager.Instance.AdvanceWave();
            
            // If not victory, start new day
            if (GameManager.Instance.CurrentPhase.Value != GameManager.GamePhase.Victory)
            {
                GameManager.Instance.TransitionToPhase(GameManager.GamePhase.Day);
            }
        }
    }

    private void StopTimer()
    {
        _isTimerRunning = false;
    }
    #endregion

    #region Client RPCs
    [ClientRpc]
    private void NotifyDayStartClientRpc()
    {
        Debug.Log($"[DayNightManager] Day started (client notification)");
        OnDayStart?.Invoke();
    }

    [ClientRpc]
    private void NotifyNightStartClientRpc()
    {
        Debug.Log($"[DayNightManager] Night started (client notification)");
        OnNightStart?.Invoke();
    }
    #endregion

    #region Public Helpers
    public float GetNormalizedTime()
    {
        float totalDuration = GameManager.Instance?.IsDayPhase == true ? _dayDuration : _nightDuration;
        return totalDuration > 0 ? TimeInPhase.Value / totalDuration : 0f;
    }

    public string GetFormattedTimeRemaining()
    {
        int minutes = Mathf.FloorToInt(TimeRemaining.Value / 60f);
        int seconds = Mathf.FloorToInt(TimeRemaining.Value % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
    #endregion
}
