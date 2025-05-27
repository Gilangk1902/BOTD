using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtraBullets : BuffItem
{
    [SerializeField] private int extraBullets = 1;
    protected override void ApplyBuff(GameObject player)
    {
        PlayerStat stats = player.GetComponent<PlayerStat>();
        if (stats != null)
        {
            stats.setExtraBulletPerShot(stats.getExtraBulletPerShot() + extraBullets);
            DifficultyManager.Instance.playerPowerLevel++;
        }
    }
}
