using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryMenu : MonoBehaviour
{
    public string mainMenuSceneName = "MainMenu";

    public void Retry()
    {
        Time.timeScale = 1f; // resume time
        SceneManager.LoadScene("Loading");
    }

    public void MainMenu()
    {
        Time.timeScale = 1f; // resume time
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
