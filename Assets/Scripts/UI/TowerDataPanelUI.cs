 
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerDataPanelUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image spriteImage;
    [SerializeField] private TMP_Dropdown targetingDropdown;
    [SerializeField] private Transform upgradeButtonsParent; // Should have 3 children: Upgrade1, Upgrade2, Upgrade3
    [SerializeField] private Sprite[] upgradeBarSprites; // Assign Green_1, Green2, Green_3, Green_end, Green_Full in order
    [SerializeField] private Button SellButton;
    [SerializeField] private TextMeshProUGUI SellPrice;

    private Tower currentTower;
    private int sellAmount;


    private void Start()
    {
       
        // Setup Sell button
        if (SellButton != null)
        {
            SellButton.onClick.AddListener(OnSellButtonPressed);
        }


    }

    private void Update()
    {
        // Refresh UI if panel is open and a tower is selected
        if (panel.activeSelf && currentTower != null)
        {
            ShowTowerData(currentTower);
        }

        if(currentTower == null)
        {
            Hide();
        }
        else
        {
            sellAmount = currentTower.GetSellPrice();
            if (SellPrice != null)
            {
                SellPrice.text = $"${sellAmount}";
            }
        }
    }

    public void ShowTowerData(Tower tower)
    {
        if (tower == null) return;

        currentTower = tower;
        panel.SetActive(true);

        string displayName = tower.name.Replace("(Clone)", "").Trim();
        nameText.text = displayName;
        spriteImage.sprite = tower.GetComponentInChildren<SpriteRenderer>().sprite;
         
        // Setup upgrade buttons for 3 paths
        for (int path = 0; path < 3; path++)
        {
            Transform upgradeButtonGroup = upgradeButtonsParent.GetChild(path);
            Button upgradeButton = upgradeButtonGroup.Find("Button").GetComponent<Button>();
            TextMeshProUGUI buttonText = upgradeButtonGroup.Find("Button/Text (TMP)").GetComponent<TextMeshProUGUI>();
            Image upgradeIcon = upgradeButtonGroup.Find("UpgradeNumberImage").GetComponent<Image>();

            TowerUpgrade nextUpgrade = tower.GetNextUpgrade(path);



            // Setup targeting dropdown
            if (targetingDropdown != null)
            {
                targetingDropdown.ClearOptions();
                var options = System.Enum.GetNames(typeof(Tower.TargetMode));
                targetingDropdown.AddOptions(new System.Collections.Generic.List<string>(options));
                targetingDropdown.value = (int)tower.targetMode;
                targetingDropdown.onValueChanged.RemoveAllListeners();
                targetingDropdown.onValueChanged.AddListener(idx =>
                {
                    tower.targetMode = (Tower.TargetMode)idx;
                });

            }

            // Set the upgrade bar image based on the current tier
            int tier = tower.upgradeTiers[path];
            if (upgradeBarSprites != null && upgradeBarSprites.Length >= 5)
            {
                // 0: Green_1, 1: Green2, 2: Green_3, 3: Green_end, 4: Green_Full
                if (tier == 0)
                    upgradeIcon.sprite = upgradeBarSprites[0];
                else if (tier == 1)
                    upgradeIcon.sprite = upgradeBarSprites[1];
                else if (tier == 2)
                    upgradeIcon.sprite = upgradeBarSprites[2];
                else if (tier == 3)
                    upgradeIcon.sprite = upgradeBarSprites[3];
                else // Maxed
                    upgradeIcon.sprite = upgradeBarSprites[4];
            }

            // Remove previous listeners
            upgradeButton.onClick.RemoveAllListeners();

            if (nextUpgrade != null && tower.CanUpgrade(path))
            {
                upgradeButton.interactable = GameManager.Instance.GetMoney() >= nextUpgrade.cost;
                buttonText.text = $"{nextUpgrade.upgradeName}\n${nextUpgrade.cost}";

                // Add upgrade logic
                int capturedPath = path;
                upgradeButton.onClick.AddListener(() =>
                {
                    if (GameManager.Instance.GetMoney() >= nextUpgrade.cost)
                    {
                        GameManager.Instance.SpendMoney(nextUpgrade.cost);
                        tower.ApplyUpgrade(capturedPath);
                        ShowTowerData(tower); // Refresh UI
                    }
                });
            }
            else
            {
                upgradeButton.interactable = false;
                buttonText.text = "Maxed";
            }


        }
    }


    public void OnSellButtonPressed()
    {
        if (currentTower != null)
        {
            
            GameManager.Instance.AddMoney(sellAmount);
            Destroy(currentTower.gameObject);
            Hide();
        }
    }

    public void Hide()
    {
        panel.SetActive(false);
        currentTower = null;
    }
}