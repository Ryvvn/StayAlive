using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Player inventory system. Host-authoritative.
/// </summary>
public class Inventory : NetworkBehaviour
{
    #region Configuration
    [Header("Inventory Settings")]
    [SerializeField] private int _maxSlots = 20;
    [SerializeField] private ItemDatabase _itemDatabase;
    #endregion

    #region State
    // Network synced inventory using simple struct
    public NetworkList<InventorySlot> Slots;
    #endregion

    #region Events
    public event Action OnInventoryChanged;
    public event Action<int> OnSlotChanged;
    public event Action<ItemData> OnItemAdded;
    public event Action<ItemData> OnItemRemoved;
    #endregion

    #region Structs
    [Serializable]
    public struct InventorySlot : INetworkSerializable, IEquatable<InventorySlot>
    {
        public int ItemId; // -1 = empty
        public int Quantity;
        
        public InventorySlot(int itemId, int quantity)
        {
            ItemId = itemId;
            Quantity = quantity;
        }
        
        public static InventorySlot Empty => new InventorySlot(-1, 0);
        public bool IsEmpty => ItemId < 0 || Quantity <= 0;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ItemId);
            serializer.SerializeValue(ref Quantity);
        }
        
        public bool Equals(InventorySlot other)
        {
            return ItemId == other.ItemId && Quantity == other.Quantity;
        }
    }
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        Slots = new NetworkList<InventorySlot>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsServer)
        {
            // Initialize empty slots
            for (int i = 0; i < _maxSlots; i++)
            {
                Slots.Add(InventorySlot.Empty);
            }
        }
        
        Slots.OnListChanged += HandleSlotsChanged;
    }

    public override void OnNetworkDespawn()
    {
        Slots.OnListChanged -= HandleSlotsChanged;
        base.OnNetworkDespawn();
    }
    #endregion

    #region Add Items
    /// <summary>
    /// Try to add item to inventory. Returns amount that couldn't be added.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void AddItemServerRpc(int itemId, int quantity)
    {
        AddItemInternal(itemId, quantity);
    }

    private int AddItemInternal(int itemId, int quantity)
    {
        if (!IsServer) return quantity;
        
        ItemData item = _itemDatabase?.GetItem(itemId);
        if (item == null) return quantity;
        
        int remaining = quantity;
        
        // First, try to stack with existing items
        if (item.IsStackable)
        {
            for (int i = 0; i < Slots.Count && remaining > 0; i++)
            {
                var slot = Slots[i];
                if (slot.ItemId == itemId && slot.Quantity < item.MaxStackSize)
                {
                    int canAdd = Mathf.Min(remaining, item.MaxStackSize - slot.Quantity);
                    Slots[i] = new InventorySlot(itemId, slot.Quantity + canAdd);
                    remaining -= canAdd;
                }
            }
        }
        
        // Then, find empty slots
        for (int i = 0; i < Slots.Count && remaining > 0; i++)
        {
            if (Slots[i].IsEmpty)
            {
                int toAdd = item.IsStackable ? Mathf.Min(remaining, item.MaxStackSize) : 1;
                Slots[i] = new InventorySlot(itemId, toAdd);
                remaining -= toAdd;
            }
        }
        
        if (remaining < quantity)
        {
            Debug.Log($"[Inventory] Added {quantity - remaining}x {item.ItemName}");
            NotifyItemAddedClientRpc(itemId);
        }
        
        return remaining;
    }
    #endregion

    #region Remove Items
    [ServerRpc(RequireOwnership = false)]
    public void RemoveItemServerRpc(int itemId, int quantity)
    {
        RemoveItemInternal(itemId, quantity);
    }

    private int RemoveItemInternal(int itemId, int quantity)
    {
        if (!IsServer) return quantity;
        
        int remaining = quantity;
        
        for (int i = Slots.Count - 1; i >= 0 && remaining > 0; i--)
        {
            var slot = Slots[i];
            if (slot.ItemId == itemId)
            {
                int toRemove = Mathf.Min(remaining, slot.Quantity);
                int newQuantity = slot.Quantity - toRemove;
                
                Slots[i] = newQuantity > 0 
                    ? new InventorySlot(itemId, newQuantity) 
                    : InventorySlot.Empty;
                
                remaining -= toRemove;
            }
        }
        
        return remaining;
    }
    #endregion

    #region Use Items
    [ServerRpc(RequireOwnership = false)]
    public void UseItemServerRpc(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= Slots.Count) return;
        
        var slot = Slots[slotIndex];
        if (slot.IsEmpty) return;
        
        ItemData item = _itemDatabase?.GetItem(slot.ItemId);
        if (item == null || !item.IsConsumable) return;
        
        // Apply effects
        var playerStats = GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            if (item.HealthRestore > 0) playerStats.HealServerRpc(item.HealthRestore);
            if (item.HungerRestore > 0) playerStats.FeedServerRpc(item.HungerRestore);
            if (item.ThirstRestore > 0) playerStats.DrinkServerRpc(item.ThirstRestore);
        }
        
        // Consume item
        int newQuantity = slot.Quantity - 1;
        Slots[slotIndex] = newQuantity > 0 
            ? new InventorySlot(slot.ItemId, newQuantity) 
            : InventorySlot.Empty;
        
        Debug.Log($"[Inventory] Used {item.ItemName}");
    }
    #endregion

    #region Query Methods
    public int GetItemCount(int itemId)
    {
        int count = 0;
        for (int i = 0; i < Slots.Count; i++)
        {
            if (Slots[i].ItemId == itemId)
            {
                count += Slots[i].Quantity;
            }
        }
        return count;
    }

    public bool HasItem(int itemId, int quantity = 1)
    {
        return GetItemCount(itemId) >= quantity;
    }

    public int GetEmptySlotCount()
    {
        int count = 0;
        for (int i = 0; i < Slots.Count; i++)
        {
            if (Slots[i].IsEmpty) count++;
        }
        return count;
    }

    public ItemData GetItemInSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= Slots.Count) return null;
        var slot = Slots[slotIndex];
        return slot.IsEmpty ? null : _itemDatabase?.GetItem(slot.ItemId);
    }
    #endregion

    #region Events
    private void HandleSlotsChanged(NetworkListEvent<InventorySlot> changeEvent)
    {
        OnInventoryChanged?.Invoke();
        OnSlotChanged?.Invoke(changeEvent.Index);
    }

    [ClientRpc]
    private void NotifyItemAddedClientRpc(int itemId)
    {
        ItemData item = _itemDatabase?.GetItem(itemId);
        if (item != null)
        {
            OnItemAdded?.Invoke(item);
        }
    }
    #endregion
}
