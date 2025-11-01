using UnityEngine;

public class Collectible : MonoBehaviour
{
    [Header("Value of the collectible (e.g. score, coins, etc.)")]
    public int value = 1;

    [Header("Effect prefab to spawn on collect (optional)")]
    public GameObject collectEffect;

    [Header("Time before destroying the collectible after being collected")]
    public float destroyDelay = 0.05f;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that entered the trigger is the player
        if (!other.CompareTag("Player")) return;

        // Add value to player's score or inventory (replace with your own system)
        // Example: GameManager.Instance.AddScore(value);

        // Spawn a visual or sound effect if assigned
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }

        // Destroy the collectible after a short delay
        Destroy(gameObject, destroyDelay);
    }
}
