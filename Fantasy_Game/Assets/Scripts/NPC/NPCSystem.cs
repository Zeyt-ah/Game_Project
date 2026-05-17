using UnityEngine;

public class NPCSystem : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogue;
    [SerializeField] private string npcName;
    [SerializeField] private DialogueNode startingNode;
    [SerializeField] private BossIntroSequence bossIntroSequence;

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
            onClosed: () => player.EnableMovement(),
            onDialogueAction: HandleDialogueAction
        );
    }

    // Handles actions triggered by dialogue choices
    private void HandleDialogueAction(DialogueActionType actionType)
    {
        if (actionType == DialogueActionType.StartDragonBossIntro)
        {
            if (bossIntroSequence != null)
            {
                bossIntroSequence.StartBossIntro();
            }
            else
            {
                Debug.LogWarning("Boss intro sequence is not assigned on this NPC.");
            }
        }
    }

}