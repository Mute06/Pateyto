using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SpawnerDoor : MonoBehaviour
{
    [Header("Spawning")]
    public GameObject enemyPrefab1;
    public GameObject enemyPrefab2;
    public Transform spawnPoint;

    [Header("Settings")]
    public float detectionRange = 10f;
    public float spawnCooldown = 5f;
    public float timeToSpawnAfterOpen = 0.5f;

    private Animator animator;
    private Transform player;
    private float cooldownTimer;
    private bool isSpawning;

    private readonly int isOpenHash = Animator.StringToHash("isOpen");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        
        // Ensure starting closed and cooldown ready
        cooldownTimer = spawnCooldown;
    }

    private void Update()
    {
        if (player == null || isSpawning) return;

        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer <= 0f)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            if (dist <= detectionRange)
            {
                StartCoroutine(SpawnRoutine());
            }
        }
    }

    private IEnumerator SpawnRoutine()
    {
        isSpawning = true;
        cooldownTimer = spawnCooldown;

        // Open door
        animator.SetBool(isOpenHash, true);

        // Wait for animation to visually open
        yield return new WaitForSeconds(timeToSpawnAfterOpen);

        // Spawn enemy
        GameObject prefabToSpawn = Random.value > 0.5f ? enemyPrefab1 : enemyPrefab2;
        if (prefabToSpawn != null)
        {
            Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;
            Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        }

        // Slight pause before closing, to let the enemy visually step out
        yield return new WaitForSeconds(0.2f);

        // Close door
        animator.SetBool(isOpenHash, false);

        isSpawning = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
