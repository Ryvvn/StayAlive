using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Component attached to placed buildings.
/// Handles health, damage, and destruction.
/// </summary>
public class BuildingHealth : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _maxHealth = 100f;
    
    public NetworkVariable<float> CurrentHealth = new(100f);
    
    public event System.Action<float, float> OnHealthChanged; // current, max
    public event System.Action OnDestroyed;

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

    private void HandleHealthChanged(float previous, float current)
    {
        OnHealthChanged?.Invoke(current, _maxHealth);
        
        if (current <= 0 && IsServer)
        {
            DestroyBuilding();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage)
    {
        if (!IsServer) return;
        
        CurrentHealth.Value = Mathf.Max(0, CurrentHealth.Value - damage);
        Debug.Log($"[BuildingHealth] Took {damage} damage. Health: {CurrentHealth.Value}/{_maxHealth}");
    }

    [ServerRpc(RequireOwnership = false)]
    public void RepairServerRpc(float amount)
    {
        if (!IsServer) return;
        
        CurrentHealth.Value = Mathf.Min(_maxHealth, CurrentHealth.Value + amount);
    }

    private void DestroyBuilding()
    {
        OnDestroyed?.Invoke();
        
        // TODO: Spawn destruction VFX
        // TODO: Drop some resources
        
        Debug.Log("[BuildingHealth] Building destroyed!");
        
        var netObj = GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsSpawned)
        {
            netObj.Despawn();
        }
    }

    public float GetHealthPercent() => CurrentHealth.Value / _maxHealth;
}
