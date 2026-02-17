using UnityEngine;
using System.Collections;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEditor.UI;

public class PickUpableScript : MonoBehaviour
{
    public PlayerInteractionScript playerScript;
    public GameObject wholeMushroomObject;
    private MeshRenderer meshRenderer;
    private Light light;


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
        this.gameObject.SetActive(false);
        playerScript.PickedUp();
        if (this.CompareTag("Crystal"))
        {
            light = this.GetComponentInChildren<Light>();
            if (light != null)
            {
                light.enabled = false;
            }
        }
        if (wholeMushroomObject != null)
        {
            wholeMushroomObject.SetActive(false);
        }
    }


}
