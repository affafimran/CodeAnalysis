using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using System.Collections;

public class TankUserController : VehicleInputController {
	
	private Tank tank;
	private TankData tankData;
	//private TankCarData tankCarData;

	// [NEW] Experimental setup for (non)continuous x rotation
	private bool continuesX = true;
	private float storedY;
	private float maxAngle = 45f;
	private float lerpFactor = 0.7f;
	
	public override void Initialize()
	{
		// Store main component
		tank = gameObject.GetComponent<Tank>(); // main tank component
		tankData = tank.tankData;

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
		use = CrossPlatformInputManager.GetButtonDown("Use");

		if (horAxis != 0 || verAxis != 0) tankData.drivingSound.enabled = true; // driving Sound
		else tankData.drivingSound.enabled = false; // turn off driving sound

		// [MOBILE] we process X differently so the movement is NOT constant but feels like you're only offsetting the camera.
		// This allows for more precise aiming.
		float x = CrossPlatformInputManager.GetAxis("Mouse X");
		if (!continuesX){
			Vector3 euler = tankData.turret.transform.eulerAngles;
			if (x == 0f) storedY = euler.y;
			else{euler.y = Mathf.LerpAngle(euler.y, storedY + (x * maxAngle), lerpFactor);tankData.turret.transform.eulerAngles = euler;}
		} else tankData.turret.transform.Rotate(0, x * (10f*GameData.mouseSensitivity), 0);

		// Aiming sound
		if (x != 0) tankData.aimingSound.enabled = true;
		else tankData.aimingSound.enabled = false;
		
		// Firing
		if (primaryFire) 
		{
			tankData.currentRof -= Time.deltaTime;
			if (tankData.currentRof < 0)
			{
				tankData.currentRof = tankData.rof;
				Fire ();
			}
		} else tankData.currentRof = 0;



		// use
		if (use) tank.ExitVehicle();
	}

	void Aim()
	{
		GameObject camera = CameraManager.activeCamera;
		Ray ray = new Ray(camera.transform.position, camera.transform.forward);
		RaycastHit rayCastHit;
		LayerMask layermask = ProjectileManager.projectileLayerMask;
		Vector3 aimTarget = Vector3.zero;
		Bounds bounds = tank.vehicle.GetComponent<Renderer>().bounds;
		// This is a hack for the tank, because it is the only unit that will raycast on its barrel when to close to something
		tankData.barrel.layer = 2;
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
		// Reset the hack from before the raycast
		tankData.barrel.layer = VehicleManager.vehicleLayer;
		// Set aimtarget if zero
		if (aimTarget == Vector3.zero) aimTarget = ray.origin + ray.direction * 1000;
		// Set barrel Rotation
		tankData.barrel.transform.LookAt (aimTarget);
		// Limit barrel rotation
		if (tankData.barrel.transform.eulerAngles.x < 340 && tankData.barrel.transform.eulerAngles.x > 200) 
			tankData.barrel.transform.eulerAngles = new Vector3(340, tankData.barrel.transform.eulerAngles.y, tankData.barrel.transform.eulerAngles.z);
	}
	
	void Fire()
	{
		// Weapon sound
		Scripts.audioManager.PlaySFX3D(tankData.sound, tankData.barrel, "FireGun");
		ProjectileManager.AddProjectile (tankData.projectile, tankData.muzzleFlash1.transform.position, tankData.barrel.transform.forward, new HitData (), new HitData().gameObject, this.gameObject);
		tankData.muzzleFlash1.PlayInChildren();
		
		CameraManager.activeCameraData.shakeIntensity = 5.0f;
		
	}
	


}



