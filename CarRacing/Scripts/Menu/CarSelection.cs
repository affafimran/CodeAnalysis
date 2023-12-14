// Description : CarSelection.cs : Find this scripit on gameObject "CheckCarSelection" on Canvas_MainMenu. Use to select and save info about car when a player choose his car on Page_CarSelection 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CarSelection : MonoBehaviour {
	public bool 								SeeInspector = false;

	[System.Serializable]
	public class _ListOfCars{
		public int currentCarSelection = 0;
		public GameObject pivotToSpawnCar;																	// pivot to spawn the car
		//public List<GameObject> Cars = new List<GameObject>();												// List of cars for players or CPUs

		public _ListOfCars(){}
	}

	public List<_ListOfCars> 					ListOfCars = new List<_ListOfCars>();						//


	private EventSystem							eventSystem; 												// access the evenSystem 
	private List<string>						l_gamepadNames = new List<string>();						// list of gamepads name
	private List<bool>							l_gamepadActivateJoystick = new List<bool>();				// list to know if axis are pressed for gamepad

	public GameObject 							parentMenu;													// It is possible to change the car only if Page_CarSelection is activated	

	public InventoryCar							inventoryItemCar;												// Inventory of cars


// Use this for initialization
	void Start () {
		eventSystem = GameObject.Find ("EventSystem").GetComponent<EventSystem> ();								// Access EventSystem
		if (!eventSystem)Debug.Log ("MCR Creator : You need an EventSystem gameObject on your scene");
		updateInputs ();																						// Know how many gamepad are connected and those names																			

	}
	


