using UnityEngine;

public class TargetBook : MonoBehaviour, IDamageable
{
    public int health = 10;
    private GameObject victoryPanel;
    public float rotationSpeed = 30f;      // derajat per detik
    public float floatAmplitude = 0.25f;   // seberapa tinggi naik turun
    public float floatFrequency = 1f;      // seberapa cepat naik turun

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;

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

    void Update()
    {
        // Rotasi Y
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);

        // Gerakan naik-turun (sinusoidal)
        float yOffset = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(startPosition.x, startPosition.y + yOffset, startPosition.z);
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

            GameTimer.Instance.StopTimer();

            var texts = victoryPanel.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
            foreach (var text in texts)
            {
                if (text.name == "Score")
                {
                    text.text = "Victory Time: " + GameTimer.Instance.GetFormattedTime();
                    break;
                }
            }
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }
}
