using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombatScript : MonoBehaviour
{
    private PlayerScriptNew player;

    public BoxCollider _attackHitbox;

    public GameObject fireSpell;

    [SerializeField] private float attackTime = 0.9f;
    [SerializeField] private float spellTime = 2f;

    public System.Action OnAttackStarted;
    public System.Action OnCastStarted;



    private void Awake()
    {
        player = GetComponent<PlayerScriptNew>();
    }


    public void Attack(InputAction.CallbackContext context)
    {
        if (!context.started || !player.CanAttack() || player.IsDead()) return;
        if (player._characterController.isGrounded)
        {
            player.DisableMovement();
            OnAttackStarted?.Invoke();

            StartCoroutine(Attacking());
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
        _attackHitbox.enabled = true;
    }


    //for spells
    IEnumerator CastingSpell()
    {
        StartCoroutine(SpellHitboxOn());
        yield return new WaitForSeconds(spellTime);
        player.EnableMovement();
    }

    //waits a bit to enable attack hitbox
    IEnumerator SpellHitboxOn()
    {
        yield return new WaitForSeconds(1.3f);
        GameObject spell = Instantiate(fireSpell, transform.position, transform.rotation);
        StartCoroutine(SpellMovement(spell));
    }
    IEnumerator SpellMovement(GameObject spell)
    {
        float speed = 15f;
        float duration = 4f;
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
       
        
}
