using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("References")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool canMove = true;

    private const string IS_WALKING_PARAM = "IsWalking";
    private const string JUMP_TRIGGER = "Jump";

    // Свойства для клавиш управления
    public KeyCode JumpKey
    {
        get => (KeyCode)PlayerPrefs.GetInt("JumpKey", (int)KeyCode.W);
        set => PlayerPrefs.SetInt("JumpKey", (int)value);
    }

    public KeyCode RightKey
    {
        get => (KeyCode)PlayerPrefs.GetInt("RightKey", (int)KeyCode.D);
        set => PlayerPrefs.SetInt("RightKey", (int)value);
    }

    public KeyCode LeftKey
    {
        get => (KeyCode)PlayerPrefs.GetInt("LeftKey", (int)KeyCode.A);
        set => PlayerPrefs.SetInt("LeftKey", (int)value);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (!canMove || Time.timeScale == 0f) return; 

        CheckGrounded();
        HandleJump();
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        if (!canMove || Time.timeScale == 0f) return;
        HandleMovement();
    }

    private void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void HandleMovement()
    {
        float moveInput = 0f;

        if (Input.GetKey(RightKey)) moveInput += 1f;
        if (Input.GetKey(LeftKey)) moveInput -= 1f;

        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        if (moveInput != 0 && spriteRenderer != null)
        {
            spriteRenderer.flipX = moveInput < 0;
        }
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(JumpKey) && isGrounded)
        {
            Jump();
        }
    }

    private void UpdateAnimations()
    {
        if (animator != null)
        {
            bool isMoving = Input.GetKey(RightKey) || Input.GetKey(LeftKey);
            animator.SetBool(IS_WALKING_PARAM, isMoving);
        }
    }

    private void Jump()
    {
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        if (animator != null)
        {
            animator.SetTrigger(JUMP_TRIGGER);
        }
    }

    public void SetCanMove(bool canMove)
    {
        this.canMove = canMove;
        if (!canMove)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if (animator != null)
            {
                animator.SetBool(IS_WALKING_PARAM, false);
            }
        }
    }
    public void ClearInputBuffer()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        if (animator != null)
        {
            animator.SetBool(IS_WALKING_PARAM, false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    public void SetMovementEnabled(bool enabled)
    {
        canMove = enabled;
        if (!enabled)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if (animator != null)
            {
                animator.SetBool(IS_WALKING_PARAM, false);
            }
        }
    }

}