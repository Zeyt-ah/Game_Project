using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.VisualScripting;

public class PlayerInteractionScript : MonoBehaviour
{
    private PlayerScriptNew player;
    private PlayerMovementScript playerMovement;

    [SerializeField] private GameManagerScript gameManager;
    [SerializeField] private BoxCollider pickupHitbox;
    [SerializeField] private InteractionPromptUI promptUI;
    [SerializeField] private DialogueManager dialogueManager;

    private bool isInteracting = false;

    private int coinCount = 0;
    private int crystalCount = 0;
    public int eggCount = 0;
    private bool onPickupable = false;
    private bool countIncreased = false;

    private bool npcInRange = false;
    private NPCSystem currentNpc;
    private bool lastNpcInRange;
    private bool lastOnPickupable;

    public System.Action OnInteractStarted;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovementScript>();
        player = GetComponent<PlayerScriptNew>();

        if (dialogueManager == null)
            dialogueManager = FindFirstObjectByType<DialogueManager>();
    }

    private void Update()
    {
        if (npcInRange != lastNpcInRange || onPickupable != lastOnPickupable)
        {
            lastNpcInRange = npcInRange;
            lastOnPickupable = onPickupable;
            UpdatePrompt();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Crystal"))
        {
            if (!onPickupable) { onPickupable = true; UpdatePrompt(); }

            if (isInteracting && !countIncreased)
            {
                countIncreased = true;
                crystalCount++;
                gameManager.UpdateScore(50);
            }
        }
        else if (other.CompareTag("Egg"))
        {

            if (!onPickupable) { onPickupable = true; UpdatePrompt(); }

            if (isInteracting && !countIncreased)
            {
                countIncreased = true;
                eggCount++;
                gameManager.UpdateEggs();

            }
        }

        else if (other.CompareTag("Mushroom"))
        {
            if (!onPickupable) { onPickupable = true; UpdatePrompt(); }

            if (isInteracting && !countIncreased)
            {
                countIncreased = true;
                playerMovement.jumpPower = 10;
                playerMovement.gravityMultiplier = 0.7f;
                playerMovement.speedForFall = 0f;
                player._animator.SetBool("mushroomPicked", true);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Coin"))
        {
            coinCount++;
            gameManager.UpdateScore(20);
        }
        else if (other.CompareTag("NPCTrigger"))
        {
            npcInRange = true;
            currentNpc = other.GetComponentInParent<NPCSystem>();
            UpdatePrompt();
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Crystal") || other.CompareTag("Egg") || other.CompareTag("Mushroom"))
        {
            onPickupable = false;
            UpdatePrompt();
        }
        else if (other.CompareTag("NPCTrigger"))
        {
            npcInRange = false;
            currentNpc = null;
            UpdatePrompt();
        }
    }


    //checks if player is interacting
    public void Interact(InputAction.CallbackContext context)
    {
        if (playerMovement.isRidingHorse()) return;
        if (!context.started) return;
        if (!player.CanMove()) return;
        if (dialogueManager != null && dialogueManager.IsOpen) return;

        if (npcInRange && currentNpc != null)
        {
            currentNpc.StartDialogue(player);
            promptUI.Hide();
            return;
        }

        if (onPickupable)
        {
            player.DisableMovement();
            isInteracting = true;
            OnInteractStarted?.Invoke();
            pickupHitbox.enabled = true;
            StartCoroutine(GatherTime());
            promptUI.Hide();
        }
    }

    IEnumerator GatherTime()
    {
        yield return new WaitForSeconds(1.5f);
        pickupHitbox.enabled = false;
        player.EnableMovement();
        countIncreased = false;
    }

    public void PickedUp()
    {
        onPickupable = false;
    }

    private void UpdatePrompt()
    {
        if (promptUI == null) return;

        // If dialogue is open, never show the interact prompt
        if (dialogueManager != null && dialogueManager.IsOpen)
        {
            promptUI.Hide();
            return;
        }

        bool canInteract = npcInRange || onPickupable;

        if (!canInteract)
        {
            promptUI.Hide();
            return;
        }

        if (npcInRange)
            promptUI.Show("Press E to talk");
        else
            promptUI.Show("Press E to pick up");
    }
}
