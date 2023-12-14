using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
//using System.Reflection;

public class Hammer : Character
{
	// vehicle data since the hammer can enter and control vehicles
	public CharacterVehicleData vehicleData;

	// classes & gadgets
	public int amountOfWeapons = 1;
	public Weapon[] weapons;
	public Weapon weapon;
	public Explosive explosive;
	public Power power;
	public List<Gadget> gadgets;

	// weapons
	public WeaponManager.WEAPONS currentWeapon;
	private List<string> availableWeapons;
	
	// explosive
	public ExplosiveManager.EXPLOSIVES currentExplosive;
	private ExplosiveManager.EXPLOSIVES previousExplosive;
	
	// powers
	public PowerManager.POWERS currentPower;
	private PowerManager.POWERS previousPower;
	public bool isInBulletTime = false;

	private GameObject cameraObject;
	private CameraScript cameraScript;

	// spine
	public GameObject spine; // (DG) Made this public so we can link the jetpack.
	private Quaternion spineSourceRotation;

	// particles
	private GameObject jumpSmoke;
	private GameObject landSmoke;

	public override void InitializeSpecific()
	{

		vehicleData = new CharacterVehicleData();

		cameraObject = CameraManager.activeCamera;
		cameraScript = CameraManager.activeCameraScript;

		//--------------------------------------------------------------------------------
		// set my to player layer
		//--------------------------------------------------------------------------------
		gameObject.SetLayerRecursively(LayerMask.NameToLayer("Hammer"));

		//--------------------------------------------------------------------------------
		// controller (this is the script that actually controls the player)
		//--------------------------------------------------------------------------------
		string tComponent = "CharacterUserInput";
		controller = gameObject.AddComponent(GenericFunctionsScript.GetType(tComponent)) as CharacterInputController;
		controller.Initialize(); // [NEW]
		controller.enabled = false; // no movement!!

		//--------------------------------------------------------------------------------
		// Weapon(s)
		//--------------------------------------------------------------------------------
		availableWeapons = new List<string>(WeaponManager.availableWeapons);
		weapons = new Weapon[availableWeapons.Count];
		for (int i = 0; i < weapons.Length; i++) {
			weapons[i] = new Weapon();
		}
		currentWeapon = (WeaponManager.WEAPONS)System.Enum.Parse(typeof(WeaponManager.WEAPONS), characterData.weapon);
		weapon = System.Activator.CreateInstance(GenericFunctionsScript.GetType(currentWeapon.ToString())) as Weapon;
		weapon.Initialize(characterData, gameObject, animator);
		weapons[availableWeapons.IndexOf(currentWeapon.ToString())] = weapon;

		//--------------------------------------------------------------------------------
		// Explosive
		//--------------------------------------------------------------------------------
		currentExplosive = (ExplosiveManager.EXPLOSIVES)System.Enum.Parse(typeof(ExplosiveManager.EXPLOSIVES), characterData.explosive);
		explosive = System.Activator.CreateInstance(GenericFunctionsScript.GetType("Explosive" + currentExplosive.ToString())) as Explosive; 
		explosive.Initialize(gameObject);
		previousExplosive = currentExplosive;

		//--------------------------------------------------------------------------------
		// Gadget(s) (this is a list, since it can not be decided (through random pickups)
		// how many gadgets you will have.
		// HOWEVER IF WE ARE FINISHED AND WE KNOW HOW MANY GADGETS THERE WILL BE
		// WE CAN REVERT TO A SIMILAR SETUP AS WEAPONS. AS THAT IS MUCH MORE CONSISTENT
		//--------------------------------------------------------------------------------
		gadgets = new List<Gadget>();

		//--------------------------------------------------------------------------------
		// Power
		//--------------------------------------------------------------------------------
		currentPower = (PowerManager.POWERS)System.Enum.Parse(typeof(PowerManager.POWERS), characterData.power);
		power = System.Activator.CreateInstance(GenericFunctionsScript.GetType("Power" + currentPower.ToString())) as Power;
		power.Initialize(gameObject);
		previousPower = currentPower;

		//--------------------------------------------------------------------------------
		// other character related vars	
		//--------------------------------------------------------------------------------
		spine = GenericFunctionsScript.FindChild(gameObject, type + " Spine1");
		spineSourceRotation = spine.transform.rotation;

		//--------------------------------------------------------------------------------
		// set man(object) to correct position;
		//--------------------------------------------------------------------------------
		// We need to fix this. Looks like the artist rotated all spawn objects incorrectly
		gameObject.transform.Rotate(0, -90, 0);

		//--------------------------------------------------------------------------------
		// load particles
		//--------------------------------------------------------------------------------
		jumpSmoke = Loader.LoadGameObject("Effects/JumpSmoke_Prefab");
		//		jumpSmoke.transform.SetParent(GameObject.Find("Effects").transform); // Parented to hammer instead
		jumpSmoke.transform.SetParent(transform);
		jumpSmoke.transform.localPosition = Vector3.zero;
		landSmoke = Loader.LoadGameObject("Effects/LandSmoke_Prefab");
		landSmoke.transform.SetParent(GameObject.Find("Effects").transform);

		// register
		Scripts.hammer = this;
	}


