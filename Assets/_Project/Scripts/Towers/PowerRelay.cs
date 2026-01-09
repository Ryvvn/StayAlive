using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Power Relay tower that extends the power network.
/// Must be placed within existing power range, then provides additional coverage.
/// </summary>
public class PowerRelay : NetworkBehaviour
{
    [Header("Power Settings")]
    [SerializeField] private float _powerRadius = 12f;
    
    [Header("Visuals")]
    [SerializeField] private LineRenderer _radiusIndicator;
    [SerializeField] private Color _poweredColor = Color.cyan;
    [SerializeField] private Color _unpoweredColor = Color.red;
    
    public NetworkVariable<bool> IsPowered = new(false);
    
    private bool _isRegistered;

    public float PowerRadius => _powerRadius;
    public Vector3 Position => transform.position;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        // Register with power system
        if (PowerSystem.Instance != null)
        {
            PowerSystem.Instance.RegisterRelay(this);
            _isRegistered = true;
        }
        
        UpdateRadiusVisual();
        IsPowered.OnValueChanged += (prev, curr) => UpdateRadiusVisual();
    }

    public override void OnNetworkDespawn()
    {
        if (_isRegistered && PowerSystem.Instance != null)
        {
            PowerSystem.Instance.UnregisterRelay(this);
        }
        base.OnNetworkDespawn();
    }

    private void Update()
    {
        if (!IsServer) return;
        
        // Check if this relay is still receiving power
        bool hasPower = PowerSystem.Instance?.IsPositionPowered(transform.position, excludeRelay: this) ?? false;
        
        if (IsPowered.Value != hasPower)
        {
            IsPowered.Value = hasPower;
        }
    }

    private void UpdateRadiusVisual()
    {
        if (_radiusIndicator == null) return;
        
        // Draw circle
        int segments = 32;
        _radiusIndicator.positionCount = segments + 1;
        _radiusIndicator.startColor = IsPowered.Value ? _poweredColor : _unpoweredColor;
        _radiusIndicator.endColor = IsPowered.Value ? _poweredColor : _unpoweredColor;
        
        float angle = 0f;
        for (int i = 0; i <= segments; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * _powerRadius;
            float z = Mathf.Cos(Mathf.Deg2Rad * angle) * _powerRadius;
            
            _radiusIndicator.SetPosition(i, new Vector3(x, 0.1f, z));
            angle += 360f / segments;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 1, 0.3f);
        Gizmos.DrawWireSphere(transform.position, _powerRadius);
    }
}
