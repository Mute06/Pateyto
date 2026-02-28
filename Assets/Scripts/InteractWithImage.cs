using UnityEngine;
using UnityEngine.InputSystem;

public class InteractWithImage : MonoBehaviour
{
    [Tooltip("Aktifleşecek olan Canvas içindeki RawImage objesini buraya sürükleyin.")]
    public GameObject targetRawImage;

    private bool isPlayerInRange = false;

    // 2D Oyunlar için
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

    // 3D Oyunlar için
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }

    private void Update()
    {
        // Player objesinin alanında ve E tuşuna basmışsa
        if (isPlayerInRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (targetRawImage != null)
            {
                // RawImage açık ise kapat, kapalı ise aç.
                targetRawImage.SetActive(!targetRawImage.activeSelf);
            }
        }
    }
}
