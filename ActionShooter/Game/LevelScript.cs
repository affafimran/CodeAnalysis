using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class LevelScript : MonoBehaviour
{
	// public vars
	//public GUITexture mainInterfaceTexture = null;
	public InterfaceScript interfaceScript = null;
	public GameObject interfacePrefab;
	public GameObject eventSystemPrefab;

	public GameObject interfaceRoot = null;

	private bool acceptUserInput = false;
	private float pTimeScale = 1.0f;

	// Use this for initialization
	 void Awake()
	{
		// (DG) Added this again to be able to instantly test changes in the Game scene. Seems to work.
		if (GameObject.Find("StartUp") == null)
		{
			Debug.LogWarning("[LevelScript] Switching to StartUp scene!");
			gameObject.SetActive(false);
			Application.LoadLevel("StartUp");
			return;
		}

		// Store me in Scripts...
		Scripts.levelScript = this;

		// Get Interfacescript
//		GameObject cloneInterface = Instantiate(interfacePrefab) as GameObject;
//		GameObject cloneEventSystem = Instantiate(eventSystemPrefab) as GameObject;
//		interfaceScript = Scripts.interfaceScript;

		// Create the camera
		CameraManager.Initialize();
		CameraManager.AddCamera();
	


//		if (Scripts.shopItems != null) Scripts.shopItems.Init(); // (DG) Moved to MissionManager only now. Hope this is okay.

		// DG Disabled it now. It's already set in GameData, and the post effects are not also set in the CameraManager.AddCamera
//		Options.SetVisuals(); // TODO: needs reimplentation
//
//
//		// Sart it: TODO: rename all!
//		StartLevel();

	}

	//----------------------------------------------------------------------------------
	// Update is called once per frame
	//----------------------------------------------------------------------------------
	void Update()
	{
		Scripts.audioManager.Update();
		if(acceptUserInput) Scripts.inputManager.Update();
		Scripts.cheatManager.Update();
	}

	public void StartLevel()
	{
		// Setup level
		MissionManager.SetupMission(GameData.mission);
		acceptUserInput = true;      // user input allowed
	}

	public void PauseGame()
	{
		Debug.Log("[LevelScript] Game paused");

		CursorManager.SetCursor(true); // Show and unlock the cursor

		pTimeScale = Time.timeScale;  // store timeScale so we can restore it at unpause
		Time.timeScale = 0.0f;  // stops physics

		// make sure SFX is muted
		if (Data.sfx && !Data.muteAllSound)
			Scripts.audioManager.MuteSFX(true);

		Scripts.audioManager.SetAllMusicVolume(Data.Shared["Misc"].d["MusicPauseVolume"].f);

		Scripts.interfaceScript.PauseGame();

		Data.pause = true;
	}

	public void UnPauseGame()
	{
		CursorManager.SetCursor(false); // Show and unlock the cursor

		Time.timeScale = pTimeScale;  // restore physics

		// unmute SFX if settings are right
		if (Data.sfx && !Data.muteAllSound)
			Scripts.audioManager.MuteSFX(false);

		Scripts.audioManager.SetAllMusicVolume(Data.Shared["Misc"].d["MusicVolume"].f);

		Scripts.interfaceScript.UnPauseGame();

		Data.pause = false;
	}


	private void LevelComplete()
	{
		Data.pausingAllowed = false;  // make sure pausing is not allowed
		//GameData.player.playerScript.LevelComplete();
		//interfaceScript.LevelComplete();
		
		//UserData.Save();  // this is where you usually save user data
	}

	public void NextLevel()
	{
		if (Data.pause) UnPauseGame();
		Debug.Log("Attempt to go to next Level");
		LoadApplicationLevel("Loading");
	}

	// call this when the screen is blacked out
	public void RetryGame()
	{
		if (Data.pause) UnPauseGame();
		Debug.Log("Attempt to retry Level");
		Data.retried = true;
		LoadApplicationLevel("Game");
	}

	// call this when the screen is blacked out
	public void QuitGame()  // don't really like this function name... QuitLevel would be nicer, we may want to use QuitGame on Android in the future
	{
		if (Data.pause) UnPauseGame();
		Debug.Log("Attempt to quit Level");
		Data.retried = false;
		LoadApplicationLevel("Game");
	}

	private static void LoadApplicationLevel(string aScene)
	{
		Scripts.audioManager.StopAllAudio();
		Application.LoadLevel(aScene);
	}

	void OnDestroy()
	{
		Time.timeScale = 1.0f;  // force normal speed
		if (Data.music) AudioListener.volume = 1.0f;  // we fade out music, set it back to normal here
		Scripts.levelScript = null;
	}
}
