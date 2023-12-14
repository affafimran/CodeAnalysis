using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.ImageEffects;

/// <summary>
/// Camera manager.
/// <para>Static class which manages everything related to a camera.</para>
/// <para>E.g. Add a new one, update its settings, update a single setting, etc.</para>
/// </summary>
public static class CameraManager
{
	public static bool initialized = false; // initialized bool
	public static List<GameObject> cameraList; // list which holds all camera objects. We're storing the GameObjects on purpose instead of a script reference.
	public static GameObject activeCamera; // the active camera for easy access throughout the projects 
	public static CameraScript activeCameraScript;// the active CameraScript for easy access throughout the projects
	public static CameraData activeCameraData; // active CameraData for easy access. E.g. shakeIntensity
	public enum STATES {None, Follow, Mimic, Free} // the possible camera states.

	/// <summary>
	/// Initialize the CameraManager.
	/// You are responsible for initializing and resetting the manager!
	/// </summary>
	public static void Initialize()
	{
		cameraList = new List<GameObject>(); // new list
		initialized = true; // we're initialized
	}

	/// <summary>
	/// Adds a(nother) camera.
	/// The 'Default' settings are used which can be found in SharedData.txt.
	/// </summary>
	/// <returns>The camera as a GameObject.</returns>
	public static GameObject AddCamera()
	{
		// Create/instantiate the object.
		// You can also create a new object, but we have default image effects present our cameras
		// [MOBILE] We will use the same LQ camera as we'd use for the WebGL version!
		GameObject cameraObject;
		if (Application.platform == RuntimePlatform.WebGLPlayer || GameData.mobile) cameraObject = Loader.LoadGameObject("Cameras/DefaultCameraLQ"); // Use lowquality version camera
		else cameraObject = Loader.LoadGameObject("Cameras/DefaultCameraHQ"); // use 'nice' quality version camera

		// Rename the camera based upon amount of cameras in the list
		int id = (cameraList.Count+1);
		cameraObject.name = "Camera"+id;

		// Get the default camera data
		CameraData cameraData = GenericFunctionsScript.ParseDictionaryToClass(Data.Shared["Cameras"].d["Default"].d, new CameraData()) as CameraData;
		cameraData.id = id; // store its id

		// Add the CameraScript and initialize it with the CameraData
		CameraScript cameraScript = cameraObject.AddComponent<CameraScript>();
		cameraScript.Initialize(cameraData);	

		// If this is the first camera we create so we'll make this the 'active' one
		if (cameraList.Count == 0){ 
			activeCamera = cameraObject;
			activeCameraScript = cameraScript;
			activeCameraData = cameraData;

		} else cameraObject.SetActive(false); // disable other cameras per default

		// add to list
		cameraList.Add(cameraObject);

		// Set postprocessing/image effects on the current camera, only when quality is Fantastic
		SetPostProcessingEffects(Data.quality == "Fantastic");
		SetCullingDistances(Data.quality);

		// return the object for whatever use you'd see
		return cameraObject;
	}

	/// <summary>
	/// <para>Updates the camera settings.</para>
	/// <para>Settings can be found in SharedData.txt under Cameras_.</para>
	/// </summary>
	/// <param name="aSetting">A setting.</param>
	public static void UpdateSettings(string aSetting) {UpdateSettings(activeCamera, aSetting, null);}
	public static void UpdateSettings(string aSetting, GameObject aTarget) {UpdateSettings(activeCamera, aSetting, aTarget);}
	public static void UpdateSettings(GameObject aCamera, string aSetting, GameObject aTarget)
	{
		// Check if set actually exists
		if (!Data.Shared["Cameras"].d.ContainsKey(aSetting)){Debug.LogWarning("[CameraManager] Set does not exist. Please check SharedData.txt");return;}

		// Update camerData
		CameraScript cameraScript = activeCameraScript;
		if (aCamera != activeCamera) cameraScript = aCamera.GetComponent<CameraScript>();
		cameraScript.cameraData = GenericFunctionsScript.ParseDictionaryToClass(Data.Shared["Cameras"].d[aSetting].d, cameraScript.cameraData) as CameraData;
		cameraScript.cameraData.settings = aSetting; // Store setting string for reference
		cameraScript.InitializeSettings(); // Initialize (new) setting

		// Update the target of the camera (E.g. what should we follow or use as an offset)
		// Since this is most used we're able to set this here as well.

		// If we have a target store its name and set the transform to be used
		if (aTarget != null){		
			UpdateSetting("targetName", aTarget.name); // here we have an override
			UpdateSetting("target", aTarget.transform, cameraScript.gameObject); // here we have an override
		}else{
			// if there is no object we search for an object defined in 'targetName', and try to update that when it is != 'None'. Please note there is no additional error checking here.
			// So make sure the object you want to use exists or add another check here.
			string targetName = (string)GetSetting("targetName");
			if (targetName != "None") UpdateSetting("target", GameObject.Find(targetName).transform); // here we get the target as a string from the settings
		}
	}

	/// <summary>
	/// Updates a single camera setting.
	/// <para>This function sets a specific variable in an available class reference. It doesn't matter where it is located. Where going over all of them.</para>
	/// </summary>
	/// <param name="aSetting">A setting.</param>
	/// <param name="aValue">A value.</param>
	public static void UpdateSetting(string aSetting, object aValue) {UpdateSetting(aSetting, aValue, activeCamera);}
	public static void UpdateSetting(string aSetting, object aValue, GameObject aCamera) 
	{
		// Store references to be used
		CameraScript cameraScript = activeCameraScript;
		CameraData cameraData = activeCameraData;
		if (aCamera != activeCamera){cameraScript = aCamera.GetComponent<CameraScript>(); cameraData = cameraScript.cameraData;}

		// Check if the setting you want to update is in CameraData, if so set it and we're done here.
		if (cameraData.GetType().GetField(aSetting) != null) {
			cameraData.GetType().GetField(aSetting).SetValue(cameraData, aValue);
			return;
		}

		// Check (and set) the variable in any of the defined settings references.
		switch(activeCameraData.state){
			case STATES.Follow:
				CameraSettingsFollow follow = cameraScript.follow;
				if (follow.GetType().GetField(aSetting) != null) follow.GetType().GetField(aSetting).SetValue(follow, aValue);
				break;
			case STATES.Mimic: break;
			case STATES.Free: break;
		}
	}

