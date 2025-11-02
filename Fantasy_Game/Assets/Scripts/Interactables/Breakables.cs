using UnityEngine;
using System.Collections;

public class Breakables : MonoBehaviour
{
    public GameObject brokenCrate;
    private bool isBroken = false;

    private void OnTriggerEnter(Collider other)
    {

        if (isBroken) return;
        if (!other.CompareTag("AttackHitboxTag")) return;

        Break();
    }

    private void Break()
    {
        isBroken = true;

        if (brokenCrate != null)
        {
            GameObject brokenBox = Instantiate(brokenCrate, transform.position, transform.rotation);
            brokenBox.transform.localScale = transform.localScale;
        }

        Destroy(gameObject);
    } 
}
