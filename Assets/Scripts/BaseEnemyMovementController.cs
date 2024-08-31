using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System;

public abstract class BaseEnemyMovementController : MonoBehaviour
{
    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] protected float damageStunDuration = 0.1f;
    protected NavMeshAgent agent;
    protected Transform player;
    protected BaseHealth health;
    protected bool isStunned = false;
    protected bool isWalking = false;
    protected bool isDead = false;
    public event Action<bool> OnWalkingStateChanged;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<BaseHealth>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    protected virtual void Start()
    {
        if (health != null)
        {
            health.OnDamageTaken += HandleDamageTaken;
            health.OnDeath += HandleDeath;
        }
        agent.speed = moveSpeed;
    }

    protected virtual void Update()
    {
        if (!isDead && !isStunned)
        {
            MoveTowardsPlayer();
        }
        UpdateWalkingState();
    }

    protected virtual void MoveTowardsPlayer()
    {
        if (player != null)
        {
            agent.SetDestination(player.position);
        }
    }

    protected virtual void HandleDamageTaken(DamageInfo damageInfo)
    {
        if (!isDead)
        {
            StartCoroutine(StunRoutine());
        }
    }

    protected virtual void HandleDeath()
    {
        StopAllCoroutines();
        isDead = true;
        agent.isStopped = true;
        //agent.enabled = false;
        SetWalkingState(false);
        // Don't disable the entire component, as lunges should still finish
    }

    protected IEnumerator StunRoutine()
    {
        SetWalkingState(false);
        isStunned = true;
        agent.isStopped = true;
        yield return new WaitForSeconds(damageStunDuration);
        agent.isStopped = false;
        isStunned = false;
    }

    protected virtual void OnDestroy()
    {
        if (health != null)
        {
            health.OnDamageTaken -= HandleDamageTaken;
            health.OnDeath -= HandleDeath;
        }
    }

    protected void UpdateWalkingState()
    {
        bool currentlyWalking = !isDead && !isStunned && agent.velocity.magnitude > 0.1f;
        if (currentlyWalking != isWalking)
        {
            SetWalkingState(currentlyWalking);
        }
    }

    protected virtual void SetWalkingState(bool walking)
    {
        isWalking = walking;
        OnWalkingStateChanged?.Invoke(isWalking);
    }
}