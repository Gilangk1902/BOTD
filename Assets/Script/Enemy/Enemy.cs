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

    [Header("Audio")]
    public AudioClip hitClip;
    public AudioSource hitAudioSource;
    [Header("Vision Settings")]
    public LayerMask visionMask;
    private bool isDead = false;

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

    protected bool isAttacking = false;

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
                if (CanSeePlayer())
                {
                    if (!isAttacking)
                        StartCoroutine(HandleAttack());
                }
                else
                {
                    // Jika tidak bisa lihat, coba dekati player
                    agent.SetDestination(player.position);
                    SetAgentActive(true);
                    SetAnimationState(walk: true, attack: false, idle: false);
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

    protected virtual void OnEnable()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        currentHealth = maxHealth;
        isAttacking = false;
        isDead = false;

        agent.enabled = false;

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
            agent.enabled = true;
            agent.Warp(hit.position);
        }
        else
        {
            agent.enabled = false;
        }

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }
    }


    protected virtual IEnumerator HandleAttack()
    {
        isAttacking = true;

        if (animator != null)
        {
            animator.SetTrigger(TriggerAttack);
        }

        yield return new WaitForSeconds(GetAttackDelay());

        Attack();

        yield return new WaitForSeconds(GetAttackCoolDown());

        isAttacking = false;
    }


    protected void SetAnimationState(bool walk, bool attack, bool idle)
    {
        if (animator == null) return;
        animator.SetBool(AnimWalk, walk);
        animator.SetBool(AnimIdle, idle);
    }

    protected virtual void OnChasing(float distance) { }

    protected abstract void OnAttackDistance();

    public abstract void Attack();

    public virtual void TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (hitClip != null && hitAudioSource != null)
        {
            hitAudioSource.PlayOneShot(hitClip);
        }

        if (currentHealth <= 0)
            Die();
    }


    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;

        OnDeath?.Invoke(this);

        // Matikan AI dan Agent
        SetAgentActive(false);
        agent.enabled = false;
        this.enabled = false;

        // Set animasi mati
        if (animator != null)
        {
            animator.SetBool(AnimWalk, false);
            animator.SetBool(AnimIdle, false);
            animator.SetTrigger("Die");
        }

        PlayerStat playerStat = player.GetComponent<PlayerStat>();
        if (playerStat != null)
        {
            playerStat.Heal(5);
        }

        StartCoroutine(WaitAndDestroy());
    }

    IEnumerator WaitAndDestroy()
    {
        // Asumsikan durasi animasi kematian 3 detik
        yield return new WaitForSeconds(3f);

        // Hapus objek
        Destroy(gameObject);
    }


    protected void SetAgentActive(bool active)
    {
        agent.updatePosition = active;
        agent.updateRotation = active;
        agent.isStopped = !active;
    }

    protected virtual bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Vector3 target = player.position + Vector3.up * 0.5f;
        Vector3 direction = (target - origin).normalized;
        float distance = Vector3.Distance(origin, target);
        float sphereRadius = 1f;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance, visionMask))
        {
            //Debug.DrawLine(origin, hit.point, Color.red, 1f);
            return hit.collider.CompareTag("Player");
        }

        if (Physics.SphereCast(origin, sphereRadius, direction, out RaycastHit sphereHit, distance, visionMask))
        {
            //Debug.DrawRay(origin, direction * distance, Color.yellow, 1f);
            return sphereHit.collider.CompareTag("Player");
        }

        return false;
    }




    protected abstract float GetAttackDelay();
    protected abstract float GetAttackCoolDown();

}
