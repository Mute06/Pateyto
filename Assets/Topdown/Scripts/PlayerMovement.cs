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

        moveInput = moveAction.ReadValue<Vector2>();
   
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;
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
