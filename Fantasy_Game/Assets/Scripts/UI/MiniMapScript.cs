using UnityEngine;

public class MiniMapScript : MonoBehaviour
{
    public Transform player;
    public float offset = 80;

    private void LateUpdate()
    {
        Vector3 newPosition = player.position;
        newPosition.y = Mathf.Max(player.position.y + 30f, offset);
        transform.position = newPosition;
    }
}
