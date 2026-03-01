using UnityEngine;

public class DoorNextLevel : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null && collision.CompareTag("Player"))
        {
            P_SceneManager.Instance.LoadNextLevelWithFade(1f);
            if (P_SceneManager.Instance.GetCurrentSceneName() == "PlatformerTest")
            {
                PlayerPrefs.SetInt("HardGame", 1);
            }
            else
            {
                P_SceneManager.Instance.LoadNextLevelWithFade(1f);
            }
        }
    }
}
