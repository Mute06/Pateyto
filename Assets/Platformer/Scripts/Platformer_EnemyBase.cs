using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Platformer_EnemyBase : MonoBehaviour, IDamagable
{
    [Header("Stats")]
    [SerializeField] protected float maxHealth = 3f;
    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] protected float jumpForce = 8f;

    [Header("Detection")]
    [SerializeField] protected float detectionRange = 8f;
    [SerializeField] protected float obstacleCheckRadius = 0.15f;

    [Header("Check Points")]
    [SerializeField] protected Transform groundCheck;
    [SerializeField] protected float groundCheckRadius = 0.2f;
    [SerializeField] public Transform obstacleCheckPoint;
    [SerializeField] protected LayerMask groundLayer;

    protected Rigidbody2D rb;
    protected Transform player;
    protected float currentHealth;
    protected bool isGrounded;
    protected bool facingRight = true;
    protected bool isDead;
    protected bool isMovementPaused;
    private float movementPauseTimer;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    /// <summary> Stops movement for the given duration. Call from subclasses after an action e.g. shooting. </summary>
    public void PauseMovement(float duration)
    {
        isMovementPaused = true;
        movementPauseTimer = duration;
    }

    protected virtual void Update()
    {
        if (isDead) return;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isMovementPaused)
        {
            movementPauseTimer -= Time.deltaTime;
            if (movementPauseTimer <= 0f)
                isMovementPaused = false;
        }
    }

    protected virtual void FixedUpdate()
    {
        if (isDead || player == null) return;

        if (Vector2.Distance(transform.position, player.position) <= detectionRange)
        {
            HandleMovement();
            HandleObstacleJump();
        }
        else
        {
            rb.linearVelocity = new Vector2(
                Mathf.MoveTowards(rb.linearVelocity.x, 0f, moveSpeed),
                rb.linearVelocity.y
            );
        }
    }

    protected virtual void HandleMovement()
    {
        float dirX = Mathf.Sign(player.position.x - transform.position.x);

        // Always face the player regardless of movement state
        HandleFlip(dirX);

        if (isMovementPaused)
        {
            rb.linearVelocity = new Vector2(
                Mathf.MoveTowards(rb.linearVelocity.x, 0f, moveSpeed),
                rb.linearVelocity.y
            );
            return;
        }

        rb.linearVelocity = new Vector2(dirX * moveSpeed, rb.linearVelocity.y);
    }

    protected virtual void HandleObstacleJump()
    {
        if (!isGrounded || obstacleCheckPoint == null) return;

        if (Physics2D.OverlapCircle(obstacleCheckPoint.position, obstacleCheckRadius, groundLayer))
            Jump();
    }

    protected virtual void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    protected void HandleFlip(float dirX)
    {
        if (dirX > 0f && !facingRight)
            Flip();
        else if (dirX < 0f && facingRight)
            Flip();
    }

    protected void Flip()
    {
        facingRight = !facingRight;
        transform.eulerAngles = new Vector3(0f, facingRight ? 0f : 180f, 0f);
    }

    public virtual void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;

        if (currentHealth <= 0f)
            Die();
    }

    protected virtual void Die()
    {
        isDead = true;
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (obstacleCheckPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(obstacleCheckPoint.position, obstacleCheckRadius);
        }

        Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

    public void TakeDamage()
    {
        Die();
    }
}
