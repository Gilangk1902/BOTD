using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject optionsMenuPanel;

    public void PlayGame()
    {
        // Load the next scene in the build settings
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void ShowOptions()
    {
        mainMenuPanel.SetActive(false);
        optionsMenuPanel.SetActive(true);
    }

    public void BackToMainMenu()
    {
        optionsMenuPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
