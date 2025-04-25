using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public WeaponData weaponData;

    private void OnTriggerEnter(Collider other)
    {
        WeaponController controller = other.GetComponent<WeaponController>();
        if (controller != null)
        {
            if (controller.PickupWeapon(weaponData))
            {
                Destroy(gameObject);
            }
        }
    }
}
