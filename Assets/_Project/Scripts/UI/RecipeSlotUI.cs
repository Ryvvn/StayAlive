using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Individual recipe slot in the crafting UI list.
/// </summary>
public class RecipeSlotUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private Button _button;
    [SerializeField] private Image _background;
    
    [Header("Colors")]
    [SerializeField] private Color _craftableColor = Color.white;
    [SerializeField] private Color _notCraftableColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);
    
    private CraftingRecipe _recipe;
    private Action _onClick;

    public void Setup(CraftingRecipe recipe, Action onClick)
    {
        _recipe = recipe;
        _onClick = onClick;
        
        if (_icon != null) _icon.sprite = recipe.Icon;
        if (_nameText != null) _nameText.text = recipe.RecipeName;
        
        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => _onClick?.Invoke());
        }
    }

    public void SetCraftable(bool canCraft)
    {
        if (_background != null)
        {
            _background.color = canCraft ? _craftableColor : _notCraftableColor;
        }
        
        if (_nameText != null)
        {
            _nameText.color = canCraft ? Color.white : Color.gray;
        }
    }

    public void SetSelected(bool selected)
    {
        // Optional: Visual feedback for selection
        if (_background != null)
        {
            _background.color = selected ? Color.yellow : _craftableColor;
        }
    }
}
