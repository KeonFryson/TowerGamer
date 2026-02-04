using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private int health = 1;
    [SerializeField] private int moneyReward = 10;
    [SerializeField] private int damage = 1;
    [SerializeField] private int cost = 1;

    [Header("Tier")]
    [SerializeField] private EnemyTier tier;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private int currentNodeIndex = 0;
    private float currentHealth;

    public int Cost => cost;
    public EnemyTier Tier => tier;

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
            currentNodeIndex = 0;
        }
    }

    private void Update()
    {
        MoveAlongPath();
    }

    private void MoveAlongPath()
    {
        if (PathManager.Instance == null)
        {
            ReachEnd();
            return;
        }

        PathNodeData currentNode = PathManager.Instance.GetNode(currentNodeIndex);
        if (currentNode == null)
        {
            ReachEnd();
            return;
        }

        Vector3 targetPosition = currentNode.position;
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            if (currentNode.nextNodeIndices != null && currentNode.nextNodeIndices.Length > 0)
            {
                int nextIdx = currentNode.nextNodeIndices[Random.Range(0, currentNode.nextNodeIndices.Length)];
                currentNodeIndex = nextIdx;
            }
            else
            {
                ReachEnd();
            }
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

        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnEnemyDefeated();
        }

        Destroy(gameObject);
    }

    private void ReachEnd()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoseLife(damage);
        }
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnEnemyDefeated();
        }
        Destroy(gameObject);
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
}

public enum EnemyTier
{
    Baby,
    Weak,
    Mid,
    Strong,
    Elite,
    Boss
}