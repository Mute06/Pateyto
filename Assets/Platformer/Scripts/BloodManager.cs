using UnityEngine;

public class BloodManager : MonoBehaviour
{
    public static BloodManager Instance;

    public GameObject bloodParticlePrefab;
    public GameObject groundSplatterPrefab;
    public GameObject backgroundSplashPrefab;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void SpawnBloodEffects(Vector3 position, Vector3 direction)
    {
        // 1. Blood particles burst
        if (bloodParticlePrefab != null)
        {
            GameObject particles = Instantiate(bloodParticlePrefab, position, Quaternion.identity);
            
            // Aim particles opposite to incoming direction (or along it, depending on preference)
            if (direction != Vector3.zero)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                particles.transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        // 2. Ground splatter (squashed, on ground layer)
        if (groundSplatterPrefab != null)
        {
            // Raycast down to find exact ground position
            RaycastHit2D hit = Physics2D.Raycast(position, Vector2.down, 5f, LayerMask.GetMask("Ground", "Default"));
            Vector3 spawnPos = hit.collider != null ? hit.point : position - new Vector3(0, 0.5f, 0);
            
            Instantiate(groundSplatterPrefab, spawnPos, Quaternion.identity);
        }

        // 3. Background splash (behind everything)
        if (backgroundSplashPrefab != null)
        {
            Instantiate(backgroundSplashPrefab, position, Quaternion.identity);
        }
    }
}
