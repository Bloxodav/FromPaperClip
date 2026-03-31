using UnityEngine;
using System.Collections;

public class WorldChildMover : MonoBehaviour
{
    [SerializeField] private Transform targetPoint;
    [SerializeField] private float duration = 1f;

    private Vector3 originalPosition;
    private bool initialized = false;
    private Coroutine moveCoroutine;

    public float GetDuration() => duration;

    private void EnsureInitialized()
    {
        if (!initialized)
        {
            originalPosition = transform.position;
            initialized = true;
        }
    }

    public void StartMoving()
    {
        EnsureInitialized();

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        transform.position = originalPosition;
        moveCoroutine = StartCoroutine(MoveRoutine(originalPosition, targetPoint.position, duration));
    }

    public void ResetPosition()
    {
        EnsureInitialized();

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveRoutine(transform.position, originalPosition, duration));
    }

    private IEnumerator MoveRoutine(Vector3 from, Vector3 to, float dur)
    {
        float time = 0f;

        while (time < dur)
        {
            float t = Mathf.SmoothStep(0, 1, time / dur);
            transform.position = Vector3.Lerp(from, to, t);
            time += Time.deltaTime;
            yield return null;
        }

        transform.position = to;
        moveCoroutine = null;
    }
}