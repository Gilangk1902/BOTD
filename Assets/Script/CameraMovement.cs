using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float dragSpeed = 80f;
    private Vector3 dragOrigin;

    void LateUpdate()
    {
        HandleMouseDrag();
    }

    void HandleMouseDrag()
    {
        if (Input.GetMouseButtonDown(2)) 
        {
            dragOrigin = Input.mousePosition;
            return;
        }

        if (Input.GetMouseButton(2))
        {
            Vector3 difference = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
            Vector3 move = new Vector3(-difference.x * dragSpeed, 0, -difference.y * dragSpeed);

            transform.Translate(move, Space.World);
            dragOrigin = Input.mousePosition;
        }
    }

}