	/// <summary>
	/// <para>Get a specific camera setting value by string.</para>
	/// <para>Make sure you know what your doing and cast the returned object to the correct type.</para>
	/// </summary>
	/// <returns>The setting.</returns>
	/// <param name="aSetting">A setting.</param>
	public static object GetSetting(string aSetting) {return GetSetting(aSetting, activeCamera);}
	public static object GetSetting(string aSetting, GameObject aCamera) 
	{
		// Store references to be used
		CameraScript cameraScript = activeCameraScript;
		CameraData cameraData = activeCameraData;
		if (aCamera != activeCamera){cameraScript = aCamera.GetComponent<CameraScript>(); cameraData = cameraScript.cameraData;}
		
		// check if the setting value you want is in CameraData, if so return it.
		if (cameraData.GetType().GetField(aSetting) != null) {
			return cameraData.GetType().GetField(aSetting).GetValue(cameraData);
		}
		
		// check if the setting value you want is in either of the settings references
		switch(cameraData.state){
			case STATES.Follow:
				CameraSettingsFollow follow = cameraScript.follow;
				if (follow.GetType().GetField(aSetting) != null){
					return follow.GetType().GetField(aSetting).GetValue(follow);
				}
				break;
			case STATES.Mimic: break;
			case STATES.Free: break;
		}

		return null;
	}

	/// <summary>
	/// Allow input for the camera.
	/// <para>For now this is only used if your camera uses mouseInput.</para>
	/// </summary>
	/// <param name="aState">If set to <c>true</c> a state.</param>
	public static void AllowInput(bool aState) {AllowInput(activeCamera, aState);}
	public static void AllowInput(GameObject aCamera, bool aState)
	{
		CameraScript cameraScript = aCamera.GetComponent<CameraScript>();
		cameraScript.cameraData.allowInput = aState;
		cameraScript.Reset();
	}

	/// <summary>
	/// Sets any post effects on or off.
	/// </summary>
	/// <param name="aState">If set to <c>true</c> a state.</param>
	public static void SetPostProcessingEffects(bool aState) {SetPostProcessingEffects(activeCamera, aState);}
	public static void SetPostProcessingEffects(GameObject aCamera, bool aState)
	{
		// check if we're allowed to do something
		if (!initialized || cameraList.Count == 0) return;

		// This pretty much sets all postEffects instantly
		PostEffectsBase[] postEffects = aCamera.GetComponents<PostEffectsBase>();
		foreach(PostEffectsBase postEffect in postEffects)
			postEffect.enabled = aState;

		// EXCEPT SSAO (Unity)
		if (aCamera.GetComponent<ScreenSpaceAmbientOcclusion>() != null)	aCamera.GetComponent<ScreenSpaceAmbientOcclusion>().enabled = aState;

	}

	/// <summary>
	/// Sets the culling distances of the camera.
	/// <para>NOTE: This is HARDCODED for this specific game (Hammer 2).</para>
	/// </summary>
	/// <param name="aSetting">A setting.</param>
	public static void SetCullingDistances(string aQualitySetting){SetCullingDistances(activeCamera, aQualitySetting);}
	public static void SetCullingDistances(GameObject aCamera, string aQualitySetting)
	{
		/// Check if we're allowed to do something
		if (!initialized || cameraList.Count == 0) return;

		// [HARDCODED] Hammer 2 - WebGL
		// [MOBILE] Force the quality settings to 'Fastest' for the mobile version as well.
		if (Application.platform == RuntimePlatform.WebGLPlayer || GameData.mobile) aQualitySetting = "Fastest";

		// Get the camera component
		Camera component = aCamera.GetComponent<Camera>();
		component.layerCullSpherical = true;
		float[] distances = new float[32];

		Debug.Log("[CameraManager] SetCullingDistances. Quality = " + aQualitySetting);


		// Set based upon qualitysetting
		switch(aQualitySetting){		
			case "Fastest":
				distances[11] = 150f;	// enemies
				distances[12] = 500f;	// projectiles
				distances[13] =   0f;	// buildings
				distances[14] = 200f;	// destructibles
				distances[15] = 275f;	// vehicles
				distances[16] =  75f;	// pickups
				break;

			case "Simple":
				distances[11] = 300f;	// enemies
				distances[12] = 750f;	// projectiles
				distances[13] =   0f;	// buildings
				distances[14] = 500f;	// destructibles
				distances[15] = 700f;	// vehicles
				distances[16] = 150f;	// pickups
				break;

			case "Fantastic":
				distances[11] = 0f;		// enemies
				distances[12] = 0f;		// projectiles
				distances[13] = 0f;		// buildings
				distances[14] = 0f;		// destructibles
				distances[15] = 0f;		// vehicles
				distances[16] = 0f;		// pickups
				break;
		}

		// set the distances
		component.layerCullDistances = distances;
	}

	/// <summary>
	/// Reset the CameraManager
	/// </summary>
	public static void Reset()
	{
		// delete all cameras
		foreach(GameObject camera in cameraList)
			UnityEngine.Object.Destroy(camera);

		// clear the camera list
		cameraList.Clear();
	}


}
