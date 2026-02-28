using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightFlicker : MonoBehaviour
{
    private Light2D tvLight;
    
    [Header("Titreme (Flicker) Ayarları")]
    [Tooltip("Işığın düşebileceği en düşük parlaklık")]
    public float minIntensity = 0.4f;
    
    [Tooltip("Işığın çıkabileceği en yüksek parlaklık")]
    public float maxIntensity = 0.7f;
    
    [Tooltip("Titreme hızı. Değer ne kadar yüksekse o kadar hızlı titrer.")]
    public float flickerSpeed = 5f;

    // Perlin noise uzağımızdaki rastlantısallığı pürüzsüz yapar
    private float timeOffset;

    void Start()
    {
        tvLight = GetComponent<Light2D>();
        // Her obje kendi rastgele zaman çizgisinde başlasın diye
        timeOffset = Random.Range(0f, 100f); 
    }

    void Update()
    {
        if (tvLight == null) return;

        // PerlinNoise 0 ile 1 arasında pürüzsüz dalgalanan rastgele bir değer üretir.
        // Bunu zamanla ilerleterek (Time.time * hız) yumuşak ama düzensiz bir titreme şekli elde ederiz, tıpkı TV gibi.
        float noise = Mathf.PerlinNoise(timeOffset + Time.time * flickerSpeed, 0f);

        // Elde ettiğimiz 0 ile 1 arasındaki bu değeri, belirttiğimiz Min ve Max parlaklık arasına uyarlıyoruz
        tvLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
    }
}
