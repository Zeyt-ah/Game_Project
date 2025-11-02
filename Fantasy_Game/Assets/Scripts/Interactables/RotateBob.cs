using UnityEngine;

public class RotateBob : MonoBehaviour
{
    [Header("Rotation speed (degrees per second)")]
    public float rotationSpeed = 90f;

    [Header("Vertical bobbing movement amount")]
    public float bobAmount = 0.15f;

    [Header("How fast the bobbing movement happens")]
    public float bobSpeed = 2f;

    private Vector3 startPos;

    private void Start()
    {
        // Store the starting position so we bob up/down relative to it
        startPos = transform.position;
    }

    private void Update()
    {
        // Rotate around the Y axis
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);

        // Apply bobbing up/down movement
        float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
