using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile
{
    public readonly Vector2Int Position;
    public Types type = Types.Wall;

    public enum Types
    {
        Wall,
        Floor
    }
    public Tile(Vector2Int pos)
    {
        Position = pos;
    }
}
