using UnityEngine;
using UnityEngine.Events;

public class OnTriggerNextScene : MonoBehaviour
{
    public UnityEvent onSceneLoaded;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null && collision.CompareTag("Player"))
        {
            P_SceneManager.Instance.LoadNextLevelWithFade(1f);
            onSceneLoaded?.Invoke();
            if (P_SceneManager.Instance.GetNextSceneName() == "Level1")
            {
                P_SceneManager.Instance.LoadNextLevelWithFade(1f);
                P_SceneManager.Instance.LoadNextLevelWithFade(1f);
            }
            else
            {
                P_SceneManager.Instance.LoadNextLevelWithFade(1f);
            }
            
        }
    }
}
