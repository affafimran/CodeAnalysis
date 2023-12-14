using UnityEngine;
using System.Collections;

public class Grenade : MonoBehaviour
{
	// all projectile data
	ProjectileData projectileData;

	// raycast
	private HitData hitData; // no need in making this public -> this one is destroyed before you can see it!
	private LayerMask layerMask = ProjectileManager.projectileLayerMask;
	private Ray ray;

	private float deltaTime;
	private float lifeTime = 5f;

	private float maxRange = 35f;

	public void Initialize(ProjectileData aProjectileData, HitData aHitData)
	{
		projectileData = aProjectileData;

		gameObject.transform.position = aProjectileData.startPosition;
		gameObject.layer = ProjectileManager.projectileLayer;

		gameObject.AddComponent<Rigidbody>();
		gameObject.GetComponent<Rigidbody>().angularDrag = 1f;
			
		GameObject camera = CameraManager.activeCamera;
		Vector3 inheritedVelocity = Scripts.hammer.characterController.velocity; // [HARDCODED] 

		Vector3 direction = (camera.transform.position + (camera.transform.forward*maxRange)) - gameObject.transform.position;
		if (aHitData.result && aHitData.distance <= maxRange) direction = aHitData.position - gameObject.transform.position;

		float h = direction.y;					// get height difference
		float distance = direction.magnitude;  	// get horizontal distance
		direction.y = distance;  				// set elevation to 45 degrees
		distance += h;  						// correct for different heights
		float velocity = Mathf.Sqrt(Mathf.Abs(distance) * Physics.gravity.magnitude);
		gameObject.GetComponent<Rigidbody>().velocity = inheritedVelocity + (velocity * direction.normalized);

		// random rotation
		gameObject.GetComponent<Rigidbody>().AddRelativeTorque(Random.insideUnitCircle * 100f);
	}

	void Update()
	{
		// WHY?
		if (Data.pause) return;
		// time and life
		deltaTime = Time.deltaTime;
		lifeTime -= deltaTime;
		// cast ray and do stuff
		float speed = gameObject.GetComponent<Rigidbody>().velocity.magnitude*Time.fixedDeltaTime+0.3f; // HMMM, test
		ray = new Ray(gameObject.transform.position, gameObject.GetComponent<Rigidbody>().velocity.normalized);
		if(Physics.Raycast(ray, speed, layerMask)) Explode();
		else if (lifeTime<=0) Explode();
	}

	void Explode()
	{
		// damage
		ExplosionManager.AddExplosion("Default", gameObject.transform.position);
		AreaDamageManager.AddAreaDamage("Small", gameObject.transform.position);
		// Destroy
		Destroy();
	}

	public void Destroy()
	{
		PoolManager.ReturnObjectToPool(projectileData.prefab, gameObject); // IF THERE IS NO POOL THIS OBJECT WILL BE DESTROYED!!
	}


}

