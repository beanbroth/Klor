using UnityEngine;

public class PlayerInteractController : MonoBehaviour
{
    public float interactionDistance = 2f;
    public LayerMask interactableLayer;
    public Color gizmoColor = Color.yellow;

    private Camera mainCamera;
    private IInteractable currentInteractable;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        HandleInteraction();
    }

    private void HandleInteraction()
    {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {


                if (currentInteractable != interactable)
                {
                    if (currentInteractable != null)
                    {
                        currentInteractable.OnHoverExit();
                    }
                    currentInteractable = interactable;
                    currentInteractable.OnHoverEnter();
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    currentInteractable.Interact();
                    currentInteractable = null;
                    return;
                }

            }
        }
        else
        {
            if (currentInteractable != null)
            {
                currentInteractable.OnHoverExit();
                currentInteractable = null;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera != null)
        {
            Gizmos.color = gizmoColor;
            Vector3 direction = mainCamera.transform.forward * interactionDistance;
            Vector3 origin = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0f));
            Gizmos.DrawRay(origin, direction);
        }
    }
}
