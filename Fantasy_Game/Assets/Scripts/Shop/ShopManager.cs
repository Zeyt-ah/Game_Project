using UnityEngine;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [Header("Item Data")]
    public ItemData[] items;

    [Header("References")]
    public PlayerWallet wallet;
    public Inventory ownedInventory;
    public PlayerInventory playerInventory;
    public InventoryUI inventoryUI;

    [Header("UI")]
    public TMP_Text goldText;
    public TMP_Text messageText;

    [Header("Shop UI Root")]
    public GameObject shopUI;

    void Start()
    {
        CloseShop();

        if (wallet != null)
        {
            wallet.OnGoldChanged += UpdateGoldUI;
            UpdateGoldUI(wallet.Gold);
        }

        if (messageText != null)
            messageText.text = "";
    }

    void OnDestroy()
    {
        if (wallet != null)
            wallet.OnGoldChanged -= UpdateGoldUI;
    }

    void UpdateGoldUI(int gold)
    {
        if (goldText != null)
            goldText.text = "Gold: " + gold;
    }

    public void OpenShop()
    {
        if (shopUI != null) shopUI.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (messageText != null)
            messageText.text = "";

        if (wallet != null)
            UpdateGoldUI(wallet.Gold);

        if (inventoryUI != null)
            inventoryUI.Refresh();
    }

    public void CloseShop()
    {
        if (shopUI != null) shopUI.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Buy(int index)
    {
        Debug.Log("Buy called index: " + index);

        if (items == null || index < 0 || index >= items.Length)
        {
            Debug.LogWarning("Invalid item index!");
            return;
        }

        if (wallet == null)
        {
            Debug.LogWarning("Wallet is not assigned!");
            return;
        }

        if (ownedInventory == null)
        {
            Debug.LogWarning("Owned Inventory is not assigned!");
            return;
        }

        if (playerInventory == null)
        {
            Debug.LogWarning("Player Inventory is not assigned!");
            return;
        }

        ItemData item = items[index];

        if (item == null)
        {
            Debug.LogWarning("Item is null!");
            return;
        }

        if (item.oneTimePurchase && ownedInventory.IsOwned(item.itemId))
        {
            if (messageText != null)
                messageText.text = item.displayName + " already purchased!";
            return;
        }

        if (playerInventory.items.Count >= playerInventory.maxSlots)
        {
            if (messageText != null)
                messageText.text = "Inventory is full!";
            return;
        }

        if (!wallet.Spend(item.price))
        {
            if (messageText != null)
                messageText.text = "Not enough gold!";
            return;
        }

        bool added = playerInventory.AddItem(item);
        if (!added)
        {
            if (messageText != null)
                messageText.text = "Inventory is full!";
            return;
        }

        if (item.oneTimePurchase)
            ownedInventory.Add(item.itemId);

        if (inventoryUI != null)
            inventoryUI.Refresh();

        if (messageText != null)
            messageText.text = item.displayName + " purchased!";
    }

    public void TestClick()
    {
        Debug.Log("TEST CLICK OK");
    }
}