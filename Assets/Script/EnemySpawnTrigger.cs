using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnTrigger : MonoBehaviour
{
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;

            EnemySpawner[] spawners = GetComponentsInChildren<EnemySpawner>();
            foreach (var spawner in spawners)
            {
                spawner.SpawnEnemy();
            }
        }
    }
}
