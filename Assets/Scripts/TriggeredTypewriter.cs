using System.Collections;
using UnityEngine;
using TMPro; // Standart (Legacy) Text için bunu silip UnityEngine.UI ekle

public class TriggeredTypewriter : MonoBehaviour
{
    public enum TriggerAction 
    { 
        YaziyiBaslat, // Başlama Görevi
        YaziyiSilveDurdur // Bitiş Görevi
    }

    [Header("Bu Trigger'ın Görevi Ne Olacak?")]
    public TriggerAction gorev = TriggerAction.YaziyiBaslat;

    [Header("UI Ayarları")]
    [Tooltip("Ekrana yazının basılacağı Text objesi (A1 Text'i buraya sürükleyin)")]
    public TMP_Text uiTextComponent;
    
    [Header("Yazı İçeriği (Sadece 'YaziyiBaslat' için)")]
    [TextArea(3, 10)]
    public string stringToType = "Merhaba maceracı...";
    [Tooltip("Harflerin basılma hızı")]
    public float typingSpeed = 0.05f;

    [Header("Hedef Kim?")]
    public string playerTag = "Player";

    // O an çalışan Typewriter işlemini global olarak (arkada) aklında tutar
    private static Coroutine _currentTypingRoutine;
    private static TriggeredTypewriter _routineOwner;

    private void Start()
    {
        // Oyun başladığında ekranda rastgele bir şey kalmasını önlemek için yazıyı ilk objeden temizleyelim
        if (gorev == TriggerAction.YaziyiBaslat && uiTextComponent != null)
        {
            uiTextComponent.text = "";
        }
    }

    // --- 2D Oyunlar İçin ---
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            Tetikle();
        }
    }

    // --- 3D Oyunlar İçin ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            Tetikle();
        }
    }

    private void Tetikle()
    {
        if (gorev == TriggerAction.YaziyiBaslat)
        {
            // Önceki bir yazı varsa (ikinci bir trigger'a girdiyse falan) durdur
            if (_currentTypingRoutine != null && _routineOwner != null)
            {
                _routineOwner.StopCoroutine(_currentTypingRoutine);
            }

            _routineOwner = this;
            _currentTypingRoutine = StartCoroutine(TypeTextRoutine());
        }
        else if (gorev == TriggerAction.YaziyiSilveDurdur)
        {
            // Yazıyı anında durdur
            if (_currentTypingRoutine != null && _routineOwner != null)
            {
                _routineOwner.StopCoroutine(_currentTypingRoutine);
                _currentTypingRoutine = null;
            }
            
            // Ekrandaki yazıyı temizle ve yok et
            if (uiTextComponent != null)
            {
                uiTextComponent.text = ""; 
            }
        }
    }

    private IEnumerator TypeTextRoutine()
    {
        uiTextComponent.text = "";

        // Metni harf harf yazdır
        foreach (char letter in stringToType.ToCharArray())
        {
            uiTextComponent.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}
