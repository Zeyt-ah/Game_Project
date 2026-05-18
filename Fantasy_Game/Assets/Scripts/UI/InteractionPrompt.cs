using UnityEngine;
using TMPro;

public class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private GameObject promptObject;

    private void Awake()
    {
        Hide();
    }

    public void Show(string text = "Press E to interact")
    {
        if (promptText) promptText.text = text;
        promptObject.SetActive(true);
    }

    public void Hide()
    {
        promptObject.SetActive(false);
    }
}