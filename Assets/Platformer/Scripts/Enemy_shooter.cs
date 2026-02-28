using UnityEngine;

public class Enemy_shooter : Platformer_EnemyBase
{
    [Header("Shooting")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float shootCooldown = 2f;
    [SerializeField] private float stopDuration = 0.8f;

    [Header("Line of Sight")]
    [SerializeField] private float shootRange = 6f;
    [SerializeField] private LayerMask lineOfSightBlockLayer;

    private float shootCooldownTimer;
    private float stopTimer;
    private bool isStopped;

    protected override void Update()
    {
        base.Update();

        if (isDead) return;

        if (shootCooldownTimer > 0f)
            shootCooldownTimer -= Time.deltaTime;

        if (isStopped)
        {
            stopTimer -= Time.deltaTime;
            if (stopTimer <= 0f)
                isStopped = false;
        }

        if (shootCooldownTimer <= 0f && HasLineOfSight())
            Shoot();
    }

    protected override void HandleMovement()
    {
        if (isStopped)
        {
            // Decelerate to a stop while in shoot-pause
            rb.linearVelocity = new Vector2(
                Mathf.MoveTowards(rb.linearVelocity.x, 0f, moveSpeed),
                rb.linearVelocity.y
            );
            return;
        }

        base.HandleMovement();
    }

    // Casts a horizontal ray from the fire point in the direction the enemy faces.
    // Returns true only if the first hit object is the player.
    private bool HasLineOfSight()
    {
        if (player == null || firePoint == null) return false;

        RaycastHit2D hit = Physics2D.Raycast(
            firePoint.position,
            transform.right,
            shootRange,
            lineOfSightBlockLayer
        );

        return hit.collider != null && hit.collider.CompareTag("Player");
    }

    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        if (bullet.TryGetComponent<Rigidbody2D>(out Rigidbody2D bulletRb))
            bulletRb.linearVelocity = (Vector2)transform.right * bulletSpeed;

        shootCooldownTimer = shootCooldown;
        isStopped = true;
        stopTimer = stopDuration;
    }

    private void OnDrawGizmosSelected()
    {
        if (firePoint == null) return;

        // Shoot range ray
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(firePoint.position, transform.right * shootRange);
    }
}
