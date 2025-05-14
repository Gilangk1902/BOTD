using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Weapon/New Weapon")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public GameObject weaponModelPrefab;
    public GameObject pickupPrefab;

    public int damage = 10;
    public float fireRate = 0.1f;
    public int maxAmmo = 30;
    public float reloadTime = 1.5f;

    public int bulletPerShot = 1;
    public float spreadAmount = 0.02f; // Default spread, bisa diatur di Inspector

}


