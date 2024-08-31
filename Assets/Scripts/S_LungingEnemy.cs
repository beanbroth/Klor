using System.Collections;
using UnityEngine;

public class S_LungingEnemy : BaseEnemyMovementController
{
    [SerializeField] private float lungeInitiateRange = 5f;
    [SerializeField] private float lungeMaxDuration = 1f;
    [SerializeField] private float chargeUpDuration = 0.8f;
    [SerializeField] private float initialHorizontalLungeVelocity = 10f;
    [SerializeField] private float initialVerticalLungeVelocity = 5f;
    [SerializeField] private SO_Float gravity;

    private bool isCharging = false;
    private bool isLunging = false;
    private bool isGrounded = true;
    private Coroutine lungeCoroutine;

    protected override void Update()
    {
        if (isDead) return;
        base.Update();
        if (!isStunned && !isCharging && !isLunging && isGrounded)
        {
            CheckForLunge();
        }
    }

    private void CheckForLunge()
    {
        if (Vector3.Distance(transform.position, player.position) <= lungeInitiateRange)
        {
            StartLunge();
        }
    }

    private void StartLunge()
    {
        if (lungeCoroutine != null)
        {
            StopCoroutine(lungeCoroutine);
        }
        lungeCoroutine = StartCoroutine(LungeRoutine());
    }

    private IEnumerator LungeRoutine()
    {
        isCharging = true;
        agent.isStopped = true;

        // Charge up
        float chargeTimer = 0f;
        while (chargeTimer < chargeUpDuration)
        {
            chargeTimer += Time.deltaTime;
            yield return null;
        }

        isCharging = false;
        isLunging = true;
        isGrounded = false;

        Vector3 startPosition = transform.position;
        Vector3 directionToPlayer = (player.position - startPosition).normalized;
        Vector3 initialHorizontalVelocity = directionToPlayer * initialHorizontalLungeVelocity;
        Vector3 initialVelocity = initialHorizontalVelocity + Vector3.up * initialVerticalLungeVelocity;

        float elapsedTime = 0f;
        bool hasReachedApex = false;

        while (elapsedTime < lungeMaxDuration && !isGrounded)
        {
            Vector3 currentPosition = CalculatePosition(startPosition, initialVelocity, elapsedTime);

            if (!hasReachedApex && currentPosition.y < transform.position.y)
            {
                hasReachedApex = true;
            }

            if (hasReachedApex && currentPosition.y <= startPosition.y)
            {
                currentPosition.y = startPosition.y;
                isGrounded = true;
            }

            transform.position = currentPosition;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        CompleteLunge();
    }

    private void CompleteLunge()
    {
        isLunging = false;
        isGrounded = true;
        agent.isStopped = false;

        if (isDead)
        {
            DisableAI();
        }
        else if (Vector3.Distance(transform.position, player.position) > 0.5f)
        {
            agent.SetDestination(player.position);
        }
    }

    private Vector3 CalculatePosition(Vector3 startPos, Vector3 initialVelocity, float time)
    {
        return startPos + initialVelocity * time + 0.5f * gravity.Value * Vector3.up * time * time;
    }

    protected override void HandleDamageTaken(DamageInfo damageInfo)
    {
        base.HandleDamageTaken(damageInfo);

        if (damageInfo.isKillingBlow)
        {
            isDead = true;
            DisableAI();

            // Don't interrupt the lunge if it's ongoing
            if (!isLunging && !isCharging)
            {
            }
        }
    }

    private void DisableAI()
    {
        if (lungeCoroutine != null)
        {
            StopCoroutine(lungeCoroutine);
        }
        agent.isStopped = true;
        this.enabled = false;
        // Add any other necessary AI disabling logic here
    }
}