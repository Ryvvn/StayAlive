using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Main crafting UI controller.
/// Toggle with 'C' key.
/// </summary>
public class CraftingUI : MonoBehaviour
{
    #region Configuration
    [Header("References")]
    [SerializeField] private CraftingSystem _craftingSystem;
    [SerializeField] private Inventory _inventory;
    [SerializeField] private ItemDatabase _itemDatabase;
    
    [Header("UI Elements")]
    [SerializeField] private GameObject _panel;
    [SerializeField] private Transform _recipeListContent;
    [SerializeField] private GameObject _recipeSlotPrefab;
    
    [Header("Detail Panel")]
    [SerializeField] private Image _selectedIcon;
    [SerializeField] private TextMeshProUGUI _selectedName;
    [SerializeField] private TextMeshProUGUI _selectedDescription;
    [SerializeField] private Transform _ingredientListContent;
    [SerializeField] private GameObject _ingredientSlotPrefab;
    [SerializeField] private Button _craftButton;
    [SerializeField] private TextMeshProUGUI _craftButtonText;
    
    [Header("Progress")]
    [SerializeField] private GameObject _progressPanel;
    [SerializeField] private Image _progressBar;
    [SerializeField] private TextMeshProUGUI _progressText;
    [SerializeField] private Button _cancelButton;
    
    [Header("Input")]
    [SerializeField] private KeyCode _toggleKey = KeyCode.C;
    #endregion

    #region State
    private List<RecipeSlotUI> _recipeSlots = new();
    private CraftingRecipe _selectedRecipe;
    private int _selectedRecipeIndex = -1;
    private bool _isOpen = false;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        if (_panel != null) _panel.SetActive(false);
        if (_progressPanel != null) _progressPanel.SetActive(false);
        
        if (_craftButton != null)
            _craftButton.onClick.AddListener(OnCraftClicked);
        
        if (_cancelButton != null)
            _cancelButton.onClick.AddListener(OnCancelClicked);
        
        // Auto-find ItemDatabase if not assigned
        if (_itemDatabase == null)
        {
            _itemDatabase = Resources.Load<ItemDatabase>("ItemDatabase");
            if (_itemDatabase == null)
            {
                // Try to find in loaded assets
                _itemDatabase = FindObjectOfType<ItemDatabase>();
            }
        }
        
