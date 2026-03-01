using UnityEngine;

public class JigsawStart : MonoBehaviour
{
    [SerializeField] private GameObject jigSawPrefab;

    private JigsawPuzzleGame currentPuzzle;
    private GameObject currentJigsaw;
    private bool puzzleStarted = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !puzzleStarted)
        {
            puzzleStarted = true;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            currentJigsaw = Instantiate(jigSawPrefab);
            Time.timeScale = 1f;

            currentPuzzle = currentJigsaw.GetComponent<JigsawPuzzleGame>();

            currentPuzzle.OnPuzzleComplete.AddListener(PuzzleFinished);
        }
    }

    private void PuzzleFinished()
    {
        Destroy(currentJigsaw);
        P_SceneManager.Instance.LoadNextLevelWithFade(1f);
    }
}