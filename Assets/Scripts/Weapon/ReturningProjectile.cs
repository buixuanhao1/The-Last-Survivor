using UnityEngine;
using System.Collections.Generic;

public class ReturningProjectile : MonoBehaviour
{
    public WeaponData data;
    private Vector2 moveDir;
    private ObjectPool pool;
    private Transform owner;
    private Vector3 spawnPos;
    private bool returning = false;
    private float traveled = 0f;
    private float maxOutDistance = 8f; // distance to travel outward before returning
    private float safetyLifeTime = 5f; // fallback to avoid stuck
    private float lifeTimer = 0f;
    private PlayerStats stats;
    private readonly Dictionary<EnemyController, float> lastHitTime = new Dictionary<EnemyController, float>();

    public void Init(WeaponData weapon, Vector2 direction, ObjectPool objectPool, Transform ownerTransform, float outDistance = -1f)
    {
        data = weapon;
        moveDir = direction.normalized;
        pool = objectPool;
        owner = ownerTransform;
        spawnPos = transform.position;
        returning = false;
        traveled = 0f;
        lifeTimer = 0f;
        if (stats == null) stats = PlayerStats.Instance != null ? PlayerStats.Instance : Object.FindFirstObjectByType<PlayerStats>();
        // set out distance
        if (outDistance > 0f)
            maxOutDistance = outDistance;
        else if (data != null)
            maxOutDistance = Mathf.Max(0.1f, data.returningOutDistance);

        float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        lifeTimer += dt;
        float speed = stats != null ? stats.ComputeProjectileSpeed(data.speed) : data.speed;

        if (!returning)
        {
            Vector3 delta = (Vector3)moveDir * speed * dt;
            transform.position += delta;
            traveled += delta.magnitude;
            if (traveled >= maxOutDistance)
            {
                returning = true;
            }
        }
        else
        {
            if (owner == null)
            {
                // No owner, just return to pool
                pool.Return(gameObject);
                return;
            }
            Vector3 toOwner = (owner.position - transform.position);
            Vector3 dir = toOwner.normalized;
            transform.position += dir * speed * dt;
            if (toOwner.sqrMagnitude < 0.04f)
            {
                pool.Return(gameObject);
                return;
            }
        }

        if (data.rotateSpeed != 0)
            transform.Rotate(0, 0, data.rotateSpeed * Time.deltaTime);

        if (lifeTimer >= safetyLifeTime)
        {
            pool.Return(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyController enemy = other.GetComponent<EnemyController>();
        if (enemy != null)
        {
            float t = Time.time;
            float last;
            if (!lastHitTime.TryGetValue(enemy, out last) || (t - last) >= 0.2f)
            {
                lastHitTime[enemy] = t;
                int dmg = stats != null ? stats.ComputeDamage(data.damage) : data.damage;
                enemy.TakeDamage(dmg);
            }
            // Do not despawn on hit; keep flying until it returns to owner or safetyLifetime
        }
    }
}
