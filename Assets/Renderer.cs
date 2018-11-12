using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Renderer : MonoBehaviour {
    Tile[,] Grid;
    int width = 3; int height = 3;
    Vector3[] vertices;

    void Awake()
    {
        width = Random.Range(3, 20);
        height = Random.Range(3, 20);
        Grid = new Tile[width, height];
        for (int i = 0; i < width; ++i)
            for (int j = 0; j < height; ++j)
                Grid[i,j] = new Tile(new Vector2Int(i, j));
        foreach (Tile t in Grid)
            t.type = Random.value < 0.5 ? Tile.Types.Floor : Tile.Types.Wall;
        //Grid[1,1].type = Tile.Types.Floor;
        Render();
    }
    private void Render()
    {
        vertices = new Vector3[2 * (width + 1) * (height + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        for (int y = 0, i = 0; y < 2; y++)
        {
            for (int z = 0; z <= height; z++)
            {
                for (int x = 0; x <= width; x++, i++)
                {
                    vertices[i] = new Vector3(x, y, z);
                    uv[i] = new Vector2((float)x / width, (float)y / height);
                }
            }
        }
        Mesh mesh = new Mesh();
        mesh.name = "MainMesh";
        mesh.vertices = vertices;
        List<int> triangles = new List<int>();
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

                triangles.AddRange(new int[] { upperBottomLeft ,upperTopLeft, upperBottomRight });
                triangles.AddRange(new int[] { upperBottomRight ,upperTopLeft, upperTopRight });

                //checking neighbours to see if we need to draw a wall. also draw a wall if it's the edge of the grid.
                if (t.Position.x == 0 || Grid[t.Position.x - 1, t.Position.y].type == Tile.Types.Floor) // left neighbour
                {
                    triangles.AddRange(new int[] { upperTopLeft, upperBottomLeft, bottomLeft, });
                    triangles.AddRange(new int[] { upperTopLeft, bottomLeft, topLeft });
                }
                if (t.Position.x == width - 1 || Grid[t.Position.x + 1, t.Position.y].type == Tile.Types.Floor) // right neighbour
                {
                    triangles.AddRange(new int[] { upperTopRight, bottomRight, upperBottomRight });
                    triangles.AddRange(new int[] { upperTopRight, topRight, bottomRight });
                }
                if (t.Position.y == height - 1 || Grid[t.Position.x, t.Position.y + 1].type == Tile.Types.Floor) // top neighbour
                {
                    triangles.AddRange(new int[] { upperTopLeft, topLeft, topRight });
                    triangles.AddRange(new int[] { upperTopLeft, topRight, upperTopRight });
                }
                if (t.Position.y == 0 || Grid[t.Position.x, t.Position.y - 1].type == Tile.Types.Floor) // bottom neighbour
                {
                    triangles.AddRange(new int[] { upperBottomLeft, bottomRight, bottomLeft });
                    triangles.AddRange(new int[] { upperBottomLeft, upperBottomRight, bottomRight });
                }
            }
            else
            {
                triangles.AddRange(new int[] { bottomLeft, topLeft, bottomRight });
                triangles.AddRange(new int[] { bottomRight, topLeft, topRight });
            }
        }
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        GetComponent<MeshFilter>().mesh = mesh;
    }
}
