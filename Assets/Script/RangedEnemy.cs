using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemy : Enemy
{
    public GameObject projectilePrefab;
    public Transform shootPoint;
    public float attackCooldown = 2f;
    public float rangedRange = 15f;
    public int projectileDamage = 10;

    private float lastAttackTime = -999f;

    protected override void OnChasing(float distance)
    {
        if (distance <= rangedRange)
        {
            Attack();
        }
    }

    public override void Attack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        if (projectilePrefab == null || shootPoint == null) return;

        lastAttackTime = Time.time;

        Vector3 dir = (player.position - shootPoint.position).normalized;
        Quaternion rot = Quaternion.LookRotation(dir);

        GameObject proj = Instantiate(projectilePrefab, shootPoint.position, rot);
        Projectile p = proj.GetComponent<Projectile>();
        if (p != null)
            p.damage = projectileDamage;
    }
}

