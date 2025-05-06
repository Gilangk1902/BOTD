using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponRuntime
{
    public WeaponData data;
    public int currentAmmo;
    public float lastFireTime;
    private PlayerStat playerStat;

    public WeaponRuntime(WeaponData data, PlayerStat playerStat)
    {
        this.data = data;
        this.playerStat = playerStat;
        this.currentAmmo = data.maxAmmo + (int) (data.maxAmmo * playerStat.getAdditionalClipSize());
        this.lastFireTime = -data.fireRate;
    }
}

