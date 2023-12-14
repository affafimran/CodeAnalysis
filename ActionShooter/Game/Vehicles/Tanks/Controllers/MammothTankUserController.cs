using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using System.Collections;

public class MammothTankUserController : VehicleInputController {
	
	private MammothTank mammothTank;
	private MammothTankData mammothTankData;
	//private MammothTankCarData mammothTankCarData;

	private float deltaTime;
	private GameObject grenadeTarget;
	private int currentBarrel1 = 0;
	private int currentBarrel2 = 0;

	// [NEW] Experimental setup for (non)continuous x rotation
	private bool continuesX = true;
	private float storedY;
	private float maxAngle = 45f;
	private float lerpFactor = 0.7f;
		
	public override void Initialize()
	{
		// Store main component
		mammothTank = gameObject.GetComponent<MammothTank>(); // main tank component
		mammothTankData = mammothTank.mammothTankData;

		// Why Frank?
		if (GameObject.Find("MammothTankGrenadeTarget") == null){
			grenadeTarget = new GameObject ();
			grenadeTarget.name = "MammothTankGrenadeTarget";
		}
		else grenadeTarget = GameObject.Find("MammothTankGrenadeTarget");

		// [MOBILE] ContinousX or not?
		continuesX = !GameData.mobile;
	}
	
	//----------------------------------------------------------------
	// Update
	//----------------------------------------------------------------
	public override void Update()
	{
		// no runny when paused
		if (Data.pause) return;
		// store deltaTime for easy access
		deltaTime = Time.deltaTime;
	    // Check input
		UserInput ();
		// Aim
		Aim ();
		
	}
	
	void UserInput()
	{
		// Save the Input
		horAxis = CrossPlatformInputManager.GetAxis ("Horizontal");
		verAxis = CrossPlatformInputManager.GetAxis ("Vertical");
		
		horAxisAsBool = CrossPlatformInputManager.GetButton("Horizontal");
		verAxisAsBool = CrossPlatformInputManager.GetButton("Vertical");
		
		// fire & use
		primaryFire = CrossPlatformInputManager.GetButton("Fire1") || (CrossPlatformInputManager.GetAxis("Fire1 Joystick Trigger") > 0);
		secondaryFire = CrossPlatformInputManager.GetButton("Fire2") || (CrossPlatformInputManager.GetAxis("Fire2 Joystick Trigger") > 0);
		use = CrossPlatformInputManager.GetButtonDown("Use");
		
		if (horAxis != 0 || verAxis != 0) mammothTankData.drivingSound.enabled = true; // driving Sound
		else mammothTankData.drivingSound.enabled = false; // turn off driving sound

		// [MOBILE] we process X differently so the movement is NOT constant but feels like you're only offsetting the camera.
		// This allows for more precise aiming.
		float x = CrossPlatformInputManager.GetAxis("Mouse X");
		if (!continuesX){
			Vector3 euler = mammothTankData.turret.transform.eulerAngles;
			if (x == 0f) storedY = euler.y;
			else{euler.y = Mathf.LerpAngle(euler.y, storedY + (x * maxAngle), lerpFactor);mammothTankData.turret.transform.eulerAngles = euler;}
		} else mammothTankData.turret.transform.Rotate(0, x * (10f*GameData.mouseSensitivity), 0);

		// Aiming sound
		if (x != 0) mammothTankData.aimingSound.enabled = true;
		else mammothTankData.aimingSound.enabled = false;


		// Firing
		if (primaryFire) 
		{
			mammothTankData.currentRof -= deltaTime;
			if (mammothTankData.currentRof < 0)
			{
				currentBarrel1++;
				mammothTankData.currentRof = mammothTankData.rof;
				if (currentBarrel1 % 2 == 0) Fire1 (mammothTankData.muzzleFlash1);
				else Fire1 (mammothTankData.muzzleFlash2);
			}
		} else mammothTankData.currentRof = 0;
		
		// Firing
		if (secondaryFire) 
		{
			mammothTankData.currentRof2 -= deltaTime;
			if (mammothTankData.currentRof2 < 0)
			{
				mammothTankData.currentRof2 = mammothTankData.rof2;
				currentBarrel2++;
				if (currentBarrel2 % 2 == 0) Fire2 (mammothTankData.muzzleFlash3, grenadeTarget);
				else Fire2 (mammothTankData.muzzleFlash4, grenadeTarget);
			}
		} else mammothTankData.currentRof2 = 0;
		
		// use
		if (use) mammothTank.ExitVehicle();
	}

