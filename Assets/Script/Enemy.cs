using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class Enemy : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public int maxHealth = 100;
    protected int currentHealth;

    [Header("Chase")]
    public float chaseRange = 10f;
    public float stopDistance = 1.5f;
    public float chaseSpeed = 3.5f;

    protected NavMeshAgent agent;
    protected Transform player;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        agent = GetComponent<NavMeshAgent>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        agent.speed = chaseSpeed;
    }

    //protected virtual void OnEnable()
    //{
    //    if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
    //    {
    //        agent.Warp(hit.position);
    //    }
    //}

    protected virtual void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= chaseRange)
        {
            if (distance > stopDistance)
            {
                agent.SetDestination(player.position);
                OnChasing(distance);
            }
            else
            {
                agent.ResetPath();
                Attack(); // default attack
            }
        }
        else
        {
            agent.ResetPath();
        }
    }

    protected virtual void OnChasing(float distance) { }

    public abstract void Attack();

    public virtual void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage. Remaining: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
        Destroy(gameObject);
    }
}
