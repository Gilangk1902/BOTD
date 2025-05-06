using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject[] enemyPrefabs;

    [Header("Trigger Spawn")]
    public bool hasSpawned = false;


    public Enemy SpawnEnemy()
    {
        if (enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("EnemySpawner: No enemy prefabs assigned.");
            return null;
        }

        int index = Random.Range(0, enemyPrefabs.Length);
        GameObject enemyGO = Instantiate(enemyPrefabs[index], transform.position, Quaternion.identity);
        return enemyGO.GetComponent<Enemy>();
    }

}

