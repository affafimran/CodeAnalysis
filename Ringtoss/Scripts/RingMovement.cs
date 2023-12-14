using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingMovement : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = new Vector3((Input.acceleration.x * 15 * Time.deltaTime) + transform.position.x, transform.position.y, transform.position.z);
	}
}
