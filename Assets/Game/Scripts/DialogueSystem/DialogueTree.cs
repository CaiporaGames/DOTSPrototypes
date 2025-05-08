using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Dialogue Tree")]
public class DialogueTree : ScriptableObject
{
    public List<DialogueNodeData> nodes = new();
}
