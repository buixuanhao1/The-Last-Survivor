# Hướng Dẫn Cải Thiện Code — DoAnGame

> **Mục đích**: Tài liệu này giải thích từng vấn đề trong dự án,  
> lý do tại sao nó là vấn đề, và cách sửa cụ thể bằng code thật của dự án.  
> Được viết cho người mới học — mỗi vấn đề có phần **"Bản chất"** giải thích khái niệm trước.

---

## Trạng thái các bug đã được sửa

| File | Vấn đề | Trạng thái |
|------|--------|-----------|
| `EnemySpawner.cs` | Win UI không hiện nếu boss chết sau khi hết giờ | ✅ Đã sửa |
| `EnemySpawner.cs` | `IsTypeAllowed()` tồn tại nhưng không được gọi → map restriction không hoạt động | ✅ Đã sửa |
| `EnemySpawner.cs` | `ApplyMapConfig()` là dead code, không bao giờ được gọi | ✅ Đã dọn |
| `CircleAOEEffect.cs` | Ripple animation chạy ngay frame đầu tiên | ✅ Đã sửa |
| `CircleAOEEffect.cs` | Coroutine leak khi AOE bị destroy giữa chừng | ✅ Đã sửa |

---

## Mục Lục — Các vấn đề còn lại cần cải thiện

