using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour, InteractPrompt
{
    public WeaponData weaponData;

    public string GetPromptText()
    {
        return "Pick Up Weapon";
    }

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
