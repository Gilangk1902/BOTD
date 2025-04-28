using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Chase")]
    public float chaseRange = 10f;
    public float stopDistance = 1.5f;
    public float chaseSpeed = 3.5f;

    private NavMeshAgent agent;
    private Transform player;

    [Header("Attack")]
    public int attackDamage = 10;
    public float attackCooldown = 1f;

    private float lastAttackTime = -999f;

    [Header("Ranged Attack")]
    public GameObject projectilePrefab;
    public Transform shootPoint;
    public float rangedCooldown = 2f;
    public float rangedRange = 15f;

    private float lastRangedTime = -999f;

    private void Start()
    {
        currentHealth = maxHealth;

        agent = GetComponent<NavMeshAgent>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        agent.speed = chaseSpeed;
    }

    void OnEnable()
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 2f, NavMesh.AllAreas))
        {
            GetComponent<NavMeshAgent>().Warp(hit.position);
        }
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= chaseRange)
        {
            if (distance > stopDistance)
            {
                agent.SetDestination(player.position);

                // Jika dalam jarak ranged attack, tapi terlalu jauh untuk melee
                if (distance <= rangedRange)
                {
                    DoRangedAttack();
                }
            }
            else
            {
                agent.ResetPath();
                AttackPlayer(); // melee attack
            }
        }
        else
        {
            agent.ResetPath();
        }
    }

    void AttackPlayer()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        if (shootPoint == null) return;

        lastAttackTime = Time.time;

        Debug.Log($"{gameObject.name} performs melee attack!");

        // Gunakan posisi dan arah dari shootPoint
        Vector3 attackOrigin = shootPoint.position;
        Vector3 attackDirection = shootPoint.forward;

        // Hit area di depan shootPoint
        Collider[] hitColliders = Physics.OverlapSphere(attackOrigin + attackDirection * 1f, 1.5f);

        foreach (Collider hit in hitColliders)
        {
            IDamageable target = hit.GetComponent<IDamageable>();
            if (target != null && hit.CompareTag("Player"))
            {
                target.TakeDamage(attackDamage);
                Debug.Log($"{gameObject.name} dealt {attackDamage} melee damage to Player!");
            }
        }
    }


    void DoRangedAttack()
    {
        if (Time.time - lastRangedTime < rangedCooldown) return;
        if (projectilePrefab == null || shootPoint == null) return;

        lastRangedTime = Time.time;

        Vector3 direction = (player.position - shootPoint.position).normalized;
        Quaternion rotation = Quaternion.LookRotation(direction);

        GameObject projectile = Instantiate(projectilePrefab, shootPoint.position, rotation);
        Projectile p = projectile.GetComponent<Projectile>();
        if (p != null)
        {
            p.damage = attackDamage;
        }

        Debug.Log($"{gameObject.name} fired a projectile!");
    }
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage. Remaining health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
        Destroy(gameObject);
    }
}
