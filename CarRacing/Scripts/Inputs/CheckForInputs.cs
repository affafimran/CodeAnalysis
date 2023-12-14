// Description : CheckForInputs.cs : use to detect submit and cancel action with gamepads and keyboard
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CheckForInputs : MonoBehaviour {

	private EventSystem							eventSystem; 										// access the evenSystem 
	private List<string>						l_gamepadNames = new List<string>();				// list of gamepads name

	private string								inputValidateJoystick1 = "";
	private string								inputValidateJoystick2 = "";
	private string								inputPauseJoystick1 = "";
	private string								inputPauseJoystick2 = "";

	public	GameObject 							obj_Result;
	public	GameObject 							obj_SaveYourScore;

	// Use this for initialization
	void Start () {
		eventSystem = GameObject.Find ("EventSystem").GetComponent<EventSystem> ();
		if (!eventSystem)Debug.Log ("MCR Creator : You need an EventSystem gameObject on your scene");
		updateInputs ();
	}
	
	// Update is called once per frame
	void Update () {
		if(
			Input.anyKeyDown 																			// --> A button is pressed 
			&& eventSystem.currentSelectedGameObject != null												// A button is selected on screen
			&& eventSystem.currentSelectedGameObject.tag == "Untagged"										// the button is tagged "Untagged"
			&& FindTheKeyCodeUpdate ().ToString ().Contains ("Button")										// it is input from a Joystick 

		) {

			string tmpInputName = FindTheKeyCodeUpdate ().ToString ();
			l_gamepadNames.Clear ();																		// Save on a list : gamepads names
			for (int i = 0; i < Input.GetJoystickNames ().Length; i++) {
				l_gamepadNames.Add (Input.GetJoystickNames () [i] + " : " + i.ToString ());
			}

			for (int i = 0; i < l_gamepadNames.Count; i++) {
				for (int j = 0; j < 20; j++) {	
					if (l_gamepadNames [i] == Input.GetJoystickNames () [i] + " : " + i.ToString ()
					   && tmpInputName == "Joystick" + (i + 1).ToString () + "Button" + j) {

						GameObject tmpBtn2 = eventSystem.currentSelectedGameObject;							// access the current selected button
						eventSystem.sendNavigationEvents = false;
						if (tmpBtn2.name == "Mute")
							tmpBtn2.GetComponent<ToggleSoundManager> ().GamePadMuteUnMuteSound();				// Mute sound button case	
						else
							tmpBtn2.GetComponent<Button> ().onClick.Invoke ();									// All the other buttons : Invoke all onClic() actions								
						eventSystem.sendNavigationEvents = true;
					}
				}
			}
		}
// --> Pause Case
		else if(Input.anyKeyDown 
			&& eventSystem.currentSelectedGameObject == null
			&& FindThePauseKeyCodeUpdate () == KeyCode.P
		
			||

			eventSystem.currentSelectedGameObject != null												// A button is selected on screen
			&& eventSystem.currentSelectedGameObject.tag == "btn_Pause"										// the button is tagged "Untagged"
			&& FindThePauseKeyCodeUpdate () == KeyCode.P
		){														// Check KeyCode.P on keyboard. Start Pause
			//Debug.Log ("Keyboard");
			if(!obj_Result && !obj_SaveYourScore 
				|| obj_SaveYourScore && obj_Result && !obj_Result.activeSelf && !obj_SaveYourScore.activeSelf)			// Pause is not allowed when race is finished
			F_OpenMenu ();
		}
		else if(Input.anyKeyDown 
			&& eventSystem.currentSelectedGameObject == null
			&& FindThePauseKeyCodeUpdate ().ToString ().Contains ("Button")
		
			||

			eventSystem.currentSelectedGameObject != null												// A button is selected on screen
			&& eventSystem.currentSelectedGameObject.tag == "btn_Pause"										// the button is tagged "Untagged"
			&& FindThePauseKeyCodeUpdate().ToString ().Contains ("Button")										// it is input from a Joystick


		
		){								
			if(!obj_Result && !obj_SaveYourScore 
				|| obj_SaveYourScore && obj_Result && !obj_Result.activeSelf && !obj_SaveYourScore.activeSelf)			// Pause is not allowed when race is finished
			F_OpenMenu ();
		}
	}

	public KeyCode FindTheKeyCodeUpdate(){																// --> find the input

		foreach(KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
		{
			if(Input.GetKeyDown(key) && key.ToString () == inputValidateJoystick1
				|| Input.GetKeyDown(key) && key.ToString () == inputValidateJoystick2
				)
			{
				return key;
			}
		}
		return KeyCode.None;
	}

	public KeyCode FindThePauseKeyCodeUpdate(){															// --> find the input

		foreach(KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
		{
			if(Input.GetKeyDown(key) && key.ToString () == inputPauseJoystick1
				|| Input.GetKeyDown(key) && key.ToString () == inputPauseJoystick2
				|| Input.GetKeyDown(key) && key.ToString () == "P"
			)
			{
				return key;
			}
		}
		return KeyCode.None;
	}


	public void updateInputs (){
		inputValidateJoystick1 = PlayerPrefs.GetString ("PP_1_Pad_Validate");
		inputPauseJoystick1 = PlayerPrefs.GetString ("PP_1_Pad_Pause");

		inputValidateJoystick2 = PlayerPrefs.GetString ("PP_2_Pad_Validate");
		inputPauseJoystick2 = PlayerPrefs.GetString ("PP_2_Pad_Pause");

	}
		
	public void F_OpenMenu (){
		updateInputs ();
		GameObject tmpMenu = GameObject.Find ("Canvas_MainMenu");

		if (tmpMenu) {
			tmpMenu.GetComponent<Menu_Manager> ().GoToOtherPageWithHisNumber (0);					// Open the menu settings 
			GameObject tmpButton = GameObject.Find ("btn_Sound");									// Select the button "btn_Sound"
			if (tmpButton) {
				eventSystem.SetSelectedGameObject (tmpButton);
				eventSystem.gameObject.GetComponent<StandaloneInputModule> ().submitButton = "Submit";
			}
		}
			
		GameObject tmpPause = GameObject.Find ("PauseManager");

		if (tmpPause) {
			tmpPause.GetComponent<PauseManager> ().PauseGame ();									// Start Pause Game 
		}
	}
}
