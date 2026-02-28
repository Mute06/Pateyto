using UnityEngine;

public class DeactivateOnTrigger : MonoBehaviour
{
    [Header("Ayarlar")]
    [Tooltip("Hangi Tag'e sahip obje girince tetiklensin?")]
    public string targetTag = "Player";

    [Tooltip("Deaktif edilecek obje (Boþ býrakýrsanýz bu objenin kendisi kapanýr)")]
    public GameObject objectToDeactivate;

    [Tooltip("Kaç saniye sonra kapansýn? (0 = Anýnda)")]
    public float delay = 0f;

    // 2D Oyunlar için
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(targetTag))
        {
            HandleDeactivation();
        }
    }


    private void HandleDeactivation()
    {
        // Eðer bir obje atanmadýysa, scriptin baðlý olduðu objeyi hedef al
        GameObject target = objectToDeactivate != null ? objectToDeactivate : gameObject;

        if (delay > 0)
        {
            // Gecikmeli kapatma
            Invoke("DeactivateNow", delay);
        }
        else
        {
            // Anýnda kapatma
            target.SetActive(false);
        }
    }

    private void DeactivateNow()
    {
        if (objectToDeactivate != null)
            objectToDeactivate.SetActive(false);
        else
            gameObject.SetActive(false);
    }
}