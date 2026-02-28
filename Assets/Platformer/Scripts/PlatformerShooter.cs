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

    [Header("Sounds")]
    [SerializeField] private AudioClip[] shootClips;
    [SerializeField] private AudioClip reloadClip;




    private AudioSource gunSound;
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
        gunSound = GetComponent<AudioSource>();
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
        if (firePoint == null) firePoint = upFirePoint; // Default to upFirePoint if downFirePoint is unassigned
        
        if (firePoint == null) return; // Failsafe if neither is assigned

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        if (bullet.TryGetComponent<Rigidbody2D>(out Rigidbody2D bulletRb))
            bulletRb.linearVelocity = firePoint.right * bulletSpeed;

        currentBullets--;
        fireCooldown = fireRate;

        PlayShootSound();
    }

    private void OnReload(InputAction.CallbackContext context)
    {
        if (isReloading || currentBullets == maxBullets) return;

        gunSound.PlayOneShot(reloadClip);
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

    private void PlayShootSound()
    {
        int randshoot = Random.Range(0, shootClips.Length); 
        gunSound.PlayOneShot(shootClips[randshoot]);
        CameraShake.Instance.ShakeCamera(5f, 0.1f);
    }
}
