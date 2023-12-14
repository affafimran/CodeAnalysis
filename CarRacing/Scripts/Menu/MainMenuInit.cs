// Description : MainMenuInit.cs : Init Menu when the amin Menu start
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenuInit : MonoBehaviour {

	public Menu_Manager menuManager;
	public EventSystem eventSystem;
	public GameObject  Case01_FirstSelectedGameObject;
	public GameObject  Case02_FirstSelectedGameObject;
	public GameObject  leaderboard;
	public GameObject  leaderboardShadow;
	public GameObject  difficulty_01;
	public GameObject  difficulty_02;
	public GameObject  difficulty_03;
	public GameObject  difficulty_04;

	void Awake () {
		if (PlayerPrefs.GetInt ("WeAreOnTrack") == 1) {							// --> If player come from a circuit
			menuManager.GoToOtherPageWithHisNumber (2); 							// Open trackSelection page when scene start
			eventSystem.SetSelectedGameObject (Case01_FirstSelectedGameObject);
		} else {																// --> If scene start after the application launch
			menuManager.GoToOtherPageWithHisNumber (0); 							// Open Hub page when scene start
			eventSystem.SetSelectedGameObject (Case02_FirstSelectedGameObject);
		}
		PlayerPrefs.SetInt ("WeAreOnTrack", 0);

		if (PlayerPrefs.GetString ("Which_GameMode") == "TimeTrial") {			// --> Activate Leadeboard if TimeTrial Mode is activated
			if(leaderboard)leaderboard.SetActive(true);
			if(leaderboardShadow)leaderboardShadow.SetActive(true);
			if(difficulty_01)difficulty_01.SetActive(false);
			if(difficulty_02)difficulty_02.SetActive(false);
			if(difficulty_03)difficulty_03.SetActive(false);
			if(difficulty_04)difficulty_04.SetActive(false);
		} else {
			if(leaderboard)leaderboard.SetActive(false);
			if(leaderboardShadow)leaderboardShadow.SetActive(false);
			if(difficulty_01)difficulty_01.SetActive(true);
			if(difficulty_02)difficulty_02.SetActive(true);
			if(difficulty_03)difficulty_03.SetActive(true);
			if(difficulty_04)difficulty_04.SetActive(true);
		}
	}
}
