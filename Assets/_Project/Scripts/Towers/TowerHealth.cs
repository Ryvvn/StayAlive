using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Central base/tower health. If this is destroyed, game over.
/// Attach to the main tower/base object.
/// </summary>
public class TowerHealth : NetworkBehaviour
{
    #region Configuration
    [Header("Health Settings")]
    [SerializeField] private float _maxHealth = 1000f;
    [SerializeField] private bool _isMainTower = true; // If true, destruction = game over
    
    [Header("Repair")]
    [SerializeField] private float _repairCostPerHP = 1f; // Resource cost per HP
    [SerializeField] private bool _canRepairDuringNight = false;
    #endregion

    #region State
    public NetworkVariable<float> CurrentHealth = new();
    public NetworkVariable<bool> IsDestroyed = new(false);
    #endregion

    #region Events
    public event Action<float, float> OnHealthChanged; // current, max
    public event Action OnDestroyed;
    public event Action<float> OnDamageTaken;
    public event Action<float> OnRepaired;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        // Tag for enemy targeting
        if (!gameObject.CompareTag("Tower"))
        {
            gameObject.tag = "Tower";
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsServer)
        {
            CurrentHealth.Value = _maxHealth;
        }
        
        CurrentHealth.OnValueChanged += HandleHealthChanged;
    }

    public override void OnNetworkDespawn()
    {
        CurrentHealth.OnValueChanged -= HandleHealthChanged;
        base.OnNetworkDespawn();
    }
    #endregion

    #region Damage - Server Only
    public void TakeDamage(float damage)
    {
        if (!IsServer || IsDestroyed.Value) return;
        
        CurrentHealth.Value = Mathf.Max(0, CurrentHealth.Value - damage);
        Debug.Log($"[TowerHealth] Took {damage} damage. Health: {CurrentHealth.Value}/{_maxHealth}");
        
        NotifyDamageClientRpc(damage);
        
        if (CurrentHealth.Value <= 0)
        {
            Destroy();
        }
    }

    [ClientRpc]
    private void NotifyDamageClientRpc(float damage)
    {
        OnDamageTaken?.Invoke(damage);
    }
    #endregion

    #region Repair - Server Only
    [ServerRpc(RequireOwnership = false)]
    public void RepairServerRpc(float amount, ServerRpcParams rpcParams = default)
    {
        if (IsDestroyed.Value) return;
        
        // Check if can repair during current phase
        if (!_canRepairDuringNight && GameManager.Instance?.IsNightPhase == true)
        {
            Debug.Log("[TowerHealth] Cannot repair during night!");
            return;
        }
        
        // TODO: Check if player has resources
        
        float actualRepair = Mathf.Min(amount, _maxHealth - CurrentHealth.Value);
        CurrentHealth.Value += actualRepair;
        
        Debug.Log($"[TowerHealth] Repaired {actualRepair}. Health: {CurrentHealth.Value}/{_maxHealth}");
        NotifyRepairClientRpc(actualRepair);
    }

    [ClientRpc]
    private void NotifyRepairClientRpc(float amount)
    {
        OnRepaired?.Invoke(amount);
    }
    #endregion

    #region Destruction - Server Only
    private void Destroy()
    {
        IsDestroyed.Value = true;
        Debug.Log("[TowerHealth] Tower destroyed!");
        
        NotifyDestroyedClientRpc();
        
        // If main tower, trigger game over
        if (_isMainTower && GameManager.Instance != null)
        {
            GameManager.Instance.TriggerDefeat();
        }
    }

    [ClientRpc]
    private void NotifyDestroyedClientRpc()
    {
        OnDestroyed?.Invoke();
        // TODO: Play destruction VFX/sound
    }
    #endregion

    #region Event Handlers
    private void HandleHealthChanged(float previousValue, float newValue)
    {
        OnHealthChanged?.Invoke(newValue, _maxHealth);
    }
    #endregion

    #region Public API
    public float HealthPercentage => _maxHealth > 0 ? CurrentHealth.Value / _maxHealth : 0f;
    public float MaxHealth => _maxHealth;
    public bool CanRepair => !IsDestroyed.Value && CurrentHealth.Value < _maxHealth;
    #endregion
}
