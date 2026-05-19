using UnityEngine;

public class EnvironmentSwitcher : MonoBehaviour
{
    [Header("Environment Settings")]
    public Material newSkybox;
    public Color newFogColor = Color.gray;

    [Header("Lighting")]
    public Light currentDirectionalLight;
    public Light newDirectionalLight;

    [Header("Player Tag")]
    public string playerTag = "Player";

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // Prevent multiple triggers
        if (hasTriggered) return;

        // Check if player entered
        if (other.CompareTag(playerTag))
        {
            hasTriggered = true;

            // Change skybox
            if (newSkybox != null)
            {
                RenderSettings.skybox = newSkybox;

                // Refresh ambient lighting
                DynamicGI.UpdateEnvironment();
            }

            // Change fog color
            RenderSettings.fogColor = newFogColor;

            // Swap directional lights
            if (currentDirectionalLight != null)
            {
                currentDirectionalLight.gameObject.SetActive(false);
            }

            if (newDirectionalLight != null)
            {
                newDirectionalLight.gameObject.SetActive(true);
            }
        }
    }
}