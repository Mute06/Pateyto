using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    InputAction moveAction;
    private Animator animator;

    public bool canMove = true;
    public bool restrictToRightOnly = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        moveAction = InputSystem.actions.FindAction("Move");
        animator = GetComponentInChildren<Animator>();

        moveAction.started += OnMoveStarted;
        moveAction.canceled += OnMoveCanceled;
        moveAction.performed += OnMovePerformed;
    }



    private void Update()
    {
        if (canMove)
        {
            moveInput = moveAction.ReadValue<Vector2>();
            
            // Eğer kısıtlama varsa sadece sağ (X > 0) gitmesine izin ver
            if (restrictToRightOnly) 
            {
                moveInput.y = 0f; // Yukarı(W) / Aşağı(S) engelle
                if (moveInput.x < 0f) moveInput.x = 0f; // Sola(A) gitmeyi engelle
            }
        }
        else
        {
            moveInput = Vector2.zero;
        }
    }

    private void FixedUpdate()
    {
        if (canMove)
        {
            rb.linearVelocity = moveInput * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void OnMoveStarted(InputAction.CallbackContext context)
    {
        animator.SetBool("isWalking", true);
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        animator.SetBool("isWalking", false);
        animator.SetFloat("lastInputX", moveInput.x);
        animator.SetFloat("lastInputY", moveInput.y);
    }
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        animator.SetFloat("InputX", context.ReadValue<Vector2>().x);
        animator.SetFloat("InputY", context.ReadValue<Vector2>().y);
    }

    private void OnDestroy()
    {
        moveAction.started -= OnMoveStarted;
        moveAction.canceled -= OnMoveCanceled;
        moveAction.performed -= OnMovePerformed;
    }

}
