using UnityEngine;

public class ParallaxPointClick : MonoBehaviour
{
    public Transform cameraTransform;
    [Range(0f, 1f)]
    public float parallaxStrength = 0.3f;

    private Vector3 initialPosition;

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        initialPosition = transform.position;
    }

    void LateUpdate()
    {
        float cameraX = cameraTransform.position.x;
        float newX = initialPosition.x + cameraX * parallaxStrength;

        transform.position = new Vector3(
            newX,
            initialPosition.y,
            initialPosition.z
        );
    }
}