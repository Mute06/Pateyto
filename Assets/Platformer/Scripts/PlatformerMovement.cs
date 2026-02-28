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

    [Header("Crouch")]
    [SerializeField] private float crouchSpeedMultiplier = 0.4f;
    [SerializeField] private float crouchColliderHeight = 0.5f;

    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Ceiling Detection")]
    [SerializeField] private Transform ceilingCheck;
    [SerializeField] private float ceilingCheckRadius = 0.2f;

    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference crouchAction;

    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    private float defaultGravityScale;
    private bool isGrounded;
    private bool isCrouching;
    private bool isHoldingJump;
    private float jumpHoldTimer;
    private float jumpBufferTimer;
    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;

    public bool IsCrouching => isCrouching;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        defaultGravityScale = rb.gravityScale;
        originalColliderSize = col.size;
        originalColliderOffset = col.offset;
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();
        crouchAction.action.Enable();
        jumpAction.action.started += OnJumpStarted;
        jumpAction.action.canceled += OnJumpCanceled;
        crouchAction.action.started += OnCrouchStarted;
        crouchAction.action.canceled += OnCrouchCanceled;
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        jumpAction.action.Disable();
        crouchAction.action.Disable();
        jumpAction.action.started -= OnJumpStarted;
        jumpAction.action.canceled -= OnJumpCanceled;
        crouchAction.action.started -= OnCrouchStarted;
        crouchAction.action.canceled -= OnCrouchCanceled;
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
        float speed = isCrouching ? maxSpeed * crouchSpeedMultiplier : maxSpeed;
        float targetSpeed = moveInput * speed;
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

    private void SetCrouching(bool crouch)
    {
        if (isCrouching == crouch) return;

        isCrouching = crouch;

        if (crouch)
        {
            float heightDiff = originalColliderSize.y - crouchColliderHeight;
            col.size = new Vector2(originalColliderSize.x, crouchColliderHeight);
            col.offset = new Vector2(originalColliderOffset.x, originalColliderOffset.y - heightDiff / 2f);
        }
        else
        {
            col.size = originalColliderSize;
            col.offset = originalColliderOffset;
        }
    }

    private bool HasCeilingAbove()
    {
        return Physics2D.OverlapCircle(ceilingCheck.position, ceilingCheckRadius, groundLayer);
    }

    private void OnCrouchStarted(InputAction.CallbackContext context)
    {
        SetCrouching(true);
    }

    private void OnCrouchCanceled(InputAction.CallbackContext context)
    {
        // Prevent standing up if there's a ceiling overhead
        if (!HasCeilingAbove())
            SetCrouching(false);
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
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (ceilingCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(ceilingCheck.position, ceilingCheckRadius);
        }
    }
}
