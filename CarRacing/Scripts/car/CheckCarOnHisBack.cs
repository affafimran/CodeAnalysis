// Description : CheckCarOnHisBack.cs : know if the car is on his back.  
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckCarOnHisBack : MonoBehaviour {
	public CarController 	carController;
	public bool 			b_Pause = false;
	public BoxCollider 		boxCollider;


	void Start(){
		boxCollider = gameObject.GetComponent<BoxCollider> ();
	}

	void OnTriggerEnter(Collider other) {								// Car is on his back
		if (gameObject.activeSelf && other.tag != "TriggerStart" && boxCollider.enabled && other.tag != "TriggerAI" ) {
			StartCoroutine ("CarOnBackTimer");
			//Debug.Log ("Car is on his back : " + other.name);
		}
	}

	void OnTriggerExit(Collider other) {								// Car is not on his back
		StopCoroutine("CarOnBackTimer");
	}


	IEnumerator CarOnBackTimer(){										// Car need to be respawn after a certain among of time
		float tmpTimer = 0;

		while(tmpTimer < 2){					
			if (!b_Pause) {
				tmpTimer = Mathf.MoveTowards (tmpTimer, 2, Time.deltaTime);
			}
			yield return null;
		}

		//Debug.Log("Car need to be respawn");
		carController.RespawnTheCar();
	}

	void Pause(){
		b_Pause = true;
	}
}
