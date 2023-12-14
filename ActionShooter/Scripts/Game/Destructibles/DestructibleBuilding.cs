using UnityEngine;
using System.Collections;

/// <summary>
/// Destructible building.
/// <para>Inherits from Destructible </para>
/// </summary>
public class DestructibleBuilding : Destructible
{
	// open vars
	public enum State {INACTIVE, ACTIVE, HIT_SHAKE, SHAKE, HIT_DESTROY, DESTROY, DEAD} // different states this building van have
	public State state = State.INACTIVE; // default state
	public Vector3 sourcePosition; // holds its original default position

	private float shakeIntensity = 0.0f; // intensity of the building shake
	private Vector3 shakeVector = Vector3.zero; // shake direction

	private float height; // height of building
	private Bounds bounds = new Bounds(); // bounds
	private GameObject destructionEmitter; // object for smoke effect when building is being destroyed

	/// <summary>
	/// Initializes specific stuff for this inherited classes
	/// </summary>
	public override void InitializeSpecific()
	{
		gameObject.layer = DestructibleManager.buildingLayer; // reassign layer to buildings one
		// get the height of the building
		if (gameObject.transform.GetComponent<MeshFilter>() != null)
		{
			sourcePosition = gameObject.transform.position;
			bounds = gameObject.transform.GetComponent<MeshFilter>().mesh.bounds;
			height = bounds.size.y;
		}

	}

	void Update()
	{
		if (Data.pause) return; // don't do anything when we're paused
		if (state == State.INACTIVE) return; // do nothing if inactive

		// then what...?
		switch(state){
			case State.ACTIVE:
				shakeIntensity = 0; // reset intensity
				gameObject.transform.position = sourcePosition; // set building to original location
				state = State.INACTIVE; // deactivate building
				break;
								
			case State.HIT_SHAKE:
				shakeIntensity = Mathf.Min(1.0f, 0.05f) * 30.0f; // calc shake amount, which always seems to be 1.5f :S
				state = State.SHAKE; // shake it				
				break;
				
			case State.SHAKE:
				shakeVector = Random.onUnitSphere * shakeIntensity;	 // calculate shake vector
				shakeVector.y = 0; // don't go up
				shakeVector.Normalize(); // normalize
				gameObject.transform.position = sourcePosition + shakeVector; // offset
				shakeIntensity =  Mathf.Max(0.0f, shakeIntensity-(Time.deltaTime*13.33f)); // lower intensity per frame
				if (shakeIntensity <= 0.0f) state = State.ACTIVE; // if no shake left switch state to ACTIVE (=reset)
				break;
				
			case State.HIT_DESTROY: // Building is dead and we're preparing it here
				KillChildren(); // Destroy children on me
				// calc shake amount
				shakeIntensity = Mathf.Min(1.0f, 0.05f) * 30.0f; // once again 1.5f;
				state = State.DESTROY; // switch state
				destructibleData.destroyed = true; // we're 'done'
				
				// load destruction emitter, set it size to the building size, set to correct location
				destructionEmitter = Loader.LoadGameObject("Effects/BuildingDestructionEmitter_Prefab");
				destructionEmitter.transform.position = gameObject.transform.position;
				destructionEmitter.transform.rotation = gameObject.transform.rotation;
				Vector3 size = bounds.size; // gameObject.renderer.bounds.size;
				destructionEmitter.transform.localScale = new Vector3(size.x+3.5f, 1.0f, size.z+3.5f);		//new Vector3(size.x*1.2f, size.y*1.2f, 1.0f);		
				gameObject.AddComponent<Rigidbody>().isKinematic = true; // add rigidbody (this should optimize)
				break;
				
			case State.DESTROY: // we're currently in the process of being destroyed (shaking and moving down)
				// shake building
				float tDPerc = Mathf.Max(0.0f, 1.0f - (sourcePosition.y - gameObject.transform.position.y)/height);
				shakeVector = Random.onUnitSphere * shakeIntensity;
				shakeVector.y = 0;
				shakeVector.Normalize();		
				gameObject.transform.position = new Vector3(sourcePosition.x, gameObject.transform.position.y, sourcePosition.z) + (shakeVector*tDPerc);
				// Spawn explosion each 5 frames
				// Spawn them randomly on a 'side' of the building and only above the ground (else it would be waste)
				if (Time.frameCount%5==0)
				{
					float x, y, z;
					float extentX = bounds.extents.x + 2.5f;
					float extentZ = bounds.extents.z + 2.5f;
					if (Random.Range(0f, 1f) <= 0.5f) // left/right
					{
						x = (Random.Range(0f, 1f) <= 0.5f) ? -extentX : extentX;
						z = Random.Range(-extentZ, extentZ);
					} else // forward/backward
					{
						x = Random.Range(-extentX, extentX);
						z = (Random.Range(0f, 1f) <= 0.5f) ? -extentZ : extentZ;
					}
					y = Random.Range((int)Mathf.Abs(sourcePosition.y - Mathf.Abs(gameObject.transform.position.y)), (int)(height));
					Vector3 offset = new Vector3(x, y, z);
					// NOTE: We used to parent explosions to the building, but when the building was destroyed explosion could be destroyed as well
					// without 'notifying' the PoolManager
					ExplosionManager.AddExplosion("DefaultSmall", gameObject.transform.TransformPoint(offset));					
				}
				// move building down
				if (gameObject.transform.position.y > sourcePosition.y-height) gameObject.transform.position -= new Vector3(0, 7.5f*Time.deltaTime, 0);
				else Kill(); // finally remove me when I am completely 'down'
				break;

			case State.DEAD:
				break;
		}
	}

