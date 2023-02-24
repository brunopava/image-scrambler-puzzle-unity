using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public enum GameMode
{
    scramble,
    jigsaw
}

public class ImageScrambler : MonoBehaviour
{
    public GameMode gameMode;

    public Texture2D sourceImage;

    public int gridSizeX;
    public int gridSizeY;

    public Image imageContainer;
    public Image cellsContainer;

    private int pieceWidth;
    private int pieceHeight;

    public List<List<GameCell>> cellRows;
    public List<List<GameCell>> cellColumns;
    public List<GameCell> allCells;

    public List<Piece> pieces;

    public Image piecesContainer;

    private void Start()
    {
        LoadImage(sourceImage);
        ShuffleCells();

        if(gameMode == GameMode.jigsaw)
        {
            DistributeImages(pieces);
        }
    }

    public void LoadImage(Texture2D source)
    {
        pieces = new List<Piece>();
        cellRows = new List<List<GameCell>>();
        cellColumns = new List<List<GameCell>>();
        allCells = new List<GameCell>();

        pieceWidth = source.width / gridSizeX;
        pieceHeight = source.height / gridSizeY;

        RectTransform parentContainter = imageContainer.GetComponent<RectTransform>();

        Vector3 offset = new Vector3((parentContainter.sizeDelta.x / 2f) - pieceWidth/2, - (parentContainter.sizeDelta.y / 2f) + pieceHeight/2, 0f);

        for (int i = 0; i < gridSizeX; i++)
        {
            cellRows.Add(new List<GameCell>());

            for (int j = 0; j < gridSizeY; j++)
            {
                Texture2D pieceTexture = new Texture2D(pieceWidth, pieceHeight);
                Color[] pixels = source.GetPixels(i * pieceWidth, j * pieceHeight, pieceWidth, pieceHeight);
                pieceTexture.SetPixels(pixels);
                pieceTexture.Apply();

                Sprite pieceSprite = Sprite.Create(pieceTexture, new Rect(0, 0, pieceWidth, pieceHeight), new Vector2(0.5f, 0.5f));

                string pieceName = string.Format("Piece_{0}_{1}",i,j);
                GameObject pieceGO = new GameObject(pieceName);
                Image pieceImage = pieceGO.AddComponent<Image>();
                Piece piece = pieceGO.AddComponent<Piece>();
                piece.grid = this;
                pieces.Add(piece);
                
                pieceImage.GetComponent<RectTransform>().sizeDelta = new Vector2(pieceWidth, pieceHeight);
                pieceImage.sprite = pieceSprite;
                pieceImage.transform.SetParent(imageContainer.transform, false);

                float posX = -offset.x + (i * pieceWidth);
                float posY = offset.y + (j * pieceHeight);

                if(posX < 0)
                {
                    posX = Mathf.Ceil(posX);
                }else{
                    posX = Mathf.Floor(posX);
                }

                pieceImage.rectTransform.anchoredPosition = new Vector2(posX, posY);

                piece.gameCell = CreateGameCell(i, j, pieceImage);
            }
        }

        for(int k = 0; k < cellRows[0].Count; k++)
        {
            cellColumns.Add(new List<GameCell>());

            for(int l = 0; l < cellRows.Count; l++)
            {
                cellColumns[k].Add(cellRows[l][k]);

                if (k - 1 >= 0)
                {
                    cellRows[l][k].lowerCell = cellRows[l][k - 1];
                }
     
                if (k + 1 < cellRows[0].Count)
                {
                    cellRows[l][k].upperCell = cellRows[l][k +1];
                }
      
                if (l - 1 >= 0)
                {
                    cellRows[l][k].leftCell = cellRows[l - 1][k];
                }
                
                if (l + 1 < cellRows.Count)
                {
                    cellRows[l][k].rightCell = cellRows[l + 1][k];
                }
            }
        }
    }

