using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Database of all items in the game.
/// Create via: Right-click → Create → StayAlive → Item Database
/// </summary>
[CreateAssetMenu(fileName = "ItemDatabase", menuName = "StayAlive/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<ItemData> _items = new();
    
    private Dictionary<int, ItemData> _itemLookup;
    private Dictionary<string, ItemData> _nameLookup;
    
    private void OnEnable()
    {
        BuildLookups();
    }
    
    private void BuildLookups()
    {
        _itemLookup = new Dictionary<int, ItemData>();
        _nameLookup = new Dictionary<string, ItemData>();
        
        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i] != null)
            {
                _itemLookup[i] = _items[i];
                _nameLookup[_items[i].ItemName.ToLower()] = _items[i];
            }
        }
    }
    
    public ItemData GetItem(int id)
    {
        if (_itemLookup == null) BuildLookups();
        return _itemLookup.TryGetValue(id, out var item) ? item : null;
    }
    
    public ItemData GetItem(string name)
    {
        if (_nameLookup == null) BuildLookups();
        return _nameLookup.TryGetValue(name.ToLower(), out var item) ? item : null;
    }
    
    public int GetItemId(ItemData item)
    {
        return _items.IndexOf(item);
    }
    
    public int ItemCount => _items.Count;
    public IReadOnlyList<ItemData> AllItems => _items;
}
