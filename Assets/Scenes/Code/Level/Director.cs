using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Director : MonoBehaviour {
	public Vector2Int GridSize;
	[SerializeField]
	private Renderer render;

	void Awake () {
		var level = new Level(GridSize);
        BSPAlgorithm.Apply(level, BSPAlgorithm.Parameters.Default, 100);
        Random.InitState((int)System.DateTime.Now.Ticks);
        render.Init(level.Grid.Tiles, GridSize);	
	}

}
