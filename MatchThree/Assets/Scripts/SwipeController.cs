using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using GG.Infrastructure.Utils.Swipe;


public class SwipeController : MonoBehaviour
{
    // dependencies
    SwipeListener swipeListener;
    TileSelector tileSelector;
    [SerializeField] MatchChecker matchChecker;
    [SerializeField] InputManager inputManager;

    
    
    SwipeType swipeType = SwipeType.none;
    Tile selectedTile = null;
    bool isValidMove;
    Coroutine dropMoveCoroutine;
    [SerializeField] float forbiddenSwipeAttemptEffectStrength = .1f;
    [SerializeField] float forbiddenSwipeAttemptEffectDuration = .1f;
    


    void Awake()
    {
        swipeListener = GetComponent<SwipeListener>();
        tileSelector = GetComponent<TileSelector>();
    }

    #region RegisterEvents

    void OnEnable()
    {
        inputManager.OnMouseButtonUp += CheckSwipe;
        swipeListener.OnSwipe.AddListener(SetSwipeType);
        tileSelector.OnTileSelected += SetSelectedTile;
        
    }

    void OnDisable()
    {
        inputManager.OnMouseButtonUp  -= CheckSwipe;
        swipeListener.OnSwipe.RemoveListener(SetSwipeType);
        tileSelector.OnTileSelected -= SetSelectedTile;
    }

    #endregion

    void SetSwipeType(string swipe)
    {
        switch (swipe)
        {
            case "Right":
                swipeType = SwipeType.right;
                break;
            case "Left":
                swipeType = SwipeType.left;
                break;
            case "Up":
                swipeType = SwipeType.up;
                break;
            case "Down":
                swipeType = SwipeType.down;
                break;
            default:
                swipeType = SwipeType.cross;
                break;
        }
    }

    void SetSelectedTile(Tile newTile)
    {
        selectedTile = newTile;
    }

    void CheckSwipe()
    {
        if (selectedTile == null) return;

        switch (swipeType)
        {
            case SwipeType.none:
                // click on tile, there is no swipe
                break;
            case SwipeType.cross:
                VisualFeedbackOfForbiddenSwiping();
                // cross movement not allowed 
            break;
            case SwipeType.right:
                SwipeProcess(selectedTile.rightNeighbour, selectedTile);
                break;
            case SwipeType.left:
                SwipeProcess(selectedTile.leftNeighbour, selectedTile);
                break;
            case SwipeType.up:
                SwipeProcess(selectedTile.upNeighbour, selectedTile);
                break;
            case SwipeType.down:
                SwipeProcess(selectedTile.downNeighbour, selectedTile);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        selectedTile = null; // deselect tile
        swipeType = SwipeType.none; // reset swipe type otherwise it will swipe when player click on a tile
    }

    void SwipeProcess(Tile neighbour, Tile selected, bool swipeBack = false)
    {
        VisualFeedbackOfForbiddenSwiping(neighbour);
        
        if (neighbour != null && neighbour.MyDrop != null && selectedTile.MyDrop != null) // to prevent swipe drops to empty tiles and outside of grid or swipe on empty tiles
        {
            Drop neighbourDrop = neighbour.MyDrop;
            // Drop selectedDrop = selectedTile.MyDrop;
            Drop selectedDrop = selected.MyDrop;
            
            // doesn't matter which one we assign to dropMoveCoroutine. they are same
            dropMoveCoroutine = StartCoroutine(selectedDrop.MoveToTile(neighbour));
            StartCoroutine(neighbourDrop.MoveToTile(selectedTile));
            
            StartCoroutine(MoveEnd(neighbour, selected, swipeBack));

            // change this part. it should be in tile class. swapping tiles' drops is not swipe controller responsibility
            SwapDrops(neighbour, neighbourDrop, selectedDrop);

            selectedTile = null;
        }
    }

    void SwapDrops(Tile neighbour, Drop neighbourDrop, Drop selectedDrop)
    {
        var tempDrop = neighbourDrop;
        neighbourDrop = selectedDrop;
        selectedDrop = tempDrop; 

        // assign new drops to tiles
        selectedTile.MyDrop = selectedDrop;
        neighbour.MyDrop = neighbourDrop;
    }

    IEnumerator MoveEnd(Tile neighbour, Tile selected, bool swipeBack)
    {
        yield return dropMoveCoroutine;
        isValidMove = matchChecker.CheckForMatches();
        if (!isValidMove && !swipeBack)
        {
            selectedTile = selected;
            SwipeProcess(neighbour, selected, true);
        }
    }

    void VisualFeedbackOfForbiddenSwiping(Tile neighbour = null)
    {
        if (selectedTile.MyDrop != null && (neighbour == null || neighbour.MyDrop == null))
        {
            switch (swipeType)
            {
                case SwipeType.none:
                    break;
                case SwipeType.cross:
                    selectedTile.MyDrop.transform.DOShakeScale(forbiddenSwipeAttemptEffectStrength,
                        forbiddenSwipeAttemptEffectDuration);
                    break;
                case SwipeType.right:
                    selectedTile.MyDrop.transform.DOPunchPosition(Vector3.right * forbiddenSwipeAttemptEffectStrength, forbiddenSwipeAttemptEffectDuration);
                    break;
                case SwipeType.left:
                    selectedTile.MyDrop.transform.DOPunchPosition(Vector3.left * forbiddenSwipeAttemptEffectStrength, forbiddenSwipeAttemptEffectDuration);
                    break;
                case SwipeType.up:
                    selectedTile.MyDrop.transform.DOPunchPosition(Vector3.up * forbiddenSwipeAttemptEffectStrength, forbiddenSwipeAttemptEffectDuration);
                    break;
                case SwipeType.down:
                    selectedTile.MyDrop.transform.DOPunchPosition(Vector3.down * forbiddenSwipeAttemptEffectStrength, forbiddenSwipeAttemptEffectDuration);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}

enum SwipeType
{
    none,
    cross,
    right,
    left,
    up,
    down
}