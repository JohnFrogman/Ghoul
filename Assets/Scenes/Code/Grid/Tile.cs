using UnityEngine;
using System.Collections.Generic;
using System;

public class Tile 
{
    public readonly Vector2 TransformPosition;
    public readonly Vector2Int GridPosition;
    public Types Type = Types.Wall;

    public GameObject attached;

    //up, down, left, right
    public HashSet<Tile> Neighbours = new HashSet<Tile>();

    public enum Types
    {
        Wall,
        Floor,
    }

    public Tile(Vector2Int gridPos)
    {
        GridPosition = gridPos;
    }
}