using UnityEngine;

public class NPCSystem : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogue;
    [SerializeField] private string npcName = "Peasant";
    [SerializeField] private DialogueNode startingNode;

    private void Awake()
    {
        if (dialogue == null)
            dialogue = FindFirstObjectByType<DialogueManager>();
    }

    public void StartDialogue(PlayerScriptNew player)
    {
        if (dialogue == null || startingNode == null) return;

        player.DisableMovement();

        dialogue.StartDialogue(
            npcName,
            startingNode,
            onClosed: () => player.EnableMovement()
        );
    }
}