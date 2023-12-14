// Decription : SetupInputs.cs : use to setup the input on menu page "Page_Inputs" 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SetupInputs : MonoBehaviour {
	public bool 								b_Enabled = false;									// Enable to change the inputs when the Input Menu is activated
	private Text 								m_Text;												// temporary text
	public bool 								b_WaitForInputs = false;							// use to know when an input are pressed
	public EventSystem							eventSystem; 										// access the evenSystem 
	public List<string>							l_gamepadNames = new List<string>();				// list of gamepads name

	public string								s_ControllerType = "Keyboard";						// Use to switvh between keyboard and gamepad inputs
	public string								s_ControllerTypeP2 = "Keyboard";						// Use to switvh between keyboard and gamepad inputs
	public string								s_currentButtonToModify = "";						// Know the name of the button that we modify
		
	[System.Serializable]
	public class _ListOfInputs{
		public List<string> Desktop = new List<string>();												// Inputs Desktop
		public List<string> Gamepad = new List<string>();												// Inputs Gamepad

		public _ListOfInputs(){}
	}

	public List<_ListOfInputs> 					ListOfInputs = new List<_ListOfInputs>();			// list of Inputs on Hierarchy

	[System.Serializable]
	public class _ListOfVisualizationObjects{
		public List<Text> 		Names = new List<Text>();												// Name of the button
		public List<GameObject> Buttons = new List<GameObject>();										// Button
		public List<GameObject> Shadow = new List<GameObject>();									// Button shadow
		public List<Text> 		InputNames = new List<Text>();											// Text to display input name
		public Text 			Info;
		public _ListOfVisualizationObjects(){}
	}

	public List<_ListOfVisualizationObjects> 	ListOfVisualizationObjects = new List<_ListOfVisualizationObjects>();	// list of Buttons and text used to display inputs on screen

	public DefaultInputsValues defaultInputsValues;

// --> Use this for initialization
	void Start () {
		eventSystem = GameObject.Find ("EventSystem").GetComponent<EventSystem> ();						// access eventSystem component
		if (!eventSystem)Debug.Log ("MCR Creator : You need an EventSystem gameObject on your scene");


		if (!PlayerPrefs.HasKey ("PP_1_Desktop_Left")) {
			DefaultInputsParameters ();
		}
		else
			LoadInputsValues();																			// Load the current inputs values

		s_ControllerType = "Keyboard";																	// Display the Keyboard inputs by default
		s_ControllerTypeP2 = "Keyboard";

		for (int i = 0; i < 2; i++) {																	// Init text and buttons for Player and Player 2
			GameObject Btn_Choose_Controller = GameObject.Find ("Btn_Choose_Controller_J" +(i+1).ToString());
			if (Btn_Choose_Controller && i == 0) {
				Text txt_btn = Btn_Choose_Controller.transform.Find ("txt").gameObject.GetComponent<Text> ();
				txt_btn.text = s_ControllerType;
			}
			if (Btn_Choose_Controller && i == 1) {
				Text txt_btn = Btn_Choose_Controller.transform.Find ("txt").gameObject.GetComponent<Text> ();
				txt_btn.text = s_ControllerTypeP2;
			}
		}
		SwitchInputFromGamepadToDesktop (2);															// Display Destop Inputs on screen												
	}


// --> Update is called once per frame
	void Update(){
		if (b_Enabled 
			&& FindTheKeyCodeUpdate () != KeyCode.None														// --> A button is pressed to Remap a Gamepad button
			&& !b_WaitForInputs
			&& eventSystem.currentSelectedGameObject != null													// A button is selected on screen
			&& eventSystem.currentSelectedGameObject.tag == "btnRemapInput"										// the button is tagged "btnRemapInput"
			&& eventSystem.currentSelectedGameObject.name.Contains("J1")
			) {																	

			if ((FindTheKeyCodeUpdate ().ToString ().Contains ("Button")										// -> If a button on gamePad or button ''return'' on keyboard are pressed
				|| FindTheKeyCodeUpdate () == KeyCode.Return
				|| FindTheKeyCodeUpdate ().ToString ().Contains ("Mouse"))
				&& s_ControllerType == "Gamepad") {

				s_currentButtonToModify = eventSystem.currentSelectedGameObject.name;							// Save the current gameobject name

				GameObject tmpBtn = eventSystem.currentSelectedGameObject;								
				Text txt_btn = tmpBtn.transform.Find ("txt").gameObject.GetComponent<Text> ();					
				ChangeInput (txt_btn,1);																		// -> Change the input 
			}
			if ((FindTheKeyCodeUpdate () == KeyCode.Return
				|| FindTheKeyCodeUpdate ().ToString ().Contains ("Mouse"))
				&& s_ControllerType == "Keyboard") {										// -> If a button on gamePad or button ''return'' on keyboard are pressed

				s_currentButtonToModify = eventSystem.currentSelectedGameObject.name;							// Save the current gameobject name

				GameObject tmpBtn = eventSystem.currentSelectedGameObject;								
				Text txt_btn = tmpBtn.transform.Find ("txt").gameObject.GetComponent<Text> ();					

				ChangeInput (txt_btn,1);																		// -> Change the input 
			}
		}
		if (b_Enabled 
			&& FindTheKeyCodeUpdate () != KeyCode.None														// --> A button is pressed to Remap a Gamepad button
			&& !b_WaitForInputs
			&& eventSystem.currentSelectedGameObject != null													// A button is selected on screen
			&& eventSystem.currentSelectedGameObject.tag == "btnRemapInput"										// the button is tagged "btnRemapInput"
			&& eventSystem.currentSelectedGameObject.name.Contains("J2")
			) {																	

			if ((FindTheKeyCodeUpdate ().ToString ().Contains ("Button")										// -> If a button on gamePad or button ''return'' on keyboard are pressed
				|| FindTheKeyCodeUpdate () == KeyCode.Return
				|| FindTheKeyCodeUpdate ().ToString ().Contains ("Mouse"))
				&& s_ControllerTypeP2 == "Gamepad") {

				s_currentButtonToModify = eventSystem.currentSelectedGameObject.name;							// Save the current gameobject name

				GameObject tmpBtn = eventSystem.currentSelectedGameObject;								
				Text txt_btn = tmpBtn.transform.Find ("txt").gameObject.GetComponent<Text> ();					

				ChangeInput (txt_btn,2);																		// -> Change the input 
			}

			if ((FindTheKeyCodeUpdate () == KeyCode.Return
				|| FindTheKeyCodeUpdate ().ToString ().Contains ("Mouse"))
				&& s_ControllerTypeP2 == "Keyboard") {										// -> If a button on gamePad or button ''return'' on keyboard are pressed
				s_currentButtonToModify = eventSystem.currentSelectedGameObject.name;							// Save the current gameobject name

				GameObject tmpBtn = eventSystem.currentSelectedGameObject;								
				Text txt_btn = tmpBtn.transform.Find ("txt").gameObject.GetComponent<Text> ();					

				ChangeInput (txt_btn,2);																		// -> Change the input 
			}
		}
		else if(Input.anyKeyDown 																			// --> A button is pressed to Switch between Gamepad or Desktop controller
			&& (FindTheKeyCodeUpdate ().ToString ().Contains ("Joystick")
			&& eventSystem.currentSelectedGameObject != null													// A button is selected on screen
			&& eventSystem.currentSelectedGameObject.tag == "btnNavigationInputMenu"							// the button is tagged "btnNavigationInputMenu"
			/*&& (ListOfInputs[0].Gamepad[6] != "" && Input.GetButtonDown(ListOfInputs[0].Gamepad[6]) 			// Player 1 Gamepad button validate is pressed
				|| ListOfInputs[1].Gamepad[6] != "" && Input.GetButtonDown(ListOfInputs[1].Gamepad[6])*/) 		// Player 2 Gamepad button validate is pressed 
			) {

			GameObject tmpBtn = eventSystem.currentSelectedGameObject;

			if(tmpBtn.name == "Btn_Choose_Controller_J1"){												// -> Change controller type (gamepad or keyboard)
				Text txt_btn = tmpBtn.transform.Find("txt").gameObject.GetComponent<Text>();
				eventSystem.sendNavigationEvents = false;
				if (s_ControllerType == "Keyboard") {															// Switch to gamepad controller
					
					if (tmpBtn.name == "Btn_Choose_Controller_J1" && Input.GetJoystickNames ().Length > 0) {
						s_ControllerType = "Gamepad";
						SwitchInputFromKeyboardToGamepad (0);
						txt_btn.text = s_ControllerType;
					}

				} else if (s_ControllerType == "Gamepad") {																					// Switch to keyboard controller
					if (tmpBtn.name == "Btn_Choose_Controller_J1" && Input.GetJoystickNames ().Length > 0) {
						s_ControllerType = "Keyboard";
						SwitchInputFromGamepadToDesktop (0);
						txt_btn.text = s_ControllerType;
					}

				} 

				eventSystem.sendNavigationEvents = true;
			}
			if(tmpBtn.name == "Btn_Choose_Controller_J2"){												// -> Change controller type (gamepad or keyboard)
				Text txt_btn = tmpBtn.transform.Find("txt").gameObject.GetComponent<Text>();
				eventSystem.sendNavigationEvents = false;
				if (s_ControllerTypeP2 == "Keyboard") {
					if (tmpBtn.name == "Btn_Choose_Controller_J2" && Input.GetJoystickNames ().Length > 1) {
						s_ControllerTypeP2 = "Gamepad";
						SwitchInputFromKeyboardToGamepad (1);
						txt_btn.text = s_ControllerTypeP2;
					}
				} else if (s_ControllerTypeP2 == "Gamepad") {
					if (tmpBtn.name == "Btn_Choose_Controller_J2" && Input.GetJoystickNames ().Length > 1) {
						s_ControllerTypeP2 = "Keyboard";
						SwitchInputFromGamepadToDesktop (1);
						txt_btn.text = s_ControllerTypeP2;
					}
				}

				eventSystem.sendNavigationEvents = true;
			}

				if(tmpBtn.name == "Btn_DefaultValue"){															// -> Init Inputs Value
					eventSystem.sendNavigationEvents = false;
					DefaultInputsParameters ();
					eventSystem.sendNavigationEvents = true;
				}
			
		}
	}


