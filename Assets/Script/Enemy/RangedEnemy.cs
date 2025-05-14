using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemy : Enemy
{
    [Header("Ranged Attack Settings")]
    public GameObject projectilePrefab;
    public Transform shootPoint;
    public float attackCooldown = 2f;
    public float attackDelay = 0.2f;
    public float rangedRange = 15f;
    public int projectileDamage = 10;

    private float lastAttackTime = -999f;

    protected override void Start()
    {
        base.Start(); // penting agar animator, player, agent di-setup
    }

    protected override void OnChasing(float distance)
    {
        // Jika dalam jangkauan serangan jarak jauh, berhenti dan menyerang
        if (distance <= rangedRange)
        {
            agent.ResetPath();
            SetAnimationState(walk: false, attack: true, idle: false);
            Attack();
        }
    }

    public override void Attack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        if (projectilePrefab == null || shootPoint == null || player == null) return;

        lastAttackTime = Time.time;

        Vector3 dir = (player.position - shootPoint.position).normalized;
        Quaternion rot = Quaternion.LookRotation(dir);

        GameObject proj = ProjectilePool.Instance.GetProjectile(shootPoint.position, rot);

        Projectile p = proj.GetComponent<Projectile>();
        if (p != null)
        {
            p.damage = projectileDamage;
        }

        Debug.Log($"{gameObject.name} fired projectile at player.");
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
