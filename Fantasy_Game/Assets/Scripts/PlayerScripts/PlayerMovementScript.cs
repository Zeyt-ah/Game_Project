using System;
using System.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.UI;

public class PlayerMovementScript : MonoBehaviour
{
    private PlayerScriptNew player;
    private PlayerCombatScript playerCombat;
    public Image staminaBar;
    public GameObject staminaBarBackground;

    [Header("stats")]
    public float baseSpeed = 6f;
    public float sprintSpeed = 9f;
    public float iceBaseSpeed = 4f;
    public float iceSprintSpeed = 7f;
    public float smoothTime = 0.1f;
    public float jumpPower = 8f;
    public float gravity = -9.81f;
    public float gravityMultiplier = 3f;
    public float speedForFall = -11f;
    public int jumpCount = 1;
    public int maxJumpCount = 2;

    [SerializeField]
    public Vector2 input;
    private Vector3 moveDir;
    private float velocityY;
    private float turnVelocity;
    private float speed = 6;
    private bool isSprinting = false;
    private float horizontalSpeed = 0;
    private float maxFallVelocity = 0;
    public bool disableFallDamage = false;

    //for dodging
    private float dodgeSpeed = 8;
    private bool isDodging = false;
    private bool canDodge = true;
    private Vector3 dodgeDirection;

    //for climbing
    private bool isClimbing = false;
    private bool isInClimbZone = false;
    private float climbSpeed = 3f;
    private float totalClimbStamina = 100f;
    private float climbStamina = 100f;
    //Numbers can be tweaked
    public float staminaDrain = 50f;
    public float staminaRecovery = 100f;
    private Transform currentWall;


    //for horse
    public HorseScript currentHorse;
    public float mountDistance = 2f;
    private bool isMounted = false;

    //events
    public System.Action OnJumpStarted;
    public System.Action OnClimbStarted;
    public System.Action OnDodgeStarted;
    public System.Action MountedHorse;


    private void Awake()
    {
        player = GetComponent<PlayerScriptNew>();
        playerCombat = GetComponent<PlayerCombatScript>();
    }

    private void Update()
    {
        if (player.IsDead()) Dismount();
        if (player == null) return;
        if (!playerCanMove()) return;
        UpdateStaminaBar();
        if (isInClimbZone && Keyboard.current.eKey.wasPressedThisFrame && !isRidingHorse())
        {
            if (!isClimbing)
            {
                staminaBarBackground.SetActive(true);
                isClimbing = true;
                OnClimbStarted?.Invoke();
            }
            else
            {
                isClimbing = false;
            }
        }
        if(!isClimbing) staminaBarBackground.SetActive(false);

        if (isClimbing && climbStamina > 0)
        {
            jumpCount = 0;
            Climb();
            return; //stops gravity and such for climbing
        }
        else
        {
            isClimbing = false;
        }

        if (isMounted && currentHorse != null)
        {
            currentHorse.Move(input, player.cam, isSprinting);
            return;
        }


        ApplyGravity();
        ApplyMovement();
        ApplyRotation();


        //checks to see if it should reset jump count
        if (player._characterController.isGrounded)
        {
            jumpCount = maxJumpCount;
            climbStamina += staminaRecovery * Time.deltaTime; // gradual recovery
            climbStamina = Mathf.Min(climbStamina, 100f);
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
            if (player.IsIcey())
            {
                speed = iceSprintSpeed;
            }
        }

        else if (context.canceled)
        {
            speed = baseSpeed;
            isSprinting = false;
            if (player.IsIcey())
            {
                speed = iceBaseSpeed;
            }
        }
    }

    //for animations + horse
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


        // Cancel fall damage since player jumped mid-air
        maxFallVelocity = 0f;
        
