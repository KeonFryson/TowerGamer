using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private int health = 1;
    [SerializeField] private int moneyReward = 10;
    [SerializeField] private int damage = 1;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private int currentWaypointIndex = 0;
    private float currentHealth;

    private void Start()
    {
        currentHealth = health;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (PathManager.Instance != null && PathManager.Instance.GetWaypointCount() > 0)
        {
            transform.position = PathManager.Instance.GetWaypoint(0);
            currentWaypointIndex = 1;
        }
    }

    private void Update()
    {
        MoveAlongPath();
    }

    private void MoveAlongPath()
    {
        if (PathManager.Instance == null || currentWaypointIndex >= PathManager.Instance.GetWaypointCount())
        {
            ReachEnd();
            return;
        }

        Vector3 targetPosition = PathManager.Instance.GetWaypoint(currentWaypointIndex);
        Vector3 direction = (targetPosition - transform.position).normalized;

        transform.position += direction * moveSpeed * Time.deltaTime;

        // Check if reached waypoint
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            currentWaypointIndex++;
        }
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(moneyReward);
        }
        Destroy(gameObject);
    }

    private void ReachEnd()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoseLife(damage);
        }
        Destroy(gameObject);
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
}