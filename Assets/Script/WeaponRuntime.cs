using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponRuntime
{
    public WeaponData data;
    public int currentAmmo;
    public float lastFireTime;

    public WeaponRuntime(WeaponData data)
    {
        this.data = data;
        this.currentAmmo = data.maxAmmo;
        this.lastFireTime = -data.fireRate;
    }
}

