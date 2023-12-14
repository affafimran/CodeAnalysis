using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public static class ProjectileManager
{
	public static int projectileCounter;
	//public static List<GameObject> projectiles;
	public static int projectileLayer;
	public static LayerMask projectileLayerMask;
	public static GameObject projectileHolder;

	public static void Initialize()
	{
		projectileCounter = 0;
		//projectiles = new List<GameObject>();
		projectileLayer = LayerMask.NameToLayer("Projectiles");
		projectileLayerMask = (1 << 0) | (1 << 10) | (1 << 11) | (1 << 13) | (1 << 14)| (1 << 15);
		projectileHolder = new GameObject("Projectiles");

		// Pooling (NEW!)
		// PIETER. I have added the most common projectile types!
		PoolManager.CreatePool("Projectiles/", "DefaultBullet", 100);
		PoolManager.CreatePool("Projectiles/", "Grenade", 15);
		PoolManager.CreatePool("Projectiles/", "RpgRocket", 20);

	}

	public static void AddProjectile(string aType, Vector3 aPosition, Vector3 aDirection, HitData aHitData) {AddProjectile(aType, aPosition, aDirection, aHitData, null, null);}
	public static void AddProjectile(string aType, Vector3 aPosition, Vector3 aDirection, HitData aHitData, GameObject aTarget) {AddProjectile(aType, aPosition, aDirection, aHitData, aTarget, null);}
	public static void AddProjectile(string aType, Vector3 aPosition, Vector3 aDirection, HitData aHitData, GameObject aTarget, GameObject aSender)
	{
		// count amount of projectiles
		++projectileCounter;

		// check if the projectile has been defined in shared
		if (Data.Shared["Projectiles"].d.ContainsKey(aType) == false) return;

		// Create new/update projectiledata
		ProjectileData projectileData = new ProjectileData();
		projectileData = GenericFunctionsScript.ParseDictionaryToClass(Data.Shared["Projectiles"].d[aType].d, projectileData) as ProjectileData;

		// Update some dynamic ones
		projectileData.startPosition  = aPosition;
		projectileData.endPosition    = aHitData.position;;
		projectileData.direction      = aDirection;
		projectileData.sourceStrength = projectileData.strength;
		projectileData.sourceLifeTime = projectileData.lifetime;
		projectileData.target         = aTarget;
		projectileData.sender         = aSender;

		// check for instant hit, if so route to projectileDamage
		if (aHitData.result && ((projectileData.speed*Time.deltaTime) >= aHitData.distance)){
			ProjectileDamage(projectileData, aHitData);
			return;
		}

		// create the actual object!
		GameObject projectile;
		if(PoolManager.DoesPoolExist(projectileData.prefab)) projectile = PoolManager.GetObjectFromPool(projectileData.prefab);
		else projectile	= Loader.LoadGameObject("Projectiles/" + projectileData.prefab + "_Prefab");

		// what to do with it!
		switch(aType)
		{
			// Weapons
			case "DefaultBullet":
			case "FiftyCaliber":
			case "EnemyBullet":
			case "Remington":	
			case "DesertEagle":	
			case "Magnum":	
			case "AK47":	
			case "HelicopterBullet":	
				projectile.AddComponent<DefaultBullet>().Initialize(projectileData);
				break;		
			case "Missile":
				projectile.AddComponent<Missile>().Initialize(projectileData);
				break;
			case "F22Missile":
				projectile.AddComponent<F22Missile>().Initialize(projectileData);
				break;
			case "RpgRocket":
			case "TankShell":
				projectile.AddComponent<Rocket>().Initialize(projectileData);
				break;
			case "ApacheRocket":
				projectile.AddComponent<ApacheRocket>().Initialize(projectileData);
				break;

			case "Shell":
				projectile.AddComponent<Shell>().Initialize(projectileData);
				break;
			case "Bomb":
				projectile.AddComponent<Bomb>().Initialize(projectileData);
				break;

			// Explosives
			case "Grenade":
				projectile.AddComponent<Grenade>().Initialize(projectileData, aHitData);
				break;
		}	

		// parent to the holder
		projectile.transform.parent = projectileHolder.transform;
		// Add to list for future reference
		//ProjectileManager.projectiles.Add(projectile); EFF it!
	}

	public static void ProjectileDamage(ProjectileData aProjectileData, HitData aHitData)
	{
		// this is odd, we should investigate!!
		if (aHitData.gameObject == null) return;
		// continue...
		string layer = LayerMask.LayerToName(aHitData.gameObject.layer);
		// topparent
		GameObject topParent;
		// (Pieter} Not sure if we'd still need this, but I am afraid to remove it.
		if (layer == "Hammer"){
			if (aHitData.gameObject.name != "Hammer_Prefab") return;
		}

		switch(layer){
			case "Hammer":
			case "Enemies":
				Character character = aHitData.gameObject.GetComponent<Character>();
				if (character != null){
					character.Damage(aProjectileData, aHitData); 
					if (GameData.mobile) ImpactManager.AddImpact("EnemyHitLQ", aHitData); // [MOBILE] Override to make sure we'd use the LowQuality bloodimpact prefab/
					else ImpactManager.AddImpact(character.GetImpactType(aProjectileData.damage), aHitData);
				}
				break;

			case "Vehicles":
				topParent = GenericFunctionsScript.FindTopParentInCurrentLayer(aHitData.gameObject.gameObject, aHitData.gameObject.layer);
				if (topParent.GetComponent<Vehicle>() != null)
				{
					Vehicle vehicle = topParent.GetComponent<Vehicle>();
					vehicle.Damage(aProjectileData, aHitData);
					ImpactManager.AddImpact(vehicle.vehicleData.impact, aHitData);
				}
				break;

			// Destructibles
			case "Buildings":
			case "Destructibles":
				Destructible destructible = aHitData.gameObject.GetComponent<Destructible>();
				if(destructible==null) return;
				destructible.Damage(aProjectileData, aHitData);
				ImpactManager.AddImpact(destructible.destructibleData.impact, aHitData);
				break;

			// Default layer is simple ricochet!
			case "Default":
				ImpactManager.AddImpact("RicochetConcrete", aHitData);
				break;

			default:
				//aRaycastHit.collider.gameObject.transform.root.SendMessage("HitByProjectile", SendMessageOptions.DontRequireReceiver); // keep testing this!
				break;
		}
	}

	// Convert a RayCastHit into a more userfriendly HitData
	public static HitData RayCastHitToHitData(RaycastHit aRayCastHit)
	{
		HitData hitData    = new HitData();
		hitData.result     = true;
		hitData.gameObject = aRayCastHit.collider.gameObject;
		hitData.position   = aRayCastHit.point;
		hitData.direction  = aRayCastHit.normal;
		hitData.distance   = aRayCastHit.distance;
		return hitData;
	}

	// Reset this manager
	public static void Reset()
	{
		projectileCounter = 0;
		//List<GameObject> projectiles = new List<GameObject>();
	}

}

