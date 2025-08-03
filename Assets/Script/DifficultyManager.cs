using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance;

    public int playerPowerLevel = 0;

    public float GetEnemyHealthMultiplier() => 1f + (playerPowerLevel * 0.1f);
    public float GetEnemyDamageMultiplier() => 1f + (playerPowerLevel * 0.1f);
    public float GetEnemySpeedMultiplier() => 1f + (playerPowerLevel * 0.05f);

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else Destroy(gameObject);
    }
}