        // Start coroutine to wait for local player spawn
        StartCoroutine(WaitForLocalPlayer());
    }

    private System.Collections.IEnumerator WaitForLocalPlayer()
    {
        // Wait until we have references (player spawns dynamically)
        float timeout = 10f;
        float elapsed = 0f;
        
        while ((_craftingSystem == null || _inventory == null) && elapsed < timeout)
        {
            TryBindToLocalPlayer();
            yield return new WaitForSeconds(0.5f);
            elapsed += 0.5f;
        }
        
        if (_craftingSystem != null && _inventory != null)
        {
            PopulateRecipeList();
            SubscribeToEvents();
            Debug.Log("[CraftingUI] Successfully bound to local player!");
        }
        else
        {
            Debug.LogWarning("[CraftingUI] Could not find local player CraftingSystem/Inventory!");
        }
    }

    private void TryBindToLocalPlayer()
    {
        // Find local player's crafting system
        var allCraftingSystems = FindObjectsOfType<CraftingSystem>();
        foreach (var cs in allCraftingSystems)
        {
            if (cs.IsOwner) // This is the local player
            {
                _craftingSystem = cs;
                _inventory = cs.GetComponent<Inventory>();
                break;
            }
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void Update()
    {
        if (Input.GetKeyDown(_toggleKey))
        {
            ToggleUI();
        }
        
        UpdateProgressBar();
    }
    #endregion

    #region UI Toggle
    public void ToggleUI()
    {
        _isOpen = !_isOpen;
        if (_panel != null) _panel.SetActive(_isOpen);
        
        if (_isOpen)
        {
            RefreshRecipeList();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void CloseUI()
    {
        _isOpen = false;
        if (_panel != null) _panel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    #endregion

    #region Recipe List
    private void PopulateRecipeList()
    {
        if (_craftingSystem == null || _recipeListContent == null || _recipeSlotPrefab == null) return;
        
        // Clear existing
        foreach (Transform child in _recipeListContent)
        {
            Destroy(child.gameObject);
        }
        _recipeSlots.Clear();
        
        var recipes = _craftingSystem.GetAvailableRecipes();
        for (int i = 0; i < recipes.Count; i++)
        {
            var recipe = recipes[i];
            var slotGO = Instantiate(_recipeSlotPrefab, _recipeListContent);
            var slotUI = slotGO.GetComponent<RecipeSlotUI>();
            
            if (slotUI != null)
            {
                int index = i; // Capture for closure
                slotUI.Setup(recipe, () => SelectRecipe(index));
                _recipeSlots.Add(slotUI);
            }
        }
    }

    private void RefreshRecipeList()
    {
        if (_craftingSystem == null || _inventory == null) return;
        
        var recipes = _craftingSystem.GetAvailableRecipes();
        for (int i = 0; i < _recipeSlots.Count && i < recipes.Count; i++)
        {
            bool canCraft = recipes[i].CanCraft(_inventory, _itemDatabase);
            _recipeSlots[i].SetCraftable(canCraft);
        }
        
        // Refresh selected recipe details
        if (_selectedRecipe != null)
        {
            UpdateDetailPanel();
        }
    }
    #endregion

    #region Recipe Selection
    private void SelectRecipe(int index)
    {
        var recipes = _craftingSystem.GetAvailableRecipes();
        if (index < 0 || index >= recipes.Count) return;
        
        _selectedRecipeIndex = index;
        _selectedRecipe = recipes[index];
        UpdateDetailPanel();
    }

    private void UpdateDetailPanel()
    {
        if (_selectedRecipe == null) return;
        
        // Set basic info
        if (_selectedIcon != null) _selectedIcon.sprite = _selectedRecipe.Icon;
        if (_selectedName != null) _selectedName.text = _selectedRecipe.RecipeName;
        if (_selectedDescription != null) _selectedDescription.text = _selectedRecipe.Description;
        
        // Populate ingredients
        if (_ingredientListContent != null && _ingredientSlotPrefab != null)
        {
            foreach (Transform child in _ingredientListContent)
            {
                Destroy(child.gameObject);
            }
            
            foreach (var ingredient in _selectedRecipe.Ingredients)
            {
                if (ingredient.Item == null) continue;
                
                var slotGO = Instantiate(_ingredientSlotPrefab, _ingredientListContent);
                
                // Setup ingredient display
                var icon = slotGO.GetComponentInChildren<Image>();
                var text = slotGO.GetComponentInChildren<TextMeshProUGUI>();
                
                if (icon != null) icon.sprite = ingredient.Item.Icon;
                if (text != null)
                {
                    int itemId = _itemDatabase != null ? _itemDatabase.GetItemId(ingredient.Item) : -1;
                    int have = _inventory != null ? _inventory.GetItemCount(itemId) : 0;
                    text.text = $"{ingredient.Item.ItemName} ({have}/{ingredient.Quantity})";
                    text.color = have >= ingredient.Quantity ? Color.white : Color.red;
                }
            }
        }
        
        // Update craft button
        bool canCraft = _selectedRecipe.CanCraft(_inventory, _itemDatabase);
        if (_craftButton != null) _craftButton.interactable = canCraft;
        if (_craftButtonText != null) _craftButtonText.text = canCraft ? "Craft" : "Missing Items";
    }
    #endregion

    #region Crafting Actions
    private void OnCraftClicked()
    {
        if (_craftingSystem == null || _selectedRecipeIndex < 0) return;
        
        _craftingSystem.CraftItemServerRpc(_selectedRecipeIndex);
    }

    private void OnCancelClicked()
    {
        if (_craftingSystem == null) return;
        
        _craftingSystem.CancelCraftingServerRpc();
    }

    private void UpdateProgressBar()
    {
        if (_craftingSystem == null) return;
        
        bool isCrafting = _craftingSystem.IsCrafting.Value;
        if (_progressPanel != null) _progressPanel.SetActive(isCrafting);
        
        if (isCrafting && _progressBar != null)
        {
            _progressBar.fillAmount = _craftingSystem.CraftingProgress.Value;
        }
        
        if (isCrafting && _progressText != null)
        {
            int percent = Mathf.RoundToInt(_craftingSystem.CraftingProgress.Value * 100);
            _progressText.text = $"Crafting... {percent}%";
        }
    }
    #endregion

    #region Events
    private void SubscribeToEvents()
    {
        if (_craftingSystem != null)
        {
            _craftingSystem.OnCraftingComplete += HandleCraftingComplete;
            _craftingSystem.OnCraftingFailed += HandleCraftingFailed;
        }
        
        if (_inventory != null)
        {
            _inventory.OnInventoryChanged += RefreshRecipeList;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (_craftingSystem != null)
        {
            _craftingSystem.OnCraftingComplete -= HandleCraftingComplete;
            _craftingSystem.OnCraftingFailed -= HandleCraftingFailed;
        }
        
        if (_inventory != null)
        {
            _inventory.OnInventoryChanged -= RefreshRecipeList;
        }
    }

    private void HandleCraftingComplete(CraftingRecipe recipe)
    {
        Debug.Log($"[CraftingUI] Crafted {recipe.RecipeName}!");
        RefreshRecipeList();
    }

    private void HandleCraftingFailed(CraftingRecipe recipe, string reason)
    {
        Debug.Log($"[CraftingUI] Failed to craft {recipe.RecipeName}: {reason}");
    }
    #endregion
}
