using UnityEditor.UI;
using UnityEngine;

public class CoinScript : MonoBehaviour
{
    public GameObject coin;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(coin);
        }
    }
}