// -> use by button ''Btn_Choose_Controller'' on the Hierarchy : Change controller type (gamepad or keyboard)
	public void ControllerType(int PlayerNumber){															
		GameObject tmpBtn = eventSystem.currentSelectedGameObject;
		Text txt_btn = tmpBtn.transform.Find("txt").gameObject.GetComponent<Text>();
									
		if (Input.GetJoystickNames ().Length > 0 && PlayerNumber == 0){											// Player 1

			if (s_ControllerType == "Keyboard") {																// Switch to gamepad controller
				s_ControllerType = "Gamepad";
				SwitchInputFromKeyboardToGamepad (PlayerNumber);
			} else {																							// Switch to keyboard controller
				s_ControllerType = "Keyboard";	
				SwitchInputFromGamepadToDesktop (PlayerNumber);
			}

			txt_btn.text = s_ControllerType;
		}
		if (Input.GetJoystickNames ().Length > 1 && PlayerNumber == 1) {										// Player 2
			if (s_ControllerTypeP2 == "Keyboard") {																// Switch to gamepad controller
				s_ControllerTypeP2 = "Gamepad";
				SwitchInputFromKeyboardToGamepad (PlayerNumber);
			} else {																							// Switch to keyboard controller
				s_ControllerTypeP2 = "Keyboard";	
				SwitchInputFromGamepadToDesktop (PlayerNumber);
			}

			txt_btn.text = s_ControllerTypeP2;
		}
	}


// --> Start Checking if Key or Axis are pressed
	public void ChangeInput (Text ui_Text, int PlayerNumber) {																
		m_Text = ui_Text;
		eventSystem.sendNavigationEvents = false;
		b_WaitForInputs = true;																		
		m_Text.text = "press key";

		for (int j = 0; j < ListOfVisualizationObjects.Count; j++) {												// prevent bug : first button is pressed with the mouse. Before choosing an input a second button is pressed with the mouse
			for (int k = 0; k < ListOfVisualizationObjects [j].Buttons.Count; k++) {
				if (ListOfVisualizationObjects [j].Buttons [k] != null
					&& ListOfVisualizationObjects [j].Buttons [k].name != eventSystem.currentSelectedGameObject.name
					&& ListOfVisualizationObjects [j].Buttons [k].name != m_Text.gameObject.transform.parent.name) {

					if (s_ControllerType == "Gamepad" && j == 0) {
						if (ListOfVisualizationObjects [0].InputNames [k] != null)
							ListOfVisualizationObjects [0].InputNames [k].text = ListOfInputs [0].Gamepad [k];
					} else if (s_ControllerType == "Keyboard" && j == 0) {
						if (ListOfVisualizationObjects [0].InputNames [k] != null)
							ListOfVisualizationObjects [0].InputNames [k].text = ListOfInputs [0].Desktop [k];	
					}
					if (s_ControllerTypeP2 == "Gamepad" && j == 1) {
						if (ListOfVisualizationObjects [1].InputNames [k] != null)
							ListOfVisualizationObjects [1].InputNames [k].text = ListOfInputs [1].Gamepad [k];
					} else if (s_ControllerTypeP2 == "Keyboard" && j == 1) {
						if (ListOfVisualizationObjects [1].InputNames [k] != null)
							ListOfVisualizationObjects [1].InputNames [k].text = ListOfInputs [1].Desktop [k];	
					}
				}
			}
		}

			l_gamepadNames.Clear ();																				// Save on a list : gamepads names
			for (int i = 0; i < Input.GetJoystickNames ().Length; i++) {
				l_gamepadNames.Add (Input.GetJoystickNames () [i] + " : " + i.ToString ());
			}

			Input.ResetInputAxes ();																				// Reset Axis values



		if (PlayerNumber == 1) {
			if (s_ControllerType == "Gamepad")
				StartCoroutine ("New_Input_Gamepad");																// Start Checking Gamepad Inputs
			else if (s_ControllerType == "Keyboard")
				StartCoroutine ("New_Input_Keyboard");																// Start Checking Keyboard Inputs
		} else if (PlayerNumber == 2) {
			if (s_ControllerTypeP2 == "Gamepad")
				StartCoroutine ("New_Input_Gamepad");																// Start Checking Gamepad Inputs
			else if (s_ControllerTypeP2 == "Keyboard")
				StartCoroutine ("New_Input_Keyboard");	
		}
	}


