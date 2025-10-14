using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public List<WeaponData> weapons = new List<WeaponData>();

    private Dictionary<WeaponData, float> cooldowns = new Dictionary<WeaponData, float>();
    private Dictionary<WeaponData, ObjectPool> pools = new Dictionary<WeaponData, ObjectPool>();

    void Start()
    {
        foreach (var weapon in weapons)
        {
            cooldowns[weapon] = 0f;

            if (weapon.weaponType == WeaponType.Projectile)
            {
                // tạo pool riêng cho projectile
                GameObject poolObj = new GameObject(weapon.weaponName + "_Pool");
                poolObj.transform.SetParent(transform);
                ObjectPool pool = poolObj.AddComponent<ObjectPool>();
                pool.prefab = weapon.prefab;
                pools[weapon] = pool;
            }
        }
    }

    public void RegisterWeapon(WeaponData weapon)
    {
        if (!weapons.Contains(weapon))
        {
            weapons.Add(weapon);
        }

        cooldowns[weapon] = 0f;

        if (weapon.weaponType == WeaponType.Projectile && !pools.ContainsKey(weapon))
        {
            GameObject poolObj = new GameObject(weapon.weaponName + "_Pool");
            poolObj.transform.SetParent(transform);
            ObjectPool pool = poolObj.AddComponent<ObjectPool>();
            pool.prefab = weapon.prefab;
            pools[weapon] = pool;
        }
    }

    void Update()
    {
        foreach (var weapon in weapons)
        {
            cooldowns[weapon] -= Time.deltaTime;

            if (cooldowns[weapon] <= 0f)
            {
                if (weapon.weaponType == WeaponType.Projectile)
                {
                    GameObject target = FindClosestEnemy(weapon.attackRange);
                    if (target != null)
                    {
                        Shoot(weapon, target.transform.position);
                        cooldowns[weapon] = 1f / weapon.fireRate;
                    }
                }
                else if (weapon.weaponType == WeaponType.Orbit)
                {
                    Shoot(weapon, Vector3.zero);
                    cooldowns[weapon] = weapon.orbitDuration + weapon.orbitCooldown;
                }
                else if (weapon.weaponType == WeaponType.CircleAOE)
                {
                    Shoot(weapon, Vector3.zero);
                    cooldowns[weapon] = weapon.orbitDuration + weapon.orbitCooldown;
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
        if (weapon.weaponType == WeaponType.Projectile)
        {
            Vector2 dir = (targetPos - transform.position).normalized;
            ObjectPool pool = pools[weapon];
            GameObject bullet = pool.Get(transform.position, Quaternion.identity);
            Projectile proj = bullet.GetComponent<Projectile>();
            proj.Init(weapon, dir, pool);
        }
        else if (weapon.weaponType == WeaponType.Orbit)
        {
            for (int i = 0; i < weapon.orbitCount; i++)
            {
                float angleStep = 360f / weapon.orbitCount;
                float startAngle = i * angleStep;

                GameObject stone = Instantiate(weapon.prefab, transform.position, Quaternion.identity);
                OrbitWeapon orbit = stone.GetComponent<OrbitWeapon>();
                orbit.Init(weapon, transform, startAngle);
            }
        }
        else if (weapon.weaponType == WeaponType.CircleAOE)
        {
            Debug.Log("Alooooo");
            GameObject aoe = Instantiate(weapon.prefab, transform.position, Quaternion.identity);
            CircleAOE circle = aoe.GetComponent<CircleAOE>();
            circle.Init(weapon, transform);
        }
    }
}
