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
        public GameObject muzzle;
        public GameObject bulletTrailPrefab;

        [SerializeField] private PlayerStat playerStat;

        private Vector3 defaultWeaponPos;
        private Coroutine weaponAnimRoutine;


        private void Start()
        {
            defaultWeaponPos = weaponHolder.localPosition;

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
            if (Time.timeScale == 0f)
                return;
            if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchWeapon(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchWeapon(1);
            //if (Input.GetKeyDown(KeyCode.G)) DropCurrentWeapon();

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
            current.currentAmmo--;

            for (int i = 0; i < data.bulletPerShot; i++)
            {
                for (int j = 0; j < playerStat.getExtraBulletPerShot() + 1; j++)
                {
                    shoot(data);
                }
            }

            if (weaponAnimRoutine != null) StopCoroutine(weaponAnimRoutine);
            weaponAnimRoutine = StartCoroutine(PlayRecoilAnimation());
        }


        void shoot(WeaponData data)
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
                Debug.DrawRay(ray.origin, finalDirection * hit.distance, Color.red, .1f);
                Debug.Log("Hit: " + hit.collider.name);

                IDamageable target = hit.collider.GetComponent<IDamageable>();
                if (target != null)
                {
                    target.TakeDamage(data.damage);
                }

                GameObject trail = BulletTrailPool.Instance.GetTrail();
                //trail.transform.position = muzzle.transform.position; // optional

                BulletTrail bt = trail.GetComponent<BulletTrail>();
                bt.Init(muzzle.transform.position, hit.point, 0.1f);

            }
            else
            {
                Vector3 endPoint = ray.origin + finalDirection * 1000f;
                Debug.DrawRay(ray.origin, finalDirection * 1000f, Color.red, .1f);

                GameObject trail = BulletTrailPool.Instance.GetTrail();
                BulletTrail bt = trail.GetComponent<BulletTrail>();
                bt.Init(muzzle.transform.position, endPoint, 0.1f); // <- pakai endPoint, bukan hit.point
            }

        }

        void TryReload()
        {
            if (weaponSlots[currentSlot] == null || isReloading) return;

            WeaponRuntime current = weaponStates[currentSlot];
            int maxAmmo = current.data.maxAmmo + (int)(current.data.maxAmmo * playerStat.getAdditionalClipSize());

            if (current.currentAmmo >= maxAmmo) return; 

            StartCoroutine(ReloadCoroutine());
        }


        IEnumerator ReloadCoroutine()
        {
            isReloading = true;

            float reloadTime = weaponStates[currentSlot].data.reloadTime;

            if (weaponAnimRoutine != null) StopCoroutine(weaponAnimRoutine);
            weaponAnimRoutine = StartCoroutine(PlayWeaponDownUpAnimation(reloadTime));

            yield return new WaitForSeconds(reloadTime);

            weaponStates[currentSlot].currentAmmo = weaponStates[currentSlot].data.maxAmmo +
                (int)(weaponStates[currentSlot].data.maxAmmo * playerStat.getAdditionalClipSize());

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
                    // Coba cari slot kosong dulu
                    int emptySlot = -1;
                    for (int i = 0; i < weaponSlots.Length; i++)
                    {
                        if (weaponSlots[i] == null)
                        {
                            emptySlot = i;
                            break;
                        }
                    }

                    if (emptySlot != -1)
                    {
                        weaponSlots[emptySlot] = pickup.weaponData;
                        weaponObjects[emptySlot] = Instantiate(pickup.weaponData.weaponModelPrefab, weaponHolder);
                        SetWeaponPhysics(weaponObjects[emptySlot], false);
                        weaponStates[emptySlot] = new WeaponRuntime(pickup.weaponData, playerStat);
                        weaponObjects[emptySlot].SetActive(false);
                        Destroy(pickup.gameObject);
                    }
                    else
                    {
                        DropCurrentWeapon();

                        weaponSlots[currentSlot] = pickup.weaponData;
                        weaponObjects[currentSlot] = Instantiate(pickup.weaponData.weaponModelPrefab, weaponHolder);
                        SetWeaponPhysics(weaponObjects[currentSlot], false);
                        weaponStates[currentSlot] = new WeaponRuntime(pickup.weaponData, playerStat);
                        Destroy(pickup.gameObject);

                        weaponObjects[currentSlot].SetActive(true);
                    }


                }
            }
        }


        void SwitchWeapon(int slot)
        {
            if (slot == currentSlot || weaponSlots[slot] == null) return;

            if (weaponAnimRoutine != null) StopCoroutine(weaponAnimRoutine);
            weaponAnimRoutine = StartCoroutine(PlayWeaponDownUpAnimation(0.2f, () =>
            {
                if (weaponObjects[currentSlot] != null)
                    weaponObjects[currentSlot].SetActive(false);

                currentSlot = slot;

                if (weaponObjects[currentSlot] != null)
                    weaponObjects[currentSlot].SetActive(true);
            }));
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

            GameObject dropped = Instantiate(
                weaponSlots[currentSlot].pickupPrefab, transform.position + transform.forward, Quaternion.identity
            );

            SetWeaponPhysics(dropped, true);

            Destroy(weaponObjects[currentSlot]);
            weaponSlots[currentSlot] = null;
            weaponObjects[currentSlot] = null;
            weaponStates[currentSlot] = null;
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

        IEnumerator PlayRecoilAnimation(float distance = 0.1f, float speed = 20f)
        {
            Vector3 backPos = defaultWeaponPos - new Vector3(0f, 0f, distance);
            float t = 0;

            while (t < 1f)
            {
                t += Time.deltaTime * speed;
                weaponHolder.localPosition = Vector3.Lerp(defaultWeaponPos, backPos, t);
                yield return null;
            }

            t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * speed;
                weaponHolder.localPosition = Vector3.Lerp(backPos, defaultWeaponPos, t);
                yield return null;
            }

            weaponHolder.localPosition = defaultWeaponPos;
        }
        IEnumerator PlayWeaponDownUpAnimation(float duration, System.Action onMid = null)
        {
            Vector3 downPos = defaultWeaponPos - new Vector3(0, 1.5f, 0);
            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime * 5f;
                weaponHolder.localPosition = Vector3.Lerp(defaultWeaponPos, downPos, t);
                yield return null;
            }

            onMid?.Invoke();

            yield return new WaitForSeconds(duration);

            t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * 5f;
                weaponHolder.localPosition = Vector3.Lerp(downPos, defaultWeaponPos, t);
                yield return null;
            }

            weaponHolder.localPosition = defaultWeaponPos;
        }
    }
