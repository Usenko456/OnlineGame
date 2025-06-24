using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField]
    private NetworkVariable<int> health = new NetworkVariable<int>(100); // Health synced across network

    public int MaxHealth = 100; // Maximum health value

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Initialize health only on the server (host)
            health.Value = MaxHealth;
        }
    }

    public void TakeDamage(int damage)
    {
        if (!IsServer) return; // Damage is processed only on the server

        health.Value -= damage; // Decrease health
        Debug.Log("HP=" + health.Value);

        if (health.Value <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player died!");
        // Here you can trigger respawn, effects, or removal
        // For simplicity, just deactivate the player object
        gameObject.SetActive(false);
    }

    public int GetHealth()
    {
        return health.Value; // Return current health
    }
}