    public GameCell CreateGameCell(int rowIndex, int columIndex,Image current)
    {
        string cellName = string.Format("GameCell_{0}_{1}", rowIndex, columIndex);

        GameObject piece = new GameObject(cellName);

        GameCell cell = piece.AddComponent<GameCell>();

        Image pieceImage = piece.AddComponent<Image>();

        Color randomColor = new Color(Random.value, Random.value, Random.value, 1.0f);
        pieceImage.color = randomColor;

        RectTransform rect = pieceImage.GetComponent<RectTransform>();

        rect.sizeDelta = current.rectTransform.sizeDelta;

        piece.transform.SetParent(cellsContainer.transform);

        rect.anchoredPosition = current.rectTransform.anchoredPosition;

        cellRows[rowIndex].Add(cell);
        allCells.Add(cell);

        cell.correctPiece = current.GetComponent<Piece>();

        cell.grid = this;

        return cell;
    }

    public void SwapPieces(Piece from, Piece to)
    {
        GameCell fromCell = from.gameCell;
        GameCell toCell = to.gameCell;

        Vector2 fromPosition = fromCell.transform.position;
        Vector2 toPosition = toCell.transform.position;

        from.transform.DOMove(toPosition, 0.5f);
        to.transform.DOMove(fromPosition, 0.5f);

        fromCell.pieceOnTop = to;
        toCell.pieceOnTop = from;

        from.gameCell = toCell;
        to.gameCell = fromCell;

        CheckForVictory();
    }

    public void ShuffleCells()
    {
        List<Piece> shuffledList = pieces.OrderBy( x => Random.value ).ToList( );

        int index = 0;
        foreach(List<GameCell> currentList in cellRows)
        {
            foreach(GameCell currentCell in currentList)
            {
                Piece currentPiece = shuffledList[index];

                currentPiece.gameCell = currentCell;
                currentCell.pieceOnTop = currentPiece;

                currentPiece.transform.position = currentCell.transform.position;

                index++;
            }
        }
    }

    public void SnapInPlace(Piece target, GameCell place)
    {
        target.transform.DOMove(place.transform.position, 0.5f);

        if(place.pieceOnTop != null && place.pieceOnTop != target)
        {
            place.pieceOnTop.transform.DOMove(place.pieceOnTop.originalPosition, 0.5f);
            place.pieceOnTop = null;
        }

        target.gameCell = place;
        place.pieceOnTop = target;

        foreach(GameCell current in allCells)
        {
            if(current.pieceOnTop == target && current != place)
            {
                current.pieceOnTop = null;
            }
        }

        CheckForVictory();
    }

    public void CheckForVictory()
    {
        bool hasWon = true;
        foreach(List<GameCell> currentList in cellRows)
        {
            foreach(GameCell currentCell in currentList)
            {
                if(currentCell.pieceOnTop != currentCell.correctPiece)
                {
                    hasWon = false;
                    break;
                }
            }
        }

        if(hasWon)
        {
            Debug.Log("VICTORY");
        }
    }

    private void DistributeImages(List<Piece> imagesToDistribute)
    {
        RectTransform targetTransform = piecesContainer.rectTransform;
        Rect targetBounds = targetTransform.rect;
        
        foreach (Piece image in imagesToDistribute)
        {
            RectTransform imageTransform = image.GetComponent<RectTransform>();
            Rect imageBounds = imageTransform.rect;

            image.transform.SetParent(piecesContainer.transform);
            
            // Calculate random position within target bounds
            float x = Random.Range(targetBounds.xMin + imageBounds.width / 2, targetBounds.xMax - imageBounds.width / 2);
            float y = Random.Range(targetBounds.yMin + imageBounds.height / 2, targetBounds.yMax - imageBounds.height / 2);
            
            // Set image position
            imageTransform.DOLocalMove(new Vector3(x, y, imageTransform.position.z),0.5f).SetDelay(1f);

            image.originalPosition = imageTransform.position;
        }

        foreach(GameCell current in allCells)
        {
            current.pieceOnTop = null;
        }
    }
}