using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomEncounter : MonoBehaviour
{
    private bool hasStarted = false;
    private List<Enemy> aliveEnemies = new List<Enemy>();

    [Header("Room Connectors (Assign these in prefab)")]
    public Transform[] connectors;

    private List<Door> doors = new List<Door>();
    public List<Door> doorsThatWereOpen = new List<Door>();

    private Generator generator;

    private void Start()
    {
        generator = FindObjectOfType<Generator>();
        StartCoroutine(WaitForGenerationAndFindDoors());
    }

    private IEnumerator WaitForGenerationAndFindDoors()
    {
        yield return new WaitUntil(() => generator != null && generator.dungeonGeneratorState == Generator.DungeonGeneratorState.Finished);

        foreach (var connector in connectors)
        {
            if (connector == null) continue;

            Door[] foundDoors = connector.GetComponentsInChildren<Door>(true);
            foreach (var door in foundDoors)
            {
                if (!doors.Contains(door))
                    doors.Add(door);
            }
        }

        Debug.Log($"RoomEncounter initialized. Doors found: {doors.Count}");
    }

    public void StartEncounter()
    {
        if (hasStarted) return;
        hasStarted = true;

        // Remember doors that are open and force close them
        doorsThatWereOpen.Clear();
        foreach (var door in doors)
        {
            if (door.isOpen)
            {
                doorsThatWereOpen.Add(door);
                door.ForceClose();
            }
            door.Lock();
        }

        // Spawn enemies
        EnemySpawner[] spawners = GetComponentsInChildren<EnemySpawner>();
        foreach (var spawner in spawners)
        {
            Enemy e = spawner.SpawnEnemy();
            if (e != null)
            {
                aliveEnemies.Add(e);
                e.OnDeath += CheckEnemiesRemaining;
            }
        }
    }

    private void CheckEnemiesRemaining(Enemy enemy)
    {
        aliveEnemies.Remove(enemy);
        if (aliveEnemies.Count == 0)
        {
            UnlockDoors();
        }
    }

    private void UnlockDoors()
    {
        foreach (var door in doors)
        {
            door.Unlock();

            // Only reopen doors that were originally open and are currently still closed
            if (doorsThatWereOpen.Contains(door) && !door.isOpen)
            {
                StartCoroutine(door.ToggleDoor());
                Debug.Log("open door automatically");
            }
        }
    }
}
