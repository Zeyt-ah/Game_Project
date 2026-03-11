using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Node")]
public class DialogueNode : ScriptableObject
{
    [TextArea(2, 6)]
    public string line;

    public DialogueChoice option1;
    public DialogueChoice option2;
}