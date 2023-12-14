using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

/// <summary>
/// MissionManager.
/// <para>Controls and sets everything for each mission.</para>
/// </summary>
public static class MissionManager
{
	public static GameObject missionHolder; // object that will hold the main mission script
	public static Mission missionScript; // main mission script

	public static MissionData missionData; // data reference for current mission 
	public static List<MissionData> missionProgress = new List<MissionData>(); // all missions played/saved, etc.

	public static Dictionary<string, int> targetProgress; // Dictionary with objects destroyed/killed and their score
	public static bool missionInProgress; // is the mission in progress?
	public static List<string> missionSaveList = new List<string>(){"completed", "totalTime", "totalScore", "hiddenPackage"}; // List of variable we'll save

	/// <summary>
	/// Loads the mission progress.
	/// </summary>
	/// <param name="aString">A string.</param>
	public static void LoadMissionProgress(string aString) // This loads a json string and prepares the full list
	{
		// deserialize string
		// convert to missionprogress
		if (aString == "Dynamic") return;
		string dataAsString = aString; 
		dataAsString = dataAsString.Replace(".", ","); // REPLACE POINTS I CAN'T SEEM TO CICRUMVENT STIJNS CODE REGARDING .'s
		Dictionary<string, object> savedMissionProgress = Json.Deserialize(dataAsString) as Dictionary<string, object>;
		Dictionary<string, object> dataHolder;
		foreach(string key in savedMissionProgress.Keys){
			dataHolder = savedMissionProgress[key] as Dictionary<string, object>;
			MissionData missionData = new MissionData();
			missionData = GenericFunctionsScript.ParseDictionaryToClass(Data.Shared["Missions"].d[key].d, missionData) as MissionData;
			missionData.mission = int.Parse(key.Substring(7, key.Length-7));
			missionData.targetsLeft = 0;
			foreach(string var in missionSaveList)
			{
				FieldInfo prop = missionData.GetType().GetField(var);
				// set it
				if (prop.FieldType == typeof(int))    missionData.GetType().GetField(var).SetValue(missionData, Convert.ToInt32(dataHolder[var]));
				if (prop.FieldType == typeof(bool))   missionData.GetType().GetField(var).SetValue(missionData, Convert.ToBoolean(dataHolder[var]));
			}
			missionProgress.Add(missionData);
		}
	}

	/// <summary>
	/// Saves the mission progress.
	/// </summary>
	public static void SaveMissionProgress()
	{
		// make new dictionary, stringify,  store in global
		Dictionary<string, object> savedMissionProgress = new Dictionary<string, object>();
		Dictionary<string, object> dataHolder;
		foreach(MissionData missionData in missionProgress){
			dataHolder = new Dictionary<string, object>();
			foreach(string field in missionSaveList){
				dataHolder[field] = missionData.GetType().GetField(field).GetValue(missionData);
			}
			savedMissionProgress["Mission"+missionData.mission] = dataHolder;
		}

		string dataAsString = Json.Serialize(savedMissionProgress);
		dataAsString = dataAsString.Replace(",", "."); // REPLACE COMMA'S I CAN'T SEEM TO CICRUMVENT STIJNS CODE REGARDING ,'s

		GameData.missionProgress = dataAsString;
		UserData.Save();
	}

