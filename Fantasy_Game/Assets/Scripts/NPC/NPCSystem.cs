using UnityEngine;

public class NPCSystem : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogue;
    [SerializeField] private string npcName;
    [SerializeField] private DialogueNode startingNode;

    [Header("Player Info For Quests/ receiving items")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject windmillEgg;
    [SerializeField] private GameObject windmillEggVisual;
    [SerializeField] private GameObject berryBag;

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
        if (player == null) player = GameObject.FindGameObjectWithTag("Player");
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

                PlayerInteractionScript interaction = player.GetComponent<PlayerInteractionScript>();

                if (interaction != null) interaction.DialogueClosed();
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
        //fpr dayne to give spell to the player.
        if (actionType == DialogueActionType.GiveSpell)
        {
            PlayerCombatScript playerCombat = player.GetComponent<PlayerCombatScript>();
            playerCombat.EnableSpell();
        }

        //for kals quest (enables egg pickup)
        if(actionType == DialogueActionType.AllowWindmillEggPickup)
        {
            windmillEgg.SetActive(true);
            windmillEggVisual.GetComponent<MeshRenderer>().enabled = false;
        }
        //spawns the bag after talking
        if(actionType == DialogueActionType.KalQuestAllowPickup)
        {
 
            berryBag.SetActive(true);
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