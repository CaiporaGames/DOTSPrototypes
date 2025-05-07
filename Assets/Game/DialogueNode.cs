// DialogueManager.cs updated to use Next output and branching choices with condition evaluation
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Unity.VisualScripting;
using System.Linq;

public class DialogueNode : Node
{
    public DialogueNodeType nodeType = DialogueNodeType.Dialogue;   
    public Color? color = null;

    public string GUID;
    public string speaker;
    public string text;
    public string emotion;

    public Port input;
    public Port output;
    public List<Port> choicePorts = new();
    public List<DialogueChoice> choices = new();

    public void Draw()
    {
        title = speaker;

        extensionContainer.Clear();
        inputContainer.Clear();
        outputContainer.Clear();
        choicePorts.Clear();

        var nodeTypeField = new EnumField("Node Type", nodeType);
        nodeTypeField.RegisterValueChangedCallback(evt => nodeType = (DialogueNodeType)evt.newValue);
        extensionContainer.Add(nodeTypeField);

        input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
        input.portName = "Input";
        input.name = "input";
        inputContainer.Add(input);

        var speakerField = new TextField("Speaker") { value = speaker };
        speakerField.RegisterValueChangedCallback(evt => { speaker = evt.newValue; title = speaker; });
        extensionContainer.Add(speakerField);

        var textField = new TextField("Text") { value = text, multiline = true };
        textField.RegisterValueChangedCallback(evt => text = evt.newValue);
        extensionContainer.Add(textField);

        var emotionField = new TextField("Emotion") { value = emotion };
        emotionField.RegisterValueChangedCallback(evt => emotion = evt.newValue);
        extensionContainer.Add(emotionField);

        var colorField = new ColorField("Color Override") { value = color ?? Color.clear };
        colorField.RegisterValueChangedCallback(evt => { color = evt.newValue; UpdateNodeColor(); });
        extensionContainer.Add(colorField);

        var addChoiceButton = new Button(() => AddChoicePort()) { text = "Add Choice" };
        extensionContainer.Add(addChoiceButton);

        output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
        output.portName = "Next";
        output.name = "output";
        outputContainer.Add(output);

        // --- Recreate Branching Choice Ports ---
        choices = choices.DistinctBy(c => c.portName).ToList(); 
        for (int i = 0; i < choices.Count; i++)
        {
            var choice = choices[i];

            var port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            port.portName = choice.portName;
            port.name = "choice_" + choice.portName;
            choicePorts.Add(port);

            var container = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };

            var nameField = new TextField { value = choice.portName };
            nameField.style.flexGrow = 1;
            nameField.RegisterValueChangedCallback(evt => {
                choice.portName = evt.newValue;
                port.portName = evt.newValue;
            });
            container.Add(nameField);

            var conditionField = new TextField { value = choice.condition };
            conditionField.style.flexGrow = 1;
            conditionField.RegisterValueChangedCallback(evt => choice.condition = evt.newValue);
            container.Add(conditionField);

            var removeButton = new Button(() => {
                choices.Remove(choice);
                Draw();
            }) { text = "X" };
            removeButton.style.marginLeft = 4;
            removeButton.style.unityFontStyleAndWeight = FontStyle.Bold;
            container.Add(removeButton);

            container.Add(port);
            extensionContainer.Add(container);
        }

        RefreshExpandedState();
        RefreshPorts();
        UpdateNodeColor();
    }

    public void UpdateNodeColor()
    {
        Color colorToUse = color.HasValue && color.Value.a > 0.01f
            ? color.Value
            : nodeType == DialogueNodeType.Start ? Color.green
            : nodeType == DialogueNodeType.End ? Color.red
            : Color.gray;

        style.borderBottomColor = colorToUse;
        style.borderTopColor = colorToUse;
        style.borderLeftColor = colorToUse;
        style.borderRightColor = colorToUse;
        style.borderBottomWidth = 4;
        style.borderTopWidth = 4;
        style.borderLeftWidth = 4;
        style.borderRightWidth = 4;
    }

    public void AddChoicePort(string portName = "")
    {
        string finalName = string.IsNullOrWhiteSpace(portName) ? $"Choice {choices.Count + 1}" : portName;
        var choice = new DialogueChoice { portName = finalName, condition = "" };
        choices.Add(choice);
        Draw();
    }
}
