using UnityEngine;

[RequireComponent(typeof(EnemyController))]
public class EnemyShooter : MonoBehaviour
{
    [Header("Bắn đạn")]
    public GameObject bulletPrefab;   
    public Transform firePoint;       
    public float fireRate = 1f;       
    public float attackRange = 8f;    

    private Transform player;
    private float fireTimer;

    private void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    private void Update()
    {
        if (player == null) return;

        // chỉ bắn khi player nằm trong range
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > attackRange) return;

        fireTimer += Time.deltaTime;
        if (fireTimer >= fireRate)
        {
            Shoot();
            fireTimer = 0f;
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        Vector2 dir = (player.position - firePoint.position).normalized;

        GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet bullet = bulletGO.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Setup(dir);
        }
    }
}
