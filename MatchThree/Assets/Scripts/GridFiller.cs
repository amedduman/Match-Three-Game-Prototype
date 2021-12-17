using System.Collections.Generic;
using UnityEngine;

public class GridFiller
{
    readonly Drop drop;
    readonly List<Sprite> possibleSprites = new List<Sprite>();
    readonly Transform dropParent;

    public GridFiller(Drop drop, Transform dropParent)
    {
        this.drop = drop;
        foreach (var sprite in drop.DropSprites)
        {
            possibleSprites.Add(sprite);
        }
        this.dropParent = dropParent;
    }

    public void FillTiles(Tile[,] tiles)
    {
        Sprite belowSprite = null;
        Sprite leftSprite = null;

        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                InstantiateDrop(tiles, x, y);

                belowSprite = GetSpriteOfBelowDrop(tiles, y, belowSprite, x);

                leftSprite = GetSpriteOfLeftSprite(tiles, x, leftSprite, y);
                
                // this method will choose a sprite for current drop which is different from below and left sprites. this way we will prevent having matches at start 
                SetSprite(tiles, x, y, belowSprite, leftSprite);
                
                NameDrop(tiles, x, y);
            }
        }
    }

    static Sprite GetSpriteOfLeftSprite(Tile[,] tiles, int x, Sprite leftSprite, int y)
    {
        if (x > 0)
        {
            leftSprite = tiles[x - 1, y].MyDrop.currentSprite;
        }

        return leftSprite;
    }

    static Sprite GetSpriteOfBelowDrop(Tile[,] tiles, int y, Sprite belowSprite, int x)
    {
        if (y > 0)
        {
            belowSprite = tiles[x, y - 1].MyDrop.currentSprite;
        }

        return belowSprite;
    }

    void InstantiateDrop(Tile[,] tiles, int x, int y)
    {
        tiles[x, y].MyDrop = Object.Instantiate(drop,
            tiles[x, y].transform.position,
            Quaternion.identity,
            dropParent);
    }

    static void NameDrop(Tile[,] tiles, int x, int y)
    {
        tiles[x, y].MyDrop.name = tiles[x, y].MyDrop.currentSprite.name;
    }

    void SetSprite(Tile[,] tiles, int x, int y, Sprite belowSprite, Sprite leftSprite)
    {
        tiles[x, y].MyDrop.currentSprite = GetRandomFromPossibleSprites(belowSprite, leftSprite);
        
        // update sprite on drop
        tiles[x, y].MyDrop.UpdateDropSprite();
    }

    Sprite GetRandomFromPossibleSprites(Sprite previous, Sprite left)
    {
        var tempSpriteList = new List<Sprite>();
        foreach (var sprite in possibleSprites)
        {
            tempSpriteList.Add(sprite);
        }
        tempSpriteList.Remove(previous);
        tempSpriteList.Remove(left);
        return tempSpriteList[Random.Range(0, tempSpriteList.Count)];
    }
}
