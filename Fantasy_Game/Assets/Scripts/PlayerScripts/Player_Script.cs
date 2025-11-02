using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class Player_Script : MonoBehaviour
{

    [SerializeField] private float speed = 6f;
    [SerializeField] private float sprintSpeedMult = 1.5f;

    [SerializeField] private float smoothTime = 0.1f;
    [SerializeField] private float jumpPower = 8f;

    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float gravityMultiplier = 3f;

    public Transform cam;

    private CharacterController _characterController;
    private Animator _animator;
    public BoxCollider pickupHitbox;
    public BoxCollider _attackHitbox;

    public CapsuleCollider hurtBox;

    public GameManagerScript gameManager;

    private Vector2 _input;
    private Vector3 _moveDir;
    private float _velocityY;
    private float _currentTurnVelocity;

    private int health;
    private bool canTakeDmg = true;
    [SerializeField] private float IFrameTime = 1;

    [SerializeField] private float attackTime = 2;
    //used to trigger a death animation and death screen
    private bool canMove = true;
    private bool canAttack = true;
    private bool dead = false;
    private bool isInteracting = false;

    private int coinCount = 0;
    private int crystalCount = 0;
    public int eggCount = 0;
    private bool onPickupable = false;
    public bool pickedUp = false;
    private bool alreadyFalling = false;



    private void Awake()
    {

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _animator = GetComponentInChildren<Animator>();
        _characterController = GetComponent<CharacterController>();
        
        //sets base health
        health = 100;
    }

    private void Update()
    {
        //stops everything on player death
        if (dead || !canMove) return;
        ApplyGravity();
        ApplyRotation();
        ApplyMovement();

    }

    private void ApplyMovement()
    {
        // cam relative movement
        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        //cam relative direction
        Vector3 move = camForward * _input.y + camRight * _input.x;
        move.Normalize();

        // Apply movement and gravity
        Vector3 finalMove = move * speed;
        finalMove.y = _velocityY;

        _characterController.Move(finalMove * Time.deltaTime);

        _moveDir = move;


        //for animations

        //checks speed to see if running animation should play
        float horizontalSpeed = new Vector3(_moveDir.x, 0f, _moveDir.z).magnitude;
        _animator.SetFloat("Speed", horizontalSpeed);

        //checks that your grounded for animator
        _animator.SetBool("Grounded", _characterController.isGrounded);
    }

    private void ApplyRotation()
    {
        // Only rotate when there's movement input
        if (_input.sqrMagnitude < 0.01f) return;

        float targetAngle = Mathf.Atan2(_moveDir.x, _moveDir.z) * Mathf.Rad2Deg;
        float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y,targetAngle,ref _currentTurnVelocity,smoothTime);

        transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
    }

    private void ApplyGravity()
    {
        if (_characterController.isGrounded && _velocityY < 0f)
        {
            _velocityY = -2f; // small downward force to stay grounded
            alreadyFalling = false;
        }
        else
        {
            _velocityY += gravity * gravityMultiplier * Time.deltaTime;
        }

        if (_velocityY <= -7f && !alreadyFalling)
        {
            alreadyFalling = true;
            _animator.SetBool("isFalling", true);
        }
        else
        {
            _animator.SetBool("isFalling", false);

        }
    }


    public void Move(InputAction.CallbackContext context)
    {
        _input = context.ReadValue<Vector2>();
    }

    public void Sprint(InputAction.CallbackContext context)
    {
        if (context.started)
        { 
            speed = speed * sprintSpeedMult;
            _animator.SetBool("isSprinting", true);
        }

        else if (context.canceled)
            {
                speed = speed / sprintSpeedMult;
                _animator.SetBool("isSprinting", false);
            }
    }


    public void Jump(InputAction.CallbackContext context)
    {
        if (!canMove) return;
        if (!context.started) return;
        if (!_characterController.isGrounded) return;

        _velocityY = jumpPower;

        //plays jump animation
        _animator.SetTrigger("Jump");
    }

    public void Attack(InputAction.CallbackContext context)
    {
        if (!context.started || !canAttack || dead) return;
        if (_characterController.isGrounded) 
        {
            canMove = false;
            canAttack = false;
            _animator.SetTrigger("Attack");
            StartCoroutine(Attacking());
        }
        
        
    }

    IEnumerator Attacking()
    {
        StartCoroutine(AttackHitboxOn());
        yield return new WaitForSeconds(attackTime);
        canMove = true;
        canAttack = true;
        _attackHitbox.enabled = false;
    }

    //waits a bit to enable attack hitbox
    IEnumerator AttackHitboxOn()
    {
        yield return new WaitForSeconds(0.6f);
        _attackHitbox.enabled = true;
    }
    //will handle all collision for player running into something
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Enemy") && canTakeDmg)
        {
            health -= 25;
            StartCoroutine(IFrames());
            _animator.SetTrigger("TookDamage");
            gameManager.UpdateHealth(25);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Enemy") && canTakeDmg && !dead)
        {
            health -= 25;

            StartCoroutine(IFrames());
            _animator.SetTrigger("TookDamage");
            gameManager.UpdateHealth(25);
        }

        else if (other.CompareTag("Crystal"))
        {
            onPickupable = true;
            if (isInteracting && !pickedUp)
            {
                pickedUp = true;
                crystalCount++;
                gameManager.UpdateScore(50);
            }
        }
        else if (other.CompareTag("Egg"))
        {

            onPickupable = true;
            if (isInteracting && !pickedUp)
            {
                pickedUp = true;
                eggCount++;
                gameManager.UpdateEggs();

            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Crystal"))
        {
            onPickupable = false;
        }
        else if (other.CompareTag("Egg"))
        {
            onPickupable = false;
        }
    }
    IEnumerator IFrames()
    {
        canTakeDmg = false;
        canMove = false;
        if (health <= 0)
        {
            Death();
        }
        //stun
        yield return new WaitForSeconds(0.6f);
        canMove = true;
        yield return new WaitForSeconds(IFrameTime);
        canTakeDmg = true;
    }


    public void Death()
    {
        canMove = false;
        dead = true;
        _animator.SetTrigger("Death");
        _animator.SetBool("Dead", true);
        gameManager.GameOver();
    }


    //coin detection
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Coin"))
        {
            coinCount++;
            gameManager.UpdateScore(20);
        }
    }

          
    //checks if player is interacting
    public void Interact(InputAction.CallbackContext context)
    {
        if (context.started && onPickupable)
        {
            isInteracting = true;
            _animator.SetTrigger("isGathering");
            canMove = false;
            pickupHitbox.enabled = true;
            StartCoroutine(GatherTime());   
        }

        else if (context.canceled)
        {
            isInteracting = false;
        }
    }

    IEnumerator GatherTime()
    {
        yield return new WaitForSeconds(1.5f);
        canMove = true;
        pickupHitbox.enabled = false;
    }
}