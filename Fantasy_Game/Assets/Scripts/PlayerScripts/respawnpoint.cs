using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerScriptNew player = other.GetComponent<PlayerScriptNew>();

        if (player != null)
        {
            player.SetCheckpoint(transform);
        }
    }
}