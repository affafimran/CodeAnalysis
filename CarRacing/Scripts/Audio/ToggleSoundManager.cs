//Description : ToggleSoundManager.cs : call mute or unmute sound for the gamepad
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleSoundManager : MonoBehaviour {

	public AudioSource 		clic;
	public AudioManager		audioListener;
	public Toggle			toggle;


	public void GamePadMuteUnMuteSound () {
		if (clic)
			clic.Play ();
		if (audioListener && toggle) {
			if (toggle.isOn)
				toggle.isOn = false;
			else
				toggle.isOn = true;


			audioListener.MuteUnmuteAudio (toggle.isOn);
		}
	}
}
