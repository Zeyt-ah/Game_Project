using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FadeScreenTrigger : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup blackScreen;
    public CanvasGroup textGroup;
    public CanvasGroup buttonGroup;

    public TMP_Text messageText;
    public Button continueButton;

    [Header("Message")]
    [TextArea]
    public string displayMessage = "You entered the forbidden area...";

    [Header("Fade Settings")]
    public float fadeSpeed = 1.5f;
    public float delayBeforeText = 1f;

    [Header("Player Tag")]
    public string playerTag = "Player";

    private bool triggered = false;

    private void Start()
    {
        // Ensure UI starts hidden
        blackScreen.alpha = 0;
        textGroup.alpha = 0;
        buttonGroup.alpha = 0;

        blackScreen.gameObject.SetActive(false);
        textGroup.gameObject.SetActive(false);
        buttonGroup.gameObject.SetActive(false);

        continueButton.onClick.AddListener(HideDisplay);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        if (other.CompareTag(playerTag))
        {
            triggered = true;
            StartCoroutine(ShowSequence());
        }
    }

    IEnumerator ShowSequence()
    {
        // Enable black screen
        blackScreen.gameObject.SetActive(true);

        // Fade screen to black
        yield return StartCoroutine(FadeCanvasGroup(blackScreen, 0, 1));

        yield return new WaitForSeconds(delayBeforeText);

        // Show text
        textGroup.gameObject.SetActive(true);
        messageText.text = displayMessage;

        yield return StartCoroutine(FadeCanvasGroup(textGroup, 0, 1));

        // Show button
        buttonGroup.gameObject.SetActive(true);

        yield return StartCoroutine(FadeCanvasGroup(buttonGroup, 0, 1));
    }

    public void HideDisplay()
    {
        StartCoroutine(HideSequence());
    }

    IEnumerator HideSequence()
    {
        // Fade out button
        yield return StartCoroutine(FadeCanvasGroup(buttonGroup, 1, 0));

        // Fade out text
        yield return StartCoroutine(FadeCanvasGroup(textGroup, 1, 0));

        // Fade out black screen
        yield return StartCoroutine(FadeCanvasGroup(blackScreen, 1, 0));

        blackScreen.gameObject.SetActive(false);
        textGroup.gameObject.SetActive(false);
        buttonGroup.gameObject.SetActive(false);
    }

    IEnumerator FadeCanvasGroup(CanvasGroup group, float start, float end)
    {
        float time = 0;

        while (time < 1)
        {
            time += Time.deltaTime * fadeSpeed;
            group.alpha = Mathf.Lerp(start, end, time);
            yield return null;
        }

        group.alpha = end;
    }
}