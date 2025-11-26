using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 5f;
    public int damage = 10;
    private Vector2 direction;

    public void Setup(Vector2 dir)
    {
        direction = dir.normalized;
        Destroy(gameObject, 4f); // tự xóa nếu bay quá xa
    }

    void Update()
    {
        transform.position += (Vector3)direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Nếu trúng Player thì gây damage
        if (collision.CompareTag("Player"))
        {
            // Nếu player có script nhận damage thì gọi
            // collision.GetComponent<PlayerHealth>()?.TakeDamage(damage);

            Destroy(gameObject);
        }
    }
}
