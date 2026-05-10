using System.Collections.Generic;
using UnityEngine;

public class JumpGravityModifier : MonoBehaviour
{
    [Header("Player Stat Modifiers")]
    public float jumpPowerMultiplier = 1.5f;
    public float gravityMultiplier = 0.5f;

    private PlayerMovementScript playerMovement;

    // Track all zones the player is inside
    private static HashSet<JumpGravityModifier> activeZones = new HashSet<JumpGravityModifier>();

    // Store original player stats
    private static float originalJumpPower;
    private static float originalGravityMultiplier;

    private static bool buffApplied = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var movement = other.GetComponent<PlayerMovementScript>();
        if (movement == null) return;

        playerMovement = movement;

        activeZones.Add(this);

        // Only apply buff once
        if (!buffApplied)
        {
            ApplyBuff();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (playerMovement == null) return;

        activeZones.Remove(this);
    }

    private void Update()
    {
        if (playerMovement == null) return;

        // Remove buff only if player is grounded and outside all zones
        if (buffApplied && activeZones.Count == 0 && playerMovement.IsGrounded())
        {
            RemoveBuff();
        }
    }

    private void ApplyBuff()
    {
        if (playerMovement == null || buffApplied) return;

        // Store the ORIGINAL stats only once
        originalJumpPower = playerMovement.jumpPower;
        originalGravityMultiplier = playerMovement.gravityMultiplier;

        // Apply buffed values based on original stats
        playerMovement.jumpPower = originalJumpPower * jumpPowerMultiplier;
        playerMovement.gravityMultiplier = originalGravityMultiplier * gravityMultiplier;

        buffApplied = true;
    }

    private void RemoveBuff()
    {
        if (!buffApplied || playerMovement == null) return;

        // Restore original stats
        playerMovement.jumpPower = originalJumpPower;
        playerMovement.gravityMultiplier = originalGravityMultiplier;

        buffApplied = false;
        playerMovement = null;
    }
}