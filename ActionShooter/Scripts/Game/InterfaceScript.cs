using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// InterfaceScript is an essential script for the game flow.
/// It's always running, and determines what happens when the scene starts.
///	All UI panels are references here as public variables.
/// So make sure these are available and assigned properly.
/// Main purpose of the scripts is flow / panel and button / navigation management.
/// This script also contains some stuff that ought to be moved elsewhere.
/// But isn't yet for various reasons.
/// Other UI related functionality is mostly done by scripts on the panel themselves.
/// The GamePanel script being a good example of this.
/// </summary> 

public class InterfaceScript : MonoBehaviour
{
	public EventSystem eventSystem; // Required to processes events for buttons.

	public GameObject preloaderPanel;
	public GameObject splashPanel;
	public GameObject missionSelectPanel;
	public GameObject shopPanel;
	public GameObject gamePanel;
	public GameObject fadePanel;
	public GameObject menuPanel;
	public GameObject achievementPanel;
	public GameObject pausePanel;
	public GameObject optionsPanel;
	public GameObject gameOverPanel;
	public GameObject resultsPanel;
	public GameObject helpPanel;
	public GameObject settingsPanel;
	public GameObject achievementsOverviewPanel;
	public GameObject confirmationPanel;
	public GameObject creditsPanel;
	public GameObject endGamePanel;
	public GameObject overlayPanel;

	public GamePanel gamePanelScript;
	private GameObject goToPanel;

	private Text creditsVersionText;

	public bool cinematicInProgress;
	private string previousCameraSetting;
	private GameObject previousCameraGameObject;

	public GameObject selectedGUIObject;


	void Awake()
	{
		// Check if we're starting in the correct unity scene.
		if (GameObject.Find("StartUp") == null)
		{
			Debug.LogWarning("[InterfaceScript] Deactivating InterfaceScript.");
			gameObject.SetActive(false);
			return;
		}
		// Reset the time to 1, this should be done elsewhere.
		Time.timeScale = 1.0f;

		// Register this into Scripts for access elsewhere.
		Scripts.interfaceScript = this;

		// So that we can easily access sprites on a sheet.
		AtlasManager.Initialize();

		// Get and display the version on the credits panel
		creditsVersionText = creditsPanel.transform.Find("Version").GetComponent<Text>();
		creditsVersionText.text = XLocalization.Get("VersionText") + ": " + Data.versionNumber;

		// Check for controller input
		CheckForControllers();

		// Deactivate all panels in case we accidentally left one active when applying prefab.
		foreach (Transform t in transform)
		{
			t.gameObject.SetActive(false);
		}

		// Some panels have their own scripts running on them, and need to be accessable all the time.
		// To make sure we can activate overlays at all time.
		overlayPanel.SetActive(true);
		// To make sure we can add fades!
		fadePanel.SetActive(true);
		// To make sure we can show trophies
		achievementPanel.SetActive(true);
	}

	void Start()
	{
		Debug.Log ("[InterfaceScript] Scene is : " + Data.scene);
		if (Data.scene == "Menu"){
			if (Data.splash) StartSplash();
			else StartMenu();
		}else{
			StartGame();
		}
	}

	void Update()
	{
		// Hack:  Added this in case the reference to the eventsystem is lost somehow.
		if (eventSystem != null) selectedGUIObject = eventSystem.currentSelectedGameObject;
		else eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();

		// Hack: making sure all buttons in the hierarchy have OnClick events specified.
		// This prevents you having to define these manually for all buttons.

		bool updateOverride = GameData.mobile; // [MOBILE] bool/toggle to force run this code. It is essential since we need the additional mobile buttons
		if (!gamePanel.activeSelf || updateOverride) // Added this for optimizing reasons.
		{
			// Making sure that all buttons in this interface are clickable
			SetUpButtonsInHierarchy(gameObject); // (DG) We might need to investigate if we should call this every frame.

			// Controller hack.
			// Keeping track of which GUI object is selected. Imperative for joystick control.
			// It seems that it's possible to have a button/object selected although it's not active. 
			// Furthermore, you can select a gameobject that is not a button. Which is not what we want since it messes up joystick navigation.
			// Could maybe later be rewritten to something that not has to be called in Update.
			if (Data.controllerUseDetected)
			{
				if (selectedGUIObject == null) SelectAnyButtonGameObject(); // (DG) We don't have an object selected, so pick one!
				else
				{
					if (!selectedGUIObject.activeSelf) SelectAnyButtonGameObject(); // (DG) it seems that the object is not active, so it's probably not one we want.
					else
					{
						Button tempButton = selectedGUIObject.GetComponent<Button>();
						if (tempButton == null) SelectAnyButtonGameObject(); // (DG) it seems that it's not a button at all!
						else if(selectedGUIObject.name == "PlayButton" && !menuPanel.activeSelf) SelectAnyButtonGameObject(); // (DG) Special exception for if we thing we're still in the menupanel.
					}
				}
			}
		}

		// [MOBILE] [ANDROID] Android exception so we can use the hardware back button to return to a previous menu panel or exit the app completely/
		#if UNITY_ANDROID
		if (Input.GetKeyUp(KeyCode.Escape)){
			if(creditsPanel.activeSelf){OnButton("CreditsBackButton"); return;} // back to main menu (DON'T FORGET THE return, otherwise the it will carry on)
			if(helpPanel.activeSelf){OnButton("HelpBackButton"); return;} // back to main menu (DON'T FORGET THE return, otherwise the it will carry on)
			if(menuPanel.activeSelf) Application.Quit(); // Quit from main menu (you can also put this one first and you can skip the return ;))
		}
		#endif



	}

