using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorldItem : MonoBehaviour, IInteractable
{
    public BaseItemData itemData;
    public BaseItemInstance itemInstance;
    public TextMeshPro hoverText;
    public Rigidbody rb;
    [SerializeField] private Renderer itemRenderer;
    [SerializeField] private Transform quadTransform;
    
    private MaterialPropertyBlock propertyBlock;
    private static readonly int MainTexProperty = Shader.PropertyToID("_MainTex");

    private static ItemFactory itemFactory;
    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
        if (itemFactory == null)
        {
            itemFactory = new ItemFactory();
        }
    }

    private void Start()
    {
        SetupItem();
    }

    public void SetupItem()
    {
        hoverText.text = itemData.ItemName;
        hoverText.gameObject.SetActive(false);

        if (itemRenderer != null && itemData.ItemSprite != null)
        {
            itemRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetTexture(MainTexProperty, itemData.ItemSprite.texture);
            itemRenderer.SetPropertyBlock(propertyBlock);
        }

        if (quadTransform != null && itemData.ItemSprite != null)
        {
            float aspectRatio = (float)itemData.ItemSprite.texture.width / itemData.ItemSprite.texture.height;
            quadTransform.localScale = new Vector3(aspectRatio, 1, 1);
        }
    }
    public void Interact()
    {
        Debug.Log($"Interact method called on {gameObject.name}");
        Debug.Log($"ItemData type: {itemData.GetType().Name}");

        if (itemInstance == null)
        {
            Debug.Log("ItemInstance is null. Creating new instance...");
            try
            {
                itemInstance = itemFactory.CreateItemInstance(itemData, 0, 0);
                Debug.Log($"Successfully created ItemInstance of type: {itemInstance.GetType().Name}, with a data of type {itemInstance.ItemData.GetType().Name}");
            }
            catch (System.ArgumentException e)
            {
                Debug.LogError($"Failed to create ItemInstance: {e.Message}");
                return;
            }
        }
        else
        {
            Debug.Log($"Using existing ItemInstance of type: {itemInstance.GetType().Name}");
        }

        Debug.Log("Attempting to add item to inventory...");
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem(itemInstance);
            Debug.Log("Item successfully added to inventory.");
        }
        else
        {
            Debug.LogError("InventoryManager.Instance is null. Unable to add item to inventory.");
        }

        Debug.Log($"Destroying ItemPickup GameObject: {gameObject.name}");
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