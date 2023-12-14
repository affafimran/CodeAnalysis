// Description : Cam_Follow.cs : use on camera to follow the cars
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cam_Follow : MonoBehaviour {

	// The target we are following
	public Transform 	target;
	// The distance in the x-z plane to the target
	public float 		distance = 10.0f;
	// the height we want the camera to be above the target
	public float 		height = 5.0f;
	public float 		rotationDamping;		
	public float 		heightDamping;

	public bool 		b_Pause = false;	

	public bool 		b_Find_Target_Automatically = false;

	void Start(){
		if (b_Find_Target_Automatically) {
			GameObject[] arrCars = GameObject.FindGameObjectsWithTag("Car");							

			foreach (GameObject carFind in arrCars) {														// Put the car in the player order 1,2,3,4 on the array
				if (carFind.GetComponent<CarController> ())
					target = carFind.GetComponent<CarController> ().camTarget.transform;
			}
		}
	}

	void FixedUpdate () {
		if (!b_Pause && target) {
			// Calculate the current rotation angles
			float wantedRotationAngle = target.eulerAngles.y;
			float wantedHeight = target.position.y + height;

			float currentRotationAngle = transform.eulerAngles.y;
			float currentHeight = transform.position.y;


			// Damp the rotation around the y-axis
			currentRotationAngle = Mathf.LerpAngle (currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);

			// Damp the height
			currentHeight = Mathf.Lerp (currentHeight, wantedHeight, heightDamping * Time.deltaTime);

			// Convert the angle into a rotation
			Quaternion currentRotation = Quaternion.Euler (0, currentRotationAngle, 0);

			// Set the position of the camera on the x-z plane to:
			// distance meters behind the target
			transform.position = target.position;
			transform.position -= currentRotation * Vector3.forward * distance;

			// Set the height of the camera
			transform.position = new Vector3 (transform.position.x, currentHeight, transform.position.z);

			// Always look at the target
			transform.LookAt (target);
		}
	}

// --> Init camera position when the scene starts
	public void InitCamera(CarController car, bool b_Splitscreen){
		target = car.camTarget.transform;
		if (b_Splitscreen) {
			Camera cam = GetComponent<Camera> ();
			cam.rect = new Rect(new Vector2(0,0),new Vector2(.5f,1));

		}
	}

// --> Pause the camera in Pause Mode
	public void Pause(){
		if (!b_Pause) {
			b_Pause = true;
		} else {
			b_Pause = false;
		}
	}
}