	void Aim()
	{
		GameObject camera = CameraManager.activeCamera;
		Ray ray = new Ray(camera.transform.position, camera.transform.forward);
		RaycastHit rayCastHit;
		LayerMask layermask = ProjectileManager.projectileLayerMask;
		Vector3 aimTarget = Vector3.zero;
		Bounds bounds = mammothTank.vehicle.GetComponent<Renderer>().bounds;
		// Raycast forward to crosshair
		if (Physics.Raycast(ray, out rayCastHit, 1000, layermask))
		{
			aimTarget = rayCastHit.point;
			if (Mathf.Abs(aimTarget.x - transform.position.x) < bounds.extents.x &&
			    Mathf.Abs(aimTarget.z - transform.position.z) < bounds.extents.z)
			{
				aimTarget = Vector3.zero;
			}
		}
		// Set the aimTarget if it is zero
		if (aimTarget == Vector3.zero) aimTarget = ray.origin + ray.direction * 1000;
		// Set Grenade target position
		grenadeTarget.transform.position = aimTarget;
		// Barrel Rotation
		mammothTankData.barrel.transform.LookAt (aimTarget);
		// Barrel Limitation
		if (mammothTankData.barrel.transform.eulerAngles.x < 340 && mammothTankData.barrel.transform.eulerAngles.x > 200) 
			mammothTankData.barrel.transform.eulerAngles = new Vector3(340, mammothTankData.barrel.transform.eulerAngles.y, mammothTankData.barrel.transform.eulerAngles.z);
	}
	
	void Fire1(GameObject muzzleFlash)
	{
		// Weapon sound
		Scripts.audioManager.PlaySFX3D(mammothTankData.sound, mammothTankData.barrel, "FireGun");
		ProjectileManager.AddProjectile (mammothTankData.projectile, muzzleFlash.transform.position, mammothTankData.barrel.transform.forward, new HitData (), new HitData().gameObject);
		muzzleFlash.PlayInChildren();
		CameraManager.activeCameraData.shakeIntensity = 12.0f;
	}
	
	void Fire2(GameObject muzzleFlash, GameObject target)
	{
		// Weapon sound
		Scripts.audioManager.PlaySFX3D(mammothTankData.sound2, mammothTankData.launcher, "FireGun");
		// Add projectile and activate the particleSystem
		ProjectileManager.AddProjectile (mammothTankData.projectile2, muzzleFlash.transform.position, mammothTankData.barrel.transform.forward, new HitData (), target, this.gameObject);
		muzzleFlash.PlayInChildren();
		CameraManager.activeCameraData.shakeIntensity = 8.0f;
	}
}


