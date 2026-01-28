using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TowerUpgrade
{
    public string upgradeName;
    public string description;
    public int cost;
    public Sprite icon;

    // Example stat bonuses (customize as needed)
    public float rangeBonus;
    public float fireRateBonus;
    public float damageBonus;
}

 
public class Tower : MonoBehaviour
{
    [Header("Tower Stats")]
    [SerializeField] private float range = 5f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float damage = 1f;
    [SerializeField] private int cost = 100;

    [Header("Upgrade System")]
    [Tooltip("Current tier for each path (e.g., [2,1,0] means path 0 is tier 2, path 1 is tier 1, path 2 is tier 0)")]
    public int[] upgradeTiers = new int[3]; // Only here!
    [Tooltip("Maximum tier for each path.")]
    public int[] maxPathLevels = new int[3] { 5, 5, 5 }; // Only here!
    [Tooltip("Upgrade data for each path. Each path is an array of TowerUpgrade objects.")]
    public TowerUpgradePath[] upgradePaths = new TowerUpgradePath[3];

    [Header("Targeting")]
    public TargetMode targetMode = TargetMode.First;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private GameObject projectilePrefab;
    private GameObject rangeIndicator;
    [Header("Debug")]
    [SerializeField] private bool showRange = true;

    private float fireTimer = 0f;
    private Enemy currentTarget;
    public bool IsPlaced { get; private set; } = false;

    public void MarkAsPlaced()
    {
        IsPlaced = true;
    }

    public enum TargetMode
    {
        First,
        Last,
        Strong,
        Weak,
        Close
    }

    public void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        fireTimer += Time.deltaTime;

