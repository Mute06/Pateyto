using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class DeactivateTargetOnInteract : MonoBehaviour
{
    [Header("Hedef Ayarları")]
    [Tooltip("Player bu objeye yaklaşıp E'ye bastığında DEAKTİF olacak objeyi buraya sürükle.")]
    public GameObject targetToDeactivate;

    private bool isPlayerInRange = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }

    private void Update()
    {
        // Player alan içindeyse ve E tuşuna basıldıysa
        if (isPlayerInRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (targetToDeactivate != null)
            {
                targetToDeactivate.SetActive(false);
            }
            else
            {
                Debug.LogWarning("Deaktif edilecek bir hedef obje atanmamış!", this);
            }
        }
    }
}
