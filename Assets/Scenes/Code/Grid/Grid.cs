using UnityEngine;

public class Grid 
{
    public Tile[,] Tiles { get; private set; }

    public Grid(Vector2 tileDimensions, Vector2 levelSize)
    {
        Build(tileDimensions, levelSize);
    }

	private void Build(Vector2 tileDimensions, Vector2 levelSize)
	{
        Vector2 startPos = Vector2.zero;
        Tiles = new Tile[(int)levelSize.x, (int)levelSize.y];

		for (int i = 0; i < levelSize.x; i++) 
		{
			for (int j = 0; j < levelSize.y; j++) 
			{
                var tilePos = startPos + tileDimensions;
                var tile = new Tile(tilePos, new Vector2Int(i, j));
				Tiles[i, j] = tile;

                if(j>0)
                {
                    SetNeighbours(tile, Tiles[i, j-1]);
                }

                if(i>0)
                {
                    SetNeighbours(tile, Tiles[i - 1, j]);
                }

				startPos.y += tileDimensions.y;
			}

			startPos.y = 0;
			startPos.x += tileDimensions.x;
		}
	}

    private void SetNeighbours(Tile one, Tile two)
    {
        two.Neighbours.Add(one);
        one.Neighbours.Add(two);
    }

    public static Tile[,] GetSection(Tile[,] parentGrid, int minX, int maxX, int minY, int maxY)
    {
        Tile[,] newGrid = new Tile[maxX - minX, maxY - minY];
        for (int i = minX; i < maxX; i++)
            for (int j = minY; j < maxY; j++)
                newGrid[i - minX, j - minY] = parentGrid[i, j];
        return newGrid;
    }

    public static void SetSection(Tile[,] partition, Tile.Types type)
    {
        foreach (var item in partition)
        {
            item.Type = type;
        }
    }
}