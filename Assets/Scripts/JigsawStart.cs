using UnityEngine;
using UnityEngine.Events;

public class JigsawStart : MonoBehaviour
{
    [SerializeField] private GameObject jigSawPrefab;
    public UnityEvent onPuzzleComplete;

    private JigsawPuzzleGame currentPuzzle;
    private GameObject currentjigsaw;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            currentjigsaw = Instantiate(jigSawPrefab);
            currentPuzzle = currentjigsaw.GetComponent<JigsawPuzzleGame>();
            onPuzzleComplete.AddListener(() =>
            {
                Destroy(currentjigsaw);
                P_SceneManager.Instance.LoadNextLevelWithFade(1f);
            });
            currentPuzzle.OnPuzzleComplete = onPuzzleComplete;
        }   
    }
}
