using UnityEngine;
using TMPro;

public class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] private GameObject root;     // panel root (or just use this.gameObject)
    [SerializeField] private TMP_Text promptText; // optional

    private void Awake()
    {
        Hide();
    }

    public void Show(string text = "Press E to interact")
    {
        if (promptText) promptText.text = text;
        if (root) root.SetActive(true);
        else gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (root) root.SetActive(false);
        else gameObject.SetActive(false);
    }
}