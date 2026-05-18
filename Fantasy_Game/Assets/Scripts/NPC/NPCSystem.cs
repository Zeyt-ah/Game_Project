using UnityEngine;

public class NPCSystem : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogue;
    [SerializeField] private string npcName;
    [SerializeField] private DialogueNode startingNode;

    [Header("player quests /spell")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject windMillEgg;
    [SerializeField] private GameObject windMillEggVisual;
    [SerializeField] private GameObject berryBag;
    private bool eggDone = false;
    private bool bagDone = false;

    [Header("Boss Intro")]
    [SerializeField] private BossIntroSequence bossIntroSequence;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string idleStateName = "Idle";
    [SerializeField] private string talkingStateName = "HumanM@Talk01";

    [Header("Dragon Fight Requirements")]
    [SerializeField] private bool requiresEggsToInteract = false;
    [SerializeField] private int requiredEggCount = 3;
    [SerializeField] private bool disableAfterDragonFightAccepted = false;

    private bool dragonFightAccepted = false;

    // Finds the dialogue manager if one has not been assigned in the Inspector
    private void Awake()
    {
        if (dialogue == null)
        {
            dialogue = FindFirstObjectByType<DialogueManager>();
        }
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Starts this NPC's dialogue if the player is allowed to speak to them
    public void StartDialogue(PlayerScriptNew player)
    {
        if (dialogue == null || startingNode == null)
        {
            return;
        }

        if (!CanInteract(player.gameObject))
        {
            Debug.Log(GetBlockedInteractionMessage(player.gameObject));
            return;
        }

        player.DisableMovement();

        PlayTalkingAnimation();

        dialogue.StartDialogue(
            npcName,
            startingNode,
            onClosed: () =>
            {
                player.EnableMovement();
                PlayIdleAnimation();

                PlayerInteractionScript interaction = player.GetComponent<PlayerInteractionScript>();

                if (interaction != null)
                {
                    interaction.DialogueClosed();
                }
            },
            onDialogueAction: HandleDialogueAction
        );
    }

    // Checks whether this NPC can currently be interacted with
    public bool CanInteract(GameObject playerObject)
    {
        if (dragonFightAccepted && disableAfterDragonFightAccepted)
        {
            return false;
        }

        if (!requiresEggsToInteract)
        {
            return true;
        }

        PlayerScriptNew playerScript = playerObject.GetComponent<PlayerScriptNew>();

        if (playerScript == null)
        {
            playerScript = playerObject.GetComponentInParent<PlayerScriptNew>();
        }

        if (playerScript == null)
        {
            Debug.LogWarning("PlayerScriptNew was not found when checking egg requirement.");
            return false;
        }

        if (playerScript.gameManager == null)
        {
            Debug.LogWarning("GameManagerScript is not assigned on PlayerScriptNew.");
            return false;
        }

        return playerScript.gameManager.EggCount() >= requiredEggCount;
    }

    // Returns a message explaining why the NPC cannot currently be interacted with
    public string GetBlockedInteractionMessage(GameObject playerObject)
    {
        if (dragonFightAccepted && disableAfterDragonFightAccepted)
        {
            return "The fight has already started.";
        }

        if (!requiresEggsToInteract)
        {
            return "";
        }

        PlayerScriptNew playerScript = playerObject.GetComponent<PlayerScriptNew>();

        if (playerScript == null)
        {
            playerScript = playerObject.GetComponentInParent<PlayerScriptNew>();
        }

        if (playerScript == null || playerScript.gameManager == null)
        {
            return "Cannot check egg requirement.";
        }

        int currentEggs = playerScript.gameManager.EggCount();

        return "You need " + requiredEggCount + " eggs to speak to the wizard. Current eggs: " + currentEggs + "/" + requiredEggCount;
    }

    // Handles actions triggered by dialogue choices
    private void HandleDialogueAction(DialogueActionType actionType)
    {
        if (actionType == DialogueActionType.StartDragonBossIntro)
        {
            dragonFightAccepted = true;

            PlayIdleAnimation();

            if (bossIntroSequence != null)
            {
                bossIntroSequence.StartBossIntro();
            }
            else
            {
                Debug.LogWarning("Boss intro sequence is not assigned on this NPC.");
            }

            if (disableAfterDragonFightAccepted)
            {
                Collider npcCollider = GetComponent<Collider>();

                if (npcCollider != null)
                {
                    npcCollider.enabled = false;
                }
            }
        }
        if(actionType == DialogueActionType.GiveSpell)
        {
            PlayerCombatScript playerCombat = player.GetComponent<PlayerCombatScript>();
            playerCombat.EnableSpell();
        }
        if(actionType == DialogueActionType.AllowWindmillEggPickup && !eggDone)
        {
            windMillEggVisual.GetComponent<MeshRenderer>().enabled = false;
            windMillEgg.SetActive(true);
            eggDone = true;
        }
        if(actionType == DialogueActionType.KalQuestAllowPickup && !bagDone)
        {
            berryBag.SetActive(true);
            bagDone = true;
        }
    }

    // Plays the NPC talking animation
    private void PlayTalkingAnimation()
    {
        if (animator == null)
        {
            return;
        }

        animator.Play(talkingStateName);
    }

    // Returns the NPC to idle after dialogue closes
    private void PlayIdleAnimation()
    {
        if (animator == null)
        {
            return;
        }

        animator.Play(idleStateName);
    }
}