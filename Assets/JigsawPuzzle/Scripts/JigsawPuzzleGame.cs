using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary>
/// Self-contained jigsaw puzzle game.
/// Instantiate the prefab (or place in a scene) and it auto-starts.
/// Uses the new Input System via inline InputAction fields.
/// </summary>
public class JigsawPuzzleGame : MonoBehaviour
{
    [Header("Puzzle Settings")]
    [Tooltip("The image to cut into pieces. Must have Read/Write enabled.")]
    [SerializeField] private Texture2D puzzleTexture;

    [Tooltip("Number of pieces along the shortest texture axis.")]
    [SerializeField] private int difficulty = 4;

    [Tooltip("How close a piece must be (as a fraction of piece width) to snap.")]
    [Range(1f, 10f)]
    [SerializeField] private float snapDifficulty = 2f;

    [Tooltip("How much of the screen the assembled puzzle should fill. Higher = bigger pieces.")]
    [Range(0.2f, 1f)]
    [SerializeField] private float screenCoverage = 0.6f;

    [Header("References")]
    [SerializeField] private Transform gameHolder;
    [SerializeField] private Transform piecePrefab;
    [SerializeField] private Camera puzzleCamera;

    [Header("Border")]
    [Range(0.02f, 0.3f)]
    [SerializeField] private float borderThickness = 0.08f;

    [Header("Events")]
    public UnityEvent OnPuzzleComplete;
    public UnityEvent OnPuzzleExit;

    // ── New Input System ──
    [Header("Input (New Input System)")]
    [SerializeField] private InputAction clickAction = new InputAction("Click", InputActionType.Button, "<Mouse>/leftButton");
    [SerializeField] private InputAction pointAction = new InputAction("Point", InputActionType.Value, "<Mouse>/position");

    // ── Runtime state ──
    private readonly List<JigsawPuzzlePiece> pieces = new List<JigsawPuzzlePiece>();
    private Vector2Int dimensions;
    private float pieceWidth;
    private float pieceHeight;
    private JigsawPuzzlePiece draggingPiece;
    private Vector2 dragOffset;   // xy only — z is handled separately
    private int piecesCorrect;
    private bool isPlaying;

    // ───────────────────────────────────────────────
    // Lifecycle
    // ───────────────────────────────────────────────

    private void Awake()
    {
        // Add touch bindings so it works on mobile too
        clickAction.AddBinding("<Touchscreen>/primaryTouch/tap");
        pointAction.AddBinding("<Touchscreen>/primaryTouch/position");

        if (puzzleTexture != null)
        {
            StartGame(puzzleTexture);
        }
    }

    private void OnEnable()
    {
        clickAction.Enable();
        pointAction.Enable();

        clickAction.started += OnClickStarted;
        clickAction.canceled += OnClickCanceled;
    }

    private void OnDisable()
    {
        clickAction.started -= OnClickStarted;
        clickAction.canceled -= OnClickCanceled;

        clickAction.Disable();
        pointAction.Disable();
    }

    // ───────────────────────────────────────────────
    // Public API
    // ───────────────────────────────────────────────

    /// <summary>Start (or restart) the puzzle with the given texture.</summary>
    public void StartGame(Texture2D texture)
    {
        ClearPuzzle();

        puzzleTexture = texture;
        dimensions = GetDimensions(texture, difficulty);
        CreatePieces(texture);
        AutoFitCamera();   // Size the camera so pieces are clearly visible
        Scatter();
        UpdateBorder();

        piecesCorrect = 0;
        isPlaying = true;
    }

    /// <summary>Destroy all pieces and reset.</summary>
    public void ClearPuzzle()
    {
        foreach (var p in pieces)
        {
            if (p != null) Destroy(p.gameObject);
        }
        pieces.Clear();
        isPlaying = false;
        draggingPiece = null;
    }

    /// <summary>Exit / close the puzzle.</summary>
    public void ExitGame()
    {
        OnPuzzleExit?.Invoke();

        if (transform.parent != null)
            Destroy(transform.parent.gameObject);
        else
            Destroy(gameObject);
    }

    // ───────────────────────────────────────────────
    // Input callbacks (New Input System)
    // ───────────────────────────────────────────────

