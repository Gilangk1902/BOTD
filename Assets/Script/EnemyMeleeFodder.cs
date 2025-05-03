using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMeleeFodder : Enemy
{
    public Transform attackPoint;
    public float attackCooldown = 1f;
    public int attackDamage = 10;

    private float lastAttackTime = -999f;

    public override void Attack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        if (attackPoint == null) return;

        lastAttackTime = Time.time;
        Vector3 origin = attackPoint.position + attackPoint.forward * 1f;

        Collider[] hits = Physics.OverlapSphere(origin, 1.5f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player") && hit.TryGetComponent<IDamageable>(out var target))
            {
                target.TakeDamage(attackDamage);
                Debug.Log($"{gameObject.name} melee hit player for {attackDamage}");
            }
        }
    }
}

