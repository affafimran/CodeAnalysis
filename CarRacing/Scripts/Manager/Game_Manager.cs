// Description : Game_Manager.cs : Manage game Rules
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Game_Manager : MonoBehaviour {
	public bool 						SeeInspector 				= false;
	public static Game_Manager 			instance 					= null;					// Access Game_Manager component
	public bool 						b_Pause 					= false;				// if true game is paused
	public bool 						b_UseCountdown				= true;					// know if countdown activated
	public Countdown 					countdown;											// countdown component
	public LapCounter 					lapcounter;											// lapcounter component


	public List<CarController> 			list_Cars	= new List<CarController> ();			// List of car 

	public Cam_Follow 					cam_P1;												// Camera used by the player 1
	public Cam_Follow 					cam_P2;												// Camera used by the Player 2
	public GameObject					obj_LineSplitScreen_Part_01;
	public GameObject					obj_LineSplitScreen_Part_02;

	public InventoryGlobalPrefs 		inventoryItemList;									// Use to activate the gameObject "Canvas_TextMode" if Test Mode is activated
	public InventoryCar 				inventoryItemCar;									// Use to instantiate cars When scene start.

	public Menu_Manager					canvas_MainMenu;									// access Menu_Manager component
	public EventSystem 					eventSystem;										// access eventsystem component
	public StandaloneInputModule 		inputModule;										// access StandaloneInputModule component
	public GameObject  					buttonRestartGame_FirstSelectedGameObject;			// access some gameObjects on scene
	public GameObject  					buttonValidateLetter_FirstSelectedGameObject;
	public GameObject					Player1Position;
	public GameObject					Player1PositionPart2;
	public GameObject					Player2Position;
	public GameObject					Player2PositionPart2;
	public GameObject					Player1LapCounter;
	public GameObject					Player2LapCounter;

	public float 						TimeBetweenCongratulationAndResultBoard = 2;		// time between the car ended the race and the moment where the result board is diplayed on screen
	public LeaderboardSystem 			leaderboard;										// access leaderboradSyestem component
	public GameObject					canvasMobileInputs;									// access the mobile input canvas gameobject

	public DefaultInputsValues 			defaultInputsValues;

	public bool 						b_TuningZone = false;

	// Use this for initialization
	void Awake () {
		PlayerPrefs.SetInt ("WeAreOnTrack", 1);																			// This PlayerPrefs = 1 : allow to display race selection when player came back to main menu
	
		if (canvas_MainMenu 
			&& inventoryItemList.inventoryItem[0].b_TestMode == false
			/*&& PlayerPrefs.GetInt ("TestMode") == 0 */
			&& !b_TuningZone)													// If we are not in test mode
			canvas_MainMenu.GoToOtherPageWithHisNumber (7);																// Display Loading_Page

		if(instance ==  null){
			instance = this;
		}
		else if(instance != this){
			Destroy(gameObject);
		}
		if (!PlayerPrefs.HasKey ("PP_1_Desktop_Left")) {																// If no Input have been setup, init inputs
			DefaultInputsParameters ();
		}

		if (countdown == null) {																						// Access coundown gameobject component
			if (gameObject.GetComponent<Countdown>())
				countdown = GetComponent<Countdown> ();
		}

		if (lapcounter == null) {																						// Access coundown gameobject component
			GameObject objLapCounter = GameObject.Find("StartLine_lapCounter");
			if (objLapCounter)
				lapcounter = objLapCounter.GetComponent<LapCounter> ();
		}

		if (/*PlayerPrefs.GetInt ("TestMode") == 1 */
			inventoryItemList.inventoryItem[0].b_TestMode == true														// --> Demo Mode is activated
			&& inventoryItemList != null) {																					// --> Feedback on screen : to say that the game is in Test Mode
			GameObject objTextModeTest = inventoryItemList.inventoryItem [0].Canvas_TestMode;
			if (objTextModeTest) {
				GameObject instance = Instantiate (objTextModeTest) as GameObject;
				instance.name = "Canvas_TestMode";
			}
		} 
	}

	void Start(){
		if (lapcounter != null) {
			if (Player1LapCounter)
				Player1LapCounter.GetComponent<Text> ().text = "Lap 1/" + lapcounter.lapNumber;
			if (Player2LapCounter)
				Player2LapCounter.GetComponent<Text> ().text = "Lap 1/" + lapcounter.lapNumber;
		}


		if(inventoryItemCar)b_UseCountdown = inventoryItemCar.b_Countdown;

		if (/*PlayerPrefs.GetInt ("TestMode") == 0 */
			inventoryItemList.inventoryItem[0].b_TestMode == false														// --> Nomal Mode
			&& !b_TuningZone) {
			InitGame ();
		} else {																										// --> Demo Mode is activated
			CreateCarList ();																								// Create car list
			if (countdown && b_UseCountdown) {
				countdown.b_ActivateCountdown = true;																	// --> Start countdown if needed
			}										
			else {
				for (var i = 0; i < list_Cars.Count; i++) {
					if(list_Cars [i])list_Cars [i].b_CountdownActivate = false;											// car could move
				}
			}

			if (b_TuningZone) {
				if (list_Cars [0] != null) {
					list_Cars [0].b_InitInputWhenGameStart = false;
					list_Cars [0].b_CountdownActivate = false;
				}
			}

			if (list_Cars [1] != null && cam_P2 && list_Cars [1].gameObject.GetComponent<CarAI> ().enabled == false) {	// 2 Players
				cam_P2.gameObject.SetActive (true);																		// ACtivate cam 2
				cam_P2.InitCamera(list_Cars [1],false);
			}
			if (list_Cars [0] != null && cam_P1) {
				if (list_Cars [1] != null && list_Cars [1].gameObject.GetComponent<CarAI> ().enabled == false) {
					cam_P1.InitCamera (list_Cars [0], true);															// Display Split Screen
					if(obj_LineSplitScreen_Part_01)obj_LineSplitScreen_Part_01.SetActive(true);
					if(obj_LineSplitScreen_Part_02)obj_LineSplitScreen_Part_02.SetActive(true);
				} else {
					cam_P1.InitCamera (list_Cars [0], false);															// Single Camera
					if(obj_LineSplitScreen_Part_01)obj_LineSplitScreen_Part_01.SetActive(false);
					if(obj_LineSplitScreen_Part_02)obj_LineSplitScreen_Part_02.SetActive(false);
				}
			}

			if (canvas_MainMenu 
				&& inventoryItemList.inventoryItem[0].b_TestMode == false
				/*&& PlayerPrefs.GetInt ("TestMode") == 0 */
				&& !b_TuningZone)
				canvas_MainMenu.GoToOtherPageWithHisNumber (5);															// Display Pause_Page
			
		}
	}

	/*void Update(){
		if(Input.GetKeyDown(KeyCode.P)){
			if(!b_Pause)b_Pause = true;
			else b_Pause = false;
		}

	}*/

// --> The Race starts
	public void RaceStart(){
		for (var i = 0; i < list_Cars.Count; i++) {
			if(list_Cars [i])list_Cars [i].b_CountdownActivate = false;									// car could move
		}

		if (lapcounter)																					// Start the timer in LapCOunter Script
			lapcounter.startTimer = true;
	}

// --> Race is finished
	public void RaceIsFinished(){
		if (inputModule)
			inputModule.submitButton = "Submit";

		if (canvasMobileInputs)
			canvasMobileInputs.SetActive (false);

		if (PlayerPrefs.GetString ("Which_GameMode") == "Arcade") {	
			StartCoroutine ("WinProcessARcade");
		} else {
			StartCoroutine ("WinProcessTimeTrial");
		}
	}

// --> Arcade race is finished. Display result Page
	IEnumerator WinProcessARcade(){
		if(canvas_MainMenu)canvas_MainMenu.GoToOtherPageWithHisNumber (10);								// Congratulation page
		yield return new WaitForSeconds (TimeBetweenCongratulationAndResultBoard);
		if(canvas_MainMenu)canvas_MainMenu.GoToOtherPageWithHisNumber (6);								// Time for each car page
		if(eventSystem)eventSystem.SetSelectedGameObject (buttonRestartGame_FirstSelectedGameObject);
	}

// --> Trial Mode is finished. Display result Page
	IEnumerator WinProcessTimeTrial(){
		if(canvas_MainMenu)canvas_MainMenu.GoToOtherPageWithHisNumber (10);								// Congratulation page
		yield return new WaitForSeconds (TimeBetweenCongratulationAndResultBoard);
		if(canvas_MainMenu)canvas_MainMenu.GoToOtherPageWithHisNumber (9);								// Save your name and score page
		if(leaderboard)leaderboard.InitPlayerScore();

		if(eventSystem)eventSystem.SetSelectedGameObject (buttonValidateLetter_FirstSelectedGameObject);
	}


// --> Find car on scene and init Car list
	void CreateCarList(){
		GameObject[] arrCars = GameObject.FindGameObjectsWithTag("Car");							

		for (var i = 0; i < 4; i++) {																	// Create the array with needed size
			list_Cars.Add (null);
		}

		foreach (GameObject carFind in arrCars) {														// Put the car in the player order 1,2,3,4 on the array
			if (carFind.GetComponent<CarController> ()) {
				if (inventoryItemCar.b_mobile) {
					carFind.GetComponent<CarController> ().offsetSpeedForMobile = inventoryItemCar.mobileMaxSpeedOffset;
					carFind.GetComponent<CarController> ().offsetRotationForMobile = inventoryItemCar.mobileWheelStearingOffsetReactivity;
				}
				list_Cars [carFind.GetComponent<CarController> ().playerNumber - 1] = carFind.GetComponent<CarController> ();

			}
		}



		GameObject[] arrTriggerAI = GameObject.FindGameObjectsWithTag("TriggerAI");	
		foreach (GameObject triggerAI in arrTriggerAI) {														
			triggerAI.GetComponent<TriggersAI> ().InitTriggersAI(list_Cars);
		}
	}


	void InitGame (){
		StartCoroutine ("I_InitGame");
	}


	IEnumerator I_InitGame(){
		Debug.Log (
			"Game Mode : " + PlayerPrefs.GetString ("Which_GameMode") +
			"\nHowManyPlayers : " + PlayerPrefs.GetInt ("HowManyPlayers") +
			"\nDifficulty : " + PlayerPrefs.GetInt ("DifficultyChoise") +
			"\nCar 01 : " + PlayerPrefs.GetInt ("Player_0_CarLastSelection") +
			"\nCar 02 : " + PlayerPrefs.GetInt ("Player_1_CarLastSelection") +
			"\nCar 03 : " + PlayerPrefs.GetInt ("Player_2_CarLastSelection") +
			"\nCar 03 : " + PlayerPrefs.GetInt ("Player_3_CarLastSelection")); 

// --> Find car on scene
		GameObject[] arrCars = GameObject.FindGameObjectsWithTag ("Car");							

		foreach (GameObject carFind in arrCars) {																// if there are cars on scene delete them before creating the needed car for this race
			if (carFind.GetComponent<CarController> ()) {
				carFind.gameObject.SetActive (false);
				Destroy (carFind);
			}
		}

		if (PlayerPrefs.GetString ("Which_GameMode") == "Arcade") {												// ARcade mode is selected
			for (var i = 0; i < 4; i++) {
				GameObject instance = Instantiate (inventoryItemCar.inventoryItem [i].Cars [PlayerPrefs.GetInt ("Player_" + i + "_CarLastSelection")]);

				instance.name = instance.name.Replace ("(Clone)", "");

				GameObject tmpPosition = GameObject.Find ("Start_Position_0" + (i + 1));

				if (tmpPosition) {
					instance.GetComponent<CarController> ().playerNumber = i + 1;								// Select the player number

					if(i==0)instance.GetComponent<CarAI> ().enabled = false;									// Car 1 control by Player 1

					if(i==1 && PlayerPrefs.GetInt ("HowManyPlayers") == 2){
						instance.GetComponent<CarAI> ().enabled = false;										// Car 2 control by Player 2
						if(Player2Position)Player2Position.SetActive(true);
						if(Player2PositionPart2)Player2PositionPart2.SetActive(true);
						if(Player2LapCounter)Player2LapCounter.SetActive(true);
					}

					if(i==1 && PlayerPrefs.GetInt ("HowManyPlayers") == 1){
						instance.GetComponent<CarAI> ().enabled = true;											// Car 2 control by CPU
						if(Player2Position)Player2Position.SetActive(false);
						if(Player2PositionPart2)Player2PositionPart2.SetActive(false);
						if(Player2LapCounter)Player2LapCounter.SetActive(false);
					}

					if(i>1)instance.GetComponent<CarAI> ().enabled = true;										// Car 3 and 4 control by CPU

					if (tmpPosition) {
						instance.transform.position = new Vector3 (tmpPosition.transform.position.x, 
							tmpPosition.transform.position.y + .15f, tmpPosition.transform.position.z);
						instance.transform.eulerAngles = tmpPosition.transform.eulerAngles;
					}
					while (!instance) {
					}
				}
			}
		} 
		else if (PlayerPrefs.GetString ("Which_GameMode") == "TimeTrial") {										// Time Trial is selected

			GameObject instance = Instantiate (inventoryItemCar.inventoryItem [0].Cars [PlayerPrefs.GetInt ("Player_0_CarLastSelection")]);
			instance.name = instance.name.Replace ("(Clone)", "");
			GameObject tmpPosition = GameObject.Find ("Start_Position_04");
			instance.GetComponent<CarController> ().playerNumber = 1;											// Select the player number
			instance.GetComponent<CarAI> ().enabled = false;													// Car 1 control by Player 1
			if (tmpPosition) {
				instance.transform.position = new Vector3 (tmpPosition.transform.position.x, 
					tmpPosition.transform.position.y + .15f, tmpPosition.transform.position.z);
				instance.transform.eulerAngles = tmpPosition.transform.eulerAngles;
			}
			while (!instance) {}

			if(Player1Position)Player1Position.SetActive(false);
			if(Player1PositionPart2)Player1PositionPart2.SetActive(false);
			if(Player2Position)Player2Position.SetActive(false);
			if(Player2PositionPart2)Player2PositionPart2.SetActive(false);

			if(Player2LapCounter)Player2LapCounter.SetActive(false);
		}
			
		CreateCarList ();																						// Create car list
		if(lapcounter)lapcounter.InitCar ();																					// Init Lap counter

		if (countdown && b_UseCountdown) {countdown.b_ActivateCountdown = true;}								// Start countdown if needed

		if (list_Cars [1] != null && cam_P2 && list_Cars [1].gameObject.GetComponent<CarAI> ().enabled == false) {
			cam_P2.gameObject.SetActive (true);
			cam_P2.InitCamera(list_Cars [1],false);
		}

		if (list_Cars [0] != null && cam_P1) {
			if (list_Cars [1] != null && list_Cars [1].gameObject.GetComponent<CarAI> ().enabled == false) {
				cam_P1.InitCamera (list_Cars [0], true);														// Display Split Screen
				if(obj_LineSplitScreen_Part_01)obj_LineSplitScreen_Part_01.SetActive(true);
				if(obj_LineSplitScreen_Part_02)obj_LineSplitScreen_Part_02.SetActive(true);

			} else {
				cam_P1.InitCamera (list_Cars [0], false);														// Single Camera
				if(obj_LineSplitScreen_Part_01)obj_LineSplitScreen_Part_01.SetActive(false);
				if(obj_LineSplitScreen_Part_02)obj_LineSplitScreen_Part_02.SetActive(false);
			}
		}

		if (canvas_MainMenu 
			&& inventoryItemList.inventoryItem[0].b_TestMode == false
			/*&& PlayerPrefs.GetInt ("TestMode") == 0 */
			&& !b_TuningZone)
			canvas_MainMenu.GoToOtherPageWithHisNumber (5);														// Display Pause_Page

		return null;
	}




// --> Use to initialize all the inputs values Player 1 and Player 2 , Keyboard and gamepad
	public void DefaultInputsParameters(){																					

		if (defaultInputsValues != null) {
			InputsData ();
		}
		else{
			InputsNoData ();
		}
	}

	void InputsNoData(){
		// -> Init Input Player 1 Joystick
		PlayerPrefs.SetString ("PP_1_Pad_Left", 		"Joystick1Axis1");										
		PlayerPrefs.SetString ("PP_1_Pad_Right", 		"Joystick1Axis1");
		PlayerPrefs.SetString ("PP_1_Pad_Acceleration", "Joystick1Axis6");
		PlayerPrefs.SetString ("PP_1_Pad_Break", 		"Joystick1Axis5");
		PlayerPrefs.SetString ("PP_1_Pad_Other", 		"");
		PlayerPrefs.SetString ("PP_1_Pad_Respawn", 		"Joystick1Button19");
		PlayerPrefs.SetString ("PP_1_Pad_Validate", 	"Joystick1Button16");
		PlayerPrefs.SetString ("PP_1_Pad_Back",		 	"Joystick1Button17");
		PlayerPrefs.SetString ("PP_1_Pad_Pause", 		"Joystick1Button9");


		// -> Init Keyboard buttons Player 1
		PlayerPrefs.SetString ("PP_1_Desktop_Left", 		"LeftArrow");
		PlayerPrefs.SetString ("PP_1_Desktop_Right", 		"RightArrow");
		PlayerPrefs.SetString ("PP_1_Desktop_Acceleration", "UpArrow");
		PlayerPrefs.SetString ("PP_1_Desktop_Break", 		"DownArrow");
		PlayerPrefs.SetString ("PP_1_Desktop_Other", 		"");
		PlayerPrefs.SetString ("PP_1_Desktop_Respawn", 		"H");
		PlayerPrefs.SetString ("PP_1_Desktop_Validate", 	"");
		PlayerPrefs.SetString ("PP_1_Desktop_Back", 		"");
		PlayerPrefs.SetString ("PP_1_Desktop_Pause", 		"");



		// -> Init Input Player 2 Joystick
		PlayerPrefs.SetString ("PP_2_Pad_Left", 		"Joystick2Axis1");										
		PlayerPrefs.SetString ("PP_2_Pad_Right", 		"Joystick2Axis1");
		PlayerPrefs.SetString ("PP_2_Pad_Acceleration", "Joystick2Axis6");
		PlayerPrefs.SetString ("PP_2_Pad_Break", 		"Joystick2Axis5");
		PlayerPrefs.SetString ("PP_2_Pad_Other", 		"");
		PlayerPrefs.SetString ("PP_2_Pad_Respawn", 		"Joystick2Button19");
		PlayerPrefs.SetString ("PP_2_Pad_Validate", 	"Joystick2Button16");
		PlayerPrefs.SetString ("PP_2_Pad_Back",		 	"Joystick2Button17");
		PlayerPrefs.SetString ("PP_2_Pad_Pause", 		"Joystick2Button9");


		// -> Init Keyboard buttons Player 2
		PlayerPrefs.SetString ("PP_2_Desktop_Left", 		"S");
		PlayerPrefs.SetString ("PP_2_Desktop_Right", 		"F");
		PlayerPrefs.SetString ("PP_2_Desktop_Acceleration", "E");
		PlayerPrefs.SetString ("PP_2_Desktop_Break", 		"D");
		PlayerPrefs.SetString ("PP_2_Desktop_Other", 		"");
		PlayerPrefs.SetString ("PP_2_Desktop_Respawn", 		"C");
		PlayerPrefs.SetString ("PP_2_Desktop_Validate", 	"");
		PlayerPrefs.SetString ("PP_2_Desktop_Back", 		"");
		PlayerPrefs.SetString ("PP_2_Desktop_Pause", 		"");
	}
	public void InputsData(){	
		// -> Init Input Player 1 Joystick
		PlayerPrefs.SetString ("PP_1_Pad_Left", 		defaultInputsValues.ListOfInputs[0].Gamepad[0]);										
		PlayerPrefs.SetString ("PP_1_Pad_Right", 		defaultInputsValues.ListOfInputs[0].Gamepad[1]);
		PlayerPrefs.SetString ("PP_1_Pad_Acceleration",defaultInputsValues. ListOfInputs[0].Gamepad[2]);
		PlayerPrefs.SetString ("PP_1_Pad_Break", 		defaultInputsValues.ListOfInputs[0].Gamepad[3]);
		PlayerPrefs.SetString ("PP_1_Pad_Other", 		"");
		PlayerPrefs.SetString ("PP_1_Pad_Respawn", 		defaultInputsValues.ListOfInputs[0].Gamepad[5]);
		PlayerPrefs.SetString ("PP_1_Pad_Validate", 	defaultInputsValues.ListOfInputs[0].Gamepad[6]);
		PlayerPrefs.SetString ("PP_1_Pad_Back",		 	defaultInputsValues.ListOfInputs[0].Gamepad[7]);
		PlayerPrefs.SetString ("PP_1_Pad_Pause", 		defaultInputsValues.ListOfInputs[0].Gamepad[8]);


		// -> Init Keyboard buttons Player 1
		PlayerPrefs.SetString ("PP_1_Desktop_Left", 		defaultInputsValues.ListOfInputs[0].Desktop[0]);
		PlayerPrefs.SetString ("PP_1_Desktop_Right", 		defaultInputsValues.ListOfInputs[0].Desktop[1]);
		PlayerPrefs.SetString ("PP_1_Desktop_Acceleration",defaultInputsValues. ListOfInputs[0].Desktop[2]);
		PlayerPrefs.SetString ("PP_1_Desktop_Break", 		defaultInputsValues.ListOfInputs[0].Desktop[3]);
		PlayerPrefs.SetString ("PP_1_Desktop_Other", 		"");
		PlayerPrefs.SetString ("PP_1_Desktop_Respawn", 		defaultInputsValues.ListOfInputs[0].Desktop[5]);
		PlayerPrefs.SetString ("PP_1_Desktop_Validate", 	"");
		PlayerPrefs.SetString ("PP_1_Desktop_Back", 		"");
		PlayerPrefs.SetString ("PP_1_Desktop_Pause", 		"");

		// -> Init Input Player 2 Joystick
		PlayerPrefs.SetString ("PP_2_Pad_Left", 		defaultInputsValues.ListOfInputs[1].Gamepad[0]);										
		PlayerPrefs.SetString ("PP_2_Pad_Right", 		defaultInputsValues.ListOfInputs[1].Gamepad[1]);
		PlayerPrefs.SetString ("PP_2_Pad_Acceleration", defaultInputsValues.ListOfInputs[1].Gamepad[2]);
		PlayerPrefs.SetString ("PP_2_Pad_Break", 		defaultInputsValues.ListOfInputs[1].Gamepad[3]);
		PlayerPrefs.SetString ("PP_2_Pad_Other", 		"");
		PlayerPrefs.SetString ("PP_2_Pad_Respawn", 		defaultInputsValues.ListOfInputs[1].Gamepad[5]);
		PlayerPrefs.SetString ("PP_2_Pad_Validate", 	defaultInputsValues.ListOfInputs[1].Gamepad[6]);
		PlayerPrefs.SetString ("PP_2_Pad_Back",		 	defaultInputsValues.ListOfInputs[1].Gamepad[7]);
		PlayerPrefs.SetString ("PP_2_Pad_Pause", 		defaultInputsValues.ListOfInputs[1].Gamepad[8]);


		// -> Init Keyboard buttons Player 2
		PlayerPrefs.SetString ("PP_2_Desktop_Left", 		defaultInputsValues.ListOfInputs[1].Desktop[0]);
		PlayerPrefs.SetString ("PP_2_Desktop_Right", 		defaultInputsValues.ListOfInputs[1].Desktop[1]);
		PlayerPrefs.SetString ("PP_2_Desktop_Acceleration", defaultInputsValues.ListOfInputs[1].Desktop[2]);
		PlayerPrefs.SetString ("PP_2_Desktop_Break", 		defaultInputsValues.ListOfInputs[1].Desktop[3]);
		PlayerPrefs.SetString ("PP_2_Desktop_Other", 		"");
		PlayerPrefs.SetString ("PP_2_Desktop_Respawn", 		defaultInputsValues.ListOfInputs[1].Desktop[5]);
		PlayerPrefs.SetString ("PP_2_Desktop_Validate", 	"");
		PlayerPrefs.SetString ("PP_2_Desktop_Back", 		"");
		PlayerPrefs.SetString ("PP_2_Desktop_Pause", 		"");

	}
}
