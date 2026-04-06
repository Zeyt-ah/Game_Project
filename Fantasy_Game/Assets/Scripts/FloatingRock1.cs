using UnityEngine;

public class moving : MonoBehaviour
{
    public float movedistance = 0.5f;    
    public float moveSpeed = 1f;
    public float bobHeight = 0.5f;
    public float bobSpeed = 1f;

    private Vector3 startPosition;
    private float randomPhase;       
    void Start()
    {
        
        startPosition = transform.position;

        
        randomPhase = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
    
        float newx = Mathf.Sin(Time.time * moveSpeed + randomPhase) * movedistance;
        float newY = Mathf.Sin(Time.time * bobSpeed + randomPhase) * bobHeight;

        transform.position = new Vector3(startPosition.x + newx, startPosition.y + newY, startPosition.z);
    }
}
