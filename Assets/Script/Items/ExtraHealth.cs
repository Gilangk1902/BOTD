using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtraHealth : BuffItem
{
    private int extraHealth = 10;
    protected override void ApplyBuff(GameObject player)
    {
        PlayerStat stats = player.GetComponent<PlayerStat>();
        if (stats != null)
        {
            stats.setMaxHealth(stats.getMaxHealth() + extraHealth);
            stats.setCurrentHealth(stats.getCurrentHealth() + extraHealth);
            DifficultyManager.Instance.playerPowerLevel++;
        }
    }
}
