using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MatchChecker : MonoBehaviour
{
    public Tile[,] tiles { get; private set; }
    HashSet<Tile> matchedTiles = new HashSet<Tile>(); // we don't wanna add same drop twice in case a drop is a match vertically and horizontally
    int lastCheckedTileIndex = -1;
    Coroutine matchedDropAnimationCoroutine;

    public void Init(Tile[,] myTiles)
    {
        tiles = myTiles;
    }

    [ContextMenu("CheckForMatches")]
    public bool CheckForMatches()
    {
        InputManager.Instance.canInput = false;
        
        VerticalSearch();

        HorizontalSearch();
        
        if (HasMoveCreateMatch())
        { 
            // there is no match as result of this move
            InputManager.Instance.canInput = true;
            return false;
        }
        
        ClearMatchedDrops();

        // falling will wait for drops disappear
        StartCoroutine(FallProcess());
        return true;
    }

    IEnumerator FallProcess()
    {
        yield return matchedDropAnimationCoroutine;
        
        bool hasEmptyTile = true;
        while (hasEmptyTile)
        {
            hasEmptyTile = false;
            foreach (Tile tile in tiles)
            {
                if (tile.GetMyTopTile(tiles).isSpawner)
                {
                    if (tile.MyDrop == null)
                    {
                        hasEmptyTile = true;
                    }
                }
                else if(tile.MyDrop != null && tile.downNeighbour != null && tile.downNeighbour.MyDrop == null)
                {
                    hasEmptyTile = true;
                }
                tile.Fall(tiles);
            }
        }
        MatchEndProcess();
        DOVirtual.DelayedCall(tiles[0,0].fallTime, () => CheckForMatches()); // it waits for drops end falling. It is a very poor implementation but it will do for now
    }
    
    void MatchEndProcess()
    {
        matchedTiles.Clear();
    }

    void HorizontalSearch()
    {
        for (int y = 0; y < tiles.GetLength(1); y++)
        {
            lastCheckedTileIndex = -1;

            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                if (x <= lastCheckedTileIndex)
                {
                    continue;
                }

                if (hasMatch(x, y, SearchType.horizontal))
                {
                    bool matchContinue = true;
                    lastCheckedTileIndex = x;

                    while (matchContinue && tiles[lastCheckedTileIndex, y].rightNeighbour != null)
                    {
                        if (tiles[lastCheckedTileIndex, y].MyDrop.SpriteIndex ==
                            tiles[lastCheckedTileIndex, y].rightNeighbour.MyDrop.SpriteIndex)
                        {
                            matchedTiles.Add(tiles[lastCheckedTileIndex, y]);
                            matchedTiles.Add(tiles[lastCheckedTileIndex, y].rightNeighbour);
                            lastCheckedTileIndex++;
                        }
                        else
                        {
                            matchContinue = false;
                        }
                    }
                }
            }
        }
    }

    void VerticalSearch()
    {
        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            lastCheckedTileIndex = -1; // reset lastCheckedTile value to prevent not checking tiles in next column

            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                // to prevent checking tiles which already checked while looking for match
                if (y <= lastCheckedTileIndex)
                {
                    // dont check this tiles
                    continue;
                }

                if (hasMatch(x, y, SearchType.vertical))
                {
                    bool matchContinue = true;
                    lastCheckedTileIndex = y;
                    while (matchContinue && tiles[x, lastCheckedTileIndex].upNeighbour != null)
                    {
                        if (tiles[x, lastCheckedTileIndex].MyDrop.SpriteIndex ==
                            tiles[x, lastCheckedTileIndex].upNeighbour.MyDrop.SpriteIndex)
                        {
                            matchedTiles.Add(tiles[x, lastCheckedTileIndex]);
                            matchedTiles.Add(tiles[x, lastCheckedTileIndex].upNeighbour);
                            lastCheckedTileIndex++;
                        }
                        else
                        {
                            matchContinue = false;
                        }
                    }
                }
            }
        }
    }

    bool HasMoveCreateMatch()
    {
        return matchedTiles.Count == 0;
    }
    
    void ClearMatchedDrops()
    {
        foreach (var tile in matchedTiles)
        {
            Tile topTile = tile.GetMyTopTile(tiles);
            Drop drop = tile.MyDrop;

            matchedDropAnimationCoroutine = StartCoroutine(drop.MatchProcess(topTile));
            tile.MyDrop = null;
        }
    }

    bool hasMatch(int x, int y, SearchType searchType)
    {
        Tile currentTile = tiles[x, y];
        try
        {
            Tile primaryNeighbour;
            Tile secondaryNeighbour;
            switch (searchType)
            {
                case SearchType.vertical:
                    primaryNeighbour = currentTile.upNeighbour;
                    secondaryNeighbour = primaryNeighbour.upNeighbour;
                    break;
                case SearchType.horizontal:
                    primaryNeighbour = currentTile.rightNeighbour;
                    secondaryNeighbour = primaryNeighbour.rightNeighbour;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(searchType), searchType, null);
            }
            
            if(currentTile.MyDrop.SpriteIndex == primaryNeighbour.MyDrop.SpriteIndex && primaryNeighbour.MyDrop.SpriteIndex == secondaryNeighbour.MyDrop.SpriteIndex)
            {
                return true;
            }
        }
        catch (NullReferenceException) // while assigning primary and secondary neighbours if  null reference exception occurs it means there is no primary or secondary neighbour for current tile so there is no match we can return false
        {
            return false;
        }

        return false;
    }
}

enum SearchType
{
    vertical,
    horizontal
}
