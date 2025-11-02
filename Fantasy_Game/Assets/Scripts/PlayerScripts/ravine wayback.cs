using UnityEngine;

public class TeleportOtherObject : MonoBehaviour
{
    [Header("Teleport Settings")]
    public Transform objectToTeleport;   // The object that will be teleported
    public Transform teleportTarget;     // Destination position
    public string triggerTag = "TeleportTrigger";  // Tag of the object that triggers teleport

    private void OnTriggerEnter(Collider other)
    {
        // Check if the collided object has the specified tag
        if (other.CompareTag(triggerTag))
        {
            if (objectToTeleport != null && teleportTarget != null)
            {
                // Move the target object to the teleport destination
                objectToTeleport.position = teleportTarget.position;

                // Optional: reset rotation
                // objectToTeleport.rotation = teleportTarget.rotation;
            }
            else
            {
                Debug.LogWarning("Teleport object or target is not assigned!");
            }
        }
    }
}
