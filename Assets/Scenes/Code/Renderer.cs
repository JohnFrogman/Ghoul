using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Renderer : MonoBehaviour 
// Change this to static that returns a mesh
{
	[SerializeField]
	int ChunkSize = 50;
    [SerializeField]
    int extent = 25;
    [SerializeField]
	GameObject MeshRendererPrefab;
	[SerializeField]
	GameObject GridColliderPrefab;
    Tile[,][,] ChunkGrid;
    Vector2Int gridSize;
    public void Init(Tile[,] grid, Vector2Int GridSize)
    {
        gridSize = GridSize;
		int xChunks = GridSize.x / ChunkSize + ((GridSize.x % ChunkSize == 0) ? 0 : 1);
		int yChunks = GridSize.y / ChunkSize + ((GridSize.y % ChunkSize == 0) ? 0 : 1);
		ChunkGrid = new Tile[xChunks,yChunks][,];
		for (int y = 0; y < yChunks; y++) {
			for (int x = 0; x < xChunks; x++) {
                // Build a smaller chunk when necessary 
                int xSize = ChunkSize, ySize = ChunkSize;
                if (x == GridSize.x / ChunkSize && GridSize.x % ChunkSize != 0)
                    xSize = gridSize.x - (ChunkSize * (xChunks - 1));
                if (y == GridSize.y / ChunkSize && GridSize.y % ChunkSize != 0)
                    ySize = gridSize.y - (ChunkSize * (yChunks - 1));

                ChunkGrid[x, y] = new Tile[xSize, ySize];
                for (int j = 0; j < ySize; j++) {
					for (int i = 0; i < xSize; i++) {
						ChunkGrid [x, y] [i, j] = grid [i + (x * ChunkSize), j + (y * ChunkSize)];
					}
				}	
			}
		}
        for (int z = 0; z < yChunks; z++)
            for (int x = 0; x < xChunks; x++)
            {
                RenderChunk(ChunkGrid[x, z], new Vector2Int(x, z));
            }
        RenderOuterMesh();
		AttachCollider ();
    }
	private void AttachCollider ()
	{
		Mesh mesh = new Mesh ();
		Vector3[] vertices = new Vector3[4];
		vertices [0] = new Vector3 (0, 0, 0); vertices [1] = new Vector3 (0, 0, gridSize.y); vertices [2] = new Vector3 (gridSize.x, 0, gridSize.y); vertices [3] = new Vector3 (gridSize.x, 0, 0);
		List<int> triangles = new List<int> ();
		triangles.AddRange(new int[] { 0,1,2 }); triangles.AddRange(new int[] { 0,2,3 });
		mesh.vertices = vertices;
		mesh.triangles = triangles.ToArray ();
		GameObject obj = Instantiate(GridColliderPrefab, new Vector3(0,0,0), Quaternion.identity);
		obj.GetComponent<MeshFilter>().mesh = mesh;
		obj.GetComponent<MeshCollider> ().sharedMesh = mesh;
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
                    //uv[i] = new Vector2(x%2, z%2);
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
            // X Y coordinates of tiles in the chunk
            int ChunkX = t.GridPosition.x - ChunkCoords.x * ChunkSize;
            int ChunkY = t.GridPosition.y - ChunkCoords.y * ChunkSize;
            // index of all the vertices relevant to the tile t.
            int bottomLeft = (ChunkY * (ChunkSize + 1)) + ChunkX;
            int bottomRight = bottomLeft + 1;
            int topLeft = bottomLeft + ChunkSize + 1;
            int topRight = topLeft + 1;

            // indexes for the upper Z level vertices
            int upperBottomLeft = bottomLeft + (ChunkSize + 1) * (ChunkSize + 1);
            int upperBottomRight = bottomRight + (ChunkSize + 1) * (ChunkSize + 1);
            int upperTopLeft = topLeft + (ChunkSize + 1) * (ChunkSize + 1);
            int upperTopRight = topRight + (ChunkSize + 1) * (ChunkSize + 1);

            bool LeftWall = false, RightWall = false, TopWall = false, BottomWall = false;
            if (t.Type == Tile.Types.Wall)
            {
                wallTriangles.AddRange(new int[] { upperBottomLeft, upperTopLeft, upperBottomRight });
                wallTriangles.AddRange(new int[] { upperBottomRight, upperTopLeft, upperTopRight });
                //checking neighbours to see if we need to draw a wall. also draw a wall if it's the edge of the grid.
                if (ChunkX == 0)
                {
                    // If neighbour in next chunk is a floor tile or we're on the edge of the grid we need to draw a wall.
                    if ((ChunkCoords.x != 0) && (ChunkGrid[ChunkCoords.x - 1, ChunkCoords.y][ChunkSize - 1, ChunkY].Type == Tile.Types.Floor))
                        LeftWall = true;
                }
                else if (Chunk[ChunkX - 1, ChunkY].Type == Tile.Types.Floor) // left neighbour
                    LeftWall = true;
                // -----------------------
                
                if (ChunkX == Chunk.GetLength(0) - 1)
                {
                    if ((ChunkCoords.x != ChunkGrid.GetLength(0) - 1) && (ChunkGrid[ChunkCoords.x + 1, ChunkCoords.y][0, ChunkY].Type == Tile.Types.Floor))
                        RightWall = true;
                }
                else if (Chunk[ChunkX + 1, ChunkY].Type == Tile.Types.Floor) // right neighbour
                    RightWall = true;
                // -----------------------
                if (ChunkY == Chunk.GetLength(1) - 1)
                {
                    if ((ChunkCoords.y != ChunkGrid.GetLength(1) - 1) && (ChunkGrid[ChunkCoords.x, ChunkCoords.y + 1][ChunkX, 0].Type == Tile.Types.Floor))
                        TopWall = true;
                }
                else if (t.GridPosition.y + 1 < gridSize.y && Chunk[ChunkX, ChunkY + 1].Type == Tile.Types.Floor) // top neighbour
                    TopWall = true;
                // -----------------------
                if (ChunkY == 0)
                {
                    if ((ChunkCoords.y != 0) && (ChunkGrid[ChunkCoords.x, ChunkCoords.y - 1][ChunkX, ChunkSize - 1].Type == Tile.Types.Floor))
                        BottomWall = true;
                }
                else if (ChunkY != 0 && Chunk[ChunkX, ChunkY - 1].Type == Tile.Types.Floor) // bottom neighbour
                    BottomWall = true;
            }
            else
            {
                // Draws a floow
                floorTriangles.AddRange(new int[] { bottomLeft, topLeft, bottomRight });
                floorTriangles.AddRange(new int[] { bottomRight, topLeft, topRight });
                // walls at the outer edge of the grid;
                if (ChunkCoords.y == 0 && ChunkY == 0)
                {
                    wallTriangles.AddRange(new int[] { upperBottomLeft, bottomLeft, bottomRight });
                    wallTriangles.AddRange(new int[] { upperBottomLeft, bottomRight, upperBottomRight });
                }
                if (ChunkCoords.y == ChunkGrid.GetLength(1) - 1 && ChunkY == ChunkSize - 1)
                {
                    wallTriangles.AddRange(new int[] { upperTopLeft, topRight, topLeft });
                    wallTriangles.AddRange(new int[] { upperTopLeft, topRight, upperTopRight });
                }
                if (ChunkCoords.x == 0 && ChunkX == 0)
                {
                    wallTriangles.AddRange(new int[] { upperTopLeft, bottomLeft, upperBottomLeft });
                    wallTriangles.AddRange(new int[] { upperTopLeft, topLeft, bottomLeft });
                }
                if (ChunkCoords.x == ChunkGrid.GetLength(0) - 1 && ChunkX == ChunkSize - 1)
                {
                    wallTriangles.AddRange(new int[] { upperTopRight, upperBottomRight, bottomRight });
                    wallTriangles.AddRange(new int[] { upperTopRight, bottomRight, topRight });
                }
            }
            if (LeftWall)
            {
                wallTriangles.AddRange(new int[] { upperTopLeft, upperBottomLeft, bottomLeft, });
                wallTriangles.AddRange(new int[] { upperTopLeft, bottomLeft, topLeft });
            }

            if (RightWall)
            {
                wallTriangles.AddRange(new int[] { upperTopRight, bottomRight, upperBottomRight });
                wallTriangles.AddRange(new int[] { upperTopRight, topRight, bottomRight });
            }
            if (TopWall)
            {
                wallTriangles.AddRange(new int[] { upperTopLeft, topLeft, topRight });
                wallTriangles.AddRange(new int[] { upperTopLeft, topRight, upperTopRight });
            }
            if (BottomWall)
            {
                wallTriangles.AddRange(new int[] { upperBottomLeft, bottomRight, bottomLeft });
                wallTriangles.AddRange(new int[] { upperBottomLeft, upperBottomRight, bottomRight });
            }
        }
        mesh.SetTriangles(floorTriangles.ToArray(), 0);
        mesh.SetTriangles(wallTriangles.ToArray(), 1);
        mesh.RecalculateNormals();
        for (int i = 0; i < mesh.normals.Length; i++)
            mesh.normals[i] = Vector3.up;

        GameObject obj = Instantiate(MeshRendererPrefab, new Vector3(ChunkCoords.x * ChunkSize, 0, ChunkCoords.y * ChunkSize), Quaternion.identity);
        obj.GetComponent<MeshFilter>().mesh = mesh;
        Material[] mats = obj.GetComponent<MeshRenderer>().materials;
        foreach (Material m in mats)
        {
            m.mainTextureScale = new Vector2(ChunkSize, ChunkSize);
        }
    }
    void RenderOuterMesh()
    {

        Vector3[] vertices = new Vector3[16];
        Vector2[] uv = new Vector2[vertices.Length];
        Vector2Int extentMeshSize = new Vector2Int(gridSize.x + (2 * extent), gridSize.y + (2 * extent));
        //Bottom left corner quad vertices
        vertices[0] = new Vector3(0, 1, 0); vertices[1] = new Vector3(0, 1, -extent); vertices[2] = new Vector3(-extent, 1, 0); vertices[3] = new Vector3(-extent, 1, -extent);
		uv[0] = new Vector2((float)extent/(float)extentMeshSize.x, (float)extent/(float)extentMeshSize.y); uv[1] = new Vector2((float)extent/(float)extentMeshSize.x, 0); uv[2] = new Vector2(0, (float)extent/(float)extentMeshSize.y); uv[3] = new Vector2(0,0);
        //Bottom right corner quad vertices
        vertices[4] = new Vector3(gridSize.x, 1, 0); vertices[5] = new Vector3(gridSize.x, 1, -extent); vertices[6] = new Vector3(gridSize.x+extent, 1, 0); vertices[7] = new Vector3(gridSize.x+extent, 1, -extent);
		uv[4] = new Vector2 ((float)(gridSize.x + (float)extent) / (float)extentMeshSize.x, (float)extent / (float)extentMeshSize.y); uv[5] = new Vector2 ((float)((float)gridSize.x + (float)extent) / (float)extentMeshSize.x, 0); uv[6] = new Vector2 (1f, (float)extent / (float)extentMeshSize.y); uv[7] = new Vector2 (1f, 0f);
		//Top left corner quad vertices
        vertices[8] = new Vector3(0, 1, gridSize.y); vertices[9] = new Vector3(0, 1, gridSize.y+extent); vertices[10] = new Vector3(-extent, 1, gridSize.y); vertices[11] = new Vector3(-extent, 1, gridSize.y+extent);
		uv [8] = new Vector2 ((float)extent/(float)extentMeshSize.x, ((float)extent+(float)gridSize.y)/(float)extentMeshSize.y); uv [9] = new Vector2 ((float)extent / (float)extentMeshSize.x, 1); uv [10] = new Vector2 (0, ((float)extent+(float)gridSize.y)/(float)extentMeshSize.y); uv [11] = new Vector2 (0, 1);

		//top right corner quad vertices
        vertices[12] = new Vector3(gridSize.x, 1, gridSize.y); vertices[13] = new Vector3(gridSize.x, 1, gridSize.y+extent); vertices[14] = new Vector3(gridSize.x+extent, 1, gridSize.y); vertices[15] = new Vector3(gridSize.x+extent, 1, gridSize.x+extent);
		uv [12] = new Vector2 (((float)gridSize.x + (float)extent)/(float)extentMeshSize.x, ((float)gridSize.y + (float)extent)/(float)extentMeshSize.y); uv [13] = new Vector2 (((float)gridSize.x + (float)extent)/(float)extentMeshSize.x, 1); uv [14] = new Vector2 (1, ((float)gridSize.y + (float)extent)/(float)extentMeshSize.y); uv [15] = new Vector2 (1,1);

		Mesh mesh = new Mesh();
        mesh.name = "MainMesh";
        mesh.vertices = vertices;
        mesh.uv = uv;

        mesh.subMeshCount = 2;
        List<int> triangles = new List<int>();
        triangles.AddRange(new int[]{ 0, 1, 3 }); triangles.AddRange(new int[] { 0, 3, 2 }); // Bottom left
        triangles.AddRange(new int[] { 4, 7, 5 }); triangles.AddRange(new int[] { 4, 6, 7 }); // bottom right
        triangles.AddRange(new int[] { 8, 11, 9 }); triangles.AddRange(new int[] { 8, 10, 11 }); // top left
        triangles.AddRange(new int[] { 12, 13, 15 }); triangles.AddRange(new int[] { 12, 15, 14 }); // top right

        triangles.AddRange(new int[] { 0, 5, 1 }); triangles.AddRange(new int[] { 0, 4, 5 }); //bottom
        triangles.AddRange(new int[] { 4, 12, 14}); triangles.AddRange(new int[] { 4, 14, 6 }); // right
        triangles.AddRange(new int[] { 8, 13,12  }); triangles.AddRange(new int[] { 8, 9, 13 }); //top
        triangles.AddRange(new int[] { 0, 10, 8 }); triangles.AddRange(new int[] { 0, 2, 10 }); //right

        mesh.SetTriangles(triangles.ToArray(), 1);
        mesh.RecalculateNormals();
        for (int i = 0; i < mesh.normals.Length; i++)
            mesh.normals[i] = Vector3.up;

        GameObject obj = Instantiate(MeshRendererPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        obj.GetComponent<MeshFilter>().mesh = mesh;
        Material[] mats = obj.GetComponent<MeshRenderer>().materials;
        foreach (Material m in mats)
        {
            m.mainTextureScale = new Vector2(extentMeshSize.x, extentMeshSize.y);
        }
        //mesh.norma
    }
}
