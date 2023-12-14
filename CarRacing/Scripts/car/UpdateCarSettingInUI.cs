// Description : UpdateCarSettingInUI use to Update car settings when game is in pause Mode 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateCarSettingInUI : MonoBehaviour {

	public void updateAllCarInputsWhenExitInputMenu(){						// --> Update Player Inputs during the game
		GameObject[] Cars = GameObject.FindGameObjectsWithTag ("Car");		// Find all the car in the scene

		for(var i = 0; i < Cars.Length; i++){
			//Debug.Log (Cars[i].name);
			if(Cars [i].GetComponent<CarController> ())Cars [i].GetComponent<CarController> ().UpdateCarInputs ();		// Update inputs
		}
	}
}
				 