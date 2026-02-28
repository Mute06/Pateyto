using UnityEngine;

/// <summary>
/// Lightweight data component attached to each jigsaw piece.
/// Stores the grid indices and the correct local position so the
/// manager can check snap distance without look-ups.
/// </summary>
public class JigsawPuzzlePiece : MonoBehaviour
{
    [HideInInspector] public int col;
    [HideInInspector] public int row;
    [HideInInspector] public Vector2 correctLocalPosition;
    [HideInInspector] public bool isPlaced = false;
}
