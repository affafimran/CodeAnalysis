using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ApacheRocket : Projectile
{
	public GameObject trail;
	public bool trailEnabled = false;

	protected override void InitializeSpecific()
	{
		// trail
		GameObject temp = Loader.LoadGameObject("Effects/MissileTrail_Prefab");
		trail = Instantiate (temp, transform.position, Quaternion.identity) as GameObject;
		Destroy (temp);
		trail.transform.ResetToParent(gameObject);
		trail.SetEmissionInChildren (false);

		// here is some fun stuff
		gameObject.transform.Rotate(0, Random.Range(0, 360), 0);

	}
	
	
	protected override void Update()
	{
		// WHY?
		if (Data.pause) return;
		
		// trail
		if (!trailEnabled)
		{
			trail.SetEmissionInChildren(true);
			trailEnabled = true;
		}
				
		// prepare ray
		ray = new Ray(gameObject.transform.position, projectileData.direction);
		if(Physics.Raycast(ray, out rayCastHit, (projectileData.speed*Time.deltaTime)+0.1f, layerMask)) Explode();
		else gameObject.transform.position += gameObject.transform.up *(projectileData.speed*Time.deltaTime);

		// rotate
		gameObject.transform.Rotate(0.15f, 0, 0, Space.Self);

		// update dynamic projectileData vars 
		projectileData.position = gameObject.transform.position;
		projectileData.lifePercentage = projectileData.lifetime/projectileData.sourceLifeTime;
		projectileData.strength = projectileData.sourceStrength*projectileData.lifePercentage;
		
		// time to destroy?
		projectileData.lifetime -= Time.deltaTime;
		if (projectileData.lifetime <=0) Destroy();
	}
	
	void Explode()
	{
		// damage
		ExplosionManager.AddExplosion(projectileData.explosion, gameObject.transform.position);
		AreaDamageManager.AddAreaDamage("Small", gameObject.transform.position, projectileData.sender);
		// Destroy
		Destroy();
	}
	
	protected override void Destroy()
	{
		// time destroy trail
		trail.transform.parent = null;
		trail.SetEmissionInChildren(false);
		Destroy(trail, 2f);
		trail.transform.parent = ExplosionManager.explosionHolder.transform;
		Scripts.audioManager.StopSFX(projectileData.sound);
		// destroy rocket
		PoolManager.ReturnObjectToPool(projectileData.prefab, gameObject); // IF THERE IS NO POOL THIS OBJECT WILL BE DESTROYED!!
	}
}

