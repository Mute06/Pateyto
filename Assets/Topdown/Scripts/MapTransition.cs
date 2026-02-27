using UnityEngine;
using Unity.Cinemachine;

public class MapTransition : MonoBehaviour
{
    [SerializeField] private Collider2D mapBoundary;
    [SerializeField] private Dirction dirction;
    [SerializeField] private float playerPositionOffset = 1f;
    CinemachineConfiner2D confiner;


    enum Dirction {         
        Up,
        Down,
        Left,
        Right
    };

    private void Awake()
    {
        confiner = FindFirstObjectByType<CinemachineConfiner2D>();

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && mapBoundary != null)
        {
            confiner.BoundingShape2D = mapBoundary;
            UpdatePlayerPosition(collision.gameObject);
        }
    }

    private void UpdatePlayerPosition(GameObject player)
    {
        Vector3 newPos = player.transform.position;
        switch (dirction)
        {
            case Dirction.Up:
                newPos.y += playerPositionOffset;
                break;
            case Dirction.Down:
                newPos.y -= playerPositionOffset;
                break;
            case Dirction.Left:
                newPos.x -= playerPositionOffset;
                break;
            case Dirction.Right:
                newPos.x += playerPositionOffset;
                break;
        }

        player.transform.position = newPos;
    }
}
