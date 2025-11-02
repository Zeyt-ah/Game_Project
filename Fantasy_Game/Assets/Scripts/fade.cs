using UnityEngine;
using System.Collections;

public class FadeController : MonoBehaviour
{
    //for damage on fall
    public GameManagerScript gameManagerScript;
    public Player_Script playerScript;

    public CanvasGroup canvasGroup;
    public float fadeDuration = 0.5f;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.alpha = 0; // start fully transparent
    }

    public IEnumerator FadeOut()
    {
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, elapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        TakeDamage();
    }

    public IEnumerator FadeIn()
    {
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;

    }


    public void TakeDamage()
    {
        //takes away 10 health from UI
        gameManagerScript.UpdateHealth(10);

        //takes away 10 health from player
        playerScript.health -= 10;
    }
}
