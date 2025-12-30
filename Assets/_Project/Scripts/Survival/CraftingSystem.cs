using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Handles crafting items from recipes.
/// Host-authoritative validation.
/// </summary>
public class CraftingSystem : NetworkBehaviour
{
    #region Configuration
    [Header("References")]
    [SerializeField] private ItemDatabase _itemDatabase;
    [SerializeField] private List<CraftingRecipe> _availableRecipes = new();
    #endregion

    #region State
    private Inventory _inventory;
    public NetworkVariable<bool> IsCrafting = new(false);
    public NetworkVariable<float> CraftingProgress = new(0f);
    
    private CraftingRecipe _currentRecipe;
    private float _craftingTimer;
    #endregion

    #region Events
    public event Action<CraftingRecipe> OnCraftingStarted;
    public event Action<CraftingRecipe> OnCraftingComplete;
    public event Action<CraftingRecipe, string> OnCraftingFailed;
    #endregion

    #region Unity Lifecycle
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _inventory = GetComponent<Inventory>();
    }

    private void Update()
    {
        if (!IsServer || !IsCrafting.Value) return;
        
        _craftingTimer += Time.deltaTime;
        CraftingProgress.Value = _currentRecipe != null 
            ? _craftingTimer / _currentRecipe.CraftingTime 
            : 0f;
        
        if (_craftingTimer >= _currentRecipe?.CraftingTime)
        {
            CompleteCrafting();
        }
    }
    #endregion

    #region Crafting
    [ServerRpc(RequireOwnership = false)]
    public void CraftItemServerRpc(int recipeIndex)
    {
        if (IsCrafting.Value)
        {
            Debug.Log("[CraftingSystem] Already crafting!");
            return;
        }
        
        if (recipeIndex < 0 || recipeIndex >= _availableRecipes.Count)
        {
            Debug.Log("[CraftingSystem] Invalid recipe index");
            return;
        }
        
        CraftingRecipe recipe = _availableRecipes[recipeIndex];
        
        // Validate can craft
        if (!recipe.CanCraft(_inventory, _itemDatabase))
        {
            Debug.Log($"[CraftingSystem] Missing ingredients for {recipe.RecipeName}");
            NotifyCraftingFailedClientRpc(recipeIndex, "Missing ingredients");
            return;
        }
        
        // Consume ingredients
        foreach (var ingredient in recipe.Ingredients)
        {
            if (ingredient.Item == null) continue;
            
            int itemId = _itemDatabase.GetItemId(ingredient.Item);
            _inventory.RemoveItemServerRpc(itemId, ingredient.Quantity);
        }
        
        // Start crafting
        _currentRecipe = recipe;
        _craftingTimer = 0f;
        IsCrafting.Value = true;
        
        Debug.Log($"[CraftingSystem] Started crafting {recipe.RecipeName}");
        NotifyCraftingStartedClientRpc(recipeIndex);
    }

    private void CompleteCrafting()
    {
        if (_currentRecipe == null) return;
        
        // Add output item to inventory
        int outputId = _itemDatabase.GetItemId(_currentRecipe.OutputItem);
        _inventory.AddItemServerRpc(outputId, _currentRecipe.OutputQuantity);
        
        Debug.Log($"[CraftingSystem] Crafted {_currentRecipe.OutputQuantity}x {_currentRecipe.OutputItem.ItemName}");
        
        int recipeIndex = _availableRecipes.IndexOf(_currentRecipe);
        NotifyCraftingCompleteClientRpc(recipeIndex);
        
        // Reset
        IsCrafting.Value = false;
        CraftingProgress.Value = 0f;
        _currentRecipe = null;
    }

    [ServerRpc(RequireOwnership = false)]
    public void CancelCraftingServerRpc()
    {
        if (!IsCrafting.Value) return;
        
        // Refund ingredients (optional)
        if (_currentRecipe != null)
        {
            foreach (var ingredient in _currentRecipe.Ingredients)
            {
                if (ingredient.Item == null) continue;
                
                int itemId = _itemDatabase.GetItemId(ingredient.Item);
                _inventory.AddItemServerRpc(itemId, ingredient.Quantity);
            }
        }
        
        IsCrafting.Value = false;
        CraftingProgress.Value = 0f;
        _currentRecipe = null;
        
        Debug.Log("[CraftingSystem] Crafting cancelled, ingredients refunded");
    }
    #endregion

    #region Client RPCs
    [ClientRpc]
    private void NotifyCraftingStartedClientRpc(int recipeIndex)
    {
        if (recipeIndex >= 0 && recipeIndex < _availableRecipes.Count)
        {
            OnCraftingStarted?.Invoke(_availableRecipes[recipeIndex]);
        }
    }

    [ClientRpc]
    private void NotifyCraftingCompleteClientRpc(int recipeIndex)
    {
        if (recipeIndex >= 0 && recipeIndex < _availableRecipes.Count)
        {
            OnCraftingComplete?.Invoke(_availableRecipes[recipeIndex]);
        }
    }

    [ClientRpc]
    private void NotifyCraftingFailedClientRpc(int recipeIndex, string reason)
    {
        if (recipeIndex >= 0 && recipeIndex < _availableRecipes.Count)
        {
            OnCraftingFailed?.Invoke(_availableRecipes[recipeIndex], reason);
        }
    }
    #endregion

    #region Query Methods
    public IReadOnlyList<CraftingRecipe> GetAvailableRecipes() => _availableRecipes;
    
    public List<CraftingRecipe> GetCraftableRecipes()
    {
        var result = new List<CraftingRecipe>();
        foreach (var recipe in _availableRecipes)
        {
            if (recipe.CanCraft(_inventory, _itemDatabase))
            {
                result.Add(recipe);
            }
        }
        return result;
    }
    #endregion
}
