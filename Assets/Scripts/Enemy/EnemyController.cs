using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public EnemyData enemyData;
    public GameObject expOrbPrefab; //prefab ExpOrb
    [HideInInspector] public Transform player; 
    private SpriteRenderer rbSprite;
    private int currentHP;
    [HideInInspector] public bool canMove = true;

    [Header("Death")]
    private Animator animator;          
    private  Collider2D col2D;
    private Rigidbody2D rb2D;           
    public float deathDestroyDelay = 1.0f; 

    private bool isDead = false;


    private void Awake()
    {
        rbSprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        col2D = GetComponent<Collider2D>();
        rb2D = GetComponent<Rigidbody2D>();
    }
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentHP = enemyData.baseHP;
        GetComponent<SpriteRenderer>().sprite = enemyData.sprite;
    }

    void Update()
    {
        if (player == null) return;
        if (!canMove) return;
        // Di chuyển về phía player
        Vector2 dir = (player.position - transform.position).normalized;
        if (dir.x < 0)
        {
            rbSprite.flipX = true;
        }else if (dir.x > 0)
        {
            rbSprite.flipX=false;
        }
        transform.position += (Vector3)dir * enemyData.moveSpeed * Time.deltaTime;
    }

  
    public void TakeDamage(int dmg)
    {
        Debug.Log("TakeDamage called");
        Debug.Log("Pool instance = " + DamageTextPool.Instance);

        currentHP -= dmg;
        DamageTextPool.Instance.Show(transform.position + Vector3.up * 1.2f, dmg);
        if (currentHP <= 0) Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        // Drop orb từ pool
        ExpOrbPool.Instance.SpawnOrb(transform.position, enemyData.expDrop);
        canMove = false;
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        var rb = GetComponent<Rigidbody2D>();
        if (rb)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        GetComponent<Animator>().SetTrigger("Die");
    }

    public void OnDeathAnimationEnd()
    {
        Destroy(gameObject);
    }
    public void Knockback(Vector3 hitSource, float force)
    {
        Vector2 knockDir = (transform.position - hitSource).normalized;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.AddForce(knockDir * force, ForceMode2D.Impulse);
        StartCoroutine(ResetVelocity(rb));
    }
    private System.Collections.IEnumerator ResetVelocity(Rigidbody2D rb)
    {
        yield return new WaitForSeconds(0.1f); // knockback 
        rb.linearVelocity = Vector2.zero;
    }

}
