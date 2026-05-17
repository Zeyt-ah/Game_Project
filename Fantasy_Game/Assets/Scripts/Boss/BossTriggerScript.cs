using System.Collections;
using UnityEngine;
using TMPro;

public class BossTriggerScript : MonoBehaviour
{


    [Header("References")]
    public PlayerScriptNew player;
    public GameManagerScript gameManager;
    public Collider triggerCollider;
    public GameObject wizard;
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public GameObject smoke;
    public GameObject smokePosition;


    public int eggsRequired;

    private bool triggered = false;

    void Start()
    {
        triggerCollider = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;


        if (other.CompareTag("Player") && gameManager.EggCount() == eggsRequired)
        {
           
            triggered = true;
            triggerCollider.enabled = false;
            StartCoroutine(BossFightCutscene());
        }

    }

    private IEnumerator BossFightCutscene()
    {
        //smoke for entry of the boss
        GameObject entrySmoke = Instantiate(smoke, smokePosition.transform);
        yield return new WaitForSeconds(0.4f);

        Vector3 lookDirection = player.transform.position - wizard.transform.position;
        lookDirection.y = 0f;

        wizard.transform.forward = lookDirection;

        wizard.SetActive(true);
        Vector3 direction = (wizard.transform.position - player.transform.position).normalized;
        direction.y = 0f;

        player.transform.forward = direction;

        yield return new WaitForSeconds(0.1f);
        entrySmoke.SetActive(false);


        dialoguePanel.SetActive(true);

        dialogueText.text = "How dare you return the dragon eggs."; 
        yield return new WaitForSeconds(2.5f);

        dialogueText.text = "I warned you not to interfere";
        yield return new WaitForSeconds(2.5f);

        dialogueText.text = "I'll make sure this never happens again.";
        yield return new WaitForSeconds(2.5f);

        dialoguePanel.SetActive(false);

    }
}
