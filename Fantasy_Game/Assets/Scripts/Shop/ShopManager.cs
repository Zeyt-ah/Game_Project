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

    [Header("Debug / Test")]
    public bool clearOwnedItemsOnStart = false;
    public bool clearPlayerInventoryOnStart = false;
    public bool useDebugKey = true;
    public KeyCode debugOpenKey = KeyCode.P;

    private void Start()
    {
        CloseShop();

        if (clearOwnedItemsOnStart && ownedInventory != null)
        {
            ownedInventory.ClearOwnedItems();
            Debug.Log("Owned items cleared on Start");
        }

        if (clearPlayerInventoryOnStart && playerInventory != null)
        {
            playerInventory.ClearInventory();
            Debug.Log("Player inventory cleared on Start");
        }

        if (wallet != null)
        {
            wallet.OnGoldChanged += UpdateGoldUI;
            UpdateGoldUI(wallet.Gold);
        }
        else
        {
            Debug.LogWarning("ShopManager Start: wallet is not assigned");
        }

        Debug.Log("ShopManager Start complete");
        Debug.Log("Items length: " + (items != null ? items.Length.ToString() : "NULL"));
    }

    private void Update()
    {
        if (!useDebugKey) return;

        if (Input.GetKeyDown(debugOpenKey))
        {
            if (shopUI != null && shopUI.activeSelf)
                CloseShop();
            else
                OpenShop();
        }
    }

    private void OnDestroy()
    {
        if (wallet != null)
            wallet.OnGoldChanged -= UpdateGoldUI;
    }

    private void UpdateGoldUI(int gold)
    {
        if (goldText != null)
            goldText.text = "Gold: " + gold;
    }

    private void ShowShopMessage(string message)
    {
        Debug.Log("ShowShopMessage: " + message);

        if (shopMessageUI != null)
            shopMessageUI.ShowMessage(message);
        else
            Debug.LogWarning("ShopMessageUI is not assigned");
    }

    public void OpenShop()
    {
        Debug.Log("OpenShop called");

        if (shopUI != null)
            shopUI.SetActive(true);
        else
            Debug.LogWarning("shopUI is not assigned");

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (wallet != null)
            UpdateGoldUI(wallet.Gold);

        if (inventoryUI != null)
            inventoryUI.Refresh();
        else
            Debug.LogWarning("inventoryUI is not assigned");

        ShowShopMessage("Welcome to the Tavern.");
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
        Debug.Log("=== BUY START ===");
        Debug.Log("Buy called index: " + index);

        if (items == null)
        {
            Debug.LogWarning("items is null");
            return;
        }

        Debug.Log("items length: " + items.Length);

        if (index < 0 || index >= items.Length)
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

        Debug.Log("item name: " + item.displayName);
        Debug.Log("item id: " + item.itemId);
        Debug.Log("item price: " + item.price);
        Debug.Log("current gold before spend: " + wallet.Gold);

        bool alreadyOwned = ownedInventory.IsOwned(item.itemId);
        Debug.Log("already owned: " + alreadyOwned);
        Debug.Log("oneTimePurchase: " + item.oneTimePurchase);

        if (item.oneTimePurchase && alreadyOwned)
        {
            Debug.LogWarning("Blocked: already purchased");
            ShowShopMessage("You already bought that.");
            return;
        }

        Debug.Log("inventory count: " + playerInventory.items.Count + "/" + playerInventory.maxSlots);

        if (playerInventory.items.Count >= playerInventory.maxSlots)
        {
            Debug.LogWarning("Blocked: inventory full");
            ShowShopMessage("Your bag is full.");
            return;
        }

        bool spendSuccess = wallet.Spend(item.price);
        Debug.Log("Spend success: " + spendSuccess);
        Debug.Log("current gold after spend: " + wallet.Gold);

        if (!spendSuccess)
        {
            Debug.LogWarning("Blocked: not enough gold");
            ShowShopMessage("You don't have enough gold.");
            return;
        }

        bool added = playerInventory.AddItem(item);
        Debug.Log("AddItem success: " + added);

        if (!added)
        {
            Debug.LogWarning("Blocked: AddItem failed");
            ShowShopMessage("Your bag is full.");
            return;
        }

        if (item.oneTimePurchase)
            ownedInventory.Add(item.itemId);

        if (inventoryUI != null)
            inventoryUI.Refresh();
        else
            Debug.LogWarning("inventoryUI is null, so UI was not refreshed");

        UpdateGoldUI(wallet.Gold);
        ShowShopMessage("You bought " + item.displayName + ".");
        Debug.Log("=== BUY COMPLETE ===");
    }

    public void TestClick()
    {
        Debug.Log("TEST CLICK OK");
        ShowShopMessage("Test message");
    }
}