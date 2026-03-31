using UnityEngine;

public class InteractableSceneObject : InteractableObject
{
    [SerializeField] private GameObject characterCloseup;
    [SerializeField] private DialogueController dialogueController;

    private void Start()
    {
        if (characterCloseup != null)
            characterCloseup.SetActive(false);
    }

    public override void Interact()
    {
        if (characterCloseup != null)
            characterCloseup.SetActive(true);

        RemoveHighlight();
        gameObject.SetActive(false);

        dialogueController.OnDialogueEnd += OnDialogueEnded;
        dialogueController.StartDialogue();
    }

    private void OnDialogueEnded()
    {
        dialogueController.OnDialogueEnd -= OnDialogueEnded;
        gameObject.SetActive(true);
    }
}