	public override void Update()
	{
		// Pausing
		characterMotor.enabled = !Data.pause;
		controller.enabled = !Data.pause;
		if (Data.pause) return;

		// controller
		controller.enabled = allowInput;

		//--------------------------------------------------------------
		// Character movement (animation)
		//--------------------------------------------------------------
		// Jump (Do this in update. in FixedUpdate issue can occur)
		characterMotor.inputJump = controller.jump;
		animator.applyRootMotion = !characterMotor.grounded; // (Pieter) probably one if the hacks I dislike the most!
		if (characterMotor.grounded && controller.jump)
		{
			Scripts.audioManager.PlaySFX("Characters/Hammer/HammerJump");
			jumpSmoke.PlayInChildren();
		}

		// Rest of the movement that is not physics depended
		animator.SetFloat("Horizontal", controller.horAxis);		
		animator.SetFloat("Vertical", controller.verAxis);		

		if (animator.GetBool("Jump") && characterMotor.grounded)
		{
			Scripts.audioManager.PlaySFX("Characters/Hammer/HammerLand");
			landSmoke.transform.localPosition = transform.position;
			landSmoke.PlayInChildren();
		}

		animator.SetBool("Jump", !characterMotor.grounded);
		animator.SetBool("Fire3", controller.dive);



		//--------------------------------------------------------------
		// Weapons (select, switch, update, etc.)
		//--------------------------------------------------------------
		// previous/next primary weapon
		if (controller.previousPrimary) PreviousWeapon();
		if (controller.nextPrimary) NextWeapon();

		// update autoAim
		if (characterData.autoAim && controller.primaryFire && !vehicleData.isInVehicle){ // we only autoAim when true, when firing AND when you are not in a vehicle.
			// NOTE!
			// For autoaiming we use the camera to calculate offset/angle, etc.
			// The reason we do this is because we aim from the camera NOT the hammer
			// In the end we need to rotate the hammer in such a way that the camera is looking/aiming at the closest enemy
			// Another thing is that the camera is always update last (and in LateUpdate) so we CANNOT override the camera here!
			Enemy enemy = CharacterManager.AutoAimEnemy(cameraObject, 50, 2.0f); // Get the closest, most correct enemy
			if (enemy != null){ // We got a closest enemy
				Vector3 position = enemy.transform.position; // store its position
				position.y = cameraObject.transform.position.y; // flatten position to cameraObjects y

				// Get camera forward, flatten and normalize
				Vector3 forward = cameraObject.transform.forward; 
				forward.y = 0;
				forward.Normalize();

				// calculate angle between closest enemy and flattened forward
				float angle = Vector3.Angle(position-cameraObject.transform.position, forward);
				float left = 1f; // calculate if the object is left or right of the cameraObject
				if (cameraObject.transform.InverseTransformPoint(position).x < 0) left = -1f;
				// Rotate the hammer so the CAMERA is aiming at the enemy!
				if (angle != 0f) gameObject.transform.Rotate(0, angle * left, 0);
			}
		}

		bool weaponResult = weapon.Update(controller.primaryFire, hitData);
		if (!weaponResult && weapon.ammo == 0) RemoveWeapon(currentWeapon);

		// bulletTime
		if (characterData.bulletTimeAsPercentage > 0.1f)
		{
			if (controller.bulletTime && !isInBulletTime && !vehicleData.isInVehicle) EnterBulletTime();
		}
		if (!controller.bulletTime && isInBulletTime || characterData.bulletTimeAsPercentage < 0.01f) ExitBulletTime();
		if (isInBulletTime) {
			characterData.bulletTimeAsPercentage = Mathf.Max(0.0f, characterData.bulletTimeAsPercentage -= 0.25f * Time.unscaledDeltaTime); // Deplete the bulletTime when in use
			CameraManager.activeCameraData.shakeIntensity = 0f;
		}else characterData.bulletTimeAsPercentage = Mathf.Min(1.0f, characterData.bulletTimeAsPercentage += 0.10f * Time.unscaledDeltaTime); // Add it again when not in use.

		//--------------------------------------------------------------
		// Explosives (select, switch, update, etc.)
		//--------------------------------------------------------------
		// select
		int explosiveAsInt = (int)currentExplosive;
		int maxExplosives = Enum.GetNames(typeof(ExplosiveManager.EXPLOSIVES)).Length-1;
		//if (Input.GetKeyDown(KeyCode.LeftBracket)) explosiveAsInt -= 1;    // TEMP? Move to input?
		//if (Input.GetKeyDown(KeyCode.RightBracket)) explosiveAsInt += 1;   // TEMP? Move to input?
		explosiveAsInt = Mathf.Max(0, Mathf.Min(maxExplosives, explosiveAsInt)); 
		currentExplosive = (ExplosiveManager.EXPLOSIVES)explosiveAsInt;
		// switch
		if (previousExplosive != currentExplosive){
			explosive.Destroy();
			explosive = System.Activator.CreateInstance(GenericFunctionsScript.GetType("Explosive" + currentExplosive.ToString())) as Explosive; 
			explosive.Initialize(gameObject);
			previousExplosive = currentExplosive;
		}
		// update
		explosive.Update(controller.secondaryFire, hitData);

		//--------------------------------------------------------------
		// Powers (select, switch, update, etc.)
		//--------------------------------------------------------------
		// select
		int powerAsInt = (int)currentPower;
		int maxPowers = Enum.GetNames(typeof(PowerManager.POWERS)).Length-1;
		//if (Input.GetKeyDown(KeyCode.Minus)) powerAsInt -= 1;  // TEMP? Move to input?
		//if (Input.GetKeyDown(KeyCode.Equals)) powerAsInt += 1;   // TEMP? Move to input?
		powerAsInt = Mathf.Max(0, Mathf.Min(maxPowers, powerAsInt)); 
		currentPower = (PowerManager.POWERS)powerAsInt;
		// switch
		if (previousPower != currentPower){
			power.Destroy();
			power = System.Activator.CreateInstance(GenericFunctionsScript.GetType(currentPower.ToString())) as Power; 
			power.Initialize(gameObject);
			previousPower = currentPower;
		}
		// update
		power.Update(controller.powerOn);

		//--------------------------------------------------------------
		// Gadgets
		//--------------------------------------------------------------
		foreach(Gadget gadget in gadgets)
			gadget.Update();

		//--------------------------------------------------------------
		// HitData
		//--------------------------------------------------------------
		Vector3 muzzlePosition = weapon.bulletOrigin;
		Vector3 cameraPosition = cameraObject.transform.position;
		if (CameraManager.activeCameraData.settings == "HammerReverse") ray = new Ray(weapon.bulletOrigin, weapon.bulletDirection); // :(
		else ray = new Ray(cameraPosition, cameraObject.transform.forward);
		// TEMP
		hitData.result = Physics.Raycast(ray, out rayCastHit, 500, layerMask);
		hitData.position = Vector3.zero;
		hitData.distance = 0f;
		hitData.gameObject = null;

		if (hitData.result)
		{
			// (PIETER) I commented this, as it seems to work better for the player (always hit)
			//if (rayCastHit.distance > (cameraPosition - muzzlePosition).magnitude) // but only when it is not between the camera and man
			//{
			hitData.position = rayCastHit.point;
			hitData.distance = (hitData.position - muzzlePosition).magnitude;
			hitData.gameObject   = rayCastHit.collider.gameObject;
			//} else hitData.result = false;
			//Debug.DrawRay(cameraPosition, cameraObject.transform.forward*rayCastHit.distance, Color.green);
		} else Debug.DrawRay(cameraPosition, cameraObject.transform.forward*350f, Color.green);

		// Vehicle Generator
		if (controller.buildVehicle && hitData.result && hitData.distance >= 4f)
		{
			SpawnData spawnData = new SpawnData();
			spawnData.position = hitData.position;
			spawnData.rotation = gameObject.transform.rotation;
			Vehicle vehicle = VehicleManager.AddVehicle(VehicleManager.generatorVehicles[controller.selectedVehicle], VehicleManager.CONTROLLER.Empty, spawnData);
			vehicle.SetVehicleActive(true);
		}

		// (DG) Added this so that the interface can find out what we're aiming at.
		// (PIETER) This is not nice, you should get it from the interface script
		// NO globals sets from here!
		// Needs to be changed in the future
		GameData.currentObjectUnderMouse = hitData.gameObject;

		// Falling through floor
		if (gameObject.transform.position.y <= -25) Relocate();

		//--------------------------------------------------------------
		// Vehicledata
		//--------------------------------------------------------------
		UpdateVehicleData();
		
		if (controller.use && vehicleData.canEnterVehicle)
		{
			controller.Reset();
			vehicleData.isInVehicle = vehicleData.vehicle.GetComponent<Vehicle>().EnterVehicle(VehicleManager.CONTROLLER.User, gameObject); //, "Hammer");
			if (vehicleData.isInVehicle){
				Scripts.audioManager.PlaySFX("Interface/VehicleExit");
				vehicleData.canEnterVehicle = false;
			}
		}
	}

