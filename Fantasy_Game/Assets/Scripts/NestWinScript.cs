using UnityEngine;

public class NestWin : MonoBehaviour
{
    public Player_Script playerScript;
    public GameManagerScript gameManager;

    private bool hasWon = false;

    private void OnTriggerStay(Collider other)
    {
        if (hasWon) return;
        if (!other.CompareTag("Player")) return;
        if (playerScript == null) return;


        if (playerScript.eggCount >= 3) { 
            hasWon = true;
            Win();
        }
    }

    private void Win()
    {
        Debug.Log("you win");
        gameManager.Win();
    }
}