	/// <summary>
	/// Stores the mission progress.
	/// <para>NOTE: This is NOT saving, but storing, replacing and updating!</para>
	/// </summary>
	/// <param name="aMissionData">A mission data.</param>
	public static void StoreMissionProgress(MissionData aMissionData){StoreMissionProgress(aMissionData, false);}
	public static void StoreMissionProgress(MissionData aMissionData, bool aHammerDead)
	{
		int index = GetMissionIndex(aMissionData); // Does it exist? Get mision id, replace
		if (index >= 0) // replace, but take a good look
		{
			MissionData current = missionProgress[index]; // Get the already stored one

			// update completed
			if (!current.completed && aMissionData.completed){
				current.completed = aMissionData.completed;
				// These medals can only be rewarded if you finish the mission
				if (!Scripts.medalsManager.IsMedalObtained((int)MedalsManager.Medal.FIRSTBLOOD)) Scripts.medalsManager.UpdateMedal((int)MedalsManager.Medal.FIRSTBLOOD, 1);
				if (!Scripts.medalsManager.IsMedalObtained((int)MedalsManager.Medal.MISSIONMASTER)) Scripts.medalsManager.UpdateMedal((int)MedalsManager.Medal.MISSIONMASTER, 1);
			}

			// we only replace score & time, if they are better AND if the mission is completed!!
			if (aMissionData.completed){
				if (aMissionData.totalScore > current.totalScore) current.totalScore = aMissionData.totalScore;
				if (aMissionData.totalTime < current.totalTime) current.totalTime = aMissionData.totalTime;
			}

			// Here is the exception! As we want the hidden package to be saved always, so people can hunt and quit. This is more user-friendly
			if (!current.hiddenPackage && aMissionData.hiddenPackage) current.hiddenPackage = aMissionData.hiddenPackage;
		}else{
			if (!aMissionData.completed) {
				// We reset his just in case!!
				aMissionData.totalScore = 0;
				aMissionData.totalTime = 0;
			} else {
				// These medals can only be rewarded if you finish the mission
				if (!Scripts.medalsManager.IsMedalObtained((int)MedalsManager.Medal.FIRSTBLOOD)) Scripts.medalsManager.UpdateMedal((int)MedalsManager.Medal.FIRSTBLOOD, 1);
				if (!Scripts.medalsManager.IsMedalObtained((int)MedalsManager.Medal.MISSIONMASTER)) Scripts.medalsManager.UpdateMedal((int)MedalsManager.Medal.MISSIONMASTER, 1);
			}
			// Add it!
			missionProgress.Add(aMissionData);
		}

		// aHammerDead, updateMedal :(
		if (aHammerDead && !aMissionData.completed && !Scripts.medalsManager.IsMedalObtained((int)MedalsManager.Medal.MANDOWN)) Scripts.medalsManager.UpdateMedal((int)MedalsManager.Medal.MANDOWN, 1);
	}

	/// <summary>
	/// Gets the index of the mission from missionData.
	/// <para>This is NOT the same as the actual mission number.</para>
	/// </summary>
	/// <returns>The mission index.</returns>
	/// <param name="aMissionData">A mission data.</param>
	public static int GetMissionIndex(MissionData aMissionData)
	{
		int counter = -1;
		foreach(MissionData missionData in missionProgress){
			counter++;
			if (missionData.mission == aMissionData.mission) return counter;
		}
		return -1;
	}

	/// <summary>
	/// Gets the mission by ID.
	/// </summary>
	/// <returns>The mission by ID.</returns>
	/// <param name="anID">An ID.</param>
	public static MissionData GetMissionByID(int anID)
	{
		foreach(MissionData missionData in missionProgress)
			if (missionData.mission == anID) return missionData;
		return null;
	}

	/// <summary>
	/// Sets up the mission.
	/// </summary>
	/// <param name="aMission">A mission.</param>
	public static void SetupMission(int aMission)
	{
		Debug.Log("[MissionManager] SetupMission("+aMission+")");

		// New mission data and fill it with details from SharedData.txt
		missionData = new MissionData();
		missionData = GenericFunctionsScript.ParseDictionaryToClass(Data.Shared["Missions"].d["Mission"+aMission].d, missionData) as MissionData;
		missionData.mission = aMission; // update mission number
		missionData.targetsLeft = missionData.targetAmount; // set targetsleft (which we will use to countdown) to targetAmount

		// [MOBILE] I did this for the user. Gameplay is much 'slower' on mobile devices. Controls are harder and less precise.
		// You need to 'help' the user and not set the same challenges as on e.g. desktop version. 
		// I didn't want to setup a completely new Missions_ structure in SharedData.txt so I am just multiplying the targettime here...
		if (GameData.mobile) missionData.targetTime *= 2;


		// Missionholder object
		missionHolder = GameObject.Find("Mission");
		if (missionHolder == null) missionHolder = new GameObject("Mission");
		// Add and initialize the mission script
		missionHolder.AddComponent<Mission>().Initialize(missionData, missionProgress);
		missionScript = missionHolder.GetComponent<Mission>();

		ActivateMissionObject(aMission); // Disable missionX gameObjects, not the current mission

		// Does it exist? Have we played it already?
		// If so update mission vars with the stored vars so we properly compare them at the end of the mission
		int index = GetMissionIndex(missionData);
		if (index >= 0){
			MissionData stored = missionProgress[index];
			// update the important vars
			missionData.completed  = stored.completed;
			missionData.totalTime  = stored.totalTime;
			missionData.totalScore = stored.totalScore;
			missionData.hiddenPackage = stored.hiddenPackage;
			// if we already have the hidden package destroy it in game so we can't collect them twice
			if (stored.hiddenPackage){
				missionData.hiddenPackage = true;
				GameObject.DestroyImmediate(GameObject.Find("HiddenPackage")); // This is the spawner.
				GameObject.Destroy(GameObject.Find("PickUpHiddenPackage_Prefab"));
			}		
		}

		targetProgress = new Dictionary<string, int>(); // clear the progress list
		missionInProgress = false; // onhold

		InitializeManagers(); // Managers (very important!)

		BuildUser(); // build player

		ShopItemManager.InitializeShopItems(); // Setup ShopItems. E.g. Parking, outfits, etc.

		BuildMissionContent(missionData.mission); // Build all mission content
	}

