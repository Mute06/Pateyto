using UnityEngine;

public class OnTriggerNextScene : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null && collision.CompareTag("Player"))
        {
            P_SceneManager.Instance.LoadNextLevelWithFade(1f);
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
