using UnityEngine;
using UnityEngine.InputSystem;

public class TowerPlacement : MonoBehaviour
{
    [Header("Placement Settings")]
    [SerializeField] private GameObject towerPrefab;
    [SerializeField] private LayerMask placementLayer;
    [SerializeField] private LayerMask blockingLayer;
    [SerializeField] private float gridSize = 1f;

    [Header("Visual")]
    [SerializeField] private GameObject placementPreview;
    [SerializeField] private Color validColor = Color.green;
    [SerializeField] private Color invalidColor = Color.red;

    private Camera cam;
    private bool isPlacingTower = false;
    private GameObject currentPreview;
    private SpriteRenderer previewRenderer;
    private GameObject rangeIndicator;

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (isPlacingTower)
        {
            UpdatePlacementPreview();

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                TryPlaceTower();
            }

            if (Mouse.current.rightButton.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CancelPlacement();
            }
        }
    }

    public void StartPlacingTower(GameObject tower)
    {
        // Cancel any existing placement
        if (isPlacingTower)
        {
            CancelPlacement();
        }

        towerPrefab = tower;
        isPlacingTower = true;

        // Create preview
        if (placementPreview != null)
        {
            currentPreview = Instantiate(placementPreview);
            previewRenderer = currentPreview.GetComponent<SpriteRenderer>();
        }
        else if (towerPrefab != null)
        {
            currentPreview = Instantiate(towerPrefab);
            previewRenderer = currentPreview.GetComponent<SpriteRenderer>();

            Tower towerComponent = currentPreview.GetComponent<Tower>();
            if (towerComponent != null)
            {
                towerComponent.enabled = false;
            }
        }

        // Create range indicator
        CreateRangeIndicator();
    }

    private void CreateRangeIndicator()
    {
        if (towerPrefab == null) return;

        Tower tower = towerPrefab.GetComponent<Tower>();
        if (tower == null) return;

        rangeIndicator = new GameObject("RangeIndicator");
        LineRenderer lineRenderer = rangeIndicator.AddComponent<LineRenderer>();

        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.startColor = new Color(1f, 1f, 1f, 0.5f);
        lineRenderer.endColor = new Color(1f, 1f, 1f, 0.5f);
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.sortingOrder = -1;

        // Draw circle
        int segments = 64;
        lineRenderer.positionCount = segments + 1;

        float range = tower.GetRange();
        float angle = 0f;

        for (int i = 0; i <= segments; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * range;
            float y = Mathf.Cos(Mathf.Deg2Rad * angle) * range;
            lineRenderer.SetPosition(i, new Vector3(x, y, 0f));
            angle += 360f / segments;
        }

        rangeIndicator.transform.SetParent(currentPreview.transform);
        rangeIndicator.transform.localPosition = Vector3.zero;
    }

    private void UpdatePlacementPreview()
    {
        if (currentPreview == null) return;

        Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorldPos.z = 0f;

        Vector3 snappedPos = new Vector3(
            Mathf.Round(mouseWorldPos.x / gridSize) * gridSize,
            Mathf.Round(mouseWorldPos.y / gridSize) * gridSize,
            0f
        );

        currentPreview.transform.position = snappedPos;

        bool isValidPlacement = IsValidPlacement(snappedPos);
        if (previewRenderer != null)
        {
            Color previewColor = isValidPlacement ? validColor : invalidColor;
            previewColor.a = 0.5f;
            previewRenderer.color = previewColor;
        }

        // Update range indicator color
        if (rangeIndicator != null)
        {
            LineRenderer lineRenderer = rangeIndicator.GetComponent<LineRenderer>();
            if (lineRenderer != null)
            {
                Color rangeColor = isValidPlacement ? Color.white : Color.red;
                rangeColor.a = 0.5f;
                lineRenderer.startColor = rangeColor;
                lineRenderer.endColor = rangeColor;
            }
        }
    }

    private bool IsValidPlacement(Vector3 position)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, 0.4f, blockingLayer);
        foreach (var hit in hits)
        {
            if (currentPreview == null || hit.gameObject != currentPreview)
            {
                return false;
            }
        }
        return true;
    }

    private void TryPlaceTower()
    {
        if (currentPreview == null || towerPrefab == null) return;

        Vector3 placementPos = currentPreview.transform.position;

        if (IsValidPlacement(placementPos))
        {
            Tower tower = towerPrefab.GetComponent<Tower>();
            int cost = tower != null ? tower.GetCost() : 0;

            if (GameManager.Instance != null && GameManager.Instance.SpendMoney(cost))
            {
                Instantiate(towerPrefab, placementPos, Quaternion.identity);
                // Don't cancel placement - allow placing multiple towers
                // CancelPlacement();
            }
        }
    }

    private void CancelPlacement()
    {
        isPlacingTower = false;
        if (currentPreview != null)
        {
            Destroy(currentPreview);
        }
        if (rangeIndicator != null)
        {
            Destroy(rangeIndicator);
        }
    }
}