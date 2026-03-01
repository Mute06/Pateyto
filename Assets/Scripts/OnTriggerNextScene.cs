using UnityEngine;

public class OnTriggerNextScene : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null && collision.CompareTag("Player"))
        {
            P_SceneManager.Instance.LoadNextLevelWithFade(1f);
        }
    }
}