	void OnEnable(){
		if (!allowInput) return;
		vehicleData.isInVehicle = false;
		animator.SetBool(weapon.type, true);
	}

	void LateUpdate()
	{
		// no rotaty if pause
		if (Data.pause) return;

		// rotate spine
		spine.transform.Rotate(0, 0, cameraScript.mouseSettings.y);
	}

	void UpdateVehicleData()
	{
		// Reset data
		vehicleData.canEnterVehicle = false;
		vehicleData.vehicle = null;
		vehicleData.name = "";
		vehicleData.type = "Vehicle";

		// Get new data
		GameObject closestVehicle = VehicleManager.GetClosestVehicle(gameObject.transform.position);
		if (closestVehicle == null) return; /// BLAAAA

		Vector3 myPosition = gameObject.transform.position + new Vector3(0, 1, 0);
		Vector3 theirPosition = closestVehicle.transform.position;
		vehicleData.ray = new Ray(myPosition, (theirPosition-myPosition).normalized);
		bool hit = Physics.Raycast(vehicleData.ray, out vehicleData.rayCastHit, 50f, vehicleData.layerMask);
		// These lines will solve the geting into destroyed vehicles 
		// the raycast will return from destroyed vehicles because it is a layer check. 
		// This is what makes the enter vehicle text show up and allows you to enter vehicles that are miles away.
		if (hit) // the raycast hit something
		{
			GameObject raycastObject = vehicleData.rayCastHit.collider.gameObject; // easy acces
			raycastObject = GenericFunctionsScript.FindTopParentInCurrentLayer(raycastObject, raycastObject.layer); // get top parent object
			if (raycastObject != closestVehicle) hit = false; // check if the raycast returned from closestVehicle
		}

		if (hit && vehicleData.rayCastHit.distance <= vehicleData.canEnterRange)
		{
			vehicleData.canEnterVehicle = true;
			vehicleData.vehicle = closestVehicle;
			vehicleData.name = closestVehicle.name.Substring(0, closestVehicle.name.Length-7);
			vehicleData.type = closestVehicle.GetComponent<Vehicle>().GetType().Name;
		}
	}

