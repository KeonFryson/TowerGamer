using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TowerSelector : MonoBehaviour
{
    [SerializeField] private TowerDataPanelUI towerDataPanelUI;
    [SerializeField] private LayerMask towerLayerMask = ~0; // Default: all layers, set in Inspector

    private Camera cam;
    private Tower selectedTower;

    private void Start()
    {
        cam = Camera.main;
        if (towerDataPanelUI != null)
            towerDataPanelUI.Hide();
    }

    private void Update()
    {
        // Use new Input System for mouse click
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && !EventSystem.current.IsPointerOverGameObject())
        {
            Vector2 mousePos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Collider2D hit = Physics2D.OverlapPoint(mousePos, towerLayerMask);
            Debug.Log("Mouse clicked at: " + mousePos);
            Debug.Log("Hit collider: " + (hit != null ? hit.name : "None"));

            if (hit != null)
            {
                Tower tower = hit.GetComponent<Tower>();
                if (tower != null && tower.IsPlaced)
                {
                    // Deselect previous tower
                    if (selectedTower != null && selectedTower != tower)
                        selectedTower.HideRangeIndicator();

                    // Select new tower
                    selectedTower = tower;
                    selectedTower.ShowRangeIndicator();

                    towerDataPanelUI.ShowTowerData(tower);
                    return;
                }
            }

            // Deselect if clicking empty space
            if (selectedTower != null)
            {
                selectedTower.HideRangeIndicator();
                selectedTower = null;
            }
            towerDataPanelUI.Hide();
        }
    }
}