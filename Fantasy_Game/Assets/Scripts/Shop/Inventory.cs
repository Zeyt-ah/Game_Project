using UnityEngine;
using System.Collections.Generic;

// Stores purchased/owned items
public class Inventory : MonoBehaviour
{
    // Stores item IDs of owned items
    private HashSet<string> ownedItems = new HashSet<string>();

    void Awake()
    {
        Load();
    }

    // Load saved items from PlayerPrefs
    void Load()
    {
        string raw = PlayerPrefs.GetString("OWNED_ITEMS", "");
        ownedItems = new HashSet<string>(
            raw.Split('|', System.StringSplitOptions.RemoveEmptyEntries)
        );
    }

    // Save owned items to PlayerPrefs
    void Save()
    {
        PlayerPrefs.SetString("OWNED_ITEMS", string.Join("|", ownedItems));
    }

    // Check if the player already owns an item
    public bool IsOwned(string itemId)
    {
        return ownedItems.Contains(itemId);
    }

    // Add a new item to the inventory
    public void Add(string itemId)
    {
        ownedItems.Add(itemId);
        Save();
    }
}
