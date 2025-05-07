using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

public class DialogueNode : Node
{
    public string GUID;
    public string speaker;
    public string text;
    public string emotion;

    public Port input;
    public List<Port> outputs = new();
    public List<DialogueChoice> choices = new(); // persistent choice data

    public void Draw()
    {
        title = speaker;

        extensionContainer.Clear();

        inputContainer.Clear();
        outputContainer.Clear();
        outputs.Clear();

        // --- INPUT PORT ---
        input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
        input.portName = "Input";
        input.name = "input";
        inputContainer.Add(input);

        // --- FIELDS ---
        var speakerField = new TextField("Speaker") { value = speaker };
        speakerField.RegisterValueChangedCallback(evt =>
        {
            speaker = evt.newValue;
            title = speaker;
        });
        mainContainer.Add(speakerField);

        var textField = new TextField("Text") { value = text, multiline = true };
        textField.RegisterValueChangedCallback(evt => text = evt.newValue);
        mainContainer.Add(textField);

        var emotionField = new TextField("Emotion") { value = emotion };
        emotionField.RegisterValueChangedCallback(evt => emotion = evt.newValue);
        mainContainer.Add(emotionField);

        var button = new Button(() => AddChoicePort()) { text = "Add Choice" };
        mainContainer.Add(button);

        // --- OUTPUT PORTS FROM CHOICES ---
        foreach (var choice in choices)
        {
            var port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            port.portName = choice.portName;
            outputContainer.Add(port);
            outputs.Add(port);
        }

        RefreshExpandedState();
        RefreshPorts();
    }

    public void AddChoicePort(string portName = "")
    {
        string finalName = string.IsNullOrWhiteSpace(portName) ? $"Choice {choices.Count + 1}" : portName;

        // Save to model
        choices.Add(new DialogueChoice { portName = finalName });

        // Create port visually
        var port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
        port.portName = finalName;
        outputContainer.Add(port);
        outputs.Add(port);

        RefreshExpandedState();
        RefreshPorts();
    }
}
