# CHEAT SHEET - TRA CỨU NHANH

## FILE QUAN TRỌNG NHẤT (TOP 15)

| File | Chức năng | Vị trí |
|------|-----------|--------|
| **Player.cs** | Di chuyển player | Assets/Scripts/Player/ |
| **PlayerHealth.cs** | Quản lý HP + invincibility | Assets/Scripts/Player/ |
| **PlayerExperience.cs** | EXP + Level up | Assets/Scripts/Player/ |
| **ThongSoNguoiChoi.cs** | Singleton stats (damage, speed) | Assets/Scripts/Player/ |
| **WeaponManager.cs** | Quản lý bắn vũ khí | Assets/Scripts/Weapon/ |
| **WeaponData.cs** | ScriptableObject vũ khí | Assets/Scripts/Weapon/ |
| **EnemySpawner.cs** | Spawn enemy (phase/burst/boss) | Assets/Scripts/Enemy/ |
| **EnemyController.cs** | AI enemy + TakeDamage | Assets/Scripts/Enemy/ |
| **SkillData.cs** | ScriptableObject skill | Assets/Scripts/Data/ |
| **SkillManager.cs** | Quản lý skill levels | Assets/Scripts/Player/ |
| **LevelUpPanel.cs** | UI chọn skill | Assets/Scripts/Ui/ |
| **ExpOrb.cs** | Orb EXP + magnet | Assets/Scripts/Exp/ |
| **ObjectPool.cs** | Pool projectile | Assets/Scripts/Weapon/ |
| **UIManager.cs** | Singleton quản lý UI | Assets/Scripts/Ui/ |
| **Projectile.cs** | Đạn bay cơ bản | Assets/Scripts/Weapon/ |

---

## CÁC HỆ THỐNG CHÍNH (1 DÒNG MỖI HỆ THỐNG)

| Hệ thống | Mô tả |
|----------|-------|
| **Player** | Di chuyển (Player.cs) + Máu (PlayerHealth.cs) + EXP (PlayerExperience.cs) + Stats (ThongSoNguoiChoi.cs) |
| **Weapon** | Manager (WeaponManager.cs) + Pool (ObjectPool.cs) + 6 loại vũ khí (Projectile, Knife, Orbit, AoE, Bomb, Returning) |
| **Skill** | Data (SkillData.cs) + Manager (SkillManager.cs) + Effects (StatModifier, WeaponCount) + UI (LevelUpPanel.cs) |
| **Enemy** | Spawner (EnemySpawner.cs) + Controller (EnemyController.cs) + Data (EnemyData.cs) |
| **EXP** | Orb (ExpOrb.cs) + Pool (ExpOrbPool.cs) + Magnet mechanic |
| **UI** | Manager (UIManager.cs) + Panels (exclusive) + Overlays (stack) |

---

## VŨ KHÍ (6 LOẠI)

| Vũ khí | File | Kiểu | Đặc điểm |
|--------|------|------|----------|
| **Shuriken** | Projectile.cs | Projectile | Bay thẳng, despawn khi hết tầm |
| **Tarot** | Projectile.cs | Projectile | Giống Shuriken + rotate |
| **Knife** | Knife.cs | Persistent | Bounce nhiều enemy, per-enemy cooldown |
| **Stone** | OrbitWeapon.cs | Orbit | Quay quanh player, knockback |
| **Sword** | CircleAOE.cs | AOE | Vùng damage, tick 0.2s |
| **Bomb** | BombProjectile.cs | Projectile | Cung bay, nổ AoE |

---

## SKILL EFFECTS (2 LOẠI CHÍNH)

| Effect | File | Chức năng |
|--------|------|-----------|
| **StatModifierEffect** | StatModifierEffect.cs | Tăng stats: damage%, speed%, fireRate%, pickupRange, healthRegen |
| **WeaponCountEffect** | WeaponCountEffect.cs | Tăng số lượng vũ khí (Shuriken, Knife, etc.) |

---

## DESIGN PATTERNS

