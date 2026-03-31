using UnityEngine;
using System.Collections;

public class CameraScroller : MonoBehaviour
{
    public static CameraScroller Instance;

    public float minX = 0f;
    public float maxX = 13.66f;
    public float stepSize = 4.55f;
    public float moveDuration = 0.35f;

    [SerializeField] private AudioSource moveAudioSource;

    private bool _isMoving = false;
    private bool _isLocked = false;

    void Awake() { Instance = this; }

    public void Lock() => _isLocked = true;
    public void Unlock() => _isLocked = false;

    public void MoveLeft()
    {
        if (_isMoving || _isLocked) return;
        float target = Mathf.Max(minX, transform.position.x - stepSize);
        if (Mathf.Approximately(transform.position.x, target)) return;
        PlayMoveSound();
        StartCoroutine(MoveTo(target));
    }

    public void MoveRight()
    {
        if (_isMoving || _isLocked) return;
        float target = Mathf.Min(maxX, transform.position.x + stepSize);
        if (Mathf.Approximately(transform.position.x, target)) return;
        PlayMoveSound();
        StartCoroutine(MoveTo(target));
    }

    private void PlayMoveSound()
    {
        if (moveAudioSource != null)
            moveAudioSource.Play();
    }

    IEnumerator MoveTo(float targetX)
    {
        _isMoving = true;
        float startX = transform.position.x;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;
            t = 1f - Mathf.Pow(1f - t, 3f);
            float newX = Mathf.Lerp(startX, targetX, t);
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
            yield return null;
        }

        transform.position = new Vector3(targetX, transform.position.y, transform.position.z);
        _isMoving = false;
    }
}