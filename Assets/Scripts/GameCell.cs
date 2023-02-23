using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCell : MonoBehaviour
{
    public ImageScrambler grid;

    public Piece pieceOnTop;
    public Piece correctPiece;

    public Vector2 cellPosition;

    public GameCell leftCell;
    public GameCell rightCell;
    public GameCell upperCell;
    public GameCell lowerCell;
}