	//-------------------------------------------------------
	// Weapons
	//-------------------------------------------------------
	public void AddWeapon(WeaponManager.WEAPONS aWeapon)
	{
		// weaponAsString
		string weaponAsString = aWeapon.ToString();

		// Do I already own this weapon? Yes, then reset!
		Weapon holder = weapons[availableWeapons.IndexOf(weaponAsString)];
		if (holder.initialized){
			holder.ResetWeapon(false);
			return; // We're done here
		}

		// deactivate the currentweapon
		weapon.SetWeaponActive(false);

		// I do not own the incoming weapon
		// Create it and make it active

		// create the new weapon
		currentWeapon = aWeapon;
		weapon = System.Activator.CreateInstance(GenericFunctionsScript.GetType(currentWeapon.ToString())) as Weapon;
		weapon.Initialize(characterData, gameObject, animator);

		// store in list
		weapons[availableWeapons.IndexOf(weaponAsString)] = weapon;

		// countweapons
		CountWeapons();

		Scripts.audioManager.PlaySFX("Characters/Hammer/HammerSwitchWeapon");

	}

	public void RemoveWeapon(WeaponManager.WEAPONS aWeapon)
	{
		weapon.Destroy(); // destroy it (throw away)
		int weaponAsInt =  availableWeapons.IndexOf(aWeapon.ToString());
		weapons[weaponAsInt] = new Weapon(); // clear in list

		// revert to next best weapon
		for (int i = weaponAsInt; i >= 0; i--) {
			if (weapons[i].type != "None"){
				weaponAsInt = i;
				break;
			}
		}

		currentWeapon = (WeaponManager.WEAPONS)System.Enum.Parse(typeof(WeaponManager.WEAPONS), availableWeapons[weaponAsInt]);
		weapon = weapons[weaponAsInt];
		weapon.SetWeaponActive(true);
				
		// countweapons
		CountWeapons();

		Scripts.audioManager.PlaySFX("Characters/Hammer/HammerSwitchWeapon");
	}

