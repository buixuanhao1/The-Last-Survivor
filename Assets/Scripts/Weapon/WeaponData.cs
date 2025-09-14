using UnityEngine;

public enum WeaponType { Projectile, Orbit }

[CreateAssetMenu(menuName = "Weapon/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public WeaponType weaponType;
    public GameObject prefab;
    public float fireRate = 1f;
    public float attackRange = 6f;
    public float speed = 12f;
    public int damage = 10;
    public float rotateSpeed = 0f;

    // dành cho orbit
    public int orbitCount = 2;       // số viên xoay quanh
    public float orbitRadius = 1.5f; // bán kính
    public float orbitDuration = 5f; // tồn tại bao lâu
    public float orbitCooldown = 3f;
}