// Update is called once per frame
	void Update () {
		if (parentMenu.activeSelf) {
			for (int i = 0; i < Input.GetJoystickNames ().Length; i++) {																			// --> Check if player an axis on his gamepad
				if (i < 2) {																														// Prevent bug if more than 2 gamepad connected.Limit to 2 gamepad.
					for (int j = 1; j < 11; j++) {
						if (Input.GetAxis ("Joystick" + (i + 1).ToString () + "Axis" + j) > .5F &&
						   !l_gamepadActivateJoystick [i] && PlayerPrefs.GetString ("PP_" + (i + 1) + "_Pad_Right") == "Joystick" + (i + 1).ToString () + "Axis" + j) {					// -> Axis is detected	(Right)
							//Debug.Log ("here");
							l_gamepadActivateJoystick [i] = true;
							ListOfCars [i].currentCarSelection = (ListOfCars [i].currentCarSelection + 1) % inventoryItemCar.inventoryItem [i].Cars.Count;
							LoadNewCar (i);																																					// load a new car
						} else if (Input.GetAxis ("Joystick" + (i + 1).ToString () + "Axis" + j) < -.5F &&
						        !l_gamepadActivateJoystick [i] && PlayerPrefs.GetString ("PP_" + (i + 1) + "_Pad_Left") == "Joystick" + (i + 1).ToString () + "Axis" + j) {					// -> Axis is detected	(Left)
							l_gamepadActivateJoystick [i] = true;
							ListOfCars [i].currentCarSelection--;
							if (ListOfCars [i].currentCarSelection < 0)
								ListOfCars [i].currentCarSelection = inventoryItemCar.inventoryItem [i].Cars.Count - 1;
							LoadNewCar (i);																																					// load a new car
						} else if ((Input.GetAxisRaw ("Joystick" + (i + 1).ToString () + "Axis" + j) < .5F && Input.GetAxisRaw ("Joystick" + (i + 1).ToString () + "Axis" + j) >= 0 ||	// -> Axis released
						        Input.GetAxisRaw ("Joystick" + (i + 1).ToString () + "Axis" + j) > -.5F && Input.GetAxisRaw ("Joystick" + (i + 1).ToString () + "Axis" + j) <= 0)
						        &&

						        (l_gamepadActivateJoystick [i] && PlayerPrefs.GetString ("PP_1_Pad_Left") == "Joystick" + (i + 1).ToString () + "Axis" + j ||
						        l_gamepadActivateJoystick [i] && PlayerPrefs.GetString ("PP_2_Pad_Left") == "Joystick" + (i + 1).ToString () + "Axis" + j)) {		
							l_gamepadActivateJoystick [i] = false;
						}
					}
				}
			}

			if (Input.anyKeyDown) { 																												// --> A button is pressed 

				string tmpInputName = FindTheKeyCodeUpdate ().ToString ();
				l_gamepadNames.Clear ();																										// Save on a list : gamepads names
				for (int i = 0; i < Input.GetJoystickNames ().Length; i++) {
					l_gamepadNames.Add (Input.GetJoystickNames () [i] + " : " + i.ToString ());
				}

				for (int i = 0; i < 2; i++) {																									// --> Check Keyboard
					if (i == 0 || i == 1 && PlayerPrefs.GetInt ("HowManyPlayers") == 2) {										// Time Trial is selected
						if (tmpInputName == PlayerPrefs.GetString ("PP_" + (i + 1) + "_Desktop_Left")// Keyboard Left Button
							|| (i == 0 && PlayerPrefs.GetString ("PP_1_Desktop_Left") != "LeftArrow" && tmpInputName == "LeftArrow")) {													
							ListOfCars [i].currentCarSelection--;
							if (ListOfCars [i].currentCarSelection < 0)
								ListOfCars [i].currentCarSelection = inventoryItemCar.inventoryItem [i].Cars.Count - 1;
							LoadNewCar (i);																													// load a new car
						}
						if (tmpInputName == PlayerPrefs.GetString ("PP_" + (i + 1) + "_Desktop_Right")// Keyboard Right Button
						   || (i == 0 && PlayerPrefs.GetString ("PP_1_Desktop_Right") != "RightArrow" && tmpInputName == "RightArrow")) {													
							ListOfCars [i].currentCarSelection = (ListOfCars [i].currentCarSelection + 1) % inventoryItemCar.inventoryItem [i].Cars.Count;
							LoadNewCar (i);																													// load a new car
						}

						if (tmpInputName == PlayerPrefs.GetString ("PP_" + (i + 1) + "_Pad_Left")) {														// gamepad Left Button
							ListOfCars [i].currentCarSelection--;
							if (ListOfCars [i].currentCarSelection < 0)
								ListOfCars [i].currentCarSelection = inventoryItemCar.inventoryItem [i].Cars.Count - 1;
							LoadNewCar (i);																													// load a new car
						}
						if (tmpInputName == PlayerPrefs.GetString ("PP_" + (i + 1) + "_Pad_Right")) {														// gamepad Right Button
							ListOfCars [i].currentCarSelection = (ListOfCars [i].currentCarSelection + 1) % inventoryItemCar.inventoryItem [i].Cars.Count;
							LoadNewCar (i);																													// load a new car
						}
					}
				}
			}
		}
	}

