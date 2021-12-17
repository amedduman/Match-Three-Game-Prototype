using UnityEngine;

public class GridGenerator : MonoBehaviour 
{
	// dependencies
	GridFiller gridFiller;
	[SerializeField] MatchChecker matchChecker;
	
	[SerializeField] Camera gameCam;
	[SerializeField] Tile tilePrefab;
	[SerializeField] Drop drop;
	[SerializeField] Transform dropParent;
	[SerializeField] int xSize = 8;
	[SerializeField] int ySize = 8;
	[SerializeField] bool[] spawnPermissionForTopTiles;

	
	
	Tile[,] tiles;

	void OnValidate()
	{
		if (spawnPermissionForTopTiles.Length != xSize)
		{
			spawnPermissionForTopTiles = new bool[xSize];
			for (int i = 0; i < xSize; i++)
			{
				spawnPermissionForTopTiles[i] = true;
			}
		}
	}

	void Awake()
	{
		gridFiller = new GridFiller(drop, dropParent);
	}

	void Start () 
	{
		CreateGrid();
    }

	private void CreateGrid () 
	{
		tiles = new Tile[xSize, ySize];
		
		float aspectRatio = gameCam.aspect;
		
		// calculate needed distance between tiles
		Vector2 offset = tilePrefab.TileSpriteRenderer.bounds.size;
		float xOffset = offset.x;
		float yOffset = offset.y;
		
		// calculate grid size
		float gridWidth = xOffset * xSize;
		float gridHeight = yOffset * ySize;
		
		// calculate screen size 
		float screenWidth = gameCam.orthographicSize * 2 * aspectRatio;
		float screenHeight = gameCam.orthographicSize * 2;
		
		// calculate padding amount to place grid to center
		float paddingX = (screenWidth - gridWidth) / 2;
		float paddingY = (screenHeight - gridHeight) / 2;
	
		// calculate position of first tile in grid
		float startX = -screenWidth / 2 + paddingX;
		float startY = -screenHeight / 2 + paddingY;
		
		// generate grid
		for (int x = 0; x < xSize; x++) 
		{
			for (int y = 0; y < ySize; y++) 
			{
				Tile tile = InstantiateTile(x, y, startX, xOffset, startY, yOffset);

				NameTile(tile, x, y);

				SetTileIds(tile, x, y);
				
				tiles[x, y] = tile; 
			}
        }

		foreach (var tile in tiles)
		{
			tile.SetNeighbours(tiles);
		}
		
		gridFiller.FillTiles(tiles);
		matchChecker.Init(tiles);		
    }

	Tile InstantiateTile(int x, int y, float startX, float xOffset, float startY, float yOffset)
	{
		Tile tile = InstantiateTile(startX, xOffset, x, startY, yOffset, y);
		
		// if column allowed to have spawner tile 
		if (spawnPermissionForTopTiles[x])
		{
			// if it is the top tile 
			if (y == ySize - 1)
			{
				tile.isSpawner = true;
			}
		}

		return tile;
	}

	Tile InstantiateTile(float startX, float xOffset, int x, float startY, float yOffset, int y)
	{
		Tile tile = Instantiate(tilePrefab,
			new Vector3(startX + (xOffset * x),
				startY + (yOffset * y), 0),
			tilePrefab.transform.rotation, transform);
		return tile;
	}
	
	void SetTileIds(Tile tile, int x, int y)
	{
		tile.tileIdX = x;
		tile.tileIdY = y;
	}

	void NameTile(Tile tile, int x, int y)
	{
		tile.gameObject.name = $"Tile ({x + 1},{y + 1})";
	}
}

