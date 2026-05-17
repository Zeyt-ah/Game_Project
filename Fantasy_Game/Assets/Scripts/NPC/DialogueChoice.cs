using UnityEngine;

public enum DialogueActionType
{
    None,
    StartDragonBossIntro
}

[System.Serializable]
public class DialogueChoice
{
    public string text;
    public DialogueNode next;   // if null => ends dialogue

    [Header("Optional Action")]
    public DialogueActionType action;
}
