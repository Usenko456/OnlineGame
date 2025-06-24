using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public float moveSpeed = 5f;      // Movement speed
    private Rigidbody rb;             // Rigidbody component reference
    private Vector3 movement;         // Input movement vector

    public override void OnNetworkSpawn()
    {
        if (!IsLocalPlayer) return;  // Only control local player

        rb = GetComponent<Rigidbody>();  // Cache Rigidbody component
    }

    void Update()
    {
        if (!IsLocalPlayer) return;

        // Get input axes (WASD or arrow keys)
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // Create normalized movement vector for consistent speed in all directions
        movement = new Vector3(h, 0f, v).normalized;
    }

    void FixedUpdate()
    {
        if (!IsLocalPlayer) return;
        if (rb == null) return;

        if (movement.magnitude > 0)
        {
            // Convert movement to local space relative to player rotation
            Vector3 moveDirection = transform.TransformDirection(movement);

            // Calculate new position
            Vector3 newPosition = rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime;

            // Move Rigidbody to new position for smooth physics movement
            rb.MovePosition(newPosition);
        }
    }
}
