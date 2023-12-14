using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Map : MonoBehaviour {

	/// <summary>
	/// Map class manages the world Map and it's related objects for use as a minimap.
	/// Important: Map class handles the 3D stuff. Minimap class handles the 2D stuff.
	/// This scripts should be attached to a parent object called Map (this gameobject).
	/// A MapCamera renders the MapPlane and the MapIcons (as long as they are on a layer called Map).
	/// See the Minimap class for more details.
	/// </summary>

	public int mapLayer; // Make sure there is a layer called Map!

	public Transform mapTargetTransform; // The focus or center of the map: The player. This is set below and cannot be changed externally.

	private Transform mapPlaneTransform; // A representation of the world as a texture on a plane (not realtime rendered).

	private Transform mapCameraTransform; // The camera pointing at this plane using Map layer as culling mask.
	private Camera mapCamera; // The actual camera component;

	[HideInInspector]
	public float mapCameraSize; // Field of view of the camera. Also important for minimap viewradius. Public but forced below in Update.

	private Transform mapIconsTransform; // A parent object to hold the MapIcons moving on top of the plane.

	private GameObject mapIconPrefab; // A plane with a material and texture.

	private float mapIconDepth; // Counting how many we have created and adjusting the depth accordingly.

	public Minimap minimap; // Script attached to the Minimap UI Element found on the GamePanel.
	//IMPORTANT: This is assigned in the Editor at this moment!!!

	// Temp variable for optimization.
	private string mapIconName;
	private string missionTarget;

	void Awake ()
	{
		// As you can see below, a big problem was that stuff might not be not available when this thing becomes active.
		// It needs to become active a.s.a.p. though, since some objects might call a CreateMapIcon very early on.
		// Furthermore, at the same time CreateMinimapIcon is also called, so Minimap should be ready to go as well :(.

		Scripts.map = this;

		mapLayer = LayerMask.NameToLayer("Map");

		// set all chldiren and myself to the correct layer
		gameObject.SetLayerRecursively(mapLayer);

		if (Scripts.hammer !=null)
		{
			if (Scripts.hammer.vehicleData.isInVehicle) mapTargetTransform = Scripts.hammer.vehicleData.vehicle.transform;
			else mapTargetTransform = Scripts.hammer.transform;
		}
		else Debug.Log("[Map] Could not find Hammer in Scripts!");

		mapPlaneTransform = transform.Find("MapPlane");
		if (mapPlaneTransform == null) Debug.Log("[Map] Plane not found!");

		mapCameraTransform = transform.Find("MapCamera");
		if (mapCameraTransform == null) Debug.Log("[Map] CameraTransform not found!");

		mapCamera = mapCameraTransform.GetComponent<Camera>();
		if (mapCamera == null) Debug.Log("[Map] Camera component not found!");
		mapCamera.cullingMask = 1 << mapLayer;

		mapIconsTransform = transform.Find("MapIcons");
		if (mapIconsTransform == null) Debug.Log("[Map] Icons not found!");

		mapIconPrefab = Resources.Load("Prefabs/2D/MapIconTarget_Prefab") as GameObject;
		if (mapIconPrefab == null) Debug.LogWarning("[Map] Could not load MapIconDefault_Prefab!");

		mapIconDepth = 0;

//		minimap = Scripts.interfaceScript.gamePanelScript.minimapScript; // Not working or not available at this moment. Annoying.
		if (minimap == null) Debug.Log("[Map] Minimap not found!");

//		foreach (Transform t in transform) t.gameObject.SetActive(false);
	}


	void Update ()
	{
		if (Scripts.hammer !=null)
		{
			if (Scripts.hammer.vehicleData.isInVehicle)
			{
				mapCameraSize = 140.0f;
				mapTargetTransform = Scripts.hammer.vehicleData.vehicle.transform;
			}
			else
			{
				mapCameraSize = 70.0f;
				mapTargetTransform = Scripts.hammer.transform;
			}
		}
//		else Debug.Log("[Map] Could not find Hammer in Scripts!");

		if (mapTargetTransform != null)
		{
			UpdateMapCamera();
		}
	}

	private void UpdateMapCamera()
	{
		// Set the camera size /fov
		mapCamera.orthographicSize = mapCameraSize;

		// Move the camera alond with the target.
		mapCameraTransform.localPosition = new Vector3 (mapTargetTransform.localPosition.x, 0, mapTargetTransform.localPosition.z);
		
		// Rotate the camera according to the rotation of the target. Could be disabled if you want I think.
		mapCameraTransform.localEulerAngles = new Vector3(90.0f, mapTargetTransform.localEulerAngles.y, 0.0f);
	}

	public void CreateMapIcon(GameObject go){ CreateMapIcon(go, "None"); }
	public void CreateMapIcon(GameObject go, string type)
	{
		//Debug.Log("[Map] CreateMapIcon received: " + go.name + ", of type: " + type + ". Type is: " + type);
		// This method is called for each object that is being processed, even the ones that turn out to not need a map icon.
		// The testing for the icon is done here, and not before calling this method to keep the testing in one place.
		// So, rather than doing tests on each object when it's processed elsewhere, we do it here for each one.
		// Primarily by checking the MapIcons for a ContainsKey. Not sure how fast that is.
		// Maybe we need to change this later on.

		mapIconName = type;

		// Determine if this is something we want to build a map icon for!
		// Exception here: If the type is somehow related to the target of the current mission, we need to catch this.
		missionTarget = MissionManager.missionData.target; // Done here to make sure it's available.

		if (type == missionTarget) mapIconName = "Target";

		if (missionTarget == "Enemy" && type.Contains("Enemy")) mapIconName = "Target";
		if (missionTarget == "Building" && type.Contains("Building")) mapIconName = "Target";

		if (missionTarget == "Traffic" && type.Contains("Traffic") && !type.Contains("Light")) mapIconName = "Target"; // (DG) Great exception for the traffic lights! Winning!
		if (missionTarget == "Unit" && go.GetComponent<Vehicle>() != null) mapIconName = "Target";

		if (type.Contains("Safe"))
		{
			if (ShopItemManager.IsBought("ShopItem8")) mapIconName = "Safe";
		}

		if (type.Contains("BeverageMachine") && !ShopItemManager.IsBought("ShopItem11")) return;
		if (type.Contains("ColdOne") && !ShopItemManager.IsBought("ShopItem11")) return;

		// Get properties of the map icon from shared data here!
		// Preferably, we're not looking up stuff if we know it's not there!!!
		Dictionary<string, DicEntry> mapIcon;
		if (!Data.Shared["MapIcons"].d.ContainsKey(mapIconName))
		{
//			Debug.LogWarning("[Map] There is no MapIcon for this type : " + mapIconName);
			return;
		} else mapIcon = Data.Shared["MapIcons"].d[mapIconName].d;

		mapIconPrefab = Resources.Load("Prefabs/2D/" + mapIcon["prefab"].s + "_Prefab") as GameObject;
		if (mapIconPrefab == null) Debug.LogWarning("[Map] Loading " + "BundleAssets/Shared/Prefabs/2D/" + mapIcon["prefab"].s + "_Prefab failed!!!");

		Transform parentTransform = go.transform;
		GameObject clone;
		Transform cloneTransform;
		MapIcon mapIconScript;

		mapIconDepth++;
		if (mapIconDepth == 100.0f) mapIconDepth = 1.0f; // Making sure they don't stack up infinitely high if you have a continous long stream of icons.

		clone = Instantiate(mapIconPrefab) as GameObject;
		clone.layer = mapLayer;
		cloneTransform = clone.GetComponent<Transform>();

		cloneTransform.SetParent(mapIconsTransform, false); // (DG) Parenting so we can easily do things to all of them.

		mapIconScript = clone.AddComponent<MapIcon>(); // (DG) Now each icon has it's own script to handle position and destruction.
		mapIconScript.itemTransform = parentTransform;

		cloneTransform.localPosition = new Vector3 (0.0f, mapIconDepth, 0.0f); // (DG) giving an offset trying to avoid z-fighting.

		cloneTransform.localScale = new Vector3 (mapIcon["scale"].f,1.0f,mapIcon["scale"].f);

		clone.name = type + "MapIcon";

		// Passing on the mapIconName we determined instead of the type that was passed on to avoid getting shared data stuff again!
		if (mapIcon["minimapIcon"].b) minimap.CreateMinimapIcon(go, mapIconName);

	}
}