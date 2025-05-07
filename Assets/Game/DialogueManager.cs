// DialogueManager.cs updated to use Next output and branching choices
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

        // Find Start node
        var startNode = dialogueTree.nodes.Find(n => n.nodeType == DialogueNodeType.Start);
        if (startNode != null)
            StartCoroutine(RunNode(startNode));
        else
            Debug.LogWarning("[DialogueManager] No Start node found.");
    }

    IEnumerator RunNode(DialogueNodeData node)
    {
        Debug.Log($"Speaker: {node.speaker}");
        Debug.Log($"Emotion: {node.emotion}");
        Debug.Log($"Text: {node.text}");

        // Display dialogue here (UI, TextMeshPro, etc.)
        yield return new WaitForSeconds(2f); // simulate time

        if (node.nodeType == DialogueNodeType.End)
        {
            Debug.Log("[DialogueManager] Dialogue ended.");
            yield break;
        }

        // Multiple choices (branching)
        if (node.choices.Count > 1)
        {
            Debug.Log("[DialogueManager] Presenting choices:");
            foreach (var choice in node.choices)
            {
                Debug.Log(" - " + choice.portName);
            }

            // Automatically pick the first for now
            string nextGuid = node.choices[0].targetNodeGuid;
            if (nodeLookup.TryGetValue(nextGuid, out var nextNode))
                StartCoroutine(RunNode(nextNode));
        }
        // Linear next node
        else if (node.choices.Count == 1)
        {
            string nextGuid = node.choices[0].targetNodeGuid;
            if (nodeLookup.TryGetValue(nextGuid, out var nextNode))
                StartCoroutine(RunNode(nextNode));
        }
        else
        {
            Debug.Log("[DialogueManager] No further nodes.");
        }
    }
}
