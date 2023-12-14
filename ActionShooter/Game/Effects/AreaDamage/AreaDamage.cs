using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Area damage script.
/// <para>Creates a growing sphere, doing damage to objects inside its range.</para>
/// </summary>
public class AreaDamage : MonoBehaviour
{
	public AreaDamageData areaDamageData; // the areadamage data
	public List<GameObject> damagedList; // objects already damaged by this areadamage.

	public ProjectileData projectileData; // reference to ProjectileData class (used to damage objects)
	public HitData hitData; // reference to HitData class (used to damage objects)

	public LayerMask layerMask = ProjectileManager.projectileLayerMask; // layers of objects to be damaged (same as projectile since we're using ProjectileData as well)

	private GameObject sphere; // for debug purposes
	private Material sphereMaterial;

	/// <summary>
	/// Initialize AreaDamage
	/// </summary>
	/// <param name="anAreaDamageData">An area damage data.</param>
	public void Initialize(AreaDamageData anAreaDamageData) { Initialize (anAreaDamageData, null); }
	public void Initialize(AreaDamageData anAreaDamageData, GameObject aSender)
	{
		areaDamageData = anAreaDamageData; // store it
		damagedList = new List<GameObject>(); // new list
	
		hitData = new HitData(); // hitdata

		// Create new/update projectiledata (get from SharedData.txt)
		projectileData = new ProjectileData();
		projectileData = GenericFunctionsScript.ParseDictionaryToClass(Data.Shared["Projectiles"].d["AreaDamage"].d, projectileData) as ProjectileData;
		
		// Update some dynamic ones
		projectileData.startPosition  = areaDamageData.position; // actual position
		projectileData.direction      = Vector3.up; // only up
		projectileData.sender         = aSender; // store me as sender
		projectileData.strength = areaDamageData.strength; // update projectildata strength

		gameObject.transform.position = areaDamageData.position; // set on position

		// if we debug create a sphere so you can view its behavior
		if (AreaDamageManager.areaDamageDebug)
		{
			sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			sphereMaterial = Instantiate(Loader.LoadMaterial("Effects/AreaDamageDebug_Material")) as Material;
			sphere.GetComponent<Renderer>().material = sphereMaterial;
			Destroy(sphere.GetComponent<SphereCollider>());
			sphere.transform.ResetToParent(gameObject);
		}
	}


	void Update()
	{
		if (Data.pause) return; // don't do anything when we're paused

		if (areaDamageData.damage <= 0){Destroy(gameObject); if(AreaDamageManager.areaDamageDebug)Destroy(sphere);} // Destroy me when I have no more damage to deal
		else{
			// check and do damage
			Collider[] colliders = Physics.OverlapSphere(gameObject.transform.position, areaDamageData.radius, layerMask);
			int max = colliders.Length-1;
			GameObject tObject;
			for (int i = max; i >= 0; i--) { // go over all objects from overlap.
				tObject = colliders[i].gameObject; // get actual object
				if (damagedList.Contains(tObject)) continue; // if object is in the list we're already damaged it
				// get rigidbody, if available blast object away
				Rigidbody rb = tObject.GetComponent<Rigidbody>();
				if (rb != null) {
					Vector3 offset = Vector3.up * 7f;
					Vector3 direction = (tObject.transform.position - (gameObject.transform.position-offset)).normalized;
					float multiplier = (rb.mass * areaDamageData.strength * 8f) * areaDamageData.lifePercentage;
					Vector3 force = direction * multiplier;
					rb.AddForce(force);
					Vector3 orientation = Quaternion.AngleAxis(90 + Random.Range(-20, 21), Vector3.up) * direction;
					rb.AddTorque(orientation * multiplier);
				}

				projectileData.damage = areaDamageData.damage; // amount of damage
				projectileData.lifePercentage = areaDamageData.lifePercentage; // life as percentage
				projectileData.direction = (tObject.transform.position - gameObject.transform.position).normalized; // direction of damage

				hitData.gameObject = tObject; // store hit object

				AreaDamageManager.AreaDamage(projectileData, hitData); // handle the damage to the object
				damagedList.Add(tObject); // add to list
			}

			// debug
			// update the scale and color of the debug sphere
			if (AreaDamageManager.areaDamageDebug)
			{
				sphere.transform.position = gameObject.transform.position;
				sphere.transform.localScale = Vector3.one * areaDamageData.radius;
				sphereMaterial.color = new Color(255*areaDamageData.lifePercentage, 190*areaDamageData.lifePercentage, 0, areaDamageData.lifePercentage);
			}

		}

		areaDamageData.lifeTime -= Time.deltaTime; // update lifetime
		areaDamageData.lifePercentage = areaDamageData.lifeTime/areaDamageData.sourceLifeTime; // calculate life as percentage
		
		float reversePercentage = Mathf.Max(0f, 1f - areaDamageData.lifePercentage);
		areaDamageData.damage = areaDamageData.sourceDamage * areaDamageData.lifePercentage; // calculate the amount of damage left
		areaDamageData.radius = areaDamageData.start + (areaDamageData.size*reversePercentage); // calculate the radius of the areadamage
	}
	
}

