using UnityEngine;

public class NPCSystem : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogue;   // assign in inspector
    [SerializeField] private string npcName = "Peasant";

    private void Awake()
    {
        if (dialogue == null)
            dialogue = FindFirstObjectByType<DialogueManager>(); // or FindObjectOfType
    }

    public void StartDialogue(PlayerScriptNew player)
    {
        Debug.Log($"StartDialogue called. dialogue={(dialogue ? dialogue.name : "NULL")} player={(player ? player.name : "NULL")}");

        if (dialogue == null)
        {
            Debug.LogError("DialogueManager is NULL. Assign it in NPCSystem inspector, or ensure one exists in scene.");
            return;
        }

        if (player == null)
        {
            Debug.LogError("PlayerScriptNew passed to StartDialogue is NULL.");
            return;
        }

        player.DisableMovement();

        dialogue.Open(
            npcName,
            "Hello traveller.",
            option1: ("Goodbye", () => { dialogue.Close(); player.EnableMovement(); }
        ),
            option2: null
        );
    }
}