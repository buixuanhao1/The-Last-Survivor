using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public List<WeaponData> weapons = new List<WeaponData>();

    private Dictionary<WeaponData, float> cooldowns = new Dictionary<WeaponData, float>();
    private Dictionary<WeaponData, ObjectPool> pools = new Dictionary<WeaponData, ObjectPool>();

    [Header("Bindings")]
    [SerializeField] private PlayerWeaponSystem weaponSystem;
    [SerializeField] private WeaponData shurikenWeapon;   // Projectile (phi tiêu)
    [SerializeField] private WeaponData tarotWeapon;      // Projectile
    [SerializeField] private WeaponData stoneOrbitWeapon; // Orbit
    [SerializeField] private WeaponData swordWeapon;      // CircleAOE (tạm dùng AOE nếu chưa có Sword riêng)
    [SerializeField] private PlayerStats playerStats;

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

    public void ForceFire(WeaponData weapon)
    {
        if (weapon == null) return;
        if (!cooldowns.ContainsKey(weapon)) cooldowns[weapon] = 0f;

        if (weapon.weaponType == WeaponType.Projectile)
        {
            int count = GetCountForProjectile(weapon);
            if (count > 0)
            {
                GameObject target = FindClosestEnemy(weapon.attackRange);
                if (target != null)
                {
                    ShootProjectiles(weapon, target.transform.position, count);
                    float fr = playerStats != null ? playerStats.ComputeFireRate(weapon.fireRate) : weapon.fireRate;
                    cooldowns[weapon] = 1f / Mathf.Max(0.0001f, fr);
                }
                else
                {
                    // không có mục tiêu: để cooldown = 0 để bắn ngay khi có mục tiêu
                    cooldowns[weapon] = 0f;
                }
            }
        }
        else if (weapon.weaponType == WeaponType.Orbit)
        {
            int count = GetCountForOrbit(weapon);
            ShootOrbit(weapon, count);
            float cd = weapon.orbitCooldown;
            if (playerStats != null && playerStats.fireRatePercent != 0f)
            {
                cd = cd / (1f + Mathf.Max(0f, playerStats.fireRatePercent));
            }
            cooldowns[weapon] = weapon.orbitDuration + cd;
        }
        else if (weapon.weaponType == WeaponType.CircleAOE)
        {
            int count = GetCountForAOE(weapon);
            ShootAOE(weapon, count);
            float cd = weapon.orbitCooldown;
            if (playerStats != null && playerStats.fireRatePercent != 0f)
            {
                cd = cd / (1f + Mathf.Max(0f, playerStats.fireRatePercent));
            }
            cooldowns[weapon] = weapon.orbitDuration + cd;
        }
    }

    // Set cooldown to 0 so the weapon fires immediately on next Update
    public void ResetCooldown(WeaponId id)
    {
        WeaponData w = null;
        switch (id)
        {
            case WeaponId.Shuriken: w = shurikenWeapon; break;
            case WeaponId.Tarot: w = tarotWeapon; break;
            case WeaponId.StoneOrbit: w = stoneOrbitWeapon; break;
            case WeaponId.Sword: w = swordWeapon; break;
        }
        if (w == null) return;
        if (!cooldowns.ContainsKey(w)) cooldowns[w] = 0f;
        cooldowns[w] = 0f;
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
                    int count = GetCountForProjectile(weapon);
                    if (count > 0)
                    {
                        GameObject target = FindClosestEnemy(weapon.attackRange);
                        if (target != null)
                        {
                            ShootProjectiles(weapon, target.transform.position, count);
                            float fr = playerStats != null ? playerStats.ComputeFireRate(weapon.fireRate) : weapon.fireRate;
                            cooldowns[weapon] = 1f / Mathf.Max(0.0001f, fr);
                        }
                    }
                }
                else if (weapon.weaponType == WeaponType.Orbit)
                {
                    int count = GetCountForOrbit(weapon);
                    ShootOrbit(weapon, count);
                    float cd = weapon.orbitCooldown;
                    if (playerStats != null && playerStats.fireRatePercent != 0f)
                    {
                        // Giảm cooldown theo fireRatePercent (đơn giản: tăng tốc => giảm thời gian hồi)
                        cd = cd / (1f + Mathf.Max(0f, playerStats.fireRatePercent));
                    }
                    cooldowns[weapon] = weapon.orbitDuration + cd;
                }
                else if (weapon.weaponType == WeaponType.CircleAOE)
                {
                    int count = GetCountForAOE(weapon);
                    ShootAOE(weapon, count);
                    float cd = weapon.orbitCooldown;
                    if (playerStats != null && playerStats.fireRatePercent != 0f)
                    {
                        cd = cd / (1f + Mathf.Max(0f, playerStats.fireRatePercent));
                    }
                    cooldowns[weapon] = weapon.orbitDuration + cd;
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

    int GetCountForProjectile(WeaponData weapon)
    {
        if (weaponSystem == null) return 1;
        if (weapon == shurikenWeapon) return Mathf.Max(weaponSystem.shurikenCount, 0);
        if (weapon == tarotWeapon) return Mathf.Max(weaponSystem.tarotCount, 0);
        return 1;
    }

    int GetCountForOrbit(WeaponData weapon)
    {
        if (weaponSystem == null) return weapon.orbitCount;
        // Dùng runtime stoneCount cho tất cả vũ khí Orbit (giả định chỉ có StoneOrbit)
        return Mathf.Max(weaponSystem.stoneCount, 0);
    }

    int GetCountForAOE(WeaponData weapon)
    {
        if (weaponSystem == null) return 1;
        // Dùng runtime swordCount cho tất cả CircleAOE (giả định là Sword)
        return Mathf.Max(weaponSystem.swordCount, 0);
    }

    void ShootProjectiles(WeaponData weapon, Vector3 targetPos, int count)
    {
        Vector2 baseDir = (targetPos - transform.position).normalized;
        ObjectPool pool = pools[weapon];

        if (count <= 1)
        {
            GameObject bullet = pool.Get(transform.position, Quaternion.identity);
            Projectile proj = bullet.GetComponent<Projectile>();
            proj.Init(weapon, baseDir, pool);
            return;
        }

        // Spread projectiles evenly around base direction
        float totalSpread = Mathf.Min(60f, 10f * (count - 1));
        float step = count > 1 ? totalSpread / (count - 1) : 0f;
        float start = -totalSpread * 0.5f;

        float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;
        for (int i = 0; i < count; i++)
        {
            float angle = baseAngle + start + step * i;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
            GameObject bullet = pool.Get(transform.position, Quaternion.identity);
            Projectile proj = bullet.GetComponent<Projectile>();
            proj.Init(weapon, dir, pool);
        }
    }

    void ShootOrbit(WeaponData weapon, int count)
    {
        int n = Mathf.Max(0, count);
        if (n == 0) return;

        for (int i = 0; i < n; i++)
        {
            float angleStep = 360f / Mathf.Max(1, n);
            float startAngle = i * angleStep;

            GameObject stone = Instantiate(weapon.prefab, transform.position, Quaternion.identity);
            OrbitWeapon orbit = stone.GetComponent<OrbitWeapon>();
            orbit.Init(weapon, transform, startAngle);
        }
    }

    void ShootAOE(WeaponData weapon, int count)
    {
        int n = Mathf.Max(0, count);
        if (n == 0) return;

        for (int i = 0; i < n; i++)
        {
            GameObject aoe = Instantiate(weapon.prefab, transform.position, Quaternion.identity);
            CircleAOE circle = aoe.GetComponent<CircleAOE>();
            circle.Init(weapon, transform);
        }
    }
}
