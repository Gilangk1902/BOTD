using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 10;
    public float lifeTime = 5f;

    private Coroutine lifeCoroutine;

    private void OnEnable()
    {
        // Mulai timer untuk auto-return ke pool
        lifeCoroutine = StartCoroutine(ReturnAfterTime());
    }

    private void OnDisable()
    {
        // Bersihkan jika coroutine masih berjalan
        if (lifeCoroutine != null)
            StopCoroutine(lifeCoroutine);
    }

    private void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        int layer = other.gameObject.layer;
        string layerName = LayerMask.LayerToName(layer);

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

            // Kembalikan ke pool (bukan Destroy)
            ProjectilePool.Instance.ReturnProjectile(gameObject);
        }
    }

    private IEnumerator ReturnAfterTime()
    {
        yield return new WaitForSeconds(lifeTime);
        ProjectilePool.Instance.ReturnProjectile(gameObject);
    }
}
