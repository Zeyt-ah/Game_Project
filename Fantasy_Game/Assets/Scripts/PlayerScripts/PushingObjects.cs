using UnityEngine;

public class PushingObjects : MonoBehaviour
{

    public float pushForce = 5f;   
    public float torqueForce = 5f; 

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!hit.collider.CompareTag("Pushable")) return;

        Rigidbody body = hit.collider.attachedRigidbody;
        if (body == null || body.isKinematic) return;

        
        if (hit.moveDirection.y < -0.3f) return;

        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z).normalized;

        body.AddForce(pushDir * pushForce, ForceMode.VelocityChange);

        Vector3 torqueAxis = Vector3.Cross(Vector3.up, pushDir).normalized;
        body.AddTorque(torqueAxis * torqueForce, ForceMode.VelocityChange);
    }
}
