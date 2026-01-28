using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerShopUI : MonoBehaviour
{
    [System.Serializable]
    public class TowerButton
    {
        public GameObject towerPrefab;
    }

    [Header("Tower Buttons")]
    [SerializeField] private GameObject TowerShopPanel;
    [SerializeField] private TowerButton[] towerButtons;
    [SerializeField] private TowerPlacement towerPlacement;
    [Header("Button Template")]
    [SerializeField] private GameObject buttonTemplate; // Assign your TowerButton prefab in the inspector
    [SerializeField] private Transform buttonParent;    // Assign the parent container for buttons

    // Store references to buy buttons and their costs for updating
    private struct ButtonData
    {
        public Button buyButton;
        public int cost;
        public GameObject towerPrefab;
        public GameObject buttonObj;
    }
    private ButtonData[] buttonDataArray;

    private void Start()
    {
        if (towerPlacement == null)
        {
            towerPlacement = FindFirstObjectByType<TowerPlacement>();
        }

        SetupButtons();
        TowerShopPanel.SetActive(false);

        // Subscribe to money changed event
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMoneyChanged.AddListener(OnMoneyChanged);
        }
    }

    private void SetupButtons()
    {
        // Remove old buttons if any
        foreach (Transform child in buttonParent)
        {
            Destroy(child.gameObject);
        }

        buttonDataArray = new ButtonData[towerButtons.Length];

        for (int i = 0; i < towerButtons.Length; i++)
        {
            TowerButton towerButton = towerButtons[i];
            if (towerButton.towerPrefab != null)
            {
                GameObject buttonObj = Instantiate(buttonTemplate, buttonParent);

                // Find components in the template
                Image icon = buttonObj.transform.Find("Icon").GetComponent<Image>();
                TextMeshProUGUI nameText = buttonObj.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
                Button buyButton = buttonObj.transform.Find("Button").GetComponent<Button>();
                TextMeshProUGUI costText = buttonObj.transform.Find("Button/BuyButton").GetComponent<TextMeshProUGUI>();

                Tower tower = towerButton.towerPrefab.GetComponent<Tower>();
                int cost = tower != null ? tower.GetCost() : 0;

                // Use towerPrefab's name for towerName
                string towerName = towerButton.towerPrefab.name;

                // Get sprite from towerPrefab
                Sprite sprite = null;
                SpriteRenderer sr = towerButton.towerPrefab.GetComponentInChildren<SpriteRenderer>();
                if (sr != null)
                {
                    sprite = sr.sprite;
                }
                else
                {
                    Image img = towerButton.towerPrefab.GetComponentInChildren<Image>();
                    if (img != null)
                    {
                        sprite = img.sprite;
                    }
                }

                if (icon != null && sprite != null)
                {
                    icon.sprite = sprite;
                }
                if (nameText != null)
                {
                    nameText.text = towerName;
                }
                if (costText != null)
                {
                    costText.text = "$" + cost;
                }

                GameObject prefab = towerButton.towerPrefab;
                int capturedIndex = i;
                buyButton.onClick.AddListener(() => OnTowerButtonClicked(prefab, cost));

                buttonDataArray[i] = new ButtonData
                {
                    buyButton = buyButton,
                    cost = cost,
                    towerPrefab = prefab,
                    buttonObj = buttonObj
                };
            }
        }

        // Initial update
        OnMoneyChanged(GameManager.Instance != null ? GameManager.Instance.GetMoney() : 0);
    }

    private void OnTowerButtonClicked(GameObject towerPrefab, int cost)
    {
        if (GameManager.Instance != null && GameManager.Instance.GetMoney() >= cost)
        {
            if (towerPlacement != null)
            {
                towerPlacement.StartPlacingTower(towerPrefab, cost);
            }
        }
    }

    // Called when money changes
    private void OnMoneyChanged(int currentMoney)
    {
        if (buttonDataArray == null) return;

        foreach (var data in buttonDataArray)
        {
            if (data.buyButton != null)
            {
                bool canAfford = currentMoney >= data.cost;
                data.buyButton.interactable = canAfford;

                // Optional: visually gray out the buttonObj if not interactable
                CanvasGroup cg = data.buttonObj.GetComponent<CanvasGroup>();
                if (cg == null)
                {
                    cg = data.buttonObj.AddComponent<CanvasGroup>();
                }
                cg.alpha = canAfford ? 1f : 0.5f;
            }
        }

        // If currently placing a tower, cancel if not enough money
        if (towerPlacement != null && towerPlacement.IsPlacingTower)
        {
            int placingCost = towerPlacement.CurrentPlacingCost;
            if (currentMoney < placingCost)
            {
                towerPlacement.CancelPlacement();
            }
        }
    }

    public void ToggleShop()
    {
        bool isActive = !TowerShopPanel.activeSelf;
        OpenTowerShop(isActive);
    }

    private void OpenTowerShop(bool isOpen)
    {
        TowerShopPanel.SetActive(isOpen);
    }
}