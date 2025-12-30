using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Resource node that players can gather from.
/// Examples: Trees, rocks, bushes
/// </summary>
public class ResourceNode : NetworkBehaviour
{
    #region Configuration
    [Header("Resource Info")]
    [SerializeField] private string _resourceName = "Resource";
    [SerializeField] private ItemData _dropItem;
    [SerializeField] private int _dropMin = 1;
    [SerializeField] private int _dropMax = 3;
    
    [Header("Gathering")]
    [SerializeField] private float _gatherTime = 2f;
    [SerializeField] private int _maxGathers = 3;
    [SerializeField] private float _respawnTime = 60f;
    
    [Header("Visual")]
    [SerializeField] private GameObject _fullModel;
    [SerializeField] private GameObject _depletedModel;
    #endregion

    #region State
    public NetworkVariable<int> GathersRemaining = new();
    public NetworkVariable<bool> IsDepleted = new(false);
    
    private float _respawnTimer;
    #endregion

    #region Unity Lifecycle
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsServer)
        {
            GathersRemaining.Value = _maxGathers;
        }
        
        GathersRemaining.OnValueChanged += HandleGathersChanged;
        IsDepleted.OnValueChanged += HandleDepletedChanged;
        
        UpdateVisual();
    }

    public override void OnNetworkDespawn()
    {
        GathersRemaining.OnValueChanged -= HandleGathersChanged;
        IsDepleted.OnValueChanged -= HandleDepletedChanged;
        base.OnNetworkDespawn();
    }

    private void Update()
    {
        if (!IsServer || !IsDepleted.Value) return;
        
        // Respawn timer
        _respawnTimer += Time.deltaTime;
        if (_respawnTimer >= _respawnTime)
        {
            Respawn();
        }
    }
    #endregion

    #region Gathering
    [ServerRpc(RequireOwnership = false)]
    public void GatherServerRpc(ServerRpcParams rpcParams = default)
    {
        if (IsDepleted.Value)
        {
            Debug.Log($"[ResourceNode] {_resourceName} is depleted");
            return;
        }
        
        // Find player's inventory
        ulong clientId = rpcParams.Receive.SenderClientId;
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            return;
        }
        
        var inventory = client.PlayerObject?.GetComponent<Inventory>();
        if (inventory == null)
        {
            Debug.LogError("[ResourceNode] Player has no inventory");
            return;
        }
        
        // Give resources
        int dropAmount = Random.Range(_dropMin, _dropMax + 1);
        
        // TODO: Use ItemDatabase to get item ID
        // For now, just log
        Debug.Log($"[ResourceNode] Player gathered {dropAmount}x {_dropItem?.ItemName ?? _resourceName}");
        
        // Reduce gathers
        GathersRemaining.Value--;
        
        if (GathersRemaining.Value <= 0)
        {
            IsDepleted.Value = true;
            _respawnTimer = 0f;
        }
        
        // Notify client
        NotifyGatherClientRpc(clientId, dropAmount);
    }

    private void Respawn()
    {
        GathersRemaining.Value = _maxGathers;
        IsDepleted.Value = false;
        _respawnTimer = 0f;
        
        Debug.Log($"[ResourceNode] {_resourceName} respawned");
    }
    #endregion

    #region Visual
    private void UpdateVisual()
    {
        if (_fullModel != null)
        {
            _fullModel.SetActive(!IsDepleted.Value);
        }
        if (_depletedModel != null)
        {
            _depletedModel.SetActive(IsDepleted.Value);
        }
    }

    private void HandleGathersChanged(int previousValue, int newValue)
    {
        // Could add visual feedback for partial depletion
    }

    private void HandleDepletedChanged(bool previousValue, bool newValue)
    {
        UpdateVisual();
    }
    #endregion

    #region Client RPC
    [ClientRpc]
    private void NotifyGatherClientRpc(ulong clientId, int amount)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            // Show gather feedback for local player
            Debug.Log($"[ResourceNode] Gathered {amount}x {_dropItem?.ItemName ?? _resourceName}!");
            
            // Could trigger UI notification
            if (GameHUD.Instance != null)
            {
                GameHUD.Instance.ShowNotification($"+{amount} {_dropItem?.ItemName ?? _resourceName}");
            }
        }
    }
    #endregion

    #region Interaction
    public bool CanGather => !IsDepleted.Value;
    public float GetGatherProgress() => 0f; // TODO: Implement hold-to-gather
    #endregion
}
