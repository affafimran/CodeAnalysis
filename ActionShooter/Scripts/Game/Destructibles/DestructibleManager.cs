using UnityEngine;
using System.Collections;

/// <summary>
/// Destructible manager.
/// Static class which manages everything related to destructible objects.
/// </summary>
public static class DestructibleManager
{
	public static int destructibleLayer = LayerMask.NameToLayer("Destructibles"); // main destructible layer
	public static int buildingLayer     = LayerMask.NameToLayer("Buildings"); // buildings layer
	public static GameObject debrisHolder; // object where debris will be parented to.


	/// <summary>
	/// Initialize the DestructibleManager
	/// </summary>
	public static void Initialize()
	{
		debrisHolder = GameObject.Find("Debris"); // Main unit object
		if (debrisHolder == null) debrisHolder = new GameObject("Debris"); // Create or assign
	}

	/// <summary>
	/// Get all children from an object.
	/// Try to create a destructible from each child.
	/// </summary>
	/// <param name="aParent">The parent object.</param>
	/// <param name="aType">Force type for destructible (see SharedData.txt for all types). 'Dynamic' will get the one based upon its name.</param>
	/// <param name="anExcludeWildCard">Exclude objects with the wildcard in its name</param>
	/// <param name="aComponent">Force a component. Either Destructible or DestructibleBuilding</param>
	/// <param name="aReparent">Reparent the object?</param>
	public static void AddDestructiblesFromChildren(GameObject aParent) {AddDestructiblesFromChildren(aParent, "Dynamic", "None", "Destructible", false);}
	public static void AddDestructiblesFromChildren(GameObject aParent, string aType) {AddDestructiblesFromChildren(aParent, aType, "None", "Destructible", false);}
	public static void AddDestructiblesFromChildren(GameObject aParent, string aType, string anExcludeWildCard) {AddDestructiblesFromChildren(aParent, aType, anExcludeWildCard, "Destructible", false);}
	public static void AddDestructiblesFromChildren(GameObject aParent, string aType, string anExcludeWildCard, string aComponent) {AddDestructiblesFromChildren(aParent, aType, anExcludeWildCard, aComponent, false);}
	public static void AddDestructiblesFromChildren(GameObject aParent, string aType, string anExcludeWildCard, string aComponent, bool aReparent)
	{
		if (!GameData.createDestructibles) return;

		Transform[] children = aParent.GetComponentsInChildren<Transform>();
		foreach(Transform child in children)
		{
			//if (child == aParent.transform || (child.GetComponent<MeshRenderer>() == null && child.transform.childCount == 0)) continue;
			if (child == aParent.transform || child.GetComponent<MeshRenderer>() == null) continue;
			AddDestructible(child.gameObject, aType, anExcludeWildCard, aComponent, aReparent);
		}
	}

	/// <summary>
	/// Add a destructible.
	/// </summary>
	/// <param name="aDestructible">A destructible.</param>
	public static void AddDestructible(GameObject aDestructible) {AddDestructible(aDestructible, "Dynamic", "None", "Destructible", false);}

	/// <summary>
	/// Add a destructible.
	/// </summary>
	/// <param name="aDestructible">The object.</param>
	/// <param name="aType">Force type for destructible (see SharedData.txt for all types). 'Dynamic' will get the one based upon its name.</param>
	/// <param name="anExcludeWildCard">Exclude objects with the wildcard in its name</param>
	/// <param name="aComponent">Force a component. Either Destructible or DestructibleBuilding</param>
	/// <param name="aReparent">Reparent the object?</param>
	public static void AddDestructible(GameObject aDestructible, string aType, string anExcludeWildCard, string aComponent, bool aReparent)
	{
		if (!GameData.createDestructibles) return; // global override to create destructibles or not. Easy for debugging

		string type = "Default"; // default type 
		string name = aDestructible.name; // name
		string shortName = name.Split('_')[0]; // Without the _Prefab
		
		if (aType == "Building" && !aDestructible.name.Contains("Building")){type = "Default"; aComponent = "Destructible";} // force component to building if type is building or name contains building

		string tempType = shortName;
		if (Data.Shared["Destructibles"].d.ContainsKey(tempType)) type = tempType; // if defined in SharedData.txt - Destructibles ALWAYS use it!
		else if (aType != "Dynamic" && Data.Shared["Destructibles"].d.ContainsKey(aType)) type = aType; // if not "Dynamic" check (and use) aType defined.

		int exclude = name.IndexOf(anExcludeWildCard);
		if (exclude > -1 && type == "Default") return;  // We're excluding objects with the wildcard in its name. E.g. create destructibles from all objects under street, but not the street object itself

		// Prepare data
		DestructibleData destructibleData = new DestructibleData();
		destructibleData.type = type; // store type
		destructibleData = GenericFunctionsScript.ParseDictionaryToClass(Data.Shared["Destructibles"].d[type].d, destructibleData) as DestructibleData; // fill it
		destructibleData.reParent = aReparent; // set reparent

		// Finally add the component and initialize it
		Destructible component = aDestructible.AddComponent(GenericFunctionsScript.GetType(aComponent)) as Destructible;
		component.Initialize(destructibleData);

		// Create an icon for the minimap
		Scripts.map.CreateMapIcon(aDestructible, shortName);
	}
}

