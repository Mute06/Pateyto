using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ComicStoryManager : MonoBehaviour
{
    [Header("Story Panels")]
    public Image[] comicPanels; // Çizgi roman paneli UI objeleri
    
    [Header("Settings")]
#if UNITY_EDITOR
    [Tooltip("Çizgi roman bitince yüklenecek sahneyi buraya sürükleyin")]
    public UnityEditor.SceneAsset nextScene; // Sahneyi doğrudan sürükleyip bırakmak için
#endif
    [HideInInspector]
    public string nextSceneName; 
    public bool autoPlay = false; 
    public float timePerPanel = 3f; 
    public float fadeDuration = 0.5f; 
    

    
    private int currentPanelIndex = 0;
    private float timer = 0f;
    private bool isFading = false; 

    void Start()
    {
#if UNITY_EDITOR
        if (nextScene != null)
        {
            // SceneAsset nesnesinin barındırdığı asıl adı kesin olarak alıp kaydederiz
            nextSceneName = nextScene.name;
            Debug.Log("Scene Asset algılandı: " + nextSceneName);
        }
#endif

        // Tüm panelleri başlangıçta tamamen şeffaf (görünmez) yap
        foreach (Image panel in comicPanels)
        {
            if (panel != null)
            {
                SetAlpha(panel, 0f);
            }
        }

        if (comicPanels.Length > 0 && comicPanels[0] != null)
        {
            // İlk paneli yavaşça göster (fade in)
            StartCoroutine(FadeInPanel(currentPanelIndex));
        }
        else
        {
            Debug.LogWarning("ComicStoryManager: Lütfen comicPanels dizisine UI Image objelerini ekleyin.");
        }
    }

    void Update()
    {
        // Eğer paneller atanmamışsa veya şu an bir geçiş(fade) yapılıyorsa bekle
        if (comicPanels == null || comicPanels.Length == 0 || isFading) return;

        // Bütün paneller gösterildi mi?
        if (currentPanelIndex >= comicPanels.Length) 
        {
            if (autoPlay)
            {
                timer += Time.deltaTime;
                if (timer >= timePerPanel)
                {
                    EndComic();
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
                {
                    EndComic();
                }
            }
            return;
        }

        if (autoPlay)
        {
            timer += Time.deltaTime;
            if (timer >= timePerPanel)
            {
                timer = 0f;
                TransitionToNextPanel();
            }
        }
        else
        {
            // Tıklandığında sıradaki panele geç
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                TransitionToNextPanel();
            }
        }
    }

    void TransitionToNextPanel()
    {
        if (currentPanelIndex + 1 < comicPanels.Length)
        {
            currentPanelIndex++; // Bir sonraki panele geç
            StartCoroutine(FadeInPanel(currentPanelIndex));
        }
        else
        {
            // Son paneli de gösterdik
            currentPanelIndex++; 
        }
    }

    IEnumerator FadeInPanel(int index)
    {
        isFading = true;
        Image currentImage = comicPanels[index];
        float t = 0f;
        
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            SetAlpha(currentImage, Mathf.Lerp(0f, 1f, t / fadeDuration));
            yield return null;
        }
        SetAlpha(currentImage, 1f);
        isFading = false;
    }

    void SetAlpha(Image img, float alpha)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }

    void EndComic()
{
    Debug.Log("Çizgi roman hikayesi bitti!");

    if (!string.IsNullOrEmpty(nextSceneName))
    {
        SceneManager.LoadScene(nextSceneName);
    }
    else
    {
        Debug.LogWarning("Next Scene boş!");
    }
}

}