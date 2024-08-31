using UnityEngine;
using System.Collections;

public class S_SpriteDamageController : MonoBehaviour
{
    private int enemyIndex;
    private MaterialPropertyBlock propertyBlock;
    private Renderer rendererComponent;
    private BaseHealth healthComponent;
    [SerializeField] private float flashDuration = 0.2f;
    [SerializeField] private Color flashColor = Color.red;

    // Gizmo settings
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private Color spriteBoundsColor = Color.yellow;
    [SerializeField] private Color projectionColor = Color.green;
    [SerializeField] private Color hitPositionColor = Color.red;
    [SerializeField] private float gizmoLineWidth = 2f;


    // Debug
    private Vector3 lastWorldHitPosition;
    private Vector2 lastProjectedPosition;
    //TODO: Why does this have to exist? Potentially due to camera vs gun offset, but not sure. Investigate.
    [SerializeField] private Vector2 offsetAdjustment;

    void Start()
    {
        enemyIndex = S_DamageManager.Instance.GetUniqueEnemyIndex();
        rendererComponent = GetComponent<Renderer>();
        healthComponent = GetComponentInParent<BaseHealth>();
        if (healthComponent == null)
        {
            Debug.LogError("BaseHealth component not found on this GameObject!");
            return;
        }
        // Subscribe to events
        healthComponent.OnDamageTaken += HandleDamageTaken;
        healthComponent.OnDeath += HandleDeath;
        propertyBlock = new MaterialPropertyBlock();
        rendererComponent.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat("_EnemyID", enemyIndex);
        rendererComponent.SetPropertyBlock(propertyBlock);
        // Set initial flash color and intensity
        propertyBlock.SetColor("_FlashColor", flashColor);
        propertyBlock.SetFloat("_FlashIntensity", 0f);
        rendererComponent.SetPropertyBlock(propertyBlock);
    }

    private void HandleDamageTaken(DamageInfo damageInfo)
    {
        lastWorldHitPosition = damageInfo.hitLocation;
        Vector2 spritePosition = ProjectToSprite(lastWorldHitPosition);
        lastProjectedPosition = spritePosition;

        // Calculate rotation based on hit position (optional, for visual variety)
        float hitRotation = Mathf.Atan2(spritePosition.y - 0.5f, spritePosition.x - 0.5f);

        // Trigger visual effects for damage using the projected position
        S_DamageManager.Instance.TakeDamage(enemyIndex, damageInfo.amount, 0, spritePosition, hitRotation);

        // Trigger flash effect
        StartCoroutine(FlashRoutine());
    }

