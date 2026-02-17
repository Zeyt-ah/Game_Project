using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombatScript : MonoBehaviour
{
    private PlayerScriptNew player;

    public BoxCollider _attackHitbox;

    [SerializeField] private float attackTime = 0.9f;


    public System.Action OnAttackStarted;



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
}
