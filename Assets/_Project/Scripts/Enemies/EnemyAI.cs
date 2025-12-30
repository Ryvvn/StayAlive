using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Enemy AI controller. Host-authoritative.
/// Handles pathfinding, target selection, and attacks.
/// Target priority: Tower > Nearest Player
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : NetworkBehaviour
{
    #region Configuration
    [Header("AI Settings")]
    [SerializeField] private float _detectionRange = 20f;
    [SerializeField] private float _attackRange = 2f;
    [SerializeField] private float _attackCooldown = 1f;
    [SerializeField] private float _retargetInterval = 2f;
    
    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 4f;
    [SerializeField] private float _rotationSpeed = 120f;
    
    [Header("Combat")]
    [SerializeField] private float _attackDamage = 10f;
    [SerializeField] private bool _prefersTower = true;
    #endregion

    #region Components
    private NavMeshAgent _agent;
    private EnemyStats _stats;
    #endregion

    #region State
    public enum EnemyState { Idle, Moving, Attacking, Dead }
    public NetworkVariable<EnemyState> CurrentState = new(EnemyState.Idle);
    
    private Transform _currentTarget;
    private float _attackTimer;
    private float _retargetTimer;
    private bool _isDead;
    #endregion

    #region Events
    public event Action<EnemyAI> OnDeath;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _stats = GetComponent<EnemyStats>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsServer)
        {
            _agent.speed = _moveSpeed;
            _agent.angularSpeed = _rotationSpeed;
            CurrentState.Value = EnemyState.Idle;
            _retargetTimer = 0f;
            
            // Subscribe to death event
            if (_stats != null)
            {
                _stats.OnDeath += HandleDeath;
            }
        }
        else
        {
            // Disable NavMeshAgent on clients (server controls movement)
            _agent.enabled = false;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (_stats != null)
        {
            _stats.OnDeath -= HandleDeath;
        }
        base.OnNetworkDespawn();
    }

    private void Update()
    {
        if (!IsServer || _isDead) return;
        
        // Update retarget timer
        _retargetTimer -= Time.deltaTime;
        if (_retargetTimer <= 0f)
        {
            FindTarget();
            _retargetTimer = _retargetInterval;
        }
        
        // Update attack timer
        if (_attackTimer > 0)
        {
            _attackTimer -= Time.deltaTime;
        }
        
        // State machine
        UpdateStateMachine();
    }
    #endregion

    #region State Machine - Server Only
    private void UpdateStateMachine()
    {
        switch (CurrentState.Value)
        {
            case EnemyState.Idle:
                UpdateIdle();
                break;
            case EnemyState.Moving:
                UpdateMoving();
                break;
            case EnemyState.Attacking:
                UpdateAttacking();
                break;
        }
    }

    private void UpdateIdle()
    {
        if (_currentTarget != null)
        {
            CurrentState.Value = EnemyState.Moving;
        }
    }

    private void UpdateMoving()
    {
        if (_currentTarget == null)
        {
            CurrentState.Value = EnemyState.Idle;
            _agent.ResetPath();
            return;
        }
        
        float distance = Vector3.Distance(transform.position, _currentTarget.position);
        
        // Check if in attack range
        if (distance <= _attackRange)
        {
            _agent.ResetPath();
            CurrentState.Value = EnemyState.Attacking;
            return;
        }
        
        // Move towards target
        _agent.SetDestination(_currentTarget.position);
    }

    private void UpdateAttacking()
    {
        if (_currentTarget == null)
        {
            CurrentState.Value = EnemyState.Idle;
            return;
        }
        
        float distance = Vector3.Distance(transform.position, _currentTarget.position);
        
        // Check if target moved out of range
        if (distance > _attackRange * 1.2f)
        {
            CurrentState.Value = EnemyState.Moving;
            return;
        }
        
        // Face target
        Vector3 direction = (_currentTarget.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, 
                targetRotation, 
                _rotationSpeed * Time.deltaTime
            );
        }
        
        // Attack if ready
        if (_attackTimer <= 0f)
        {
            PerformAttack();
            _attackTimer = _attackCooldown;
        }
    }
    #endregion

    #region Targeting - Server Only
    private void FindTarget()
    {
        Transform bestTarget = null;
        float bestScore = float.MaxValue;
        
        // Priority 1: Tower (if prefers tower)
        if (_prefersTower)
        {
            // Find tower/base
            var tower = GameObject.FindGameObjectWithTag("Tower");
            if (tower != null)
            {
                float dist = Vector3.Distance(transform.position, tower.transform.position);
                if (dist <= _detectionRange)
                {
                    bestTarget = tower.transform;
                    bestScore = dist * 0.5f; // Prefer tower
                }
            }
        }
        
        // Priority 2: Nearest player
        foreach (var player in FindObjectsOfType<PlayerController>())
        {
            float dist = Vector3.Distance(transform.position, player.transform.position);
            if (dist <= _detectionRange && dist < bestScore)
            {
                bestTarget = player.transform;
                bestScore = dist;
            }
        }
        
        _currentTarget = bestTarget;
    }
    #endregion

    #region Combat - Server Only
    private void PerformAttack()
    {
        if (_currentTarget == null) return;
        
        Debug.Log($"[EnemyAI] Attacking {_currentTarget.name} for {_attackDamage} damage");
        
        // Try to damage player
        var playerStats = _currentTarget.GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.TakeDamageServerRpc(_attackDamage);
            return;
        }
        
        // Try to damage tower
        // var towerHealth = _currentTarget.GetComponent<TowerHealth>();
        // if (towerHealth != null)
        // {
        //     towerHealth.TakeDamage(_attackDamage);
        //     return;
        // }
        
        // Fallback: just notify attack happened
        NotifyAttackClientRpc();
    }

    [ClientRpc]
    private void NotifyAttackClientRpc()
    {
        // Play attack animation/sound on clients
        Debug.Log($"[EnemyAI] Attack animation triggered");
    }
    #endregion

    #region Death
    private void HandleDeath()
    {
        if (_isDead) return;
        _isDead = true;
        
        Debug.Log($"[EnemyAI] Enemy died");
        CurrentState.Value = EnemyState.Dead;
        _agent.enabled = false;
        
        // Notify WaveManager
        OnDeath?.Invoke(this);
        
        // Despawn after short delay (for death animation)
        Invoke(nameof(DespawnEnemy), 0.5f);
    }

    private void DespawnEnemy()
    {
        if (IsServer && NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
    }
    #endregion

    #region Public API
    public bool IsAlive => !_isDead && CurrentState.Value != EnemyState.Dead;
    #endregion
}
