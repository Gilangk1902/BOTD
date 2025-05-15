using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMeleeFodder : Enemy
{
    [Header("Attack Settings")]
    public Transform attackPoint;
    public float attackCooldown = 1f;
    public float attackDelay = .5f;
    public int attackDamage = 10;
    public float attackRange = 1.5f;

    private float lastAttackTime = -999f;

    protected override void Start()
    {
        base.Start(); // penting agar animator, player, agent di-setup
    }

    public override void Attack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        if (attackPoint == null) return;

        lastAttackTime = Time.time;
        PerformMeleeHit();
    }


    private void PerformMeleeHit()
    {
        Vector3 origin = attackPoint.position + attackPoint.forward * 1f;
        Collider[] hits = Physics.OverlapSphere(origin, attackRange);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player") && hit.TryGetComponent<IDamageable>(out var target))
            {
                target.TakeDamage(attackDamage);
                Debug.Log($"{gameObject.name} melee hit player for {attackDamage}");
            }
        }
    }

    protected override float GetAttackCoolDown()
    {
        return attackCooldown; // atau return 1.2f jika beda dari cooldown logic
    }

    protected override float GetAttackDelay()
    {
        return attackDelay;
    }

    protected override void OnAttackDistance()
    {
        if (player == null) return;

        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0f; // biar nggak mendongak ke atas/bawah

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }
}