//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//
//public class MammothTankUserController : VehicleController
//{
//	private MammothTank mammothTank;
//	private MammothTankData mammothTankData;
//	
//	private float deltaTime;
//	
//	// Grenade launcher vars
//
//	public GameObject grenadeTarget;
//	
//	private int currentBarrel1 = 0;
//	private int currentBarrel2 = 0;
//	
//	//----------------------------------------------------------------
//	// Initialize
//	//----------------------------------------------------------------
//	public override void Initialize()
//	{
//		mammothTank = gameObject.GetComponent<MammothTank>(); // main tank component
//		mammothTankData = mammothTank.mammothTankData; // get the tankData for east access
//
//		if (GameObject.Find("MammothTankGrenadeTarget") == null)
//		{
//			grenadeTarget = new GameObject ();
//			grenadeTarget.name = "MammothTankGrenadeTarget";
//		}
//		else grenadeTarget = GameObject.Find("MammothTankGrenadeTarget");
//		
//		// Launcher rotation
//		mammothTankData.launcher.transform.eulerAngles = mammothTankData.launcher.transform.eulerAngles + new Vector3 (-30, 0, 0);
//	}
//	
//	//----------------------------------------------------------------
//	// Update
//	//----------------------------------------------------------------
//	public override void Update()
//	{
//		// no runny when paused
//		if (Data.pause) return;
//		
//		// store deltaTime for easy access
//		deltaTime = Time.deltaTime;
//		
//		// calc rof
//		mammothTankData.currentRof -= deltaTime;
//		mammothTankData.currentRof2 -= deltaTime;
//		
//		UserInput ();
//		Aim ();
//	}
//	
//	void UserInput()
//	{
//		// Save the Input
//		horAxis = Input.GetAxis ("Horizontal");
//		verAxis = Input.GetAxis ("Vertical");
//		
//		primaryFire = Input.GetButton("Fire1"); 
//		secondaryFire = Input.GetButtonDown("Fire2");
//		use = Input.GetButtonDown("Use");
//		
//		if (horAxis != 0 || verAxis != 0) 
//		{
//			mammothTankData.drivingSound.enabled = true; // driving Sound
//			Movement();
//		}
//		else mammothTankData.drivingSound.enabled = false; // turn off driving sound
//		
//		// Rotate the turret up and down
//		float tRotX = Input.GetAxis("Mouse X") * (10f*GameData.mouseSensitivity);
//		mammothTankData.turret.transform.Rotate(0, tRotX, 0);
//		// Aiming sound
//		if (tRotX != 0) mammothTankData.aimingSound.enabled = true;
//		else mammothTankData.aimingSound.enabled = false;
//		
//		// Firing
//		if (primaryFire) 
//		{
//			if (mammothTankData.currentRof < 0)
//			{
//				currentBarrel1++;
//				mammothTankData.currentRof = mammothTankData.rof;
//				if (currentBarrel1 % 2 == 0) Fire1 (mammothTankData.muzzleFlash1);
//				else Fire1 (mammothTankData.muzzleFlash2);
//			}
//		} else mammothTankData.currentRof = 0;
//		
//		// Firing
//		if (secondaryFire) 
//		{
//			if (mammothTankData.currentRof2 < 0)
//			{
//				mammothTankData.currentRof2 = mammothTankData.rof2;
//				currentBarrel2++;
//				if (currentBarrel2 % 2 == 0) Fire2 (mammothTankData.muzzleFlash3, grenadeTarget);
//				else Fire2 (mammothTankData.muzzleFlash4, grenadeTarget);
//			}
//		} else mammothTankData.currentRof2 = 0;
//		
//		// use
//		if (use) mammothTank.ExitVehicle();
//	}
//	
//	void Aim()
//	{
//		GameObject camera = CameraManager.activeCamera;
//		Ray ray = new Ray(camera.transform.position, camera.transform.forward);
//		RaycastHit rayCastHit;
//		LayerMask layermask = ProjectileManager.projectileLayerMask;
//		Vector3 aimTarget = Vector3.zero;
//		Bounds bounds = mammothTank.vehicle.GetComponent<Renderer>().bounds;
//		// Raycast forward to crosshair
//		if (Physics.Raycast(ray, out rayCastHit, 1000, layermask))
//		{
//			aimTarget = rayCastHit.point;
//			if (Mathf.Abs(aimTarget.x - transform.position.x) < bounds.extents.x &&
//			    Mathf.Abs(aimTarget.z - transform.position.z) < bounds.extents.z)
//			{
//				aimTarget = Vector3.zero;
//			}
//		}
//		// Set the aimTarget if it is zero
//		if (aimTarget == Vector3.zero) aimTarget = ray.origin + ray.direction * 1000;
//		// Set Grenade target position
//		grenadeTarget.transform.position = aimTarget;
//		// Barrel Rotation
//		mammothTankData.barrel.transform.LookAt (aimTarget);
//		// Barrel Limitation
//		if (mammothTankData.barrel.transform.eulerAngles.x < 340 && mammothTankData.barrel.transform.eulerAngles.x > 200) 
//			mammothTankData.barrel.transform.eulerAngles = new Vector3(340, mammothTankData.barrel.transform.eulerAngles.y, mammothTankData.barrel.transform.eulerAngles.z);
//	}
//	
//	void Fire1(GameObject muzzleFlash)
//	{
//		// Weapon sound
//		Scripts.audioManager.PlaySFX3D(mammothTankData.sound, mammothTankData.barrel, "FireGun");
//		ProjectileManager.AddProjectile (mammothTankData.projectile, muzzleFlash.transform.position, mammothTankData.barrel.transform.forward, new HitData (), new HitData().gameObject);
//		muzzleFlash.PlayInChildren();
//		CameraManager.activeCameraData.shakeIntensity = 12.0f;
//	}
//	
//	void Fire2(GameObject muzzleFlash, GameObject target)
//	{
//		// Weapon sound
//		Scripts.audioManager.PlaySFX3D(mammothTankData.sound2, mammothTankData.launcher, "FireGun");
//		// Add projectile and activate the particleSystem
//		ProjectileManager.AddProjectile (mammothTankData.projectile2, muzzleFlash.transform.position, mammothTankData.barrel.transform.forward, new HitData (), target, this.gameObject);
//		muzzleFlash.PlayInChildren();
//		CameraManager.activeCameraData.shakeIntensity = 8.0f;
//	}
//	
//	void Movement()
//	{
//		// Drive forward or backwards
//		mammothTankData.rigidBody.AddForce (mammothTank.vehicle.transform.forward * verAxis * 60 * 40 * deltaTime, ForceMode.Acceleration);
//		// Rotate the tank around
//		if (verAxis >= 0) mammothTankData.rigidBody.AddTorque (new Vector3(0, 1, 0) * horAxis * 5 * 40 * deltaTime, ForceMode.Acceleration);
//		else if (verAxis < 0) mammothTankData.rigidBody.AddTorque (new Vector3(0, 1, 0) * -horAxis * 5 * 40 * deltaTime, ForceMode.Acceleration);
//	}
//}