using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using System.Collections;
using System.Collections.Generic;

public class MissileTurretUserController : VehicleInputController
{
	private MissileTurret missileTurret;
	private MissileTurretData missileTurretData;
	
	private float deltaTime;	
	private int currentBarrel = 0;

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
		missileTurret = gameObject.GetComponent<MissileTurret>(); // main turret component
		missileTurretData = missileTurret.missileTurretData; // get the turretData for east access

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
		if (x != 0) missileTurretData.aimingSound.enabled = true;
		else missileTurretData.aimingSound.enabled = false;
		
		// Firing
		if (primaryFire) 
		{ 
			// calc rof
			missileTurretData.currentRof -= deltaTime; 

			if (missileTurretData.currentRof < 0)
			{
				missileTurretData.currentRof = missileTurretData.rof;
				GameObject target = GetTarget();
				currentBarrel++;
				if (currentBarrel % 2 == 0)	Fire (missileTurretData.muzzleFlash1, target); 
				else Fire (missileTurretData.muzzleFlash2, target);
			}
		} else missileTurretData.currentRof = 0;
		
		// 'Use' (exit)
		if (use) missileTurret.ExitVehicle();
	}
	
	GameObject GetTarget()
	{
		GameObject camera = CameraManager.activeCamera;
		Ray ray = new Ray (camera.transform.position, camera.transform.forward);
		RaycastHit rayCastHit;
		LayerMask layermask = ProjectileManager.projectileLayerMask;
		if (Physics.Raycast (ray, out rayCastHit, 1000, layermask))
		{
			if (rayCastHit.collider.gameObject.name.Contains("MissileTurret")) return null;
			return rayCastHit.collider.gameObject;
		}
		return null;
	}
	
	
	void Aim()
	{
		GameObject camera = CameraManager.activeCamera;
		Ray ray = new Ray(camera.transform.position, camera.transform.forward);
		RaycastHit rayCastHit;
		LayerMask layermask = ProjectileManager.projectileLayerMask;
		Vector3 aimTarget = Vector3.zero;
		if (Physics.Raycast(ray, out rayCastHit, 1000, layermask))
		{
			aimTarget = rayCastHit.point;
		}
		if (aimTarget == Vector3.zero) aimTarget = ray.origin + ray.direction * 1000;
		
		missileTurretData.barrel.transform.LookAt (aimTarget);
		
		//		if (missileTurretData.barrel.transform.eulerAngles.x < 334 && missileTurretData.barrel.transform.eulerAngles.x > 200) 
		//			missileTurretData.barrel.transform.eulerAngles = new Vector3(334, missileTurretData.barrel.transform.eulerAngles.y, missileTurretData.barrel.transform.eulerAngles.z);
		
	}
	
	void Fire(GameObject muzzleFlash, GameObject target)
	{
		// Weapon sound
		Scripts.audioManager.PlaySFX3D(missileTurretData.sound, missileTurretData.barrel, "FireGun");
		ProjectileManager.AddProjectile (missileTurretData.projectile, muzzleFlash.transform.position, missileTurretData.barrel.transform.forward, new HitData (), target, this.gameObject);
		muzzleFlash.PlayInChildren();
	}

	
	public override void Destroy()
	{
		// [MOBILE] Show/Hide the correct joystick(s) and set the correct layout!
		if (GameData.mobile) Scripts.interfaceScript.gamePanelScript.UpdateJoystickSet(GamePanel.JoystickSets.Normal);
		// Destroy this instance
		Destroy(this);
	}
}