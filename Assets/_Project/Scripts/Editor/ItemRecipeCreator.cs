using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor tool to create basic items and recipes for testing.
/// </summary>
public class ItemRecipeCreator : Editor
{
    private static string ItemPath = "Assets/_Project/ScriptableObjects/Items/";
    private static string RecipePath = "Assets/_Project/ScriptableObjects/Recipes/";
    
    [MenuItem("Tools/StayAlive/Create Basic Items and Recipes")]
    public static void CreateBasicItemsAndRecipes()
    {
        // Ensure directories exist
        if (!Directory.Exists(ItemPath)) Directory.CreateDirectory(ItemPath);
        if (!Directory.Exists(RecipePath)) Directory.CreateDirectory(RecipePath);
        
        // ========== ITEMS ==========
        
        // Wood
        ItemData wood = CreateItem("Wood", "Raw wood from trees", ItemData.ItemType.Resource, true, 99);
        
        // Stone
        ItemData stone = CreateItem("Stone", "Raw stone from rocks", ItemData.ItemType.Resource, true, 99);
        
        // Wood Plank
        ItemData plank = CreateItem("Wood Plank", "Processed wood for building", ItemData.ItemType.CraftingMaterial, true, 50);
        
        // Stone Brick
        ItemData brick = CreateItem("Stone Brick", "Processed stone for building", ItemData.ItemType.CraftingMaterial, true, 50);
        
        // Wooden Stick
        ItemData stick = CreateItem("Wooden Stick", "A simple stick", ItemData.ItemType.CraftingMaterial, true, 20);
        
        // Stone Axe
        ItemData axe = CreateItem("Stone Axe", "Basic tool for chopping", ItemData.ItemType.Weapon, false, 1);
        
        // Cooked Meat
        ItemData cookedMeat = CreateItem("Cooked Meat", "Restores hunger", ItemData.ItemType.Consumable, true, 10);
        cookedMeat.IsConsumable = true;
        cookedMeat.HungerRestore = 30f;
        EditorUtility.SetDirty(cookedMeat);
        
        // ========== RECIPES ==========
        
        // Wood -> Planks (2 wood = 4 planks)
        CreateRecipe("Craft Planks", "Turn wood into planks", 
            new CraftingRecipe.Ingredient[] { new() { Item = wood, Quantity = 2 } },
            plank, 4, CraftingRecipe.RecipeCategory.Building);
        
        // Stone -> Bricks (3 stone = 2 bricks)
        CreateRecipe("Craft Bricks", "Turn stone into bricks",
            new CraftingRecipe.Ingredient[] { new() { Item = stone, Quantity = 3 } },
            brick, 2, CraftingRecipe.RecipeCategory.Building);
        
        // Wood -> Sticks (1 wood = 2 sticks)
        CreateRecipe("Craft Sticks", "Turn wood into sticks",
            new CraftingRecipe.Ingredient[] { new() { Item = wood, Quantity = 1 } },
            stick, 2, CraftingRecipe.RecipeCategory.Tools);
        
        // Stone Axe (2 sticks + 3 stone)
        CreateRecipe("Craft Stone Axe", "A basic chopping tool",
            new CraftingRecipe.Ingredient[] {
                new() { Item = stick, Quantity = 2 },
                new() { Item = stone, Quantity = 3 }
            },
            axe, 1, CraftingRecipe.RecipeCategory.Tools);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("[ItemRecipeCreator] Created basic items and recipes!");
    }
    
    private static ItemData CreateItem(string name, string desc, ItemData.ItemType type, bool stackable, int maxStack)
    {
        string path = ItemPath + "Item_" + name.Replace(" ", "") + ".asset";
        
        ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
        if (item == null)
        {
            item = ScriptableObject.CreateInstance<ItemData>();
            AssetDatabase.CreateAsset(item, path);
        }
        
        item.ItemName = name;
        item.Description = desc;
        item.Type = type;
        item.IsStackable = stackable;
        item.MaxStackSize = maxStack;
        
        EditorUtility.SetDirty(item);
        return item;
    }
    
    private static CraftingRecipe CreateRecipe(string name, string desc, CraftingRecipe.Ingredient[] ingredients, ItemData output, int outputQty, CraftingRecipe.RecipeCategory category)
    {
        string path = RecipePath + "Recipe_" + name.Replace(" ", "") + ".asset";
        
        CraftingRecipe recipe = AssetDatabase.LoadAssetAtPath<CraftingRecipe>(path);
        if (recipe == null)
        {
            recipe = ScriptableObject.CreateInstance<CraftingRecipe>();
            AssetDatabase.CreateAsset(recipe, path);
        }
        
        recipe.RecipeName = name;
        recipe.Description = desc;
        recipe.Ingredients = new System.Collections.Generic.List<CraftingRecipe.Ingredient>(ingredients);
        recipe.OutputItem = output;
        recipe.OutputQuantity = outputQty;
        recipe.Category = category;
        recipe.CraftingTime = 1;
        
        EditorUtility.SetDirty(recipe);
        return recipe;
    }
}
