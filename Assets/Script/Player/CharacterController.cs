using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    [SerializeField] private PlayerStat playerStat;
    public float mouseSensitivity = 20f;
    public Transform playerCamera;
    public float verticalLookLimit = 80f;

    private Rigidbody rb;
    private float xRotation = 0f;
    public float gravityMultiplier = 2f;
    public float groundCheckDistance = 0.3f;
    public LayerMask groundLayer;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;

    }

    void Update()
    {
        if (Time.timeScale == 0f)
            return;
        LookAround();
    }

    void FixedUpdate()
    {
        CheckGrounded();
        Move();
        ApplyExtraGravity();
    }
    void ApplyExtraGravity()
    {
        if (!isGrounded)
        {
            // Tambahkan gravity tambahan agar karakter nempel saat di turunan
            rb.AddForce(Vector3.down * gravityMultiplier * Physics.gravity.magnitude, ForceMode.Acceleration);
        }
    }

    void CheckGrounded()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        isGrounded = Physics.Raycast(origin, Vector3.down, groundCheckDistance, groundLayer);
    }


    void LookAround()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

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
        targetVelocity = transform.TransformDirection(inputDir) * playerStat.getMoveSpeed() * Time.deltaTime;

        // SmoothDamp untuk transisi kecepatan yang halus
        Vector3 velocity = Vector3.SmoothDamp(rb.velocity, targetVelocity, ref currentVelocity, inputDir.magnitude > 0 ? 1f / acceleration : 1f / deceleration);

        rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z); // jaga Y velocity (untuk gravity/jump)
    }


}
