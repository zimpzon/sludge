using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScript : MonoBehaviour
{
    string mainSceneName = "Scene";

    void Start()
    {
        StartCoroutine(LoadMainScene());
    }

    IEnumerator LoadMainScene()
    {
        float minTime = 0.5f;
        float timer = 0;

        AsyncOperation op = SceneManager.LoadSceneAsync(mainSceneName);
        op.allowSceneActivation = false;

        while (timer < minTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        op.allowSceneActivation = true;
    }
}
