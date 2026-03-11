using System;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
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
    [SerializeField] private string gameplayMap = "Player";
    [SerializeField] private string uiMap = "UI";
    [SerializeField] private Behaviour cinemachineRotationControl;

    private string currentNpcName;
    private Action onClose;
    private DialogueNode currentNode;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        dialogueRoot.SetActive(false);
    }


    public void StartDialogue(string npcName, DialogueNode startNode, Action onClosed = null)
    {
        if (startNode == null) return;

        IsOpen = true;
        currentNpcName = npcName;
        currentNode = startNode;
        onClose = onClosed;

        if (playerInput) playerInput.SwitchCurrentActionMap(uiMap);

        if (cinemachineRotationControl) cinemachineRotationControl.enabled = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        dialogueRoot.SetActive(true);

        ShowNode(currentNode);
    }
    private void ShowNode(DialogueNode node)
    {
        currentNode = node;

        nameText.text = currentNpcName;
        lineText.text = node.line;

        SetupChoice(option1Button, option1Label, node.option1);
        SetupChoice(option2Button, option2Label, node.option2);
    }

    private void SetupChoice(Button button, TMP_Text label, DialogueChoice choice)
    {
        button.onClick.RemoveAllListeners();

        // If no text hide button
        if (choice == null || string.IsNullOrWhiteSpace(choice.text))
        {
            button.gameObject.SetActive(false);
            return;
        }

        button.gameObject.SetActive(true);
        label.text = choice.text;

        button.onClick.AddListener(() =>
        {
            if (choice.next == null)
            {
                Close();
            }
            else
            {
                ShowNode(choice.next);
            }
        });
    }

    public void Close()
    {
        IsOpen = false;

        dialogueRoot.SetActive(false);

        if (playerInput) playerInput.SwitchCurrentActionMap(gameplayMap);

        if (cinemachineRotationControl) cinemachineRotationControl.enabled = true;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        onClose?.Invoke();
        onClose = null;
        currentNode = null;
    }
}