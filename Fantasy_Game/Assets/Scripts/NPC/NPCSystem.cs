using UnityEngine;

public class NPCSystem : MonoBehaviour
{
    public void StartDialogue()
    {
        Debug.Log($"Dialogue started on NPCSystem attached to: {gameObject.name}", this);
    }

}