	// Buttons
	// Using an OnClick event, all buttons send their name here if they are pressed.
	// Some panels have so many buttons, that they have their own switch to resolve it.
	// If the button name is not found here, it can be redirected to that panel (script).
	public void OnButton (GameObject aButton) { OnButton(aButton.name);}
	public void OnButton(string aButtonName)
	{
		Debug.Log("[InterfaceScript] OnButton received: " + aButtonName);

		switch(aButtonName)
		{
			case "PlayButton":
			menuPanel.SetActive(false);
			optionsPanel.SetActive(false);
			GameData.skipMissionSelect = (GameData.unlockedMissions == 1);
			if (GameData.skipMissionSelect) EndMenu();
			else
			{
				GoToMenuPanel(missionSelectPanel);
				Fade("FadeWhiteFlash");
				AmbientManager.SetAmbientStyle("Level1");
				CameraManager.UpdateSettings("MenuNoShake", GameObject.Find("MissionSelectCamera_Dummy"));
			}
				Scripts.audioManager.PlaySFX("Interface/Select");
				Scripts.audioManager.PlaySFX("Interface/StartGame");
				Scripts.audioManager.PlaySFX("Weapons/FiftyCaliberFire");
				Scripts.audioManager.PlaySFX("Effects/JetPackIgnition");
			break;

			case "MissionSelectBackButton":
			missionSelectPanel.SetActive(false);
			BackToMenu();
			break;

			case "QuitApplicationButton":
			Scripts.audioManager.PlaySFX("Interface/Select");
			Application.Quit();
			break;

			case "MenuSkipButton": // for very impatient people.
			menuPanel.GetComponent<MenuPanel>().SkipMenuPanelSequence();
			Scripts.audioManager.PlaySFX("Interface/Select");
			break;

			case "BrandingButton":
			OpenURL("BrandingURL");
			Scripts.audioManager.PlaySFX("Interface/Select");
			break;

			case "MoreGamesButton":
			OpenURL("BrandingMoreGamesURL");
			Scripts.audioManager.PlaySFX("Interface/Select");
			break;
			
			case "SplashButton":
			OpenURL("BrandingSplashURL");
			Scripts.audioManager.PlaySFX("Interface/Select");
			break;

			case "PauseButton":
				Scripts.levelScript.PauseGame();
			break;

			case "PauseQuitButton":
			pausePanel.SetActive(false);
			optionsPanel.SetActive(false);
			confirmationPanel.SetActive(true);
			SetSelectedGameObject(GameObject.Find("ConfirmationNoButton"));
			Scripts.audioManager.PlaySFX("Interface/Back");
			Scripts.advertising.forceShowInterstitial = true;
			break;

			case "PauseResumeButton":
			//UnPauseGame();
			Scripts.levelScript.UnPauseGame();
			break;

			case "ConfirmationYesButton":
			confirmationPanel.SetActive(false);
			MissionManager.AbortMission();
			EndGame();
			Scripts.audioManager.PlaySFX("Interface/Quit");
			break;
			
			case "ConfirmationNoButton":
			confirmationPanel.SetActive(false);
			pausePanel.SetActive(true);
			SetSelectedGameObject(GameObject.Find("PauseResumeButton"));
			optionsPanel.SetActive(true);
			Scripts.audioManager.PlaySFX("Interface/Back");
			break;

			case "RetryButton":
			pausePanel.SetActive(false);
			gameOverPanel.SetActive(false);
			resultsPanel.SetActive(false);
			RetryGame();
			Scripts.audioManager.PlaySFX("Interface/Retry");
			break;

			case "ContinueButton":
			ContinueGame();
			Scripts.audioManager.PlaySFX("Interface/Select");
			Scripts.audioManager.PlaySFX("Interface/StartGame");
			break;

			case "QuitButton":
			EndGame();
			Scripts.audioManager.PlaySFX("Interface/Quit");
			break;

			case "EndGameButton":
			GameData.skipToEndGame = true;
			EndGame();
			Scripts.audioManager.PlaySFX("Interface/Select");
			break;

			case "EndGameBackButton":
			GameData.skipToEndGame = false;
			endGamePanel.SetActive(false);
			BackToMenu();
			break;

			case "ShopButton":
			optionsPanel.SetActive(false);
			menuPanel.SetActive(false);
			GoToMenuPanel(shopPanel);
//			shopPanel.SetActive(true);
//			SetSelectedGameObject(GameObject.Find("ShopBackButton"));
			Fade("FadeWhiteFlash");
			AmbientManager.SetAmbientStyle("Level1");
			CameraManager.UpdateSettings("MenuNoShake", GameObject.Find("ShopCamera_Dummy"));
			Scripts.audioManager.PlaySFX("Interface/GadgetBulletTime");
			Scripts.audioManager.PlaySFX("Interface/Select");
			break;
			
			case "ShopBackButton":
			shopPanel.SetActive(false);
			BackToMenu();
			break;

			case "AchievementsButton":
			optionsPanel.SetActive(false);
			menuPanel.SetActive(false);
			GoToMenuPanel(achievementsOverviewPanel);
			Fade("FadeWhiteFlash");
			AmbientManager.SetAmbientStyle("Level6");
			CameraManager.UpdateSettings("MenuNoShake", GameObject.Find("AchievementsCamera_Dummy"));
			Scripts.audioManager.PlaySFX("Interface/GadgetBulletTime");
			Scripts.audioManager.PlaySFX("Interface/Select");
			break;
			
			case "AchievementsBackButton":
			achievementsOverviewPanel.SetActive(false);
			BackToMenu();
			break;

			case "CreditsButton":
			optionsPanel.SetActive(false);
			menuPanel.SetActive(false);
			GoToMenuPanel(creditsPanel);
			Fade("FadeWhiteFlash");
			AmbientManager.SetAmbientStyle("Level3");
			CameraManager.UpdateSettings("MenuNoShake", GameObject.Find("CreditsCamera_Dummy"));
			Scripts.audioManager.PlaySFX("Interface/GadgetBulletTime");
			Scripts.audioManager.PlaySFX("Interface/Select");

			break;
			
			case "CreditsBackButton":
			creditsPanel.SetActive(false);
			BackToMenu();
			break;

			// OptionsPanel buttons. May be moved later on.
			case "SettingsButton":
			optionsPanel.SetActive(false);
			menuPanel.SetActive(false);
			pausePanel.SetActive(false);
			GoToMenuPanel(settingsPanel);
			if (Data.scene == "Menu")
			{
				Fade("FadeWhiteFlash");
				AmbientManager.SetAmbientStyle("Level1");
				CameraManager.UpdateSettings("MenuNoShake", GameObject.Find("SettingsCamera_Dummy"));
				Scripts.audioManager.PlaySFX("Interface/GadgetBulletTime");
			}
			Scripts.audioManager.PlaySFX("Interface/Select");
			break;
			
			case "SettingsBackButton":
			settingsPanel.SetActive(false);
			if (Data.scene == "Menu")
			{
				BackToMenu();
			}
			else
			{
				pausePanel.SetActive(true);
				SetSelectedGameObject(GameObject.Find("PauseResumeButton"));
				optionsPanel.SetActive(true);
				Scripts.audioManager.PlaySFX("Interface/Back");
			}
			break;

			case "HelpButton":
			optionsPanel.SetActive(false);
			menuPanel.SetActive(false);
			pausePanel.SetActive(false);
			GoToMenuPanel(helpPanel);
			if (Data.scene == "Menu")
			{
				Fade("FadeWhiteFlash");
				AmbientManager.SetAmbientStyle("Level1");
				CameraManager.UpdateSettings("MenuNoShake", GameObject.Find("HelpCamera_Dummy"));
				Scripts.audioManager.PlaySFX("Interface/GadgetBulletTime");
			}
			Scripts.audioManager.PlaySFX("Interface/Select");
			break;

			case "HelpBackButton":
			helpPanel.SetActive(false);
			if (Data.scene == "Menu")
			{
				BackToMenu();
			}
			else
			{
				pausePanel.SetActive(true);
				SetSelectedGameObject(GameObject.Find("PauseResumeButton"));
				optionsPanel.SetActive(true);
				Scripts.audioManager.PlaySFX("Interface/Back");
			}
			break;

			case "AudioToggleButton":
			// Sprite for this button is set in the optionspanel script.
			if (!Data.muteAllSound)
			{
				Data.muteAllSound = true;
				Data.sfx = false;
				Scripts.audioManager.MuteMusic(true);
				Data.music = false;
				Scripts.audioManager.MuteMusic(true);
			}
			else
			{
				Data.muteAllSound = false;
				Data.sfx = true;
				Scripts.audioManager.MuteMusic(false);
				Data.music = true;
				Scripts.audioManager.MuteMusic(false);
				Scripts.audioManager.PlaySFX("Interface/Select");
			}
			UserData.Save();
			break;

			case "FullscreenToggleButton":
			// Sprite for this button is set in the optionspanel script.
			Screen.fullScreen = !Screen.fullScreen;
			Scripts.audioManager.PlaySFX("Interface/Select");
			break;

			case "EffectsQualityToggleButton":
			// Sprite for this button is set in the optionspanel script.
			switch (Data.quality)
			{
			case "Fantastic":
				Data.quality = "Fastest";
				break;
			case "Simple":
				Data.quality = "Fantastic";
				break;
			case "Fastest":
				Data.quality = "Simple";
				break;
			}
			Options.SetVisuals();
			UserData.Save();
			Scripts.audioManager.PlaySFX("Interface/Select");
			break;

			case "LeaderboardsButton":
				// Sound effect
				Scripts.audioManager.PlaySFX("Interface/Select");
				// turn off fullscreen if so...
				if (Screen.fullScreen) Screen.fullScreen = false;
				// Setup data
//				uint lbLevel = 1;
//				string lbLevelName = "Highscore";
				// Send data
				// Not calling anything now. Implement your highscore manager here.
			break;

			case "SubmitButton":	
				// Sound effect
				//Scripts.audioManager.PlaySFX("Interface/Select");
				// turn off fullscreen if so...
				//if (Screen.fullScreen) Screen.fullScreen = false;
				// Setup data
//				uint hsLevel = 1;
//				string hsLevelName = "Highscore";
				// Send data.
				// Not sending it anywhere now!!!! Implement your highscore manager here!		
				
				Scripts.audioManager.MuteSFX(true);
				Scripts.audioManager.MuteMusic(true);	
							
				Scripts.advertising.ShowVideo();
				// Turn off the button
				GameObject.Find("SubmitButton").SetActive(false);
				resultsPanel.GetComponent<ResultsPanel>().cash.SetActive(false);
			break;

			// [MOBILE] These are the new buttons for mobile.
			// We're using the new CrossPlatformInputManager so we can manually set Up/Down for an inputmanager button!
			// I found this not to be very easy as you NEED to set it down/up after you did up/down. Making sure the button will 'reset'!
			case "Weapon": CrossPlatformInputManager.SetButtonUp("Fire1"); break;
			case "Explosive": CrossPlatformInputManager.SetButtonUp("Fire2"); break;
			case "PrimaryFireButton": CrossPlatformInputManager.SetButtonUp("Fire1"); break;
			case "SecondaryFireButton": CrossPlatformInputManager.SetButtonUp("Fire2"); break;
			case "JumpButton": CrossPlatformInputManager.SetButtonUp("Jump"); break;
			case "CrouchButton": CrossPlatformInputManager.SetButtonUp("Crouch"); break;
			case "DriftButton": CrossPlatformInputManager.SetButtonUp("Jump"); break;
			case "HammerTimeButton": CrossPlatformInputManager.SetButtonUp("HammerTime"); break;

			// Exceptions (the other way around as we want this to work on up! The other buttons must work when holding them down!)
			case "VehicleEnter": CrossPlatformInputManager.SetButtonDown("Use"); break;
			case "VehicleExit": CrossPlatformInputManager.SetButtonDown("Use"); break;
			case "CycleWeaponButton":CrossPlatformInputManager.SetButtonDown("NextPrimaryWeapon"); break;

			default:
				Debug.Log("[InterfaceScript] OnButton did not recognize it.");
				// Calling this button on other scripts.
				if (settingsPanel.activeSelf) settingsPanel.GetComponent<SettingsPanel>().OnButton(aButtonName);
				if (missionSelectPanel.activeSelf) missionSelectPanel.GetComponent<MissionSelectPanel>().OnButton(aButtonName);
				if (shopPanel.activeSelf) shopPanel.GetComponent<ShopPanel>().OnButton(aButtonName);
				break;
		}
	}

