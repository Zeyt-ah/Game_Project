using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public int maxSlots = 8;
    public List<ItemData> items = new List<ItemData>();

    public bool AddItem(ItemData item)
    {
        if (item == null) return false;
        if (items.Count >= maxSlots) return false;

        items.Add(item);
        return true;
    }

    public void RemoveItem(ItemData item)
    {
        if (item == null) return;
        items.Remove(item);
    }

    public bool HasItem(ItemData item)
    {
        return items.Contains(item);
    }
}