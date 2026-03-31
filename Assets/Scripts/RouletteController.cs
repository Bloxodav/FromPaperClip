using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using static RouletteController;

public class RouletteController : MonoBehaviour
{
    [System.Serializable]
    public class Prize
    {
        public string prizeName;
        public Sprite icon;
        [Range(0, 100)]
        public float weight;
    }

    [Header("Призы")]
    public Prize[] prizes;

    [Header("UI")]
    public Transform itemStrip;
    public GameObject itemPrefab;
    public Button spinButton;
    public GameObject roulettePanel;

    [Header("Указатель")]
    public Transform pointer;

    [Header("Настройки")]
    public int totalItems = 50;
    public float itemWidth = 160f;
    public float spinDuration = 7f;
    public float closePanelDelay = 3f;
    public AnimationCurve spinCurve;

    [Header("Звук")]
    [SerializeField] private AudioSource winSound;
    [SerializeField] private float tickVolume = 0.3f;
    [SerializeField] private float tickFrequency = 600f;

    private List<GameObject> _stripItems = new List<GameObject>();
    private bool _isSpinning = false;
    private int _winnerPrizeIndex;
    private int _winnerStripPosition;
    private int _lastTickIndex = -1;
    private AudioSource _tickAudioSource;

    void Start()
    {
        if (spinCurve == null || spinCurve.length == 0)
        {
            spinCurve = new AnimationCurve(
                new Keyframe(0f, 0f, 0f, 2f),
                new Keyframe(1f, 1f, 0f, 0f)
            );
        }

        _tickAudioSource = gameObject.AddComponent<AudioSource>();
        _tickAudioSource.volume = tickVolume;
        _tickAudioSource.playOnAwake = false;

        spinButton.onClick.AddListener(StartSpin);
        BuildStrip();
    }

    private AudioClip GenerateTickClip(float frequency)
    {
        int sampleRate = 44100;
        int samples = sampleRate / 30;
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = 1f - (float)i / samples;
            data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * envelope * envelope;
        }

        AudioClip clip = AudioClip.Create("tick", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private void PlayTick()
    {
        float freq = tickFrequency + Random.Range(-40f, 40f);
        _tickAudioSource.clip = GenerateTickClip(freq);
        _tickAudioSource.Play();
    }

    int PickWinner()
    {
        float totalWeight = 0f;
        foreach (var p in prizes) totalWeight += p.weight;

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < prizes.Length; i++)
        {
            cumulative += prizes[i].weight;
            if (roll <= cumulative) return i;
        }
        return 0;
    }

    void BuildStrip()
    {
        foreach (Transform child in itemStrip)
            Destroy(child.gameObject);
        _stripItems.Clear();

        _winnerStripPosition = totalItems - 8;

        for (int i = 0; i < totalItems; i++)
        {
            int prizeIndex = GetRandomPrizeIndex();

            GameObject item = Instantiate(itemPrefab, itemStrip);
            SpriteRenderer sr = item.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = prizes[prizeIndex].icon;
                sr.sortingOrder = 2;
            }
            item.transform.localPosition = new Vector3(i * itemWidth, 0, 0);
            _stripItems.Add(item);
        }

        itemStrip.localPosition = Vector3.zero;
    }

    void PlaceWinner(int winnerIndex)
    {
        SpriteRenderer sr = _stripItems[_winnerStripPosition].GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.sprite = prizes[winnerIndex].icon;
    }

    int GetRandomPrizeIndex() => Random.Range(0, prizes.Length);

    public void StartSpin()
    {
        if (_isSpinning) return;
        _winnerPrizeIndex = PickWinner();
        PlaceWinner(_winnerPrizeIndex);
        _lastTickIndex = -1;
        StartCoroutine(SpinRoutine());
    }

    IEnumerator SpinRoutine()
    {
        _isSpinning = true;
        spinButton.interactable = false;

        float stripStartX = itemStrip.position.x;
        float pointerX = pointer != null ? pointer.position.x : 0f;
        float winnerWorldX = stripStartX + _winnerStripPosition * itemWidth;
        float targetX = itemStrip.localPosition.x - (winnerWorldX - pointerX);

        float startX = itemStrip.localPosition.x;
        float elapsed = 0f;

        while (elapsed < spinDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / spinDuration);
            float eased = spinCurve.Evaluate(t);
            float newX = Mathf.Lerp(startX, targetX, eased);
            itemStrip.localPosition = new Vector3(newX, 0, 0);

            CheckTick(pointerX);

            yield return null;
        }

        itemStrip.localPosition = new Vector3(targetX, 0, 0);

        yield return StartCoroutine(BounceSettle(targetX));

        _isSpinning = false;
        spinButton.interactable = true;

        OnSpinFinished();
    }

    private void CheckTick(float pointerX)
    {
        float stripWorldX = itemStrip.position.x;
        float relativePointer = pointerX - stripWorldX;
        int currentIndex = Mathf.RoundToInt(relativePointer / itemWidth);

        if (currentIndex != _lastTickIndex)
        {
            _lastTickIndex = currentIndex;
            PlayTick();
        }
    }

    IEnumerator BounceSettle(float targetX)
    {
        float bounceAmount = itemWidth * 0.12f;
        float bounceDuration = 0.25f;
        float elapsed = 0f;

        float overshootX = targetX - bounceAmount;
        Vector3 start = itemStrip.localPosition;

        while (elapsed < bounceDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / bounceDuration;
            itemStrip.localPosition = new Vector3(Mathf.Lerp(start.x, overshootX, EaseOutCubic(t)), 0, 0);
            yield return null;
        }

        elapsed = 0f;
        start = itemStrip.localPosition;

        while (elapsed < bounceDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / bounceDuration;
            itemStrip.localPosition = new Vector3(Mathf.Lerp(start.x, targetX, EaseOutCubic(t)), 0, 0);
            yield return null;
        }

        itemStrip.localPosition = new Vector3(targetX, 0, 0);
    }

    void OnSpinFinished()
    {
        Debug.Log("Выиграл: " + prizes[_winnerPrizeIndex].prizeName);

        if (winSound != null)
            winSound.Play();

        StartCoroutine(CloseAfterDelay());
    }

    IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(closePanelDelay);
        if (roulettePanel != null)
            roulettePanel.SetActive(false);
    }

    float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
}