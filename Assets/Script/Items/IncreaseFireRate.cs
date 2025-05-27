using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncreaseFireRate : BuffItem
{
    private float speedMultiplier = .1f;
    protected override void ApplyBuff(GameObject player)
    {
        PlayerStat stats = player.GetComponent<PlayerStat>();
        if (stats != null)
        {
            stats.setAdditionalFireRate(stats.getAdditionalFireRate() + speedMultiplier);
            DifficultyManager.Instance.playerPowerLevel++;
        }
    }
}
