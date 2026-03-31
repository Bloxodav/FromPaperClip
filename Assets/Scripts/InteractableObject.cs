using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SpriteRenderer))]
public class InteractableObject : MonoBehaviour
{
    [SerializeField] private Color outlineColor = Color.yellow;
    [SerializeField] private float outlineSize = 2f;

    [SerializeField] protected bool hasBeenInteractedWith = false;
    [SerializeField] protected UnityEvent onInteraction;
    [SerializeField] protected UnityEvent onSecondInteraction;

    private SpriteRenderer spriteRenderer;
    private Material outlineMaterial;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        outlineMaterial = new Material(Shader.Find("Custom/SpriteOutline"));
        outlineMaterial.mainTexture = spriteRenderer.sprite != null ? spriteRenderer.sprite.texture : null;
        outlineMaterial.SetColor("_OutlineColor", outlineColor);
        outlineMaterial.SetFloat("_OutlineSize", outlineSize);
        outlineMaterial.SetFloat("_Outline", 0f);

        spriteRenderer.material = outlineMaterial;
    }

    public void Highlight()
    {
        outlineMaterial.SetFloat("_Outline", 1f);
    }

    public void RemoveHighlight()
    {
        outlineMaterial.SetFloat("_Outline", 0f);
    }

    public virtual void Interact()
    {
        if (hasBeenInteractedWith && onSecondInteraction != null)
            onSecondInteraction.Invoke();
        else
            onInteraction.Invoke();

        hasBeenInteractedWith = !hasBeenInteractedWith;
    }
}