	static void ActivateMissionObject(int aMission)
	{
		GameObject child;
		foreach (Transform childTransform in missionHolder.transform){ 
			child = childTransform.gameObject;
			if (child.name == "Mission"+aMission) child.SetActive(true);
			else child.SetActive(false);
		}
	}

	/// <summary>
	/// Builds the content of the mission.
	/// <para>Destructibles, vehicles, units, what not</para>
	/// </summary>
	/// <param name="aMission">A mission.</param>
	public static void BuildMissionContent(int aMission)
	{
		// Setup MAIN Destructibles & buildings
		// And add player units.
		// This only needs to be done once on mission start
		if (!missionData.mainContentBuilt)
		{
			DestructibleManager.AddDestructiblesFromChildren(GameObject.Find("Street"), "Dynamic", "Street"); // Go through all objects parented to Street. Get type from SharedData. If object has "Street" in name exclude, else revert to Default data.
			DestructibleManager.AddDestructiblesFromChildren(GameObject.Find("Buildings"), "Building", "None", "DestructibleBuilding"); // Go through all objects parented to Buildings. All objects have Building data, unless specifically defined in SharedData.
			DestructibleManager.AddDestructiblesFromChildren(GameObject.Find("Misc"), "Dynamic", "None", "Destructible", true); // We do this last!! because we're telling the destrucibles they can be reparent! So if we did this first we could run into issues as the system wants to initialize an existing destructible
			VehicleManager.AddVehiclesFromChildren(GameObject.Find("PlayerUnitsMain"), VehicleManager.CONTROLLER.Empty, false);
			if (GameObject.Find("PlayerUnits") != null)	VehicleManager.AddVehiclesFromChildren(GameObject.Find("PlayerUnits"), VehicleManager.CONTROLLER.Empty, false); // Added this so we COULD move them inside the missions.
			missionData.mainContentBuilt = true;
		}

		// Setup mission specific destructibles, units and traffic
		SetupDestructibles(aMission);
		SetupUnits(aMission);
		SetupTraffic(aMission);
	}

	/// <summary>
	/// Starts the mission.
	/// </summary>
	public static void StartMission()
	{
		Debug.Log("[MissionManager] StartMission()");
		MissionInput(true);
		missionInProgress = true;
	}

	/// <summary>
	/// Completes the mission.
	/// </summary>
	public static void CompleteMission()
	{
		if (!missionInProgress || !GameData.missionEnds) return; // no double endmission stuff, this can happen due to areadamage and timed destruction...
		Debug.Log("[MissionManager] CompleteMission()");	

		// disable (mission)input
		MissionInput(false);
		missionInProgress = false;

		// Save mission
		missionData.completed  = true; // tag as finished
		if (missionData.totalTime == 0) missionData.totalTime = missionData.time;
		else missionData.totalTime  = (missionData.time <= missionData.totalTime) ? missionData.time : missionData.totalTime; // store the time
		missionData.totalScore = (missionData.score >= missionData.totalScore) ? missionData.score : missionData.totalScore; // store the score

		NextMission(); // Next mission // moved this about so your next mission progress is saved properly

		StoreMissionProgress(missionData);
		SaveMissionProgress(); // In this function we call Userdata.Save()!

		ResetManagers(); // Reset managers between missions...

		Scripts.interfaceScript.Results(); // Show results
	}

	/// <summary>
	/// Fails the mission.
	/// </summary>
	public static void FailMission()
	{
		if (!missionInProgress || !GameData.missionEnds) return; // no double endmission stuff, this can happen due to areadamage and timed destruction...
		Debug.Log("[MissionManager] FailMission()");	

		// disable (mission)input
		MissionInput(false);
		missionInProgress = false;

		// Save mission
		StoreMissionProgress(missionData, true);
		SaveMissionProgress();

		// Reset managers ??
		ResetManagers();

		// Show results
		Scripts.interfaceScript.GameOver();
	}

