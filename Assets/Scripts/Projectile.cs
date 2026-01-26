using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float maxLifetime = 5f;

    private Enemy target;
    private float damage;
    private float lifetime = 0f;

    public void SetTarget(Enemy targetEnemy, float damageAmount)
    {
        target = targetEnemy;
        damage = damageAmount;
    }

    private void Update()
    {
        lifetime += Time.deltaTime;

        if (lifetime >= maxLifetime)
        {
            Destroy(gameObject);
            return;
        }

        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 direction = (target.GetPosition() - transform.position).normalized;

        // Make the projectile face the target (2D)
        if (direction != Vector3.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        transform.position += direction * speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, target.GetPosition()) < 0.2f)
        {
            HitTarget();
        }
    }

    private void HitTarget()
    {
        if (target != null)
        {
            target.TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}