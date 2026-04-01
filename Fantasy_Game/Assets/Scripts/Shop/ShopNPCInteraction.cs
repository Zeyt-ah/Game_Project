using UnityEngine;
using UnityEngine.InputSystem;

public class ShopNPCInteraction : MonoBehaviour
{
    public ShopManager shopManager;
    public ShopMessageUI shopMessageUI;

    public KeyCode interactKey = KeyCode.E;
    public bool closeWhenLeave = true;

    public PlayerInput playerInput;

    private bool playerInRange = false;

    void Start()
    {
        if (shopManager != null)
            shopManager.CloseShop();
    }

    void Update()
    {
        if (!playerInRange) return;

        if (Input.GetKeyDown(interactKey))
        {
            ToggleShop();
        }
    }

    void ToggleShop()
    {
        if (shopManager == null || shopManager.shopUI == null) return;

        bool open = !shopManager.shopUI.activeSelf;

        if (open)
        {
            shopManager.OpenShop();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (playerInput != null)
                playerInput.enabled = false;
        }
        else
        {
            shopManager.CloseShop();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (playerInput != null)
                playerInput.enabled = true;

            if (shopMessageUI != null)
                shopMessageUI.ShowMessage("Shop closed");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;

        if (shopMessageUI != null)
            shopMessageUI.ShowMessage("Press E to open shop");
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;

        if (closeWhenLeave && shopManager != null && shopManager.shopUI != null && shopManager.shopUI.activeSelf)
        {
            shopManager.CloseShop();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (playerInput != null)
                playerInput.enabled = true;

            if (shopMessageUI != null)
                shopMessageUI.ShowMessage("Shop closed");
        }
    }
}