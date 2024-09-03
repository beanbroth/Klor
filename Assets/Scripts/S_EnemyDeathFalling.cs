using UnityEngine;
using System.Collections;

public class S_EnemyFalling : MonoBehaviour
{
    public float fallDuration = 1.0f;
    public float maxFallAngle = 90f;
    public Transform positionOffsetTransform;

    [SerializeField] private AnimationCurve fallingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField]
    private AnimationCurve heightOffsetCurve = new AnimationCurve(
        new Keyframe(0, 0, 0, 0),
        new Keyframe(0.7f, 0.1f, 0.5f, 0.5f),
        new Keyframe(1, 1, 2, 2)
    );

    private bool isFalling = false;
    private bool isFullyDead = false;
    private Coroutine fallingCoroutine;
    private S_EnemyHealth enemyHealth;
    private Vector3 initialPositionOffset;

    private void Start()
    {
        enemyHealth = GetComponentInParent<S_EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.OnStartFalling += StartFalling;
            enemyHealth.OnResetFalling += ResetFalling;
        }

        if (positionOffsetTransform != null)
        {
            initialPositionOffset = positionOffsetTransform.localPosition;
        }
    }

    private void StartFalling()
    {
        if (!isFalling && !isFullyDead)
        {
            isFalling = true;
            StartFallingCoroutine();
        }
    }

    private void ResetFalling()
    {
        if (isFullyDead) return;

        isFalling = false;
        if (fallingCoroutine != null)
        {
            StopCoroutine(fallingCoroutine);
        }
        StartCoroutine(QuickReset());
    }

    private IEnumerator QuickReset()
    {
        float resetDuration = 0.2f;
        float elapsedTime = 0f;
        Quaternion startRotation = transform.localRotation;
        Quaternion targetRotation = Quaternion.identity;
        Vector3 startPosition = positionOffsetTransform.localPosition;

        while (elapsedTime < resetDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / resetDuration;
            transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);
            positionOffsetTransform.localPosition = Vector3.Lerp(startPosition, initialPositionOffset, t);
            yield return null;
        }

        StartFalling();
    }

    private void StartFallingCoroutine()
    {
        if (fallingCoroutine != null)
        {
            StopCoroutine(fallingCoroutine);
        }
        fallingCoroutine = StartCoroutine(FallOverTime());
    }

    private IEnumerator FallOverTime()
    {
        float elapsedTime = 0f;
        float startAngle = transform.localRotation.eulerAngles.z;
        float targetAngle = startAngle + (Random.value > 0.5f ? maxFallAngle : -maxFallAngle);
        Vector3 startPosition = positionOffsetTransform.localPosition;

        while (elapsedTime < fallDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fallDuration;

            // Use the falling curve to determine the rotation progress
            float rotationProgress = fallingCurve.Evaluate(t);
            float currentAngle = Mathf.Lerp(startAngle, targetAngle, rotationProgress);
            transform.localRotation = Quaternion.Euler(0, 0, currentAngle);

            // Use the height offset curve to determine the position offset progress
            float offsetProgress = heightOffsetCurve.Evaluate(t);
            positionOffsetTransform.localPosition = Vector3.Lerp(startPosition, Vector3.zero, offsetProgress);

            yield return null;
        }

        isFullyDead = true;
    }

    private void OnDestroy()
    {
        if (enemyHealth != null)
        {
            enemyHealth.OnStartFalling -= StartFalling;
            enemyHealth.OnResetFalling -= ResetFalling;
        }
    }
}