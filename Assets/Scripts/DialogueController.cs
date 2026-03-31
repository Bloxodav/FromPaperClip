using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using TMPro;

public class DialogueController : MonoBehaviour
{
    [System.Serializable]
    public class KeyEvent
    {
        public string KeyCode;
        public UnityEvent Event;
    }

    [field: SerializeField] public DialogueNode firstTimeDialogue { get; private set; }
    [field: SerializeField] public DialogueNode dialogueStart { get; private set; }
    [field: SerializeField] public DialogueNode currentNode { get; private set; }
    [field: SerializeField] public PlayerProgressManager progressManager { get; private set; }
    [field: SerializeField] public List<KeyEvent> dialogueChoiceEvents { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject characterCloseup;
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private TextMeshProUGUI[] choiceTexts;

    [Header("Objects")]
    [SerializeField] private GameObject[] objectsToHide;
    [SerializeField] private GameObject handObject;

    [Header("Audio")]
    [SerializeField] private AudioSource choiceSound;
    [SerializeField] private float typeClickFrequency = 480f;
    [SerializeField] private float typeClickVolume = 0.18f;

    [Header("Timing")]
    [SerializeField] private float typeSpeed = 0.04f;
    [SerializeField] private float choiceFadeInDelay = 0.15f;

    [Header("Auto Start")]
    [SerializeField] private bool startOnAwake = false;

    private PlayerInput playerInput;
    private InputAction continueAction;
    private InputAction choice1Action;
    private InputAction choice2Action;
    private InputAction choice3Action;
    private InputAction choice4Action;

    private bool firstTimeChatting = true;
    private bool isTyping = false;
    private bool isInDialogue = false;
    private Coroutine typeCoroutine;

    private AudioSource typeAudioSource;

    public event System.Action OnDialogueEnd;

    private void Awake()
    {
        if (progressManager == null)
            progressManager = FindFirstObjectByType<PlayerProgressManager>();

        playerInput = FindFirstObjectByType<PlayerInput>();
        continueAction = playerInput.actions["Continue"];
        choice1Action = playerInput.actions["Choice1"];
        choice2Action = playerInput.actions["Choice2"];
        choice3Action = playerInput.actions["Choice3"];
        choice4Action = playerInput.actions["Choice4"];

        typeAudioSource = gameObject.AddComponent<AudioSource>();
        typeAudioSource.volume = typeClickVolume;
        typeAudioSource.playOnAwake = false;
    }

    private void Start()
    {
        if (startOnAwake)
            StartDialogue();
    }

    private AudioClip GenerateClickClip(float frequency)
    {
        int sampleRate = 44100;
        int samples = sampleRate / 40;
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = 1f - (float)i / samples;
            data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * envelope;
        }

        AudioClip clip = AudioClip.Create("typeClick", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private void PlayTypeClick()
    {
        float freq = typeClickFrequency + Random.Range(-30f, 30f);
        typeAudioSource.clip = GenerateClickClip(freq);
        typeAudioSource.Play();
    }

    private void Update()
    {
        if (!isInDialogue) return;

        if (currentNode == null)
        {
            EndDialogue();
            return;
        }

        if (isTyping)
        {
            if (continueAction.WasPerformedThisFrame())
                SkipTyping();
            return;
        }

        if (currentNode.transitions.Count == 1)
        {
            if (continueAction.WasPerformedThisFrame())
                AdvanceContinue();
        }
        else if (currentNode.transitions.Count > 1)
        {
            if (choice1Action.WasPerformedThisFrame()) SelectChoice(0);
            if (choice2Action.WasPerformedThisFrame()) SelectChoice(1);
            if (choice3Action.WasPerformedThisFrame()) SelectChoice(2);
            if (choice4Action.WasPerformedThisFrame()) SelectChoice(3);
        }
        else
        {
            currentNode = null;
        }
    }

    private void FireTransitionEvent(DialogueTransition transition)
    {
        if (!string.IsNullOrEmpty(transition.onChoiceEventKey))
            dialogueChoiceEvents.Find(e => e.KeyCode == transition.onChoiceEventKey)?.Event.Invoke();
    }

    private void AdvanceContinue()
    {
        if (currentNode.transitions == null || currentNode.transitions.Count == 0)
        {
            EndDialogue();
            return;
        }

        var transition = currentNode.transitions[0];
        FireTransitionEvent(transition);
        currentNode = transition.nextNode;
        ShowNode(currentNode);
    }

    private void SelectChoice(int index)
    {
        if (index < 0 || index >= currentNode.transitions.Count) return;

        var transition = currentNode.transitions[index];

        if (!progressManager.checkProgress(transition.conditionKey))
            return;

        if (choiceSound != null)
            choiceSound.Play();

        FireTransitionEvent(transition);
        currentNode = transition.nextNode;
        ShowNode(currentNode);
    }

    private void ShowNode(DialogueNode node)
    {
        if (node == null)
        {
            EndDialogue();
            return;
        }

        foreach (var choice in choiceTexts)
            choice.gameObject.SetActive(false);

        if (typeCoroutine != null)
            StopCoroutine(typeCoroutine);

        typeCoroutine = StartCoroutine(TypeLine(node));
    }

    private IEnumerator TypeLine(DialogueNode node)
    {
        isTyping = true;
        speakerText.text = "";
        speakerText.maxVisibleCharacters = 0;
        speakerText.text = node.line;

        int total = node.line.Length;

        for (int i = 0; i <= total; i++)
        {
            speakerText.maxVisibleCharacters = i;

            if (i > 0)
            {
                char c = node.line[i - 1];

                if (c != ' ')
                    PlayTypeClick();

                if (c == '.' || c == '!' || c == '?')
                    yield return new WaitForSeconds(typeSpeed * 6f);
                else if (c == ',')
                    yield return new WaitForSeconds(typeSpeed * 3f);
                else
                    yield return new WaitForSeconds(typeSpeed);
            }
        }

        isTyping = false;

        if (node.transitions.Count > 1)
            StartCoroutine(ShowChoices(node));
    }

    private IEnumerator ShowChoices(DialogueNode node)
    {
        for (int i = 0; i < choiceTexts.Length; i++)
        {
            if (i < node.transitions.Count)
            {
                choiceTexts[i].text = $"{i + 1}. {node.transitions[i].choiceText}";
                choiceTexts[i].gameObject.SetActive(true);

                var btn = choiceTexts[i].GetComponent<UnityEngine.UI.Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    int captured = i;
                    btn.onClick.AddListener(() => SelectChoice(captured));
                }

                yield return StartCoroutine(FadeInText(choiceTexts[i]));
                yield return new WaitForSeconds(choiceFadeInDelay);
            }
        }
    }

    private IEnumerator FadeInText(TextMeshProUGUI tmp)
    {
        Color c = tmp.color;
        c.a = 0f;
        tmp.color = c;

        float elapsed = 0f;
        float duration = 0.2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Clamp01(elapsed / duration);
            tmp.color = c;
            yield return null;
        }

        c.a = 1f;
        tmp.color = c;
    }

    private void SkipTyping()
    {
        if (typeCoroutine != null)
            StopCoroutine(typeCoroutine);

        speakerText.maxVisibleCharacters = speakerText.text.Length;
        isTyping = false;

        if (currentNode.transitions.Count > 1)
            StartCoroutine(ShowChoices(currentNode));
    }

    private void EndDialogue()
    {
        isInDialogue = false;
        dialoguePanel.SetActive(false);

        foreach (var obj in objectsToHide)
            if (obj != null) obj.SetActive(true);

        CameraScroller.Instance.Unlock();

        if (characterCloseup != null)
            characterCloseup.SetActive(false);

        currentNode = null;
        OnDialogueEnd?.Invoke();
    }

    public void StartDialogue()
    {
        if (isInDialogue) return;

        foreach (var obj in objectsToHide)
            if (obj != null) obj.SetActive(false);

        if (handObject != null)
            handObject.SetActive(false);

        CameraScroller.Instance.Lock();

        if (firstTimeChatting && firstTimeDialogue != null)
        {
            currentNode = firstTimeDialogue;
            firstTimeChatting = false;
        }
        else
            currentNode = dialogueStart;

        isInDialogue = true;
        dialoguePanel.SetActive(true);
        ShowNode(currentNode);
    }
}