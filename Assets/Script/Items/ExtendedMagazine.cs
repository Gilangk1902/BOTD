using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtendedMagazine : BuffItem
{
    public float clipSizeMultiplier = .25f;

    protected override void ApplyBuff(GameObject player)
    {
        PlayerStat stats = player.GetComponent<PlayerStat>();
        if (stats != null)
        {
            stats.setAdditionalClipSize(stats.getAdditionalClipSize() + clipSizeMultiplier);
        }
    }
}
