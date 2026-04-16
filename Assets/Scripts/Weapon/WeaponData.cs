 using UnityEngine;

public enum WeaponType
{
    Projectile,
    Orbit,
    CircleAOE   
}


[CreateAssetMenu(menuName = "Weapon/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("Common Settings")]
    public string weaponName;
    public WeaponType weaponType;
    public GameObject prefab;
    public float fireRate = 1f;
    public float attackRange = 6f;
    public int damage = 10;

    [Header("Projectile Settings")]
    public float speed = 12f;
    public float rotateSpeed = 0f;

    [Header("Returning Settings")]
    [Tooltip("Khoảng cách bay ra trước khi thu về (áp dụng cho ReturningProjectile)")]
    public float returningOutDistance = 8f;
    [Tooltip("Mỗi cấp cộng thêm bao nhiêu đơn vị khoảng cách bay ra (áp dụng cho ReturningProjectile)")]
    public float returningDistancePerLevel = 0f;

    [Header("Orbit Settings")]
    public int orbitCount = 2;        // số viên xoay quanh
    public float orbitRadius = 1.5f;  // bán kính
    public float orbitDuration = 5f;  // tồn tại bao lâu
    public float orbitCooldown = 3f;  // hồi chiêu

    [Header("Circle AOE Settings")]
    public float aoeRadius = 2f;       // bán kính vòng
    public float aoeDuration = 4f;     // tồn tại bao lâu
    public float aoeTickInterval = 0.2f; // thời gian giữa mỗi lần gây dmg

    [Header("Bomb Settings")]
    public GameObject explosionPrefab; // prefab hiệu ứng nổ (có animator)
    public float bombFuseTime = 1.2f;  // thời gian bay trước khi nổ
    public float explosionRadius = 2.5f; // bán kính gây sát thương khi nổ
    [Tooltip("Bán kính điểm rơi ngẫu nhiên tính từ player")] public float bombDropRadius = 6f;
    [Tooltip("Thời gian bay đến điểm rơi (giây)")] public float bombTravelTime = 0.8f;
    [Tooltip("Độ cao đỉnh cung (chỉ để mô phỏng/hiệu ứng)")] public float bombMaxHeight = 1.5f;
}
