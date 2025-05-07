using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public DialogueTree dialogueTree;
    private Dictionary<string, DialogueNodeData> nodeLookup;

    public void StartDialogue()
    {
        nodeLookup = new();
        foreach (var node in dialogueTree.nodes)
            nodeLookup[node.guid] = node;

        StartCoroutine(RunNode(dialogueTree.nodes[0])); // Start from the first node
    }

    IEnumerator RunNode(DialogueNodeData node)
    {
        Debug.Log($"Speaker: {node.speaker}");
        Debug.Log($"Emotion: {node.emotion}");
        Debug.Log($"Text: {node.text}");

        // Display dialogue here (UI, TextMeshPro, etc.)
        yield return new WaitForSeconds(2f); // simulate time

        if (node.choices.Count == 0)
            yield break;

        var nextNodeGuid = node.choices[0].targetNodeGuid;
        if (nodeLookup.TryGetValue(nextNodeGuid, out var nextNode))
        {
            StartCoroutine(RunNode(nextNode));
        }
    }
}
