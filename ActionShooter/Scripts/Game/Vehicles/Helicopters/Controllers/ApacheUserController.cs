using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using System.Collections;
using System.Collections.Generic;

public class ApacheUserController : VehicleInputController
{
	private Apache apache;
	private ApacheData apacheData;

	private float maxSpeed = 50f;
	private Vector3 accelerationMultiplier = new Vector3(1.4f, 1.4f, 1.4f);
	private Vector3 dragMultiplier =  new Vector3(0.9f, 0.95f, 0.9f);
	
	private float maxPitchAngle = 25f;
	private float maxRollAngle  = 25f; 
	
	private RaycastHit rayCastHit;

	// Firing vars
	public float rof2;
	public float currentRof2;
	private int currentLauncher = 1; // 1 = left, -1 = right
	private float deltaTime;

	// [NEW] Experimental setup for (non)continuous x rotation
	private bool continuesX = true;
	private float storedY;
	private float maxAngle = 25f;
	private float lerpFactor = 0.7f;
	//----------------------------------------------------------------
	// Initialize
	//----------------------------------------------------------------
	public override void Initialize()
	{
		apache = gameObject.GetComponent<Apache>(); // main tank component
		apacheData = apache.apacheData; // get the data for easy access
		
		transform.eulerAngles = new Vector3 (0, transform.eulerAngles.y, 0); // Reset the rotation
		apacheData.rotorSpeed = 1200;
		apacheData.rigidBody.useGravity = false;
		apacheData.rigidBody.freezeRotation = true;

		//activate sound
		apacheData.rotorSound.enabled = true;

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

		UserInput ();
		Aim ();
		
		// Firing
		if (primaryFire) 
		{
			apacheData.currentRof -= deltaTime; 
			if (apacheData.currentRof < 0)
			{
				apacheData.currentRof = apacheData.rof;
				Fire1();
			}
		} else apacheData.currentRof = 0;

		if (secondaryFire)
		{
			apacheData.currentRof2 -= deltaTime;
			if (apacheData.currentRof2 < 0)
			{
				currentLauncher++;
				apacheData.currentRof2 = apacheData.rof2;
				Fire2(apacheData.launchers[currentLauncher % 4]);
			}
		} else currentRof2 = 0;
	}
	
