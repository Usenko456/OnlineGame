using UnityEngine;
using Unity.Netcode;

public class Projectile : NetworkBehaviour
{
    public float speed = 22f;          // Projectile speed
    public float lifeTime = 2f;        // Time before projectile auto-deactivates
    public int damage = 10;             // Damage dealt on hit

    private float timer;               // Timer to track projectile life duration
    private Rigidbody rb;              // Reference to Rigidbody component

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;    // Ensure Rigidbody is non-kinematic for physics to work
        }
    }

    // Fires the projectile in a given direction by setting velocity
    public void Fire(Vector3 direction)
    {
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;  // Set velocity based on direction and speed
            timer = 0f;                             // Reset lifetime timer on firing
        }
    }

    void Update()
    {
        if (!IsServer) return;          // Only run lifetime logic on server/host

        timer += Time.deltaTime;
        if (timer > lifeTime)
        {
            // Despawn projectile on network if spawned, else just deactivate locally
            if (NetworkObject.IsSpawned)
                NetworkObject.Despawn();
            else
                gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Try to get PlayerHealth component on collided object
        PlayerHealth ph = other.GetComponent<PlayerHealth>();
        if (ph == null)
        {
            // If not found directly, try to find in parents (optional)
            ph = other.GetComponent<PlayerHealth>();
        }
        if (ph != null)
        {
            ph.TakeDamage(damage);             // Apply damage to the player
            Debug.Log("HP-" + ph.GetHealth()); // Log remaining health
            gameObject.SetActive(false);       // Deactivate projectile after hitting player
        }
    }
}
