using System.Collections;
using UnityEngine;

public class S_LungingEnemy : BaseEnemyMovementController
{
    [SerializeField] private float lungeRange = 5f;
    [SerializeField] private float lungeDistance = 5f; // New variable for lunge distance
    [SerializeField] private float lungeSpeed = 10f;
    [SerializeField] private float chargeUpDuration = 0.8f;
    [SerializeField] private float lungeHeight = 2f; // Maximum height of the arc

    private bool isCharging = false;
    private bool isLunging = false;

    protected override void Update()
    {
        base.Update();
        if (!isStunned && !isCharging && !isLunging)
        {
            CheckForLunge();
        }
    }

    private void CheckForLunge()
    {
        if (Vector3.Distance(transform.position, player.position) <= lungeRange)
        {
            StartCoroutine(LungeRoutine());
        }
    }

    private IEnumerator LungeRoutine()
    {
        isCharging = true;
        agent.isStopped = true;

        // Charge up
        yield return new WaitForSeconds(chargeUpDuration);

        isCharging = false;
        isLunging = true;

        // Calculate lunge direction and end position
        Vector3 startPosition = transform.position;
        Vector3 directionToPlayer = (player.position - startPosition).normalized;
        Vector3 endPosition = startPosition + directionToPlayer * lungeDistance;

        // Calculate lunge duration based on distance and speed
        float lungeDuration = lungeDistance / lungeSpeed;

        float elapsedTime = 0f;
        while (elapsedTime < lungeDuration)
        {
            float t = elapsedTime / lungeDuration;

            // Calculate the current position along the path
            Vector3 currentPosition = Vector3.Lerp(startPosition, endPosition, t);

            // Add vertical displacement for the arc
            float heightOffset = Mathf.Sin(t * Mathf.PI) * lungeHeight;
            currentPosition.y += heightOffset;

            // Move to the calculated position
            transform.position = currentPosition;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the enemy lands exactly at the end position
        transform.position = endPosition;

        isLunging = false;
        agent.isStopped = false;
    }

    protected override void HandleDamageTaken(float damage)
    {
        base.HandleDamageTaken(damage);
        // Interrupt charging or lunging when damaged
        if (isCharging || isLunging)
        {
            StopAllCoroutines();
            isCharging = false;
            isLunging = false;
            agent.isStopped = false;
        }
    }
}