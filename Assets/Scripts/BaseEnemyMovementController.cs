using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System;

public abstract class BaseEnemyMovementController : MonoBehaviour
{
    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] protected float damageStunDuration = 0.1f;
    [SerializeField] protected float destinationUpdateInterval = 0.3f;
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
        StartCoroutine(UpdateDestinationRoutine());
    }

    protected virtual void Update()
    {
        UpdateWalkingState();
    }

    protected IEnumerator UpdateDestinationRoutine()
    {
        while (!isDead)
        {
            if (!isStunned && player != null)
            {
                agent.SetDestination(player.position);
            }
            yield return new WaitForSeconds(destinationUpdateInterval);
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
        SetWalkingState(false);
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