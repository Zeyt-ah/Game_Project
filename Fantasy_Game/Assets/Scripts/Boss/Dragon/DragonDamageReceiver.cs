using UnityEngine;

public class DragonDamageReceiver : MonoBehaviour
{
    [SerializeField] private DragonBossController dragonBossController;
    [SerializeField] private float swordDamage = 20f;
    [SerializeField] private float spellDamage = 15f;
    [SerializeField] private float fistDamage = 5f;

    // Gets the dragon boss controller if it has not been assigned
    private void Awake()
    {
        if (dragonBossController == null)
        {
            dragonBossController = GetComponentInParent<DragonBossController>();
        }
    }

    // Applies damage when the players attack hitbox touches the dragon
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Dragon damage hitbox touched: " + other.name + " with tag: " + other.tag);
        
        if (dragonBossController == null)
        {
            return;
        }

        if (other.CompareTag("SwordAttackHitboxTag"))
        {
            dragonBossController.TakeDamage(swordDamage);
        }
        else if (other.CompareTag("PlayerSpellHitbox"))
        {
            dragonBossController.TakeDamage(spellDamage);
        }
        else if (other.CompareTag("AttackHitboxTag"))
        {
            dragonBossController.TakeDamage(fistDamage);
        }
    }
}