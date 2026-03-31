using UnityEngine;

public class HeldItemBobbing : MonoBehaviour
{
    [SerializeField] private float amplitude = 0.08f;
    [SerializeField] private float frequency = 1.8f;

    private Vector3 startLocalPos;

    private void OnEnable()
    {
        startLocalPos = transform.localPosition;
    }

    private void Update()
    {
        float offset = Mathf.Sin(Time.time * frequency * Mathf.PI * 2f) * amplitude;
        transform.localPosition = startLocalPos + new Vector3(0f, offset, 0f);
    }

    private void OnDisable()
    {
        transform.localPosition = startLocalPos;
    }
}
