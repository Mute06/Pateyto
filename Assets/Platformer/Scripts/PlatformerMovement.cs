using UnityEngine;
using UnityEngine.InputSystem;

public class PlatformerMovement : MonoBehaviour, IDamagable
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

    [Header("Sounds")]
    [SerializeField] private AudioClip dieSoundClip;

    private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    private CapsuleCollider2D col;
    private float defaultGravityScale;
    private bool isGrounded;
    private bool isCrouching;
    private bool isMovementLocked;
    private bool isHoldingJump;
    private bool facingRight = true;
    private float jumpHoldTimer;
    private float jumpBufferTimer;
    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;

    public bool GetIsCrouching() => isCrouching;

    /// <summary> Locks all movement and jumping. Used externally e.g. during reload. </summary>
    public void SetMovementLocked(bool locked) => isMovementLocked = locked;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        //animator = GetComponent<Animator>();
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
        // Bring the player to a full stop while crouching or movement is locked
        if (isCrouching || isMovementLocked)
        {
            rb.AddForce(Vector2.right * (-rb.linearVelocity.x * deceleration));
            animator.SetFloat("Speed", 0f);
            return;
        }

        float moveInput = moveAction.action.ReadValue<Vector2>().x;

        HandleFlip(moveInput);

        float targetSpeed = moveInput * maxSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;
        float rate = Mathf.Abs(moveInput) > 0.01f ? acceleration : deceleration;

        rb.AddForce(Vector2.right * (speedDiff * rate));

        animator.SetFloat("Speed", Mathf.Abs(moveInput));
    }

    private void HandleFlip(float moveInput)
    {
        if (moveInput > 0f && !facingRight)
            Flip();
        else if (moveInput < 0f && facingRight)
            Flip();
    }

    private void Flip()
    {
        facingRight = !facingRight;
        transform.eulerAngles = new Vector3(0f, facingRight ? 0f : 180f, 0f);
    }

    private void HandleJumpHold()
    {
        if (!isHoldingJump) return;
        rb.AddForce(Vector2.up * jumpHoldForce);
    }

    private void ExecuteJump()
    {
        if (isCrouching || isMovementLocked) return;

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
            animator.SetBool("isCrouching", true);
            col.size = new Vector2(originalColliderSize.x, crouchColliderHeight);
        }
        else
        {
            animator.SetBool("isCrouching", false);
            col.size = originalColliderSize;
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
        if (!HasCeilingAbove())
            SetCrouching(false);
    }

    private void OnJumpStarted(InputAction.CallbackContext context)
    {
        if (isCrouching || isMovementLocked) return;

        if (isGrounded)
        {
            ExecuteJump();
            return;
        }

        jumpBufferTimer = jumpBufferTime;
    }

    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        isHoldingJump = false;
        jumpBufferTimer = 0f;

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

    public void TakeDamage()
    {
        if (BloodManager.Instance != null)
        {
            BloodManager.Instance.SpawnBloodEffects(transform.position, Vector3.zero);
        }
        Destroy(gameObject);
        P_AudioPlayer.Instance.PlaySFX(dieSoundClip);
        Platform_SceneManager.Instance.RealoadAfter(0.75f);
    }
}