	public void ButtonOnPointerDown(GameObject anObject){
		if (!GameData.mobile) return; // [MOBILE] This prevents running this code in e.g. standalone which returns an error (you don't need those buttons anyway)
		switch(anObject.name){
			case "Weapon": CrossPlatformInputManager.SetButtonDown("Fire1"); break;
			case "Explosive": CrossPlatformInputManager.SetButtonDown("Fire2"); break;
			case "PrimaryFireButton": CrossPlatformInputManager.SetButtonDown("Fire1"); break;
			case "SecondaryFireButton": CrossPlatformInputManager.SetButtonDown("Fire2"); break;
			//case "VehicleEnter": CrossPlatformInputManager.SetButtonDown("Use"); break;
			//case "VehicleExit": CrossPlatformInputManager.SetButtonDown("Use"); break;
			case "JumpButton": CrossPlatformInputManager.SetButtonDown("Jump"); break;
			case "CrouchButton": CrossPlatformInputManager.SetButtonDown("Crouch"); break;
			case "DriftButton": CrossPlatformInputManager.SetButtonDown("Jump"); break;
			case "HammerTimeButton": CrossPlatformInputManager.SetButtonDown("HammerTime"); break;
			//case "CycleWeapon": CrossPlatformInputManager.SetButtonDown("NextPrimaryWeapon"); break;
		}
	}

