using UnityEngine;

public class InventoryToggler : MonoBehaviour
{
    [SerializeField] private GameObject inventoryUI;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    private void ToggleInventory()
    {
        bool inventoryOpen = !inventoryUI.activeSelf;
        inventoryUI.SetActive(inventoryOpen);
        S_GamePauseManager.Instance.SetPaused(inventoryOpen);

        Cursor.visible = inventoryOpen;
        Cursor.lockState = inventoryOpen ? CursorLockMode.None : CursorLockMode.Locked;
    }
}