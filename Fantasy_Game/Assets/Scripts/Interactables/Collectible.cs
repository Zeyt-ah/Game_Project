using UnityEngine;

public class Collectible : MonoBehaviour
{
    [Header("Value of the collectible (e.g. score, coins, etc.)")]
    public int healthToAdd = 50;

    [Header("Effect prefab to spawn on collect (optional)")]
    public GameObject collectEffect;

    [Header("Time before destroying the collectible after being collected")]
    public float destroyDelay = 0.05f;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that entered the trigger is the player
        if (!other.CompareTag("Player")) return;

        PlayerScriptNew player = other.GetComponent<PlayerScriptNew>();
        if (player != null)
        {
            player.Heal(50);
        }

        // Spawn a visual or sound effect if assigned
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }

        // Destroy the collectible after a short delay
        Destroy(gameObject, destroyDelay);
    }
}
