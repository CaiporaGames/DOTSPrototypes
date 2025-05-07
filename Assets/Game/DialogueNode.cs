// Updated DialogueNode.cs with hybrid linear + branching model
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

public class DialogueNode : Node
{
    public DialogueNodeType nodeType = DialogueNodeType.Dialogue;   
    public string GUID;
    public string speaker;
    public string text;
    public string emotion;

    public Port input;
    public Port output;
    public List<Port> choicePorts = new();
    public List<DialogueChoice> choices = new(); // persistent choice data

    public void Draw()
    {
        title = speaker;

        // ⚠️ Only clear extension/output/input — not mainContainer!
        extensionContainer.Clear();
        inputContainer.Clear();
        outputContainer.Clear();
        choicePorts.Clear();

        // --- Node Type Dropdown ---
        var nodeTypeField = new EnumField("Node Type", nodeType);
        nodeTypeField.RegisterValueChangedCallback(evt => nodeType = (DialogueNodeType)evt.newValue);
        extensionContainer.Add(nodeTypeField);

        // --- Input Port ---
        input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
        input.portName = "Input";
        input.name = "input";
        inputContainer.Add(input);

        // --- Speaker Field ---
        var speakerField = new TextField("Speaker") { value = speaker };
        speakerField.RegisterValueChangedCallback(evt =>
        {
            speaker = evt.newValue;
            title = speaker;
        });
        extensionContainer.Add(speakerField);

        // --- Text Field ---
        var textField = new TextField("Text") { value = text, multiline = true };
        textField.RegisterValueChangedCallback(evt => text = evt.newValue);
        extensionContainer.Add(textField);

        // --- Emotion Field ---
        var emotionField = new TextField("Emotion") { value = emotion };
        emotionField.RegisterValueChangedCallback(evt => emotion = evt.newValue);
        extensionContainer.Add(emotionField);

        // --- Add Choice Button ---
        var button = new Button(() => AddChoicePort()) { text = "Add Choice" };
        extensionContainer.Add(button);

        // --- Output (Next) Port ---
        output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
        output.portName = "Next";
        output.name = "output";
        outputContainer.Add(output);

        // --- Recreate Branching Choice Ports ---
        foreach (var choice in choices)
        {
            var port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            port.portName = choice.portName;
            port.name = "choice_" + choice.portName;

            var labelField = new TextField { value = choice.portName };
            labelField.style.flexGrow = 1;
            labelField.RegisterValueChangedCallback(evt =>
            {
                choice.portName = evt.newValue;
                port.portName = evt.newValue;
            });

            var container = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };
            container.Add(labelField);
            container.Add(port);

            outputContainer.Add(container);
            choicePorts.Add(port);
        }

        RefreshExpandedState();
        RefreshPorts();
    }


    public void AddChoicePort(string portName = "")
    {
        string finalName = string.IsNullOrWhiteSpace(portName) ? $"Choice {choices.Count + 1}" : portName;

        var choice = new DialogueChoice { portName = finalName };
        choices.Add(choice);

        var port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
        port.portName = finalName;
        port.name = "choice_" + finalName;

        // TextField for editable port name
        var nameField = new TextField { value = finalName };
        nameField.style.flexGrow = 1;
        nameField.RegisterValueChangedCallback(evt =>
        {
            choice.portName = evt.newValue;
            port.portName = evt.newValue;
        });

        var container = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };
        container.Add(nameField);
        container.Add(port);

        outputContainer.Add(container);
        choicePorts.Add(port);

        RefreshExpandedState();
        RefreshPorts();
    }
}