        if (fireTimer >= 1f / fireRate)
        {
            FindAndAttackTarget();
            fireTimer = 0f;
        }
    }

    private void FindAndAttackTarget()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        List<Enemy> enemiesInRange = new List<Enemy>();

        foreach (Enemy enemy in enemies)
        {
            if (Vector3.Distance(transform.position, enemy.GetPosition()) <= range)
            {
                enemiesInRange.Add(enemy);
            }
        }

        if (enemiesInRange.Count > 0)
        {
            currentTarget = SelectTarget(enemiesInRange);
            Attack(currentTarget);
        }
    }

    private Enemy SelectTarget(List<Enemy> enemies)
    {
        switch (targetMode)
        {
            case TargetMode.First:
                return enemies[0];
            case TargetMode.Last:
                return enemies[enemies.Count - 1];
            case TargetMode.Close:
                Enemy closest = enemies[0];
                float closestDist = Vector3.Distance(transform.position, closest.GetPosition());
                foreach (Enemy enemy in enemies)
                {
                    float dist = Vector3.Distance(transform.position, enemy.GetPosition());
                    if (dist < closestDist)
                    {
                        closest = enemy;
                        closestDist = dist;
                    }
                }
                return closest;
            case TargetMode.Strong:
                Enemy strongest = enemies[0];
                foreach (Enemy enemy in enemies)
                {
                    if ((int)enemy.Tier > (int)strongest.Tier)
                    {
                        strongest = enemy;
                    }
                }
                return strongest;
            case TargetMode.Weak:
                Enemy weakest = enemies[0];
                foreach (Enemy enemy in enemies)
                {
                    if ((int)enemy.Tier < (int)weakest.Tier)
                    {
                        weakest = enemy;
                    }
                }
                return weakest;
            default:
                return enemies[0];
        }
    }

    private void Attack(Enemy target)
    {
        if (target == null) return;

        if (projectilePrefab != null && projectileSpawnPoint != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
            Projectile proj = projectile.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.SetTarget(target, damage);
            }
        }
        else
        {
            target.TakeDamage(damage);
        }
    }

    // --- Upgrade System ---

    // Place this inside your Tower class

    /// <summary>
    /// Returns true if the given path can be upgraded (not at max tier and follows Bloons-style rules).
    /// </summary>
    public bool CanUpgrade(int path)
    {
        if (upgradePaths == null || path < 0 || path >= upgradePaths.Length || upgradePaths[path].upgrades == null)
            return false;

        // Check if already at max for this path
        if (upgradeTiers[path] >= maxPathLevels[path])
            return false;

        // Check if there are enough upgrades defined for this path
        if (upgradeTiers[path] >= upgradePaths[path].upgrades.Length)
            return false;


   
        int upgradedOtherPaths = 0;
        for (int i = 0; i < upgradeTiers.Length; i++)
        {
            if (i != path && upgradeTiers[i] > 0)
                upgradedOtherPaths++;
        }
        if (upgradedOtherPaths >= 2)
            return false;

        // Bloons-style rules:
        // Only one path can go above tier 2 (i.e., reach tier 3+)
        // Only one path can reach tier 4 or 5

        int highTierCount = 0;
        for (int i = 0; i < upgradeTiers.Length; i++)
        {
            if (upgradeTiers[i] >= 3)
                highTierCount++;
        }

        // If trying to upgrade to tier 3 and another path is already 3+
        if (upgradeTiers[path] == 2 && highTierCount > 0)
            return false;

        // If already at 3+ and another path is at 3+
        if (upgradeTiers[path] >= 3 && highTierCount > 1)
            return false;

        // Only one path can reach 4 or 5
        if (upgradeTiers[path] >= 4)
            return false;

        return true;
    }

    /// <summary>
    /// Returns the next upgrade for the given path, or null if maxed.
    /// </summary>
    public TowerUpgrade GetNextUpgrade(int path)
    {
        if (upgradePaths == null || path < 0 || path >= upgradePaths.Length || upgradePaths[path].upgrades == null)
            return null;
        int tier = upgradeTiers[path];
        if (tier < upgradePaths[path].upgrades.Length)
            return upgradePaths[path].upgrades[tier];
        return null;
    }

    /// <summary>
    /// Applies the next upgrade in the given path, if possible.
    /// </summary>
    public bool ApplyUpgrade(int path)
    {
        if (!CanUpgrade(path)) return false;
        TowerUpgrade upgrade = GetNextUpgrade(path);
        if (upgrade == null) return false;

        // Apply stat bonuses
        range += upgrade.rangeBonus;
        fireRate += upgrade.fireRateBonus;
        damage += upgrade.damageBonus;

        // Apply icon if provided
        if (upgrade.icon != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = upgrade.icon;
        }

        // Optionally update the name (if you want the tower's name to change)
        if (!string.IsNullOrEmpty(upgrade.upgradeName))
        {
            gameObject.name = upgrade.upgradeName;
        }

        upgradeTiers[path]++;
        return true;
    }

    public int GetCost()
    {
        return cost;
    }

    public float GetRange()
    {
        return range;
    }

    public void SetShowRange(bool value)
    {
        showRange = value;
    }

    public void ShowRangeIndicator()
    {
        if (rangeIndicator != null) return;

        rangeIndicator = new GameObject("RangeIndicator");
        var lineRenderer = rangeIndicator.AddComponent<LineRenderer>();

        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.startColor = new Color(1f, 1f, 1f, 0.5f);
        lineRenderer.endColor = new Color(1f, 1f, 1f, 0.5f);
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.sortingOrder = -1;

        int segments = 64;
        lineRenderer.positionCount = segments + 1;
        float range = GetRange();
        float angle = 0f;

        for (int i = 0; i <= segments; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * range;
            float y = Mathf.Cos(Mathf.Deg2Rad * angle) * range;
            lineRenderer.SetPosition(i, new Vector3(x, y, 0f));
            angle += 360f / segments;
        }

        rangeIndicator.transform.SetParent(transform);
        rangeIndicator.transform.localPosition = Vector3.zero;
    }

    public void HideRangeIndicator()
    {
        if (rangeIndicator != null)
        {
            Destroy(rangeIndicator);
            rangeIndicator = null;
        }
    }
}