using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingController : MonoBehaviour
{
    public string sceneToLoad = "SampleScene"; // Ubah ke nama scene game-mu
    public Slider progressBar; // opsional, kalau pakai slider

    void Start()
    {
        StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneToLoad);
        async.allowSceneActivation = false;

        while (!async.isDone)
        {
            float progress = Mathf.Clamp01(async.progress / 0.9f);

            if (progressBar != null)
                progressBar.value = progress;

            // Auto activate scene jika sudah siap
            if (async.progress >= 0.9f)
            {
                yield return new WaitForSeconds(0.5f); // jeda sejenak (opsional)
                async.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
