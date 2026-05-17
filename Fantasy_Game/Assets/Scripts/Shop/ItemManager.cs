using UnityEngine;
using Ilumisoft.HealthSystem; // Import the custom health system namespace

public class ItemManager : MonoBehaviour
{
    [Header("Player References")]
    [Tooltip("Assign the Player object that has the Health component attached.")]
    public Health playerHealth; // Slot to assign the Player's Health component via the Unity Inspector

    /// <summary>
    /// Consumes meat and restores 20 health points.
    /// </summary>
    public void UseMeat()
    {
        if (playerHealth != null)
        {
            playerHealth.AddHealth(20.0f);
            Debug.Log("Item Used: Meat. Restored 20 Health.");
        }
        else
        {
            Debug.LogWarning("ItemManager: Player Health component is not assigned!");
        }
    }

    /// <summary>
    /// Consumes beer and restores 10 health points.
    /// </summary>
    public void UseBeer()
    {
        if (playerHealth != null)
        {
            playerHealth.AddHealth(10.0f);
            Debug.Log("Item Used: Beer. Restored 10 Health.");
        }
        else
        {
            Debug.LogWarning("ItemManager: Player Health component is not assigned!");
        }
    }
}