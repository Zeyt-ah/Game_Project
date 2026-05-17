using UnityEngine;

public class NPCSystem : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogue;
    [SerializeField] private string npcName;
    [SerializeField] private DialogueNode startingNode;

    [Header("Boss Intro")]
    [SerializeField] private BossIntroSequence bossIntroSequence;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string idleStateName = "Idle";
    [SerializeField] private string talkingStateName = "HumanM@Talk01";

    private void Awake()
    {
        if (dialogue == null)
            dialogue = FindFirstObjectByType<DialogueManager>();
    }

    public void StartDialogue(PlayerScriptNew player)
    {
        if (dialogue == null || startingNode == null) return;

        player.DisableMovement();

        PlayTalkingAnimation();

        dialogue.StartDialogue(
            npcName,
            startingNode,
            onClosed: () => {
                player.EnableMovement();
                PlayIdleAnimation();
            },
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

    // Plays the NPC talking animation
    private void PlayTalkingAnimation()
    {
        if (animator == null) return;

        animator.Play(talkingStateName);
    }

    // Returns the NPC to idle after dialogue closes
    private void PlayIdleAnimation()
    {
        if (animator == null) return;

        animator.Play(idleStateName);
    }

}