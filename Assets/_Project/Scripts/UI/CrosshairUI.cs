using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Simple crosshair UI that changes color when targeting enemies.
/// </summary>
public class CrosshairUI : MonoBehaviour
{
    [Header("Crosshair Settings")]
    [SerializeField] private RectTransform _crosshair;
    [SerializeField] private float _normalSize = 20f;
    [SerializeField] private float _expandedSize = 30f;
    [SerializeField] private float _sizeChangeSpeed = 10f;
    
    [Header("Colors")]
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _enemyColor = Color.red;
    [SerializeField] private Color _friendlyColor = Color.green;
    
    [Header("Raycast")]
    [SerializeField] private float _raycastDistance = 100f;
    [SerializeField] private LayerMask _raycastLayers;
    
    private Camera _mainCamera;
    private UnityEngine.UI.Image _crosshairImage;
    private float _currentSize;
    private float _targetSize;
    
    private void Start()
    {
        _mainCamera = Camera.main;
        
        if (_crosshair != null)
        {
            _crosshairImage = _crosshair.GetComponent<UnityEngine.UI.Image>();
        }
        
        _currentSize = _normalSize;
        _targetSize = _normalSize;
    }
    
    private void Update()
    {
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
            return;
        }
        
        UpdateCrosshairTarget();
        UpdateCrosshairSize();
    }
    
    private void UpdateCrosshairTarget()
    {
        Ray ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        
        if (Physics.Raycast(ray, out RaycastHit hit, _raycastDistance, _raycastLayers))
        {
            // Check if we're looking at an enemy
            var enemy = hit.collider.GetComponent<EnemyStats>();
            if (enemy != null && !enemy.IsDead.Value)
            {
                SetCrosshairColor(_enemyColor);
                _targetSize = _expandedSize;
                return;
            }
            
            // Check for friendly
            var player = hit.collider.GetComponent<PlayerController>();
            if (player != null)
            {
                SetCrosshairColor(_friendlyColor);
                _targetSize = _normalSize;
                return;
            }
        }
        
        // Nothing targeted
        SetCrosshairColor(_normalColor);
        _targetSize = _normalSize;
    }
    
    private void UpdateCrosshairSize()
    {
        _currentSize = Mathf.Lerp(_currentSize, _targetSize, Time.deltaTime * _sizeChangeSpeed);
        
        if (_crosshair != null)
        {
            _crosshair.sizeDelta = new Vector2(_currentSize, _currentSize);
        }
    }
    
    private void SetCrosshairColor(Color color)
    {
        if (_crosshairImage != null)
        {
            _crosshairImage.color = color;
        }
    }
    
    /// <summary>
    /// Expand crosshair temporarily (for firing feedback)
    /// </summary>
    public void Pulse()
    {
        _currentSize = _expandedSize;
    }
}
