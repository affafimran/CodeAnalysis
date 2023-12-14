// Description : TimeResult.cs : Allow to display car name and car time when a race is finished
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeResult : MonoBehaviour {
	public List<Text> 	Player_Name = new List<Text>(); 							// gameObjects to display car name for each car
	public List<Text> 	Player_Time = new List<Text>(); 							// gameObjects to display time for each car
	public List<bool> 	TimeIsCalulated = new List<bool>(); 						// Calculate the time one time for each car
	public LapCounter 	lapCounter;													// access component

	public string 		TextWhenPlayerIsInRace = "Waiting...";						// the text dispaly on screen when a has not ended the race
	public bool 		useCarName = false;											// if True : The name of the car prefab is used on score board

	public string 		stringForPlayerText = "P";									// Text for player 1 or 2
	public string 		stringForCPUText = "CPU ";									// Text for CPU 2,3,4


	// Update is called once per frame
	void Update () {
		if (lapCounter != null) {
			for (var i = 0; i < Player_Name.Count; i++) {
				if (lapCounter.carPosition [i] == 1) {
					PlayerName (0, i);
				}
				if (lapCounter.carPosition [i] == 2) {
					PlayerName (1, i);
				}
				if (lapCounter.carPosition [i] == 3) {
					PlayerName (2, i);
				}
				if (lapCounter.carPosition [i] == 4) {
					PlayerName (3, i);
				}
			}
		}
	}

	void PlayerName (int numText, int value){
		if (lapCounter.carController [value] != null
			&& lapCounter.carController [value].playerNumber == 1) {											// --> Player 1

			if (useCarName)
				Player_Name [numText].text = lapCounter.carController [value].name;									// display name
			else
				Player_Name [numText].text = stringForPlayerText + 1;					

		}
		else if (
			lapCounter.carController [value] != null
			&& lapCounter.carController [value].playerNumber == 2
			&& !lapCounter.carController [value].b_AutoAcceleration) {											// --> Player 2

			if (useCarName)
				Player_Name [numText].text = lapCounter.carController [value].name;									// display name
			else
				Player_Name [numText].text = stringForPlayerText + 2;

		}
		else {

			if (useCarName)
				Player_Name [numText].text = lapCounter.carController [value].name;									// display name
			else
				Player_Name [numText].text = stringForCPUText;		

		}

		if (lapCounter.raceFinished [value]) {
			Player_Time [numText].text = F_Timer (lapCounter.carTime [value]);									// display time
			//TimeIsCalulated [i] = true;
		} else {
			Player_Time [numText].text = TextWhenPlayerIsInRace;													// display text if the car has not finished the race
		}

	}


// Format result to 00:00:00
	string F_Timer (float value){
		//value += Time.deltaTime;
		string minutes = "";
		if(Mathf.Floor(value / 60) > 0 && Mathf.Floor(value / 60) < 10)
			minutes = Mathf.Floor(value / 60).ToString("0");

		if(Mathf.Floor(value / 60) >  10)
			minutes = Mathf.Floor(value / 60).ToString("00");


		string seconds = Mathf.Floor(value % 60).ToString("00");
		string milliseconds = Mathf.Floor((value*1000) % 1000).ToString("000");

		string result = "";
		if(Mathf.Floor(value / 60) == 0)
			result = seconds + ":" + milliseconds;
		else
			result = minutes + ":" + seconds + ":" + milliseconds;

		return result;
	}
}
