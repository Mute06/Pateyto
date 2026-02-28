using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public Animator doorAnimator; // Assign the Animator component of the door in the Inspector
    private bool isOpen = false; // Track the door's state


    private void Awake()
    {
        if (doorAnimator == null)
        {
            doorAnimator = GetComponent<Animator>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Only trigger if the player enters (Optional: replace "Player" with your player's tag)
        {
            // Open the door when player enters
            isOpen = true;
            if (doorAnimator != null) doorAnimator.SetBool("IsOpen", isOpen);
        }
    }


    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isOpen = false;
            if (doorAnimator != null) doorAnimator.SetBool("IsOpen", isOpen);
        }
    }
    
}