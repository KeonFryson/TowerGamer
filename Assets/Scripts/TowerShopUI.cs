using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerShopUI : MonoBehaviour
{
    [System.Serializable]
    public class TowerButton
    {
        public GameObject towerPrefab;
        public Button button;
        public TextMeshProUGUI costText;
        public Image icon;
    }

    [Header("Tower Buttons")]
    [SerializeField] private TowerButton[] towerButtons;
    [SerializeField] private TowerPlacement towerPlacement;

    private void Start()
    {
        if (towerPlacement == null)
        {
            towerPlacement = FindFirstObjectByType<TowerPlacement>();
        }

        SetupButtons();
    }

    private void SetupButtons()
    {
        foreach (TowerButton towerButton in towerButtons)
        {
            if (towerButton.button != null && towerButton.towerPrefab != null)
            {
                Tower tower = towerButton.towerPrefab.GetComponent<Tower>();
                int cost = tower != null ? tower.GetCost() : 0;

                if (towerButton.costText != null)
                {
                    towerButton.costText.text = "$" + cost;
                }

                GameObject prefab = towerButton.towerPrefab;
                towerButton.button.onClick.AddListener(() => OnTowerButtonClicked(prefab));
            }
        }
    }

    private void OnTowerButtonClicked(GameObject towerPrefab)
    {
        if (towerPlacement != null)
        {
            towerPlacement.StartPlacingTower(towerPrefab);
        }
    }
}