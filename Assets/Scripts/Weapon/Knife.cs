using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Knife : MonoBehaviour
{
    public WeaponData data;

    private Rigidbody2D rb;
    private PlayerStats stats;

    // chống hit spam (như code cũ)
    private readonly Dictionary<EnemyController, float> lastHitTime = new();
    private float perEnemyHitCooldown = 0.15f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Init(WeaponData weapon, Vector2 dir, ObjectPool objectPool)
    {
        data = weapon;
        stats = PlayerStats.Instance != null
            ? PlayerStats.Instance
            : Object.FindFirstObjectByType<PlayerStats>();

        Vector2 velocity = dir.normalized * data.speed;
        rb.linearVelocity = velocity;

        UpdateRotation(velocity);
    }

    private void FixedUpdate()
    {
        // quay dao theo hướng đang bay
        if (rb.linearVelocity.sqrMagnitude > 0.001f)
        {
            UpdateRotation(rb.linearVelocity);
        }
    }

    private void UpdateRotation(Vector2 vel)
    {
        float angle = Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // nếu đụng enemy thì gây damage
        EnemyController enemy = collision.collider.GetComponent<EnemyController>();
        if (enemy == null) return;

        float now = Time.time;
        if (lastHitTime.TryGetValue(enemy, out float t) && now - t < perEnemyHitCooldown)
            return;

        lastHitTime[enemy] = now;

        int dmg = stats != null ? stats.ComputeDamage(data.damage) : data.damage;
        enemy.TakeDamage(dmg);
        // không cần code bounce, Rigidbody2D + PhysicsMaterial tự xử lý
    }
}
