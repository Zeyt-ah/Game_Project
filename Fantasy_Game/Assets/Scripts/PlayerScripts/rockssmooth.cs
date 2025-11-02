using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MovingPlatformHandler : MonoBehaviour
{
    private CharacterController controller;
    private Transform currentPlatform;
    private Vector3 lastPlatformPosition;
    private bool isOnPlatform;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (isOnPlatform && currentPlatform != null)
        {
         
            Vector3 platformDelta = currentPlatform.position - lastPlatformPosition;

            controller.Move(platformDelta);

            lastPlatformPosition = currentPlatform.position;
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("MovingPlatform"))
        {
            if (currentPlatform != hit.collider.transform)
            {
                currentPlatform = hit.collider.transform;
                lastPlatformPosition = currentPlatform.position;
                isOnPlatform = true;
            }
        }
    }

    void OnControllerExit(Collider other)
    {
        if (other.transform == currentPlatform)
        {
            isOnPlatform = false;
            currentPlatform = null;
        }
    }

    void OnControllerColliderHitExit(ControllerColliderHit hit)
    {
       
        if (hit.collider.CompareTag("MovingPlatform") == false)
        {
            isOnPlatform = false;
            currentPlatform = null;
        }
    }
}
