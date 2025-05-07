using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueNodeData
{
    public DialogueNodeType nodeType;
    public string guid;
    public string speaker;
    public string text;
    public string emotion;

    public Vector2 position; // for the editor
    public List<DialogueChoice> choices = new();
}
