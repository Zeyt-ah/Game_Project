using UnityEngine;
using TMPro;
using Ilumisoft.HealthSystem; // Required for HealthComponent

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
        Debug.Log("Shop Message: " + message);

        if (shopMessageUI != null)
            shopMessageUI.ShowMessage(message);
    }

    public void OpenShop()
    {
        if (shopUI != null) shopUI.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (wallet != null) UpdateGoldUI(wallet.Gold);
        if (inventoryUI != null) inventoryUI.Refresh();

        ShowShopMessage("Welcome to the Tavern.");
    }

    public void CloseShop()
    {
        if (shopUI != null) shopUI.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    /// <summary>
    /// Core logic for buying items from the shop
    /// </summary>
    public void Buy(int index)
    {
        // 1. Validation Checks
        if (items == null || index < 0 || index >= items.Length)
        {
            Debug.LogError("ShopManager: Invalid item index!");
            return;
        }

        ItemData item = items[index];
        if (item == null) return;

        // 2. Check One-Time Purchase
        if (item.oneTimePurchase && ownedInventory != null && ownedInventory.IsOwned(item.itemId))
        {
            ShowShopMessage("You already own this item.");
            return;
        }

        // 3. Try to spend gold
        if (wallet == null || !wallet.Spend(item.price))
        {
            ShowShopMessage("You don't have enough gold.");
            return;
        }

        // 4. Branching Logic based on Item Index
        bool purchaseProcessed = false;

        // INDEX 0 & 1: Consumables (Roasted Meat, Beer) -> Immediate Healing
        if (index == 0 || index == 1)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                HealthComponent health = player.GetComponent<HealthComponent>();
                if (health != null)
                {
                    // Restores 20 health. You can change this value.
                    health.AddHealth(20f);
                    ShowShopMessage("You consumed " + item.displayName + " and felt better!");
                    purchaseProcessed = true;
                }
                else
                {
                    Debug.LogWarning("Player found but HealthComponent is missing!");
                }
            }
            else
            {
                Debug.LogWarning("Player object with tag 'Player' not found!");
            }
        }
        // INDEX 2 (and others): Equipment (Sword) -> Add to Inventory
        else
        {
            if (playerInventory != null)
            {
                purchaseProcessed = playerInventory.AddItem(item);

                if (purchaseProcessed)
                {
                    ShowShopMessage("Purchased " + item.displayName + ".");
                }
                else
                {
                    // Refund if inventory is full
                    ShowShopMessage("Your bag is full.");
                    wallet.AddGold(item.price); 
                    return; 
                }
            }
        }

        // 5. Finalize Purchase
        if (purchaseProcessed)
        {
            if (item.oneTimePurchase && ownedInventory != null)
            {
                ownedInventory.Add(item.itemId);
            }

            if (inventoryUI != null) inventoryUI.Refresh();
            UpdateGoldUI(wallet.Gold);
        }
    }

    public void TestClick()
    {
        ShowShopMessage("Test message works!");
    }
}