using UnityEngine;



public class PlayerInteractController : MonoBehaviour
{
    public float interactionRange = 2f;
    public LayerMask interactableLayer;
    public InventoryManager playerInventory;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }
    }

    void TryInteract()
    {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactableLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact(this);
            }
        }
    }

    public bool AddItemToInventory(BaseItemInstance item)
    {
        return playerInventory.AddItem(item);
    }
}

public interface IInteractable
{
    void Interact(PlayerInteractController player);
    string GetInteractionPrompt();
}