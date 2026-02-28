using UnityEngine;

public class Interactable : MonoBehaviour
{
    [Tooltip("Ekranda belirecek 'Press E' yazı objesi (Canvas Text veya Sprite)")]
    public GameObject pressEPrompt;

    [Tooltip("Sadece bu Tag'e sahip obje (Player) yaklaştığında çalışır")]
    public string playerTag = "Player";

    private void Start()
    {
        // Oyun başladığında emin olmak için yazıyı baştan gizliyoruz
        if (pressEPrompt != null)
        {
            pressEPrompt.SetActive(false);
        }
    }

    // ------------------ 2D Oyunlar İçin ------------------
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Eğer trigger alanına giren obje Player ise yazıyı göster
        if (other.CompareTag(playerTag) && pressEPrompt != null)
        {
            pressEPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Player trigger alanından çıkarsa yazıyı gizle
        if (other.CompareTag(playerTag) && pressEPrompt != null)
        {
            pressEPrompt.SetActive(false);
        }
    }

}