	public void ButtonOnPointerExit(GameObject anObject){
		if (!GameData.mobile) return; // [MOBILE] This prevents running this code in e.g. standalone which returns an error (you don't need those buttons anyway)
		switch(anObject.name){
			case "Weapon": CrossPlatformInputManager.SetButtonUp("Fire1"); break;
			case "Explosive": CrossPlatformInputManager.SetButtonUp("Fire2"); break;
			case "PrimaryFireButton": CrossPlatformInputManager.SetButtonUp("Fire1"); break;
			case "SecondaryFireButton": CrossPlatformInputManager.SetButtonUp("Fire2"); break;
			case "JumpButton": CrossPlatformInputManager.SetButtonUp("Jump"); break;
			case "CrouchButton": CrossPlatformInputManager.SetButtonUp("Crouch"); break;
			case "DriftButton": CrossPlatformInputManager.SetButtonUp("Jump"); break;
			case "HammerTimeButton": CrossPlatformInputManager.SetButtonUp("HammerTime"); break;
		}
	}

	void LateUpdate(){
		// [MOBILE] Only run this code when mobile version
		bool lateUpdate = GameData.mobile;
		if (lateUpdate){ // reset the input. See exceptions above!
			CrossPlatformInputManager.SetButtonUp("Use");
			CrossPlatformInputManager.SetButtonUp("NextPrimaryWeapon");
		}
	}

