using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public List<WeaponData> weapons = new List<WeaponData>();
    private Dictionary<WeaponData, float> cooldowns = new Dictionary<WeaponData, float>();

    void Start()
    {
        foreach (var weapon in weapons)
        {
            cooldowns[weapon] = 0f; // khởi tạo cooldown cho mỗi vũ khí
        }
    }

    void Update()
    {
        foreach (var weapon in weapons)
        {
            cooldowns[weapon] -= Time.deltaTime;

            if (cooldowns[weapon] <= 0f)
            {
                GameObject target = FindClosestEnemy(weapon.attackRange);
                if (target != null)
                {
                    Shoot(weapon, target.transform.position);
                    cooldowns[weapon] = 1f / weapon.fireRate;
                }
            }
        }
    }

    GameObject FindClosestEnemy(float range)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range);

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

    void Shoot(WeaponData weapon, Vector3 targetPos)
    {
        Vector2 dir = (targetPos - transform.position).normalized;

        GameObject bullet = Instantiate(weapon.prefab, transform.position, Quaternion.identity);
        Projectile proj = bullet.GetComponent<Projectile>();
        proj.data = weapon;
        proj.SetDirection(dir);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        foreach (var weapon in weapons)
        {
            Gizmos.DrawWireSphere(transform.position, weapon.attackRange);
        }
    }
}
