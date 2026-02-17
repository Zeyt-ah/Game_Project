using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementScript : MonoBehaviour
{
    private PlayerScriptNew player;
    [Header("stats")]
    public float baseSpeed = 6f;
    public float sprintSpeed = 9f;
    public float smoothTime = 0.1f;
    public float jumpPower = 8f;
    public float gravity = -9.81f;
    public float gravityMultiplier = 3f;
    public float speedForFall = -11f;
    public int jumpCount = 1;
    public int maxJumpCount = 2;

    private Vector2 input;
    private Vector3 moveDir;
    private float velocityY;
    private float turnVelocity;
    private float speed = 6;
    private bool isSprinting = false;
    private float horizontalSpeed = 0;

    //for climbing
    private bool isClimbing = false;
    private bool isInClimbZone = false;
    public float climbSpeed = 3f;


    //events
    public System.Action OnJumpStarted;

    private void Awake()
    {
        player = GetComponent<PlayerScriptNew>();
    }

    private void Update()
    {
        if (player == null) return;
        if (!playerCanMove()) return;

        if (isInClimbZone && Keyboard.current.eKey.wasPressedThisFrame)
        {
            isClimbing = !isClimbing;
        }

        if (isClimbing)
        {
            Climb();
            return; //stops gravity and such for climbing
        }

        ApplyGravity();
        ApplyMovement();
        ApplyRotation();


        //checks to see if it should reset jump count
        if (player._characterController.isGrounded)
        {
            jumpCount = maxJumpCount;
        }
    }

    bool playerCanMove()
    {
        return !player.IsDead() && player.CanMove();
    }

    public void Move(InputAction.CallbackContext context)
    {

        input = context.ReadValue<Vector2>();
    }

    public void Sprint(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            speed = sprintSpeed;
            isSprinting = true;
        }

        else if (context.canceled)
        {
            speed = baseSpeed;
            isSprinting = false;
        }
    }

    //for animations
    public bool IsSprinting()
    {
        return isSprinting;
    }


    public void Jump(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        //removes a jump if you walked off a platform (so you only get maxJumpCount - 1 jumps)
        if (!player._characterController.isGrounded && jumpCount == maxJumpCount)
        {
            jumpCount -= 1;
        }
        if (jumpCount <= 0) return;
        if (!playerCanMove()) return;
        jumpCount -= 1;
        velocityY = jumpPower;

        //plays jump animation
        OnJumpStarted?.Invoke();
    }

    void ApplyMovement()
    {
        Vector3 camForward = player.cam.forward;
        Vector3 camRight = player.cam.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 move = camForward * input.y + camRight * input.x;
        move.Normalize();

        Vector3 finalMove = move * speed;
        finalMove.y = velocityY;

        player._characterController.Move(finalMove * Time.deltaTime);

        moveDir = move;

        horizontalSpeed = new Vector3(moveDir.x, 0f, moveDir.z).magnitude;
    }

    //for animations
    public float HorizontalSpeed()
    {
        return horizontalSpeed;
    }
    public bool IsGrounded()
    {
        return player._characterController.isGrounded;
    }

    void ApplyRotation()
    {
        if (input.sqrMagnitude < 0.01f) return;

        float targetAngle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
        float smoothAngle = Mathf.SmoothDampAngle(
            transform.eulerAngles.y,
            targetAngle,
            ref turnVelocity,
            smoothTime
        );

        transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
    }

    void ApplyGravity()
    {
        if (player._characterController.isGrounded && velocityY < 0f)
        {
            velocityY = -2f;
        }
        else
        {
            velocityY += gravity * gravityMultiplier * Time.deltaTime;
        }
    }

    //for falling animation
    public float VerticalVelocity()
    {
        return velocityY;
    }



    //climbing code
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ClimbableWallTag"))
        {
            isInClimbZone = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ClimbableWallTag"))
        {
            isInClimbZone = false;
            isClimbing = false;
        }
    }

    public void Climb()
    {
        velocityY = 0f;

        float verticalInput = input.y; // W/S control for climbing

        Vector3 climbMove = Vector3.up * verticalInput * climbSpeed;

        player._characterController.Move(climbMove * Time.deltaTime);
    }

    public bool IsClimbing()
    {
        return isClimbing;
    }
}