	public void ResetDynamicallySetButtons(){
		List<string> buttons = new List<string>(){"Fire1", "Fire2", "Jump", "Crouch", "HammerTime", "NextPrimaryWeapon"};
		foreach(string button in buttons)
			if (CrossPlatformInputManager.GetButton(button)) CrossPlatformInputManager.SetButtonUp(button);
	}


	public void OnRollover (GameObject aButton) { OnRollover(aButton.name);}
	public void OnRollover(string aButtonName){
		Scripts.audioManager.PlaySFX("Interface/RollOver");
	}

	// Navigation functions
	public void StartPreloader()
	{
		Debug.Log("[InterfaceScript] StartPreloader called.");
		Data.preloader = false;
		preloaderPanel.SetActive(true);
	}

	public void StartSplash()
	{
		Debug.Log("[InterfaceScript] StartSplash called.");
		Data.splash = false;
		preloaderPanel.SetActive(false);
		splashPanel.SetActive(true);
		Invoke("StartMenu", 2.5f);
		Scripts.audioManager.PlaySFX("Interface/Submit");
	}

	public void StartMenu()
	{
		Debug.Log("[InterfaceScript] StartMenu called.");
		Data.scene = "Menu";
		AmbientManager.SetAmbientStyle("Level1"); // Level1 being the default for the menu
		preloaderPanel.SetActive(false);
		splashPanel.SetActive(false);
		CursorManager.SetCursor(true); // Show and unlock the cursor
		GameObject.Find("MenuObjects").gameObject.SetActive(true);
		if (GameData.skipToEndGame)  endGamePanel.SetActive(true);
		else menuPanel.SetActive(true); // The script on this panel handles everything from here.
		CameraManager.UpdateSettings("Instant", GameObject.Find("StartMenuCamera_Dummy"));
		Fade("FadeInMenu");
	}

	public void BackToMenu()
	{
		menuPanel.SetActive(true);
		SetSelectedGameObject(menuPanel.GetComponent<MenuPanel>().playButton);
		optionsPanel.SetActive(true);
		Fade("FadeInBack");
		AmbientManager.SetAmbientStyle("Level1");
		CameraManager.UpdateSettings("Instant", GameObject.Find("MenuCamera_Dummy"));
		Scripts.audioManager.PlaySFX("Interface/Back");
	}

	// This causes the activation of a panel to happen after some time.
	// Allowing for the camera animation to finish.
	public void GoToMenuPanel(GameObject panel)
	{
		goToPanel = panel;
		Invoke("OpenMenuPanel", Time.timeScale == 0 ? 0 : 1.5f / Time.timeScale); // In pause, timescale was low and the panels did not open!
	}
	                        
