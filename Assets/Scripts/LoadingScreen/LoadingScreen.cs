using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
    public string sceneToLoad = "GameScene"; 
    public float minDisplayTime = 3f;
    private float startTime;

    void Start()
    {
        startTime = Time.time;
        StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            if (operation.progress >= 0.9f && Time.time - startTime >= minDisplayTime)
            {
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}