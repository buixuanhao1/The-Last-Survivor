using UnityEngine;

public class Projectile : MonoBehaviour
{
    public WeaponData data;
    private Vector2 direction;
    private ObjectPool pool;
    private float maxDistance = 20f;
    private Vector3 spawnPos;

    public void Init(WeaponData weapon, Vector2 dir, ObjectPool objectPool)
    {
        data = weapon;
        direction = dir.normalized;
        pool = objectPool;
        spawnPos = transform.position;
        // xoay đầu tarot
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    void Update()
    {
        transform.position += (Vector3)direction * data.speed * Time.deltaTime;
        if (Vector3.Distance(spawnPos, transform.position) > maxDistance)
        {
            pool.Return(gameObject);   
        }
        if (data.rotateSpeed != 0)
            transform.Rotate(0, 0, data.rotateSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        EnemyController enemy = other.GetComponent<EnemyController>();
        if (enemy != null)
        {
            enemy.TakeDamage(data.damage);
            if(pool != null)
            {
                Debug.Log("pool: true ");
                Debug.Log(pool.ToString());
            }
            else
                Debug.Log("pool: null");
            pool.Return(gameObject);
        }
    }

}
