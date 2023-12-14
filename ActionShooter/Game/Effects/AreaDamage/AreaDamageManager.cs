using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// AreaDamageManager.
/// </summary>
public static class AreaDamageManager
{
	public static int areaDamageCounter = 0; // amount of AreaDamages created
	public static GameObject areaDamageHolder; // object that holds the AreaDamages

	public static bool areaDamageDebug = false; // set to true if you want to visualize the AreaDamages

	/// <summary>
	/// Initialize the AreaDamageManager
	/// </summary>
	public static void Initialize()
	{
		GameObject tEffects = GameObject.Find("Effects"); //find effects object
		if (tEffects == null) areaDamageHolder = new GameObject("Effects"); // create one 
		else areaDamageHolder = tEffects; // store it
	}

	/// <summary>
	/// Add an AreaDamage
	/// </summary>
	/// <param name="aSet">A set.</param>
	/// <param name="aPosition">A position.</param>
	public static void AddAreaDamage(string aSet, Vector3 aPosition) { AddAreaDamage(aSet, aPosition, null); }
	public static void AddAreaDamage(string aSet, Vector3 aPosition, GameObject aSender)
	{
		++areaDamageCounter; // count up (for whatever reason)

		GameObject areaDamage = new GameObject("AreaDamage"+areaDamageCounter); // new object

		// Create and store AreaDamageData
		AreaDamageData areaDamageData = new AreaDamageData();
		areaDamageData = GenericFunctionsScript.ParseDictionaryToClass(Data.Shared["AreaDamage"].d[aSet].d, areaDamageData) as AreaDamageData;
		areaDamageData.position = aPosition;
		areaDamageData.sourceDamage = areaDamageData.damage;
		areaDamageData.radius = areaDamageData.start;
		areaDamageData.size   = areaDamageData.end - areaDamageData.start;
		areaDamageData.sourceLifeTime = areaDamageData.lifeTime;
	
		areaDamage.AddComponent<AreaDamage>().Initialize(areaDamageData, aSender); // Add component and initialize

		areaDamage.transform.parent = areaDamageHolder.transform; // parent to holder
	}


	/// <summary>
	/// This function deals the damage
	/// </summary>
	/// <param name="aProjectileData">A projectile data.</param>
	/// <param name="aHitData">A hit data.</param>
	public static void AreaDamage(ProjectileData aProjectileData, HitData aHitData){
		// Get object check for hit
		string layer = LayerMask.LayerToName(aHitData.gameObject.layer);
		switch(layer){
			case "Hammer":
			case "Enemies":
				Character character = aHitData.gameObject.GetComponent<Character>();
				if (character != null) character.Damage(aProjectileData, aHitData); 
				break;

			case "Buildings":
			case "Destructibles":
				Destructible destructible = aHitData.gameObject.GetComponent<Destructible>();
				if (destructible == null) return;
				destructible.Damage(aProjectileData, aHitData);
				break;

			case "Vehicles":
				Vehicle vehicle = aHitData.gameObject.GetComponent<Vehicle>();
				// Vehicles can have multiple objects. We look for the object that has the component
				// This should be better... 
				GameObject obj = aHitData.gameObject;
				while (vehicle == null)
				{
					if (obj == aProjectileData.sender) return; // preventing units from damaging their self
					if (obj.GetComponent<Vehicle>() != null) vehicle = obj.GetComponent<Vehicle>();
					if (obj.transform.parent == null) return;
					obj = obj.transform.parent.gameObject;
				}
				vehicle.Damage(aProjectileData, aHitData);
				break;

			case "Default":
				break;
				
			default:
				break;
		}
	}
}

