using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class ObjectPool : NetworkBehaviour
{
    public static ObjectPool Instance;

    public GameObject projectilePrefab;
    public int poolSize = 40;

    private List<GameObject> pool = new List<GameObject>();

    private void Awake()
    {
        // Create singleton instance for global access
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        // Initialize the pool only on the server to avoid duplicates on clients
        if (!IsServer)
        {
            Debug.Log("Not server - skipping pool creation");
            return;
        }

        Debug.Log("Server spawned - creating projectile pool");

        // Pre-instantiate projectiles and keep them inactive for reuse
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(projectilePrefab);
            obj.SetActive(false);
            pool.Add(obj);
        }

        Debug.Log($"Projectile pool created with {pool.Count} objects");
    }

    // Find and return an inactive projectile to reuse from the pool
    public GameObject GetPooledObject()
    {
        for (int i = 0; i < pool.Count; i++)
        {
            GameObject obj = pool[i];
            if (obj != null && !obj.activeInHierarchy)
            {
                return obj;
            }
            else if (obj == null)
            {
                // Remove destroyed objects to keep pool clean
                pool.RemoveAt(i);
                i--;
            }
        }
        // Return null if no inactive projectile is found
        return null;
    }

    // Spawn and activate a pooled projectile at a position with direction
    public void SpawnPooledObject(Vector3 position, Quaternion rotation, Vector3 direction)
    {
        // Only server should spawn projectiles in multiplayer setup
        if (!IsServer) return;

        GameObject proj = GetPooledObject();
        if (proj != null)
        {
            // Set projectile position and rotation before activation
            proj.transform.position = position;
            proj.transform.rotation = rotation;

            Rigidbody rb = proj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Enable physics simulation by disabling kinematic mode
                rb.isKinematic = false;
                // Reset velocity to prevent unexpected movement
                rb.linearVelocity = Vector3.zero;
            }

            var netObj = proj.GetComponent<NetworkObject>();
            if (netObj != null && !netObj.IsSpawned)
            {
                // Spawn network object so clients receive updates
                netObj.Spawn();
            }

            // Activate projectile and start its movement
            proj.SetActive(true);
            proj.GetComponent<Projectile>().Fire(direction);
        }
        else
        {
            Debug.LogWarning("No available projectile in pool!");
        }
    }
}
