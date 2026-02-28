using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Input")]
    [Tooltip("The input action for interacting (e.g. 'E' or 'Gamepad Button West')")]
    public InputActionReference interactAction;
    
    [Header("Detection Settings")]
    public float interactRadius = 1.5f;
    public LayerMask interactableLayerMask;
    public Vector2 interactionOffset = Vector2.zero;

    [Header("State")]
    // Use this to freeze the player while they are doing a puzzle.
    public bool canInteract = true; 

    private IInteractable currentInteractable;

    private void OnEnable()
    {
        if (interactAction != null)
        {
            interactAction.action.performed += OnInteractPerformed;
            interactAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.action.performed -= OnInteractPerformed;
        }
    }

    private void Update()
    {
        if (!canInteract) 
        {
            ClearCurrentInteractable();
            return;
        }

        Vector2 checkPos = (Vector2)transform.position + interactionOffset;
        Collider2D hit = Physics2D.OverlapCircle(checkPos, interactRadius, interactableLayerMask);

        if (hit != null && hit.TryGetComponent(out IInteractable interactable))
        {
            if (currentInteractable != interactable)
            {
                ClearCurrentInteractable();
                currentInteractable = interactable;
                currentInteractable.ToggleInteractPrompt(true);
            }
        }
        else
        {
            ClearCurrentInteractable();
        }
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("Interact input performed");
        if (!canInteract || currentInteractable == null) return;
        Debug.Log($"Interacting with {currentInteractable}");
        currentInteractable.Interact(this);
    }

    private void ClearCurrentInteractable()
    {
        if (currentInteractable != null)
        {
            currentInteractable.ToggleInteractPrompt(false);
            currentInteractable = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere((Vector2)transform.position + interactionOffset, interactRadius);
    }
}
