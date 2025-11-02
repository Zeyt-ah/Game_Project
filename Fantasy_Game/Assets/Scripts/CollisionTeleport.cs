using UnityEngine;
using System.Collections;

public class TeleportOnTrigger : MonoBehaviour
{
    [Header("Teleport Settings")]
    public Transform teleportDestination;
    public string playerTag = "Player";

    [Header("Fade Effect")]
    public FadeController fadeController;

    [Tooltip("How long the screen stays black after teleport.")]
    public float blackScreenDuration = 1f;

    private bool isTeleporting = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isTeleporting) return;

        if (other.CompareTag(playerTag) && teleportDestination != null)
        {
            StartCoroutine(TeleportWithFade(other));
        }
    }

    private IEnumerator TeleportWithFade(Collider player)
    {
        isTeleporting = true;

        CharacterController controller = player.GetComponent<CharacterController>();

        // Fade out to black
        if (fadeController != null)
            yield return StartCoroutine(fadeController.FadeOut());

        // Teleport instantly while screen is black
        if (controller != null) controller.enabled = false;
        player.transform.position = teleportDestination.position;
        if (controller != null) controller.enabled = true;

        // Wait while screen stays black
        yield return new WaitForSeconds(blackScreenDuration);

        // Fade back in
        if (fadeController != null)
            yield return StartCoroutine(fadeController.FadeIn());

        isTeleporting = false;
    }
}
