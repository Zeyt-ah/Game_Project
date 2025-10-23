using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class Player_Script : MonoBehaviour
{

    [SerializeField] private float speed = 6f;
    [SerializeField] private float smoothTime = 0.1f;
    [SerializeField] private float jumpPower = 8f;

    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float gravityMultiplier = 3f;

    public Transform cam;

    private CharacterController _characterController;
    private Animator _animator;

    private Vector2 _input;
    private Vector3 _moveDir;
    private float _velocityY;
    private float _currentTurnVelocity;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
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
        }
        else
        {
            _velocityY += gravity * gravityMultiplier * Time.deltaTime;
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        _input = context.ReadValue<Vector2>();
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (!_characterController.isGrounded) return;

        _velocityY = jumpPower;

        //plays jump animation
        _animator.SetTrigger("Jump");
    }
}