using UnityEngine;

public class InventoryToggler : MonoBehaviour
{
    [SerializeField] private GameObject inventoryUI;

    private void Start()
    {
        if (inventoryUI.activeSelf)
        {
            ToggleInventory(true);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            bool inventoryOpen = !inventoryUI.activeSelf;
            ToggleInventory(inventoryOpen);
        }
    }

    private void ToggleInventory(bool inventoryOpen)
    {
        inventoryUI.SetActive(inventoryOpen);
        S_GamePauseManager.Instance.SetPaused(inventoryOpen);
        Cursor.visible = inventoryOpen;
        Cursor.lockState = inventoryOpen ? CursorLockMode.None : CursorLockMode.Locked;
    }
}