// ENGINE SCRIPT: AVOID PUTTING GAME SPECIFIC CODE IN HERE
// Stijn 23/07/2012
// This entire class only consist of static variables for easy access without having to create or get instances
// It should only contain engine data such as cheats, debug, sfx, etc. and not game specific data
// The annoying thing is the copying of data from and to globals, I think what we'd like is a reference variable (pointers)

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Data
{
	// Text files accessible through dictionaries
	private static Dictionary<string, DicEntry> Globals = null;  // from now on Globals aren't accessed from here, but simply from variables in this class (this does mean we have to do manual copying :( )
	public static Dictionary<string, DicEntry> Shared = null;

	// Each variable in Globals.txt should occur here as well  ;(
	public static string scene = "Menu";

	public static string versionNumber = "";
	public static bool loadUserData = false;
	public static List<string> saveList = null;

	public static bool sfx = true;
	public static bool music = true;
	public static float sfxVolume = 1.0f;
	public static float musicVolume = 1.0f;

	public static string quality = "Simple";
	public static int fullScreenWidth = 800;
	public static int fullScreenHeight = 450;
	public static int targetFrameRate = -1;  // -1 = render as fast as possible
	public static string basePath = "";
	public static bool clickLinks = true;
	public static string branding = "None";
	public static string platform = "PC";
	public static bool preloader = false;
	public static bool splash = false;
	public static bool debug = false;
	public static bool cheats = false;

	public static bool controllerUseDetected = false; // (DG) Added this to not annoy mouse users.

	// Global variables
	public static bool firstRun = true;

	// Maybe these should be moved to gamedata, if they are used at all.

	public static bool skipToMission = false;
	public static bool skipToShop = false;

	public static bool muteAllSound = false;  // toggled by pressing 'm', see GlobalAudio

	// Track variables
	public static bool pause = false;  // whether game is paused or not, if you want to pause or unpause, call TrackScript.pauseGame / unPauseGame
	public static bool pausingAllowed = true;  // whether pausing the game is allowed
	public static bool retried = false;  // true whenever retry button was clicked, reset after start of track
	public static bool raceInProgress = true;  // should only be set by TrackScript.cs and read by others

	public static void Initialize(string aGlobalsText, string aSharedText)
	{
		Globals = new Dictionary<string, DicEntry>();
		Shared = new Dictionary<string, DicEntry>();

		// Immediately load globals and shared
		TextLoader.LoadText(aGlobalsText, Globals);
		TextLoader.LoadText(aSharedText, Shared);

		// set base path
		if (Globals.ContainsKey("BasePath"))
		{  // a base path is specified use it
			basePath = Globals["BasePath"].s;
		} else
		{
			basePath = Application.absoluteURL;
			int tLastForwardSlashIndex = basePath.LastIndexOf('/');
			basePath = basePath.Substring(0, tLastForwardSlashIndex+1);  // strip off the unity file name, leaving the path (with trailing '/')
		}

		GameData.Init();

		CopyFromGlobalsToData();
	}

	// Copy entries from SaveList objects from Globals dictionary to Data (yes, a bit annoying..., do not have a better solution for now...)
	// This is so all data can be accessed from the Data class instead of through Data.Globals  (which was fine for me, but understandably confusing for others)
	public static void CopyFromGlobalsToData()
	{
		scene = Globals["Scene"].s;

		versionNumber = Globals["VersionNumber"].s;
		loadUserData = Globals["LoadUserData"].b;

		saveList = new List<string>();
		foreach (DicEntry tDicEntry in Globals["SaveList"].l)
			saveList.Add(tDicEntry.s);

		sfx = Globals["SFX"].b;
		music = Globals["Music"].b;
		sfxVolume = Globals["SFXVolume"].f;
		musicVolume = Globals["MusicVolume"].f;
		muteAllSound = !Data.sfx && !Data.music; // Test to make sure this variable is correct as well.
		quality = Globals["Quality"].s;
		fullScreenWidth = Globals["FullScreenWidth"].i;
		fullScreenHeight = Globals["FullScreenHeight"].i;
		targetFrameRate = Globals["TargetFrameRate"].i;
		clickLinks = Globals["ClickLinks"].b;
		branding = Globals["Branding"].s;
		platform = Globals["Platform"].s;
		preloader = Globals["Preloader"].b;
		splash = Globals["Splash"].b;
		debug = Globals["Debug"].b;
		cheats = Globals["Cheats"].b;
	
		GameData.CopyFromGlobalsToGameData();
	}

	// Copy entries from Data to Globals dictionary (yes, a bit annoying..., do not have a better solution for now...)
	// This is so all data can be accessed from the Data class instead of through Data.Globals  (which was fine for me, but understandably confusing for others)
	public static void CopyFromDataToGlobals()
	{
		Globals["Scene"].s = scene;
	
		Globals["VersionNumber"].s = versionNumber;
		Globals["LoadUserData"].b = loadUserData;

		Globals["SaveList"].l.Clear();
		foreach (string tStr in saveList)
			Globals["SaveList"].l.Add(new DicEntry(tStr));

		Globals["SFX"].b = sfx;
		Globals["Music"].b = music;
		Globals["SFXVolume"].f = sfxVolume;
		Globals["MusicVolume"].f = musicVolume;
		Globals["Quality"].s = quality;
		Globals["FullScreenWidth"].i = fullScreenWidth;
		Globals["FullScreenHeight"].i = fullScreenHeight;
		Globals["TargetFrameRate"].i = targetFrameRate;
		Globals["ClickLinks"].b = clickLinks;
		Globals["Branding"].s = branding;
		Globals["Platform"].s = platform;
		Globals["Preloader"].b = preloader;
		Globals["Splash"].b = splash;
		Globals["Debug"].b = debug;
		Globals["Cheats"].b = cheats;
	
		GameData.CopyFromGameDataToGlobals();
	}

	// this function should really only be used in UserData (when saving data)
	public static Dictionary<string, DicEntry> GetGlobals()
	{
		return Globals;
	}
}

