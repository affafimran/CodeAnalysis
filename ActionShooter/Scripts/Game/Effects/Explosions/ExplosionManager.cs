using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// ExplosionManager.
/// <para>NOTE: This only handles the audio- and visual representation. You should take care of actual damage<./para>
/// </summary>
public static class ExplosionManager
{
	public static int explosionCounter = 0; // amount of explosions created
	public static GameObject explosionHolder; // holder object to parent explosions to

	/// <summary>
	/// Initialize the ExplosionManagers
	/// </summary>
	public static void Initialize()
	{
		GameObject tExplosionHolder = GameObject.Find("Effects"); // Does it already exists? Note: We're looking for 'Effects'. Don't want holder objects for ALL types of effects.
		if (tExplosionHolder == null) explosionHolder = new GameObject("Effects"); // No, make a new one.
		else explosionHolder = tExplosionHolder; // Yes, store it.

		// Pooling (NEW!)
		// PIETER. I have added the most common explosion types!
		PoolManager.CreatePool("Effects/", "ExplosionSmall", 15);
		PoolManager.CreatePool("Effects/", "ExplosionDefault", 20);
		if (!GameData.mobile) PoolManager.CreatePool("Effects/", "ExplosionDeluxe", 30); // [MOBILE] NOTE: This one here is for show when you deploy mobile builds, since this one is quite heavy we don't use this see 'mobile hack' below

	}

	/// <summary>
	/// Add a explosion.
	/// </summary>
	/// <returns>The explosion.</returns>
	/// <param name="anExplosion">An explosion.</param>
	/// <param name="aPosition">A position.</param>
	public static GameObject AddExplosion(string anExplosion, Vector3 aPosition) {return AddExplosion(anExplosion, aPosition, true);}
	public static GameObject AddExplosion(string anExplosion, Vector3 aPosition, bool aDelete)
	{
		++explosionCounter; // count up (for whatever reason)

		// Get data!
		// We're only using this data for references, maybe in the future we can send or need to send this along.
		ExplosionData explosionData = GenericFunctionsScript.ParseDictionaryToClass(Data.Shared["Explosions"].d[anExplosion].d, new ExplosionData()) as ExplosionData;

		// [MOBILE] 'Hack'. Revert Deluxe to Default for optimization!
		if (GameData.mobile && explosionData.prefab == "ExplosionDeluxe") explosionData.prefab = "ExplosionDefault";


		// Create object or return
		if (explosionData.prefab == "None") return null;

		// Get from pool or create!
		GameObject explosion;
		if(PoolManager.DoesPoolExist(explosionData.prefab)) explosion = PoolManager.GetObjectFromPool(explosionData.prefab);
		else explosion	= Loader.LoadGameObject("Effects/" + explosionData.prefab + "_Prefab");

		if (explosion == null) return null;
		explosion.transform.position = aPosition;

		// Since the introduction of the poolmanager you have to handle this differently.
		// These prefabs don't have any scripts to destroy them, we time it with the default Destroy() function.
		// We've introduced a new Component which does better timing and destroys/returns to a pool
		if (aDelete) explosion.AddComponent<DestroyOnParticlesDone>().Initialize(explosionData.prefab);

		// Sound effect
		if (explosionData.sound != "None"){
			string sfx = explosionData.sound;
			if (sfx.IndexOf("_") != -1){ // pick random sfx if _X in name where x is an int
				string[] sfxArray = sfx.Split('_');
				sfx = sfxArray[0];
				sfx = sfx + Random.Range(1, int.Parse(sfxArray[1]));				
			}
			Scripts.audioManager.PlaySFX3D("Effects/"+sfx, explosion, "Impact"); // 3d sound, link to actual effect
		}

		// shake
		if (explosionData.shake > 0f){
			float distance = (CameraManager.activeCamera.transform.position - aPosition).magnitude;
			float perc = 1.0f - Mathf.Min(1.0f, (distance/explosionData.range));
			CameraManager.activeCameraData.shakeIntensity = explosionData.shake * perc;
		}

		// parent
		explosion.transform.parent = explosionHolder.transform;

		// return
		return explosion;
	}

	/// <summary>
	/// Reset ExplosionManager
	/// </summary>
	public static void Reset()
	{
		explosionCounter = 0;
	}
}