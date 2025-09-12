using UnityEngine;

public class Projectile : MonoBehaviour
{
    public WeaponData data;
    private Vector2 direction;

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    void Update()
    {
        transform.position += (Vector3)direction * data.speed * Time.deltaTime;

        if (data.rotateSpeed != 0)
            transform.Rotate(0, 0, data.rotateSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        EnemyController enemy = other.GetComponent<EnemyController>();
        if (enemy != null)
        {
            enemy.TakeDamage(data.damage);
            Destroy(gameObject);
        }
    }
}
