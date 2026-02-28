using UnityEngine;

public class DoorNextLevel : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null && collision.CompareTag("Player"))
        {
            Platform_SceneManager.Instance.LoadNextScene();
        }
    }
}
