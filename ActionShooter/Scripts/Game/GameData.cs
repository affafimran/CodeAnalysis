// Stijn 15/10/2012
// This entire class only consist of static variables for easy access without having to create or get instances
// If you need globals in your game, you put them here (so not in Data as that's an engine script)

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameData
{
	private static Dictionary<string, DicEntry> Globals = null;  // refers to Data.Globals

	// Global variables, these match the variables from Globals.txt
	public static Dictionary<string, DicEntry> medalProgression = null;
	public static int cash = 0;

	// Generics
	public static GameObject currentObjectUnderMouse;

	// Layers
	public static int defaultLayer = 0;
	public static int interfaceLayer = 0;
	public static int skyboxLayer = 0;
	public static int playerLayer = 0;
	public static int destructiblesLayer = 0;

	// Tests
	public static bool testWebGL = false;

	// Debugs
	public static bool buildAI        = true;
	public static bool AIActive       = true;

	public static bool createDestructibles = true;
	public static bool createVehicles = true;

	public static bool soundDebug    = false;
	public static bool skipGetReady   = false;
	public static bool AIController   = true;

	// Invert mouse active & sensitivity
	public static bool invertMouse = false;
	public static float mouseSensitivity = 1.0f;

	// Input disabling
	public static bool disableInput = false;

	// others
	public static int mission = 1;
	public static string missionProgress = "";
	public static bool missionEnds = true;
	public static int unlockedMissions = 1;
	public static int loopMissionsAt = 1;
	public static bool skipMissionSelect = false;

	// Reminder: Not all variables are also in globals! Clean up... TBD
	public static bool skipMenuPanelSequence = false;
	public static bool skipToEndGame = false;

	public static List<string> boughtShopItems = new List<string>();

	public static int highscore = 0;

	public static bool godMode = false;

	public static string cheatText = "";

	public static List<DicEntry> obtainedHiddenPackages = null;

	public static bool mobile = false; // This is our global bool to check if we're running/developping for a mobile version. I find this easier and faster then switching or checking Application.

	// NOTE: user data has not yet been loaded when Init is called, it is loaded when Start is called though
	public static void Init()
	{
		Globals = Data.GetGlobals();

		// Layer
		defaultLayer = LayerMask.NameToLayer("Default");
		interfaceLayer = LayerMask.NameToLayer("Interface");
		skyboxLayer = LayerMask.NameToLayer("Skybox");
		playerLayer = LayerMask.NameToLayer("Player");
		destructiblesLayer = LayerMask.NameToLayer("Destructibles");

		// IMPORTANT: DO NOT SET REFERENCES HERE (SUCH AS THE COMMENTED ONES BELOW)
		//            User data has not been loaded yet here
		//medalProgression = Globals["MedalProgression"].d;

		// do not call CopyFromGlobalsToGameData it is called from Data.Init
	}

	// The start is called when the user data is loaded
	public static void Start()
	{
		// Doing a couple of checks to see if we're mobile
		// We use this var throughout the game to set/check/update mobile specific stuff
		mobile = (Application.isMobilePlatform || Data.platform == "iOS" || Data.platform == "Android"); // Either check the application OR our own platform var


		medalProgression = Globals["MedalProgression"].d;
		Scripts.medalsManager = new MedalsManager();  // also updates GameData.player.progression

		Options.SetVisuals();

		Languages.Init();

		Application.LoadLevel("Loading");
	}

	// Copy entries from SaveList objects from Globals dictionary to Data (yes, a bit annoying..., do not have a better solution for now...)
	// This is so all data can be accessed from the Data class instead of through Data.Globals  (which was fine for me, but understandably confusing for others)
	public static void CopyFromGlobalsToGameData()
	{
		// Game specific variables
		cash = Globals["Cash"].i;

		// Generics
		invertMouse = Globals["InvertMouse"].b;
		mouseSensitivity = Globals["MouseSensitivity"].f;

		// misc
		mission = Globals["Mission"].i;
		missionProgress = Globals["MissionProgress"].s;
		missionEnds = Globals["MissionEnds"].b;
		unlockedMissions = Globals["UnlockedMissions"].i;
		loopMissionsAt = Globals["LoopMissionsAt"].i;
		skipMissionSelect = Globals["SkipMissionSelect"].b;

		foreach (DicEntry tDicEntry in Globals["BoughtShopItems"].l)
			boughtShopItems.Add(tDicEntry.s);

		godMode = Globals["GodMode"].b;
		cheatText = Globals["CheatText"].s;
		highscore = Globals["Highscore"].i;

		// Debug
		buildAI  = Globals["BuildAI"].b;
		AIController = Globals["AIController"].b;

		createDestructibles  = Globals["CreateDestructibles"].b;
		createVehicles       = Globals["CreateVehicles"].b;

		skipGetReady = Globals["SkipGetReady"].b;

		// medalProgression, playerChallengeData do not have to be copied, they are referenced, values are intrinsically coupled
	}

	// Copy entries from Data to Globals dictionary (yes, a bit annoying..., do not have a better solution for now...)
	// This is so all data can be accessed from the Data class instead of through Data.Globals  (which was fine for me, but understandably confusing for others)
	public static void CopyFromGameDataToGlobals()
	{
		// Game specific variables
		Globals["Cash"].i = cash;

		// Generics
		Globals["InvertMouse"].b = invertMouse;
		Globals["MouseSensitivity"].f = mouseSensitivity;

		// Misc
		Globals["Mission"].i = mission;
		Globals["MissionProgress"].s = missionProgress;
		Globals["MissionEnds"].b = missionEnds;
		Globals["UnlockedMissions"].i = unlockedMissions;
		Globals["LoopMissionsAt"].i = loopMissionsAt;
		Globals["SkipMissionSelect"].b = skipMissionSelect;

		Globals["GodMode"].b = godMode;
		Globals["CheatText"].s = cheatText;
		Globals["Highscore"].i = highscore;

		// Debugs
		Globals["BuildAI"].b   = buildAI;
		Globals["AIController"].b = AIController;

		Globals["CreateDestructibles"].b = createDestructibles;
		Globals["CreateVehicles"].b = createVehicles;

		Globals["SkipGetReady"].b = skipGetReady;

		Globals["BoughtShopItems"].l.Clear();
		foreach (string tStr in boughtShopItems)
			Globals["BoughtShopItems"].l.Add(new DicEntry(tStr));

		// medalProgression, playerChallengeData do not have to be copied, they are referenced, values are intrinsically coupled
	}
}
