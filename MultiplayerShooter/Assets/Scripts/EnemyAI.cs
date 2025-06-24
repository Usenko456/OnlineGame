using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using System.Collections.Generic;

public class EnemyAI : NetworkBehaviour
{
    public Transform[] waypoints;
    public float detectionRadius = 10f;
    public float fireRate = 3f;
    public Transform firePoint;

    private NavMeshAgent agent;
    private int currentWaypointIndex = 0;
    private float fireTimer = 0f;
    private GameObject currentTarget;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return; // Run logic only on the server

        agent = GetComponent<NavMeshAgent>();

        // Dynamically find patrol waypoints from the scene
        GameObject wpGroup = GameObject.Find("WaypointsGroup");
        if (wpGroup != null)
        {
            List<Transform> wpList = new List<Transform>();
            foreach (Transform child in wpGroup.transform)
                wpList.Add(child);

            waypoints = wpList.ToArray();
        }

        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component not found on Enemy!");
        }
        else
        {
            GoToNextWaypoint(); // Start patrolling
        }
    }

    private void Update()
    {
        if (!IsServer) return; // Update only on server

        fireTimer += Time.deltaTime;

        if (currentTarget == null)
        {
            DetectPlayers(); // Look for players nearby
            Patrol();        // Patrol if no target found
        }
        else
        {
            Attack();        // Attack the detected target
        }
    }

    private void DetectPlayers()
    {
        // Detect players within detection radius using physics overlap
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                currentTarget = hit.gameObject;
                break; // Target found, stop searching
            }
        }
    }

    private void Patrol()
    {
        // Move to next waypoint if close enough to current one
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GoToNextWaypoint();
        }
    }

    void GoToNextWaypoint()
    {
        if (waypoints.Length == 0) return;
        agent.destination = waypoints[currentWaypointIndex].position;
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
    }

    void Attack()
    {
        if (currentTarget == null) return;

        agent.SetDestination(transform.position); // Stop moving
        transform.LookAt(currentTarget.transform); // Face the target

        if (fireTimer >= fireRate)
        {
            fireTimer = 0f;

            // Spawn projectile from pool, server-side
            ObjectPool.Instance.SpawnPooledObject(firePoint.position, firePoint.rotation, transform.forward);
        }

        // Drop target if it goes too far away
        float dist = Vector3.Distance(transform.position, currentTarget.transform.position);
        if (dist > detectionRadius * 1.2f)
        {
            currentTarget = null;
        }
    }
}
