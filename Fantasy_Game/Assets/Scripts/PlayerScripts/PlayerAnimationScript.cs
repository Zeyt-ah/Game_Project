using TMPro;
using UnityEngine;

public class PlayerAnimationScript : MonoBehaviour
{
    private PlayerScriptNew playerScript;
    private PlayerMovementScript movementScript;
    private PlayerCombatScript combatScript;
    private PlayerInteractionScript interactionScript;
    private Animator animator;



    void Start()
    {
        playerScript = GetComponent<PlayerScriptNew>();
        movementScript = GetComponent<PlayerMovementScript>();
        combatScript = GetComponent<PlayerCombatScript>();
        interactionScript = GetComponent<PlayerInteractionScript>();

        animator = GetComponentInChildren<Animator>();

        //event listeners
        combatScript.OnAttackStarted += AttackAnimation;
        interactionScript.OnInteractStarted += InteractAnimation;
        movementScript.OnJumpStarted += JumpAnimation;
        movementScript.OnClimbStarted += ClimbAnimation;
        movementScript.OnDodgeStarted += DodgeAnimation;
    }

    private void Update()
    {
        //checks if sprinting is true in movement script
        animator.SetBool("isSprinting", movementScript.IsSprinting());
        //checks for horizontal speed
        animator.SetFloat("Speed", movementScript.HorizontalSpeed());
        //checks if the player is grounded for the running animation
        animator.SetBool("Grounded", movementScript.IsGrounded());

        bool falling = !movementScript.IsGrounded() && movementScript.VerticalVelocity() < -1.5;
        animator.SetBool("isFalling", falling);

        animator.SetBool("isClimbingBool", movementScript.IsClimbing());

        animator.SetBool("isDodging", movementScript.IsDodging());
    }



    private void AttackAnimation()
    {
        animator.SetTrigger("Attack");
    }

    private void InteractAnimation()
    {
        animator.SetTrigger("isGathering");
    }

    private void JumpAnimation()
    {
        animator.SetTrigger("Jump");
    }

    private void ClimbAnimation()
    {
        animator.SetTrigger("isClimbing");
    }

    private void DodgeAnimation()
    {
        Vector2 dodgeDir = movementScript.DodgeDirection();
        animator.SetFloat("DodgeX", dodgeDir.x);
        animator.SetFloat("DodgeY", dodgeDir.y);

        animator.SetBool("IsDodging", true);
        animator.SetTrigger("Dodge");
    }

    

    private void OnDestroy()
    {
        combatScript.OnAttackStarted -= AttackAnimation;
        interactionScript.OnInteractStarted -= InteractAnimation;
        movementScript.OnJumpStarted -= JumpAnimation;
        movementScript.OnDodgeStarted -= DodgeAnimation;

    }
}