	void UserInput()
	{
		// Save the Input
		horAxis = CrossPlatformInputManager.GetAxis ("Horizontal");
		verAxis = CrossPlatformInputManager.GetAxis ("Vertical");
		up = CrossPlatformInputManager.GetButton("Jump");
		down = CrossPlatformInputManager.GetButton("Crouch");

		primaryFire   = CrossPlatformInputManager.GetButton ("Fire1") || (CrossPlatformInputManager.GetAxis("Fire1 Joystick Trigger") > 0); 

		// The && !down prevents flying down (with a 360 controller) and shooting rockets at the same time
		// This might be the ugliest hack ever. At the end of the project (and looking at the current InputManger) it'll have to do
		secondaryFire = (CrossPlatformInputManager.GetButton ("Fire2") && !down) || (CrossPlatformInputManager.GetAxis("Fire2 Joystick Trigger") > 0);

		use = CrossPlatformInputManager.GetButtonDown ("Use");
		
		// Rotate the helicopter left, right
		// We're rotating the rigidbody using MoveRotation
		// This uses the interpolation setting as well. Preventing jittering with the camera!

		// [MOBILE] we process X differently so the movement is NOT constant but feels like you're only offsetting the camera.
		// This allows for more precise aiming.
		float x = CrossPlatformInputManager.GetAxis("Mouse X");
		if (!continuesX){
			Vector3 euler = gameObject.transform.localEulerAngles;
			if (x == 0f) storedY = euler.y;
			else{euler.y = Mathf.LerpAngle(euler.y, storedY + (x * maxAngle), lerpFactor);apacheData.rigidBody.MoveRotation(Quaternion.Euler(euler));}
		} else {
			Vector3 rotation = gameObject.transform.localEulerAngles + new Vector3(0, x,  0);
			Quaternion euler = Quaternion.Euler(rotation);
			apacheData.rigidBody.MoveRotation(euler);
		}

		// Movement
		Vector3 velocity = gameObject.transform.InverseTransformDirection(apacheData.rigidBody.velocity);
		float speedDifference;
		
		Vector3 force;
		Vector3 drag;
		
		// X: left/right
		if (horAxis > 0){
			speedDifference = maxSpeed - velocity.x; // forward speed difference
			force = Vector3.right * (horAxis * apacheData.rigidBody.mass * speedDifference * accelerationMultiplier.x);
			apacheData.rigidBody.AddRelativeForce(force);
		} else if (horAxis < 0) {
			speedDifference = -maxSpeed - velocity.x; // forward speed difference
			force = Vector3.left * (horAxis * apacheData.rigidBody.mass * speedDifference * accelerationMultiplier.x);
			apacheData.rigidBody.AddRelativeForce(force);
		}
		
		// Y: up/down
		if (up){
			speedDifference = maxSpeed - velocity.y; // upward speed difference
			force = Vector3.up * (apacheData.rigidBody.mass * speedDifference * accelerationMultiplier.y);
			apacheData.rigidBody.AddRelativeForce(force);
		}
		
		if (down) {
			speedDifference = -maxSpeed - velocity.y; // upward speed difference
			force = Vector3.up * (apacheData.rigidBody.mass * speedDifference * accelerationMultiplier.y);
			apacheData.rigidBody.AddRelativeForce(force);
		}
		
		// Z: forward/backward
		if (verAxis > 0){
			speedDifference = maxSpeed - velocity.z; // forward speed difference
			force = Vector3.forward * (verAxis * apacheData.rigidBody.mass * speedDifference * accelerationMultiplier.z);
			apacheData.rigidBody.AddRelativeForce(force);
		} else if (verAxis < 0) {
			speedDifference = -maxSpeed - velocity.z; // forward speed difference
			force = Vector3.back * (verAxis * apacheData.rigidBody.mass * speedDifference * accelerationMultiplier.z);
			apacheData.rigidBody.AddRelativeForce(force);
		}
		
		// Apply drag in all positions.
		// You can also opt to get the new velocity, invert it and have one force to add
		// But I would like to have different drag settings per axis.


		// left/right
		drag    = Vector3.left * (velocity.x * dragMultiplier.x * apacheData.rigidBody.mass);
		apacheData.rigidBody.AddRelativeForce(drag);

		// up/down
		drag    = Vector3.down * (velocity.y * dragMultiplier.y * apacheData.rigidBody.mass);
		apacheData.rigidBody.AddRelativeForce(drag);
		
		// forward/backward
		drag    = Vector3.back * (velocity.z * dragMultiplier.z * apacheData.rigidBody.mass);
		apacheData.rigidBody.AddRelativeForce(drag);
		

		// pitch and roll, based upon speed
		float roll  = Mathf.Max (-1f, -velocity.x/maxSpeed);
		float pitch = Mathf.Min(1f, velocity.z/maxSpeed);
		Vector3 localEuler = apacheData.body.transform.localEulerAngles;
		apacheData.body.transform.localEulerAngles = new Vector3(maxPitchAngle*pitch, localEuler.y, maxRollAngle*roll);
		
		// sound update
		float perc = velocity.magnitude/maxSpeed;
		apacheData.rotorSound.pitch = Mathf.Min(1f, 0.6f + perc);
		apacheData.rotorSpeed = 1200; // Mathf.Min(1200, 699 + (1100*perc));

		if (use)
		{
			apacheData.rigidBody.constraints = RigidbodyConstraints.None;
			apacheData.rigidBody.useGravity = true;
			apacheData.rotorSpeed = 0;
			apacheData.rotorSound.enabled = false;
			
			apache.ExitVehicle();
		}
	}
	
	void Aim()
	{
		// aiming
		GameObject camera = CameraManager.activeCamera;
		float distance  = (camera.transform.position - gameObject.transform.position).magnitude + 3f;
		Vector3 position = camera.transform.position + (camera.transform.forward * distance); // this works okay for now
		Ray ray = new Ray(position, camera.transform.forward);
		Vector3 aimTarget = Vector3.zero;
		// Raycast forward to crosshair
		if (Physics.Raycast(ray, out rayCastHit, 1000, ProjectileManager.projectileLayerMask)){
			aimTarget = rayCastHit.point;
		} else aimTarget = ray.origin + ray.direction * 1000;
		// Set barrel Rotation
		apacheData.gun.transform.LookAt (aimTarget);
	}
	
	public void Fire1()
	{
		// Play first weapon sound
		Scripts.audioManager.PlaySFX3D("Weapons/M4Fire", apacheData.gun, "FireGun");
		ProjectileManager.AddProjectile(apacheData.projectile, apacheData.muzzleFlash5.transform.position, apacheData.gun.transform.forward, new HitData(), new HitData().gameObject);
		apacheData.muzzleFlash5.PlayInChildren();


	}
	
	public void Fire2(GameObject muzzleFlash)
	{
		// Play second weapon sound
		Scripts.audioManager.PlaySFX3D("Weapons/TankShot", apacheData.rocketLauncher, "FireGun");
		ProjectileManager.AddProjectile(apacheData.projectile2, muzzleFlash.transform.position, apacheData.gun.transform.forward, new HitData(), new HitData().gameObject, this.gameObject);
		muzzleFlash.PlayInChildren();
	}
	

}
