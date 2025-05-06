using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    public TextMeshProUGUI ammoText; // Assign via Inspector
    public WeaponController weaponController; // Assign WeaponController from Player
    public PlayerStat playerStat;
    public TextMeshProUGUI healthText;

    void Update()
    {
        UpdateAmmoUI();
        UpdateHealth();
    }

    void UpdateHealth()
    {
        healthText.text = playerStat.getCurrentHealth().ToString();
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
            int maxAmmo = currentWeapon.data.maxAmmo + (int)(currentWeapon.data.maxAmmo * weaponController.GetPlayerStat().getAdditionalClipSize());
            ammoText.text = $"{currentAmmo} / {maxAmmo}";
        }
        else
        {
            ammoText.text = "- / -";
        }
    }
}

