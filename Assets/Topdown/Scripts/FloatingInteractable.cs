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

    [Header("Glow")]
    [Tooltip("Base emission intensity (0 = no glow).")]
    public float baseEmission = 0.2f;
    [Tooltip("How much the emission pulses (0‑1).")]
    [Range(0f, 1f)]
    public float emissionPulse = 0.3f;
    [Tooltip("Pulse speed (cycles per second).")]
    public float emissionFrequency = 1.5f;

    [Header("Interaction")]
    public InputActionReference interactAction;   // bind to “E”
    public UnityEvent onInteract;                // assign in Inspector (e.g. open dialogue)

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

    private void OnEnable()
    {
        interactAction.action.Enable();
        interactAction.action.performed += OnInteractPerformed;
    }

    private void OnDisable()
    {
        interactAction.action.performed -= OnInteractPerformed;
        interactAction.action.Disable();
    }

    private void Update()
    {
        // ---- Bobbing ----
        float bobOffset = Mathf.Sin(Time.time * bobFrequency * 2f * Mathf.PI) * bobHeight;
        transform.position = _startPos + new Vector3(0f, bobOffset, 0f);

        // ---- Glow pulse ----
        float emission = baseEmission + Mathf.Sin(Time.time * emissionFrequency * 2f * Mathf.PI) * emissionPulse;
        _mat.SetColor("_EmissionColor", Color.white * emission);
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

    private void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        if (_playerNearby && onInteract != null)
            onInteract.Invoke();
    }
}
