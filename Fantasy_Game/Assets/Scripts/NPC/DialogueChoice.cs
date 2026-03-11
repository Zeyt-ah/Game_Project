using UnityEngine;

[System.Serializable]
public class DialogueChoice
{
    public string text;
    public DialogueNode next;   // if null => ends dialogue
}