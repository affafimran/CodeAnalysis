//Description : Countdown.cs : Work in association with CountdownEditor.cs . Use to setup the countdown
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Countdown : MonoBehaviour {

	public bool 					SeeInspector 		= false;
	public bool						b_Pause 			= false;						// if true the game is on pause mode
	public bool						b_ActivateCountdown = false;						// if true the countdown is activated
	private float					Timer 				= 0;							// 
	private string					LastTimer 			= "";							// 
	public Text 					txt_Countdown;										// gameobject that display countdown


	public List<string> 			list_Txt			= new List<string> ();			// List of texts to display on screen
	public List<AudioClip> 			list_Audio			= new List<AudioClip> ();		// List of audioclip to play 
	private AudioSource				_audio;												// AudioSource component
	private Game_Manager	 		gameManager;										// access Game_Mananger component


	// Use this for initialization
	void Start () {
		Timer = list_Txt.Count;															// Every seconds the countdown display a text from the txt_Countdown list 
		_audio = GetComponent<AudioSource>();

		if (gameManager == null) {
			GameObject tmpObj = GameObject.Find ("Game_Manager");
			if (tmpObj)
				gameManager = tmpObj.GetComponent<Game_Manager> ();
		}
	}
	
	void Update(){
		if (!b_Pause && b_ActivateCountdown && Timer > 0) {
			F_Countdown();																
		}
	}


// Countdown function
	void F_Countdown (){
		Timer -= Time.deltaTime;
		string seconds = Mathf.Floor(Timer % 60).ToString("0");


		if (txt_Countdown && LastTimer != seconds && Timer >= 0) {
			int tmpINt = int.Parse (seconds);
			txt_Countdown.text = list_Txt [list_Txt.Count-1-tmpINt];

			if(_audio && list_Audio [list_Txt.Count-1-tmpINt] != null ){
				_audio.clip = list_Audio [list_Txt.Count - 1 - tmpINt];
				_audio.Play();
			}
				

			if (gameManager && seconds == "0")
				gameManager.RaceStart ();
				
		} else if (txt_Countdown && LastTimer != seconds) {
			txt_Countdown.text = "";

		}
		LastTimer = seconds;
	}


// --> use to pause or unpause the game
	public void Pause(){
		if (b_Pause) {									// -> Stop Pause
			b_Pause = false;
			_audio.UnPause();								// Pause audio
		} 
		else {											// -> Start Pause
			_audio.Pause();									// Pause audio
			b_Pause = true;
		}
	}
}
