using UnityEngine;

[CreateAssetMenu(menuName = "Weapon/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public GameObject prefab;
    public float fireRate = 1f;
    public float attackRange = 6f;
    public float speed = 12f;
    public int damage = 10;
    public float rotateSpeed = 0f;
}
