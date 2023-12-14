// Description : MainMenuManageChooses.cs : Save info when player navigate on menus. These info are used when a scene is loading
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManageChooses : MonoBehaviour {


	public string b_ModeSoloMulti 	= "Solo";
	public string b_GameMode 		= "Arcade";


// --> Know if payer want to play on solo mode or multiplayer mode
	public void Choose_Solo_Or_Multi (string newMode){									
		b_ModeSoloMulti = newMode;
		if (b_ModeSoloMulti == "Solo")														// Button solo on Page Hub Menu
			PlayerPrefs.SetInt ("HowManyPlayers", 1);										
		else if (b_ModeSoloMulti == "Multi")												// Button Versus on Page Hub Menu
			PlayerPrefs.SetInt ("HowManyPlayers", 2);
	}


// --> Know if player want to play Arcade Mode or Time Trial Mode.
	public void Choose_GameMode (string newMode){										
		b_GameMode = newMode;																
		if (newMode == "Arcade")													// button Arcade on Page_RaceModeSelection
			PlayerPrefs.SetString ("Which_GameMode", newMode);
		else if (newMode == "TimeTrial")											// button TimeTrial on Page_RaceModeSelection
			PlayerPrefs.SetString ("Which_GameMode", newMode);
	}
}
