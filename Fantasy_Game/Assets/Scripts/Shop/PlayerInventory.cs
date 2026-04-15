using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    public int maxSlots = 8;

    [Header("Current Items")]
    public List<ItemData> items = new List<ItemData>();

    // Add item to inventory
    public bool AddItem(ItemData item)
    {
        if (item == null)
        {
            Debug.LogWarning("AddItem failed: item is null");
            return false;
        }

        if (items.Count >= maxSlots)
        {
            Debug.LogWarning("AddItem failed: inventory is full");
            return false;
        }

        items.Add(item);
        Debug.Log("AddItem success: " + item.displayName);
        return true;
    }

    // Remove item from inventory
    public bool RemoveItem(ItemData item)
    {
        if (item == null)
        {
            Debug.LogWarning("RemoveItem failed: item is null");
            return false;
        }

        bool removed = items.Remove(item);

        if (removed)
            Debug.Log("RemoveItem success: " + item.displayName);
        else
            Debug.LogWarning("RemoveItem failed: item not found - " + item.displayName);

        return removed;
    }

    // Check if inventory contains this item
    public bool HasItem(ItemData item)
    {
        if (item == null) return false;
        return items.Contains(item);
    }

    // Check if inventory is full
    public bool IsFull()
    {
        return items.Count >= maxSlots;
    }

    // Get current item count
    public int GetItemCount()
    {
        return items.Count;
    }

    // Clear all items (for testing)
    public void ClearInventory()
    {
        items.Clear();
        Debug.Log("Player inventory cleared");
    }

    // Print all items in console
    public void PrintInventory()
    {
        if (items.Count == 0)
        {
            Debug.Log("Inventory is empty");
            return;
        }

        string result = "Inventory: ";
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] != null)
                result += items[i].displayName;

            if (i < items.Count - 1)
                result += ", ";
        }

        Debug.Log(result);
    }
}