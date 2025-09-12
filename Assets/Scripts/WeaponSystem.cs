using UnityEngine;

public class WeaponSystem : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float fireRate = 1f;
    public float attackRange = 6f;

    private float fireCooldown;

    void Update()
    {
        fireCooldown -= Time.deltaTime;

        if (fireCooldown <= 0f)
        {
            GameObject target = FindClosestEnemy();
            if (target != null)
            {
                Shoot(target.transform.position);
                fireCooldown = 1f / fireRate; // reset cooldown
            }
        }
    }

    GameObject FindClosestEnemy()
    {
        // Lấy tất cả Collider2D trong attackRange
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);

        GameObject closest = null;
        float minDist = Mathf.Infinity;
        Vector3 pos = transform.position;

        foreach (var hit in hits)
        {
            EnemyController enemy = hit.GetComponent<EnemyController>();
            if (enemy != null)
            {
                float dist = Vector3.Distance(pos, enemy.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = enemy.gameObject;
                }
            }
        }

        return closest;
    }

    void Shoot(Vector3 targetPos)
    {
        Vector2 dir = (targetPos - transform.position).normalized;

        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        bullet.GetComponent<Shuriken>().SetDirection(dir);
    }

    // Vẽ phạm vi attackRange trong Scene để dễ debug
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