// --> Change Input Gamepad Input
	IEnumerator New_Input_Gamepad(){																		
		l_gamepadNames.Clear ();																				// Save on a list : gamepads names
		for (int i = 0; i < Input.GetJoystickNames ().Length; i++) {
			l_gamepadNames.Add (Input.GetJoystickNames () [i] + " : " + i.ToString ());
		}
			

		while (b_WaitForInputs) {																				// Wait until a button or an axis is pressed or moved
			int i = 0;
			while (i < Input.GetJoystickNames ().Length && b_WaitForInputs) {
				if (i < 2) {																														// Prevent bug if more than 2 gamepad connected.Limit to 2 gamepad.
					for (int j = 1; j < 11; j++) {
						if (Mathf.Abs (Input.GetAxisRaw ("Joystick" + (i + 1).ToString () + "Axis" + j)) == 1) {		// Axis is detected
							b_WaitForInputs = false;
							UpdatePlayerPrefsGamepadInputs ("Joystick" + (i + 1).ToString () + "Axis" + j, i);
						}
					}
				}
				foreach(KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
				{


					if (Input.GetKey(key) && key == KeyCode.Escape) {											//	Undo Change Input
						for (int j = 0; j < ListOfVisualizationObjects.Count; j++) {
							for (int k = 0; k < ListOfVisualizationObjects [j].Buttons.Count; k++) {
								if (ListOfVisualizationObjects [j].Buttons [k] != null
									&& ListOfVisualizationObjects [j].Buttons [k].name == eventSystem.currentSelectedGameObject.name) {
									if (ListOfVisualizationObjects [j].InputNames [k] != null)
										ListOfVisualizationObjects [j].InputNames[k].text = ListOfInputs [j].Gamepad [k];
									b_WaitForInputs = false;
									NewTextInfo (ListOfVisualizationObjects [j].Info, "Input canceled");
								}
							}
						}
					} 

					for (int j = 0; j < 20; j++) {																// Joystick button is detected
						if (Input.GetKey (key) && key.ToString () 
								== "Joystick" + (i + 1).ToString () + "Button" + j) {
							b_WaitForInputs = false;
							UpdatePlayerPrefsGamepadInputs ("Joystick" + (i + 1).ToString () + "Button" + j, i);
						}
					}
				}

				if (FindTheKeyCode ().ToString().Contains ("Mouse")) {											// Mouse button is pressed. Undo Change Input					
					for (int j = 0; j < ListOfVisualizationObjects.Count; j++) {
						for (int k = 0; k < ListOfVisualizationObjects [j].Buttons.Count; k++) {
							if (eventSystem.currentSelectedGameObject != null 
								&& ListOfVisualizationObjects [j].Buttons [k] != null 
								&& ListOfVisualizationObjects [j].Buttons [k].name == eventSystem.currentSelectedGameObject.name) {
								if (ListOfVisualizationObjects [j].InputNames [k] != null) {
									if (s_ControllerType == "Gamepad") {
										if (ListOfVisualizationObjects [0].InputNames [k] != null)
											ListOfVisualizationObjects [0].InputNames [k].text = ListOfInputs [0].Gamepad [k];
									} else if (s_ControllerType == "Keyboard") {
										if (ListOfVisualizationObjects [0].InputNames [k] != null)
											ListOfVisualizationObjects [0].InputNames [k].text = ListOfInputs [0].Desktop [k];	
									}
									if (s_ControllerTypeP2 == "Gamepad") {
										if (ListOfVisualizationObjects [1].InputNames [k] != null)
											ListOfVisualizationObjects [1].InputNames [k].text = ListOfInputs [1].Gamepad [k];
									} else if (s_ControllerTypeP2 == "Keyboard") {
										if (ListOfVisualizationObjects [1].InputNames [k] != null)
											ListOfVisualizationObjects [1].InputNames [k].text = ListOfInputs [1].Desktop [k];	
									}
								}
								NewTextInfo (ListOfVisualizationObjects [j].Info, "Mouse button is not allowed");
							}

							if (eventSystem.currentSelectedGameObject == null) {										// prevent bug if player. the button and no input have been selected
								if (s_ControllerType == "Gamepad") {
									if (ListOfVisualizationObjects [0].InputNames [k] != null)
										ListOfVisualizationObjects [0].InputNames [k].text = ListOfInputs [0].Gamepad [k];
								} else if (s_ControllerType == "Keyboard") {
									if (ListOfVisualizationObjects [0].InputNames [k] != null)
										ListOfVisualizationObjects [0].InputNames [k].text = ListOfInputs [0].Desktop [k];	
								}
								if (s_ControllerTypeP2 == "Gamepad") {
									if (ListOfVisualizationObjects [1].InputNames [k] != null)
										ListOfVisualizationObjects [1].InputNames [k].text = ListOfInputs [1].Gamepad [k];
								} else if (s_ControllerTypeP2 == "Keyboard") {
									if (ListOfVisualizationObjects [1].InputNames [k] != null)
										ListOfVisualizationObjects [1].InputNames [k].text = ListOfInputs [1].Desktop [k];	
								}
							}
						}
					}
				}

				i++;
			}
			yield return null;
		}
		eventSystem.sendNavigationEvents = true;
	}


// --> Check keyboard Inputs
	IEnumerator New_Input_Keyboard(){																		

		while (!Input.anyKeyDown) {																				// When until a key is pressed
			yield return null;
		}
			

		b_WaitForInputs = false;

		bool b_KeyFromKeyboardAlreadyUse = false;

		for (int j = 0; j < ListOfInputs.Count; j++) {															// Check if this key is already use
			for (int i = 0; i < ListOfInputs [j].Desktop.Count; i++) {
				if (ListOfInputs [j].Desktop[i] == FindTheKeyCode ().ToString ()) {
					b_KeyFromKeyboardAlreadyUse = true;
				}
			}
		}

		if (b_KeyFromKeyboardAlreadyUse) {																		//	Key is already used. Undo Change Input
			for (int j = 0; j < ListOfVisualizationObjects.Count; j++) {
				for (int i = 0; i < ListOfVisualizationObjects [j].Buttons.Count; i++) {
					if (ListOfVisualizationObjects [j].Buttons [i] != null 
						&& ListOfVisualizationObjects [j].Buttons [i].name == eventSystem.currentSelectedGameObject.name) {
						if (ListOfVisualizationObjects [j].InputNames [i] != null)
							ListOfVisualizationObjects [j].InputNames[i].text = ListOfInputs [j].Desktop [i];
						NewTextInfo (ListOfVisualizationObjects [j].Info, "Key is already used");
					}
				}
			}
		} 
		else if (FindTheKeyCode ().ToString().Contains ("Button")) {											// Joypad button is pressed. Undo Change Input					
			for (int j = 0; j < ListOfVisualizationObjects.Count; j++) {
				for (int i = 0; i < ListOfVisualizationObjects [j].Buttons.Count; i++) {
					if (ListOfVisualizationObjects [j].Buttons [i] != null 
						&& ListOfVisualizationObjects [j].Buttons [i].name == eventSystem.currentSelectedGameObject.name) {
						if (ListOfVisualizationObjects [j].InputNames [i] != null)
							ListOfVisualizationObjects [j].InputNames[i].text = ListOfInputs [j].Desktop [i];
						NewTextInfo (ListOfVisualizationObjects [j].Info, "Joystick button is not allowed");
					}
				}
			}
		}
		else if (FindTheKeyCode ().ToString().Contains ("Mouse")) {											// Mouse button is pressed. Undo Change Input					
			for (int j = 0; j < ListOfVisualizationObjects.Count; j++) {
				for (int i = 0; i < ListOfVisualizationObjects [j].Buttons.Count; i++) {
					if (eventSystem.currentSelectedGameObject != null 
						&& ListOfVisualizationObjects [j].Buttons [i] != null 
						&& ListOfVisualizationObjects [j].Buttons [i].name == eventSystem.currentSelectedGameObject.name) {
						if (ListOfVisualizationObjects [j].InputNames [i] != null)
							ListOfVisualizationObjects [j].InputNames[i].text = ListOfInputs [j].Desktop [i];
						NewTextInfo (ListOfVisualizationObjects [j].Info, "Mouse button is not allowed");
					}

					if (eventSystem.currentSelectedGameObject == null) {										// prevent bug if player. the button and no input have been selected
						if (s_ControllerType == "Gamepad") {
							if (ListOfVisualizationObjects [0].InputNames [i] != null)
								ListOfVisualizationObjects [0].InputNames [i].text = ListOfInputs [0].Gamepad [i];
						} else if (s_ControllerType == "Keyboard") {
							if (ListOfVisualizationObjects [0].InputNames [i] != null)
								ListOfVisualizationObjects [0].InputNames [i].text = ListOfInputs [0].Desktop [i];	
						}
						if (s_ControllerTypeP2 == "Gamepad") {
							if (ListOfVisualizationObjects [1].InputNames [i] != null)
								ListOfVisualizationObjects [1].InputNames [i].text = ListOfInputs [1].Gamepad [i];
						} else if (s_ControllerTypeP2 == "Keyboard") {
							if (ListOfVisualizationObjects [1].InputNames [i] != null)
								ListOfVisualizationObjects [1].InputNames [i].text = ListOfInputs [1].Desktop [i];	
						}
					}
				}
			}
		}
		else if (FindTheKeyCode () == KeyCode.Escape) {															//	button ''Escape'' is pressed. Undo Change Input
			for (int j = 0; j < ListOfVisualizationObjects.Count; j++) {
				for (int i = 0; i < ListOfVisualizationObjects [j].Buttons.Count; i++) {
					if (ListOfVisualizationObjects [j].Buttons [i] != null 
						&& ListOfVisualizationObjects [j].Buttons [i].name == eventSystem.currentSelectedGameObject.name) {
						if (ListOfVisualizationObjects [j].InputNames [i] != null)
							ListOfVisualizationObjects [j].InputNames[i].text = ListOfInputs [j].Desktop [i];
						NewTextInfo (ListOfVisualizationObjects [j].Info, "Input canceled");
					}
				}
			}
		} 
		else {																									// New key is detected :
			m_Text.text = FindTheKeyCode ().ToString ();
			s_currentButtonToModify = eventSystem.currentSelectedGameObject.name;
			UpdatePlayerPrefsKeyboardInputs (FindTheKeyCode ().ToString ());
		}
			
		eventSystem.sendNavigationEvents = true;
	}


	public KeyCode FindTheKeyCode(){
		foreach(KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
		{if(Input.GetKey(key)){return key;}}return KeyCode.None;
	}

	public KeyCode FindTheKeyCodeUpdate(){
		foreach(KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
		{if(Input.GetKeyDown(key)){return key;}}
		return KeyCode.None;
	}


// --> update the gamepad inputs when a new button or axis is detected
	void UpdatePlayerPrefsGamepadInputs(string _newString, int gamepadNumber){																		
		bool b_AllowedKey = false;
		for (int i = 0; i < Input.GetJoystickNames ().Length; i++) {
			if (s_currentButtonToModify == "Btn_Left_J" + (i+1).ToString() && l_gamepadNames[i] == Input.GetJoystickNames () [gamepadNumber] + " : " + gamepadNumber.ToString ()) {									
				PlayerPrefs.SetString ("PP_" + (i+1).ToString() + "_Pad_Left", _newString);
				ListOfInputs [i].Gamepad [0] = _newString;
				m_Text.text = _newString;

				if (_newString.Contains ("Axis")) {
					PlayerPrefs.SetString ("PP_" + (i+1).ToString() + "_Pad_Right", _newString);
					ListOfInputs [i].Gamepad [1] = _newString;
					SwitchInputFromKeyboardToGamepad (i);
				}
				b_AllowedKey = true;
			}
			if (s_currentButtonToModify == "Btn_Right_J" + (i+1).ToString() && l_gamepadNames[i] == Input.GetJoystickNames () [gamepadNumber] + " : " + gamepadNumber.ToString ()) {
				PlayerPrefs.SetString ("PP_" + (i+1).ToString() + "_Pad_Right", _newString);
				ListOfInputs [i].Gamepad [1] = _newString;
				m_Text.text = _newString;

				if (_newString.Contains ("Axis")) {
					PlayerPrefs.SetString ("PP_" + (i+1).ToString() + "_Pad_Left", _newString);
					ListOfInputs [i].Gamepad [0] = _newString;
					SwitchInputFromKeyboardToGamepad (i);
				}
				b_AllowedKey = true;
			}
			if (s_currentButtonToModify == "Btn_Acceleration_J" + (i+1).ToString() && l_gamepadNames[i] == Input.GetJoystickNames () [gamepadNumber] + " : " + gamepadNumber.ToString ()) {
				PlayerPrefs.SetString ("PP_" + (i+1).ToString() + "_Pad_Acceleration", _newString);
				ListOfInputs [i].Gamepad [2] = _newString;
				m_Text.text = _newString;
				b_AllowedKey = true;
			}
			if (s_currentButtonToModify == "Btn_Break_J" + (i+1).ToString() && l_gamepadNames[i] == Input.GetJoystickNames () [gamepadNumber] + " : " + gamepadNumber.ToString ()) {
				PlayerPrefs.SetString ("PP_" + (i+1).ToString() + "_Pad_Break", _newString);
				ListOfInputs [i].Gamepad [3] = _newString;
				m_Text.text = _newString;
				b_AllowedKey = true;
			}
			if (s_currentButtonToModify == "Btn_Other_J" + (i+1).ToString() && l_gamepadNames[i] == Input.GetJoystickNames () [gamepadNumber] + " : " + gamepadNumber.ToString ()) {
				PlayerPrefs.SetString ("PP_" + (i+1).ToString() + "_Pad_Other", _newString);
				ListOfInputs [i].Gamepad [4] = _newString;
				m_Text.text = _newString;
				b_AllowedKey = true;
			}
			if (s_currentButtonToModify == "Btn_Respawn_J" + (i+1).ToString() && l_gamepadNames[i] == Input.GetJoystickNames () [gamepadNumber] + " : " + gamepadNumber.ToString ()) {
				if(_newString.Contains("Axis")){
					NewTextInfo (ListOfVisualizationObjects [i].Info, "You need to use a Joystick button");
					m_Text.text = PlayerPrefs.GetString ("PP_" + (i+1).ToString() + "_Pad_Respawn");
				}
				else{
					PlayerPrefs.SetString ("PP_" + (i+1).ToString() + "_Pad_Respawn", _newString);
					ListOfInputs [i].Gamepad [5] = _newString;
					m_Text.text = _newString;
				}
				b_AllowedKey = true;
			}
			if (s_currentButtonToModify == "Btn_Validate_J" + (i+1).ToString() && l_gamepadNames[i] == Input.GetJoystickNames () [gamepadNumber] + " : " + gamepadNumber.ToString ()) {
				if(_newString.Contains("Axis")){
					NewTextInfo (ListOfVisualizationObjects [i].Info, "You need to use a Joystick button");
					m_Text.text = PlayerPrefs.GetString ("PP_" + (i+1).ToString() + "_Pad_Validate");
				}
				else{
					PlayerPrefs.SetString ("PP_" + (i+1).ToString() + "_Pad_Validate", _newString);
					ListOfInputs [i].Gamepad [6] = _newString;
					m_Text.text = _newString;
				}

				b_AllowedKey = true;
			}
			if (s_currentButtonToModify == "Btn_Back_J" + (i+1).ToString() && l_gamepadNames[i] == Input.GetJoystickNames () [gamepadNumber] + " : " + gamepadNumber.ToString ()) {
				if(_newString.Contains("Axis")){
					NewTextInfo (ListOfVisualizationObjects [i].Info, "You need to use a Joystick button");
					m_Text.text = PlayerPrefs.GetString ("PP_" + (i+1).ToString() + "_Pad_Back");
				}
				else{
					PlayerPrefs.SetString ("PP_" + (i+1).ToString() + "_Pad_Back", _newString);
					ListOfInputs [i].Gamepad [7] = _newString;
					m_Text.text = _newString;
				}


				b_AllowedKey = true;
			}
			if (s_currentButtonToModify == "Btn_Pause_J" + (i+1).ToString() && l_gamepadNames[i] == Input.GetJoystickNames () [gamepadNumber] + " : " + gamepadNumber.ToString ()) {
				if(_newString.Contains("Axis")){
					NewTextInfo (ListOfVisualizationObjects [i].Info, "You need to use a Joystick button");
					m_Text.text = PlayerPrefs.GetString ("PP_" + (i+1).ToString() + "_Pad_Pause");
				}
				else{
					PlayerPrefs.SetString ("PP_" + (i+1).ToString() + "_Pad_Pause", _newString);
					ListOfInputs [i].Gamepad [8] = _newString;
					m_Text.text = _newString;
				}

				b_AllowedKey = true;
			}
		}
		if (!b_AllowedKey) {
			for (int j = 0; j < ListOfVisualizationObjects.Count; j++) {
				for (int k = 0; k < ListOfVisualizationObjects [j].Buttons.Count; k++) {
					if (ListOfVisualizationObjects [j].Buttons [k] != null 
						&& eventSystem.currentSelectedGameObject != null
						&& ListOfVisualizationObjects [j].Buttons [k].name == eventSystem.currentSelectedGameObject.name
					) {
						if (ListOfVisualizationObjects [j].InputNames [k] != null)
							ListOfVisualizationObjects [j].InputNames[k].text = ListOfInputs [j].Gamepad [k];
						NewTextInfo (ListOfVisualizationObjects [j].Info, "You need to use Joystick " + (j+1).ToString());
					}
				}
			}
		}

		GameObject obj_Check_GamepadInputs = GameObject.Find ("Check_GamepadInputs");
		if (obj_Check_GamepadInputs)
			obj_Check_GamepadInputs.GetComponent<CheckForInputs> ().updateInputs ();
	}


// --> update the keyboard inputs when a new button is detected
	void UpdatePlayerPrefsKeyboardInputs(string _newString){		
		for (int i = 0; i < 2; i++) {
			if (s_currentButtonToModify == "Btn_Left_J" + (i+1).ToString()) {											
				PlayerPrefs.SetString ("PP_" + (i+1).ToString() + "_Desktop_Left", _newString);
				//Debug.Log ("PP_" + (i + 1).ToString () + "_Desktop_Left" + " : "+PlayerPrefs.GetString ("PP_" + (i + 1).ToString () + "_Desktop_Left"));
				ListOfInputs [i].Desktop [0] = _newString;
			}
			if (s_currentButtonToModify == "Btn_Right_J" + (i+1).ToString()) {
				PlayerPrefs.SetString ("PP_" + (i+1).ToString() + "_Desktop_Right", _newString);
				ListOfInputs [i].Desktop [1] = _newString;
			}
			if (s_currentButtonToModify == "Btn_Acceleration_J" + (i+1).ToString()) {
				PlayerPrefs.SetString ("PP_" + (i+1).ToString() + "_Desktop_Acceleration", _newString);
				ListOfInputs [i].Desktop [2] = _newString;
			}
			if (s_currentButtonToModify == "Btn_Break_J" + (i+1).ToString()) {
				PlayerPrefs.SetString ("PP_" + (i+1).ToString() + "_Desktop_Break", _newString);
				ListOfInputs [i].Desktop [3] = _newString;
			}
			if (s_currentButtonToModify == "Btn_Other_J" + (i+1).ToString()) {
				PlayerPrefs.SetString ("PP_" + (i+1).ToString() + "_Desktop_Other", _newString);
				ListOfInputs [i].Desktop [4] = _newString;
			}
			if (s_currentButtonToModify == "Btn_Respawn_J" + (i+1).ToString()) {
				PlayerPrefs.SetString ("PP_" + (i+1).ToString() + "_Desktop_Respawn", _newString);
				ListOfInputs [i].Desktop [5] = _newString;
			}
			if (s_currentButtonToModify == "Btn_Validate_J" + (i+1).ToString()) {
				PlayerPrefs.SetString ("PP_" + (i+1).ToString() + "_Desktop_Validate", _newString);
				ListOfInputs [i].Desktop [6] = _newString;
			}
			if (s_currentButtonToModify == "Btn_Back_J" + (i+1).ToString()) {
				PlayerPrefs.SetString ("PP_" + (i+1).ToString() + "_Desktop_Back", _newString);
				ListOfInputs [i].Desktop [7] = _newString;
			}
			if (s_currentButtonToModify == "Btn_Pause_J" + (i+1).ToString()) {
				PlayerPrefs.SetString ("PP_" + (i+1).ToString() + "_Desktop_Pause", _newString);
				ListOfInputs [i].Desktop [8] = _newString;
			}
		}
	}


// --> Display Gamepad inputs on screen
	void SwitchInputFromKeyboardToGamepad(int PlayerNumber){																	
		if (PlayerNumber == 2) {
			for (int j = 0; j < ListOfVisualizationObjects.Count; j++) {
				for (int i = 0; i < ListOfVisualizationObjects [j].Names.Count; i++) {

					if (i == ListOfVisualizationObjects [j].Names.Count - 1
						|| i == ListOfVisualizationObjects [j].Names.Count - 2
					    || i == ListOfVisualizationObjects [j].Names.Count - 3) {
						if (ListOfVisualizationObjects [j].Buttons [i] != null) {
							ListOfVisualizationObjects [j].Names [i].gameObject.SetActive (true);
							ListOfVisualizationObjects [j].Buttons [i].gameObject.SetActive (true);
							if(ListOfVisualizationObjects [j].Shadow [i]!= null)ListOfVisualizationObjects [j].Shadow [i].gameObject.SetActive (true);
						}
					}

					if(ListOfVisualizationObjects [j].InputNames [i] != null)
						ListOfVisualizationObjects [j].InputNames [i].text = ListOfInputs [j].Gamepad [i];
				}
			}
		} else {

			for (int i = 0; i < ListOfVisualizationObjects [PlayerNumber].Names.Count; i++) {

				if (i == ListOfVisualizationObjects [PlayerNumber].Names.Count - 1
					|| i == ListOfVisualizationObjects [PlayerNumber].Names.Count - 2
					|| i == ListOfVisualizationObjects [PlayerNumber].Names.Count - 3) {
					if (ListOfVisualizationObjects [PlayerNumber].Buttons [i] != null) {
						ListOfVisualizationObjects [PlayerNumber].Names [i].gameObject.SetActive (true);
						ListOfVisualizationObjects [PlayerNumber].Buttons [i].gameObject.SetActive (true);
						if(ListOfVisualizationObjects [PlayerNumber].Shadow [i]!= null)ListOfVisualizationObjects [PlayerNumber].Shadow [i].gameObject.SetActive (true);
					}
				}

				if(ListOfVisualizationObjects [PlayerNumber].InputNames [i] != null)
					ListOfVisualizationObjects [PlayerNumber].InputNames [i].text = ListOfInputs [PlayerNumber].Gamepad [i];
			}
		}


	}


// --> Display keyboard inputs on screen
	void SwitchInputFromGamepadToDesktop(int PlayerNumber){																	
		if (PlayerNumber == 2) {
			for (int j = 0; j < ListOfVisualizationObjects.Count; j++) {
				for (int i = 0; i < ListOfVisualizationObjects [j].Names.Count; i++) {
					if(ListOfVisualizationObjects [j].InputNames [i] != null)
						ListOfVisualizationObjects [j].InputNames [i].text = ListOfInputs [j].Desktop [i];

					if (i == ListOfVisualizationObjects [j].Names.Count - 3
					    || i == ListOfVisualizationObjects [j].Names.Count - 2
						|| i == ListOfVisualizationObjects [j].Names.Count - 1) {
						if (ListOfVisualizationObjects [j].Buttons [i] != null) {
							ListOfVisualizationObjects [j].Names [i].gameObject.SetActive (false);
							ListOfVisualizationObjects [j].Buttons [i].gameObject.SetActive (false);
							if(ListOfVisualizationObjects [j].Shadow [i]!= null)ListOfVisualizationObjects [j].Shadow [i].gameObject.SetActive (false);
						}
					}
				}
			}
		} else {

			for (int i = 0; i < ListOfVisualizationObjects [PlayerNumber].Names.Count; i++) {
				if(ListOfVisualizationObjects [PlayerNumber].InputNames [i] != null)
					ListOfVisualizationObjects [PlayerNumber].InputNames [i].text = ListOfInputs [PlayerNumber].Desktop [i];

				if (i == ListOfVisualizationObjects [PlayerNumber].Names.Count - 3
					|| i == ListOfVisualizationObjects [PlayerNumber].Names.Count - 1
					|| i == ListOfVisualizationObjects [PlayerNumber].Names.Count - 2) {
					if (ListOfVisualizationObjects [PlayerNumber].Buttons [i] != null) {
						ListOfVisualizationObjects [PlayerNumber].Names [i].gameObject.SetActive (false);
						ListOfVisualizationObjects [PlayerNumber].Buttons [i].gameObject.SetActive (false);
						if(ListOfVisualizationObjects [PlayerNumber].Shadow [i]!= null)ListOfVisualizationObjects [PlayerNumber].Shadow[i].gameObject.SetActive (false);
					}
				}
			}
		}
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
		ListOfInputs [0].Gamepad [0] = 					"Joystick1Axis1";
		PlayerPrefs.SetString ("PP_1_Pad_Right", 		"Joystick1Axis1");
		ListOfInputs [0].Gamepad [1] = 					"Joystick1Axis1";
		PlayerPrefs.SetString ("PP_1_Pad_Acceleration", "Joystick1Axis6");
		ListOfInputs [0].Gamepad [2] = 					"Joystick1Axis6";
		PlayerPrefs.SetString ("PP_1_Pad_Break", 		"Joystick1Axis5");
		ListOfInputs [0].Gamepad [3] = 					"Joystick1Axis5";
		PlayerPrefs.SetString ("PP_1_Pad_Other", 		"");
		ListOfInputs [0].Gamepad [4] = 					"";
		PlayerPrefs.SetString ("PP_1_Pad_Respawn", 		"Joystick1Button19");
		ListOfInputs [0].Gamepad [5] = 					"Joystick1Button19";
		PlayerPrefs.SetString ("PP_1_Pad_Validate", 	"Joystick1Button16");
		ListOfInputs [0].Gamepad [6] = 					"Joystick1Button16";
		PlayerPrefs.SetString ("PP_1_Pad_Back",		 	"Joystick1Button17");
		ListOfInputs [0].Gamepad [7] =				 	"Joystick1Button17";
		PlayerPrefs.SetString ("PP_1_Pad_Pause", 		"Joystick1Button9");
		ListOfInputs [0].Gamepad [8] = 					"Joystick1Button9";


// -> Init Keyboard buttons Player 1
		PlayerPrefs.SetString ("PP_1_Desktop_Left", 		"LeftArrow");
		ListOfInputs [0].Desktop [0] = 						"LeftArrow";
		PlayerPrefs.SetString ("PP_1_Desktop_Right", 		"RightArrow");
		ListOfInputs [0].Desktop [1] = 						"RightArrow";
		PlayerPrefs.SetString ("PP_1_Desktop_Acceleration", "UpArrow");
		ListOfInputs [0].Desktop [2] = 						"UpArrow";
		PlayerPrefs.SetString ("PP_1_Desktop_Break", 		"DownArrow");
		ListOfInputs [0].Desktop [3] = 						"DownArrow";
		PlayerPrefs.SetString ("PP_1_Desktop_Other", 		"");
		ListOfInputs [0].Desktop [4] = 						"";
		PlayerPrefs.SetString ("PP_1_Desktop_Respawn", 		"H");
		ListOfInputs [0].Desktop [5] = 						"H";
		PlayerPrefs.SetString ("PP_1_Desktop_Validate", 	"");
		ListOfInputs [0].Desktop [6] = 						"";
		PlayerPrefs.SetString ("PP_1_Desktop_Back", 		"");
		ListOfInputs [0].Desktop [7] = 						"";
		PlayerPrefs.SetString ("PP_1_Desktop_Pause", 		"");
		ListOfInputs [0].Desktop [8] = 						"";



// -> Init Input Player 2 Joystick
		PlayerPrefs.SetString ("PP_2_Pad_Left", 		"Joystick2Axis1");										
		ListOfInputs [1].Gamepad [0] = 					"Joystick2Axis1";
		PlayerPrefs.SetString ("PP_2_Pad_Right", 		"Joystick2Axis1");
		ListOfInputs [1].Gamepad [1] = 					"Joystick2Axis1";
		PlayerPrefs.SetString ("PP_2_Pad_Acceleration", "Joystick2Axis6");
		ListOfInputs [1].Gamepad [2] = 					"Joystick2Axis6";
		PlayerPrefs.SetString ("PP_2_Pad_Break", 		"Joystick2Axis5");
		ListOfInputs [1].Gamepad [3] = 					"Joystick2Axis5";
		PlayerPrefs.SetString ("PP_2_Pad_Other", 		"");
		ListOfInputs [1].Gamepad [4] = 					"";
		PlayerPrefs.SetString ("PP_2_Pad_Respawn", 		"Joystick2Button19");
		ListOfInputs [1].Gamepad [5] = 					"Joystick2Button19";
		PlayerPrefs.SetString ("PP_2_Pad_Validate", 	"Joystick2Button16");
		ListOfInputs [1].Gamepad [6] = 					"Joystick2Button16";
		PlayerPrefs.SetString ("PP_2_Pad_Back",		 	"Joystick2Button17");
		ListOfInputs [1].Gamepad [7] =				 	"Joystick2Button17";
		PlayerPrefs.SetString ("PP_2_Pad_Pause", 		"Joystick2Button9");
		ListOfInputs [1].Gamepad [8] = 					"Joystick2Button9";


// -> Init Keyboard buttons Player 2
		PlayerPrefs.SetString ("PP_2_Desktop_Left", 		"S");
		ListOfInputs [1].Desktop [0] = 						"S";
		PlayerPrefs.SetString ("PP_2_Desktop_Right", 		"F");
		ListOfInputs [1].Desktop [1] = 						"F";
		PlayerPrefs.SetString ("PP_2_Desktop_Acceleration", "E");
		ListOfInputs [1].Desktop [2] = 						"E";
		PlayerPrefs.SetString ("PP_2_Desktop_Break", 		"D");
		ListOfInputs [1].Desktop [3] = 						"D";
		PlayerPrefs.SetString ("PP_2_Desktop_Other", 		"");
		ListOfInputs [1].Desktop [4] = 						"";
		PlayerPrefs.SetString ("PP_2_Desktop_Respawn", 		"C");
		ListOfInputs [1].Desktop [5] = 						"C";
		PlayerPrefs.SetString ("PP_2_Desktop_Validate", 	"");
		ListOfInputs [1].Desktop [6] = 						"";
		PlayerPrefs.SetString ("PP_2_Desktop_Back", 		"");
		ListOfInputs [1].Desktop [7] = 						"";
		PlayerPrefs.SetString ("PP_2_Desktop_Pause", 		"");
		ListOfInputs [1].Desktop [8] = 						"";

		if (s_ControllerType == "Gamepad") {											// Display gamepad controller values
			SwitchInputFromKeyboardToGamepad (0);
		} else if (s_ControllerType == "Keyboard"){																		// Display keyboard controller values
			SwitchInputFromGamepadToDesktop (0);
		}

		if (s_ControllerTypeP2 == "Gamepad") {											// Display gamepad controller values
			SwitchInputFromKeyboardToGamepad (1);
		} else if (s_ControllerTypeP2 == "Keyboard"){																		// Display keyboard controller values
			SwitchInputFromGamepadToDesktop (1);
		}

	}

	public void InputsData(){	
		// -> Init Input Player 1 Joystick
		PlayerPrefs.SetString ("PP_1_Pad_Left", 		defaultInputsValues.ListOfInputs[0].Gamepad[0]);										
		ListOfInputs [0].Gamepad [0] = 					defaultInputsValues.ListOfInputs[0].Gamepad[0];
		PlayerPrefs.SetString ("PP_1_Pad_Right", 		defaultInputsValues.ListOfInputs[0].Gamepad[1]);
		ListOfInputs [0].Gamepad [1] = 					defaultInputsValues.ListOfInputs[0].Gamepad[1];
		PlayerPrefs.SetString ("PP_1_Pad_Acceleration",defaultInputsValues. ListOfInputs[0].Gamepad[2]);
		ListOfInputs [0].Gamepad [2] = 					defaultInputsValues.ListOfInputs[0].Gamepad[2];
		PlayerPrefs.SetString ("PP_1_Pad_Break", 		defaultInputsValues.ListOfInputs[0].Gamepad[3]);
		ListOfInputs [0].Gamepad [3] = 					defaultInputsValues.ListOfInputs[0].Gamepad[3];
		PlayerPrefs.SetString ("PP_1_Pad_Other", 		"");
		ListOfInputs [0].Gamepad [4] = 					"";
		PlayerPrefs.SetString ("PP_1_Pad_Respawn", 		defaultInputsValues.ListOfInputs[0].Gamepad[5]);
		ListOfInputs [0].Gamepad [5] = 					defaultInputsValues.ListOfInputs[0].Gamepad[5];
		PlayerPrefs.SetString ("PP_1_Pad_Validate", 	defaultInputsValues.ListOfInputs[0].Gamepad[6]);
		ListOfInputs [0].Gamepad [6] = 					defaultInputsValues.ListOfInputs[0].Gamepad[6];
		PlayerPrefs.SetString ("PP_1_Pad_Back",		 	defaultInputsValues.ListOfInputs[0].Gamepad[7]);
		ListOfInputs [0].Gamepad [7] =				 	defaultInputsValues.ListOfInputs[0].Gamepad[7];
		PlayerPrefs.SetString ("PP_1_Pad_Pause", 		defaultInputsValues.ListOfInputs[0].Gamepad[8]);
		ListOfInputs [0].Gamepad [8] = 					defaultInputsValues.ListOfInputs[0].Gamepad[8];


		// -> Init Keyboard buttons Player 1
		PlayerPrefs.SetString ("PP_1_Desktop_Left", 		defaultInputsValues.ListOfInputs[0].Desktop[0]);
		ListOfInputs [0].Desktop [0] = 						defaultInputsValues.ListOfInputs[0].Desktop[0];
		PlayerPrefs.SetString ("PP_1_Desktop_Right", 		defaultInputsValues.ListOfInputs[0].Desktop[1]);
		ListOfInputs [0].Desktop [1] = 						defaultInputsValues.ListOfInputs[0].Desktop[1];
		PlayerPrefs.SetString ("PP_1_Desktop_Acceleration",defaultInputsValues. ListOfInputs[0].Desktop[2]);
		ListOfInputs [0].Desktop [2] = 						defaultInputsValues.ListOfInputs[0].Desktop[2];
		PlayerPrefs.SetString ("PP_1_Desktop_Break", 		defaultInputsValues.ListOfInputs[0].Desktop[3]);
		ListOfInputs [0].Desktop [3] = 						defaultInputsValues.ListOfInputs[0].Desktop[3];
		PlayerPrefs.SetString ("PP_1_Desktop_Other", 		"");
		ListOfInputs [0].Desktop [4] = 						"";
		PlayerPrefs.SetString ("PP_1_Desktop_Respawn", 		defaultInputsValues.ListOfInputs[0].Desktop[5]);
		ListOfInputs [0].Desktop [5] = 						defaultInputsValues.ListOfInputs[0].Desktop[5];
		PlayerPrefs.SetString ("PP_1_Desktop_Validate", 	"");
		ListOfInputs [0].Desktop [6] = 						"";
		PlayerPrefs.SetString ("PP_1_Desktop_Back", 		"");
		ListOfInputs [0].Desktop [7] = 						"";
		PlayerPrefs.SetString ("PP_1_Desktop_Pause", 		"");
		ListOfInputs [0].Desktop [8] = 						"";



		// -> Init Input Player 2 Joystick
		PlayerPrefs.SetString ("PP_2_Pad_Left", 		defaultInputsValues.ListOfInputs[1].Gamepad[0]);										
		ListOfInputs [1].Gamepad [0] = 					defaultInputsValues.ListOfInputs[1].Gamepad[0];
		PlayerPrefs.SetString ("PP_2_Pad_Right", 		defaultInputsValues.ListOfInputs[1].Gamepad[1]);
		ListOfInputs [1].Gamepad [1] = 					defaultInputsValues.ListOfInputs[1].Gamepad[1];
		PlayerPrefs.SetString ("PP_2_Pad_Acceleration", defaultInputsValues.ListOfInputs[1].Gamepad[2]);
		ListOfInputs [1].Gamepad [2] = 					defaultInputsValues.ListOfInputs[1].Gamepad[2];
		PlayerPrefs.SetString ("PP_2_Pad_Break", 		defaultInputsValues.ListOfInputs[1].Gamepad[3]);
		ListOfInputs [1].Gamepad [3] = 					defaultInputsValues.ListOfInputs[1].Gamepad[3];
		PlayerPrefs.SetString ("PP_2_Pad_Other", 		"");
		ListOfInputs [1].Gamepad [4] = 					"";
		PlayerPrefs.SetString ("PP_2_Pad_Respawn", 		defaultInputsValues.ListOfInputs[1].Gamepad[5]);
		ListOfInputs [1].Gamepad [5] = 					defaultInputsValues.ListOfInputs[1].Gamepad[5];
		PlayerPrefs.SetString ("PP_2_Pad_Validate", 	defaultInputsValues.ListOfInputs[1].Gamepad[6]);
		ListOfInputs [1].Gamepad [6] = 					defaultInputsValues.ListOfInputs[1].Gamepad[6];
		PlayerPrefs.SetString ("PP_2_Pad_Back",		 	defaultInputsValues.ListOfInputs[1].Gamepad[7]);
		ListOfInputs [1].Gamepad [7] =				 	defaultInputsValues.ListOfInputs[1].Gamepad[7];
		PlayerPrefs.SetString ("PP_2_Pad_Pause", 		defaultInputsValues.ListOfInputs[1].Gamepad[8]);
		ListOfInputs [1].Gamepad [8] = 					defaultInputsValues.ListOfInputs[1].Gamepad[8];


		// -> Init Keyboard buttons Player 2
		PlayerPrefs.SetString ("PP_2_Desktop_Left", 		defaultInputsValues.ListOfInputs[1].Desktop[0]);
		ListOfInputs [1].Desktop [0] = 						defaultInputsValues.ListOfInputs[1].Desktop[0];
		PlayerPrefs.SetString ("PP_2_Desktop_Right", 		defaultInputsValues.ListOfInputs[1].Desktop[1]);
		ListOfInputs [1].Desktop [1] = 						defaultInputsValues.ListOfInputs[1].Desktop[1];
		PlayerPrefs.SetString ("PP_2_Desktop_Acceleration", defaultInputsValues.ListOfInputs[1].Desktop[2]);
		ListOfInputs [1].Desktop [2] = 						defaultInputsValues.ListOfInputs[1].Desktop[2];
		PlayerPrefs.SetString ("PP_2_Desktop_Break", 		defaultInputsValues.ListOfInputs[1].Desktop[3]);
		ListOfInputs [1].Desktop [3] = 						defaultInputsValues.ListOfInputs[1].Desktop[3];
		PlayerPrefs.SetString ("PP_2_Desktop_Other", 		"");
		ListOfInputs [1].Desktop [4] = 						"";
		PlayerPrefs.SetString ("PP_2_Desktop_Respawn", 		defaultInputsValues.ListOfInputs[1].Desktop[5]);
		ListOfInputs [1].Desktop [5] = 						defaultInputsValues.ListOfInputs[1].Desktop[5];
		PlayerPrefs.SetString ("PP_2_Desktop_Validate", 	"");
		ListOfInputs [1].Desktop [6] = 						"";
		PlayerPrefs.SetString ("PP_2_Desktop_Back", 		"");
		ListOfInputs [1].Desktop [7] = 						"";
		PlayerPrefs.SetString ("PP_2_Desktop_Pause", 		"");
		ListOfInputs [1].Desktop [8] = 						"";

		if (s_ControllerType == "Gamepad") {											// Display gamepad controller values
			SwitchInputFromKeyboardToGamepad (0);
		} else if (s_ControllerType == "Keyboard"){																		// Display keyboard controller values
			SwitchInputFromGamepadToDesktop (0);
		}

		if (s_ControllerTypeP2 == "Gamepad") {											// Display gamepad controller values
			SwitchInputFromKeyboardToGamepad (1);
		} else if (s_ControllerTypeP2 == "Keyboard"){																		// Display keyboard controller values
			SwitchInputFromGamepadToDesktop (1);
		}

	}

	void LoadInputsValues(){																					// --> Load Inputs values on a list when the script start
		for (int i = 0; i < ListOfInputs.Count; i++) {
			ListOfInputs [i].Desktop [0] = PlayerPrefs.GetString ("PP_" + (i+1) + "_Desktop_Left");
			ListOfInputs [i].Desktop [1] = PlayerPrefs.GetString ("PP_" + (i+1) + "_Desktop_Right");
			ListOfInputs [i].Desktop [2] = PlayerPrefs.GetString ("PP_" + (i+1) + "_Desktop_Acceleration");
			ListOfInputs [i].Desktop [3] = PlayerPrefs.GetString ("PP_" + (i+1) + "_Desktop_Break");
			ListOfInputs [i].Desktop [4] = PlayerPrefs.GetString ("PP_" + (i+1) + "_Desktop_Other");
			ListOfInputs [i].Desktop [5] = PlayerPrefs.GetString ("PP_" + (i+1) + "_Desktop_Respawn");
			ListOfInputs [i].Desktop [6] = PlayerPrefs.GetString ("PP_" + (i+1) + "_Pad_Validate");
			ListOfInputs [i].Desktop [7] = PlayerPrefs.GetString ("PP_" + (i+1) + "_Pad_Back");
			ListOfInputs [i].Desktop [8] = PlayerPrefs.GetString ("PP_" + (i+1) + "_Pad_Pause");

			ListOfInputs [i].Gamepad [0] = PlayerPrefs.GetString ("PP_" + (i+1) + "_Pad_Left");
			ListOfInputs [i].Gamepad [1] = PlayerPrefs.GetString ("PP_" + (i+1) + "_Pad_Right");
			ListOfInputs [i].Gamepad [2] = PlayerPrefs.GetString ("PP_" + (i+1) + "_Pad_Acceleration");
			ListOfInputs [i].Gamepad [3] = PlayerPrefs.GetString ("PP_" + (i+1) + "_Pad_Break");
			ListOfInputs [i].Gamepad [4] = PlayerPrefs.GetString ("PP_" + (i+1) + "_Pad_Other");
			ListOfInputs [i].Gamepad [5] = PlayerPrefs.GetString ("PP_" + (i+1) + "_Pad_Respawn");
			ListOfInputs [i].Gamepad [6] = PlayerPrefs.GetString ("PP_" + (i+1) + "_Pad_Validate");
			ListOfInputs [i].Gamepad [7] = PlayerPrefs.GetString ("PP_" + (i+1) + "_Pad_Back");
			ListOfInputs [i].Gamepad [8] = PlayerPrefs.GetString ("PP_" + (i+1) + "_Pad_Pause");
		}
	}

	public void InitWhenButtonInputsIsPressed () {
		LoadInputsValues();																				// Load the current inputs values

		s_ControllerType = "Keyboard";																	// Display the Keyboard inputs by default
		s_ControllerTypeP2 = "Keyboard";	
		for (int i = 0; i < 2; i++) {																	// Init text and buttons for Player and Player 2
			GameObject Btn_Choose_Controller = GameObject.Find ("Btn_Choose_Controller_J" +(i+1).ToString());
			if (Btn_Choose_Controller) {
				Text txt_btn = Btn_Choose_Controller.transform.Find ("txt").gameObject.GetComponent<Text> ();
				txt_btn.text = s_ControllerType;
			}
		}
		SwitchInputFromGamepadToDesktop (2);														
	}


	void StopAllCoroutine(){															
		StopAllCoroutines ();
		ListOfVisualizationObjects [0].Info.text = "";
		ListOfVisualizationObjects [1].Info.text = "";
	}

	void NewTextInfo(Text tmpText,string newText){	
		StopCoroutine ("IE_NewTextInfo");
		object[] parms = new object[2]{tmpText,newText};
		StartCoroutine ("IE_NewTextInfo", parms);
	}

	IEnumerator IE_NewTextInfo(object[] parms){
		Text tmpText = (Text)parms[0];
		string newText = (string)parms[1];
		tmpText.text = newText;
		yield return new WaitForSeconds(2);
		tmpText.text = "";
	}


	public void EnableDisabled_SetupInput(bool newBool){
		b_Enabled = newBool;
	}



}

