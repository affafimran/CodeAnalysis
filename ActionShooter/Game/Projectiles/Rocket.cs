using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Rocket : Projectile
{
	public GameObject trail;
	public bool trailEnabled = false;
	public bool homeTo = true;
	private float distance = 500f;

	protected override void InitializeSpecific()
	{
		// homeTo
		if (projectileData.target == null) homeTo = false;
		else
		{
			// offset 
			gameObject.transform.Rotate(Random.Range(-15, -5), 0, Random.Range(-7, 7));
			projectileData.direction = gameObject.transform.up;
		}

		// trail
		GameObject temp = Loader.LoadGameObject("Effects/RocketTrail_Prefab");
		trail = Instantiate (temp, transform.position, Quaternion.identity) as GameObject;
		Destroy (temp);
		trail.transform.ResetToParent(gameObject);
		trail.SetEmissionInChildren (false);
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
		else 
		{
			// orientate and move!
			if (homeTo)
			{
				Quaternion sourceRotation = gameObject.transform.rotation;
				gameObject.transform.LookAt(projectileData.endPosition);//, new Vector3(1, 0, 0));
				gameObject.transform.Rotate(90, 0, 0); // FFS
				Quaternion endRotation = gameObject.transform.rotation;
				gameObject.transform.rotation = Quaternion.Slerp(sourceRotation, endRotation, 5f * Time.deltaTime);
				projectileData.direction = gameObject.transform.up;
				// if overshot destroy rocket
				float tempDistance = (gameObject.transform.position-projectileData.endPosition).magnitude;
				if (tempDistance <= distance) distance = tempDistance;
				else Explode();
			} else gameObject.transform.RotateAround(gameObject.transform.position, projectileData.direction, 45*Time.deltaTime);
			gameObject.transform.position += projectileData.direction*(projectileData.speed*Time.deltaTime);
		}
		
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
		AreaDamageManager.AddAreaDamage(projectileData.areaDamage, gameObject.transform.position, projectileData.sender);
		// Destroy
		Destroy();
	}

	protected override void Destroy()
	{
		// time destroy trail
		trail.transform.parent = null;
		trail.SetEmissionInChildren(false);
		Destroy(trail, 3f);
		Scripts.audioManager.StopSFX(projectileData.sound);
		// destroy rocket
		PoolManager.ReturnObjectToPool(projectileData.prefab, gameObject); // IF THERE IS NO POOL THIS OBJECT WILL BE DESTROYED!!
	}
}