	void NextWeapon()
	{
		int weaponAsInt = availableWeapons.IndexOf(currentWeapon.ToString());

		// [MOBILE] This is a weapon cycle. We revert back to the first weapon when the last weapon is reached.
		if (GameData.mobile){
			int n = weaponAsInt+1;
			if (n == weapons.Length) n = 0;
			while(weapons[n].type == "None"){
				n++;
				if (n == weapons.Length) n = 0;
			}
			weaponAsInt = n;
		} else { // This is the 'normal' weapon cycle. We stop at the last weapon available.
			if (weaponAsInt == weapons.Length-1) return; // last weapon
			for (int i = weaponAsInt+1; i < weapons.Length; i++) {
				if (weapons[i].type != "None"){
					weaponAsInt = i;
					break;
				}
			}
		}

		// skip below if same weapon
		if (availableWeapons.IndexOf(currentPower.ToString()) == weaponAsInt) return;

		// disable currentweapon
		weapon.SetWeaponActive(false);
		weapon = weapons[weaponAsInt];
		weapon.SetWeaponActive(true);
		currentWeapon = (WeaponManager.WEAPONS)System.Enum.Parse(typeof(WeaponManager.WEAPONS), availableWeapons[weaponAsInt]);
		Scripts.audioManager.PlaySFX("Characters/Hammer/HammerSwitchWeapon");
	}

	void PreviousWeapon()
	{
		int weaponAsInt = availableWeapons.IndexOf(currentWeapon.ToString());
		if (weaponAsInt == 0) return; // first weapon

		for (int i = weaponAsInt-1; i >= 0; i--) {
			if (weapons[i].type != "None"){
				weaponAsInt = i;
				break;
			}
		}

		// disable currentweapon
		weapon.SetWeaponActive(false);
		weapon = weapons[weaponAsInt];
		weapon.SetWeaponActive(true);
		currentWeapon = (WeaponManager.WEAPONS)System.Enum.Parse(typeof(WeaponManager.WEAPONS), availableWeapons[weaponAsInt]);
		Scripts.audioManager.PlaySFX("Characters/Hammer/HammerSwitchWeapon");
	}

	void CountWeapons(){
		amountOfWeapons = 0;
		foreach(Weapon weapon in weapons)
			if (weapon.type != "None") ++amountOfWeapons;
	}
	
	void EnterBulletTime()
	{
		isInBulletTime = true;
		Scripts.audioManager.PlaySFX("Interface/GadgetBulletTime");
		CameraManager.UpdateSettings("HammerBulletTime");
		Time.timeScale = 0.1f;
		Time.fixedDeltaTime = 0.1f * 0.02f;
	}
	
	void ExitBulletTime()
	{
		isInBulletTime = false;
		CameraManager.UpdateSettings("Hammer");
		Time.timeScale = 1.0f;
		Time.fixedDeltaTime = 0.02f;
	}

	public void AddGadget(GadgetManager.GADGETS aGadget)
	{
		// This has changes since we're not going over an array anympre
		string gadgetAsString = aGadget.ToString();

		foreach(Gadget gadget in gadgets)
		{
			if (gadget.type == gadgetAsString)
			{
				gadget.ResetGadget();
				gadget.SetGadgetActive(true);
				return; // We're done here
			}
		}

		// make new since we didn't return above and we're here
		Gadget newGadget =System.Activator.CreateInstance(GenericFunctionsScript.GetType(gadgetAsString)) as Gadget;
		newGadget.Initialize(characterData, weapons); // NEEDS MORE LATER
		gadgets.Add(newGadget);
	}

	public override bool Damage(ProjectileData aProjectileData, HitData aHitData)
	{
		// do damage & kill
		if (!characterData.godMode) characterData.health -= aProjectileData.damage;
		characterData.healthAsPercentage = characterData.health/characterData.maxHealth;
		int random = UnityEngine.Random.Range(1, 5);
		if (characterData.health <= 0 && !characterData.playerDead)
		{
			if (GadgetManager.GadgetOwned("Pigeon")){ Respawn(); return false;}
			else {Kill(aProjectileData, aHitData); return true;}
		}
		else animator.Play("Hit"+random);//, true); Not a prefered solution but ok for now
		Scripts.audioManager.PlaySFX("Characters/Hammer/HammerHit"+random);
		return false;
	}

