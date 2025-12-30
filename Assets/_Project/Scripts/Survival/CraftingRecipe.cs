using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Crafting recipe definition.
/// Create via: Right-click → Create → StayAlive → Recipe
/// </summary>
[CreateAssetMenu(fileName = "Recipe_New", menuName = "StayAlive/Recipe")]
public class CraftingRecipe : ScriptableObject
{
    [Header("Recipe Info")]
    public string RecipeName;
    public string Description;
    public Sprite Icon;
    public RecipeCategory Category = RecipeCategory.Tools;
    
    [Header("Requirements")]
    public List<Ingredient> Ingredients = new();
    public int CraftingTime = 1; // Seconds
    public bool RequiresCraftingStation = false;
    
    [Header("Output")]
    public ItemData OutputItem;
    public int OutputQuantity = 1;
    
    [Serializable]
    public struct Ingredient
    {
        public ItemData Item;
        public int Quantity;
    }
    
    public enum RecipeCategory
    {
        Tools,
        Weapons,
        Ammo,
        Food,
        Building,
        Upgrades
    }
    
    /// <summary>
    /// Check if inventory has all required ingredients.
    /// </summary>
    public bool CanCraft(Inventory inventory, ItemDatabase database)
    {
        if (inventory == null || database == null) return false;
        
        foreach (var ingredient in Ingredients)
        {
            if (ingredient.Item == null) continue;
            
            int itemId = database.GetItemId(ingredient.Item);
            if (!inventory.HasItem(itemId, ingredient.Quantity))
            {
                return false;
            }
        }
        
        return true;
    }
}
