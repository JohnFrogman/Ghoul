using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour {

	void Update(){
		if (Input.GetMouseButton(0))
			HandleInput();
	}
	void HandleInput(){
		Ray inputRay = Camera.main.ScreenPointToRay (Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast (inputRay, out hit))
			Debug.Log ("touched " + hit.point);
	}
}
