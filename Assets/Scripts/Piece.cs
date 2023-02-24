using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DG.Tweening;

public class Piece : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [HideInInspector]
    public ImageScrambler grid;
    [HideInInspector]
    public GameCell gameCell;

    //===========
    float clicked = 0;
    float clicktime = 0;
    float clickdelay = 0.5f;
    //===========

    public Vector3 originalPosition;

    public void OnPointerDown(PointerEventData pointerEventData)
    {
        OnDoubleClick();
    }

    public void OnPointerUp(PointerEventData pointerEventData)
    {

    }

    public bool isMovingLeft = false;
    public bool isMovingUp = false;
    public bool isMovingDown = false;
    public bool isMovingRight = false;

    public void OnDrag(PointerEventData data)
    {
        switch(grid.gameMode)
        {
            case(GameMode.scramble):
                isMovingLeft = false;
                isMovingUp = false;
                isMovingDown = false;
                isMovingRight = false;

                float distanceX = Input.mousePosition.x - initialDragPosition.x;
                float distanceY = Input.mousePosition.y - initialDragPosition.y;

                float positiveX = Mathf.Abs(distanceX);
                float positiveY = Mathf.Abs(distanceY);

                Vector3 newPosition = Vector3.zero;

                if(positiveX > positiveY)
                {
                    newPosition = new Vector3(Input.mousePosition.x, initialDragPosition.y, 1f);
                    
                    if(gameCell.rightCell != null)
                    {
                        if(Input.mousePosition.x > gameCell.rightCell.transform.position.x)
                        {
                            newPosition = gameCell.rightCell.transform.position;
                        }
                    }

                    if(gameCell.leftCell != null)
                    {
                        if(Input.mousePosition.x < gameCell.leftCell.transform.position.x)
                        {
                            newPosition = gameCell.leftCell.transform.position;
                        }
                    }

                    if(distanceX > 0)
                    {
                        isMovingRight = true;
                    }else{
                        isMovingLeft = true;
                    }
                }else{
                    newPosition = new Vector3(initialDragPosition.x, Input.mousePosition.y, 1f);

                    if(gameCell.upperCell != null)
                    {
                        if(Input.mousePosition.y > gameCell.upperCell.transform.position.y)
                        {
                            newPosition = gameCell.upperCell.transform.position;
                        }
                    }

                    if(gameCell.lowerCell != null)
                    {
                        if(Input.mousePosition.y < gameCell.lowerCell.transform.position.y)
                        {
                            newPosition = gameCell.lowerCell.transform.position;
                        }
                    }

                    if(distanceY > 0)
                    {
                        isMovingUp = true;
                    }else{
                        isMovingDown = true;
                    }

                }
                transform.position = newPosition;
            break;

            case(GameMode.jigsaw):
                transform.position = Input.mousePosition;
            break;
        }
    }

    private Vector3 initialDragPosition;
    private Vector3 endDragPosition;

    public void OnBeginDrag(PointerEventData eventData)
    {
        initialDragPosition = transform.position;
        GetComponent<RectTransform>().SetAsLastSibling();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        switch(grid.gameMode)
        {
            case(GameMode.scramble):
                if(isMovingDown)
                {
                    grid.SwapPieces(this, gameCell.lowerCell.pieceOnTop);
                }

                if(isMovingUp)
                {
                    grid.SwapPieces(this, gameCell.upperCell.pieceOnTop);
                }

                if(isMovingLeft)
                {
                    grid.SwapPieces(this, gameCell.leftCell.pieceOnTop);
                }

                if(isMovingRight)
                {
                    grid.SwapPieces(this, gameCell.rightCell.pieceOnTop);
                }
            break;

            case(GameMode.jigsaw):

                bool isInPlace = false; 
                GameCell targetCell = null;

                foreach(GameCell current in grid.allCells)
                {
                    RectTransform rect = current.GetComponent<RectTransform>();
                    isInPlace = IsPositionWithinBounds(Input.mousePosition, rect);

                    if(isInPlace)
                    {
                        targetCell = current;
                        grid.SnapInPlace(this, targetCell);
                        break;
                    }
                }

                if(!isInPlace)
                {
                    transform.DOMove(originalPosition, 0.5f);
                }
            break;
        }
    }

    public void OnDoubleClick()
    {
        clicked++;
        if (clicked == 1) clicktime = Time.time;

        if (clicked > 1 && Time.time - clicktime < clickdelay)
        {
            clicked = 0;
            clicktime = 0;
            
            if(gameCell.leftCell != null)
                gameCell.leftCell.pieceOnTop.GetComponent<Image>().DOColor(Color.red, 0.5f).SetLoops(2, LoopType.Yoyo);

            if(gameCell.rightCell != null)
                gameCell.rightCell.pieceOnTop.GetComponent<Image>().DOColor(Color.red, 0.5f).SetLoops(2, LoopType.Yoyo);

            if(gameCell.upperCell != null)
                gameCell.upperCell.pieceOnTop.GetComponent<Image>().DOColor(Color.red, 0.5f).SetLoops(2, LoopType.Yoyo);

            if(gameCell.lowerCell != null)
                gameCell.lowerCell.pieceOnTop.GetComponent<Image>().DOColor(Color.red, 0.5f).SetLoops(2, LoopType.Yoyo);
        }
        else if (clicked > 2 || Time.time - clicktime > 1) clicked = 0;
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {

    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {

    }

    public bool IsPositionWithinBounds(Vector3 position, RectTransform target)
    {
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);

        float left = corners[0].x;
        float right = corners[2].x;
        float bottom = corners[0].y;
        float top = corners[1].y;

        if (position.x >= left && position.x <= right && position.y >= bottom && position.y <= top)
        {
            return true;
        }

        return false;
    }
}
