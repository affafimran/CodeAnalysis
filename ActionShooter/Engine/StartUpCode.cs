// ENGINE SCRIPT: AVOID PUTTING GAME SPECIFIC CODE IN HERE
// This script starts up the game, and checks for sitelock

using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class StartUpCode : MonoBehaviour
{
	// Referencing assets to make sure these are loaded in time.
	// Could be handled differently I guess.
	public TextAsset[] localizationTextAssets;
	public Sprite[] brandingSpriteAssets;

	private static bool pRanOnce = false;  // prevent multiple instances
	public TextAsset globalsText;
	public TextAsset sharedText;
	
	private bool pInitialized = false;
	private bool avoidStartUpPause = true;
	
	void Awake()
	{
		Debug.Log("[StartUpCode] Awake() called.");
		
		// Check if this already ran.
		// (PIETER) why do we need this???
		if (pRanOnce)
		{
			Destroy(gameObject);
			return;
		}
		
		pRanOnce = true;
		
		// Don't ever destroy this component
		DontDestroyOnLoad(gameObject);
		
		// Init data with globals and shareddata, very important.
		Data.Initialize(globalsText.text, sharedText.text);
		
		// Check if the site lock should kick in.
		if (SiteLock.IsSiteLocked())
		{
			Debug.Log("[StartUpCode] SiteLock is TRUE. Site lock: " + Application.absoluteURL);
			Application.LoadLevel("SiteLock");
			return;  // do nothing anymore
		}
		
		// Set the required target frame rate. Maybe move to visuals...
		if (Application.isEditor) Application.runInBackground = true;
		Application.targetFrameRate = Data.targetFrameRate;
	}
	
	void Update()
	{
		if (!pInitialized)
		{
			// Check if the required scenes are ready to go.
			if(Application.CanStreamedLevelBeLoaded(0) && Application.CanStreamedLevelBeLoaded(1) && Application.CanStreamedLevelBeLoaded(2)) //  && Application.CanStreamedLevelBeLoaded(3))
			{
				Initialize();
				pInitialized= true;
			}
		}
	}
	
	void Initialize()
	{
		Debug.Log("[StartUpCode] Initialize() called.");
//		Languages.Init(); (DG) Temp disabled here since it's also done in GameData.Start
		
		// If the game was already initialized, stop now.
		if(pInitialized) return;
		
		// Overwrite the variables found in the globals with the ones from the savegame.
		if (Data.loadUserData)
		{
			UserData.Load();
			MissionManager.LoadMissionProgress(GameData.missionProgress);
		}

		// Initializes easy references to some important scripts
		Scripts.Initialize();

		// This takes the game toward the Loading scene.
		// Then the LoaderScript will take over.
		// This will eventually mtake the game into the Game scene. 
		// This can be either Data.Scene = Menu or Data.Scene = Level 
		// Then, the InterfaceScript will take over.
		GameData.Start();
	}
	
	void OnApplicationQuit()
	{}

	void OnApplicationFocus(bool aFocus)
	{
		if (!Application.isEditor){
			if (!Data.pause && Data.scene == "Level" && MissionManager.missionInProgress) Scripts.levelScript.PauseGame(); // Pause game
		}
	}

	void OnApplicationPause(bool aPaused)
	{
		if (avoidStartUpPause) { avoidStartUpPause = false; return; }
		if (!aPaused) Scripts.advertising.ShowInterstitial(); // we're returning from pause
	}
}
