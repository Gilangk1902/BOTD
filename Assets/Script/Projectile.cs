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
        int layer = other.gameObject.layer;
        string layerName = LayerMask.LayerToName(layer);

        // Cek apakah terkena layer "Tile" atau tag "Player"
        bool hitTile = layerName == "Tile";
        bool hitPlayer = other.CompareTag("Player");

        if (hitTile || hitPlayer)
        {
            if (hitPlayer)
            {
                IDamageable target = other.GetComponent<IDamageable>();
                if (target != null)
                {
                    target.TakeDamage(damage);
                    Debug.Log($"Projectile hit {other.name} for {damage} damage!");
                }
            }

            Destroy(gameObject); // Hancurkan peluru jika mengenai Player atau Tile
        }
    }


}

