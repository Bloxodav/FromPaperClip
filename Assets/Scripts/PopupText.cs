using UnityEngine;
using TMPro;

public class PopupText : MonoBehaviour
{
    [SerializeField] private float moveDuration = 0.8f;
    [SerializeField] private float holdDuration = 1.5f;
    [SerializeField] private float fadeDuration = 0.6f;

    private RectTransform rectTransform;
    private TextMeshProUGUI tmp;

    private Vector2 startPosition;
    private Vector2 targetPosition;

    private float timer = 0f;
    private enum State { Moving, Holding, Fading }
    private State state = State.Moving;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        tmp = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        startPosition = rectTransform.anchoredPosition;
        targetPosition = new Vector2(rectTransform.anchoredPosition.x, 120f);

        Color c = tmp.color;
        c.a = 0f;
        tmp.color = c;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        switch (state)
        {
            case State.Moving:
                float t = Mathf.Clamp01(timer / moveDuration);
                float curved = EaseOutBack(t);

                rectTransform.anchoredPosition = Vector2.LerpUnclamped(startPosition, targetPosition, curved);

                SetAlpha(Mathf.Clamp01(t * 2f));

                if (timer >= moveDuration)
                {
                    rectTransform.anchoredPosition = targetPosition;
                    SetAlpha(1f);
                    timer = 0f;
                    state = State.Holding;
                }
                break;

            case State.Holding:
                if (timer >= holdDuration)
                {
                    timer = 0f;
                    state = State.Fading;
                }
                break;

            case State.Fading:
                float fadeT = Mathf.Clamp01(timer / fadeDuration);
                SetAlpha(1f - EaseInCubic(fadeT));

                if (timer >= fadeDuration)
                {
                    Destroy(gameObject);
                }
                break;
        }
    }

    private void SetAlpha(float alpha)
    {
        Color c = tmp.color;
        c.a = alpha;
        tmp.color = c;
    }

    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    private float EaseInCubic(float t)
    {
        return t * t * t;
    }
}