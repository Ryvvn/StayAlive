using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Core game state manager. Handles game phases, win/loss conditions, and coordinates other managers.
/// Following StayAlive Architecture: GameManager Singleton + Specialized Managers pattern.
/// </summary>
public class GameManager : NetworkBehaviour
{
    #region Singleton
    public static GameManager Instance { get; private set; }
    
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

    #region Game State
    public enum GamePhase { Menu, Loading, Day, Night, Victory, Defeat }
    
    public NetworkVariable<GamePhase> CurrentPhase = new(GamePhase.Menu);
    public NetworkVariable<int> CurrentWave = new(0);
    public NetworkVariable<int> TotalWaves = new(20); // Default: 20 waves for standard mode
    
    public event Action<GamePhase> OnPhaseChanged;
    public event Action<int> OnWaveChanged;
    public event Action OnVictory;
    public event Action OnDefeat;
    #endregion

    #region References
    [Header("Manager References")]
    [SerializeField] private DayNightManager _dayNightManager;
    [SerializeField] private WaveManager _waveManager;
    //[SerializeField] private TowerManager _towerManager;
    #endregion

    #region Unity Lifecycle
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        CurrentPhase.OnValueChanged += HandlePhaseChanged;
        CurrentWave.OnValueChanged += HandleWaveChanged;
        
        if (IsServer)
        {
            Debug.Log($"[GameManager] Server spawned. Starting game...");
        }
    }

    public override void OnNetworkDespawn()
    {
        CurrentPhase.OnValueChanged -= HandlePhaseChanged;
        CurrentWave.OnValueChanged -= HandleWaveChanged;
        base.OnNetworkDespawn();
    }
    #endregion

    #region Game Flow - Server Only
    [ServerRpc(RequireOwnership = false)]
    public void StartGameServerRpc()
    {
        if (!IsServer) return;
        
        Debug.Log($"[GameManager] Starting new game...");
        CurrentWave.Value = 0;
        TransitionToPhase(GamePhase.Day);
    }

    public void TransitionToPhase(GamePhase newPhase)
    {
        if (!IsServer) return;
        
        Debug.Log($"[GameManager] Phase transition: {CurrentPhase.Value} -> {newPhase}");
        CurrentPhase.Value = newPhase;
    }

    public void AdvanceWave()
    {
        if (!IsServer) return;
        
        CurrentWave.Value++;
        Debug.Log($"[GameManager] Wave advanced to {CurrentWave.Value}");
        
        // Check for victory
        if (CurrentWave.Value > TotalWaves.Value)
        {
            TransitionToPhase(GamePhase.Victory);
        }
    }

    public void TriggerDefeat()
    {
        if (!IsServer) return;
        
        Debug.Log($"[GameManager] Defeat triggered!");
        TransitionToPhase(GamePhase.Defeat);
    }
    #endregion

    #region Event Handlers
    private void HandlePhaseChanged(GamePhase previousValue, GamePhase newValue)
    {
        Debug.Log($"[GameManager] Phase changed: {previousValue} -> {newValue}");
        OnPhaseChanged?.Invoke(newValue);
        
        switch (newValue)
        {
            case GamePhase.Victory:
                OnVictory?.Invoke();
                break;
            case GamePhase.Defeat:
                OnDefeat?.Invoke();
                break;
        }
    }

    private void HandleWaveChanged(int previousValue, int newValue)
    {
        Debug.Log($"[GameManager] Wave changed: {previousValue} -> {newValue}");
        OnWaveChanged?.Invoke(newValue);
    }
    #endregion

    #region Public Helpers
    public bool IsPlaying => CurrentPhase.Value == GamePhase.Day || CurrentPhase.Value == GamePhase.Night;
    public bool IsDayPhase => CurrentPhase.Value == GamePhase.Day;
    public bool IsNightPhase => CurrentPhase.Value == GamePhase.Night;
    #endregion
}
