using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public Transform weaponHolder;
    private GameObject[] weaponObjects = new GameObject[2];
    public WeaponData[] weaponSlots = new WeaponData[2];
    public int currentSlot = 0;
    public Camera playerCamera;
    public float pickupRange = 3f;

    private GameObject currentWeaponObject;
    private WeaponRuntime[] weaponStates = new WeaponRuntime[2];
    private bool isReloading = false;
    public float meleeCooldown = 1f;
    private float lastMeleeTime = -999f;
    public LayerMask shootableLayers = Physics.DefaultRaycastLayers; // assign in Inspector

    [SerializeField] private PlayerStat playerStat;

    private void Start()
    {
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i] != null)
            {
                weaponObjects[i] = Instantiate(weaponSlots[i].weaponModelPrefab, weaponHolder);
                SetWeaponPhysics(weaponObjects[i], false);
                weaponObjects[i].SetActive(false);

                weaponStates[i] = new WeaponRuntime(weaponSlots[i], playerStat); // <- tambahkan ini
            }
        }

        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i] != null)
            {
                currentSlot = i;
                weaponObjects[i].SetActive(true);
                break;
            }
        }
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchWeapon(1);
        if (Input.GetKeyDown(KeyCode.G)) DropCurrentWeapon();

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryPickupWeapon();
        }
        if (Input.GetMouseButton(0)) TryFire();
        if (Input.GetKeyDown(KeyCode.R)) TryReload();
        if (Input.GetMouseButtonDown(1))
        {
            TryMeleeAttack();
        }



    }

    public float meleeRange = 2f;
    public int meleeDamage = 25;

    void TryMeleeAttack()
    {
        if (Time.time - lastMeleeTime < meleeCooldown) return;

        lastMeleeTime = Time.time;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, meleeRange))
        {

            IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(meleeDamage);
            }

            // TODO: tambahkan animasi, efek, suara
        }
        else
        {
        }
    }



    void TryFire()
    {
        if (weaponSlots[currentSlot] == null || isReloading) return;

        WeaponRuntime current = weaponStates[currentSlot];
        WeaponData data = current.data;

        if (Time.time - current.lastFireTime < data.fireRate) return;
        if (current.currentAmmo <= 0)
        {
            return;
        }

        current.lastFireTime = Time.time;
        current.currentAmmo--; // Kurangi hanya sekali meskipun multi-raycast


        for (int i = 0; i < data.bulletPerShot + playerStat.getExtraBulletPerShot(); i++)
        {
            Vector3 baseDirection = playerCamera.transform.forward;
            Vector3 spreadOffset = new Vector3(
                Random.Range(-data.spreadAmount, data.spreadAmount),
                Random.Range(-data.spreadAmount, data.spreadAmount),
                0f
            );
            Vector3 finalDirection = (baseDirection + playerCamera.transform.TransformDirection(spreadOffset)).normalized;

            Ray ray = new Ray(playerCamera.transform.position, finalDirection);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, shootableLayers))
            {
                Debug.DrawRay(ray.origin, finalDirection * hit.distance, Color.red, 1f);
                Debug.Log("Hit: " + hit.collider.name);

                IDamageable target = hit.collider.GetComponent<IDamageable>();
                if (target != null)
                {
                    target.TakeDamage(data.damage);
                }
            }
        }
    }




    void TryReload()
    {
        if (weaponSlots[currentSlot] == null || isReloading) return;

        StartCoroutine(ReloadCoroutine());
    }

    IEnumerator ReloadCoroutine()
    {
        isReloading = true;

        yield return new WaitForSeconds(weaponStates[currentSlot].data.reloadTime);

        weaponStates[currentSlot].currentAmmo = weaponStates[currentSlot].data.maxAmmo + (int)(weaponStates[currentSlot].data.maxAmmo * playerStat.getAdditionalClipSize());
        isReloading = false;

    }


    void TryPickupWeapon()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange))
        {
            WeaponPickup pickup = hit.collider.GetComponent<WeaponPickup>();
            if (pickup != null)
            {
                if (PickupWeapon(pickup.weaponData))
                {
                    Destroy(pickup.gameObject); // Hapus pickup setelah diambil
                }
            }
        }
    }

    void SwitchWeapon(int slot)
    {
        if (slot == currentSlot || weaponSlots[slot] == null) return;

        if (weaponObjects[currentSlot] != null)
            weaponObjects[currentSlot].SetActive(false);

        currentSlot = slot;

        if (weaponObjects[currentSlot] != null)
            weaponObjects[currentSlot].SetActive(true);
    }


    public bool PickupWeapon(WeaponData newWeapon)
    {
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i] == null)
            {
                weaponSlots[i] = newWeapon;
                weaponObjects[i] = Instantiate(newWeapon.weaponModelPrefab, weaponHolder);
                SetWeaponPhysics(weaponObjects[i], false);
                weaponStates[i] = new WeaponRuntime(newWeapon, playerStat);
                weaponObjects[i].SetActive(false);

                if (i == currentSlot)
                    SwitchWeapon(i);
                return true;
            }
        }

        return false;
    }


    void DropCurrentWeapon()
    {
        if (weaponSlots[currentSlot] == null) return;

        GameObject dropped = Instantiate(weaponSlots[currentSlot].pickupPrefab, transform.position + transform.forward, Quaternion.identity);

        // Aktifkan rigidbody pada pickup
        SetWeaponPhysics(dropped, true);
        weaponStates[currentSlot] = null;

        Destroy(weaponObjects[currentSlot]);
        weaponSlots[currentSlot] = null;
        weaponObjects[currentSlot] = null;
    }
    void SetWeaponPhysics(GameObject weaponObject, bool isActive)
    {
        if (weaponObject == null) return;

        Rigidbody rb = weaponObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = !isActive;
            rb.useGravity = isActive;
        }

        Collider col = weaponObject.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = isActive;
        }
    }
    public WeaponRuntime GetCurrentRuntime()
    {
        return weaponStates[currentSlot];
    }

    public PlayerStat GetPlayerStat()
    {
        return playerStat;
    }

}
