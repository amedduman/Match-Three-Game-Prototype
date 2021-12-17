using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public SpriteRenderer TileSpriteRenderer;
    [HideInInspector] public int tileIdX;
    [HideInInspector] public int tileIdY;
   
    public bool isSpawner { get;  set; } = false;
    
    [HideInInspector] public Tile rightNeighbour;
    [HideInInspector] public Tile leftNeighbour;
    [HideInInspector] public Tile upNeighbour;
    [HideInInspector] public Tile downNeighbour;

    public Drop MyDrop;
    public Queue<Drop> dropPile; // drops which will fall will store here if tile is a spawner  
    
    public float fallTime = .5f;
    [SerializeField] Ease ease = Ease.Linear;
    
    void Start()
    {
        if (isSpawner)
        {
            dropPile = new Queue<Drop>();
        }
    }

    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            MyDrop.dropSpriteRenderer.color = Color.black;
        }
    }

    public void Fall(Tile[,] tiles)
    {
        if (downNeighbour == null) return;
        
        if (isSpawner)
        {
            if (MyDrop == null)
            {
                if (dropPile.Count <= 0)
                {
                    Debug.Log("empty ", this);
                }
                Drop drop = dropPile.Dequeue();
                drop.transform.DOMove(transform.position, fallTime).SetEase(ease);
                MyDrop = drop;
            }
        }

        if(!GetMyTopTile(tiles).isSpawner)
        {
            FallInNonRefillableColumn();
            return;
        }

        if (downNeighbour.MyDrop == null && MyDrop != null)
        {
            downNeighbour.MyDrop = MyDrop;
            MyDrop.transform.DOMove(downNeighbour.transform.position, fallTime).SetEase(ease);
            MyDrop = null;
        }
    }

    void FallInNonRefillableColumn()
    {
        if (downNeighbour == null) return;
        if (downNeighbour.MyDrop == null && MyDrop != null)
        {
            downNeighbour.MyDrop = MyDrop;
            MyDrop.transform.DOMove(downNeighbour.transform.position, fallTime).SetEase(ease);
            MyDrop = null;
        }
    }

    public void AddToDropPile(Drop drop)
    {
        dropPile.Enqueue(drop);
        float tileHeight = TileSpriteRenderer.bounds.size.y;
        drop.transform.position = transform.position + new Vector3(0, tileHeight * dropPile.Count , 0);
    }

    public void SetNeighbours(Tile[,] tiles)
    {
        // right neighbour
        try
        {
            rightNeighbour = tiles[tileIdX + 1, tileIdY];
        }
        catch (IndexOutOfRangeException)
        {
            rightNeighbour = null;
        }
        
        // left neighbour
        try
        {
            leftNeighbour = tiles[tileIdX - 1, tileIdY];
        }
        catch (IndexOutOfRangeException)
        {
            leftNeighbour = null;
        }
        
        // up neighbour
        try
        {
            upNeighbour = tiles[tileIdX, tileIdY + 1];
        }
        catch (IndexOutOfRangeException)
        {
            upNeighbour = null;
        }
        
        // down neighbour
        try
        {
            downNeighbour = tiles[tileIdX, tileIdY - 1];
        }
        catch (IndexOutOfRangeException)
        {
            downNeighbour = null;
        }
    }

    public Tile GetMyTopTile(Tile[,] tiles)
    {
        return tiles[tileIdX, tiles.GetLength(1) - 1];
    }
}