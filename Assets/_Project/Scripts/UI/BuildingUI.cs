using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI for building system. Shows available pieces and costs.
/// </summary>
public class BuildingUI : MonoBehaviour
{
    #region Configuration
    [Header("References")]
    [SerializeField] private BuildingSystem _buildingSystem;
    
    [Header("UI Elements")]
    [SerializeField] private GameObject _panel;
    [SerializeField] private Transform _buildingListContent;
    [SerializeField] private GameObject _buildingSlotPrefab;
    
    [Header("Selected Info")]
    [SerializeField] private Image _selectedIcon;
    [SerializeField] private TextMeshProUGUI _selectedName;
    [SerializeField] private TextMeshProUGUI _selectedCost;
    
    [Header("Input")]
    [SerializeField] private KeyCode _toggleKey = KeyCode.B;
    #endregion

    #region State
    private List<BuildingSlotUI> _slots = new();
    private bool _isOpen;
    #endregion

    private void Start()
    {
        if (_panel != null) _panel.SetActive(false);
        
        StartCoroutine(WaitForBuildingSystem());
    }

    private System.Collections.IEnumerator WaitForBuildingSystem()
    {
        float timeout = 10f;
        float elapsed = 0f;
        
        while (_buildingSystem == null && elapsed < timeout)
        {
            _buildingSystem = FindObjectOfType<BuildingSystem>();
            yield return new WaitForSeconds(0.5f);
            elapsed += 0.5f;
        }
        
        if (_buildingSystem != null)
        {
            PopulateBuildingList();
            SubscribeToEvents();
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
    }

    #region UI Toggle
    public void ToggleUI()
    {
        _isOpen = !_isOpen;
        if (_panel != null) _panel.SetActive(_isOpen);
        
        if (_isOpen)
        {
            RefreshList();
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
    }
    #endregion

    #region Building List
    private void PopulateBuildingList()
    {
        if (_buildingSystem == null || _buildingListContent == null) return;
        
        foreach (Transform child in _buildingListContent)
        {
            Destroy(child.gameObject);
        }
        _slots.Clear();
        
        var buildings = _buildingSystem.GetAvailableBuildings();
        for (int i = 0; i < buildings.Count; i++)
        {
            var building = buildings[i];
            
            if (_buildingSlotPrefab != null)
            {
                var slotGO = Instantiate(_buildingSlotPrefab, _buildingListContent);
                var slot = slotGO.GetComponent<BuildingSlotUI>();
                
                if (slot != null)
                {
                    int index = i;
                    slot.Setup(building, () => SelectBuilding(index));
                    _slots.Add(slot);
                }
            }
        }
    }

    private void RefreshList()
    {
        // Update slot availability based on resources
        // For now just refresh all
    }

    private void SelectBuilding(int index)
    {
        if (_buildingSystem == null) return;
        
        CloseUI();
        _buildingSystem.StartPlacement(index);
    }

    private void UpdateSelectedInfo(BuildingPiece piece)
    {
        if (_selectedIcon != null) _selectedIcon.sprite = piece.Icon;
        if (_selectedName != null) _selectedName.text = piece.PieceName;
        
        if (_selectedCost != null)
        {
            string costText = "";
            foreach (var cost in piece.Costs)
            {
                if (cost.Item != null)
                {
                    costText += $"{cost.Item.ItemName} x{cost.Quantity}\n";
                }
            }
            _selectedCost.text = costText;
        }
    }
    #endregion

    #region Events
    private void SubscribeToEvents()
    {
        if (_buildingSystem != null)
        {
            _buildingSystem.OnBuildingSelected += UpdateSelectedInfo;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (_buildingSystem != null)
        {
            _buildingSystem.OnBuildingSelected -= UpdateSelectedInfo;
        }
    }
    #endregion
}
