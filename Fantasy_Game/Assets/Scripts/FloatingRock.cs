using UnityEngine;

public class Bobbing : MonoBehaviour
{
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
    
        float newY = Mathf.Sin(Time.time * bobSpeed + randomPhase) * bobHeight;

        transform.position = new Vector3(startPosition.x, startPosition.y + newY, startPosition.z);
    }
}
