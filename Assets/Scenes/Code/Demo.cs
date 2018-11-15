using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo : MonoBehaviour
{
    public GameObject TilePrefab;
    public Vector2Int GridSize;
    [SerializeField]
    private Renderer render;

    // Use this for initialization
    void Start ()
    {
        var level = new Level(GridSize);
        BSPAlgorithm.Apply(level, BSPAlgorithm.Parameters.Default, 100);

        Tile firstFloor = null;
        Tile secondFloor = null;

        int count = 0;
        //Random.InitState(563534); // 4-5 ms  
        //Random.InitState(10000); //23 ms
        // Random.InitState(2342452);  //31 ms

        Random.InitState((int)System.DateTime.Now.Ticks);
        while (true)
        {
            var tile = level.Grid.Tiles[Random.Range(0, level.Grid.Tiles.GetLength(0) - 1), Random.Range(0, level.Grid.Tiles.GetLength(1) - 1)];
            if (tile.Type == Tile.Types.Floor)
            {
                firstFloor = tile;
                break;
            }
            count++;

            if (count == 1000)
            {
                break;
            }
        }

        count = 0;
        while (true)
        {
            var tile = level.Grid.Tiles[Random.Range(0, level.Grid.Tiles.GetLength(0) - 1), Random.Range(0, level.Grid.Tiles.GetLength(1) - 1)];

            if (tile.Type == Tile.Types.Floor)
            {
                secondFloor = tile;
                break;
            }

            count++;

            if (count == 1000)
            {
                break;
            }
        }

        AStarAlgorithm pathfinding = new AStarAlgorithm();

        var path = pathfinding.GetPath(firstFloor, secondFloor);

        foreach (var item in path)
        {
            //item.GetToNode.Node.attached.GetComponent<SpriteRenderer>().color = Color.red;
        }
        render.Init(level.Grid.Tiles, GridSize);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
