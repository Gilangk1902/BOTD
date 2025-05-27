using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedPotion : BuffItem
{
    private float speedMultiplier = .1f;
    protected override void ApplyBuff(GameObject player)
    {
        PlayerStat stats = player.GetComponent<PlayerStat>();
        if (stats != null)
        {
            stats.setMoveSpeed(stats.getMoveSpeed() + stats.getMoveSpeed()*speedMultiplier);
            DifficultyManager.Instance.playerPowerLevel++;
        }
    }
}
