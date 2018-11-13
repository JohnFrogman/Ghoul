using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo : MonoBehaviour
{
    public GameObject TilePrefab;
    public Vector2Int GridSize;

    // Use this for initialization
    void Start ()
    {
        Vector2 tileDim = TilePrefab.GetComponent<Renderer>().bounds.size;
        var level = new Level(tileDim, GridSize);
        BSPAlgorithm.Apply(level, BSPAlgorithm.Parameters.Default, 100);

        Tile firstFloor = null;
        Tile secondFloor = null;

        foreach (var tile in level.Grid.Tiles)
        {
            var obj = Instantiate(TilePrefab, tile.TransformPosition, Quaternion.identity, transform);
            obj.GetComponent<SpriteRenderer>().color = tile.Type == Tile.Types.Floor ? Color.gray : Color.black;
            tile.attached = obj;
        }

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
            item.GetToNode.Node.attached.GetComponent<SpriteRenderer>().color = Color.red;
        }

        firstFloor.attached.GetComponent<SpriteRenderer>().color = Color.blue;
        secondFloor.attached.GetComponent<SpriteRenderer>().color = Color.green;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