	public void OpenMenuPanel()
	{
		// WARNING This only works if the goToPanel has been set first!
		goToPanel.SetActive(true); 
		string panelName = goToPanel.name.Substring(0, goToPanel.name.IndexOf("Panel"));
		SetSelectedGameObject(GameObject.Find(panelName + "BackButton"));
		Scripts.audioManager.PlaySFX("Interface/Respawn");
	}

	public void EndMenu()
	{
		Debug.Log("[InterfaceScript] EndMenu called.");
		MusicFadeOut();
		Fade("FadeOutMenu");
		missionSelectPanel.SetActive(false);
		menuPanel.SetActive(false);
		optionsPanel.SetActive(false);
		CameraManager.UpdateSettings("MenuNoShake", GameObject.Find("EndMenuCamera_Dummy"));
		Invoke("StartGame", 1.0f);
	}
	
	public void StartGame()
	{
		Debug.Log("[InterfaceScript] StartGame called. Mission: " + GameData.mission.ToString());
		Data.scene = "Level";
		GameObject.Find("MenuObjects").gameObject.SetActive(false);
		Scripts.levelScript.StartLevel();
		AmbientManager.SetAmbientStyle(MissionManager.missionData.ambientStyle);
		Scripts.interfaceScript.Fade("FadeInGame");
		gamePanel.SetActive(true);  // The script on this panel handles everything from here.

	}

	public void EndGame()
	{
		// Quitting from GameOver and Results.
		resultsPanel.SetActive(false);
		gameOverPanel.SetActive(false);
		gamePanel.SetActive(false);
		MusicFadeOut();
		Scripts.interfaceScript.Fade("FadeOutGame");
		Data.scene = "Menu";
		Time.timeScale = 1.0f;
		// Looking for target, since we can be in a vehicle/traffic. Kind of a hack.
		TargetData targetData = TargetManager.GetTargetData(CameraManager.activeCamera);
		CameraManager.UpdateSettings("HammerSky", targetData.gameObject);
		Invoke("LevelQuitGame", 0.9f);
	}

	private void LevelQuitGame(){ Scripts.levelScript.QuitGame();}

	public void RetryGame()
	{
		pausePanel.SetActive(false);
		resultsPanel.SetActive(false);
		gameOverPanel.SetActive(false);
		gamePanel.SetActive(false);
		MusicFadeOut();
		Scripts.interfaceScript.Fade("FadeOutGame");
		Data.scene = "Level";
		Time.timeScale = 1.0f;
		// Looking for target, since we can be in a vehicle/traffic. Kind of a hack.
		TargetData targetData = TargetManager.GetTargetData(CameraManager.activeCamera);
		CameraManager.UpdateSettings("HammerSky", targetData.gameObject);
		Invoke("LevelRetryGame", 0.9f);
	}
	private void LevelRetryGame(){ Scripts.levelScript.RetryGame();}

	public void ContinueGame()
	{
		resultsPanel.SetActive(false);
		Scripts.interfaceScript.Fade("FadeOutGame");
		Invoke("LevelQuitGame", 0.9f);
		Invoke("StartGame", 2.0f);
	}

	public void PauseGame()
	{
		Scripts.audioManager.SetAllMusicVolume(0.1f);
		gamePanel.SetActive(false);
		Scripts.audioManager.PlaySFX("Interface/Pause");
		optionsPanel.SetActive(true);
		pausePanel.SetActive(true);
		SetSelectedGameObject(GameObject.Find("PauseResumeButton"));
		// We need to save the previous camera settings, including the exact target transform.
		previousCameraSetting = CameraManager.activeCameraData.settings;
		Transform transform = CameraManager.GetSetting("target") as Transform;
		previousCameraGameObject = (transform != null) ? transform.gameObject : null; //CameraManager.activeCameraData.target.gameObject;
		CameraManager.UpdateSettings("Pause", previousCameraGameObject);
	}

	public void UnPauseGame()
	{
		Scripts.audioManager.SetAllMusicVolume(1.0f);
		pausePanel.SetActive(false);
		optionsPanel.SetActive(false);
		Scripts.audioManager.PlaySFX("Interface/UnPause");
		gamePanel.SetActive(true);
		// We need to make sure to override the settings found in shared data with the transform's PARENT game object.
		CameraManager.UpdateSettings(previousCameraSetting, previousCameraGameObject);
	}