1. [Debug.Log còn sót trong production code](#1-debuglog-còn-sót-trong-production-code)
2. [GetComponent gọi lặp lại không cần thiết](#2-getcomponent-gọi-lặp-lại-không-cần-thiết)
3. [Magic Numbers — con số thần bí không có tên](#3-magic-numbers--con-số-thần-bí-không-có-tên)
4. [Không có Namespace — 70 class cùng "tầng"](#4-không-có-namespace--70-class-cùng-tầng)
5. [Singleton bị lạm dụng](#5-singleton-bị-lạm-dụng)
6. [WeaponManager làm quá nhiều việc trong 1 file](#6-weaponmanager-làm-quá-nhiều-việc-trong-1-file)
7. [Enemy AI chưa dùng đúng Kế thừa (Inheritance)](#7-enemy-ai-chưa-dùng-đúng-kế-thừa-inheritance)
8. [UI cập nhật thủ công — dễ bị quên, dễ bị lỗi](#8-ui-cập-nhật-thủ-công--dễ-bị-quên-dễ-bị-lỗi)
9. [FindObjectOfType dùng sai chỗ](#9-findobjectoftype-dùng-sai-chỗ)
10. [Lộ trình học thêm](#10-lộ-trình-học-thêm)

---

## 1. Debug.Log còn sót trong production code

### Bản chất

`Debug.Log()` **không tự tắt** khi bạn build game ra file .exe hay .apk. Nó vẫn chạy ngầm và tốn CPU — nhất là khi đặt trong hàm gọi nhiều lần như `TakeDamage()`.

### Vị trí trong dự án

**[EnemyController.cs:53-55](Assets/Scripts/Enemy/EnemyController.cs)**

```csharp
// HIỆN TẠI — BAD
public void TakeDamage(int dmg)
{
    Debug.Log("TakeDamage called");                          // gọi mỗi lần bị đánh
    Debug.Log("Pool instance = " + DamageTextPool.Instance); // tạo string mới mỗi lần gọi → rác GC
    currentHP -= dmg;
    ...
}
```

Nếu 50 quái mỗi quái bị đánh 2 lần/giây → **200 lần log/giây** trong khi chơi game.

### Cách sửa — 3 mức

**Mức 1 — Đơn giản nhất**: Xóa các log không cần thiết.

**Mức 2 — Chỉ log trong Editor** (dùng Conditional Compilation):

```csharp
public void TakeDamage(int dmg)
{
    // #if UNITY_EDITOR chỉ compile đoạn này khi chạy trong Editor
    // Khi build ra game thật, đoạn này hoàn toàn bị bỏ qua — không tốn 1 byte CPU
    #if UNITY_EDITOR
    Debug.Log($"[Enemy] TakeDamage: {dmg} dmg, HP còn {currentHP - dmg}/{enemyData.baseHP}");
    #endif

    currentHP -= dmg;
    DamageTextPool.Instance.Show(transform.position + Vector3.up * 1.2f, dmg);
    if (currentHP <= 0) Die();
}
```

**Mức 3 — Tạo lớp log riêng** (dùng khi project lớn):

```csharp
// Tạo file Assets/Scripts/Utils/GameLog.cs
public static class GameLog
{
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Info(string msg)  => Debug.Log($"[INFO] {msg}");

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Warn(string msg)  => Debug.LogWarning($"[WARN] {msg}");
}

// Dùng ở bất cứ đâu:
GameLog.Info($"Enemy took {dmg} damage");
```

> **Attribute `[System.Diagnostics.Conditional("UNITY_EDITOR")]`** có nghĩa:  
> "Hàm này chỉ được compile và gọi khi build target là UNITY_EDITOR.  
> Trong build release, mọi lời gọi hàm này bị xóa hoàn toàn bởi compiler."

---

## 2. GetComponent gọi lặp lại không cần thiết

### Bản chất

`GetComponent<T>()` là một **thao tác tìm kiếm** — Unity duyệt qua danh sách tất cả component gắn trên GameObject để tìm kiểu T. Gọi nó nhiều lần trên cùng một object là đi tìm lại thứ đã biết vị trí rồi.

**Quy tắc vàng:**
- `Awake()` / `Start()` → lấy component và lưu vào biến thành viên
- Mọi hàm khác → chỉ dùng biến đó, không gọi `GetComponent` lại

### Vị trí trong dự án

**[EnemyController.cs:63-80](Assets/Scripts/Enemy/EnemyController.cs)**

```csharp
// HIỆN TẠI — BAD: Die() tìm lại component đã có từ Awake()
void Die()
{
    if (isDead) return;
    isDead = true;

    var col = GetComponent<Collider2D>();    // ← tìm lại lần 2, đã có col2D
    if (col) col.enabled = false;

    var rb = GetComponent<Rigidbody2D>();    // ← tìm lại lần 2, đã có rb2D
    if (rb) { rb.linearVelocity = Vector2.zero; rb.simulated = false; }

    GetComponent<Animator>().SetTrigger("Die"); // ← tìm lại lần 3, đã có animator
}
```

### Cách sửa

```csharp
// SAU KHI SỬA — dùng biến đã cache trong Awake()
void Die()
{
    if (isDead) return;
    isDead = true;

    ExpOrbPool.Instance.SpawnOrb(transform.position, enemyData.expDrop);

    col2D.enabled       = false;          // dùng col2D từ Awake
    rb2D.linearVelocity = Vector2.zero;   // dùng rb2D từ Awake
    rb2D.simulated      = false;
    animator.SetTrigger("Die");           // dùng animator từ Awake
}
```

---

## 3. Magic Numbers — con số thần bí không có tên

### Bản chất

"Magic Number" là những con số xuất hiện thẳng trong code mà không có tên giải thích ý nghĩa. Vấn đề:
- Đọc lại sau 1 tháng không hiểu `0.06f` là gì
- Muốn balance game phải tìm từng file một
- Cùng một giá trị nhưng xuất hiện ở nhiều chỗ → sửa một chỗ, quên chỗ khác

### Vị trí trong dự án

**[WeaponManager.cs](Assets/Scripts/Weapon/WeaponManager.cs)**
```csharp
returningAngleOffset = (returningAngleOffset + 20f) % 360f;  // 20f là gì?
cooldowns[weapon] = 0.1f;                                     // 0.1f là gì?
SpawnShurikenBurst(..., count, 0.06f);                        // 0.06f là gì?
float totalSpread = Mathf.Min(60f, 10f * (count - 1));        // 60f, 10f là gì?
```

**[ExpOrb.cs:65](Assets/Scripts/Exp/ExpOrb.cs)**
```csharp
float finalSpeed = Mathf.Max(dynamicSpeed, 15f); // 15f = tốc độ tối thiểu, nhưng ai biết?
```

### Cách sửa — tạo file Constants

```csharp
// Tạo file: Assets/Scripts/Utils/GameConstants.cs
public static class GameConstants
{
    // ---------- WEAPON ----------
    /// <summary>Góc xoay mỗi loạt bắn của Returning Shuriken (độ)</summary>
    public const float RETURNING_SHURIKEN_ANGLE_STEP  = 20f;

    /// <summary>Cooldown check dao Knife mỗi frame (giây)</summary>
    public const float KNIFE_MAINTAIN_INTERVAL        = 0.1f;

    /// <summary>Delay giữa mỗi phi tiêu trong burst shuriken (giây)</summary>
    public const float SHURIKEN_BURST_DELAY           = 0.06f;

    /// <summary>Góc tán xạ tối đa khi bắn nhiều đạn (độ)</summary>
    public const float MAX_SPREAD_ANGLE               = 60f;

    /// <summary>Góc thêm vào per đạn khi spread</summary>
    public const float SPREAD_ANGLE_PER_PROJECTILE    = 10f;

    // ---------- EXP ORB ----------
    /// <summary>Tốc độ hút orb tối thiểu (world unit/giây)</summary>
    public const float ORB_MIN_ATTRACT_SPEED          = 15f;

    // ---------- ENEMY ----------
    public const float ENEMY_DEFAULT_SPAWN_RADIUS     = 10f;
    public const float ENEMY_KNOCKBACK_DURATION       = 0.1f;

    // ---------- PLAYER ----------
    /// <summary>Thời gian bất tử sau khi nhận sát thương (giây)</summary>
    public const float PLAYER_INVINCIBLE_DURATION     = 0.2f;
}
```

Sau đó thay vào code:
```csharp
// WeaponManager.cs — SAU KHI SỬA (đọc là hiểu ngay)
returningAngleOffset = (returningAngleOffset + GameConstants.RETURNING_SHURIKEN_ANGLE_STEP) % 360f;
cooldowns[weapon]    = GameConstants.KNIFE_MAINTAIN_INTERVAL;
SpawnShurikenBurst(..., count, GameConstants.SHURIKEN_BURST_DELAY);
float totalSpread    = Mathf.Min(
    GameConstants.MAX_SPREAD_ANGLE,
    GameConstants.SPREAD_ANGLE_PER_PROJECTILE * (count - 1)
);
```

---

## 4. Không có Namespace — 70 class cùng "tầng"

### Bản chất

Namespace giống như **thư mục** cho tên class. Hiện tại tất cả 70 class đều ở global namespace — giống như để 70 file vào cùng thư mục gốc ổ C:\.

Rủi ro:
- Import plugin ngoài có class tên `Player`, `ObjectPool`, `GameManager` → **xung đột tên, lỗi compile**
- Nhìn vào tên `EnemyData` không biết nó thuộc module nào

### Cách sửa

**Bước 1**: Wrap code trong namespace, **không thay đổi gì khác**:

```csharp
// Assets/Scripts/Player/PlayerHealth.cs — TRƯỚC
public class PlayerHealth : MonoBehaviour { ... }

// SAU — chỉ thêm namespace, code bên trong không đổi
namespace DoAnGame.Player
{
    public class PlayerHealth : MonoBehaviour { ... }
}
```

**Bước 2**: Khi class A (namespace khác) cần dùng class B, thêm `using`:

```csharp
// EnemyController.cs cần dùng ExpOrbPool và DamageTextPool
using DoAnGame.Pooling;   // ExpOrbPool, DamageTextPool

namespace DoAnGame.Enemy
{
    public class EnemyController : MonoBehaviour { ... }
}
```

**Cấu trúc namespace đề xuất:**
```
DoAnGame.Player     → Player, PlayerHealth, PlayerExperience, PlayerStats (ThongSoNguoiChoi)
DoAnGame.Weapons    → WeaponManager, WeaponData, Projectile, OrbitWeapon, BombProjectile ...
DoAnGame.Enemy      → EnemyController, EnemySpawner, EnemyData, EnemyBase
DoAnGame.Skills     → SkillManager, SkillData, SkillEffect, StatModifierEffect
DoAnGame.UI         → UIManager, LevelUpPanel, HeroPanelUI, WinPanelUI ...
DoAnGame.Database   → FirebaseManager, AuthManager, UserDataManager
DoAnGame.Pooling    → ObjectPool, ExpOrbPool, DamageTextPool
DoAnGame.Utils      → GameConstants, GameLog
```

> **Khi nào làm?** Đây là refactor lớn, ảnh hưởng tất cả file. Nên tạo **nhánh git riêng** và làm từng module một, test kỹ sau mỗi nhóm.

---

## 5. Singleton bị lạm dụng

### Bản chất

**Singleton** đảm bảo chỉ có **đúng 1 instance** của class và truy cập từ bất kỳ đâu qua `ClassName.Instance`. Rất tiện, nhưng nếu dùng quá nhiều sẽ gây:

1. **Tight coupling**: `ExpOrb` phụ thuộc vào `PlayerStats`, `ExpOrbPool`, `PlayerExperience` — cả 3 đều là Singleton. Muốn test ExpOrb riêng lẻ thì không được vì nó đòi tất cả Singleton phải tồn tại.
2. **Khó tìm nguồn gốc bug**: `PlayerStats.damagePercent` bị thay đổi — ai đã gọi? Mọi class đều có thể là thủ phạm.
3. **Thứ tự khởi tạo nguy hiểm**: Nếu `DamageTextPool` chưa `Awake()` mà `EnemyController.TakeDamage()` đã chạy → NullReferenceException.

### Singleton trong dự án của bạn

```
PlayerStats.Instance    → Projectile, ExpOrb, OrbitWeapon, WeaponManager, ...
ExpOrbPool.Instance     → EnemyController
DamageTextPool.Instance → EnemyController
UIManager.Instance      → khắp nơi
UserDataManager.Instance→ UI panels
MapSelection.Instance   → EnemySpawner
```

### Singleton nào nên giữ?

| Singleton | Đánh giá | Lý do |
|-----------|----------|-------|
| `ExpOrbPool` | Giữ | Duy nhất, global, không thay đổi |
| `DamageTextPool` | Giữ | Duy nhất, global, không thay đổi |
| `UIManager` | Giữ | Hub UI toàn game, hợp lý |
| `MapSelection` | Giữ | Shared state qua scenes |
| `UserDataManager` | Giữ | Shared state qua scenes |
| `PlayerStats` | Cân nhắc | Chỉ cần trong gameplay scene |

### Cải thiện PlayerStats — dùng Dependency Injection

Thay vì `ExpOrb` tự đi tìm Singleton, để **ExpOrbPool inject vào** khi spawn:

```csharp
// ExpOrb.cs — SAU KHI SỬA
public class ExpOrb : MonoBehaviour
{
    private int expAmount;
    private Transform player;
    private bool isActive, isCollected;
    private PlayerStats stats; // nhận từ bên ngoài, không tự tìm

    // playerStats được truyền vào từ pool — ExpOrb không cần biết Singleton tồn tại không
    public void Init(int amount, Transform playerRef, PlayerStats playerStats)
    {
        expAmount   = amount;
        player      = playerRef;
        stats       = playerStats;
        isActive    = true;
        isCollected = false;
        gameObject.SetActive(true);
    }
    // ... phần còn lại không đổi
}
```

```csharp
// ExpOrbPool.cs — truyền playerStats khi spawn
public class ExpOrbPool : MonoBehaviour
{
    public static ExpOrbPool Instance;
    [SerializeField] private GameObject orbPrefab;
    [SerializeField] private PlayerStats playerStats;    // kéo vào Inspector
    [SerializeField] private Transform   playerTransform;// kéo vào Inspector

    public void SpawnOrb(Vector3 position, int expAmount)
    {
        ExpOrb orb = GetFromPool();
        orb.transform.position = position;
        orb.Init(expAmount, playerTransform, playerStats); // inject vào
    }
}
```

> **Nguyên tắc "Dependency Injection" đơn giản**:  
> Thay vì class tự đi tìm thứ nó cần (FindObjectOfType, .Instance),  
> hãy để **bên ngoài truyền vào** qua constructor hoặc hàm Init.  
> Code dễ test hơn và rõ ràng hơn về "ai phụ thuộc vào ai".

---

## 6. WeaponManager làm quá nhiều việc trong 1 file

### Bản chất

**Single Responsibility Principle (SRP)**: Mỗi class chỉ nên có **1 lý do để thay đổi**.  
`WeaponManager.cs` (~535 dòng) hiện tại có ít nhất 5 lý do để thay đổi:
- Khi thêm weapon type mới
- Khi thay đổi logic cooldown
- Khi thay đổi cách tìm target
- Khi thay đổi logic Returning Shuriken
- Khi thay đổi logic Bomb

Hệ quả: thêm vũ khí mới → phải sửa WeaponManager → dễ làm hỏng vũ khí cũ.

### Dấu hiệu rõ nhất: code bị trùng lặp

Đoạn này xuất hiện **2 lần giống hệt nhau** trong `WeaponManager.cs` (Update + ForceFire):

```csharp
// Lần 1 — trong ForceFire() (dòng ~62)
if (weapon == returningShurikenWeapon)
{
    Vector2 dir = new Vector2(Mathf.Cos(returningAngleOffset * Mathf.Deg2Rad), ...);
    ShootProjectiles(weapon, transform.position + (Vector3)dir, count);
    returningAngleOffset = (returningAngleOffset + 20f) % 360f;
    cooldowns[weapon] = 1f / Mathf.Max(0.0001f, ...);
}

// Lần 2 — trong Update() (dòng ~183) — GIỐNG HỆT
if (weapon == returningShurikenWeapon)
{
    Vector2 dir = new Vector2(Mathf.Cos(returningAngleOffset * Mathf.Deg2Rad), ...);
    ShootProjectiles(weapon, transform.position + (Vector3)dir, count);
    returningAngleOffset = (returningAngleOffset + 20f) % 360f;
    cooldowns[weapon] = 1f / Mathf.Max(0.0001f, ...);
}
```

**Khi code bị duplicate → dấu hiệu cần tách ra.**

### Cách sửa — tạo interface IWeaponHandler

**Bước 1**: Interface chung:

```csharp
// Assets/Scripts/Weapons/IWeaponHandler.cs
public interface IWeaponHandler
{
    bool CanHandle(WeaponData weapon);

    // Trả về thời gian cooldown cần set, trả về 0 nếu không fire được
    float TryFire(WeaponData weapon, Transform origin, PlayerStats stats,
                  PlayerWeaponSystem weaponSys);
}
```

**Bước 2**: Handler riêng cho Returning Shuriken:

```csharp
// Assets/Scripts/Weapons/Handlers/ReturningShurikenHandler.cs
public class ReturningShurikenHandler : IWeaponHandler
{
    private readonly WeaponData targetWeapon;
    private readonly ObjectPool pool;
    private float angleOffset;

    public ReturningShurikenHandler(WeaponData weapon, ObjectPool pool)
    {
        this.targetWeapon = weapon;
        this.pool = pool;
    }

    public bool CanHandle(WeaponData weapon) => weapon == targetWeapon;

    public float TryFire(WeaponData weapon, Transform origin, PlayerStats stats,
                         PlayerWeaponSystem weaponSys)
    {
        int count = Mathf.Max(weaponSys.returningShurikenCount, 0);
        if (count == 0) return 0f;

        // Toàn bộ logic nằm ở đây, không còn bị trùng trong WeaponManager
        Vector2 dir = new Vector2(
            Mathf.Cos(angleOffset * Mathf.Deg2Rad),
            Mathf.Sin(angleOffset * Mathf.Deg2Rad)
        );
        SpawnPattern(origin.position, dir, count, weapon);
        angleOffset = (angleOffset + GameConstants.RETURNING_SHURIKEN_ANGLE_STEP) % 360f;

        float fr = stats != null ? stats.ComputeFireRate(weapon.fireRate) : weapon.fireRate;
        return 1f / Mathf.Max(0.0001f, fr);
    }

    private void SpawnPattern(Vector3 origin, Vector2 baseDir, int count, WeaponData weapon)
    {
        float step    = 360f / Mathf.Max(1, count);
        float baseAng = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;
        float outDist = weapon.returningOutDistance + weapon.returningDistancePerLevel * (count - 1);

        for (int i = 0; i < count; i++)
        {
            float   angle = baseAng + step * i;
            Vector2 d     = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            var     go    = pool.Get(origin, Quaternion.identity);
            var     rp    = go.GetComponent<ReturningProjectile>();
            rp?.Init(weapon, d, pool, null, outDist);
        }
    }
}
```

**Bước 3**: WeaponManager trở thành "điều phối viên" đơn giản:

```csharp
// WeaponManager.cs — sau khi refactor (ngắn hơn nhiều)
public class WeaponManager : MonoBehaviour
{
    // ... fields giữ nguyên ...
    private List<IWeaponHandler> handlers;

    void Start()
    {
        // Đăng ký handler — thêm weapon mới = thêm dòng này, không sửa gì khác
        handlers = new List<IWeaponHandler>
        {
            new ReturningShurikenHandler(returningShurikenWeapon, CreatePool(returningShurikenWeapon)),
            new BombHandler(bombWeapon, CreatePool(bombWeapon)),
            new KnifeHandler(knifeWeapon, CreatePool(knifeWeapon)),
            new StandardProjectileHandler(this)   // fallback cho projectile thường
        };
    }

    void Update()
    {
        foreach (var weapon in weapons)
        {
            cooldowns[weapon] -= Time.deltaTime;
            if (cooldowns[weapon] > 0f) continue;

            foreach (var handler in handlers)
            {
                if (!handler.CanHandle(weapon)) continue;
                float cd = handler.TryFire(weapon, transform, playerStats, weaponSystem);
                if (cd > 0f) cooldowns[weapon] = cd;
                break;
            }
        }
    }
}
```

> **Lợi ích**: Thêm weapon mới = tạo 1 Handler mới + đăng ký 1 dòng. Không bao giờ phải sửa vào Update() hay ForceFire() nữa.

---

## 7. Enemy AI chưa dùng đúng Kế thừa (Inheritance)

### Bản chất

Bạn đã có `EnemyMelee.cs`, `EnemyShooter.cs`, `EnemyBossDash.cs` nhưng chúng chưa có nội dung khác nhau — `EnemyController.cs` vẫn xử lý tất cả. Khi muốn thêm hành vi riêng (Shooter đứng xa bắn đạn, Boss dash), bạn sẽ phải thêm `if/else` vào `EnemyController` → file phình to giống `WeaponManager`.

**Inheritance (kế thừa)** giúp:
- Class con `override` (ghi đè) hành vi của class cha
- Class cha xử lý logic chung (nhận damage, die, knockback)
- Class con chỉ quan tâm đến điều khác biệt của nó (cách di chuyển, cách tấn công)

### Cách sửa — tạo EnemyBase

**Bước 1**: Tạo class cha xử lý mọi thứ chung:

```csharp
// Assets/Scripts/Enemy/EnemyBase.cs
public abstract class EnemyBase : MonoBehaviour
{
    [Header("Data")]
    public EnemyData enemyData;

    // Cache component một lần trong Awake — các subclass dùng chung
    protected SpriteRenderer spriteRenderer;
    protected Animator        animator;
    protected Collider2D      col;
    protected Rigidbody2D     rb;
    protected Transform       player;
    protected int             currentHP;
    protected bool            isDead;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator       = GetComponent<Animator>();
        col            = GetComponent<Collider2D>();
        rb             = GetComponent<Rigidbody2D>();
    }

    protected virtual void Start()
    {
        player    = GameObject.FindGameObjectWithTag("Player")?.transform;
        currentHP = enemyData.baseHP;
        spriteRenderer.sprite = enemyData.sprite;
    }

    protected virtual void Update()
    {
        if (isDead || player == null) return;
        UpdateBehavior(); // giao cho subclass
    }

    // Subclass BẮT BUỘC phải implement cách di chuyển / tấn công
    protected abstract void UpdateBehavior();

    // TakeDamage + Die CHUNG cho mọi loại quái — subclass không cần viết lại
    public virtual void TakeDamage(int dmg)
    {
        if (isDead) return;
        currentHP -= dmg;
        DamageTextPool.Instance.Show(transform.position + Vector3.up * 1.2f, dmg);
        if (currentHP <= 0) Die();
    }

    protected virtual void Die()
    {
        if (isDead) return;
        isDead       = true;

        ExpOrbPool.Instance.SpawnOrb(transform.position, enemyData.expDrop);
        col.enabled  = false;
        rb.simulated = false;
        animator.SetTrigger("Die");
    }

    public void Knockback(Vector3 hitSource, float force)
    {
        Vector2 dir = (transform.position - hitSource).normalized;
        rb.AddForce(dir * force, ForceMode2D.Impulse);
        StartCoroutine(ResetVelocity());
    }

    private System.Collections.IEnumerator ResetVelocity()
    {
        yield return new WaitForSeconds(GameConstants.ENEMY_KNOCKBACK_DURATION);
        if (!isDead) rb.linearVelocity = Vector2.zero;
    }
}
```

**Bước 2**: EnemyMelee — chỉ cần 10 dòng:

```csharp
// Assets/Scripts/Enemy/EnemyMelee.cs
public class EnemyMelee : EnemyBase
{
    protected override void UpdateBehavior()
    {
        Vector2 dir = (player.position - transform.position).normalized;
        spriteRenderer.flipX = dir.x < 0;
        transform.position  += (Vector3)(dir * enemyData.moveSpeed * Time.deltaTime);
    }
}
```

**Bước 3**: EnemyShooter — giữ khoảng cách và bắn đạn:

```csharp
// Assets/Scripts/Enemy/EnemyShooter.cs
public class EnemyShooter : EnemyBase
{
    [SerializeField] private float      preferredRange = 5f;
    [SerializeField] private float      shootCooldown  = 2f;
    [SerializeField] private GameObject bulletPrefab;

    private float shootTimer;

    protected override void UpdateBehavior()
    {
        Vector2 dir  = (player.position - transform.position).normalized;
        float   dist = Vector2.Distance(transform.position, player.position);

        // Giữ khoảng cách: lùi nếu quá gần, tiến nếu quá xa
        if (dist < preferredRange)
            transform.position -= (Vector3)dir * enemyData.moveSpeed * Time.deltaTime;
        else if (dist > preferredRange + 1f)
            transform.position += (Vector3)dir * enemyData.moveSpeed * Time.deltaTime;

        spriteRenderer.flipX = dir.x < 0;

        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0f)
        {
            FireBullet(dir);
            shootTimer = shootCooldown;
        }
    }

    private void FireBullet(Vector2 direction)
    {
        if (bulletPrefab == null) return;
        var go = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        go.GetComponent<Rigidbody2D>()?.AddForce(direction * 5f, ForceMode2D.Impulse);
    }
}
```

**Bước 4**: EnemyBossDash — di chuyển chậm + dash định kỳ:

```csharp
// Assets/Scripts/Enemy/EnemyBossDash.cs
public class EnemyBossDash : EnemyBase
{
    [SerializeField] private float dashForce    = 20f;
    [SerializeField] private float dashCooldown = 3f;

    private float dashTimer = 2f; // chờ 2 giây trước khi dash lần đầu
    private bool  isDashing;

    protected override void UpdateBehavior()
    {
        Vector2 dir = (player.position - transform.position).normalized;
        spriteRenderer.flipX = dir.x < 0;

        dashTimer -= Time.deltaTime;

        if (!isDashing)
        {
            // Di chuyển chậm về phía player
            transform.position += (Vector3)dir * enemyData.moveSpeed * Time.deltaTime;

            if (dashTimer <= 0f)
            {
                StartCoroutine(PerformDash(dir));
            }
        }
    }

    private System.Collections.IEnumerator PerformDash(Vector2 direction)
    {
        isDashing = true;
        rb.AddForce(direction * dashForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(0.3f);

        rb.linearVelocity = Vector2.zero;
        isDashing         = false;
        dashTimer         = dashCooldown;
    }

    // Override Die để thêm hiệu ứng đặc biệt cho boss
    protected override void Die()
    {
        StopAllCoroutines();
        base.Die(); // vẫn chạy logic die của class cha
        // TODO: có thể thêm camera shake, drop item đặc biệt, etc.
    }
}
```

> **Kết quả**: Thêm enemy mới = tạo class kế thừa `EnemyBase` + implement `UpdateBehavior()`.  
> Không bao giờ phải sửa code của enemy khác.

---

## 8. UI cập nhật thủ công — dễ bị quên, dễ bị lỗi

### Bản chất

Khi `currentHP` giảm, ai đó phải **chủ động gọi hàm cập nhật thanh máu**. Nếu quên gọi, UI hiển thị sai. Vấn đề thực tế: khi thêm tính năng mới (ví dụ: hồi máu qua item), bạn phải nhớ gọi refresh UI ở mọi chỗ mới thêm.

**Giải pháp**: Dùng **Event / Delegate** — UI tự đăng ký lắng nghe, khi data thay đổi nó tự cập nhật mà không cần ai nhắc.

### Cái bạn đã làm đúng trong PlayerHealth.cs

```csharp
// PlayerHealth.cs — ĐÃ ĐÚNG!
public System.Action<int, int> onHealthChanged; // event khi HP thay đổi
public System.Action           onDeath;         // event khi chết

public void TakeDamage(int satThuong)
{
    currentHP = Mathf.Max(0, currentHP - satThuong);
    onHealthChanged?.Invoke(currentHP, maxHP); // thông báo cho ai đang lắng nghe
    ...
}
```

`?.Invoke` nghĩa là "nếu có ai đăng ký lắng nghe thì gọi, không thì bỏ qua — không lỗi".

### Chuẩn hóa pattern này cho PlayerExperience

```csharp
// Assets/Scripts/Player/PlayerExperience.cs — thêm events
public class PlayerExperience : MonoBehaviour
{
    // Bất kỳ UI nào quan tâm đều tự đăng ký — PlayerExperience không biết UI tồn tại
    public System.Action<int, int> onExpChanged;  // (currentExp, requiredExp)
    public System.Action<int>      onLevelUp;     // (newLevel)

    private int currentExp;
    private int level       = 1;
    private int requiredExp = 100;

    public void AddExp(int amount)
    {
        currentExp += amount;
        onExpChanged?.Invoke(currentExp, requiredExp); // UI tự cập nhật exp bar

        while (currentExp >= requiredExp)
        {
            currentExp  -= requiredExp;
            level++;
            requiredExp  = Mathf.RoundToInt(requiredExp * 1.2f);
            onLevelUp?.Invoke(level); // LevelUpPanel tự hiện
        }
    }
}
```

```csharp
// UI lắng nghe — không cần biết PlayerExperience gọi khi nào
public class ExpBarUI : MonoBehaviour
{
    [SerializeField] private Image            fillBar;
    [SerializeField] private PlayerExperience playerExp;

    void Start()
    {
        // Đăng ký 1 lần duy nhất
        playerExp.onExpChanged += UpdateFillBar;
        playerExp.onLevelUp   += OnLevelUp;
    }

    void OnDestroy()
    {
        // QUAN TRỌNG: hủy đăng ký khi bị destroy — tránh memory leak và lỗi NullReference
        playerExp.onExpChanged -= UpdateFillBar;
        playerExp.onLevelUp   -= OnLevelUp;
    }

    private void UpdateFillBar(int current, int required)
    {
        fillBar.fillAmount = (float)current / required;
    }

    private void OnLevelUp(int newLevel)
    {
        // hiệu ứng level up nếu muốn
    }
}
```

> **Quy tắc**: Gameplay code (Player, Enemy) **không được biết UI tồn tại**.  
> UI đăng ký lắng nghe Gameplay — không phải ngược lại.

---

## 9. FindObjectOfType dùng sai chỗ

### Bản chất

`FindObjectOfType<T>()` và `FindGameObjectWithTag()` duyệt **toàn bộ scene** để tìm object. Trong scene có hàng trăm object, đây là thao tác tốn kém.

| Chỗ gọi | Đánh giá |
|---------|----------|
| `Awake()` hoặc `Start()` (chạy 1 lần) | Chấp nhận được |
| `Update()` (chạy 60 lần/giây) | **Tuyệt đối không** |
| `Init()` của pool object (gọi nhiều lần) | Nên tránh |

### Vị trí trong dự án

**[ExpOrb.cs:28](Assets/Scripts/Exp/ExpOrb.cs)**

```csharp
// ExpOrb.Init() được gọi mỗi khi orb được lấy ra từ pool
public void Init(int amount, Transform playerRef)
{
    // FindFirstObjectByType chạy khi PlayerStats.Instance là null
    // Trường hợp bình thường nó không chạy, nhưng nếu thứ tự khởi tạo sai → chạy và tốn kém
    if (stats == null) stats = PlayerStats.Instance != null
                              ? PlayerStats.Instance
                              : FindFirstObjectByType<PlayerStats>();
}
```

**[EnemySpawner.cs:63](Assets/Scripts/Enemy/EnemySpawner.cs)**

```csharp
void Start()
{
    // Ổn — chỉ gọi 1 lần trong Start
    player = GameObject.FindGameObjectWithTag("Player")?.transform;
}
```

**[EnemySpawner.cs:250](Assets/Scripts/Enemy/EnemySpawner.cs)** — trong `ShowWinUI()`:

```csharp
void ShowWinUI()
{
    var canvas = FindFirstObjectByType<Canvas>(); // gọi khi boss chết → lúc này scene đang busy
    ...
}
```

### Cách sửa

```csharp
// EnemySpawner.cs — cache Canvas trong Start thay vì tìm khi boss chết
private Canvas mainCanvas;

void Start()
{
    player     = GameObject.FindGameObjectWithTag("Player")?.transform;
    mainCanvas = FindFirstObjectByType<Canvas>(); // 1 lần duy nhất, lúc scene còn yên tĩnh
}

void ShowWinUI()
{
    if (winPanelPrefab == null) return;
    Time.timeScale = 0f;

    // Dùng cache thay vì tìm lại
    if (mainCanvas != null)
        Instantiate(winPanelPrefab, mainCanvas.transform, false);
    else
        Instantiate(winPanelPrefab);
}
```

---

## 10. Lộ trình học thêm

### Tóm tắt việc cần làm theo độ ưu tiên

| # | Việc cần làm | Độ khó | Ảnh hưởng |
|---|-------------|--------|-----------|
| 1 | Xóa Debug.Log thừa trong EnemyController | Dễ | Hiệu năng |
| 2 | Sửa GetComponent trong `EnemyController.Die()` | Dễ | Hiệu năng |
| 3 | Tạo `GameConstants.cs` và thay magic numbers | Dễ | Dễ bảo trì |
| 4 | Cache Canvas trong `EnemySpawner.Start()` | Dễ | Hiệu năng |
| 5 | Inject PlayerStats vào ExpOrb qua pool | Trung bình | Code quality |
| 6 | Thêm events vào PlayerExperience | Trung bình | Decoupling |
| 7 | Tạo EnemyBase + refactor subclass | Trung bình | Extensibility |
| 8 | Thêm Namespace | Trung bình | Organization |
| 9 | Refactor WeaponManager với IWeaponHandler | Khó | Extensibility |

---

### Điều bạn đã làm đúng (nên tiếp tục)

- **ScriptableObject cho data** (WeaponData, EnemyData, SkillData) — tách data khỏi logic, đúng hướng
- **Object Pooling** (Projectile, ExpOrb, DamageText) — tư duy hiệu năng tốt từ đầu
- **Delegate/Action trong PlayerHealth** (`onHealthChanged`, `onDeath`) — đây là Observer Pattern, rất đúng
- **Abstract SkillEffect** — Strategy Pattern, đúng cách thiết kế extensible system
- **Cache component trong Awake()** (ở hầu hết chỗ) — đúng

---

### Khái niệm nên học tiếp

| Khái niệm | Tại sao liên quan đến dự án này |
|-----------|--------------------------------|
| **SOLID Principles** | Giải thích tại sao WeaponManager bị "fat", tại sao nên tách Handler |
| **Observer Pattern** | Đã dùng trong PlayerHealth, nên áp dụng rộng hơn |
| **Strategy Pattern** | Đã dùng trong SkillEffect, nên áp dụng cho WeaponHandler và EnemyBehavior |
| **Object Pool Pattern** | Đã dùng — nên hiểu sâu hơn về khi nào pooling không cần thiết |
| **Dependency Injection** | Tại sao inject tốt hơn Singleton.Instance |

**Tài liệu miễn phí:**
- [Game Programming Patterns](https://gameprogrammingpatterns.com) — quyển sách online miễn phí, viết cho game dev
- [Unity Scriptable Objects Talk — Ryan Hipple (Unite Austin 2017)](https://www.youtube.com/watch?v=raQ3iHhE_Kk) — giải thích tại sao ScriptableObject mạnh

---

*Cập nhật lần cuối: 2026-04-15*  
*Bugs đã sửa trực tiếp: EnemySpawner.cs (3 vấn đề), CircleAOEEffect.cs (2 vấn đề)*
