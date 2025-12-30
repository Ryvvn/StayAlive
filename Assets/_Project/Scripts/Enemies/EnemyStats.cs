using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Enemy health and stats. Networked for sync.
/// </summary>
public class EnemyStats : NetworkBehaviour
{
    #region Configuration
    [Header("Stats")]
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private float _defense = 0f;
    
    [Header("Rewards")]
    [SerializeField] private int _experienceReward = 10;
    [SerializeField] private int _chipReward = 5;
    #endregion

    #region State
    public NetworkVariable<float> CurrentHealth = new();
    public NetworkVariable<bool> IsDead = new(false);
    #endregion

    #region Events
    public event Action OnDeath;
    public event Action<float, float> OnHealthChanged; // current, max
    public event Action<float> OnDamageTaken;
    #endregion

    #region Unity Lifecycle
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
        if (!IsServer || IsDead.Value) return;
        
        // Apply defense
        float actualDamage = Mathf.Max(0, damage - _defense);
        
        CurrentHealth.Value -= actualDamage;
        Debug.Log($"[EnemyStats] Took {actualDamage} damage. Health: {CurrentHealth.Value}/{_maxHealth}");
        
        // Notify clients of damage
        NotifyDamageClientRpc(actualDamage);
        
        // Check death
        if (CurrentHealth.Value <= 0)
        {
            Die();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage)
    {
        TakeDamage(damage);
    }
    #endregion

    #region Death - Server Only
    private void Die()
    {
        if (IsDead.Value) return;
        
        IsDead.Value = true;
        Debug.Log($"[EnemyStats] Enemy died!");
        
        // Drop rewards
        DropRewards();
        
        // Notify
        OnDeath?.Invoke();
        NotifyDeathClientRpc();
    }

    private void DropRewards()
    {
        // TODO: Spawn loot drops
        // For now, just log
        Debug.Log($"[EnemyStats] Would drop {_chipReward} chips, {_experienceReward} XP");
    }

    [ClientRpc]
    private void NotifyDeathClientRpc()
    {
        // Play death effects on clients
        OnDeath?.Invoke();
    }

    [ClientRpc]
    private void NotifyDamageClientRpc(float damage)
    {
        OnDamageTaken?.Invoke(damage);
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
    #endregion
}
