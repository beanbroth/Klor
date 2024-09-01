using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorldItem : MonoBehaviour, IInteractable
{
    public BaseItemData itemData;
    public BaseItemInstance itemInstance;
    public TextMeshPro hoverText;
    [SerializeField] private Renderer itemRenderer;
    [SerializeField] private Transform quadTransform;

    //:D :D :D
    [SerializeField] private Rigidbody rb;

    [SerializeField] private float spawnForce = 5f;
    [SerializeField] private float spawnTorque = 2f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        SetupItem();
    }

    private void SetupItem()
    {
        hoverText.text = itemData.itemName;
        hoverText.gameObject.SetActive(false);

        if (itemRenderer != null && itemData.itemSprite != null)
        {
            itemRenderer.material.mainTexture = itemData.itemSprite.texture;
        }

        if (quadTransform != null && itemData.itemSprite != null)
        {
            float aspectRatio = (float)itemData.itemSprite.texture.width / itemData.itemSprite.texture.height;
            quadTransform.localScale = new Vector3(aspectRatio, 1, 1);
        }
    }

    public void Spawn(Vector3 spawnPosition, Vector3 spawnDirection)
    {
        transform.position = spawnPosition;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.AddForce(spawnDirection.normalized * spawnForce, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * spawnTorque, ForceMode.Impulse);
    }

    public void Interact()
    {
        if (itemInstance == null)
        {
            itemInstance = new BaseItemInstance(itemData, 0, 0);
        }
        InventoryManager.Instance.AddItem(itemInstance);
        Destroy(gameObject);
    }

    public void OnHoverEnter()
    {
        hoverText.gameObject.SetActive(true);
    }

    public void OnHoverExit()
    {
        hoverText.gameObject.SetActive(false);
    }
}

public interface IInteractable
{
    void Interact();
    void OnHoverEnter();
    void OnHoverExit();
}