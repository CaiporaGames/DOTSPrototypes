using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ChoiceUI : MonoBehaviour 
{
    [SerializeField] private Button choiceButton = null;
    [SerializeField] private TextMeshProUGUI choiceText = null;

    private DialogueNodeData dialogueNodeData = null;

    private void Start() 
    {
        choiceButton.onClick.RemoveAllListeners();
        choiceButton.onClick.AddListener(() => OnChoiceButtonClicked());
    }

    public void SetupChoice(DialogueNodeData node, int choiceIndex)
    {
        choiceText.text = node.choices[choiceIndex].portName;
        dialogueNodeData = node;
    }

    private void OnChoiceButtonClicked()
    {
        
    }

    private void OnDestroy() 
    {
        choiceButton.onClick.RemoveAllListeners();
    }
} 