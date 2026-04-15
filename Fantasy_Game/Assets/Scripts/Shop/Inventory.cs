using UnityEngine;
using System.Collections.Generic;

// Stores purchased/owned item IDs
public class Inventory : MonoBehaviour
{
    private const string SaveKey = "OWNED_ITEMS";

    private HashSet<string> ownedItems = new HashSet<string>();

    private void Awake()
    {
        Load();
    }

    private void Load()
    {
        string raw = PlayerPrefs.GetString(SaveKey, "");
        ownedItems = new HashSet<string>(
            raw.Split('|', System.StringSplitOptions.RemoveEmptyEntries)
        );

        Debug.Log("Inventory Load: " + raw);
    }

    private void Save()
    {
        string raw = string.Join("|", ownedItems);
        PlayerPrefs.SetString(SaveKey, raw);
        PlayerPrefs.Save();

        Debug.Log("Inventory Save: " + raw);
    }

    public bool IsOwned(string itemId)
    {
        bool owned = ownedItems.Contains(itemId);
        Debug.Log("IsOwned check: " + itemId + " => " + owned);
        return owned;
    }

    public void Add(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            Debug.LogWarning("Inventory Add failed: itemId is null or empty");
            return;
        }

        if (ownedItems.Contains(itemId))
        {
            Debug.Log("Inventory Add skipped: already owns " + itemId);
            return;
        }

        ownedItems.Add(itemId);
        Save();

        Debug.Log("Inventory Add success: " + itemId);
    }

    public void Remove(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            Debug.LogWarning("Inventory Remove failed: itemId is null or empty");
            return;
        }

        if (ownedItems.Remove(itemId))
        {
            Save();
            Debug.Log("Inventory Remove success: " + itemId);
        }
        else
        {
            Debug.Log("Inventory Remove skipped: item not found " + itemId);
        }
    }

    public void ClearOwnedItems()
    {
        ownedItems.Clear();
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();

        Debug.Log("Owned items cleared");
    }

    public void PrintOwnedItems()
    {
        string raw = string.Join(", ", ownedItems);
        Debug.Log("Owned Items: " + raw);
    }
}