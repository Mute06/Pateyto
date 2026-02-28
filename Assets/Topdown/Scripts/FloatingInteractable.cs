using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class FloatingInteractable : MonoBehaviour
{
    // ────── CONFIG ──────
    [Header("Bobbing")]
    [Tooltip("How high the sprite moves up/down (world units).")]
    public float bobHeight = 0.2f;
    [Tooltip("Speed of the bobbing motion (cycles per second).")]
    public float bobFrequency = 1f;


    // ────── PRIVATE ──────
    private SpriteRenderer _sr;
    private Vector3 _startPos;
    private Material _mat;
    private bool _playerNearby = false;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _startPos = transform.position;

        // Ensure the sprite uses a material that supports emission
        if (_sr.sharedMaterial == null)
        {
            Debug.LogWarning($"{name}: SpriteRenderer has no material – assigning default glow material.");
            var glowMat = Resources.Load<Material>("Topdown/Materials/GlowMaterial");
            _sr.sharedMaterial = glowMat;
        }
        _mat = _sr.material; // instantiate a copy for this instance
        _mat.EnableKeyword("_EMISSION");
    }

    private void Update()
    {
        // ---- Bobbing ----
        float bobOffset = Mathf.Sin(Time.time * bobFrequency * 2f * Mathf.PI) * bobHeight;
        transform.position = _startPos + new Vector3(0f, bobOffset, 0f);

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            _playerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            _playerNearby = false;
    }

}
