using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractUIController : MonoBehaviour
{
    public float interactRange = 3f;
    public LayerMask interactLayer;
    public Camera playerCamera;
    public GameObject promptUI;
    public TMPro.TextMeshProUGUI promptText;

    private InteractPrompt currentInteractable;

    void Update()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactLayer))
        {
            InteractPrompt interactable = hit.collider.GetComponent<InteractPrompt>();
            if (interactable != null)
            {
                currentInteractable = interactable;

                KeyCode interactKey = InputManager.Instance.keyBindings.interact;

                promptUI.SetActive(true);
                promptText.text = $"[{interactKey}] {interactable.GetPromptText()}";

                return;
            }
        }

        currentInteractable = null;
        promptUI.SetActive(false);
    }
}
