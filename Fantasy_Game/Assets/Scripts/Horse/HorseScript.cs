using UnityEngine;
using System.Collections;

public class HorseScript : MonoBehaviour
{
    public float walkSpeed = 10f;
    public float gallopSpeed = 30f;
    public float jumpPower = 8f;
    public float stamina = 100f;
    public float staminaDrain = 10f;
    public float staminaRecovery = 5f;

    private CharacterController controller;
    private Vector3 moveDir;
    private float verticalVelocity;

    public bool isMounted = false;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public void Move(Vector2 input, Transform cam,bool sprinting)
    {
        if (!isMounted) return;
        float currentSpeed;

        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;
        camForward.y = 0; camRight.y = 0;
        camForward.Normalize(); camRight.Normalize();

        Vector3 direction = camForward * input.y + camRight * input.x;
        direction = Vector3.ClampMagnitude(direction, 1f);

        if (stamina > 0 && sprinting)
        {
            currentSpeed = gallopSpeed;
        }
        else
        {
            currentSpeed = walkSpeed;
        }
        moveDir = direction * currentSpeed;
        moveDir.y = verticalVelocity;

        controller.Move(moveDir * Time.deltaTime);

        // Gravity
        if (!controller.isGrounded) verticalVelocity += Physics.gravity.y * Time.deltaTime;
        else
        {
            verticalVelocity = -1f;
        }

        if (currentSpeed == gallopSpeed)
        {
            stamina -= staminaDrain * Time.deltaTime;
        }
        else if (stamina < 100)
        {
            stamina += staminaRecovery * Time.deltaTime;
            stamina = Mathf.Min(stamina, 100);
        }

        // Rotate horse
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
        }
    }

    public void Jump()
    {
        if (!isMounted || !controller.isGrounded) return;
        verticalVelocity = jumpPower;
    }
}