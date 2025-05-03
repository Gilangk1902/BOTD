using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnTrigger : MonoBehaviour
{
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !other.CompareTag("Player")) return;

        Transform roomRoot = transform.parent;
        if (roomRoot != null && roomRoot.name.Contains("Start"))
        {
            Debug.Log("Start room detected, enemy will not spawn.");
            return;
        }

        hasTriggered = true;

        EnemySpawner[] spawners = GetComponentsInChildren<EnemySpawner>();
        foreach (var spawner in spawners)
        {
            spawner.SpawnEnemy();
        }
    }
}

