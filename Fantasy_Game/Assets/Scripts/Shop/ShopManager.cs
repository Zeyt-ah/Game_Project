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

    [Header("Close Settings")]
    [SerializeField] private KeyCode shopCloseKey = KeyCode.E;

    [Header("Dialogue System Connection")]
    [SerializeField] private DialogueManager dialogueManager;

    private bool isShopActive = false;

    private void Start()
    {
        CloseShop();

        if (dialogueManager == null)
        {
            dialogueManager = FindFirstObjectByType<DialogueManager>();
        }

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
        if (isShopActive && Input.GetKeyDown(shopCloseKey))
        {
            CloseShop();
            return;
        }

        // Force maximum UI priority constraints to keep hardware mouse inputs alive
        if (isShopActive)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

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
        isShopActive = true;

        if (shopUI != null) shopUI.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (wallet != null) UpdateGoldUI(wallet.Gold);
        if (inventoryUI != null) inventoryUI.Refresh();

        ShowShopMessage("Welcome to the Tavern.");
    }

    public void CloseShop()
    {
        isShopActive = false;

        if (shopUI != null) shopUI.SetActive(false);

        if (dialogueManager != null)
        {
            dialogueManager.Close();
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                PlayerScriptNew playerScript = player.GetComponent<PlayerScriptNew>();
                if (playerScript != null)
                {
                    playerScript.EnableMovement();
                    playerScript.EnableAttack();
                }
            }
        }
    }

    /// <summary>
    /// Processes item purchase sequences. 
    /// Restores player health even if current health parameters are full.
    /// </summary>
    public void Buy(int index)
    {
        if (items == null || index < 0 || index >= items.Length)
        {
            Debug.LogError("ShopManager: Invalid item index!");
            return;
        }

        ItemData item = items[index];
        if (item == null) return;

        if (item.oneTimePurchase && ownedInventory != null && ownedInventory.IsOwned(item.itemId))
        {
            ShowShopMessage("You already own this item.");
            return;
        }

        if (wallet == null || !wallet.Spend(item.price))
        {
            ShowShopMessage("You don't have enough gold.");
            return;
        }

        bool purchaseProcessed = false;

        if (index == 0 || index == 1)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                PlayerScriptNew playerScript = player.GetComponent<PlayerScriptNew>();
                if (playerScript != null)
                {
                    // Forces execution cycle to call Heal regardless of active currentHealth ratios
                    playerScript.Heal(20);
                    ShowShopMessage("You consumed " + item.displayName + " and felt better!");
                    purchaseProcessed = true;
                }
                else
                {
                    Debug.LogWarning("Player found but PlayerScriptNew is missing!");
                }
            }
            else
            {
                Debug.LogWarning("Player object with tag 'Player' not found!");
            }
        }
        else
        {
            ShowShopMessage("This item is no longer available.");
            wallet.AddGold(item.price);
            return;
        }

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