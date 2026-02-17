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

    private bool isInteracting = false;

    private int coinCount = 0;
    private int crystalCount = 0;
    public int eggCount = 0;
    private bool onPickupable = false;
    private bool countIncreased = false;

    public System.Action OnInteractStarted;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovementScript>();
        player = GetComponent<PlayerScriptNew>();
    }


    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Crystal"))
        {
            onPickupable = true;
            if (isInteracting && !countIncreased)
            {
                countIncreased = true;
                crystalCount++;
                gameManager.UpdateScore(50);
            }
        }
        else if (other.CompareTag("Egg"))
        {

            onPickupable = true;
            if (isInteracting && !countIncreased)
            {
                countIncreased = true;
                eggCount++;
                gameManager.UpdateEggs();

            }
        }

        else if (other.CompareTag("Mushroom"))
        {
            onPickupable = true;
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
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Crystal"))
        {
            onPickupable = false;
        }
        else if (other.CompareTag("Egg"))
        {
            onPickupable = false;
        }
        else if (other.CompareTag("Mushroom"))
        {
            onPickupable = false;
        }
    }


    //checks if player is interacting
    public void Interact(InputAction.CallbackContext context)
    {
        if (context.started && onPickupable && player.CanMove())
        {
            player.DisableMovement();
            isInteracting = true;
            OnInteractStarted?.Invoke();
            pickupHitbox.enabled = true;
            StartCoroutine(GatherTime());
        }

        else if (context.canceled)
        {
            isInteracting = false;
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
}
