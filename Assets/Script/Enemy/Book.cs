using UnityEngine;

public class TargetBook : MonoBehaviour, IDamageable
{
    public int health = 10;
    private GameObject victoryPanel;

    void Start()
    {
        // Find the player in the scene
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            // Navigate down the hierarchy to find VictoryPanel
            Transform victoryPanelTransform = player.transform
                .Find("Main Camera/UI/Victory/VictoryMenui");

            if (victoryPanelTransform != null)
            {
                victoryPanel = victoryPanelTransform.gameObject;
                victoryPanel.SetActive(false); // ensure it's hidden on start
            }
            else
            {
                Debug.LogWarning("VictoryPanel not found under Player > Main Camera > UI > VictoryCanvas");
            }
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log($"Book took {damage} damage. Remaining health: {health}");

        if (health <= 0)
        {
            Destroy(gameObject);
            ShowVictory();
        }
    }

    void ShowVictory()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }
}
