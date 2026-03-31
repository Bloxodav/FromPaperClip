using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DealManager : MonoBehaviour
{
    public DealStage[] stages;
    public Transform[] slots;
    public TextMeshProUGUI sentenceText;

    [SerializeField] private AudioSource wordSelectSound;

    public float flyToSlotDuration = 0.5f;
    public float fadeDuration = 0.3f;
    public int successThreshold = 1;

    private int currentStage = 0;
    private int totalPoints = 0;
    private bool stageLocked = false;
    private List<WordData> activeWords = new List<WordData>();

    private void Start()
    {
        sentenceText.text = "";

        foreach (var stage in stages)
            foreach (var word in stage.words)
            {
                word.startPosition = word.wordObject.transform.position;
                word.wordObject.SetActive(false);
            }

        LaunchStage(0);
    }

    private void LaunchStage(int stageIndex)
    {
        activeWords.Clear();
        stageLocked = false;

        DealStage stage = stages[stageIndex];
        int wordCount = stage.words.Length;
        int missedCount = 0;

        foreach (var word in stage.words)
            StartCoroutine(FlyWordUp(word, stage.flySpeed, () =>
            {
                missedCount++;
                if (missedCount >= wordCount && !stageLocked)
                    Debug.Log("Сделка провалена. Все слова пропущены.");
            }));
    }

    private IEnumerator FlyWordUp(WordData word, float speed, System.Action onMissed)
    {
        GameObject obj = word.wordObject;
        obj.SetActive(true);

        Transform tr = obj.transform;
        Vector3 startPos = tr.position;
        Vector3 centerPos = new Vector3(startPos.x, 0f, startPos.z);

        var btn = obj.GetComponent<UnityEngine.UI.Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            WordData captured = word;
            btn.onClick.AddListener(() => { if (!stageLocked) OnWordClicked(captured); });
        }

        activeWords.Add(word);

        while (Vector3.Distance(tr.position, centerPos) > 0.01f)
        {
            if (stageLocked) yield break;
            tr.position = Vector3.MoveTowards(tr.position, centerPos, speed * Time.deltaTime);
            yield return null;
        }

        tr.position = centerPos;

        if (!stageLocked)
        {
            StartCoroutine(FadeOutWord(word.wordObject, () =>
            {
                word.wordObject.SetActive(false);
                word.wordObject.transform.position = startPos;
                onMissed?.Invoke();
            }));
        }
    }

    private void OnWordClicked(WordData word)
    {
        stageLocked = true;
        totalPoints += word.points;

        if (wordSelectSound != null)
            wordSelectSound.Play();

        Debug.Log($"Выбрано: \"{word.text}\" ({word.points:+#;-#;0}). Итого: {totalPoints}");

        StartCoroutine(FlyToSlot(word.wordObject, slots[currentStage], () =>
        {
            DealStage stage = stages[currentStage];

            sentenceText.text += word.text + stage.postfix;

            if (stage.slotUnderline != null)
                stage.slotUnderline.SetActive(false);

            word.wordObject.SetActive(false);
            word.wordObject.transform.position = GetStartPos(word);

            StartCoroutine(FadeOutAllExcept(word, () =>
            {
                currentStage++;
                if (currentStage < stages.Length)
                    LaunchStage(currentStage);
                else
                    FinishDeal();
            }));
        }));
    }

    private IEnumerator FlyToSlot(GameObject obj, Transform slot, System.Action onDone)
    {
        Transform tr = obj.transform;
        Vector3 startPos = tr.position;
        Vector3 targetPos = slot.position;

        float elapsed = 0f;
        while (elapsed < flyToSlotDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / flyToSlotDuration);
            tr.position = Vector3.Lerp(startPos, targetPos, EaseInOutCubic(t));
            yield return null;
        }

        tr.position = targetPos;
        onDone?.Invoke();
    }

    private IEnumerator FadeOutAllExcept(WordData chosen, System.Action onDone)
    {
        List<CanvasGroup> groups = new List<CanvasGroup>();

        foreach (var w in activeWords)
        {
            if (w == chosen) continue;
            if (!w.wordObject.activeSelf) continue;

            var cg = w.wordObject.GetComponent<CanvasGroup>();
            if (cg == null) cg = w.wordObject.AddComponent<CanvasGroup>();
            groups.Add(cg);
        }

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);
            foreach (var cg in groups) cg.alpha = alpha;
            yield return null;
        }

        foreach (var w in activeWords)
        {
            if (w == chosen) continue;
            w.wordObject.SetActive(false);
            var cg = w.wordObject.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 1f;
            w.wordObject.transform.position = w.startPosition;
        }

        onDone?.Invoke();
    }

    private IEnumerator FadeOutWord(GameObject obj, System.Action onDone)
    {
        var cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.AddComponent<CanvasGroup>();

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        cg.alpha = 1f;
        onDone?.Invoke();
    }

    private void FinishDeal()
    {
        if (totalPoints >= successThreshold)
            Debug.Log($"Сделка прошла успешно! Очки: {totalPoints}");
        else
            Debug.Log($"Сделка провалена. Очки: {totalPoints}");
    }

    private Vector3 GetStartPos(WordData word) => word.startPosition;

    private float EaseInOutCubic(float t)
    {
        return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
    }
}