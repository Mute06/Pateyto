using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class JigsawManager : MonoBehaviour
{
    [Tooltip("Difficulty is the number of pieces on the shortest texture dimension")]
    [SerializeField] public int difficulty = 4;
    [Range(0.05f,0.5f)] [SerializeField] private float borderThickness = 0.1f;
    [SerializeField] private float snapDifficulty = 2;

    [SerializeField] private Transform gameHolder;
    [SerializeField] private Transform piecePrefab;
    [SerializeField] private GameObject completeButton;

    // --- Decoupling Events ---
    [Space(10)]
    public UnityEvent OnPuzzleComplete;
    public UnityEvent OnPuzzleExit;
    // -------------------------

    private List<Transform> pieces;
    private Vector2Int dimensions;
    private float width;
    private float height;
    private Transform draggingPiece = null;
    private Vector3 offset;
    private int piecesCorrect = 0;
    private bool isPlaying = false;

    public void StartGame(Texture2D jigsawTexture)
    {
        pieces = new List<Transform>();
        dimensions = GetDimensions(jigsawTexture, difficulty);
        CreateJigsawPieces(jigsawTexture);
        Scatter();
        UpdateBorder();
        
        piecesCorrect = 0;
        isPlaying = true;
    }

    private void Update()
    {
        if (!isPlaying) return;
        
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit)
            {
                draggingPiece = hit.transform;
                offset = draggingPiece.position - mousePos;
                offset += Vector3.back;
            }
        }

        if (draggingPiece && Input.GetMouseButtonUp(0))
        {
            draggingPiece.position += Vector3.forward;
            SnapAndDisableIfCorrect();
            draggingPiece = null;
        }

        if (draggingPiece)
        {
            Vector3 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            newPosition += offset;
            draggingPiece.position = newPosition;
        }
    }

    public void ExitGame()
    {
        // Broadcast that the puzzle was exited
        OnPuzzleExit?.Invoke();

        if(transform.parent != null)
            Destroy(transform.parent.gameObject);
        else
            Destroy(gameObject);
    }
    
    private void SnapAndDisableIfCorrect()
    {
        int pieceIndex = pieces.IndexOf(draggingPiece);
        if(pieceIndex == -1) return;
        
        int col = pieceIndex % dimensions.x;
        int row = pieceIndex / dimensions.x;
        
        Vector2 targetPosition = new Vector2()
        {
            x = (-width * dimensions.x / 2) + (col * width) + (width / 2),
            y = (-height * dimensions.y / 2) + (row * height) + (height / 2)
        };

        if (Vector2.Distance(draggingPiece.localPosition, targetPosition) <= (width / snapDifficulty))
        {
            draggingPiece.localPosition = targetPosition;
            draggingPiece.GetComponent<BoxCollider2D>().enabled = false;
            piecesCorrect++;

            if (piecesCorrect >= pieces.Count)
            {
                completeButton?.SetActive(true);
                // Broadcast that the puzzle has been successfully finished
                OnPuzzleComplete?.Invoke();
            }
        }
    }

    private void CreateJigsawPieces(Texture2D texture)
    {
        float aspectRatio = (float)texture.width / texture.height;
        height = 1f / dimensions.y;
        width = aspectRatio / dimensions.x;

        for (int row = 0; row < dimensions.y; row++)
        {
            for (int col = 0; col < dimensions.x; col++)
            {
                Transform piece = Instantiate(piecePrefab, gameHolder);
                piece.localPosition = new Vector3
                {
                    x = (-width * dimensions.x / 2) + (col * width) + (width / 2),
                    y = (-height * dimensions.y / 2) + (row * height) + (height / 2)
                };
                piece.localScale = new Vector3(width, height, 1f);
                piece.name = $"Piece {col},{row}";

                pieces.Add(piece);

                // Ensure the piece has exactly a Quad mesh so we don't encounter UV bounds errors.
                Mesh mesh = new Mesh();
                mesh.vertices = new Vector3[]
                {
                    new Vector3(-0.5f, -0.5f, 0),
                    new Vector3(0.5f, -0.5f, 0),
                    new Vector3(-0.5f, 0.5f, 0),
                    new Vector3(0.5f, 0.5f, 0)
                };
                mesh.triangles = new int[] { 0, 2, 1, 2, 3, 1 };
                mesh.normals = new Vector3[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back };

                float width1 = 1f / dimensions.x;
                float height1 = 1f / dimensions.y;
                Vector2[] uv = new Vector2[4];
                uv[0] = new Vector2(width1 * col, height1 * row);
                uv[1] = new Vector2(width1 * (col + 1), height1 * row);
                uv[2] = new Vector2(width1 * col, height1 * (row + 1));
                uv[3] = new Vector2(width1 * (col + 1), height1 * (row + 1));
                
                mesh.uv = uv;
                piece.GetComponent<MeshFilter>().mesh = mesh;
                
                Material mat = piece.GetComponent<MeshRenderer>().material;
                if(mat.shader.name == "Universal Render Pipeline/Lit")
                {
                     mat.shader = Shader.Find("Universal Render Pipeline/Unlit");
                }
                mat.SetTexture("_BaseMap", texture); // URP uses _BaseMap
                mat.SetTexture("_MainTex", texture); // Built-in uses _MainTex
            }
        }
    }

    private Vector2Int GetDimensions(Texture2D texture, int difficulty)
    {
        Vector2Int dim = Vector2Int.zero;
        if (texture.width < texture.height)
        {
            dim.x = difficulty;
            dim.y = difficulty * (texture.height / texture.width); // Fixed division logic
        }
        else
        {
            dim.x = difficulty * (texture.width / texture.height); // Fixed division logic
            dim.y = difficulty;
        }
        return dim;
    }

    private void Scatter()
    {
        float orthoHeight = Camera.main.orthographicSize;
        float screenAspect = (float)Screen.width / Screen.height;
        float orthoWidth = (screenAspect * orthoHeight);

        float pieceWidth = width * gameHolder.localScale.x;
        float pieceHeight = height * gameHolder.localScale.y;

        orthoHeight -= pieceHeight;
        orthoWidth -= pieceWidth;

        foreach (Transform piece in pieces)
        {
            float x = Random.Range(-orthoWidth, orthoWidth);
            float y = Random.Range(-orthoHeight, orthoHeight);
            piece.position = new Vector3(x, y, -1);
        }
    }

    public void ClearPuzzle()
    {
        foreach (var piece in pieces)
        {
            if(piece != null) Destroy(piece.gameObject);
        }
        pieces.Clear();
        isPlaying = false;
    }
    
    private void UpdateBorder()
    {
        LineRenderer line = gameHolder.GetComponent<LineRenderer>();
        if (line == null) return;
        
        float halfWidth = (width * dimensions.x) / 2f;
        float halfHeight = (height * dimensions.y) / 2f;
        float borderZ = 0f;

        line.positionCount = 4; // Fix for "index out of bounds" error
        line.loop = true; // Connects the last point to the first

        line.SetPosition(0, new Vector3(-halfWidth, halfHeight,borderZ));
        line.SetPosition(1, new Vector3(halfWidth, halfHeight, borderZ));
        line.SetPosition(2, new Vector3(halfWidth, -halfHeight, borderZ));
        line.SetPosition(3, new Vector3(-halfWidth, -halfHeight, borderZ));

        line.startWidth = borderThickness;
        line.endWidth = borderThickness;
        line.enabled = true;
    }
}
