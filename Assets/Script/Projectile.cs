using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 10;
    public float lifeTime = 5f;

    private void Start()
    {
        Destroy(gameObject, lifeTime); // auto-destroy after X seconds
    }

    private void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamageable target = other.GetComponent<IDamageable>();
        if (target != null && other.CompareTag("Player"))
        {
            target.TakeDamage(damage);
            Debug.Log($"Projectile hit {other.name} for {damage} damage!");
        }

        Destroy(gameObject); // Destroy on impact
    }
}

