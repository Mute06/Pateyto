using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Platform_SceneManager : MonoBehaviour
{
    public static Platform_SceneManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadNextScene()
    {
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            Debug.LogWarning("No next scene found in build settings.");
        }
    }

    public void RealoadAfter(float time)
    {
        StartCoroutine(ReloadAfterTime(time));
    }

    private IEnumerator ReloadAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        ReloadScene();
    }
}