    private Vector2 ProjectToSprite(Vector3 worldHitPosition)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found!");
            return Vector2.zero;
        }

        // Get the sprite's world position and scale
        Vector3 spriteWorldPosition = transform.position;
        Vector3 spriteWorldScale = transform.lossyScale;

        // Calculate the camera's right and up vectors
        Vector3 cameraRight = mainCamera.transform.right;
        Vector3 cameraUp = mainCamera.transform.up;

        // Apply the sprite's Z rotation
        float zRotation = transform.rotation.eulerAngles.z;
        Vector3 spriteRight = Quaternion.AngleAxis(zRotation, mainCamera.transform.forward) * cameraRight;
        Vector3 spriteUp = Quaternion.AngleAxis(zRotation, mainCamera.transform.forward) * cameraUp;

        // Calculate the hit position relative to the sprite's center
        Vector3 relativeHitPosition = worldHitPosition - spriteWorldPosition;

        // Project the relative hit position onto the camera's plane
        Vector3 projectedPosition = Vector3.ProjectOnPlane(relativeHitPosition, mainCamera.transform.forward);

        // Calculate the 2D coordinates within the sprite
        float x = Vector3.Dot(projectedPosition, spriteRight);
        float y = Vector3.Dot(projectedPosition, spriteUp);

        // Normalize the position to fit within the 0-1 range, accounting for scale
        // Reverse the x-coordinate to counteract the shader's mirroring effect
        Vector2 normalizedPosition = new Vector2(
            1 - (x / spriteWorldScale.x + 0.5f),
            y / spriteWorldScale.y + 0.5f
        );

        // Apply alignment adjustment
        Vector2 adjustedOffset = GetRotationAdjustedOffset(zRotation);
        normalizedPosition += offsetAdjustment;

        // Clamp the values to ensure they're within the 0-1 range
        return new Vector2(
            Mathf.Clamp01(normalizedPosition.x),
            Mathf.Clamp01(normalizedPosition.y)
        );
    }

    private Vector2 GetRotationAdjustedOffset(float zRotation)
    {
        // Normalize the rotation to 0-360 range
        zRotation = (zRotation + 360) % 360;

        // Interpolate between offsets based on rotation
        float t = Mathf.InverseLerp(0, 180, zRotation);
        Vector2 startOffset = offsetAdjustment;
        Vector2 endOffset = -offsetAdjustment;

        return Vector2.Lerp(startOffset, endOffset, t);
    }

    private void HandleDeath()
    {
        // Handle death effects if needed
        Debug.Log("Enemy died!");
    }

    private IEnumerator FlashRoutine()
    {
        float elapsedTime = 0f;
        while (elapsedTime < flashDuration)
        {
            float intensity = Mathf.PingPong(elapsedTime / flashDuration * 2, 1f);
            propertyBlock.SetFloat("_FlashIntensity", 1);
            rendererComponent.SetPropertyBlock(propertyBlock);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        // Ensure flash is turned off at the end
        propertyBlock.SetFloat("_FlashIntensity", 0f);
        rendererComponent.SetPropertyBlock(propertyBlock);
    }

    void OnDestroy()
    {
        if (healthComponent != null)
        {
            // Unsubscribe from events
            healthComponent.OnDamageTaken -= HandleDamageTaken;
            healthComponent.OnDeath -= HandleDeath;
        }
        S_DamageManager.Instance.ReleaseEnemyIndex(enemyIndex);
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos || !Application.isPlaying) return;

        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        Vector3 spriteWorldPosition = transform.position;
        Vector3 spriteWorldScale = transform.lossyScale;
        Vector3 spriteRight = transform.right * spriteWorldScale.x;
        Vector3 spriteUp = transform.up * spriteWorldScale.y;

        // Draw sprite bounds
        DrawRectangle(spriteWorldPosition, spriteRight, spriteUp, spriteBoundsColor);

        // Draw hit position
        Gizmos.color = hitPositionColor;
        Gizmos.DrawSphere(lastWorldHitPosition, 0.05f);

        // Draw projection
        Vector3 projectedWorldPosition = spriteWorldPosition +
            (spriteRight * (lastProjectedPosition.x - 0.5f)) +
            (spriteUp * (lastProjectedPosition.y - 0.5f));
        Gizmos.color = projectionColor;
        Gizmos.DrawLine(lastWorldHitPosition, projectedWorldPosition);
        Gizmos.DrawSphere(projectedWorldPosition, 0.05f);

        // Draw camera forward
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(mainCamera.transform.position, mainCamera.transform.forward * 10f);
    }


    private void DrawRectangle(Vector3 center, Vector3 right, Vector3 up, Color color)
    {
        Vector3 topLeft = center - (right / 2) + (up / 2);
        Vector3 topRight = center + (right / 2) + (up / 2);
        Vector3 bottomLeft = center - (right / 2) - (up / 2);
        Vector3 bottomRight = center + (right / 2) - (up / 2);

        Gizmos.color = color;
        DrawLine(topLeft, topRight);
        DrawLine(topRight, bottomRight);
        DrawLine(bottomRight, bottomLeft);
        DrawLine(bottomLeft, topLeft);
    }

    private void DrawLine(Vector3 from, Vector3 to)
    {
        Gizmos.DrawLine(from, to);
        // Draw a thicker line
        Vector3 dir = (to - from).normalized;
        Vector3 perpendicular = Vector3.Cross(dir, Camera.main.transform.forward).normalized * gizmoLineWidth / 1000f;
        for (int i = -2; i <= 2; i++)
        {
            Gizmos.DrawLine(from + perpendicular * i, to + perpendicular * i);
        }
    }
}