	public void Relocate()
	{
		gameObject.transform.position = spawnData.position;
		gameObject.transform.rotation = spawnData.rotation;
		gameObject.transform.Rotate(0, -90, 0); // We need to check this why
		Scripts.interfaceScript.Fade("FadeWhiteFlash");
	}

	public void Respawn()
	{
		characterData.health = characterData.maxHealth;
		// new spawnlocation
		Vector3 spawnLocation = transform.position + new Vector3(0.0f, 100.0f, 0.0f);
		RaycastHit spawnRayCastHit;
		Ray spawnRay = new Ray(gameObject.transform.position + new Vector3(0, 2f, 0), Vector3.up);
		if(Physics.Raycast(spawnRay, out spawnRayCastHit, 100f, layerMask)) spawnLocation = spawnRayCastHit.point - new Vector3(0, 2f, 0);
		transform.position = spawnLocation;
		Scripts.interfaceScript.Fade("FadeWhiteFlash");
		Scripts.audioManager.PlaySFX("Interface/GadgetHolyPigeon");
	}
	
	public void Kill(ProjectileData aProjectileData, HitData aHitData)
	{
		// set to dead
		characterData.playerDead = true;

		// animation
		foreach(Weapon weapon in weapons){
			if (weapon.initialized) weapon.Destroy();
		}
		//weaponData.Destroy();
		int random = UnityEngine.Random.Range(1, 5);
//		Debug.Log("-----------------------------------------------------------------------------");
//		Debug.Log("Random animation was: " + random);
//		Debug.Log ("Hit1 animation: " + animator.GetCurrentAnimatorStateInfo(0).IsName("Hit1"));
//		Debug.Log ("Hit2 animation: " + animator.GetCurrentAnimatorStateInfo(0).IsName("Hit2"));
//		Debug.Log ("Hit3 animation: " + animator.GetCurrentAnimatorStateInfo(0).IsName("Hit3"));
//		Debug.Log ("Hit4 animation: " + animator.GetCurrentAnimatorStateInfo(0).IsName("Hit4"));
		animator.SetLayerWeight(1, 0f); // turn off weapon layer instantly!
		animator.SetBool("Death"+random, true);
		animator.Play("HammerDeath"+random); // play this animation instantly! DO NOT use setBool or any other way that would blend/fade animation. Some AnimationEvents can be triggered, but this component can be gone!!!
		Scripts.audioManager.PlaySFX("Characters/Hammer/HammerDeath"+random);
		// Destroy components
		Destroy(characterMotor);
		Destroy(characterController);
		Destroy(controller);
		Destroy(this);

		// See script for details
		gameObject.AddComponent<CharacterAnimationEventCatcher>();

		// reset spine
		spine.transform.rotation = spineSourceRotation;
		// just to be sure
		animator.applyRootMotion = false;

		// make sure the Hammer can fall (but not trhough floors when he's killed mid air)

		Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
		rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
		rigidbody.freezeRotation = true;
		BoxCollider collider = gameObject.AddComponent<BoxCollider>();
		collider.center = new Vector3(0, .5f, 0);

		// Fail the mission
		MissionManager.FailMission(); // Added this here to end the mission etc.
	}

	// (DG) Not sure if this is a good place to put this.
	public void ChangeSkin(string skinName)
	{
		Material hammerMaterial = gameObject.GetComponentInChildren<SkinnedMeshRenderer>().material;
		Texture newTexture = Resources.Load("Textures/Hammer/Hammer" + skinName + "_Texture") as Texture;
		if (newTexture != null) hammerMaterial.mainTexture = newTexture;
		else Debug.Log("[Hammer] Could not load the new skin! Check the path and the file.");
	}


	public override void PlaySFX(string aSoundEffect)
	{
		// (DG) This catches an event in the animation (i.e. footsteps!)
		string aSFX = aSoundEffect;
		if (aSoundEffect.Contains("_"))
		{
			string[] array = aSoundEffect.Split('_');
			aSFX = array[0] + UnityEngine.Random.Range(1, int.Parse(array[1]));
		}
		Scripts.audioManager.PlaySFX("Characters/Hammer/"+aSFX);
		
		
	}
	
}
