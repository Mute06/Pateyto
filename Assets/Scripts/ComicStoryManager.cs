using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class ComicStoryManager : MonoBehaviour
{
    [Header("Story Panels")]
    public Image[] comicPanels; // Çizgi roman paneli UI objeleri

    [Header("Fade to Black (Son Panel Sonrası)")]
    public Image fadeOverlay; // Tam ekran siyah Image
    public TextMeshProUGUI dreamText; // "…I wish it was just a dream." yazısı
    public float blackFadeDuration = 1.5f; // Siyahlığın fade-in süresi
    public float textFadeDuration = 1.5f; // Yazının fade-in süresi
    public float textDisplayDuration = 3f; // Yazının ekranda kalma süresi
    [TextArea]
    public string dreamMessage = "\u2026I wish it was just a dream.";

    [HideInInspector]
    public string nextSceneName;
    public bool autoPlay = false;
    public float timePerPanel = 3f;
    public float fadeDuration = 0.5f;

    [Header("Panel Sounds")]
    public AudioSource audioSource;
    public AudioClip[] panelSounds;


    private int currentPanelIndex = 0;
    private float timer = 0f;
    private bool isFading = false;
    private bool dreamSequenceStarted = false;

    void Start()
    {

        // Tüm panelleri başlangıçta tamamen şeffaf (görünmez) yap
        foreach (Image panel in comicPanels)
        {
            if (panel != null)
            {
                SetAlpha(panel, 0f);
            }
        }

        // Fade overlay ve dream text'i başlangıçta gizle
        if (fadeOverlay != null) SetAlpha(fadeOverlay, 0f);
        if (dreamText != null) SetTextAlpha(dreamText, 0f);

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
        if (dreamSequenceStarted) return;

        // Bütün paneller gösterildi mi?
        if (currentPanelIndex >= comicPanels.Length)
        {
            if (autoPlay)
            {
                timer += Time.deltaTime;
                if (timer >= timePerPanel)
                {
                    StartDreamSequence();
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
                {
                    StartDreamSequence();
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

    void StartDreamSequence()
    {
        dreamSequenceStarted = true;
        StartCoroutine(DreamSequenceRoutine());
    }

    IEnumerator DreamSequenceRoutine()
    {
        isFading = true;

        // 1) Siyah overlay fade in — tüm ekranı kaplar
        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            float t = 0f;
            while (t < blackFadeDuration)
            {
                t += Time.deltaTime;
                SetAlpha(fadeOverlay, Mathf.Lerp(0f, 1f, t / blackFadeDuration));
                yield return null;
            }
            SetAlpha(fadeOverlay, 1f);
        }

        // Kısa bir bekleme
        yield return new WaitForSeconds(0.5f);

        // 2) Dream text fade in
        if (dreamText != null)
        {
            dreamText.text = dreamMessage;
            dreamText.gameObject.SetActive(true);
            float t = 0f;
            while (t < textFadeDuration)
            {
                t += Time.deltaTime;
                SetTextAlpha(dreamText, Mathf.Lerp(0f, 1f, t / textFadeDuration));
                yield return null;
            }
            SetTextAlpha(dreamText, 1f);
        }

        // 3) Yazının ekranda kalma süresi
        yield return new WaitForSeconds(textDisplayDuration);

        isFading = false;

        // 4) Sahne geçişi
        EndComic();
    }

    IEnumerator FadeInPanel(int index)
    {
        isFading = true;
        Image currentImage = comicPanels[index];

        // 🔊 Panel sesi çal
        if (audioSource != null && panelSounds != null && index < panelSounds.Length)
        {
            if (panelSounds[index] != null)
            {
                audioSource.PlayOneShot(panelSounds[index]);
            }
        }

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

    void SetTextAlpha(TextMeshProUGUI txt, float alpha)
    {
        if (txt == null) return;
        Color c = txt.color;
        c.a = alpha;
        txt.color = c;
    }

    void EndComic()
    {
        Debug.Log("Çizgi roman hikayesi bitti!");
        P_SceneManager.Instance.LoadNextLevel();
    }
}