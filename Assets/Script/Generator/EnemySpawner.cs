using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Pool Tags")]
    public string[] enemyPoolTags;

    [Header("Trigger Spawn")]
    public bool hasSpawned = false;

    public float navmeshSearchRadius = 2f;

    public Enemy SpawnEnemy()
    {
        if (enemyPoolTags.Length == 0)
        {
            Debug.LogWarning("EnemySpawner: No enemy pool tags assigned.");
            return null;
        }

        int index = Random.Range(0, enemyPoolTags.Length);
        string tag = enemyPoolTags[index];

        // Cari posisi NavMesh terdekat dari titik spawner
        Vector3 spawnPos = GetValidNavMeshPosition(transform.position, navmeshSearchRadius);

        GameObject enemyGO = EnemyPool.Instance.SpawnFromPool(tag, spawnPos, Quaternion.identity);
        if (enemyGO == null)
        {
            Debug.LogWarning($"EnemySpawner: Failed to spawn enemy with tag {tag}");
            return null;
        }

        return enemyGO.GetComponent<Enemy>();
    }

    private Vector3 GetValidNavMeshPosition(Vector3 position, float range)
    {
        if (NavMesh.SamplePosition(position, out NavMeshHit hit, range, NavMesh.AllAreas))
        {
            return hit.position;
        }

        Debug.LogWarning($"EnemySpawner: No valid NavMesh position found near {position}. Using raw position.");
        return position; // fallback
    }
}
