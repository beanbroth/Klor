using UnityEngine;

public class S_MeshAnimation : MonoBehaviour
{
    [SerializeField] private float stepAngle = 5f;
    [SerializeField] private float stepFrequency = 2f;
    [SerializeField] private float bobAmplitude = 0.1f;
    [SerializeField] private float bobFrequency = 2f;

    private Vector3 initialPosition;
    private float bobOffset;
    private float stepOffset;
    private bool isWalking = true;
    private BaseEnemyMovementController enemyMovement;
    private S_EnemyHealth enemyHealth;

    void Start()
    {
        initialPosition = transform.localPosition;
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
        stepOffset = Mathf.Sin(Time.time * stepFrequency) * stepAngle;
        transform.localRotation = Quaternion.Euler(0, 0, stepOffset);
    }

    private void UpdateBob()
    {
        bobOffset = Mathf.Abs(Mathf.Sin(Time.time * bobFrequency)) * bobAmplitude;
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
}