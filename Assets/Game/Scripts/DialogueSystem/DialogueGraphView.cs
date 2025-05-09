using System.Collections.Generic;
using System.Linq;
using Unity.Serialization.Json;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueGraphView : GraphView
{
    public DialogueGraphView(DialogueEditorWindow  editorWindow)
    {
        this.SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        GridBackground grid = new();
        Insert(0, grid);
        grid.StretchToParentSize();

        // Add default styles
        styleSheets.Add(Resources.Load<StyleSheet>("DialogueGraphStyles"));

        // Add sample node
        AddElement(CreateNode("New Speaker"));
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();

        ports.ForEach(port =>
        {
            if (port != startPort &&
                port.direction != startPort.direction &&
                port.node != startPort.node)
            {
                compatiblePorts.Add(port);
            }
        });

        return compatiblePorts;
    }


    public DialogueNode CreateNode(string speaker)
    {
        var node = new DialogueNode
        {
            GUID = System.Guid.NewGuid().ToString(),
            title = speaker,
            speaker = speaker,
            text = "Dialogue line...",
            emotion = "Neutral"
        };

        node.Draw();
        node.SetPosition(new Rect(Vector2.zero, new Vector2(250, 200)));
        AddElement(node); 
        return node;
    }

    public void Save(DialogueTree tree)
    {
        tree.nodes.Clear();
    UnityEngine.Debug.Log("Saving " + this.nodes.Count().ToString() + " nodes");

        // Save all DialogueNodes
        foreach (var node in nodes.ToList().OfType<DialogueNode>())
        {
            DialogueNodeData data = new DialogueNodeData
            {
                nodeType = node.nodeType,
                color = node.color,
                guid = node.GUID,
                speaker = node.speaker,
                text = node.text,
                emotion = node.emotion,
                position = node.GetPosition().position,
                choices = new List<DialogueChoice>()
            };

            if (node.output != null && node.output.connections.Any())
            {
                var nextConnection = node.output.connections.First();
                if (nextConnection.input.node is DialogueNode targetNode)
                {
                    data.nextNodeGuid = targetNode.GUID;
                }
            }

            for (int i = 0; i < node.choicePorts.Count; i++)
            {
                var port = node.choicePorts[i];
                if (i >= node.choices.Count) continue;

                var choiceData = node.choices[i];

                foreach (var connection in port.connections)
                {
                    if (connection.input.node is DialogueNode targetNode)
                    {
                        data.choices.Add(new DialogueChoice
                        {
                            portName = choiceData.portName,
                            targetNodeGuid = targetNode.GUID,
                            condition = choiceData.condition
                        });
                    }
                }

                // Even if no connection, still save the choice itself:
                if (!port.connections.Any())
                {
                    data.choices.Add(new DialogueChoice
                    {
                        portName = choiceData.portName,
                        targetNodeGuid = "", // no connection
                        condition = choiceData.condition
                    });
                }
            }

            tree.nodes.Add(data);
        }

        EditorUtility.SetDirty(tree);
    }
    public void Load(DialogueTree tree)
    {
        DeleteElements(graphElements.ToList()); // Clear old graph

        var nodeLookup = new Dictionary<string, DialogueNode>();

        // Create nodes
        foreach (var data in tree.nodes)
        {
            var node = new DialogueNode
            {
                nodeType = data.nodeType,
                color = data.color,
                GUID = data.guid,
                speaker = data.speaker,
                text = data.text,
                emotion = data.emotion,
                title = data.speaker,
                choices = new List<DialogueChoice>(data.choices)
            };

            node.SetPosition(new Rect(data.position, new Vector2(250, 200)));
            AddElement(node);
            nodeLookup.Add(node.GUID, node);

            // Add same number of choice ports
            foreach (var choice in data.choices)
                node.AddChoicePort(choice.portName);

            node.Draw();
        }

        // Next output
        foreach (var data in tree.nodes)
        {
            var fromNode = nodeLookup[data.guid];
            
            if (!string.IsNullOrEmpty(data.nextNodeGuid) && nodeLookup.TryGetValue(data.nextNodeGuid, out var toNode))
            {
                var edge = new Edge
                {
                    output = fromNode.output,
                    input = toNode.input
                };
                edge.input.Connect(edge);
                edge.output.Connect(edge);
                AddElement(edge);
            }
        }


        // Connect choices
        foreach (var data in tree.nodes)
        {
            var fromNode = nodeLookup[data.guid];
            for (int i = 0; i < data.choices.Count; i++)
            {
                var choice = data.choices[i];
                var toNode = nodeLookup[choice.targetNodeGuid];
                var outputPort = fromNode.choicePorts[i];
                var inputPort = toNode.input;

                var edge = new Edge
                {
                    output = outputPort,
                    input = inputPort
                };
                edge.input.Connect(edge);
                edge.output.Connect(edge);
                AddElement(edge);
            }
        }
    }
}
