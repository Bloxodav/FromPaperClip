using UnityEngine;

public class InteractableItemObject : InteractableObject
{
    [field: SerializeField] public float moveAmplitude { get; private set; } = 0.25f;
    [field: SerializeField] public bool isHeld { get; private set; } = false;

    private Vector3 heldPos = Vector3.zero;

    public void Grab()
    {
        isHeld = true;
        heldPos = transform.position;
    }

    public void Drop()
    {
        isHeld = false;
        heldPos = Vector3.zero;
    }

    public override void Interact()
    {
        onInteraction.Invoke();
    }

    private void Update()
    {
        if (isHeld)
            transform.position = new Vector3(transform.position.x, heldPos.y + moveAmplitude * Mathf.Sin(Time.fixedTime), transform.position.z);
    }
}