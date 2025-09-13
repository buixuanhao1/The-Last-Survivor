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
            cooldowns[weapon] = 0f; // khởi tạo cooldown cho mỗi vũ khí
            // tạo pool riêng cho từng weapon
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

        ObjectPool pool = pools[weapon];
        Debug.Log("Manager pool :" + pool.ToString());
        GameObject bullet = pool.Get(transform.position, Quaternion.identity);
        Projectile proj = bullet.GetComponent<Projectile>();
        proj.Init(weapon, dir, pool);
    }

 


}
