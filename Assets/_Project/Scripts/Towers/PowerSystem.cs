using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Manages power radius and tower connections.
/// Attach to the main base. Towers must be within power radius to function.
/// </summary>
public class PowerSystem : NetworkBehaviour
{
    #region Singleton
    public static PowerSystem Instance { get; private set; }
    
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
    [Header("Power Settings")]
    [SerializeField] private float _basePowerRadius = 15f;
    [SerializeField] private float _maxPowerRadius = 50f;
    [SerializeField] private float _radiusUpgradeAmount = 5f;
    
    [Header("Visuals")]
    [SerializeField] private LineRenderer _radiusIndicator;
    [SerializeField] private Material _powerLineMaterial;
    [SerializeField] private Color _poweredColor = Color.cyan;
    [SerializeField] private Color _unpoweredColor = Color.red;
    #endregion

    #region State
    public NetworkVariable<float> CurrentPowerRadius = new();
    public NetworkVariable<int> UpgradeLevel = new(0);
    
    private List<TowerController> _connectedTowers = new();
    private List<LineRenderer> _powerLines = new();
    #endregion

    #region Events
    public event Action<float> OnPowerRadiusChanged;
    public event Action<TowerController> OnTowerConnected;
    public event Action<TowerController> OnTowerDisconnected;
    #endregion

    #region Unity Lifecycle
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsServer)
        {
            CurrentPowerRadius.Value = _basePowerRadius;
        }
        
        CurrentPowerRadius.OnValueChanged += HandleRadiusChanged;
        
        UpdateRadiusVisual();
    }

    public override void OnNetworkDespawn()
    {
        CurrentPowerRadius.OnValueChanged -= HandleRadiusChanged;
        base.OnNetworkDespawn();
    }

    private void Update()
    {
        // Update power line visuals
        UpdatePowerLines();
    }
    #endregion

    #region Power Radius
    /// <summary>
    /// Check if a position is within power range.
    /// </summary>
    public bool IsInPowerRange(Vector3 position)
    {
        float distance = Vector3.Distance(transform.position, position);
        return distance <= CurrentPowerRadius.Value;
    }

    /// <summary>
    /// Check if a tower placement is valid (within power range).
    /// </summary>
    public bool CanPlaceTower(Vector3 position)
    {
        return IsInPowerRange(position);
    }

    /// <summary>
    /// Get distance from base.
    /// </summary>
    public float GetDistanceFromBase(Vector3 position)
    {
        return Vector3.Distance(transform.position, position);
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpgradePowerRadiusServerRpc()
    {
        if (CurrentPowerRadius.Value >= _maxPowerRadius) return;
        
        // TODO: Check resources
        
        CurrentPowerRadius.Value = Mathf.Min(
            CurrentPowerRadius.Value + _radiusUpgradeAmount,
            _maxPowerRadius
        );
        
        UpgradeLevel.Value++;
        Debug.Log($"[PowerSystem] Power radius upgraded to {CurrentPowerRadius.Value}");
    }
    #endregion

    #region Tower Registration
    public void RegisterTower(TowerController tower)
    {
        if (!_connectedTowers.Contains(tower))
        {
            _connectedTowers.Add(tower);
            OnTowerConnected?.Invoke(tower);
            
            // Create power line
            CreatePowerLine(tower);
            
            Debug.Log($"[PowerSystem] Tower registered. Total: {_connectedTowers.Count}");
        }
    }

    public void UnregisterTower(TowerController tower)
    {
        if (_connectedTowers.Remove(tower))
        {
            OnTowerDisconnected?.Invoke(tower);
            Debug.Log($"[PowerSystem] Tower unregistered. Total: {_connectedTowers.Count}");
        }
    }
    #endregion

    #region Visual Updates
    private void UpdateRadiusVisual()
    {
        if (_radiusIndicator == null) return;
        
        // Draw circle for power radius
        int segments = 64;
        _radiusIndicator.positionCount = segments + 1;
        
        float angle = 0f;
        for (int i = 0; i <= segments; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * CurrentPowerRadius.Value;
            float z = Mathf.Cos(Mathf.Deg2Rad * angle) * CurrentPowerRadius.Value;
            
            _radiusIndicator.SetPosition(i, new Vector3(x, 0.1f, z));
            angle += 360f / segments;
        }
    }

    private void CreatePowerLine(TowerController tower)
    {
        var lineObj = new GameObject($"PowerLine_{tower.name}");
        lineObj.transform.SetParent(transform);
        
        var line = lineObj.AddComponent<LineRenderer>();
        line.material = _powerLineMaterial ?? new Material(Shader.Find("Sprites/Default"));
        line.startWidth = 0.1f;
        line.endWidth = 0.1f;
        line.positionCount = 2;
        line.startColor = _poweredColor;
        line.endColor = _poweredColor;
        
        _powerLines.Add(line);
    }

    private void UpdatePowerLines()
    {
        // Clean up destroyed towers
        for (int i = _connectedTowers.Count - 1; i >= 0; i--)
        {
            if (_connectedTowers[i] == null)
            {
                _connectedTowers.RemoveAt(i);
                if (i < _powerLines.Count && _powerLines[i] != null)
                {
                    Destroy(_powerLines[i].gameObject);
                    _powerLines.RemoveAt(i);
                }
            }
        }
        
        // Update line positions
        for (int i = 0; i < _connectedTowers.Count && i < _powerLines.Count; i++)
        {
            if (_connectedTowers[i] != null && _powerLines[i] != null)
            {
                Vector3 basePos = transform.position + Vector3.up * 0.5f;
                Vector3 towerPos = _connectedTowers[i].transform.position + Vector3.up * 0.5f;
                
                _powerLines[i].SetPosition(0, basePos);
                _powerLines[i].SetPosition(1, towerPos);
                
                // Color based on power status
                bool inRange = IsInPowerRange(_connectedTowers[i].transform.position);
                Color lineColor = inRange ? _poweredColor : _unpoweredColor;
                _powerLines[i].startColor = lineColor;
                _powerLines[i].endColor = lineColor;
            }
        }
    }

    private void HandleRadiusChanged(float previousValue, float newValue)
    {
        UpdateRadiusVisual();
        OnPowerRadiusChanged?.Invoke(newValue);
    }
    #endregion

    #region Debug
    private void OnDrawGizmosSelected()
    {
        // Draw power radius in editor
        Gizmos.color = new Color(0, 1, 1, 0.3f);
        Gizmos.DrawWireSphere(transform.position, _basePowerRadius);
    }
    #endregion
}