| Pattern | Dùng ở đâu | Lý do |
|---------|-----------|--------|
| **Singleton** | UIManager, PlayerStats, SkillManager, ExpOrbPool | Truy cập global |
| **Object Pool** | ObjectPool, ExpOrbPool | Tối ưu performance (tránh Instantiate/Destroy) |
| **ScriptableObject** | WeaponData, EnemyData, SkillData, MapConfig | Tách data khỏi logic |
| **Strategy** | SkillEffect (base) → StatModifier, WeaponCount | Dễ extend, combine effects |
| **Event System** | PlayerHealth delegates (onHealthChanged, onDeath) | Loose coupling |

---

## CÔNG THỨC TÍNH TOÁN

### Damage
```csharp
finalDamage = (baseDamage + damageFlat) * (1 + damagePercent)
```

### Speed
```csharp
finalSpeed = baseSpeed * (1 + moveSpeedPercent)
```

### Fire Rate
```csharp
finalFireRate = baseFireRate * (1 + fireRatePercent)
```

### EXP Required
```csharp
Level 1→2: 100
Level 2→3: 120 (100 * 1.2)
Level 3→4: 144 (120 * 1.2)
...
requiredExp *= 1.2 mỗi level
```

### Attract Range (ExpOrb)
```csharp
attractRange = 0.5f + pickupRangeAdd
```

---

## ENEMY SPAWNER (3 CƠ CHẾ)

| Cơ chế | Mô tả |
|--------|-------|
| **Phase** | Thêm enemy types theo thời gian (0-20s: E1,E2 → 20-60s: E1,E2,E3,Mage → 60+: All) |
| **Continuous** | Mỗi 2s spawn random từ activeTypes |
| **Burst** | Spawn custom tại thời điểm cố định (VD: 30s spawn 10x E3) |
| **Boss** | 170s spawn Boss, stop normal spawning, win khi boss dies |

---

## LUỒNG QUAN TRỌNG

### Damage Flow
```
Weapon → Projectile → OnTrigger → Enemy.TakeDamage()
→ ComputeDamage() → currentHP -= damage
→ if HP <= 0: Die() → SpawnOrb()
```

### Level Up Flow
```
ExpOrb → Player.AddExp() → if exp >= required: LevelUp()
→ LevelUpPanel.Show() → Player chọn skill
→ SkillManager.AddSkill() → Effect.Apply()
```

### Weapon Shooting Flow
```
WeaponManager.Update() → cooldown -= dt
→ if cooldown <= 0: ForceFire()
→ FindClosestEnemy() → GetCount() → ShootProjectiles()
```

### ExpOrb Magnet Flow
```
Drop → Wait 0.5s → Player gần → Offset (lùi ra 0.25s)
→ Attract (hút về min 15f/s) → OnTrigger → AddExp()
```

---

## UNITY LIFECYCLE (THỨ TỰ)

```
1. Awake()      - Init trước tất cả (Singleton init ở đây)
2. OnEnable()   - Khi object được enable
3. Start()      - Init khi object active lần đầu
4. Update()     - Mỗi frame
5. FixedUpdate() - Fixed timestep (physics)
6. LateUpdate() - Sau Update (camera follow)
7. OnDestroy()  - Khi object bị destroy
```

---

## THÔNG SỐ GAME QUAN TRỌNG

| Thông số | Giá trị |
|----------|---------|
| **Game Duration** | 180s (3 phút) |
| **Boss Spawn Time** | 170s |
| **Enemy Spawn Interval** | 2s |
| **Invincibility Duration** | 0.2s |
| **Sword Tick Rate** | 0.2s |
| **EXP Multiplier** | 1.2x mỗi level |
| **Base Required EXP** | 100 |
| **Attract Delay (Orb)** | 0.5s |
| **Offset Duration (Orb)** | 0.25s |
| **Min Attract Speed** | 15f/s |
| **Stone Knockback Force** | 3f |

---

## KEYWORDS ĐỂ SEARCH CODE

