using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class EnemySpawner : NetworkBehaviour
{
    public GameObject enemyPrefab;       // Enemy prefab to spawn
    public Transform[] spawnPoints;      // Possible spawn locations
    public float spawnInterval = 5f;     // Time between spawns in seconds

    public override void OnNetworkSpawn()
    {
        // Only the server runs the spawning coroutine to avoid duplicates
        if (IsServer)
        {
            StartCoroutine(SpawnEnemiesRoutine());
        }
    }

    IEnumerator SpawnEnemiesRoutine()
    {
        // Loop indefinitely spawning enemies at intervals
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        // Choose a random spawn point
        int index = Random.Range(0, spawnPoints.Length);

        // Instantiate enemy at chosen spawn point with default rotation
        GameObject enemy = Instantiate(enemyPrefab, spawnPoints[index].position, Quaternion.identity);

        // Spawn the enemy on the network so all clients see it
        enemy.GetComponent<NetworkObject>().Spawn();
    }
}
