using UnityEngine;

/// <summary>
/// Item definition using ScriptableObject.
/// Create items via: Right-click → Create → StayAlive → Item
/// </summary>
[CreateAssetMenu(fileName = "Item_New", menuName = "StayAlive/Item")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string ItemName = "New Item";
    public string Description = "";
    public Sprite Icon;
    public ItemType Type = ItemType.Resource;
    public ItemRarity Rarity = ItemRarity.Common;
    
    [Header("Stacking")]
    public bool IsStackable = true;
    public int MaxStackSize = 99;
    
    [Header("Usage")]
    public bool IsConsumable = false;
    public bool IsEquippable = false;
    
    [Header("Effects (if consumable)")]
    public float HealthRestore = 0f;
    public float HungerRestore = 0f;
    public float ThirstRestore = 0f;
    
    [Header("Value")]
    public int ChipValue = 1; // Currency value
    
    public enum ItemType
    {
        Resource,       // Wood, stone, metal
        Consumable,     // Food, drink, health items
        Weapon,
        Ammo,
        CraftingMaterial,
        TowerPart,
        Upgrade
    }
    
    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
    
    public Color GetRarityColor()
    {
        return Rarity switch
        {
            ItemRarity.Common => Color.white,
            ItemRarity.Uncommon => Color.green,
            ItemRarity.Rare => Color.cyan,
            ItemRarity.Epic => new Color(0.6f, 0.2f, 0.8f), // Purple
            ItemRarity.Legendary => new Color(1f, 0.6f, 0f), // Orange
            _ => Color.white
        };
    }
}
