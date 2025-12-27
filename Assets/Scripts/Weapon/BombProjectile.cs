using UnityEngine;

public class BombProjectile : MonoBehaviour
{
    public WeaponData data;
    private Vector2 dir;
    private ObjectPool pool;
    private float timer;
    private PlayerStats stats;

    public void Init(WeaponData weapon, Vector2 direction, ObjectPool objectPool)
    {
        data = weapon;
        dir = direction.normalized;
        pool = objectPool;
        timer = 0f;
        if (stats == null) stats = PlayerStats.Instance != null ? PlayerStats.Instance : Object.FindFirstObjectByType<PlayerStats>();
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        float speed = stats != null ? stats.ComputeProjectileSpeed(data.speed) : data.speed;
        transform.position += (Vector3)(dir * speed * dt);
        timer += dt;
        if (timer >= data.bombFuseTime)
        {
            Explode();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<EnemyController>() != null)
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (data.explosionPrefab != null)
        {
            var go = GameObject.Instantiate(data.explosionPrefab, transform.position, Quaternion.identity);
            var ex = go.GetComponent<Explosion>();
            if (ex != null)
            {
                int dmg = stats != null ? stats.ComputeDamage(data.damage) : data.damage;
                ex.Trigger(dmg, data.explosionRadius);
            }
        }
        pool.Return(gameObject);
    }
}
