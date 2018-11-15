using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Renderer : MonoBehaviour 
// Change this to static that returns a mesh
{
	int ChunkSize = 50;
	[SerializeField]
	GameObject MeshRendererPrefab;
    public void Init(Tile[,] grid, Vector2Int GridSize)
    {
		int xChunks = GridSize.x / ChunkSize + ((GridSize.x % ChunkSize == 0) ? 0 : 1);
		int yChunks = GridSize.y / ChunkSize + ((GridSize.y % ChunkSize == 0) ? 0 : 1);
		Tile[,][,] ChunkGrid = new Tile[xChunks,yChunks][,]; //[ChunkSize, ChunkSize]
		for (int y = 0; y < yChunks; y++) {
			for (int x = 0; x < xChunks; x++) {
                ChunkGrid[x, y] = new Tile[ChunkSize, ChunkSize];
                for (int j = 0; j < ChunkSize; j++) {
					for (int i = 0; i < ChunkSize; i++) {
						if (grid [i + (x * ChunkSize), j + (y * ChunkSize)] != null)
							ChunkGrid [x, y] [i, j] = grid [i + (x * ChunkSize), j + (y * ChunkSize)];
					}
				}	
			}
		} 
		for (int z = 0; z < xChunks; z++)
			for (int x = 0; x < yChunks; x++)
				RenderChunk(ChunkGrid[x,z], new Vector2Int(x,z));
    }
	private void RenderChunk(Tile[,] Chunk, Vector2Int ChunkCoords)
    {
		Vector3[] vertices = new Vector3[2 * (ChunkSize + 1) * (ChunkSize + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        for (int y = 0, i = 0; y < 2; y++)
        {
			for (int z = 0; z <= ChunkSize; z++)
            {
				for (int x = 0; x <= ChunkSize; x++, i++)
                {
                    vertices[i] = new Vector3(x, y, z);
					uv[i] = new Vector2((float)x / ChunkSize, (float)z / ChunkSize);
                }
            }
        }
        Mesh mesh = new Mesh();
        mesh.name = "MainMesh";
        mesh.vertices = vertices;
        mesh.uv = uv;

        mesh.subMeshCount = 2;
        List<int> floorTriangles = new List<int>();
        List<int> wallTriangles = new List<int>();
        foreach (Tile t in Chunk)
        {
            int ChunkX = t.GridPosition.x - ChunkCoords.x * ChunkSize;
            int ChunkY = t.GridPosition.y - ChunkCoords.y * ChunkSize;
            // index of all the vertices relevant to the tile t.
            int bottomLeft = (ChunkY * (ChunkSize + 1)) + ChunkX;
            int bottomRight = bottomLeft + 1;
			int topLeft = bottomLeft + ChunkSize + 1;
            int topRight = topLeft + 1;

            if (t.Type == Tile.Types.Wall)
            {
                // indexes for the upper Z level vertices
                int upperBottomLeft = bottomLeft + (ChunkSize + 1) * (ChunkSize + 1);
                int upperBottomRight = bottomRight + (ChunkSize + 1) * (ChunkSize + 1);
                int upperTopLeft = topLeft + (ChunkSize + 1) * (ChunkSize + 1);
                int upperTopRight = topRight + (ChunkSize + 1) * (ChunkSize + 1);

                wallTriangles.AddRange(new int[] { upperBottomLeft, upperTopLeft, upperBottomRight });
                wallTriangles.AddRange(new int[] { upperBottomRight, upperTopLeft, upperTopRight });

                //checking neighbours to see if we need to draw a wall. also draw a wall if it's the edge of the grid.
                if (ChunkX == 0 || Chunk[ChunkX - 1, ChunkY].Type == Tile.Types.Floor) // left neighbour
                {
                    wallTriangles.AddRange(new int[] { upperTopLeft, upperBottomLeft, bottomLeft, });
                    wallTriangles.AddRange(new int[] { upperTopLeft, bottomLeft, topLeft });
                }
                if (ChunkX == ChunkSize - 1 || Chunk[ChunkX + 1, ChunkY].Type == Tile.Types.Floor) // right neighbour
                {
                    wallTriangles.AddRange(new int[] { upperTopRight, bottomRight, upperBottomRight });
                    wallTriangles.AddRange(new int[] { upperTopRight, topRight, bottomRight });
                }
                if (ChunkY == ChunkSize - 1 || Chunk[ChunkX, ChunkY + 1].Type == Tile.Types.Floor) // top neighbour
                {
                    wallTriangles.AddRange(new int[] { upperTopLeft, topLeft, topRight });
                    wallTriangles.AddRange(new int[] { upperTopLeft, topRight, upperTopRight });
                }
                if (ChunkY == 0 || Chunk[ChunkX, ChunkY - 1].Type == Tile.Types.Floor) // bottom neighbour
                {
                    wallTriangles.AddRange(new int[] { upperBottomLeft, bottomRight, bottomLeft });
                    wallTriangles.AddRange(new int[] { upperBottomLeft, upperBottomRight, bottomRight });
                }
            }
            else
            {
                floorTriangles.AddRange(new int[] { bottomLeft, topLeft, bottomRight });
                floorTriangles.AddRange(new int[] { bottomRight, topLeft, topRight });
            }
        }
        mesh.SetTriangles(floorTriangles.ToArray(), 0);
        mesh.SetTriangles(wallTriangles.ToArray(), 1);
        mesh.RecalculateNormals();
		GameObject obj = Instantiate (MeshRendererPrefab, new Vector3(ChunkCoords.x*ChunkSize, 0, ChunkCoords.y*ChunkSize), Quaternion.identity); 
        obj.GetComponent<MeshFilter>().mesh = mesh;
    }
}
