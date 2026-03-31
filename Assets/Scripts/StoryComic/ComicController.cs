namespace Scenes
{
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.SceneManagement;
    using UnityEngine.EventSystems;

    public class ComicController : MonoBehaviour
    {
        [Header("Комикс")]
        public Image[] comicImages;
        public Button nextSceneButton;
        public float delay = 2f;

        private int _currentIndex;
        private float _timer;
        private bool _waiting = true;
        private GameObject _clickZone;

        private void Start()
        {
            CreateClickZone();
            
            foreach (var img in comicImages)
            {
                if (img != null)
                    img.gameObject.SetActive(false);
            }

            if (nextSceneButton != null)
                nextSceneButton.gameObject.SetActive(false);
        }

        private void CreateClickZone()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Canvas не найден!");
                return;
            }

            _clickZone = new GameObject("ClickZone");
            _clickZone.transform.SetParent(canvas.transform, false);
            
            Image img = _clickZone.AddComponent<Image>();
            img.color = new Color(1, 1, 1, 0);
            img.raycastTarget = true;
            
            RectTransform rect = _clickZone.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            EventTrigger trigger = _clickZone.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerClick
            };
            entry.callback.AddListener((data) => { OnScreenClick(); });
            trigger.triggers.Add(entry);
            
            _clickZone.transform.SetSiblingIndex(0);
        }

        private void Update()
        {
            if (_currentIndex >= comicImages.Length)
                return;

            if (_waiting)
            {
                _timer += Time.deltaTime;
                if (_timer >= delay)
                {
                    ShowNextImage();
                }
            }
        }

        private void ShowNextImage()
        {
            if (_currentIndex >= comicImages.Length) return;
            
            comicImages[_currentIndex].gameObject.SetActive(true);
            _currentIndex++;
            _timer = 0f;
            _waiting = true;

            if (_currentIndex >= comicImages.Length && nextSceneButton != null)
            {
                nextSceneButton.gameObject.SetActive(true);
            }
        }

        public void OnScreenClick()
        {
            if (_currentIndex < comicImages.Length)
            {
                ShowNextImage();
            }
        }

        public void GoToNextScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}