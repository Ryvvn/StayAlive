using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Player health, hunger, and thirst stats.
/// Host-authoritative with client prediction for UI.
/// </summary>
public class PlayerStats : NetworkBehaviour
{
    #region Configuration
    [Header("Health")]
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private float _healthRegenRate = 0f;
    
    [Header("Survival Needs")]
    [SerializeField] private float _maxHunger = 100f;
    [SerializeField] private float _hungerDecayRate = 1f; // per minute
    [SerializeField] private float _maxThirst = 100f;
    [SerializeField] private float _thirstDecayRate = 1.5f; // per minute
    
    [Header("Survival Effects")]
    [SerializeField] private float _starvingDamageRate = 5f; // damage per second when starving
    [SerializeField] private float _dehydratedDamageRate = 7f;
    #endregion

    #region State
    public NetworkVariable<float> Health = new();
    public NetworkVariable<float> Hunger = new();
    public NetworkVariable<float> Thirst = new();
    public NetworkVariable<bool> IsDead = new(false);
    #endregion

    #region Events
    public event Action OnDeath;
    public event Action OnRespawn;
    public event Action<float, float> OnHealthChanged;
    public event Action<float, float> OnHungerChanged;
    public event Action<float, float> OnThirstChanged;
    public event Action<float> OnDamageTaken;
    #endregion

    #region Unity Lifecycle
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsServer)
        {
            Health.Value = _maxHealth;
            Hunger.Value = _maxHunger;
            Thirst.Value = _maxThirst;
        }
        
        // Subscribe to value changes
        Health.OnValueChanged += (p, c) => OnHealthChanged?.Invoke(c, _maxHealth);
        Hunger.OnValueChanged += (p, c) => OnHungerChanged?.Invoke(c, _maxHunger);
        Thirst.OnValueChanged += (p, c) => OnThirstChanged?.Invoke(c, _maxThirst);
    }

    private void Update()
    {
        if (!IsServer || IsDead.Value) return;
        
        // Only decay during gameplay
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;
        
        UpdateSurvivalNeeds();
    }
    #endregion

    #region Survival - Server Only
    private void UpdateSurvivalNeeds()
    {
        float deltaMinute = Time.deltaTime / 60f;
        
        // Decay hunger and thirst
        Hunger.Value = Mathf.Max(0, Hunger.Value - _hungerDecayRate * deltaMinute);
        Thirst.Value = Mathf.Max(0, Thirst.Value - _thirstDecayRate * deltaMinute);
        
        // Take damage if starving/dehydrated
        if (Hunger.Value <= 0)
        {
            TakeDamage(_starvingDamageRate * Time.deltaTime);
        }
        if (Thirst.Value <= 0)
        {
            TakeDamage(_dehydratedDamageRate * Time.deltaTime);
        }
        
        // Health regen if fed
        if (Hunger.Value > 50 && Thirst.Value > 50 && _healthRegenRate > 0)
        {
            Health.Value = Mathf.Min(_maxHealth, Health.Value + _healthRegenRate * Time.deltaTime);
        }
    }
    #endregion

    #region Damage - Server Only
    public void TakeDamage(float damage)
    {
        if (!IsServer || IsDead.Value) return;
        
        Health.Value -= damage;
        
        if (damage >= 1f)
        {
            Debug.Log($"[PlayerStats] Player {OwnerClientId} took {damage} damage. Health: {Health.Value}");
            NotifyDamageClientRpc(damage);
        }
        
        if (Health.Value <= 0)
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

    #region Healing/Feeding - Server Only
    [ServerRpc(RequireOwnership = false)]
    public void HealServerRpc(float amount)
    {
        if (!IsServer || IsDead.Value) return;
        Health.Value = Mathf.Min(_maxHealth, Health.Value + amount);
    }

    [ServerRpc(RequireOwnership = false)]
    public void FeedServerRpc(float amount)
    {
        if (!IsServer) return;
        Hunger.Value = Mathf.Min(_maxHunger, Hunger.Value + amount);
    }

    [ServerRpc(RequireOwnership = false)]
    public void DrinkServerRpc(float amount)
    {
        if (!IsServer) return;
        Thirst.Value = Mathf.Min(_maxThirst, Thirst.Value + amount);
    }
    #endregion

    #region Death/Respawn - Server Only
    private void Die()
    {
        IsDead.Value = true;
        Debug.Log($"[PlayerStats] Player {OwnerClientId} died!");
        
        // Drop chips for team to revive
        DropChips();
        
        NotifyDeathClientRpc();
    }

    private void DropChips()
    {
        // TODO: Spawn chip pickup at player location
        Debug.Log($"[PlayerStats] Player dropped revival chips");
    }

    [ServerRpc(RequireOwnership = false)]
    public void RespawnServerRpc()
    {
        if (!IsDead.Value) return;
        
        Health.Value = _maxHealth;
        Hunger.Value = _maxHunger;
        Thirst.Value = _maxThirst;
        IsDead.Value = false;
        
        Debug.Log($"[PlayerStats] Player {OwnerClientId} respawned!");
        NotifyRespawnClientRpc();
    }
    #endregion

    #region Client RPCs
    [ClientRpc]
    private void NotifyDeathClientRpc()
    {
        OnDeath?.Invoke();
    }

    [ClientRpc]
    private void NotifyRespawnClientRpc()
    {
        OnRespawn?.Invoke();
    }

    [ClientRpc]
    private void NotifyDamageClientRpc(float damage)
    {
        OnDamageTaken?.Invoke(damage);
    }
    #endregion

    #region Public API
    public float HealthPercentage => _maxHealth > 0 ? Health.Value / _maxHealth : 0f;
    public float HungerPercentage => _maxHunger > 0 ? Hunger.Value / _maxHunger : 0f;
    public float ThirstPercentage => _maxThirst > 0 ? Thirst.Value / _maxThirst : 0f;
    public float MaxHealth => _maxHealth;
    public float MaxHunger => _maxHunger;
    public float MaxThirst => _maxThirst;
    public bool IsStarving => Hunger.Value <= 0;
    public bool IsDehydrated => Thirst.Value <= 0;
    #endregion
}
