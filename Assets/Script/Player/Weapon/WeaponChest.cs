using UnityEngine;

public class WeaponChest : MonoBehaviour, InteractPrompt
{
    [Header("Chest Components")]
    public Transform lid; // Assign the chest lid Transform in the Inspector
    public float openAngle = 90f; // Angle to open the lid
    public float openSpeed = 2f; // Speed of opening

    [Header("Items to Spawn")]
    public GameObject[] itemDatas; // Replace itemPrefabs with this

    public Transform spawnPoint; // Assign the spawn point Transform

    [Header("Interaction Settings")]
    public float interactDistance = 3f; // Maximum distance to interact
    public KeyCode interactKey = KeyCode.E; // Key to press for interaction

    private bool isOpened = false;
    private Quaternion closedRotation;
    private Quaternion openedRotation;
    private bool isLocked = false;

    private Camera playerCamera;

    private void Start()
    {
        if (lid == null)
        {
            Debug.LogError("Lid Transform is not assigned.");
            return;
        }

        closedRotation = lid.localRotation;
        openedRotation = Quaternion.Euler(openAngle, 0f, 0f) * closedRotation;

        // Find the player's camera
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerCamera = player.GetComponentInChildren<Camera>();
        }

        if (playerCamera == null)
        {
            Debug.LogError("Player camera not found.");
        }
    }

    private void Update()
    {
        if (isOpened || playerCamera == null || isLocked) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            if (hit.transform == transform)
            {
                if (Input.GetKeyDown(InputManager.Instance.keyBindings.interact))
                {
                    OpenChest();
                }
            }
        }
    }



    public void LockChest()
    {
        isLocked = true;
    }

    public void UnlockChest()
    {
        isLocked = false;
    }


    private void OpenChest()
    {
        isOpened = true;
        StartCoroutine(OpenLid());

        int index = Random.Range(0, itemDatas.Length);
        if (itemDatas[index] != null && spawnPoint != null)
        {
            Instantiate(itemDatas[index], spawnPoint.position, spawnPoint.rotation);
        }

    }

    private System.Collections.IEnumerator OpenLid()
    {
        float elapsed = 0f;
        Quaternion startRotation = lid.localRotation;
        Quaternion endRotation = Quaternion.Euler(-openAngle, 0f, 0f) * startRotation;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * openSpeed;
            lid.localRotation = Quaternion.Slerp(startRotation, endRotation, elapsed);
            yield return null;
        }

        lid.localRotation = endRotation;
    }

    public string GetPromptText()
    {
        return "Open Chest";
    }
}
