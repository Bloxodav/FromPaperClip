using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueNode", menuName = "Dialogues/Node")]
public class DialogueNode : ScriptableObject
{
    public string line;
    public List<DialogueTransition> transitions;
}

[System.Serializable]
public class DialogueTransition
{
    public string choiceText;
    public string conditionKey;
    public string onChoiceEventKey;
    public DialogueNode nextNode;
}