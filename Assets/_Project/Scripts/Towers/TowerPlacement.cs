using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Handles tower placement by players.
/// Shows preview, validates position, and spawns tower on confirm.
/// </summary>
public class TowerPlacement : NetworkBehaviour
{
    #region Configuration
    [Header("Placement Settings")]
    [SerializeField] private float _placementRange = 10f;
    [SerializeField] private LayerMask _placementLayers; // Ground layers
    [SerializeField] private LayerMask _obstacleLayers;  // Check for obstacles
    [SerializeField] private float _minDistanceBetweenTowers = 3f;
    
    [Header("Tower Prefabs")]
    [SerializeField] private GameObject _damageTowerPrefab;
    [SerializeField] private GameObject _supportTowerPrefab;
    
    [Header("Preview")]
    [SerializeField] private Material _validPlacementMaterial;
    [SerializeField] private Material _invalidPlacementMaterial;
    #endregion

    #region State
    private bool _isPlacing;
    private int _selectedTowerIndex;
    private GameObject _previewObject;
    private bool _isValidPlacement;
    private Vector3 _placementPosition;
    private Camera _playerCamera;
    #endregion

    #region Unity Lifecycle
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsOwner)
        {
            _playerCamera = GetComponentInChildren<Camera>();
        }
    }

    private void Update()
    {
        if (!IsOwner) return;
        
        HandleInput();
        
        if (_isPlacing)
        {
            UpdatePlacementPreview();
        }
    }
    #endregion

    #region Input
    private void HandleInput()
    {
        // Toggle placement mode (B key)
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (_isPlacing)
            {
                CancelPlacement();
            }
            else
            {
                StartPlacement(0); // Default to damage tower
            }
        }
        
        // Switch tower type while placing (1, 2 keys)
        if (_isPlacing)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) SelectTower(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SelectTower(1);
            
            // Confirm placement (left click)
            if (Input.GetMouseButtonDown(0) && _isValidPlacement)
            {
                ConfirmPlacement();
            }
            
            // Cancel (right click or Escape)
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                CancelPlacement();
            }
        }
    }
    #endregion

    #region Placement Mode
    public void StartPlacement(int towerIndex)
    {
        // Only during day phase
        if (GameManager.Instance != null && !GameManager.Instance.IsDayPhase)
        {
            Debug.Log("[TowerPlacement] Can only build during day!");
            return;
        }
        
        _isPlacing = true;
        _selectedTowerIndex = towerIndex;
        
        // Create preview object
        GameObject prefab = GetTowerPrefab(towerIndex);
        if (prefab != null)
        {
            _previewObject = Instantiate(prefab);
            
            // Disable all scripts and colliders on preview
            foreach (var comp in _previewObject.GetComponents<MonoBehaviour>())
            {
                comp.enabled = false;
            }
            foreach (var col in _previewObject.GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
            }
            
            // Remove network object if present
            var netObj = _previewObject.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                Destroy(netObj);
            }
        }
        
        Debug.Log("[TowerPlacement] Placement mode started");
    }

    public void SelectTower(int index)
    {
        if (!_isPlacing) return;
        
        _selectedTowerIndex = index;
        
        // Recreate preview
        if (_previewObject != null)
        {
            Destroy(_previewObject);
        }
        StartPlacement(index);
    }

    public void CancelPlacement()
    {
        _isPlacing = false;
        
        if (_previewObject != null)
        {
            Destroy(_previewObject);
            _previewObject = null;
        }
        
        Debug.Log("[TowerPlacement] Placement cancelled");
    }
    #endregion

    #region Preview Update
    private void UpdatePlacementPreview()
    {
        if (_previewObject == null || _playerCamera == null) return;
        
        // Raycast from camera
        Ray ray = _playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        
        if (Physics.Raycast(ray, out RaycastHit hit, _placementRange, _placementLayers))
        {
            _placementPosition = hit.point;
            _previewObject.transform.position = _placementPosition;
            
            // Check if valid placement
            _isValidPlacement = ValidatePlacement(_placementPosition);
            
            // Update preview material
            UpdatePreviewMaterial(_isValidPlacement);
        }
        else
        {
            _isValidPlacement = false;
            UpdatePreviewMaterial(false);
        }
    }

    private bool ValidatePlacement(Vector3 position)
    {
        // Check power range
        if (PowerSystem.Instance != null && !PowerSystem.Instance.CanPlaceTower(position))
        {
         
            return false;
        }
        
        // Check for obstacles
        if (Physics.CheckSphere(position, 1f, _obstacleLayers))
        {
      
            return false;
        }
        
        // Check distance from other towers
        foreach (var tower in FindObjectsOfType<TowerController>())
        {
            if(tower == null) continue;
            if (tower.GetComponent<NetworkObject>() == null)
        {
            continue;
        }
            if (Vector3.Distance(position, tower.transform.position) < _minDistanceBetweenTowers)
            {
            
                return false;
            }
        }
        
        return true;
    }

    private void UpdatePreviewMaterial(bool isValid)
    {
        if (_previewObject == null) return;
        
        Material mat = isValid ? _validPlacementMaterial : _invalidPlacementMaterial;
        if (mat == null) return;
        
        foreach (var renderer in _previewObject.GetComponentsInChildren<Renderer>())
        {
            var mats = new Material[renderer.materials.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = mat;
            }
            renderer.materials = mats;
        }
    }
    #endregion

    #region Confirm Placement
    private void ConfirmPlacement()
    {
        if (!_isValidPlacement) return;
        
        // Request server to spawn tower
        PlaceTowerServerRpc(_selectedTowerIndex, _placementPosition);
        
        // End placement mode
        CancelPlacement();
    }

    [ServerRpc]
    private void PlaceTowerServerRpc(int towerIndex, Vector3 position)
    {
        // Validate on server
        if (PowerSystem.Instance != null && !PowerSystem.Instance.CanPlaceTower(position))
        {
            Debug.Log("[TowerPlacement] Server rejected: out of power range");
            return;
        }
        
        // TODO: Check resources
        
        // Spawn tower
        Debug.Log($"[TowerPlacement] Placing tower {towerIndex} at {position}");
        GameObject prefab = GetTowerPrefab(towerIndex);
        if (prefab != null)
        {
            GameObject tower = Instantiate(prefab, position, Quaternion.identity);
            var netObj = tower.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Spawn();
            }
            
            Debug.Log($"[TowerPlacement] Tower spawned at {position}");
        }
        else
        {
            Debug.LogError($"[TowerPlacement] Tower prefab not found for index {towerIndex}");
        }
    }
    #endregion

    #region Helpers
    private GameObject GetTowerPrefab(int index)
    {
        return index switch
        {
            0 => _damageTowerPrefab,
            1 => _supportTowerPrefab,
            _ => _damageTowerPrefab
        };
    }
    #endregion
}
