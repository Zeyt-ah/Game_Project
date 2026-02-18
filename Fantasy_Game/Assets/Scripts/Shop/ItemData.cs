using UnityEngine;

// Defines the category/type of the item
public enum ItemType { Consumable, Weapon, Skin }

// ScriptableObject that stores item data for the shop
// Create new items via: Right Click → Create → Shop → Item
[CreateAssetMenu(menuName = "Shop/Item")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemId;          // Unique ID (must not be duplicated)
    public string displayName;     // Name shown in UI

    [Header("Item Properties")]
    public ItemType type;          // Item category
    public int price;              // Purchase price

    [Header("Visuals")]
    public Sprite icon;            // Icon displayed in shop

    [TextArea]
    public string description;     // Description shown in shop

    [Header("Purchase Settings")]
    public bool oneTimePurchase = true; // If true, item can only be bought once
}