//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//
//public class TankUserController : VehicleController
//{
//	private Tank tank;
//	private TankData tankData;
//	
//	private float deltaTime;
//	
//	//----------------------------------------------------------------
//	// Initialize
//	//----------------------------------------------------------------
//	public override void Initialize()
//	{
//		tank = gameObject.GetComponent<Tank>(); // main tank component
//		tankData = tank.tankData; // get the tankData for east access
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
//		tankData.currentRof -= deltaTime; 
//		
//		UserInput ();
//		Aim ();
//		
//		
//		
//	}
//	
//	void UserInput()
//	{
//		// Save the Input
//		horAxis = Input.GetAxis ("Horizontal");
//		verAxis = Input.GetAxis ("Vertical");
//		
//		primaryFire = Input.GetButton("Fire1"); 
//		use = Input.GetButtonDown("Use");
//		
//		if (horAxis != 0 || verAxis != 0) 
//		{
//			tankData.drivingSound.enabled = true; // driving Sound
//			Movement();
//		}
//		else tankData.drivingSound.enabled = false; // turn off driving sound
//		
//		// Rotate the turret up and down
//		float tRotX = Input.GetAxis("Mouse X") * (10f*GameData.mouseSensitivity);
//		tankData.turret.transform.Rotate(0, tRotX, 0);
//		// Aiming sound
//		if (tRotX != 0) tankData.aimingSound.enabled = true;
//		else tankData.aimingSound.enabled = false;
//		
//		// Firing
//		if (primaryFire) 
//		{
//			if (tankData.currentRof < 0)
//			{
//				tankData.currentRof = tankData.rof;
//				Fire ();
//			}
//		} else tankData.currentRof = 0;
//		
//		// use
//		if (use) tank.ExitVehicle();
//		
//		
//		
//	}
//	
//	void Aim()
//	{
//		GameObject camera = CameraManager.activeCamera;
//		Ray ray = new Ray(camera.transform.position, camera.transform.forward);
//		RaycastHit rayCastHit;
//		LayerMask layermask = ProjectileManager.projectileLayerMask;
//		Vector3 aimTarget = Vector3.zero;
//		Bounds bounds = tank.vehicle.GetComponent<Renderer>().bounds;
//		// This is a hack for the tank, because it is the only unit that will raycast on its barrel when to close to something
//		tankData.barrel.layer = 2;
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
//		// Reset the hack from before the raycast
//		tankData.barrel.layer = 14;
//		// Set aimtarget if zero
//		if (aimTarget == Vector3.zero) aimTarget = ray.origin + ray.direction * 1000;
//		// Set barrel Rotation
//		tankData.barrel.transform.LookAt (aimTarget);
//		// Limit barrel rotation
//		if (tankData.barrel.transform.eulerAngles.x < 340 && tankData.barrel.transform.eulerAngles.x > 200) 
//			tankData.barrel.transform.eulerAngles = new Vector3(340, tankData.barrel.transform.eulerAngles.y, tankData.barrel.transform.eulerAngles.z);
//	}
//	
//	void Fire()
//	{
//		// Weapon sound
//		Scripts.audioManager.PlaySFX3D(tankData.sound, tankData.barrel, "FireGun");
//		ProjectileManager.AddProjectile (tankData.projectile, tankData.muzzleFlash1.transform.position, tankData.barrel.transform.forward, new HitData (), new HitData().gameObject, this.gameObject);
//		tankData.muzzleFlash1.EmitInChildren(20);
//		
//		CameraManager.activeCameraData.shakeIntensity = 12.0f;
//		
//	}
//	
//	void Movement()
//	{
//		// Drive forward or backwards
//		tankData.rigidBody.AddForce (tank.vehicle.transform.forward * verAxis * 60 * 40 * deltaTime, ForceMode.Acceleration);
//		// Rotate the tank around
//		if (verAxis >= 0) tankData.rigidBody.AddTorque (new Vector3(0, 1, 0) * horAxis * 5 * 40 * deltaTime, ForceMode.Acceleration);
//		else if (verAxis < 0) tankData.rigidBody.AddTorque (new Vector3(0, 1, 0) * -horAxis * 5 * 40 * deltaTime, ForceMode.Acceleration);		
//	}
//}