	/// <summary>
	/// Damage this destructible with projectile data and hitdata
	/// </summary>
	/// <param name="aProjectileData">A projectile data.</param>
	/// <param name="aHitData">A hit data.</param>
	public override void Damage(ProjectileData aProjectileData, HitData aHitData)
	{
		if (destructibleData.destroyed) return; // no damage when already destroyed
		destructibleData.health -= aProjectileData.damage; // damage
		if (destructibleData.health <= 0 && !destructibleData.destroyed) state = State.HIT_DESTROY; // setup my destruction when no health
		else{
			// Shake the building (when damage is not coming from a bullet)
			// Ideally you should implement something better here. E.g. only from explosive projectiles or explosions. Or only above a certain amount of damage.
			if (!aProjectileData.prefab.Contains("Bullet") && !aProjectileData.prefab.Contains("Fifty") && !aProjectileData.prefab.Contains("")) state = State.HIT_SHAKE; 
		}
	}	

	/// <summary>
	/// Kill (destroy) the destructible.
	/// </summary>
	public override void Kill()
	{
		state = State.DEAD; // final state, just to be clear
		Destroy (gameObject); // Destroy the building
		if (destructionEmitter != null){ // if there is a smoke emitter
			destructionEmitter.SetEmissionInChildren(false); // stop emitting
			Destroy (destructionEmitter, 1.2f); // timed Destroy so the particles fade out nicely
		}

		// Test to destroy objects ontop of buildings if they are parented to the buildings.
		Transform [] children = gameObject.GetComponentsInChildren<Transform>();
		foreach(Transform trans in children) if (trans != gameObject.transform) Destroy(trans.gameObject); // Destroy any other non-important objects parented to this building. E.g. billboards

		// Tell the MissionManager to process this object.
		MissionManager.ProcessTarget(destructibleData.type, destructibleData.score); // (DG) Added this here since. Not sure why it wasn't here.
	}

	/// <summary>
	/// Kills the children of this object.
	/// It will only look for other Destructibles and Vehicles.
	/// It will exclude 'myself'
	/// </summary>
	public void KillChildren()
	{
		// Kill vehicles
		Vehicle[] vehicles = gameObject.GetComponentsInChildren<Vehicle>();
		foreach(Vehicle vehicle in vehicles) vehicle.Kill();

		// Kill destructibles, avoid myself.
		Destructible[] destructibles = gameObject.GetComponentsInChildren<Destructible>();
		foreach(Destructible destructible in destructibles) 
		{
			if(destructible == this) continue;
			destructible.Kill();
		}
	}

}
	
