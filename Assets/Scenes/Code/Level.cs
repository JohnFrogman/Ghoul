﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level
{
    public Grid Grid { get; private set; }
    public HashSet<Tile[,]> Rooms;

    public Level(Vector2Int GridSize)
    {
        Grid = new Grid(GridSize);
    }
}
