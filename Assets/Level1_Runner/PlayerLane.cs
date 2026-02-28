using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerLane : MonoBehaviour
{
    [Header("Lane Settings")]
    [SerializeField] private int laneCount = 3;
    [SerializeField] private float laneDistance = 3f;
    [SerializeField] private float laneSpeed = 15f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float groundCheckDistance = 0.3f;

    private int targetLane = 1;
    private Rigidbody rb;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    // ---------------- INPUT ----------------

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        float inputX = context.ReadValue<Vector2>().x;

        if (inputX > 0.5f)
            targetLane++;

        else if (inputX < -0.5f)
            targetLane--;

        targetLane = Mathf.Clamp(targetLane, 0, laneCount - 1);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (IsGrounded())
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    // ---------------- MOVEMENT ----------------

    private void FixedUpdate()
    {
        MoveToLane();
    }

    private void MoveToLane()
    {
        float targetX = (targetLane - 1) * laneDistance;

        Vector3 newPosition = Vector3.MoveTowards(
            rb.position,
            new Vector3(targetX, rb.position.y, rb.position.z),
            laneSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(newPosition);
    }

    // ---------------- GROUND CHECK ----------------

    private bool IsGrounded()
    {
        return Physics.Raycast(
            transform.position,
            Vector3.down,
            groundCheckDistance
        );
    }
}