	/// <summary>
	/// Aborts the mission.
	/// </summary>
	public static void AbortMission()
	{
		if (!missionInProgress || !GameData.missionEnds) return; // no double endmission stuff, this can happen due to areadamage and timed destruction...
		Debug.Log("[MissionManager] AbortMission()");	

		// disable (mission)input
		MissionInput(false);
		missionInProgress = false;

		// Should be thought through. You now get a medal for quitting.
		// But you might want to make sure you're hidden package is saved.
		StoreMissionProgress(missionData);
		SaveMissionProgress();
		
		// Reset managers ??
		ResetManagers();
	}

	/// <summary>
	/// Nexts the mission.
	/// </summary>
	public static void NextMission()
	{
		GameData.mission += 1; // count next mission
		if (GameData.mission > GameData.unlockedMissions) GameData.unlockedMissions = GameData.mission;
		if (GameData.mission > GameData.loopMissionsAt)  GameData.mission = 1; // loop
	}

	/// <summary>
	/// Initializes the managers.
	/// </summary>
	public static void InitializeManagers()
	{
		Debug.Log("[MissionManager] InitalizeManagers()");
		PoolManager.Initialize();			// PoolManager (NEW!)
		CharacterManager.Initialize();		// Characters 
		VehicleManager.Initialize();		// Vehicles
		ProjectileManager.Initialize();		// Projectiles (bullets, rockets, etc.)
		PickUpManager.Initialize(); 		// Pickups
		SpawnerManager.Initialize();		// Spawners
		ImpactManager.Initialize();			// Impacts
		ExplosionManager.Initialize();		// Explosions
		DestructibleManager.Initialize();	// Destructibles
		AreaDamageManager.Initialize();		// AreaDamage

		// Not nice
		WeaponManager.Initialize(); // Added this to reset possible weapon alterations.
	}

	/// <summary>
	/// Resets the managers.
	/// </summary>
	public static void ResetManagers()
	{
		// For NOW we only need to reset THESE managers.
		// Keep this in mind
		SpawnerManager.Reset();
		VehicleManager.Reset();
	} 

	/// <summary>
	/// Builds the user - Hammer.
	/// </summary>
	public static void BuildUser()
	{
		Debug.Log("[MissionManager] BuildUser() at location:" + missionData.spawnLocation);
		GameObject hammerSpawnlocation = null;
		GameObject defaultSpawnLocation = GameObject.Find("HammerSpawnDefault");
		GameObject missionSpawnLocation = GameObject.Find("HammerSpawn");
		hammerSpawnlocation =  (missionSpawnLocation != null)? missionSpawnLocation : defaultSpawnLocation;
		CharacterManager.AddCharacter("Hammer", CharacterManager.CONTROLLER.User, hammerSpawnlocation);
		CameraManager.UpdateSettings("Hammer");
	}

	/// <summary>
	/// Setups the units.
	/// <para>Mostly military type vehicles</para>
	/// </summary>
	/// <param name="aMission">A mission.</param>
	public static void SetupUnits(int aMission)
	{
		Debug.Log("[MissionManager] SetupUnits()");
		string name = "Mission"+aMission+"/Units";
		GameObject units = missionHolder.transform.Find(name).gameObject;
		// with a small debug so you can test missions without AI controlled vehicles
		if (GameData.AIController) VehicleManager.AddVehiclesFromChildren(units, VehicleManager.CONTROLLER.AI);
		else VehicleManager.AddVehiclesFromChildren(units, VehicleManager.CONTROLLER.Empty);
	}

	/// <summary>
	/// Setups the traffic.
	/// <para>Mostly non-military type vehicles</para>
	/// </summary>
	/// <param name="aMission">A mission.</param>
	public static void SetupTraffic(int aMission)
	{
		Debug.Log("[MissionManager] SetupTraffic()");
		GameObject mission = GenericFunctionsScript.FindChild(missionHolder, "Mission"+aMission);
		GameObject traffic = GenericFunctionsScript.FindChild(mission, "Traffic");
		if (traffic != null) VehicleManager.AddVehiclesFromChildren(traffic, VehicleManager.CONTROLLER.Empty);		
	}

	/// <summary>
	/// Setups the destructibles.
	/// </summary>
	/// <param name="aMission">A mission.</param>
	public static void SetupDestructibles(int aMission)
	{
		Debug.Log("[MissionManager] SetupDestructibles()");
		string name = "Mission"+aMission+"/Misc"; // (DG) Added this to make some misc only active in some missions.
		GameObject miscs = missionHolder.transform.Find(name).gameObject; //  No good since it errors when there is none. Should not be done differently.
		DestructibleManager.AddDestructiblesFromChildren(miscs, "Dynamic", "None", "Destructible", true);
	}

