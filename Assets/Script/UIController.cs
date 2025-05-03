using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    public TextMeshProUGUI ammoText; // Assign via Inspector
    public WeaponController weaponController; // Assign WeaponController from Player

    void Update()
    {
        UpdateAmmoUI();
    }

    void UpdateAmmoUI()
    {
        if (weaponController == null || weaponController.weaponSlots[weaponController.currentSlot] == null)
        {
            ammoText.text = "- / -";
            return;
        }

        WeaponRuntime currentWeapon = weaponController.GetCurrentRuntime();
        if (currentWeapon != null)
        {
            int currentAmmo = currentWeapon.currentAmmo;
            int maxAmmo = currentWeapon.data.maxAmmo;
            ammoText.text = $"{currentAmmo} / {maxAmmo}";
        }
        else
        {
            ammoText.text = "- / -";
        }
    }
}

