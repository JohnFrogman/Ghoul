using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Renderer : MonoBehaviour {
    Tile[,] Grid;
    int width = 50; int height = 50;

    void Awake()
    {
        Grid = new Tile[width, height];
        for (int i = 0; i < width; ++i)
            for (int j = 0; j < height; ++j)
                Grid[i,j] = new Tile(new Vector2Int(i, j));
        foreach (Tile t in Grid)
            t.type = Random.value < 0.5 ? Tile.Types.Floor : Tile.Types.Wall;
        Render();
    }
    private void Render()
    {
        Vector3[] vertices = new Vector3[2 * (width + 1) * (height + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        for (int y = 0, i = 0; y < 2; y++)
        {
            for (int z = 0; z <= height; z++)
            {
                for (int x = 0; x <= width; x++, i++)
                {
                    vertices[i] = new Vector3(x, y, z);
                    uv[i] = new Vector2((float)x / width, (float)z / height);
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
        foreach (Tile t in Grid)
        {
            // index of all the vertices relevant to the tile t.
            int bottomLeft = (t.Position.y * (width + 1)) + t.Position.x;
            int bottomRight = bottomLeft + 1;
            int topLeft = bottomLeft + width + 1;
            int topRight = topLeft + 1;

            if (t.type == Tile.Types.Wall)
            {
                // indexes for the upper Z level vertices
                int upperBottomLeft = bottomLeft + (width + 1) * (height + 1);
                int upperBottomRight = bottomRight + (width + 1) * (height + 1);
                int upperTopLeft = topLeft + (width + 1) * (height + 1);
                int upperTopRight = topRight + (width + 1) * (height + 1);

                wallTriangles.AddRange(new int[] { upperBottomLeft ,upperTopLeft, upperBottomRight });
                wallTriangles.AddRange(new int[] { upperBottomRight ,upperTopLeft, upperTopRight });

                //checking neighbours to see if we need to draw a wall. also draw a wall if it's the edge of the grid.
                if (t.Position.x == 0 || Grid[t.Position.x - 1, t.Position.y].type == Tile.Types.Floor) // left neighbour
                {
                    wallTriangles.AddRange(new int[] { upperTopLeft, upperBottomLeft, bottomLeft, });
                    wallTriangles.AddRange(new int[] { upperTopLeft, bottomLeft, topLeft });
                }
                if (t.Position.x == width - 1 || Grid[t.Position.x + 1, t.Position.y].type == Tile.Types.Floor) // right neighbour
                {
                    wallTriangles.AddRange(new int[] { upperTopRight, bottomRight, upperBottomRight });
                    wallTriangles.AddRange(new int[] { upperTopRight, topRight, bottomRight });
                }
                if (t.Position.y == height - 1 || Grid[t.Position.x, t.Position.y + 1].type == Tile.Types.Floor) // top neighbour
                {
                    wallTriangles.AddRange(new int[] { upperTopLeft, topLeft, topRight });
                    wallTriangles.AddRange(new int[] { upperTopLeft, topRight, upperTopRight });
                }
                if (t.Position.y == 0 || Grid[t.Position.x, t.Position.y - 1].type == Tile.Types.Floor) // bottom neighbour
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
        GetComponent<MeshFilter>().mesh = mesh;
    }
}
