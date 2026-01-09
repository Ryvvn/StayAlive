using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines a building piece (wall, floor, door, etc.)
/// </summary>
[CreateAssetMenu(fileName = "Building_New", menuName = "StayAlive/Building Piece")]
public class BuildingPiece : ScriptableObject
{
    [Header("Basic Info")]
    public string PieceName = "New Building";
    public string Description = "";
    public Sprite Icon;
    public BuildingCategory Category = BuildingCategory.Structure;
    
    [Header("Prefab")]
    public GameObject Prefab;
    public GameObject PreviewPrefab; // Optional separate preview prefab
    
    [Header("Placement")]
    public Vector3 GridSize = Vector3.one; // For grid snapping
    public bool CanRotate = true;
    public float RotationStep = 90f; // Degrees per rotation
    public bool SnapToGrid = true;
    public float GridSnapSize = 1f;
    
    [Header("Cost")]
    public List<BuildingCost> Costs = new();
    
    [Header("Properties")]
    public float MaxHealth = 100f;
    public bool IsFoundation = false; // Must be placed on ground
    public bool RequiresFoundation = false; // Must be placed on another building
    
    [Header("Power Settings")]
    public bool RequiresPower = false; // Must be placed in power range
    public bool IsPowerRelay = false; // This piece extends power range
    public bool IsTower = false; // This is a defense tower
    
    [System.Serializable]
    public struct BuildingCost
    {
        public ItemData Item;
        public int Quantity;
    }
    
    public enum BuildingCategory
    {
        Foundation,
        Structure,
        Defense,
        Utility,
        Decoration
    }
    
    /// <summary>
    /// Check if player can afford this building.
    /// </summary>
    public bool CanAfford(Inventory inventory, ItemDatabase database)
    {
        if (inventory == null || database == null) return false;
        
        foreach (var cost in Costs)
        {
            if (cost.Item == null) continue;
            
            int itemId = database.GetItemId(cost.Item);
            if (!inventory.HasItem(itemId, cost.Quantity))
            {
                return false;
            }
        }
        return true;
    }
    
    /// <summary>
    /// Consume resources from inventory.
    /// </summary>
    public void ConsumeCost(Inventory inventory, ItemDatabase database)
    {
        if (inventory == null || database == null) return;
        
        foreach (var cost in Costs)
        {
            if (cost.Item == null) continue;
            
            int itemId = database.GetItemId(cost.Item);
            inventory.RemoveItemServerRpc(itemId, cost.Quantity);
        }
    }
}