// --> find the input
	public KeyCode FindTheKeyCodeUpdate(){																										
		foreach(KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
		{
			if (Input.GetKeyDown (key) && key.ToString () == PlayerPrefs.GetString ("PP_1_Desktop_Left")// Check if it is a button setup for player 1 or 2 
				|| Input.GetKeyDown (key) && key.ToString () == PlayerPrefs.GetString ("PP_2_Desktop_Left")
				|| Input.GetKeyDown (key) && key.ToString () == PlayerPrefs.GetString ("PP_1_Desktop_Right")
				|| Input.GetKeyDown (key) && key.ToString () == PlayerPrefs.GetString ("PP_2_Desktop_Right")
				|| Input.GetKeyDown (key) && key.ToString () == PlayerPrefs.GetString ("PP_1_Pad_Left")
				|| Input.GetKeyDown (key) && key.ToString () == PlayerPrefs.GetString ("PP_2_Pad_Left")
				|| Input.GetKeyDown (key) && key.ToString () == PlayerPrefs.GetString ("PP_1_Pad_Right")
				|| Input.GetKeyDown (key) && key.ToString () == PlayerPrefs.GetString ("PP_2_Pad_Right")
				|| Input.GetKeyDown (key) && key.ToString () == "LeftArrow"
				|| Input.GetKeyDown (key) && key.ToString () == "RightArrow") {
				return key;
			} else if( Input.GetKeyDown (key)){
				return KeyCode.None;
			}
		}
		return KeyCode.None;

	}
		

// --> Save the name of the connected gamepad
	public void updateInputs (){																								
		l_gamepadActivateJoystick.Clear ();
		for (int i = 0; i < Input.GetJoystickNames ().Length; i++) {
			l_gamepadActivateJoystick.Add(false);
		}
	}

// --> Use for player 1 and 2 on button last on Page_CarSelection
	public void lastCar(int playerNumber){
		ListOfCars [playerNumber].currentCarSelection--;
		if (ListOfCars [playerNumber].currentCarSelection < 0)
			ListOfCars [playerNumber].currentCarSelection = inventoryItemCar.inventoryItem  [playerNumber].Cars.Count - 1;
		LoadNewCar (playerNumber);																					// Load a new Car
	}

// --> Use for player 1 and 2 on button next on Page_CarSelection
	public void NextCar(int playerNumber){
		ListOfCars [playerNumber].currentCarSelection = (ListOfCars [playerNumber].currentCarSelection + 1) % inventoryItemCar.inventoryItem  [playerNumber].Cars.Count;
		LoadNewCar (playerNumber);																					// Load a new car
	}


// Init Car. Call when button Solo or Versus are pressed on the main menu
	public void initCarSelection (int NumberOfPlayer){						
		// --> First Car (Player 1);
		if (PlayerPrefs.HasKey ("Player_0_CarLastSelection")) {														// If the playerPrefs exist
			if (PlayerPrefs.GetInt ("Player_0_CarLastSelection") < inventoryItemCar.inventoryItem  [0].Cars.Count){
				ListOfCars [0].currentCarSelection = PlayerPrefs.GetInt ("Player_0_CarLastSelection");					// Load car for each player
			}
			else {
				PlayerPrefs.SetInt ("Player_0_CarLastSelection", 0);
				ListOfCars [0].currentCarSelection = 0;
			}

			LoadNewCar (0);
		} else
			LoadNewCar (0);


		int tmpRand = 0;																							// Choose randomly car for each player and CPU if needed

		List<int> tmpListOfCarValue = new List<int> ();
		for (int i = 0; i < ListOfCars.Count; i++) {
			if (ListOfCars [0].currentCarSelection != i && NumberOfPlayer == 1)
				tmpListOfCarValue.Add (i);
			if (ListOfCars [0].currentCarSelection != i && ListOfCars [1].currentCarSelection != i && NumberOfPlayer == 2)
				tmpListOfCarValue.Add (i);
		}
			

		List<int> tmpListOfRandomValue = new List<int> ();

		for (int i = 0; i < tmpListOfCarValue.Count; i++) {
			tmpRand = Random.Range (0, tmpListOfCarValue.Count);
			tmpListOfRandomValue.Add(tmpListOfCarValue[tmpRand]);
			tmpListOfCarValue.RemoveAt (tmpRand);
		}
		tmpListOfRandomValue.Add(tmpListOfCarValue[0]);



		for (int i = 1; i < ListOfCars.Count; i++) {
			// --> Second car (Player 2)
			if (PlayerPrefs.GetInt ("HowManyPlayers") == 2 && i == 1) {												// If 2 player are playing		
				if (PlayerPrefs.HasKey ("Player_" + i + "_CarLastSelection")) {										// If the playerPrefs exist
					//ListOfCars [i].currentCarSelection = PlayerPrefs.GetInt ("Player_" + i + "_CarLastSelection");	// Load car for each player

					if (PlayerPrefs.GetInt ("Player_1_CarLastSelection") < inventoryItemCar.inventoryItem  [1].Cars.Count){
						ListOfCars [i].currentCarSelection = PlayerPrefs.GetInt ("Player_" + i + "_CarLastSelection");	// Load car for each player
					}
					else {
						PlayerPrefs.SetInt ("Player_1_CarLastSelection", 0);
						ListOfCars [1].currentCarSelection = 0;
					}

					LoadNewCar (i);
				} else
					LoadNewCar (i);
			}
			else if (PlayerPrefs.GetInt ("HowManyPlayers") == 1 && i == 1) {										// If only one player is playing
				//tmpRand = Random.Range (0, ListOfCars [i].Cars.Count);
				ListOfCars [i].currentCarSelection = tmpListOfRandomValue [i-1];
				LoadNewCar (i);
			}

			// --> Choose car 3 and 4
			if (i > 1) {		
				
				if(NumberOfPlayer == 1)
					ListOfCars [i].currentCarSelection = tmpListOfRandomValue [i-1];
				if(NumberOfPlayer == 2)
					ListOfCars [i].currentCarSelection = tmpListOfRandomValue [i-2];

				LoadNewCar (i);
			}
		}
	}


// Init Car. Call when button Solo or Versus are pressed on the main menu
	public void initCarSelectionFromTrackSelection(){
		for (int i = 0; i < ListOfCars.Count; i++) {
			int carAlreadyExist = ListOfCars [i].pivotToSpawnCar.transform.childCount;							// if > 0 the car already exist

			if (carAlreadyExist == 0) {
				ListOfCars [i].currentCarSelection = PlayerPrefs.GetInt ("Player_" + i + "_CarLastSelection");	// Load car for each player
				LoadNewCar (i);
			}
		}
	}



// --> Load and save on playerPrefs a new car for a specific player
	public void LoadNewCar(int playerNumber){
		GameObject Spawn = ListOfCars [playerNumber].pivotToSpawnCar;

		Transform[] oldCars = Spawn.GetComponentsInChildren<Transform>();

		foreach (Transform oldCar in oldCars) {
			if(oldCar != Spawn.transform)
				Destroy (oldCar.gameObject);
		}

		if (inventoryItemCar.inventoryItem  [playerNumber].Cars [ListOfCars [playerNumber].currentCarSelection] != null) {
			GameObject newCar = Instantiate (inventoryItemCar.inventoryItem  [playerNumber].Cars [ListOfCars [playerNumber].currentCarSelection], ListOfCars [playerNumber].pivotToSpawnCar.transform);		// Load a new car from the resource folder

			newCar.name = newCar.name.Replace ("(Clone)", "");
			newCar.GetComponent<Rigidbody> ().isKinematic = true;
			CarController carController = newCar.GetComponent<CarController> ();
			carController.enabled = false;
			carController.audio_.mute = true;
			carController.objSkid_Sound.mute = true;
			carController.obj_CarImpact_Sound.mute = true;
			newCar.GetComponent<CarAI> ().enabled = false;


			Transform pivotCarSelection = newCar.GetComponent<CarController> ().pivotCarSelection.transform;														// Move the car to a specific position

			pivotCarSelection.parent = ListOfCars [playerNumber].pivotToSpawnCar.transform;
			newCar.transform.parent = pivotCarSelection;
			pivotCarSelection.transform.localPosition = new Vector3 (0, 0, 0);
			pivotCarSelection.transform.eulerAngles = ListOfCars [playerNumber].pivotToSpawnCar.transform.eulerAngles;


			PlayerPrefs.SetString ("Player_" + playerNumber + "_Car", newCar.gameObject.name);																		// Save the name of the car for the selected player
			PlayerPrefs.SetInt ("Player_" + playerNumber + "_CarLastSelection", ListOfCars [playerNumber].currentCarSelection);										// Save the current Car Selection for the selecte)d player
		} else {
			Debug.Log ("MCR Creator : You need to add a car on slot " + ListOfCars [playerNumber].currentCarSelection + " for player " + (playerNumber+1).ToString() + ". GameObject ''CheckCarSelection'' on the Hierarchy (Hierarchy: Canvas_MainMenu -> CheckCarSelection)" );
		}
	}
}





