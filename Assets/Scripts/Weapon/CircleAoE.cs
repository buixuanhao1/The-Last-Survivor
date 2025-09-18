using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class CircleAOE : MonoBehaviour
{
    private WeaponData data;
    private Transform player;

    private float lifeTime;
    private float tickTimer;
    private int damage;

    private CircleCollider2D col;
    private List<EnemyController> enemiesInside = new List<EnemyController>();

    private CircleAOEEffect effect;

    public void Init(WeaponData weapon, Transform player, int level = 1)
    {
        this.data = weapon;
        this.player = player;

        col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;

        // Rigidbody2D để trigger hoạt động
        var rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = true;

        ApplyLevelStats(level);

        lifeTime = data != null ? data.aoeDuration + (level - 1) * 1f : 4f;
        tickTimer = 0f;

        // lấy effect từ con
        effect = GetComponentInChildren<CircleAOEEffect>();
        if (effect != null) effect.MatchCollider(col);
    }

    void Update()
    {
        if (player == null) return;

        transform.position = player.position;

        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0f) Destroy(gameObject);

        tickTimer -= Time.deltaTime;
        if (tickTimer <= 0f)
        {
            DealDamage();
            tickTimer = data != null ? data.aoeTickInterval : 1f;
        }
    }

    void ApplyLevelStats(int level)
    {
        if (data == null) return;

        float desiredRadius = data.aoeRadius + (level - 1) * 0.5f;
        damage = data.damage + (level - 1) * 5;

        // set collider
        col.radius = desiredRadius;

        // sync effect
        if (effect != null) effect.MatchCollider(col);
    }

    void DealDamage()
    {
        for (int i = enemiesInside.Count - 1; i >= 0; i--)
        {
            var enemy = enemiesInside[i];
            if (enemy == null)
            {
                enemiesInside.RemoveAt(i);
                continue;
            }
            enemy.TakeDamage(damage);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        EnemyController enemy = other.GetComponent<EnemyController>();
        if (enemy != null && !enemiesInside.Contains(enemy))
        {
            enemiesInside.Add(enemy);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        EnemyController enemy = other.GetComponent<EnemyController>();
        if (enemy != null && enemiesInside.Contains(enemy))
        {
            enemiesInside.Remove(enemy);
        }
    }

    void OnDrawGizmos()
    {
        if (col == null) col = GetComponent<CircleCollider2D>();
        if (col == null) return;

        float worldRadius = col.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);

        Gizmos.color = new Color(1f, 0f, 0f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, worldRadius);
    }
}
