using UnityEngine;

public class OrbitWeapon : MonoBehaviour
{
    private WeaponData data;
    private Transform player;
    private float lifeTime;
    private float angleOffset;
    private PlayerStats stats;

    public void Init(WeaponData weapon, Transform player, float startAngle)
    {
        this.data = weapon;
        this.player = player;
        this.lifeTime = weapon.orbitDuration;
        this.angleOffset = startAngle;
        if (stats == null) stats = PlayerStats.Instance != null ? PlayerStats.Instance : FindFirstObjectByType<PlayerStats>();
    }

    void Update()
    {
        if (player == null) return;

        // giảm thời gian tồn tại
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0f) Destroy(gameObject);

        // xoay quanh player
        float angle = (Time.time * data.speed * 50f) + angleOffset;
        float rad = angle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * data.orbitRadius;
        transform.position = player.position + offset;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        EnemyController enemy = other.GetComponent<EnemyController>();
        if (enemy != null)
        {
            int dmg = stats != null ? stats.ComputeDamage(data.damage) : data.damage;
            enemy.TakeDamage(dmg);
            enemy.Knockback(transform.position, 3f); // 3f = lực đẩy lùi, chỉnh tuỳ ý

        }
    }
}
