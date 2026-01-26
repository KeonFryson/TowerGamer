using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Tower Stats")]
    [SerializeField] private float range = 5f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float damage = 1f;
    [SerializeField] private int cost = 100;

    [Header("Targeting")]
    [SerializeField] private TargetMode targetMode = TargetMode.First;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private GameObject projectilePrefab;

    [Header("Debug")]
    [SerializeField] private bool showRange = true;

    private float fireTimer = 0f;
    private Enemy currentTarget;

    public enum TargetMode
    {
        First,
        Last,
        Strong,
        Weak,
        Close
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

    public int GetCost()
    {
        return cost;
    }

    public float GetRange()
    {
        return range;
    }

    private void OnDrawGizmosSelected()
    {
        if (showRange)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}