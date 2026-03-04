using UnityEngine;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [Header("Item Data")]
    public ItemData[] items;

    [Header("References")]
    public PlayerWallet wallet;

    [Header("UI")]
    public TMP_Text goldText;
    public TMP_Text messageText;

    [Header("Shop UI Root")]
    public GameObject shopUI;   // Drag your Canvas or the Shop Panel root here (the whole shop UI)

    void Start()
    {
        // Start with the shop closed (optional but recommended)
        CloseShop();

        // Subscribe to gold change event so UI updates automatically
        if (wallet != null)
        {
            wallet.OnGoldChanged += UpdateGoldUI;
            UpdateGoldUI(wallet.Gold);
        }

        // Clear message text at start
        if (messageText != null)
            messageText.text = "";
    }

    void OnDestroy()
    {
        // Unsubscribe to avoid event leaks when object is destroyed
        if (wallet != null)
            wallet.OnGoldChanged -= UpdateGoldUI;
    }

    void UpdateGoldUI(int gold)
    {
        // Update gold UI text
        if (goldText != null)
            goldText.text = "Gold: " + gold;
    }

    public void OpenShop()
    {
        // Enable shop UI
        if (shopUI != null) shopUI.SetActive(true);

        // Show and unlock cursor so player can click UI
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Reset message and refresh gold display
        if (messageText != null) messageText.text = "";
        if (wallet != null) UpdateGoldUI(wallet.Gold);
    }

    public void CloseShop()
    {
        // Disable shop UI
        if (shopUI != null) shopUI.SetActive(false);

        // Hide and lock cursor back for gameplay
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Buy(int index)
    {
        Debug.Log("Buy called index: " + index);

        // Validate index
        if (items == null || index < 0 || index >= items.Length)
        {
            Debug.LogWarning("Invalid item index!");
            return;
        }

        // Validate wallet reference
        if (wallet == null)
        {
            Debug.LogWarning("Wallet is not assigned!");
            return;
        }

        ItemData item = items[index];

        // Try spending gold; if failed, show message
        if (!wallet.Spend(item.price))
        {
            if (messageText != null)
                messageText.text = "Not enough gold!";
            return;
        }

        // Purchase success message
        if (messageText != null)
            messageText.text = item.displayName + " purchased!";
    }

    // Simple click test method for debugging
    public void TestClick()
    {
        Debug.Log("TEST CLICK OK");
    }
}