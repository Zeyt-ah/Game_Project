using System;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject dialogueRoot;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text lineText;
    [SerializeField] private Button option1Button;
    [SerializeField] private TMP_Text option1Label;
    [SerializeField] private Button option2Button;
    [SerializeField] private TMP_Text option2Label;

    [Header("Input")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private string gameplayMap = "Gameplay";
    [SerializeField] private string uiMap = "UI";

    private Action option1Action;
    private Action option2Action;
    private Action onClose;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        dialogueRoot.SetActive(false);
    }

    public void Open(
        string npcName,
        string line,
        (string text, Action onClick)? option1 = null,
        (string text, Action onClick)? option2 = null,
        Action onClosed = null)
    {
        IsOpen = true;
        onClose = onClosed;

        // Freeze gameplay controls + enable UI controls
        if (playerInput) playerInput.SwitchCurrentActionMap(uiMap);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        dialogueRoot.SetActive(true);
        nameText.text = npcName;
        lineText.text = line;

        SetupButton(option1Button, option1Label, option1, out option1Action);
        SetupButton(option2Button, option2Label, option2, out option2Action);

        option1Button.onClick.RemoveAllListeners();
        option2Button.onClick.RemoveAllListeners();

        option1Button.onClick.AddListener(() => option1Action?.Invoke());
        option2Button.onClick.AddListener(() => option2Action?.Invoke());
    }

    private void SetupButton(Button btn, TMP_Text label, (string text, Action onClick)? opt, out Action action)
    {
        if (opt.HasValue)
        {
            btn.gameObject.SetActive(true);
            label.text = opt.Value.text;
            action = opt.Value.onClick;
        }
        else
        {
            btn.gameObject.SetActive(false);
            label.text = "";
            action = null;
        }
    }

    public void Close()
    {
        IsOpen = false;

        dialogueRoot.SetActive(false);

        if (playerInput) playerInput.SwitchCurrentActionMap(gameplayMap);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        onClose?.Invoke();
        onClose = null;
    }
}