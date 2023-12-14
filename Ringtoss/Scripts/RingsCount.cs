using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingsCount : MonoBehaviour {
	Game_Manager gameManager;
	// Use this for initialization
	void Start () {
		gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<Game_Manager>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter(Collider other) {
		if(other.gameObject.tag == "Ring") {
			// print("Obj Enter = " + other.gameObject.name);
			if(PlayerPrefs.GetInt("rollingBallVibration") == 1)
				Handheld.Vibrate();			
			// GameObject ring = other.transform.parent.gameObject;
			// ring.GetComponent<Rigidbody>().drag = 5;
			gameManager.noOfRingsOnRod += 1;
		}
	}

	void OnTriggerExit(Collider other) {
		if(other.gameObject.tag == "Ring") {
			// print("Obj Exit = " + other.gameObject.name);
			// GameObject ring = other.transform.parent.gameObject;
			// ring.GetComponent<Rigidbody>().drag = 0.1f;
			gameManager.noOfRingsOnRod -= 1;
		}
	}
}
