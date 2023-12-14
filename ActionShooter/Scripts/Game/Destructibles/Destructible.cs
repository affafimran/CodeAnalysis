using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Destructible script.
/// </summary>
public class Destructible : MonoBehaviour
{
	public string destructible; // name
	public DestructibleData destructibleData; // its data

	/// <summary>
	/// Initialize the destructinle object with destructible data.
	/// </summary>
	/// <param name="aDestructibleData">A destructible data.</param>
	public void Initialize(DestructibleData aDestructibleData)
	{
		destructibleData = aDestructibleData; // assign data
		destructible = destructibleData.type; // set type for reference
		gameObject.layer = DestructibleManager.destructibleLayer; // assign layer

		if(destructibleData.dynamic) gameObject.AddComponent<Rigidbody>().mass = destructibleData.mass; // dynamic
		else { // if not dynamic (those objects are never parented) and we should reparent; get the parent below and parent it
			if(destructibleData.reParent) {
				Transform newParent = GenericFunctionsScript.GetParentBelow(gameObject.transform.position);
				if (newParent != null) gameObject.transform.parent = newParent;
			}
		}
		InitializeSpecific(); // init specific stuff (like building?
	}

	/// <summary>
	/// Initializes specific stuff for this or other inherited classes
	/// </summary>
	public virtual void InitializeSpecific(){}

	/// <summary>
	/// Damage this destructible with projectile data and hitdata
	/// </summary>
	/// <param name="aProjectileData">A projectile data.</param>
	/// <param name="aHitData">A hit data.</param>
	public virtual void Damage(ProjectileData aProjectileData, HitData aHitData)
	{
		destructibleData.health -= aProjectileData.damage; // remove damage from health
		if (destructibleData.health <= 0 && !destructibleData.destroyed) Kill(aProjectileData, aHitData); // destroy if no health
	}

	/// <summary>
	/// Kill (destroy) the destructible.
	/// </summary>
	public virtual void Kill(){Kill(new ProjectileData(), new HitData());}
	public virtual void Kill(ProjectileData aProjectileData, HitData aHitData)
	{
		destructibleData.destroyed = true; // tag it. Some destructible might not be gone yet and don't need to be killed/processed again
		Collider collider = gameObject.GetComponent<Collider>(); // Collider set... :S
		if (collider.GetType() == typeof(MeshCollider)) gameObject.GetComponent<MeshCollider>().convex = true; // set to convex so we blast it away
	
		gameObject.transform.parent = DestructibleManager.debrisHolder.transform; // reparent to debrisholder (to keep things clear in our hierarchy)

		// add rigidbody if not already available
		// with some default data
		Rigidbody rb = gameObject.GetComponent<Rigidbody>();
		if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
		rb.SetDensity(1f);
		rb.drag = 0;
		rb.angularDrag = 0.05f;
		rb.useGravity = true;
		rb.constraints = RigidbodyConstraints.None;
		rb.velocity = Vector3.zero;

		// Add a random force and torque
		// Ideally you should take any ProjectileData and/or HitData into account. E.g. multiply the force by the damage strenght.
		rb.AddRelativeTorque(Random.onUnitSphere * Random.Range(200, 400) * rb.mass);
		rb.AddForce(Vector3.up * Random.Range(800, 1500) * rb.mass);

		// Spawn a (visual only) explosion
		ExplosionManager.AddExplosion(destructibleData.explosion, gameObject.transform.position);
		// Add area damage (Default small)
		AreaDamageManager.AddAreaDamage("Small", gameObject.transform.position);

		// Change the material to ugly debris
		if (gameObject.GetComponent<Renderer>() != null) gameObject.GetComponent<Renderer>().material = Loader.LoadMaterial("Effects/Debris_Material");

		// Spawn spawners
		SpawnerManager.AddSpawners(gameObject.transform.position, destructibleData.spawners);
		// Spawn pickups
		PickUpManager.AddPickUps(gameObject.transform.position, destructibleData.pickUps, gameObject.GetComponent<Collider>().bounds.extents.magnitude);

		// Detroy the object based upon lifetime in destrucible data
		// if time == 0f then the object will NOT be destroyed. It is cool to have some debris floating around
		if(destructibleData.lifetime >= 0f) { Destroy(gameObject, destructibleData.lifetime); gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero; }

		// Tell the MissionManager to process this object.
		MissionManager.ProcessTarget(destructibleData.type, destructibleData.score);
	}
}

