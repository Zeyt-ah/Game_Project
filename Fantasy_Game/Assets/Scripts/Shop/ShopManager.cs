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
    public ShopMessageUI shopMessageUI;

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

    void ShowShopMessage(string message)
    {
        if (shopMessageUI != null)
            shopMessageUI.ShowMessage(message);
        else
            Debug.Log(message);
    }

    public void OpenShop()
    {
        if (shopUI != null)
            shopUI.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (wallet != null)
            UpdateGoldUI(wallet.Gold);

        if (inventoryUI != null)
            inventoryUI.Refresh();

        ShowShopMessage("Select an item");
    }

    public void CloseShop()
    {
        if (shopUI != null)
            shopUI.SetActive(false);

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
            ShowShopMessage(item.displayName + " already purchased!");
            return;
        }

        if (playerInventory.items.Count >= playerInventory.maxSlots)
        {
            ShowShopMessage("Inventory is full!");
            return;
        }

        if (!wallet.Spend(item.price))
        {
            ShowShopMessage("Not enough gold!");
            return;
        }

        bool added = playerInventory.AddItem(item);
        if (!added)
        {
            ShowShopMessage("Inventory is full!");
            return;
        }

        if (item.oneTimePurchase)
            ownedInventory.Add(item.itemId);

        if (inventoryUI != null)
            inventoryUI.Refresh();

        ShowShopMessage(item.displayName + " purchased!");
    }

    public void TestClick()
    {
        Debug.Log("TEST CLICK OK");
        ShowShopMessage("Test message");
    }
}