	/// <summary>
	/// Sets all input for a mission!
	/// </summary>
	/// <param name="aState">If set to <c>true</c> a state.</param>
	public static void MissionInput(bool aState)
	{
		Scripts.hammer.AllowInput(aState); // The function allowinput also resets other controllers and button input and stuff
		CameraManager.AllowInput(aState); 
	 
		CharacterManager.AllowInputEnemies(aState); // 

		// Manager
		VehicleManager.SetVehiclesActive(aState);
		SpawnerManager.SetSpawnersActive(aState);

		// Lock cursor
		CursorManager.SetCursor(!aState); // Show and unlock the cursor (inverse from input!)

		// Pausing enabled. Don't want people to pause during cinematic stuff.
		Data.pausingAllowed = aState;
	}

	/// <summary>
	/// Process targets that are destroyed/killed.
	/// </summary>
	/// <param name="aTarget">A target.</param>
	public static void ProcessTarget(string aTarget) {ProcessTarget(aTarget, 0);}
	public static void ProcessTarget(string aTarget, int aScore)
	{
		// no more processing when mission is done
		if (!missionInProgress) return;
		missionData.score += aScore;
		Scripts.interfaceScript.gamePanelScript.PickUp("Score", aScore); // Adding the little thing that pops up in the interface

		// Medal & gadget exception
		if (aTarget.Contains("Enemy")){
			if (GadgetManager.GadgetOwned("CashForKills")) GameData.cash += aScore;
			Scripts.interfaceScript.gamePanelScript.AddKillStreakKill();  // Add to killstreak. The test should be different I think (tag/layer)
			Scripts.medalsManager.UpdateMedal((int)MedalsManager.Medal.THEBUTCHER, 1);
		}

		// Medal exception
		if (aTarget.Contains("Building")){
			Scripts.medalsManager.UpdateMedal((int)MedalsManager.Medal.DESTROYEROFWORLDS, 1);
		}

		// Medal exception
		if (aTarget == "Tank"){
			Scripts.medalsManager.UpdateMedal((int)MedalsManager.Medal.TANKBUSTER, 1);
		}

		// Medal exception
		if (aTarget.Contains("RedBarrel")){
			Scripts.medalsManager.UpdateMedal((int)MedalsManager.Medal.REDBARRELRAGE, 1);
		}

		// either add 1 to an existing key or add key with 1
		if (targetProgress.ContainsKey(aTarget)) targetProgress[aTarget] += 1;
		else targetProgress[aTarget] = 1;

		// evaluate (with enemy exception)
		if (missionData.target == "Enemy"){
			int amount = 0;
			List<string> enemies = new List<string>(){"EnemySuit", "EnemySwat", "EnemySoldier", "EnemyArmored", "EnemyKatie"};
			foreach(string enemy in enemies)
				if(targetProgress.ContainsKey(enemy)) amount += targetProgress[enemy];
			if (enemies.IndexOf(aTarget) > -1) missionData.targetsLeft -= 1;
			if (amount == missionData.targetAmount) CompleteMission();
		} else if (missionData.target == "Traffic"){
			int amount = 0;
			List<string> traffics = new List<string>(){"TrafficBus", "TrafficSchoolBus", "TrafficSedan", "TrafficDeliveryTruck", "TrafficHatchback", "TrafficVan", "TrafficPolice", "TrafficTaxi", "TrafficSportsCar", "TrafficTruck", "TrafficTruckTrailer", "TrafficTruckGasTrailer"};
			foreach(string traffic in traffics){
				if(targetProgress.ContainsKey(traffic)) amount += targetProgress[traffic];
			}
			if (traffics.IndexOf(aTarget) > -1) missionData.targetsLeft -= 1;
			if (amount == missionData.targetAmount) CompleteMission();
		} else if (missionData.target == "Unit"){
			int amount = 0;
			List<string> unitList = new List<string>(){"Apache", "ArmyTruck", "ArmyTruckCloth", "F22", "Helicopter", "Hind", "Limousine", "MammothTank", "MissileTurret", "SwatVan", "Tank", "Turret", "Van"};
			foreach(string unitElement in unitList)
				if(targetProgress.ContainsKey(unitElement)) amount += targetProgress[unitElement];
			if (unitList.IndexOf(aTarget) > -1) missionData.targetsLeft -= 1;
			if (amount == missionData.targetAmount) CompleteMission();
		}else {
			// calc down (for easier access)
			if (aTarget == missionData.target) missionData.targetsLeft -= 1;
			if (targetProgress.ContainsKey(missionData.target) && targetProgress[missionData.target] == missionData.targetAmount) CompleteMission();
		}
	}
}