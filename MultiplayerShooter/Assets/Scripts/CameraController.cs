using Unity.Netcode;
using UnityEngine;

public class CameraController : NetworkBehaviour
{
    public Vector3 offset = new Vector3(0f, 5f, -6f);  // Camera position offset relative to player
    public float mouseSensitivity = 100f;              // Mouse sensitivity for rotation

    private Transform cameraTransform;                  // Reference to main camera transform
    private float yaw;                                  // Horizontal rotation angle

    void Start()
    {
        if (!IsLocalPlayer) return;                     // Only control camera for the local player

        cameraTransform = Camera.main.transform;       // Get main camera transform

        Cursor.lockState = CursorLockMode.Locked;      // Lock and hide cursor for better control
        Cursor.visible = false;

        yaw = transform.eulerAngles.y;                  // Initialize yaw to player's current rotation
    }

    void LateUpdate()
    {
        if (!IsLocalPlayer || cameraTransform == null) return;

        // Get horizontal mouse movement and apply sensitivity and delta time
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        yaw += mouseX;

        // Rotate player horizontally around Y axis
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        // Position camera relative to player using the offset, transformed by player rotation
        cameraTransform.position = transform.position + transform.TransformDirection(offset);

        // Make camera look slightly above player's position (e.g., head height)
        cameraTransform.LookAt(transform.position + Vector3.up * 1.5f);
    }
}