| Muốn tìm | Keyword search |
|----------|----------------|
| Player di chuyển | `MovePlayer` hoặc `linearVelocity` |
| Player bị damage | `PlayerHealth.TakeDamage` |
| Bắn vũ khí | `WeaponManager.ForceFire` |
| Spawn enemy | `EnemySpawner.SpawnRandom` |
| Level up | `PlayerExperience.LevelUp` |
| Chọn skill | `LevelUpPanel.SelectSkill` |
| Damage calculation | `ComputeDamage` |
| Enemy AI | `MoveTowardsPlayer` |
| Pool objects | `ObjectPool.Get` |
| UI navigation | `UIManager.ShowPanel` |

---

## CÂU LỆNH DEBUG HỮU ÍCH

```csharp
// Xem damage
Debug.Log($"Final Damage: {finalDamage} = ({baseDamage} + {damageFlat}) * (1 + {damagePercent})");

// Xem weapon count
Debug.Log($"Knife Count: {playerWeaponSystem.knifeCount}");

// Xem skill level
Debug.Log($"Skill Level: {SkillManager.Instance.GetSkillLevel(skillId)}");

// Xem enemy HP
Debug.Log($"Enemy HP: {currentHealth}/{maxHealth}");

// Xem EXP
Debug.Log($"EXP: {currentExp}/{requiredExp}");
```

---

## CẤU TRÚC THƯ MỤC (SIMPLIFIED)

```
Assets/Scripts/
├── Player/          - Player, Health, Experience, Stats, WeaponSystem, SkillManager
├── Weapon/          - WeaponManager, WeaponData, Projectile, Knife, Orbit, AoE, Bomb, Pool
├── Enemy/           - EnemySpawner, EnemyController, EnemyData
├── Skills/          - SkillEffect, StatModifier, WeaponCount
├── Exp/             - ExpOrb, ExpOrbPool
├── Ui/              - UIManager, LevelUpPanel, SkillOptionUI
├── Data/            - SkillData, MapConfig
└── DB/              - FirebaseController
```

---

## TRẢ LỜI NHANH

### "Giải thích PlayerStats?"
Singleton lưu modifiers (damage%, speed%, fireRate%). Compute final values cho Player và Weapon.

### "WeaponManager làm gì?"
Quản lý cooldown tất cả vũ khí. Mỗi frame giảm cooldown, khi <= 0 thì ForceFire().

### "Skill System hoạt động thế nào?"
Player level up → LevelUpPanel show 3 skills → Chọn → SkillManager tăng level → Effect.Apply() tăng stats/weapon count.

### "Enemy spawn như thế nào?"
Phase (thêm types theo thời gian) + Continuous (mỗi 2s random) + Burst (custom) + Boss (170s).

### "ExpOrb magnet?"
Drop → Wait 0.5s → Player gần → Offset lùi ra → Attract hút về → AddExp().

### "Tại sao dùng Object Pool?"
Tránh Instantiate/Destroy liên tục → giảm GC → smooth performance.

### "ScriptableObject để làm gì?"
Tách data (weapon stats, enemy stats, skill effects) khỏi code → dễ balance không cần code.

---

## SỐ ĐIỆN THOẠI QUAN TRỌNG (FILES)

**Gặp lỗi damage sai?** → `PlayerStats.ComputeDamage()` hoặc `WeaponData.baseDamage`

**Gặp lỗi không bắn?** → `WeaponManager.Update()` hoặc `WeaponManager.RegisterWeapon()`

**Gặp lỗi skill không work?** → `SkillEffect.Apply()` hoặc `SkillManager.AddSkill()`

**Gặp lỗi enemy không spawn?** → `EnemySpawner.Update()` hoặc `EnemySpawner.phases`

**Gặp lỗi EXP không hút?** → `ExpOrb.Update()` hoặc `pickupRangeAdd`

**Gặp lỗi UI không show?** → `UIManager.ShowPanel()` hoặc check `currentPanel`

---

## GHI NHỚ 5 ĐIỀU NÀY LÀ ĐỦ

1. **ScriptableObject = Data, MonoBehaviour = Logic**
2. **Singleton cho Global Access (UIManager, PlayerStats, SkillManager)**
3. **Object Pool cho Performance (Projectile, ExpOrb)**
4. **Cooldown-based Weapons (dễ control)**
5. **Event-driven (Delegates cho loose coupling)**

---

**In ra giấy phần này để tra nhanh khi cần!** 📝
