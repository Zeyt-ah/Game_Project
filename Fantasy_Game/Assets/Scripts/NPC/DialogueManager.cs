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

    [Header("Shop")]
    [SerializeField] private GameObject shopUI;

    private string currentNpcName;
    private Action onClose;
    private DialogueNode currentNode;
    private Action<DialogueActionType> onAction;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        if (dialogueRoot != null) dialogueRoot.SetActive(false);
        if (shopUI != null) shopUI.SetActive(false);
    }

    public void StartDialogue(string npcName, DialogueNode startNode, Action onClosed = null, Action<DialogueActionType> onDialogueAction = null)
    {
        if (startNode == null) return;
        IsOpen = true;
        currentNpcName = npcName;
        currentNode = startNode;
        onClose = onClosed;
        onAction = onDialogueAction;

        if (playerInput != null) playerInput.SwitchCurrentActionMap(uiMap);
        if (cinemachineRotationControl != null) cinemachineRotationControl.enabled = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (dialogueRoot != null) dialogueRoot.SetActive(true);
        ShowNode(currentNode);
    }

    private void ShowNode(DialogueNode node)
    {
        if (node == null) return;

        currentNode = node;
        if (nameText != null) nameText.text = currentNpcName;
        if (lineText != null) lineText.text = node.line;

        SetupChoice(option1Button, option1Label, node.option1);
        SetupChoice(option2Button, option2Label, node.option2);
    }

    private void SetupChoice(Button button, TMP_Text label, DialogueChoice choice)
    {
        if (button == null) return;
        button.onClick.RemoveAllListeners();

        if (choice == null || string.IsNullOrWhiteSpace(choice.text))
        {
            button.gameObject.SetActive(false);
            return;
        }

        button.gameObject.SetActive(true);
        if (label != null) label.text = choice.text;

        button.onClick.AddListener(() =>
        {
            bool isOpeningShop = (choice.action == DialogueActionType.OpenShop);

            // Prevent external script exceptions from blocking execution
            if (choice.action != DialogueActionType.None)
            {
                try { onAction?.Invoke(choice.action); }
                catch (Exception e) { Debug.LogWarning("Action Error Blocked: " + e.Message); }
            }

            if (isOpeningShop)
            {
                IsOpen = false;
                if (dialogueRoot != null) dialogueRoot.SetActive(false);

                // Secure cursor visibility and input control immediately
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                if (playerInput != null) playerInput.SwitchCurrentActionMap(uiMap);
                if (cinemachineRotationControl != null) cinemachineRotationControl.enabled = false;

                if (shopUI != null)
                {
                    shopUI.SetActive(true);
                }

                // Handle external close callbacks safely
                try { onClose?.Invoke(); }
                catch (Exception e) { Debug.LogWarning("Close Error Blocked: " + e.Message); }

                onClose = null;
                onAction = null;
                currentNode = null;

                return;
            }

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
        if (dialogueRoot != null) dialogueRoot.SetActive(false);

        if (playerInput != null) playerInput.SwitchCurrentActionMap(gameplayMap);
        if (cinemachineRotationControl != null) cinemachineRotationControl.enabled = true;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        try { onClose?.Invoke(); }
        catch (Exception e) { Debug.LogWarning("Close Error Blocked: " + e.Message); }

        onClose = null;
        onAction = null;
        currentNode = null;
    }

    public void CloseShop()
    {
        if (shopUI != null) shopUI.SetActive(false);

        if (playerInput != null) playerInput.SwitchCurrentActionMap(gameplayMap);
        if (cinemachineRotationControl != null) cinemachineRotationControl.enabled = true;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}