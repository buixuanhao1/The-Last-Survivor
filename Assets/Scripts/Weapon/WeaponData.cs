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

    [Header("Orbit Settings")]
    public int orbitCount = 2;        // số viên xoay quanh
    public float orbitRadius = 1.5f;  // bán kính
    public float orbitDuration = 5f;  // tồn tại bao lâu
    public float orbitCooldown = 3f;  // hồi chiêu

    [Header("Circle AOE Settings")]
    public float aoeRadius = 2f;       // bán kính vòng
    public float aoeDuration = 4f;     // tồn tại bao lâu
    public float aoeTickInterval = 0.2f; // thời gian giữa mỗi lần gây dmg
}
