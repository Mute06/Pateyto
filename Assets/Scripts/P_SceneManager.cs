using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


public class P_SceneManager : MonoBehaviour
{
    private static P_SceneManager _instance;

    public static P_SceneManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("P_SceneManager");
                _instance = go.AddComponent<P_SceneManager>();
                DontDestroyOnLoad(go);
            }

            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Immediate ────────────────────────────────────────────────

    public void LoadNextLevel()
    {
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(nextIndex);
        else
            Debug.LogWarning("P_SceneManager: No next scene found in build settings.");
    }

    public void LoadLevelIndex(int index)
    {
        if (index >= 0 && index < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(index);
        else
            Debug.LogWarning($"P_SceneManager: Scene index {index} is out of range.");
    }

    // ── Delayed (no StartCoroutine needed at the call site) ──────

    public void LoadNextLevel(float delay)
    {
        StartCoroutine(LoadNextLevelRoutine(delay));
    }

    public void LoadLevelIndex(int index, float delay)
    {
        StartCoroutine(LoadLevelIndexRoutine(index, delay));
    }

    // ── Fade Transitions (Code Only) ─────────────────────────────

    public void LoadNextLevelWithFade(float fadeDuration)
    {
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextIndex < SceneManager.sceneCountInBuildSettings)
            StartCoroutine(FadeAndLoadRoutine(nextIndex, fadeDuration));
        else
            Debug.LogWarning("P_SceneManager: No next scene found in build settings to fade into.");
    }

    public void LoadLevelIndexWithFade(int index, float fadeDuration)
    {
        if (index >= 0 && index < SceneManager.sceneCountInBuildSettings)
            StartCoroutine(FadeAndLoadRoutine(index, fadeDuration));
        else
            Debug.LogWarning($"P_SceneManager: Scene index {index} is out of range to fade into.");
    }

    public void ReloadLevelWithFade(float duration)
    {
        StartCoroutine(FadeAndLoadRoutine(SceneManager.GetActiveScene().buildIndex, duration));
    }

    // ── Coroutines ───────────────────────────────────────────────

    private IEnumerator LoadNextLevelRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadNextLevel();
    }

    private IEnumerator LoadLevelIndexRoutine(int index, float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadLevelIndex(index);
    }

    private IEnumerator FadeAndLoadRoutine(int sceneIndex, float duration)
    {
        // 1. Create a dynamic Canvas
        GameObject canvasGO = new GameObject("FadeCanvas");
        canvasGO.transform.SetParent(transform); // Attach to P_SceneManager so it persists via DontDestroyOnLoad
        
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // Ensure it renders on top of everything
        
        canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>(); // Optional, but blocks interactions during fade

        // 2. Create the Image object as a child of the Canvas
        GameObject imageGO = new GameObject("BlackImage");
        imageGO.transform.SetParent(canvasGO.transform, false);
        
        UnityEngine.UI.Image fadeImage = imageGO.AddComponent<UnityEngine.UI.Image>();
        fadeImage.color = new Color(0, 0, 0, 0); // Start clear
        
        // Stretch the image to cover the whole screen
        RectTransform rectTransform = fadeImage.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        // 3. Fade Out (to black)
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / duration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, 1);

        // 4. Load the scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 5. Fade In (from black back to clear)
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(elapsed / duration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // 6. Cleanup the dynamic Canvas
        Destroy(canvasGO);
    }
}
