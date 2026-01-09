using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// UI slot for a building piece in the building menu.
/// </summary>
public class BuildingSlotUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _costText;
    [SerializeField] private Button _button;
    [SerializeField] private Image _background;
    
    private BuildingPiece _piece;
    private Action _onClick;

    public void Setup(BuildingPiece piece, Action onClick)
    {
        _piece = piece;
        _onClick = onClick;
        
        if (_icon != null) _icon.sprite = piece.Icon;
        if (_nameText != null) _nameText.text = piece.PieceName;
        
        if (_costText != null)
        {
            string costStr = "";
            foreach (var cost in piece.Costs)
            {
                if (cost.Item != null)
                {
                    costStr += $"{cost.Quantity}x {cost.Item.ItemName}\n";
                }
            }
            _costText.text = costStr.TrimEnd('\n');
        }
        
        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => _onClick?.Invoke());
        }
    }

    public void SetAffordable(bool canAfford)
    {
        if (_background != null)
        {
            _background.color = canAfford ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.8f);
        }
        
        if (_button != null)
        {
            _button.interactable = canAfford;
        }
    }
}
