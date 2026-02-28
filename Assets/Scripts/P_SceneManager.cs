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
}
