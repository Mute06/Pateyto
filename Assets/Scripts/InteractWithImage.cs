using UnityEngine;
using UnityEngine.InputSystem;

public class InteractWithImage : MonoBehaviour
{
    [Header("Hedef Obje Ayarları")]
    public GameObject targetGameObject;

    [Header("Etkileşim Ayarları")]
    public bool autoInteract = false;
    [Tooltip("Alana girdikten kaç saniye sonra etkileşime geçebilir?")]
    public float interactionDelay = 5f;

    [Header("Cooldown (Kapatma Engeli)")]
    [Tooltip("Resim açıldıktan sonra kaç saniye boyunca KAPATILAMAZ?")]
    public float closeCooldown = 3f;

    [Header("Kapanma Ayarları")]
    [Tooltip("Resim kapandıktan kaç saniye sonra BU obje deaktif olsun?")]
    public float selfDeactivateDelay = 2f;

    private bool isPlayerInRange = false;
    private float entryTime;
    private float openTime; // Resmin açıldığı an
    private bool isOpened = false;
    private bool isSelfDeactivating = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isSelfDeactivating)
        {
            isPlayerInRange = true;
            entryTime = Time.time;
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
        if (!isPlayerInRange || isSelfDeactivating) return;

        bool ePressed = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;

        // --- AÇMA MANTIĞI ---
        if (!isOpened)
        {
            bool timeIsUp = (Time.time - entryTime >= interactionDelay);
            if ((autoInteract && timeIsUp) || (timeIsUp && ePressed))
            {
                OpenImage();
            }
        }
        // --- KAPATMA MANTIĞI ---
        else if (ePressed)
        {
            // Resim açılalı 'closeCooldown' kadar süre geçmiş mi?
            if (Time.time - openTime >= closeCooldown)
            {
                CloseAndScheduleDestroy();
            }
            else
            {
                Debug.Log("Henüz kapatamazsın! Bekle: " + (closeCooldown - (Time.time - openTime)).ToString("F1") + "s");
            }
        }
    }

    private void OpenImage()
    {
        if (targetGameObject != null)
        {
            targetGameObject.SetActive(true);
            isOpened = true;
            openTime = Time.time; // Açıldığı anı kaydet
        }
    }

    private void CloseAndScheduleDestroy()
    {
        if (targetGameObject != null)
        {
            targetGameObject.SetActive(false);
            isSelfDeactivating = true; // Artık hiçbir tuş işlemez

            // Belirlenen süre sonra etkileşim objesini kapat
            Invoke("DisableThisObject", selfDeactivateDelay);
        }
    }

    private void DisableThisObject()
    {
        this.gameObject.SetActive(false);
    }
}