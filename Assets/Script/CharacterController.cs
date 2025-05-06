using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;
    public Transform playerCamera;
    public float verticalLookLimit = 80f;

    private Rigidbody rb;
    private float xRotation = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;

    }

    void Update()
    {
        LookAround();
    }

    void FixedUpdate()
    {
        Move();
    }

    void LookAround()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -verticalLookLimit, verticalLookLimit);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    Vector3 currentVelocity;
    Vector3 targetVelocity;

    public float acceleration = 10f;
    public float deceleration = 10f;

    void Move()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 inputDir = new Vector3(moveX, 0f, moveZ).normalized;
        targetVelocity = transform.TransformDirection(inputDir) * moveSpeed;

        // SmoothDamp untuk transisi kecepatan yang halus
        Vector3 velocity = Vector3.SmoothDamp(rb.velocity, targetVelocity, ref currentVelocity, inputDir.magnitude > 0 ? 1f / acceleration : 1f / deceleration);

        rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z); // jaga Y velocity (untuk gravity/jump)
    }


}
