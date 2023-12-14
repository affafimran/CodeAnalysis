using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using System.Collections;
using System.Collections.Generic;

public class TurretUserController : VehicleInputController
{
	private Turret turret;
	private TurretData turretData;
	
	private float deltaTime;
	private int currentBarrel = 0; 
	private HitData hitData;

	// [NEW] Experimental setup for (non)continuous x rotation
	private bool continuesX = true;
	private float storedY;
	private float maxAngle = 35f;
	private float lerpFactor = 0.7f;

	//----------------------------------------------------------------
	// Initialize
	//----------------------------------------------------------------
	public override void Initialize()
	{
		turret = gameObject.GetComponent<Turret>(); // main turret component
		turretData = turret.turretData; // get the turretData for east access\
		hitData = new HitData();

		// [MOBILE] ContinousX or not?
		continuesX = !GameData.mobile;

		// [MOBILE] Show/Hide the correct joystick(s) and set the correct layout!
		if (GameData.mobile) Scripts.interfaceScript.gamePanelScript.UpdateJoystickSet(GamePanel.JoystickSets.Turret);

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

		UserInput ();
		Aim ();
	}
	
	void UserInput()
	{
		// Save the Input
		primaryFire = CrossPlatformInputManager.GetButton("Fire1") || (CrossPlatformInputManager.GetAxis("Fire1 Joystick Trigger") > 0); 
		use = CrossPlatformInputManager.GetButtonDown("Use");
		

		// [MOBILE] we process X differently so the movement is NOT constant but feels like you're only offsetting the camera.
		// This allows for more precise aiming.
		float x = CrossPlatformInputManager.GetAxis("Mouse X");
		if (!continuesX){
			Vector3 euler = gameObject.transform.eulerAngles;
			if (x == 0f) storedY = euler.y;
			else{euler.y = Mathf.LerpAngle(euler.y, storedY + (x * maxAngle), lerpFactor);gameObject.transform.eulerAngles = euler;}
		} else gameObject.transform.Rotate(0, x * (10f*GameData.mouseSensitivity), 0);

		// Aiming sound
		if (x != 0) turretData.aimingSound.enabled = true;
		else turretData.aimingSound.enabled = false;
		
		// Firing
		if (primaryFire) 
		{ 
			// calc rof
			turretData.currentRof -= deltaTime; 

			if (turretData.currentRof < 0)
			{
				turretData.currentRof = turretData.rof;
				currentBarrel++;
				if (currentBarrel % 2 == 0)	Fire (turretData.muzzleFlash1); 
				else Fire (turretData.muzzleFlash2);
			}
		} else turretData.currentRof = 0;
		
		// 'Use' (exit)
		if (use) turret.ExitVehicle();
		
	}
	
	void Aim()
	{
		GameObject camera = CameraManager.activeCamera;
		Ray ray = new Ray(camera.transform.position + camera.transform.forward * 5f, camera.transform.forward);
		//Ray ray = new Ray(camera.transform.position, camera.transform.forward);
		RaycastHit rayCastHit;
		LayerMask layermask = ProjectileManager.projectileLayerMask;
		Vector3 aimTarget = Vector3.zero;
		if (Physics.Raycast(ray, out rayCastHit, 1000, layermask))
		{
			aimTarget = rayCastHit.point;
			hitData = ProjectileManager.RayCastHitToHitData(rayCastHit);
			//Debug.Log(hitData.distance+ " - " + hitData.position);
		}

		if (aimTarget == Vector3.zero) aimTarget = ray.origin + ray.direction * 1000;
		turretData.barrel.transform.LookAt (aimTarget);
		
		//		if (turretData.barrel.transform.eulerAngles.x < 334 && turretData.barrel.transform.eulerAngles.x > 200) 
		//			turretData.barrel.transform.eulerAngles = new Vector3(334, turretData.barrel.transform.eulerAngles.y, turretData.barrel.transform.eulerAngles.z);
		
	}
	
	void Fire(GameObject muzzleFlash)
	{
		// Weapon sound
		Scripts.audioManager.PlaySFX3D(turretData.sound, turretData.barrel, "FireGun");
		ProjectileManager.AddProjectile (turretData.projectile, muzzleFlash.transform.position, turretData.barrel.transform.forward, hitData);
		muzzleFlash.PlayInChildren ();
		CameraManager.activeCameraData.shakeIntensity = 2.0f;
	}


	
	public override void Destroy()
	{
		// [MOBILE] Show/Hide the correct joystick(s) and set the correct layout!
		if (GameData.mobile) Scripts.interfaceScript.gamePanelScript.UpdateJoystickSet(GamePanel.JoystickSets.Normal);
		// Destroy this instance
		Destroy(this);
	}
}