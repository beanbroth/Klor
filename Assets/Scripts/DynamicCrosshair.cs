using UnityEngine;
#if UNITY_EDITOR
#endif

public class DynamicCrosshair : MonoBehaviour
{
    //TODO: Make this update with weapon accuracy, player movement, etc.
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform[] crosshairSprites; // Array of 4 sprite transforms
    [SerializeField] private float defaultSize = 10f;
    [SerializeField] private float maxSize = 30f;
    [SerializeField] private float expansionSpeed = 5f;
    [SerializeField] private float contractionSpeed = 10f;

    private float currentSize;
    private Vector3 lastPlayerPosition;
    private bool isMoving;

    private void Start()
    {
        if (playerTransform == null)
        {
            Debug.LogError("Player Transform is not assigned to the Dynamic Crosshair script!");
            enabled = false;
            return;
        }

        currentSize = defaultSize;
        lastPlayerPosition = playerTransform.position;
        UpdateCrosshairSize();
    }

    private void Update()
    {
        // Check if the player is moving
        isMoving = (playerTransform.position - lastPlayerPosition).magnitude > 0.01f;
        lastPlayerPosition = playerTransform.position;

        // Adjust the crosshair size
        if (isMoving)
        {
            currentSize = Mathf.Min(currentSize + expansionSpeed * Time.deltaTime, maxSize);
        }
        else
        {
            currentSize = Mathf.Max(currentSize - contractionSpeed * Time.deltaTime, defaultSize);
        }

        UpdateCrosshairSize();
    }

    private void UpdateCrosshairSize()
    {
        // Update positions of crosshair sprites
        crosshairSprites[0].localPosition = new Vector3(0, currentSize, 0); // Top
        crosshairSprites[1].localPosition = new Vector3(0, -currentSize, 0); // Bottom
        crosshairSprites[2].localPosition = new Vector3(-currentSize, 0, 0); // Left
        crosshairSprites[3].localPosition = new Vector3(currentSize, 0, 0); // Right
    }

    #if UNITY_EDITOR
    private void OnValidate()
    {
        // Only update if we have all four sprites assigned
        if (crosshairSprites != null && crosshairSprites.Length == 4)
        {
            // Update positions of crosshair sprites to default size
            crosshairSprites[0].localPosition = new Vector3(0, defaultSize, 0); // Top
            crosshairSprites[1].localPosition = new Vector3(0, -defaultSize, 0); // Bottom
            crosshairSprites[2].localPosition = new Vector3(-defaultSize, 0, 0); // Left
            crosshairSprites[3].localPosition = new Vector3(defaultSize, 0, 0); // Right
        }
    }
    #endif
}