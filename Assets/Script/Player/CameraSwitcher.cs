using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public Camera fpsCamera;
    public Camera freeCamera;
    public Camera ViewModelCamera;

    public DungeonCameraController freeCameraController;
    public Transform player;
    public MonoBehaviour[] gameplayComponentsToDisable;

    private bool isFreeCameraActive = false;

    void Start()
    {
        // Assign free camera otomatis jika belum diisi
        if (freeCamera == null)
        {
            GameObject found = GameObject.Find("Free Camera"); // pastikan nama persis
            if (found != null)
                freeCamera = found.GetComponent<Camera>();
        }

        // Assign controller otomatis
        if (freeCameraController == null && freeCamera != null)
        {
            freeCameraController = freeCamera.GetComponent<DungeonCameraController>();
        }

        // Assign player otomatis
        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null)
                player = found.transform;
        }
    }

    void Update()
    {


        if (Input.GetKeyDown(KeyCode.Tab) && freeCamera != null)
        {
            isFreeCameraActive = !isFreeCameraActive;

            // Reset posisi kamera dungeon ke atas player
            if (isFreeCameraActive && freeCameraController != null && player != null)
            {
                freeCameraController.ResetToPlayerPosition(player);
            }

            freeCamera.enabled = isFreeCameraActive;
            ViewModelCamera.enabled = !isFreeCameraActive;
            fpsCamera.enabled = !isFreeCameraActive;

            foreach (var comp in gameplayComponentsToDisable)
                comp.enabled = !isFreeCameraActive;

            //Time.timeScale = isFreeCameraActive ? 0f : 1f;
            //Cursor.lockState = isFreeCameraActive ? CursorLockMode.None : CursorLockMode.Locked;
            //Cursor.visible = isFreeCameraActive;
        }
    }
}
