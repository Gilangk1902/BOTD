using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    public float openAngle = 90f;
    public float openSpeed = 2f;
    public float interactionDistance = 5f;
    public KeyCode interactKey = KeyCode.E;

    [Header("Layer Mask")]
    public LayerMask interactableLayer;

    private bool isOpen = false;
    private bool isMoving = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    private Transform player;
    private Camera playerCamera;

    private GameObject promptObject;
    private BoxCollider doorCollider;

    void Start()
    {
        closedRotation = transform.localRotation;
        openRotation = Quaternion.Euler(0f, openAngle, 0f) * closedRotation;

        // Ambil collider dari objek ini
        doorCollider = GetComponent<BoxCollider>();
        if (doorCollider == null)
        {
            Debug.LogWarning("Tidak ditemukan BoxCollider pada pintu.");
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerCamera = player.GetComponentInChildren<Camera>();

            Transform promptTransform = playerCamera.transform.Find("UI/Canvas/Prompt");
            if (promptTransform != null)
            {
                promptObject = promptTransform.gameObject;
                promptObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning("Prompt object tidak ditemukan di path: UI/Canvas/Prompt");
            }
        }

        if (playerCamera == null)
        {
            Debug.LogError("Camera di player tidak ditemukan.");
        }
    }

    void Update()
    {
        if (player == null || isMoving || playerCamera == null) return;
        PromtController();

        if (CanInteract() && Input.GetKeyDown(interactKey))
        {
            StartCoroutine(ToggleDoor());
        }
    }

    void PromtController()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.red);

        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            ShowPrompt(true);
        }
        else
        {
            ShowPrompt(false);
        }
    }

    bool CanInteract()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.red);

        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            return hit.transform == transform || hit.transform.IsChildOf(transform);
        }
        return false;
    }

    void ShowPrompt(bool show)
    {
        if (promptObject != null)
        {
            promptObject.SetActive(show);
        }
    }

    IEnumerator ToggleDoor()
    {
        isMoving = true;
        Quaternion startRotation = transform.localRotation;
        Quaternion targetRotation = isOpen ? closedRotation : openRotation;

        // Jadikan trigger saat membuka
        if (doorCollider != null)
        {
            doorCollider.isTrigger = true;
        }

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;
            transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        transform.localRotation = targetRotation;
        isOpen = !isOpen;
        isMoving = false;

        // Jika menutup kembali, non-trigger supaya solid lagi
        if (doorCollider != null)
        {
            doorCollider.isTrigger = isOpen; // true saat terbuka, false saat tertutup
        }
    }

}
