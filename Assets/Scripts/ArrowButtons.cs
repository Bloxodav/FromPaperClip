using UnityEngine;
using UnityEngine.UI;

public class ArrowButtons : MonoBehaviour
{
    public Button leftButton;
    public Button rightButton;
    [SerializeField] private AudioSource leftSound;
    [SerializeField] private AudioSource rightSound;

    void Start()
    {
        leftButton.onClick.AddListener(() =>
        {
            if (leftSound != null) leftSound.Play();
            CameraScroller.Instance.MoveLeft();
        });

        rightButton.onClick.AddListener(() =>
        {
            if (rightSound != null) rightSound.Play();
            CameraScroller.Instance.MoveRight();
        });
    }
}