// Descrition : InitMobileInputs.cs : Use to find which car is manage by the mobile Inputs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitMobileInputs : MonoBehaviour {
	public CarController carController;

	public void F_InitMobileButtons (GameObject car) {
		carController = car.GetComponent<CarController> ();
	}
}
