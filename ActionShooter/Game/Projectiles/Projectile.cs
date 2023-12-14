using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Projectile : MonoBehaviour {
	// all projectile data
	public ProjectileData projectileData;
		
	// raycast
	internal HitData hitData; // no need in making this public -> this one is destroyed before you can see it!
	internal LayerMask layerMask = ProjectileManager.projectileLayerMask;
	internal RaycastHit rayCastHit;
	internal Ray ray;
	
	public void Initialize(ProjectileData aProjectileData)
	{
		// store date
		projectileData = aProjectileData;

		// set & orientate
		gameObject.transform.position = projectileData.startPosition + (projectileData.direction * 2.2f);
		gameObject.transform.LookAt(projectileData.startPosition + (projectileData.direction * 10));
		gameObject.transform.Rotate(90, 0, 0); // FFS

		// update position
		projectileData.position = gameObject.transform.position;

		// layer
		gameObject.layer = ProjectileManager.projectileLayer;
		// Play the projectiles moving sound
		//Scripts.audioManager.PlaySFX("", "Projectiles/"+projectileData.sound, 0.2f, -1);
		// Specific stuff for each projectile
		InitializeSpecific();
	}
	
	protected virtual void InitializeSpecific()
	{}

	protected virtual void Update()
	{
		// WHY?
		if (Data.pause) return;
		
		// prepare ray
		ray = new Ray(gameObject.transform.position, projectileData.direction);
		if(Physics.Raycast(ray, out rayCastHit, (projectileData.speed*Time.deltaTime)+0.1f, layerMask)) // Keep checking the distance
		{
			// hitdata
			hitData = ProjectileManager.RayCastHitToHitData(rayCastHit);
			// damage
			ProjectileManager.ProjectileDamage(projectileData, hitData);		
			// Destroy
			Destroy();
			
		} else gameObject.transform.position += projectileData.direction*(projectileData.speed*Time.deltaTime);

		// update dynamic projectileData vars 
		projectileData.position = gameObject.transform.position;
		projectileData.lifePercentage = projectileData.lifetime/projectileData.sourceLifeTime;
		projectileData.strength = projectileData.sourceStrength*projectileData.lifePercentage;

		// time to destroy?
		projectileData.lifetime -= Time.deltaTime;
		if (projectileData.lifetime <=0) Destroy();
	}
	
	protected virtual void Destroy()
	{
		//ProjectileManager.projectiles.Remove(gameObject); EFF it!?
		Scripts.audioManager.StopSFX(projectileData.sound);
		PoolManager.ReturnObjectToPool(projectileData.prefab, gameObject); // IF THERE IS NO POOL THIS OBJECT WILL BE DESTROYED!!
	}
}