        //plays jump animation
        OnJumpStarted?.Invoke();
    }

    void ApplyMovement()
    {
        Vector3 move = GetCameraRelativeInputDirection();

        Vector3 finalMove = move * speed;
        if (player.IsIcey())
        {
            finalMove = move * iceBaseSpeed;
        }
        //diff movement for dodging
        if (IsDodging())
        {
            finalMove = dodgeDirection * dodgeSpeed;
        }

        finalMove.y = velocityY;

        player._characterController.Move(finalMove * Time.deltaTime);
        //player._characterController.Move(new Vector3(input.x * speed, velocityY, input.y * speed) * Time.deltaTime);

        moveDir = GetCameraRelativeInputDirection();

        horizontalSpeed = new Vector3(moveDir.x, 0f, moveDir.z).magnitude;
    }

    Vector3 GetCameraRelativeInputDirection()
    {
        Vector3 camForward = player.cam.forward;
        Vector3 camRight = player.cam.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 direction = camForward * input.y + camRight * input.x;
        return Vector3.ClampMagnitude(direction, 1f);
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
        moveDir = GetCameraRelativeInputDirection();
        if (input.sqrMagnitude < 0.01f || IsDodging()) return;

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
            if (!disableFallDamage && maxFallVelocity < -25f)
            {
                int damage = 0;

                if (maxFallVelocity > -30f)
                {
                    // Moderate fall damage
                    damage = -(int)Mathf.Floor(maxFallVelocity);
                }
                else
                {
                    // Hard fall damage, capped at 100
                    damage = Mathf.Min(-(int)Mathf.Floor(maxFallVelocity) * 2, 100);
                }

                player.TakeDamage(damage);
            }

            // Reset for next fall
            maxFallVelocity = 0f;
            velocityY = -2f;
        }
        else
        {
            velocityY += gravity * gravityMultiplier * Time.deltaTime;

            //Track downward velocity
            if (velocityY < maxFallVelocity)
            {
                maxFallVelocity = velocityY;
            }
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
            currentWall = other.transform;
        }
    }

    //stops climbing on exiting climbable surface
    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ClimbableWallTag"))
        {
            isInClimbZone = false;
            isClimbing = false;
            currentWall = null;

        }
    }

    public void Climb()
    {
        if (currentWall == null) return;

        velocityY = 0f;

        float verticalInput = input.y;
        float horizontalInput = input.x;

        // Wall normal
        Vector3 wallNormal = currentWall.forward;

        // Direction along wall surface
        Vector3 wallRight = Vector3.Cross(wallNormal, Vector3.up).normalized;
        Vector3 wallUp = Vector3.up;

        Vector3 climbMove = (wallUp * verticalInput + wallRight * horizontalInput);

        // Prevents faster diagonal movement
        if (climbMove.magnitude > 1f)
            climbMove.Normalize();

        climbMove *= climbSpeed;

        player._characterController.Move(climbMove * Time.deltaTime);

        // Rotate player to face wall
        Quaternion targetRotation = Quaternion.LookRotation(-wallNormal);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        
        climbStamina -= staminaDrain * Time.deltaTime;
    }

    private void UpdateStaminaBar()
    {
        staminaBar.fillAmount = (float) climbStamina/ totalClimbStamina;
    }

    public bool IsClimbing()
    {
        return isClimbing;
    }


    public void Dodge(InputAction.CallbackContext context)
    {
        if (!context.started || !IsGrounded() || IsDodging() || !canDodge || !player.CanMove()) return;

        Vector3 inputDirection = GetCameraRelativeInputDirection();
        if (inputDirection.sqrMagnitude < 0.01f)
        {
            dodgeDirection = -player.cam.transform.forward;
        }
        else
        {
            dodgeDirection = inputDirection.normalized;
        }

        
        player.SetInvulnerable(true);
        player.DisableAttack();
        isDodging = true;
        canDodge = false;
        OnDodgeStarted?.Invoke();
        StartCoroutine(DodgingTimer());
        StartCoroutine(DodgingCooldown());

        
    }

    IEnumerator DodgingTimer()
    {
       yield return new WaitForSeconds(0.6f);
       isDodging = false;
       player.EnableAttack();
       player.SetInvulnerable(false);
    }

    IEnumerator DodgingCooldown()
    {
        yield return new WaitForSeconds(1f);
        canDodge = true;
    }

    public bool IsDodging()
    {
        return isDodging;
    }


    public Vector2 DodgeDirection()
    {
        Vector3 localDir = transform.InverseTransformDirection(dodgeDirection);
        return new Vector2(localDir.x, localDir.z);
    }


    public void Mount(InputAction.CallbackContext context)
    {
        if (!context.started || player.IsDead()) return;

        if (!isMounted)
        {
            TryMount();
        }
        else
        {
            Dismount();
        }
    }

    void TryMount()
    {
        if (currentHorse == null) return;

        float distance = Vector3.Distance(transform.position, currentHorse.transform.position);
        if (distance > mountDistance) return;

        //unequips the sword when mounting the horse
        if (playerCombat.SwordEquipped())playerCombat.UnequipSword();


        player.DisableAttack();

        // Mount horse
        isMounted = true;
        currentHorse.isMounted = true;
        transform.rotation = currentHorse.transform.rotation;
        transform.position = currentHorse.transform.position + new Vector3(0, 1.5f, 0);
        transform.SetParent(currentHorse.transform);

        // Position player on horse
        transform.position = currentHorse.transform.position + new Vector3(0, 1.48f, 0); // adjust height

        MountedHorse?.Invoke();

    }

    void Dismount()
    {
        if (currentHorse == null) return;

        player.EnableAttack();
        isMounted = false;
        currentHorse.isMounted = false;

        // Detach player
        transform.SetParent(null);

        Vector3 dismountDir = currentHorse.transform.right;

        Vector3 dismountOffset = dismountDir * 2f + Vector3.up * 0.5f;
        dismountOffset.y -= 0.5f;
        transform.position = currentHorse.transform.position + dismountOffset;

    }


    public bool isRidingHorse()
    {
        return isMounted;
    }
}