using System.Collections.Generic;
using UnityEngine;

public class DragonFireBreathHitbox : MonoBehaviour
{
    [SerializeField] private int damagePerTick = 5;
    [SerializeField] private float tickRate = 0.5f;

    private bool isActive;
    private readonly Dictionary<PlayerScriptNew, float> nextDamageTimeByPlayer = new();

    // Enables or disables the fire breath hitbox
    public void SetActiveHitbox(bool active)
    {
        isActive = active;
        gameObject.SetActive(active);

        if (!active)
        {
            nextDamageTimeByPlayer.Clear();
        }
    }

    // Damages the player repeatedly while they stay inside the fire breath hitbox
    private void OnTriggerStay(Collider other)
    {
        if (!isActive)
        {
            return;
        }

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

        Debug.Log("Dragon fire breath damaged player for " + damagePerTick);
    }

    // Removes the player from the damage tracking list when they leave the flame
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