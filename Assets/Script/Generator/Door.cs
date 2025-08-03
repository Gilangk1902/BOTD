using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour, InteractPrompt
{
    [Header("Door Settings")]
    public float openAngle = 90f;
    public float openSpeed = 2f;
    public float interactionDistance = 5f;
    public KeyCode interactKey = KeyCode.E;

    [Header("References")]
    public Transform doorTransform; // assign the child that will rotate
    public LayerMask interactableLayer;

    public bool isOpen = false;
    private bool isMoving = false;
    private bool isLocked = false;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    private Transform player;
    private Camera playerCamera;
    private BoxCollider doorCollider;

    void Start()
    {
        if (doorTransform == null)
        {
            Debug.LogError("Door: 'doorTransform' not assigned.");
            return;
        }

        closedRotation = doorTransform.localRotation;
        openRotation = Quaternion.Euler(0f, openAngle, 0f) * closedRotation;

        doorCollider = GetComponentInChildren<BoxCollider>();
        if (doorCollider == null)
        {
            Debug.LogWarning("Door: BoxCollider not found.");
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerCamera = player.GetComponentInChildren<Camera>();
        }

        if (playerCamera == null)
        {
            Debug.LogError("Door: Player camera not found.");
        }
    }

    void Update()
    {
        if (player == null || isMoving || playerCamera == null) return;

        if (!isLocked && CanInteract() && Input.GetKey(InputManager.Instance.keyBindings.interact))
        {
            StartCoroutine(ToggleDoor());
        }
    }

    bool CanInteract()
    {
        if (isLocked) return false;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            return hit.transform == transform || hit.transform.IsChildOf(transform);
        }
        return false;
    }

    public IEnumerator ToggleDoor()
    {
        isMoving = true;
        Quaternion startRotation = doorTransform.localRotation;
        Quaternion targetRotation = isOpen ? closedRotation : openRotation;

        if (doorCollider != null)
            doorCollider.isTrigger = true;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;
            doorTransform.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        doorTransform.localRotation = targetRotation;
        isOpen = !isOpen;
        isMoving = false;

        if (doorCollider != null)
            doorCollider.isTrigger = isOpen;
    }

    public void Lock()
    {
        isLocked = true;
    }

    public void Unlock()
    {
        isLocked = false;
    }

    public void ForceClose()
    {
        if (isMoving) return;

        StopAllCoroutines();
        StartCoroutine(ForceCloseCoroutine());
    }

    private IEnumerator ForceCloseCoroutine()
    {
        isMoving = true;

        Quaternion startRotation = doorTransform.localRotation;
        Quaternion targetRotation = closedRotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;
            doorTransform.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        doorTransform.localRotation = targetRotation;
        isMoving = false;

        if (doorCollider != null)
            doorCollider.isTrigger = false;

        isOpen = !isOpen; // Optional: preserve original logic
    }

    public string GetPromptText()
    {
        return "Open Door";
    }
}
