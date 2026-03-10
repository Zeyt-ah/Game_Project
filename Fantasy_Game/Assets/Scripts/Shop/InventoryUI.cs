using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public PlayerInventory playerInventory;
    public InventorySlotUI[] slots;

    public void Refresh()
    {
        if (slots == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (playerInventory != null && i < playerInventory.items.Count)
                slots[i].SetItem(playerInventory.items[i]);
            else
                slots[i].SetItem(null);
        }
    }
}