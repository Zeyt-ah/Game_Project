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

    void Start()
    {
        if (wallet != null)
        {
            wallet.OnGoldChanged += UpdateGoldUI;
            UpdateGoldUI(wallet.Gold);
        }

        if (messageText != null)
            messageText.text = "";
    }

    void UpdateGoldUI(int gold)
    {
        if (goldText != null)
            goldText.text = "Gold: " + gold;
    }

    public void Buy(int index)
    {
        Debug.Log("Buy called index: " + index);

        if (items == null || index < 0 || index >= items.Length)
        {
            Debug.LogWarning("Invalid item index!");
            return;
        }

        ItemData item = items[index];

        if (!wallet.Spend(item.price))
        {
            if (messageText != null)
                messageText.text = "Not enough gold!";
            return;
        }

        if (messageText != null)
            messageText.text = item.displayName + " purchased!";
    }
}
