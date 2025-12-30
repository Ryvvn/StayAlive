using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Defensive tower that auto-targets and attacks enemies.
/// Must be placed within power range of base.
/// </summary>
public class TowerController : NetworkBehaviour
{
    #region Configuration
    [Header("Tower Info")]
    [SerializeField] private string _towerName = "Turret";
    [SerializeField] private TowerType _towerType = TowerType.DamageTurret;
    
    [Header("Targeting")]
    [SerializeField] private float _detectionRange = 15f;
    [SerializeField] private float _rotationSpeed = 90f;
    [SerializeField] private Transform _turretHead; // Part that rotates
    
    [Header("Combat")]
    [SerializeField] private float _damage = 15f;
    [SerializeField] private float _fireRate = 1f;
    [SerializeField] private Transform _firePoint;
    
    [Header("Visual")]
    [SerializeField] private LineRenderer _laserLine;
    [SerializeField] private ParticleSystem _muzzleFlash;
    #endregion

    #region State
    public NetworkVariable<bool> IsPowered = new(true);
    public NetworkVariable<int> UpgradeLevel = new(0);
    
    private Transform _currentTarget;
    private float _fireTimer;
    private bool _isRegistered;
    #endregion

    #region Events
    public event Action<EnemyAI> OnTargetAcquired;
    public event Action OnFired;
    #endregion

    #region Enums
    public enum TowerType
    {
        DamageTurret,   // Standard damage dealer
        SlowTower,      // Slows enemies
        SupportTower,   // Buffs nearby players/towers
        MortarTower     // Area damage
    }
    #endregion

    #region Unity Lifecycle
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        // Register with power system
        if (PowerSystem.Instance != null)
        {
            PowerSystem.Instance.RegisterTower(this);
            _isRegistered = true;
            
            // Check initial power status
            UpdatePowerStatus();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (_isRegistered && PowerSystem.Instance != null)
        {
            PowerSystem.Instance.UnregisterTower(this);
        }
        base.OnNetworkDespawn();
    }

    private void Update()
    {
        if (!IsServer) return;
        if (!IsPowered.Value) return;
        
        UpdatePowerStatus();
        FindTarget();
        RotateTowardsTarget();
        TryFire();
    }
    #endregion

    #region Power Check
    private void UpdatePowerStatus()
    {
        if (PowerSystem.Instance == null) return;
        
        bool inRange = PowerSystem.Instance.IsInPowerRange(transform.position);
        if (IsPowered.Value != inRange)
        {
            IsPowered.Value = inRange;
            Debug.Log($"[TowerController] {_towerName} power status: {(inRange ? "POWERED" : "UNPOWERED")}");
        }
    }
    #endregion

    #region Targeting - Server Only
    private void FindTarget()
    {
        // Find closest enemy in range
        float closestDist = float.MaxValue;
        Transform closestEnemy = null;
        
        foreach (var enemy in FindObjectsOfType<EnemyAI>())
        {
            if (!enemy.IsAlive) continue;
            
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist <= _detectionRange && dist < closestDist)
            {
                closestDist = dist;
                closestEnemy = enemy.transform;
            }
        }
        
        if (closestEnemy != _currentTarget)
        {
            _currentTarget = closestEnemy;
            if (_currentTarget != null)
            {
                OnTargetAcquired?.Invoke(_currentTarget.GetComponent<EnemyAI>());
            }
        }
    }

    private void RotateTowardsTarget()
    {
        if (_currentTarget == null || _turretHead == null) return;
        
        Vector3 direction = (_currentTarget.position - _turretHead.position).normalized;
        direction.y = 0; // Keep turret level
        
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            _turretHead.rotation = Quaternion.RotateTowards(
                _turretHead.rotation,
                targetRotation,
                _rotationSpeed * Time.deltaTime
            );
        }
    }
    #endregion

    #region Firing - Server Only
    private void TryFire()
    {
        if (_currentTarget == null) return;
        
        _fireTimer -= Time.deltaTime;
        if (_fireTimer > 0) return;
        
        // Check if facing target
        if (_turretHead != null)
        {
            Vector3 toTarget = (_currentTarget.position - _turretHead.position).normalized;
            toTarget.y = 0;
            float angle = Vector3.Angle(_turretHead.forward, toTarget);
            if (angle > 15f) return; // Not facing target
        }
        
        Fire();
        _fireTimer = 1f / _fireRate;
    }

    private void Fire()
    {
        if (_currentTarget == null) return;
        
        // Deal damage
        var enemyStats = _currentTarget.GetComponent<EnemyStats>();
        if (enemyStats != null)
        {
            float actualDamage = _damage * (1 + UpgradeLevel.Value * 0.2f); // 20% more per upgrade
            enemyStats.TakeDamage(actualDamage);
            Debug.Log($"[TowerController] {_towerName} hit for {actualDamage} damage");
        }
        
        // Notify clients for VFX
        Vector3 targetPos = _currentTarget.position;
        FireEffectClientRpc(targetPos);
        
        OnFired?.Invoke();
    }

    [ClientRpc]
    private void FireEffectClientRpc(Vector3 targetPos)
    {
        // Muzzle flash
        if (_muzzleFlash != null)
        {
            _muzzleFlash.Play();
        }
        
        // Laser line effect
        if (_laserLine != null && _firePoint != null)
        {
            _laserLine.enabled = true;
            _laserLine.SetPosition(0, _firePoint.position);
            _laserLine.SetPosition(1, targetPos);
            
            // Disable after short delay
            Invoke(nameof(HideLaser), 0.1f);
        }
    }

    private void HideLaser()
    {
        if (_laserLine != null)
        {
            _laserLine.enabled = false;
        }
    }
    #endregion

    #region Upgrades
    [ServerRpc(RequireOwnership = false)]
    public void UpgradeServerRpc()
    {
        // TODO: Check resources
        UpgradeLevel.Value++;
        Debug.Log($"[TowerController] {_towerName} upgraded to level {UpgradeLevel.Value}");
        
        // Apply upgrade effects
        _damage *= 1.2f;
        _detectionRange *= 1.1f;
    }
    #endregion

    #region Debug
    private void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, _detectionRange);
    }
    #endregion
}
