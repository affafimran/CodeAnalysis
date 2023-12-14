// Description : AudioManager.cs : Functions to manage global volume in Audio Menu Page
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour {

	public float 		volume = 0;
	public Slider 		tmpSliderVolume;
	public Toggle 		tmpToggleEnebleAudio;

	// Use this for initialization
	void Awake () {
		if (PlayerPrefs.HasKey ("GlobalVolume"))
			volume = PlayerPrefs.GetFloat ("GlobalVolume");
		else {
			volume = 1;
			PlayerPrefs.SetFloat ("GlobalVolume", volume);
		}

		if(tmpSliderVolume)tmpSliderVolume.value = volume ;
		AudioListener.volume = volume;

		if (tmpToggleEnebleAudio && volume != 0)
			tmpToggleEnebleAudio.isOn = true;
	}

// --> Change the volume in the application
	public void ChangeGlobalVolume (float newVolume) {
		volume = newVolume;
		PlayerPrefs.SetFloat ("GlobalVolume", volume);
		AudioListener.volume = volume;

		if (tmpToggleEnebleAudio && volume == 0)
			tmpToggleEnebleAudio.isOn = false;
		else if(!tmpToggleEnebleAudio.isOn)
			tmpToggleEnebleAudio.isOn = true;
	}

// --> Mute or Unmute sound in the application
	public void MuteUnmuteAudio(bool newAudioState){
		if (newAudioState) {
			PlayerPrefs.SetFloat ("GlobalVolume", .5F);
			volume = .5F;
			if(tmpSliderVolume)tmpSliderVolume.value = volume ;
			AudioListener.volume = .5F;
		} else {
			PlayerPrefs.SetFloat ("GlobalVolume", 0);
			volume = 0;
			if(tmpSliderVolume)tmpSliderVolume.value = volume ;
			AudioListener.volume = 0;
		}
	}
}
