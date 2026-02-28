using UnityEngine;
using System.Collections;

public partial class ObjectFader : MonoBehaviour
{
    [Header("Ayarlar")]
    [Tooltip("Kaybolma hızı")]
    public float fadeSpeed = 1f;

    [Tooltip("İşlem bitince obje tamamen kapansın mı?")]
    public bool disableAfterFade = true;

    private Renderer objRenderer;
    private Color originalColor;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        objRenderer = GetComponent<Renderer>();
        if (objRenderer != null)
        {
            // Materyalin orijinal rengini al
            originalColor = objRenderer.material.color;
        }
    }

    // Dışarıdan çağırmak için ana fonksiyon
    public void StartFadeOut()
    {
        if (objRenderer == null) return;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        Color currentColor = objRenderer.material.color;

        // Alpha değeri 0'a yaklaşana kadar devam et
        while (currentColor.a > 0.01f)
        {
            // Alpha değerini zamanla azalt
            currentColor.a -= fadeSpeed * Time.deltaTime;

            // Yeni rengi materyale uygula
            objRenderer.material.color = currentColor;

            yield return null;
        }

        // Tam sıfır yap
        currentColor.a = 0f;
        objRenderer.material.color = currentColor;

        if (disableAfterFade)
        {
            gameObject.SetActive(false);
        }
    }

    // Objeyi tekrar görünür yapmak istersen:
    public void ResetVisibility()
    {
        if (objRenderer == null) return;
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        objRenderer.material.color = originalColor;
        gameObject.SetActive(true);
    }
}