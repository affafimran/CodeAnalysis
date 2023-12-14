// ENGINE SCRIPT: AVOID PUTTING GAME SPECIFIC CODE IN HERE

using UnityEngine;
using System.Collections;

public class InputManager
{
	// Constructor
	public InputManager(){
	}

	public void Update(){

		if (Data.pausingAllowed)
		{
			bool tPauseKey = Input.GetKeyUp(KeyCode.P) || Input.GetKeyUp(KeyCode.Escape) || Input.GetButtonUp("Joystick Pause");
			if (tPauseKey)
			{
				if (Data.pause)
				{
					Scripts.levelScript.UnPauseGame();
				} else
				{
					Scripts.levelScript.PauseGame();
				}
			}
		}
	}
}
