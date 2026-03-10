using UnityEngine;

public class ShopDebugOpen : MonoBehaviour
{
    public ShopManager shopManager;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (shopManager != null)
                shopManager.OpenShop();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            if (shopManager != null)
                shopManager.CloseShop();
        }
    }
}