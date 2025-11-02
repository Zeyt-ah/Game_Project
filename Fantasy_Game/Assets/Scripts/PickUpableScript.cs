using UnityEngine;
using System.Collections;
using UnityEngine.Experimental.GlobalIllumination;

public class PickUpableScript : MonoBehaviour
{
    public Player_Script playerScript;
    private MeshRenderer meshRenderer;
    private Light light;

    private bool pickedUp = false;
    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("PickupTag"))
        {
            StartCoroutine(WaitForAnim());
        }
    }
    IEnumerator WaitForAnim()
    {
        yield return new WaitForSeconds(0.5f);
        this.meshRenderer.enabled = false;
        pickedUp = true;
        if (this.CompareTag("Crystal"))
        {
            light = this.GetComponentInChildren<Light>();
            if (light != null)
            {
                light.enabled = false;
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && pickedUp)
        {
            Destroy(this.gameObject);
            playerScript.pickedUp = false;
        }
    }
}
