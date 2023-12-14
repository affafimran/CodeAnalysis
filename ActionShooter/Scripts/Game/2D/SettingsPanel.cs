using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// SettingsPanel.
/// Pretty complex script handling some options for the game engine.
/// Has a lot of buttons that all need to be set/reset properly.
/// It's important that the hierarchy of these objects is not fiddles with too much.
/// </summary> 

public class SettingsPanel : MonoBehaviour {

	public GameObject audioSettings;
	public GameObject graphics;
	public GameObject controls;

	public GameObject settingsBackButton;
	public GameObject settingsWindow;
	public GameObject music;
	public GameObject musicVolume;
	public GameObject sound;
	public GameObject soundVolume;
	public GameObject fullscreen;
	public GameObject fullscreenResolution;
	public GameObject effectsQuality;
	public GameObject invertMouse;
	public GameObject mouseSensitivity;
	public GameObject cameraMode;
	
	public GameObject aButton;
	
	public Color normalColor;
	public Color greyColor;
	
	void OnEnable () {

		// hack!
		graphics.SetActive(true);

		ResetButtonStates(music);
		ResetButtonStates(musicVolume);
		ResetButtonStates(sound);
		ResetButtonStates(soundVolume);
		ResetButtonStates(fullscreen);
		ResetButtonStates(fullscreenResolution);
		ResetButtonStates(effectsQuality);
		ResetButtonStates(invertMouse);
		ResetButtonStates(mouseSensitivity);

		// Set the initial button states.
		// Music
		SetButtonState(GameObject.Find("MusicOnButton"), Data.music);
		SetButtonState(GameObject.Find("MusicOffButton"), !Data.music);
		int tCurrentVolume = (int)(Data.musicVolume*10);
		bool tState = false;
		for (int i = 0; i <= 10; i++) {
			tState = false;
			if (i==tCurrentVolume) tState = true;
			//Debug.Log (tState);
			SetButtonState(GameObject.Find("MusicVolume"+i+"Button"), tState);
		}
		
		// SFX
		SetButtonState(GameObject.Find("SoundOnButton"), Data.sfx);
		SetButtonState(GameObject.Find("SoundOffButton"), !Data.sfx);
		tCurrentVolume = (int)(Data.sfxVolume*10);
		tState = false;
		for (int i = 0; i <= 10; i++) {
			tState = false;
			if (i==tCurrentVolume) tState = true;
			SetButtonState(GameObject.Find("SoundVolume"+i+"Button"), tState);
		}
		
		// Fullscreen
		SetButtonState(GameObject.Find("FullscreenOnButton"), Screen.fullScreen);
		SetButtonState(GameObject.Find("FullscreenOffButton"), !Screen.fullScreen);
		
		
		Data.fullScreenWidth = Screen.currentResolution.width;
		Data.fullScreenHeight = Screen.currentResolution.height;
		
		Debug.Log("Setting the current screen display resolution to the fullscreen resolution: " + Data.fullScreenWidth.ToString() + " x " + Data.fullScreenHeight.ToString());
		
		// TODO: set all buttons on or off depending on what the current state is!
		SetButtonState(GameObject.Find("FullscreenResolution0Button"), (Data.fullScreenWidth == 1024 && Data.fullScreenHeight == 768));
		SetButtonState(GameObject.Find("FullscreenResolution1Button"), (Data.fullScreenWidth == 1280 && Data.fullScreenHeight == 720));
		SetButtonState(GameObject.Find("FullscreenResolution2Button"), (Data.fullScreenWidth == 1280 && Data.fullScreenHeight == 1024));
		SetButtonState(GameObject.Find("FullscreenResolution3Button"), (Data.fullScreenWidth == 1366 && Data.fullScreenHeight == 768));
		SetButtonState(GameObject.Find("FullscreenResolution4Button"), (Data.fullScreenWidth == 1680 && Data.fullScreenHeight == 1050));
		SetButtonState(GameObject.Find("FullscreenResolution5Button"), (Data.fullScreenWidth == 1920 && Data.fullScreenHeight == 1080));
		SetButtonState(GameObject.Find("FullscreenOffButton"), !Screen.fullScreen);
		
		// MOUSE
		SetButtonState(GameObject.Find("InvertMouseOnButton"), GameData.invertMouse);
		SetButtonState(GameObject.Find("InvertMouseOffButton"), !GameData.invertMouse);
		int tCurrentSensitivity = (int)(GameData.mouseSensitivity*10);
		tState = false;
		for (int i = 0; i <= 10; i++) {
			tState = false;
			if (i==tCurrentSensitivity) tState = true;
			SetButtonState(GameObject.Find("MouseSensitivity"+i+"Button"), tState);
		}

		SetButtonState(GameObject.Find("EffectsQualityLowButton"), (Data.quality == "Fastest"));
		SetButtonState(GameObject.Find("EffectsQualityMediumButton"), (Data.quality == "Simple"));
		SetButtonState(GameObject.Find("EffectsQualityHighButton"), (Data.quality == "Fantastic"));

		// [MOBILE] We disabled the graphics part of the settings since we're a mobile version
		// No need in setting resolutions OR fullscreen mode. Furthermore we disabled the quality settings to focus on optimizing a default version
		if (GameData.mobile){
			graphics.SetActive(false); // hide graphics part
			Vector3 newPosition = controls.GetComponent<RectTransform>().anchoredPosition; // calculate a new position
			newPosition.y = 60; // update position
			controls.GetComponent<RectTransform>().anchoredPosition = newPosition; // move controls upwards so it aligns nicely below audio.
		}
	}


