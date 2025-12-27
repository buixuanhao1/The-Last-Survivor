using System.Collections.Generic;
using System.Collections;
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
    [SerializeField] private WeaponData knifeWeapon;      // Persistent bouncing knife
    [SerializeField] private WeaponData returningShurikenWeapon; // Returning projectile that flies out then comes back
    [SerializeField] private WeaponData bombWeapon;       // Bomb projectile that explodes with animation
    [SerializeField] private PlayerStats playerStats;

    // Track persistent instances for weapons like Knife
    private Dictionary<WeaponData, List<GameObject>> persistentInstances = new Dictionary<WeaponData, List<GameObject>>();

    // Rotating angle offset for returning shuriken volleys
    private float returningAngleOffset = 0f;

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

            if (!persistentInstances.ContainsKey(weapon))
                persistentInstances[weapon] = new List<GameObject>();
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
                if (weapon == returningShurikenWeapon)
                {
                    // Always fire in rotating pattern, independent of enemies
                    Vector2 dir = new Vector2(Mathf.Cos(returningAngleOffset * Mathf.Deg2Rad), Mathf.Sin(returningAngleOffset * Mathf.Deg2Rad));
                    Vector3 pseudoTarget = transform.position + (Vector3)dir;
                    ShootProjectiles(weapon, pseudoTarget, count);
                    returningAngleOffset = (returningAngleOffset + 20f) % 360f;
                    float fr = playerStats != null ? playerStats.ComputeFireRate(weapon.fireRate) : weapon.fireRate;
                    cooldowns[weapon] = 1f / Mathf.Max(0.0001f, fr);
                }
                else if (weapon == bombWeapon)
                {
                    // Fire bombs on cooldown in random directions
                    ShootBombs(weapon, count);
                    float fr = playerStats != null ? playerStats.ComputeFireRate(weapon.fireRate) : weapon.fireRate;
                    cooldowns[weapon] = 1f / Mathf.Max(0.0001f, fr);
                }
                else
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
            case WeaponId.Knife: w = knifeWeapon; break;
            case WeaponId.ReturningShuriken: w = returningShurikenWeapon; break;
            case WeaponId.Bomb: w = bombWeapon; break;
        }
        if (w == null) return;
        if (!weapons.Contains(w)) RegisterWeapon(w);
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

        if (!persistentInstances.ContainsKey(weapon))
            persistentInstances[weapon] = new List<GameObject>();
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
                    // Special handling for persistent Knife: maintain instance count, no cooldown firing
                    if (weapon == knifeWeapon)
                    {
                        MaintainKnifeInstances(weapon);
                        // Set a small cooldown to avoid reprocessing every frame
                        cooldowns[weapon] = 0.1f;
                        continue;
                    }

                    int count = GetCountForProjectile(weapon);
                    if (count > 0)
                    {
                        if (weapon == returningShurikenWeapon)
                        {
                            // Always fire in rotating pattern, independent of enemies
                            Vector2 dir = new Vector2(Mathf.Cos(returningAngleOffset * Mathf.Deg2Rad), Mathf.Sin(returningAngleOffset * Mathf.Deg2Rad));
                            Vector3 pseudoTarget = transform.position + (Vector3)dir;
                            ShootProjectiles(weapon, pseudoTarget, count);
                            returningAngleOffset = (returningAngleOffset + 20f) % 360f;
                            float fr = playerStats != null ? playerStats.ComputeFireRate(weapon.fireRate) : weapon.fireRate;
                            cooldowns[weapon] = 1f / Mathf.Max(0.0001f, fr);
                        }
                        else if (weapon == bombWeapon)
                        {
                            // Fire bombs on cooldown in random directions
                            ShootBombs(weapon, count);
                            float fr = playerStats != null ? playerStats.ComputeFireRate(weapon.fireRate) : weapon.fireRate;
                            cooldowns[weapon] = 1f / Mathf.Max(0.0001f, fr);
                        }
                        else
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
        if (weapon == knifeWeapon) return Mathf.Max(weaponSystem.knifeCount, 0);
        if (weapon == returningShurikenWeapon) return Mathf.Max(weaponSystem.returningShurikenCount, 0);
        if (weapon == bombWeapon) return Mathf.Max(weaponSystem.bombCount, 0);
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
            // returning projectile support
            if (weapon == returningShurikenWeapon)
            {
                ReturningProjectile rp = bullet.GetComponent<ReturningProjectile>();
                if (rp != null)
                {
                    float outDist = weapon.returningOutDistance + weapon.returningDistancePerLevel * Mathf.Max(0, count - 1);
                    rp.Init(weapon, baseDir, pool, transform, outDist);
                }
            }
            else
            {
                Projectile proj = bullet.GetComponent<Projectile>();
                if (proj != null)
                {
                    proj.Init(weapon, baseDir, pool);
                }
                else
                {
                    Knife knife = bullet.GetComponent<Knife>();
                    if (knife != null)
                        knife.Init(weapon, baseDir, pool);
                }
            }
            return;
        }

        // If this is the shuriken weapon, fire projectiles in a quick stagger (follow each other)
        if (weapon == shurikenWeapon)
        {
            StartCoroutine(SpawnShurikenBurst(pool, weapon, baseDir, count, 0.06f));
            return;
        }

        // If this is the returning shuriken, fire pairs in opposite directions
        if (weapon == returningShurikenWeapon)
        {
            SpawnReturningPattern(pool, weapon, baseDir, count);
            return;
        }

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
            if (proj != null)
            {
                proj.Init(weapon, dir, pool);
            }
            else
            {
                Knife knife = bullet.GetComponent<Knife>();
                if (knife != null)
                    knife.Init(weapon, dir, pool);
            }
        }
    }

    private IEnumerator SpawnShurikenBurst(ObjectPool pool, WeaponData weapon, Vector2 baseDir, int count, float delay)
    {
        int n = Mathf.Max(0, count);
        for (int i = 0; i < n; i++)
        {
            GameObject bullet = pool.Get(transform.position, Quaternion.identity);
            Projectile proj = bullet.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.Init(weapon, baseDir, pool);
            }
            else
            {
                Knife knife = bullet.GetComponent<Knife>();
                if (knife != null)
                    knife.Init(weapon, baseDir, pool);
            }

            if (i < n - 1 && delay > 0f)
                yield return new WaitForSeconds(delay);
        }
    }

    // Spawn totalCount returning shurikens evenly spaced around the circle.
    private void SpawnReturningPattern(ObjectPool pool, WeaponData weapon, Vector2 baseDir, int totalCount)
    {
        int n = Mathf.Max(1, totalCount);
        float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;
        float step = 360f / n;
        float outDist = weapon.returningOutDistance + weapon.returningDistancePerLevel * Mathf.Max(0, n - 1);
        for (int i = 0; i < n; i++)
        {
            float angle = baseAngle + step * i;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
            GameObject go = pool.Get(transform.position, Quaternion.identity);
            ReturningProjectile rp = go.GetComponent<ReturningProjectile>();
            if (rp != null)
            {
                rp.Init(weapon, dir, pool, transform, outDist);
            }
        }
    }

    // Duy trì số lượng dao Knife đang tồn tại theo knifeCount
    private void MaintainKnifeInstances(WeaponData weapon)
    {
        if (weapon == null) return;
        // nếu chưa có pool thì đăng ký để tạo pool
        if (!pools.ContainsKey(weapon))
        {
            RegisterWeapon(weapon);
            if (!pools.ContainsKey(weapon)) return;
        }

        ObjectPool pool = pools[weapon];
        int desired = GetCountForProjectile(weapon);

        if (!persistentInstances.TryGetValue(weapon, out var list))
        {
            list = new List<GameObject>();
            persistentInstances[weapon] = list;
        }

        // remove các object null (bị destroy ngoài ý muốn)
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (list[i] == null)
                list.RemoveAt(i);
        }

        // spawn cho đủ số lượng
        while (list.Count < desired)
        {
            GameObject target = FindClosestEnemy(weapon.attackRange);
            Vector2 dir;

            if (target != null)
            {
                dir = (target.transform.position - transform.position);
                // nếu quá gần hoặc trùng vị trí player -> tránh dir = (0,0)
                if (dir.sqrMagnitude < 0.01f)
                {
                    dir = Random.insideUnitCircle.normalized;
                }
                else
                {
                    dir = dir.normalized;
                }
            }
            else
            {
                // không có enemy nào -> bắn random
                dir = Random.insideUnitCircle.normalized;
            }

            GameObject bullet = pool.Get(transform.position, Quaternion.identity);

            Projectile proj = bullet.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.Init(weapon, dir, pool);
            }
            else
            {
                Knife knife = bullet.GetComponent<Knife>();
                if (knife != null)
                    knife.Init(weapon, dir, pool);
            }

            list.Add(bullet);
        }


        // nếu thừa thì trả bớt về pool
        while (list.Count > desired)
        {
            var go = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
            if (go != null)
            {
                pool.Return(go);
            }
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

    // Fire bombs in random directions regardless of enemies
    void ShootBombs(WeaponData weapon, int count)
    {
        if (!pools.TryGetValue(weapon, out var pool)) return;
        int n = Mathf.Max(0, count);
        if (n == 0) return;
        for (int i = 0; i < n; i++)
        {
            Vector2 dir = Random.insideUnitCircle.normalized;
            if (dir.sqrMagnitude < 0.001f) dir = Vector2.right;
            GameObject go = pool.Get(transform.position, Quaternion.identity);
            BombProjectile bomb = go.GetComponent<BombProjectile>();
            if (bomb != null)
            {
                bomb.Init(weapon, dir, pool);
            }
        }
    }

    
}
