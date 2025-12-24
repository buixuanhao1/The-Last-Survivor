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
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<PlayerHealth>()?.TakeDamage(damage);

            Destroy(gameObject);
        }
    }
}
