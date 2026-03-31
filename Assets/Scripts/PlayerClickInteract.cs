using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerClickInteract : MonoBehaviour
{
    public LayerMask mask;
    [SerializeField] private GameObject toggleTarget;
    [SerializeField] private AudioSource handAudioSource;
    [field: SerializeField] public InteractableObject selected { get; private set; } = null;

    private PlayerInput playerInput;
    private InputAction clickAction;
    private InputAction moveLeftAction;
    private InputAction moveRightAction;
    private InputAction handAction;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        clickAction = playerInput.actions["Click"];
        moveLeftAction = playerInput.actions["MoveLeft"];
        moveRightAction = playerInput.actions["MoveRight"];
        handAction = playerInput.actions["HandOnE"];
    }

    private void Update()
    {
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        var hit = Physics2D.Raycast(mouseWorld, Vector2.zero, 0f, mask);

        var hovered = hit.collider != null
            ? hit.collider.GetComponent<InteractableObject>()
            : null;

        if (hovered != selected)
        {
            selected?.RemoveHighlight();
            selected = hovered;
            selected?.Highlight();
        }

        if (clickAction.WasPerformedThisFrame())
            selected?.Interact();

        if (moveLeftAction.WasPerformedThisFrame())
            CameraScroller.Instance.MoveLeft();

        if (moveRightAction.WasPerformedThisFrame())
            CameraScroller.Instance.MoveRight();

        if (handAction.WasPerformedThisFrame())
        {
            toggleTarget.SetActive(!toggleTarget.activeSelf);
            if (toggleTarget.activeSelf && handAudioSource != null)
                handAudioSource.Play();
        }
    }
}