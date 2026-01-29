using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float maxLifetime = 5f;
    [SerializeField] private bool followTarget = false;

    private Enemy target;
    private float damage;
    private float lifetime = 0f;
    private Vector3 moveDirection;
    private bool lostTarget = false;

    public void SetTarget(Enemy targetEnemy, float damageAmount, bool follow = true)
    {
        target = targetEnemy;
        damage = damageAmount;
        followTarget = follow;

        // Always set moveDirection at spawn, regardless of followTarget
        if (target != null)
            moveDirection = (target.GetPosition() - transform.position).normalized;
        else
            moveDirection = Vector3.right;
    }

    private void Start()
    {
        // No need to set moveDirection here; it's always set in SetTarget
    }

    private void Update()
    {
        lifetime += Time.deltaTime;

        if (lifetime >= maxLifetime)
        {
            Destroy(gameObject);
            return;
        }

        if (followTarget && !lostTarget)
        {
            if (target == null)
            {
                lostTarget = true;
                // Keep moving in last known direction
            }
            else
            {
                Vector3 direction = (target.GetPosition() - transform.position).normalized;
                if (direction != Vector3.zero)
                {
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                }
                moveDirection = direction;
                transform.position += direction * speed * Time.deltaTime;
                return;
            }
        }

        // Move in the last known or default direction
        if (moveDirection != Vector3.zero)
        {
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}