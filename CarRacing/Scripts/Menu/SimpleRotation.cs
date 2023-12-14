//Description : SimpleRotation.cs : Change the gameObject rotation
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRotation : MonoBehaviour {

	public int 		speed = 10;
	public Vector3 	dir = new Vector3(0,1,0);

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		transform.Rotate(dir * Time.deltaTime*speed, Space.World);			// Rotate the gameObject
	}
}
