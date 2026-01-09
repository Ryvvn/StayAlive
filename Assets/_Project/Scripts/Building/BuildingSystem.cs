using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Handles building placement by players.
/// Similar to TowerPlacement but for general structures with grid snapping and rotation.
/// </summary>
public class BuildingSystem : NetworkBehaviour
{
    #region Singleton
    public static BuildingSystem Instance { get; private set; }
    
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
    [Header("Building Pieces")]
    [SerializeField] private List<BuildingPiece> _availableBuildings = new();
    
    [Header("Placement Settings")]
    [SerializeField] private float _placementRange = 15f;
    [SerializeField] private LayerMask _groundLayers;
    [SerializeField] private LayerMask _obstacleLayers;
    [SerializeField] private float _gridSize = 1f;
    
    [Header("Preview Materials")]
    [SerializeField] private Material _validPlacementMaterial;
    [SerializeField] private Material _invalidPlacementMaterial;
    
    [Header("References")]
    [SerializeField] private ItemDatabase _itemDatabase;
    #endregion

    #region State
    private bool _isPlacing;
    private int _selectedBuildingIndex;
    private GameObject _previewObject;
    private bool _isValidPlacement;
    private Vector3 _placementPosition;
    private Quaternion _placementRotation = Quaternion.identity;
    private Camera _playerCamera;
    private Inventory _playerInventory;
    #endregion

    #region Events
    public event System.Action<bool> OnBuildModeChanged;
    public event System.Action<BuildingPiece> OnBuildingSelected;
    public event System.Action<BuildingPiece, Vector3> OnBuildingPlaced;
    #endregion