	void Update(){
		#if UNITY_ANDROID
		if (Input.GetKeyUp(KeyCode.Escape)) Scripts.interfaceScript.OnButton(settingsBackButton);
		#endif
	}


	// Buttons are referred to here by the InterfaceScript.
	public void OnButton(GameObject go)
	{
		aButton = go;
		OnButton(go.name);
	}

	public void OnButton(string aButtonName)
	{
		Debug.Log("Settings OnButton received: " + aButtonName);

		aButton = GameObject.Find(aButtonName);

		switch(aButtonName)
		{
			
		case "MusicOnButton":
			ResetButtonStates(music);
			Data.music = true;
			Scripts.audioManager.MuteMusic(false);
			break;
			
		case "MusicOffButton":
			ResetButtonStates(music);
			Data.music = false;
			Scripts.audioManager.MuteMusic(true);
			break;
			
		case "SoundOnButton":
			ResetButtonStates(sound);
			Data.sfx = true;
			Scripts.audioManager.MuteSFX(false);
			break;
			
		case "SoundOffButton":
			ResetButtonStates(sound);
			Data.sfx = false;
			Scripts.audioManager.MuteSFX(true);
			break;
			
		case "FullscreenOnButton":
			Screen.fullScreen = true;
			ResetButtonStates(fullscreen);
			break;
			
		case "FullscreenOffButton":
			Screen.fullScreen = false;
			ResetButtonStates(fullscreen);
			break;
			
		case "InvertMouseOnButton":
			GameData.invertMouse = true;
			ResetButtonStates(invertMouse);
			break;
			
		case "InvertMouseOffButton":
			GameData.invertMouse = false;
			ResetButtonStates(invertMouse);
			break;
		}
		
		if (aButtonName.Contains("MusicVolume"))
		{
			int selectedMusicVolume = int.Parse(aButton.transform.Find("Label").gameObject.GetComponent<Text>().text);
			Debug.Log("MusicVolume received:" + selectedMusicVolume.ToString());
			Data.musicVolume = selectedMusicVolume * 0.1f;
			Scripts.audioManager.SetAllMusicVolume(Data.musicVolume);
			ResetButtonStates(musicVolume);
		}
		
		if (aButtonName.Contains("SoundVolume"))
		{
			int selectedSoundVolume = int.Parse(aButton.transform.Find("Label").gameObject.GetComponent<Text>().text);
			Debug.Log("SoundVolume received:" + selectedSoundVolume.ToString());
			Data.sfxVolume = selectedSoundVolume * 0.1f;
			Scripts.audioManager.SetAllSFXVolume(Data.sfxVolume);
			ResetButtonStates(soundVolume);
		}
		
		if (aButtonName.Contains("FullscreenResolution"))
		{
			Debug.Log("FullscreenResolution received:" + aButton.transform.Find("Label").gameObject.GetComponent<Text>().text);
			if (aButtonName.Contains("0"))
			{
				Data.fullScreenWidth = 1024;
				Data.fullScreenHeight = 768;
			}
			if (aButtonName.Contains("1"))
			{
				Data.fullScreenWidth = 1280;
				Data.fullScreenHeight = 720;
			}
			if (aButtonName.Contains("2"))
			{
				Data.fullScreenWidth = 1280;
				Data.fullScreenHeight = 1024;
			}
			
			if (aButtonName.Contains("3"))
			{
				Data.fullScreenWidth = 1366;
				Data.fullScreenHeight = 768;
			}
			
			if (aButtonName.Contains("4"))
			{
				Data.fullScreenWidth = 1680;
				Data.fullScreenHeight = 1050;
			}
			
			if (aButtonName.Contains("5"))
			{
				Data.fullScreenWidth = 1920;
				Data.fullScreenHeight = 1080;
			}
			Screen.SetResolution(Data.fullScreenWidth, Data.fullScreenHeight, Screen.fullScreen);
			ResetButtonStates(fullscreenResolution);
		}
		
		if (aButtonName.Contains("EffectsQuality"))
		{
			Debug.Log("EffectsQuality received:" + aButton.transform.Find("Label").gameObject.GetComponent<Text>().text);
			switch (aButtonName)
			{
			case "EffectsQualityHighButton":
				Data.quality = "Fantastic";
				Options.SetVisuals();
				break;
			case "EffectsQualityMediumButton":
				Data.quality = "Simple";
				Options.SetVisuals();
				break;
			case "EffectsQualityLowButton":
				Data.quality = "Fastest";
				Options.SetVisuals();
				break;
			}
			
			ResetButtonStates(effectsQuality);
		}
		
		if (aButtonName.Contains("MouseSensitivity"))
		{
			int selectedSensitivity = int.Parse(aButton.transform.Find("Label").gameObject.GetComponent<Text>().text);
			Debug.Log("MouseSensitivity received:" + selectedSensitivity);
			GameData.mouseSensitivity = Mathf.Max(0.05f, selectedSensitivity * 0.1f);
			ResetButtonStates(mouseSensitivity);
		}
		
		SetButtonState(aButton, true);
		Scripts.audioManager.PlaySFX("Interface/Select");
		
		UserData.Save();
	}
	
	public void SetButtonState (GameObject go, bool state)
	{
		go.GetComponent<Image>().enabled = state;
		go.GetComponent<Button>().enabled = !state;
		if (state) go.transform.Find("Label").GetComponent<Text>().color = greyColor;
		else go.transform.Find("Label").GetComponent<Text>().color = normalColor;
	}
	
	public void ResetButtonStates(GameObject parentGameObject)
	{
		foreach (Transform t in parentGameObject.transform)
		{
			SetButtonState(t.gameObject, false);
		}
	}

	public void ResetPanel()
	{
		OnEnable();
	}
	
}
