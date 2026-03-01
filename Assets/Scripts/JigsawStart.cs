using UnityEngine;
using UnityEngine.Events;

public class JigsawStart : MonoBehaviour
{
    [SerializeField] private GameObject jigSawPrefab;
    public UnityEvent OnPuzzleStart;


    private JigsawPuzzleGame currentPuzzle;
    private GameObject currentJigsaw;
    private bool puzzleStarted = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !puzzleStarted)
        {
            OnPuzzleStart?.Invoke();

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
        StartLoadAfter(3f);
    }

    private void LoadNextScene()
    {
        P_SceneManager.Instance.LoadNextLevelWithFade(2f);
    }

    private void StartLoadAfter(float delay)
    {
        Invoke(nameof(LoadNextScene), delay);
    }
}