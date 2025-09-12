using UnityEngine;

public class Shuriken : MonoBehaviour
{
    public float speed = 12f;
    public int damage = 15;
    public float rotateSpeed = 500f; 
    private Vector2 direction;
    private ParticleSystem trail;

    void Start()
    {
        trail = GetComponentInChildren<ParticleSystem>();
        if (trail != null) trail.Play(); // bật hiệu ứng khi vừa tạo
    }
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    void Update()
    {
        transform.position += (Vector3)direction * speed * Time.deltaTime;

        transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        EnemyController enemy = other.GetComponent<EnemyController>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            // tắt trail 
            if (trail != null)
            {
                trail.transform.parent = null;
                Destroy(trail.gameObject, 0.5f);
            }
            Destroy(gameObject);
        }
    }
}
