using UnityEngine;

/// <summary>
/// Marks a spawn point location in the scene.
/// Players will spawn at one of these points when joining.
/// </summary>
public class SpawnPoint : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private bool _isOccupied = false;
    [SerializeField] private float _occupiedResetTime = 2f;
    
    /// <summary>
    /// Whether this spawn point is currently occupied by a spawning player.
    /// </summary>
    public bool IsOccupied => _isOccupied;
    
    /// <summary>
    /// Get the spawn position.
    /// </summary>
    public Vector3 Position => transform.position;
    
    /// <summary>
    /// Get the spawn rotation.
    /// </summary>
    public Quaternion Rotation => transform.rotation;
    
    /// <summary>
    /// Mark this spawn point as occupied temporarily.
    /// </summary>
    public void SetOccupied()
    {
        _isOccupied = true;
        Invoke(nameof(ResetOccupied), _occupiedResetTime);
    }
    
    private void ResetOccupied()
    {
        _isOccupied = false;
    }
    
    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Draw spawn point visualization
        Gizmos.color = _isOccupied ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        // Draw forward direction
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 1.5f);
    }
    #endif
}
