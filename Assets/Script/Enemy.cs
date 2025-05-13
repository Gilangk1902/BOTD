using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class Enemy : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public int maxHealth = 100;
    protected int currentHealth;

    [Header("Chase Settings")]
    public float chaseRange = 10f;
    public float stopDistance = 1.5f;
    public float chaseSpeed = 3.5f;

    protected NavMeshAgent agent;
    protected Transform player;
    protected Animator animator;

    protected static readonly int AnimWalk = Animator.StringToHash("Walk");
    protected static readonly int AnimIdle = Animator.StringToHash("Idle");
    protected static readonly int AnimAttack = Animator.StringToHash("Attack");
    protected static readonly int TriggerAttack = Animator.StringToHash("Attack");

    public event System.Action<Enemy> OnDeath;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        agent.speed = chaseSpeed;
    }

    protected bool isAttacking = false; // NEW

    protected virtual void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= chaseRange)
        {
            if (distance > stopDistance)
            {
                if (!isAttacking)
                {
                    agent.SetDestination(player.position);
                    SetAgentActive(true);
                }

                // Aktifkan animasi jalan hanya saat agent punya velocity
                if (!agent.pathPending && agent.remainingDistance > stopDistance && !isAttacking)
                {
                    SetAnimationState(walk: true, attack: false, idle: false);
                }
                else
                {
                    SetAnimationState(walk: false, attack: false, idle: true);
                }

                OnChasing(distance);
            }

            else
            {
                agent.ResetPath();
                OnAttackDistance();
                if (!isAttacking) // only attack if not currently attacking
                {
                    StartCoroutine(HandleAttack());
                }
            }
        }
        else
        {
            agent.ResetPath();
            SetAnimationState(walk: false, attack: false, idle: true);
            isAttacking = false;
        }
    }
    protected virtual IEnumerator HandleAttack()
    {
        isAttacking = true;

        if (animator != null)
        {
            animator.SetTrigger(TriggerAttack); // pakai trigger, bukan bool
        }

        yield return new WaitForSeconds(GetAttackDelay());

        Attack(); // logic hit / projectile

        yield return new WaitForSeconds(GetAttackCoolDown()); // tunggu animasi selesai

        isAttacking = false;
    }


    protected void SetAnimationState(bool walk, bool attack, bool idle)
    {
        if (animator == null) return;
        animator.SetBool(AnimWalk, walk);
        //animator.SetBool(AnimAttack, attack);
        animator.SetBool(AnimIdle, idle);
    }

    protected virtual void OnChasing(float distance) { }

    protected abstract void OnAttackDistance();

    public abstract void Attack();

    public virtual void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
            Die();
    }

    protected virtual void Die()
    {
        OnDeath?.Invoke(this);
        Destroy(gameObject);
    }
    protected void SetAgentActive(bool active)
{
    agent.updatePosition = active;
    agent.updateRotation = active;
    agent.isStopped = !active;
}

    protected abstract float GetAttackDelay();
    protected abstract float GetAttackCoolDown();

}
