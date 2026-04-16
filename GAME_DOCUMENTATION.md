# TÀI LIỆU HỆ THỐNG HÓA GAME - DoAnGame

## MỤC LỤC
1. [Tổng quan dự án](#1-tổng-quan-dự-án)
2. [Kiến trúc tổng thể](#2-kiến-trúc-tổng-thể)
3. [Hệ thống Player](#3-hệ-thống-player)
4. [Hệ thống Weapon](#4-hệ-thống-weapon)
5. [Hệ thống Skill](#5-hệ-thống-skill)
6. [Hệ thống Enemy](#6-hệ-thống-enemy)
7. [Hệ thống EXP & Level Up](#7-hệ-thống-exp--level-up)
8. [Hệ thống UI](#8-hệ-thống-ui)
9. [Luồng hoạt động game](#9-luồng-hoạt-động-game)
10. [Câu hỏi thường gặp](#10-câu-hỏi-thường-gặp)

---

## 1. TỔNG QUAN DỰ ÁN

### Loại game
- **Thể loại**: Survivor-like / Vampire Survivors-like
- **Gameplay**: Top-down, tự động bắn, thu thập EXP, chọn skill để upgrade
- **Mục tiêu**: Sống sót và tiêu diệt Boss để thắng

### Cấu trúc thư mục chính
```
Assets/
├── Scripts/          - 70+ file C# logic game
├── Prefab/          - Prefab của Player, Enemy, Weapon, UI
├── ScriptableObjects/ - Dữ liệu cấu hình (Weapon, Enemy, Skill, Map)
├── Scenes/          - Menu.unity và GamePlay.unity
└── Sprites/         - Hình ảnh và animation
```

### Công nghệ sử dụng
- **Engine**: Unity 2D
- **Ngôn ngữ**: C#
- **Design Patterns**: Singleton, Object Pool, ScriptableObject, Strategy Pattern
- **Backend**: Firebase (Authentication, Database)

---

## 2. KIẾN TRÚC TỔNG THỂ

### Các hệ thống chính
```
GAME
├── Player System
│   ├── Di chuyển (Player.cs)
│   ├── Máu (PlayerHealth.cs)
│   ├── EXP & Level (PlayerExperience.cs)
│   ├── Stats (ThongSoNguoiChoi.cs)
│   └── Weapon Count (PlayerWeaponSystem.cs)
│
├── Weapon System
│   ├── Quản lý bắn (WeaponManager.cs)
│   ├── Pool đạn (ObjectPool.cs)
│   └── Các loại vũ khí (Projectile, Knife, Orbit, AoE, Bomb)
│
├── Skill System
│   ├── Quản lý skill (SkillManager.cs)
│   ├── Effect (StatModifier, WeaponCount)
│   └── UI chọn skill (LevelUpPanel.cs)
│
├── Enemy System
│   ├── Spawn (EnemySpawner.cs)
│   ├── AI (EnemyController.cs)
│   └── Data (EnemyData.cs)
│
├── EXP System
│   ├── Orb (ExpOrb.cs)
│   └── Pool (ExpOrbPool.cs)
│
└── UI System
    └── Manager (UIManager.cs)
```

### Design Patterns quan trọng

#### Singleton Pattern
```csharp
// Các class sử dụng Singleton
- UIManager
- PlayerStats (ThongSoNguoiChoi)
- SkillManager
- ExpOrbPool

// Lý do: Cần truy cập global từ nhiều nơi
```

#### Object Pool Pattern
```csharp
// ObjectPool.cs - Pool cho Projectile
- Tránh Instantiate/Destroy liên tục
- Tái sử dụng objects → tối ưu hiệu năng

// ExpOrbPool.cs - Pool cho ExpOrb
- Quản lý hàng trăm orb cùng lúc
```

#### ScriptableObject Pattern
```csharp
// Tách data khỏi logic
- WeaponData.cs → Cấu hình vũ khí
- EnemyData.cs → Cấu hình enemy
- SkillData.cs → Cấu hình skill
- MapConfig.cs → Cấu hình map

// Lợi ích: Dễ balance, không cần code
```

---

## 3. HỆ THỐNG PLAYER

### 3.1. Di chuyển (Player.cs)
**Vị trí**: `Assets/Scripts/Player/Player.cs`

**Chức năng chính**:
- Nhận input từ Joystick hoặc bàn phím
- Di chuyển player với Rigidbody2D
- Flip sprite theo hướng di chuyển

**Code flow**:
```csharp
void Update()
{
    MovePlayer();
}

void MovePlayer()
{
    1. Đọc input (Joystick hoặc WASD)
    2. Tính finalSpeed = baseSpeed * (1 + PlayerStats.moveSpeedPercent)
    3. rb.linearVelocity = direction * finalSpeed
    4. FlipSprite() nếu đổi hướng
}
```

**Câu hỏi vấn đáp**:
- **Q**: Tại sao dùng `linearVelocity` thay vì `transform.position`?
- **A**: Để tận dụng physics engine của Unity, có collision và smooth movement tự nhiên

---

### 3.2. Máu (PlayerHealth.cs)
**Vị trí**: `Assets/Scripts/Player/PlayerHealth.cs`

**Chức năng**:
- Quản lý HP (currentHP / maxHP)
- Invincibility frame 0.2s sau khi bị đánh
- Event system cho UI update
- Die animation

**Code flow**:
```csharp
TakeDamage(amount)
├── if invincible → return
├── currentHP -= amount
├── onHealthChanged?.Invoke(current, max)
├── if currentHP <= 0 → Die()
└── StartCoroutine(InvincibilityFrames())

Die()
├── animator.SetTrigger("Die")
├── onDeath?.Invoke()
└── Disable player
```

**Delegates sử dụng**:
```csharp
public Action<float, float> onHealthChanged; // (current, max)
public Action onDeath;
```

**Câu hỏi vấn đáp**:
- **Q**: Tại sao cần invincibility frame?
- **A**: Tránh player chết ngay lập tức khi bị nhiều enemy đánh cùng lúc

---

### 3.3. Stats (ThongSoNguoiChoi.cs)
**Vị trí**: `Assets/Scripts/Player/ThongSoNguoiChoi.cs`

**Chức năng**: Singleton lưu và tính toán tất cả stats của player

**Stats quan trọng**:
```csharp
// Stat modifiers (từ skill)
public float moveSpeedPercent = 0f;      // +20% = 0.2f
public float damagePercent = 0f;         // +50% = 0.5f
public float damageFlat = 0f;            // +10 damage
public float fireRatePercent = 0f;       // +30% = 0.3f
public float pickupRangeAdd = 0f;        // +2 units
public float healthRegenPerSec = 0f;     // +5 HP/s

// Hàm tính toán
ComputeDamage(float baseDmg)
{
    return (baseDmg + damageFlat) * (1 + damagePercent);
}

ComputeFireRate(float baseRate)
{
    return baseRate * (1 + fireRatePercent);
}
```

**Câu hỏi vấn đáp**:
- **Q**: Tại sao tách thành `damagePercent` và `damageFlat`?
- **A**: Cho phép balance linh hoạt - flat tốt early game, percent scale tốt late game

---

### 3.4. EXP & Level (PlayerExperience.cs)
**Vị trí**: `Assets/Scripts/Player/PlayerExperience.cs`

**Code flow**:
```csharp
AddExp(int amount)
├── currentExp += amount
├── UpdateUI()
└── while (currentExp >= requiredExp)
    ├── currentExp -= requiredExp
    ├── LevelUp()
    └── requiredExp *= expMultiplier (1.2f)

LevelUp()
├── currentLevel++
├── Time.timeScale = 0 (pause game)
└── LevelUpPanel.Show()
```

**Thông số**:
- Base required EXP: 100
- EXP multiplier mỗi level: 1.2x
- VD: Level 1→2: 100 EXP, Level 2→3: 120 EXP, Level 3→4: 144 EXP

---

### 3.5. Weapon Count (PlayerWeaponSystem.cs)
**Vị trí**: `Assets/Scripts/Player/PlayerWeaponSystem.cs`

**Chức năng**: Runtime storage cho số lượng từng loại vũ khí

**Variables**:
```csharp
public int shurikenCount = 1;        // Default 1
public int tarotCount = 0;
public int knifeCount = 0;
public int stoneCount = 0;
public int swordCount = 0;
public int returningShurikenCount = 0;
public int bombCount = 0;
```

**Sử dụng**:
```csharp
// WeaponManager đọc count để bắn đúng số lượng
int count = playerWeaponSystem.GetCountForProjectile(weaponType);
ShootProjectiles(weapon, targetPos, count);
```

---

## 4. HỆ THỐNG WEAPON

### 4.1. WeaponManager.cs
**Vị trí**: `Assets/Scripts/Weapon/WeaponManager.cs`

**Vai trò**: Quản lý và bắn TẤT CẢ vũ khí trong game

**Core Logic**:
```csharp
Update()
{
    foreach (weapon in registeredWeapons)
    {
        cooldown[weapon] -= deltaTime;

        if (cooldown[weapon] <= 0)
        {
            ForceFire(weapon);
            cooldown[weapon] = 1f / ComputedFireRate(weapon);
        }
    }
}

ForceFire(weapon)
{
    1. FindClosestEnemy(attackRange)
    2. GetCountForProjectile(weapon)
    3. ShootProjectiles(weapon, target, count)
}

ShootProjectiles(weapon, target, count)
{
    for (int i = 0; i < count; i++)
    {
        angle = startAngle + i * angleSpacing;
        direction = Quaternion.Euler(0, 0, angle) * baseDirection;

        obj = pool.Get();
        obj.transform.position = player.position;
        obj.Initialize(direction, weapon);
    }
}
```

**Câu hỏi vấn đáp**:
- **Q**: Tại sao dùng cooldown-based thay vì coroutine?
- **A**: Dễ quản lý, dễ pause/resume, dễ modify fireRate realtime

---

### 4.2. WeaponData.cs (ScriptableObject)
**Vị trí**: `Assets/Scripts/Weapon/WeaponData.cs`

**Chứa dữ liệu**:
```csharp
[CreateAssetMenu(fileName = "NewWeapon", menuName = "Game/Weapon")]
public class WeaponData : ScriptableObject
{
    public string weaponId;          // "Shuriken", "Knife", etc.
    public string weaponName;        // "Phi tiêu"
    public Sprite icon;

    public float baseDamage;         // 10
    public float baseFireRate;       // 1.5 shots/s
    public float projectileSpeed;    // 15 units/s
    public float maxDistance;        // 20 units
    public float attackRange;        // 15 units (tìm enemy)

    public GameObject projectilePrefab;
}
```

**Ví dụ cụ thể** (Shuriken):
```
weaponId: "Shuriken"
baseDamage: 10
baseFireRate: 1.5
projectileSpeed: 15
maxDistance: 20
attackRange: 15
```

---

### 4.3. Các loại vũ khí

#### A. Projectile (Đạn bay)
**File**: `Assets/Scripts/Weapon/Projectile.cs`

**Loại vũ khí**: Shuriken, Tarot

**Logic**:
```csharp
Initialize(direction, weaponData)
{
    this.direction = direction.normalized;
    this.speed = weaponData.projectileSpeed;
    this.damage = weaponData.baseDamage;
    this.maxDistance = weaponData.maxDistance;
    distanceTraveled = 0;
}

Update()
{
    // Bay theo hướng
    transform.position += direction * speed * deltaTime;
    distanceTraveled += speed * deltaTime;

    // Xoay sprite
    transform.Rotate(0, 0, rotateSpeed * deltaTime);

    // Return pool khi hết tầm
    if (distanceTraveled >= maxDistance)
        ReturnToPool();
}

OnTriggerEnter2D(enemy)
{
    enemy.TakeDamage(ComputedDamage);
    ReturnToPool();
}
```

---

#### B. Knife (Dao bouncing)
**File**: `Assets/Scripts/Weapon/Knife.cs`

**Đặc điểm**: Persistent, bounce nhiều enemy

**Logic phức tạp**:
```csharp
// Duy trì số lượng dao đang bay
activeKnives[knifeInstance] = true;
if (activeKnives.Count > maxKnives)
    DestroyOldest();

OnTriggerEnter2D(enemy)
{
    // Per-enemy cooldown
    if (hitCooldowns[enemy] > 0)
        return;

    enemy.TakeDamage(damage);
    hitCooldowns[enemy] = 0.5f; // 0.5s per enemy

    // Bounce
    BounceToNextEnemy();
}

Update()
{
    // Giảm cooldown cho tất cả enemies
    foreach (enemy in hitCooldowns.Keys)
        hitCooldowns[enemy] -= deltaTime;
}
```

**Câu hỏi vấn đáp**:
- **Q**: Tại sao cần per-enemy cooldown?
- **A**: Tránh dao đánh 1 enemy liên tục, muốn nó bounce sang enemy khác

---

#### C. OrbitWeapon (Vũ khí quay quanh)
**File**: `Assets/Scripts/Weapon/OrbitWeapon.cs`

**Loại vũ khí**: Stone

**Logic**:
```csharp
Update()
{
    // Quay quanh player
    currentAngle += orbitSpeed * deltaTime;

    offset = new Vector2(
        Mathf.Cos(currentAngle) * orbitRadius,
        Mathf.Sin(currentAngle) * orbitRadius
    );

    transform.position = owner.position + offset;
}

OnTriggerEnter2D(enemy)
{
    // Gây damage
    enemy.TakeDamage(damage);

    // Knockback
    direction = (enemy.position - owner.position).normalized;
    enemy.ApplyKnockback(direction * knockbackForce);
}
```

**Thông số**:
- orbitRadius: 2.5 units
- orbitSpeed: 180 degrees/s
- knockbackForce: 3f

---

#### D. CircleAOE (Vùng damage)
**File**: `Assets/Scripts/Weapon/CircleAOE.cs`

**Loại vũ khí**: Sword

**Logic**:
```csharp
Start()
{
    StartCoroutine(DamageOverTime());
}

IEnumerator DamageOverTime()
{
    while (true)
    {
        // Overlap circle để tìm enemies
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            radius,
            enemyLayer
        );

        foreach (hit in hits)
        {
            enemy.TakeDamage(damagePerTick);
        }

        yield return new WaitForSeconds(tickRate); // 0.2s
    }
}

Update()
{
    // Follow player
    transform.position = owner.position;
}
```

---

#### E. BombProjectile (Bom với cung bay)
**File**: `Assets/Scripts/Weapon/BombProjectile.cs`

**Logic đẹp**:
```csharp
Initialize(direction, weaponData)
{
    startPos = transform.position;
    targetPos = startPos + direction * maxDistance;

    // Tính arc (cung bay)
    arcHeight = 3f;
    travelProgress = 0f;
}

Update()
{
    travelProgress += (speed / maxDistance) * deltaTime;

    // Lerp vị trí
    currentPos = Vector2.Lerp(startPos, targetPos, travelProgress);

    // Cộng arc (parabol)
    yOffset = arcHeight * Mathf.Sin(travelProgress * Mathf.PI);

    transform.position = currentPos + Vector2.up * yOffset;

    if (travelProgress >= 1f)
        Explode();
}

Explode()
{
    // Spawn Explosion prefab
    explosion = Instantiate(explosionPrefab, position);
    explosion.Initialize(explosionRadius, explosionDamage);

    Destroy(gameObject);
}
```

---

#### F. Explosion (Hiệu ứng nổ)
**File**: `Assets/Scripts/Weapon/Explosion.cs`

**Logic**:
```csharp
Initialize(radius, damage)
{
    // Overlap circle
    Collider2D[] hits = Physics2D.OverlapCircleAll(
        transform.position,
        radius,
        enemyLayer
    );

    foreach (hit in hits)
    {
        enemy.TakeDamage(damage);
    }

    // Animation rồi destroy
    Destroy(gameObject, animationDuration);
}
```

---

### 4.4. ObjectPool
**File**: `Assets/Scripts/Weapon/ObjectPool.cs`

**Chức năng**: Tái sử dụng Projectile để tối ưu

**Code**:
```csharp
public class ObjectPool
{
    Queue<GameObject> pool = new Queue<GameObject>();
    GameObject prefab;
    Transform parent;

    GameObject Get()
    {
        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            return Instantiate(prefab, parent);
        }
    }

    void Return(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
```

**Lợi ích**:
- Không Instantiate/Destroy mỗi shot → giảm GC
- Hiệu năng tốt khi bắn hàng trăm projectile

---

## 5. HỆ THỐNG SKILL

### 5.1. SkillData.cs (ScriptableObject)
**Vị trí**: `Assets/Scripts/Data/SkillData.cs`

**Giải thích file này** (vì user đang mở):
```csharp
[CreateAssetMenu(fileName = "NewSkill", menuName = "Game/Skill")]
public class SkillData : ScriptableObject
{
    [Header("Thông tin Skill")]
    public string skillId;      // ID duy nhất: "KnifeCount", "DamageUp"
    public string skillName;    // Tên hiển thị: "Dao phóng"
    public Sprite icon;         // Icon trong LevelUpPanel
    [TextArea] public string description; // Mô tả cho player

    public enum SkillType { Weapon, Stat, Utility }
    public SkillType type;      // Phân loại skill

    [Header("Cấp tối đa")]
    public int maxLevel = 5;    // Skill có thể lên tối đa cấp bao nhiêu

    [Header("Effects (SO)")]
    public List<SkillEffect> effects; // Danh sách effect khi chọn skill
}
```

**Ví dụ cụ thể**:

**Skill: Knife (Weapon)**
```
skillId: "KnifeCount"
skillName: "Dao phóng"
type: Weapon
maxLevel: 5
effects:
  - WeaponCountEffect (weaponType: Knife, countDelta: 1)
```

**Skill: Damage Up (Stat)**
```
skillId: "DamageUp"
skillName: "Sức mạnh"
type: Stat
maxLevel: 5
effects:
  - StatModifierEffect (statType: DamagePercent, valueDelta: 0.1)
```

**Câu hỏi vấn đáp**:
- **Q**: Tại sao effects là List thay vì 1 effect?
- **A**: Một skill có thể có nhiều effect cùng lúc (VD: Tăng damage + tăng fireRate)

---

### 5.2. SkillEffect.cs (Abstract Base)
**Vị trí**: `Assets/Scripts/Skills/SkillEffect.cs`

**Pattern**: Strategy Pattern

**Code**:
```csharp
public abstract class SkillEffect : ScriptableObject
{
    // Abstract method - subclass implement
    public abstract void Apply(PlayerContext context, int level);
}

// PlayerContext chứa references
public struct PlayerContext
{
    public PlayerWeaponSystem weaponSystem;
    public WeaponManager weaponManager;
    public PlayerStats stats;
    public PlayerHealth health;
}
```

---

### 5.3. StatModifierEffect
**Vị trí**: `Assets/Scripts/Skills/StatModifierEffect.cs`

**Chức năng**: Tăng stats (damage%, speed%, fireRate%)

**Code**:
```csharp
[CreateAssetMenu(fileName = "StatModifier", menuName = "Game/Effects/StatModifier")]
public class StatModifierEffect : SkillEffect
{
    public enum StatType
    {
        DamagePercent,      // Tăng % damage
        DamageFlat,         // Tăng flat damage
        MoveSpeed,          // Tăng % speed
        FireRate,           // Tăng % fireRate
        PickupRange,        // Tăng range hút EXP
        HealthRegen         // Hồi máu/giây
    }

    public StatType statType;
    public float valueDelta;    // Giá trị tăng mỗi level

    public override void Apply(PlayerContext context, int level)
    {
        switch (statType)
        {
            case StatType.DamagePercent:
                context.stats.damagePercent += valueDelta;
                break;
            case StatType.MoveSpeed:
                context.stats.moveSpeedPercent += valueDelta;
                break;
            // ... etc
        }
    }
}
```

**Ví dụ**:
- Skill "Damage Up" level 1: valueDelta = 0.1 → +10% damage
- Skill "Damage Up" level 2: valueDelta = 0.1 → +10% nữa → tổng +20%

---

### 5.4. WeaponCountEffect
**Vị trí**: `Assets/Scripts/Skills/WeaponCountEffect.cs`

**Chức năng**: Tăng số lượng vũ khí

**Code**:
```csharp
[CreateAssetMenu(fileName = "WeaponCount", menuName = "Game/Effects/WeaponCount")]
public class WeaponCountEffect : SkillEffect
{
    public enum WeaponType
    {
        Shuriken,
        Tarot,
        Knife,
        Stone,
        Sword,
        ReturningShuriken,
        Bomb
    }

    public WeaponType weaponType;
    public int countDelta;      // Tăng bao nhiêu (thường = 1)

    public override void Apply(PlayerContext context, int level)
    {
        switch (weaponType)
        {
            case WeaponType.Shuriken:
                context.weaponSystem.shurikenCount += countDelta;
                break;
            case WeaponType.Knife:
                context.weaponSystem.knifeCount += countDelta;
                break;
            // ... etc
        }

        // Reset cooldown để bắn ngay
        context.weaponManager.ResetCooldown(weaponType);
    }
}
```

---

### 5.5. SkillManager
**Vị trí**: `Assets/Scripts/Player/SkillManager.cs`

**Vai trò**: Lưu trữ level của từng skill

**Code**:
```csharp
public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance;

    // Key: skillId, Value: current level
    Dictionary<string, int> skillLevels = new Dictionary<string, int>();

    SkillEffectApplier effectApplier;

    public void AddSkill(SkillData skill)
    {
        // Tăng level
        if (!skillLevels.ContainsKey(skill.skillId))
            skillLevels[skill.skillId] = 0;

        skillLevels[skill.skillId]++;
        int currentLevel = skillLevels[skill.skillId];

        // Apply effect
        effectApplier.ApplySkill(skill, currentLevel);
    }

    public int GetSkillLevel(string skillId)
    {
        return skillLevels.ContainsKey(skillId) ? skillLevels[skillId] : 0;
    }
}
```

---

### 5.6. Luồng hoàn chỉnh khi chọn skill

```
1. PlayerExperience.LevelUp()
   └── LevelUpPanel.Show()

2. LevelUpPanel
   ├── Random 3 skills từ allSkills[]
   ├── Filter: skill.level < skill.maxLevel
   └── Display SkillOptionUI (icon, name, level)

3. Player click chọn skill
   └── LevelUpPanel.SelectSkill(skillData)

4. SkillManager.AddSkill(skillData)
   ├── skillLevels[skillId]++
   └── effectApplier.ApplySkill(skill, level)

5. SkillEffectApplier.ApplySkill(skill, level)
   ├── Build PlayerContext
   └── foreach effect in skill.effects
       └── effect.Apply(context, level)

6. Effect.Apply()
   ├── StatModifierEffect → stats.damagePercent += 0.1
   └── WeaponCountEffect → weaponSystem.knifeCount++
                       └── weaponManager.ResetCooldown()

7. Resume game
   └── Time.timeScale = 1
```

---

## 6. HỆ THỐNG ENEMY

### 6.1. EnemySpawner.cs
**Vị trí**: `Assets/Scripts/Enemy/EnemySpawner.cs`

**Vai trò**: Spawn enemy theo phase, burst wave và boss

**Code structure**:
```csharp
[System.Serializable]
public class SpawnPhase
{
    public float startTime;         // 0s, 20s, 60s
    public List<EnemyType> types;   // Loại enemy spawn ở phase này
}

[System.Serializable]
public class BurstWave
{
    public float time;              // 30s
    public EnemyType type;          // Boss, hoặc loại đặc biệt
    public int count;               // 5 enemies
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Phase Spawning")]
    public List<SpawnPhase> phases;
    public float spawnInterval = 2f;

    [Header("Burst Waves")]
    public List<BurstWave> bursts;

    [Header("Boss")]
    public float bossSpawnTime = 170f;
    public EnemyType bossType;
    public bool stopSpawningAfterBoss = true;
}
```

**Logic spawn**:
```csharp
Update()
{
    elapsedTime += deltaTime;

    // 1. Update phase
    UpdatePhase();

    // 2. Continuous spawn
    spawnTimer += deltaTime;
    if (spawnTimer >= spawnInterval)
    {
        SpawnRandom();
        spawnTimer = 0;
    }

    // 3. Check burst
    foreach (burst in bursts)
    {
        if (elapsedTime >= burst.time && !burst.spawned)
        {
            SpawnBurst(burst);
            burst.spawned = true;
        }
    }

    // 4. Boss
    if (elapsedTime >= bossSpawnTime && !bossSpawned)
    {
        SpawnBoss();
        bossSpawned = true;
    }

    // 5. Win condition
    if (bossSpawned && bossInstance == null)
    {
        ShowWinUI();
    }
}

void SpawnRandom()
{
    EnemyType type = activeTypes[Random.Range(0, activeTypes.Count)];
    Vector2 spawnPos = GetRandomSpawnPosition();
    Instantiate(type.prefab, spawnPos, Quaternion.identity);
}

Vector2 GetRandomSpawnPosition()
{
    // Random ở ngoài camera view
    angle = Random.Range(0f, 360f);
    distance = spawnDistance (12 units);
    return playerPos + (Vector2)(Quaternion.Euler(0, 0, angle) * Vector2.right * distance);
}
```

**Ví dụ phase config**:
```
Phase 0: startTime = 0s
  types: EnemyOne, EnemyTwo

Phase 1: startTime = 20s
  types: EnemyOne, EnemyTwo, EnemyThree, Mage

Phase 2: startTime = 60s
  types: All enemies

Boss: spawnTime = 170s
  type: Boss
```

---

### 6.2. EnemyController.cs
**Vị trí**: `Assets/Scripts/Enemy/EnemyController.cs`

**Chức năng**: AI di chuyển, nhận damage, die

**Code flow**:
```csharp
Start()
{
    // Load data từ EnemyData
    maxHealth = enemyData.baseHealth;
    currentHealth = maxHealth;
    moveSpeed = enemyData.moveSpeed;
    damage = enemyData.contactDamage;
    expValue = enemyData.expDrop;
}

Update()
{
    MoveTowardsPlayer();
}

void MoveTowardsPlayer()
{
    direction = (playerPos - transform.position).normalized;
    rb.linearVelocity = direction * moveSpeed;

    // Flip sprite
    if (direction.x < 0)
        spriteRenderer.flipX = true;
}

public void TakeDamage(float amount)
{
    currentHealth -= amount;

    // Show damage text
    DamageTextPool.Show(amount, transform.position);

    // Flash effect
    StartCoroutine(FlashRed());

    if (currentHealth <= 0)
        Die();
}

void Die()
{
    // Animation
    animator.SetTrigger("Die");

    // Drop EXP
    ExpOrbPool.Instance.SpawnOrb(transform.position, expValue);

    // Destroy sau animation
    Destroy(gameObject, 0.5f);
}

void OnTriggerEnter2D(player)
{
    // Gây damage cho player
    player.GetComponent<PlayerHealth>().TakeDamage(damage);
}
```

---

### 6.3. EnemyData.cs (ScriptableObject)
**Vị trí**: `Assets/Scripts/Enemy/EnemyData.cs`

**Dữ liệu**:
```csharp
[CreateAssetMenu(fileName = "NewEnemy", menuName = "Game/Enemy")]
public class EnemyData : ScriptableObject
{
    public string enemyId;           // "EnemyOne"
    public string enemyName;         // "Zombie"

    public float baseHealth;         // 50
    public float moveSpeed;          // 3
    public float contactDamage;      // 10
    public int expDrop;              // 5

    public Sprite sprite;
    public RuntimeAnimatorController animator;
}
```

**Ví dụ**:
```
EnemyOne:
  baseHealth: 50
  moveSpeed: 3
  contactDamage: 10
  expDrop: 5

Boss:
  baseHealth: 5000
  moveSpeed: 2
  contactDamage: 50
  expDrop: 1000
```

---

## 7. HỆ THỐNG EXP & LEVEL UP

### 7.1. ExpOrb.cs
**Vị trí**: `Assets/Scripts/Exp/ExpOrb.cs`

**Mechanic phức tạp**:
```csharp
Start()
{
    // Phase 1: Timeout (0.5s)
    yield return new WaitForSeconds(attractDelay);
    canAttract = true;
}

Update()
{
    if (!canAttract)
        return;

    // Tính khoảng cách đến player
    distance = Vector2.Distance(transform.position, playerPos);

    attractRange = 0.5f + PlayerStats.Instance.pickupRangeAdd;

    if (distance <= attractRange)
    {
        // Phase 2: Offset (lùi ra)
        if (!offsetApplied)
        {
            direction = (transform.position - playerPos).normalized;
            rb.AddForce(direction * offsetForce, ForceMode2D.Impulse);
            offsetApplied = true;

            yield return new WaitForSeconds(offsetDuration);
        }

        // Phase 3: Attract (hút về)
        direction = (playerPos - transform.position).normalized;

        currentSpeed = Mathf.Max(
            minAttractSpeed,
            distance / attractDuration
        );

        rb.linearVelocity = direction * currentSpeed;
    }
}

OnTriggerEnter2D(player)
{
    player.GetComponent<PlayerExperience>().AddExp(expValue);
    ExpOrbPool.Instance.ReturnOrb(gameObject);
}
```

**Thông số**:
- attractDelay: 0.5s
- offsetForce: 5f
- offsetDuration: 0.25s
- minAttractSpeed: 15f
- attractDuration: 0.3s

**Câu hỏi vấn đáp**:
- **Q**: Tại sao có phase Offset (lùi ra)?
- **A**: Tạo hiệu ứng đẹp, EXP orb bật ra rồi mới hút về, giống game Vampire Survivors

---

### 7.2. ExpOrbPool.cs
**Vị trí**: `Assets/Scripts/Exp/ExpOrbPool.cs`

**Singleton Pool**:
```csharp
public class ExpOrbPool : MonoBehaviour
{
    public static ExpOrbPool Instance;

    Queue<GameObject> pool = new Queue<GameObject>();
    public GameObject orbPrefab;

    public void SpawnOrb(Vector2 position, int expValue)
    {
        GameObject orb = GetOrb();
        orb.transform.position = position;
        orb.GetComponent<ExpOrb>().Initialize(expValue);
    }

    GameObject GetOrb()
    {
        if (pool.Count > 0)
        {
            GameObject orb = pool.Dequeue();
            orb.SetActive(true);
            return orb;
        }
        else
        {
            return Instantiate(orbPrefab);
        }
    }

    public void ReturnOrb(GameObject orb)
    {
        orb.SetActive(false);
        pool.Enqueue(orb);
    }
}
```

---

## 8. HỆ THỐNG UI

### 8.1. UIManager.cs
**Vị trí**: `Assets/Scripts/Ui/UIManager.cs`

**Singleton quản lý UI**:
```csharp
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    GameObject currentPanel;        // Chỉ 1 panel active cùng lúc
    Stack<GameObject> overlays;     // Stack overlays

    public void ShowPanel(GameObject panel)
    {
        // Hide current
        if (currentPanel != null)
            currentPanel.SetActive(false);

        // Show new
        currentPanel = panel;
        currentPanel.SetActive(true);
    }

    public void OpenOverlay(GameObject overlay)
    {
        overlays.Push(overlay);
        overlay.SetActive(true);
    }

    public void CloseTopOverlay()
    {
        if (overlays.Count > 0)
        {
            GameObject top = overlays.Pop();
            top.SetActive(false);
        }
    }
}
```

**UI Hierarchy**:
```
UIManager
├── Panels (exclusive - chỉ 1 active)
│   ├── LoginPanel
│   ├── MainUI (menu chính)
│   ├── ShopPanel
│   └── HeroPanel
│
└── Overlays (stack - có thể nhiều)
    ├── SettingsPanel
    ├── RankPanel
    └── PauseMenu
```

---

### 8.2. LevelUpPanel.cs
**Vị trí**: `Assets/Scripts/Ui/LevelUpPanel.cs`

**Core logic**:
```csharp
public void Show()
{
    // Pause game
    Time.timeScale = 0;

    // Random 3 skills
    List<SkillData> available = GetAvailableSkills();
    List<SkillData> choices = RandomPick(available, 3);

    // Display
    for (int i = 0; i < 3; i++)
    {
        skillOptions[i].SetSkill(choices[i]);
    }

    gameObject.SetActive(true);
}

List<SkillData> GetAvailableSkills()
{
    List<SkillData> result = new List<SkillData>();

    foreach (skill in allSkills)
    {
        int currentLevel = SkillManager.Instance.GetSkillLevel(skill.skillId);

        // Chỉ add skill chưa max level
        if (currentLevel < skill.maxLevel)
            result.Add(skill);
    }

    return result;
}

public void SelectSkill(SkillData skill)
{
    // Add skill
    SkillManager.Instance.AddSkill(skill);

    // Resume game
    Time.timeScale = 1;
    gameObject.SetActive(false);
}
```

---

### 8.3. SkillOptionUI.cs
**Vị trí**: `Assets/Scripts/Ui/SkillOptionUI.cs`

**Display 1 skill option**:
```csharp
public void SetSkill(SkillData skill)
{
    this.skillData = skill;

    // Update UI
    iconImage.sprite = skill.icon;
    nameText.text = skill.skillName;

    int currentLevel = SkillManager.Instance.GetSkillLevel(skill.skillId);
    levelText.text = $"Lv {currentLevel + 1}";

    // Frame color theo type
    if (skill.type == SkillType.Weapon)
        frameImage.color = Color.red;
    else if (skill.type == SkillType.Stat)
        frameImage.color = Color.blue;
}

public void OnClick()
{
    LevelUpPanel.Instance.SelectSkill(skillData);
}
```

---

## 9. LUỒNG HOẠT ĐỘNG GAME

### 9.1. Khởi động game (Menu → Gameplay)

```
1. Menu Scene Load
   └── UIManager.Awake() - Singleton init

2. UIMainMenu.Start()
   ├── LoadSavedMap()
   ├── CheckFirebaseLogin()
   └── Show MainUI hoặc LoginPanel

3. Player chọn Map & Hero
   ├── MapConfig saved
   └── HeroData saved

4. Click "Play"
   └── SceneManager.LoadScene("GamePlay")

5. GamePlay Scene Load
   ├── HeroInitializer.Start()
   │   └── ApplyHeroConfig (sprite, animator, HP)
   │
   ├── WeaponManager.Start()
   │   ├── RegisterWeapon(Shuriken)
   │   └── CreateObjectPools()
   │
   ├── EnemySpawner.Start()
   │   └── timer = 0
   │
   ├── PlayerStats.Awake()
   │   └── Reset all stats
   │
   └── PlayerExperience.Start()
       └── level = 1, exp = 0

6. Game Loop Start
   └── (Xem section 9.2)
```

---

### 9.2. Game Loop (mỗi frame)

```
EVERY FRAME:

├── Player.Update()
│   └── MovePlayer()
│       ├── Read input (Joystick/Keyboard)
│       ├── finalSpeed = base * (1 + PlayerStats.moveSpeedPercent)
│       └── rb.linearVelocity = direction * finalSpeed
│
├── WeaponManager.Update()
│   └── foreach weapon in registered
│       ├── cooldown -= deltaTime
│       └── if cooldown <= 0
│           ├── FindClosestEnemy(range)
│           ├── count = PlayerWeaponSystem.GetCount(weapon)
│           └── ShootProjectiles(weapon, target, count)
│
├── EnemySpawner.Update()
│   ├── elapsedTime += deltaTime
│   ├── UpdatePhase()
│   ├── if spawnTimer >= interval: SpawnRandom()
│   ├── CheckBurstWaves()
│   ├── CheckBossSpawn()
│   └── if bossKilled: ShowWinUI()
│
├── EnemyController.Update() (mỗi enemy)
│   └── MoveTowardsPlayer()
│       ├── direction = (player - self).normalized
│       └── rb.linearVelocity = direction * speed
│
├── Projectile.Update() (mỗi đạn)
│   ├── Move(direction * speed)
│   ├── Rotate(rotateSpeed)
│   └── if distance >= max: ReturnToPool()
│
├── ExpOrb.Update() (mỗi orb)
│   ├── if distance < attractRange:
│   │   ├── Offset (lùi ra)
│   │   └── Attract (hút về)
│   └── if OnTrigger: AddExp()
│
└── CircleAOE.DamageOverTime() (coroutine)
    └── every 0.2s: OverlapCircle → TakeDamage()
```

---

### 9.3. Damage Flow (Chi tiết)

```
1. Weapon shoots
   WeaponManager.ShootProjectiles()
   └── pool.Get() → Initialize(direction, weaponData)

2. Projectile flies
   Projectile.Update()
   └── transform.position += direction * speed * dt

3. Collision
   Projectile.OnTriggerEnter2D(enemy)
   └── EnemyController enemy = collider.GetComponent<>()

4. Compute damage
   baseDamage = weaponData.baseDamage
   finalDamage = PlayerStats.Instance.ComputeDamage(baseDamage)
                = (baseDamage + damageFlat) * (1 + damagePercent)

5. Apply damage
   enemy.TakeDamage(finalDamage)
   ├── currentHealth -= finalDamage
   ├── DamageTextPool.Show(finalDamage, position)
   ├── FlashRed() coroutine
   └── if currentHealth <= 0: Die()

6. Enemy dies
   EnemyController.Die()
   ├── animator.SetTrigger("Die")
   ├── ExpOrbPool.SpawnOrb(position, expValue)
   └── Destroy(gameObject, 0.5f)

7. Return projectile
   Projectile.ReturnToPool()
   └── ObjectPool.Return(this)
```

---

### 9.4. Level Up Flow (Chi tiết)

```
1. Player kills enemy
   ExpOrb drops → Player collects
   └── PlayerExperience.AddExp(amount)

2. Check level up
   currentExp += amount
   while (currentExp >= requiredExp)
   {
       currentExp -= requiredExp
       LevelUp()
       requiredExp *= 1.2
   }

3. Show panel
   LevelUp()
   ├── currentLevel++
   ├── Time.timeScale = 0
   └── LevelUpPanel.Show()

4. Random skills
   LevelUpPanel.Show()
   ├── GetAvailableSkills() - filter maxLevel
   ├── RandomPick(3 skills)
   └── foreach skill: skillOptions[i].SetSkill(skill)

5. Player selects
   SkillOptionUI.OnClick()
   └── LevelUpPanel.SelectSkill(skillData)

6. Apply skill
   SkillManager.AddSkill(skillData)
   ├── skillLevels[skillId]++
   └── SkillEffectApplier.ApplySkill(skill, level)
       └── foreach effect in skill.effects
           └── effect.Apply(playerContext, level)

7. Effect applies
   StatModifierEffect.Apply()
   └── stats.damagePercent += 0.1

   WeaponCountEffect.Apply()
   ├── weaponSystem.knifeCount++
   └── weaponManager.ResetCooldown(Knife)

8. Resume game
   Time.timeScale = 1
   └── Continue game loop
```

---

### 9.5. Win/Lose Conditions

**Win Flow**:
```
1. Timer reaches 170s
   EnemySpawner: if elapsed >= bossSpawnTime

2. Spawn boss
   SpawnBoss()
   ├── bossInstance = Instantiate(bossType.prefab)
   └── bossSpawned = true

3. Stop normal spawning
   if stopSpawningAfterBoss: canSpawn = false

4. Player kills boss
   Boss.Die() → Destroy(gameObject)

5. Check win
   if bossSpawned && bossInstance == null
   └── ShowWinUI()
       ├── Time.timeScale = 0
       └── Display "Victory!"
```

**Lose Flow**:
```
1. Enemy collides with player
   Enemy.OnTriggerEnter2D(player)
   └── player.TakeDamage(contactDamage)

2. Player HP reaches 0
   PlayerHealth.TakeDamage()
   └── if currentHP <= 0: Die()

3. Show death
   PlayerHealth.Die()
   ├── animator.SetTrigger("Die")
   ├── onDeath?.Invoke()
   └── Disable player

4. Game Over UI shows (handled by listener)
```

---

## 10. CÂU HỎI THƯỜNG GẶP

### Q1: Tại sao dùng ScriptableObject cho data?
**A**:
- Tách data khỏi logic → dễ balance
- Không cần code để thay đổi số liệu
- Designer có thể tự tweak trong Unity Editor
- Tránh hardcode values trong script

---

### Q2: Tại sao cần Object Pool?
**A**:
- Instantiate/Destroy tốn performance
- Gây GC (Garbage Collection) spike → lag
- Pool tái sử dụng objects → mượt hơn
- Quan trọng khi bắn hàng trăm projectile

---

### Q3: Tại sao WeaponManager dùng cooldown-based?
**A**:
- Dễ modify fireRate realtime (từ skill)
- Dễ pause/resume (set timeScale = 0)
- Không cần quản lý nhiều Coroutine
- Clear logic, dễ debug

---

### Q4: Tại sao Skill System dùng Strategy Pattern?
**A**:
- Mỗi effect là 1 ScriptableObject độc lập
- Dễ thêm effect type mới (không sửa code cũ)
- Combine nhiều effects trong 1 skill
- Reusable: 1 effect dùng cho nhiều skills

---

### Q5: EnemySpawner hoạt động như thế nào?
**A**:
```
Phase System: Thêm enemy types theo thời gian
  0-20s: E1, E2
  20-60s: E1, E2, E3, Mage
  60+s: All

Continuous Spawn: Mỗi 2s spawn random từ activeTypes

Burst Waves: Spawn custom tại thời điểm cố định
  VD: 30s spawn 10x E3

Boss: 170s spawn Boss, stop normal spawning

Win: Boss dies → Victory screen
```

---

### Q6: ExpOrb magnet mechanic hoạt động thế nào?
**A**:
```
1. Spawn → Wait 0.5s (attractDelay)
2. Player đến gần (distance < attractRange)
   attractRange = 0.5 + pickupRangeAdd (từ skill)
3. Offset: Lùi ra với force 5f trong 0.25s
4. Attract: Hút về với min speed 15f/s
5. OnTrigger: AddExp() → ReturnToPool()
```

**Tại sao có Offset?**
- Hiệu ứng đẹp (giống Vampire Survivors)
- Feedback rõ ràng khi kill enemy

---

### Q7: Damage được tính như thế nào?
**A**:
```csharp
float baseDamage = weaponData.baseDamage;  // VD: 10
float finalDamage = PlayerStats.ComputeDamage(baseDamage);

// Formula:
finalDamage = (baseDamage + damageFlat) * (1 + damagePercent)

// Example:
baseDamage = 10
damageFlat = 5 (từ skill)
damagePercent = 0.3 (từ skill +30%)

finalDamage = (10 + 5) * (1 + 0.3)
            = 15 * 1.3
            = 19.5
```

---

### Q8: Làm thế nào để thêm vũ khí mới?
**A**:
```
1. Tạo Prefab cho projectile
   - Sprite, Collider2D, Rigidbody2D
   - Script: Projectile.cs (hoặc custom)

2. Tạo WeaponData ScriptableObject
   - weaponId, damage, fireRate, speed, etc.
   - Assign projectilePrefab

3. Thêm weapon type vào enum (nếu custom)
   - WeaponCountEffect.WeaponType

4. Thêm count variable vào PlayerWeaponSystem
   - public int newWeaponCount = 0;

5. WeaponManager.RegisterWeapon()
   - registeredWeapons.Add(weaponData);
   - CreatePool(weaponData);

6. Tạo Skill để unlock weapon
   - SkillData với WeaponCountEffect
```

---

### Q9: Làm thế nào để thêm skill mới?
**A**:
```
1. Tạo SkillData ScriptableObject
   - skillId, skillName, icon, description

2. Tạo Effect (hoặc dùng existing)
   - StatModifierEffect: Tăng stats
   - WeaponCountEffect: Tăng weapon
   - Custom: Tạo class extends SkillEffect

3. Add effect vào skill.effects[]

4. Add skill vào LevelUpPanel.allSkills[]

5. Test in-game
```

---

### Q10: Tại sao dùng Singleton cho PlayerStats?
**A**:
- Stats cần được truy cập từ nhiều nơi:
  - WeaponManager (compute damage)
  - Player (compute speed)
  - ExpOrb (pickup range)
- Singleton đảm bảo chỉ có 1 instance
- Dễ access: `PlayerStats.Instance.damagePercent`

---

### Q11: UI System hoạt động thế nào?
**A**:
```
UIManager (Singleton)
├── currentPanel (exclusive)
│   - Chỉ 1 panel active cùng lúc
│   - ShowPanel() → Hide current, Show new
│
└── overlays (stack)
    - Nhiều overlays có thể stack
    - OpenOverlay() → Push to stack
    - CloseTopOverlay() → Pop từ stack

Example:
MainUI (panel)
  └── SettingsPanel (overlay)
      └── ConfirmDialog (overlay)
```

---

### Q12: Firebase được dùng để làm gì?
**A**:
- Authentication: Login/Register
- Realtime Database: Save/Load player data
- Leaderboard: Ranking system
- Cloud Functions: Server-side logic (nếu có)

**Files liên quan**:
- `Assets/Scripts/DB/FirebaseController.cs`
- `Assets/Scripts/Ui/LoginPanelUI.cs`

---

## 11. CÁCH ĐỌC VÀ HIỂU CODE

### 11.1. Quy trình đọc 1 file mới

```
1. Đọc class name và inheritance
   - VD: public class Projectile : MonoBehaviour
   - Kế thừa MonoBehaviour → có lifecycle Unity

2. Đọc các variables (public/private)
   - Public: Được assign từ Inspector
   - Private: Internal logic
   - SerializeField: Private nhưng show Inspector

3. Đọc Unity lifecycle methods
   - Awake(): Init trước Start
   - Start(): Init khi object enable lần đầu
   - Update(): Mỗi frame
   - OnTriggerEnter2D(): Collision

4. Đọc public methods
   - API để class khác gọi
   - VD: TakeDamage(), Initialize()

5. Đọc private helper methods
   - Logic internal
   - VD: CalculateDirection(), FindTarget()

6. Tìm references đến class này
   - Ctrl+Shift+F (Visual Studio)
   - Hiểu class này được dùng ở đâu
```

---

### 11.2. Debug tips

```
1. Debug.Log() là người bạn
   Debug.Log($"Damage: {finalDamage}");

2. Unity Inspector
   - Xem real-time values
   - Serialize private variables để debug

3. Breakpoints (Visual Studio)
   - F9: Set breakpoint
   - F5: Debug mode
   - Step through code

4. Unity Profiler
   - Window > Analysis > Profiler
   - Tìm performance bottleneck

5. Scene view Debug
   - Gizmos.DrawLine(): Vẽ debug lines
   - OnDrawGizmos(): Visualize logic
```

---

## 12. TÓM TẮT QUAN TRỌNG NHẤT

### Để hiểu game này, nhớ 5 điều:

1. **ScriptableObject cho Data**
   - WeaponData, EnemyData, SkillData
   - Tách data khỏi logic

2. **Object Pool cho Performance**
   - Projectile, ExpOrb
   - Tái sử dụng thay vì Instantiate/Destroy

3. **Singleton cho Global Access**
   - UIManager, PlayerStats, SkillManager
   - Truy cập từ mọi nơi

4. **Event-driven với Delegates**
   - PlayerHealth events
   - Loose coupling giữa systems

5. **Cooldown-based Weapons**
   - WeaponManager.Update()
   - Dễ control, pause, modify

---

## KẾT LUẬN

Game này là một Survivor-like với kiến trúc rõ ràng:
- **Player** di chuyển và tự động bắn
- **Weapons** được quản lý bởi cooldown system
- **Skills** upgrade stats và weapons
- **Enemies** spawn theo phases và boss fight
- **EXP** để level up và chọn skills
- **UI** quản lý bởi UIManager singleton

Tất cả được thiết kế với:
- Clean separation of concerns
- Data-driven với ScriptableObjects
- Performance-oriented với Object Pools
- Maintainable với Design Patterns

---

**Tài liệu này sẽ giúp bạn**:
- Trả lời khi thầy hỏi về bất kỳ hệ thống nào
- Giải thích luồng hoạt động
- Hiểu tại sao code được viết như vậy
- Thêm features mới một cách tự tin

Chúc bạn vấn đáp tốt! 🎮
