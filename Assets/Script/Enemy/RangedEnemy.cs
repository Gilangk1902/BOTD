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

    [Header("Audio")]
    public AudioClip shootClip;
    public AudioSource audioSource;

    protected override void Start()
    {
        base.Start();
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

        if (shootClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootClip);
        }

    }

    protected override float GetAttackCoolDown()
    {
        return attackCooldown; 
    }

    protected override float GetAttackDelay()
    {
        return attackDelay;
    }

    protected override void OnAttackDistance()
    {
        if (player == null) return;

        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

}
