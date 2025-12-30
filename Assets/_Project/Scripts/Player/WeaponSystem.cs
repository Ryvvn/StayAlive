using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Manages player weapons - equipping, switching, and firing.
/// Attach to player prefab.
/// </summary>
public class WeaponSystem : NetworkBehaviour
{
    #region Configuration
    [Header("Weapon Slots")]
    [SerializeField] private Weapon[] _weaponSlots = new Weapon[3]; // 3 weapon slots
    [SerializeField] private Transform _weaponHolder;
    [SerializeField] private Transform _muzzlePoint;
    
    [Header("Camera")]
    [SerializeField] private Camera _playerCamera;
    
    [Header("Settings")]
    [SerializeField] private LayerMask _shootableLayers;
    [SerializeField] private float _maxShootDistance = 100f;
    #endregion

    #region State
    public NetworkVariable<int> CurrentWeaponIndex = new(0);
    private Weapon _currentWeapon;
    private float _lastFireTime;
    private bool _isFiring;
    #endregion

    #region Unity Lifecycle
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsOwner)
        {
            // Find camera if not assigned
            if (_playerCamera == null)
            {
                _playerCamera = GetComponentInChildren<Camera>();
            }
            
            // Equip first weapon
            EquipWeapon(0);
        }
        
        CurrentWeaponIndex.OnValueChanged += OnWeaponChanged;
    }

    public override void OnNetworkDespawn()
    {
        CurrentWeaponIndex.OnValueChanged -= OnWeaponChanged;
        base.OnNetworkDespawn();
    }

    private void Update()
    {
        if (!IsOwner) return;
        
        HandleInput();
        HandleFiring();
    }
    #endregion

    #region Input
    private void HandleInput()
    {
        // Fire input (left mouse)
        _isFiring = Input.GetMouseButton(0);
        
        // Weapon switching (1, 2, 3 keys)
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchWeaponServerRpc(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchWeaponServerRpc(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchWeaponServerRpc(2);
        
        // Scroll wheel switching
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0) SwitchToNextWeapon();
        if (scroll < 0) SwitchToPreviousWeapon();
        
        // Reload
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReloadServerRpc();
        }
    }

    private void HandleFiring()
    {
        if (_currentWeapon == null) return;
        if (!_isFiring) return;
        
        // Check fire rate
        if (Time.time < _lastFireTime + _currentWeapon.FireRate) return;
        
        // Check ammo
        if (_currentWeapon.CurrentAmmo <= 0)
        {
            // Click sound / auto reload
            return;
        }
        
        _lastFireTime = Time.time;
        Fire();
    }
    #endregion

    #region Firing
    private void Fire()
    {
        // Raycast from camera center
        Ray ray = _playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        NetworkObjectReference hitObjectRef = default;
        // Calculate hit point
        Vector3 targetPoint;
        if (Physics.Raycast(ray, out RaycastHit hit, _maxShootDistance, _shootableLayers))
        {
            Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red, 10f);
            targetPoint = hit.point;
            
            // Store hit object reference
            if (hit.collider.TryGetComponent<NetworkObject>(out var networkObject))
            {
                hitObjectRef = networkObject;
            }
            // Deal damage if we hit something damageable
            FireServerRpc(targetPoint, hitObjectRef);
        }
        else
        {
            targetPoint = ray.GetPoint(_maxShootDistance);
            FireServerRpc(targetPoint, hitObjectRef);
        }
        
        // Local effects (muzzle flash, sound)
        PlayFireEffectsLocal();
    }
    
    [ServerRpc]
    private void FireServerRpc(Vector3 hitPoint, NetworkObjectReference hitObjectRef)
    {
        if (_currentWeapon == null) return;
        if (_currentWeapon.CurrentAmmo <= 0) return;
    
        _currentWeapon.CurrentAmmo--;

        // Try to get the NetworkObject from the reference
        if (hitObjectRef.TryGet(out NetworkObject hitNetworkObject))
        {
            // Try to get damage component
            if (hitNetworkObject.TryGetComponent<EnemyStats>(out var enemy))
            {
                enemy.TakeDamage(_currentWeapon.Damage);
                Debug.Log($"[WeaponSystem] Hit {enemy.name} for {_currentWeapon.Damage} damage");
            }
        }

        // Notify all clients of fire effect
        FireEffectClientRpc(hitPoint);
    }

    [ClientRpc]
    private void FireEffectClientRpc(Vector3 hitPoint)
    {
        // Skip on owner (already played locally)
        if (IsOwner) return;
        
        PlayFireEffectsLocal();
    }

    private void PlayFireEffectsLocal()
    {
        // TODO: Muzzle flash VFX
        // TODO: Fire sound
        // TODO: Camera recoil
        Debug.Log($"[WeaponSystem] Fire! Ammo: {_currentWeapon?.CurrentAmmo ?? 0}");
    }
    #endregion

    #region Weapon Switching
    private void SwitchToNextWeapon()
    {
        int nextIndex = (CurrentWeaponIndex.Value + 1) % _weaponSlots.Length;
        SwitchWeaponServerRpc(nextIndex);
    }

    private void SwitchToPreviousWeapon()
    {
        int prevIndex = CurrentWeaponIndex.Value - 1;
        if (prevIndex < 0) prevIndex = _weaponSlots.Length - 1;
        SwitchWeaponServerRpc(prevIndex);
    }

    [ServerRpc]
    private void SwitchWeaponServerRpc(int index)
    {
        if (index < 0 || index >= _weaponSlots.Length) return;
        if (_weaponSlots[index] == null) return;
        
        CurrentWeaponIndex.Value = index;
    }

    private void OnWeaponChanged(int previousValue, int newValue)
    {
        EquipWeapon(newValue);
    }

    private void EquipWeapon(int index)
    {
        // Hide all weapons
        for (int i = 0; i < _weaponSlots.Length; i++)
        {
            if (_weaponSlots[i] != null)
            {
                _weaponSlots[i].gameObject.SetActive(i == index);
            }
        }
        
        // Set current weapon
        if (index >= 0 && index < _weaponSlots.Length)
        {
            _currentWeapon = _weaponSlots[index];
            Debug.Log($"[WeaponSystem] Equipped: {_currentWeapon?.WeaponName ?? "None"}");
        }
    }
    #endregion

    #region Reload
    [ServerRpc]
    private void ReloadServerRpc()
    {
        if (_currentWeapon == null) return;
        if (_currentWeapon.CurrentAmmo == _currentWeapon.MagazineSize) return;
        
        // TODO: Implement ammo reserve system
        _currentWeapon.CurrentAmmo = _currentWeapon.MagazineSize;
        
        ReloadEffectClientRpc();
    }

    [ClientRpc]
    private void ReloadEffectClientRpc()
    {
        Debug.Log($"[WeaponSystem] Reloaded: {_currentWeapon?.WeaponName}");
        // TODO: Reload animation/sound
    }
    #endregion

    #region Public API
    public Weapon GetCurrentWeapon() => _currentWeapon;
    public int GetCurrentAmmo() => _currentWeapon?.CurrentAmmo ?? 0;
    public int GetMagazineSize() => _currentWeapon?.MagazineSize ?? 0;
    #endregion
}
