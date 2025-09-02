using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5.0f;
    private Rigidbody2D rb;
    private SpriteRenderer rbSprite;
    private Animator animator;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rbSprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        MovePlayer();
    }

    void MovePlayer()
    {
        Vector2 playerInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        rb.linearVelocity = playerInput.normalized * moveSpeed;
        if (playerInput.x < 0)
        {
            rbSprite.flipX = true;
        }else if (playerInput.x > 0)
        {
            rbSprite.flipX=false;
        }
        if(playerInput != Vector2.zero)
        {
            animator.SetBool("isRun", true);
        }else if(playerInput == Vector2.zero)
        {
            animator.SetBool("isRun", false);
        }
    }
}