    #region Unity Lifecycle
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsOwner)
        {
            StartCoroutine(FindLocalPlayerReferences());
        }
    }

    private System.Collections.IEnumerator FindLocalPlayerReferences()
    {
        float timeout = 10f;
        float elapsed = 0f;
        
        while ((_playerCamera == null || _playerInventory == null) && elapsed < timeout)
        {
            // Find local player
            var players = FindObjectsOfType<PlayerController>();
            foreach (var player in players)
            {
                if (player.IsOwner)
                {
                    _playerCamera = player.GetComponentInChildren<Camera>();
                    _playerInventory = player.GetComponent<Inventory>();
                    break;
                }
            }
            
            yield return new WaitForSeconds(0.5f);
            elapsed += 0.5f;
        }
        
        if (_playerCamera != null)
        {
            Debug.Log("[BuildingSystem] Found local player references");
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
        // Toggle build mode (B key)
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (_isPlacing)
            {
                CancelPlacement();
            }
            else
            {
                StartPlacement(0);
            }
        }
        
        if (_isPlacing)
        {
            // Number keys to select building
            for (int i = 0; i < Mathf.Min(9, _availableBuildings.Count); i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    SelectBuilding(i);
                }
            }
            
            // Rotate (R key)
            if (Input.GetKeyDown(KeyCode.R))
            {
                RotatePreview();
            }
            
            // Confirm (Left Click)
            if (Input.GetMouseButtonDown(0) && _isValidPlacement)
            {
                ConfirmPlacement();
            }
            
            // Cancel (Right Click or Escape)
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                CancelPlacement();
            }
        }
    }
    #endregion

    #region Placement Mode
    public void StartPlacement(int buildingIndex)
    {
        if (buildingIndex < 0 || buildingIndex >= _availableBuildings.Count) return;
        
        _isPlacing = true;
        _selectedBuildingIndex = buildingIndex;
        _placementRotation = Quaternion.identity;
        
        CreatePreview();
        
        OnBuildModeChanged?.Invoke(true);
        OnBuildingSelected?.Invoke(_availableBuildings[buildingIndex]);
        
        // Show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Debug.Log($"[BuildingSystem] Build mode started: {_availableBuildings[buildingIndex].PieceName}");
    }

    public void SelectBuilding(int index)
    {
        if (!_isPlacing || index < 0 || index >= _availableBuildings.Count) return;
        
        _selectedBuildingIndex = index;
        _placementRotation = Quaternion.identity;
        
        if (_previewObject != null) Destroy(_previewObject);
        CreatePreview();
        
        OnBuildingSelected?.Invoke(_availableBuildings[index]);
    }

    public void CancelPlacement()
    {
        _isPlacing = false;
        
        if (_previewObject != null)
        {
            Destroy(_previewObject);
            _previewObject = null;
        }
        
        OnBuildModeChanged?.Invoke(false);
        
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        Debug.Log("[BuildingSystem] Build mode cancelled");
    }

    private void CreatePreview()
    {
        var piece = _availableBuildings[_selectedBuildingIndex];
        GameObject prefab = piece.PreviewPrefab != null ? piece.PreviewPrefab : piece.Prefab;
        
        if (prefab == null) return;
        
        _previewObject = Instantiate(prefab);
        
        // Disable scripts and colliders
        foreach (var comp in _previewObject.GetComponents<MonoBehaviour>())
        {
            if (comp != _previewObject.GetComponent<Transform>())
                comp.enabled = false;
        }
        foreach (var col in _previewObject.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }
        
        // Remove NetworkObject
        var netObj = _previewObject.GetComponent<NetworkObject>();
        if (netObj != null) Destroy(netObj);
    }
    #endregion

    #region Preview Update
    private void UpdatePlacementPreview()
    {
        if (_previewObject == null || _playerCamera == null) return;
        
        Ray ray = _playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        
        if (Physics.Raycast(ray, out RaycastHit hit, _placementRange, _groundLayers))
        {
            var piece = _availableBuildings[_selectedBuildingIndex];
            
            // Apply grid snapping
            _placementPosition = hit.point;
            if (piece.SnapToGrid)
            {
                float snapSize = piece.GridSnapSize > 0 ? piece.GridSnapSize : _gridSize;
                _placementPosition = SnapToGrid(_placementPosition, snapSize);
            }
            
            _previewObject.transform.position = _placementPosition;
            _previewObject.transform.rotation = _placementRotation;
            
            // Validate
            _isValidPlacement = ValidatePlacement(piece, _placementPosition);
            
            UpdatePreviewMaterial(_isValidPlacement);
        }
        else
        {
            _isValidPlacement = false;
            UpdatePreviewMaterial(false);
        }
    }

    private Vector3 SnapToGrid(Vector3 position, float gridSize)
    {
        return new Vector3(
            Mathf.Round(position.x / gridSize) * gridSize,
            position.y,
            Mathf.Round(position.z / gridSize) * gridSize
        );
    }

    private void RotatePreview()
    {
        var piece = _availableBuildings[_selectedBuildingIndex];
        if (!piece.CanRotate) return;
        
        _placementRotation *= Quaternion.Euler(0, piece.RotationStep, 0);
        
        if (_previewObject != null)
        {
            _previewObject.transform.rotation = _placementRotation;
        }
    }

    private bool ValidatePlacement(BuildingPiece piece, Vector3 position)
    {
        // Check resources
        if (!piece.CanAfford(_playerInventory, _itemDatabase))
        {
            return false;
        }
        
        // Check for obstacles
        if (Physics.CheckSphere(position + Vector3.up * 0.5f, 0.5f, _obstacleLayers))
        {
            return false;
        }
        
        // Check power requirements (towers, relays, power-dependent structures)
        if (piece.RequiresPower || piece.IsTower || piece.IsPowerRelay)
        {
            if (PowerSystem.Instance != null && !PowerSystem.Instance.IsInPowerRange(position))
            {
                return false;
            }
        }
        
        // TODO: Check foundation requirements
        
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
        
        PlaceBuildingServerRpc(_selectedBuildingIndex, _placementPosition, _placementRotation);
    }

    [ServerRpc]
    private void PlaceBuildingServerRpc(int buildingIndex, Vector3 position, Quaternion rotation)
    {
        if (buildingIndex < 0 || buildingIndex >= _availableBuildings.Count) return;
        
        var piece = _availableBuildings[buildingIndex];
        
        // Validate resources on server
        if (!piece.CanAfford(_playerInventory, _itemDatabase))
        {
            Debug.Log("[BuildingSystem] Server rejected: not enough resources");
            return;
        }
        
        // Consume resources
        piece.ConsumeCost(_playerInventory, _itemDatabase);
        
        // Spawn building
        if (piece.Prefab != null)
        {
            GameObject building = Instantiate(piece.Prefab, position, rotation);
            var netObj = building.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Spawn();
            }
            
            Debug.Log($"[BuildingSystem] Placed {piece.PieceName} at {position}");
            
            // Notify clients
            NotifyBuildingPlacedClientRpc(buildingIndex, position);
        }
    }

    [ClientRpc]
    private void NotifyBuildingPlacedClientRpc(int buildingIndex, Vector3 position)
    {
        if (buildingIndex >= 0 && buildingIndex < _availableBuildings.Count)
        {
            OnBuildingPlaced?.Invoke(_availableBuildings[buildingIndex], position);
        }
    }
    #endregion

    #region Public API
    public IReadOnlyList<BuildingPiece> GetAvailableBuildings() => _availableBuildings;
    public bool IsInBuildMode => _isPlacing;
    public BuildingPiece GetSelectedBuilding() => _isPlacing ? _availableBuildings[_selectedBuildingIndex] : null;
    #endregion
}
