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

    [Header("Shop")] // Newly added section!
    [SerializeField] private GameObject shopUI;

    private string currentNpcName;
    private Action onClose;
    private DialogueNode currentNode;
    private Action<DialogueActionType> onAction;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        dialogueRoot.SetActive(false);
        if (shopUI != null) shopUI.SetActive(false); // Hide shop UI on awake
    }

    public void StartDialogue(string npcName, DialogueNode startNode, Action onClosed = null, Action<DialogueActionType> onDialogueAction = null)
    {
        if (startNode == null) return;
        IsOpen = true;
        currentNpcName = npcName;
        currentNode = startNode;
        onClose = onClosed;
        onAction = onDialogueAction;

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
            // Check if the selected action is opening the shop (Newly added)
            bool isOpeningShop = (choice.action == DialogueActionType.OpenShop); 

            if (choice.action != DialogueActionType.None)
            {
                onAction?.Invoke(choice.action);
            }

            if (choice.next == null)
            {
                Close();
            }
            else
            {
                ShowNode(choice.next);
            }

            // Open the shop UI immediately after the dialogue closes and regain mouse control (Newly added)
            if (isOpeningShop && shopUI != null)
            {
                shopUI.SetActive(true);

                // Re-enable the cursor that was hidden by Close()
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                
                if (playerInput) playerInput.SwitchCurrentActionMap(uiMap);
                if (cinemachineRotationControl) cinemachineRotationControl.enabled = false;
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
        onAction = null;
        currentNode = null;
    }

    // Function to call when the 'Close' button in the Shop UI is clicked (Newly added)
    public void CloseShop()
    {
        if (shopUI != null) shopUI.SetActive(false);

        // Return to gameplay state (hide cursor, enable player input and rotation)
        if (playerInput) playerInput.SwitchCurrentActionMap(gameplayMap);
        if (cinemachineRotationControl) cinemachineRotationControl.enabled = true;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}