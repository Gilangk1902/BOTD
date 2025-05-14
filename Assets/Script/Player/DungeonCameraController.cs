using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonCameraController : MonoBehaviour
{
    public float moveSpeed = 20f;
    public float rotationSpeed = 3f;
    public float zoomSpeed = 20f;
    public float minZoom = 5f;
    public float maxZoom = 100f;

    private float yaw;
    private float pitch;
    private Vector3 dragOrigin;
    public Transform player; // assign di Inspector atau cari otomatis

    void Start()
    {
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
    }

    void Update()
    {
        if (!GetComponent<Camera>().enabled) return;

        HandleMovement();
        HandleRotation();
        HandleZoom();
    }

    void HandleMovement()
    {
        Vector3 dir = new Vector3(
            Input.GetAxis("Horizontal"),
            0,
            Input.GetAxis("Vertical")
        );
        Vector3 move = Quaternion.Euler(0, yaw, 0) * dir;
        transform.position += move * moveSpeed * Time.deltaTime;

        // Optional: Pan dengan middle mouse button
        if (Input.GetMouseButtonDown(2)) // middle click
            dragOrigin = Input.mousePosition;

        if (Input.GetMouseButton(2))
        {
            Vector3 delta = Input.mousePosition - dragOrigin;
            transform.position -= transform.right * delta.x * 0.01f;
            transform.position -= transform.up * delta.y * 0.01f;
            dragOrigin = Input.mousePosition;
        }
    }
    public void ResetToPlayerPosition(Transform player)
    {
        // Sync rotasi
        Vector3 euler = transform.rotation.eulerAngles;
        yaw = euler.y;
        pitch = euler.x;

        // Pindah ke atas player
        if (player != null)
        {
            Vector3 newPos = new Vector3(player.position.x, transform.position.y, player.position.z);
            transform.position = newPos;
        }
    }

    void HandleRotation()
    {
        //if (Input.GetMouseButton(1)) // right click to rotate
        //{
        //    yaw += Input.GetAxis("Mouse X") * rotationSpeed;   // tambahkan ini untuk yaw
        //    pitch -= Input.GetAxis("Mouse Y") * rotationSpeed; // sudah ada, tetap pakai

        //    pitch = Mathf.Clamp(pitch, -80f, 80f); // batasi rotasi vertikal

        //    transform.rotation = Quaternion.Euler(pitch, yaw, 0);
        //}
        yaw += Input.GetAxis("Mouse X") * rotationSpeed;   // tambahkan ini untuk yaw
        pitch -= Input.GetAxis("Mouse Y") * rotationSpeed; // sudah ada, tetap pakai

        pitch = Mathf.Clamp(pitch, -80f, 80f); // batasi rotasi vertikal

        transform.rotation = Quaternion.Euler(pitch, yaw, 0);
    }


    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        float newY = transform.position.y - scroll * zoomSpeed;

        // Clamp agar tidak terlalu dekat atau terlalu jauh
        newY = Mathf.Clamp(newY, minZoom, maxZoom);

        Vector3 newPosition = new Vector3(transform.position.x, newY, transform.position.z);
        transform.position = newPosition;
    }

}
