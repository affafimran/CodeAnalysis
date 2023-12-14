using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// ImpactManager.
/// <para>NOTE: This only handles the audio- and visual representation. You should take care of actual damage<./para>
/// </summary>
public static class ImpactManager
{	
	public static int impactCounter = 0; // amount of impacts created
	public static GameObject impactHolder; // object to parent impacts to

	/// <summary>
	/// Initialize ImpactManager
	/// </summary>
	public static void Initialize()
	{
		GameObject tImpactHolder = GameObject.Find("Effects"); // Does it already exists? Note: We're looking for 'Effects'. Don't want holder objects for ALL types of effects.
		if (tImpactHolder == null) impactHolder = new GameObject("Effects"); // No, make a new one.
		else impactHolder = tImpactHolder; // Yes, store it.

		// Pooling (NEW!)
		// PIETER. I have added the most common projectile types!
		PoolManager.CreatePool("Effects/", "Ricochet", 15);
		PoolManager.CreatePool("Effects/", "BloodSplatsLQ", 10);
		PoolManager.CreatePool("Effects/", "VehicleCollision", 3);

	}

	/// <summary>
	/// Add an impact.
	/// </summary>
	/// <returns>The impact.</returns>
	/// <param name="anImpact">An impact.</param>
	/// <param name="aHitData">A hit data.</param>
	public static GameObject AddImpact(string anImpact, HitData aHitData) {return AddImpact(anImpact, aHitData, true);}
	public static GameObject AddImpact(string anImpact, HitData aHitData, bool aDelete)
	{
		++impactCounter; // count up (for whatever reason)

		// Get data!
		// We're only using this data for references, maybe in the future we can send or need to send this along.
		ImpactData impactData = GenericFunctionsScript.ParseDictionaryToClass(Data.Shared["Impacts"].d[anImpact].d, new ImpactData()) as ImpactData;

		// Create object or return
		if (impactData.prefab == "None") return null;
		GameObject impact;
		if(PoolManager.DoesPoolExist(impactData.prefab)) impact = PoolManager.GetObjectFromPool(impactData.prefab);
		else impact	= Loader.LoadGameObject("Effects/" + impactData.prefab + "_Prefab");
		if (impact == null) return null;
		impact.transform.position = aHitData.position;

		// Since the introduction of the poolmanager you have to handle this differently.
		// These prefabs don't have any scripts to destroy them, we time it with the default Destroy() function.
		// We've introduced a new Component which does better timing and destroys/returns to a pool
		if (aDelete) impact.AddComponent<DestroyOnParticlesDone>().Initialize(impactData.prefab);

		// Sound effect
		if (impactData.sound != "None"){
			string sfx = impactData.sound;
			if (sfx.IndexOf("_") != -1){ // pick random sfx if _X in name where x is an int
				string[] sfxArray = sfx.Split('_');
				sfx = sfxArray[0];
				sfx = sfx + Random.Range(1, int.Parse(sfxArray[1]));				
			}
			Scripts.audioManager.PlaySFX3D("Effects/"+sfx, impact, "Impact"); // 3d sound, link to actual effect
		}

		// parent
		impact.transform.parent = impactHolder.transform;

		// return the object (as a GameObject)
		return impact;
	}

	/// <summary>
	/// Blast a rigidbody away.
	/// <para>This is not correct, but it looks fine.</para>
	/// </summary>
	/// <param name="aRigidBody">A rigid body.</param>
	/// <param name="aDirection">A direction.</param>
	/// <param name="aStrength">A strength.</param>
	public static void ImpactForce(Rigidbody aRigidBody, Vector3 aDirection, float aStrength)
	{
		// Force
		//Debug.Log(aStrength);
		Vector3 force = (aDirection + (Vector3.up * (1.7f + Random.Range(0.3f, 1.0f) * aRigidBody.mass )) ) * (80f * aStrength); //aDamage;
		aRigidBody.AddForce(force);

		// Torque
		// rotate the orienation so the object rotates away from the impact
		Vector3 orientation = Quaternion.AngleAxis(90 + Random.Range(-20, 21), Vector3.up) * aDirection;
		aRigidBody.AddTorque(orientation * 1000f); // NEED VARIATION TOO!
	}

	/// <summary>
	/// Reset ImpactManager
	/// </summary>
	public static void Reset()
	{
		impactCounter = 0;
	}
}