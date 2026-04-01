using UnityEngine;
using TMPro;
using System.Collections;

public class ShopMessageUI : MonoBehaviour
{
    public TextMeshProUGUI messageText;
    public float showTime = 2f;

    private Coroutine messageCoroutine;

    private void Start()
    {
        if (messageText != null)
        {
            messageText.text = "";
            messageText.gameObject.SetActive(false);
        }
    }

    public void ShowMessage(string message)
    {
        if (messageText == null)
        {
            Debug.LogWarning("MessageText is not assigned in ShopMessageUI!");
            return;
        }

        if (messageCoroutine != null)
            StopCoroutine(messageCoroutine);

        messageCoroutine = StartCoroutine(ShowMessageRoutine(message));
    }

    private IEnumerator ShowMessageRoutine(string message)
    {
        messageText.gameObject.SetActive(true);
        messageText.text = message;

        yield return new WaitForSeconds(showTime);

        messageText.text = "";
        messageText.gameObject.SetActive(false);
        messageCoroutine = null;
    }
}