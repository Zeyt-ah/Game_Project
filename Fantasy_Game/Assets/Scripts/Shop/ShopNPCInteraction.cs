using UnityEngine;
using UnityEngine.InputSystem;

public class ShopNPCInteraction : MonoBehaviour
{
    public GameObject shopCanvas;
    public KeyCode interactKey = KeyCode.E;
    public bool closeWhenLeave = true;

    public PlayerInput playerInput;   // PlayerInput 오브젝트 넣기(Inspector에서 드래그)

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

            if (playerInput != null) playerInput.enabled = false; // ★ 공격/이동 입력 차단
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
