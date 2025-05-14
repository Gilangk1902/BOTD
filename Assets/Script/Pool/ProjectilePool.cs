using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance;

    public GameObject projectilePrefab;
    public int poolSize = 20;

    private Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(projectilePrefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject GetProjectile(Vector3 position, Quaternion rotation)
    {
        if (pool.Count == 0)
        {
            GameObject fallback = Instantiate(projectilePrefab, position, rotation);
            return fallback;
        }

        GameObject projectile = pool.Dequeue();
        projectile.transform.position = position;
        projectile.transform.rotation = rotation;
        projectile.SetActive(true);
        return projectile;
    }

    public void ReturnProjectile(GameObject projectile)
    {
        projectile.SetActive(false);
        pool.Enqueue(projectile);
    }
}
