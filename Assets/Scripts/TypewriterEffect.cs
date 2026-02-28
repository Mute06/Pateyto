using System.Collections;
using UnityEngine;
using TMPro; // TextMeshPro kullanıyorsanız (Unity'nin modern varsayılanı)
using UnityEngine.UI; // Eski sistem Text kullanıyorsanız

public class TypewriterEffect : MonoBehaviour
{
    [Header("UI Referansı")]
    [Tooltip("Metnin yazılacağı Text objesi (Canvas'taki o A1 Text'i buraya sürükle)")]
    // Eğer oyunda standart Text (Legacy) kullanıyorsanız alttaki 'TMP_Text' yazan yeri 'Text' olarak değiştirin.
    // Modern Unity projelerinde genelde TextMeshPro kullanıldığı için standart olarak onu koydum.
    public TMP_Text uiTextComponent; 
    
    // public Text uiTextComponent; // <-- Eğer TextMeshPro değilde eski usül Text ise bu satırın başındaki // silip, üsttekini silin.

    [Header("Yazı Ayarları")]
    [Tooltip("Ekrana yazılmasını istediğiniz tam metin")]
    [TextArea(3, 10)] // Inspector'da büyük bir metin kutusu sağlar
    public string stringToType = "Merhaba maceracı, ormana hoş geldin...";

    [Tooltip("Harflerin ekrana gelme hızı (Saniye bazında. Örn: 0.05 çok hızlı, 0.2 yavaş)")]
    public float typingSpeed = 0.05f;

    [Tooltip("Script çalıştığı (Obje aktifleştiği) an kendiliğinden yazmaya başlasın mı?")]
    public bool playOnAwake = true;

    private Coroutine typingCoroutine;

    private void OnEnable()
    {
        // Obje aktifleştiğinde veya oyun başladığında otomatik başla
        if (playOnAwake)
        {
            StartTyping(stringToType);
        }
    }

    /// <summary>
    /// Yazıyı başlatır. Dışarıdan veya bir butondan vs. çağırabilirsiniz.
    /// </summary>
    public void StartTyping(string textToType)
    {
        // Önceki yarım kalan bir yazı varsa durdur ki üst üste binmesinler
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        stringToType = textToType;
        typingCoroutine = StartCoroutine(TypeTextRoutine());
    }

    /// <summary>
    /// Yazıyı aniden tamamlar ve ekrana tam basar. (Oyuncu yazının bitmesini beklemek istemeyip tıklandığında kullanılabilir)
    /// </summary>
    public void FinishTypingImmediately()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        
        if (uiTextComponent != null)
        {
            uiTextComponent.text = stringToType;
        }
    }

    private IEnumerator TypeTextRoutine()
    {
        // Yazı alanını tamamen temizle (0 harf)
        uiTextComponent.text = "";

        // stringToType içindeki her bir harf (char) için tek tek dön
        foreach (char letter in stringToType.ToCharArray())
        {
            // Harfi mevcut texte ekle
            uiTextComponent.text += letter;
            
            // Eğer isterseniz buraya küçük bir daktilo sesi çaldırabilirsiniz
            // AudioManager.Instance.Play("TypeSound");
            
            // Belirlenen saniye kadar bekle, sonraki harfe geç
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}