	public void GameOver()
	{
		gamePanel.SetActive(false);
		// Scripts.audioManager.StopAllSFX(); // So the healthwarning heartbeat continous playing.
		Scripts.audioManager.StopAllMusic();
		Scripts.audioManager.PlaySFX("Interface/DeathJingle");
		Invoke("GameOverInvoke", 2.0f);
	}
	public void GameOverInvoke()
	{
		Debug.Log("[InterfaceScript] GameOver called.");
		Transform transform = CameraManager.GetSetting("target") as Transform;
		GameObject currentTarget = (transform != null) ? transform.gameObject : null; //CameraManager.activeCameraData.target.gameObject;
		string gameOverCam = Scripts.hammer.vehicleData.isInVehicle ? "VehicleGameOver" : "HammerGameOver" ; // Not sure if you'll ever die INSIDE a vehicle.
		CameraManager.UpdateSettings(gameOverCam, currentTarget);
		gamePanel.SetActive(false);
		gameOverPanel.SetActive(true);
	}

	public void Results()
	{
		if (GameData.mobile) ResetDynamicallySetButtons(); // [MOBILE] Force reset buttons
		gamePanel.SetActive(false);
		Scripts.audioManager.StopAllMusic();
		Scripts.audioManager.PlaySFX("Interface/VictoryJingle");
		Invoke("ResultsInvoke", 2.0f);
	}
	public void ResultsInvoke()
	{
		Debug.Log("[InterfaceScript] Results called.");
		Transform transform = CameraManager.GetSetting("target") as Transform;
		GameObject currentTarget = (transform != null) ? transform.gameObject : null; //CameraManager.activeCameraData.target.gameObject;
		string resultsCam = Scripts.hammer.vehicleData.isInVehicle ? "VehicleResults" : "HammerResults" ;
		CameraManager.UpdateSettings(resultsCam, currentTarget);
		resultsPanel.SetActive(true);
	}

	public void DisplayAchievement(int number)
	{
		int achievementNumber = number;
		GameObject achievement = achievementPanel.transform.Find("Achievement").gameObject;
		achievement.SetActive(true);
		achievement.GetComponent<XTweenPosition>().enabled = false;
		achievement.GetComponent<XTweenPosition>().enabled = true;
		achievement.GetComponent<XTweenAlpha>().enabled = false;
		achievement.GetComponent<XTweenAlpha>().enabled = true;
		achievement.transform.Find("Header").GetComponent<Text>().text = XLocalization.Get("Achievement" + achievementNumber.ToString() + "HeaderText");
		achievement.transform.Find("Description").GetComponent<Text>().text = XLocalization.Get("Achievement" + achievementNumber.ToString() + "DescriptionText");
		achievement.transform.Find("Progress").GetComponent<Text>().text = XLocalization.Get("AchievementUnlockedText");
		achievement.transform.Find("Icon").GetComponent<Image>().sprite = AtlasManager.hammer2AchievementsAtlas.Get("Achievement" + number.ToString());
		Scripts.audioManager.PlaySFX("Interface/Achievement");
	}

	public void ActivateCinematicShot(string name)
	{
		// Used in game to introduce things.
		Debug.Log("[InterfaceScript] ActivateCinematicShot received: " + name);
		cinematicInProgress = true;
		if (!name.Contains("Camera_Dummy")) name = name + "Camera_Dummy"; // (DG) So you need to make a proper dummy in CameraDummies!
		gamePanel.SetActive(false);
		overlayPanel.GetComponent<OverlayPanel>().ActivateOverlay("BarsIn");
		previousCameraSetting = CameraManager.activeCameraData.settings;
		CameraManager.UpdateSettings("Cinematic",  GameObject.Find(name));
		Scripts.audioManager.PlaySFX("Interface/GadgetBulletTime");
		Scripts.audioManager.SetAllMusicVolume(0.1f);
		MissionManager.MissionInput(false);
		Data.pausingAllowed = false;
		Time.timeScale = 0.1f;
		Time.fixedDeltaTime = 0.1f * 0.02f;
		Invoke("DeactivatCinematicShot", 0.3f); // Hack: Smaller since we're in slomo!
	}

	public void DeactivatCinematicShot()
	{
		overlayPanel.GetComponent<OverlayPanel>().ActivateOverlay("BarsOut");
		CameraManager.UpdateSettings(previousCameraSetting);  // Hopefully this will take you back to the correct setting
		Scripts.audioManager.PlaySFX("Interface/Unlocked");
		Time.timeScale = 1.0f;
		Time.fixedDeltaTime = 0.02f;
		Scripts.audioManager.SetAllMusicVolume(Data.musicVolume);
		Data.pausingAllowed = true;
		MissionManager.MissionInput(true);
		Invoke("GamePanelSetActive", 1.0f);
		cinematicInProgress = false;
	}

	private void GamePanelSetActive()
	{
		CameraManager.UpdateSetting("lerp", false); 
		gamePanel.SetActive(true);
	}

