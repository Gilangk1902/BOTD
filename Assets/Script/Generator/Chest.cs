using UnityEngine;

public class Chest : MonoBehaviour, InteractPrompt
{
    [Header("Chest Components")]
    public Transform lid;
    public float openAngle = 90f;
    public float openSpeed = 2f;

    [Header("Items to Spawn")]
    public ItemData[] itemDatas;

    public Transform spawnPoint;

    [Header("Interaction Settings")]
    public float interactDistance = 3f;
    public KeyCode interactKey = KeyCode.E;

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

        GameObject selectedItem = GetWeightedRandomItem();
        if (selectedItem != null && spawnPoint != null)
        {
            Instantiate(selectedItem, spawnPoint.position, spawnPoint.rotation);
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

    private GameObject GetWeightedRandomItem()
    {
        if (itemDatas == null || itemDatas.Length == 0)
            return null;

        float totalWeight = 0f;
        foreach (var item in itemDatas)
        {
            totalWeight += item.weight;
        }

        float randomValue = Random.Range(0f, totalWeight);
        float currentSum = 0f;

        foreach (var item in itemDatas)
        {
            currentSum += item.weight;
            if (randomValue <= currentSum)
            {
                return item.prefab;
            }
        }

        return itemDatas[0].prefab;
    }

    public string GetPromptText()
    {
        return "Open Chest";
    }
}
