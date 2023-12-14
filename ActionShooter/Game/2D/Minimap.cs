using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Minimap : MonoBehaviour {
	/// <summary>
	/// Minimap class manages the visual representation of the minimap.
	/// The minimap view is rendered by the MapCamera onto the a Minimap_RenderTexture.
	/// MinimapView UI element us the MinimapRender_Material that has this texture specified in a RawImage component.
	/// A shader on the material makes it appear round.
	/// This script also manages 2D MinimapIcons (not to be confused with MapIcons) that can hover on the edges of the minimap.
	/// To be clear: the actual icons you see INSIDE the view are actual 3d planes on the Map below and are called MapIcons.
	/// </summary>

	public Map map; // For now, it's assigned in the Editor.
	[HideInInspector]
	public float minimapViewRadius; // How far in 3d meters is a 3D MapIcon out of view and should the 2D MinimapIcon appear? Also see map.mapCameraSize TBD.
	public Transform minimapIconsTransform; // The object that all minimapicons will be parented to.
	private GameObject minimapIconPrefab; // The icon we're using.

	public void Awake () 
	{
		// As with the map system, the big problem is that stuff is not initialized in time.
		// So CreateMinimapIcon could in fact be called BEFORE this object or script is Awake and is ready.
		// This is why for the sake of simplicity some variables need to be set in the Editor.

		if (map == null) Debug.LogError("[Minimap] Map script not assigned!");

		if (minimapIconsTransform == null) Debug.LogError("[Minimap] MinimapIconsTransform script not assigned!");
	}

	void Update ()
	{
		minimapViewRadius = map.mapCameraSize;
	}

	public void CreateMinimapIcon (GameObject go, string type)
	{
		//Debug.Log("[Minimap] CreateMinimapIcon received: " + go.name + ", of type: " + type);

		Transform parentTransform = go.transform;
		
		GameObject clone;
		Transform cloneTransform;
		Image cloneImage;

		MinimapIcon minimapIconScript = null;

		if (minimapIconPrefab == null)
		{
			minimapIconPrefab = Resources.Load("Prefabs/2D/MinimapIcon_Prefab") as GameObject;
			if (minimapIconPrefab == null) Debug.Log("[Minimap] MinimapIcon_Prefab not loaded!");
		}

		clone = Instantiate(minimapIconPrefab) as GameObject;

		cloneTransform = clone.GetComponent<Transform>();

		cloneTransform.SetParent(minimapIconsTransform, false); // Parenting so we can easily do things to all of them.
		
		minimapIconScript = clone.AddComponent<MinimapIcon>(); // Now each icon has it's own script to handle position and destruction.
		minimapIconScript.minimap = this;
		minimapIconScript.mapTargetTransform = map.mapTargetTransform;
		minimapIconScript.itemTransform = parentTransform;

		cloneImage = cloneTransform.GetComponent<Image>();
		cloneImage.sprite = AtlasManager.hammer2MinimapIconsAtlas.Get("MinimapIcon" + type); // Getting it straight from the AtlasManager!!!
		clone.name = type + "MinimapIcon";
	}
}
