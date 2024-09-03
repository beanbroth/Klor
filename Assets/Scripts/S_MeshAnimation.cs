using UnityEngine;
using System.Collections;

public class S_MeshAnimation : MonoBehaviour
{
    [SerializeField] private float stepAngle = 5f;
    [SerializeField] private float stepFrequency = 2f;
    [SerializeField] private float bobAmplitude = 0.1f;
    [SerializeField] private float bobFrequency = 2f;
    [SerializeField] private float scaleUpDuration = 0.3f; // Duration of scale-up animation
    private Vector3 initialPosition;
    private float bobOffset;
    private float stepOffset;
    private bool isWalking = true;
    private BaseEnemyMovementController enemyMovement;
    private S_EnemyHealth enemyHealth;

    // New variables for random offsets and scaling
    private float randomTimeOffset;
    private Vector3 initialScale;

    void Start()
    {
        initialPosition = transform.localPosition;
        initialScale = transform.localScale;
        enemyMovement = GetComponentInParent<BaseEnemyMovementController>();
        enemyHealth = GetComponentInParent<S_EnemyHealth>();
        if (enemyMovement != null)
        {
            enemyMovement.OnWalkingStateChanged += HandleWalkingStateChanged;
        }
        if (enemyHealth != null)
        {
            enemyHealth.OnDeath += HandleEnemyDeath;
        }

        // Generate a random time offset
        randomTimeOffset = Random.Range(0f, 2f * Mathf.PI);

        // Start the scale-up animation
        StartCoroutine(ScaleUpAnimation());
    }

    void OnDestroy()
    {
        if (enemyMovement != null)
        {
            enemyMovement.OnWalkingStateChanged -= HandleWalkingStateChanged;
        }
        if (enemyHealth != null)
        {
            enemyHealth.OnDeath -= HandleEnemyDeath;
        }
    }

    void Update()
    {
        if (isWalking)
        {
            UpdateStep();
            UpdateBob();
        }
        else
        {
            ResetAnimation();
        }
    }

    private void UpdateStep()
    {
        stepOffset = Mathf.Sin((Time.time + randomTimeOffset) * stepFrequency) * stepAngle;
        transform.localRotation = Quaternion.Euler(0, 0, stepOffset);
    }

    private void UpdateBob()
    {
        bobOffset = Mathf.Abs(Mathf.Sin((Time.time + randomTimeOffset) * bobFrequency)) * bobAmplitude;
        transform.localPosition = initialPosition + new Vector3(0, bobOffset, 0);
    }

    private void ResetAnimation()
    {
        transform.localRotation = Quaternion.identity;
        transform.localPosition = initialPosition;
    }

    private void HandleWalkingStateChanged(bool walking)
    {
        isWalking = walking;
    }

    private void HandleEnemyDeath()
    {
        isWalking = false;
        ResetAnimation();
        this.enabled = false; // Disable this component
    }

    private IEnumerator ScaleUpAnimation()
    {
        float elapsedTime = 0f;
        transform.localScale = Vector3.zero; // Start from zero scale

        while (elapsedTime < scaleUpDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / scaleUpDuration;
            transform.localScale = Vector3.Lerp(Vector3.zero, initialScale, t);
            yield return null;
        }

        transform.localScale = initialScale; // Ensure we end at the exact initial scale
    }
}