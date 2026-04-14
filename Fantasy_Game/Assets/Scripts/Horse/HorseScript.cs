using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class HorseScript : MonoBehaviour
{
    public float walkSpeed = 10f;
    public float gallopSpeed = 30f;
    public float jumpPower = 8f;
    public float stamina = 100f;
    public float staminaDrain = 10f;
    public float staminaRecovery = 5f;

    public float gravity = -9.81f;
    public float gravityMultiplier = 3f;

    private CharacterController controller;
    private Animator animator;
    private Vector3 moveDir;
    private float verticalVelocity;

    //to have a threshold that needs to be met before you can sprint again
    private bool enoughStaminaRecovered = true;
    public bool isMounted = false;





    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    public void Update()
    {
        ApplyGravity();
        Vector3 finalMove = moveDir;
        if (!isMounted)
        {
            finalMove.x = 0;
            finalMove.z = 0;
        }
        finalMove.y = verticalVelocity;

        controller.Move(finalMove * Time.deltaTime);



        if (stamina < 100 && !isMounted)
        {
            stamina += staminaRecovery * Time.deltaTime;
            stamina = Mathf.Min(stamina, 100);
        }
        if (!isMounted)
        {
            animator.SetBool("isMoving", false);
        }
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Jump();
        }
    }



    public void Move(Vector2 input, Transform cam, bool sprinting)
    {
        if (!isMounted) return;
        float currentSpeed;

        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;
        camForward.y = 0; camRight.y = 0;
        camForward.Normalize(); camRight.Normalize();

        Vector3 direction = camForward * input.y + camRight * input.x;
        direction = Vector3.ClampMagnitude(direction, 1f);


        //for animations
        CheckIfMoving(direction);
        CheckIfSprinting(sprinting);

        if (stamina > 0 && sprinting && enoughStaminaRecovered)
        {
            currentSpeed = gallopSpeed;
        }
        else
        {
            currentSpeed = walkSpeed;
        }
        moveDir = direction * currentSpeed;
        moveDir.y = verticalVelocity;

        if (currentSpeed == gallopSpeed && enoughStaminaRecovered)
        {
            stamina -= staminaDrain * Time.deltaTime;
        }
        else if (stamina < 100)
        {
            stamina += staminaRecovery * Time.deltaTime;
            stamina = Mathf.Min(stamina, 100);
            enoughStaminaRecovered = (stamina > 10);
        }

        // Rotate horse
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
        }
    }


    public void ApplyGravity()
    { 
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * gravityMultiplier * Time.deltaTime;
        }
    }

    public void Jump()
    {
        if (!isMounted || !controller.isGrounded) return;
        verticalVelocity = jumpPower;
        animator.SetTrigger("Jump");
    }












    public void CheckIfMoving(Vector3 moveDirection)
    {
        bool moving = !(moveDirection.x == 0f && moveDirection.z == 0f);
        animator.SetBool("isMoving", moving);
    }

    public void CheckIfSprinting(bool isSprinting)
    {
        animator.SetBool("isSprinting", isSprinting);
    }


}