	public void Fade(string fadeName)
	{
		Debug.Log("[InterfaceScript] Fade received: " + fadeName);
		foreach (Transform t in fadePanel.transform) t.gameObject.SetActive(false);

		GameObject fadeObject = fadePanel.transform.Find(fadeName).gameObject;
		if (fadeObject != null)
		{
		fadePanel.transform.Find(fadeName).gameObject.SetActive(true);
		fadePanel.transform.Find(fadeName).gameObject.GetComponent<XTweenAlpha>().enabled = false;
		fadePanel.transform.Find(fadeName).gameObject.GetComponent<XTweenAlpha>().enabled = true;
		Invoke ("FadeDone", fadePanel.transform.Find(fadeName).GetComponent<XTweenAlpha>().duration);
		}
		else Debug.Log("Fade FadeObject not found!");
	}

	public void FadeDone()
	{
		foreach (Transform t in fadePanel.transform) t.gameObject.SetActive(false);
	}

	public void OpenURL(string url)
	{
		if (!Data.clickLinks) { Debug.Log("[InterfaceScript] OpenURL called. Globals ClickLinks was false!"); return;}
		string myURL;
		// Check if it's an url or a localization key
		if (url.Contains(".")) myURL = url;
		else myURL = XLocalization.Get(url);

		// create an evaluation
		string myEval;
		myEval = "window.open(\"" + myURL + "\", \"_blank\")";
		
		Debug.Log("[InterfaceScript] OpenURL called,  evaluate: " + myEval);
		
		if (Application.platform == RuntimePlatform.WebGLPlayer ) 
		{
			Application.ExternalEval(myEval);
		}
		else
		{
			Application.OpenURL(myURL);
		}
	}

	// This is made so that we don't have to set this up manually on every button.
	public void SetUpButtonsInHierarchy(GameObject go)
	{
		Button[] buttons = go.GetComponentsInChildren<Button>();
		foreach (Button buttonItem in buttons)
		{
			SetUpButton(buttonItem);
		}
	}

	public void SetUpButton(Button button)
	{
		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(() => OnButton(button.gameObject));
	}

	public void CheckForControllers()
	{
		Debug.Log("[InterfaceScript] Detecting joystick names...!");
		string[] joystickNames = Input.GetJoystickNames();
		foreach (string name in joystickNames)
			Debug.Log("[InterfaceScript] Joystick name: " + name);
		
		if (joystickNames.Length == 0) Debug.Log("[InterfaceScript] No joystick names found!");
		else
		{
			Debug.Log("[InterfaceScript] At least one joystick found!");
			Data.controllerUseDetected= true;
		}
	}

	private void SelectAnyButtonGameObject()
	{
		//		Debug.Log ("InterfaceScript SelectAnyButtonGameObject called!");
		Button[] buttons = gameObject.GetComponentsInChildren<Button>();
		int availableButtons = buttons.Length;
		if (availableButtons == 0)
		{
			//			Debug.Log("InterfaceScript SelectAnyButtonGameObject: There are no buttons!");
			return;
		}
		GameObject anyButton = null;
		for (int i = 0; i < availableButtons; i++)
		{
			anyButton = buttons[i].gameObject;
			if (anyButton.name != "BrandingButton") break;
		}
		if (anyButton.name != "BrandingButton") SetSelectedGameObject(anyButton);
		//		else Debug.Log("Could not find anything else but Branding!");
	}
	
	public void SetSelectedGameObject(GameObject go)
	{
		if (Data.controllerUseDetected)
		{
			eventSystem.SetSelectedGameObject(go);
		}
		else return;
	}

	public void Trigger(string triggerName)
	{
		switch (triggerName)
		{
		case "Katie": ActivateCinematicShot(triggerName); break;
		case "BigBoss": ActivateCinematicShot(triggerName); break;
		default:
			Debug.Log("[InterfaceScript] Trigger not recognized, trying to activate cinematic: " + triggerName);
			ActivateCinematicShot(triggerName);
		break;
		}
	}

	// This might not be the greates place on earth for a crappy fade out script, I know...
	// This function is so bad it sometimes still runs when you want to hear music. :(.
	public void MusicFadeOut(){	StartCoroutine(MusicFadeOutSequence());}
	private IEnumerator MusicFadeOutSequence()
	{
		Debug.Log("[InterfaceScript] MusicFadeOutSequence started!");
		float musicVolume;
		musicVolume = Data.Shared["Misc"].d["MusicVolume"].f;
		for(int i = 99; i > 0; i--)
		{
			musicVolume = musicVolume * (i * 0.01f);
			Scripts.audioManager.SetAllMusicVolume(musicVolume);
			yield return new WaitForSeconds(0.025f);
		}
		Debug.Log("[InterfaceScript] MusicFadeOutSequenceout ended!");
	}

}