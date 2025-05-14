using System.Collections.Generic;
using UnityEngine;
using static EnemyPool;

public class BulletTrailPool : MonoBehaviour
{
    public static BulletTrailPool Instance;

    public GameObject trailPrefab;
    public int poolSize = 20;

    private Queue<GameObject> trailPool = new Queue<GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(trailPrefab);
            obj.SetActive(false);
            trailPool.Enqueue(obj);
        }
    }

    public GameObject GetTrail()
    {
        GameObject trail = trailPool.Dequeue();
        trail.SetActive(true);
        trailPool.Enqueue(trail);
        return trail;
    }

    public void ReturnTrail(GameObject trail)
    {
        trail.SetActive(false);
        trailPool.Enqueue(trail);
    }


    
}
