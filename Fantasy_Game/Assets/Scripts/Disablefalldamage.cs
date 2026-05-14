using UnityEngine;

public class NoFallDamageZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerMovementScript player = other.GetComponent<PlayerMovementScript>();

        if (player != null)
        {
            player.disableFallDamage = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerMovementScript player = other.GetComponent<PlayerMovementScript>();

        if (player != null)
        {
            player.disableFallDamage = false;
        }
    }
}