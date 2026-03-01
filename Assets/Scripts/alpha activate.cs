using UnityEngine;
using UnityEngine.UI; // Image bileþeni için gerekli

public class AlphaGozlemleyici : MonoBehaviour
{
    [Header("Referanslar")]
    public Image hedefResim;       // Alphasýný kontrol edeceðimiz Image
    public GameObject cikacakYazi; // Görünür olacak Text'in bulunduðu GameObject

    void Update()
    {
        // Eðer Image referansý veya Text objesi atanmadýysa hata vermesin diye kontrol ediyoruz
        if (hedefResim == null || cikacakYazi == null) return;

        // Alpha deðeri 1.0f (arayüzdeki 255) oldu mu?
        // Float (ondalýklý) sayýlarda bazen 0.9999 gibi küsuratlar olabildiði için 
        // tam eþittir (==) yerine >= 0.99f kullanmak her zaman daha güvenlidir.
        if (hedefResim.color.a >= 0.99f)
        {
            cikacakYazi.SetActive(true); // Yazýyý aktif et
        }
        else
        {
            cikacakYazi.SetActive(false); // Alpha 255 deðilse yazýyý gizle
        }
    }
}