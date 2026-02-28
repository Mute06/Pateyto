using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlatformerShooter : MonoBehaviour
{
    [Header("Fire Points")]
    [SerializeField] private Transform upFirePoint;
    [SerializeField] private Transform downFirePoint;

    [Header("Bullet")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 15f;

    [Header("Ammo")]
    [SerializeField] private int maxBullets = 10;
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private float reloadTime = 1.5f;

    [Header("Input")]
    [SerializeField] private InputActionReference shootAction;
    [SerializeField] private InputActionReference reloadAction;

    [Header("References")]
    [SerializeField] private PlatformerMovement movement;

    private int currentBullets;
    private float fireCooldown;
    private bool isReloading;
    private Coroutine reloadCoroutine;

    public int GetCurrentBullets() => currentBullets;
    public int GetMaxBullets() => maxBullets;
    public bool GetIsReloading() => isReloading;

    private void Awake()
    {
        currentBullets = maxBullets;
    }

    private void OnEnable()
    {
        shootAction.action.Enable();
        reloadAction.action.Enable();
        shootAction.action.started += OnShoot;
        reloadAction.action.started += OnReload;
    }

    private void OnDisable()
    {
        shootAction.action.Disable();
        reloadAction.action.Disable();
        shootAction.action.started -= OnShoot;
        reloadAction.action.started -= OnReload;
    }

    private void Update()
    {
        if (fireCooldown > 0f)
            fireCooldown -= Time.deltaTime;
    }

    private void OnShoot(InputAction.CallbackContext context)
    {
        if (isReloading || fireCooldown > 0f || currentBullets <= 0) return;

        Transform firePoint = movement.GetIsCrouching() ? downFirePoint : upFirePoint;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        if (bullet.TryGetComponent<Rigidbody2D>(out Rigidbody2D bulletRb))
            bulletRb.linearVelocity = firePoint.right * bulletSpeed;

        currentBullets--;
        fireCooldown = fireRate;
    }

    private void OnReload(InputAction.CallbackContext context)
    {
        if (isReloading || currentBullets == maxBullets) return;

        reloadCoroutine = StartCoroutine(ReloadCoroutine());
    }

    private IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        movement.SetMovementLocked(true);

        yield return new WaitForSeconds(reloadTime);

        currentBullets = maxBullets;
        isReloading = false;
        movement.SetMovementLocked(false);
    }
}
