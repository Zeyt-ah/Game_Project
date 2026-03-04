using UnityEngine;
using UnityEngine.InputSystem;

public class ShopNPCInteraction : MonoBehaviour
{
    public GameObject shopCanvas;
    public KeyCode interactKey = KeyCode.E;
    public bool closeWhenLeave = true;

    public PlayerInput playerInput;   // PlayerInput obeject (Inspector drag)

    private bool playerInRange = false;

    void Start()
    {
        if (shopCanvas != null) shopCanvas.SetActive(false);
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
        if (shopCanvas == null) return;

        bool open = !shopCanvas.activeSelf;
        shopCanvas.SetActive(open);

        if (open)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (playerInput != null) playerInput.enabled = false; // attack move cancel
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (playerInput != null) playerInput.enabled = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) playerInRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;

        if (closeWhenLeave && shopCanvas != null && shopCanvas.activeSelf)
        {
            shopCanvas.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (playerInput != null) playerInput.enabled = true;
        }
    }
}
