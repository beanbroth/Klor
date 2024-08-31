using UnityEngine;
using UnityEngine.AI;
using System.Collections;


public abstract class BaseEnemyMovementController : MonoBehaviour
{
    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] protected float damageStunDuration = 0.1f;

    protected NavMeshAgent agent;
    protected Transform player;
    protected BaseHealth health;
    protected bool isStunned = false;

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
        }

        agent.speed = moveSpeed;
    }

    protected virtual void Update()
    {
        if (!isStunned)
        {
            MoveTowardsPlayer();
        }
    }

    protected virtual void MoveTowardsPlayer()
    {
        if (player != null)
        {
            agent.SetDestination(player.position);
        }
    }

    protected virtual void HandleDamageTaken(float damage)
    {
        StartCoroutine(StunRoutine());
    }

    protected IEnumerator StunRoutine()
    {
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
        }
    }
}