using UnityEngine;
using UnityEngine.InputSystem;

public class PlatformerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float deceleration = 80f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float maxJumpHoldTime = 0.3f;
    [SerializeField] private float jumpHoldForce = 30f;
    [SerializeField] private float fallGravityMultiplier = 2.5f;
    [SerializeField] private float jumpBufferTime = 0.2f;

    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;

    private Rigidbody2D rb;
    private float defaultGravityScale;
    private bool isGrounded;
    private bool isHoldingJump;
    private float jumpHoldTimer;
    private float jumpBufferTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        defaultGravityScale = rb.gravityScale;
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();
        jumpAction.action.started += OnJumpStarted;
        jumpAction.action.canceled += OnJumpCanceled;
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        jumpAction.action.Disable();
        jumpAction.action.started -= OnJumpStarted;
        jumpAction.action.canceled -= OnJumpCanceled;
    }

    private void Update()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Fire buffered jump the moment the player lands
        if (!wasGrounded && isGrounded && jumpBufferTimer > 0f)
            ExecuteJump();

        if (jumpBufferTimer > 0f)
            jumpBufferTimer -= Time.deltaTime;

        if (isHoldingJump)
        {
            jumpHoldTimer += Time.deltaTime;
            if (jumpHoldTimer >= maxJumpHoldTime)
                isHoldingJump = false;
        }

        // Heavier gravity on the way down for snappier feel
        rb.gravityScale = rb.linearVelocity.y < 0f
            ? defaultGravityScale * fallGravityMultiplier
            : defaultGravityScale;
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleJumpHold();
    }

    private void HandleMovement()
    {
        float moveInput = moveAction.action.ReadValue<Vector2>().x;
        float targetSpeed = moveInput * maxSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;
        float rate = Mathf.Abs(moveInput) > 0.01f ? acceleration : deceleration;

        rb.AddForce(Vector2.right * (speedDiff * rate));
    }

    private void HandleJumpHold()
    {
        if (!isHoldingJump) return;
        rb.AddForce(Vector2.up * jumpHoldForce);
    }

    private void ExecuteJump()
    {
        isHoldingJump = true;
        jumpHoldTimer = 0f;
        jumpBufferTimer = 0f;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    private void OnJumpStarted(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            ExecuteJump();
            return;
        }

        // Store the input for when the player lands
        jumpBufferTimer = jumpBufferTime;
    }

    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        isHoldingJump = false;
        jumpBufferTimer = 0f;

        // Cut jump short if released early while ascending
        if (rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
