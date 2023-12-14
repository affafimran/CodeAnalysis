// Description : TrackSelection : Use to  Init the Main menu when you came back from the track selection
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TrackSelection : MonoBehaviour {

	public GameObject 		J2;
	public GameObject 		J3;
	public GameObject 		J4;
	public CarSelection		carSelection;
	public GameObject 		button_Trial;
	public Text 			TextPlayer2;
	public GameObject 		Buttons_Choose_Car;


	/*public void Start(){
		Debug.Log (
			PlayerPrefs.GetString ("Which_GameMode") + " : " + 
			PlayerPrefs.GetInt ("HowManyPlayers") + " : " +
			PlayerPrefs.GetString ("Player_0_Car") + " : " +
			PlayerPrefs.GetString ("Player_1_Car") + " : " +
			PlayerPrefs.GetString ("Player_2_Car") + " : " +
			PlayerPrefs.GetString ("Player_3_Car"));

	}*/

// --> Init the Main menu when you came back from the track selection
	public void BackFromTrackSelection(){
		if (PlayerPrefs.GetString ("Which_GameMode") == "Arcade") {												// ARcade mode is selected
			if (J2)J2.SetActive (true);
			if (J3)J3.SetActive (true);
			if (J4)J4.SetActive (true);
		} else if (PlayerPrefs.GetString ("Which_GameMode") == "TimeTrial") {									// Time Trial is selected
			if (J2)J2.SetActive (false);
			if (J3)J3.SetActive (false);
			if (J4)J4.SetActive (false);
		}


		if (PlayerPrefs.GetInt ("HowManyPlayers") == 1) {														// Button solo on Page Hub Menu
			if (button_Trial)button_Trial.SetActive (true);
			if (TextPlayer2)TextPlayer2.text = "CPU";
			if (Buttons_Choose_Car)Buttons_Choose_Car.SetActive (false);
		}
		else if(PlayerPrefs.GetInt ("HowManyPlayers") == 2){													// Button Versus on Page Hub Menu
			if (button_Trial)button_Trial.SetActive (false);
			if (TextPlayer2)TextPlayer2.text = "P2";
			if (Buttons_Choose_Car)Buttons_Choose_Car.SetActive (true);
		}

		if(carSelection)carSelection.initCarSelectionFromTrackSelection ();
	}
}
