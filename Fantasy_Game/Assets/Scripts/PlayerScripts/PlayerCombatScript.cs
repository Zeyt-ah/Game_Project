using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombatScript : MonoBehaviour
{
    private PlayerScriptNew player;

    public BoxCollider _attackHitbox;
    public BoxCollider swordAttackHitbox;

    public GameObject fireSpell;
    public GameObject sword;

    [SerializeField] private float attackTime = 0.9f;
    [SerializeField] private float swordAttackTime = 0.9f;
    [SerializeField] private float spellTime = 2f;
    private bool swordEquipped = false;

    public System.Action OnAttackStarted;
    public System.Action OnCastStarted;
    public System.Action OnSwordAttackStarted;



    private void Awake()
    {
        player = GetComponent<PlayerScriptNew>();
    }


    public void Attack(InputAction.CallbackContext context)
    {
        if (!context.started || !player.CanAttack() || player.IsDead()) return;
        if (!player._characterController.isGrounded) return;
        //checks if the sword is currently equipped
        if (!swordEquipped)
        {
            player.DisableMovement();
            OnAttackStarted?.Invoke();

            StartCoroutine(Attacking());
        }
        else
        {
            player.DisableMovement();
            OnSwordAttackStarted?.Invoke();

            StartCoroutine(SwordAttacking());
        }

    }

    public void CastSpell(InputAction.CallbackContext context)
    {
        if (!context.started || !player.CanAttack() || player.IsDead()) return;
        if (player._characterController.isGrounded)
        {
            player.DisableMovement();
            OnCastStarted?.Invoke();

            StartCoroutine(CastingSpell());
        }
    }

    IEnumerator Attacking()
    {
        StartCoroutine(AttackHitboxOn());
        yield return new WaitForSeconds(attackTime);
        player.EnableMovement();
        _attackHitbox.enabled = false;
    }

    //waits a bit to enable attack hitbox
    IEnumerator AttackHitboxOn()
    {
        yield return new WaitForSeconds(0.6f);
        if(!player.TookDamageRecently())_attackHitbox.enabled = true;
    }


    //for sword
    IEnumerator SwordAttacking()
    {
        StartCoroutine(SwordAttackHitboxOn());
        yield return new WaitForSeconds(swordAttackTime);
        player.EnableMovement();
        swordAttackHitbox.enabled = false;
    }

    //waits a bit to enable attack hitbox
    IEnumerator SwordAttackHitboxOn()
    {
        yield return new WaitForSeconds(0.6f);
        if(!player.TookDamageRecently())swordAttackHitbox.enabled = true;
    }

    //for spells
    IEnumerator CastingSpell()
    {
        StartCoroutine(SpellHitboxOn());
        yield return new WaitForSeconds(spellTime);
        player.EnableMovement();
    }

    //waits a bit to enable spell hitbox
    IEnumerator SpellHitboxOn()
    {
        yield return new WaitForSeconds(1.35f);
        //makes sure you dont cast a spell mid hit.
        if (!player.TookDamageRecently())
        {
            GameObject spell = Instantiate(fireSpell, transform.position, transform.rotation);
            StartCoroutine(SpellMovement(spell));
        }
    }
    IEnumerator SpellMovement(GameObject spell)
    {
        float speed = 15f;
        float duration = 1.5f;
        float elapsed = 0f;

        Vector3 moveDirection = transform.forward;

        while (elapsed < duration)
        {
            spell.transform.position += moveDirection * speed * Time.deltaTime; // move forward
            elapsed += Time.deltaTime;
            yield return null;
        }
        Destroy(spell); // remove spell after movement
    }


    //switching between weapons (hand or sword)
    public void SwappingWeapons(InputAction.CallbackContext context)
    {
        //makes sure you can only swap if not attacking and once per scroll tick
        if (!context.performed || !player.CanAttack()) return;
        Vector2 scroll = context.ReadValue<Vector2>();
        
        if (scroll.y != 0)
        {
            if (!sword.activeSelf)
            {
                swordEquipped = true;
                sword.SetActive(true);
            }
            else
            {
                swordEquipped = false;
                sword.SetActive(false);
            }
        }

    }

    public bool SwordEquipped()
    {
        return swordEquipped;
    }

    public void UnequipSword()
    {
        swordEquipped = false;
        sword.SetActive(swordEquipped);
    }
}
