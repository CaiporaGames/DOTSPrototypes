// DialogueManager.cs updated to use Next output and branching choices
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI speakerNameText = null;
    [SerializeField] private TextMeshProUGUI speakerEmotionText = null;
    [SerializeField] private TextMeshProUGUI speakerText = null;
    [SerializeField] private Transform choiceContainer = null;
    [SerializeField] private GameObject choicePrefab = null;


    public DialogueTree dialogueTree;
    private Dictionary<string, DialogueNodeData> nodeLookup;

    void Start()
    {
        Invoke("StartDialogue", 2);
    }

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
        speakerNameText.text = node.speaker;
        speakerEmotionText.text = node.emotion;
        speakerText.text = node.text;

        // Display dialogue here (UI, TextMeshPro, etc.)
        yield return new WaitForSeconds(2f); // simulate time

        if (node.nodeType == DialogueNodeType.End)
        {
            Debug.Log("[DialogueManager] Dialogue ended.");
            yield break;
        }

        if(node.nextNodeGuid != string.Empty && node.choices.Count == 0)
        {
            if (nodeLookup.TryGetValue(node.nextNodeGuid, out var nextNode))
                StartCoroutine(RunNode(nextNode));
        }
        // Multiple choices (branching)
        else if (node.choices.Count > 0)
        {
            for(int i = 0; i < node.choices.Count; i++)
            {
                if(node.choices[i].condition == string.Empty /* Make a and condition to see if the condition was fullfiled */)
                {
                    ChoiceUI choiceUI = Instantiate(choicePrefab, choiceContainer).GetComponent<ChoiceUI>();
                    choiceUI.SetupChoice(node, i);
                }
            }
           /*  string nextGuid = node.choices[0].targetNodeGuid;
            if (nodeLookup.TryGetValue(nextGuid, out var nextNode))
                StartCoroutine(RunNode(nextNode)); */
        }
        else
        {
            Debug.Log("[DialogueManager] No further nodes.");
        }
    }
}
