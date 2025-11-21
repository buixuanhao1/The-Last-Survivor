using PinePie.SimpleJoystick;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private JoystickController joystickController;
    [SerializeField] private float moveSpeed = 5.0f;
    private Rigidbody2D rb;
    private SpriteRenderer rbSprite;
    private Animator animator;
    private PlayerStats stats;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rbSprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        stats = FindFirstObjectByType<PlayerStats>();
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
        // Lấy hướng di chuyển từ joystick (Vector2)
        Vector2 playerInput = joystickController != null
            ? joystickController.InputDirection
            : new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")); // fallback nếu test bằng bàn phím

        float speed = stats != null ? stats.ComputeMoveSpeed(moveSpeed) : moveSpeed;
        rb.linearVelocity = playerInput.normalized * speed;

        if (playerInput.x < 0)
            rbSprite.flipX = true;
        else if (playerInput.x > 0)
            rbSprite.flipX = false;

        animator.SetBool("isRun", playerInput != Vector2.zero);
    }

}
