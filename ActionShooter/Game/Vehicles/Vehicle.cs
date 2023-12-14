using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Vehicle : MonoBehaviour {
	
	public bool vehicleActive = false; // is this vehicle active
	public bool allowInput = false; // bool to allowInput

	public VehicleInputController controller; // controller (can be None, AI, User)
	public VehicleInUseData vehicleInUseData; // contains in use data
	public VehicleData vehicleData; // vehicle data

	private Rigidbody rigidBody;
	private Vector3 speed;

	internal GameObject vehicle; // the actual 3d object, the gameobject is just a holder with the scripts on it
	internal GameObject boundsObject;
	internal GameObject spawner; // holder for a spawner object, this object can be the parent of multiple spawner objects.

	private SpawnData spawnData;
		
	public void Initialize(VehicleData aVehicleData, VehicleManager.CONTROLLER aController, SpawnData aSpawnData)
	{
		// set this and the children to the vehicle layer
		gameObject.SetLayerRecursively(VehicleManager.vehicleLayer);

		// Store vehicle data & create 'in use data'
		vehicleData = aVehicleData;
		vehicleInUseData = new VehicleInUseData();

		// get/store vehicle & spawner if there. BEFORE InitializeSpecific
		vehicle = gameObject.transform.Find(vehicleData.prefab).gameObject; // this is always setup like this. gameObject = prefab (go with scripts).
		boundsObject = vehicle;
		spawner = GenericFunctionsScript.FindChild(gameObject, "Spawners"); // 

		// Initialize the specific vars for this Unit. BEFORE CONTROLLER
		InitializeSpecific();
		rigidBody = gameObject.GetComponent<Rigidbody>();

		// assign the correct controller
		AddController(aController); 

		// set correct position and orientation
		spawnData = aSpawnData;
		gameObject.transform.position = spawnData.position; // set position
		gameObject.transform.rotation = spawnData.rotation; // set rotation

		if (!vehicleData.dynamic) gameObject.transform.parent = GenericFunctionsScript.GetParentBelow(gameObject.transform.position);		

	}
	
	//---------------------------------------------------------------
	// Updates
	//---------------------------------------------------------------
	void FixedUpdate()
	{
		speed = rigidBody.velocity;
		FixedUpdateSpecific();
	}
	
	void Update()
	{
		controller.enabled = vehicleActive && allowInput && !Data.pause;
		if (Data.pause || !vehicleActive) return; // Not when paused or when unit is disabled.
		if (vehicleData.health > 0) UpdateSpecific(); // if health is bigger then 0 start the specific update
		if (gameObject.transform.position.y <= -25 && vehicleInUseData.inUse) Relocate(); // Falling through floor
	}

	void Relocate()
	{
		gameObject.transform.position = spawnData.position; // set position
		gameObject.transform.rotation = spawnData.rotation; // set rotation
		Scripts.interfaceScript.Fade("FadeWhiteFlash");
	}

	//---------------------------------------------------------------
	// Assign the controller (public so we can reassign controller
	// from somewhere else as well)
	//---------------------------------------------------------------
	public void AddController(VehicleManager.CONTROLLER aController)
	{
		// First clear controller, if any (so we can switch on the fly)
		if (controller != null)
		{
			if (controller.type == aController.ToString()) return;  // no need to change to the same controller
			else controller.Destroy();
		}
		string controllerAsString = aController.ToString();
		string tComponent = vehicleData.prefab+controllerAsString+"Controller";
		if (vehicleData.type == VehicleManager.TYPES.Car) tComponent = "Car"+controllerAsString+"Controller";
		Type tType = GenericFunctionsScript.GetType(tComponent);
		if (controllerAsString == "Empty" || tType == null) tComponent = "EmptyVehicleController";
		controller = gameObject.AddComponent(GenericFunctionsScript.GetType(tComponent)) as VehicleInputController;
		controller.Initialize();
		if (tType == null)controller.type = "Empty";
		else controller.type = aController.ToString();
		allowInput = true;
	}

	//---------------------------------------------------------------
	// Enter/Exit vehicle
	//---------------------------------------------------------------
	public virtual bool EnterVehicle(VehicleManager.CONTROLLER aController, GameObject aDriver)
	{
		vehicleInUseData.inUse = true;
		vehicleInUseData.driver = aDriver;
		vehicleInUseData.driver.SetActive(false);
		vehicleInUseData.previousCameraSetting = CameraManager.activeCameraData.settings;
		AddController(aController); // correct controller
		// Specifics
		EnterVehicleSpecific(); // Not sending anything through until I need it?
		// set the camera (maybe this could be a bit better)
		GameObject cameraTarget = gameObject;
		string target = Data.Shared["Cameras"].d[vehicleData.cameraSetting].d["targetName"].s;
		if (target+"_Prefab" != gameObject.name) cameraTarget = GenericFunctionsScript.FindChild(gameObject, target);
		CameraManager.UpdateSettings(vehicleData.cameraSetting, cameraTarget);
		return true;
	}
	
	public virtual void ExitVehicle()
	{
		vehicleInUseData.inUse = false;
		vehicleInUseData.driver.SetActive(true);
	
		// bounds check
		Bounds bounds = boundsObject.GetComponent<Renderer>().bounds;

		Vector3 position = boundsObject.transform.position + new Vector3(0, bounds.size.y+0.1f, 0);
		Vector3 rotation = new Vector3(0, vehicle.transform.eulerAngles.y, 0);
		
		vehicleInUseData.driver.transform.position = position;
		vehicleInUseData.driver.transform.eulerAngles = rotation;
	
		// Hack for now. Trying to find better solution. 
		// (Also tried: SetVelocity(Vector3.zero); but got an error because there was no receiver. Don't think I should change things in characterMotor).
		// Making the character act as if it just landed. Removes the weird velocity spikes. 
		CharacterMotor characterMotor = vehicleInUseData.driver.GetComponent<Hammer> ().characterMotor;
		characterMotor.grounded = false;
		
		vehicleInUseData.driver = null;

		AddController(VehicleManager.CONTROLLER.Empty); // correct controller
		CameraManager.UpdateSettings(vehicleInUseData.previousCameraSetting);
		vehicleInUseData.previousCameraSetting = "";
		ExitVehicleSpecific(); // Not sending anything through until I need it?
	}

	//---------------------------------------------------------------
	// Damage the vehicle
	//---------------------------------------------------------------
	public void Damage(ProjectileData aProjectileData, HitData aHitData)
	{
		if (vehicleData.destroyed) return; // just in case, since we could get (timed) areaDamage
		float actualDamage = (vehicleInUseData.inUse) ? aProjectileData.damage*0.1f : aProjectileData.damage; // if the vehicle is inuse lower the damage so you can enjoy your vehicle longer...
		vehicleData.health -= actualDamage; // damage it
		if (vehicleData.health <= 10 && !vehicleData.damageEffect) AddDamageEffect();
		if (vehicleData.health <= 0) Kill (aProjectileData, aHitData); // kill it
	}

	void AddDamageEffect()
	{
		vehicleData.vehicleDamageEffect = gameObject.AddComponent<VehicleDamageEffect>();
		vehicleData.vehicleDamageEffect.Initialize(this);
	}

	//---------------------------------------------------------------
	// Kill and destroy the vehicle
	//---------------------------------------------------------------
	public void Kill(){Kill(new ProjectileData(), new HitData());}
	public void Kill(ProjectileData aProjectileData, HitData aHitData)
	{
		// If we're driving this vehicle, exit the user...
		if (vehicleInUseData.inUse) ExitVehicle();
		
		// We switch to a None controller. Everything else will continue to run properly and it avoids all 'if destroyed then' mess
		AddController(VehicleManager.CONTROLLER.Empty); // Remove current controller and add the None one.
		
		// unit is destroyed
		vehicleData.destroyed = true;
		
		// Turn off all sounds (on me and children)
		gameObject.SetAudioSourcesInChildren(false);
		
		// Remove the smoke and fire visuals and SFX
		if (vehicleData.damageEffect) vehicleData.vehicleDamageEffect.Destroy();
		
		// Set all materials in children
		//gameObject.SetMaterialOnChildren();
		
		Material debris = Loader.LoadMaterial("Effects/Debris_Material"); // get material
		PhysicMaterial physics = Loader.LoadPhysicMaterial("ObjectPhysics_Material"); // get material
		
		vehicle.GetComponent<Renderer>().sharedMaterial = debris; // set unit to debris
		List<GameObject> children = vehicle.UnparentChildren(true); // unparent and get children from vehicle!!!
		Vector3 direction;
		BoxCollider boxCollider;
		Rigidbody rigidBody;
		foreach (GameObject child in children)
		{
			child.transform.parent = DestructibleManager.debrisHolder.transform;
			
			child.GetComponent<Renderer>().sharedMaterial = debris;
			
			rigidBody = child.GetComponent<Rigidbody>();
			if (rigidBody == null) rigidBody = child.AddComponent<Rigidbody>();
			if (child.GetComponent<Collider>() == null) 
			{
				boxCollider = child.AddComponent<BoxCollider>();
				boxCollider.material = physics;
				//rigidBody.mass = (boxCollider.size.x * boxCollider.size.y * boxCollider.size.z) * 2.75f;
				rigidBody.SetDensity(2.75f);
			}
			
			rigidBody.drag = 0.9f;
			rigidBody.angularDrag = 0.6f;
			
			direction = (child.transform.position - vehicle.transform.position).normalized;
			ImpactManager.ImpactForce(rigidBody, direction, aProjectileData.strength);
			
			if (child != vehicle) Destroy(child, 5f + UnityEngine.Random.Range(0f, 1f));
			
		}
		
		// Explosion
		ExplosionManager.AddExplosion(vehicleData.explosion, gameObject.transform.position);
		AreaDamageManager.AddAreaDamage("Small", gameObject.transform.position);
		
		// Destroy Spawners on xxxx gameobject
		SpawnerManager.DestroySpawners(spawner, true);
		
		// spawn spawners
		SpawnerManager.AddSpawners(gameObject.transform.position, vehicleData.spawners);
		PickUpManager.AddPickUps(gameObject.transform.position, vehicleData.pickUps, vehicle.GetComponent<Collider>().bounds.extents.magnitude); // should we use bounds as well? NO, probabl;y not
		
		// Destroy (SHABBY)
		vehicle.GetComponent<Rigidbody>().mass = gameObject.GetComponent<Rigidbody>().mass;
		vehicle.GetComponent<Rigidbody>().useGravity = true;
		vehicle.GetComponent<Collider>().material = physics;

		// WE NEED TO REMOVE THE IMPACT FORCE!
		ImpactManager.ImpactForce(vehicle.GetComponent<Rigidbody>(), Vector3.up, aProjectileData.strength);

		if(vehicleData.lifeTime > 0f) Destroy(vehicle, vehicleData.lifeTime); // destroy UNIT over period of time (or NOT)

		// Remove from list
		VehicleManager.RemoveVehicle(gameObject);
		
		// Score
		MissionManager.ProcessTarget(vehicleData.prefab, vehicleData.score);
		
		// Specific
		KillSpecific(aProjectileData, aHitData);
	}

	//---------------------------------------------------------------
	// Collision & damage to the stuff you hit
	//---------------------------------------------------------------
	void OnCollisionEnter(Collision collision)
	{
		// Note: collision is with single collider. I don't think it would be wise in our scenario
		// to process ALL contactPoints
		Vector3 hitPosition = collision.contacts[0].point;
		Vector3 hitNormal   = collision.contacts[0].normal;
		Rigidbody rb = gameObject.GetComponent<Rigidbody> ();

		HitData hitData = new HitData();
		hitData.position = hitPosition;

		ProjectileData projectileData = new ProjectileData();
		projectileData.direction = -rb.velocity.normalized;
		projectileData.position = hitPosition;

		projectileData.damage   = Mathf.Abs(Vector3.Dot (hitNormal, collision.relativeVelocity) * (rb.mass / 1000) / 100);
		projectileData.strength = Mathf.Abs(Vector3.Dot (hitNormal, collision.relativeVelocity) * (rb.mass / 1000) / 50);

		// Damage object
		GameObject damagedObject = collision.collider.gameObject;
		switch(damagedObject.layer) {
			case 11: // enemies
				Enemy enemy = damagedObject.GetComponent<Enemy>();
				if(enemy == null) return;
				// damage enemy
				projectileData.damage = 5; // HUH?
				enemy.Damage(projectileData, hitData);
				rigidBody.velocity = speed;
				break;

			case 13: // buildings
			case 14: // destructibles
				Destructible destructible =	damagedObject.GetComponent<Destructible>();
				if (destructible !=null) destructible.Damage(projectileData, hitData);
				ImpactManager.AddImpact("VehicleCollision", hitData, true);
				break;

			case 15: // vehicles
				GameObject vehicle = GenericFunctionsScript.FindTopParentInCurrentLayer(damagedObject, 14);
				Vehicle component = vehicle.GetComponent<Vehicle>();
				if (component != null) component.Damage(projectileData, hitData);
				ImpactManager.AddImpact("VehicleCollision", hitData, true);
				break;

			default:
			break;
		}

		// Damage myself
		projectileData.damage /= 4f; // but give myself break ;)
		Damage(projectileData, hitData);

	}

	// Specific init
	public virtual void InitializeSpecific() {}
	
	// Specific updates
	protected virtual void FixedUpdateSpecific() {}
	protected virtual void UpdateSpecific() {}

	// Specific enter/exitvehicle (e.g. car)
	protected virtual void EnterVehicleSpecific() {}
	protected virtual void ExitVehicleSpecific() {}

	// Kill specific
	protected virtual void KillSpecific(ProjectileData aProjectileData, HitData aHitData) {}
	
	// Set unit (in)active
	public virtual void SetVehicleActive(bool aBool) {vehicleActive = aBool;}
}
