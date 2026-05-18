using System.Collections.Generic;
using UnityEngine;

public class DragonLingeringFireArea : MonoBehaviour
{
    [SerializeField] private int damagePerTick = 5;
    [SerializeField] private float tickRate = 0.5f;

    private readonly Dictionary<PlayerScriptNew, float> nextDamageTimeByPlayer = new();

    // Sets how long the fire remains on the ground.
    public void Setup(float duration)
    {
        Destroy(gameObject, duration);
    }

    // Damages the player repeatedly while they stay inside the fire
    private void OnTriggerStay(Collider other)
    {
        PlayerScriptNew playerScript = other.GetComponent<PlayerScriptNew>();

        if (playerScript == null)
        {
            playerScript = other.GetComponentInParent<PlayerScriptNew>();
        }

        if (playerScript == null)
        {
            return;
        }

        if (!nextDamageTimeByPlayer.ContainsKey(playerScript))
        {
            nextDamageTimeByPlayer[playerScript] = 0f;
        }

        if (Time.time < nextDamageTimeByPlayer[playerScript])
        {
            return;
        }

        playerScript.TakeDamage(damagePerTick);
        nextDamageTimeByPlayer[playerScript] = Time.time + tickRate;

        Debug.Log("Lingering dragon fire damaged player for " + damagePerTick);
    }

    // Stops tracking the player after they leave the fire area
    private void OnTriggerExit(Collider other)
    {
        PlayerScriptNew playerScript = other.GetComponent<PlayerScriptNew>();

        if (playerScript == null)
        {
            playerScript = other.GetComponentInParent<PlayerScriptNew>();
        }

        if (playerScript != null)
        {
            nextDamageTimeByPlayer.Remove(playerScript);
        }
    }
}