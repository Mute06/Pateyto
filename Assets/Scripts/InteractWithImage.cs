using UnityEngine;
using UnityEngine.InputSystem;

public class InteractWithImage : MonoBehaviour
{
    [Header("Hedef Obje Ayarları")]
    public GameObject targetGameObject;

    [Header("Input")]
    public InputActionReference interactAction;

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

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (isSelfDeactivating) return;

        // Only block opening if player is out of range. 
        // If it is already opened, we want them to be able to close it regardless.
        if (!isOpened && !isPlayerInRange) return; 

        if (!isOpened)
        {
            OpenImage();
        }
        else
        {
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
        // autoInteract is time-based only; key-press logic is handled in OnInteractPerformed
        if (!isPlayerInRange || isSelfDeactivating || isOpened) return;

        if (autoInteract && Time.time - entryTime >= interactionDelay)
        {
            OpenImage();
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
            // If the target has a SimpleFade component, use it to fade out smoothly.
            SimpleFade fadeComponent = targetGameObject.GetComponent<SimpleFade>();
            if (fadeComponent != null)
            {
                fadeComponent.FadeOutAndDisable();
            }
            else
            {
                // Fallback to instantly hiding it if there is no fader
                targetGameObject.SetActive(false);
            }
            
            // Mark as deactivating so no new interactions handle
            isSelfDeactivating = true; 

            // Belirlenen süre sonra etkileşim objesini kapat
            Invoke("DisableThisObject", selfDeactivateDelay);
        }
    }

    private void DisableThisObject()
    {
        this.gameObject.SetActive(false);
    }
}