    /// <summary>Convert screen position to a world-space XY point on the puzzle plane (z=0).</summary>
    private Vector2 ScreenToWorld(Camera cam)
    {
        Vector2 screenPos = pointAction.ReadValue<Vector2>();
        Vector3 wp = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -cam.transform.position.z));
        return new Vector2(wp.x, wp.y);
    }

    private void OnClickStarted(InputAction.CallbackContext ctx)
    {
        if (!isPlaying) return;

        Camera cam = GetCamera();
        Vector2 worldXY = ScreenToWorld(cam);

        RaycastHit2D hit = Physics2D.Raycast(worldXY, Vector2.zero);
        if (hit.collider != null)
        {
            var piece = hit.transform.GetComponent<JigsawPuzzlePiece>();
            if (piece != null && !piece.isPlaced)
            {
                draggingPiece = piece;
                // Store xy offset between piece center and cursor
                dragOffset = (Vector2)draggingPiece.transform.position - worldXY;
                // Bring piece in front of others while dragging
                var pos = draggingPiece.transform.position;
                pos.z = -2f;
                draggingPiece.transform.position = pos;
            }
        }
    }

    private void OnClickCanceled(InputAction.CallbackContext ctx)
    {
        if (draggingPiece == null) return;

        // Put piece back on the normal z-plane
        var pos = draggingPiece.transform.position;
        pos.z = -1f;
        draggingPiece.transform.position = pos;

        TrySnapPiece(draggingPiece);
        draggingPiece = null;
    }

    private void Update()
    {
        if (!isPlaying || draggingPiece == null) return;

        Camera cam = GetCamera();
        Vector2 worldXY = ScreenToWorld(cam);
        Vector2 final2D = worldXY + dragOffset;
        draggingPiece.transform.position = new Vector3(final2D.x, final2D.y, -2f);
    }

    // ───────────────────────────────────────────────
    // Piece creation
    // ───────────────────────────────────────────────

    private void CreatePieces(Texture2D texture)
    {
        float aspectRatio = (float)texture.width / texture.height;
        pieceHeight = 1f / dimensions.y;
        pieceWidth = aspectRatio / dimensions.x;

        for (int row = 0; row < dimensions.y; row++)
        {
            for (int col = 0; col < dimensions.x; col++)
            {
                Transform piece = Instantiate(piecePrefab, gameHolder);

                Vector3 correctPos = new Vector3(
                    (-pieceWidth * dimensions.x / 2f) + (col * pieceWidth) + (pieceWidth / 2f),
                    (-pieceHeight * dimensions.y / 2f) + (row * pieceHeight) + (pieceHeight / 2f),
                    0f
                );

                piece.localPosition = correctPos;
                piece.localScale = new Vector3(pieceWidth, pieceHeight, 1f);
                piece.name = $"Piece {col},{row}";

                // Attach data component
                var data = piece.GetComponent<JigsawPuzzlePiece>();
                if (data == null) data = piece.gameObject.AddComponent<JigsawPuzzlePiece>();
                data.col = col;
                data.row = row;
                data.correctLocalPosition = correctPos;
                pieces.Add(data);

                // Build a quad mesh with correct UVs for this piece
                Mesh mesh = new Mesh();
                mesh.vertices = new Vector3[]
                {
                    new Vector3(-0.5f, -0.5f, 0),
                    new Vector3( 0.5f, -0.5f, 0),
                    new Vector3(-0.5f,  0.5f, 0),
                    new Vector3( 0.5f,  0.5f, 0)
                };
                mesh.triangles = new int[] { 0, 2, 1, 2, 3, 1 };
                mesh.normals = new Vector3[]
                {
                    Vector3.back, Vector3.back, Vector3.back, Vector3.back
                };

                float u0 = (float)col / dimensions.x;
                float u1 = (float)(col + 1) / dimensions.x;
                float v0 = (float)row / dimensions.y;
                float v1 = (float)(row + 1) / dimensions.y;
                mesh.uv = new Vector2[]
                {
                    new Vector2(u0, v0),
                    new Vector2(u1, v0),
                    new Vector2(u0, v1),
                    new Vector2(u1, v1)
                };

                piece.GetComponent<MeshFilter>().mesh = mesh;

                // Set texture on material (supports both URP and Built-in)
                Material mat = piece.GetComponent<MeshRenderer>().material;
                if (mat.shader.name == "Universal Render Pipeline/Lit")
                    mat.shader = Shader.Find("Universal Render Pipeline/Unlit");

                mat.SetTexture("_BaseMap", texture);
                mat.SetTexture("_MainTex", texture);
            }
        }
    }

    // ───────────────────────────────────────────────
    // Snapping
    // ───────────────────────────────────────────────

    private void TrySnapPiece(JigsawPuzzlePiece piece)
    {
        float threshold = pieceWidth / snapDifficulty;
        Vector2 current = piece.transform.localPosition;
        Vector2 target = piece.correctLocalPosition;

        if (Vector2.Distance(current, target) <= threshold)
        {
            piece.transform.localPosition = (Vector3)target;
            piece.GetComponent<BoxCollider2D>().enabled = false;
            piece.isPlaced = true;
            piecesCorrect++;

            if (piecesCorrect >= pieces.Count)
            {
                isPlaying = false;
                Debug.Log("[JigsawPuzzle] Puzzle Complete!");
                OnPuzzleComplete?.Invoke();
            }
        }
    }

    // ───────────────────────────────────────────────
    // Scatter
    // ───────────────────────────────────────────────

    private void Scatter()
    {
        Camera cam = GetCamera();
        float orthoH = cam.orthographicSize;
        float orthoW = orthoH * cam.aspect;

        // Keep pieces a half-piece inside the screen edges
        float pw = pieceWidth * 0.5f;
        float ph = pieceHeight * 0.5f;

        float minX = -orthoW + pw;
        float maxX =  orthoW - pw;
        float minY = -orthoH + ph;
        float maxY =  orthoH - ph;

        foreach (var piece in pieces)
        {
            float x = Random.Range(minX, maxX);
            float y = Random.Range(minY, maxY);
            piece.transform.position = new Vector3(x, y, -1f);
        }
    }

    /// <summary>Fit the camera so the full puzzle + scatter margin is visible.</summary>
    private void AutoFitCamera()
    {
        Camera cam = GetCamera();
        if (cam == null) return;

        float totalW = pieceWidth  * dimensions.x;
        float totalH = pieceHeight * dimensions.y;

        // Use screenCoverage to determine camera scale
        float neededH = totalH / screenCoverage;
        float neededW = totalW / screenCoverage;
        float orthoForW = neededW / (2f * cam.aspect);
        float orthoForH = neededH / 2f;

        cam.orthographicSize = Mathf.Max(orthoForH, orthoForW, 0.5f);
    }

    // ───────────────────────────────────────────────
    // Border
    // ───────────────────────────────────────────────

    private void UpdateBorder()
    {
        LineRenderer line = gameHolder.GetComponent<LineRenderer>();
        if (line == null) return;

        float halfW = (pieceWidth * dimensions.x) / 2f;
        float halfH = (pieceHeight * dimensions.y) / 2f;

        line.positionCount = 4;
        line.loop = true;
        line.SetPosition(0, new Vector3(-halfW,  halfH, 0f));
        line.SetPosition(1, new Vector3( halfW,  halfH, 0f));
        line.SetPosition(2, new Vector3( halfW, -halfH, 0f));
        line.SetPosition(3, new Vector3(-halfW, -halfH, 0f));

        line.startWidth = borderThickness;
        line.endWidth = borderThickness;
        line.enabled = true;
    }

    // ───────────────────────────────────────────────
    // Helpers
    // ───────────────────────────────────────────────

    private Vector2Int GetDimensions(Texture2D tex, int diff)
    {
        Vector2Int dim = Vector2Int.zero;
        if (tex.width < tex.height)
        {
            dim.x = diff;
            dim.y = diff * (tex.height / tex.width);
        }
        else
        {
            dim.x = diff * (tex.width / tex.height);
            dim.y = diff;
        }
        // Ensure at least 1 in each dimension
        dim.x = Mathf.Max(dim.x, 1);
        dim.y = Mathf.Max(dim.y, 1);
        return dim;
    }

    private Camera GetCamera()
    {
        if (puzzleCamera != null) return puzzleCamera;
        return Camera.main;
    }
}
